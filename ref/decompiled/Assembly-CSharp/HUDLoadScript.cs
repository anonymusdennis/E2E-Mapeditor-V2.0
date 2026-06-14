using System.Collections.Generic;
using UnityEngine;

public class HUDLoadScript : MonoBehaviour
{
	public bool m_PreBuildBehaviourLists;

	[HideInInspector]
	public T17NetworkBehaviour[] m_NetworkBehaviourClasses;

	[HideInInspector]
	public T17MonoBehaviour[] m_MonoBehaviourClasses;

	protected static HUDLoadScript m_Instance;

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public static HUDLoadScript GetInstance()
	{
		return m_Instance;
	}

	public void SetPreBuiltBehaviourLists(List<T17NetworkBehaviour> nets, List<T17MonoBehaviour> monos)
	{
		m_NetworkBehaviourClasses = new T17NetworkBehaviour[nets.Count];
		for (int i = 0; i < m_NetworkBehaviourClasses.Length; i++)
		{
			m_NetworkBehaviourClasses[i] = nets[i];
		}
		m_MonoBehaviourClasses = new T17MonoBehaviour[monos.Count];
		for (int i = 0; i < m_MonoBehaviourClasses.Length; i++)
		{
			m_MonoBehaviourClasses[i] = monos[i];
		}
	}
}
