using System;
using UnityEngine;

public class OptionsHelpMenu : BaseMenuBehaviour
{
	[Serializable]
	public struct VisibleOverride
	{
		public SwitchControllerType controllerType;

		public GameObject[] objectsToHide;
	}

	public GameObject[] m_ContentPages;

	private int? m_CurrentActiveContentPage;

	public VisibleOverride m_OverrideVisibility;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		m_CurrentActiveContentPage = null;
		if (m_ContentPages != null)
		{
			for (int i = 0; i < m_ContentPages.Length; i++)
			{
				if (m_ContentPages[i].activeSelf)
				{
					m_CurrentActiveContentPage = i;
					break;
				}
			}
		}
		if (!GlobalStart.GetInstance().IsWithinLevel())
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Frontend Help Screen", "Help 'Tab' in FE Options screen selected", string.Empty, 0L);
		}
		else
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("In-Game Help Screen", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " In-Game help accessed", string.Empty, 0L);
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		return true;
	}

	public void OnContentButtonClicked(GameObject theContentPage)
	{
		if (!(theContentPage != null) || m_ContentPages == null)
		{
			return;
		}
		for (int i = 0; i < m_ContentPages.Length; i++)
		{
			if (m_ContentPages[i] == theContentPage)
			{
				if (m_CurrentActiveContentPage != i)
				{
					SetContentPageActive(m_CurrentActiveContentPage, active: false);
					m_CurrentActiveContentPage = i;
					SetContentPageActive(i, active: true);
				}
				break;
			}
		}
	}

	private void SetContentPageActive(int? pageIndex, bool active)
	{
		if (m_ContentPages != null && pageIndex.HasValue && (!pageIndex.HasValue || pageIndex.GetValueOrDefault() <= m_ContentPages.Length))
		{
			if (active)
			{
				m_ContentPages[pageIndex.Value].SetActive(value: true);
			}
			else
			{
				m_ContentPages[pageIndex.Value].SetActive(value: false);
			}
		}
	}

	public void OnCancel(FrontendMenuBehaviour menu)
	{
		FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
	}

	public void UpdateOverriddenVisibilities()
	{
	}
}
