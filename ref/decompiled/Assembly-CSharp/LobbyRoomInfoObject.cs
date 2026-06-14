using System;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.Events;

public class LobbyRoomInfoObject : MonoBehaviour
{
	public delegate void OnSelectedRoom(LobbyRoomInfoObject roomInfo);

	public OnSelectedRoom OnSelectedRoomEvent;

	public T17Text m_RoomName;

	public T17Text m_PlayerCount;

	public T17Text m_Visible;

	public T17Text m_Open;

	public T17Text m_Mode;

	public T17Text m_LevelName;

	public T17Text m_TierLevel;

	public T17Text m_Platform;

	public T17Text m_Full;

	public T17Image m_PasswordIcon;

	public T17Text m_Day;

	public UnityEvent m_ActionOnSelect;

	[HideInInspector]
	public string m_Password = string.Empty;

	private T17NetRoomListManager.NetPhotonRoom m_Room;

	private T17DialogBox m_GamePropertiesDialog;

	protected virtual void OnDestroy()
	{
		T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent -= T17NetManager_OnPhotonCustomRoomPropertiesChangedHandlerEvent;
		T17NetManager.OnLeftRoomEvent -= T17NetManager_OnLeftRoomEvent;
	}

	public void SetRoomInfo(T17NetRoomListManager.NetPhotonRoom info, int index)
	{
		m_Room = info;
		if (m_RoomName != null)
		{
			m_RoomName.text = info.Name;
			m_RoomName.SetNonLocalizedText(info.HostName);
		}
		if (m_PlayerCount != null)
		{
			string text = $"[{info.NumPlayers}/{info.MaxPlayers}]";
			m_PlayerCount.SetNonLocalizedText(text);
		}
		if (m_Visible != null)
		{
			m_Visible.text = string.Format("{0}", (!info.Visible) ? "No" : "Yes");
		}
		if (m_Open != null)
		{
			m_Open.text = string.Format("{0}", (!info.Open) ? "No" : "Yes");
		}
		if (m_Mode != null)
		{
			m_Mode.text = info.RoomType.ToString();
		}
		m_Password = info.RoomPassword;
		Color color = m_PasswordIcon.color;
		color.a = ((!(Encryption.Decrypt(m_Password, "default") != string.Empty)) ? 0f : 1f);
		m_PasswordIcon.color = color;
		if (m_LevelName != null)
		{
			if (Localization.Get(info.DisplayName, out var localized))
			{
				m_LevelName.SetNonLocalizedText(localized);
			}
			else
			{
				m_LevelName.SetNonLocalizedText(info.DisplayName);
			}
		}
		if (m_TierLevel != null)
		{
			m_TierLevel.text = $"[{info.TierLevel}]";
		}
		if (m_Platform != null)
		{
			m_Platform.text = $"{info.Plaform} ";
		}
		if (m_Full != null)
		{
			GlobalStart.GLOBALSTART_MODE roomGameState = (GlobalStart.GLOBALSTART_MODE)info.RoomGameState;
			string arg = roomGameState.ToString();
			m_Full.text = $"{arg}";
		}
		if (m_Day != null)
		{
			m_Day.SetNonLocalizedText($"{info.RoomDays} ");
		}
	}

	public void OnSelectItem(string password = "")
	{
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
		{
			if (isConnected)
			{
				string text = Encryption.Decrypt(m_Password, "default");
				if (string.IsNullOrEmpty(text) || text == password)
				{
					NetJoinRoomHelper.JoinRoom(m_Room.Name, showReconnectPrompt: false, OnJoinedRoomResult);
				}
				else if (text != password && password == string.Empty && OnSelectedRoomEvent != null)
				{
					OnSelectedRoomEvent(this);
				}
			}
		});
	}

	private void OnJoinedRoomResult(bool isConnected)
	{
		if (!isConnected)
		{
			return;
		}
		string levelNameViaPlaylists = NetBluePrintDetails.Instance.LevelNameViaPlaylists;
		if (string.IsNullOrEmpty(levelNameViaPlaylists) || levelNameViaPlaylists.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent += T17NetManager_OnPhotonCustomRoomPropertiesChangedHandlerEvent;
			T17NetManager.OnLeftRoomEvent += T17NetManager_OnLeftRoomEvent;
			m_GamePropertiesDialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
			if (m_GamePropertiesDialog != null)
			{
				m_GamePropertiesDialog.InitializeSpinner(hasCancelButton: false, "Text.Dialog.Net.JoinRoom.GettingInfoTitle", "Text.Dialog.Net.JoinRoom.GettingInfoBody", string.Empty);
				m_GamePropertiesDialog.Show();
			}
		}
		else
		{
			StartLevel();
		}
	}

	private void T17NetManager_OnLeftRoomEvent()
	{
		T17NetManager.OnLeftRoomEvent -= T17NetManager_OnLeftRoomEvent;
		T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent -= T17NetManager_OnPhotonCustomRoomPropertiesChangedHandlerEvent;
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (dialog != null)
		{
			dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.JoinRoom.FailedTitle", "Text.Dialog.Net.JoinRoom.GameUnavailableBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
			dialog.SetSymbol(T17DialogBox.Symbols.Error);
			dialog.Show();
			if (m_GamePropertiesDialog != null)
			{
				m_GamePropertiesDialog.Hide();
				m_GamePropertiesDialog = null;
			}
		}
	}

	private void T17NetManager_OnPhotonCustomRoomPropertiesChangedHandlerEvent(Hashtable propertiesThatChanged)
	{
		string levelNameViaPlaylists = NetBluePrintDetails.Instance.LevelNameViaPlaylists;
		if (!string.IsNullOrEmpty(levelNameViaPlaylists) && !levelNameViaPlaylists.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			T17NetManager.OnPhotonCustomRoomPropertiesChangedHandlerEvent -= T17NetManager_OnPhotonCustomRoomPropertiesChangedHandlerEvent;
			StartLevel();
			if (m_GamePropertiesDialog != null)
			{
				m_GamePropertiesDialog.Hide();
				m_GamePropertiesDialog = null;
			}
		}
	}

	private void StartLevel()
	{
		GlobalStart.GetInstance().SetSelectedLevelToNetRoomCurrent();
		GlobalStart.GetInstance().StartGameWithModeAndCurrentConfig(GlobalStart.GLOBALSTART_GAME_MODES.ONLINE);
	}
}
