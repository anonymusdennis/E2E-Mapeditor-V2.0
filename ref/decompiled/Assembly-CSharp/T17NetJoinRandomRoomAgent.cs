public class T17NetJoinRandomRoomAgent : T17NetConnectAgent
{
	public override bool Start()
	{
		base.Start();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		else if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			PhotonNetwork.JoinRandomRoom();
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	public override void OnConnectedToMaster()
	{
		base.OnConnectedToMaster();
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnLeftRoom()
	{
		base.OnLeftRoom();
		Connect();
		if (T17NetManager.NetOnlineMode && PhotonNetwork.connectedAndReady)
		{
			PhotonNetwork.JoinRandomRoom();
		}
	}
}
