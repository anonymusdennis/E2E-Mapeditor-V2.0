using UnityEngine;

public class PublicVersusFEMenu : VersusLobbyFEMenu
{
	[Tooltip("How long before a game starts when we have the min amount of players in the lobby")]
	public float m_TwoPlayerCountdown = 30f;

	private int m_PrevNumberOfPlayers;

	protected override void OnDestroy()
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.SetInMultiUserMenu(null);
		}
		base.OnDestroy();
	}

	protected override T17NetRoomGameView.GameRoomType GetGameRoomType()
	{
		return T17NetRoomGameView.GameRoomType.Public;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		m_buildLobbyDataInBaseShow = false;
		VersusFrontendMenu.RoomType = T17NetRoomGameView.GameRoomType.Public;
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		Platform.GetInstance().SetPresenceTag("Text.Presence.VersusOnline");
		FrontEndFlow.Instance.SetInMultiUserMenu(this);
		string text = string.Empty;
		string matchmakingParameterFromCustomProperty = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.RoomPlatformType);
		if (matchmakingParameterFromCustomProperty != null)
		{
			string text2 = text;
			text = text2 + matchmakingParameterFromCustomProperty + " = \"" + T17NetConfig.GetPlatformType() + "\"";
		}
		string matchmakingParameterFromCustomProperty2 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.AppVersion);
		if (matchmakingParameterFromCustomProperty2 != null)
		{
			string text2 = text;
			text = text2 + " AND " + matchmakingParameterFromCustomProperty2 + " = \"" + T17NetConfig.MatchingVersionString + "\"";
		}
		string nameLocalisationKey = VersusFrontendMenu.GetSelectedPlaylist().m_NameLocalisationKey;
		string matchmakingParameterFromCustomProperty3 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.PlaylistId);
		if (matchmakingParameterFromCustomProperty3 != null)
		{
			string text2 = text;
			text = text2 + " AND " + matchmakingParameterFromCustomProperty3 + " = \"" + nameLocalisationKey + "\"";
		}
		string matchmakingParameterFromCustomProperty4 = NetMatchmakingConfig.GetMatchmakingParameterFromCustomProperty(T17NetRoomGameView.CustomProperty.RoomType);
		if (matchmakingParameterFromCustomProperty4 != null)
		{
			string text2 = text;
			text = text2 + " AND " + matchmakingParameterFromCustomProperty4 + " = " + 1;
		}
		NetMatchmakingConfig instance = NetMatchmakingConfig.GetInstance(PrisonConfig.ConfigType.Versus);
		GlobalStart instance2 = GlobalStart.GetInstance();
		bool flag = true;
		if (instance2 != null && instance2.m_ReturnToFrontendRoute != 0)
		{
			flag = false;
		}
		if (null != instance && flag)
		{
			instance.m_strSearchPrefix = text;
			string lobbyName = NetConnectAndJoinRoom.GetLobbyName(PrisonConfig.ConfigType.Versus);
			NetConnectAndJoinRoom.Init_OnlineMode_Matchmaking(lobbyName, NetMatchmakingConfig.GetInstance(PrisonConfig.ConfigType.Versus), 4, OnMatchmakingCompleted);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Matchmake);
		}
		MarkLobbyForUpdate();
		return result;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (FrontEndFlow.Instance != null)
		{
			FrontEndFlow.Instance.SetInMultiUserMenu(null);
		}
		if (Platform.GetInstance() != null)
		{
			Platform.GetInstance().ExitOnlineArea();
		}
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	private void OnMatchmakingCompleted(bool bSuccess)
	{
		if (bSuccess)
		{
			return;
		}
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
		NetCreateRoomHelper.RequestCreateRoom(T17NetRoomGameView.GameRoomType.Public, PrisonConfig.ConfigType.Versus, delegate(bool isConnected)
		{
			if (isConnected)
			{
				GlobalStart.GetInstance().SetSelectedPlaylist(m_SelectedPlaylist, randomiseOrder: true);
				MarkLobbyForUpdate();
			}
		}, showDialogs: false, string.Empty);
	}

	protected override void MasterUpdateLobby()
	{
		base.MasterUpdateLobby();
		int numberRemoteMembers = m_LobbyData.GetNumberRemoteMembers();
		if (m_bIsCountingDownGame && m_bMasterCountdownSignalSent && T17NetRoomManager.IsInRoom() && numberRemoteMembers != m_PrevNumberOfPlayers && numberRemoteMembers == 3 && AreAllClientsReady())
		{
			m_PrevNumberOfPlayers = numberRemoteMembers;
			m_StartGameTimeLeft = m_LobbyFullCountdown;
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.LobbyCountdownSync, (float)PhotonNetwork.time + m_StartGameTimeLeft);
		}
		if (!m_bIsCountingDownGame && !m_bMasterCountdownSignalSent && T17NetRoomManager.IsInRoom() && numberRemoteMembers >= m_MinRemotePlayersNeeded && AreAllClientsReady())
		{
			m_bMasterCountdownSignalSent = true;
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.LobbyCountdownStarted, m_TwoPlayerCountdown);
		}
	}

	protected override void StartLoadingGame()
	{
		Platform.GetInstance().SetSessionLocked(shouldLock: true);
		base.StartLoadingGame();
	}

	protected override void SetLobbyPlayerObjectForPlayer(LobbyPlayerObject lpo, Platform.LobbyData.MemberData member)
	{
		string text = member.m_Name;
		lpo.m_Label.m_bNeedsLocalization = false;
		lpo.m_Label.SetNewPlaceHolder(text);
		lpo.m_Label.text = text;
		lpo.m_DisplayableFriend = new Platform.DisplayableFriend();
		lpo.m_DisplayableFriend.m_Gamer = member.m_Gamer;
		lpo.m_DisplayableFriend.m_OnlineID = member.m_Gamer.m_PlatformUniqueID;
		lpo.m_DisplayableFriend.m_Name = member.m_Name;
		lpo.SetPlayerReady();
	}

	protected override void SetLobbyPlayerObjectToDefault(LobbyPlayerObject lpo)
	{
		lpo.SetPlayerEmpty();
		lpo.m_Label.SetLocalisedTextCatchAll("Text.Menu.Versus.Public.WaitingForPlayerSlot");
	}
}
