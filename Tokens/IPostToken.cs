using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickConverter.Tokens
{
	public interface IPostToken
	{
		TokenBase Target { get; }
	}
}
