using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class OptionsControlsMenu : BaseMenuBehaviour
{
	[Serializable]
	public class ControllerTextIndices
	{
		public SwitchControllerType controllerType;

		public int[] indices;
	}

	public struct TextPosition
	{
		public Vector2 min;

		public Vector2 max;

		public TextAnchor allignment;
	}

	public enum DebugTestPlatform
	{
		Desktop,
		Console_PS4,
		Console_XBONE
	}

	[Tooltip("The game object containing the controls settings panel to use when running on console platforms")]
	[Header("Platform Panels")]
	public GameObject m_ConsolePanel;

	[Tooltip("The game object containing the controls setting panel to use when running on desktop platforms")]
	public GameObject m_DesktopPanel;

	public T17TabPanel m_SettingsMenuTabPanel;

	public GameObject m_ControlSettingHelpDialog;

	public T17ScrollView m_DesktopPanelScrollView;

	public GameObject m_ControlsRemapEntryPrefab;

	public T17Image m_ControllerImage;

	public Sprite m_PS4ControllerSprite;

	public Sprite m_XBOneControllerSprite;

	public ControllerTextIndices[] m_SwitchControllerTextIndices;

	protected T17Text[] m_ControllerText;

	protected TextPosition[] m_ControllerTextPositions;

	private List<ControlsRemapEntry> m_CurrentAttachedControlMapEntries = new List<ControlsRemapEntry>();

	private T17DialogBox m_DialogBox;

	private bool m_bDelayDesktopPanelScrollViewPopulation;

	private GameObject m_LastSelectedGameObjectBeforeDialog;

	public DebugTestPlatform m_TestPlatform = DebugTestPlatform.Console_PS4;

	public PCControlTypeOptionSelector m_ControlOptionSelector;

	private PCControlOptionItem m_ControlOptionItem;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		m_ControlOptionItem = new PCControlOptionItem(m_ControlOptionSelector);
	}

	protected override void Update()
	{
		base.Update();
		if (m_bDelayDesktopPanelScrollViewPopulation)
		{
			m_bDelayDesktopPanelScrollViewPopulation = false;
			CreateMappingsList();
			if (m_CurrentAttachedControlMapEntries != null && m_CurrentAttachedControlMapEntries.Count > 0 && base.CachedEventSystem != null)
			{
				base.CachedEventSystem.SetSelectedGameObject(m_CurrentAttachedControlMapEntries[0].gameObject);
				m_DesktopPanelScrollView.ScrollToEntry(m_CurrentAttachedControlMapEntries[0].gameObject, bSelect: true);
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		ShowPCPanel();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		m_CurrentAttachedControlMapEntries.Clear();
		if (m_DesktopPanelScrollView != null)
		{
			m_DesktopPanelScrollView.ClearContents();
		}
		if (m_ControlOptionItem != null)
		{
			m_ControlOptionItem.ResetToInitialValue();
		}
		if (ReInput.userDataStore != null)
		{
			ReInput.userDataStore.Load();
		}
		return true;
	}

	private void ShowPCPanel()
	{
		Rewired.Player rewiredPlayer = base.CachedEventSystem.AssignedGamer.m_RewiredPlayer;
		if (rewiredPlayer != null)
		{
			Rewired.Player.ControllerHelper controllers = rewiredPlayer.controllers;
			if (controllers != null && controllers.hasMouse)
			{
				ShowDesktopPanel();
				return;
			}
		}
		if (m_ControllerImage != null)
		{
			m_ControllerImage.sprite = m_XBOneControllerSprite;
		}
		ShowConsolePanel();
	}

	private void ShowDesktopPanel()
	{
		if (m_ConsolePanel != null)
		{
			m_ConsolePanel.SetActive(value: false);
		}
		if (m_DesktopPanel != null)
		{
			m_DesktopPanel.SetActive(value: true);
		}
		if (m_ControlOptionSelector != null)
		{
			m_ControlOptionItem.Initialise();
		}
		if (ReInput.userDataStore != null)
		{
			ReInput.userDataStore.Save();
		}
		CreateMappingsList();
		if (m_ControlOptionSelector != null && m_ControlOptionSelector.m_FirstFocus != null)
		{
			m_TopSelectable = m_ControlOptionSelector.m_FirstFocus;
		}
		else if (m_CurrentAttachedControlMapEntries != null && m_CurrentAttachedControlMapEntries.Count > 0)
		{
			T17Button component = m_CurrentAttachedControlMapEntries[0].GetComponent<T17Button>();
			if (component != null)
			{
				m_TopSelectable = component;
				m_DesktopPanelScrollView.ScrollToEntry(m_TopSelectable.gameObject, bSelect: true);
			}
		}
	}

	private void ShowConsolePanel()
	{
		if (m_ConsolePanel != null)
		{
			m_ConsolePanel.SetActive(value: true);
		}
		if (m_DesktopPanel != null)
		{
			m_DesktopPanel.SetActive(value: false);
		}
	}

	public void OnApplyClicked()
	{
		InternalApplyClicked();
	}

	private bool InternalApplyClicked()
	{
		if (m_CurrentAttachedControlMapEntries.FindIndex(delegate(ControlsRemapEntry entry)
		{
			InputMapData inputMapData = T17ControlMapper.Instance.GetInputMapData(entry.m_RewiredPlayer, entry.m_InputMapDataID);
			return inputMapData.m_ActionElementMap.keyCode == KeyCode.None;
		}) != -1)
		{
			m_DialogBox = T17DialogBoxManager.GetDialog(forSingleUser: true, base.CurrentGamePlayer, showOverPauseMenu: true);
			m_DialogBox.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.ControlsRemapping.ApplyRemapErrorDialogTitle", "Text.ControlsRemapping.ApplyRemapErrorDialogBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
			m_DialogBox.Show();
			return false;
		}
		if (m_ControlOptionItem != null)
		{
			m_ControlOptionItem.OnApply();
			GlobalSave.GetInstance().RequestSave();
			if (ReInput.userDataStore != null)
			{
				ReInput.userDataStore.Save();
			}
		}
		return true;
	}

	public void OnDefaultsClicked()
	{
		if (m_ControlOptionItem == null)
		{
			return;
		}
		m_ControlOptionItem.ResetToDefault();
		GlobalSave.GetInstance().RequestSave();
		if (base.CachedEventSystem != null && base.CachedEventSystem.AssignedGamer != null)
		{
			T17ControlMapper.Instance.ResetPlayerBindingsToDefault(base.CachedEventSystem.AssignedGamer.m_RewiredPlayer);
			if (m_DesktopPanelScrollView != null)
			{
				CreateMappingsList();
			}
		}
	}

	public void CreateMappingsList()
	{
		if (m_DesktopPanelScrollView == null || base.CachedEventSystem == null)
		{
			return;
		}
		if (!m_DesktopPanelScrollView.HasPerformedFirstTimeInitialise() || m_bDelayDesktopPanelScrollViewPopulation)
		{
			m_bDelayDesktopPanelScrollViewPopulation = true;
			return;
		}
		m_DesktopPanelScrollView.ClearContents();
		m_CurrentAttachedControlMapEntries.Clear();
		Rewired.Player rewiredPlayer = base.CachedEventSystem.AssignedGamer.m_RewiredPlayer;
		T17ControlMapper instance = T17ControlMapper.Instance;
		List<InputMapData> allInputMapDataForPlayer = instance.GetAllInputMapDataForPlayer(rewiredPlayer);
		if (allInputMapDataForPlayer == null)
		{
			return;
		}
		int count = allInputMapDataForPlayer.Count;
		for (int i = 0; i < count; i++)
		{
			InputMapData inputMapData = allInputMapDataForPlayer[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ControlsRemapEntryPrefab);
			m_DesktopPanelScrollView.AddNewObject(gameObject);
			ControlsRemapEntry component = gameObject.GetComponent<ControlsRemapEntry>();
			if (component != null)
			{
				component.SetUpMapEntry(rewiredPlayer, inputMapData.InputMapDataID);
				m_CurrentAttachedControlMapEntries.Add(component);
			}
		}
	}

	public bool IsDirty()
	{
		if (m_ControlOptionItem != null && m_ControlOptionItem.isDirty)
		{
			return true;
		}
		if (m_CurrentAttachedControlMapEntries != null)
		{
			for (int num = m_CurrentAttachedControlMapEntries.Count - 1; num >= 0; num--)
			{
				if (m_CurrentAttachedControlMapEntries[num].IsDirty)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ResetToInitialValues()
	{
		ReInput.userDataStore.Load();
		CreateMappingsList();
	}

	public void OnCancel(FrontendMenuBehaviour menu)
	{
		if (m_ControlSettingHelpDialog.activeInHierarchy)
		{
			m_ControlSettingHelpDialog.SetActive(value: false);
			if (m_SettingsMenuTabPanel != null)
			{
				m_SettingsMenuTabPanel.m_bAllowIndirectNavigation = true;
			}
			if (base.CachedEventSystem != null)
			{
				base.CachedEventSystem.SetSelectedGameObject(m_LastSelectedGameObjectBeforeDialog, forceSet: true);
				m_LastSelectedGameObjectBeforeDialog = null;
			}
			return;
		}
		if (IsDirty())
		{
			T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog2 != null)
			{
				dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.SaveChangesTitle", "Text.Menu.SaveChangesBody", "Text.Yes", "Text.No", string.Empty);
				dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, (T17DialogBox.DialogEvent)delegate
				{
					if (InternalApplyClicked())
					{
						FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
					}
				});
				dialog2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnDecline, (T17DialogBox.DialogEvent)delegate
				{
					ResetToInitialValues();
					FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
				});
				dialog2.Show();
				return;
			}
		}
		FrontEndFlow.Instance.SwitchBackToFrontendMenu(menu);
	}

	public override void ConfirmChangeFocus(ConfirmFocusCallback confirmCallback)
	{
		if (m_ControlOptionItem == null || m_CurrentAttachedControlMapEntries == null)
		{
			confirmCallback(canChangeFocus: true);
		}
		else if (IsDirty())
		{
			T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog2 != null)
			{
				dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.SaveChangesTitle", "Text.Menu.SaveChangesBody", "Text.Yes", "Text.No", string.Empty);
				dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, (T17DialogBox.DialogEvent)delegate
				{
					confirmCallback(InternalApplyClicked());
				});
				dialog2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnDecline, (T17DialogBox.DialogEvent)delegate
				{
					ResetToInitialValues();
					confirmCallback(canChangeFocus: true);
				});
				dialog2.Show();
			}
		}
		else
		{
			confirmCallback(canChangeFocus: true);
		}
	}

	public void ShowControlSettingsHelp()
	{
		if (m_ControlSettingHelpDialog != null)
		{
			if (m_SettingsMenuTabPanel != null)
			{
				m_SettingsMenuTabPanel.m_bAllowIndirectNavigation = false;
			}
			if (base.CachedEventSystem != null)
			{
				m_LastSelectedGameObjectBeforeDialog = base.CachedEventSystem.currentSelectedGameObject;
				base.CachedEventSystem.SetSelectedGameObject(null, forceSet: true);
			}
			m_ControlSettingHelpDialog.SetActive(value: true);
		}
	}
}
