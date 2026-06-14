using UnityEngine;

namespace ExtensionMethods;

public static class ComponentExtensions
{
	public static string GetComponentPath(this Component component, char separator = '|', bool leadingSeparator = false)
	{
		return component.transform.GetPath(separator) + separator + component.GetType().ToString();
	}
}
