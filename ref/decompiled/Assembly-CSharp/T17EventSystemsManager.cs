using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class T17EventSystemsManager
{
	private static T17EventSystemsManager m_Instance;

	private Dictionary<Gamer, T17EventSystem> m_EventSystems;

	private Dictionary<Gamer, GameObject> m_SelectedBeforeDisable;

	private List<T17EventSystem> m_FreeEventSystems;

	private int m_DisabledRefCount;

	public static T17EventSystemsManager Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new T17EventSystemsManager();
			}
			return m_Instance;
		}
	}

	private T17EventSystemsManager()
	{
		m_EventSystems = new Dictionary<Gamer, T17EventSystem>();
		m_SelectedBeforeDisable = new Dictionary<Gamer, GameObject>();
		m_FreeEventSystems = new List<T17EventSystem>();
		Gamer.OnDeleteImminent += Gamer_OnDeleteImminent;
	}

	~T17EventSystemsManager()
	{
		Gamer.OnDeleteImminent -= Gamer_OnDeleteImminent;
	}

	private void Gamer_OnDeleteImminent(Gamer gamer)
	{
		if (m_EventSystems.ContainsKey(gamer))
		{
			T17EventSystem eventSystem = m_EventSystems[gamer];
			ResetEventSystem(eventSystem);
		}
		if (m_DisabledRefCount > 0 && m_SelectedBeforeDisable.ContainsKey(gamer))
		{
			m_SelectedBeforeDisable.Remove(gamer);
			if (m_SelectedBeforeDisable.Count == 0)
			{
				EnableAllEventSystems();
			}
		}
	}

	private void ResetEventSystem(T17EventSystem eventSystem)
	{
		eventSystem.ResetSystem();
		eventSystem.enabled = true;
		m_FreeEventSystems.Add(eventSystem);
	}

	public void ResetAll()
	{
		foreach (KeyValuePair<Gamer, T17EventSystem> eventSystem in m_EventSystems)
		{
			ResetEventSystem(eventSystem.Value);
		}
		m_EventSystems.Clear();
		m_SelectedBeforeDisable.Clear();
		m_DisabledRefCount = 0;
	}

	public void RegisterEventSystem(T17EventSystem eventSystem)
	{
		eventSystem.SetAssignedGamer(null);
		m_FreeEventSystems.Add(eventSystem);
	}

	public void AssignFreeEventSystemToGamer(Gamer gamer)
	{
		if (gamer == null || m_EventSystems.ContainsKey(gamer))
		{
			return;
		}
		for (int num = m_FreeEventSystems.Count - 1; num >= 0; num--)
		{
			if (m_FreeEventSystems[num].AssignedGamer == null)
			{
				m_FreeEventSystems[num].SetAssignedGamer(gamer);
				m_EventSystems.Add(gamer, m_FreeEventSystems[num]);
				m_FreeEventSystems.RemoveAt(num);
				break;
			}
		}
	}

	public T17EventSystem GetEventSystemForGamer(Gamer gamer)
	{
		if (gamer == null)
		{
			return null;
		}
		if (m_EventSystems.ContainsKey(gamer))
		{
			return m_EventSystems[gamer];
		}
		return null;
	}

	public T17EventSystem GetEventSystemForRewiredPlayer(Rewired.Player player)
	{
		if (player == null)
		{
			T17NetManager.LogGoogleException("GetEventSystemForRewiredPlayer - player is NULL");
			return null;
		}
		foreach (KeyValuePair<Gamer, T17EventSystem> eventSystem in m_EventSystems)
		{
			if (eventSystem.Key != null && eventSystem.Key.m_RewiredPlayer == player)
			{
				return eventSystem.Value;
			}
		}
		return null;
	}

	public void DisableAllEventSystemsExceptFor(Gamer gamer)
	{
		m_DisabledRefCount++;
		if (m_DisabledRefCount != 1)
		{
			return;
		}
		foreach (KeyValuePair<Gamer, T17EventSystem> eventSystem in m_EventSystems)
		{
			if (!m_SelectedBeforeDisable.ContainsKey(eventSystem.Key))
			{
				m_SelectedBeforeDisable.Add(eventSystem.Key, eventSystem.Value.currentSelectedGameObject);
			}
			if (eventSystem.Key != gamer)
			{
				eventSystem.Value.enabled = false;
			}
		}
	}

	public void EnableAllEventSystems()
	{
		m_DisabledRefCount--;
		if (m_DisabledRefCount == 0)
		{
			foreach (KeyValuePair<Gamer, T17EventSystem> eventSystem in m_EventSystems)
			{
				eventSystem.Value.enabled = true;
				if (m_SelectedBeforeDisable.ContainsKey(eventSystem.Key))
				{
					eventSystem.Value.SetSelectedGameObject(null);
					eventSystem.Value.SetSelectedGameObject(m_SelectedBeforeDisable[eventSystem.Key]);
				}
			}
			m_SelectedBeforeDisable.Clear();
		}
		else if (m_DisabledRefCount >= 0)
		{
		}
	}
}
