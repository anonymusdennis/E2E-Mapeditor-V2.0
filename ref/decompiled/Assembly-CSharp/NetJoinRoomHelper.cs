using System;
using UnityEngine;

public class NetJoinRoomHelper
{
	public delegate void JoinRoomHandler(bool isConnected);

	private string m_RoomName;

	private bool m_ShowReconnectPrompt;

	private bool m_ShowConnectionFailedPrompts;

	private bool m_ShowConnectingDialog;

	private T17DialogBox m_ConnectingDialog;

	public JoinRoomHandler OnJoinedRoom;

	private float m_ShownTimestamp;

	public float m_MinDisplayTime = 2f;

	public float m_MaxDisplayTime = 10f;

	private short m_JoinResult = -1;

	private bool m_bRaisedEvent;

	private bool m_bAnswerReceived;

	public NetJoinRoomHelper(string roomName, bool showReconnectPrompt, JoinRoomHandler connectionHandler, bool showConnectionFailedPrompts, bool showConnectingDialog)
	{
		m_ShowReconnectPrompt = showReconnectPrompt;
		m_ShowConnectionFailedPrompts = showConnectionFailedPrompts;
		m_ShowConnectingDialog = showConnectingDialog;
		OnJoinedRoom = connectionHandler;
		m_RoomName = roomName;
	}

	~NetJoinRoomHelper()
	{
		if (m_ConnectingDialog != null)
		{
			m_ConnectingDialog.Hide();
		}
		T17NetManager.OnJoinedRoomEvent -= T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnPhotonConnectionChangeEvent -= PhotonConnectionChangeEvent;
	}

	public static void JoinRoomNoDialogs(string roomName, JoinRoomHandler connectionHandler)
	{
		JoinRoom(roomName, showReconnectPrompt: false, connectionHandler, showConnectionFailedPrompts: false, showConnectingDialog: false);
	}

	public static void JoinRoom(string roomName, bool showReconnectPrompt, JoinRoomHandler connectionHandler, bool showConnectionFailedPrompts = true, bool showConnectingDialog = true)
	{
		NetJoinRoomHelper netJoinRoomHelper = new NetJoinRoomHelper(roomName, showReconnectPrompt, connectionHandler, showConnectionFailedPrompts, showConnectingDialog);
		netJoinRoomHelper.StartJoinGameProcess();
	}

	public static void FindAndJoinRoom(string onlineID, bool showReconnectPrompt, JoinRoomHandler connectionHandler, bool showConnectionFailedPrompts = true, bool showConnectingDialog = true)
	{
		NetJoinRoomHelper @object = new NetJoinRoomHelper(null, showReconnectPrompt, connectionHandler, showConnectionFailedPrompts, showConnectingDialog);
		T17NetLobbyManager.Instance.FindRoom(onlineID, @object.OnRoomFound);
	}

