using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExtensionMethods;

public static class GameObjectExtensions
{
	public delegate bool GetComponentsWhereDelegate<T>(T c) where T : Component;

	public static T GetComponentInChildren<T>(this GameObject go, bool includeInactive) where T : Component
	{
		return go.GetComponentsInChildren<T>(includeInactive).FirstOrDefault();
	}

	public static IEnumerable<T> GetComponentsWhere<T>(this GameObject go, GetComponentsWhereDelegate<T> test) where T : Component
	{
		T[] components = go.GetComponents<T>();
		foreach (T child in components)
		{
			if (test(child))
			{
				yield return child;
			}
		}
	}

	public static bool IsSelectionObject(this GameObject go)
	{
		return false;
	}

	public static T[] GetInterfaces<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		MonoBehaviour[] components = gObj.GetComponents<MonoBehaviour>();
		try
		{
			return (from a in components
				where a != null && a.GetType().GetInterfaces().Any((Type k) => k == typeof(T))
				select (T)(object)a).ToArray();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return null;
		}
	}

	public static T GetInterface<T>(this GameObject gObj)
	{
		try
		{
			if (!typeof(T).IsInterface)
			{
				throw new SystemException("Specified type is not an interface!");
			}
			return gObj.GetInterfaces<T>().FirstOrDefault();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return default(T);
		}
	}

	public static T GetInterfaceInChildren<T>(this GameObject gObj, bool includeInactive = false)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		return gObj.GetInterfacesInChildren<T>(includeInactive).FirstOrDefault();
	}

	public static T[] GetInterfacesInChildren<T>(this GameObject gObj, bool includeInactive = false)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		MonoBehaviour[] componentsInChildren = gObj.GetComponentsInChildren<MonoBehaviour>(includeInactive);
		return (from a in componentsInChildren
			where a.GetType().GetInterfaces().Any((Type k) => k == typeof(T))
			select (T)(object)a).ToArray();
	}

	public static string GetPath(this GameObject go, char separator = '|', bool leadingSeparator = false)
	{
		return go.transform.GetPath(separator);
	}
}
