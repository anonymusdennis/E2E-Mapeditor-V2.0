public class T17NetJoinSpecificRoomAgent : T17NetConnectAgent
{
	private static string m_strRoomName;

	public void SetRoomName(string strRoomName)
	{
		m_strRoomName = strRoomName;
	}

	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.inRoom && PhotonNetwork.room != null && PhotonNetwork.room.Name != m_strRoomName)
		{
			PhotonNetwork.LeaveRoom();
		}
		else if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			return AttemptJoinRoom(m_strRoomName);
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	private static bool AttemptJoinRoom(string roomName)
	{
		bool flag = false;
		if (roomName.Equals(string.Empty) || roomName == null)
		{
			return false;
		}
		return PhotonNetwork.JoinRoom(roomName);
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		Connect();
		AttemptJoinRoom(m_strRoomName);
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		Connect();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			AttemptJoinRoom(m_strRoomName);
		}
	}
}
