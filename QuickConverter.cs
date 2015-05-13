using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace QuickConverter
{
	public class QuickConverter : MarkupExtension
	{
		private static Dictionary<string, Tuple<string, Func<object, object[], object, object>, DataContainer[]>> toFunctions = new Dictionary<string, Tuple<string, Func<object, object[], object, object>, DataContainer[]>>();
		private static Dictionary<string, Tuple<string, Func<object, object[], object, object>, DataContainer[]>> fromFunctions = new Dictionary<string, Tuple<string, Func<object, object[], object, object>, DataContainer[]>>();

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

		/// <summary>
		/// The converter will return DependencyObject.Unset during conversion if P is not of this type.
		/// Both QuickConverter syntax (as a string) and Type objects are valid. 
		/// </summary>
		public object PType { get; set; }

		/// <summary>
		/// The converter will return Binding.DoNothing during conversion if value is not of this type.
		/// Both QuickConverter syntax (as a string) and Type objects are valid. 
		/// </summary>
		public object ValueType { get; set; }

		/// <summary>
		/// The expression to use for converting data from the source.
		/// </summary>
		public string Convert { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source.
		/// The target value is accessible as $value.
		/// The bound parameter $P cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack { get; set; }

		/// <summary>
		/// This specifies the context to use for dynamic call sites.
		/// </summary>
		public Type DynamicContext { get; set; }	

		public QuickConverter()
		{
		}

		public QuickConverter(string convert)
		{
			Convert = convert;
		}

		private Tuple<string, Delegate, ParameterExpression[], DataContainer[]> GetLambda(string expression, bool convertBack)
		{
			List<ParameterExpression> parameters;
			List<DataContainer> dataContainers;
			Expression exp = EquationTokenizer.Tokenize(expression).GetExpression(out parameters, out dataContainers, DynamicContext);
			ParameterExpression invalid;
			if (convertBack)
				invalid = parameters.FirstOrDefault(par => !((par.Name[0] == 'V' && par.Name.Length == 2 && Char.IsDigit(par.Name[1])) || par.Name == "value" || par.Name == "par"));
			else
				invalid = parameters.FirstOrDefault(par => !((par.Name[0] == 'P' && par.Name.Length == 1) || (par.Name[0] == 'V' && par.Name.Length == 2 && Char.IsDigit(par.Name[1])) || par.Name == "par"));
			if (invalid != null)
				throw new Exception("\"$" + invalid.Name + "\" is not a valid parameter name for conversion " + (convertBack ? "to" : "from") + " source.");
			Delegate del = Expression.Lambda(Expression.Convert(exp, typeof(object)), parameters.ToArray()).Compile();
			return new Tuple<string, Delegate, ParameterExpression[], DataContainer[]>(exp.ToString(), del, parameters.ToArray(), dataContainers.ToArray());
		}

		private Expression GetFinishedLambda(Tuple<string, Delegate, ParameterExpression[], DataContainer[]> lambda, out ParameterExpression inputP, out ParameterExpression inputV, out ParameterExpression value, out ParameterExpression parameter)
		{
			ParameterExpression val = Expression.Parameter(typeof(object));
			ParameterExpression inPar = Expression.Parameter(typeof(object));
			ParameterExpression inP = Expression.Parameter(typeof(object));
			ParameterExpression inV = Expression.Parameter(typeof(object[]));
			var arguments = lambda.Item3.Select<ParameterExpression, Expression>(par =>
			{
				if (par.Name[0] == 'P')
					return inP;
				else if (par.Name[0] == 'V')
					return Expression.ArrayIndex(inV, Expression.Constant((int)(par.Name[1] - '0')));
				else if (par.Name == "value")
					return val;
				else if (par.Name == "par")
					return inPar;
				throw new Exception("Parameter name error. This shouldn't happen.");
			});
			inputP = inP;
			inputV = inV;
			value = val;
			parameter = inPar;

			return Expression.Call(Expression.Constant(lambda.Item2, lambda.Item2.GetType()), lambda.Item2.GetType().GetMethod("Invoke"), arguments);
		}

		internal static Type GetType(object pType)
		{
			if (pType == null)
				return null;
			if (pType is Type)
				return pType as Type;
			if (pType is string)
			{
				string type = "typeof(" + pType + ")";
				Tokens.TokenBase token;
				if (!new Tokens.TypeofToken().TryGetToken(ref type, out token))
					throw new Exception("\"" + pType + "\" is not a valid type.");
				return (token as Tokens.TypeofToken).Type;
			}
			throw new Exception("PType must be either string or a Type.");
		}

		public IValueConverter Get()
		{
			return ProvideValue(null) as IValueConverter;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			try
			{
				ParameterExpression inputP, inputV, value, parameter;
				Tuple<string, Func<object, object[], object, object>, DataContainer[]> toFunc = null;
				Tuple<string, Func<object, object[], object, object>, DataContainer[]> fromFunc = null;
				if (Convert != null && !toFunctions.TryGetValue(Convert, out toFunc))
				{
					Tuple<string, Delegate, ParameterExpression[], DataContainer[]> tuple = GetLambda(Convert, false);
					if (tuple == null)
						return null;

					Expression exp = GetFinishedLambda(tuple, out inputP, out inputV, out value, out parameter);
					var result = Expression.Lambda<Func<object, object[], object, object>>(exp, inputP, inputV, parameter).Compile();
					toFunc = new Tuple<string, Func<object, object[], object, object>, DataContainer[]>(tuple.Item1, result, tuple.Item4);

					toFunctions.Add(Convert, toFunc);
				}
				if (ConvertBack != null && !fromFunctions.TryGetValue(ConvertBack, out fromFunc))
				{
					Tuple<string, Delegate, ParameterExpression[], DataContainer[]> tuple = GetLambda(ConvertBack, true);
					if (tuple == null)
						return null;

					Expression exp = GetFinishedLambda(tuple, out inputP, out inputV, out value, out parameter);
					var result = Expression.Lambda<Func<object, object[], object, object>>(exp, value, inputV, parameter).Compile();
					fromFunc = new Tuple<string, Func<object, object[], object, object>, DataContainer[]>(tuple.Item1, result, tuple.Item4);

					fromFunctions.Add(ConvertBack, fromFunc);
				}

				List<object> vals = new List<object>();
				for (int i = 0; i <= 9; ++i)
					vals.Add(typeof(QuickConverter).GetProperty("V" + i).GetValue(this, null));

				if (toFunc == null)
					toFunc = new Tuple<string, Func<object, object[], object, object>, DataContainer[]>(null, null, null);
				if (fromFunc == null)
					fromFunc = new Tuple<string, Func<object, object[], object, object>, DataContainer[]>(null, null, null);

				return new DynamicSingleConverter(toFunc.Item2, fromFunc.Item2, vals.ToArray(), Convert, toFunc.Item1, ConvertBack, fromFunc.Item1, GetType(PType), GetType(ValueType), toFunc.Item3, fromFunc.Item3, ChainedConverter);
			}
			catch (Exception e)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new MarkupExtensionExceptionEventArgs(Convert, this, e));
				throw;
			}
		}
	}
}
