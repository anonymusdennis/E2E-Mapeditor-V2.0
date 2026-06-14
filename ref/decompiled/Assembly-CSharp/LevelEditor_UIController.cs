using System;
using System.Collections;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor_UIController : MonoBehaviour
{
	public enum BuildingBlockCategory
	{
		Room,
		Inside,
		Outside,
		Object,
		Max
	}

	public enum ButtonTypes
	{
		Copy,
		Delete,
		Move,
		TOTAL
	}

	private static LevelEditor_UIController m_Instance;

	public LevelEditor_ToolTip m_ToolTip;

	public LevelEditor_ToolTip m_BrushErrorToolTip;

	private int m_CurrentTooltipHandle = -1;

	private int m_CurrentBrushErrorHandle = -1;

	public EditorControlsAnimator[] m_MovableControlTabs = new EditorControlsAnimator[0];

	public GameObject[] m_PalletBackerTabs = new GameObject[0];

	private T17ScrollView[] m_PalletBackerScrollViews;

	private T17GridLayoutGroup[] m_PalletBackerGridLayoutGroups;

	private LevelEditor_GridCellPopulator[][] m_PalletBackerGridCellPopulators;

	public T17Button m_UndoButton;

	public T17Button m_RedoButton;

	public LevelEditor_PrisonSettingsDialog m_SettingsMenu;

	public T17TabPanel m_LayerTabGroup;

	public Transform m_SaveDialog;

	public T17TabPanel m_PaletteTabGroup;

	public Transform m_HelpDialog;

	public Transform m_SelectedBlockMenu;

	public Transform m_StopZoneEditMenu;

	public LevelEditor_PrisonCheckerDialog m_PrisonCheckerDialog;

	public EditorPublishMenu m_PrisonPublishDialog;

	private bool m_bInitialSetupComplete;

	private GraphicRaycaster m_RayCaster;

	private LevelEditor_Controller m_CachedController;

	private BuildingBlockManager m_BlockMan;

	private Rewired.Player m_Player;

	private int m_iPendingLimitationToSelect = -1;

	private int m_iLimitaitonSelectTimer;

	private bool m_bPushedSelectedMenu;

	public Camera m_MainCamera;

	private Vector3 m_SelectedBlockWorldPosition = Vector3.zero;

	public T17Text m_FilterThemeNameText;

	public LevelEditor_FilterButton m_FilterMenuButton;

	private BaseLevelManager.LevelLayers m_CurrentLayer = BaseLevelManager.LevelLayers.FirstFloor;

	private BuildingBlockCategory m_CurrentCategory;

	private float[] m_TabScrollPositions = new float[4];

	public GameObject m_CopyButton;

	private RectTransform m_CopyButtonTrans;

	public GameObject m_MoveButton;

	private RectTransform m_MoveButtonTrans;

	public GameObject m_DeleteButton;

	private RectTransform m_DeleteButtonTrans;

	public Transform m_WelcomeDialog;

	public T17Toggle m_DontShowAgain;

	private bool m_bInitialisedWorldItems;

	public T17Text m_WorldPrisonTitle;

	public T17Text m_WorldPrisonEditDate;

	public T17Text m_WorldPrisonDescription;

	private BaseLevelManager.BrushError m_CurrentBrushError;

	private float m_ErrorExpireTime = 4.2949673E+09f;

	private bool m_ClearErrorOnExpiry;

	private bool m_bShowingErrorToolTip;

	private float m_ErrorSetTime;

	public float m_BrushErrorDelay = 0.5f;

	private bool m_bMouseOverControls;

	public LevelEditor_CheckList m_Checklist;

	private bool m_bWaitingToShowInviteDialog;

	private bool m_bInviteServiced;

	private float m_TimeInEditorLastRecordedTimestamp;

	private const string MINUTES_IN_EDITOR_KEY = "Analytics_MinutesInEditor";

	private const string MINUTES_IN_EDITOR_ACTION_CATEGORY = "Editor scene playtime";

	private const string MINUTES_IN_EDITOR_ACTION_PREFIX = "Editor playtime ";

	public static LevelEditor_UIController GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		ShowSettingsMenu(bYesNo: false);
		m_PalletBackerScrollViews = new T17ScrollView[m_PalletBackerTabs.Length];
		m_PalletBackerGridLayoutGroups = new T17GridLayoutGroup[m_PalletBackerTabs.Length];
		m_PalletBackerGridCellPopulators = new LevelEditor_GridCellPopulator[m_PalletBackerTabs.Length][];
		int i = 0;
		for (int num = m_PalletBackerTabs.Length; i < num; i++)
		{
			m_PalletBackerGridCellPopulators[i] = m_PalletBackerTabs[i].GetComponentsInChildren<LevelEditor_GridCellPopulator>(includeInactive: true);
			m_PalletBackerScrollViews[i] = m_PalletBackerTabs[i].GetComponentInChildren<T17ScrollView>(includeInactive: true);
			if (m_PalletBackerScrollViews[i] != null && m_PalletBackerScrollViews[i].m_ContentParent != null)
			{
				m_PalletBackerGridLayoutGroups[i] = m_PalletBackerScrollViews[i].m_ContentParent.GetComponent<T17GridLayoutGroup>();
			}
		}
		m_TimeInEditorLastRecordedTimestamp = Time.unscaledTime;
		StartCoroutine(CountTimeInEditorRoutine());
	}

	private void Start()
	{
		if (m_MovableControlTabs == null || m_MovableControlTabs.Length == 0)
		{
			m_MovableControlTabs = GetComponentsInChildren<EditorControlsAnimator>(includeInactive: true);
		}
		if (m_CachedController == null)
		{
			m_CachedController = LevelEditor_Controller.GetInstance();
		}
		if (m_CachedController == null)
		{
			base.enabled = false;
		}
		else
		{
			m_CachedController.RegisterEditModeChange(OnEditModeChanged);
		}
		if (m_LayerTabGroup == null)
		{
			LevelEditorUISetup component = GetComponent<LevelEditorUISetup>();
			if (component != null)
			{
				m_LayerTabGroup = component.m_FirstLayerTab;
			}
		}
		if (m_Player == null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				m_Player = primaryGamer.m_RewiredPlayer;
			}
		}
		if (m_WelcomeDialog != null && GlobalSave.GetInstance() != null)
		{
			bool value = true;
			GlobalSave.GetInstance().Get("LevelEditor:ShowWelcomeDialog", out value, def: true);
			m_WelcomeDialog.gameObject.SetActive(value);
			if (value && m_Player != null)
			{
				T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
				if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
				{
					T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.Dialogbox);
				}
			}
		}
		if (m_CopyButton != null)
		{
			m_CopyButtonTrans = m_CopyButton.GetComponent<RectTransform>();
		}
		if (m_MoveButton != null)
		{
			m_MoveButtonTrans = m_MoveButton.GetComponent<RectTransform>();
		}
		if (m_DeleteButton != null)
		{
			m_DeleteButtonTrans = m_DeleteButton.GetComponent<RectTransform>();
		}
		LevelEditor_Cursor.GetInstance().RegisterForOverControllerChange(OnOverControlChange);
		int i = 0;
		for (int num = m_PalletBackerTabs.Length; i < num; i++)
		{
			T17ScrollView t17ScrollView = m_PalletBackerScrollViews[i];
			if (t17ScrollView != null)
			{
				if (m_PalletBackerGridLayoutGroups[i] != null)
				{
					t17ScrollView.DoSingleTimeInitialize();
				}
				LevelEditor_GridCellPopulator[] array = m_PalletBackerGridCellPopulators[i];
				int j = 0;
				for (int num2 = array.Length; j < num2; j++)
				{
					LevelEditor_GridCellPopulator obj = array[j];
					obj.OnCellsUpdated = (LevelEditor_GridCellPopulator.LevelEditor_GridCellPopulatorDelegate)Delegate.Combine(obj.OnCellsUpdated, new LevelEditor_GridCellPopulator.LevelEditor_GridCellPopulatorDelegate(OnCellContentUpdated));
				}
			}
		}
	}

	private void Update()
	{
		if (m_bWaitingToShowInviteDialog)
		{
			InviteRecieved();
		}
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.LevelLayers currentLayer = instance.GetCurrentLayer();
			if (currentLayer != m_CurrentLayer)
			{
				ExternalChangeLayer(currentLayer);
				UpdateLayerChanges();
			}
		}
		if (!m_bInitialisedWorldItems)
		{
			LevelDetailsManager instance2 = LevelDetailsManager.GetInstance();
			if (instance2 != null && !instance2.IsDetailsManagerBusy())
			{
				m_bInitialisedWorldItems = true;
				UpdateWorldTextItems();
			}
		}
		if (m_Player == null)
		{
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				m_Player = primaryGamer.m_RewiredPlayer;
			}
		}
		BuildingBlockManager instance3 = BuildingBlockManager.GetInstance();
		if (!m_bInitialSetupComplete && instance3 != null)
		{
			m_BlockMan = instance3;
			m_bInitialSetupComplete = true;
			ConstructBlockPanel();
			SetBuildingBlockCategory(BuildingBlockCategory.Room, -1L);
		}
		if (m_BlockMan != null)
		{
			m_BlockMan.CheckForLimitationUpdate();
		}
		UpdateButtons();
		if (m_iPendingLimitationToSelect != -1)
		{
			m_iLimitaitonSelectTimer--;
			if (m_iLimitaitonSelectTimer <= 0)
			{
				ScrollAndSelectCell(m_iPendingLimitationToSelect);
			}
		}
		if (m_Player != null && m_Player.GetButtonUp("UI_Close"))
		{
			if (m_SaveDialog != null && m_SaveDialog.gameObject.GetActive())
			{
				CloseDialog(m_SaveDialog);
			}
			else if (m_HelpDialog != null && m_HelpDialog.gameObject.GetActive())
			{
				CloseDialog(m_HelpDialog);
			}
			else if (m_PrisonCheckerDialog != null && m_PrisonCheckerDialog.gameObject.GetActive())
			{
				m_PrisonCheckerDialog.HidePrisonCheckerDialog();
			}
			else if (m_PrisonPublishDialog != null && m_PrisonPublishDialog.gameObject.GetActive() && !m_PrisonPublishDialog.IsBusy())
			{
				m_PrisonPublishDialog.HidePublishDialog();
			}
		}
		if (!m_bShowingErrorToolTip && m_CurrentBrushError != 0 && Time.realtimeSinceStartup >= m_ErrorSetTime + m_BrushErrorDelay && GlobalStart.GetInstance() != null && GlobalStart.GetInstance().m_EditorSettings != null && m_BrushErrorToolTip != null)
		{
			HideBrushErrorToolTip();
			m_bShowingErrorToolTip = true;
			LevelEditor_ToolTip.ToolTipData data = new LevelEditor_ToolTip.ToolTipData();
			data.m_strTitle = "Error";
			int num = GlobalStart.GetInstance().m_EditorSettings.m_BrushErrors.FindIndex((BrushError a) => a.m_BrushError == m_CurrentBrushError);
			data.m_strDesc = GlobalStart.GetInstance().m_EditorSettings.m_BrushErrors[num].Description;
			data.m_ExpireIn = m_ErrorExpireTime;
			m_CurrentBrushErrorHandle = m_BrushErrorToolTip.DisplayToolTip(ref data);
			if (m_ClearErrorOnExpiry)
			{
				m_CurrentBrushError = BaseLevelManager.BrushError.eNone;
			}
		}
	}

	private void UpdateZoneTick()
	{
		if (!(m_StopZoneEditMenu != null) || !(m_CachedController != null))
		{
			return;
		}
		LevelEditor_ZoneManager.Zone currentZone = m_CachedController.CurrentZone;
		if (currentZone == null)
		{
			m_StopZoneEditMenu.gameObject.SetActive(value: false);
			return;
		}
		LevelEditor_Controller.EditMode editMode = m_CachedController.GetEditMode();
		if (editMode == LevelEditor_Controller.EditMode.Zone_Editing)
		{
			Vector3 position = new Vector3(currentZone.m_IconPosition.x - 60f + 0.5f, currentZone.m_IconPosition.y - 60f + 1f, -10f);
			m_StopZoneEditMenu.position = m_MainCamera.WorldToScreenPoint(position);
			m_StopZoneEditMenu.gameObject.SetActive(value: true);
		}
		else
		{
			m_StopZoneEditMenu.gameObject.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (m_SelectedBlockMenu != null && m_MainCamera != null && m_SelectedBlockMenu.gameObject.GetActive())
		{
			m_SelectedBlockMenu.position = m_MainCamera.WorldToScreenPoint(m_SelectedBlockWorldPosition);
		}
		UpdateZoneTick();
	}

	private IEnumerator CountTimeInEditorRoutine()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(180f);
			EC2AnalyticsHelper.UpdatePlaytimeRecord(3, "Analytics_MinutesInEditor", "Editor scene playtime", "Editor playtime ");
			m_TimeInEditorLastRecordedTimestamp = Time.unscaledTime;
		}
	}

	protected virtual void OnDestroy()
	{
		m_Instance = null;
		if (Time.unscaledTime > m_TimeInEditorLastRecordedTimestamp)
		{
			EC2AnalyticsHelper.UpdatePlaytimeRecord((int)((Time.unscaledTime - m_TimeInEditorLastRecordedTimestamp) / 60f), "Analytics_MinutesInEditor", "Editor scene playtime", "Editor playtime ");
		}
	}

	public void OnCategoryTabButtonClicked(int tabIndex)
	{
		if (tabIndex == (int)m_CurrentCategory)
		{
			return;
		}
		int num = m_PalletBackerTabs.Length;
		if (tabIndex < num)
		{
			for (int i = 0; i < num; i++)
			{
				if (i == tabIndex)
				{
					m_PalletBackerTabs[i].SetActive(value: true);
					T17ScrollView t17ScrollView = m_PalletBackerScrollViews[i];
					if (t17ScrollView != null && m_PalletBackerGridLayoutGroups[i] != null)
					{
						t17ScrollView.Show(Gamer.GetPrimaryGamer(), null, null);
					}
				}
				else if (m_PalletBackerTabs[i].activeSelf)
				{
					T17ScrollView t17ScrollView2 = m_PalletBackerScrollViews[i];
					if (t17ScrollView2 != null && m_PalletBackerGridLayoutGroups[i] != null)
					{
						t17ScrollView2.Hide(restoreInvokerState: false, isTabSwitch: true);
					}
					m_PalletBackerTabs[i].SetActive(value: false);
				}
			}
		}
		SetBuildingBlockCategory((BuildingBlockCategory)tabIndex, -1L);
	}

	public void OnLayerButtonClicked(int buttonIndex)
	{
		ChangeLayer((BaseLevelManager.LevelLayers)buttonIndex);
	}

	public void OnUndoButtonClicked()
	{
		m_CachedController.UndoLast();
	}

	public void OnRedoButtonClicked()
	{
		m_CachedController.RedoLast();
	}

	public void OnShowSaveDialogClicked()
	{
		ShowDialog(m_SaveDialog);
	}

	public void OnHideSaveDialogClicked()
	{
		if (m_SaveDialog != null && m_SaveDialog.gameObject.GetActive())
		{
			CloseDialog(m_SaveDialog);
		}
	}

	public void OnSaveAsButtonClicked()
	{
		if (m_CachedController != null)
		{
			m_CachedController.SaveTheLevel(bForceNew: true);
		}
		CloseDialog(m_SaveDialog);
	}

	public void OnSaveButtonClicked()
	{
		if (m_CachedController != null)
		{
			m_CachedController.SaveTheLevel(bForceNew: false);
		}
		CloseDialog(m_SaveDialog);
	}

	public void OnReloadButtonClicked()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Editor.Reset", "Text.Edit.OkToReset", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(ResetLevel));
			dialog.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnCancel, new T17DialogBox.DialogEvent(ResetSaveDialog));
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, new T17DialogBox.DialogEvent(ResetSaveDialog));
			dialog.Show();
		}
		else
		{
			ResetLevel(dialog);
		}
	}

	public void OnLoadButtonClicked()
	{
		if (m_PrisonCheckerDialog != null)
		{
			if (m_CachedController != null)
			{
				m_CachedController.ExternalSelectBlock(-1);
			}
			m_PrisonCheckerDialog.ShowPrisonCheckerDialog();
		}
	}

	public void OnOptionsButtonClicked()
	{
		ShowSettingsMenu(bYesNo: true);
	}

	public void OnGameSettingsButtonClicked()
	{
		ShowSettingsMenu(bYesNo: true, 1);
	}

	public void OnRoutineSettingsButtonClicked()
	{
		ShowSettingsMenu(bYesNo: true, 2);
	}

	public void CloseSettingsMenu()
	{
		ShowSettingsMenu(bYesNo: false);
	}

	public void OnHelpButtonClicked()
	{
		if (m_HelpDialog != null)
		{
			ShowDialog(m_HelpDialog);
			T17TabPanel componentInChildren = m_HelpDialog.GetComponentInChildren<T17TabPanel>();
			if (componentInChildren != null)
			{
				componentInChildren.Show(Gamer.GetPrimaryGamer(), null, null);
				componentInChildren.SetTabIndex(0);
			}
		}
	}

	public void CloseHelpMenu()
	{
		CloseDialog(m_HelpDialog);
	}

	public void OnExitButtonClicked()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			if (LevelEditor_Cursor.GetInstance() != null)
			{
				LevelEditor_Cursor.GetInstance().EnteringControlArea();
			}
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.Quit", "Text.Menu.QuitEditor", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(DoExitEditor));
			dialog.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnCancel, new T17DialogBox.DialogEvent(DontQuit));
			dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, new T17DialogBox.DialogEvent(DontQuit));
			dialog.Show();
		}
		else
		{
			DoExitEditor(dialog);
		}
	}

	public void DontQuit(T17DialogBox dialog)
	{
		if (LevelEditor_Cursor.GetInstance() != null)
		{
			LevelEditor_Cursor.GetInstance().LeavingControlArea();
		}
	}

	public void SetBuildingBlockCategory(BuildingBlockCategory category, long iFamily = -1L, bool bResetScrollbar = true)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		BaseLevelManager.LevelLayers currentLayer = instance.GetCurrentLayer();
		m_CurrentCategory = category;
		m_CurrentLayer = currentLayer;
	}

	private void ConstructBlockPanel()
	{
		for (int i = 0; i < 4; i++)
		{
			m_TabScrollPositions[i] = 1f;
		}
	}

	public void DisplayToolTip(int iBlockID, Vector2 position)
	{
		if (!(m_ToolTip != null))
		{
			return;
		}
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
		if (!(block != null))
		{
			return;
		}
		LevelEditor_ToolTip.ToolTipData data = new LevelEditor_ToolTip.ToolTipData();
		data.m_strTitle = block.m_BlockNameID;
		data.m_strDesc = block.m_BlockDescriptionID;
		if (block.m_LimitationGroup != -1)
		{
			BuildingBlockManager instance = BuildingBlockManager.GetInstance();
			if (instance != null)
			{
				BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(block.m_LimitationGroup);
				if (limitationGroup != null && limitationGroup.m_Max != 0)
				{
					float num = (float)limitationGroup.m_CurrentTotal / (float)limitationGroup.m_Max;
					if (num > (float)limitationGroup.m_PercentToDisplayAt / 100f)
					{
						data.m_strUsage = limitationGroup.m_CurrentTotal + " / " + limitationGroup.m_Max;
						if (limitationGroup.m_CurrentTotal >= limitationGroup.m_Max)
						{
							data.m_UsageColor = new Color(1f, 0.8f, 0.8f);
						}
					}
				}
			}
		}
		m_CurrentTooltipHandle = m_ToolTip.DisplayToolTip(ref data);
	}

	public void DisplayOutOfBlocksToolTip(int iBlockID)
	{
		if (m_ToolTip != null)
		{
			HideToolTip();
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
			if (block != null)
			{
				LevelEditor_ToolTip.ToolTipData data = new LevelEditor_ToolTip.ToolTipData();
				data.m_strTitle = block.m_BlockNameID;
				data.m_strDesc = "Text.Editor.NoMoreBlocks";
				data.m_ExpireIn = 3f;
				m_CurrentTooltipHandle = m_ToolTip.DisplayToolTip(ref data);
			}
		}
	}

	public void DisplayToolTip(string strTitleID, string strMessageID)
	{
		if (m_ToolTip != null)
		{
			HideToolTip();
			LevelEditor_ToolTip.ToolTipData data = new LevelEditor_ToolTip.ToolTipData();
			data.m_strTitle = strTitleID;
			data.m_strDesc = strMessageID;
			data.m_ExpireIn = 3f;
			m_CurrentTooltipHandle = m_ToolTip.DisplayToolTip(ref data);
		}
	}

	public void HideToolTip()
	{
		if (m_ToolTip != null)
		{
			m_ToolTip.HideToolTip(m_CurrentTooltipHandle);
		}
	}

	public void HideBrushErrorToolTip()
	{
		if (m_BrushErrorToolTip != null)
		{
			m_BrushErrorToolTip.HideToolTip(m_CurrentBrushErrorHandle);
			m_bShowingErrorToolTip = false;
		}
	}

	public void ExternalChangeLayer(BaseLevelManager.LevelLayers layer)
	{
		if (m_LayerTabGroup != null)
		{
			switch (layer)
			{
			case BaseLevelManager.LevelLayers.GroundFloor:
				m_LayerTabGroup.SetTabIndex(4, null, bPlayAudio: false);
				break;
			case BaseLevelManager.LevelLayers.GroundFloor_Vent:
				m_LayerTabGroup.SetTabIndex(3, null, bPlayAudio: false);
				break;
			case BaseLevelManager.LevelLayers.FirstFloor:
				m_LayerTabGroup.SetTabIndex(2, null, bPlayAudio: false);
				break;
			case BaseLevelManager.LevelLayers.FirstFloor_Vent:
				m_LayerTabGroup.SetTabIndex(1, null, bPlayAudio: false);
				break;
			case BaseLevelManager.LevelLayers.Roof:
				m_LayerTabGroup.SetTabIndex(0, null, bPlayAudio: false);
				break;
			}
		}
	}

	public void ExternalChangePaleteTab(BuildingBlockCategory category)
	{
		if (m_CurrentCategory != category)
		{
			if (m_PaletteTabGroup != null)
			{
				m_PaletteTabGroup.SetTabIndex((int)category, null, bPlayAudio: false);
			}
			SetBuildingBlockCategory(category, -1L, bResetScrollbar: false);
		}
	}

	public void ChangeLayer(BaseLevelManager.LevelLayers layer)
	{
		HideBrushErrorToolTip();
		m_CurrentBrushError = BaseLevelManager.BrushError.eNone;
		if (m_CachedController != null)
		{
			m_CachedController.ChangeLayer(layer);
		}
	}

	public void UpdateLayerChanges()
	{
		BaseLevelManager.LevelLayers levelLayers = BaseLevelManager.LevelLayers.GroundFloor;
		while ((int)levelLayers < 6)
		{
			LevelEditorHighLightManager.GetInstance().m_MasterLayers[(uint)levelLayers].SetActive((int)levelLayers <= (int)BaseLevelManager.GetInstance().m_CurrentLayer);
			levelLayers++;
		}
		SetBuildingBlockCategory(m_CurrentCategory, -1L);
	}

	public void DoExitEditor(T17DialogBox dialog)
	{
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null)
		{
			instance.EndEditorLevel();
		}
	}

	public void ToggleAllMovablePanels()
	{
		if (m_MovableControlTabs == null || m_MovableControlTabs.Length == 0)
		{
			return;
		}
		EditorControlsAnimator.ControlAnimState controlAnimState = EditorControlsAnimator.ControlAnimState.InView;
		for (int num = m_MovableControlTabs.Length - 1; num >= 0; num--)
		{
			if (m_MovableControlTabs[num] != null && m_MovableControlTabs[num].GetManualState() == EditorControlsAnimator.ControlAnimState.InView && m_MovableControlTabs[num].gameObject.activeInHierarchy)
			{
				controlAnimState = EditorControlsAnimator.ControlAnimState.Out;
				break;
			}
		}
		for (int num2 = m_MovableControlTabs.Length - 1; num2 >= 0; num2--)
		{
			if (m_MovableControlTabs[num2] != null)
			{
				m_MovableControlTabs[num2].SetManualState(controlAnimState);
			}
		}
		if (controlAnimState == EditorControlsAnimator.ControlAnimState.InView)
		{
			m_CachedController.PlayAudio(LevelEditor_Controller.AudioTypes.TabIn);
		}
		else
		{
			m_CachedController.PlayAudio(LevelEditor_Controller.AudioTypes.TabOut);
		}
	}

	public void OnEditModeChanged(LevelEditor_Controller.EditMode newMode)
	{
		if (m_RayCaster == null)
		{
			m_RayCaster = GetComponent<GraphicRaycaster>();
		}
		if (m_RayCaster != null)
		{
			switch (newMode)
			{
			case LevelEditor_Controller.EditMode.INVALID:
			case LevelEditor_Controller.EditMode.NoBrush:
			case LevelEditor_Controller.EditMode.BlockSelected:
			case LevelEditor_Controller.EditMode.SelectingObjectInLevel:
			case LevelEditor_Controller.EditMode.SelectedObjectInLevel:
			case LevelEditor_Controller.EditMode.CopySelectedObjectInLevel:
			case LevelEditor_Controller.EditMode.Zone_WaitingToCreate:
			case LevelEditor_Controller.EditMode.Zone_Editing:
				m_RayCaster.enabled = true;
				break;
			default:
				m_RayCaster.enabled = false;
				break;
			}
		}
	}

	private void UpdateButtons()
	{
		BuildingInstructionManager instance = BuildingInstructionManager.GetInstance();
		if (m_BlockMan != null && instance != null)
		{
			if (m_UndoButton != null)
			{
				m_UndoButton.interactable = instance.CanUndo();
			}
			if (m_RedoButton != null)
			{
				m_UndoButton.interactable = instance.CanRedo();
			}
		}
	}

	public void ShowSettingsMenu(bool bYesNo, int iTabIndex = 0)
	{
		if (!(m_SettingsMenu == null))
		{
			UpdateWorldTextItems();
			if (bYesNo)
			{
				m_SettingsMenu.gameObject.SetActive(value: true);
				m_SettingsMenu.Show(iTabIndex);
			}
			else
			{
				m_SettingsMenu.Hide();
			}
		}
	}

	public void SelectBlockFromLimitationsWindow(int iLimitationID)
	{
		if (iLimitationID == -1)
		{
			return;
		}
		if (m_SettingsMenu != null && m_SettingsMenu.gameObject.GetActive())
		{
			m_SettingsMenu.Hide();
		}
		BuildingBlockCategory buildingBlockCategory = BuildingBlockCategory.Room;
		int totalBlocks = BuildingBlockManager.GetInstance().GetTotalBlocks();
		for (int i = 0; i < totalBlocks; i++)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(i);
			if (block != null && block.m_LimitationGroup == iLimitationID && block.BlockType != BaseBuildingBlock.BuildingBlockType.Room)
			{
				buildingBlockCategory = BuildingBlockCategory.Object;
			}
		}
		if (m_CurrentCategory != buildingBlockCategory)
		{
			if (m_PaletteTabGroup != null)
			{
				m_PaletteTabGroup.SetTabIndex((int)buildingBlockCategory, null, bPlayAudio: false);
			}
			SetBuildingBlockCategory(buildingBlockCategory, -1L, bResetScrollbar: false);
			m_iPendingLimitationToSelect = iLimitationID;
			m_iLimitaitonSelectTimer = 3;
		}
		else
		{
			ScrollAndSelectCell(iLimitationID);
		}
	}

	private void ScrollAndSelectCell(int iLimitationID)
	{
		m_iPendingLimitationToSelect = -1;
		if (m_CurrentCategory == BuildingBlockCategory.Max)
		{
			return;
		}
		LevelEditor_GridCellPopulator[] array = m_PalletBackerGridCellPopulators[(int)m_CurrentCategory];
		T17ScrollView t17ScrollView = m_PalletBackerScrollViews[(int)m_CurrentCategory];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			BuildingBlock_UIButton buildingBlock_UIButton = array[i].m_CellUIButtons.Find((BuildingBlock_UIButton x) => x.GetLimitationGroup() == iLimitationID);
			if (buildingBlock_UIButton != null)
			{
				if (t17ScrollView != null && t17ScrollView.gameObject.activeInHierarchy)
				{
					t17ScrollView.InstantlyScrollToItem(buildingBlock_UIButton.gameObject, bSelect: true);
				}
				buildingBlock_UIButton.OnSelected();
				break;
			}
		}
	}

	public void OnSomethingChanged()
	{
		SetBuildingBlockCategory(m_CurrentCategory, -1L);
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null && m_LayerTabGroup != null)
		{
			int index = (int)(5 - instance.m_CurrentLayer);
			m_LayerTabGroup.SetTabIndex(index, null, bPlayAudio: false);
		}
		m_Checklist.OnLimitationChange(0);
		m_CurrentBrushError = BaseLevelManager.BrushError.eNone;
		HideBrushErrorToolTip();
	}

	public void ShowDialog(Transform dialogObject)
	{
		if (!(dialogObject != null))
		{
			return;
		}
		dialogObject.gameObject.SetActive(value: true);
		if (m_Player != null)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
	}

	public void CloseDialog(Transform dialogObject)
	{
		if (dialogObject != null)
		{
			dialogObject.gameObject.SetActive(value: false);
		}
		if (m_Player != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(m_Player);
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.LevelEditor)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
	}

	public void ResetLevel(T17DialogBox dialogBox)
	{
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			instance.ResetAndCreateNewLevel(null, instance.GetBlockType());
		}
		CloseDialog(m_SaveDialog);
	}

	public void ResetSaveDialog(T17DialogBox dialogBox)
	{
		if (m_Player != null)
		{
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.Dialogbox)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.Dialogbox);
			}
		}
	}

	public void ShowSelectedBlockMenu(Vector3 vBlockWorldPosition, bool bShowMove, bool bShowDelete, bool bShowCopy)
	{
		m_SelectedBlockWorldPosition = vBlockWorldPosition;
		if (m_CopyButton != null)
		{
			m_CopyButton.SetActive(bShowCopy);
		}
		if (m_MoveButton != null)
		{
			m_MoveButton.SetActive(bShowMove);
		}
		if (m_DeleteButton != null)
		{
			m_DeleteButton.SetActive(bShowDelete);
		}
		if (m_SelectedBlockMenu != null)
		{
			m_SelectedBlockMenu.gameObject.SetActive(value: true);
		}
	}

	public void HideSelectedBlockMenu()
	{
		if (m_SelectedBlockMenu != null)
		{
			m_SelectedBlockMenu.gameObject.SetActive(value: false);
		}
	}

	public void PushSelectedBlockMenu()
	{
		if (m_SelectedBlockMenu != null && m_SelectedBlockMenu.gameObject.activeSelf)
		{
			m_bPushedSelectedMenu = true;
			m_SelectedBlockMenu.gameObject.SetActive(value: false);
		}
		else
		{
			m_bPushedSelectedMenu = false;
		}
	}

	public void PopSelectedBlockMenu()
	{
		if (m_bPushedSelectedMenu && m_SelectedBlockMenu != null)
		{
			m_SelectedBlockMenu.gameObject.SetActive(value: true);
		}
		m_bPushedSelectedMenu = false;
	}

	public void CloseWelcomeDialog()
	{
		if (!(m_WelcomeDialog != null) || !m_WelcomeDialog.gameObject.GetActive())
		{
			return;
		}
		if (m_DontShowAgain != null && GlobalSave.GetInstance() != null)
		{
			GlobalSave.GetInstance().Set("LevelEditor:ShowWelcomeDialog", !m_DontShowAgain.isOn);
			GlobalSave.GetInstance().RequestSave();
			if (m_CachedController != null && m_CachedController.m_SavingIcon != null)
			{
				m_CachedController.m_SavingIcon.ShowSavingIcon();
			}
		}
		if (m_Player != null)
		{
			T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(m_Player);
			T17EventSystem.InputCateogryStates stateForRewiredPlayer = T17EventSystem.GetStateForRewiredPlayer(m_Player);
			if (stateForRewiredPlayer != T17EventSystem.InputCateogryStates.LevelEditor)
			{
				T17EventSystem.ApplyCategories(m_Player, T17EventSystem.InputCateogryStates.LevelEditor);
			}
		}
		m_WelcomeDialog.gameObject.SetActive(value: false);
	}

	public void UpdateWorldTextItems()
	{
		LevelDetailsManager instance = LevelDetailsManager.GetInstance();
		if (instance != null)
		{
			if (m_WorldPrisonTitle != null)
			{
				m_WorldPrisonTitle.text = instance.GetLevelName();
			}
			if (m_WorldPrisonDescription != null)
			{
				m_WorldPrisonDescription.text = instance.GetLevelDecription();
			}
			if (m_WorldPrisonEditDate != null)
			{
				m_WorldPrisonEditDate.text = instance.GetDateLastEdited();
			}
		}
	}

	public void SetBrushError(BaseLevelManager.BrushError error, float fExpireTime = 4.2949673E+09f, bool bClearOnExpiry = false)
	{
		if (error == BaseLevelManager.BrushError.eNone || m_bMouseOverControls)
		{
			HideBrushErrorToolTip();
			m_CurrentBrushError = BaseLevelManager.BrushError.eNone;
			return;
		}
		if ((error & BaseLevelManager.BrushError.eRoomBlocked) == BaseLevelManager.BrushError.eRoomBlocked)
		{
			error &= BaseLevelManager.BrushError.eRoomBlocked;
		}
		else if ((error & BaseLevelManager.BrushError.eCantOverwriteZone) == BaseLevelManager.BrushError.eCantOverwriteZone)
		{
			error &= BaseLevelManager.BrushError.eCantOverwriteZone;
		}
		else if ((error & BaseLevelManager.BrushError.eZoneNoIslands) == BaseLevelManager.BrushError.eZoneNoIslands)
		{
			error &= BaseLevelManager.BrushError.eZoneNoIslands;
		}
		else if ((error & BaseLevelManager.BrushError.eZoneNoDoughnuts) == BaseLevelManager.BrushError.eZoneNoDoughnuts)
		{
			error &= BaseLevelManager.BrushError.eZoneNoDoughnuts;
		}
		else if ((error & BaseLevelManager.BrushError.eZoneOverEmptySpace) == BaseLevelManager.BrushError.eZoneOverEmptySpace)
		{
			error &= BaseLevelManager.BrushError.eZoneOverEmptySpace;
		}
		else if ((error & BaseLevelManager.BrushError.eZoneOverAnotherZone) == BaseLevelManager.BrushError.eZoneOverAnotherZone)
		{
			error &= BaseLevelManager.BrushError.eZoneOverAnotherZone;
		}
		else if ((error & BaseLevelManager.BrushError.eBlockedAbove) == BaseLevelManager.BrushError.eBlockedAbove)
		{
			error &= BaseLevelManager.BrushError.eBlockedAbove;
		}
		else if ((error & BaseLevelManager.BrushError.eBlockedBelow) == BaseLevelManager.BrushError.eBlockedBelow)
		{
			error &= BaseLevelManager.BrushError.eBlockedBelow;
		}
		else if ((error & BaseLevelManager.BrushError.eBlocked) == BaseLevelManager.BrushError.eBlocked)
		{
			error &= BaseLevelManager.BrushError.eBlocked;
		}
		else if ((error & BaseLevelManager.BrushError.eInsideRequired) == BaseLevelManager.BrushError.eInsideRequired)
		{
			error &= BaseLevelManager.BrushError.eInsideRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eInsideAboveRequired) == BaseLevelManager.BrushError.eInsideAboveRequired)
		{
			error &= BaseLevelManager.BrushError.eInsideAboveRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eInsideBelowRequired) == BaseLevelManager.BrushError.eInsideBelowRequired)
		{
			error &= BaseLevelManager.BrushError.eInsideBelowRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eOutsideRequired) == BaseLevelManager.BrushError.eOutsideRequired)
		{
			error &= BaseLevelManager.BrushError.eOutsideRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eOutsideAboveRequired) == BaseLevelManager.BrushError.eOutsideAboveRequired)
		{
			error &= BaseLevelManager.BrushError.eOutsideAboveRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eOutsideBelowRequired) == BaseLevelManager.BrushError.eOutsideBelowRequired)
		{
			error &= BaseLevelManager.BrushError.eOutsideBelowRequired;
		}
		else if ((error & BaseLevelManager.BrushError.eNoClearance) == BaseLevelManager.BrushError.eNoClearance)
		{
			error &= BaseLevelManager.BrushError.eNoClearance;
		}
		else if ((error & BaseLevelManager.BrushError.eOutOfStock) == BaseLevelManager.BrushError.eOutOfStock)
		{
			error &= BaseLevelManager.BrushError.eOutOfStock;
		}
		else if ((error & BaseLevelManager.BrushError.eOutOfBounds) == BaseLevelManager.BrushError.eOutOfBounds)
		{
			error &= BaseLevelManager.BrushError.eOutOfBounds;
		}
		else if ((error & BaseLevelManager.BrushError.eInvalid) == BaseLevelManager.BrushError.eInvalid)
		{
			error &= BaseLevelManager.BrushError.eInvalid;
		}
		if (m_CurrentBrushError != error)
		{
			HideBrushErrorToolTip();
			m_CurrentBrushError = error;
			m_ClearErrorOnExpiry = bClearOnExpiry;
			m_ErrorExpireTime = fExpireTime;
			m_ErrorSetTime = Time.realtimeSinceStartup;
		}
	}

	private void OnOverControlChange(bool bOver)
	{
		m_bMouseOverControls = bOver;
		if (bOver)
		{
			HideBrushErrorToolTip();
		}
	}

	public void InviteRecieved()
	{
		if (!T17DialogBoxManager.HasAnyOpenDialogs() && !m_PrisonPublishDialog.gameObject.GetActive())
		{
			m_bInviteServiced = false;
			m_bWaitingToShowInviteDialog = false;
			T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (!(dialog2 != null))
			{
				return;
			}
			dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.SaveChangesTitle", "Text.Menu.SaveChangesBody", "Text.Yes", "Text.No", string.Empty);
			dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, (T17DialogBox.DialogEvent)delegate
			{
				if (m_CachedController != null)
				{
					m_CachedController.SaveTheLevel(bForceNew: true, InviteSaveResult);
				}
			});
			dialog2.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnDecline, (T17DialogBox.DialogEvent)delegate
			{
				m_bInviteServiced = true;
			});
			dialog2.Show();
		}
		else
		{
			m_bWaitingToShowInviteDialog = true;
		}
	}

	public bool GetEditorInviteResponse()
	{
		return m_bInviteServiced;
	}

	private void InviteSaveResult(LevelDetailsManager.RequestResultEnum eResult)
	{
		m_bInviteServiced = true;
	}

	public void OnFilterMenuButtonSelected(LevelEditor_FilterButton button)
	{
		button.ToggleActiveState();
		m_FilterMenuButton = button;
	}

	public void OnFilterThemeSelected(int selectedTheme)
	{
		if (BuildingBlock_FilterManager.GetInstance() != null)
		{
			BuildingBlock_FilterManager.GetInstance().SetCurrentRoomBlockSetFilter((BaseBuildingBlock.BlockSet)selectedTheme);
			m_FilterMenuButton.ToggleActiveState();
			SetBuildingBlockCategory(BuildingBlockCategory.Room, -1L);
		}
	}

	public Rect GetSizeOfCopyButton(GameObject obj)
	{
		Rect zero = Rect.zero;
		RectTransform component = obj.GetComponent<RectTransform>();
		if (component != null)
		{
			Vector3[] array = new Vector3[4];
			component.GetWorldCorners(array);
			zero.xMin = array[0].x;
			zero.yMin = array[0].y;
			zero.xMax = array[2].x;
			zero.yMax = array[2].y;
		}
		return zero;
	}

	public bool GetButtonArea(ButtonTypes buttonType, ref Rect area)
	{
		RectTransform rectTransform = null;
		switch (buttonType)
		{
		case ButtonTypes.Copy:
			if (m_CopyButton.activeInHierarchy)
			{
				rectTransform = m_CopyButtonTrans;
				break;
			}
			return false;
		case ButtonTypes.Delete:
			if (m_DeleteButton.activeInHierarchy)
			{
				rectTransform = m_DeleteButtonTrans;
				break;
			}
			return false;
		default:
			if (m_MoveButton.activeInHierarchy)
			{
				rectTransform = m_MoveButtonTrans;
				break;
			}
			return false;
		}
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		area.xMin = array[0].x;
		area.yMin = array[0].y;
		area.xMax = array[2].x;
		area.yMax = array[2].y;
		return true;
	}

	private void OnCellContentUpdated(LevelEditor_GridCellPopulator gridCellPopulator)
	{
		int i = 0;
		for (int num = m_PalletBackerTabs.Length; i < num; i++)
		{
			T17ScrollView t17ScrollView = m_PalletBackerScrollViews[i];
			if (!(t17ScrollView != null) || !t17ScrollView.gameObject.activeInHierarchy)
			{
				continue;
			}
			if (t17ScrollView.m_VerticalScrollBar != null)
			{
				t17ScrollView.m_VerticalScrollBar.value = 1f;
			}
			if (m_PalletBackerGridLayoutGroups[i] != null)
			{
				LevelEditor_GridCellPopulator[] items = m_PalletBackerGridCellPopulators[i];
				if (items.FindIndex((LevelEditor_GridCellPopulator x) => object.ReferenceEquals(x, gridCellPopulator)) != -1)
				{
					t17ScrollView.Hide(restoreInvokerState: false, isTabSwitch: true);
					t17ScrollView.Show(Gamer.GetPrimaryGamer(), null, null);
					break;
				}
			}
		}
	}
}
