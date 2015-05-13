using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class TypeCastToken : TokenBase
	{
		internal TypeCastToken(bool parseTarget = true)
		{
			this.parseTarget = parseTarget;
		}

		public override Type ReturnType { get { return TargetType; } }

		public override TokenBase[] Children { get { return new[] { Target }; } }

		public Type TargetType { get; private set; }

		public TokenBase Target { get; internal set; }

		private bool parseTarget;

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			bool inQuotes = false;
			int brackets = 0;
			int i = 0;
			while (true)
			{
				if (i >= text.Length)
					return false;
				if (i > 0 && text[i] == '\'' && text[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (text[i] == '(')
						++brackets;
					else if (text[i] == ')')
					{
						--brackets;
						if (brackets == 0)
							break;
					}
				}
				++i;
			}
			string temp = text.Substring(1, i - 1);

			var tuple = GetNameMatches(temp, null, null).Where(tup => tup.Item1 is Type).Reverse().FirstOrDefault();
			if (tuple == null)
				return false;

			temp = text.Substring(i + 1).TrimStart();
			TokenBase valToken = null;
			if (parseTarget && !EquationTokenizer.TryGetValueToken(ref temp, out valToken))
				return false;
			text = temp;
			token = new TypeCastToken() { TargetType = tuple.Item1 as Type, Target = valToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.ConvertExplicit, TargetType, dynamicContext ?? typeof(object));
			return Expression.Convert(Expression.Dynamic(binder, TargetType, Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label)), typeof(object));
		}
	}
}
