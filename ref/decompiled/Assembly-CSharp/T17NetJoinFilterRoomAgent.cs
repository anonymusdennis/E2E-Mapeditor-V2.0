public class T17NetJoinFilterRoomAgent : T17NetConnectAgent
{
	private string m_strLobbyName;

	private string m_strSQLGameSearch;

	private byte m_iMaxPlayers;

	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		else if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			JoinRandomRoom();
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	public void SetSearchParameters(string lobbyName, string strSQLGameSearch, byte iMaxPlayers)
	{
		m_strLobbyName = lobbyName;
		m_strSQLGameSearch = strSQLGameSearch;
		m_iMaxPlayers = iMaxPlayers;
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		JoinRandomRoom();
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		Connect();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			JoinRandomRoom();
		}
	}

	public override void OnJoinedRoomFailed()
	{
		JoinRandomRoom();
	}

	private void JoinRandomRoom()
	{
		TypedLobby typedLobby = new TypedLobby(m_strLobbyName, LobbyType.SqlLobby);
		PhotonNetwork.JoinRandomRoom(null, m_iMaxPlayers, MatchmakingMode.FillRoom, typedLobby, m_strSQLGameSearch);
	}
}
