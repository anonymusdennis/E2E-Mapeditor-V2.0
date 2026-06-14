using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEngine;

public class T17NetLoadSync : T17NetSendMonoMessageTarget
{
	public class RoomStateConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				string[] array = ((string)value).Split(',');
				return new RoomState(int.Parse(array[0]), (PlayerLoadState)Enum.Parse(typeof(PlayerLoadState), array[1]));
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return ((RoomState)value).PhotonID + "," + ((RoomState)value).LoadState;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	[TypeConverter(typeof(RoomStateConverter))]
	public class RoomState
	{
		private int m_photonID;

		private PlayerLoadState m_State;

		public uint m_StateStartTime;

		public bool m_Synced;

		private static int m_PrimaryLocalID = 1;

		public int PhotonID
		{
			get
			{
				return m_photonID;
			}
			set
			{
				m_photonID = value;
			}
		}

		public PlayerLoadState LoadState
		{
			get
			{
				return m_State;
			}
			set
			{
				m_StateStartTime = T17NetTimeManager.Instance.GetLocalTimeInMS();
				m_State = value;
			}
		}

		public RoomState(int netId, PlayerLoadState state)
		{
			m_photonID = netId;
			m_State = state;
			m_Synced = false;
		}

		public RoomState()
			: this(-1, PlayerLoadState.NotStarted)
		{
			m_Synced = false;
		}

		public void Reset()
		{
			m_photonID = -1;
			m_State = PlayerLoadState.NotStarted;
			m_Synced = false;
		}

		public bool IsLoadFinished()
		{
			return LoadFailed() || LoadSucceeded();
		}

		public bool LoadFailed()
		{
			return LoadState == PlayerLoadState.Failed_Disconnected || LoadState == PlayerLoadState.Failed_TimedOut;
		}

		public bool LoadSucceeded()
		{
			return LoadState == PlayerLoadState.Success;
		}

		public bool IsValidPlayer()
		{
			return PhotonID != -1;
		}

		public bool IsCurrentMasterClient()
		{
			return PhotonNetwork.masterClient != null && PhotonID == PhotonNetwork.masterClient.ID;
		}

		public bool IsPrimaryLocal()
		{
			return m_PrimaryLocalID > 0 && m_photonID == m_PrimaryLocalID;
		}

		public void SetIsPrimaryLocalRoom()
		{
			m_PrimaryLocalID = m_photonID;
		}
	}

	public delegate void OnSuccess_PlayerReadyToPlay(int iPhotonId);

	public delegate void OnLoadTimedOut(PlayerLoadState state);

	public delegate void OnLoadDisconnected();

	private RoomState[] m_netRoomPlayerLoadStateView = new RoomState[4];

	private bool m_updatePlayerStateCustomProperty;

	private bool m_LevelLoadActive;

	private uint m_levelLoadingStarted;

	private uint m_levelLoadingTimeout;

	private string m_tslevelLoadingStarted = string.Empty;

	private const uint m_maxLevelLoadingTimeout = 180000u;

	private static T17NetLoadSync _instance = null;

	public PlayerLoadState TimedOutState;

	private bool m_bTimedOut;

	public static bool m_bDisconnected = false;

	private static Dictionary<PlayerLoadState, uint> LoadStateTimeouts = new Dictionary<PlayerLoadState, uint>();

	private NetUserManager m_userManager;

