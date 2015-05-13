using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QuickConverter.Tokens
{
	public class LambdaAssignmentToken : TokenBase
	{
		private Type type;
		internal LambdaAssignmentToken(Type type)
		{
			this.type = type;
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Value }; } }

		public string Name { get; private set; }
		public MemberInfo Member { get; private set; }
		public TokenBase Value { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			string temp = text;
			if (temp.Length == 0 || (!Char.IsLetter(temp[0]) && temp[0] != '_'))
				return false;
			int count = 1;
			while (count < temp.Length && (Char.IsLetterOrDigit(temp[count]) || temp[count] == '_'))
				++count;
			string name = temp.Substring(0, count);

			MemberInfo info = null;
			if (type != null)
			{
				info = type.GetMember(name).FirstOrDefault();
				if (info == null)
					return false;
			}
			else
			{
				string nameTemp = "$" + name;
				TokenBase tokenTemp;
				if (!new ParameterToken().TryGetToken(ref nameTemp, out tokenTemp))
					return false;
			}

			temp = temp.Substring(count).TrimStart();
			if (temp.Length == 0 || temp[0] != '=')
				return false;
			temp = temp.Substring(1).TrimStart();
			TokenBase valToken;
			if (!EquationTokenizer.TryEvaluateExpression(temp, out valToken))
				return false;
			text = "";
			token = new LambdaAssignmentToken(null) { Name = name, Member = info, Value = valToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			return Value.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
		}
	}
}
