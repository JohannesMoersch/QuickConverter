using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class UnaryOperatorToken : TokenBase
	{
		internal UnaryOperatorToken()
		{
		}

		private Operator operation;
		private TokenBase value;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			if (text.Length == 0)
				return false;
			Operator op;
			if (text[0] == '!')
				op = Operator.Not;
			else if (text[0] == '+')
				op = Operator.Positive;
			else if (text[0] == '-')
				op = Operator.Negative;
			else
				return false;
			TokenBase valToken;
			string temp = text.Substring(1).TrimStart();
			if (!EquationTokenizer.TryGetValueToken(ref temp, out valToken))
				return false;
			token = new UnaryOperatorToken() { operation = op, value = valToken };
			text = temp;
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext)
		{
			ExpressionType type = default(ExpressionType);
			switch (operation)
			{
				case Operator.Positive:
					return value.GetExpression(parameters, locals, dataContainers, dynamicContext);
					break;
				case Operator.Negative:
					type = ExpressionType.Negate;
					break;
				case Operator.Not:
					type = ExpressionType.Not;
					break;
			}
			CallSiteBinder binder = Binder.UnaryOperation(CSharpBinderFlags.None, type, dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), value.GetExpression(parameters, locals, dataContainers, dynamicContext));
		}
	}
}
