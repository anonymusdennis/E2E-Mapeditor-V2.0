using UnityEngine;

namespace ParadoxNotion;

public static class RectUtils
{
	public static Rect GetBoundRect(params Rect[] rects)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		for (int i = 0; i < rects.Length; i++)
		{
			num = Mathf.Min(num, rects[i].xMin);
			num2 = Mathf.Max(num2, rects[i].xMax);
			num3 = Mathf.Min(num3, rects[i].yMin);
			num4 = Mathf.Max(num4, rects[i].yMax);
		}
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static Rect GetBoundRect(params Vector2[] positions)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		for (int i = 0; i < positions.Length; i++)
		{
			num = Mathf.Min(num, positions[i].x);
			num2 = Mathf.Max(num2, positions[i].x);
			num3 = Mathf.Min(num3, positions[i].y);
			num4 = Mathf.Max(num4, positions[i].y);
		}
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static bool Encapsulates(this Rect a, Rect b)
	{
		return a.x < b.x && a.xMax > b.xMax && a.y < b.y && a.yMax > b.yMax;
	}
}
