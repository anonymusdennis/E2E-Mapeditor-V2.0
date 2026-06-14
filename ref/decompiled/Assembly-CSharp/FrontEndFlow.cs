using System.Collections;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class FrontEndFlow : BaseFlowBehaviour
{
	public enum MenuType
	{
		GameFrontend = 0,
		LevelEditor = 1,
		Unassigned = -1
	}

	private enum MODE
	{
		MODE_INIT,
		MODE_SHOW_MENUS,
		MODE_RUNNING,
		MODE_SHOW_LEVEL_EDITOR
	}

	public static FrontEndFlow Instance;

	public FrontendRootMenu m_MainMenu;

	public T17RawImage m_VideoImage;

	public T17Image m_StaticBackground;

	public VideoPlaybackSettings m_VideoSettings;

	private VideoDrone m_VideoDrone;

	public Canvas m_VideoCanvas;

	public EditorRootMenu m_EditorMenu;

	public MeshRenderer m_VideoPlane;

	public Animator m_SlideTopAnim;

	private FrontendEventListener m_BottomEventListener;

	public Animator m_SlideBottomAnim;

	public bool m_bSupressSlideIn;

	private bool m_bFrontendMusicPlaying;

	private Transform m_FrontendMenuParent;

	private T17Text m_FrontendLegendText;

	private Transform m_LevelEditorMenuParent;

	private T17Text m_LevelEditorLegendText;

	private SlotSelectionMenu m_SlotSelectionMenu;

	private PrisonSetupMenu m_PrisonSetupMenu;

	private CustomisationDialogFrontendMenu m_CustomisationDialog;

	private int m_PrisonSetupFrontendSiblingIndex = -1;

	private int m_SlotSelectionFrontendSiblingIndex = -1;

	private int m_CustomisationDialogSiblingIndex = -1;

	public MenuType m_CurrentMenuType;

	private FrontendMenuBehaviour m_MultiUserMenu;

	private MODE m_FrontEndFlow;

	protected override void Awake()
	{
		base.Awake();
		if (Instance != null)
		{
			Object.Destroy(this);
			return;
		}
		Instance = this;
		if (m_SlideTopAnim != null)
		{
			m_BottomEventListener = m_SlideBottomAnim.GetComponent<FrontendEventListener>();
			if (m_BottomEventListener != null)
			{
				m_BottomEventListener.AnimationEvent += BottomEventListener_AnimationEvent;
			}
		}
	}

	private void BottomEventListener_AnimationEvent(FrontendEventListener sender, bool transitionStarted)
	{
		if (m_MainMenu != null)
		{
			m_MainMenu.ForceSetTransitionInProgress(transitionStarted);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (Instance == this)
		{
			Instance = null;
		}
		if (m_BottomEventListener != null)
		{
			m_BottomEventListener.AnimationEvent -= BottomEventListener_AnimationEvent;
			m_BottomEventListener = null;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (m_VideoPlane != null)
		{
			m_VideoPlane.gameObject.SetActive(value: false);
		}
		if (m_VideoImage != null)
		{
			m_VideoImage.gameObject.SetActive(value: false);
		}
		m_FrontEndFlow = MODE.MODE_INIT;
		m_EditorMenu = EditorRootMenu.GetInstance();
		HideMenus();
	}

	protected override void Update()
	{
		base.Update();
		switch (m_FrontEndFlow)
		{
		case MODE.MODE_INIT:
			break;
		case MODE.MODE_SHOW_MENUS:
			HideCurrentMainMenu();
			ShowMenus();
			m_FrontEndFlow = MODE.MODE_RUNNING;
			break;
		case MODE.MODE_SHOW_LEVEL_EDITOR:
			HideCurrentMainMenu();
			ShowEditorMenus();
			m_FrontEndFlow = MODE.MODE_RUNNING;
			break;
		case MODE.MODE_RUNNING:
			break;
		}
	}

	public void HideCurrentMainMenu()
	{
		switch (m_CurrentMenuType)
		{
		case MenuType.GameFrontend:
			if (m_MainMenu != null)
			{
				m_MainMenu.Hide();
			}
			break;
		case MenuType.LevelEditor:
			if (m_EditorMenu != null)
			{
				m_EditorMenu.Hide();
			}
			break;
		}
	}

	public void SpecialFrontEndShowVideoBackGroundOnly()
	{
		StartVideoDrone();
	}

	public void StartFrontEnd()
	{
		m_FrontEndFlow = MODE.MODE_SHOW_MENUS;
		QualityManager.SetVsyncCount(0);
	}

	public void ExternalForceFlowToRunning()
	{
		m_FrontEndFlow = MODE.MODE_RUNNING;
	}

	public void StartFrontEndFromLevelEditor()
	{
		m_bSupressSlideIn = true;
		StartFrontEnd();
	}

	public void StartEditorFrontEnd()
	{
		m_FrontEndFlow = MODE.MODE_SHOW_LEVEL_EDITOR;
		GoogleAnalyticsV3.LogCommericalAnalyticEvent("GameFrontend", "Custom Prisons", string.Empty, 0L);
	}

	public void ShowMenus()
	{
		m_CurrentMenuType = MenuType.GameFrontend;
		ShowFrontendMenu();
		QualityManager.SetVsyncCount(0);
		PlayFrontendMusic(bPlay: true);
		StartVideoDrone();
	}

	public void ShowEditorMenus()
	{
		m_CurrentMenuType = MenuType.LevelEditor;
		ShowEditorMenu();
		PlayFrontendMusic(bPlay: true);
		StartVideoDrone();
	}

	private void PlayFrontendMusic(bool bPlay)
	{
		if (m_bFrontendMusicPlaying != bPlay)
		{
			m_bFrontendMusicPlaying = bPlay;
			if (bPlay)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_Music_Frontend, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Stop_Music_Frontend, base.gameObject);
			}
		}
	}

	private IEnumerator DelayedShowFrontendMenu()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		ShowFrontendMenu();
	}

	private void ShowFrontendMenu()
	{
		if (!(m_MainMenu != null))
		{
			return;
		}
		if (m_FrontendMenuParent != null)
		{
			if (m_SlotSelectionMenu != null && m_SlotSelectionMenu.transform.parent != m_FrontendMenuParent)
			{
				m_SlotSelectionMenu.transform.SetParent(m_FrontendMenuParent, worldPositionStays: false);
				m_SlotSelectionMenu.m_LegendTextItem = m_FrontendLegendText;
				if (m_SlotSelectionFrontendSiblingIndex != -1)
				{
					m_SlotSelectionMenu.transform.SetSiblingIndex(m_SlotSelectionFrontendSiblingIndex);
				}
			}
			if (m_PrisonSetupMenu != null && m_PrisonSetupMenu.transform.parent != m_FrontendMenuParent)
			{
				m_PrisonSetupMenu.transform.SetParent(m_FrontendMenuParent, worldPositionStays: false);
				m_PrisonSetupMenu.m_LegendTextItem = m_FrontendLegendText;
				if (m_PrisonSetupFrontendSiblingIndex != -1)
				{
					m_PrisonSetupMenu.transform.SetSiblingIndex(m_PrisonSetupFrontendSiblingIndex);
				}
				m_PrisonSetupMenu.SwitchOutOnCancel((FrontendMenuBehaviour)m_MainMenu.m_FrontEndTabableMenuTypes[FrontendRootMenu.FrontendMenuTypeToOpen.Campaign].m_Menus[0]);
			}
			if (m_CustomisationDialog != null && m_CustomisationDialog.transform.parent != m_FrontendMenuParent)
			{
				m_CustomisationDialog.transform.SetParent(m_FrontendMenuParent, worldPositionStays: false);
				m_CustomisationDialog.m_LegendTextItem = m_FrontendLegendText;
				if (m_PrisonSetupFrontendSiblingIndex != -1)
				{
					m_CustomisationDialog.transform.SetSiblingIndex(m_CustomisationDialogSiblingIndex);
				}
			}
			m_FrontendLegendText = null;
		}
		GlobalStart instance = GlobalStart.GetInstance();
		if (instance != null && (instance.m_ReturnToFrontendRoute == GlobalStart.ReturnToFrontendRoutes.Versus || instance.m_ReturnToFrontendRoute == GlobalStart.ReturnToFrontendRoutes.VersusLobby) && T17NetManager.ConnectionState != 0 && instance.m_ReturnToFrontendRoute == GlobalStart.ReturnToFrontendRoutes.VersusLobby)
		{
			SetFrontendUserPathForVersusLobbyReturn();
		}
		RerouteFrontendForPreviousUserPath();
		instance.m_ReturnToFrontendRoute = GlobalStart.ReturnToFrontendRoutes.None;
		m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		if (!m_bSupressSlideIn)
		{
			if (m_BottomEventListener != null)
			{
				m_MainMenu.ForceSetTransitionInProgress(inProgress: true);
			}
			if (m_SlideTopAnim != null)
			{
				m_SlideTopAnim.SetTrigger("TransitionForward");
			}
			if (m_SlideBottomAnim != null)
			{
				m_SlideBottomAnim.SetTrigger("TransitionForward");
			}
		}
		else
		{
			m_bSupressSlideIn = false;
			RootMenu.MenuList_Container menuList_Container = m_MainMenu.m_FrontEndTabableMenuTypes[m_MainMenu.m_CurrentFrontEndMenuType];
			BaseMenuBehaviour baseMenuBehaviour = menuList_Container.m_Menus[menuList_Container.m_DefaultTab];
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_Out, base.gameObject);
			baseMenuBehaviour.PlayBackTransition();
		}
	}

	private void SetFrontendUserPathForVersusLobbyReturn()
	{
		int menuChildIndex = 0;
		if (T17NetManager.ConnectedAndReady && !T17NetManager.OfflineMode)
		{
			T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
			menuChildIndex = ((!T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue)) ? 2 : ((outValue != T17NetRoomGameView.GameRoomType.Private) ? 2 : 3));
		}
		else if (T17NetManager.OfflineMode)
		{
			menuChildIndex = 1;
		}
		FrontendUserPath.RecordFrontendPath(MenuType.GameFrontend, 2, menuChildIndex);
	}

	private void RerouteFrontendForPreviousUserPath()
	{
		switch (FrontendUserPath.m_FrontendSection)
		{
		case MenuType.Unassigned:
			m_MainMenu.SetFrontEndMenuTypeToOpen(FrontendRootMenu.FrontendMenuTypeToOpen.MainMenu);
			FrontendUserPath.ClearPath();
			break;
		case MenuType.GameFrontend:
		{
			FrontendRootMenu.FrontendMenuTypeToOpen frontendMenuIndex = (FrontendRootMenu.FrontendMenuTypeToOpen)FrontendUserPath.m_FrontendMenuIndex;
			int menuChildIndex = FrontendUserPath.m_MenuChildIndex;
			FrontendUserPath.ClearPath();
			SwitchToFrontEndMenuType(frontendMenuIndex, isBack: false);
			if (menuChildIndex != 0)
			{
				OpenChildOnTopOfMenu(menuChildIndex);
			}
			break;
		}
		case MenuType.LevelEditor:
			StartEditorFrontEnd();
			break;
		}
	}

	private void ShowEditorMenu()
	{
		bool flag = m_EditorMenu == null;
		m_EditorMenu = EditorRootMenu.GetInstance();
		if (!(m_EditorMenu != null))
		{
			return;
		}
		if (flag)
		{
			FixUpEditorMenuReferences();
		}
		if (m_LevelEditorMenuParent != null)
		{
			if (m_SlotSelectionMenu != null && m_SlotSelectionMenu.transform.parent != m_LevelEditorMenuParent)
			{
				m_SlotSelectionMenu.transform.SetParent(m_LevelEditorMenuParent, worldPositionStays: false);
				if (m_SlotSelectionMenu.m_LegendTextItem != null && m_FrontendLegendText == null)
				{
					m_FrontendLegendText = m_SlotSelectionMenu.m_LegendTextItem;
				}
				m_SlotSelectionMenu.m_LegendTextItem = m_LevelEditorLegendText;
			}
			if (m_PrisonSetupMenu != null && m_PrisonSetupMenu.transform.parent != m_LevelEditorMenuParent)
			{
				m_PrisonSetupMenu.transform.SetParent(m_LevelEditorMenuParent, worldPositionStays: false);
				if (m_PrisonSetupMenu.m_LegendTextItem != null && m_FrontendLegendText == null)
				{
					m_FrontendLegendText = m_PrisonSetupMenu.m_LegendTextItem;
				}
				m_PrisonSetupMenu.m_LegendTextItem = m_LevelEditorLegendText;
			}
			if (m_CustomisationDialog != null && m_CustomisationDialog.transform.parent != m_LevelEditorMenuParent)
			{
				m_CustomisationDialog.transform.SetParent(m_LevelEditorMenuParent, worldPositionStays: false);
				if (m_CustomisationDialog.m_LegendTextItem != null && m_FrontendLegendText == null)
				{
					m_FrontendLegendText = m_CustomisationDialog.m_LegendTextItem;
				}
				m_CustomisationDialog.m_LegendTextItem = m_LevelEditorLegendText;
			}
		}
		EditorRootMenu.EditorMenuTypeToOpen frontEndMenuTypeToOpen = EditorRootMenu.EditorMenuTypeToOpen.EditorHomepageMenu;
		int index = 0;
		if (FrontendUserPath.m_FrontendSection == MenuType.LevelEditor)
		{
			frontEndMenuTypeToOpen = (EditorRootMenu.EditorMenuTypeToOpen)FrontendUserPath.m_FrontendMenuIndex;
			index = FrontendUserPath.m_MenuChildIndex;
		}
		FrontendUserPath.ClearPath();
		m_EditorMenu.SetFrontEndMenuTypeToOpen(frontEndMenuTypeToOpen);
		m_EditorMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		RootMenu.MenuList_Container menuList_Container = m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.EditorHomepageMenu];
		BaseMenuBehaviour baseMenuBehaviour = menuList_Container.m_Menus[index];
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_In, base.gameObject);
		baseMenuBehaviour.PlayForwardTransition();
	}

	public void EditorSetPrisonSetupMenuOnCancel(bool bMyPrisonMenu)
	{
		if (m_PrisonSetupMenu != null)
		{
			if (bMyPrisonMenu)
			{
				m_PrisonSetupMenu.SwitchOutOnCancel((FrontendMenuBehaviour)m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.MyPrisonsMenu].m_Menus[0]);
			}
			else
			{
				m_PrisonSetupMenu.SwitchOutOnCancel((FrontendMenuBehaviour)m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.SubscribedMenu].m_Menus[0]);
			}
		}
	}

	private void StartVideoDrone()
	{
		if (m_VideoDrone == null && m_VideoSettings != null && m_VideoImage != null)
		{
			bool flag = PlayerPrefs.GetInt("Settings:BackgroundVideoEnabled", 1) == 1;
			if (m_VideoCanvas != null)
			{
				m_VideoCanvas.gameObject.SetActive(value: true);
			}
			m_VideoDrone = VideoDrone.CreateDrone(m_VideoImage.gameObject, m_VideoSettings, m_VideoImage, m_VideoPlane);
			m_VideoImage.gameObject.SetActive(value: true);
			m_VideoImage.enabled = true;
			m_VideoDrone.gameObject.SetActive(value: true);
			m_VideoDrone.Play(videoLoops: true, audioOn: false);
			if (!flag)
			{
				ToggleBackgroundVideo(flag);
			}
			if (m_VideoDrone.OutputTexture != null)
			{
				m_VideoImage.texture = m_VideoDrone.OutputTexture;
			}
			if (!m_VideoImage.texture)
			{
			}
		}
	}

	public void HideMenus()
	{
		if (m_MainMenu != null)
		{
			m_MainMenu.Hide();
		}
		if (m_EditorMenu != null)
		{
			m_EditorMenu.Hide();
		}
		m_MultiUserMenu = null;
		PlayFrontendMusic(bPlay: false);
		if (Gamer.GetPrimaryGamer() != null && Gamer.GetPrimaryGamer().m_RewiredPlayer != null)
		{
			T17EventSystem.ApplyCategories(Gamer.GetPrimaryGamer().m_RewiredPlayer, T17EventSystem.InputCateogryStates.Loading);
		}
		if (m_VideoCanvas != null)
		{
			m_VideoCanvas.gameObject.SetActive(value: false);
		}
		m_VideoImage.gameObject.SetActive(value: false);
	}

	public void SwitchBackToFrontendMenu(FrontendMenuBehaviour menu)
	{
		SwitchToFrontendMenu(menu, isBack: true);
	}

	public void SwitchToFrontendMenu(FrontendMenuBehaviour menu)
	{
		bool flag = false;
		switch (m_CurrentMenuType)
		{
		case MenuType.GameFrontend:
			flag = m_MainMenu.IsTransitionInProgress();
			break;
		case MenuType.LevelEditor:
			flag = m_EditorMenu.IsTransitionInProgress();
			break;
		}
		if (!flag)
		{
			SwitchToFrontendMenu(menu, isBack: false);
		}
	}

	private void SwitchToFrontendMenu(FrontendMenuBehaviour menu, bool isBack)
	{
		SwitchMenuNoAnim(menu);
		if (isBack)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_Out, base.gameObject);
			menu.PlayBackTransition();
			return;
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_In, base.gameObject);
		menu.PlayForwardTransition();
		if (menu.m_bLogAnalyticOnEnter)
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent(m_CurrentMenuType.ToString(), menu.name, string.Empty, 0L);
		}
	}

	public void SwitchMenuNoAnim(BaseMenuBehaviour menu)
	{
		switch (m_CurrentMenuType)
		{
		case MenuType.GameFrontend:
			if (m_MainMenu != null)
			{
				m_MainMenu.Hide();
				m_MainMenu.SetFrontEndMenuTypeToOpen(menu);
				m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
			}
			break;
		case MenuType.LevelEditor:
			if (m_EditorMenu != null)
			{
				m_EditorMenu.Hide();
				m_EditorMenu.SetFrontEndMenuTypeToOpen(menu);
				m_EditorMenu.Show(Gamer.GetPrimaryGamer(), null, null);
			}
			break;
		}
	}

	public void SwitchBackToMainMenu()
	{
		switch (m_CurrentMenuType)
		{
		case MenuType.GameFrontend:
			SwitchToFrontEndMenuType(FrontendRootMenu.FrontendMenuTypeToOpen.MainMenu, isBack: true);
			break;
		case MenuType.LevelEditor:
			SwitchToFrontEndMenuType(EditorRootMenu.EditorMenuTypeToOpen.EditorHomepageMenu, isBack: true);
			break;
		}
	}

	public void SwitchToFrontEndMenuType(FrontendRootMenu.FrontendMenuTypeToOpen type)
	{
		SwitchToFrontEndMenuType(type, isBack: false);
	}

	public void SwitchToFrontEndMenuType(EditorRootMenu.EditorMenuTypeToOpen type)
	{
		SwitchToFrontEndMenuType(type, isBack: false);
	}

	public void SetInMultiUserMenu(FrontendMenuBehaviour menu)
	{
		m_MultiUserMenu = menu;
	}

	public FrontendMenuBehaviour InMultiUserMenu()
	{
		return m_MultiUserMenu;
	}

	private void SwitchToFrontEndMenuType(FrontendRootMenu.FrontendMenuTypeToOpen type, bool isBack)
	{
		if (m_MainMenu != null)
		{
			m_MainMenu.Hide();
			m_MainMenu.SetFrontEndMenuTypeToOpen(type);
			m_MainMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		}
		RootMenu.MenuList_Container menuList_Container = m_MainMenu.m_FrontEndTabableMenuTypes[type];
		BaseMenuBehaviour baseMenuBehaviour = menuList_Container.m_Menus[menuList_Container.m_DefaultTab];
		if (isBack)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_Out, base.gameObject);
			baseMenuBehaviour.PlayBackTransition();
		}
		else
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_In, base.gameObject);
			baseMenuBehaviour.PlayForwardTransition();
		}
	}

	private void SwitchToFrontEndMenuType(EditorRootMenu.EditorMenuTypeToOpen type, bool isBack)
	{
		if (m_EditorMenu != null)
		{
			m_EditorMenu.Hide();
			m_EditorMenu.SetFrontEndMenuTypeToOpen(type);
			m_EditorMenu.Show(Gamer.GetPrimaryGamer(), null, null);
		}
		RootMenu.MenuList_Container menuList_Container = m_EditorMenu.m_FrontEndTabableMenuTypes[type];
		BaseMenuBehaviour baseMenuBehaviour = menuList_Container.m_Menus[menuList_Container.m_DefaultTab];
		if (isBack)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_Out, base.gameObject);
			baseMenuBehaviour.PlayBackTransition();
		}
		else
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Transition_In, base.gameObject);
			baseMenuBehaviour.PlayForwardTransition();
		}
	}

	public void OpenChildOnTopOfMenu(int index)
	{
		if (index == 0)
		{
			return;
		}
		switch (m_CurrentMenuType)
		{
		case MenuType.GameFrontend:
			if (m_MainMenu != null)
			{
				m_MainMenu.OpenFrontendChildOfCurrent(index);
			}
			break;
		case MenuType.LevelEditor:
			if (m_EditorMenu != null)
			{
				m_EditorMenu.OpenFrontendChildOfCurrent(index);
			}
			break;
		}
	}

	private void FixUpEditorMenuReferences()
	{
		if (m_EditorMenu != null)
		{
			m_SlotSelectionMenu = (SlotSelectionMenu)m_MainMenu.m_FrontEndTabableMenuTypes[FrontendRootMenu.FrontendMenuTypeToOpen.Campaign].m_Menus[1];
			m_PrisonSetupMenu = (PrisonSetupMenu)m_MainMenu.m_FrontEndTabableMenuTypes[FrontendRootMenu.FrontendMenuTypeToOpen.PrisonSetupMenu].m_Menus[0];
			m_CustomisationDialog = (CustomisationDialogFrontendMenu)m_MainMenu.m_FrontEndTabableMenuTypes[FrontendRootMenu.FrontendMenuTypeToOpen.PrisonSetupMenu].m_Menus[1];
			if (m_SlotSelectionMenu != null)
			{
				m_SlotSelectionFrontendSiblingIndex = m_SlotSelectionMenu.transform.GetSiblingIndex();
				m_FrontendMenuParent = m_SlotSelectionMenu.transform.parent;
				m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.MyPrisonsMenu].m_Menus.Add(m_SlotSelectionMenu);
				m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.SubscribedMenu].m_Menus.Add(m_SlotSelectionMenu);
			}
			if (m_PrisonSetupMenu != null)
			{
				m_PrisonSetupFrontendSiblingIndex = m_PrisonSetupMenu.transform.GetSiblingIndex();
				m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.PrisonSetupMenu].m_Menus.Add(m_PrisonSetupMenu);
			}
			if (m_CustomisationDialog != null)
			{
				m_CustomisationDialogSiblingIndex = m_CustomisationDialog.transform.GetSiblingIndex();
				m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.PrisonSetupMenu].m_Menus.Add(m_CustomisationDialog);
			}
			m_LevelEditorMenuParent = m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.MyPrisonsMenu].m_Menus[0].transform.parent;
			m_LevelEditorLegendText = m_EditorMenu.m_FrontEndTabableMenuTypes[EditorRootMenu.EditorMenuTypeToOpen.MyPrisonsMenu].m_Menus[0].m_LegendTextItem;
		}
	}

	public MenuType GetCurrentMenuType()
	{
		return m_CurrentMenuType;
	}

	public void ToggleBackgroundVideo(bool bOnOff)
	{
		if (!(m_VideoDrone != null) || !(m_StaticBackground != null))
		{
			return;
		}
		if (bOnOff)
		{
			m_VideoDrone.gameObject.SetActive(value: true);
			if (!m_VideoDrone.IsPlaying)
			{
				m_VideoDrone.Play(videoLoops: true, audioOn: false);
			}
			m_StaticBackground.gameObject.SetActive(value: false);
		}
		else
		{
			m_VideoDrone.gameObject.SetActive(value: false);
			if (m_VideoDrone.IsPlaying)
			{
				m_VideoDrone.StopVideo();
			}
			m_StaticBackground.gameObject.SetActive(value: true);
		}
	}
}
