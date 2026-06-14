using System;
using System.Collections.Generic;
using UnityEngine;

public class ResultsRootMenu : RootMenu
{
	public enum ResultsMenuTypeToOpen
	{
		CoopResults,
		VersusResults
	}

	[HideInInspector]
	public ResultsMenuTypeToOpen m_CurrentResultsMenuType;

	public Dictionary<ResultsMenuTypeToOpen, MenuList_Container> m_ResultMenuTypes;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void InitializeData()
	{
		m_RootMenuType = RootMenuType.Results;
		base.InitializeData();
		m_ResultMenuTypes = new Dictionary<ResultsMenuTypeToOpen, MenuList_Container>();
		ResultsMenuTypeToOpen[] array = (ResultsMenuTypeToOpen[])Enum.GetValues(typeof(ResultsMenuTypeToOpen));
		for (int i = 0; i < array.Length; i++)
		{
			MenuList_Container menuList_Container = new MenuList_Container();
			menuList_Container.m_Menus = m_EditorTabAbleMenuTypes[i].menus;
			for (int j = 0; j < menuList_Container.m_Menus.Count; j++)
			{
				if (menuList_Container.m_Menus[j] != null)
				{
					menuList_Container.m_Menus[j].Hide();
				}
			}
			menuList_Container.m_DefaultTab = m_EditorTabAbleMenuTypes[i].m_DefaultTab;
			m_ResultMenuTypes.Add(array[i], menuList_Container);
		}
	}

	public override BaseMenuBehaviour GetMenuOFType<T>(RootMenuType typeofMenus)
	{
		for (int i = 0; i < m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus.Count; i++)
		{
			if (m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus[i].GetType() == typeof(T))
			{
				return m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus[i];
			}
		}
		return null;
	}

	public override int GetTabNumberOfType<T>()
	{
		for (int i = 0; i < m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus.Count; i++)
		{
			if (m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus[i].GetType() == typeof(T))
			{
				return i;
			}
		}
		return -1;
	}

	public override BaseMenuBehaviour GetCurrentOpenMenu()
	{
		int index = 0;
		if (m_MainTabPanel != null)
		{
			index = m_MainTabPanel.CurrentTabIndex;
		}
		return m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus[index];
	}

	public override List<BaseMenuBehaviour> GetCurrentMenuSet()
	{
		return m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (!(m_MainTabPanel != null))
		{
			m_ResultMenuTypes[m_CurrentResultsMenuType].m_Menus[m_ResultMenuTypes[m_CurrentResultsMenuType].m_DefaultTab].Show(currentGamer, this, null);
		}
		return true;
	}

	public void SetResultsMenuTypeToOpen(ResultsMenuTypeToOpen type)
	{
		m_CurrentResultsMenuType = type;
	}
}
