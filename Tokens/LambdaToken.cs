using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace QuickConverter.Tokens
{
	public class LambdaToken : TokenBase
	{
		private static Type[] funcTypes = new Type[] { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>), typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>), typeof(Func<,,,,,,,,>), typeof(Func<,,,,,,,,,>), typeof(Func<,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,>), typeof(Func<,,,,,,,,,,,,,,,,>) };

		internal LambdaToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Value, Arguments }; } }

		public ArgumentListToken Arguments { get; private set; }
		public TokenBase Value { get; private set; }
		public bool Lambda { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			TokenBase arguments;
			bool lambda = false;
			if (!new ArgumentListToken('(', ')', null).TryGetToken(ref text, out arguments))
			{
				lambda = true;
				if (!new ArgumentListToken(true, '(', ')').TryGetToken(ref text, out arguments))
					return false;
			}
			string temp = text.TrimStart();
			if (!temp.StartsWith("=>"))
				return false;
			temp = temp.Substring(2).TrimStart();
			TokenBase method;
			if (!EquationTokenizer.TryEvaluateExpression(temp, out method))
				return false;
			text = "";
			token = new LambdaToken() { Arguments = arguments as ArgumentListToken, Value = method, Lambda = lambda };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			if (Lambda)
			{
				List<Tuple<string, Type>> pars = new List<Tuple<string, Type>>();
				foreach (TokenBase token in Arguments.Arguments)
				{
					if (token is TypeCastToken)
						pars.Add(new Tuple<string, Type>(((token as TypeCastToken).Target as ParameterToken).Name, (token as TypeCastToken).TargetType));
					else
						pars.Add(new Tuple<string, Type>((token as ParameterToken).Name, typeof(object)));
				}
				Dictionary<string, ConstantExpression> subLocals = new Dictionary<string, ConstantExpression>();
				foreach (var tuple in pars)
				{
					var container = new DataContainer();
					subLocals.Add(tuple.Item1, Expression.Constant(container));
					dataContainers.Add(container);
				}

				List<ParameterExpression> parExps = new List<ParameterExpression>();
				Expression exp = Value.GetExpression(parExps, subLocals, dataContainers, dynamicContext, label);

				if (parExps.Count != 0)
				{
					foreach (ParameterExpression par in parExps)
					{
						if (!(parameters.Any(p => p.Name == par.Name) || locals.Any(l => l.Key == par.Name)))
							parameters.Add(par);
						var container = new DataContainer();
						subLocals.Add(par.Name, Expression.Constant(container));
						dataContainers.Add(container);
					}
					parExps.Clear();
					exp = Value.GetExpression(parExps, subLocals, dataContainers, dynamicContext, label);
				}

				foreach (var tuple in pars)
					parExps.Add(Expression.Parameter(tuple.Item2, tuple.Item1));

				CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, Value.ReturnType, dynamicContext);

				Expression block = Expression.Block(subLocals.Zip(parExps, (l, p) => Expression.Assign(Expression.Property(l.Value, "Value"), Expression.Convert(p, typeof(object)))).Concat(new Expression[] { Expression.Dynamic(binder, Value.ReturnType, exp) }));

				Type type = funcTypes[pars.Count].MakeGenericType(pars.Select(t => t.Item2).Concat(new[] { Value.ReturnType }).ToArray());
				MethodInfo method = typeof(Expression).GetMethods().FirstOrDefault(m => m.Name == "Lambda" && m.IsGenericMethod && m.GetParameters().Length == 2).MakeGenericMethod(type);
				object func = ((dynamic)method.Invoke(null, new object[] { block, parExps.ToArray() })).Compile();
				
				Expression ret = Expression.Block(subLocals.Skip(parExps.Count).Select(kvp => Expression.Assign(Expression.Property(kvp.Value, "Value"), parameters.Select(p => new Tuple<string, Expression>(p.Name, p)).Concat(locals.Select(k => new Tuple<string, Expression>(k.Key, Expression.Property(k.Value, "Value")))).First(p => p.Item1 == kvp.Key).Item2)).Concat(new [] { Expression.Constant(func) as Expression }));

				return ret;
			}
			else
			{
				List<ConstantExpression> newLocals = new List<ConstantExpression>();
				foreach (var arg in Arguments.Arguments.Cast<LambdaAssignmentToken>())
				{
					if (locals.Any(name => name.Key == arg.Name))
						throw new Exception("Duplicate local variable name \"" + arg.Name + "\" found.");
					var container = new DataContainer();
					var value = Expression.Constant(container);
					dataContainers.Add(container);
					newLocals.Add(value);
					locals.Add(arg.Name, value);
				}
				IEnumerable<BinaryExpression> assignments = Arguments.Arguments.Cast<LambdaAssignmentToken>().Zip(newLocals, (t, l) => Expression.Assign(Expression.Property(l, "Value"), t.Value.GetExpression(parameters, locals, dataContainers, dynamicContext, label)));
				Expression ret = Expression.Block(assignments.Cast<Expression>().Concat(new Expression[] {Value.GetExpression(parameters, locals, dataContainers, dynamicContext, label)}));
				foreach (var arg in Arguments.Arguments.Cast<LambdaAssignmentToken>())
					locals.Remove(arg.Name);
				return ret;
			}
		}
	}
}
