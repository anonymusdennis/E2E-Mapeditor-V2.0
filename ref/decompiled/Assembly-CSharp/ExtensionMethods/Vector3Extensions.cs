using UnityEngine;

namespace ExtensionMethods;

public static class Vector3Extensions
{
	public static Vector3 Normalize(this Vector3 v, out float magnitude)
	{
		magnitude = v.magnitude;
		if (magnitude > float.Epsilon)
		{
			return v / magnitude;
		}
		return Vector3.zero;
	}
}
