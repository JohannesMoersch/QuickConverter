using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace QuickConverter
{
	/// <summary>
	/// This type can be substituted for System.Windows.Data.Binding. Both Convert and ConvertBack to be specified inline which makes two way binding possible.
	/// </summary>
	public class Binding : MarkupExtension
	{
		private static Dictionary<string, Tuple<Func<object, object[], object>, string[]>> toFunctions = new Dictionary<string, Tuple<Func<object, object[], object>, string[]>>();
		private static Dictionary<string, Tuple<Func<object, object[], object>, string[]>> fromFunctions = new Dictionary<string, Tuple<Func<object, object[], object>, string[]>>();

		/// <summary>Creates a bound parameter. This can be accessed inside the converter as $P.</summary>
		public System.Windows.Data.Binding P { get; set; }

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

		public Binding()
		{
		}

		public Binding(string convert)
		{
			Convert = convert;
		}

		private Tuple<Delegate, ParameterExpression[]> GetLambda(string expression, bool convertBack)
		{
			List<ParameterExpression> parameters = new List<ParameterExpression>();
			Expression exp = EquationTokenizer.Tokenize(expression).GetExpression(parameters, DynamicContext);
			ParameterExpression invalid;
			if (convertBack)
				invalid = parameters.FirstOrDefault(par => !((par.Name[0] == 'V' && par.Name.Length == 2 && Char.IsDigit(par.Name[1])) || (par.Name == "value")));
			else
				invalid = parameters.FirstOrDefault(par => !((par.Name[0] == 'P' && par.Name.Length == 1) || (par.Name[0] == 'V' && par.Name.Length == 2 && Char.IsDigit(par.Name[1]))));
			if (invalid != null)
				throw new Exception("\"$" + invalid.Name + "\" is not a valid parameter name for conversion " + (convertBack ? "to" : "from") + " source.");
			Delegate del = Expression.Lambda(Expression.Convert(exp, typeof(object)), parameters.ToArray()).Compile();
			return new Tuple<Delegate, ParameterExpression[]>(del, parameters.ToArray());
		}

		private Expression GetFinishedLambda(Tuple<Delegate, ParameterExpression[]> lambda, out ParameterExpression inputP, out ParameterExpression inputV, out ParameterExpression value, out List<string> values)
		{
			List<string> vals = new List<string>();
			int vCount = 0;
			ParameterExpression val = Expression.Parameter(typeof(object));
			ParameterExpression inP = Expression.Parameter(typeof(object));
			ParameterExpression inV = Expression.Parameter(typeof(object[]));
			var arguments = lambda.Item2.Select<ParameterExpression, Expression>(par =>
			{
				if (par.Name[0] == 'P')
					return inP;
				else if (par.Name[0] == 'V')
				{
					vals.Add(par.Name);
					return Expression.ArrayIndex(inV, Expression.Constant(vCount++));
				}
				return val;
			});
			inputP = inP;
			inputV = inV;
			value = val;
			values = vals;

			return Expression.Call(Expression.Constant(lambda.Item1, lambda.Item1.GetType()), lambda.Item1.GetType().GetMethod("Invoke"), arguments);
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			var pvt = serviceProvider as IProvideValueTarget;
			if (pvt == null || P == null)
				return null;

			ParameterExpression inputP, inputV, value;
			List<string> values;
			Tuple<Func<object, object[], object>, string[]> toFunc = null;
			Tuple<Func<object, object[], object>, string[]> fromFunc = null;
			if (Convert != null && !toFunctions.TryGetValue(Convert, out toFunc))
			{
				Tuple<Delegate, ParameterExpression[]> tuple = GetLambda(Convert, false);
				if (tuple == null)
					return null;

				Expression exp = GetFinishedLambda(tuple, out inputP, out inputV, out value, out values);
				var result = Expression.Lambda<Func<object, object[], object>>(exp, inputP, inputV).Compile();
				toFunc = new Tuple<Func<object, object[], object>, string[]>(result, values.ToArray());

				toFunctions.Add(Convert, toFunc);
			}
			if (ConvertBack != null && !fromFunctions.TryGetValue(ConvertBack, out fromFunc))
			{
				Tuple<Delegate, ParameterExpression[]> tuple = GetLambda(ConvertBack, true);
				if (tuple == null)
					return null;

				Expression exp = GetFinishedLambda(tuple, out inputP, out inputV, out value, out values);
				var result = Expression.Lambda<Func<object, object[], object>>(exp, value, inputV).Compile();
				fromFunc = new Tuple<Func<object, object[], object>, string[]>(result, values.ToArray());

				fromFunctions.Add(ConvertBack, fromFunc);
			}

			object[] toVals = null;
			if (toFunc != null)
				toVals = toFunc.Item2.Select(str => typeof(Binding).GetProperty(str).GetValue(this, null)).ToArray();

			object[] fromVals = null;
			if (fromFunc != null)
				fromVals = fromFunc.Item2.Select(str => typeof(Binding).GetProperty(str).GetValue(this, null)).ToArray();

			var holder = new System.Windows.Data.MultiBinding() { Mode = P.Mode, UpdateSourceTrigger = P.UpdateSourceTrigger };
			holder.Bindings.Add(P);
			holder.Converter = new DynamicConverter(toFunc != null ? toFunc.Item1 : null, fromFunc != null ? fromFunc.Item1 : null, toVals, fromVals);

			return holder.ProvideValue(serviceProvider);
		}
	}
}
