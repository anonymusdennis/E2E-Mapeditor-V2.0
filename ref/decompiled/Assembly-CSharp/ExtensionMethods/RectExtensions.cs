using UnityEngine;

namespace ExtensionMethods;

public static class RectExtensions
{
	public static Rect Setup(this Rect r, float x, float y, float width, float height)
	{
		r.Set(x, y, width, height);
		return r;
	}
}
