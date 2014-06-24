using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class BracketedToken : TokenBase
	{
		internal BracketedToken()
		{
		}

		public override Type ReturnType { get { return value.ReturnType; } } 

		private TokenBase value;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			if (!text.TrimStart().StartsWith("("))
				return false;
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
			TokenBase valToken;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(1, i - 1), out valToken))
				return false;
			text = text.Substring(i + 1);
			token = new BracketedToken() { value = valToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext)
		{
			return value.GetExpression(parameters, locals, dataContainers, dynamicContext);
		}
	}
}
