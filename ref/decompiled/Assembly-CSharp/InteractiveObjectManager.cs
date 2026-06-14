using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;

public class InteractiveObjectManager : MonoBehaviour, INetworkLoadable
{
	private static InteractiveObjectManager ms_Instance;

	private List<InteractiveObject> m_InteractiveObjects = new List<InteractiveObject>();

	private T17NetView m_NetView;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	private List<NetObjectLock> m_SyncedObjects = new List<NetObjectLock>();

	public static InteractiveObjectManager GetInstance()
	{
		return ms_Instance;
	}

	public void Awake()
	{
		if (ms_Instance == null)
		{
			ms_Instance = this;
			T17NetRoomGameView.OnRoomSignalEvent += OnEvent;
			NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
			if (m_NetView == null)
			{
				m_NetView = GetComponent<T17NetView>();
			}
		}
	}

	public void AddInteracteractiveObject(InteractiveObject interactiveObject)
	{
		if (interactiveObject != null && m_InteractiveObjects != null)
		{
			m_InteractiveObjects.Add(interactiveObject);
			m_InteractiveObjects.Sort((InteractiveObject a, InteractiveObject b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
		}
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
		m_InteractiveObjects.Clear();
		T17NetRoomGameView.OnRoomSignalEvent -= OnEvent;
		if (NetLoadManagerSync.m_AllNetworkLoadables.Contains(this))
		{
			NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		}
		ms_Instance = null;
	}

	public void OnEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal != T17NetConfig.NetEventTypes.InteractiveObjectEvent || !(payload is object[] array))
		{
			return;
		}
		int num = (int)array[0];
		if (num >= 0 && num < m_InteractiveObjects.Count)
		{
			InteractiveObject.InteractiveEventType interactiveEventType = (InteractiveObject.InteractiveEventType)array[1];
			short num2 = (short)array[2];
			Character interactingCharacter = null;
			FastList<Character> sortedCharacterList = CharacterSerializer.GetInstance().GetSortedCharacterList();
			if (sortedCharacterList != null && num2 >= 0 && num2 < sortedCharacterList.Count)
			{
				interactingCharacter = sortedCharacterList[num2];
			}
			switch (interactiveEventType)
			{
			case InteractiveObject.InteractiveEventType.InteractionReadyStart:
				m_InteractiveObjects[num].InteractionReadyStartEvent(interactingCharacter);
				break;
			case InteractiveObject.InteractiveEventType.InteractionStarted:
				m_InteractiveObjects[num].InteractionStartedEvent(interactingCharacter);
				break;
			case InteractiveObject.InteractiveEventType.InteractionReadyEnd:
				m_InteractiveObjects[num].InteractionReadyEndEvent(interactingCharacter);
				break;
			case InteractiveObject.InteractiveEventType.InteractionEnded:
				m_InteractiveObjects[num].InteractionEndedEvent(interactingCharacter);
				break;
			}
		}
	}

	public void SendEvent(InteractiveObject interactiveObject, InteractiveObject.InteractiveEventType interactionType, short characterIndex)
	{
		for (int num = m_InteractiveObjects.Count - 1; num >= 0; num--)
		{
			if (m_InteractiveObjects[num] == interactiveObject)
			{
				T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.InteractiveObjectEvent, T17NetConfig.NetSequenceChannel.CombatAndInteractoins, new object[3]
				{
					num,
					(byte)interactionType,
					characterIndex
				});
				break;
			}
		}
	}

	public int GetInteractiveObjectsListSize()
	{
		if (m_InteractiveObjects == null)
		{
			return -1;
		}
		return m_InteractiveObjects.Count;
	}

	public NetObjectLock GetInteractiveObject(int index)
	{
		if (m_InteractiveObjects == null || index < 0 || index >= m_InteractiveObjects.Count)
		{
			return null;
		}
		return m_InteractiveObjects[index].m_NetObjectLock;
	}

	public void OnNetObjectLockSync(NetObjectLock netObjectLock)
	{
		if (netObjectLock.IsPlayerLocked())
		{
			if (!m_SyncedObjects.Contains(netObjectLock))
			{
				m_SyncedObjects.Add(netObjectLock);
			}
		}
		else if (m_SyncedObjects.Contains(netObjectLock))
		{
			m_SyncedObjects.Remove(netObjectLock);
		}
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
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			for (int i = 0; i < m_SyncedObjects.Count; i++)
			{
				if (m_SyncedObjects[i] != null)
				{
					m_SyncedObjects[i].SyncStateTo(player);
				}
			}
			m_NetView.RPC("RPC_RequestStateResponce_Yes_InteractiveObjectManager", player);
		}
		else
		{
			m_NetView.RPC("RPC_RequestStateResponce_No_InteractiveObjectManager", player);
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_InteractiveObjectManager(PhotonMessageInfo info)
	{
		m_LoadState = LOADSTATE.Finished_OK;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_InteractiveObjectManager(PhotonMessageInfo info)
	{
		m_LoadError = "InteractiveObjectManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}
}
