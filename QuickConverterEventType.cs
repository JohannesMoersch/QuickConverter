using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	public enum QuickConverterEventType
	{
		TokenizationSuccess,
		TokenizationFailure,
		RuntimeCodeException,
		MarkupException,
		ChainedConverterException
	}
}
