using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class IsToken : TokenBase, IPostToken
	{
		internal IsToken()
		{
		}

		public TokenBase Target { get; set; }
		private Type type;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text.TrimStart();
			if (!temp.StartsWith("is"))
				return false;
			temp = temp.Substring(2).TrimStart();
			var name = GetNameMatches(temp, null, null).Reverse().FirstOrDefault(tuple => tuple.Item1 is Type);
			if (name == null || (name.Item2.Length != 0 && name.Item2[0] == '.'))
				return false;
			text = name.Item2.TrimStart();
			token = new IsToken() { type = name.Item1 as Type };
			return true;
		}

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			return Expression.Convert(Expression.TypeIs(Target.GetExpression(parameters, dynamicContext), type), typeof(object));
		}
	}
}
