using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class BinaryOperatorToken : TokenBase
	{
		private static ExpressionType[] types = new[] 
		{ 
			default(ExpressionType), 
			default(ExpressionType), 
			default(ExpressionType), 
			ExpressionType.Multiply, 
			ExpressionType.Divide, 
			ExpressionType.Modulo, 
			ExpressionType.Add, 
			ExpressionType.Subtract, 
			ExpressionType.GreaterThanOrEqual,
			ExpressionType.LessThanOrEqual,
			ExpressionType.GreaterThan,
			ExpressionType.LessThan,
			ExpressionType.Equal,
			ExpressionType.NotEqual,
			ExpressionType.AndAlso,
			ExpressionType.AndAlso,
			ExpressionType.OrElse,
			ExpressionType.And,
			ExpressionType.And,
			ExpressionType.Or,
			ExpressionType.ExclusiveOr,
			default(ExpressionType)
		};

		public override Type ReturnType { get { return Operation >= Operator.GreaterOrEqual && Operation <= Operator.Or ? typeof(bool) : typeof(object); } } 

		private TokenBase left;
		private TokenBase right;
		internal Operator Operation { get; set; }

		internal BinaryOperatorToken(TokenBase left, TokenBase right, Operator operation)
		{
			this.left = left;
			this.right = right;
			this.Operation = operation;
		}

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			throw new NotImplementedException();
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext)
		{
			if (Operation == Operator.And || Operation == Operator.AlternateAnd)
				return Expression.Convert(Expression.AndAlso(Expression.Convert(left.GetExpression(parameters, locals, dataContainers, dynamicContext), typeof(bool)), Expression.Convert(right.GetExpression(parameters, locals, dataContainers, dynamicContext), typeof(bool))), typeof(object));
			if (Operation == Operator.Or)
				return Expression.Convert(Expression.OrElse(Expression.Convert(left.GetExpression(parameters, locals, dataContainers, dynamicContext), typeof(bool)), Expression.Convert(right.GetExpression(parameters, locals, dataContainers, dynamicContext), typeof(bool))), typeof(object));
			CallSiteBinder binder = Binder.BinaryOperation(CSharpBinderFlags.None, types[(int)Operation], dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), left.GetExpression(parameters, locals, dataContainers, dynamicContext), right.GetExpression(parameters, locals, dataContainers, dynamicContext));
		}
	}
}
