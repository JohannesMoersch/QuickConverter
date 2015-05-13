using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QuickConverter.Tokens
{
	public class ArgumentListToken : TokenBase
	{
		private char open;
		private char close;
		private bool findAssignments;
		private Type assignmentType;
		private bool allowSubLists;
		private bool allowTypeCasts;

		public override Type ReturnType { get { return typeof(object); } }

		public override TokenBase[] Children { get { return Arguments.ToArray(); } }

		public TokenBase[] Arguments { get; private set; }

		internal ArgumentListToken(char open, char close, Type assignmentType)
		{
			this.open = open;
			this.close = close;
			findAssignments = true;
			this.assignmentType = assignmentType;
			allowSubLists = false;
			allowTypeCasts = false;
		}

		internal ArgumentListToken(char open, char close, bool allowSubLists = false)
		{
			this.open = open;
			this.close = close;
			findAssignments = false;
			assignmentType = null;
			this.allowSubLists = allowSubLists;
			allowTypeCasts = false;
		}

		internal ArgumentListToken(bool allowTypeCasts, char open, char close)
		{
			this.open = open;
			this.close = close;
			findAssignments = false;
			assignmentType = null;
			allowSubLists = false;
			this.allowTypeCasts = allowTypeCasts;
		}

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			var list = new List<TokenBase>();
			List<string> split;
			string temp = text;
			if (!TrySplitByCommas(ref temp, open, close, out split))
				return false;
			foreach (string str in split)
			{
				TokenBase newToken;
				string s = str.Trim();
				if (allowSubLists && s.StartsWith(open.ToString()) && s.EndsWith(close.ToString()))
				{
					if (new ArgumentListToken(open, close).TryGetToken(ref s, out newToken))
						list.Add(newToken);
					else
						return false;
				}
				else if (findAssignments)
				{
					if (new LambdaAssignmentToken(assignmentType).TryGetToken(ref s, out newToken))
						list.Add(newToken);
					else
						return false;
				}
				else if (allowTypeCasts)
				{
					if (new TypeCastToken(false).TryGetToken(ref s, out newToken))
					{
						string nameTemp = "$" + s;
						TokenBase tokenTemp;
						if (!new ParameterToken().TryGetToken(ref nameTemp, out tokenTemp) || !String.IsNullOrWhiteSpace(nameTemp))
							return false;
						(newToken as TypeCastToken).Target = tokenTemp;
						list.Add(newToken);
					}
					else
					{
						string nameTemp = "$" + s;
						TokenBase tokenTemp;
						if (!new ParameterToken().TryGetToken(ref nameTemp, out tokenTemp) || !String.IsNullOrWhiteSpace(nameTemp))
							return false;
						list.Add(tokenTemp);
					}
				}
				else
				{
					if (EquationTokenizer.TryEvaluateExpression(str.Trim(), out newToken))
						list.Add(newToken);
					else
						return false;
				}
			}
			token = new ArgumentListToken('\0', '\0') { Arguments = list.ToArray() };
			text = temp;
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			throw new NotImplementedException();
		}
	}
}
