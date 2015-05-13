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

		public override Type ReturnType { get { return Condition.ReturnType == OnNull.ReturnType ? Condition.ReturnType : typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Condition, OnNull }; } }

		public TokenBase Condition { get; private set; }
		public TokenBase OnNull { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
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
			token = new NullCoalesceOperatorToken() { Condition = left, OnNull = right };
			text = "";
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			Expression c = Condition.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			Expression n = OnNull.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			return Expression.Coalesce(Expression.Convert(c, typeof(object)), Expression.Convert(n, typeof(object)));
		}
	}
}
