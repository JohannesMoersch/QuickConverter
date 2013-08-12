using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Data;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter
{
	public class DynamicMultiConverter : IMultiValueConverter
	{
		private static Dictionary<Type, Func<object, object>> castFunctions = new Dictionary<Type, Func<object, object>>();

		private Func<object[], object[], object> _converter;
		private object[] _values;

		public DynamicMultiConverter(Func<object[], object[], object> converter, object[] values)
		{
			_converter = converter;
			_values = values;
		}

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			object result;
			try { result = _converter(values, _values); }
			catch { return null; }

			if (targetType == typeof(string))
				return result.ToString();

			Func<object, object> cast;
			if (!castFunctions.TryGetValue(targetType, out cast))
			{
				ParameterExpression par = Expression.Parameter(typeof(object));
				cast = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, targetType, typeof(object)), targetType, par), typeof(object)), par).Compile();
				castFunctions.Add(targetType, cast);
			}

			result = cast(result);
			return result;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
