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
	public class DynamicMultiConverter : IMultiValueConverter
	{
		private static Dictionary<Type, Func<object, object>> castFunctions = new Dictionary<Type, Func<object, object>>();

		public string ConvertExpression { get; private set; }
		public string[] ConvertBackExpression { get; private set; }
		public Exception LastException { get; private set; }
		public int ExceptionCount { get; private set; }

		public Type[] PTypes { get; private set; }

		public object[] Values { get { return _values; } }

		private Func<object[], object[], object> _converter;
		private Func<object, object[], object>[] _convertBack;
		private object[] _values;
		private DataContainer[] _dataContainers;
		public DynamicMultiConverter(Func<object[], object[], object> converter, Func<object, object[], object>[] convertBack, object[] values, string convertExp, string[] convertBackExp, Type[] pTypes, DataContainer[] dataContainers)
		{
			_converter = converter;
			_convertBack = convertBack;
			_values = values;
			_dataContainers = dataContainers;
			ConvertExpression = convertExp;
			ConvertBackExpression = convertBackExp;
			PTypes = pTypes;
		}

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			for (int i = 0; i < values.Length; ++i)
			{
				if (PTypes[i] != null && (values[i] == DependencyProperty.UnsetValue || !PTypes[i].IsInstanceOfType(values[i])))
					return DependencyProperty.UnsetValue;
			}

			object result;
			try { result = _converter(values, _values); }
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
				if (_dataContainers != null)
				{
					foreach (var container in _dataContainers)
						container.Value = null;
				}
			}

			result = CastResult(result, targetType);
			return result;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			object[] ret = new object[_convertBack.Length];
			for (int i = 0; i < _convertBack.Length; ++i)
			{
				try { ret[i] = _convertBack[i](value, _values); }
				catch (Exception e)
				{
					LastException = e;
					++ExceptionCount;
					if (Debugger.IsAttached)
						Console.WriteLine("QuickMultiConverter Exception (\"" + ConvertBackExpression[i] + "\") - " + e.Message + (e.InnerException != null ? " (Inner - " + e.InnerException.Message + ")" : ""));
					ret[i] = DependencyProperty.UnsetValue;
				}
				ret[i] = CastResult(ret[i], targetTypes[i]);
			}
			if (_dataContainers != null)
			{
				foreach (var container in _dataContainers)
					container.Value = null;
			}
			return ret;
		}

		private object CastResult(object result, Type targetType)
		{
			if (result == null || result == DependencyProperty.UnsetValue || result == System.Windows.Data.Binding.DoNothing)
				return result;

			if (targetType == typeof(string))
				return result.ToString();

			Func<object, object> cast;
			if (!castFunctions.TryGetValue(targetType, out cast))
			{
				ParameterExpression par = Expression.Parameter(typeof(object));
				cast = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, typeof(object)), targetType, par), typeof(object)), par).Compile();
				castFunctions.Add(targetType, cast);
			}

			return cast(result);
		}
	}
}
