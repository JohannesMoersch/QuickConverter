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

		private ArgumentListToken Arguments;
		private TokenBase Value;
		private bool lambda;

		internal override bool TryGetToken(ref string text, out TokenBase token)
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
			token = new LambdaToken() { Arguments = arguments as ArgumentListToken, Value = method, lambda = lambda };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			if (lambda)
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
					subLocals.Add(tuple.Item1, Expression.Constant(new DataContainer()));

				List<ParameterExpression> parExps = new List<ParameterExpression>();
				Expression exp = Value.GetExpression(parExps, subLocals, dynamicContext);
				if (parExps.Any())
					throw new Exception("Lambda expression contained unknown parameter or variable \"" + parExps.First().Name + "\".");
				foreach (var tuple in pars)
					parExps.Add(Expression.Parameter(tuple.Item2, tuple.Item1));

				Type targetType = (Value is TypeCastToken) ? (Value as TypeCastToken).TargetType : typeof(object);

				CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, targetType, dynamicContext);

				Expression block = Expression.Block(subLocals.Zip(parExps, (l, p) => Expression.Assign(Expression.Field(l.Value, "Value"), Expression.Convert(p, typeof(object)))).Concat(new Expression[] { Expression.Dynamic(binder, targetType, exp) }));

				Type type = funcTypes[pars.Count].MakeGenericType(pars.Select(t => t.Item2).Concat(new[] { targetType }).ToArray());
				MethodInfo method = typeof(Expression).GetMethods().FirstOrDefault(m => m.Name == "Lambda" && m.IsGenericMethod && m.GetParameters().Length == 2).MakeGenericMethod(type);
				object func = ((dynamic)method.Invoke(null, new object[] { block, parExps.ToArray() })).Compile();
				
				return Expression.Constant(func);
			}
			else
			{
				List<ConstantExpression> newLocals = new List<ConstantExpression>();
				foreach (var arg in Arguments.Arguments.Cast<AssignmentToken>())
				{
					if (locals.Any(name => name.Key == arg.Name))
						throw new Exception("Duplicate local variable name \"" + arg.Name + "\" found.");
					var value = Expression.Constant(new DataContainer());
					newLocals.Add(value);
					locals.Add(arg.Name, value);
				}
				IEnumerable<BinaryExpression> assignments = Arguments.Arguments.Cast<AssignmentToken>().Zip(newLocals, (t, l) => Expression.Assign(Expression.Field(l, "Value"), t.Value.GetExpression(parameters, locals, dynamicContext)));
				Expression ret = Expression.Block(assignments.Cast<Expression>().Concat(new Expression[] {Value.GetExpression(parameters, locals, dynamicContext)}));
				foreach (var arg in Arguments.Arguments.Cast<AssignmentToken>())
					locals.Remove(arg.Name);
				return ret;
			}
		}
	}
}
