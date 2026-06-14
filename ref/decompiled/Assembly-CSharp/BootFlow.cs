using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class BootFlow : BaseFlowBehaviour
{
	private enum BOOTFLOW_MODE
	{
		INIT,
		SHOW_LEGAL_PANEL,
		WF_LEGAL_PANEL,
		SHOW_AUTOSAVE_PANEL,
		WF_AUTOSAVE_PANEL,
		SHOW_UNITY_PANEL,
		WF_UNITY_PANEL,
		SHOW_LOGO_VIDEO,
		SHOW_TEAM17_PANEL,
		WF_TEAM17_PANEL,
		SHOW_MOULDY_PANEL,
		WF_MOULDY_PANEL,
		SHOW_BOOT_SCREEN,
		WAIT_FOR_BOOT_SCREEN,
		SHOW_START,
		WAIT_FOR_CURTAIN_RAISE,
		WAIT_FOR_START,
		WAIT_FOR_SIGNIN,
		POST_SIGNIN,
		WAIT_FOR_SLIDEOFF,
		POST_START,
		LOADING_GLOBAL_SAVEDATA,
		LOADING_SAVEMANAGER_DATA,
		WF_LOAD_OR_NEWSAVE,
		DONE
	}

	public Canvas m_BootCanvas;

	public Image m_BootPanel;

	public Image m_LegalPanel;

	public Image m_AutoSavePanel;

	public Image m_UnityPanel;

	public Image m_Team17Panel;

	public Image m_MoudlyPanel;

	public Canvas m_PressStartCanvas;

	public Animator m_SplashSlideOffAnim;

	private float m_SlideOffTimeDone;

	public T17RawImage m_VideoImage;

	public VideoPlaybackSettings m_VideoSettings;

	private VideoDrone m_VideoDrone;

	private bool m_bVideoPlaying;

	private int m_BootControllerIndex = -1;

	private int m_FinalSetupIndex;

	private BOOTFLOW_MODE _BootFlowMode;

	private float m_MenuTransTimerEnd;

	private const float BOOT_TIME_DELAY = 5f;

	private BOOTFLOW_MODE m_BootFlowMode
	{
		get
		{
			return _BootFlowMode;
		}
		set
		{
			_BootFlowMode = value;
			Debug.LogWarning("BootFlowMode = " + value);
		}
	}

	protected override void Start()
	{
		base.Start();
		m_BootFlowMode = BOOTFLOW_MODE.INIT;
		if (m_BootCanvas != null)
		{
			m_BootCanvas.gameObject.SetActive(value: true);
		}
		if (m_BootPanel != null)
		{
			m_BootPanel.gameObject.SetActive(value: false);
		}
		if (m_PressStartCanvas != null)
		{
			m_PressStartCanvas.gameObject.SetActive(value: false);
		}
		if (m_VideoImage != null)
		{
			m_VideoImage.gameObject.SetActive(value: false);
		}
		if (m_LegalPanel != null)
		{
			m_LegalPanel.gameObject.SetActive(value: false);
		}
		if (m_AutoSavePanel != null)
		{
			m_AutoSavePanel.gameObject.SetActive(value: false);
		}
		if (m_UnityPanel != null)
		{
			m_UnityPanel.gameObject.SetActive(value: false);
		}
		if (m_Team17Panel != null)
		{
			m_Team17Panel.gameObject.SetActive(value: false);
		}
		if (m_MoudlyPanel != null)
		{
			m_MoudlyPanel.gameObject.SetActive(value: false);
		}
		m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
		Time.fixedDeltaTime = 1f / 60f;
		QualityManager.SetVsyncCount(0);
		QualitySettings.softParticles = false;
		AudioController.InitialiseForBootFlow();
		if (GlobalStart.GetInstance() != null && GlobalStart.GetInstance().LoadingBackToStart)
		{
			m_BootFlowMode = BOOTFLOW_MODE.SHOW_BOOT_SCREEN;
		}
	}

	protected override void Update()
	{
		base.Update();
		Cursor.visible = false;
		bool flag = false;
		switch (m_BootFlowMode)
		{
		case BOOTFLOW_MODE.SHOW_LOGO_VIDEO:
		case BOOTFLOW_MODE.SHOW_TEAM17_PANEL:
		case BOOTFLOW_MODE.WF_TEAM17_PANEL:
		case BOOTFLOW_MODE.SHOW_MOULDY_PANEL:
		case BOOTFLOW_MODE.WF_MOULDY_PANEL:
			flag = true;
			break;
		}
		if (flag && Platform.GetInstance().CheckForAnyUserPress())
		{
			SkipToShowBootScreen();
		}
		switch (m_BootFlowMode)
		{
		case BOOTFLOW_MODE.INIT:
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			m_BootFlowMode = BOOTFLOW_MODE.SHOW_LEGAL_PANEL;
			break;
		case BOOTFLOW_MODE.SHOW_LEGAL_PANEL:
			if (m_LegalPanel != null)
			{
				m_LegalPanel.gameObject.SetActive(value: true);
			}
			m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
			m_BootFlowMode = BOOTFLOW_MODE.WF_LEGAL_PANEL;
			break;
		case BOOTFLOW_MODE.WF_LEGAL_PANEL:
			if (Time.realtimeSinceStartup >= m_MenuTransTimerEnd)
			{
				m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
				if (m_LegalPanel != null)
				{
					m_LegalPanel.gameObject.SetActive(value: false);
				}
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_UNITY_PANEL;
			}
			break;
		case BOOTFLOW_MODE.SHOW_UNITY_PANEL:
			m_BootFlowMode = BOOTFLOW_MODE.SHOW_AUTOSAVE_PANEL;
			break;
		case BOOTFLOW_MODE.WF_UNITY_PANEL:
			if (Time.realtimeSinceStartup >= m_MenuTransTimerEnd)
			{
				m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
				if (m_UnityPanel != null)
				{
					m_UnityPanel.gameObject.SetActive(value: false);
				}
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_AUTOSAVE_PANEL;
			}
			break;
		case BOOTFLOW_MODE.SHOW_TEAM17_PANEL:
			if (m_AutoSavePanel != null)
			{
				m_Team17Panel.gameObject.SetActive(value: true);
			}
			m_BootFlowMode = BOOTFLOW_MODE.WF_TEAM17_PANEL;
			break;
		case BOOTFLOW_MODE.WF_TEAM17_PANEL:
			if (Time.realtimeSinceStartup >= m_MenuTransTimerEnd)
			{
				m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
				if (m_Team17Panel != null)
				{
					m_Team17Panel.gameObject.SetActive(value: false);
				}
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_MOULDY_PANEL;
			}
			break;
		case BOOTFLOW_MODE.SHOW_MOULDY_PANEL:
			if (m_MoudlyPanel != null)
			{
				m_MoudlyPanel.gameObject.SetActive(value: true);
			}
			m_BootFlowMode = BOOTFLOW_MODE.WF_MOULDY_PANEL;
			break;
		case BOOTFLOW_MODE.WF_MOULDY_PANEL:
			if (Time.realtimeSinceStartup >= m_MenuTransTimerEnd)
			{
				m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
				if (m_MoudlyPanel != null)
				{
					m_MoudlyPanel.gameObject.SetActive(value: false);
				}
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_BOOT_SCREEN;
			}
			break;
		case BOOTFLOW_MODE.SHOW_AUTOSAVE_PANEL:
			if (m_AutoSavePanel != null)
			{
				m_AutoSavePanel.gameObject.SetActive(value: true);
			}
			m_BootFlowMode = BOOTFLOW_MODE.WF_AUTOSAVE_PANEL;
			break;
		case BOOTFLOW_MODE.WF_AUTOSAVE_PANEL:
			if (Time.realtimeSinceStartup >= m_MenuTransTimerEnd)
			{
				m_MenuTransTimerEnd = Time.realtimeSinceStartup + 5f;
				if (m_AutoSavePanel != null)
				{
					m_AutoSavePanel.gameObject.SetActive(value: false);
				}
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_LOGO_VIDEO;
			}
			break;
		case BOOTFLOW_MODE.SHOW_LOGO_VIDEO:
			if (m_bVideoPlaying)
			{
				break;
			}
			if (m_BootCanvas != null)
			{
				m_BootCanvas.gameObject.SetActive(value: true);
			}
			if (m_VideoDrone == null && m_VideoSettings != null && m_VideoImage != null)
			{
				m_VideoDrone = VideoDrone.CreateDrone(m_VideoImage.gameObject, m_VideoSettings, m_VideoImage);
				m_VideoImage.gameObject.SetActive(value: true);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_EC2_Logo_Idents, AudioController.UI_Audio_GO);
				m_VideoDrone.Play();
				m_VideoDrone.OnMovieEnd += VideoComplete;
				if (m_VideoDrone.OutputTexture != null)
				{
					m_VideoImage.texture = m_VideoDrone.OutputTexture;
				}
				if ((bool)m_VideoImage.texture)
				{
				}
				m_bVideoPlaying = true;
			}
			else
			{
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_BOOT_SCREEN;
			}
			break;
		case BOOTFLOW_MODE.SHOW_BOOT_SCREEN:
			FadeManager.GetInstance().StartCurtainLower(delegate
			{
				m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_BOOT_SCREEN;
			});
			break;
		case BOOTFLOW_MODE.WAIT_FOR_BOOT_SCREEN:
			if (Platform.GetInstance().IsInitialized)
			{
				m_BootFlowMode = BOOTFLOW_MODE.SHOW_START;
			}
			break;
		case BOOTFLOW_MODE.SHOW_START:
			if (GlobalStart.GetInstance().ShowTheFrontEndBackGroundVideoOnly())
			{
				if (m_PressStartCanvas != null)
				{
					m_PressStartCanvas.gameObject.SetActive(value: true);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Splash_Screen_Music, AudioController.UI_Audio_GO);
				}
				m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_CURTAIN_RAISE;
				FadeManager.GetInstance().StartCurtainRaise(delegate
				{
					Platform.GetInstance().StartDiscovery(bMainUser: true);
					m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_START;
				});
				if (Bootstrap.Instance != null)
				{
					Debug.Log("**DART** Start");
					Bootstrap.Instance.CheckAndExecStartUpCommand();
				}
				for (int j = 0; j < ReInput.players.playerCount; j++)
				{
					T17EventSystem.ApplyCategories(j, T17EventSystem.InputCateogryStates.Assignment);
				}
				T17DialogBoxManager.ReleaseAll();
				PlatformIO.GetInstance().CancelAllIORequests();
				SaveManager.GetInstance().ResetBackToIdle();
				T17EventSystem.ResetCategoryData();
			}
			break;
		case BOOTFLOW_MODE.WAIT_FOR_CURTAIN_RAISE:
			break;
		case BOOTFLOW_MODE.WAIT_FOR_START:
		{
			Rewired.Player player = Platform.GetInstance().CheckForAPress();
			if (player == null)
			{
				break;
			}
			if (!player.controllers.hasMouse)
			{
				IList<Rewired.Player> players = ReInput.players.Players;
				int num2 = players.FindIndex((Rewired.Player x) => x.controllers.hasMouse);
				if (num2 != -1)
				{
					Rewired.Player player2 = players[num2];
					if (player2 != null)
					{
						Controller lastActiveController = player.controllers.GetLastActiveController();
						if (lastActiveController != null && lastActiveController.type == ControllerType.Joystick)
						{
							IList<Joystick> joysticks = player2.controllers.Joysticks;
							if (joysticks.Count > 0)
							{
								Joystick controller = joysticks[0];
								player.controllers.RemoveController(lastActiveController);
								player2.controllers.RemoveController(controller);
								player.controllers.AddController(controller, removeFromOtherPlayers: true);
								player2.controllers.AddController(lastActiveController, removeFromOtherPlayers: true);
								player = player2;
							}
						}
					}
				}
			}
			m_BootControllerIndex = player.id;
			if (!Platform.GetInstance().EndDiscovery(player.id, bIsPrimary: true))
			{
				m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_SIGNIN;
			}
			else
			{
				m_BootFlowMode = BOOTFLOW_MODE.POST_SIGNIN;
			}
			break;
		}
		case BOOTFLOW_MODE.WAIT_FOR_SIGNIN:
			if (Platform.GetInstance().FinishedDiscovery(m_BootControllerIndex))
			{
				switch (Platform.GetInstance().GetPlatformError())
				{
				case Platform.PlatformError.None:
					m_BootFlowMode = BOOTFLOW_MODE.POST_SIGNIN;
					break;
				default:
					m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_START;
					Platform.GetInstance().RemoveRewiredPlayer(m_BootControllerIndex);
					Platform.GetInstance().RemoveUnusedUsers();
					m_BootControllerIndex = -1;
					break;
				case Platform.PlatformError.UserCancelled:
					m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_START;
					Platform.GetInstance().RemoveRewiredPlayer(m_BootControllerIndex);
					Platform.GetInstance().RemoveUnusedUsers();
					m_BootControllerIndex = -1;
					break;
				}
			}
			break;
		case BOOTFLOW_MODE.POST_SIGNIN:
			if (T17NetDirectoryService.theInstance != null)
			{
				T17NetDirectoryService.theInstance.BeginRetrieveURLs();
			}
			if (m_SplashSlideOffAnim != null)
			{
				m_SplashSlideOffAnim.enabled = true;
				m_SplashSlideOffAnim.SetTrigger("SlideOff");
				m_SlideOffTimeDone = Time.realtimeSinceStartup + 0.8f;
			}
			m_BootFlowMode = BOOTFLOW_MODE.WAIT_FOR_SLIDEOFF;
			break;
		case BOOTFLOW_MODE.WAIT_FOR_SLIDEOFF:
			if (Time.realtimeSinceStartup < m_SlideOffTimeDone)
			{
				break;
			}
			Platform.GetInstance().ProgressedPastIIS();
			Platform.GetInstance().SetPresenceTag("Text.Presence.FrontEndGeneral");
			if (m_BootControllerIndex != -1)
			{
				for (int i = 0; i < ReInput.players.playerCount; i++)
				{
					if (i != m_BootControllerIndex)
					{
						ReInput.players.GetPlayer(i).controllers.maps.ClearMaps(ControllerType.Keyboard, userAssignableOnly: false);
						ReInput.players.GetPlayer(i).controllers.maps.ClearMaps(ControllerType.Mouse, userAssignableOnly: false);
					}
				}
				Gamer gamer = Gamer.UpsertGamer(m_BootControllerIndex, T17NetManager.PhotonPlayerID, -1, Platform.GetInstance().GetPrimaryUserName(), null, bPrimarySet: true, bPrimary: true);
				T17EventSystemsManager.Instance.AssignFreeEventSystemToGamer(gamer);
				T17EventSystem eventSystemForGamer2 = T17EventSystemsManager.Instance.GetEventSystemForGamer(gamer);
				eventSystemForGamer2.m_RewiredInputModule.allowMouseInput = true;
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_FRONTEND, Events.Play_UI_Press_Start, AudioController.UI_Audio_GO);
			}
			m_BootFlowMode = BOOTFLOW_MODE.POST_START;
			break;
		case BOOTFLOW_MODE.POST_START:
			GlobalSave.GetInstance().NewUser();
			m_BootFlowMode = BOOTFLOW_MODE.LOADING_GLOBAL_SAVEDATA;
			break;
		case BOOTFLOW_MODE.LOADING_GLOBAL_SAVEDATA:
			if (!GlobalSave.GetInstance().IsBusy())
			{
				SaveManager.NotifyNewUserLoggedIn(UserSaveGameDataLoaded);
				m_BootFlowMode = BOOTFLOW_MODE.LOADING_SAVEMANAGER_DATA;
			}
			break;
		case BOOTFLOW_MODE.LOADING_SAVEMANAGER_DATA:
		{
			SaveManager instance2 = SaveManager.GetInstance();
			if (instance2 != null)
			{
				instance2.LoadSaveManagerData();
			}
			break;
		}
		case BOOTFLOW_MODE.WF_LOAD_OR_NEWSAVE:
		{
			int value;
			switch (m_FinalSetupIndex)
			{
			case 0:
				GlobalSave.GetInstance().Get("Audio:MusicVol", out value, 100);
				AudioController.SetParameter(Game_Parameter.Music_Volume, (float)value / 100f);
				break;
			case 1:
				GlobalSave.GetInstance().Get("Audio:SFXVol", out value, 100);
				AudioController.SetParameter(Game_Parameter.SFX_Volume, (float)value / 100f);
				break;
			case 2:
				UnlockManager.GetInstance().LoadData();
				break;
			case 3:
				ProgressManager.GetInstance().LoadData();
				break;
			case 4:
				LevelDataManager.GetInstance().LoadData();
				break;
			case 5:
				CustomisationManager.GetInstance().LoadData();
				break;
			case 6:
				CraftManager.GetInstance().LoadData();
				break;
			case 7:
				Platform.GetInstance().SetupStatSystem();
				StatSystem.GetInstance().LoadStats();
				break;
			case 8:
				Platform.GetInstance().LoadData();
				break;
			case 9:
			{
				float optionsSettingValue = PhotonRegionOptionSelector.GetOptionsSettingValue(CloudRegionCode.none);
				GlobalSave.GetInstance().Get("Network:PhotonRegion", out value, (int)optionsSettingValue);
				PhotonRegionOptionSelector.LookupData lookupDataFromOptionsSettingValue = PhotonRegionOptionSelector.GetLookupDataFromOptionsSettingValue(value);
				if (lookupDataFromOptionsSettingValue != null)
				{
					NetConnectAndJoinRoom.PhotonRegion = lookupDataFromOptionsSettingValue.m_PhotonRegion;
					if (NetConnectAndJoinRoom.PhotonRegion != CloudRegionCode.none && NetConnectAndJoinRoom.PhotonRegion != PhotonNetwork.networkingPeer.CloudRegion && T17NetManager.IsConnectedOnline())
					{
						NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
						NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
					}
				}
				break;
			}
			case 10:
				KeyAwardManager.GetInstance().LoadData();
				MatchingGames.GetInstance().LoadIn();
				break;
			case 11:
			{
				QualityManager instance = QualityManager.GetInstance();
				if (instance != null && !instance.AutoDetectQualitySettings())
				{
					return;
				}
				break;
			}
			case 12:
				GlobalSave.GetInstance().Get("Settings:ShadowQuality", out value, 3);
				switch (value)
				{
				case 0:
					QualitySettings.shadows = ShadowQuality.Disable;
					break;
				case 1:
					QualitySettings.shadowResolution = ShadowResolution.Low;
					break;
				case 2:
					QualitySettings.shadowResolution = ShadowResolution.Medium;
					break;
				case 3:
					QualitySettings.shadowResolution = ShadowResolution.High;
					break;
				}
				break;
			case 13:
				base.SignalDoneWithFlow(fromDestroy: true);
				m_BootFlowMode = BOOTFLOW_MODE.DONE;
				break;
			}
			m_FinalSetupIndex++;
			break;
		}
		case BOOTFLOW_MODE.DONE:
		{
			Cursor.visible = true;
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int num = allGamers.Length - 1; num >= 0; num--)
			{
				T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(allGamers[num]);
				if (eventSystemForGamer != null)
				{
					T17RewiredStandaloneInputModule t17RewiredStandaloneInputModule = (T17RewiredStandaloneInputModule)eventSystemForGamer.m_RewiredInputModule;
					if (t17RewiredStandaloneInputModule != null && t17RewiredStandaloneInputModule.m_bCanBeMouseOwner)
					{
						t17RewiredStandaloneInputModule.InitialiseMouseActive();
					}
				}
			}
			break;
		}
		}
	}

	private void SkipToShowBootScreen()
	{
		if (m_UnityPanel != null)
		{
			m_UnityPanel.gameObject.SetActive(value: false);
		}
		if (m_Team17Panel != null)
		{
			m_Team17Panel.gameObject.SetActive(value: false);
		}
		if (m_MoudlyPanel != null)
		{
			m_MoudlyPanel.gameObject.SetActive(value: false);
		}
		if (m_AutoSavePanel != null)
		{
			m_AutoSavePanel.gameObject.SetActive(value: false);
		}
		if (m_BootCanvas != null)
		{
			m_BootCanvas.gameObject.SetActive(value: true);
		}
		if (m_VideoDrone != null)
		{
			if (m_VideoDrone.IsPlaying)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_EC2_Logo_Idents, AudioController.UI_Audio_GO);
			}
			m_VideoDrone.StopVideo();
			VideoComplete();
		}
		else
		{
			m_BootFlowMode = BOOTFLOW_MODE.SHOW_BOOT_SCREEN;
			m_bVideoPlaying = false;
		}
	}

	private void VideoComplete()
	{
		m_VideoDrone.OnMovieEnd -= VideoComplete;
		m_VideoImage.gameObject.SetActive(value: false);
		m_BootFlowMode = BOOTFLOW_MODE.SHOW_BOOT_SCREEN;
		m_bVideoPlaying = false;
	}

	private void UserSaveGameDataLoaded()
	{
		m_FinalSetupIndex = 0;
		m_BootFlowMode = BOOTFLOW_MODE.WF_LOAD_OR_NEWSAVE;
	}
}
