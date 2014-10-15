using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	public delegate void QuickConverterEventHandler(QuickConverterEventArgs args);

	public class QuickConverterEventArgs
	{
		public QuickConverterEventType Type { get; private set; }

		public string Expression { get; private set; }

		internal QuickConverterEventArgs(QuickConverterEventType type, string expression)
		{
			Type = type;
			Expression = expression;
		}
	}
}
