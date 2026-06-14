using UnityEngine;

public class PeerLatency : T17MonoBehaviour, IControlledUpdate
{
	private const int PeerCount = 3;

	private const float SendFrequency = 0.2f;

	private const byte SendFlag = 1;

	private const byte RecvFlag = 2;

	private const float PastWeightValue = 25f;

	private static float[] ms_Latencies = new float[3];

	private static int[] ms_PeerID = new int[3];

	private int[] m_SendCount = new int[3];

	private float[] m_LastSent = new float[3];

	private int m_PeerCount;

	public void Start()
	{
		T17NetRoomGameView.OnRoomSignalEvent += OnEvent;
		T17NetManager.OnPhotonPlayerConnectedEvent += OnPhotonPlayerChange;
		T17NetManager.OnPhotonPlayerDisconnectedEvent += OnPhotonPlayerChange;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Register(this, UpdateCategory.RapidPeriodic);
		}
		ResetPeers();
	}

	protected virtual void OnDestroy()
	{
		T17NetRoomGameView.OnRoomSignalEvent -= OnEvent;
		T17NetManager.OnPhotonPlayerConnectedEvent -= OnPhotonPlayerChange;
		T17NetManager.OnPhotonPlayerDisconnectedEvent -= OnPhotonPlayerChange;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.RapidPeriodic);
		}
	}

	public static bool GetPeerLatency(int peerID, ref float fLatency)
	{
		if (peerID == 0)
		{
			peerID = T17NetManager.MasterClientID;
		}
		for (int num = 2; num >= 0; num--)
		{
			if (ms_PeerID[num] == peerID)
			{
				fLatency = ms_Latencies[num];
				return true;
			}
		}
		return false;
	}

	public static string GetLatencyString()
	{
		string text = string.Empty;
		for (int num = 2; num >= 0; num--)
		{
			if (ms_PeerID[num] != -1)
			{
				text = text + ms_Latencies[num] + " ";
			}
		}
		return text;
	}

	public void ControlledUpdate()
	{
		if (m_PeerCount < 1)
		{
			return;
		}
		float time = UpdateManager.time;
		for (int num = 2; num >= 0; num--)
		{
			if (NetLoadSync.GetLoadStateForPhotonId(ms_PeerID[num]) == PlayerLoadState.Success && ms_PeerID[num] != -1 && m_LastSent[num] < time)
			{
				SendLatencyCheck(num);
				m_LastSent[num] = time + 0.2f;
				PhotonNetwork.networkingPeer.SendOutgoingCommands();
				break;
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	private void SendLatencyCheck(int index)
	{
		int[] photonID = new int[1] { ms_PeerID[index] };
		T17NetRoomGameView.Instance.SignalEventToPeers(T17NetConfig.NetEventTypes.Ping, T17NetConfig.NetSequenceChannel.Ping, photonID, new object[3]
		{
			(byte)1,
			UpdateManager.frameCount,
			Time.realtimeSinceStartup
		}, reliable: false);
		PhotonNetwork.networkingPeer.SendOutgoingCommands();
	}

	private void ReplyToLatencyCheck(int senderID, int count, float time)
	{
		int[] photonID = new int[1] { senderID };
		T17NetRoomGameView.Instance.SignalEventToPeers(T17NetConfig.NetEventTypes.Ping, T17NetConfig.NetSequenceChannel.Ping, photonID, new object[3]
		{
			(byte)2,
			count,
			time
		}, reliable: false);
		PhotonNetwork.networkingPeer.SendOutgoingCommands();
	}

	private void ReceiveReply(int senderID, int count, float time)
	{
		for (int num = 2; num >= 0; num--)
		{
			if (ms_PeerID[num] == senderID)
			{
				if (m_SendCount[num] < count)
				{
					m_SendCount[num] = count;
					float num2 = ms_Latencies[num] * 25f;
					float num3 = (Time.realtimeSinceStartup - time) * 0.5f;
					num2 += num3;
					ms_Latencies[num] = num2 / 26f;
				}
				break;
			}
		}
	}

	private void OnEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal != T17NetConfig.NetEventTypes.Ping)
		{
			return;
		}
		object[] array = (object[])payload;
		if (array != null && array.Length == 3)
		{
			byte b = (byte)array[0];
			int count = (int)array[1];
			float time = (float)array[2];
			switch (b)
			{
			case 1:
				ReplyToLatencyCheck(senderId, count, time);
				break;
			case 2:
				ReceiveReply(senderId, count, time);
				break;
			}
		}
	}

	private void ResetPeers()
	{
		for (int num = 2; num >= 0; num--)
		{
			ms_PeerID[num] = -1;
			ms_Latencies[num] = 0f;
			m_LastSent[num] = 0f;
			m_SendCount[num] = 0;
		}
		PhotonPlayer[] playerList = PhotonNetwork.playerList;
		int num2 = 2;
		m_PeerCount = 0;
		int num3 = playerList.Length - 1;
		while (num3 >= 0 && num2 >= 0)
		{
			PhotonPlayer photonPlayer = playerList[num3];
			if (photonPlayer != null && !photonPlayer.IsLocal)
			{
				ms_PeerID[num2] = playerList[num3].ID;
				m_PeerCount++;
				num2--;
			}
			num3--;
		}
	}

	private void OnPhotonPlayerChange(PhotonPlayer newPlayer)
	{
		ResetPeers();
	}

	private void OnLeftRoom()
	{
		ResetPeers();
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}
}
