using System;
using System.Collections.Generic;
using T17.UI.Carousel;
using UnityEngine;
using UnityEngine.UI;

public class CampaignFrontendMenu : FrontendMenuBehaviour
{
	public GameObject m_CoopGoalText;

	public GameObject m_SoloGoalText;

	public Selectable m_BrowseGamesButton;

	public Selectable m_MatchmakeGamesButton;

	public SaveSlotController m_ContinueButtonController;

	public SaveSlotController m_NewGameButtonController;

	public SaveSlotController m_LoadButtonController;

	public T17Text m_KeysRequiredText;

	public string m_MatchmakingTitle = "Text.Menu.Searching";

	public string m_MatchmakingBody = "Text.Menu.Matchmaking.SearchingForBody";

	public string m_MatchmakingFailedTitle = "Text.Matchmaking.FailedTitle";

	public string m_MatchmakingFailedBody = "Text.Matchmaking.FailedBody";

	public BrowseGamesFrontendMenu m_LobbyListMenu;

	public PlaylistDataCarousel m_PlaylistCarousel;

	public UIFriendCampaignCarousel m_FriendsCarousel;

	public GameObject m_OnlinePrisonersContainer;

	public T17Text m_OnlinePrisoners;

	private T17DialogBox m_MatchMakingDialogBox;

	public FrontendPopup m_CampaignPopup;

	private bool m_bCampaignPopupShown;

	public FrontendPopup m_LevelUnlock_1;

	private bool m_bLvlPopup1Shown;

	public FrontendPopup m_LevelUnlock_2;

	private bool m_bLvlPopup2Shown;

	public FrontendPopup m_LevelUnlock_3;

	private bool m_bLvlPopup3Shown;

	public void OnStartSinglePlayer()
	{
		if (GlobalStart.GetInstance() != null)
		{
			GlobalStart.GetInstance().SetupLoadStatesAndStartGameWithConfig();
		}
	}

	protected override void Start()
	{
		base.Start();
		UpdatePlaylistCarousel();
		if (null != m_OnlinePrisoners)
		{
			m_OnlinePrisoners.m_bNeedsLocalization = false;
		}
		SetBrowseOrMatchmakeButton();
		T17NetManager.OnPhotonConnectionChangeEvent += OnPhotonConnectionChange;
	}

