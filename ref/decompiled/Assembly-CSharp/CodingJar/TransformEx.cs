using System.Linq;
using System.Text;
using UnityEngine;

namespace CodingJar;

public static class TransformEx
{
	public static void DestroyChildren(this Transform transform)
	{
		if (!(transform == null))
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform child = transform.GetChild(i);
				child.gameObject.SetActive(value: false);
				Object.Destroy(child.gameObject);
			}
		}
	}

	public static string FullPath(this Transform transform)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (transform != null)
		{
			stringBuilder.Insert(0, transform.name);
			stringBuilder.Insert(0, '/');
			transform = transform.parent;
		}
		return stringBuilder.ToString();
	}

	public static string GetPathRelativeTo(this Transform transform, Transform parent)
	{
		if (transform == parent)
		{
			return string.Empty;
		}
		if (transform.IsChildOf(parent))
		{
			return transform.FullPath().Substring(parent.FullPath().Length + 1);
		}
		return transform.FullPath();
	}

	public static T FindInParents<T>(this Transform transform, bool bIncludeSelf = true) where T : Component
	{
		T[] componentsInParent = transform.GetComponentsInParent<T>(includeInactive: true);
		if (componentsInParent.Length < 1)
		{
			return (T)null;
		}
		if (bIncludeSelf)
		{
			return componentsInParent.FirstOrDefault();
		}
		return componentsInParent.SkipWhile((T x) => x.transform == transform).FirstOrDefault();
	}
}
