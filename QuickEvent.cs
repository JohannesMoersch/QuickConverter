using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;
using QuickConverter;

namespace QuickConverter
{
	public class QuickEvent : MarkupExtension
	{
		private static Dictionary<string, Tuple<string, Delegate, string[], DataContainer[]>> handlers = new Dictionary<string, Tuple<string, Delegate, string[], DataContainer[]>>();

		/// <summary>
		/// The expression to use for handling the event.
		/// </summary>
		public string Handler { get; set; }

		/// <summary>
		/// This specifies the context to use for dynamic call sites.
		/// </summary>
		public Type DynamicContext { get; set; }

		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V0.</summary>
		public object V0 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V1.</summary>
		public object V1 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V2.</summary>
		public object V2 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V3.</summary>
		public object V3 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V4.</summary>
		public object V4 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V5.</summary>
		public object V5 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V6.</summary>
		public object V6 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V7.</summary>
		public object V7 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V8.</summary>
		public object V8 { get; set; }
		/// <summary>Creates a constant parameter. This can be accessed inside the handler as $V9.</summary>
		public object V9 { get; set; }

		public QuickEvent() { }

		public QuickEvent(string handlerExpression)
		{
			Handler = handlerExpression;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			try
			{
				if (Handler == null || !(serviceProvider is IProvideValueTarget) || !((serviceProvider as IProvideValueTarget).TargetProperty is EventInfo))
					throw new Exception("QuickEvent must be used on event handlers only.");
				var target = (serviceProvider as IProvideValueTarget).TargetProperty as EventInfo;
				var types = target.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
				if (types.Length != 2)
					throw new Exception("QuickEvent only supports event handlers with the standard (sender, eventArgs) signature.");

				var tuple = GetLambda(Handler);

				var handlerType = typeof(QuickEventHandler<,>).MakeGenericType(types);

				var instance = Activator.CreateInstance(handlerType, tuple.Item2, tuple.Item3, new[] { V0, V1, V2, V3, V4, V5, V6, V7, V8, V9 }, Handler, tuple.Item1, tuple.Item4);

				return Delegate.CreateDelegate(target.EventHandlerType, instance, "Handle");
			}
			catch (Exception e)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new MarkupExtensionExceptionEventArgs(Handler, this, e));
				throw;
			}
		}

		private Tuple<string, Delegate, string[], DataContainer[]> GetLambda(string expression)
		{
			Tuple<string, Delegate, string[], DataContainer[]> tuple;
			if (handlers.TryGetValue(expression, out tuple))
				return tuple;
			List<ParameterExpression> parameters;
			List<DataContainer> dataContainers;
			Expression exp = EquationTokenizer.Tokenize(expression, false).GetExpression(out parameters, out dataContainers, DynamicContext, false);
			Delegate del = Expression.Lambda(exp, parameters.ToArray()).Compile();
			tuple = new Tuple<string, Delegate, string[], DataContainer[]>(exp.ToString(), del, parameters.Select(p => p.Name).ToArray(), dataContainers.ToArray());
			handlers.Add(expression, tuple);
			return tuple;
		}
	}
}
