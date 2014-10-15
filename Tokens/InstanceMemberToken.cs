using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class InstanceMemberToken : TokenBase, IPostToken
	{
		internal InstanceMemberToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } } 

		private string memberName;
		TokenBase IPostToken.Target { get; set; }
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text;
			if (temp.Length < 2 || temp[0] != '.' || (!Char.IsLetter(temp[1]) && temp[1] != '_'))
				return false;
			int count = 2;
			while (count < temp.Length && (Char.IsLetterOrDigit(temp[count]) || temp[count] == '_'))
				++count;
			if (count < temp.Length && temp[count] == '(')
				return false;
			string name = temp.Substring(1, count - 1);
			text = temp.Substring(count);
			token = new InstanceMemberToken() { memberName = name };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label)
		{
			CallSiteBinder binder = Binder.GetMember(CSharpBinderFlags.None, memberName, dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), (this as IPostToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label));
		}
	}
}
