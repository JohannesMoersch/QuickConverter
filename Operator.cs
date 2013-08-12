using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	internal enum Operator
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
		Or = 15,
		Ternary = 16,
	}
}
