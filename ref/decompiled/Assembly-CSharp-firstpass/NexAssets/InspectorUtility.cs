using System;
using System.Collections.Generic;

namespace NexAssets;

public static class InspectorUtility
{
	public static ulong GetUlong(string numstring, ulong min = 0uL, ulong max = ulong.MaxValue)
	{
		ulong result = 0uL;
		if (!ulong.TryParse(numstring, out result))
		{
			result = 0uL;
		}
		return Math.Max(min, Math.Min(result, max));
	}

	public static int ChangeEverythingFlag<T>(int flag)
	{
		if (flag >= 0)
		{
			return flag;
		}
		int num = 0;
		foreach (object value in Enum.GetValues(typeof(T)))
		{
			if ((flag & (int)value) != 0)
			{
				num |= (int)value;
			}
		}
		return num;
	}

	public static void ValidateTag(List<string> tags)
	{
		if (tags == null)
		{
			return;
		}
		ValidateList(tags, 16);
		for (int i = 0; i < tags.Count; i++)
		{
			if ((long)tags[i].Length > 24L)
			{
				tags[i] = tags[i].Remove(24);
			}
		}
	}

	public static void ValidateList<T>(List<T> list, int max)
	{
		if (list != null && list.Count > max)
		{
			list.RemoveRange(max - 1, list.Count - max);
		}
	}
}
