using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class AsToken : TokenBase, IPostToken
	{
		internal AsToken()
		{
		}

		public TokenBase Target { get; set; }
		private Type type;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text.TrimStart();
			if (!temp.StartsWith("as"))
				return false;
			temp = temp.Substring(2).TrimStart();
			var name = GetNameMatches(temp, null, null).Reverse().FirstOrDefault(tuple => tuple.Item1 is Type);
			if (name == null || (name.Item2.Length != 0 && name.Item2[0] == '.'))
				return false;
			text = name.Item2.TrimStart();
			token = new AsToken() { type = name.Item1 as Type };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			return Expression.Convert(Expression.TypeAs(Target.GetExpression(parameters, locals, dynamicContext), type), typeof(object));
		}
	}
}
