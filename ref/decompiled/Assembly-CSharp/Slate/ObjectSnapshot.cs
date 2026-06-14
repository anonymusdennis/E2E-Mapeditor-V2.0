using System.Collections.Generic;
using UnityEngine;

namespace Slate;

public class ObjectSnapshot
{
	private Dictionary<Object, string> serialized;

	public ObjectSnapshot(Object target, bool fullObjectHierarchy = false)
	{
		Store(target, fullObjectHierarchy);
	}

	public void Store(Object target, bool fullObjectHierarchy = false)
	{
		if (target == null)
		{
			return;
		}
		serialized = new Dictionary<Object, string>();
		if (target is MonoBehaviour || target is ScriptableObject)
		{
			serialized[target] = JsonUtility.ToJson(target);
		}
		if (!(target is GameObject))
		{
			return;
		}
		GameObject gameObject = (GameObject)target;
		Component[] array = ((!fullObjectHierarchy) ? gameObject.GetComponents<Component>() : gameObject.GetComponentsInChildren<Component>(includeInactive: true));
		foreach (Component component in array)
		{
			if (component != null && !(component is BoxCollider) && component is MonoBehaviour)
			{
				serialized[component] = JsonUtility.ToJson(component);
			}
		}
	}

	public void Restore()
	{
		foreach (KeyValuePair<Object, string> item in serialized)
		{
			if (item.Key != null && (item.Key is MonoBehaviour || item.Key is ScriptableObject))
			{
				JsonUtility.FromJsonOverwrite(item.Value, item.Key);
			}
		}
	}
}
