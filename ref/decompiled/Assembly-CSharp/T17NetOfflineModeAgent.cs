public class T17NetOfflineModeAgent : T17NetAgentBase
{
	public virtual bool Start()
	{
		if (T17NetManager.NetOnlineMode)
		{
			bool connected = PhotonNetwork.connected;
			T17NetEncryptionKeys.Instance.ClearKeys();
			PhotonNetwork.Disconnect();
			if (!connected)
			{
				PhotonNetwork.offlineMode = true;
				PhotonNetwork.CreateRoom("offline room");
			}
		}
		else if (!PhotonNetwork.inRoom)
		{
			PhotonNetwork.CreateRoom("offline room");
		}
		if (PhotonNetwork.insideLobby)
		{
			PhotonNetwork.LeaveLobby();
		}
		return true;
	}

	public virtual void OnDisconnected()
	{
		PhotonNetwork.offlineMode = true;
		if (PhotonNetwork.inRoom)
		{
			PhotonNetwork.LeaveRoom();
		}
		if (T17NetManager.NetOfflineMode && !PhotonNetwork.inRoom)
		{
			PhotonNetwork.CreateRoom("offline room");
		}
	}

	public virtual void OnConnectedToMaster()
	{
	}

	public virtual void OnJoinedRoom()
	{
	}

	public virtual void OnLeftRoom()
	{
		if (T17NetManager.NetOfflineMode && !PhotonNetwork.inRoom)
		{
			PhotonNetwork.CreateRoom("offline room");
		}
	}

	public virtual void OnCreateRoomFailed()
	{
	}

	public virtual void OnJoinedRoomFailed()
	{
	}

	public virtual void OnPlatformReadyToConnect()
	{
	}

	public virtual void Update()
	{
	}
}
