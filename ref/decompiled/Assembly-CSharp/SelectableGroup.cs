using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectableGroup : MonoBehaviour
{
	private List<Selectable> m_Selectables = new List<Selectable>();

	private int m_LastSelectedIndex = -1;

	private UnityAction<BaseEventData> m_OnSelectEvent;

	public void AddNewSelectable(Selectable sel)
	{
		if (!(sel != null) || m_Selectables.Contains(sel))
		{
			return;
		}
		m_Selectables.Add(sel);
		T17_UISelectDeselectEvents component = sel.GetComponent<T17_UISelectDeselectEvents>();
		if (component != null)
		{
			m_OnSelectEvent = delegate
			{
				OnSelectableSelected(sel);
			};
			component.m_OnSelectEvent.AddListener(m_OnSelectEvent);
		}
	}

	public void RemoveSelectable(Selectable sel)
	{
		int num = m_Selectables.FindIndex((Selectable x) => x == sel);
		if (num != -1)
		{
			if (m_LastSelectedIndex >= num)
			{
				m_LastSelectedIndex--;
			}
			m_Selectables.Remove(sel);
			T17_UISelectDeselectEvents component = sel.GetComponent<T17_UISelectDeselectEvents>();
			if (component != null && m_OnSelectEvent != null)
			{
				component.m_OnSelectEvent.RemoveListener(m_OnSelectEvent);
			}
		}
	}

	public Selectable GetLastSelectedSelectableInGroup()
	{
		if (m_LastSelectedIndex != -1 && m_LastSelectedIndex < m_Selectables.Count)
		{
			return m_Selectables[m_LastSelectedIndex];
		}
		return null;
	}

	public void OnSelectableSelected(Selectable sel)
	{
		if (m_Selectables.Contains(sel))
		{
			m_LastSelectedIndex = m_Selectables.FindIndex((Selectable x) => x == sel);
		}
	}
}
