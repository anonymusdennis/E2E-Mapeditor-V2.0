using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods;

public static class TransformExtensions
{
	public delegate bool FindDescendantsCondition(Transform transform);

	public delegate bool FindAncestorCondition(Transform transform);

	public static bool AllDescendants(Transform transform)
	{
		return true;
	}

	public static IEnumerable<Transform> FindDescendants(this Transform transform, FindDescendantsCondition cond)
	{
		IEnumerator enumerator = transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Transform child = (Transform)enumerator.Current;
				if (cond(child))
				{
					yield return child;
				}
				foreach (Transform item in child.FindDescendants(cond))
				{
					yield return item;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			IDisposable disposable2 = (disposable = enumerator as IDisposable);
			if (disposable != null)
			{
				disposable2.Dispose();
			}
		}
	}

	public static IEnumerable<Transform> FindAncestor(this Transform transform, FindAncestorCondition cond)
	{
		Transform ancestor = transform.parent;
		while (ancestor != null)
		{
			if (cond(ancestor))
			{
				yield return ancestor;
			}
			ancestor = ancestor.parent;
		}
	}

	public static T Find<T>(this Transform transform, string name) where T : Component
	{
		T result = (T)null;
		Transform transform2 = transform.Find(name);
		if (transform2 != null)
		{
			result = transform2.GetComponent<T>();
		}
		return result;
	}

	public static GameObject FindGameObject(this Transform transform, string name)
	{
		GameObject result = null;
		Transform transform2 = transform.Find(name);
		if (transform2 != null)
		{
			result = transform2.gameObject;
		}
		return result;
	}

	public static string GetPath(this Transform current, char separator = '|', bool leadingSeparator = false)
	{
		if (current.parent == null)
		{
			if (leadingSeparator)
			{
				return separator + current.name;
			}
			return current.name;
		}
		return current.parent.GetPath() + separator + current.name;
	}

	public static bool HasComponent<T>(Transform transform)
	{
		return transform.GetComponent<T>() != null;
	}
}
