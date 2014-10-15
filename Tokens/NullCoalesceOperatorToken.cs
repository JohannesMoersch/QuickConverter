using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class NullCoalesceOperatorToken : TokenBase
	{
		internal NullCoalesceOperatorToken()
		{
		}

		public override Type ReturnType { get { return condition.ReturnType == onNull.ReturnType ? condition.ReturnType : typeof(object); } } 

		private TokenBase condition;
		private TokenBase onNull;

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			bool inQuotes = false;
			int brackets = 0;
			int i = 0;
			int qPos = -1;
			while (true)
			{
				if (i >= text.Length - 1)
					return false;
				if (i > 0 && text[i] == '\'' && text[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (text[i] == '(')
						++brackets;
					else if (text[i] == ')')
						--brackets;
					else if (brackets == 0 && text[i] == '?' && text[i + 1] == '?')
					{
						qPos = i;
						break;
					}
				}
				++i;
			}
			TokenBase left, right;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(0, qPos).Trim(), out left))
				return false;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(qPos + 2).Trim(), out right))
				return false;
			token = new NullCoalesceOperatorToken() { condition = left, onNull = right };
			text = "";
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label)
		{
			Expression c = condition.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			Expression n = onNull.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			return Expression.Coalesce(Expression.Convert(c, typeof(object)), Expression.Convert(n, typeof(object)));
		}
	}
}
