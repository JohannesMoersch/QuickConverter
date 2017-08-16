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
		private int _eventArgsIndex = -1;
		private int[] _pIndex = new int[] { -1, -1, -1, -1, -1 };
		private bool _ignoreIfNotOriginalSource;
		private bool _setHandled;

		private object _lastSender;

		public QuickEventHandler(Delegate handler, string[] parameters, object[] values, string expression, string expressionDebug, DataContainer[] dataContainers, bool ignoreIfNotOriginalSource, bool setHandled)
			: base(handler, parameters, values, expression, expressionDebug, dataContainers)
		{
			_ignoreIfNotOriginalSource = ignoreIfNotOriginalSource;
			_setHandled = setHandled;
		}

		public void Handle(T1 sender, T2 args)
		{
			if (_ignoreIfNotOriginalSource && args is RoutedEventArgs && (args as RoutedEventArgs).OriginalSource != (args as RoutedEventArgs).Source)
				return;

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
				if (_dataContextIndex >= 0)
					_parArray[_dataContextIndex] = null;
				if (_eventArgsIndex >= 0)
					_parArray[_eventArgsIndex] = null;
				if (_dataContainers != null)
				{
					foreach (var container in _dataContainers)
						container.Value = null;
				}
				if (_setHandled && args is RoutedEventArgs)
					(args as RoutedEventArgs).Handled = true;
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
						case "eventArgs":
							_eventArgsIndex = i;
							break;
						case "dataContext":
							_dataContextIndex = i;
							break;
						default:
							if (par.Length == 2 && par[0] == 'V' && Char.IsDigit(par[1]))
								_parArray[i] = Values[par[1] - '0'];
							else if (par.Length == 2 && par[0] == 'P' && par[1] >= '0' && par[1] <= '4')
								_pIndex[par[1] - '0'] = i;
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
			}
			if (_dataContextIndex >= 0 && sender is FrameworkElement)
				_parArray[_dataContextIndex] = (sender as FrameworkElement).DataContext;
			if (_eventArgsIndex >= 0)
				_parArray[_eventArgsIndex] = args;
			for (int i = 0; i <= 4; ++i)
			{
				if (_pIndex[i] == -1)
					continue;
				if (!(sender is DependencyObject))
				{
					failMessage = "Cannot access $P0-$P4 when sender is not a DependencyObject.";
					break;
				}
				if (i == 0)
					_parArray[_pIndex[i]] = QuickEvent.GetP0(sender as DependencyObject);
				else if (i == 1)
					_parArray[_pIndex[i]] = QuickEvent.GetP1(sender as DependencyObject);
				else if (i == 2)
					_parArray[_pIndex[i]] = QuickEvent.GetP2(sender as DependencyObject);
				else if (i == 3)
					_parArray[_pIndex[i]] = QuickEvent.GetP3(sender as DependencyObject);
				else if (i == 4)
					_parArray[_pIndex[i]] = QuickEvent.GetP4(sender as DependencyObject);
			}
			if (failMessage != null)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new RuntimeEventHandlerExceptionEventArgs(sender, args, HandlerExpression, HandlerExpressionDebugView, Values, this, new Exception(failMessage)));
				return false;
			}
			return true;
		}
	}
}
