using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace QuickConverter.Tokens
{
	public class NullPropagatingToken : TokenBase, IPostToken
	{
		internal NullPropagatingToken()
		{
		}

		public override Type ReturnType { get { return (this as IPostToken).Target.ReturnType; } } 

		TokenBase IPostToken.Target { get; set; }
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			string temp = text;
			if (temp.Length < 2 || temp[0] != '?' || (temp[1] != '.' && temp[1] != '['))
				return false;
			text = temp.Substring(1);
			token = new NullPropagatingToken();
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label)
		{
			var container = new DataContainer();
			var constant = Expression.Constant(container);
			dataContainers.Add(container);

			return Expression.Block(new Expression[]
			{
				Expression.Assign(Expression.Property(constant, "Value"), (this as IPostToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label)),
				Expression.IfThen(Expression.Equal(Expression.Property(constant, "Value"), Expression.Default(typeof(object))), Expression.Goto(label, Expression.Default(typeof(object)))),
				Expression.Property(constant, "Value")
			});
		}
	}
}
