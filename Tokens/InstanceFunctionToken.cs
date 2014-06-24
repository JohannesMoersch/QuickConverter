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

		private string methodName;
		private ArgumentListToken arguments;
		private List<Type> types;
		TokenBase IPostToken.Target { get; set; }
		internal override bool TryGetToken(ref string text, out TokenBase token)
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
			token = new InstanceFunctionToken() { arguments = args as ArgumentListToken, methodName = name, types = typeArgs };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext)
		{
			CallSiteBinder binder = Binder.InvokeMember(CSharpBinderFlags.None, methodName, types, dynamicContext ?? typeof(object), new object[arguments.Arguments.Count + 1].Select(val => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)));
			Expression dynamicCall = Expression.Dynamic(binder, typeof(object), new[] { (this as IPostToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext) }.Concat(arguments.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext))));

			var targetVar = Expression.Variable(typeof(object));
			var argsVar = Expression.Variable(typeof(object[]));
			var methodVar = Expression.Variable(typeof(Tuple<MethodInfo, object[]>));
			var resultVar = Expression.Variable(typeof(object));

			Expression test = Expression.Equal(methodVar, Expression.Constant(null, typeof(Tuple<MethodInfo, object[]>)));
			Expression ifNotNull = Expression.Assign(resultVar, Expression.Call(Expression.Property(methodVar, Item1Prop), InvokeMethod, Expression.Constant(null), Expression.Property(methodVar, Item2Prop)));
			Expression ifNull = Expression.Assign(resultVar, dynamicCall);

			Expression branch = Expression.IfThenElse(test, ifNull, ifNotNull);

			Expression block = Expression.Block(new[] { targetVar, argsVar, methodVar }, new[] 
				{
					Expression.Assign(targetVar, (this as IPostToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext)),
					Expression.Assign(argsVar, Expression.NewArrayInit(typeof(object), new[] { targetVar }.Concat(arguments.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext))))),
					Expression.Assign(methodVar, Expression.Call(GetMethod, Expression.Constant(methodName, typeof(string)), Expression.Constant(types, typeof(List<Type>)), argsVar)),
					branch,
					resultVar
				});

			Expression ret = Expression.Block(new[] { resultVar }, new Expression[] { Expression.IfThenElse(Expression.Property(null, SupportsExtensions), block, ifNull), resultVar });

			return ret;
		}
	}
}
