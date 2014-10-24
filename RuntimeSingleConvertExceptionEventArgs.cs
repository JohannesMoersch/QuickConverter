using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter
{
	public class RuntimeSingleConvertExceptionEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.RuntimeConvertException; } }

		public object P { get; private set; }

		public object V0 { get; private set; }
		public object V1 { get; private set; }
		public object V2 { get; private set; }
		public object V3 { get; private set; }
		public object V4 { get; private set; }
		public object V5 { get; private set; }
		public object V6 { get; private set; }
		public object V7 { get; private set; }
		public object V8 { get; private set; }
		public object V9 { get; private set; }

		public object Value { get; private set; }

		public object Parameter { get; private set; }

		public DynamicSingleConverter Converter { get; private set; }

		public Exception Exception { get; private set; }

		public string DebugView { get; private set; }

		internal RuntimeSingleConvertExceptionEventArgs(string expression, string debugView, object p, object value, object[] values, object parameter, DynamicSingleConverter converter, Exception exception)
			: base(expression)
		{
			DebugView = debugView;
			P = p;
			V0 = values[0];
			V1 = values[1];
			V2 = values[2];
			V3 = values[3];
			V4 = values[4];
			V5 = values[5];
			V6 = values[6];
			V7 = values[7];
			V8 = values[8];
			V9 = values[9];
			Value = value;
			Parameter = parameter;
			Converter = converter;
			Exception = exception;
		}
	}
}
