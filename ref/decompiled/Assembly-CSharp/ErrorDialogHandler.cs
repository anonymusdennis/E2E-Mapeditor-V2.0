using System;
using System.Collections.Generic;
using UnityEngine;

public class ErrorDialogHandler
{
	public enum Type : uint
	{
		None = 0u,
		PlatformDisconnected = 3976200193u,
		PhotonDisconnected = 3976200194u,
		DisconnectedDuringLoad = 3976200195u,
		OnFailedToConnectToPhoton = 250609665u,
		InviteFailedToGoOnline = 4010803202u,
		InviteFailedToConnect = 4010803203u,
		InviteJoinFailed = 4010803204u,
		InviteRoomFull = 4010803205u,
		RequestingAuthKeysFailed = 4023386113u,
		AuthKeyChangeFailed = 4023386114u,
		DirectoryServiceFailure = 4023386115u,
		Kicked = 4009757933u,
		DEBUG = 4026531839u
	}

	public class Error
	{
		public Type m_Type;

		public bool m_wasMasterClient;

		public bool m_duringLoading;
	}

	public static bool bErrorFromDirectoryService = false;

	public static bool bErrorFromBeingKicked = false;

	private static List<Error> AllErrors = new List<Error>();

	public static void ShowError(Type errorType)
	{
		if (isTypeAllowed(errorType))
		{
			Debug.Log("ShowError( " + errorType.ToString() + " )");
			AllErrors.Add(new Error
			{
				m_Type = errorType,
				m_wasMasterClient = T17NetManager.IsMasterClient,
				m_duringLoading = isLoading()
			});
			ConsiderShowingDialog();
		}
	}

	public static void ShowError(Type errorType, string debugData)
	{
		if (!isTypeAllowed(errorType))
		{
			debugData = debugData + "   --- Type Allowed Error: debugData: " + T17NetManager.CanDisplayDisconnectionDialog;
		}
		Debug.Log("ShowError( " + errorType.ToString() + " ) - " + debugData);
		AllErrors.Add(new Error
		{
			m_Type = Type.DEBUG,
			m_wasMasterClient = T17NetManager.IsMasterClient,
			m_duringLoading = isLoading()
		});
		ConsiderShowingDialog();
	}

	public static void ShowError(Type errorType, bool wasOriginalMasterClient, bool? wasDuringLoadingOverride = null)
	{
		if (isTypeAllowed(errorType))
		{
			Debug.Log("ShowError( " + errorType.ToString() + " ) - " + wasOriginalMasterClient + " wasDuringLoadingOverride: " + (wasDuringLoadingOverride.HasValue ? wasDuringLoadingOverride.ToString() : "null"));
			bool duringLoading = (wasDuringLoadingOverride.HasValue ? wasDuringLoadingOverride.Value : isLoading());
			AllErrors.Add(new Error
			{
				m_Type = errorType,
				m_wasMasterClient = wasOriginalMasterClient,
				m_duringLoading = duringLoading
			});
			ConsiderShowingDialog();
		}
	}

