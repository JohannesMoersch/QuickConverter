using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuickConverter
{
	public class RuntimeEventHandlerExceptionEventArgs : QuickConverterEventArgs
	{
		public override QuickConverterEventType Type { get { return QuickConverterEventType.RuntimeCodeException; } }

		public object Sender { get; private set; }

		public object EventArgs { get; private set; }

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

		public object P0 { get; private set; }
		public object P1 { get; private set; }
		public object P2 { get; private set; }
		public object P3 { get; private set; }
		public object P4 { get; private set; }

		public QuickEventHandler Handler { get; private set; }

		public Exception Exception { get; private set; }

		public string DebugView { get; private set; }

		internal RuntimeEventHandlerExceptionEventArgs(object sender, object eventArgs, string expression, string debugView, object[] values, QuickEventHandler handler, Exception exception)
			: base(expression)
		{
			Sender = sender;
			EventArgs = eventArgs;
			DebugView = debugView;
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
			if (sender is DependencyObject)
			{
				P0 = QuickEvent.GetP0(sender as DependencyObject);
				P1 = QuickEvent.GetP1(sender as DependencyObject);
				P2 = QuickEvent.GetP2(sender as DependencyObject);
				P3 = QuickEvent.GetP3(sender as DependencyObject);
				P4 = QuickEvent.GetP4(sender as DependencyObject);
			}
			Handler = handler;
			Exception = exception;
		}
	}
}
