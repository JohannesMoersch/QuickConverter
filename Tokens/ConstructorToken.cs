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

		public override Type ReturnType { get { return ConstructorType != null ? ConstructorType : ArrayType.MakeArrayType(); } }

		public override TokenBase[] Children 
		{ 
			get 
			{
				if (Arguments != null)
				{
					if (Initializers != null)
						return new[] { Arguments, Initializers };
					else
						return new[] { Arguments };
				}
				else if (Initializers != null)
					return new[] { Initializers };
				return new TokenBase[0];
			} 
		}

		public Type ArrayType { get; private set; }
		public Type ConstructorType { get; private set; }
		public ConstructorInfo Constructor { get; private set; }
		public ArgumentListToken Arguments { get; private set; }
		public ArgumentListToken Initializers { get; private set; } // Allow non-assignments for arrays and ICollection<>
		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
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
			List<ConstructorInfo> cons = null;
			if (type.IsArray)
			{
				type = type.GetElementType();
				array = true;
				string s = "[]";
				new ArgumentListToken('[', ']').TryGetToken(ref s, out args);
			}
			else if (array)
			{
				if (!new ArgumentListToken('[', ']').TryGetToken(ref temp, out args))
					return false;
			}
			else
			{
				if (!new ArgumentListToken('(', ')').TryGetToken(ref temp, out args))
					return false;
				if ((args as ArgumentListToken).Arguments.Length > 0 || type.IsClass)
				{
					cons = type.GetConstructors().Where(info => info.GetParameters().Length == (args as ArgumentListToken).Arguments.Length).ToList();
					for (int i = cons.Count - 1; i >= 0; --i)
					{
						for (int j = 0; j < (args as ArgumentListToken).Arguments.Length; ++j)
						{
							TypeCastToken cast = (args as ArgumentListToken).Arguments[j] as TypeCastToken;
							if (cast != null && !cons[i].GetParameters()[j].ParameterType.IsAssignableFrom(cast.TargetType))
							{
								cons.RemoveAt(j);
								break;
							}
						}
					}

					if (cons.Count > 1)
						throw new Exception("Ambiguous constructor call with " + (args as ArgumentListToken).Arguments.Length + " parameter(s) found for type " + type.FullName + ". Try using type casts to disambiguate the call.");
					if (cons.Count == 0)
						return false;
				}
			}

			Type genericType = null;
			TokenBase inits = null;
			string str = temp.TrimStart();
			if (str.Length > 0 && str[0] == '{')
			{
				if (array && (args as ArgumentListToken).Arguments.Length != 0)
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
				if ((args as ArgumentListToken).Arguments.Length == 0 && inits == null)
					return false;
				token = new ConstructorToken() { Arguments = args as ArgumentListToken, ArrayType = type, Initializers = inits as ArgumentListToken };
			}
			else if (genericType != null)
				token = new ConstructorToken() { Arguments = args as ArgumentListToken, ArrayType = genericType.GetGenericArguments()[0], Constructor = cons != null ? cons[0] : null, ConstructorType = type, Initializers = inits as ArgumentListToken };
			else
				token = new ConstructorToken() { Arguments = args as ArgumentListToken, Constructor = cons != null ? cons[0] : null, ConstructorType = type, Initializers = inits as ArgumentListToken };
			text = temp;
			return true;
		}

		private IEnumerable<object> ConvertInitializers(ArgumentListToken arguments, List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, Type[] expectedTypes, LabelTarget label)
		{
			foreach (TokenBase token in arguments.Arguments)
			{
				if (token is ArgumentListToken)
				{
					MethodInfo add = Constructor.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == (token as ArgumentListToken).Arguments.Length);
					int i = 0;
					Expression[] exps = new Expression[add.GetParameters().Length];
					foreach (ParameterInfo info in add.GetParameters())
					{
						CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, info.ParameterType, dynamicContext);
						exps[i] = Expression.Dynamic(binder, info.ParameterType, (token as ArgumentListToken).Arguments[i++].GetExpression(parameters, locals, dataContainers, dynamicContext, label));
					}
					yield return Expression.ElementInit(add, exps);
				}
				else
				{
					MethodInfo add = Constructor.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1);
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, add.GetParameters()[0].ParameterType, dynamicContext);
					yield return Expression.ElementInit(add, Expression.Dynamic(binder, add.GetParameters()[0].ParameterType, token.GetExpression(parameters, locals, dataContainers, dynamicContext, label)));
				}
			}
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			Expression exp;
			if (ConstructorType != null)
			{
				if (Constructor != null)
				{
					ParameterInfo[] info = Constructor.GetParameters();
					List<Expression> args = new List<Expression>();
					for (int i = 0; i < info.Length; ++i)
						args.Add(Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, info[i].ParameterType, dynamicContext ?? typeof(object)), info[i].ParameterType, Arguments.Arguments[i].GetExpression(parameters, locals, dataContainers, dynamicContext, label)));
					exp = Expression.New(Constructor, args);
				}
				else
					exp = Expression.New(ConstructorType);
				if (Initializers != null)
				{
					if (Initializers.Arguments.Any(token => token is LambdaAssignmentToken))
					{
						Func<MemberInfo, Type> getType = mem => mem is FieldInfo ? (mem as FieldInfo).FieldType : (mem as PropertyInfo).PropertyType;
						var inits = Initializers.Arguments.Cast<LambdaAssignmentToken>().Select(token => new Tuple<MemberInfo, Expression>(token.Member, Expression.Dynamic(Binder.Convert(CSharpBinderFlags.None, getType(token.Member), dynamicContext ?? typeof(object)), getType(token.Member), token.Value.GetExpression(parameters, locals, dataContainers, dynamicContext, label))));
						exp = Expression.MemberInit(exp as NewExpression, inits.Select(init => (MemberBinding)Expression.Bind(init.Item1, init.Item2)));
					}
					else
						exp = Expression.ListInit(exp as NewExpression, ConvertInitializers(Initializers, parameters, locals, dataContainers, dynamicContext, null, label).Cast<ElementInit>());
				}
			}
			else
			{
				if (Initializers != null)
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, ArrayType, dynamicContext ?? typeof(object));
					exp = Expression.NewArrayInit(ArrayType, Initializers.Arguments.Select(token => Expression.Dynamic(binder, ArrayType, token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))));
				}
				else
				{
					CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, typeof(int), dynamicContext ?? typeof(object));
					exp = Expression.NewArrayBounds(ArrayType, Arguments.Arguments.Select(token => Expression.Dynamic(binder, typeof(int), token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))));
				}
			}
			return Expression.Convert(exp, typeof(object));
		}
	}
}
