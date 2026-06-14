using UnityEngine;

namespace Slate;

public abstract class Path : MonoBehaviour
{
	public abstract float length { get; }

	public abstract Vector3 GetPointAt(float t);

	public static Vector3 GetCubicCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
	{
		t = Mathf.Clamp01(t);
		Vector3 vector = Mathf.Pow(1f - t, 3f) * p1;
		Vector3 vector2 = 3f * Mathf.Pow(1f - t, 2f) * t * p2;
		Vector3 vector3 = 3f * (1f - t) * Mathf.Pow(t, 2f) * p3;
		Vector3 vector4 = Mathf.Pow(t, 3f) * p4;
		return vector + vector2 + vector3 + vector4;
	}

	public static Vector3 GetQuadraticCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		t = Mathf.Clamp01(t);
		Vector3 vector = Mathf.Pow(1f - t, 2f) * p1;
		Vector3 vector2 = 2f * (1f - t) * t * p2;
		Vector3 vector3 = Mathf.Pow(t, 2f) * p3;
		return vector + vector2 + vector3;
	}

	public static Vector3 GetLinearPoint(Vector3 p1, Vector3 p2, float t)
	{
		return p1 + (p2 - p1) * t;
	}

	public static Vector3 GetPoint(float t, params Vector3[] path)
	{
		if (t <= 0f)
		{
			return path[0];
		}
		if (t >= 1f)
		{
			return path[path.Length - 1];
		}
		Vector3 a = Vector3.zero;
		Vector3 b = Vector3.zero;
		float num = 0f;
		float num2 = 0f;
		float num3 = GetLength(path);
		for (int i = 0; i < path.Length - 1; i++)
		{
			num2 = Vector3.Distance(path[i], path[i + 1]) / num3;
			if (num + num2 > t)
			{
				a = path[i];
				b = path[i + 1];
				break;
			}
			num += num2;
		}
		t -= num;
		return Vector3.Lerp(a, b, t / num2);
	}

	public static float GetLength(Vector3[] path)
	{
		if (path == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < path.Length; i++)
		{
			num += Vector3.Distance(path[i], path[(i != path.Length - 1) ? (i + 1) : i]);
		}
		return num;
	}
}
