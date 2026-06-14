using NetworkLoadable;

public interface INetworkLoadable
{
	void ResetLoadState();

	LOADSTATE GetLoadState();

	string GetLoadError();

	void SendLoadDataToClientRPC(PhotonPlayer player);
}
