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

		private MemberInfo member;
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			var tuple = GetNameMatches(text, null, null).Where(tup => tup.Item1 is FieldInfo || tup.Item1 is PropertyInfo).Reverse().FirstOrDefault();
			if (tuple == null)
				return false;
			text = tuple.Item2;
			token = new StaticMemberToken() { member = tuple.Item1 as MemberInfo };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext)
		{
			return Expression.Convert(Expression.MakeMemberAccess(null, member), typeof(object));
		}
	}
}
