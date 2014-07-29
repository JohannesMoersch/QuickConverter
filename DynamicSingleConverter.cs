using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Microsoft.CSharp.RuntimeBinder;
using Expression = System.Linq.Expressions.Expression;

namespace QuickConverter
{
	public class DynamicSingleConverter : IValueConverter
	{
		public string ConvertExpression { get; private set; }
		public string ConvertBackExpression { get; private set; }
		public Exception LastException { get; private set; }
		public int ExceptionCount { get; private set; }

		public Type PType { get; private set; }

		public object[] Values { get { return _values; } }

		private Func<object, object[], object> _converter;
		private Func<object, object[], object> _convertBack;
		private object[] _values;
		private DataContainer[] _toDataContainers;
		private DataContainer[] _fromDataContainers;
		public DynamicSingleConverter(Func<object, object[], object> converter, Func<object, object[], object> convertBack, object[] values, string convertExp, string convertBackExp, Type pType, DataContainer[] toDataContainers, DataContainer[] fromDataContainers)
		{
			_converter = converter;
			_convertBack = convertBack;
			_values = values;
			_toDataContainers = toDataContainers;
			_fromDataContainers = fromDataContainers;
			ConvertExpression = convertExp;
			ConvertBackExpression = convertBackExp;
			PType = pType;
		}

		private object DoConversion(object value, Type targetType, Func<object, object[], object> func, bool convertingBack)
		{
			if (!convertingBack && PType != null && (value == DependencyProperty.UnsetValue || !PType.IsInstanceOfType(value)))
				return DependencyProperty.UnsetValue;

			object result = value;
			if (func != null)
			{
				try { result = func(result, _values); }
				catch (Exception e)
				{
					LastException = e;
					++ExceptionCount;
					if (Debugger.IsAttached)
						Console.WriteLine("QuickMultiConverter Exception (\"" + ConvertExpression + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
					return DependencyProperty.UnsetValue; 
				}
				finally
				{
					var dataContainers = convertingBack ? _fromDataContainers : _toDataContainers;
					if (dataContainers != null)
					{
						foreach (var container in dataContainers)
							container.Value = null;
					}
				}
			}

			if (result == null || result == DependencyProperty.UnsetValue || result == System.Windows.Data.Binding.DoNothing)
				return result;

			if (targetType == typeof(string))
				return result.ToString();

			return result;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DoConversion(value, targetType, _converter, false);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return DoConversion(value, targetType, _convertBack, true);
		}
	}
}
