using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class ArrayAccessToken : TokenBase, IPostToken
	{
		internal ArrayAccessToken()
		{
		}

		private ArgumentListToken index;
		TokenBase IPostToken.Target { get; set; }
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text;
			if (temp.Length < 3 || temp[0] != '[')
				return false;

			bool inQuotes = false;
			int brackets = 0;
			int i = 0;
			while (true)
			{
				if (i >= temp.Length)
					return false;
				if (i > 0 && temp[i] == '\'' && temp[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (temp[i] == '[')
						++brackets;
					else if (temp[i] == ']')
					{
						--brackets;
						if (brackets == 0)
							break;
					}
				}
				++i;
			}

			TokenBase ind;
			if (!new ArgumentListToken('[', ']').TryGetToken(ref temp, out ind))
				return false;
			text = text.Substring(i + 1);
			token = new ArrayAccessToken() { index = ind as ArgumentListToken };
			return true;
		}

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			CallSiteBinder binder = Binder.GetIndex(CSharpBinderFlags.None, dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), new Expression[] { (this as IPostToken).Target.GetExpression(parameters, dynamicContext) }.Concat(index.Arguments.Select(token => token.GetExpression(parameters, dynamicContext))));
		}
	}
}
