using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class EditorRootMenu : RootMenu
{
	public enum EditorMenuTypeToOpen
	{
		EditorHomepageMenu,
		MyPrisonsMenu,
		BrowseGamesMenu,
		SubscribedMenu,
		PrisonSetupMenu
	}

	private static EditorRootMenu m_Instance;

	[HideInInspector]
	public EditorMenuTypeToOpen m_CurrentFrontEndMenuType;

	[HideInInspector]
	public EditorMenuTypeToOpen m_CurrentPendingFrontEndMenuType;

	public Dictionary<EditorMenuTypeToOpen, MenuList_Container> m_FrontEndTabableMenuTypes;

	public GameObject m_PCBackButton;

	public static EditorRootMenu GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		m_Instance = this;
		m_RootMenuType = RootMenuType.FrontEnd;
		base.Awake();
		m_FrontEndTabableMenuTypes = new Dictionary<EditorMenuTypeToOpen, MenuList_Container>();
		EditorMenuTypeToOpen[] array = (EditorMenuTypeToOpen[])Enum.GetValues(typeof(EditorMenuTypeToOpen));
		for (int i = 0; i < array.Length; i++)
		{
			MenuList_Container menuList_Container = new MenuList_Container();
			if (i < m_EditorTabAbleMenuTypes.Length)
			{
				menuList_Container.m_Menus = m_EditorTabAbleMenuTypes[i].menus;
				for (int j = 0; j < menuList_Container.m_Menus.Count; j++)
				{
					if (menuList_Container.m_Menus[j] != null)
					{
						menuList_Container.m_Menus[j].Hide();
					}
				}
				menuList_Container.m_DefaultTab = m_EditorTabAbleMenuTypes[i].m_DefaultTab;
			}
			else
			{
				menuList_Container.m_Menus = new List<BaseMenuBehaviour>();
			}
			m_FrontEndTabableMenuTypes.Add(array[i], menuList_Container);
		}
		if (m_PCBackButton != null)
		{
			m_PCBackButton.SetActive(value: true);
		}
	}

	protected override void Start()
	{
		base.Start();
		Hide();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_Instance = null;
	}

	protected override void Update()
	{
		base.Update();
		if (m_PCBackButton != null && base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer != null && base.CurrentGamer.m_RewiredPlayer.controllers != null && base.CurrentGamer.m_RewiredPlayer.controllers.GetLastActiveController() != null)
		{
			if (base.CurrentGamer.m_RewiredPlayer.controllers.GetLastActiveController().type == ControllerType.Mouse)
			{
				m_PCBackButton.SetActive(value: true);
			}
			else if (m_PCBackButton.activeInHierarchy)
			{
				m_PCBackButton.SetActive(value: false);
			}
		}
	}

	public override BaseMenuBehaviour GetMenuOFType<T>(RootMenuType typeofMenus)
	{
		for (int i = 0; i < m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus.Count; i++)
		{
			if (m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[i].GetType() == typeof(T))
			{
				return m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[i];
			}
		}
		return null;
	}

	public override int GetTabNumberOfType<T>()
	{
		for (int i = 0; i < m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus.Count; i++)
		{
			if (m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[i].GetType() == typeof(T))
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
		return m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[index];
	}

	public override List<BaseMenuBehaviour> GetCurrentMenuSet()
	{
		return m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus;
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
			int num = 0;
			m_MainTabPanel.SetMenuBodies(m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus);
			num = m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_DefaultTab;
			m_MainTabPanel.SetTabIndex(num);
		}
		else
		{
			if (currentGamer.m_RewiredPlayer != null && T17EventSystem.GetStateForRewiredPlayer(currentGamer.m_RewiredPlayer) != T17EventSystem.InputCateogryStates.Frontend)
			{
				T17EventSystem.ApplyCategories(currentGamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Frontend);
			}
			m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_DefaultTab].Show(currentGamer, this, null);
		}
		return true;
	}

	public void SetFrontEndMenuTypeToOpen(EditorMenuTypeToOpen type)
	{
		m_CurrentFrontEndMenuType = type;
	}

	public void SetFrontEndMenuTypeToOpen(BaseMenuBehaviour menu)
	{
		foreach (KeyValuePair<EditorMenuTypeToOpen, MenuList_Container> frontEndTabableMenuType in m_FrontEndTabableMenuTypes)
		{
			BaseMenuBehaviour baseMenuBehaviour = frontEndTabableMenuType.Value.m_Menus[frontEndTabableMenuType.Value.m_DefaultTab];
			if (baseMenuBehaviour == menu)
			{
				SetFrontEndMenuTypeToOpen(frontEndTabableMenuType.Key);
				break;
			}
		}
	}

	public bool OpenFrontendChildOfCurrent(int index)
	{
		if (m_RootMenuType == RootMenuType.FrontEnd && index > 0 && index < m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus.Count && m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[index].Show(base.CurrentGamer, m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[0], null))
		{
			RaiseMenuChangedEvent();
		}
		return false;
	}

	public bool IsChildMenuOpen()
	{
		if (m_RootMenuType == RootMenuType.FrontEnd)
		{
			for (int num = m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus.Count - 1; num >= 1; num--)
			{
				if (m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[num].gameObject.activeInHierarchy)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ReturnToNormalFrontend()
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.StartFrontEndFromLevelEditor();
		}
	}

	public BaseMenuBehaviour ReturnChildMenuOpen()
	{
		if (m_RootMenuType == RootMenuType.FrontEnd)
		{
			for (int num = m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus.Count - 1; num >= 1; num--)
			{
				if (m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[num] != null && m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[num].gameObject.activeInHierarchy)
				{
					return m_FrontEndTabableMenuTypes[m_CurrentFrontEndMenuType].m_Menus[num];
				}
			}
		}
		return null;
	}

	public void DoNavigateOnUICancel()
	{
		FrontendMenuBehaviour frontendMenuBehaviour = (FrontendMenuBehaviour)ReturnChildMenuOpen();
		if (frontendMenuBehaviour == null)
		{
			frontendMenuBehaviour = (FrontendMenuBehaviour)GetCurrentOpenMenu();
		}
		if (frontendMenuBehaviour != null)
		{
			frontendMenuBehaviour.InvokeNavigateOnUICancel();
		}
	}
}
