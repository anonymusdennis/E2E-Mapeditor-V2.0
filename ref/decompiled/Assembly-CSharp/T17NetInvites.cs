using System;
using UnityEngine;

public class T17NetInvites : MonoBehaviour
{
	[Serializable]
	public class InviteData
	{
		public string n;

		public int l;

		public int m;
	}

	public enum InviteResult
	{
		NotStarted,
		InProgress,
		JoinFailed,
		JoinFailedNoPhotonConnectionError,
		JoinCancelled,
		JoinSucceeded
	}

	public static string RoomName = string.Empty;

	public static string OnlineUserId = string.Empty;

	public static CloudRegionCode Region = CloudRegionCode.none;

	public static InviteResult m_Result = InviteResult.NotStarted;

	private static bool m_bPlayTogetherHost = false;

	private static bool m_bFailedToFindRoom = false;

	public static void Clear()
	{
		RoomName = string.Empty;
		OnlineUserId = string.Empty;
		m_bPlayTogetherHost = false;
		m_Result = InviteResult.NotStarted;
		m_bFailedToFindRoom = false;
	}

	public static bool StartJoiningRoom(string roomName)
	{
		if (PhotonNetwork.room != null && string.Equals(PhotonNetwork.room.Name, roomName))
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.AlreadyInThisGame", "Text.Dialog.AlreadyInThisGame.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
			dialog.Show();
			return false;
		}
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		RoomName = roomName;
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			instance.InviteReceived();
		}
		return true;
	}

	public static void StartJoiningFriend(string friendId, CloudRegionCode friendRegion)
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		OnlineUserId = friendId;
		Region = friendRegion;
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			instance.InviteReceived();
		}
	}

	public static bool JoinFriend()
	{
		if (!string.IsNullOrEmpty(OnlineUserId))
		{
			m_Result = InviteResult.InProgress;
			NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
			{
				if (isConnected)
				{
					T17NetLobbyManager.Instance.FindRoom(OnlineUserId, OnRoomFound);
					RoomName = "SEARCHING";
					OnlineUserId = string.Empty;
				}
				else
				{
					m_Result = InviteResult.JoinFailedNoPhotonConnectionError;
					Platform.GetInstance().ExitOnlineArea();
				}
			}, delegate
			{
				m_Result = InviteResult.JoinCancelled;
				Platform.GetInstance().ExitOnlineArea();
			});
		}
		else if (!string.IsNullOrEmpty(RoomName) && RoomName != "SEARCHING")
		{
			NetJoinRoomHelper.JoinRoom(RoomName, showReconnectPrompt: false, OnJoinedRoomResult);
			return true;
		}
		return false;
	}

	private static void OnRoomFound(string roomName)
	{
		if (string.IsNullOrEmpty(roomName))
		{
			RoomName = string.Empty;
			m_Result = InviteResult.JoinFailed;
			Platform.GetInstance().ExitOnlineArea();
			m_bFailedToFindRoom = true;
		}
		else
		{
			RoomName = roomName;
		}
	}

	public static bool CheckPlatformErrors(string roomName)
	{
		if (Platform.GetInstance().InvitedRoomNameInvalid == roomName)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.InviteJoinFailed);
			return true;
		}
		if (Platform.GetInstance().InvitedRoomNameAlreadyFull == roomName)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.InviteRoomFull);
			return true;
		}
		return false;
	}

	public static bool InviteReceived(string roomInfoJSON)
	{
		if (CheckPlatformErrors(roomInfoJSON))
		{
			return false;
		}
		bool result = false;
		try
		{
			InviteData inviteData = JsonUtility.FromJson<InviteData>(roomInfoJSON);
			if (StartJoiningRoom(inviteData.n))
			{
				Region = (CloudRegionCode)inviteData.l;
				result = true;
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return result;
	}

	public static bool HasInvite()
	{
		return !string.IsNullOrEmpty(RoomName) || !string.IsNullOrEmpty(OnlineUserId);
	}

	public static bool JoinInvitedRoom()
	{
		bool result = false;
		if (HasInvite())
		{
			Gamer.DeleteLocalNonPrimaryGamers();
			m_Result = InviteResult.InProgress;
			result = true;
			NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
			{
				if (isConnected)
				{
					T17NetEncryptionKeys.OnKeysRetrieved -= OnKeysRetrived;
					T17NetEncryptionKeys.OnKeysRetrieved += OnKeysRetrived;
					NetJoinRoomHelper.JoinRoom(RoomName, showReconnectPrompt: false, OnJoinedRoomResult);
				}
				else
				{
					m_Result = InviteResult.JoinFailedNoPhotonConnectionError;
					Platform.GetInstance().ExitOnlineArea();
				}
			}, delegate
			{
				m_Result = InviteResult.JoinCancelled;
				Platform.GetInstance().ExitOnlineArea();
			});
		}
		return result;
	}

	public static void OnJoinedRoomResult(bool isConnected)
	{
		if ((!isConnected || PhotonNetwork.room == null || !(PhotonNetwork.room.Name == RoomName)) && m_Result != InviteResult.JoinFailedNoPhotonConnectionError)
		{
			m_Result = InviteResult.JoinFailed;
		}
	}

	public static void OnKeyRetrivalError()
	{
		if (HasInvite())
		{
			if (m_Result != InviteResult.JoinFailedNoPhotonConnectionError)
			{
				m_Result = InviteResult.JoinFailed;
			}
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		}
	}

	public static void OnKeysRetrived()
	{
		if (HasInvite())
		{
			m_Result = InviteResult.JoinSucceeded;
		}
	}

	public static void PlayTogetherHostRequest()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			m_bPlayTogetherHost = true;
			instance.InviteReceived();
		}
	}

	public static bool IsPlayTogetherHost()
	{
		return m_bPlayTogetherHost;
	}

	public static void InviteErrorReceived()
	{
		if (string.IsNullOrEmpty(RoomName))
		{
			RoomName = "INVALID";
			GlobalStart.GetInstance().InviteReceived();
		}
	}

	public static bool FailedToFindRoom()
	{
		return m_bFailedToFindRoom;
	}
}
