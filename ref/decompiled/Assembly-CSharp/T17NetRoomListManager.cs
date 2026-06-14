using System;
using System.Collections.Generic;

public class T17NetRoomListManager : T17NetSendMonoMessageTarget
{
	public class NetPhotonRoom
	{
		public string Name;

		public int NumPlayers;

		public int MaxPlayers;

		public bool Visible;

		public bool Open;

		public int RoomGameState;

		public int TierLevel;

		public string Plaform;

		public int RoomDays;

		public LevelScript.PRISON_ENUM PrisonEnum;

		public T17NetRoomGameView.GameRoomType RoomType;

		public string RoomPassword = string.Empty;

		public int MapRotationIndex;

		public string PlaylistId;

		public string DisplayName;

		public string HostName;

		public bool IsFull => NumPlayers >= MaxPlayers;

		public NetPhotonRoom(RoomInfo roomInfo)
		{
			Name = roomInfo.Name;
			NumPlayers = roomInfo.PlayerCount;
			MaxPlayers = roomInfo.MaxPlayers;
			Visible = roomInfo.IsVisible;
			Open = roomInfo.IsOpen;
			T17NetRoomGameView.GetCustomPropertyAsInt(T17NetRoomGameView.CustomProperty.GamerCount, ref NumPlayers, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.RoomPlatformType, ref Plaform, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsInt(T17NetRoomGameView.CustomProperty.GameState, ref RoomGameState, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsInt(T17NetRoomGameView.CustomProperty.PrisonDay, ref RoomDays, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.PrisonEnum, ref PrisonEnum, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref RoomType, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.PlaylistId, ref PlaylistId, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.DisplayName, ref DisplayName, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.HostName, ref HostName, roomInfo.CustomProperties);
			T17NetRoomGameView.GetCustomPropertyAsString(T17NetRoomGameView.CustomProperty.Password, ref RoomPassword, roomInfo.CustomProperties);
		}
	}

	private static T17NetRoomListManager _instance;

	private bool m_logScreenLogActive;

	public static T17NetRoomListManager Instance => _instance;

	public bool LogScreenActive
	{
		get
		{
			return m_logScreenLogActive;
		}
		set
		{
			m_logScreenLogActive = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (_instance != null)
		{
			throw new Exception("You cannot have multiple managers");
		}
		_instance = this;
	}

	public GlobalStart.GLOBALSTART_MODE GetRoomState()
	{
		int outValue = 0;
		if (T17NetRoomGameView.GetCustomPropertyAsInt(T17NetRoomGameView.CustomProperty.GameState, ref outValue))
		{
			return (GlobalStart.GLOBALSTART_MODE)outValue;
		}
		return GlobalStart.GLOBALSTART_MODE.INIT;
	}

	public List<NetPhotonRoom> GetLobbyRoomList()
	{
		List<NetPhotonRoom> list = new List<NetPhotonRoom>();
		RoomInfo[] roomList = PhotonNetwork.GetRoomList();
		for (int i = 0; i < roomList.Length; i++)
		{
			NetPhotonRoom item = new NetPhotonRoom(roomList[i]);
			list.Add(item);
		}
		return list;
	}

	public void OnReceivedRoomList()
	{
		if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonEvent))
		{
		}
	}

	public void OnReceivedRoomListUpdate()
	{
		if (!DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonEvent))
		{
		}
	}
}
