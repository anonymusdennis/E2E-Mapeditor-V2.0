using System.Collections.Generic;

public class NetLoadManagerSync : T17NetworkBehaviour
{
	public enum LOADSTATE
	{
		NotStarted,
		Requested,
		RequestConfirmed
	}

	public static List<INetworkLoadable> m_AllNetworkLoadables = new List<INetworkLoadable>();

	private T17NetView m_NetView;

	public static NetLoadManagerSync Instance = null;

	public LOADSTATE m_eLoadState;

	protected override void Awake()
	{
		m_NetView = GetComponent<T17NetView>();
		Instance = this;
		base.Awake();
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
	}

	public void RequestLoadDataRPC()
	{
		if (null != m_NetView)
		{
			m_eLoadState = LOADSTATE.Requested;
			m_NetView.RPCQuestion("RPC_RequestLoadStates", NetTargets.MasterClient, PhotonNetwork.player.ID);
		}
	}

	[PunRPC]
	public void RPC_RequestLoadStates(int RPCID, int playerID, PhotonMessageInfo info)
	{
		PhotonPlayer player = PhotonPlayer.Find(playerID);
		int count = m_AllNetworkLoadables.Count;
		for (int i = 0; i < count; i++)
		{
			INetworkLoadable networkLoadable = m_AllNetworkLoadables[i];
			networkLoadable.SendLoadDataToClientRPC(player);
			T17NetManager.Service();
		}
		m_NetView.RPCResponse("RPC_RequestConfirmed", RPCID);
		T17NetManager.Service();
	}

	[PunRPC]
	public void RPC_RequestConfirmed(PhotonMessageInfo info)
	{
		m_eLoadState = LOADSTATE.RequestConfirmed;
	}
}
