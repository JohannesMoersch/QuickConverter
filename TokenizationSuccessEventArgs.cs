using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickConverter.Tokens;

namespace QuickConverter
{
	public class TokenizationSuccessEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.TokenizationSuccess; } }

		public TokenBase Root { get; set; }

		internal TokenizationSuccessEventArgs(string expression, TokenBase root)
			: base(expression)
		{
			Root = root;
		}
	}
}
