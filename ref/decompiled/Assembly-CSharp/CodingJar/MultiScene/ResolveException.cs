using System;

namespace CodingJar.MultiScene;

public class ResolveException : Exception
{
	public ResolveException(string message)
		: base(message)
	{
	}
}
