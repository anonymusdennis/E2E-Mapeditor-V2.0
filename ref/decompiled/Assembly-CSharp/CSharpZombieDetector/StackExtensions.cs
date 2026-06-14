using System.Collections.Generic;
using System.Linq;

namespace CSharpZombieDetector;

public static class StackExtensions
{
	public static Stack<T> Clone<T>(this Stack<T> source)
	{
		return new Stack<T>(source.Reverse());
	}
}
