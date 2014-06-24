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

		public override Type ReturnType { get { return method.ReturnType; } } 

		private MethodInfo method;
		private ArgumentListToken arguments;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			var list = GetNameMatches(text, null, null).Where(tup => tup.Item1 is MethodInfo && (tup.Item1 as MethodInfo).ReturnType != typeof(void)).ToArray();
			Tuple<MethodInfo, TokenBase, string> info = null;
			foreach (var method in list)
			{
				string temp = method.Item2;
				TokenBase args;
				if (!new ArgumentListToken('(', ')').TryGetToken(ref temp, out args))
					continue;
				if ((args as ArgumentListToken).Arguments.Count <= (method.Item1 as MethodInfo).GetParameters().Length)
				{
					bool good = true;
					for (int i = 0; i < (method.Item1 as MethodInfo).GetParameters().Length; ++i)
					{
						if (i < (args as ArgumentListToken).Arguments.Count)
						{
							if ((args as ArgumentListToken).Arguments[i].ReturnType.IsAssignableFrom((method.Item1 as MethodInfo).GetParameters()[i].ParameterType) || (method.Item1 as MethodInfo).GetParameters()[i].ParameterType.IsAssignableFrom((args as ArgumentListToken).Arguments[i].ReturnType))
								continue;
						}
						else if ((method.Item1 as MethodInfo).GetParameters()[i].IsOptional)
							continue;
						good = false;
						break;
					}
					if (!good)
						continue;
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

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext)
		{
			ParameterInfo[] pars = method.GetParameters();
			Expression[] args = new Expression[pars.Length];
			for (int i = 0; i < pars.Length; ++i)
			{
				if (i < arguments.Arguments.Count)
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, pars[i].ParameterType, dynamicContext ?? typeof(object));
					args[i] = Expression.Dynamic(binder, pars[i].ParameterType, arguments.Arguments[i].GetExpression(parameters, locals, dataContainers, dynamicContext));
				}
				else
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, pars[i].ParameterType, dynamicContext ?? typeof(object));
					args[i] = Expression.Dynamic(binder, pars[i].ParameterType, Expression.Constant(pars[i].DefaultValue, typeof(object)));
				}
			}
			return Expression.Convert(Expression.Call(method, args), typeof(object));
		}
	}
}
