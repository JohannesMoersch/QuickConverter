using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class TernaryOperatorToken : TokenBase
	{
		internal TernaryOperatorToken()
		{
		}

		public override Type ReturnType { get { return OnTrue.ReturnType == OnFalse.ReturnType ? OnTrue.ReturnType : typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Condition, OnTrue, OnFalse }; } }

		public TokenBase Condition { get; private set; }
		public TokenBase OnTrue { get; private set; }
		public TokenBase OnFalse { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			bool inQuotes = false;
			int brackets = 0;
			int count = 0;
			int i = 0;
			int qPos = -1;
			int cPos = -1;
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
						--brackets;
					else if (brackets == 0)
					{
						if (text[i] == '?' && (i >= text.Length - 1 || (text[i + 1] != '.' && text[i + 1] != '[')))
						{
							if (count == 0)
								qPos = i;
							++count;
						}
						else if (text[i] == ':')
						{
							--count;
							if (count < 0)
								return false;
							if (count == 0)
							{
								cPos = i;
								break;
							}
						}
					}
				}
				++i;
			}
			TokenBase left, middle, right;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(0, qPos).Trim(), out left))
				return false;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(qPos + 1, cPos - qPos - 1).Trim(), out middle))
				return false;
			if (!EquationTokenizer.TryEvaluateExpression(text.Substring(cPos + 1).Trim(), out right))
				return false;
			token = new TernaryOperatorToken() { Condition = left, OnTrue = middle, OnFalse = right };
			text = "";
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, typeof(bool), typeof(object));
			Expression c = Condition.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			Expression t = OnTrue.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			Expression f = OnFalse.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			return Expression.Condition(Expression.Dynamic(binder, typeof(bool), c), Expression.Convert(t, typeof(object)), Expression.Convert(f, typeof(object)));
		}
	}
}
