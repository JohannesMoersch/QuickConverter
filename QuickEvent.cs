using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using QuickConverter;
using Expression = System.Linq.Expressions.Expression;

namespace QuickConverter
{
	public class QuickEvent : MarkupExtension
	{
		public static object GetP0(DependencyObject obj) { return (object)obj.GetValue(P0Property); }
		public static void SetP0(DependencyObject obj, object value) { obj.SetValue(P0Property, value); }
		public static readonly DependencyProperty P0Property = DependencyProperty.RegisterAttached("P0", typeof(object), typeof(QuickEvent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

		public static object GetP1(DependencyObject obj) { return (object)obj.GetValue(P1Property); }
		public static void SetP1(DependencyObject obj, object value) { obj.SetValue(P1Property, value); }
		public static readonly DependencyProperty P1Property = DependencyProperty.RegisterAttached("P1", typeof(object), typeof(QuickEvent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

		public static object GetP2(DependencyObject obj) { return (object)obj.GetValue(P2Property); }
		public static void SetP2(DependencyObject obj, object value) { obj.SetValue(P2Property, value); }
		public static readonly DependencyProperty P2Property = DependencyProperty.RegisterAttached("P2", typeof(object), typeof(QuickEvent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

		public static object GetP3(DependencyObject obj) { return (object)obj.GetValue(P3Property); }
		public static void SetP3(DependencyObject obj, object value) { obj.SetValue(P3Property, value); }
		public static readonly DependencyProperty P3Property = DependencyProperty.RegisterAttached("P3", typeof(object), typeof(QuickEvent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

		public static object GetP4(DependencyObject obj) { return (object)obj.GetValue(P4Property); }
		public static void SetP4(DependencyObject obj, object value) { obj.SetValue(P4Property, value); }
		public static readonly DependencyProperty P4Property = DependencyProperty.RegisterAttached("P4", typeof(object), typeof(QuickEvent), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

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

		/// <summary>
		/// If true, events are bubble up to the target control will be ignored.
		/// </summary>
		public bool IgnoreIfNotOriginalSource { get; set; }

		/// <summary>
		/// Indicates whether or not to set the event args handled flag to true.
		/// </summary>
		public bool SetHandled { get; set; }

		/// <summary>
		/// This allows the delegate type to be explicitly instead of inferred from the service provider.
		/// </summary>
		public Type DelegateTypeOverride { get; set; }

		public QuickEvent()
		{
			SetHandled = true;
		}

		public QuickEvent(string handlerExpression)
			: this()
		{
			Handler = handlerExpression;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			try
			{
				if (String.IsNullOrWhiteSpace(Handler))
					throw new Exception("A QuickEvent cannot be created without code for the handler.");

				Type delegateType;
				if (DelegateTypeOverride == null)
				{
					if (serviceProvider is IProvideValueTarget)
					{
						var prop = (serviceProvider as IProvideValueTarget).TargetProperty;
						if (prop is EventInfo)
							delegateType = (prop as EventInfo).EventHandlerType;
						else if (prop is MethodInfo && (prop as MethodInfo).GetParameters().Length == 2 && typeof(Delegate).IsAssignableFrom((prop as MethodInfo).GetParameters()[1].ParameterType))
							delegateType = (prop as MethodInfo).GetParameters()[1].ParameterType;
						else
							throw new Exception("QuickEvent must be used on event handlers only.");
					}
					else
						throw new Exception("Either service provider or DelegateTypeOverride must have a value.");

				}
				else
					delegateType = DelegateTypeOverride;

				var types = delegateType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
				if (types.Length != 2)
					throw new Exception("QuickEvent only supports event handlers with the standard (sender, eventArgs) signature.");

				var tuple = GetLambda(Handler);

				var handlerType = typeof(QuickEventHandler<,>).MakeGenericType(types);

				var instance = Activator.CreateInstance(handlerType, tuple.Item2, tuple.Item3, new[] { V0, V1, V2, V3, V4, V5, V6, V7, V8, V9 }, Handler, tuple.Item1, tuple.Item4, IgnoreIfNotOriginalSource, SetHandled);

				return Delegate.CreateDelegate(delegateType, instance, "Handle");
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
