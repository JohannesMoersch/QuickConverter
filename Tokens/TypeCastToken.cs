using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class TypeCastToken : TokenBase
	{
		internal TypeCastToken()
		{
		}

		private Type type;
		private TokenBase target;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			bool inQuotes = false;
			int brackets = 0;
			int i = 0;
			while (true)
			{
				if (i >= text.Length)
					return false;
				if (i > 0 && text[i] == '\'' && text[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (text[i] == '(')
						++brackets;
					else if (text[i] == ')')
					{
						--brackets;
						if (brackets == 0)
							break;
					}
				}
				++i;
			}
			string temp = text.Substring(1, i - 1);

			var tuple = GetNameMatches(temp, null, null).Where(tup => tup.Item1 is Type).Reverse().FirstOrDefault();
			if (tuple == null)
				return false;

			temp = text.Substring(i + 1).TrimStart();
			TokenBase valToken;
			if (!EquationTokenizer.TryGetValueToken(ref temp, out valToken))
				return false;
			text = temp;
			token = new TypeCastToken() { type = tuple.Item1 as Type, target = valToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, type, dynamicContext ?? typeof(object));
			return Expression.Convert(Expression.Dynamic(binder, type, target.GetExpression(parameters, locals, dynamicContext)), typeof(object));
		}
	}
}
