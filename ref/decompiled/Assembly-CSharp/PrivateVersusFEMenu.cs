using UnityEngine;

public class PrivateVersusFEMenu : VersusLobbyFEMenu
{
	public T17Button m_StartButton;

	protected override T17NetRoomGameView.GameRoomType GetGameRoomType()
	{
		return T17NetRoomGameView.GameRoomType.Private;
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
		lpo.m_Label.SetLocalisedTextCatchAll("Text.Menu.Versus.InvitePlayer");
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		m_StartButton.interactable = false;
		bool result = base.Show(currentGamer, parent, invoker, hideInvoker);
		VersusFrontendMenu.RoomType = T17NetRoomGameView.GameRoomType.Private;
		Platform.GetInstance().SetPresenceTag("Text.Presence.VersusOnline");
		FrontEndFlow.Instance.SetInMultiUserMenu(this);
		if (!T17NetManager.IsConnectedOnline() || !T17NetRoomManager.IsInRoom())
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
			NetCreateRoomHelper.RequestCreateRoom(T17NetRoomGameView.GameRoomType.Private, PrisonConfig.ConfigType.Versus, delegate(bool isConnected)
			{
				if (isConnected)
				{
					GlobalStart.GetInstance().SetSelectedPlaylist(m_SelectedPlaylist, randomiseOrder: true);
					MarkLobbyForUpdate();
				}
			}, showDialogs: false, string.Empty);
		}
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

	protected override void UpdateLobbyData()
	{
		base.UpdateLobbyData();
		if (T17NetManager.IsMasterClient)
		{
			int numberRemoteMembers = m_LobbyData.GetNumberRemoteMembers();
			m_StartButton.interactable = numberRemoteMembers >= m_MinRemotePlayersNeeded;
		}
	}

	public void OnInviteButtonPressed()
	{
		if (m_LobbyData.m_MemberCount != 4)
		{
			Platform.GetInstance().OpenInvitePicker();
		}
	}

	public override void On_CancelButtonPressed()
	{
		if (m_bIsCountingDownGame && T17NetManager.IsMasterClient)
		{
			StopCountdownRPC();
		}
		else
		{
			base.On_CancelButtonPressed();
		}
	}

	public void OnStartButtonPressed()
	{
		if (m_LobbyData.m_MemberCount >= 2 && !m_bMasterCountdownSignalSent)
		{
			m_bMasterCountdownSignalSent = true;
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.LobbyCountdownStarted, m_LobbyFullCountdown);
		}
	}
}