	public static void ConsiderShowingDialog()
	{
		if (!canShowDialog())
		{
			return;
		}
		int count = AllErrors.Count;
		if (count <= 0)
		{
			return;
		}
		Debug.Log("**DART** Network error");
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		Error error = null;
		for (int i = 0; i < count; i++)
		{
			Error error2 = AllErrors[i];
			if (error == null || error2.m_Type > error.m_Type)
			{
				error = error2;
			}
		}
		AllErrors.Clear();
		if (error == null)
		{
			return;
		}
		if (NetGoOnlineHelper.IsActive)
		{
			NetGoOnlineHelper.Cancel();
		}
		Type type = ((!bErrorFromDirectoryService) ? error.m_Type : Type.DirectoryServiceFailure);
		switch (error.m_Type)
		{
		case Type.InviteFailedToGoOnline:
		{
			T17DialogBox dialog8 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog8 == null)
			{
				LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
				break;
			}
			bool hasConfirm = true;
			bool hasDecline = false;
			bool hasCancel = false;
			string declineBtn = "Text.Dialog.NoConnection";
			string title = "Text.Dialog.NoConnection.Body";
			string message = "Text.Dialog.Prompt.Ok";
			Type errorCode = type;
			dialog8.Initialize(hasConfirm, hasDecline, hasCancel, declineBtn, title, message, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
			dialog8.SetSymbol(T17DialogBox.Symbols.Error);
			dialog8.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog8.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
			dialog8.Show();
			break;
		}
		case Type.InviteFailedToConnect:
		{
			T17DialogBox dialog6 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog6 == null)
			{
				LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
				break;
			}
			bool hasCancel = true;
			bool hasDecline = false;
			bool hasConfirm = false;
			string message = "Text.Dialog.NetworkError";
			string title = "Text.Dialog.NetworkFailedToConnect";
			string declineBtn = "Text.Dialog.Prompt.Ok";
			Type errorCode = type;
			dialog6.Initialize(hasCancel, hasDecline, hasConfirm, message, title, declineBtn, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
			dialog6.SetSymbol(T17DialogBox.Symbols.Error);
			dialog6.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog6.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
			dialog6.Show();
			break;
		}
		case Type.InviteJoinFailed:
		{
			T17DialogBox dialog3 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog3 == null)
			{
				LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndContinueCallback);
				break;
			}
			bool hasConfirm = true;
			bool hasDecline = false;
			bool hasCancel = false;
			string declineBtn = "Text.Dialog.Net.JoinRoom.FailedTitle";
			string title = "Text.Dialog.Net.JoinRoom.GameUnavailableBody";
			string message = "Text.Dialog.Prompt.Ok";
			Type errorCode = type;
			dialog3.Initialize(hasConfirm, hasDecline, hasCancel, declineBtn, title, message, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
			dialog3.SetSymbol(T17DialogBox.Symbols.Error);
			dialog3.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog3.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
			dialog3.Show();
			break;
		}
		case Type.InviteRoomFull:
		{
			T17DialogBox dialog16 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog16 == null)
			{
				LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndContinueCallback);
				break;
			}
			bool hasCancel = true;
			bool hasDecline = false;
			bool hasConfirm = false;
			string message = "Text.Dialog.Net.JoinRoom.FailedTitle";
			string title = "Text.Dialog.Net.JoinRoom.GameFull";
			string declineBtn = "Text.Dialog.Prompt.Ok";
			Type errorCode = type;
			dialog16.Initialize(hasCancel, hasDecline, hasConfirm, message, title, declineBtn, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
			dialog16.SetSymbol(T17DialogBox.Symbols.Error);
			dialog16.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog16.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
			dialog16.Show();
			break;
		}
		case Type.Kicked:
		{
			bErrorFromBeingKicked = true;
			T17DialogBox dialog7 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog7 == null)
			{
				LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
				break;
			}
			bool hasConfirm = true;
			bool hasDecline = true;
			bool hasCancel = false;
			string declineBtn = "Text.Dialog.NetworkDisconnect";
			string title = "Text.Dialog.NetworkKicked";
			string message = "Text.Dialog.NetworkContinueOffline";
			string confirmBtn = "Text.Dialog.NetworkQuit";
			Type errorCode = type;
			dialog7.Initialize(hasConfirm, hasDecline, hasCancel, declineBtn, title, message, confirmBtn, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
			dialog7.SetSymbol(T17DialogBox.Symbols.Error);
			dialog7.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog7.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
			dialog7.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog7.OnDecline, new T17DialogBox.DialogEvent(notificationBackToDefaultAndQuitCallback));
			dialog7.Show();
			break;
		}
		case Type.OnFailedToConnectToPhoton:
			if (canOfferContinueOffline(error))
			{
				T17DialogBox dialog4 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog4 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasCancel = true;
				bool hasDecline = true;
				bool hasConfirm = false;
				string confirmBtn = "Text.Dialog.NetworkError";
				string message = "Text.Dialog.NetworkFailedToConnect";
				string title = "Text.Dialog.NetworkContinueOffline";
				string declineBtn = "Text.Dialog.NetworkQuit";
				Type errorCode = type;
				dialog4.Initialize(hasCancel, hasDecline, hasConfirm, confirmBtn, message, title, declineBtn, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog4.SetSymbol(T17DialogBox.Symbols.Error);
				dialog4.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog4.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
				dialog4.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog4.OnDecline, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog4.Show();
			}
			else
			{
				T17DialogBox dialog5 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog5 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasConfirm = true;
				bool hasDecline = false;
				bool hasCancel = false;
				string declineBtn = "Text.Dialog.NetworkError";
				string title = "Text.Dialog.NetworkFailedToConnect";
				string message = "Text.Dialog.Prompt.Ok";
				Type errorCode = type;
				dialog5.Initialize(hasConfirm, hasDecline, hasCancel, declineBtn, title, message, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog5.SetSymbol(T17DialogBox.Symbols.Error);
				dialog5.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog5.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog5.Show();
			}
			break;
		case Type.DisconnectedDuringLoad:
			if (canOfferContinueOffline(error))
			{
				T17DialogBox dialog9 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog9 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasCancel = true;
				bool hasDecline = true;
				bool hasConfirm = false;
				string message = "Text.Dialog.NetworkDisconnect";
				string title = "Text.Dialog.NetworkDisconnectDuringLoad";
				string declineBtn = "Text.Dialog.NetworkContinueOffline";
				string confirmBtn = "Text.Dialog.NetworkQuit";
				Type errorCode = type;
				dialog9.Initialize(hasCancel, hasDecline, hasConfirm, message, title, declineBtn, confirmBtn, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog9.SetSymbol(T17DialogBox.Symbols.Error);
				dialog9.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog9.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
				dialog9.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog9.OnDecline, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog9.Show();
			}
			else
			{
				T17DialogBox dialog10 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog10 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasConfirm = true;
				bool hasDecline = false;
				bool hasCancel = false;
				string confirmBtn = "Text.Dialog.NetworkDisconnect";
				string declineBtn = "Text.Dialog.NetworkDisconnectDuringLoad";
				string title = "Text.Dialog.Prompt.Ok";
				Type errorCode = type;
				dialog10.Initialize(hasConfirm, hasDecline, hasCancel, confirmBtn, declineBtn, title, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog10.SetSymbol(T17DialogBox.Symbols.Error);
				dialog10.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog10.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog10.Show();
			}
			break;
		case Type.PhotonDisconnected:
			if (canOfferContinueOffline(error))
			{
				T17DialogBox dialog12 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog12 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasCancel = true;
				bool hasDecline = true;
				bool hasConfirm = false;
				string title = "Text.Dialog.NetworkDisconnect";
				string declineBtn = "Text.Dialog.NetworkDisconnect.PhotonGame.Body";
				string confirmBtn = "Text.Dialog.NetworkContinueOffline";
				string message = "Text.Dialog.NetworkQuit";
				Type errorCode = type;
				dialog12.Initialize(hasCancel, hasDecline, hasConfirm, title, declineBtn, confirmBtn, message, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog12.SetSymbol(T17DialogBox.Symbols.Error);
				dialog12.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog12.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
				dialog12.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog12.OnDecline, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog12.Show();
			}
			else
			{
				T17DialogBox dialog13 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog13 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
				}
				else
				{
					bool hasConfirm = true;
					bool hasDecline = false;
					bool hasCancel = false;
					string message = "Text.Dialog.NetworkDisconnect";
					string confirmBtn = "Text.Dialog.NetworkDisconnect.PhotonGame.Body";
					string declineBtn = "Text.Dialog.Prompt.Ok";
					Type errorCode = type;
					dialog13.Initialize(hasConfirm, hasDecline, hasCancel, message, confirmBtn, declineBtn, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
					dialog13.SetSymbol(T17DialogBox.Symbols.Error);
					dialog13.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog13.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				}
				dialog13.Show();
			}
			break;
		case Type.AuthKeyChangeFailed:
			if (canOfferContinueOffline(error))
			{
				T17DialogBox dialog14 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog14 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasCancel = true;
				bool hasDecline = true;
				bool hasConfirm = false;
				string declineBtn = "Text.Dialog.NetworkDisconnect";
				string confirmBtn = "Text.Dialog.NetworkDisconnect.PhotonGame.Body";
				string message = "Text.Dialog.NetworkContinueOffline";
				string title = "Text.Dialog.NetworkQuit";
				Type errorCode = type;
				dialog14.Initialize(hasCancel, hasDecline, hasConfirm, declineBtn, confirmBtn, message, title, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog14.SetSymbol(T17DialogBox.Symbols.Error);
				dialog14.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog14.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
				dialog14.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog14.OnDecline, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog14.Show();
			}
			else
			{
				T17DialogBox dialog15 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog15 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasConfirm = true;
				bool hasDecline = false;
				bool hasCancel = false;
				string title = "Text.Dialog.NetworkDisconnect";
				string message = "Text.Dialog.NetworkDisconnect.PhotonGame.Body";
				string confirmBtn = "Text.Dialog.Prompt.Ok";
				Type errorCode = type;
				dialog15.Initialize(hasConfirm, hasDecline, hasCancel, title, message, confirmBtn, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog15.SetSymbol(T17DialogBox.Symbols.Error);
				dialog15.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog15.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog15.Show();
			}
			break;
		case Type.RequestingAuthKeysFailed:
		{
			T17DialogBox dialog11 = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (dialog11 != null)
			{
				bool hasCancel = true;
				bool hasDecline = false;
				bool hasConfirm = false;
				string confirmBtn = "Text.Dialog.NetworkDisconnect";
				string message = "Text.Dialog.NetworkFailedToConnect";
				string title = "Text.Dialog.Prompt.Ok";
				Type errorCode = type;
				dialog11.Initialize(hasCancel, hasDecline, hasConfirm, confirmBtn, message, title, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog11.SetSymbol(T17DialogBox.Symbols.Error);
				dialog11.Show();
			}
			break;
		}
		case Type.PlatformDisconnected:
			if (canOfferContinueOffline(error))
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasConfirm = true;
				bool hasDecline = true;
				bool hasCancel = false;
				string title = "Text.Dialog.NetworkDisconnect";
				string message = "Text.Dialog.NetworkDisconnect.Body";
				string confirmBtn = "Text.Dialog.NetworkContinueOffline";
				string declineBtn = "Text.Dialog.NetworkQuit";
				Type errorCode = type;
				dialog.Initialize(hasConfirm, hasDecline, hasCancel, title, message, confirmBtn, declineBtn, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog.SetSymbol(T17DialogBox.Symbols.Error);
				dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndContinueCallback));
				dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog.Show();
			}
			else
			{
				T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
				if (dialog2 == null)
				{
					LogNoDialogErrorAndExectureCallback(error, notificationDisconnectAndQuitCallback);
					break;
				}
				bool hasCancel = true;
				bool hasDecline = false;
				bool hasConfirm = false;
				string declineBtn = "Text.Dialog.NetworkDisconnect";
				string confirmBtn = "Text.Dialog.NetworkDisconnect.Body";
				string message = "Text.Dialog.Prompt.Ok";
				Type errorCode = type;
				dialog2.Initialize(hasCancel, hasDecline, hasConfirm, declineBtn, confirmBtn, message, string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: true, errorCode);
				dialog2.SetSymbol(T17DialogBox.Symbols.Error);
				dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, new T17DialogBox.DialogEvent(notificationDisconnectAndQuitCallback));
				dialog2.Show();
			}
			break;
		}
	}

	private static void LogNoDialogErrorAndExectureCallback(Error highestError, T17DialogBox.DialogEvent functionToExecute)
	{
		Debug.LogError("No dialog available to show " + highestError.m_Type);
		functionToExecute();
	}

	private static bool isTypeAllowed(Type errorType)
	{
		bool result = true;
		if (T17NetManager.SilentErrorDialogMode)
		{
			switch (errorType)
			{
			default:
				result = false;
				break;
			case Type.InviteFailedToGoOnline:
			case Type.InviteFailedToConnect:
			case Type.InviteJoinFailed:
			case Type.InviteRoomFull:
			case Type.RequestingAuthKeysFailed:
			case Type.AuthKeyChangeFailed:
				break;
			}
		}
		else
		{
			switch (errorType)
			{
			case Type.PlatformDisconnected:
			case Type.PhotonDisconnected:
			case Type.DisconnectedDuringLoad:
				result = T17NetManager.CanDisplayDisconnectionDialog;
				T17NetManager.CanDisplayDisconnectionDialog = false;
				break;
			case Type.InviteFailedToGoOnline:
			case Type.InviteFailedToConnect:
			case Type.InviteJoinFailed:
			case Type.InviteRoomFull:
				T17NetManager.SilentErrorDialogMode = true;
				break;
			}
		}
		return result;
	}

	private static bool canShowDialog()
	{
		bool result = false;
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			GlobalStart.GLOBALSTART_MODE mode = instance.GetMode();
			if (mode == GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND || mode == GlobalStart.GLOBALSTART_MODE.IN_LEVEL || mode == GlobalStart.GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS)
			{
				result = true;
			}
		}
		return result;
	}

	private static bool isLoading()
	{
		bool result = false;
		GlobalStart instance = GlobalStart.GetInstance();
		if (null != instance)
		{
			switch (instance.GetMode())
			{
			case GlobalStart.GLOBALSTART_MODE.START_LEVEL_LOAD:
			case GlobalStart.GLOBALSTART_MODE.KILL_FRONTEND:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND:
			case GlobalStart.GLOBALSTART_MODE.LOADING_LEVEL:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL:
			case GlobalStart.GLOBALSTART_MODE.LOADING_OTHER_INGAME_SCENES_HUD:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_HUD:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_IGM:
			case GlobalStart.GLOBALSTART_MODE.SETUP_AREA_MANAGERS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART2:
			case GlobalStart.GLOBALSTART_MODE.SETUP_ITEM_MANAGER:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART3:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_CUSTOMISATION:
			case GlobalStart.GLOBALSTART_MODE.LOAD_CUSTOMISATION:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS:
			case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS:
			case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS:
			case GlobalStart.GLOBALSTART_MODE.REQUEST_PLAYER_STARTING_ITEMS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS:
			case GlobalStart.GLOBALSTART_MODE.NETWORK_INIT_MANAGERS:
			case GlobalStart.GLOBALSTART_MODE.INIT_MANAGERS:
			case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYERS:
				result = true;
				break;
			}
		}
		return result;
	}

	private static bool canOfferContinueOffline(Error proposedError)
	{
		if (Helpers.IsInGameplayScene())
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (instance != null && instance.gameType == PrisonConfig.ConfigType.Cooperative && proposedError.m_wasMasterClient)
			{
				return true;
			}
		}
		return false;
	}

	private static void notificationDisconnectAndContinueCallback(T17DialogBox dialog)
	{
	}

	private static void notificationDisconnectAndQuitCallback(T17DialogBox dialog)
	{
		if (Helpers.IsInGameplayScene())
		{
			GlobalStart.GetInstance().EndLevel(bShowResults: false);
		}
		else if (Helpers.IsInResultsScene())
		{
			GlobalStart.GetInstance().DisconnectAndEndResults();
		}
		if (Platform.GetInstance() != null && Platform.GetInstance().m_OnlineAreaNewDisallowedUserCallback != null)
		{
			Platform.GetInstance().m_OnlineAreaNewDisallowedUserCallback();
		}
	}

	private static void notificationBackToDefaultAndQuitCallback(T17DialogBox dialog)
	{
		if (Helpers.IsInGameplayScene())
		{
			GlobalStart.GetInstance().EndLevel(bShowResults: false);
			NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
		}
		else if (Helpers.IsInResultsScene())
		{
			GlobalStart.GetInstance().EndResultsAndBackToDefault();
		}
		if (Platform.GetInstance() != null && Platform.GetInstance().m_OnlineAreaNewDisallowedUserCallback != null)
		{
			Platform.GetInstance().m_OnlineAreaNewDisallowedUserCallback();
		}
	}

	public static void ShowDisconnectedDialog()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.NetworkDisconnect", "Text.Dialog.NetworkDisconnect.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
		dialog.SetSymbol(T17DialogBox.Symbols.Error);
		dialog.Show();
	}

	public static bool HasErrorsToShow()
	{
		return AllErrors != null && AllErrors.Count > 0;
	}
}
