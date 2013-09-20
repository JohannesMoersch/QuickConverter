using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QuickConverter.Tokens
{
	public abstract class TokenBase
	{
		private static Dictionary<char, char> brackets = new Dictionary<char, char>();
		static TokenBase()
		{
			brackets.Add('(', ')');
			brackets.Add('[', ']');
			brackets.Add('{', '}');
		}

		internal static IEnumerable<Tuple<object, string>> GetNameMatches(string name, string previous, Type parent)
		{
			if (name.Length > 0 && (Char.IsLetter(name[0]) || name[0] == '_'))
			{
				int count = 1;
				while (count < name.Length && (Char.IsLetterOrDigit(name[count]) || name[count] == '_'))
					++count;
				string val = (previous != null && parent == null ? previous + "." : "") + name.Substring(0, count);
				string temp = name.Substring(count).TrimStart();
				List<Type> typeArgs = null;
				if (temp.Length > 0 && temp[0] == '[')
				{
					typeArgs = new List<Type>();
					List<string> split;
					if (TrySplitByCommas(ref temp, '[', ']', out split))
					{
						foreach (string str in split)
						{
							Tuple<object, string> tuple = GetNameMatches(str.Trim(), null, null).FirstOrDefault(tp => tp.Item1 is Type && string.IsNullOrWhiteSpace(tp.Item2));
							if (tuple == null)
								yield break;
							typeArgs.Add(tuple.Item1 as Type);
						}
					}
				}
				bool more = temp.Length > 0 && temp[0] == '.';
				if (parent == null)
				{
					Type type;
					if (EquationTokenizer.TryGetType(val, typeArgs, out type))
					{
						yield return new Tuple<object, string>(type, temp);
						if (more)
						{
							foreach (var match in GetNameMatches(temp.Substring(1), val, type))
								yield return match;
						}
					}
					if (more)
					{
						foreach (var match in GetNameMatches(temp.Substring(1), val, null))
							yield return match;
					}
				}
				else
				{
					foreach (MemberInfo info in parent.GetMember(val, BindingFlags.Public | BindingFlags.Static))
					{
						if (info.MemberType == MemberTypes.Method)
						{
							MemberInfo ret = info;
							if ((typeArgs != null) != (info as MethodInfo).IsGenericMethodDefinition)
								continue;
							if ((info as MethodInfo).IsGenericMethodDefinition && typeArgs.Count != (info as MethodInfo).GetGenericArguments().Length)
								continue;
							if ((info as MethodInfo).IsGenericMethodDefinition)
							{
								try { ret = (info as MethodInfo).MakeGenericMethod(typeArgs.ToArray()); }
								catch { continue; }
							}
							yield return new Tuple<object, string>(ret, temp);
						}
						else if (info.MemberType == MemberTypes.Field || info.MemberType == MemberTypes.Property)
							yield return new Tuple<object, string>(info, temp);
					}
				}
			}
		}

		protected static bool TrySplitByCommas(ref string str, char open, char close, out List<string> split)
		{
			split = new List<string>();
			bool inQuotes = false;
			int lastPos = 1;
			int i = 0;
			Stack<char> bracketing = new Stack<char>();
			while (true)
			{
				if (i >= str.Length)
					return false;
				if (i > 0 && str[i] == '\'' && str[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (bracketing.Count == 0)
					{
						if (str[i] == open)
							bracketing.Push(str[i]);
					}
					else
					{
						if (brackets.Keys.Contains(str[i]))
							bracketing.Push(str[i]);
						else if (brackets.Values.Contains(str[i]))
						{
							if (bracketing.Count == 1)
							{
								if (str[i] != close)
									return false;
								break;
							}
							if (brackets[bracketing.Peek()] != str[i])
								return false;
							bracketing.Pop();
						}
						else if (bracketing.Count == 1 && str[i] == ',')
						{
							split.Add(str.Substring(lastPos, i - lastPos));
							lastPos = i + 1;
						}
					}
				}
				if (bracketing.Count == 0 && !Char.IsWhiteSpace(str[i]))
					return false;
				++i;
			}
			if (i != 1)
				split.Add(str.Substring(lastPos, i - lastPos));
			str = str.Substring(i + 1);
			return true;
		}

		internal abstract bool TryGetToken(ref string text, out TokenBase token);

		public Expression GetExpression(out List<ParameterExpression> parameters, Type dynamicContext = null)
		{
			parameters = new List<ParameterExpression>();
			return GetExpression(parameters, new Dictionary<string, ConstantExpression>(), dynamicContext);
		}

		internal abstract Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, Type dynamicContext);
	}
}
