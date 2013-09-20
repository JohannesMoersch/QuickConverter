using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class LambdaToken : TokenBase
	{
		internal LambdaToken()
		{
		}

		private ArgumentListToken Arguments;
		private TokenBase Value;

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			TokenBase arguments;
			if (!new ArgumentListToken('(', ')', null).TryGetToken(ref text, out arguments))
				return false;
			string temp = text.TrimStart();
			if (!temp.StartsWith("=>"))
				return false;
			temp = temp.Substring(2).TrimStart();
			TokenBase method;
			if (!EquationTokenizer.TryEvaluateExpression(temp, out method))
				return false;
			text = "";
			token = new LambdaToken() { Arguments = arguments as ArgumentListToken, Value = method };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			List<ConstantExpression> newLocals = new List<ConstantExpression>();
			foreach (var arg in Arguments.Arguments.Cast<AssignmentToken>())
			{
				if (locals.Any(name => name.Key == arg.Name))
					throw new Exception("Duplicate local variable name \"" + arg.Name + "\" found.");
				var value = Expression.Constant(new DataContainer());
				newLocals.Add(value);
				locals.Add(arg.Name, value);
			}
			IEnumerable<BinaryExpression> assignments = Arguments.Arguments.Cast<AssignmentToken>().Zip(newLocals, (t, l) => Expression.Assign(Expression.Field(l, "Value"), t.Value.GetExpression(parameters, locals, dynamicContext)));
			return Expression.Block(assignments.Cast<Expression>().Concat(new Expression[] { Value.GetExpression(parameters, locals, dynamicContext) }));
		}
	}
}
