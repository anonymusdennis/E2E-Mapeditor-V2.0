using UnityEngine;

namespace ExtensionMethods;

public static class Matrix4x4Extensions
{
	public static bool AlmostEquals(this Matrix4x4 thisMatrix4x4, object other, float tolerance)
	{
		if (!(other is Matrix4x4 matrix4x))
		{
			return false;
		}
		return thisMatrix4x4.GetColumn(0).AlmostEquals(matrix4x.GetColumn(0), tolerance) && thisMatrix4x4.GetColumn(1).AlmostEquals(matrix4x.GetColumn(1), tolerance) && thisMatrix4x4.GetColumn(2).AlmostEquals(matrix4x.GetColumn(2), tolerance) && thisMatrix4x4.GetColumn(3).AlmostEquals(matrix4x.GetColumn(3), tolerance);
	}
}
