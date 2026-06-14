using System;
using UnityEngine;

public class UIProxyLink : MonoBehaviour
{
	[Serializable]
	public enum LinkDirection
	{
		left,
		right,
		top,
		bottom
	}

	public BaseMenuBehaviour m_theLink;

	public LinkDirection m_LinkSelectableToUse = LinkDirection.right;

	private bool m_bOnSelected;

	private void Update()
	{
		if (!m_bOnSelected)
		{
			return;
		}
		m_bOnSelected = false;
		if (!(m_theLink != null))
		{
			return;
		}
		Gamer currentGamer = m_theLink.CurrentGamer;
		if (currentGamer == null)
		{
			return;
		}
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(currentGamer);
		if (!(eventSystemForGamer != null))
		{
			return;
		}
		BaseMenuBehaviour baseMenuBehaviour = m_theLink;
		if (m_theLink is T17TabPanel)
		{
			baseMenuBehaviour = ((T17TabPanel)m_theLink).GetCurrentPage();
		}
		if (!(baseMenuBehaviour != null))
		{
			return;
		}
		switch (m_LinkSelectableToUse)
		{
		case LinkDirection.left:
			if (baseMenuBehaviour.m_LeftSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_LeftSelectable.gameObject);
			}
			else if (baseMenuBehaviour.m_TopSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_TopSelectable.gameObject);
			}
			break;
		case LinkDirection.right:
			if (baseMenuBehaviour.m_RightSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_RightSelectable.gameObject);
			}
			else if (baseMenuBehaviour.m_TopSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_TopSelectable.gameObject);
			}
			break;
		case LinkDirection.bottom:
			if (baseMenuBehaviour.m_BottomSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_BottomSelectable.gameObject);
			}
			else if (baseMenuBehaviour.m_TopSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_TopSelectable.gameObject);
			}
			break;
		default:
			if (baseMenuBehaviour.m_TopSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(baseMenuBehaviour.m_TopSelectable.gameObject);
			}
			break;
		}
	}

	public void OnSelected()
	{
		m_bOnSelected = true;
	}
}
