using System;

public class NetGoOnlineHelper
{
	public delegate void ConnectionHandler(bool isConnected);

	public delegate void CancelOnlineHandler();

	public ConnectionHandler OnConnection;

	public CancelOnlineHandler OnCancelConnection;

	private T17DialogBox m_GoingOnlineSpinnerDialog;

	private static NetGoOnlineHelper m_HelperInstance;

	public bool m_ShowConnectionFailedPrompt;

	private float m_ShownTimestamp;

	public float m_MinDisplayTime = 2f;

	public float m_MaxDisplayTime = 35f;

	private bool m_bIsConnectedToPhoton;

	private bool m_bIsPlatformAllowedOnline;

	private bool m_bHasGotConnectionResultFromPlatform;

	private bool m_bRaisedEvent;

	private bool m_bAnswerReceived;

	private const int m_iRetryAllowance = 3;

	private int m_iCurrentRetry;

	private bool m_bProfanityCheck;

	public static bool IsActive;

	public NetGoOnlineHelper(T17DialogBox dialogBox, bool showConnectionFailedPrompt, bool bProfanity, ConnectionHandler connectionHandler, CancelOnlineHandler cancelHandler)
	{
		OnConnection = connectionHandler;
		OnCancelConnection = cancelHandler;
		m_GoingOnlineSpinnerDialog = dialogBox;
		m_ShowConnectionFailedPrompt = showConnectionFailedPrompt;
		m_iCurrentRetry = 0;
		m_bProfanityCheck = bProfanity;
	}

	private void Cancel(T17DialogBox dialog)
	{
		if (m_GoingOnlineSpinnerDialog != null)
		{
			m_GoingOnlineSpinnerDialog.Hide();
			m_GoingOnlineSpinnerDialog = null;
		}
		T17NetManager.OnPhotonConnectionChangeEvent -= PhotonConnectionChangeEvent;
		if (OnCancelConnection != null)
		{
			OnCancelConnection();
		}
		m_HelperInstance = null;
		m_bRaisedEvent = true;
		IsActive = false;
	}

	public static void Cancel()
	{
		if (m_HelperInstance != null)
		{
			m_HelperInstance.Cancel(null);
		}
	}

