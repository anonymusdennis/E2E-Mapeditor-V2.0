using System;
using System.Collections.Generic;

public static class FindExtention
{
	public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}
}
