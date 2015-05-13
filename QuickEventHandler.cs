using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace QuickConverter
{
	public abstract class QuickEventHandler
	{
		public string HandlerExpression { get; private set; }

		public object[] Values { get; private set; }

		public string HandlerExpressionDebugView { get; private set; }

		public Exception LastException { get; protected set; }
		public int ExceptionCount { get; protected set; }

		protected Delegate _handler;
		protected string[] _parameters;
		protected DataContainer[] _dataContainers;

		public QuickEventHandler(Delegate handler, string[] parameters, object[] values, string expression, string expressionDebug, DataContainer[] dataContainers)
		{
			_handler = handler;
			_parameters = parameters;
			Values = values;
			HandlerExpression = expression;
			HandlerExpressionDebugView = expressionDebug;
			_dataContainers = dataContainers;
		}
	}

	internal class QuickEventHandler<T1, T2> : QuickEventHandler
	{
		private object[] _parArray;
		private int _dataContextIndex = -1;
		private object _lastSender;

		public QuickEventHandler(Delegate handler, string[] parameters, object[] values, string expression, string expressionDebug, DataContainer[] dataContainers)
			: base(handler, parameters, values, expression, expressionDebug, dataContainers)
		{
		}

		public void Handle(T1 sender, T2 args)
		{
			if (!SetupParameters(sender, args))
				return;

			try { _handler.DynamicInvoke(_parArray); }
			catch (Exception e)
			{
				LastException = e;
				++ExceptionCount;
				if (Debugger.IsAttached)
					Console.WriteLine("QuickEvent Exception (\"" + HandlerExpression + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
				EquationTokenizer.ThrowQuickConverterEvent(new RuntimeEventHandlerExceptionEventArgs(sender, args, HandlerExpression, HandlerExpressionDebugView, Values, this, e));
			}
			finally
			{
				if (_dataContainers != null)
				{
					foreach (var container in _dataContainers)
						container.Value = null;
				}
			}
		}

		private bool SetupParameters(object sender, object args)
		{
			if (_lastSender != sender)
				_parArray = null;
			_lastSender = sender;
			string failMessage = null;
			if (_parArray == null)
			{
				_parArray = new object[_parameters.Length];
				for (int i = 0; i < _parameters.Length; ++i)
				{
					var par = _parameters[i];
					switch (par)
					{
						case "sender":
							_parArray[i] = sender;
							break;
						case "args":
							_parArray[i] = args;
							break;
						case "dataContext":
							_dataContextIndex = i;
							break;
						default:
							if (par.Length == 2 && par[0] == 'V' && Char.IsDigit(par[1]))
								_parArray[i] = typeof(QuickEvent).GetProperty(par).GetValue(this, null);
							else if (sender is FrameworkElement)
							{
								_parArray[i] = (sender as FrameworkElement).FindName(par);
								if (_parArray[i] == null)
									failMessage = "Could not find target for $" + par + ".";
							}
							else
								failMessage = "Sender is not a framework element. Finding targets by name only works when sender is a framework element.";
							break;
					}
				}
				if (failMessage != null)
					EquationTokenizer.ThrowQuickConverterEvent(new RuntimeEventHandlerExceptionEventArgs(sender, args, HandlerExpression, HandlerExpressionDebugView, Values, this, new Exception(failMessage)));
			}
			if (_dataContextIndex >= 0 && sender is FrameworkElement)
				_parArray[_dataContextIndex] = (sender as FrameworkElement).DataContext;
			return failMessage == null;
		}
	}
}
