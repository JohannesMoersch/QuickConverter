using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QuickConverter;
using QuickConverter.Tokens;

namespace QuickConverter
{
	public static class EquationTokenizer
	{
		private static int[] precedenceLevel = new[] { 1, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 4, 5, 5, 6, 7, 8 };
		private static string[] representations = new[] { "+", "-", "!", "*", "/", "%", "+", "-", ">=", "<=", ">", "<", "==", "!=", "&&", "||", null };
		private static Tuple<string, string>[] namespaces = new Tuple<string, string>[0];
		private static Dictionary<string, Type> types = new Dictionary<string, Type>();
		private static HashSet<string> assemblies = new HashSet<string>();
		private static TokenBase[] valueTypeInstanceList;
		private static TokenBase[] postValueTypeInstanceList;
		static EquationTokenizer()
		{
			valueTypeInstanceList = new TokenBase[]
				{
					new StaticFunctionToken(),
					new ConstructorToken(),
					new StaticMemberToken(),
					new ParameterToken(),
					new ConstantToken(),
					new UnaryOperatorToken(),
					new BracketedToken(),
					new TypeCastToken(),
					new TypeofToken()
				};
			postValueTypeInstanceList = new TokenBase[]
				{
					new InstanceFunctionToken(),
					new InstanceMemberToken(),
					new ArrayAccessToken(),
					new IsToken()
				};
			types.Add("bool", typeof(bool));
			types.Add("byte", typeof(byte));
			types.Add("sbyte", typeof(sbyte));
			types.Add("short", typeof(short));
			types.Add("ushort", typeof(ushort));
			types.Add("int", typeof(int));
			types.Add("uint", typeof(uint));
			types.Add("long", typeof(long));
			types.Add("ulong", typeof(ulong));
			types.Add("float", typeof(float));
			types.Add("double", typeof(double));
			types.Add("decimal", typeof(decimal));
			types.Add("string", typeof(string));
			types.Add("object", typeof(object));
		}

		/// <summary>
		/// Adds a namespace for QuickConverter to use when looking up types.
		/// When namespaces span multiple assemblies, each assembly must be added separately.
		/// </summary>
		/// <param name="ns">The namespace to add.</param>
		/// <param name="assembly">The assembly which contains this namespace.</param>
		public static void AddNamespace(string ns, Assembly assembly)
		{
			namespaces = namespaces.Concat(new[] { new Tuple<string, string>(ns, assembly.FullName) }).ToArray();
		}

		/// <summary>
		/// Adds a namespace for QuickConverter to use when looking up types.
		/// When namespaces span multiple assemblies, each assembly must be added separately.
		/// </summary>
		/// <param name="type">The type whose namespace to add.</param>
		public static void AddNamespace(Type type)
		{
			namespaces = namespaces.Concat(new[] { new Tuple<string, string>(type.Namespace, type.Assembly.FullName) }).ToArray();
		}

		/// <summary>
		/// Adds an assembly to search through when using full type names.
		/// </summary>
		/// <param name="assembly">The assembly to add.</param>
		public static void AddAssembly(Assembly assembly)
		{
			assemblies.Add(assembly.FullName);
		}

		internal static bool TryGetType(string name, List<Type> typeParams, out Type type)
		{
			if (typeParams != null)
				name += "`" + typeParams.Count;
			if (types.TryGetValue(name, out type))
			{
				if (typeParams != null)
				{
					try { type = type.MakeGenericType(typeParams.ToArray()); }
					catch { type = null; }
				}
				return type != null;
			}
			if (name.Contains('.'))
			{
				type = assemblies.Select(s => Type.GetType(name + ", " + s)).FirstOrDefault();
				types.Add(name, type);
			}
			else
			{
				Type[] matches = namespaces.Select(str => Type.GetType(str.Item1 + "." + name + ", " + str.Item2)).Where(t => t != null).ToArray();
				if (matches.Length > 1)
					throw new Exception("Ambiguous type found. Could not choose between " + matches.Select(t => t.FullName).Aggregate((s1, s2) => s1 + " and " + s2) + ".");
				if (matches.Length != 0)
					type = matches[0];
			}
			if (type == null)
				return false;
			types.Add(name, type);
			if (typeParams != null)
			{
				try { type = type.MakeGenericType(typeParams.ToArray()); }
				catch { type = null; }
			}
			return type != null;
		}

		internal static bool TryGetValueToken(ref string text, out TokenBase token)
		{
			string temp = null;
			token = null;
			foreach (TokenBase type in valueTypeInstanceList)
			{
				temp = text;
				if (type.TryGetToken(ref temp, out token))
					break;
				token = null;
			}
			if (token == null)
				return false;
			text = temp;

			while (true)
			{
				bool cont = false;
				TokenBase newToken;
				foreach (TokenBase type in postValueTypeInstanceList)
				{
					if (type.TryGetToken(ref temp, out newToken))
					{
						(newToken as IPostToken).Target = token;
						token = newToken;
						text = temp;
						cont = true;
						break;
					}
				}
				if (!cont)
					break;
			}

			return true;
		}

		internal static bool TryEvaluateExpression(string text, out TokenBase token)
		{
			string temp = text;
			if (new TernaryOperatorToken().TryGetToken(ref temp, out token))
				return true;

			token = null;
			List<TokenBase> tokens = new List<TokenBase>();
			List<Operator> operators = new List<Operator>();
			while (operators.Count == tokens.Count)
			{
				TokenBase newToken;
				if (!TryGetValueToken(ref text, out newToken))
					return false;
				tokens.Add(newToken);
				text = text.TrimStart();
				for (int i = (int)Operator.Multiply; i < (int)Operator.Or; ++i)
				{
					if (text.StartsWith(representations[i]))
					{
						operators.Add((Operator)i);
						text = text.Substring(representations[i].Length).TrimStart();
						break;
					}
				}
			}
			if (!String.IsNullOrWhiteSpace(text))
				return false;
			while (tokens.Count > 1)
			{
				int lastPrecedence = 1000;
				int last = -1;
				for (int i = 0; i < operators.Count; ++i)
				{
					int precendence = precedenceLevel[(int)operators[i]];
					if (precendence < lastPrecedence)
					{
						lastPrecedence = precendence;
						last = i;
					}
					else
						break;
				}
				tokens[last] = new BinaryOperatorToken(tokens[last], tokens[last + 1], operators[last]);
				tokens.RemoveAt(last + 1);
				operators.RemoveAt(last);
			}
			token = tokens[0];
			return true;
		}

		/// <summary>
		/// Tokenizes the given expression into a token tree.
		/// </summary>
		/// <param name="expression">The string to tokenize.</param>
		/// <returns>The resulting root token.</returns>
		public static TokenBase Tokenize(string expression)
		{
			TokenBase token;
			if (!TryEvaluateExpression(expression, out token))
				throw new Exception("Failed to tokenize expression \"" + expression + "\".");
			return token;
		}
	}
}
