# QuickConverter
QuickConverter provides you with WPF markup that allows you to write inline converters, multi-bindings, and event handlers using a C# like language directly in your xaml.

Even though QuickConverter compiles converter expressions at runtime, its use introduces very little overhead. QuickConverter takes advantage of expression trees and caching to make compilation of dynamically parsed expressions very efficient. 

QuickConverter is also available on NuGet.

Quick Setup Steps
-----------------

1.  Add the assembly reference to your project. This can also easily be done using [the NuGet package](http://www.nuget.org/packages/QuickConverter/).
2.  Add the namespaces to Quick Converter that it will need to know about (before any xaml that uses it is loaded).  
    e.g. In your WPF application's App.xaml.cs file:  
    
```csharp
 public partial class App : Application
{
  public App() : base()
  {
    // Setup Quick Converter.
    // Add the System namespace so we can use primitive types (i.e. int, etc.).
    QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
    // Add the System.Windows namespace so we can use Visibility.Collapsed, etc.
    QuickConverter.EquationTokenizer.AddNamespace(typeof(System.Windows.Visibility));
  }
}
```  

3.  Add the Quick Converter namespace to your xaml files so you can reference it.  

```
xmlns:qc="http://QuickConverter.CodePlex.com/"
```
    

More Details
------------

There are two primary classes to know when using of QuickConverter. These are Binding and MultiBinding. These classes can be used in place of System.Windows.Data.Binding and System.Windows.Data.MultiBinding respectively.

However, before these classes can be used, a small amount of setup is required. Since QuickConverter does static type look ups, the namespaces in which to search need to be specified. For instance, to be able to use 'Visibility.Collapsed' in a converter, you must first register the 'System.Windows' namespace. This only needs to be done once at initialization time. Adding the 'System' namespace from mscorlib can be done using the following call:

```csharp
QuickConverter.EquationTokenizer.AddNamespace("System", Assembly.GetAssembly(typeof(object)));
```

In this call you are specifying the namespace and the assembly. This can also be done as follows:

```csharp
QuickConverter.EquationTokenizer.AddNamespace(typeof(object));
```

In this call, the namespace and the assembly are inferred from the type.

Additionally, for full type names (like "System.Int32"), the assembly it is contained within must be specified. The following shows how to do this:

```csharp
QuickConverter.EquationTokenizer.AddAssembly(typeof(object).Assembly);
```

In addition to adding namespaces, extension methods must also be manually registered. To add all the typical LINQ IEnumerable extensions, call the following:

```csharp
QuickConverter.EquationTokenizer.AddExtensionMethods(typeof(Enumerable));
```

**Single Bindings (see QuickConverter Markup section for recommended syntax)**

QuickConverter.Binding allows you to write a binding with a custom two-way converters in one line of xaml code.

Here is a binding with a Boolean to System.Visibility converter written with QuickConverter:

```xml
<Control Visibility="{qc:Binding '$P ? Visibility.Visible : Visibility.Collapsed', P={Binding ShowElement}}" />
```

Following are two more examples of valid converter code:

```
'$P != null ? $P.GetType() == typeof(int) : false'
```

```
'(Double.Parse($P) + 123.0).ToString(\\’0.00\\’)'
```

Constructors and object initializers are also valid:

```xml
<Control FontSize="{qc:Binding 'new Dictionary\[string, int\]() { { \\'Sml\\', 16 }, { \\'Lrg\\', 32 } }\[$P\]', P={Binding TestIndex}}" />
```

```xml
<Control Content="{qc:Binding 'new TestObject(1,2,3) { FieldOne = \\'Hello\\', FieldTwo = \\'World\\' }}" />
```

\* Note that generic arguments are enclosed in square brackets instead of angle brackets. Xml doesn't play well with angle brackets.

The following shows how to write a two-way binding:

```xml
<Control Height="{qc:Binding '$P * 10', ConvertBack='$value * 0.1', P={Binding TestWidth, Mode=TwoWay}}" />
```

**Multi-Bindings**

Multibinding work in a very similar way.

The following demonstrates an inline multibinding:

```xml
<Control Angle="{qc:MultiBinding 'Math.Atan2($P0, $P1) * 180 / 3.14159', P0={Binding ActualHeight, ElementName=rootElement}, P1={Binding ActualWidth, ElementName=rootElement}}" />
```

**QuickConverter Markup**

Converters can also be created independently of the QuickConverter binding extensions. This allows an extra level of flexibility. The following is an example of this:

```xml
<Control Width="{Binding Data, Converter={qc:QuickConverter '$P * 10', ConvertBack='$value * 0.1'}}" />
```

**Local Variables**

Local variables can be used through a lambda expression like syntax. Local variables have precedence over binding variables and are only valid with the scope of the lambda expression in which they are declared. The following shows this:

```xml
<Control IsEnabled="{qc:Binding '(Loc = $P.Value, A = $P.Show) => $Loc != null ## $A', P={Binding Obj}}" />
```

\* Note that the "&&" operator must be written as "&amp;amp;&amp;amp;" in XML.

\*\* Due to a bug with the designer, using "&amp;amp;" in a markup extension breaks Intellisense. Instead of two ampersands, use the alternate syntax "##". "#" also works for bitwise and operations.

**Lambda Expressions**

Support for lambda expressions is limited, but its support is sufficient to allow LINQ to be used. They look quite similar to conventional C# lambda expressions, but there are a few important differences. First off, block expressions are not supported. Only single, inline statements are allowed. Also, the expression must return a value. Additionally, the function will default to object for all generic parameters (eg. Func<object, object>). This can be overridden with typecast operators. The following shows this:

```xml
<Control ItemsSource="{qc:Binding '$P.Where(( (int)i ) => (bool)($i % 2 == 0))', P={Binding Source}}" />
```

*Note: The parameters must always be enclosed by parenthesis.

**Null Propagation Operator**

The null propagation operator allows safe property/field, method, and indexer access on objects. When used, if the target object is null, instead of throwing an exception null is returned. The operator is implemented by "?". 

Instead of this:

```
'$P != null ? $P.Value : null'
```

You can write this:

```
'$P?.Value'
```

This can be combined with the null coalescing operator to change this:

```
'$P != null ? $P.GetType() == typeof(int) : false'
```

Into this:

```
'($P?.GetType() == typeof(int)) ?? false'
```

This operator is particularly useful in long statements where there are multiple accessors that could throw null exceptions. In this example, we assume Data can never be null when Value is not null.

```
'$P?.Value?.Data.ArrayData?\[4\]'
```

**QuickEvent**

This markup extension allows you to create event handlers inline. Aside from allowing void functions, the code is identical to QuickConverters. However, QuickEvent exposes a number of variables by default.

```
$sender - The sender of the event.

$eventArgs - The arguments object of the event.

$dataContext - The data context of the sender.

$V0-$V9 - The values set on the QuickEvent Vx properties.

$P0-$P4 - The values of the QuickEvent.P0-QuickEvent.P4 inherited attached properties on sender.

${name} - Any element within the name scope where {name} is the value of x:Name on that element.
```

An example:

```xml
<StackPanel qc:QuickEvent.P0="{Binding SomeValue}">
   <TextBlock x:Name="textField" />
   <Button Content="Click Me" Click="{qc:QuickEvent '$textField.Text = $dataContext.Transform($P0.Value)'}" />
</StackPanel>
```

The P0-P4 values are useful for crossing name scope boundaries like DataTemplates. Typically, when set outside a DataTemplate they will be inherited by the contents inside the DataTemplate. This allows you to easily make external controls and values accessible from inside DataTemplates.

**QuickValue**

This markup extension evaluates exactly like a QuickConverter except there are no P0-P9 variables, and it is evaluated at load. The markup extension returns the result of the expression.
