using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Slate;

public static class TransformExtensions
{
	public static Vector3 GetLocalEulerAngles(this Transform transform)
	{
		if (Application.isPlaying)
		{
			return transform.localEulerAngles;
		}
		return transform.localEulerAngles;
	}

	public static void SetLocalEulerAngles(this Transform transform, Vector3 value)
	{
		if (Application.isPlaying)
		{
			transform.localEulerAngles = value;
		}
		else
		{
			transform.localEulerAngles = value;
		}
	}

	public static T GetAddComponent<T>(this GameObject go) where T : Component
	{
		return go.GetAddComponent(typeof(T)) as T;
	}

	public static T GetAddComponent<T>(this Component comp) where T : Component
	{
		return comp.gameObject.GetAddComponent(typeof(T)) as T;
	}

	public static Component GetAddComponent(this GameObject go, Type type)
	{
		Component component = go.GetComponent(type);
		if (component == null)
		{
			component = go.AddComponent(type);
		}
		return component;
	}

	public static Transform FindInChildren(this Transform root, string name, bool includeHidden)
	{
		if (root == null || string.IsNullOrEmpty(name))
		{
			return root;
		}
		return root.GetComponentsInChildren<Transform>(includeHidden).FirstOrDefault((Transform t) => t.name == name);
	}

	public static List<string> GetBlendShapeNames(this SkinnedMeshRenderer skinnedMesh)
	{
		List<string> list = new List<string>();
		if (skinnedMesh == null)
		{
			return list;
		}
		for (int i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; i++)
		{
			list.Add(skinnedMesh.sharedMesh.GetBlendShapeName(i));
		}
		return list;
	}

	public static int GetBlendShapeIndex(this SkinnedMeshRenderer skinnedMesh, string shapeName)
	{
		if (skinnedMesh == null)
		{
			return -1;
		}
		for (int i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; i++)
		{
			if (skinnedMesh.sharedMesh.GetBlendShapeName(i) == shapeName)
			{
				return i;
			}
		}
		return -1;
	}

	public static string SplitCamelCase(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = char.ToUpper(s[0]) + s.Substring(1);
		return Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1").Trim();
	}
}
