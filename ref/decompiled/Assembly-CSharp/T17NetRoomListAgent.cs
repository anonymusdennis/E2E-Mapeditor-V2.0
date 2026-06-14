public class T17NetRoomListAgent : T17NetConnectAgent
{
	private string m_strLobbyName;

	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady && !PhotonNetwork.insideLobby)
		{
			PhotonNetwork.JoinLobby();
		}
		return true;
	}

	public void SetLobby(string strLobbyName)
	{
		m_strLobbyName = strLobbyName;
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		TypedLobby typedLobby = new TypedLobby(m_strLobbyName, LobbyType.SqlLobby);
		PhotonNetwork.JoinLobby(typedLobby);
	}
}
