using System.Collections.Generic;
using UnityEngine;

public static class StaticExtensionMethods
{
	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = Random.Range(0, num);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void NormalizeAndMag(this Vector2 v, out float mag)
	{
		mag = Mathf.Sqrt(v.x * v.x + v.y * v.y);
		float num = 1f / mag;
		v.x = num * v.x;
		v.y = num * v.y;
	}
}
