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
using System.Xaml;

namespace QuickConverter
{
	/// <summary>
	/// This type can be substituted for System.Windows.Data.Binding. Both Convert and ConvertBack to be specified inline which makes two way binding possible.
	/// </summary>
	public class Binding : MarkupExtension
	{
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
		/// The converter will return DependencyObject.Unset during conversion if P is not of this type.
		/// Both QuickConverter syntax (as a string) and Type objects are valid. 
		/// </summary>
		public object PType { get; set; }
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

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			try
			{
				if (P == null)
					return null;

				bool getExpression;
				if (serviceProvider == null)
					getExpression = false;
				else
				{
					var targetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
					if (targetProvider != null && (targetProvider.TargetObject is Setter))
						getExpression = false;
					else if (targetProvider == null || !(targetProvider.TargetProperty is PropertyInfo))
						getExpression = true;
					else
					{
						Type propType = (targetProvider.TargetProperty as PropertyInfo).PropertyType;
						if (propType == typeof(Binding))
							return this;
						getExpression = !propType.IsAssignableFrom(typeof(System.Windows.Data.MultiBinding));
					}
				}

				P.Converter = new QuickConverter()
				{
					Convert = Convert,
					ConvertBack = ConvertBack,
					DynamicContext = DynamicContext,
					PType = PType,
					V0 = V0,
					V1 = V1,
					V2 = V2,
					V3 = V3,
					V4 = V4,
					V5 = V5,
					V6 = V6,
					V7 = V7,
					V8 = V8,
					V9 = V9
				}.Get();

				return getExpression ? P.ProvideValue(serviceProvider) : P;
			}
			catch (Exception e)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new MarkupExtensionExceptionEventArgs(Convert, this, e));
				throw;
			}
		}
	}
}
