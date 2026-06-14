public class T17NetOnlineModeIdleAgent : T17NetConnectAgent
{
	public override bool Start()
	{
		base.Start();
		return true;
	}

	protected override void Connect()
	{
		if (PhotonNetwork.inRoom || PhotonNetwork.room != null)
		{
			PhotonNetwork.LeaveRoom();
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		base.Connect();
	}
}
