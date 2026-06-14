using UnityEngine;

namespace ExtensionMethods;

public static class ColorExtensions
{
	public static Color ModifiedAlpha(this Color color, float alpha)
	{
		Color result = color;
		result.a = alpha;
		return result;
	}
}
