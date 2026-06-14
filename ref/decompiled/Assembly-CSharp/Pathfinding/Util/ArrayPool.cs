using System;
using System.Collections.Generic;

namespace Pathfinding.Util;

public static class ArrayPool<T>
{
	private static readonly Stack<T[]>[] pool = new Stack<T[]>[31];

	private static readonly HashSet<T[]> inPool = new HashSet<T[]>();

	public static T[] Claim(int minimumLength)
	{
		int i;
		for (i = 0; 1 << i < minimumLength && i < 30; i++)
		{
		}
		if (i == 30)
		{
			throw new ArgumentException("Too high minimum length");
		}
		lock (pool)
		{
			if (pool[i] == null)
			{
				pool[i] = new Stack<T[]>();
			}
			if (pool[i].Count > 0)
			{
				T[] array = pool[i].Pop();
				inPool.Remove(array);
				return array;
			}
		}
		return new T[1 << i];
	}

	public static void Release(ref T[] array)
	{
		lock (pool)
		{
			int i;
			for (i = 0; 1 << i < array.Length && i < 30; i++)
			{
			}
			if (array.Length != 1 << i)
			{
				throw new ArgumentException("Array length is not a power of 2");
			}
			if (pool[i] == null)
			{
				pool[i] = new Stack<T[]>();
			}
			pool[i].Push(array);
		}
		array = null;
	}
}
