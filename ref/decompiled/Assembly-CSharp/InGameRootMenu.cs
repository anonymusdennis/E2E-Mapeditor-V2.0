using System;
using System.Collections.Generic;
using UnityEngine;

public class InGameRootMenu : RootMenu
{
	public enum InGameMenuTypeToOpen
	{
		MainSelf,
		Inmate,
		DownedInmate,
		FavourInmate,
		ShopInmate,
		Guard,
		DownedGuard,
		Desk,
		Toilet,
		SwagBag,
		Cutlrey,
		RobinsonFavour,
		RobinsonContinueFavour
	}

	[HideInInspector]
	public InGameMenuTypeToOpen m_CurrentInGameMenuType;

	public Dictionary<InGameMenuTypeToOpen, MenuList_Container> m_InGameTabableMenuTypes;

	public T17Button m_ClickOffButton;

	[SerializeField]
	private GameObject TabLeft;

	[SerializeField]
	private GameObject TabRight;

	private int m_EnabledMenuTypeMask = -1;

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
		m_RootMenuType = RootMenuType.InGame;
		base.InitializeData();
		m_InGameTabableMenuTypes = new Dictionary<InGameMenuTypeToOpen, MenuList_Container>();
		InGameMenuTypeToOpen[] array = (InGameMenuTypeToOpen[])Enum.GetValues(typeof(InGameMenuTypeToOpen));
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
			m_InGameTabableMenuTypes.Add(array[i], menuList_Container);
		}
	}

	public override BaseMenuBehaviour GetMenuOFType<T>(RootMenuType typeofMenus)
	{
		for (int i = 0; i < m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count; i++)
		{
			if (m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i].GetType() == typeof(T))
			{
				return m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i];
			}
		}
		return null;
	}

	public override int GetTabNumberOfType<T>()
	{
		for (int i = 0; i < m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count; i++)
		{
			if (m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i].GetType() == typeof(T))
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
		return m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[index];
	}

	public override List<BaseMenuBehaviour> GetCurrentMenuSet()
	{
		return m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_ClickOffButton != null)
		{
			m_ClickOffButton.SetGamerForEventSystem(currentGamer, base.CachedEventSystem);
		}
		if (m_MainTabPanel != null)
		{
			m_MainTabPanel.Show(currentGamer, this, null);
			int num = 0;
			for (int i = 0; i < m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count; i++)
			{
				GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i];
				if (gameMenuBehaviour != null)
				{
					gameMenuBehaviour.m_bMenuIsEnabled = CheckMenuEnabled(gameMenuBehaviour.m_MenuType);
				}
			}
			m_MainTabPanel.SetMenuBodies(m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus);
			num = m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_DefaultTab;
			m_MainTabPanel.AttemptToSetTabIndex(num);
			bool bCanInteractWithMouse = true;
			if (m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count == 1)
			{
				bCanInteractWithMouse = false;
			}
			T17Button[] buttons = m_MainTabPanel.m_Buttons;
			int j = 0;
			for (int num2 = buttons.Length; j < num2; j++)
			{
				buttons[j].m_bCanInteractWithMouse = bCanInteractWithMouse;
			}
		}
		else
		{
			m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_DefaultTab].Show(currentGamer, this, null);
		}
		int count = m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count;
		if (count > 1)
		{
			TabLeft.SetActive(value: true);
			TabRight.SetActive(value: true);
		}
		else
		{
			TabLeft.SetActive(value: false);
			TabRight.SetActive(value: false);
		}
		return true;
	}

	public void SetInGameMenuTypeToOpen(InGameMenuTypeToOpen type)
	{
		m_CurrentInGameMenuType = type;
	}

	public bool HasPanelsToShow(InGameMenuTypeToOpen type)
	{
		bool flag = false;
		for (int i = 0; i < m_InGameTabableMenuTypes[type].m_Menus.Count; i++)
		{
			GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)m_InGameTabableMenuTypes[type].m_Menus[i];
			if (gameMenuBehaviour != null)
			{
				flag |= CheckMenuEnabled(gameMenuBehaviour.m_MenuType);
			}
		}
		return flag;
	}

	public override void SetGamePlayerForMenus(Player gamePlayer)
	{
		if (m_PlayerInventoryOnThisRoot != null)
		{
			m_PlayerInventoryOnThisRoot.SetGamePlayer(gamePlayer);
		}
		if (m_InGameTabableMenuTypes[m_CurrentInGameMenuType] == null)
		{
			return;
		}
		for (int i = 0; i < m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus.Count; i++)
		{
			if (m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i] != null)
			{
				m_InGameTabableMenuTypes[m_CurrentInGameMenuType].m_Menus[i].SetGamePlayer(gamePlayer);
			}
		}
	}

	public bool CheckMenuEnabled(InGameMenuTypes menuType)
	{
		if (menuType >= (InGameMenuTypes)0 && menuType <= (InGameMenuTypes)2147483647 && (int)((uint)m_EnabledMenuTypeMask & (uint)menuType) > 0)
		{
			return true;
		}
		return false;
	}

	public void SetMenuEnabled(InGameMenuTypes menuType, bool enabled)
	{
		if (menuType >= (InGameMenuTypes)0 && menuType <= (InGameMenuTypes)2147483647)
		{
			if (enabled)
			{
				m_EnabledMenuTypeMask |= (int)menuType;
			}
			else
			{
				m_EnabledMenuTypeMask &= (int)(~menuType);
			}
		}
	}
}
