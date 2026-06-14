using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectiveSceneElement : MonoBehaviour
{
	public enum ObjectiveSceneElementType
	{
		ItemContainer,
		Character,
		GameObject,
		Collider,
		InteractiveObject,
		Cutscene,
		MultistageInputInteraction
	}

	public ObjectiveSceneElementType m_LinksTo;

	[ReadOnly]
	public int m_ObjectiveElementID = -1;

	[ReadOnly]
	public string m_UsedInScene = "Not Set";

	public static List<ObjectiveSceneElement> m_AllSceneReferences;

	private void Awake()
	{
		RegisterWithGlobalSceneReferences();
	}

	public void RegisterWithGlobalSceneReferences()
	{
		if (m_AllSceneReferences == null)
		{
			m_AllSceneReferences = new List<ObjectiveSceneElement>();
		}
		if (!m_AllSceneReferences.Contains(this))
		{
			m_AllSceneReferences.Add(this);
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_AllSceneReferences != null)
		{
			m_AllSceneReferences.Remove(this);
		}
	}

	public static void Cleanup()
	{
		if (m_AllSceneReferences != null)
		{
			m_AllSceneReferences.Clear();
			m_AllSceneReferences = null;
		}
	}

	public static ObjectiveSceneElement FindSceneReference(int id)
	{
		if (m_AllSceneReferences == null)
		{
			return null;
		}
		return m_AllSceneReferences.FirstOrDefault((ObjectiveSceneElement r) => r.m_ObjectiveElementID == id);
	}
}
