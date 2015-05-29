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
	public class AssignmentToken : TokenBase, IPostToken
	{
		internal AssignmentToken()
		{
		}

		public override Type ReturnType { get { return Value.ReturnType; } }

		public override TokenBase[] Children { get { return new[] { Target, Value }; } }

		public TokenBase Target { get; private set; }

		public TokenBase Value { get; private set; }

		public Operator Operator { get; private set; }

		internal override bool SetPostTarget(TokenBase target)
		{
			if (target is StaticMemberToken)
			{
				if ((target as StaticMemberToken).Member is PropertyInfo)
				{
					if (!((target as StaticMemberToken).Member as PropertyInfo).CanWrite)
						throw new Exception("Static member \"" + ((target as StaticMemberToken).Member as PropertyInfo).Name + "\" is readonly and cannot be set.");
					else if (Operator != default(Operator) && !((target as StaticMemberToken).Member as PropertyInfo).CanRead)
						throw new Exception("Static member \"" + ((target as StaticMemberToken).Member as PropertyInfo).Name + "\" is writeonly and cannot be read.");
				}
				else if (!((target as StaticMemberToken).Member is FieldInfo))
					return false;
			}
			else if (!(target is InstanceMemberToken))
				return false;
			Target = target;
			if (Operator != default(Operator))
				Value = new BinaryOperatorToken(Target, Value, Operator);
			return true;
		}

		internal override bool TryGetToken(ref string text, out TokenBase token, bool requireReturnValue = true)
		{
			token = null;
			string temp = text;
			var op = default(Operator);
			if (temp.Length < 2)
				return false;
			if (temp[0] != '=')
			{
				if (temp[1] != '=' || temp.Length < 3)
					return false;
				for (int i = (int)Operator.Multiply; i <= (int)Operator.Subtract; ++i)
				{
					if (EquationTokenizer.representations[i][0] == temp[0])
						op = (Operator)i;
				}
				for (int i = (int)Operator.BitwiseAnd; i <= (int)Operator.BitwiseXor; ++i)
				{
					if (EquationTokenizer.representations[i][0] == temp[0])
						op = (Operator)i;
				}
				if (op == default(Operator))
					return false;
			}

			if (op == default(Operator))
				temp = temp.Substring(1).TrimStart();
			else
				temp = temp.Substring(2).TrimStart();
			TokenBase valToken;
			if (!EquationTokenizer.TryEvaluateExpression(temp, out valToken))
				return false;
			text = "";
			token = new AssignmentToken() { Value = valToken, Operator = op };
			return true;
		}

		internal override Expression GetExpression(List<ParameterExpression> parameters, Dictionary<string, ConstantExpression> locals, List<DataContainer> dataContainers, Type dynamicContext, LabelTarget label, bool requiresReturnValue = true)
		{
			var value = Value.GetExpression(parameters, locals, dataContainers, dynamicContext, label);
			if (Target is InstanceMemberToken)
			{
				CallSiteBinder binder = Binder.SetMember(CSharpBinderFlags.None, (Target as InstanceMemberToken).MemberName, dynamicContext ?? typeof(object), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
				return Expression.Dynamic(binder, typeof(object), (Target as InstanceMemberToken).Target.GetExpression(parameters, locals, dataContainers, dynamicContext, label), value);
			}
			else
			{
				var type = (Target as StaticMemberToken).Member is PropertyInfo ? ((Target as StaticMemberToken).Member as PropertyInfo).PropertyType : ((Target as StaticMemberToken).Member as FieldInfo).FieldType;
				CallSiteBinder binder = Binder.Convert(CSharpBinderFlags.None, type, dynamicContext ?? typeof(object));
				return Expression.Convert(Expression.Assign(Expression.MakeMemberAccess(null, (Target as StaticMemberToken).Member), Expression.Dynamic(binder, type, value)), typeof(object));
			}
		}
	}
}
