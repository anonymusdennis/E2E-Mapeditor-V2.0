using System;

public class SettingsMenu : GameMenuBehaviour
{
	public void RequestExit()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.Quit", "Text.Menu.QuitYouSure", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(DoExit));
			dialog.Show();
		}
		else
		{
			DoExit(dialog);
		}
	}

	private void DoExit(T17DialogBox dialog)
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetClientDisconnect))
		{
		}
		if (SaveManager.GetInstance() != null)
		{
			SaveManager.GetInstance().SaveGame(null);
		}
		if (T17NetManager.NetOnlineMode)
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		}
		GlobalStart.GetInstance().EndLevel(bShowResults: false);
	}

	public void OnOnlineRequest()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetModeTransition))
		{
		}
		if (null != ConfigManager.GetInstance())
		{
			PrisonConfig.ConfigType gameType = ConfigManager.GetInstance().gameType;
			string lobbyName = NetConnectAndJoinRoom.GetLobbyName(gameType);
			NetCreateRoomHelper.CreateRoomMatchmakingProperties(PrisonConfig.ConfigType.Singleplayer, T17NetRoomGameView.GameRoomType.Undefined, out var customPropertyDefinitionsForLobby, out var initialRoomPropertyValues, string.Empty);
			NetConnectAndJoinRoom.Init_OnlineMode_CreateRoom(gameType, lobbyName, customPropertyDefinitionsForLobby, initialRoomPropertyValues);
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OnlineMode_CreateRoom);
		}
	}

	public void OnOfflineRequest()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetModeTransition))
		{
		}
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
	}
}
