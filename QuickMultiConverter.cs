using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace QuickConverter
{
	public class QuickMultiConverter : MarkupExtension
	{
		private static Dictionary<string, Tuple<Func<object[], object[], object>, string[], DataContainer[]>> functions = new Dictionary<string, Tuple<Func<object[], object[], object>, string[], DataContainer[]>>();

		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V0.</summary>
		public object V0 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V1.</summary>
		public object V1 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V2.</summary>
		public object V2 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V3.</summary>
		public object V3 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V4.</summary>
		public object V4 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V5.</summary>
		public object V5 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V6.</summary>
		public object V6 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V7.</summary>
		public object V7 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V8.</summary>
		public object V8 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the converter as $V9.</summary>
		public object V9 { get; set; }

		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P0Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P1Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P2Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P3Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P4Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P5Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P6Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P7Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P8Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P9Type { get; set; }

		/// <summary>
		/// The expression to use for converting data from the source.
		/// </summary>
		public string Converter { get; set; }

		/// <summary>
		/// This specifies the context to use for dynamic call sites.
		/// </summary>
		public Type DynamicContext { get; set; }

		public QuickMultiConverter()
		{
		}

		public QuickMultiConverter(string converter)
		{
			Converter = converter;
		}

		private Tuple<Delegate, ParameterExpression[], DataContainer[]> GetLambda(string expression)
		{
			List<ParameterExpression> parameters;
			List<DataContainer> dataContainers;
			Expression exp = EquationTokenizer.Tokenize(expression).GetExpression(out parameters, out dataContainers, DynamicContext);
			var invalid = parameters.FirstOrDefault(par => par.Name.Length != 2 || (par.Name[0] != 'P' && par.Name[0] != 'V') || !Char.IsDigit(par.Name[1]));
			if (invalid != null)
				throw new Exception("\"$" + invalid.Name + "\" is not a valid parameter name for conversion from source.");
			Delegate del = Expression.Lambda(Expression.Convert(exp, typeof(object)), parameters.ToArray()).Compile();
			return new Tuple<Delegate, ParameterExpression[], DataContainer[]>(del, parameters.ToArray(), dataContainers.ToArray());
		}

		private string[] _parameterOrder;

		public IMultiValueConverter Get(out string[] parameterOrder)
		{
			var conv = ProvideValue(null) as IMultiValueConverter;
			parameterOrder = _parameterOrder;
			return conv;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			Tuple<Func<object[], object[], object>, string[], DataContainer[]> func = null;
			if (Converter != null && !functions.TryGetValue(Converter, out func))
			{
				Tuple<Delegate, ParameterExpression[], DataContainer[]> tuple = GetLambda(Converter);
				if (tuple == null)
					return null;

				List<string> parNames = new List<string>();
				int pCount = 0;
				ParameterExpression inputP = Expression.Parameter(typeof(object[]));
				ParameterExpression inputV = Expression.Parameter(typeof(object[]));
				int[] inds = new int[10];
				foreach (var par in tuple.Item2.Where(p => p.Name[0] == 'P').OrderBy(e => e.Name))
				{
					parNames.Add(par.Name);
					inds[par.Name[1] - '0'] = pCount++;
				}
				var arguments = tuple.Item2.Select<ParameterExpression, Expression>(par =>
				{
					if (par.Name[0] == 'P')
						return Expression.ArrayIndex(inputP, Expression.Constant(inds[par.Name[1] - '0']));
					return Expression.ArrayIndex(inputV, Expression.Constant((int)(par.Name[1] - '0')));
				});

				Expression exp = Expression.Call(Expression.Constant(tuple.Item1, tuple.Item1.GetType()), tuple.Item1.GetType().GetMethod("Invoke"), arguments);
				var result = Expression.Lambda<Func<object[], object[], object>>(exp, inputP, inputV).Compile();
				func = new Tuple<Func<object[], object[], object>, string[], DataContainer[]>(result, parNames.ToArray(), tuple.Item3);

				functions.Add(Converter, func);
			}

			if (func == null)
				return null;

			List<object> pTypes = new List<object>();
			foreach (string name in func.Item2)
				pTypes.Add(typeof(QuickMultiConverter).GetProperty(name + "Type").GetValue(this, null));

			List<object> vals = new List<object>();
			for (int i = 0; i <= 9; ++i)
				vals.Add(typeof(QuickMultiConverter).GetProperty("V" + i).GetValue(this, null));

			_parameterOrder = func.Item2;

			return new DynamicMultiConverter(func.Item1, vals.ToArray(), Converter, pTypes.Select(t => QuickConverter.GetType(t)).ToArray(), func.Item3);
		}
	}
}
