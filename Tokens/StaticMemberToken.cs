using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QuickConverter.Tokens
{
	public class StaticMemberToken : TokenBase
	{
		internal StaticMemberToken()
		{
		}

		public override Type ReturnType { get { return Member is FieldInfo ? (Member as FieldInfo).FieldType : (Member as PropertyInfo).PropertyType; } }

		public override TokenBase[] Children { get { return new TokenBase[0]; } }

		public MemberInfo Member { get; private set; }

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			var tuple = GetNameMatches(text, null, null).Where(tup => tup.Item1 is FieldInfo || tup.Item1 is PropertyInfo).Reverse().FirstOrDefault();
			if (tuple == null)
				return false;
			text = tuple.Item2;
			token = new StaticMemberToken() { Member = tuple.Item1 as MemberInfo };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			return Expression.Convert(Expression.MakeMemberAccess(null, Member), typeof(object));
		}
	}
}