	public string NetPlayerStateMapping
	{
		get
		{
			using StringWriter stringWriter = new StringWriter();
			RoomState[] netRoomPlayerLoadStateView = m_netRoomPlayerLoadStateView;
			foreach (RoomState roomState in netRoomPlayerLoadStateView)
			{
				stringWriter.WriteLine(TypeDescriptor.GetConverter(roomState).ConvertToString(roomState));
			}
			return stringWriter.ToString();
		}
		private set
		{
			RoomState[] array = (RoomState[])m_netRoomPlayerLoadStateView.Clone();
			using StringReader stringReader = new StringReader(value);
			int num = 0;
			string text = stringReader.ReadLine();
			while (!string.IsNullOrEmpty(text))
			{
				m_netRoomPlayerLoadStateView[num] = (RoomState)TypeDescriptor.GetConverter(typeof(RoomState)).ConvertFromString(text);
				if (m_netRoomPlayerLoadStateView[num].PhotonID != -1)
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].PhotonID != m_netRoomPlayerLoadStateView[num].PhotonID)
						{
							continue;
						}
						m_netRoomPlayerLoadStateView[num].m_StateStartTime = array[i].m_StateStartTime;
						m_netRoomPlayerLoadStateView[num].m_Synced = array[i].m_Synced;
						if (array[num].LoadState != PlayerLoadState.Success && m_netRoomPlayerLoadStateView[num].LoadState == PlayerLoadState.Success && T17NetLoadSync.c_OnPlayerReadyToPlay != null)
						{
							T17NetLoadSync.c_OnPlayerReadyToPlay(m_netRoomPlayerLoadStateView[num].PhotonID);
						}
						else if (m_netRoomPlayerLoadStateView[num].PhotonID == T17NetManager.PhotonPlayerID && array[num].LoadState != PlayerLoadState.Failed_TimedOut && m_netRoomPlayerLoadStateView[num].LoadState == PlayerLoadState.Failed_TimedOut)
						{
							TimedOutState = array[num].LoadState;
							if (T17NetLoadSync.c_OnTimedOut != null)
							{
								T17NetLoadSync.c_OnTimedOut(TimedOutState);
							}
						}
						break;
					}
				}
				num++;
				text = stringReader.ReadLine();
			}
		}
	}

	public static T17NetLoadSync Instance => _instance;

	public static uint LevelLoadingTime => 0u;

	public static bool TimedOut
	{
		get
		{
			bool flag = Instance.m_bTimedOut;
			if (!flag)
			{
				RoomState[] netRoomPlayerLoadStateView = Instance.m_netRoomPlayerLoadStateView;
				foreach (RoomState roomState in netRoomPlayerLoadStateView)
				{
					if (roomState.IsValidPlayer() && roomState.PhotonID == PhotonNetwork.player.ID && roomState.LoadState == PlayerLoadState.Failed_TimedOut)
					{
						flag = true;
					}
				}
			}
			return flag;
		}
	}

	public static event OnSuccess_PlayerReadyToPlay c_OnPlayerReadyToPlay;

	public static event OnLoadTimedOut c_OnTimedOut;

	public static event OnLoadDisconnected c_OnDisconnected;

	public PlayerLoadState GetLoadStateForPhotonId(int iPhotonId)
	{
		if (m_netRoomPlayerLoadStateView != null)
		{
			for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
			{
				RoomState roomState = m_netRoomPlayerLoadStateView[i];
				if (roomState != null && iPhotonId == roomState.PhotonID)
				{
					return roomState.LoadState;
				}
			}
		}
		return PlayerLoadState.NotStarted;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		_instance = this;
		PhotonNetwork.OnEventCall = (PhotonNetwork.EventCallback)Delegate.Combine(PhotonNetwork.OnEventCall, new PhotonNetwork.EventCallback(OnEvent));
	}

	private void OnEvent(byte eventcode, object content, int senderid)
	{
		T17NetConfig.NetEventTypes netEventTypes = (T17NetConfig.NetEventTypes)eventcode;
		if (netEventTypes == T17NetConfig.NetEventTypes.Load_NotStarted || netEventTypes == T17NetConfig.NetEventTypes.Load_LevelLoad_Done || netEventTypes == T17NetConfig.NetEventTypes.Load_LevelInit_InProgress || netEventTypes == T17NetConfig.NetEventTypes.Load_LevelInit_Done || netEventTypes == T17NetConfig.NetEventTypes.Load_Inventory_InProgress || netEventTypes == T17NetConfig.NetEventTypes.Load_Inventory_Done || netEventTypes == T17NetConfig.NetEventTypes.Load_Spawn_InProgress || netEventTypes == T17NetConfig.NetEventTypes.Load_Spawn_Done || netEventTypes == T17NetConfig.NetEventTypes.Load_Managers_InProgress || netEventTypes == T17NetConfig.NetEventTypes.Load_ReadyToPlay)
		{
			HandleEvent((T17NetConfig.NetEventTypes)eventcode, (int)content);
		}
	}

	private void HandleEvent(T17NetConfig.NetEventTypes eventType, int netPlayerId)
	{
		switch (eventType)
		{
		case T17NetConfig.NetEventTypes.Load_NotStarted:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Success, PlayerLoadState.NotStarted);
			break;
		case T17NetConfig.NetEventTypes.Load_LevelLoad_Done:
			EventReceive(netPlayerId, eventType, PlayerLoadState.NotStarted, PlayerLoadState.LevelLoad_Done);
			break;
		case T17NetConfig.NetEventTypes.Load_LevelInit_InProgress:
			EventReceive(netPlayerId, eventType, PlayerLoadState.LevelLoad_Done, PlayerLoadState.LevelInit_InProgress);
			break;
		case T17NetConfig.NetEventTypes.Load_LevelInit_Done:
			EventReceive(netPlayerId, eventType, PlayerLoadState.LevelInit_InProgress, PlayerLoadState.LevelInit_Done);
			break;
		case T17NetConfig.NetEventTypes.Load_Inventory_InProgress:
			EventReceive(netPlayerId, eventType, PlayerLoadState.LevelInit_Done, PlayerLoadState.Inventory_InProgress);
			break;
		case T17NetConfig.NetEventTypes.Load_Inventory_Done:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Inventory_InProgress, PlayerLoadState.Inventory_Done);
			break;
		case T17NetConfig.NetEventTypes.Load_Spawn_InProgress:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Inventory_Done, PlayerLoadState.Spawn_InProgress);
			break;
		case T17NetConfig.NetEventTypes.Load_Spawn_Done:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Spawn_InProgress, PlayerLoadState.Spawn_Done);
			break;
		case T17NetConfig.NetEventTypes.Load_Managers_InProgress:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Spawn_Done, PlayerLoadState.Managers_InProgress);
			break;
		case T17NetConfig.NetEventTypes.Load_ReadyToPlay:
			EventReceive(netPlayerId, eventType, PlayerLoadState.Managers_InProgress, PlayerLoadState.Success);
			break;
		}
	}

	private void Start()
	{
		if (m_userManager == null)
		{
			m_userManager = NetUserManager.Instance;
		}
		for (int i = 0; i < 4; i++)
		{
			m_netRoomPlayerLoadStateView[i] = new RoomState();
		}
		LoadStateTimeouts[PlayerLoadState.NotStarted] = 0u;
		LoadStateTimeouts[PlayerLoadState.LevelLoad_Done] = 0u;
		LoadStateTimeouts[PlayerLoadState.LevelInit_InProgress] = 70000u;
		LoadStateTimeouts[PlayerLoadState.LevelInit_Done] = 0u;
		LoadStateTimeouts[PlayerLoadState.Inventory_InProgress] = 70000u;
		LoadStateTimeouts[PlayerLoadState.Inventory_Done] = 0u;
		LoadStateTimeouts[PlayerLoadState.Spawn_InProgress] = 35000u;
		LoadStateTimeouts[PlayerLoadState.Spawn_Done] = 0u;
		LoadStateTimeouts[PlayerLoadState.Managers_InProgress] = 350000u;
		LoadStateTimeouts[PlayerLoadState.Success] = 0u;
		LoadStateTimeouts[PlayerLoadState.Failed_Disconnected] = 0u;
		LoadStateTimeouts[PlayerLoadState.Failed_TimedOut] = 0u;
		Gamer.OnCreate += OnGamerCreated;
		Gamer.OnDeleted += OnGamerDeleted;
		Gamer.OnUpdated += OnGamerUpdated;
	}

	protected void OnDestroy()
	{
		Gamer.OnCreate -= OnGamerCreated;
		Gamer.OnDeleted -= OnGamerDeleted;
		Gamer.OnUpdated -= OnGamerUpdated;
	}

	public void OnGamerDeleted()
	{
		UpdateStatesFromGamers();
	}

	public void OnGamerCreated(Gamer gamer)
	{
		UpdateStatesFromGamers();
	}

	public void OnGamerUpdated(Gamer gamer)
	{
		UpdateStatesFromGamers();
	}

	public void Update()
	{
		if (m_updatePlayerStateCustomProperty)
		{
			if (T17NetManager.OfflineMode || T17NetManager.IsMasterClient)
			{
				T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.LoadStates, NetPlayerStateMapping.ToString());
			}
			m_updatePlayerStateCustomProperty = false;
		}
		if (!(T17NetTimeManager.Instance != null))
		{
			return;
		}
		if (m_LevelLoadActive)
		{
			uint num = T17NetTimeManager.Instance.GetLocalTimeInMS() - m_levelLoadingStarted;
			if (num > m_levelLoadingTimeout)
			{
				TimeoutTriggered();
				m_LevelLoadActive = false;
			}
		}
		if (!T17NetManager.IsMasterClient || !T17NetManager.IsConnectedOnline())
		{
			return;
		}
		RoomState[] netRoomPlayerLoadStateView = m_netRoomPlayerLoadStateView;
		foreach (RoomState roomState in netRoomPlayerLoadStateView)
		{
			uint num2 = LoadStateTimeouts[roomState.LoadState];
			if (num2 != 0)
			{
				uint num3 = T17NetTimeManager.Instance.GetLocalTimeInMS() - roomState.m_StateStartTime;
				if (num3 > num2)
				{
					Debug.Log("Client timed out! " + roomState.PhotonID + ", State: " + roomState.LoadState);
					roomState.LoadState = PlayerLoadState.Failed_TimedOut;
					m_updatePlayerStateCustomProperty = true;
				}
			}
		}
	}

	public void Reset()
	{
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			m_netRoomPlayerLoadStateView[i]?.Reset();
		}
	}

	public void OnLoadStatesChanged(string strValue)
	{
		if (!T17NetManager.IsMasterClient)
		{
			NetPlayerStateMapping = strValue;
		}
	}

	public RoomState GetRoomStateByPhotonID(int photonID)
	{
		for (int num = m_netRoomPlayerLoadStateView.Length - 1; num >= 0; num--)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[num];
			if (roomState.PhotonID == photonID)
			{
				return roomState;
			}
		}
		return null;
	}

	public RoomState GetEmptyRoomState()
	{
		for (int num = m_netRoomPlayerLoadStateView.Length - 1; num >= 0; num--)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[num];
			if (!roomState.IsValidPlayer())
			{
				return roomState;
			}
		}
		return null;
	}

	public void UpdateStatesFromGamers()
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		bool flag = true;
		for (int num = m_netRoomPlayerLoadStateView.Length - 1; num >= 0; num--)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[num];
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null && roomState.IsPrimaryLocal() && primaryGamer.m_PhotonID != roomState.PhotonID)
			{
				roomState.PhotonID = primaryGamer.m_PhotonID;
				roomState.SetIsPrimaryLocalRoom();
			}
			for (int num2 = 3; num2 >= 0; num2--)
			{
				flag = true;
				Gamer gamer = allGamers[num2];
				if (gamer != null && gamer.m_PhotonID == roomState.PhotonID)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (roomState == null || DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
				{
				}
				roomState.Reset();
				m_updatePlayerStateCustomProperty = true;
			}
		}
		for (int num3 = allGamers.Length - 1; num3 >= 0; num3--)
		{
			Gamer gamer = allGamers[num3];
			if (gamer != null)
			{
				RoomState roomState2 = GetRoomStateByPhotonID(gamer.m_PhotonID);
				if (roomState2 == null)
				{
					roomState2 = GetEmptyRoomState();
					if (roomState2 != null)
					{
						roomState2.PhotonID = gamer.m_PhotonID;
						m_updatePlayerStateCustomProperty = true;
					}
				}
				if (roomState2 != null && !DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
				{
				}
			}
		}
	}

	public void TimeoutTriggered()
	{
		m_bTimedOut = true;
		StopLevelLoad();
	}

	public void EventSend(T17NetConfig.NetEventTypes eventType)
	{
		PhotonPlayer player = PhotonNetwork.player;
		if (T17NetManager.ConnectionDetailed == T17NetPeerState.Joined)
		{
			if (T17NetManager.IsMasterClient)
			{
				HandleEvent(eventType, player.ID);
				return;
			}
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.Receivers = ReceiverGroup.MasterClient;
			raiseEventOptions.Encrypt = true;
			PhotonNetwork.RaiseEvent((byte)eventType, player.ID, sendReliable: true, raiseEventOptions);
			T17NetManager.CriticalMessageNeedToSendNow();
		}
		else if (player.ID == -1)
		{
			if (eventType != T17NetConfig.NetEventTypes.Load_NotStarted)
			{
			}
			Reset();
		}
	}

	public void EventReceive(int netPlayerId, T17NetConfig.NetEventTypes eventType, PlayerLoadState stateFrom, PlayerLoadState stateTo)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[i];
			if (roomState == null || roomState.PhotonID != netPlayerId)
			{
				continue;
			}
			if (roomState.LoadState == stateFrom)
			{
				roomState.LoadState = stateTo;
				if (stateTo == PlayerLoadState.Success && T17NetLoadSync.c_OnPlayerReadyToPlay != null)
				{
					T17NetLoadSync.c_OnPlayerReadyToPlay(netPlayerId);
				}
				m_updatePlayerStateCustomProperty = true;
			}
			else if (stateTo == roomState.LoadState)
			{
			}
			if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
			{
			}
		}
	}

	public bool AreAllClientsInState(List<PlayerLoadState> states)
	{
		bool flag = true;
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState netPlayerState = m_netRoomPlayerLoadStateView[i];
			if (netPlayerState != null && netPlayerState.m_Synced && netPlayerState.IsValidPlayer() && !netPlayerState.LoadFailed() && !states.Exists((PlayerLoadState x) => x == netPlayerState.LoadState))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
			{
				uint num = T17NetTimeManager.Instance.GetLocalTimeInMS() - m_levelLoadingStarted;
			}
			return true;
		}
		return flag;
	}

	public bool AreAllClientsInState(PlayerLoadState state)
	{
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[i];
			if (roomState != null && roomState.m_Synced && roomState.IsValidPlayer() && !roomState.LoadFailed() && roomState.LoadState != state)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsMasterClientInState(PlayerLoadState state)
	{
		bool flag = true;
		RoomState[] netRoomPlayerLoadStateView = m_netRoomPlayerLoadStateView;
		foreach (RoomState roomState in netRoomPlayerLoadStateView)
		{
			if (roomState.IsValidPlayer() && roomState.IsCurrentMasterClient())
			{
				return roomState.LoadState == state || roomState.LoadFailed();
			}
		}
		if (flag)
		{
			if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLevelLoadState))
			{
				uint num = T17NetTimeManager.Instance.GetLocalTimeInMS() - m_levelLoadingStarted;
			}
			return true;
		}
		return flag;
	}

	public void StartLevelLoad()
	{
		UpdateStatesFromGamers();
		for (int i = 0; i < 4; i++)
		{
			if (m_netRoomPlayerLoadStateView[i].IsValidPlayer())
			{
				m_netRoomPlayerLoadStateView[i].m_Synced = true;
			}
			else
			{
				m_netRoomPlayerLoadStateView[i].m_Synced = false;
			}
			m_netRoomPlayerLoadStateView[i].m_StateStartTime = T17NetTimeManager.Instance.GetLocalTimeInMS();
		}
		m_bTimedOut = false;
		m_bDisconnected = false;
		m_LevelLoadActive = true;
		m_levelLoadingStarted = T17NetTimeManager.Instance.GetLocalTimeInMS();
		m_levelLoadingTimeout = 180000u;
		m_tslevelLoadingStarted = T17NetTimeManager.Instance.GetLocalMachineTimeStamp();
	}

	public void StopLevelLoad()
	{
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null)
		{
			for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
			{
				if (m_netRoomPlayerLoadStateView[i].PhotonID == primaryGamer.m_PhotonID)
				{
					m_netRoomPlayerLoadStateView[i].SetIsPrimaryLocalRoom();
					break;
				}
			}
		}
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetLogLevelLoadState))
		{
		}
		m_LevelLoadActive = false;
		m_levelLoadingTimeout = 0u;
	}

	public virtual void OnLeftRoom()
	{
		if (T17NetManager.NetOnlineMode && m_LevelLoadActive)
		{
			m_bDisconnected = true;
			if (T17NetLoadSync.c_OnDisconnected != null)
			{
				T17NetLoadSync.c_OnDisconnected();
			}
		}
	}

	public virtual void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		if (m_LevelLoadActive)
		{
			m_bDisconnected = true;
			if (T17NetLoadSync.c_OnDisconnected != null)
			{
				T17NetLoadSync.c_OnDisconnected();
			}
		}
	}

	public bool AnyClientsReadyForClientSerialization()
	{
		int iD = PhotonNetwork.player.ID;
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[i];
			if (roomState != null && roomState.PhotonID != iD)
			{
				PlayerLoadState loadState = roomState.LoadState;
				if (loadState != 0 && loadState != PlayerLoadState.LevelInit_Done && loadState != PlayerLoadState.LevelInit_InProgress && loadState != PlayerLoadState.LevelInit_Done)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool GetClientsReadyForGameplayRPC(ref List<int> clientsNotReady)
	{
		clientsNotReady.Clear();
		bool result = true;
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[i];
			if (roomState != null && roomState.IsValidPlayer() && !roomState.LoadSucceeded())
			{
				clientsNotReady.Add(roomState.PhotonID);
				result = false;
			}
		}
		return result;
	}

	public bool GetClientsPastLoadedLevel(ref List<int> clientsNotReady)
	{
		clientsNotReady.Clear();
		bool result = true;
		for (int i = 0; i < m_netRoomPlayerLoadStateView.Length; i++)
		{
			RoomState roomState = m_netRoomPlayerLoadStateView[i];
			if (roomState != null && roomState.IsValidPlayer() && roomState.LoadState < PlayerLoadState.LevelLoad_Done)
			{
				clientsNotReady.Add(roomState.PhotonID);
				result = false;
			}
		}
		return result;
	}
}