	public void GoOnline()
	{
		if (m_GoingOnlineSpinnerDialog != null)
		{
			m_GoingOnlineSpinnerDialog.InitializeSpinner(hasCancelButton: true, "Text.Dialog.Net.GoOnline.StartTitle", "Text.Dialog.Net.GoOnline.StartBody", "Text.Dialog.Prompt.Cancel");
			T17DialogBox goingOnlineSpinnerDialog = m_GoingOnlineSpinnerDialog;
			goingOnlineSpinnerDialog.OnCancel = (T17DialogBox.DialogEvent)Delegate.Combine(goingOnlineSpinnerDialog.OnCancel, new T17DialogBox.DialogEvent(Cancel));
			T17DialogBox goingOnlineSpinnerDialog2 = m_GoingOnlineSpinnerDialog;
			goingOnlineSpinnerDialog2.OnUpdate = (T17DialogBox.DialogEvent)Delegate.Combine(goingOnlineSpinnerDialog2.OnUpdate, new T17DialogBox.DialogEvent(Update));
			m_GoingOnlineSpinnerDialog.Show();
			m_ShownTimestamp = T17NetManager.RealTime;
			m_iCurrentRetry = 0;
			IsActive = true;
		}
		T17NetManager.OnPhotonConnectionChangeEvent += PhotonConnectionChangeEvent;
		m_bAnswerReceived = false;
		Platform.GetInstance().GetOnlineAccessCode(delegate(Platform.OnlineAccessErrorCode onlineAccessCode, bool bShowSystemDialogues)
		{
			switch (onlineAccessCode)
			{
			case Platform.OnlineAccessErrorCode.OnlineAccessOK:
				Platform.GetInstance().OnlineAreaEntryCheckRequest(isLeaderboard: false, delegate(bool allowedOnline, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
				{
					m_bHasGotConnectionResultFromPlatform = true;
					m_bIsPlatformAllowedOnline = allowedOnline;
					if (allowedOnline)
					{
						T17NetManager.m_DefaultConnectionState = NetConnectionState.OnlineMode_Idle;
						if (m_bProfanityCheck)
						{
							if (!GlobalStart.GetInstance().ProfanityFilteringComplete)
							{
								GlobalStart.GetInstance().StartNameFiltering(ConnectToPhoton);
							}
							else if (!m_bRaisedEvent)
							{
								ConnectToPhoton();
							}
						}
						else if (!m_bRaisedEvent)
						{
							ConnectToPhoton();
						}
					}
					else
					{
						T17NetManager.m_DefaultConnectionState = NetConnectionState.OfflineMode;
						if (m_ShowConnectionFailedPrompt)
						{
							m_ShowConnectionFailedPrompt = !failureHandledPlatformside;
						}
						Fail(bPlatformConnected: false);
					}
				});
				break;
			case Platform.OnlineAccessErrorCode.NotConnectedToNet:
				m_bHasGotConnectionResultFromPlatform = true;
				Fail(bPlatformConnected: false);
				break;
			default:
				m_bHasGotConnectionResultFromPlatform = true;
				if (bShowSystemDialogues && m_ShowConnectionFailedPrompt)
				{
					m_ShowConnectionFailedPrompt = !Platform.GetInstance().DisplayNativeDialogForOnlineAccessCode(onlineAccessCode);
				}
				Fail(bPlatformConnected: false);
				break;
			}
		}, bShowSystemDialogues: true);
	}

	private void ConnectToPhoton()
	{
		if (T17NetManager.IsConnectedOnline())
		{
			PhotonConnectionChangeEvent(isConnected: true);
		}
		else
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_Idle);
		}
	}

	private void Fail(bool bPlatformConnected)
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		HandleIsConnected(bPhotonConnected: false, bPlatformConnected);
		IsActive = false;
	}

	~NetGoOnlineHelper()
	{
		if (m_GoingOnlineSpinnerDialog != null)
		{
			m_GoingOnlineSpinnerDialog.Hide();
		}
	}

	public static void GoOnline(bool showConnectionFailedPrompt, bool bProfanityCheck, ConnectionHandler connectionHandler, CancelOnlineHandler cancelHandler = null)
	{
		m_HelperInstance = new NetGoOnlineHelper(T17DialogBoxManager.GetDialog(forSingleUser: false), showConnectionFailedPrompt, bProfanityCheck, connectionHandler, cancelHandler);
		m_HelperInstance.GoOnline();
	}

	private void PhotonConnectionChangeEvent(bool isConnected)
	{
		m_bIsConnectedToPhoton = isConnected;
		m_bAnswerReceived = true;
		if (T17NetManager.RealTime - m_ShownTimestamp < m_MinDisplayTime && null != m_GoingOnlineSpinnerDialog && m_GoingOnlineSpinnerDialog.IsActive)
		{
			return;
		}
		if (isConnected)
		{
			if (m_bHasGotConnectionResultFromPlatform)
			{
				HandleIsConnected(bPhotonConnected: true, m_bIsPlatformAllowedOnline);
				IsActive = false;
			}
			return;
		}
		m_iCurrentRetry++;
		if (m_iCurrentRetry >= 3)
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			HandleIsConnected(bPhotonConnected: false, bPlatformConnected: true);
			IsActive = false;
		}
	}

	private void HandleIsConnected(bool bPhotonConnected, bool bPlatformConnected)
	{
		T17NetManager.OnPhotonConnectionChangeEvent -= PhotonConnectionChangeEvent;
		if (m_GoingOnlineSpinnerDialog != null)
		{
			m_GoingOnlineSpinnerDialog.Hide();
			m_GoingOnlineSpinnerDialog = null;
		}
		if ((!bPhotonConnected || !bPlatformConnected) && m_ShowConnectionFailedPrompt)
		{
			T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog != null)
			{
				if (!bPlatformConnected)
				{
					ErrorDialogHandler.Type type = ((!ErrorDialogHandler.bErrorFromDirectoryService) ? ErrorDialogHandler.Type.PlatformDisconnected : ErrorDialogHandler.Type.DirectoryServiceFailure);
					bool hasConfirm = true;
					bool hasDecline = false;
					bool hasCancel = false;
					string title = "Text.Dialog.NetworkDisconnect";
					string message = "Text.Dialog.NetworkDisconnect.Body";
					string confirmBtn = "Text.Dialog.Prompt.Ok";
					ErrorDialogHandler.Type errorCode = type;
					dialog.Initialize(hasConfirm, hasDecline, hasCancel, title, message, confirmBtn, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				}
				else
				{
					ErrorDialogHandler.Type type2 = ((!ErrorDialogHandler.bErrorFromDirectoryService) ? ErrorDialogHandler.Type.PhotonDisconnected : ErrorDialogHandler.Type.DirectoryServiceFailure);
					bool hasCancel = true;
					bool hasDecline = false;
					bool hasConfirm = false;
					string confirmBtn = "Text.Dialog.NetworkError";
					string message = "Text.Dialog.NetworkFailedToConnect";
					string title = "Text.Dialog.Prompt.Ok";
					ErrorDialogHandler.Type errorCode = type2;
					dialog.Initialize(hasCancel, hasDecline, hasConfirm, confirmBtn, message, title, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				}
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.Show();
			}
		}
		m_HelperInstance = null;
		m_bRaisedEvent = true;
		if (OnConnection != null)
		{
			OnConnection(bPhotonConnected && bPlatformConnected);
		}
	}

	private void Update(T17DialogBox sender)
	{
		if ((T17NetManager.RealTime - m_ShownTimestamp >= m_MinDisplayTime && m_bIsConnectedToPhoton && m_bIsPlatformAllowedOnline && !m_bRaisedEvent && m_bAnswerReceived) || T17NetManager.RealTime - m_ShownTimestamp >= m_MaxDisplayTime)
		{
			HandleIsConnected(m_bIsConnectedToPhoton, m_bIsPlatformAllowedOnline);
		}
	}
}
