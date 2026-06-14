using UnityEngine;

public class PerPlatformActivator : MonoBehaviour
{
	public Platform.PlatformOverride m_Platform;

	public GameObject m_ObjectToDestroy;

	public GameObject m_ObjectToKeep;

	public bool m_EnableObjects = true;

	public bool m_Test;

	private void Awake()
	{
		DoActivate();
	}

	public void DoActivate()
	{
		bool flag = false;
		if (flag | (m_Platform == Platform.PlatformOverride.Standalone))
		{
			if (m_ObjectToDestroy != null)
			{
				m_ObjectToDestroy.transform.SetParent(null);
				Object.Destroy(m_ObjectToDestroy);
			}
			if (m_ObjectToKeep != null && m_EnableObjects)
			{
				m_ObjectToKeep.SetActive(value: true);
			}
		}
		else
		{
			if (m_ObjectToDestroy != null && m_EnableObjects)
			{
				m_ObjectToDestroy.SetActive(value: true);
			}
			if (m_ObjectToKeep != null)
			{
				m_ObjectToKeep.transform.SetParent(null);
				Object.Destroy(m_ObjectToKeep);
			}
		}
		Object.Destroy(this);
	}
}
