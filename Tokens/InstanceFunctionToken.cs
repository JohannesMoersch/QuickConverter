using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class InstanceFunctionToken : TokenBase, IPostToken
	{
		internal InstanceFunctionToken()
		{
		}

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

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			CallSiteBinder binder = Binder.InvokeMember(CSharpBinderFlags.None, methodName, types, dynamicContext ?? typeof(object), new object[arguments.Arguments.Count + 1].Select(val => CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)));
			return Expression.Dynamic(binder, typeof(object), new[] { (this as IPostToken).Target.GetExpression(parameters, dynamicContext) }.Concat(arguments.Arguments.Select(token => token.GetExpression(parameters))));
		}
	}
}
