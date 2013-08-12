using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace QuickConverter.Tokens
{
	public class ConstructorToken : TokenBase
	{
		internal ConstructorToken()
		{
		}

		private Type arrayType;
		private ConstructorInfo constructor;
		private ArgumentListToken arguments;
		private ArgumentListToken initializers; // Allow non-assignments for arrays and ICollection<>
		internal override bool TryGetToken(ref string text, out TokenBase token)
		{
			token = null;
			if (!text.StartsWith("new"))
				return false;
			string temp = text.Substring(3).TrimStart();

			var tuple = GetNameMatches(temp, null, null).Where(tup => tup.Item1 is Type).Reverse().FirstOrDefault();
			if (tuple == null)
				return false;

			Type type = tuple.Item1 as Type;
			temp = tuple.Item2;

			temp = temp.TrimStart();
			if (temp.Length == 0)
				return false;
			bool array = temp[0] == '[';
			TokenBase args;
			ConstructorInfo[] cons = null;
			if (array)
			{
				if (!new ArgumentListToken('[', ']').TryGetToken(ref temp, out args))
					return false;
			}
			else
			{
				if (!new ArgumentListToken('(', ')').TryGetToken(ref temp, out args))
					return false;
				cons = type.GetConstructors().Where(info => info.GetParameters().Length == (args as ArgumentListToken).Arguments.Count).ToArray();
				if (cons.Length > 1)
					throw new Exception("Ambiguous constructor call with " + (args as ArgumentListToken).Arguments.Count + " parameters found for type " + type.FullName + ".");
				if (cons.Length == 0)
					return false;
			}

			Type genericType = null;
			TokenBase inits = null;
			string str = temp.TrimStart();
			MethodInfo method = null;
			if (str.Length > 0 && str[0] == '{')
			{
				if (array && (args as ArgumentListToken).Arguments.Count != 0)
					throw new Exception("Array size arguments and array initializers cannot be used in conjunction.");
				genericType = type.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>));
				if (array || genericType != null)
				{
					if (!new ArgumentListToken('{', '}', true).TryGetToken(ref str, out inits))
						return false;
				}
				else
				{
					if (!new ArgumentListToken('{', '}', type).TryGetToken(ref str, out inits))
						return false;
				}
				temp = str;
			}

			if (array)
			{
				if ((args as ArgumentListToken).Arguments.Count == 0 && inits == null)
					return false;
				token = new ConstructorToken() { arguments = args as ArgumentListToken, arrayType = type, initializers = inits as ArgumentListToken };
			}
			else if (genericType != null)
				token = new ConstructorToken() { arguments = args as ArgumentListToken, arrayType = genericType.GetGenericArguments()[0], constructor = cons[0], initializers = inits as ArgumentListToken };
			else
				token = new ConstructorToken() { arguments = args as ArgumentListToken, constructor = cons[0], initializers = inits as ArgumentListToken };
			text = temp;
			return true;
		}

		private IEnumerable<object> ConvertInitializers(ArgumentListToken arguments, List<ParameterExpression> parameters, Type dynamicContext, Type[] expectedTypes)
		{
			foreach (TokenBase token in arguments.Arguments)
			{
				if (token is ArgumentListToken)
				{
					MethodInfo add = constructor.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == (token as ArgumentListToken).Arguments.Count);
					int i = 0;
					Expression[] exps = new Expression[add.GetParameters().Length];
					foreach (ParameterInfo info in add.GetParameters())
					{
						CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, info.ParameterType, dynamicContext);
						exps[i] = Expression.Dynamic(binder, info.ParameterType, (token as ArgumentListToken).Arguments[i++].GetExpression(parameters, dynamicContext));
					}
					yield return Expression.ElementInit(add, exps);
				}
				else
				{
					MethodInfo add = constructor.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, add.GetParameters()[0].ParameterType, dynamicContext);
					yield return Expression.ElementInit(add, Expression.Dynamic(binder, add.GetParameters()[0].ParameterType, token.GetExpression(parameters, dynamicContext)));
				}
			}
		}

		public override Expression GetExpression(List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			Expression exp;
			if (constructor != null)
			{
				ParameterInfo[] info = constructor.GetParameters();
				List<Expression> args = new List<Expression>();
				for (int i = 0; i < info.Length; ++i)
					args.Add(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, info[i].ParameterType, dynamicContext ?? typeof(object)), info[i].ParameterType, arguments.Arguments[i].GetExpression(parameters, dynamicContext)));
				exp = Expression.New(constructor, args);
				if (initializers != null)
				{
					args.Clear();
					if (initializers.Arguments.Any(token => token is AssignmentToken))
					{
						Func<MemberInfo, Type> getType = mem => mem is FieldInfo ? (mem as FieldInfo).FieldType : (mem as PropertyInfo).PropertyType;
						var inits = initializers.Arguments.Cast<AssignmentToken>().Select(token => new Tuple<MemberInfo, Expression>(token.Member, Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, getType(token.Member), dynamicContext ?? typeof(object)), getType(token.Member), token.Value.GetExpression(parameters, dynamicContext))));
						exp = Expression.MemberInit(exp as NewExpression, inits.Select(init => (MemberBinding)Expression.Bind(init.Item1, init.Item2)));
					}
					else
						exp = Expression.ListInit(exp as NewExpression, ConvertInitializers(initializers, parameters, dynamicContext, null).Cast<ElementInit>());
				}
			}
			else
			{
				if (initializers != null)
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, arrayType, dynamicContext ?? typeof(object));
					exp = Expression.NewArrayInit(arrayType, initializers.Arguments.Select(token => Expression.Dynamic(binder, arrayType, token.GetExpression(parameters, dynamicContext))));
				}
				else
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, typeof(int), dynamicContext ?? typeof(object));
					exp = Expression.NewArrayBounds(arrayType, arguments.Arguments.Select(token => Expression.Dynamic(binder, typeof(int), token.GetExpression(parameters, dynamicContext))));
				}
			}
			return Expression.Convert(exp, typeof(object));
		}
	}
}
