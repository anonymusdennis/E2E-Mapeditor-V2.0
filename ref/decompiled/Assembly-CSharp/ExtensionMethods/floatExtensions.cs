using UnityEngine;

namespace ExtensionMethods;

public static class floatExtensions
{
	public static bool AlmostEquals(this float thisFloat, object other, float tolerance)
	{
		if (!(other is float num))
		{
			return false;
		}
		return Mathf.Abs(thisFloat - num) < tolerance;
	}
}
