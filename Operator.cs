using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	public enum Operator
	{
		Positive = 0,
		Negative = 1,
		Not = 2,
		Multiply = 3,
		Divide = 4,
		Modulus = 5,
		Add = 6,
		Subtract = 7,
		GreaterOrEqual = 8,
		LessOrEqual = 9,
		GreaterThan = 10,
		LessThan = 11,
		Equals = 12,
		NotEquals = 13,
		And = 14,
		AlternateAnd = 15,
		Or = 16,
		BitwiseAnd = 17,
		BitwiseAlternateAnd = 18,
		BitwiseOr = 19,
		BitwiseXor = 20,
		Ternary = 21
	}
}
