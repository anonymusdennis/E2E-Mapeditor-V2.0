public class T17NetRoomListSearchAgent : T17NetConnectAgent
{
	private string m_strLobbyName;

	private string m_strSQLGameSearch;

	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		else if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			SearchForRooms();
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	public void SetSearchParameters(string lobbyName, string strSQLGameSearch)
	{
		m_strLobbyName = lobbyName;
		m_strSQLGameSearch = strSQLGameSearch;
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		SearchForRooms();
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		Connect();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			SearchForRooms();
		}
	}

	public override void OnJoinedRoomFailed()
	{
		SearchForRooms();
	}

	private void SearchForRooms()
	{
		TypedLobby typedLobby = new TypedLobby(m_strLobbyName, LobbyType.SqlLobby);
		PhotonNetwork.GetCustomRoomList(typedLobby, m_strSQLGameSearch);
	}
}
