using UnityEngine;
using UnityEngine.UI;

public class T17_PassThroughNavigation : MonoBehaviour
{
	public Selectable m_SelectableToMonitor;

	public bool PassThroughVertical = true;

	public bool PassThroughHorizontal;

	private Navigation m_InitialNavigation;

	private Navigation m_OrigUpNavigation;

	private Navigation m_OrigDownNavigation;

	private Navigation m_OrigLeftNavigation;

	private Navigation m_OrigRightNavigation;

	private bool m_bSet;

	private void Awake()
	{
	}

	public void SetPassThrough()
	{
		if (m_bSet)
		{
			return;
		}
		m_bSet = true;
		Init();
		if (PassThroughVertical)
		{
			Selectable selectOnUp = m_SelectableToMonitor.navigation.selectOnUp;
			if (selectOnUp != null)
			{
				selectOnUp.navigation = SetBindings(selectOnUp.navigation);
			}
			Selectable selectOnDown = m_SelectableToMonitor.navigation.selectOnDown;
			if (selectOnDown != null)
			{
				selectOnDown.navigation = SetBindings(selectOnDown.navigation);
			}
		}
		if (PassThroughHorizontal)
		{
			Selectable selectOnLeft = m_SelectableToMonitor.navigation.selectOnLeft;
			if (selectOnLeft != null)
			{
				selectOnLeft.navigation = SetBindings(selectOnLeft.navigation);
			}
			Selectable selectOnRight = m_SelectableToMonitor.navigation.selectOnRight;
			if (selectOnRight != null)
			{
				selectOnRight.navigation = SetBindings(selectOnRight.navigation);
			}
		}
	}

	public void RestorePassThrough()
	{
		if (m_bSet)
		{
			m_bSet = false;
			if (m_InitialNavigation.selectOnUp != null)
			{
				m_InitialNavigation.selectOnUp.navigation = m_OrigUpNavigation;
			}
			if (m_InitialNavigation.selectOnDown != null)
			{
				m_InitialNavigation.selectOnDown.navigation = m_OrigDownNavigation;
			}
			if (m_InitialNavigation.selectOnLeft != null)
			{
				m_InitialNavigation.selectOnLeft.navigation = m_OrigLeftNavigation;
			}
			if (m_InitialNavigation.selectOnRight != null)
			{
				m_InitialNavigation.selectOnRight.navigation = m_OrigRightNavigation;
			}
		}
	}

	private Navigation SetBindings(Navigation nav)
	{
		Navigation result = nav;
		if (nav.mode == Navigation.Mode.Explicit)
		{
			if (nav.selectOnDown == m_SelectableToMonitor)
			{
				result.selectOnDown = m_SelectableToMonitor.navigation.selectOnDown;
			}
			if (nav.selectOnLeft == m_SelectableToMonitor)
			{
				result.selectOnLeft = m_SelectableToMonitor.navigation.selectOnLeft;
			}
			if (nav.selectOnRight == m_SelectableToMonitor)
			{
				result.selectOnRight = m_SelectableToMonitor.navigation.selectOnRight;
			}
			if (nav.selectOnUp == m_SelectableToMonitor)
			{
				result.selectOnUp = m_SelectableToMonitor.navigation.selectOnUp;
			}
		}
		return result;
	}

	private void Init()
	{
		m_InitialNavigation = m_SelectableToMonitor.navigation;
		if (m_InitialNavigation.selectOnUp != null)
		{
			m_OrigUpNavigation = m_InitialNavigation.selectOnUp.navigation;
		}
		if (m_InitialNavigation.selectOnDown != null)
		{
			m_OrigDownNavigation = m_InitialNavigation.selectOnDown.navigation;
		}
		if (m_InitialNavigation.selectOnLeft != null)
		{
			m_OrigLeftNavigation = m_InitialNavigation.selectOnLeft.navigation;
		}
		if (m_InitialNavigation.selectOnRight != null)
		{
			m_OrigRightNavigation = m_InitialNavigation.selectOnRight.navigation;
		}
	}
}