	private void OnRoomFound(string roomName)
	{
		if (string.IsNullOrEmpty(roomName))
		{
			if (m_ShowConnectionFailedPrompts)
			{
				ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.InviteJoinFailed);
			}
			m_bRaisedEvent = true;
			if (OnJoinedRoom != null)
			{
				Debug.Log("  Calling OnJoinedRoom");
				OnJoinedRoom(isConnected: false);
			}
		}
		else
		{
			m_RoomName = roomName;
			StartJoinGameProcess();
		}
	}

	public void StartJoinGameProcess()
	{
		T17NetManager.OnJoinedRoomEvent += T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnPhotonConnectionChangeEvent += PhotonConnectionChangeEvent;
		m_bAnswerReceived = false;
		if (m_ShowConnectingDialog)
		{
			m_ConnectingDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (m_ConnectingDialog != null)
			{
				m_ConnectingDialog.InitializeSpinner(hasCancelButton: false, "Text.Dialog.Net.JoinRoom.StartTitle", "Text.Dialog.Net.JoinRoom.StartBody", string.Empty);
				m_ConnectingDialog.Show();
				T17DialogBox connectingDialog = m_ConnectingDialog;
				connectingDialog.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Combine(connectingDialog.OnUpdate, new T17DialogBox.DialogEvent(Update));
				m_ShownTimestamp = T17NetManager.RealTime;
			}
		}
		m_bRaisedEvent = false;
		m_JoinResult = -1;
		NetConnectAndJoinRoom.Init_OnlineMode_JoinSpecific(m_RoomName);
		if (NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_JoinSpecific))
		{
			return;
		}
		T17NetManager.OnJoinedRoomEvent -= T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnPhotonConnectionChangeEvent -= PhotonConnectionChangeEvent;
		if (m_ShowConnectionFailedPrompts)
		{
			m_ConnectingDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (m_ConnectingDialog != null)
			{
				m_ConnectingDialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.FailedBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
				m_ConnectingDialog.Show();
			}
		}
		m_bRaisedEvent = true;
		if (OnJoinedRoom != null)
		{
			OnJoinedRoom(isConnected: false);
		}
	}

	private void Update(T17DialogBox sender)
	{
		if ((T17NetManager.RealTime - m_ShownTimestamp >= m_MinDisplayTime && !m_bRaisedEvent && m_bAnswerReceived) || T17NetManager.RealTime - m_ShownTimestamp >= m_MaxDisplayTime)
		{
			HandleJoinRoomResult(m_JoinResult);
		}
	}

	private void PhotonConnectionChangeEvent(bool isConnected)
	{
		if (!isConnected)
		{
			T17NetManager_OnJoinedRoomEvent(-1);
		}
	}

	private void T17NetManager_OnJoinedRoomEvent(short result)
	{
		m_JoinResult = result;
		m_bAnswerReceived = true;
		if (!(T17NetManager.RealTime - m_ShownTimestamp < m_MinDisplayTime) || !(null != m_ConnectingDialog) || !m_ConnectingDialog.IsActive)
		{
			HandleJoinRoomResult(result);
		}
	}

	private void HandleJoinRoomResult(short result)
	{
		T17NetManager.OnJoinedRoomEvent -= T17NetManager_OnJoinedRoomEvent;
		T17NetManager.OnPhotonConnectionChangeEvent -= PhotonConnectionChangeEvent;
		bool flag = 0 == result;
		if (!T17NetManager.IsConnectedOnline() || T17NetLobbyManager.Instance.RoomName != m_RoomName)
		{
			flag = false;
		}
		if (flag)
		{
			if (m_ConnectingDialog != null)
			{
				m_ConnectingDialog.Hide();
				m_ConnectingDialog = null;
			}
			m_bRaisedEvent = true;
			if (OnJoinedRoom != null)
			{
				OnJoinedRoom(isConnected: true);
			}
			return;
		}
		if (m_ShowReconnectPrompt || m_ShowConnectionFailedPrompts)
		{
			if (m_ConnectingDialog != null)
			{
				m_ConnectingDialog.Hide();
				m_ConnectingDialog = null;
			}
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				if (m_ShowReconnectPrompt)
				{
					dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: true, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.FailedBody", "Text.Dialog.Prompt.Retry", string.Empty, "Text.Dialog.Prompt.Cancel");
					dialog.SetSymbol(T17DialogBox.Symbols.Error);
					dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, (T17DialogBox.DialogEvent)delegate
					{
						StartJoinGameProcess();
					});
				}
				else if (m_ShowConnectionFailedPrompts)
				{
					T17NetInvites.m_Result = T17NetInvites.InviteResult.JoinFailedNoPhotonConnectionError;
					switch (result)
					{
					case 32765:
						dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.GameFull", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						break;
					case 32764:
						dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.GameFull", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						break;
					case 32758:
						dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.GameUnavailableBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						break;
					default:
						dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.FailedBodyC", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						break;
					}
					dialog.SetSymbol(T17DialogBox.Symbols.Error);
				}
				dialog.Show();
			}
		}
		m_bRaisedEvent = true;
		if (OnJoinedRoom != null)
		{
			OnJoinedRoom(isConnected: false);
		}
	}
}
