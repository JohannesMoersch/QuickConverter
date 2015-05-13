using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows;
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

		/// <summary>The converter will return DependencyObject.Unset during conversion if P0 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P0Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P1 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P1Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P2 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P2Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P3 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P3Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P4 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P4Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P5 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P5Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P6 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P6Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P7 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P7Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P8 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P8Type { get; set; }
		/// <summary>The converter will return DependencyObject.Unset during conversion if P9 is not of this type. Both QuickConverter syntax (as a string) and Type objects are valid.</summary>
		public object P9Type { get; set; }

		/// <summary>
		/// The expression to use for converting data from the source.
		/// </summary>
		public string Converter { get; set; }

		/// <summary>
		/// The expression to use for converting data from the target back to the source for P0.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack0 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P1.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack1 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P2.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack2 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P3.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack3 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P4.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack4 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P5.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack5 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P6.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack6 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P7.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack7 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P8.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack8 { get; set; }
		/// <summary>
		/// The expression to use for converting data from the target back to the source for P9.
		/// The target value is accessible as $value.
		/// The bound parameters $P0-$P9 cannot be accessed when converting back.
		/// </summary>
		public string ConvertBack9 { get; set; }

		/// <summary>
		/// The conventional parameter passed into converters. 
		/// This is accessible as $par inside converter expressions.
		/// </summary>
		public object ConverterParameter { get; set; }

		/// <summary>
		/// This chooses whether to pass the converter parameter or V0-V9 as an array into the external converter.
		/// </summary>
		public ConverterParameterMode ExternalConverterParameterMode { get; set; }

		private bool _modeSet = false;
		private BindingMode _mode;
		/// <summary>
		/// Sets the binding mode.
		/// </summary>
		public BindingMode Mode
		{
			get { return _mode; }
			set
			{
				_mode = value;
				_modeSet = true;
			}
		}

		/// <summary>
		/// The value that is used in the target when the value of the source is null.
		/// </summary>
		public object TargetNullValue { get; set; }
		/// <summary>
		/// The value to use when the binding is unable to return a value.
		/// </summary>
		public object FallbackValue { get; set; }

		/// <summary>
		/// Sets an override converter.
		/// </summary>
		public IMultiValueConverter ExternalConverter { get; set; }

		/// <summary>
		/// Sets the update source trigger.
		/// </summary>
		public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

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

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			try
			{
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
						if (propType == typeof(MultiBinding))
							return this;
						getExpression = !propType.IsAssignableFrom(typeof(System.Windows.Data.MultiBinding));
					}
				}

				var holder = new System.Windows.Data.MultiBinding() { Mode = Mode, UpdateSourceTrigger = UpdateSourceTrigger };
				if (FallbackValue != null)
					holder.FallbackValue = FallbackValue;
				if (TargetNullValue != null)
					holder.TargetNullValue = TargetNullValue;

				if (ExternalConverter == null)
				{
					holder.ConverterParameter = ConverterParameter;
					string[] parameterOrder;
					holder.Converter = new QuickMultiConverter()
					{
						Converter = Converter,
						DynamicContext = DynamicContext,
						P0Type = P0Type,
						P1Type = P1Type,
						P2Type = P2Type,
						P3Type = P3Type,
						P4Type = P4Type,
						P5Type = P5Type,
						P6Type = P6Type,
						P7Type = P7Type,
						P8Type = P8Type,
						P9Type = P9Type,
						V0 = V0,
						V1 = V1,
						V2 = V2,
						V3 = V3,
						V4 = V4,
						V5 = V5,
						V6 = V6,
						V7 = V7,
						V8 = V8,
						V9 = V9,
						ConvertBack0 = ConvertBack0,
						ConvertBack1 = ConvertBack1,
						ConvertBack2 = ConvertBack2,
						ConvertBack3 = ConvertBack3,
						ConvertBack4 = ConvertBack4,
						ConvertBack5 = ConvertBack5,
						ConvertBack6 = ConvertBack6,
						ConvertBack7 = ConvertBack7,
						ConvertBack8 = ConvertBack8,
						ConvertBack9 = ConvertBack9
					}.Get(out parameterOrder);

					foreach (string name in parameterOrder)
						holder.Bindings.Add(typeof(MultiBinding).GetProperty(name).GetValue(this, null) as BindingBase);

					if (!_modeSet)
					{
						bool exists = false;
						for (int i = 0; i <= 9; ++i)
							exists |= !String.IsNullOrWhiteSpace(typeof(MultiBinding).GetProperty("ConvertBack" + i).GetValue(this, null) as string);
						holder.Mode = exists ? BindingMode.TwoWay : BindingMode.OneWay;
					}
				}
				else
				{
					if (ExternalConverterParameterMode == ConverterParameterMode.Parameter)
						holder.ConverterParameter = ConverterParameter;
					else
						holder.ConverterParameter = new object[] { V0, V1, V2, V3, V4, V5, V6, V7, V8, V9 };
					holder.Converter = ExternalConverter;
					for (int i = 0; i <= 9; ++i)
					{
						var binding = typeof(MultiBinding).GetProperty("P" + i).GetValue(this, null) as BindingBase;
						if (binding != null)
							holder.Bindings.Add(binding);
					}
				}

				return getExpression ? holder.ProvideValue(serviceProvider) : holder;
			}
			catch (Exception e)
			{
				EquationTokenizer.ThrowQuickConverterEvent(new MarkupExtensionExceptionEventArgs(Converter, this, e));
				throw;
			}
		}
	}
}
