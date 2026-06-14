using UnityEngine;

public class T17NetConnectAgent : T17NetAgentBase
{
	private enum ConnectingState
	{
		Idle,
		Connecting,
		DisconnectRetryDelay
	}

	private static float m_fStartConnectTime;

	private static float m_fDisconnectedRetryDelayStartTime;

	private static ConnectingState m_State;

	public virtual bool Start()
	{
		Connect();
		return true;
	}

	protected virtual void Connect()
	{
		CloudRegionCode cloudRegionCode = NetConnectAndJoinRoom.PhotonRegion;
		if (T17NetInvites.Region != CloudRegionCode.none)
		{
			cloudRegionCode = T17NetInvites.Region;
		}
		if (Platform.GetInstance().IsReadyForPhoton())
		{
			T17NetManager.SetTimePingIntervalToConnectingRate();
			if (T17NetManager.NetOfflineMode)
			{
				PhotonNetwork.offlineMode = false;
				m_fDisconnectedRetryDelayStartTime = T17NetManager.RealTime;
				m_State = ConnectingState.DisconnectRetryDelay;
			}
			else if (!PhotonNetwork.connecting && !PhotonNetwork.connected)
			{
				ConnectToRegion(cloudRegionCode);
				m_State = ConnectingState.Connecting;
			}
			else if (T17NetManager.IsConnectedOnline() && cloudRegionCode != CloudRegionCode.none && PhotonNetwork.networkingPeer.CloudRegion != cloudRegionCode)
			{
				PhotonNetwork.Disconnect();
				m_fDisconnectedRetryDelayStartTime = T17NetManager.RealTime;
				m_State = ConnectingState.DisconnectRetryDelay;
			}
			m_fStartConnectTime = T17NetManager.RealTime;
		}
	}

	private void ConnectToRegion(CloudRegionCode region)
	{
		Debug.Log("T17PhotonNetworking: Initialising photon connection TimePingInterval: " + PhotonNetwork.networkingPeer.TimePingInterval);
		SetAuthValues();
		if (region == CloudRegionCode.none)
		{
			PhotonNetwork.ConnectUsingSettings(T17NetConnectAndJoinRoom.Instance.AppVersion);
		}
		else
		{
			PhotonNetwork.ConnectToRegion(region, T17NetConnectAndJoinRoom.Instance.AppVersion);
		}
	}

	private void SetAuthValues()
	{
	}

	public virtual void OnDisconnected()
	{
		PhotonNetwork.Disconnect();
		m_State = ConnectingState.DisconnectRetryDelay;
		m_fDisconnectedRetryDelayStartTime = T17NetManager.RealTime;
	}

	public virtual void OnConnectedToMaster()
	{
		m_State = ConnectingState.Idle;
	}

	public virtual void OnJoinedRoom()
	{
	}

	public virtual void OnLeftRoom()
	{
	}

	public virtual void OnCreateRoomFailed()
	{
	}

	public virtual void OnJoinedRoomFailed()
	{
	}

	public virtual void OnPlatformReadyToConnect()
	{
		Connect();
	}

	public virtual void Update()
	{
		switch (m_State)
		{
		case ConnectingState.Connecting:
			if (PhotonNetwork.connecting && T17NetManager.RealTime > m_fStartConnectTime + 10f)
			{
				PhotonNetwork.Disconnect();
				m_State = ConnectingState.DisconnectRetryDelay;
				m_fDisconnectedRetryDelayStartTime = T17NetManager.RealTime;
			}
			break;
		case ConnectingState.DisconnectRetryDelay:
			if (T17NetManager.RealTime > m_fDisconnectedRetryDelayStartTime + 1f)
			{
				Connect();
				m_fStartConnectTime = T17NetManager.RealTime;
				m_State = ConnectingState.Connecting;
			}
			break;
		}
	}
}
