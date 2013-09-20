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
	public class StaticFunctionToken : TokenBase
	{
		internal StaticFunctionToken()
		{
		}

		private MethodInfo method;
		private ArgumentListToken arguments;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			var list = GetNameMatches(text, null, null).Where(tup => tup.Item1 is MethodInfo).ToArray();
			Tuple<MethodInfo, TokenBase, string> info = null;
			foreach (var method in list)
			{
				string temp = method.Item2;
				TokenBase args;
				if (!new ArgumentListToken('(', ')').TryGetToken(ref temp, out args))
					continue;
				if ((args as ArgumentListToken).Arguments.Count == (method.Item1 as MethodInfo).GetParameters().Length)
				{
					info = new Tuple<MethodInfo, TokenBase, string>(method.Item1 as MethodInfo, args, temp);
					break;
				}
			}
			if (info == null)
				return false;
			text = info.Item3;
			token = new StaticFunctionToken() { arguments = info.Item2 as ArgumentListToken, method = info.Item1 };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			Expression[] args = new Expression[arguments.Arguments.Count];
			ParameterInfo[] pars = method.GetParameters();
			for (int i = 0; i < pars.Length; ++i)
			{
				CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, pars[i].ParameterType, dynamicContext ?? typeof(object));
				args[i] = Expression.Dynamic(binder, pars[i].ParameterType, arguments.Arguments[i].GetExpression(parameters, locals, dynamicContext));
			}
			return Expression.Convert(Expression.Call(method, args), typeof(object));
		}
	}
}
