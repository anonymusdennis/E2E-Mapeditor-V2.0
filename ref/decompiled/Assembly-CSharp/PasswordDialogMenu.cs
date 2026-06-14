public class PasswordDialogMenu : FrontendMenuBehaviour
{
	public T17InputField m_PasswordField;

	private LobbyRoomInfoObject m_Lobby;

	public void SetCurrentLobbyRoomInfo(LobbyRoomInfoObject roomInfo)
	{
		m_Lobby = roomInfo;
	}

	public void OnConfirm()
	{
		if (m_PasswordField.text == Encryption.Decrypt(m_Lobby.m_Password, "default"))
		{
			m_Lobby.OnSelectItem(m_PasswordField.text);
			return;
		}
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false, Gamer.GetPrimaryGamer().m_PlayerObject, showOverPauseMenu: true);
		dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Menu.WrongPasswordTitle", "Text.Menu.WrongPasswordBody", "Text.Menu.Confirm", string.Empty, string.Empty, T17DialogBox.Symbols.Error);
		dialog.Show();
	}
}
