using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class T17NetConnectAndJoinRoom : T17NetSendMonoMessageTarget
{
	private byte m_version_major = 100;

	private byte m_version_minor = 1;

	private string m_applicationVersion;

	private static T17NetOfflineModeAgent m_OfflineModeAgent = new T17NetOfflineModeAgent();

	private static T17NetOnlineModeIdleAgent m_OnlineModeIdleAgent = new T17NetOnlineModeIdleAgent();

	private static T17NetCreateRoomAgent m_CreateRoomAgent = new T17NetCreateRoomAgent();

	private static T17NetJoinRandomRoomAgent m_JoinRandomRoomAgent = new T17NetJoinRandomRoomAgent();

	private static T17NetJoinSpecificRoomAgent m_JoinSpecificRoomAgent = new T17NetJoinSpecificRoomAgent();

	private static T17NetRoomListAgent m_RoomLobbyListAgent = new T17NetRoomListAgent();

	private static T17NetJoinFilterRoomAgent m_JoinFilterAgent = new T17NetJoinFilterRoomAgent();

	private static T17NetMatchmakingAgent m_MatchmakingAgent = new T17NetMatchmakingAgent();

	private static T17NetRoomListSearchAgent m_RoomSearchListAgent = new T17NetRoomListSearchAgent();

	private Dictionary<NetConnectionState, T17NetAgentBase> m_AgentMap = new Dictionary<NetConnectionState, T17NetAgentBase>(9);

	private T17NetAgentBase m_CurrentAgent = m_OfflineModeAgent;

	private NetConnectionState m_CurrentAgentState;

	private static T17NetConnectAndJoinRoom m_instance = null;

	public string AppVersion => m_applicationVersion;

	public static T17NetConnectAndJoinRoom Instance => m_instance;

	protected override void Awake()
	{
		base.Awake();
		if (m_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		m_instance = this;
		m_applicationVersion = Platform.m_MatchmakingString;
		m_AgentMap.Add(NetConnectionState.OfflineMode, m_OfflineModeAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_CreateRoom, m_CreateRoomAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_Idle, m_OnlineModeIdleAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_JoinRandom, m_JoinRandomRoomAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_JoinSpecific, m_JoinSpecificRoomAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_RoomLobbyList, m_RoomSearchListAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_JoinFilter, m_JoinFilterAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_Matchmake, m_MatchmakingAgent);
		m_AgentMap.Add(NetConnectionState.OnlineMode_RoomSearchList, m_RoomSearchListAgent);
	}

	public void Start()
	{
		T17NetManager.AutoJoinLobby = false;
		T17NetManager.AutomaticallySyncScene = false;
		if (Debug.isDebugBuild || DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
		{
		}
		Platform.OnReadyToConnectToPhoton += OnPlatformReadyToConnect;
	}

	protected virtual void OnDestroy()
	{
		Platform.OnReadyToConnectToPhoton -= OnPlatformReadyToConnect;
	}

	public void Init_OnlineMode_JoinSpecific(string strRoomName)
	{
		m_JoinSpecificRoomAgent.SetRoomName(strRoomName);
	}

	public void Init_OnlineMode_CreateRoom(string roomName, string lobbyName, string[] customPropertiesForLobby, Hashtable roomPropertyValues)
	{
		m_CreateRoomAgent.SetGameParameters(roomName, lobbyName, customPropertiesForLobby, roomPropertyValues);
	}

	public void Init_OnlineMode_JoinFilter(string lobbyName, string strSQLGameSearch, byte maxPlayers)
	{
		m_JoinFilterAgent.SetSearchParameters(lobbyName, strSQLGameSearch, maxPlayers);
	}

	public void Init_OnlineMode_Matchmaking(string lobbyName, NetMatchmakingConfig config, byte maxPlayers, T17NetMatchmakingAgent.OnComplete completeCallback)
	{
		m_MatchmakingAgent.SetMatchmakingConfig(lobbyName, config, maxPlayers, completeCallback);
	}

	public void Init_OnlineMode_RoomLobbyList(string lobbyName)
	{
		m_RoomLobbyListAgent.SetLobby(lobbyName);
	}

	public void Init_OnlineMode_RoomSearchList(string lobbyName, string strSQLGameSearch)
	{
		m_RoomSearchListAgent.SetSearchParameters(lobbyName, strSQLGameSearch);
	}

	public bool RequestConnectionState(NetConnectionState state, bool silentErrorDialogMode)
	{
		T17NetManager.SilentErrorDialogMode = silentErrorDialogMode;
		T17NetAgentBase t17NetAgentBase = m_AgentMap[state];
		if (t17NetAgentBase != m_CurrentAgent)
		{
			m_CurrentAgent = t17NetAgentBase;
			m_CurrentAgentState = state;
		}
		return m_CurrentAgent.Start();
	}

	public NetConnectionState GetRequestedConnectionState()
	{
		return m_CurrentAgentState;
	}

	public virtual void OnDisconnectedFromPhoton()
	{
		m_CurrentAgent.OnDisconnected();
	}

	public virtual void OnConnectedToMaster()
	{
		m_CurrentAgent.OnConnectedToMaster();
	}

	public virtual void OnJoinedRoom()
	{
		m_CurrentAgent.OnJoinedRoom();
	}

	public virtual void OnLeftRoom()
	{
		m_CurrentAgent.OnLeftRoom();
	}

	public virtual void OnPhotonCreateRoomFailed()
	{
		m_CurrentAgent.OnCreateRoomFailed();
	}

	public virtual void OnPhotonRandomJoinFailed()
	{
		m_CurrentAgent.OnJoinedRoomFailed();
	}

	public virtual void OnPlatformReadyToConnect()
	{
		m_CurrentAgent.OnPlatformReadyToConnect();
	}

	public void Update()
	{
		if (m_CurrentAgent != null)
		{
			m_CurrentAgent.Update();
		}
	}
}
