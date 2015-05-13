using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CSharp.RuntimeBinder;

namespace QuickConverter.Tokens
{
	public class IndexerToken : TokenBase, IPostToken
	{
		internal IndexerToken()
		{
		}

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return new[] { Indices, Target }; } }

		public ArgumentListToken Indices { get; private set; }
		public TokenBase Target { get; private set; }

		internal override bool SetPostTarget(TokenBase target)
		{
			Target = target;
			return true;
		}
		
		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			string temp = text;
			if (temp.Length < 3 || temp[0] != '[')
				return false;

			bool inQuotes = false;
			int brackets = 0;
			int i = 0;
			while (true)
			{
				if (i >= temp.Length)
					return false;
				if (i > 0 && temp[i] == '\'' && temp[i - 1] != '\\')
					inQuotes = !inQuotes;
				else if (!inQuotes)
				{
					if (temp[i] == '[')
						++brackets;
					else if (temp[i] == ']')
					{
						--brackets;
						if (brackets == 0)
							break;
					}
				}
				++i;
			}

			TokenBase ind;
			if (!new ArgumentListToken('[', ']').TryGetToken(ref temp, out ind))
				return false;
			text = text.Substring(i + 1);
			token = new IndexerToken() { Indices = ind as ArgumentListToken };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			CallSiteBinder binder = Binder.GetIndex(CSharpBinderFlags.None, dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return Expression.Dynamic(binder, typeof(object), new Expression[] { Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label) }.Concat(Indices.Arguments.Select(token => token.GetExpression(parameters, locals, dataContainers, dynamicContext, label))));
		}
	}
}
