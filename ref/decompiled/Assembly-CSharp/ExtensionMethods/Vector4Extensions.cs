using UnityEngine;

namespace ExtensionMethods;

public static class Vector4Extensions
{
	public static bool AlmostEquals(this Vector4 thisVector4, object other, float tolerance)
	{
		if (!(other is Vector4 vector))
		{
			return false;
		}
		return thisVector4.x.AlmostEquals(vector.x, tolerance) && thisVector4.y.AlmostEquals(vector.y, tolerance) && thisVector4.z.AlmostEquals(vector.z, tolerance) && thisVector4.w.AlmostEquals(vector.w, tolerance);
	}
}
