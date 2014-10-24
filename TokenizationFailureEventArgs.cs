using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	public class TokenizationFailureEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.TokenizationFailure; } }

		internal TokenizationFailureEventArgs(string expression)
			: base(expression)
		{
		}
	}
}
