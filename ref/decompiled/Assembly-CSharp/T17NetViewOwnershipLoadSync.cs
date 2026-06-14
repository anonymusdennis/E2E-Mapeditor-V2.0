using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;

internal class T17NetViewOwnershipLoadSync : MonoBehaviour, INetworkLoadable
{
	private T17NetView m_NetView;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	private void Awake()
	{
		m_NetView = GetComponent<T17NetView>();
	}

	private void Start()
	{
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		m_NetView = null;
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<Player> allPlayers = Player.GetAllPlayers();
		int count = allPlayers.Count;
		for (int i = 0; i < count; i++)
		{
			Player player2 = allPlayers[i];
			if (player2.m_NetView.ownerId != 0)
			{
				list.Add(player2.m_NetView.viewID);
				list2.Add(player2.m_NetView.ownerId);
			}
		}
		m_NetView.RPC("RPC_RequestStateResponce_NetViewOwners", player, list.ToArray(), list2.ToArray());
	}

	[PunRPC]
	private void RPC_RequestStateResponce_NetViewOwners(int[] allocatedViewIds, int[] allocatedOwnerIds, PhotonMessageInfo info)
	{
		int num = allocatedViewIds.Length;
		for (int i = 0; i < num; i++)
		{
			T17NetView t17NetView = T17NetView.Find<T17NetView>(allocatedViewIds[i]);
			if (null != t17NetView)
			{
				t17NetView.ownerId = allocatedOwnerIds[i];
			}
		}
		m_LoadState = LOADSTATE.Finished_OK;
	}
}
