using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace QuickConverter
{
	/// <summary>
	/// This type can be substituted for System.Windows.Data.MultiBinding. Multiple bindings and a one way converter can be specified inline.
	/// </summary>
	public class MultiBinding : MarkupExtension
	{
		private static Dictionary<string, Tuple<Func<object[], object[], object>, string[], string[]>> functions = new Dictionary<string, Tuple<Func<object[], object[], object>, string[], string[]>>();

		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P0.</summary>
		public BindingBase P0 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P1.</summary>
		public BindingBase P1 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P2.</summary>
		public BindingBase P2 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P3.</summary>
		public BindingBase P3 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P4.</summary>
		public BindingBase P4 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P5.</summary>
		public BindingBase P5 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P6.</summary>
		public BindingBase P6 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P7.</summary>
		public BindingBase P7 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P8.</summary>
		public BindingBase P8 { get; set; }
		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P9.</summary>
		public BindingBase P9 { get; set; }
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
		/// The expression to use for converting data from the source.
		/// </summary>
		public string Converter { get; set; }

		/// <summary>
		/// This specifies the context to use for dynamic call sites.
		/// </summary>
		public Type DynamicContext { get; set; }

		public MultiBinding()
		{
		}

		public MultiBinding(string converter)
		{
			Converter = converter;
		}

		private Tuple<Delegate, ParameterExpression[]> GetLambda(string expression)
		{
			List<ParameterExpression> parameters;
			Expression exp = EquationTokenizer.Tokenize(expression).GetExpression(out parameters, DynamicContext);
			var invalid = parameters.FirstOrDefault(par => par.Name.Length != 2 || (par.Name[0] != 'P' && par.Name[0] != 'V') || !Char.IsDigit(par.Name[1]));
			if (invalid != null)
				throw new Exception("\"$" + invalid.Name + "\" is not a valid parameter name for conversion from source.");
			Delegate del = Expression.Lambda(Expression.Convert(exp, typeof(object)), parameters.ToArray()).Compile();
			return new Tuple<Delegate, ParameterExpression[]>(del, parameters.ToArray());
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			bool getExpression;
			if (serviceProvider == null)
				getExpression = false;
			else
			{
				var targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
				if (targetProvider == null || !(targetProvider.TargetProperty is PropertyInfo))
					getExpression = true;
				else
				{
					Type propType = (targetProvider.TargetProperty as PropertyInfo).PropertyType;
					if (propType == typeof(MultiBinding))
						return this;
					getExpression = !propType.IsAssignableFrom(typeof(System.Windows.Data.MultiBinding));
				}
			}

			Tuple<Func<object[], object[], object>, string[], string[]> func = null;
			if (Converter != null && !functions.TryGetValue(Converter, out func))
			{
				Tuple<Delegate, ParameterExpression[]> tuple = GetLambda(Converter);
				if (tuple == null)
					return null;

				List<string> parNames = new List<string>();
				List<string> values = new List<string>();
				int pCount = 0;
				int vCount = 0;
				ParameterExpression inputP = Expression.Parameter(typeof(object[]));
				ParameterExpression inputV = Expression.Parameter(typeof(object[]));
				var arguments = tuple.Item2.Select<ParameterExpression, Expression>(par =>
					{
						if (par.Name[0] == 'P')
						{
							parNames.Add(par.Name);
							return Expression.ArrayIndex(inputP, Expression.Constant(pCount++));
						}
						values.Add(par.Name);
						return Expression.ArrayIndex(inputV, Expression.Constant(vCount++));
					});

				Expression exp = Expression.Call(Expression.Constant(tuple.Item1, tuple.Item1.GetType()), tuple.Item1.GetType().GetMethod("Invoke"), arguments);
				var result = Expression.Lambda<Func<object[], object[], object>>(exp, inputP, inputV).Compile();
				func = new Tuple<Func<object[], object[], object>, string[], string[]>(result, parNames.ToArray(), values.ToArray());

				functions.Add(Converter, func);
			}

			if (func == null)
				return null;

			var holder = new System.Windows.Data.MultiBinding() { Mode = BindingMode.OneWay };

			foreach (string name in func.Item2)
				holder.Bindings.Add(typeof(MultiBinding).GetProperty(name).GetValue(this, null) as BindingBase);

			var vals = func.Item3.Select(str => typeof(MultiBinding).GetProperty(str).GetValue(this, null)).ToArray();
			holder.Converter = new DynamicMultiConverter(func.Item1, vals, Converter);

			return getExpression ? holder.ProvideValue(serviceProvider) : holder;
		}
	}
}
