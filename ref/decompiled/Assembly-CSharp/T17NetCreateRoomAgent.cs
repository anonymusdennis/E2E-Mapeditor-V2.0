using ExitGames.Client.Photon;
using UnityEngine;

public class T17NetCreateRoomAgent : T17NetConnectAgent
{
	private static string m_CreatedRoomName;

	private static string m_LobbyName;

	private static string m_RoomName;

	private static string[] m_CustomPropertiesForLobby;

	private static Hashtable m_RoomPropertyValues;

	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			CreateAndJoinRoom();
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	public void SetGameParameters(string roomName, string lobbyName, string[] customPropertiesForLobby, Hashtable roomPropertyValues)
	{
		m_RoomName = roomName;
		m_LobbyName = lobbyName;
		m_CustomPropertiesForLobby = customPropertiesForLobby;
		m_RoomPropertyValues = roomPropertyValues;
	}

	private string CreateAndJoinRoom()
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 4;
		roomOptions.CustomRoomPropertiesForLobby = m_CustomPropertiesForLobby;
		roomOptions.CustomRoomProperties = m_RoomPropertyValues;
		roomOptions.PublishUserId = true;
		T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Offline;
		if (m_RoomPropertyValues != null && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue, m_RoomPropertyValues))
		{
			switch (outValue)
			{
			case T17NetRoomGameView.GameRoomType.Public:
				roomOptions.IsOpen = true;
				roomOptions.IsVisible = true;
				break;
			case T17NetRoomGameView.GameRoomType.Private:
				roomOptions.IsOpen = true;
				roomOptions.IsVisible = false;
				break;
			}
		}
		TypedLobby typedLobby = new TypedLobby(m_LobbyName, LobbyType.SqlLobby);
		bool flag = PhotonNetwork.CreateRoom(m_RoomName, roomOptions, typedLobby);
		if (!flag || T17NetConfig.NetForceCreateJoinError)
		{
			Debug.LogWarningFormat("CreateAndJoinRoom: Room Created Failed !!! - {0}", m_RoomName);
		}
		else if (flag)
		{
			Debug.LogFormat("CreateAndJoinRoom: Room Created. !!! - {0}", m_RoomName);
		}
		T17NetManager.AutomaticallySyncScene = false;
		m_CreatedRoomName = m_RoomName;
		return m_RoomName;
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		CreateAndJoinRoom();
	}

	public override void OnCreateRoomFailed()
	{
		base.OnCreateRoomFailed();
		CreateAndJoinRoom();
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		if (string.IsNullOrEmpty(m_CreatedRoomName) || PhotonNetwork.room == null || !(m_CreatedRoomName == PhotonNetwork.room.Name))
		{
			return;
		}
		T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
		if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue))
		{
			T17NetRoomManager instance = T17NetRoomManager.Instance;
			if (instance != null)
			{
				instance.SetPropertiesForGameroomType(outValue);
			}
		}
	}
}
