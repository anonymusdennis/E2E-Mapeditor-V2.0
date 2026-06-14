using System;
using System.Collections.Generic;
using UnityEngine;

public class HUDRootMenu : RootMenu
{
	public enum HUDMenuTypeToOpen
	{
		PlayerInfo,
		PlayerInfoTutorial
	}

	[HideInInspector]
	public HUDMenuTypeToOpen m_CurrentHUDMenuType;

	public Dictionary<HUDMenuTypeToOpen, MenuList_Container> m_HUDMenuTypes;

	protected override void Awake()
	{
		base.Awake();
	}

	public override void InitializeData()
	{
		if (m_bIsDataInitialized)
		{
			return;
		}
		m_RootMenuType = RootMenuType.HUD;
		base.InitializeData();
		m_HUDMenuTypes = new Dictionary<HUDMenuTypeToOpen, MenuList_Container>();
		HUDMenuTypeToOpen[] array = (HUDMenuTypeToOpen[])Enum.GetValues(typeof(HUDMenuTypeToOpen));
		for (int i = 0; i < array.Length; i++)
		{
			MenuList_Container menuList_Container = new MenuList_Container();
			menuList_Container.m_Menus = m_EditorTabAbleMenuTypes[i].menus;
			for (int j = 0; j < menuList_Container.m_Menus.Count; j++)
			{
				if (menuList_Container.m_Menus[j] != null)
				{
					menuList_Container.m_Menus[j].DoSingleTimeInitialize();
					menuList_Container.m_Menus[j].Hide();
				}
			}
			menuList_Container.m_DefaultTab = m_EditorTabAbleMenuTypes[i].m_DefaultTab;
			m_HUDMenuTypes.Add(array[i], menuList_Container);
		}
	}

	public override BaseMenuBehaviour GetMenuOFType<T>(RootMenuType typeofMenus)
	{
		for (int i = 0; i < m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus.Count; i++)
		{
			if (m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[i].GetType() == typeof(T))
			{
				return m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[i];
			}
		}
		return null;
	}

	public override int GetTabNumberOfType<T>()
	{
		for (int i = 0; i < m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus.Count; i++)
		{
			if (m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[i].GetType() == typeof(T))
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
		return m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[index];
	}

	public override List<BaseMenuBehaviour> GetCurrentMenuSet()
	{
		return m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_MainTabPanel != null)
		{
			m_MainTabPanel.Show(currentGamer, this, null);
			int index = 0;
			m_MainTabPanel.SetTabIndex(index);
		}
		else
		{
			int count = m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[i] != null)
				{
					m_HUDMenuTypes[m_CurrentHUDMenuType].m_Menus[i].Show(currentGamer, this, null);
				}
			}
		}
		return true;
	}

	public void SetHUDMenuTypeToOpen(HUDMenuTypeToOpen type)
	{
		m_CurrentHUDMenuType = type;
	}
}
