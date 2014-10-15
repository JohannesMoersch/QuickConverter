using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class PostTokenChainToken : TokenBase, IPostToken
	{
		internal PostTokenChainToken(TokenBase token)
		{
			(this as IPostToken).Target = token;
		}

		public override Type ReturnType { get { return (this as IPostToken).Target.ReturnType; } } 

		TokenBase IPostToken.Target { get; set; }
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label)
		{
			label = Expression.Label((this as IPostToken).Target.ReturnType);
			return Expression.Label(label, (this as IPostToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label));
		}
	}
}
