using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class ParameterToken : TokenBase
	{
		internal ParameterToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new TokenBase[0]; } }

		public string Name { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			if (text.Length < 2 || text[0] != '$' || (!Char.IsLetter(text[1]) && text[1] != '_'))
				return false;
			int count = 2;
			while (count < text.Length && (Char.IsLetterOrDigit(text[count]) || text[count] == '_'))
				++count;
			token = new ParameterToken() { Name = text.Substring(1, count - 1) };
			text = text.Substring(count);
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			if (locals.ContainsKey(Name))
				return Expression.Property(locals[Name], "Value");
			ParameterExpression par = parameters.FirstOrDefault(p => p.Name == Name);
			if (par == null)
			{
				par = Expression.Parameter(typeof(object), Name);
				parameters.Add(par);
			}
			return par;
		}
	}
}
