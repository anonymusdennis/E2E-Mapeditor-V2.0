using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class FrontendRootMenu : RootMenu
{
	public enum FrontendMenuTypeToOpen
	{
		MainMenu,
		Campaign,
		Versus,
		BrowseGamesMenu,
		PrisonSetupMenu,
		Customization,
		Collectables,
		Leaderboards,
		Settings,
		Credits,
		Shop,
		Editor
	}

	[HideInInspector]
	public FrontendMenuTypeToOpen m_CurrentFrontEndMenuType;

	[HideInInspector]
	public FrontendMenuTypeToOpen m_CurrentPendingFrontEndMenuType;

	public Dictionary<FrontendMenuTypeToOpen, MenuList_Container> m_FrontEndTabableMenuTypes;

	public bool m_ClearSaveSlotOnAwake;

	public GameObject m_PCBackButton;

	protected override void Awake()
	{
		m_RootMenuType = RootMenuType.FrontEnd;
		base.Awake();
		m_FrontEndTabableMenuTypes = new Dictionary<FrontendMenuTypeToOpen, MenuList_Container>();
		FrontendMenuTypeToOpen[] array = (FrontendMenuTypeToOpen[])Enum.GetValues(typeof(FrontendMenuTypeToOpen));
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
			m_FrontEndTabableMenuTypes.Add(array[i], menuList_Container);
		}
		if (m_ClearSaveSlotOnAwake && SaveManager.GetInstance() != null)
		{
			SaveManager.GetInstance().ClearSelectedSlot(null);
			SaveManager.GetInstance().ResetUIMode(setEverythingChanged: true);
		}
		if (m_PCBackButton != null)
		{
			m_PCBackButton.SetActive(value: true);
		}
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

	public void SetFrontEndMenuTypeToOpen(FrontendMenuTypeToOpen type)
	{
		if (type == FrontendMenuTypeToOpen.Leaderboards || type == FrontendMenuTypeToOpen.BrowseGamesMenu || type == FrontendMenuTypeToOpen.Versus)
		{
			m_CurrentFrontEndMenuType = type;
			return;
		}
		m_CurrentFrontEndMenuType = type;
		Platform.GetInstance().ExitOnlineArea();
	}

	public void SetFrontEndMenuTypeToOpen(BaseMenuBehaviour menu)
	{
		foreach (KeyValuePair<FrontendMenuTypeToOpen, MenuList_Container> frontEndTabableMenuType in m_FrontEndTabableMenuTypes)
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

	public void PromptGameExit()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: true);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.UI.Quit", "Text.Menu.OkToExit", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(DoExit));
			dialog.Show();
		}
	}

	private void DoExit(T17DialogBox box)
	{
		Application.Quit();
	}
}