	private void OnPhotonConnectionChange(bool connected)
	{
		if (!connected && null != m_MatchMakingDialogBox)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected);
			m_MatchMakingDialogBox.Hide();
			m_MatchMakingDialogBox = null;
		}
	}

	private void SetBrowseOrMatchmakeButton()
	{
		bool flag = false;
		flag = !T17NetConfig.FeatureCampaignMatchmaking;
		if (m_BrowseGamesButton != null)
		{
			m_BrowseGamesButton.gameObject.SetActive(flag);
		}
		if (m_MatchmakeGamesButton != null)
		{
			m_MatchmakeGamesButton.gameObject.SetActive(!flag);
		}
		Selectable selectable = ((!flag) ? m_BrowseGamesButton : m_MatchmakeGamesButton);
		Selectable selectable2 = ((!flag) ? m_MatchmakeGamesButton : m_BrowseGamesButton);
		Selectable[] componentsInChildren = GetComponentsInChildren<Selectable>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Navigation navigation = componentsInChildren[i].navigation;
			if (navigation.selectOnDown == selectable)
			{
				navigation.selectOnDown = selectable2;
			}
			if (navigation.selectOnLeft == selectable)
			{
				navigation.selectOnLeft = selectable2;
			}
			if (navigation.selectOnRight == selectable)
			{
				navigation.selectOnRight = selectable2;
			}
			if (navigation.selectOnUp == selectable)
			{
				navigation.selectOnUp = selectable2;
			}
			componentsInChildren[i].navigation = navigation;
		}
		GlobalSave instance = GlobalSave.GetInstance();
		if (instance != null)
		{
			instance.Get("CampaignPopupShown", out m_bCampaignPopupShown, def: false);
			instance.Get("LevelPopup1Shown", out m_bLvlPopup1Shown, def: false);
			instance.Get("LevelPopup2Shown", out m_bLvlPopup2Shown, def: false);
			instance.Get("LevelPopup3Shown", out m_bLvlPopup3Shown, def: false);
		}
	}

	private void DLCCheckedCallback()
	{
		LevelDataManager.GetInstance().RefreshCampaignPlaylistsforDLC();
		List<PlaylistData> campaignPlaylists = LevelDataManager.GetInstance().GetCampaignPlaylists();
		m_PlaylistCarousel.UpdateCarouselOptionsWithoutResetIndex(campaignPlaylists);
		GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		UpdateOnlinePlayerDisplay();
		UpdateLockStatus();
		UpdatePrisonInfo();
	}

	private void UpdatePlaylistCarousel()
	{
		List<PlaylistData> campaignPlaylists = LevelDataManager.GetInstance().GetCampaignPlaylists();
		m_PlaylistCarousel.SetCarouselOptions(campaignPlaylists);
		int value = 0;
		GlobalSave.GetInstance().Get("CFM:LastSelectedIndex", out value, 0);
		int numOptions = m_PlaylistCarousel.GetNumOptions();
		if (numOptions > 0 && (numOptions <= value || value < 0))
		{
			value = numOptions - 1;
			GlobalSave.GetInstance().Set("CFM:LastSelectedIndex", value);
		}
		m_PlaylistCarousel.SelectIndex(value);
		GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		UpdateOnlinePlayerDisplay();
		UpdateLockStatus();
		UpdatePrisonInfo();
	}

	private void PlaylistCarousel_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		GlobalSave.GetInstance().Set("CFM:LastSelectedIndex", index);
		UpdateOnlinePlayerDisplay();
		UpdateLockStatus();
		UpdatePrisonInfo();
	}

	public void MatchmakeCurrentPrison()
	{
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, OnOnlineEntryNowDenied);
		if (!(m_LobbyListMenu != null))
		{
			return;
		}
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isOnline)
		{
			if (isOnline)
			{
				if (m_MatchMakingDialogBox == null)
				{
					m_MatchMakingDialogBox = T17DialogBoxManager.GetDialog(forSingleUser: false);
					if (m_MatchMakingDialogBox != null)
					{
						Localization.Get(m_PlaylistCarousel.GetSelectedItem().m_NameLocalisationKey, out var localized);
						Localization.GetWithKeySwap(m_MatchmakingBody, out var localised, "$PLAYLIST_NAME", localized);
						m_MatchMakingDialogBox.InitializeSpinner(hasCancelButton: true, m_MatchmakingTitle, localised, string.Empty, bLocalizeTitle: true, bLocalizeBody: false);
						T17DialogBox matchMakingDialogBox = m_MatchMakingDialogBox;
						matchMakingDialogBox.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(matchMakingDialogBox.OnCancel, new T17DialogBox.DialogEvent(OnDialogCancelMatchmaking));
						m_MatchMakingDialogBox.Show();
						StartMatchmakingProcess();
					}
				}
			}
			else
			{
				Platform.GetInstance().ExitOnlineArea();
			}
		});
	}

	public void MatchmakeFriend(Platform.DisplayableFriend friend)
	{
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, OnOnlineEntryNowDenied);
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isOnline)
		{
			if (isOnline)
			{
				if (m_MatchMakingDialogBox == null)
				{
					m_MatchMakingDialogBox = T17DialogBoxManager.GetDialog(forSingleUser: false);
					if (m_MatchMakingDialogBox != null)
					{
						Localization.GetWithKeySwap(m_MatchmakingBody, out var localised, "$PLAYLIST_NAME", friend.m_Name);
						m_MatchMakingDialogBox.InitializeSpinner(hasCancelButton: true, m_MatchmakingTitle, localised, string.Empty, bLocalizeTitle: true, bLocalizeBody: false);
						T17DialogBox matchMakingDialogBox = m_MatchMakingDialogBox;
						matchMakingDialogBox.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(matchMakingDialogBox.OnCancel, new T17DialogBox.DialogEvent(OnDialogCancelJoining));
						m_MatchMakingDialogBox.Show();
						NetJoinRoomHelper.FindAndJoinRoom(friend.m_OnlineID, showReconnectPrompt: false, OnMatchmakingCompleted, showConnectionFailedPrompts: true, showConnectingDialog: false);
					}
				}
			}
			else
			{
				Platform.GetInstance().ExitOnlineArea();
			}
		});
	}

	private void OnDialogCancelJoining(T17DialogBox dialog = null)
	{
		T17NetLobbyManager.Instance.CancelFindRoom();
		m_MatchMakingDialogBox = null;
		NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
	}

	private void StartMatchmakingProcess()
	{
		string lobbyName = string.Empty;
		NetMatchmakingConfig config = null;
		if (NetConnectAndJoinRoom.GetCampaignMatchmakingSearch(out lobbyName, out config))
		{
			NetConnectAndJoinRoom.Init_OnlineMode_Matchmaking(lobbyName, config, 4, OnMatchmakingCompleted);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Matchmake);
		}
	}

	private void OnMatchmakingCompleted(bool bSuccess)
	{
		if (bSuccess)
		{
			if (m_MatchMakingDialogBox != null)
			{
				m_MatchMakingDialogBox.Hide();
				m_MatchMakingDialogBox = null;
			}
			StartLoadingGame();
		}
		else
		{
			m_MatchMakingDialogBox.Hide();
			m_MatchMakingDialogBox = null;
		}
	}

	private void OnDialogCancelMatchmaking(T17DialogBox dialog = null)
	{
		m_MatchMakingDialogBox = null;
		NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
	}

	public void BrowseGamesLobby()
	{
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, OnOnlineEntryNowDenied);
		if (!(m_LobbyListMenu != null))
		{
			return;
		}
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isOnline)
		{
			if (isOnline)
			{
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
				FrontEndFlow.Instance.SwitchToFrontendMenu(m_LobbyListMenu);
			}
			else
			{
				Platform.GetInstance().ExitOnlineArea();
			}
		});
	}

	private void UpdateOnlinePlayerDisplay()
	{
		if (null != m_OnlinePrisoners)
		{
			m_OnlinePrisoners.SetNonLocalizedText("-");
		}
		if (m_OnlinePrisonersContainer != null)
		{
			m_OnlinePrisonersContainer.SetActive(value: false);
		}
		if (!T17NetManager.IsConnectedOnline() || null == m_PlaylistCarousel || m_PlaylistCarousel.m_Options.Count == 0)
		{
			return;
		}
		PlaylistData selectedItem = m_PlaylistCarousel.GetSelectedItem();
		if (null == selectedItem || selectedItem.m_Prisons == null || selectedItem.m_Prisons.Count == 0)
		{
			return;
		}
		PlaylistData.PrisonSetup prisonSetup = selectedItem.m_Prisons[0];
		if (prisonSetup == null)
		{
			return;
		}
		PrisonData prisonData = prisonSetup.m_PrisonData;
		if (null == prisonData)
		{
			return;
		}
		PrisonData.LevelInfo levelInfo = prisonData.m_LevelInfo;
		if (levelInfo == null)
		{
			return;
		}
		List<TypedLobbyInfo> lobbyStatistics = PhotonNetwork.LobbyStatistics;
		if (lobbyStatistics == null || lobbyStatistics.Count == 0 || null == m_OnlinePrisoners)
		{
			return;
		}
		string lobbyName = NetConnectAndJoinRoom.GetLobbyName(PrisonConfig.ConfigType.Cooperative);
		int count = lobbyStatistics.Count;
		for (int i = 0; i < count; i++)
		{
			if (string.Equals(lobbyName, lobbyStatistics[i].Name))
			{
				m_OnlinePrisoners.text = lobbyStatistics[i].PlayerCount.ToString();
				if (m_OnlinePrisonersContainer != null)
				{
					m_OnlinePrisonersContainer.SetActive(value: true);
				}
				break;
			}
		}
	}

	private void OnOnlineEntryNowDenied()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		FrontEndFlow.Instance.SwitchToFrontendMenu(this);
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer != null && !FrontEndFlow.Instance.m_MainMenu.IsChildMenuOpen() && m_MatchMakingDialogBox == null && !T17DialogBoxManager.HasAnyOpenDialogs())
		{
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleLeft"))
			{
				m_PlaylistCarousel.SelectPrevious();
			}
			if (base.CurrentGamer.m_RewiredPlayer.GetButtonDown("UI_CycleRight"))
			{
				m_PlaylistCarousel.SelectNext();
			}
		}
		if (!T17NetManager.IsMasterClient && !Application.isConsolePlatform)
		{
			ClientUpdateMatchmakeLobby();
		}
	}

	protected void StartLoadingGame()
	{
		if (!T17NetManager.IsMasterClient)
		{
			GlobalStart.GetInstance().SetSelectedLevelToNetRoomCurrent();
		}
		GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(GlobalStart.GLOBALSTART_GAME_MODES.ONLINE);
	}

	protected virtual void ClientUpdateMatchmakeLobby()
	{
		if (!Helpers.IsMasterClientInFrontEnd() && Helpers.IsInFrontEndScene() && T17NetRoomManager.IsInRoom() && T17NetManager.IsConnectedOnline())
		{
			StartLoadingGame();
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		FrontendUserPath.RecordFrontendPath(FrontEndFlow.MenuType.GameFrontend, 1, 0);
		SetBrowseOrMatchmakeButton();
		Platform instance = Platform.GetInstance();
		instance.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Remove(instance.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(DLCCheckedCallback));
		Platform instance2 = Platform.GetInstance();
		instance2.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Combine(instance2.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(DLCCheckedCallback));
		Platform.GetInstance().RefreshDLC();
		m_PlaylistCarousel.IndexSelectedEvent += PlaylistCarousel_IndexSelectedEvent;
		if (m_PlaylistCarousel.GetNumOptions() != 0)
		{
			GlobalStart.GetInstance().SetSelectedPlaylist(m_PlaylistCarousel.GetSelectedItem());
		}
		if (m_FriendsCarousel != null)
		{
			Platform instance3 = Platform.GetInstance();
			instance3.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance3.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
			Platform instance4 = Platform.GetInstance();
			instance4.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Combine(instance4.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
			UpdateFriendCarousel();
		}
		T17NetManager.OnLobbyStatisticsUpdatedEvent += UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent += T17NetManager_OnPhotonConnectionChangeEvent;
		T17NetRoomGameView.Instance.ClearCustomProperties();
		UpdateOnlinePlayerDisplay();
		UpdateLockStatus();
		UpdatePrisonInfo();
		if (m_CampaignPopup != null && !m_bCampaignPopupShown)
		{
			FrontEndFlow instance5 = FrontEndFlow.Instance;
			if (instance5 != null)
			{
				instance5.OpenChildOnTopOfMenu(2);
				m_bCampaignPopupShown = true;
				GlobalSave instance6 = GlobalSave.GetInstance();
				if (instance6 != null)
				{
					instance6.Set("CampaignPopupShown", m_bCampaignPopupShown);
					instance6.RequestSave();
				}
			}
		}
		else
		{
			for (int i = 0; i < m_PlaylistCarousel.m_Options.Count; i++)
			{
				bool flag = true;
				if (m_PlaylistCarousel.m_Options[i].m_UnlockMilestone != null)
				{
					flag = !ProgressManager.GetInstance().GetMilestoneAchieved(m_PlaylistCarousel.m_Options[i].m_UnlockMilestone.id);
				}
				PlaylistData.PrisonSetup prisonSetup = m_PlaylistCarousel.m_Options[i].m_Prisons[0];
				if (prisonSetup == null || flag)
				{
					continue;
				}
				bool flag2 = false;
				switch (prisonSetup.m_PrisonData.m_LevelInfo.m_PrisonEnum)
				{
				case LevelScript.PRISON_ENUM.POW_Camp:
				case LevelScript.PRISON_ENUM.Oil_Rig:
				case LevelScript.PRISON_ENUM.Transport_Boat:
					if (m_LevelUnlock_1 != null && !m_bLvlPopup1Shown)
					{
						CallPopup(3, ref m_bLvlPopup1Shown, "LevelPopup1Shown");
						flag2 = true;
					}
					break;
				case LevelScript.PRISON_ENUM.Gulag_Prison:
				case LevelScript.PRISON_ENUM.Transport_Plane:
				case LevelScript.PRISON_ENUM.Area_17:
					if (m_LevelUnlock_2 != null && !m_bLvlPopup2Shown)
					{
						CallPopup(4, ref m_bLvlPopup2Shown, "LevelPopup2Shown");
						flag2 = true;
					}
					break;
				case LevelScript.PRISON_ENUM.Space_Prison:
					if (m_LevelUnlock_3 != null && !m_bLvlPopup3Shown)
					{
						CallPopup(5, ref m_bLvlPopup3Shown, "LevelPopup3Shown");
						flag2 = true;
					}
					break;
				}
				if (flag2)
				{
					break;
				}
			}
		}
		return true;
	}

	private void OnNetworkChangedCallback(Platform.PlatformNetworkStatus status)
	{
		UpdateFriendCarousel();
	}

	private void UpdateFriendCarousel()
	{
		if (Platform.GetInstance().OnlineCheck())
		{
			m_FriendsCarousel.PopulateWithFriends();
		}
		else
		{
			m_FriendsCarousel.DisableFriendFeed();
		}
	}

	private void T17NetManager_OnPhotonConnectionChangeEvent(bool isConnected)
	{
		if (m_OnlinePrisonersContainer != null)
		{
			if (!isConnected)
			{
				m_OnlinePrisonersContainer.SetActive(value: false);
			}
			else
			{
				UpdateOnlinePlayerDisplay();
			}
		}
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		Platform instance = Platform.GetInstance();
		instance.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
		Platform instance2 = Platform.GetInstance();
		instance2.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Remove(instance2.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(DLCCheckedCallback));
		m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		T17NetManager.OnLobbyStatisticsUpdatedEvent -= UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
		return true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Platform instance = Platform.GetInstance();
		instance.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
		Platform instance2 = Platform.GetInstance();
		instance2.OnDLCUpdatedEvent = (Platform.DLCUpdatedEvent)Delegate.Remove(instance2.OnDLCUpdatedEvent, new Platform.DLCUpdatedEvent(DLCCheckedCallback));
		m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		T17NetManager.OnLobbyStatisticsUpdatedEvent -= UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent -= OnPhotonConnectionChange;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
	}

	private void UpdateLockStatus()
	{
		bool flag = false;
		if (m_PlaylistCarousel.m_Options.Count == 0)
		{
			return;
		}
		PlaylistData selectedItem = m_PlaylistCarousel.GetSelectedItem();
		if (selectedItem != null && selectedItem.m_UnlockMilestone != null)
		{
			flag = !ProgressManager.GetInstance().GetMilestoneAchieved(selectedItem.m_UnlockMilestone.id);
		}
		if (KeyAwardManager.AreAllPrisonsUnlocked)
		{
			flag = false;
		}
		if (!flag)
		{
			switch (selectedItem.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum)
			{
			case LevelScript.PRISON_ENUM.POW_Camp:
			case LevelScript.PRISON_ENUM.Oil_Rig:
			case LevelScript.PRISON_ENUM.Transport_Boat:
				if (m_LevelUnlock_1 != null && !m_bLvlPopup1Shown)
				{
					CallPopup(3, ref m_bLvlPopup1Shown, "LevelPopup1Shown");
				}
				break;
			case LevelScript.PRISON_ENUM.Gulag_Prison:
			case LevelScript.PRISON_ENUM.Transport_Plane:
			case LevelScript.PRISON_ENUM.Area_17:
				if (m_LevelUnlock_2 != null && !m_bLvlPopup2Shown)
				{
					CallPopup(4, ref m_bLvlPopup2Shown, "LevelPopup2Shown");
				}
				break;
			case LevelScript.PRISON_ENUM.Space_Prison:
				if (m_LevelUnlock_3 != null && !m_bLvlPopup3Shown)
				{
					CallPopup(5, ref m_bLvlPopup3Shown, "LevelPopup3Shown");
				}
				break;
			}
		}
		if (m_ContinueButtonController != null)
		{
			m_ContinueButtonController.isLocked = flag;
		}
		if (m_NewGameButtonController != null)
		{
			m_NewGameButtonController.isLocked = flag;
		}
		if (m_LoadButtonController != null)
		{
			m_LoadButtonController.isLocked = flag;
		}
		if (!(m_KeysRequiredText != null))
		{
			return;
		}
		m_KeysRequiredText.gameObject.SetActive(flag);
		if (!flag || !(selectedItem.m_UnlockMilestone != null))
		{
			return;
		}
		ProgressMilestone.Criteria criteria = selectedItem.m_UnlockMilestone.criteria[0];
		float num = criteria.statRule.m_RefValue - StatSystem.GetInstance().GetStatValue(44);
		if (num > 0f)
		{
			string text = "Text.FE.KeysRemaining";
			if (num == 1f)
			{
				text = "Text.FE.KeyRemaining";
			}
			Localization.GetWithKeySwap(text, out var localised, "$keys", (int)num);
			m_KeysRequiredText.text = localised;
		}
		else
		{
			m_KeysRequiredText.gameObject.SetActive(value: false);
		}
	}

	private void UpdatePrisonInfo()
	{
		if (m_PlaylistCarousel == null || m_PlaylistCarousel.GetNumOptions() < 1)
		{
			return;
		}
		PlaylistData selectedItem = m_PlaylistCarousel.GetSelectedItem();
		if (selectedItem == null)
		{
			return;
		}
		if (m_CoopGoalText == null)
		{
			Debug.LogError("m_CoopGoalText is null. Returning... ");
			return;
		}
		if (m_SoloGoalText == null)
		{
			Debug.LogError("m_SoloGoalText is null. Returning... ");
			return;
		}
		PlaylistData.PrisonSetup prisonSetup = ((selectedItem.m_Prisons.Count <= 0) ? null : selectedItem.m_Prisons[0]);
		if (prisonSetup == null || prisonSetup.m_PrisonData == null || prisonSetup.m_PrisonData.m_LevelInfo == null)
		{
			Debug.LogError("prisonData is null. Returning... ");
			return;
		}
		bool flag = prisonSetup.m_PrisonData.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial;
		m_CoopGoalText.SetActive(!flag);
		m_SoloGoalText.SetActive(flag);
		m_MatchmakeGamesButton.interactable = !flag;
	}

	public override GameObject FindValidSelectableForLostFocus()
	{
		GameObject gameObject = base.FindValidSelectableForLostFocus();
		if (gameObject == null)
		{
			Selectable selectable = ((!(m_NewGameButtonController != null)) ? null : m_NewGameButtonController.GetComponent<Selectable>());
			if (selectable != null)
			{
				if (selectable.IsInteractable())
				{
					return selectable.gameObject;
				}
				return selectable.navigation.selectOnUp.gameObject;
			}
		}
		return gameObject;
	}

	private void CallPopup(int menuIndex, ref bool popupShown, string saveName)
	{
		FrontEndFlow instance = FrontEndFlow.Instance;
		if (instance != null)
		{
			instance.OpenChildOnTopOfMenu(menuIndex);
			popupShown = true;
			GlobalSave instance2 = GlobalSave.GetInstance();
			if (instance2 != null)
			{
				instance2.Set(saveName, popupShown);
				instance2.RequestSave();
			}
		}
	}

	public void OnNewGameButtonClicked()
	{
		if (!IsTransitionInProgress())
		{
			m_NewGameButtonController.OnSlotClicked();
		}
	}
}
