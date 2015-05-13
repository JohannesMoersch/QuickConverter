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
	public class InstanceFunctionToken : TokenBase, IPostToken
	{
		private static PropertyInfo SupportsExtensions = typeof(EquationTokenizer).GetProperty("SupportsExtensionMethods", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo GetMethod = typeof(EquationTokenizer).GetMethod("GetMethod", BindingFlags.NonPublic | BindingFlags.Static);
		private static MethodInfo InvokeMethod = typeof(MethodInfo).GetMethods().First(m => m.Name == "Invoke" && m.GetParameters().Length == 2);
		private static PropertyInfo Item1Prop = typeof(Tuple<MethodInfo, object[]>).GetProperty("Item1");
		private static PropertyInfo Item2Prop = typeof(Tuple<MethodInfo, object[]>).GetProperty("Item2");

		internal InstanceFunctionToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Arguments, Target }; } }

		public string MethodName { get; private set; }
		public ArgumentListToken Arguments { get; private set; }
		public Type[] Types { get; private set; }
		public TokenBase Target { get; private set; }

		internal override bool SetPostTarget(TokenBase target)
		{
			Target = target;
			return true;
		}

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			string temp = text;
			if (temp.Length < 2 || temp[0] != '.' || (!Char.IsLetter(temp[1]) && temp[1] != '_'))
				return false;
			int count = 2;
			while (count < temp.Length && (Char.IsLetterOrDigit(temp[count]) || temp[count] == '_'))
				++count;
			string name = temp.Substring(1, count - 1);
			temp = temp.Substring(count).TrimStart();
			if (temp.Length == 0)
				return false;
			List<Type> typeArgs = null;
			if (temp[0] == '[')
			{
				List<string> list;
				if (!TrySplitByCommas(ref temp, '[', ']', out list))
					return false;
				typeArgs = new List<Type>();
				foreach (string str in list)
				{
					Tuple<object, string> tuple = GetNameMatches(str.Trim(), null, null).FirstOrDefault(tp => tp.Item1 is Type && string.IsNullOrWhiteSpace(tp.Item2));
					if (tuple == null)
						return false;
					typeArgs.Add(tuple.Item1 as Type);
				}
			}
			TokenBase args;
			if (!new ArgumentListToken('(', ')').TryGetToken(ref temp, out args))
				return false;
			text = temp;
			token = new InstanceFunctionToken() { Arguments = args as ArgumentListToken, MethodName = name, Types = typeArgs != null ? typeArgs.ToArray() : null };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			CallSiteBinder binder = Binder.InvokeMember(requiresReturnValue ? CSharpBinderFlags.None : CSharpBinderFlags.ResultDiscarded, MethodName, Types, dynamicContext ?? typeof(object), new object[Arguments.Arguments.Length + 1].Select(val => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)));
			Expression dynamicCall = Expression.Dynamic(binder, requiresReturnValue ? typeof(object) : typeof(void), new[] { Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label) }.Concat(Arguments.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))));

			var targetVar = Expression.Variable(typeof(object));
			var argsVar = Expression.Variable(typeof(object[]));
			var methodVar = Expression.Variable(typeof(Tuple<MethodInfo, object[]>));

			Expression ret;
			
			if (requiresReturnValue)
			{
				var resultVar = Expression.Variable(typeof(object));

				Expression test = Expression.Equal(methodVar, Expression.Constant(null, typeof(Tuple<MethodInfo, object[]>)));
				Expression ifNotNull = Expression.Assign(resultVar, Expression.Call(Expression.Property(methodVar, Item1Prop), InvokeMethod, Expression.Constant(null), Expression.Property(methodVar, Item2Prop)));
				Expression ifNull = Expression.Assign(resultVar, dynamicCall);

				Expression branch = Expression.IfThenElse(test, ifNull, ifNotNull);

				Expression block = Expression.Block(new[] { targetVar, argsVar, methodVar }, new[] 
				{
					Expression.Assign(targetVar, Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label)),
					Expression.Assign(argsVar, Expression.NewArrayInit(typeof(object), new[] { targetVar }.Concat(Arguments.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))))),
					Expression.Assign(methodVar, Expression.Call(GetMethod, Expression.Constant(MethodName, typeof(string)), Expression.Constant(Types, typeof(Type[])), argsVar)),
					branch,
					resultVar
				});

				ret = Expression.Block(new[] { resultVar }, new Expression[] { Expression.IfThenElse(Expression.Property(null, SupportsExtensions), block, ifNull), resultVar });
			}
			else
			{
				Expression test = Expression.Equal(methodVar, Expression.Constant(null, typeof(Tuple<MethodInfo, object[]>)));
				Expression ifNotNull = Expression.Call(Expression.Property(methodVar, Item1Prop), InvokeMethod, Expression.Constant(null), Expression.Property(methodVar, Item2Prop));
				Expression ifNull = dynamicCall;

				Expression branch = Expression.IfThenElse(test, ifNull, ifNotNull);

				Expression block = Expression.Block(typeof(void), new[] { targetVar, argsVar, methodVar }, new[] 
				{
					Expression.Assign(targetVar, Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label)),
					Expression.Assign(argsVar, Expression.NewArrayInit(typeof(object), new[] { targetVar }.Concat(Arguments.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))))),
					Expression.Assign(methodVar, Expression.Call(GetMethod, Expression.Constant(MethodName, typeof(string)), Expression.Constant(Types, typeof(Type[])), argsVar)),
					branch
				});

				ret = Expression.IfThenElse(Expression.Property(null, SupportsExtensions), block, ifNull);
			}

			return ret;
		}
	}
}
