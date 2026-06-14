using System.Collections.Generic;
using T17.UI.Carousel;
using UnityEngine;

public class VersusFrontendMenu : FrontendMenuBehaviour
{
	public GameObject m_PrivateVersusButton;

	public CoopFrontEndMenu m_CoopMenu;

	public BrowseGamesFrontendMenu m_LobbyListMenu;

	public PlaylistDataCarousel m_PlaylistCarousel;

	private static PlaylistData m_SelectedPlaylist;

	public GameObject m_OnlinePrisonersContainer;

	public T17Text m_OnlinePrisoners;

	public static T17NetRoomGameView.GameRoomType RoomType { get; set; }

	protected override void Start()
	{
		base.Start();
		if (m_PlaylistCarousel == null)
		{
		}
		List<PlaylistData> versusPlaylists = LevelDataManager.GetInstance().GetVersusPlaylists();
		m_PlaylistCarousel.SetCarouselOptions(versusPlaylists);
		m_SelectedPlaylist = m_PlaylistCarousel.GetSelectedItem();
		UpdateOnlinePlayerDisplay();
		if (null != m_OnlinePrisoners)
		{
			m_OnlinePrisoners.m_bNeedsLocalization = false;
		}
	}

	private void PlaylistCarousel_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		m_SelectedPlaylist = m_PlaylistCarousel.GetSelectedItem();
		UpdateOnlinePlayerDisplay();
	}

	public void OnPublicButtonPressed()
	{
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, NewDisallowedUserCallback);
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
		{
			if (isConnected)
			{
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
				FrontEndFlow.Instance.OpenChildOnTopOfMenu(2);
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
		if (!T17NetManager.IsConnectedOnline() || null == m_SelectedPlaylist)
		{
			return;
		}
		List<TypedLobbyInfo> lobbyStatistics = PhotonNetwork.LobbyStatistics;
		if (lobbyStatistics == null || lobbyStatistics.Count == 0 || null == m_OnlinePrisoners)
		{
			return;
		}
		string lobbyName = NetConnectAndJoinRoom.GetLobbyName(PrisonConfig.ConfigType.Versus);
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

	public void NewDisallowedUserCallback()
	{
		FrontendMenuBehaviour frontendMenuBehaviour = FrontEndFlow.Instance.InMultiUserMenu();
		if (frontendMenuBehaviour != null)
		{
			frontendMenuBehaviour.Hide();
		}
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		Platform.GetInstance().ExitOnlineArea();
	}

	public void OnPrivateButtonPressed()
	{
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, NewDisallowedUserCallback);
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
		{
			if (isConnected)
			{
				string lobbyName = NetConnectAndJoinRoom.GetLobbyName(PrisonConfig.ConfigType.Versus);
				NetCreateRoomHelper.CreateRoomMatchmakingProperties(PrisonConfig.ConfigType.Versus, RoomType, out var customPropertyDefinitionsForLobby, out var initialRoomPropertyValues, string.Empty);
				NetConnectAndJoinRoom.Init_OnlineMode_CreateRoom(PrisonConfig.ConfigType.Versus, lobbyName, customPropertyDefinitionsForLobby, initialRoomPropertyValues);
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_CreateRoom);
				FrontEndFlow.Instance.OpenChildOnTopOfMenu(3);
			}
			else
			{
				Platform.GetInstance().ExitOnlineArea();
			}
		});
	}

	public void OnCoop()
	{
		NetCreateRoomHelper.RequestCreateRoom(T17NetRoomGameView.GameRoomType.Offline, PrisonConfig.ConfigType.Versus, delegate(bool roomSetupOk)
		{
			if (roomSetupOk)
			{
				GlobalStart.GetInstance().SetSelectedPlaylist(m_SelectedPlaylist, randomiseOrder: true);
				NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
				FrontEndFlow.Instance.OpenChildOnTopOfMenu(1);
			}
		}, showDialogs: false, string.Empty);
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer != null && !FrontEndFlow.Instance.m_MainMenu.IsChildMenuOpen())
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
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		FrontendUserPath.RecordFrontendPath(FrontEndFlow.MenuType.GameFrontend, 2, 0);
		m_PlaylistCarousel.IndexSelectedEvent += PlaylistCarousel_IndexSelectedEvent;
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		if (m_PrivateVersusButton != null)
		{
			m_PrivateVersusButton.SetActive(value: true);
		}
		T17NetManager.OnLobbyStatisticsUpdatedEvent += UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent += T17NetManager_OnPhotonConnectionChangeEvent;
		UpdateOnlinePlayerDisplay();
		return result;
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
		m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		T17NetManager.OnLobbyStatisticsUpdatedEvent -= UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_PlaylistCarousel.IndexSelectedEvent -= PlaylistCarousel_IndexSelectedEvent;
		T17NetManager.OnLobbyStatisticsUpdatedEvent -= UpdateOnlinePlayerDisplay;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
	}

	public static PlaylistData GetSelectedPlaylist()
	{
		return m_SelectedPlaylist;
	}

	public static int GetSelectedPlaylistIndex()
	{
		List<PlaylistData> versusPlaylists = LevelDataManager.GetInstance().GetVersusPlaylists();
		int result = 0;
		for (int i = 0; i < versusPlaylists.Count; i++)
		{
			if (m_SelectedPlaylist == versusPlaylists[i])
			{
				result = i + 1;
				break;
			}
		}
		return result;
	}
}
