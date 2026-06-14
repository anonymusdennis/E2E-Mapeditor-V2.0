public interface T17NetAgentBase
{
	bool Start();

	void OnDisconnected();

	void OnConnectedToMaster();

	void OnJoinedRoom();

	void OnLeftRoom();

	void OnCreateRoomFailed();

	void OnJoinedRoomFailed();

	void OnPlatformReadyToConnect();

	void Update();
}
