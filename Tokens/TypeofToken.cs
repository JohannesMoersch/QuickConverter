using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class TypeofToken : TokenBase
	{
		internal TypeofToken()
		{
		}

		public override Type ReturnType { get { return typeof(Type); } }

		public override TokenBase[] Children { get { return new TokenBase[0]; } }

		public Type Type { get; private set; }
		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			if (!text.StartsWith("typeof"))
				return false;
			string temp = text.Substring(6).TrimStart();
			if (temp.Length < 3 || temp[0] != '(')
				return false;
			var name = GetNameMatches(temp.Substring(1), null, null).FirstOrDefault(tuple => tuple.Item1 is Type && tuple.Item2.TrimStart().StartsWith(")"));
			if (name == null)
				return false;
			text = name.Item2.TrimStart().Substring(1);
			token = new TypeofToken() { Type = name.Item1 as Type };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			return Expression.Constant(Type, typeof(object));
		}
	}
}
