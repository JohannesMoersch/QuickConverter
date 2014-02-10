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

		public string ConvertExpression { get; private set; }
		public Exception LastException { get; private set; }

		private Func<object[], object[], object> _converter;
		private object[] _values;

		public DynamicMultiConverter(Func<object[], object[], object> converter, object[] values, string convertExp)
		{
			_converter = converter;
			_values = values;
			ConvertExpression = convertExp;
		}

		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			object result;
			try { result = _converter(values, _values); }
			catch (Exception e)
			{
				LastException = e;
				return null;
			}

			if (result == null)
				return null;

			if (targetType == typeof(string))
				return result.ToString();

			Func<object, object> cast;
			if (!castFunctions.TryGetValue(targetType, out cast))
			{
				ParameterExpression par = Expression.Parameter(typeof(object));
				cast = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.ConvertExplicit, targetType, typeof(object)), targetType, par), typeof(object)), par).Compile();
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
