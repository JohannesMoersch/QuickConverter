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
		private static Dictionary<string, Tuple<string, Func<object[], object[], object>, string[], DataContainer[]>> toFunctions = new Dictionary<string, Tuple<string, Func<object[], object[], object>, string[], DataContainer[]>>();
		private static Dictionary<string, Tuple<string, Func<object, object[], object>, DataContainer[]>> fromFunctions = new Dictionary<string, Tuple<string, Func<object, object[], object>, DataContainer[]>>();

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

		/// <summary>
		/// During Convert calls, this converter executes after the QuickConverter.
		/// During ConvertBack calls, this converter executes before the QuickConverter.
		/// </summary>
		public IValueConverter ChainedConverter { get; set; }

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
		/// The converter will return Binding.DoNothing during conversion if value is not of this type.
		/// Both QuickConverter syntax (as a string) and Type objects are valid. 
		/// </summary>
		public object ValueType { get; set; }

		/// <summary>
		/// The expression to use for converting data from the source.
		/// </summary>
		public string Converter { get; set; }

		/// <summary>
		/// The expression to use for converting data from the target back to the source for P0.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack0 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P1.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack1 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P2.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack2 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P3.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack3 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P4.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack4 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P5.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack5 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P6.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack6 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P7.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack7 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P8.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack8 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P9.
		/// The target value is accessible as $value.
		/// </summary>
		public string ConvertBack9 { get; set; }

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

		private Tuple<string, Delegate, ParameterExpression[], DataContainer[]> GetLambda(string expression, bool convertBack)
		{
			List<ParameterExpression> parameters;
			List<DataContainer> dataContainers;
			Expression exp = EquationTokenizer.Tokenize(expression).GetExpression(out parameters, out dataContainers, DynamicContext);
			ParameterExpression invalid;
			if (convertBack)
				invalid = parameters.FirstOrDefault(par => par.Name != "value" && (par.Name.Length != 2 || par.Name[0] != 'V' || !Char.IsDigit(par.Name[1])));
			else
				invalid = parameters.FirstOrDefault(par => par.Name.Length != 2 || (par.Name[0] != 'P' && par.Name[0] != 'V') || !Char.IsDigit(par.Name[1]));
			if (invalid != null)
				throw new Exception("\"$" + invalid.Name + "\" is not a valid parameter name for conversion " + (convertBack ? "to" : "from") + " source.");
			Delegate del = Expression.Lambda(Expression.Convert(exp, typeof(object)), parameters.ToArray()).Compile();
			return new Tuple<string, Delegate, ParameterExpression[], DataContainer[]>(exp.ToString(), del, parameters.ToArray(), dataContainers.ToArray());
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
			try
			{
				Tuple<string, Func<object[], object[], object>, string[], DataContainer[]> func = null;
				if (Converter != null && !toFunctions.TryGetValue(Converter, out func))
				{
					Tuple<string, Delegate, ParameterExpression[], DataContainer[]> tuple = GetLambda(Converter, false);
					if (tuple == null)
						return null;

					List<string> parNames = new List<string>();
					int pCount = 0;
					ParameterExpression inputP = Expression.Parameter(typeof(object[]));
					ParameterExpression inputV = Expression.Parameter(typeof(object[]));
					int[] inds = new int[10];
					foreach (var par in tuple.Item3.Where(p => p.Name[0] == 'P').OrderBy(e => e.Name))
					{
						parNames.Add(par.Name);
						inds[par.Name[1] - '0'] = pCount++;
					}
					var arguments = tuple.Item3.Select<ParameterExpression, Expression>(par =>
					{
						if (par.Name[0] == 'P')
							return Expression.ArrayIndex(inputP, Expression.Constant(inds[par.Name[1] - '0']));
						return Expression.ArrayIndex(inputV, Expression.Constant((int)(par.Name[1] - '0')));
					});

					Expression exp = Expression.Call(Expression.Constant(tuple.Item2, tuple.Item2.GetType()), tuple.Item2.GetType().GetMethod("Invoke"), arguments);
					var result = Expression.Lambda<Func<object[], object[], object>>(exp, inputP, inputV).Compile();
					func = new Tuple<string, Func<object[], object[], object>, string[], DataContainer[]>(tuple.Item1, result, parNames.ToArray(), tuple.Item4);

					toFunctions.Add(Converter, func);
				}
				Tuple<string, Func<object, object[], object>, DataContainer[]>[] backFuncs = new Tuple<string, Func<object, object[], object>, DataContainer[]>[10];
				for (int i = 0; i <= 9; ++i)
				{
					var converter = typeof(QuickMultiConverter).GetProperty("ConvertBack" + i).GetValue(this, null) as string;
					if (String.IsNullOrWhiteSpace(converter))
					{
						backFuncs[i] = new Tuple<string, Func<object, object[], object>, DataContainer[]>(null, null, new DataContainer[0]);
						continue;
					}
					Tuple<string, Delegate, ParameterExpression[], DataContainer[]> tuple = GetLambda(converter, true);
					if (tuple == null)
					{
						backFuncs[i] = new Tuple<string, Func<object, object[], object>, DataContainer[]>(null, null, new DataContainer[0]);
						continue;
					}

					ParameterExpression val = Expression.Parameter(typeof(object));
					ParameterExpression inV = Expression.Parameter(typeof(object[]));
					var arguments = tuple.Item3.Select<ParameterExpression, Expression>(par =>
					{
						if (par.Name[0] == 'V')
							return Expression.ArrayIndex(inV, Expression.Constant((int)(par.Name[1] - '0')));
						return val;
					});

					Expression exp = Expression.Call(Expression.Constant(tuple.Item2, tuple.Item2.GetType()), tuple.Item2.GetType().GetMethod("Invoke"), arguments);
					var result = Expression.Lambda<Func<object, object[], object>>(exp, val, inV).Compile();
					backFuncs[i] = new Tuple<string, Func<object, object[], object>, DataContainer[]>(tuple.Item1, result, tuple.Item4);
				}

				if (func == null)
					func = new Tuple<string, Func<object[], object[], object>, string[], DataContainer[]>(null, null, null, null);

				List<object> pTypes = new List<object>();
				List<int> pIndicies = new List<int>();
				List<Tuple<string, Func<object, object[], object>, DataContainer[], string>> backs = new List<Tuple<string, Func<object, object[], object>, DataContainer[], string>>();
				if (func.Item3 != null)
				{
					foreach (string name in func.Item3)
					{
						int index = name[1] - '0';
						pIndicies.Add(index);
						pTypes.Add(typeof(QuickMultiConverter).GetProperty(name + "Type").GetValue(this, null));
						backs.Add(new Tuple<string, Func<object, object[], object>, DataContainer[], string>(backFuncs[index].Item1, backFuncs[index].Item2, backFuncs[index].Item3, typeof(QuickMultiConverter).GetProperty("ConvertBack" + name[1]).GetValue(this, null) as string));
					}
				}

				List<object> vals = new List<object>();
				for (int i = 0; i <= 9; ++i)
					vals.Add(typeof(QuickMultiConverter).GetProperty("V" + i).GetValue(this, null));

				_parameterOrder = func.Item3;

				return new DynamicMultiConverter(func.Item2, backs.Select(t => t.Item2).ToArray(), vals.ToArray(), Converter, func.Item1, backs.Select(t => t.Item4).ToArray(), backs.Select(t => t.Item1).ToArray(), pTypes.Select(t => QuickConverter.GetType(t)).ToArray(), pIndicies.ToArray(), QuickConverter.GetType(ValueType), func.Item4, backs.SelectMany(t => t.Item3).ToArray(), ChainedConverter);
			}
			catch (Exception e)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new MarkupExtensionExceptionEventArgs(Converter, this, e));
				throw;
			}
		}
	}
}
