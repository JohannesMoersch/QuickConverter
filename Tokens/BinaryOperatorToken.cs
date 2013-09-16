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
			ExpressionType.And,
			ExpressionType.Or,
			ExpressionType.AndAlso,
			ExpressionType.OrElse,
			default(ExpressionType)
		};

		private TokenBase left;
		private TokenBase right;
		private Operator operation;
		internal BinaryOperatorToken(TokenBase left, TokenBase right, Operator operation)
		{
			this.left = left;
			this.right = right;
			this.operation = operation;
		}

		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			throw new NotImplementedException();
		}

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			if (operation == Operator.And)
				return Expression.Convert(Expression.AndAlso(Expression.Convert(left.GetExpression(parameters, dynamicContext), typeof(bool)), Expression.Convert(right.GetExpression(parameters, dynamicContext), typeof(bool))), typeof(object));
			if (operation == Operator.Or)
				return Expression.Convert(Expression.OrElse(Expression.Convert(left.GetExpression(parameters, dynamicContext), typeof(bool)), Expression.Convert(right.GetExpression(parameters, dynamicContext), typeof(bool))), typeof(object));
			CallSiteBinder binder = Binder.BinaryOperation(CSharpBinderFlags.None, types[(int)operation], dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), left.GetExpression(parameters, dynamicContext), right.GetExpression(parameters, dynamicContext));
		}
	}
}
