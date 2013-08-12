using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QuickConverter.Tokens
{
	public class AssignmentToken : TokenBase
	{
		private Type type;
		internal AssignmentToken(Type type)
		{
			this.type = type;
		}

		internal MemberInfo Member { get; private set; }
		internal TokenBase Value { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text;
			if (temp.Length == 0 || (!Char.IsLetter(temp[0]) && temp[0] != '_'))
				return false;
			int count = 1;
			while (count < temp.Length && (Char.IsLetterOrDigit(temp[count]) || temp[count] == '_'))
				++count;
			string name = temp.Substring(0, count);

			MemberInfo info = type.GetMember(name).FirstOrDefault();
			if (info == null)
				return false;

			temp = temp.Substring(count).TrimStart();
			if (temp.Length == 0 || temp[0] != '=')
				return false;
			temp = temp.Substring(1).TrimStart();
			TokenBase valToken;
			if (!EquationTokenizer.TryEvaluateExpression(temp, out valToken))
				return false;
			text = "";
			token = new AssignmentToken(null) { Member = info, Value = valToken };
			return true;
		}

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			return Value.GetExpression(parameters, dynamicContext);
		}
	}
}
