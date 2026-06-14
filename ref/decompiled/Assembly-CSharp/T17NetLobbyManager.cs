using System.Collections;
using UnityEngine;

public class T17NetLobbyManager : T17NetSendMonoMessageTarget
{
	public delegate void OnRoomFound(string roomName);

	private const float JOINING_TIMEOUT = 10f;

	private static T17NetLobbyManager m_instance;

	public GUISkin Skin;

	private bool m_bIsJoining;

	private bool m_bIsUpdated;

	public static T17NetLobbyManager Instance => m_instance;

	public string RoomName => (PhotonNetwork.room != null) ? PhotonNetwork.room.Name : string.Empty;

	protected override void Awake()
	{
		base.Awake();
		if (m_instance != null)
		{
			Debug.LogError("More than one UserManager instance has been created, it expects to be a singleton.", this);
		}
		else
		{
			m_instance = this;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
		}
	}

	public void OnJoinedLobby()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
		{
		}
		T17NetTimeManager.Instance.MasterServerActive = false;
		T17NetTimeManager.Instance.LobbyServerActive = true;
	}

	public void OnJoinedRoom()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
		{
		}
		T17NetTimeManager.Instance.MasterServerActive = false;
		T17NetTimeManager.Instance.LobbyServerActive = false;
		if (PhotonNetwork.room.Name.Equals("offline room"))
		{
			T17NetTimeManager.Instance.RoomServerActive = false;
		}
		else
		{
			T17NetTimeManager.Instance.RoomServerActive = true;
		}
	}

	public void OnLeftRoom()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
		{
		}
		if (!T17NetRoomManager.IsInRoom())
		{
			T17NetTimeManager.Instance.RoomServerActive = false;
		}
	}

	public virtual void OnConnectedToMaster()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetConnectionState))
		{
		}
		T17NetTimeManager.Instance.MasterServerActive = true;
	}

	public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.LogWarningFormat("NetLobbyManager::OnFailedToConnectToPhoton - ERROR - Cause: {0}", cause);
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonError))
		{
		}
		T17NetTimeManager.Instance.MasterServerActive = false;
	}

	public void FindRoom(string onlineID, OnRoomFound onRoomFound)
	{
		if (!string.IsNullOrEmpty(onlineID) && onRoomFound != null)
		{
			StartCoroutine(FindRoomSequence(onlineID, onRoomFound));
		}
	}

	public void CancelFindRoom()
	{
		m_bIsJoining = false;
	}

	protected void OnUpdatedFriendList()
	{
		m_bIsUpdated = true;
	}

	private IEnumerator FindRoomSequence(string onlineID, OnRoomFound onRoomFound)
	{
		if (m_bIsJoining)
		{
			Debug.Log("FindRoomSequence() - Already searching for a room, stopping that search");
			m_bIsJoining = false;
			yield return null;
		}
		m_bIsJoining = true;
		float time = 0f;
		string strRoomName = string.Empty;
		string[] friendsToFind = new string[1] { onlineID };
		bool findingFriends = false;
		Debug.Log("=== Find room of " + onlineID);
		while (time < 10f)
		{
			if (PhotonNetwork.connectionStateDetailed == ClientState.ConnectedToMaster)
			{
				if (findingFriends)
				{
					if (m_bIsUpdated)
					{
						if (PhotonNetwork.Friends != null && PhotonNetwork.Friends.Count != 0 && !string.IsNullOrEmpty(PhotonNetwork.Friends[0].Room))
						{
							strRoomName = PhotonNetwork.Friends[0].Room;
							Debug.Log("  Room found: " + strRoomName + ", Current room = " + ((PhotonNetwork.room != null) ? PhotonNetwork.room.Name : "null"));
							break;
						}
						findingFriends = false;
					}
				}
				else
				{
					findingFriends = PhotonNetwork.FindFriends(friendsToFind);
					m_bIsUpdated = false;
				}
			}
			yield return null;
			time += Time.deltaTime;
			if (!m_bIsJoining)
			{
				Debug.Log("   m_bIsJoining = false, stop searching for room");
				yield break;
			}
		}
		onRoomFound(strRoomName);
		m_bIsJoining = false;
	}
}
