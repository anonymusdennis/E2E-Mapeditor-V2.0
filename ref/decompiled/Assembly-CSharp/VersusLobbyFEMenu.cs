using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class VersusLobbyFEMenu : FrontendMenuBehaviour
{
	[Tooltip("The amount of remote players (ie not including local) needed to start the game")]
	public int m_MinRemotePlayersNeeded = 1;

	public GameObject m_PlayerListParentObj;

	[Tooltip("The time in seconds until a game starts when the lobby is full")]
	public float m_LobbyFullCountdown = 3f;

	protected float m_StartGameTimeLeft;

	protected bool m_bIsCountingDownGame;

	protected float m_fNextSyncTime;

	protected float m_fDeSyncBuffer = 0.5f;

	public const float SyncDelay = 0.25f;

	protected bool m_bMasterLoadingSignalSent;

	protected bool m_bMasterCountdownSignalSent;

	public T17Text m_GameCountdownLabel;

	private LobbyPlayerObject[] m_GUILobbyPlayers;

	protected Platform.LobbyData m_LobbyData = new Platform.LobbyData();

	protected PlaylistData m_SelectedPlaylist;

	protected bool m_buildLobbyDataInBaseShow = true;

	public List<Platform.VoiceChatGamer> m_VoiceChatGamers = new List<Platform.VoiceChatGamer>();

	protected abstract void SetLobbyPlayerObjectToDefault(LobbyPlayerObject lpo);

	protected abstract void SetLobbyPlayerObjectForPlayer(LobbyPlayerObject lpo, Platform.LobbyData.MemberData member);

	protected abstract T17NetRoomGameView.GameRoomType GetGameRoomType();

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 4; i++)
		{
			m_VoiceChatGamers.Add(new Platform.VoiceChatGamer());
		}
		Gamer.OnDeleted += OnGamerDeleted;
		Gamer.OnCreate += OnGamerCreated;
		Gamer.OnUpdated += OnGamerUpdated;
	}

	protected override void Start()
	{
		base.Start();
		if (m_GameCountdownLabel != null)
		{
			m_GameCountdownLabel.m_bNeedsLocalization = false;
			m_GameCountdownLabel.gameObject.SetActive(value: false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		T17NetRoomGameView.OnRoomSignalEvent -= T17NetRoomGameView_OnRoomSignalEvent;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
		Platform.OnResumeFromSuspended -= Platform_OnResumeFromSuspended;
		Gamer.OnDeleted -= OnGamerDeleted;
		Gamer.OnCreate -= OnGamerCreated;
		Gamer.OnUpdated -= OnGamerUpdated;
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_PlayerListParentObj != null)
		{
			m_GUILobbyPlayers = m_PlayerListParentObj.GetComponentsInChildren<LobbyPlayerObject>(includeInactive: true);
		}
		m_SelectedPlaylist = VersusFrontendMenu.GetSelectedPlaylist();
		T17NetRoomGameView.OnRoomSignalEvent += T17NetRoomGameView_OnRoomSignalEvent;
		T17NetManager.OnPhotonConnectionChangeEvent += T17NetManager_OnPhotonConnectionChangeEvent;
		Platform.OnResumeFromSuspended += Platform_OnResumeFromSuspended;
		if (m_buildLobbyDataInBaseShow)
		{
			MarkLobbyForUpdate();
		}
		GlobalStart instance = GlobalStart.GetInstance();
		T17NetRoomGameView instance2 = T17NetRoomGameView.Instance;
		if (!T17NetRoomManager.IsInRoom() && instance != null && instance.m_ReturnToFrontendRoute == GlobalStart.ReturnToFrontendRoutes.None && instance2 != null && !T17NetInvites.HasInvite())
		{
			instance2.ClearCustomProperties();
		}
		if (null != m_GameCountdownLabel && null != m_GameCountdownLabel.gameObject)
		{
			m_GameCountdownLabel.gameObject.SetActive(value: false);
		}
		return true;
	}

	public virtual void OnKeyRetrievalError()
	{
		ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.RequestingAuthKeysFailed);
		On_CancelButtonPressed();
	}

	private void Platform_OnResumeFromSuspended()
	{
		T17NetManager_OnPhotonConnectionChangeEvent(isConnected: false);
	}

	private void T17NetManager_OnPhotonConnectionChangeEvent(bool isConnected)
	{
		if (!isConnected)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			Hide();
		}
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		bool result = base.Hide(restoreInvokerState, isTabSwitch);
		T17NetRoomGameView.OnRoomSignalEvent -= T17NetRoomGameView_OnRoomSignalEvent;
		T17NetManager.OnPhotonConnectionChangeEvent -= T17NetManager_OnPhotonConnectionChangeEvent;
		Platform.OnResumeFromSuspended -= Platform_OnResumeFromSuspended;
		m_bMasterCountdownSignalSent = false;
		m_bMasterLoadingSignalSent = false;
		m_bIsCountingDownGame = false;
		m_StartGameTimeLeft = 30f;
		Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
		return result;
	}

	private void T17NetRoomGameView_OnRoomSignalEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (m_GameCountdownLabel != null)
		{
			m_GameCountdownLabel.gameObject.SetActive(value: false);
		}
		switch (roomSignal)
		{
		case T17NetConfig.NetEventTypes.LobbyCountdownStarted:
			if (m_GameCountdownLabel != null)
			{
				m_GameCountdownLabel.gameObject.SetActive(value: true);
			}
			m_StartGameTimeLeft = (float)payload;
			m_bIsCountingDownGame = true;
			m_bMasterLoadingSignalSent = false;
			break;
		case T17NetConfig.NetEventTypes.LobbyCountdownStopped:
			m_bMasterCountdownSignalSent = false;
			m_bIsCountingDownGame = false;
			break;
		case T17NetConfig.NetEventTypes.LobbyCountdownSync:
		{
			if (m_GameCountdownLabel != null)
			{
				m_GameCountdownLabel.gameObject.SetActive(value: true);
			}
			float num = (float)payload - (float)PhotonNetwork.time;
			if (num < m_StartGameTimeLeft || num > m_StartGameTimeLeft + m_fDeSyncBuffer)
			{
				m_StartGameTimeLeft = num;
			}
			m_bIsCountingDownGame = true;
			m_bMasterLoadingSignalSent = false;
			break;
		}
		case T17NetConfig.NetEventTypes.StartLoading:
			if (GlobalStart.GetInstance().GetMode() == GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND)
			{
				StartLoadingGame();
			}
			break;
		}
	}

	protected virtual void StartLoadingGame()
	{
		GlobalStart.GLOBALSTART_MODE roomState = T17NetRoomListManager.Instance.GetRoomState();
		if (!T17NetManager.IsMasterClient)
		{
			GlobalStart.GetInstance().SetSelectedLevelToNetRoomCurrent();
		}
		else
		{
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Versus Game Started", GetGameRoomType().ToString() + " Started", string.Empty, 0L);
		}
		GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(GlobalStart.GLOBALSTART_GAME_MODES.ONLINE);
		m_bMasterLoadingSignalSent = false;
		m_bMasterCountdownSignalSent = false;
		m_bIsCountingDownGame = false;
	}

	protected override void Update()
	{
		base.Update();
		if (m_bIsCountingDownGame)
		{
			m_StartGameTimeLeft -= Time.deltaTime;
			if (m_StartGameTimeLeft < 0f)
			{
				m_StartGameTimeLeft = 0f;
			}
			if (m_GameCountdownLabel != null)
			{
				m_GameCountdownLabel.text = ((int)m_StartGameTimeLeft).ToString();
			}
		}
		if (T17NetRoomManager.IsInRoom())
		{
			if (T17NetManager.IsMasterClient)
			{
				MasterUpdateLobby();
			}
			else
			{
				ClientUpdateLobby();
			}
		}
		Platform.GetInstance().GetTalkingGamers(ref m_VoiceChatGamers);
		for (int i = 0; i < m_GUILobbyPlayers.Length; i++)
		{
			if (m_GUILobbyPlayers[i] != null)
			{
				m_GUILobbyPlayers[i].UpdateTalkingIconsWithTalkingGamers(m_VoiceChatGamers);
			}
		}
	}

	protected virtual void ClientUpdateLobby()
	{
		GlobalStart.GLOBALSTART_MODE roomState = T17NetRoomListManager.Instance.GetRoomState();
		T17NetRoomGameView.EscapeState outValue = T17NetRoomGameView.EscapeState.Escaped;
		if ((Helpers.IsLoadingState(roomState) || roomState == GlobalStart.GLOBALSTART_MODE.IN_LEVEL) && T17NetRoomGameView.Instance != null && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.EscapeState, ref outValue) && outValue == T17NetRoomGameView.EscapeState.NotEscaped && Helpers.IsInFrontEndScene() && T17NetRoomManager.IsInRoom() && T17NetManager.IsConnectedOnline())
		{
			StartLoadingGame();
		}
	}

	protected bool AreAllClientsReady()
	{
		bool result = true;
		if (PhotonNetwork.playerList != null)
		{
			for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
			{
				if (NetLoadSync.GetLoadStateForPhotonId(PhotonNetwork.playerList[i].ID) != 0)
				{
					result = false;
				}
			}
		}
		return result;
	}

	protected virtual void MasterUpdateLobby()
	{
		int numberRemoteMembers = m_LobbyData.GetNumberRemoteMembers();
		if (m_bIsCountingDownGame)
		{
			if (numberRemoteMembers < m_MinRemotePlayersNeeded)
			{
				StopCountdownRPC();
			}
			else if (!m_bMasterLoadingSignalSent && m_StartGameTimeLeft <= 0f && AreAllClientsReady())
			{
				SendLevelLoadSignalRPC();
			}
			else if (T17NetManager.RealTime > m_fNextSyncTime)
			{
				m_fNextSyncTime = T17NetManager.RealTime + 0.25f;
				T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.LobbyCountdownSync, (float)PhotonNetwork.time + m_StartGameTimeLeft);
			}
		}
	}

	public void StopCountdownRPC()
	{
		if (m_bIsCountingDownGame)
		{
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.LobbyCountdownStopped);
		}
	}

	public void SendLevelLoadSignalRPC()
	{
		if (!m_bMasterLoadingSignalSent)
		{
			m_bMasterLoadingSignalSent = true;
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.StartLoading, true);
		}
	}

	protected virtual void UpdateLobbyGUI()
	{
		if (m_GUILobbyPlayers == null)
		{
			return;
		}
		int num = m_GUILobbyPlayers.Length;
		for (int i = 0; i < num; i++)
		{
			LobbyPlayerObject lobbyPlayerObject = m_GUILobbyPlayers[i];
			if (lobbyPlayerObject != null)
			{
				Platform.LobbyData.MemberData memberData = m_LobbyData.m_Members[i];
				if (memberData != null && memberData.m_IsValid)
				{
					SetLobbyPlayerObjectForPlayer(lobbyPlayerObject, memberData);
				}
				else
				{
					SetLobbyPlayerObjectToDefault(lobbyPlayerObject);
				}
			}
		}
	}

	public void OnGamerDeleted()
	{
		MarkLobbyForUpdate();
	}

	public void OnGamerCreated(Gamer gamer)
	{
		MarkLobbyForUpdate();
	}

	public void OnGamerUpdated(Gamer gamer)
	{
		MarkLobbyForUpdate();
	}

	protected void MarkLobbyForUpdate()
	{
		UpdateLobbyData();
		UpdateLobbyGUI();
	}

	public virtual void StopVersus()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle, silentErrorDialogMode: true);
		Platform.GetInstance().LeaveSession();
		Hide();
	}

	public virtual void On_CancelButtonPressed()
	{
		NavigateOnUICancel component = GetComponent<NavigateOnUICancel>();
		if (component != null)
		{
			component.m_DoThisOnUICancel.Invoke();
		}
	}

	protected virtual void UpdateLobbyData()
	{
		for (int i = 0; i < 4; i++)
		{
			m_LobbyData.m_Members[i].m_Gamer = null;
			m_LobbyData.m_Members[i].m_ID = -1;
			m_LobbyData.m_Members[i].m_bLocalPlayer = false;
			m_LobbyData.m_Members[i].m_Name = string.Empty;
			m_LobbyData.m_Members[i].m_NetViewID = -1;
			m_LobbyData.m_Members[i].m_bNewPlayer = false;
			m_LobbyData.m_Members[i].m_IsValid = false;
		}
		m_LobbyData.m_MemberCount = 0;
		int num = 0;
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int num2 = allGamers.Length - 1; num2 >= 0; num2--)
		{
			Gamer gamer = allGamers[num2];
			if (gamer != null)
			{
				num = m_LobbyData.m_MemberCount;
				m_LobbyData.m_Members[num].m_bNewPlayer = true;
				m_LobbyData.m_Members[num].m_Gamer = gamer;
				m_LobbyData.m_Members[num].m_ID = gamer.m_PhotonID;
				m_LobbyData.m_Members[num].m_bLocalPlayer = gamer.m_PhotonID == T17NetManager.PhotonPlayerID;
				m_LobbyData.m_Members[num].m_Name = gamer.m_GamerName;
				m_LobbyData.m_Members[num].m_NetViewID = gamer.m_NetViewID;
				m_LobbyData.m_Members[num].m_bNewPlayer = true;
				m_LobbyData.m_Members[num].m_IsValid = true;
				if (DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.PlayerGamer))
				{
				}
				m_LobbyData.m_MemberCount++;
			}
		}
		Array.Sort(m_LobbyData.m_Members, delegate(Platform.LobbyData.MemberData X, Platform.LobbyData.MemberData Y)
		{
			int num3 = X.m_ID;
			int num4 = Y.m_ID;
			if (num3 < 0)
			{
				num3 = int.MaxValue;
			}
			if (num4 < 0)
			{
				num4 = int.MaxValue;
			}
			return num3.CompareTo(num4);
		});
	}

	protected virtual int GetLobbyPlayerCount()
	{
		if (m_LobbyData != null && m_LobbyData.m_Members != null)
		{
			int result = 0;
			for (int num = m_LobbyData.m_Members.Length - 1; num <= 0; num--)
			{
				if (m_LobbyData.m_Members[num] != null)
				{
					num++;
				}
			}
			return result;
		}
		return 0;
	}
}
