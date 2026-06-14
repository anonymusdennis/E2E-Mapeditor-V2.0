using UnityEngine;

public static class LayerHelper
{
	public static float GetZOffset(Transform transform, float offsetY = 0f, float spanY = 1f)
	{
		return GetZOffset(transform.position.y, offsetY, spanY);
	}

	public static float GetZOffset(Vector3 pos, float offsetY = 0f, float spanY = 1f)
	{
		return GetZOffset(pos.y, offsetY, spanY);
	}

	public static float GetZOffset(float posY, float offsetY = 0f, float spanY = 1f)
	{
		posY -= offsetY;
		posY -= spanY / 2f - 0.5f;
		float num = posY - 60f;
		return num * 0.025f;
	}
}
