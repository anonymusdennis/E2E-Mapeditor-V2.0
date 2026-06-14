using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : T17MonoBehaviour, IMenuEventDelegate
{
	private Player m_Player;

	private IT17EventHelper[] m_EventHelperInterfaces;

	public T17Button m_ResumeButton;

	public T17Button m_QuitButton;

	public T17Button m_SettingsButton;

	public T17Button m_SessionModeButton;

	public UIFriendCampaignCarousel m_FriendsCarousel;

	public GameObject m_FriendsSelector;

	public PlayerDisplaySlot[] m_PlayerSlots = new PlayerDisplaySlot[0];

	public T17Image m_PlayerLineUpBackground;

	public Sprite m_StandardPlayerLineUpBackground;

	public Sprite m_DLC03PlayerLineUpBackground;

	public Sprite m_TalkingSprite;

	public Sprite m_SilentSprite;

	public Sprite m_MutedSprite;

	public List<Platform.VoiceChatGamer> m_VoiceChatGamers = new List<Platform.VoiceChatGamer>();

	public T17NetRoomGameView.GameRoomType m_CurrentPlaymode;

	public T17NetRoomGameView.GameRoomType m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Undefined;

	public T17InputField m_PasswordField;

	public BaseContextMenu m_PlayModeContextMenu;

	public T17Text m_PlaymodeText;

	public T17Text m_PlaymodeTitle;

	public T17Text[] m_PlaymodesText;

	public GameObject m_PauseMenu;

	public GameObject m_AnimatedButtonsParent;

	public SettingsFrontendMenu m_SettingsMenu;

	private bool m_bSettingsMenuOpen;

	public SlotSelectionMenu m_SaveSlotMenu;

	private bool m_bSaveSlotMenuOpen;

	private bool[] m_HideAllFromMap = new bool[4];

	private bool m_bNeedToResetHideAll;

	private string m_OnlineStateButtonText = string.Empty;

	private T17NetRoomGameView.GameRoomType m_NewOnlineState;

	private T17EventSystem m_CurrentEventSystem;

	public GameObject m_PCBackButtonPauseMenu;

	public GameObject m_PCBackButtonSettingsMenu;

	public T17Text m_PublicBodyTextConsole;

	public T17Text m_PublicBodyTextPC;

	public event MenuChangedHandler MenuChangedEvent;

	protected override void Awake()
	{
		base.Awake();
		m_EventHelperInterfaces = GetComponentsInChildren<IT17EventHelper>(includeInactive: true);
		for (int i = 0; i < 4; i++)
		{
			m_VoiceChatGamers.Add(new Platform.VoiceChatGamer());
		}
		Gamer.OnDeleteRequested += OnGamerDeleteRequested;
		if (m_PCBackButtonPauseMenu != null)
		{
			m_PCBackButtonPauseMenu.SetActive(value: true);
		}
		if (m_PCBackButtonSettingsMenu != null)
		{
			m_PCBackButtonSettingsMenu.SetActive(value: true);
		}
	}

	protected virtual void OnDestroy()
	{
		T17NetEncryptionKeys.OnKeysRetrieved -= OnKeysRetrived;
		Gamer.OnDeleteRequested -= OnGamerDeleteRequested;
		if (Platform.GetInstance() != null)
		{
			Platform instance = Platform.GetInstance();
			instance.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
			Platform instance2 = Platform.GetInstance();
			instance2.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance2.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnPauseRequest));
		}
		this.MenuChangedEvent = null;
		m_EventHelperInterfaces = null;
		m_ResumeButton = null;
		m_QuitButton = null;
		m_SettingsButton = null;
		m_SessionModeButton = null;
		m_FriendsCarousel = null;
		m_FriendsSelector = null;
		m_PlayerSlots = null;
		m_TalkingSprite = null;
		m_SilentSprite = null;
		m_MutedSprite = null;
		m_PlayModeContextMenu = null;
		m_PlaymodeText = null;
		m_PlaymodeTitle = null;
		m_PlaymodesText = null;
		m_PauseMenu = null;
		m_AnimatedButtonsParent = null;
		m_SettingsMenu = null;
		m_SaveSlotMenu = null;
		m_PCBackButtonPauseMenu = null;
		m_PCBackButtonSettingsMenu = null;
	}

	public void ShowPauseMenu(Player player)
	{
		m_PublicBodyTextConsole.gameObject.SetActive(value: false);
		m_PublicBodyTextPC.gameObject.SetActive(value: true);
		if (CullingObjectCollector.GetInstance().GetHideAll(ref m_HideAllFromMap))
		{
			m_bNeedToResetHideAll = true;
			CullingObjectCollector.GetInstance().SetHideAll(all: false);
		}
		base.gameObject.SetActive(value: true);
		T17NetEncryptionKeys.OnKeysRetrieved += OnKeysRetrived;
		if (m_AnimatedButtonsParent != null)
		{
			m_AnimatedButtonsParent.SetActive(value: true);
		}
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null && instance.GetBlurEffectEnabled())
		{
			instance.SetBlurEffectAllowed(bAllowed: false);
		}
		m_Player = player;
		m_CurrentEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(player.m_Gamer);
		Platform instance2 = Platform.GetInstance();
		instance2.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance2.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnPauseRequest));
		Platform instance3 = Platform.GetInstance();
		instance3.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Combine(instance3.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnPauseRequest));
		if (m_PlayerLineUpBackground != null)
		{
			if (LevelScript.GetCurrentLevelInfo().m_PrisonEnum == LevelScript.PRISON_ENUM.DLC03)
			{
				if (m_StandardPlayerLineUpBackground != null && m_DLC03PlayerLineUpBackground != null)
				{
					m_PlayerLineUpBackground.sprite = m_DLC03PlayerLineUpBackground;
					m_PlayerLineUpBackground.sprite = m_StandardPlayerLineUpBackground;
				}
			}
			else if (m_StandardPlayerLineUpBackground != null && m_DLC03PlayerLineUpBackground != null)
			{
				m_PlayerLineUpBackground.sprite = m_StandardPlayerLineUpBackground;
			}
		}
		ConfigManager instance4 = ConfigManager.GetInstance();
		bool avatarSilhouetted = instance4 != null && instance4.gameType == PrisonConfig.ConfigType.Versus;
		bool flag = false;
		if (m_PlayerSlots != null && m_PlayerSlots.Length > 0)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			int num = Mathf.Min(allPlayers.Count, m_PlayerSlots.Length);
			float animTimescale = 1f / (float)num;
			for (int i = num; i < m_PlayerSlots.Length; i++)
			{
				m_PlayerSlots[i].gameObject.SetActive(value: false);
			}
			for (int j = 0; j < num; j++)
			{
				PlayerDisplaySlot playerDisplaySlot = m_PlayerSlots[j];
				Player player2 = allPlayers[j];
				playerDisplaySlot.SetRewiredPlayerIndex(m_Player.m_Gamer);
				playerDisplaySlot.SetAnimTimescale(animTimescale);
				playerDisplaySlot.SetAvatarSilhouetted(avatarSilhouetted);
				playerDisplaySlot.SetIsHighlighted(isHighlighted: false);
				playerDisplaySlot.SetPlayerTarget(player2);
				if (!flag && player2 != null && player2.m_Gamer != null)
				{
					flag = true;
					Selectable getSelectable = playerDisplaySlot.GetSelectable;
					if (getSelectable == null)
					{
						flag = false;
					}
					if (m_ResumeButton != null)
					{
						Navigation navigation = m_ResumeButton.navigation;
						navigation.selectOnUp = getSelectable;
						m_ResumeButton.navigation = navigation;
					}
					if (m_SettingsButton != null)
					{
						Navigation navigation2 = m_SettingsButton.navigation;
						navigation2.selectOnUp = getSelectable;
						m_SettingsButton.navigation = navigation2;
					}
					if (m_QuitButton != null)
					{
						Navigation navigation3 = m_QuitButton.navigation;
						navigation3.selectOnUp = getSelectable;
						m_QuitButton.navigation = navigation3;
					}
				}
				if (playerDisplaySlot.m_VoiceChat != null)
				{
					playerDisplaySlot.m_VoiceChat.enabled = false;
				}
			}
			for (int k = 0; k < allPlayers.Count; k++)
			{
				Player player3 = allPlayers[k];
				if (player3 != null && player3.m_Gamer != null && player3.m_Gamer.IsLocal())
				{
					player3.bDisplayTutorials = false;
					if (player3 != player)
					{
						T17EventSystem.ApplyCategories(player3.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Disabled);
					}
				}
			}
		}
		T17EventSystem gamersEventSystem = null;
		if (m_Player != null && m_Player.m_Gamer != null)
		{
			gamersEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
		}
		if (m_EventHelperInterfaces != null)
		{
			for (int l = 0; l < m_EventHelperInterfaces.Length; l++)
			{
				if (m_EventHelperInterfaces[l] != null && m_Player != null && m_Player.m_Gamer != null)
				{
					m_EventHelperInterfaces[l].SetGamerForEventSystem(m_Player.m_Gamer, gamersEventSystem);
				}
			}
		}
		UpdateSessionModeButton(player);
		if (EventSystem.current != null && m_ResumeButton != null)
		{
			T17EventSystem t17EventSystem = null;
			if (m_Player != null && m_Player.m_Gamer != null)
			{
				t17EventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			}
			if (t17EventSystem != null)
			{
				t17EventSystem.SetSelectedGameObject(null);
				t17EventSystem.SetSelectedGameObject(m_ResumeButton.gameObject);
			}
			else
			{
				string text = "Get to the bottom of why this event system is null in the Pause Menu\n";
				text += "PlayerDump:\n";
				List<Player> allPlayers2 = Player.GetAllPlayers();
				for (int m = 0; m < allPlayers2.Count; m++)
				{
					text = text + m + "index ";
					if (allPlayers2[m] == m_Player)
					{
						text += " (which is the guy we care about)";
					}
					if (allPlayers2[m] == null)
					{
						text += " is null";
					}
					else
					{
						text += " is not null";
						text = ((allPlayers2[m].m_Gamer != null) ? (text + " and their gamer isn't null") : (text + " and their gamer is null"));
					}
					text += "\n\n";
				}
				Debug.Log(text);
				T17NetManager.LogGoogleException(text);
			}
		}
		UpdateSessionModeButtonText();
		if (m_FriendsCarousel != null)
		{
			if (instance4 != null && instance4.gameType != PrisonConfig.ConfigType.Singleplayer)
			{
				Platform instance5 = Platform.GetInstance();
				instance5.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance5.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
				Platform instance6 = Platform.GetInstance();
				instance6.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Combine(instance6.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
				UpdateFriendCarousel();
			}
			else
			{
				m_FriendsCarousel.DisableFriendFeed();
				m_FriendsCarousel.gameObject.SetActive(value: false);
			}
			Gamer primaryGamer = Gamer.GetPrimaryGamer();
			if (primaryGamer != null)
			{
				bool enable = primaryGamer.m_PlayerObject == player;
				m_FriendsCarousel.EnableContextClick(enable);
			}
		}
		if (m_FriendsSelector != null && instance4 != null && instance4.gameType == PrisonConfig.ConfigType.Singleplayer)
		{
			m_FriendsSelector.gameObject.SetActive(value: false);
		}
		if (T17NetManager.OfflineMode)
		{
			Time.timeScale = 0f;
		}
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
		if (m_PlayModeContextMenu != null)
		{
			m_PlayModeContextMenu.Hide();
		}
	}

	private void OnNetworkChangedCallback(Platform.PlatformNetworkStatus status)
	{
		UpdateFriendCarousel();
	}

	private void UpdateFriendCarousel()
	{
		if (Platform.GetInstance().OnlineCheck())
		{
			m_FriendsCarousel.PopulateWithFriends();
		}
		else
		{
			m_FriendsCarousel.DisableFriendFeed();
		}
	}

	public void Hide()
	{
		if (m_bNeedToResetHideAll)
		{
			CullingObjectCollector.GetInstance().SetHideAll(m_HideAllFromMap);
			m_bNeedToResetHideAll = false;
		}
		base.gameObject.SetActive(value: false);
		if (m_AnimatedButtonsParent != null)
		{
			m_AnimatedButtonsParent.SetActive(value: false);
		}
		m_Player = null;
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
		Platform instance = Platform.GetInstance();
		instance.m_OnNetworkChangedEvent = (Platform.OnNetworkEvent)Delegate.Remove(instance.m_OnNetworkChangedEvent, new Platform.OnNetworkEvent(OnNetworkChangedCallback));
		Platform instance2 = Platform.GetInstance();
		instance2.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance2.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnPauseRequest));
		T17NetEncryptionKeys.OnKeysRetrieved -= OnKeysRetrived;
	}

	public void RequestExit()
	{
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: true, m_Player, showOverPauseMenu: true);
		if (dialog != null)
		{
			SaveManager instance = SaveManager.GetInstance();
			if (instance == null || instance.CurrentSaveMode != 0)
			{
				dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.Quit", "Text.Menu.OkToExitNoSave", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			}
			else
			{
				dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Menu.Quit", "Text.Menu.OkToExit", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
			}
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
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player != null && player.m_Gamer != null && player.m_Gamer.IsLocal())
			{
				player.bDisplayTutorials = true;
			}
		}
		InGameMenuFlow instance = InGameMenuFlow.Instance;
		if (instance != null && m_Player != null && m_Player.m_Gamer != null && m_Player.m_Gamer.m_bPrimaryLocal)
		{
			instance.CloseAllMenusOnAllPlayers();
		}
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetClientDisconnect))
		{
		}
		GlobalSave instance2 = GlobalSave.GetInstance();
		if (instance2 != null)
		{
			instance2.RequestSave();
		}
		SaveManager instance3 = SaveManager.GetInstance();
		if (instance3 != null && instance3.CurrentSaveMode == SaveManager.SaveMode.Automatic)
		{
			instance3.SaveGame(null);
		}
		Time.timeScale = 1f;
		RestorePlayersMaps();
		if (!(m_Player != null) || m_Player.m_Gamer == null)
		{
			return;
		}
		if (m_Player.m_Gamer.m_bPrimaryLocal)
		{
			NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
			CameraManager instance4 = CameraManager.GetInstance();
			if (instance4 != null && instance4.GetBlurEffectEnabled())
			{
				instance4.SetBlurEffectAllowed(bAllowed: true);
			}
			InGameMenuFlow.Instance.HidePauseMenu(m_Player, bPlayerIsExitting: true);
			GlobalStart.GetInstance().EndLevel(bShowResults: false);
		}
		else if (m_Player.m_Gamer.IsLocal())
		{
			T17EventSystem.ApplyCategories(m_Player.m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
			Player player2 = m_Player;
			CameraManager instance5 = CameraManager.GetInstance();
			if (instance5 != null && instance5.GetBlurEffectEnabled())
			{
				instance5.SetBlurEffectAllowed(bAllowed: true);
			}
			Platform.GetInstance().ResetLightBar(m_Player.m_Gamer.m_RewiredPlayer.id);
			InGameMenuFlow.Instance.HidePauseMenu(m_Player, bPlayerIsExitting: true);
			T17NetworkManager.GetInstance().DeleteGamer(player2.m_Gamer.m_NetViewID);
			Time.timeScale = 1f;
		}
	}

	public void UpdateSessionModeButton(Player player)
	{
		if (!(null != m_SessionModeButton))
		{
			return;
		}
		ConfigManager instance = ConfigManager.GetInstance();
		if (LevelScript.GetCurrentLevelInfo() == null || instance.gameType != PrisonConfig.ConfigType.Singleplayer)
		{
			if (instance.gameType == PrisonConfig.ConfigType.Versus)
			{
				m_SessionModeButton.interactable = false;
			}
			else if (instance.gameType == PrisonConfig.ConfigType.Cooperative && null != player && player.m_Gamer != null && !player.m_Gamer.m_bPrimaryLocal)
			{
				m_SessionModeButton.interactable = false;
			}
			else if (!AreWeRemotePlayer())
			{
				m_SessionModeButton.interactable = true;
			}
			else
			{
				m_SessionModeButton.interactable = false;
			}
		}
		else
		{
			m_SessionModeButton.gameObject.SetActive(value: false);
		}
	}

	public void UpdateSessionModeButtonText()
	{
		switch (T17NetRoomManager.CurrentGameRoomType)
		{
		case T17NetRoomGameView.GameRoomType.Offline:
			if (m_PlaymodeText != null)
			{
				m_PlaymodeText.SetNewLocalizationTag("Text.Menu.Local");
			}
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Offline;
			m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Offline;
			break;
		case T17NetRoomGameView.GameRoomType.Public:
			if (m_PlaymodeText != null)
			{
				m_PlaymodeText.SetNewLocalizationTag("Text.Menu.Public");
			}
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Public;
			m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Public;
			break;
		case T17NetRoomGameView.GameRoomType.Private:
			if (m_PlaymodeText != null)
			{
				m_PlaymodeText.SetNewLocalizationTag("Text.Menu.Private");
			}
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Private;
			m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Private;
			break;
		}
	}

	public void ResumeGame()
	{
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null && instance.GetBlurEffectEnabled())
		{
			instance.SetBlurEffectAllowed(bAllowed: true);
		}
		RestorePlayersMaps();
		Time.timeScale = 1f;
		InGameMenuFlow.Instance.HidePauseMenu(m_Player, bPlayerIsExitting: false);
	}

	private static void RestorePlayersMaps()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (allPlayers[i] != null && allPlayers[i].m_Gamer != null && allPlayers[i].m_Gamer.IsLocal())
			{
				allPlayers[i].bDisplayTutorials = true;
				if ((bool)allPlayers[i])
				{
					T17EventSystem.SetCategoriesBackToPreviousState(allPlayers[i].m_Gamer.m_RewiredPlayer);
				}
			}
		}
	}

	private void Update()
	{
		if (!T17DialogBoxManager.HasAnyOpenDialogs() && m_CurrentEventSystem != null && m_CurrentEventSystem.currentSelectedGameObject == null && m_PlayModeContextMenu != null && m_PlayModeContextMenu.gameObject.activeInHierarchy && m_PlayModeContextMenu.m_Items.Length > 0)
		{
			m_CurrentEventSystem.SetSelectedGameObject(null);
			m_CurrentEventSystem.SetSelectedGameObject(m_PlayModeContextMenu.m_Items[0].gameObject);
		}
		if (!(m_Player != null) || m_Player.m_Gamer == null || m_Player.m_Gamer.m_RewiredPlayer == null)
		{
			return;
		}
		if (!T17DialogBoxManager.HasAnyOpenDialogs() && !FriendsContextMenu.IsContextMenuOpen && (m_Player.m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Cancel") || m_Player.m_Gamer.m_RewiredPlayer.GetButtonUp("Pause")))
		{
			if (m_PlayModeContextMenu != null && m_PlayModeContextMenu.isContextMenuOpen)
			{
				if (m_PlayModeContextMenu != null)
				{
					m_PlayModeContextMenu.Hide();
				}
			}
			else if (m_bSettingsMenuOpen)
			{
				OnSettingsClose();
			}
			else if (!m_bSaveSlotMenuOpen)
			{
				ResumeGame();
			}
		}
		if (m_CurrentPlaymode != T17NetRoomManager.CurrentGameRoomType)
		{
			UpdateSessionModeButton(m_Player);
			UpdateSessionModeButtonText();
		}
		if (T17NetManager.NetOnlineMode)
		{
			Platform.GetInstance().GetTalkingGamers(ref m_VoiceChatGamers);
			if (m_VoiceChatGamers != null && m_VoiceChatGamers.Count > 0 && m_PlayerSlots != null)
			{
				for (int i = 0; i < m_PlayerSlots.Length; i++)
				{
					if (m_PlayerSlots[i].TargetPlayer != null)
					{
						for (int j = 0; j < m_VoiceChatGamers.Count; j++)
						{
							if (m_VoiceChatGamers[j] != null && m_VoiceChatGamers[j].m_Gamer != null && m_VoiceChatGamers[j].m_Gamer == m_PlayerSlots[i].TargetPlayer.m_Gamer)
							{
								if (m_VoiceChatGamers[j].m_bIsMuted && m_PlayerSlots[i].m_VoiceChat != null)
								{
									m_PlayerSlots[i].m_VoiceChat.sprite = m_MutedSprite;
									m_PlayerSlots[i].m_VoiceChat.enabled = true;
								}
								else if (m_VoiceChatGamers[j].m_bIsTalking && m_PlayerSlots[i].m_VoiceChat != null)
								{
									m_PlayerSlots[i].m_VoiceChat.sprite = m_TalkingSprite;
									m_PlayerSlots[i].m_VoiceChat.enabled = true;
								}
								else
								{
									m_PlayerSlots[i].m_VoiceChat.sprite = m_SilentSprite;
									m_PlayerSlots[i].m_VoiceChat.enabled = false;
								}
								break;
							}
						}
					}
					else if (m_PlayerSlots[i].m_VoiceChat != null)
					{
						m_PlayerSlots[i].m_VoiceChat.enabled = false;
					}
				}
			}
		}
		if (!(m_PCBackButtonPauseMenu != null) || !(m_PCBackButtonSettingsMenu != null) || !(m_Player != null))
		{
			return;
		}
		Gamer gamer = m_Player.m_Gamer;
		if (gamer == null)
		{
			return;
		}
		Rewired.Player rewiredPlayer = gamer.m_RewiredPlayer;
		if (rewiredPlayer == null)
		{
			return;
		}
		Rewired.Player.ControllerHelper controllers = rewiredPlayer.controllers;
		if (controllers == null)
		{
			return;
		}
		Controller lastActiveController = controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			return;
		}
		if (lastActiveController.type == ControllerType.Mouse)
		{
			if (m_bSettingsMenuOpen || m_bSaveSlotMenuOpen)
			{
				m_PCBackButtonSettingsMenu.SetActive(value: true);
			}
			else
			{
				m_PCBackButtonPauseMenu.SetActive(value: true);
			}
			return;
		}
		if (m_PCBackButtonPauseMenu.activeInHierarchy)
		{
			m_PCBackButtonPauseMenu.SetActive(value: false);
		}
		if (m_PCBackButtonSettingsMenu.activeInHierarchy)
		{
			m_PCBackButtonSettingsMenu.SetActive(value: false);
		}
	}

	public void OnPlayMode()
	{
		if (!(m_PlayModeContextMenu != null))
		{
			return;
		}
		if (m_PlayModeContextMenu.isContextMenuOpen)
		{
			m_PlayModeContextMenu.Hide();
			return;
		}
		m_PlayModeContextMenu.ShowContextMenu((int)m_CurrentPlaymode);
		if (m_PasswordField != null)
		{
			m_PasswordField.text = T17NetRoomManager.GetCurrentRoomPassword();
		}
	}

	public void OnPlaymodeContextItemClicked(int playmodeIndex)
	{
		if (m_PlayModeContextMenu != null)
		{
			m_PlayModeContextMenu.Hide();
		}
		switch (playmodeIndex)
		{
		case 0:
		{
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Offline;
			if (m_CurrentPlaymode != 0)
			{
				T17NetManager.CanDisplayDisconnectionDialog = false;
				Time.timeScale = 0f;
				OnOfflineRequest();
				Platform.GetInstance().ExitOnlineArea();
				if (m_PlaymodeText != null)
				{
					m_PlaymodeText.SetNewLocalizationTag("Text.Menu.Local");
				}
			}
			m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Offline;
			T17NetRoomManager instance = T17NetRoomManager.Instance;
			if (instance != null)
			{
				instance.SetPropertiesForGameroomType((T17NetRoomGameView.GameRoomType)playmodeIndex);
				if (SaveManager.GetInstance() != null && SaveManager.GetInstance().CurrentSaveMode != SaveManager.SaveMode.Manual)
				{
					SaveManager.GetInstance().SaveGame(null);
				}
			}
			break;
		}
		case 1:
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Public;
			if (!Platform.GetInstance().OnlineCheck())
			{
				ErrorDialogHandler.ShowDisconnectedDialog();
				break;
			}
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Public;
			if (LevelScript.GetCurrentLevelInfo().m_PrisonEnum != LevelScript.PRISON_ENUM.CustomPrison && PrisonSnapshotIO.GetCurrentPrisonOriginalGameMode() == T17NetRoomGameView.GameRoomType.Offline && PrisonSnapshotIO.IsCurrentPrisonAllowedToPostToSPLeaderboard())
			{
				T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: true, m_Player, showOverPauseMenu: true);
				if (dialog2 != null)
				{
					dialog2.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Dialog.LeaderboardWarning", "Text.Dialog.LeaderboardWarning.Body", "Text.Dialog.Prompt.Ok", "Text.Dialog.Prompt.Cancel", string.Empty);
					dialog2.Show();
					dialog2.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog2.OnConfirm, new T17DialogBox.DialogEvent(OnAcceptedGoingOnlinePublic));
				}
			}
			else
			{
				OnAcceptedGoingOnlinePublic(null);
			}
			break;
		case 2:
			if (!Platform.GetInstance().OnlineCheck())
			{
				ErrorDialogHandler.ShowDisconnectedDialog();
				break;
			}
			m_DesiredPlaymode = T17NetRoomGameView.GameRoomType.Private;
			if (PrisonSnapshotIO.GetCurrentPrisonOriginalGameMode() == T17NetRoomGameView.GameRoomType.Offline && PrisonSnapshotIO.IsCurrentPrisonAllowedToPostToSPLeaderboard())
			{
				T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: true, m_Player, showOverPauseMenu: true);
				if (dialog != null)
				{
					dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Dialog.LeaderboardWarning", "Text.Dialog.LeaderboardWarning.Body", "Text.Dialog.Prompt.Ok", "Text.Dialog.Prompt.Cancel", string.Empty);
					dialog.Show();
					dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnAcceptedGoingOnlinePrivate));
				}
			}
			else
			{
				OnAcceptedGoingOnlinePrivate(null);
			}
			break;
		default:
			m_CurrentPlaymode = (T17NetRoomGameView.GameRoomType)playmodeIndex;
			break;
		case -1:
			break;
		}
	}

	private void OnAcceptedGoingOnlinePublic(T17DialogBox box)
	{
		string password = string.Empty;
		if (m_PasswordField != null)
		{
			password = m_PasswordField.text;
		}
		OnAcceptedGoingOnline(T17NetRoomGameView.GameRoomType.Public, "Text.Menu.Public", password);
	}

	private void OnAcceptedGoingOnlinePrivate(T17DialogBox box)
	{
		OnAcceptedGoingOnline(T17NetRoomGameView.GameRoomType.Private, "Text.Menu.Private", string.Empty);
	}

	private void OnAcceptedGoingOnline(T17NetRoomGameView.GameRoomType newType, string buttonText, string password = "")
	{
		Time.timeScale = 1f;
		PrisonSnapshotIO.SetCurrentPrisonSPLeaderboardNotAllowed();
		if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
		{
			ApplySettingsToCurrentOnlineRoom(newType, buttonText, password);
		}
		else
		{
			OnOnlineRequest(newType, buttonText, password);
		}
	}

	private void ApplySettingsToCurrentOnlineRoom(T17NetRoomGameView.GameRoomType newType, string buttonText, string password = "")
	{
		if (m_PlaymodeText != null)
		{
			m_PlaymodeText.SetNewLocalizationTag(buttonText);
		}
		LevelScript.PRISON_ENUM prisonEnum = LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
		bool flag = LevelDataManager.GetInstance().IsDLCLevel(prisonEnum);
		bool flag2 = LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport;
		Gamer[] allGamers = Gamer.GetAllGamers();
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null)
		{
			foreach (Gamer gamer in allGamers)
			{
				if (gamer == null || !gamer.IsLocal())
				{
					continue;
				}
				PrisonConfig.ConfigType gameType = instance.gameType;
				if (!flag)
				{
					if (!flag2 && gameType == PrisonConfig.ConfigType.Cooperative)
					{
						StatSystem.GetInstance().IncStat(17, 1f, gamer, string.Empty);
					}
					if (gameType == PrisonConfig.ConfigType.Versus)
					{
						StatSystem.GetInstance().IncStat(16, 1f, gamer, string.Empty);
					}
				}
			}
		}
		m_CurrentPlaymode = newType;
		T17NetRoomManager instance2 = T17NetRoomManager.Instance;
		if (instance2 != null)
		{
			instance2.SetPropertiesForGameroomType(newType);
			instance2.SetPropertiesForGameroomPassword(password);
		}
		if (SaveManager.GetInstance() != null && SaveManager.GetInstance().CurrentSaveMode != SaveManager.SaveMode.Manual)
		{
			SaveManager.GetInstance().SaveGame(null);
		}
	}

	private void OnOnlineRequest(T17NetRoomGameView.GameRoomType newType, string buttonText, string password = "")
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetModeTransition))
		{
		}
		if (!(null != ConfigManager.GetInstance()))
		{
			return;
		}
		Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, null);
		NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
		{
			if (isConnected)
			{
				m_OnlineStateButtonText = buttonText;
				m_NewOnlineState = newType;
				NetCreateRoomHelper.RequestCreateRoom(m_DesiredPlaymode, ConfigManager.GetInstance().gameType, delegate(bool roomSetupOk)
				{
					if (!roomSetupOk)
					{
						ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.OnFailedToConnectToPhoton);
						FailedToGoOnline();
					}
					else
					{
						GlobalStart.GetInstance().BroadcastCustomLevelData();
					}
				}, showDialogs: true, password);
			}
			else
			{
				FailedToGoOnline();
			}
		}, delegate
		{
			FailedToGoOnline();
		});
	}

	public void OnKeyRetrievalError()
	{
		ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.RequestingAuthKeysFailed);
		FailedToGoOnline();
	}

	public void OnKeysRetrived()
	{
		string password = string.Empty;
		if (m_PasswordField != null)
		{
			password = m_PasswordField.text;
		}
		ApplySettingsToCurrentOnlineRoom(m_NewOnlineState, m_OnlineStateButtonText, password);
	}

	private void FailedToGoOnline()
	{
		T17NetManager.CanDisplayDisconnectionDialog = false;
		Time.timeScale = 0f;
		OnOfflineRequest();
		Platform.GetInstance().ExitOnlineArea();
		if (m_PlaymodeText != null)
		{
			m_PlaymodeText.SetNewLocalizationTag("Text.Menu.Local");
		}
		m_CurrentPlaymode = T17NetRoomGameView.GameRoomType.Offline;
		T17NetRoomManager instance = T17NetRoomManager.Instance;
		if (instance != null)
		{
			instance.SetPropertiesForGameroomType(m_CurrentPlaymode);
		}
	}

	private void OnOfflineRequest()
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetModeTransition))
		{
		}
		T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.ConfigType, (int)ConfigManager.GetInstance().gameType);
		T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.RoomType, (int)m_DesiredPlaymode);
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		if (m_PlayerSlots != null)
		{
			for (int i = 0; i < m_PlayerSlots.Length; i++)
			{
				if (m_PlayerSlots[i] != null && m_PlayerSlots[i].m_VoiceChat != null)
				{
					m_PlayerSlots[i].m_VoiceChat.enabled = false;
				}
			}
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int j = 0; j < allPlayers.Count; j++)
		{
			Player player = allPlayers[j];
			if (null != player && !player.m_NetView.isMine && player.m_NetView.ownerId != 0)
			{
				player.m_NetView.TransferOwnership(0);
			}
		}
	}

	public void ChildMenuChanged(IMenuEventDelegate sender = null, IMenuEventDelegate changedItem = null)
	{
		if (this.MenuChangedEvent != null)
		{
			this.MenuChangedEvent();
		}
	}

	public void OnSlotSelected(int index)
	{
		if (m_PlayerSlots[index].TargetPlayer != null && m_PlayerSlots[index].TargetPlayer.m_Gamer != null)
		{
			Platform.GetInstance().ShowGamerCard(m_Player.m_Gamer, m_PlayerSlots[index].TargetPlayer.m_Gamer.m_PlatformUniqueID);
		}
	}

	public void OnSettingsClicked()
	{
		if (m_PauseMenu != null && m_SettingsMenu != null)
		{
			m_PauseMenu.SetActive(value: false);
			m_SettingsMenu.Show(m_Player.m_Gamer, null, null, hideInvoker: false);
			m_bSettingsMenuOpen = true;
		}
	}

	public void OnSettingsClose()
	{
		if (!(m_PauseMenu != null) || !(m_SettingsMenu != null))
		{
			return;
		}
		OptionsSettingsMenu componentInChildren = GetComponentInChildren<OptionsSettingsMenu>();
		if (componentInChildren != null)
		{
			componentInChildren.ConfirmChangeFocus(delegate(bool canChangeFocus)
			{
				if (canChangeFocus)
				{
					CloseSettingsMenu();
				}
			});
		}
		else
		{
			CloseSettingsMenu();
		}
	}

	private bool AreWeRemotePlayer()
	{
		return !T17NetManager.IsMasterClient;
	}

	public Player GetOwner()
	{
		return m_Player;
	}

	public void OnSaveSlotClicked()
	{
		GoogleAnalyticsV3.LogCommericalAnalyticEvent("Save Mode", "Pause Menu Save Mode Changed", string.Empty, 0L);
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null)
		{
			instance.CycleSaveMode();
			if (instance.CurrentSaveMode != SaveManager.SaveMode.Off && SaveManager.GetInstance().GetUIMode() == SaveManager.SaveUIMode.None && (!T17NetManager.IsMasterClient || !SaveManager.GetInstance().IsSlotValid()))
			{
				OpenSaveSlotMenu();
			}
		}
	}

	[ContextMenu("ResultSave")]
	public void ResultSave()
	{
		SaveManager.GetInstance().ResetUIMode();
		SaveManager.GetInstance().ClearSelectedSlot(null);
	}

	private void OpenSaveSlotMenu()
	{
		if (!(m_PauseMenu != null) || !(m_SaveSlotMenu != null) || !(SaveManager.GetInstance() != null))
		{
			return;
		}
		if (m_bSettingsMenuOpen)
		{
			m_bSettingsMenuOpen = false;
			m_SettingsMenu.Hide(restoreInvokerState: false);
		}
		SaveManager.GetInstance().GuestSaveSelected();
		m_SaveSlotMenu.Show(m_Player.m_Gamer, null, null, hideInvoker: false);
		if (EventSystem.current != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			if (m_SaveSlotMenu.m_TopSelectable != null)
			{
				eventSystemForGamer.SetSelectedGameObject(m_SaveSlotMenu.m_TopSelectable.gameObject);
			}
		}
		m_bSaveSlotMenuOpen = true;
	}

	public void OnSaveSlotsClose()
	{
		m_SaveSlotMenu.Hide(restoreInvokerState: false);
		m_SettingsMenu.Show(m_Player.m_Gamer, null, null, hideInvoker: false);
		m_bSettingsMenuOpen = true;
		m_bSaveSlotMenuOpen = false;
	}

	private void CloseSettingsMenu()
	{
		m_SettingsMenu.Hide(restoreInvokerState: false);
		m_PauseMenu.SetActive(value: true);
		if (EventSystem.current != null && m_SettingsButton != null)
		{
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(m_Player.m_Gamer);
			eventSystemForGamer.SetSelectedGameObject(null);
			eventSystemForGamer.SetSelectedGameObject(m_SettingsButton.gameObject);
		}
		m_bSettingsMenuOpen = false;
	}

	public bool IsSettingsMenuOpen()
	{
		return m_bSettingsMenuOpen;
	}

	public bool IsSaveSlotMenuOpen()
	{
		return m_bSaveSlotMenuOpen;
	}

	public void OnPlatformUnPauseRequest()
	{
		if (CutsceneManagerBase.GetState() == CutsceneManagerBase.States.Playing || CutsceneManagerBase.GetState() == CutsceneManagerBase.States.SkippingCurrent)
		{
			ResumeGame();
			Platform instance = Platform.GetInstance();
			instance.m_UnPauseCallBack = (Platform.PlatformPauseRequest)Delegate.Remove(instance.m_UnPauseCallBack, new Platform.PlatformPauseRequest(OnPlatformUnPauseRequest));
		}
	}

	public static PauseMenu GetOpenPauseMenuInstance()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int num = allPlayers.Count - 1; num >= 0; num--)
		{
			CameraManager.PlayerBindingID playerCameraManagerBindingID = allPlayers[num].m_PlayerCameraManagerBindingID;
			InGameMenuFlow.PlayerIGMData data = null;
			if (InGameMenuFlow.Instance.GetCorrectIGMData(playerCameraManagerBindingID, out data) && data != null && data.m_PauseMenu != null && data.m_PauseMenu.gameObject.activeSelf)
			{
				return data.m_PauseMenu;
			}
		}
		return null;
	}

	public void RequestUICancel()
	{
		if (m_bSettingsMenuOpen)
		{
			m_SettingsMenu.InvokeNavigateOnUICancel();
		}
		else if (m_bSaveSlotMenuOpen)
		{
			m_SaveSlotMenu.InvokeNavigateOnUICancel();
		}
		else
		{
			ResumeGame();
		}
	}

	public void PerformManualSave()
	{
		SaveManager instance = SaveManager.GetInstance();
		if (instance != null && instance.CurrentSaveMode == SaveManager.SaveMode.Manual)
		{
			instance.SaveGame(null);
		}
	}

	private void OnGamerDeleteRequested(Gamer gamer)
	{
		if (m_Player != null && object.ReferenceEquals(m_Player.m_Gamer, gamer))
		{
			ResumeGame();
		}
	}
}
