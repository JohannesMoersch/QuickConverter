using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace QuickConverter
{
	public class MarkupExtensionExceptionEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.MarkupException; } }

		public MarkupExtension MarkupExtension { get; private set; }

		public Exception Exception { get; private set; }

		internal MarkupExtensionExceptionEventArgs(string expression, MarkupExtension markupExtension, Exception exception)
			: base(expression)
		{
			MarkupExtension = markupExtension;
			Exception = exception;
		}
	}
}
