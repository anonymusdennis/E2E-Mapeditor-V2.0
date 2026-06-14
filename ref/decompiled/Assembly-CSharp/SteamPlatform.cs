using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Crosstales.BWF;
using Crosstales.BWF.Model;
using Steamworks;
using UnityEngine;

public class SteamPlatform : Platform
{
	private bool m_bRunDiscovery;

	private int m_PlayerDisPadIndex = -1;

	private bool m_bPlayerDiscovered;

	private bool m_bDiscoveryMain;

	private static bool s_EverInialized = false;

	private SteamLeaderboards m_LeaderboardsImpl;

	private SteamAchievements m_AchievementsImpl;

	private SteamMatchmaking m_MatchmakingImpl;

	private SteamFriends m_FriendsImpl;

	private SteamUGCManager m_UGCImpl;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	protected Callback<DlcInstalled_t> m_DLCInstalledCallback;

	protected Callback<GetAuthSessionTicketResponse_t> m_AuthSessionTicketResponse;

	private string m_UniqueID;

	private string[] m_CachedBWFSources;

	private float m_AuthTokenRequestTime = -1f;

	private float m_AuthTokenRetreiveTime = -1f;

	private const float kAppTicketExpireTime = 3600f;

	private const float kAppTokenRequestWaitPeriod = 10f;

	public static bool m_AuthSessionTicketReceived = false;

	public static byte[] m_AuthSessionTicketBuffer = new byte[1024];

	public static byte[] m_AuthSessionTicket = null;

	public static uint m_AuthSessionTicketLength = 0u;

	private Callback<ClientGameServerDeny_t> m_cb1;

	private Callback<SteamServerConnectFailure_t> m_cb2;

	private Callback<SteamServersConnected_t> m_cb3;

	private Callback<SteamServersDisconnected_t> m_cb4;

	protected override void Awake()
	{
		Debug.Log("  ************  STEAM PLATFORM - Awake *******  ");
		if (s_EverInialized)
		{
			throw new Exception("STEAM PLATFORM: Tried to Initialize the SteamAPI twice in one session!");
		}
		base.Awake();
		if (m_bInitialized)
		{
			return;
		}
		m_bInitialized = true;
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("STEAM PLATFORM: [Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("STEAM PLATFORM: [Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(GetAppId()))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("STEAM PLATFORM: [Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		m_bInitialized = SteamAPI.Init();
		if (!m_bInitialized)
		{
			Debug.LogError("STEAM PLATFORM: [Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		GlobalStart.EnteredLevelEvent += GlobalStart_EnteredLevelEvent;
		Gamer.OnDeleteImminent -= OnDeleteImminent;
		Gamer.OnDeleteImminent += OnDeleteImminent;
		Gamer.OnCreate -= GamerCreatedHandler;
		Gamer.OnCreate += GamerCreatedHandler;
		m_LeaderboardsImpl = base.gameObject.AddComponent<SteamLeaderboards>();
		m_AchievementsImpl = base.gameObject.AddComponent<SteamAchievements>();
		m_MatchmakingImpl = base.gameObject.AddComponent<SteamMatchmaking>();
		m_FriendsImpl = base.gameObject.AddComponent<SteamFriends>();
		m_UGCImpl = base.gameObject.AddComponent<SteamUGCManager>();
		m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		m_DLCInstalledCallback = Callback<DlcInstalled_t>.Create(OnDLCInstalled);
		m_AuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
		bool flag = SteamUserStats.RequestCurrentStats();
		m_cb1 = Callback<ClientGameServerDeny_t>.Create(OnCB1);
		m_cb2 = Callback<SteamServerConnectFailure_t>.Create(OnCB2);
		m_cb3 = Callback<SteamServersConnected_t>.Create(OnCB3);
		m_cb4 = Callback<SteamServersDisconnected_t>.Create(OnCB4);
		CacheBWFLanguageSources();
		Localization.OnLanguageChanged = (Localization.LocalizationEvent)Delegate.Combine(Localization.OnLanguageChanged, new Localization.LocalizationEvent(CacheBWFLanguageSources));
		StartCoroutine(QueryAvailableDLC());
		string personaName = Steamworks.SteamFriends.GetPersonaName();
		Debug.Log("  *********  " + personaName + "   dfgdfgdfg   ");
		s_EverInialized = true;
	}

	private void OnEnable()
	{
		if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
		{
			m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

	protected override void OnDestroy()
	{
		StopCoroutine(QueryAvailableDLC());
		Localization.OnLanguageChanged = (Localization.LocalizationEvent)Delegate.Remove(Localization.OnLanguageChanged, new Localization.LocalizationEvent(CacheBWFLanguageSources));
		if (m_bInitialized)
		{
			Gamer.OnDeleteImminent -= OnDeleteImminent;
			Gamer.OnCreate -= GamerCreatedHandler;
			GlobalStart.EnteredLevelEvent -= GlobalStart_EnteredLevelEvent;
			SteamAPI.Shutdown();
			base.OnDestroy();
		}
	}

	public void OnApplicationQuit()
	{
		SteamAPI.Shutdown();
	}

	protected override void Start()
	{
		base.Start();
		m_bRunDiscovery = false;
	}

	protected override void Update()
	{
		base.Update();
		if (m_bRunDiscovery)
		{
			int padIndex = -1;
			if (DetectAnyPadStart(ref padIndex) && (m_bDiscoveryMain || padIndex != m_PlayerDisPadIndex))
			{
				m_bRunDiscovery = false;
				m_bPlayerDiscovered = true;
				m_PlayerDisPadIndex = padIndex;
				if (!m_bDiscoveryMain)
				{
				}
			}
		}
		if (m_bInitialized)
		{
			SteamAPI.RunCallbacks();
		}
	}

	protected virtual void GlobalStart_EnteredLevelEvent()
	{
		int value = 0;
		GlobalSave.GetInstance().Get("Settings:VSync", out value, 0);
		QualityManager.SetVsyncCount(value);
		if (CameraManager.GetInstance() != null)
		{
			GlobalSave.GetInstance().Get("Settings:Blur", out value, 1);
			if ((float)value < 0.5f)
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: false);
			}
			else
			{
				CameraManager.GetInstance().SetBlurEffectEnabled(bEnable: true);
			}
		}
	}

	public override bool GetSaveDirectory()
	{
		string[] files = Directory.GetFiles("./");
		foreach (string text in files)
		{
			if (text.GetHashCode() != 175804160)
			{
				continue;
			}
			FileStream fileStream = File.OpenRead(text);
			if (fileStream.Length == 219424)
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				string text2 = string.Empty;
				using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
				{
					text2 = Convert.ToBase64String(sHA1CryptoServiceProvider.ComputeHash(array));
				}
				fileStream.Close();
				if (text2 == "TuQzdFIJBPptgMEsJz1n63tcmE4=")
				{
					return true;
				}
				return false;
			}
			fileStream.Close();
		}
		return false;
	}

	private bool DetectAnyPadStart(ref int padIndex)
	{
		bool result = false;
		KeyCode[] array = new KeyCode[2]
		{
			KeyCode.Joystick1Button0,
			KeyCode.Joystick2Button0
		};
		for (int i = 0; i < array.Length; i++)
		{
			if (Input.GetKeyDown(array[i]))
			{
				padIndex = i;
				return true;
			}
		}
		padIndex = -1;
		return result;
	}

	private void OnSaveLoadBusy(bool busy)
	{
	}

	public override bool StartDiscovery(bool bMainUser)
	{
		m_bRunDiscovery = true;
		m_bPlayerDiscovered = false;
		m_bDiscoveryMain = bMainUser;
		return true;
	}

	public override bool FinishedDiscovery(int gamePadIndex)
	{
		return m_bPlayerDiscovered;
	}

	public override bool OnlineCheck()
	{
		return SteamUser.BLoggedOn();
	}

	public override void GetOnlineAccessCode(OnlineAreaEntryResponderCallback callback, bool bShowSystemDialogues)
	{
		callback((!OnlineCheck()) ? OnlineAccessErrorCode.NotConnectedToNet : OnlineAccessErrorCode.OnlineAccessOK, bShowSystemDialogues);
	}

	public override string GetUniqueID()
	{
		if (m_UniqueID == null)
		{
			CSteamID steamID = SteamUser.GetSteamID();
			if (steamID.IsValid())
			{
				m_UniqueID = steamID.ToString();
			}
		}
		if (m_UniqueID != null)
		{
			return m_UniqueID;
		}
		return Convert.ToBase64String(Encoding.ASCII.GetBytes(SystemInfo.deviceUniqueIdentifier));
	}

	public override void OnlineAreaEntryCheckRequest(bool isLeaderboard, OnlineAreaEntryCheckCallback callback, bool isModal = true, bool bShowSystemDialogue = true)
	{
		if (callback != null)
		{
			OnlineAccessErrorCode onlineAccessErrorCode = OnlineAccessErrorCode.OnlineAccessOK;
			if (!OnlineCheck())
			{
				onlineAccessErrorCode = OnlineAccessErrorCode.NotConnectedToNet;
			}
			callback(onlineAccessErrorCode == OnlineAccessErrorCode.OnlineAccessOK, onlineAccessErrorCode, failureHandledPlatformside: false);
		}
	}

	public override bool StartOSK(int controllerIndex, string placeholderText, OSKInfo setupInfo)
	{
		return base.StartOSK(controllerIndex, placeholderText, setupInfo);
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	private void OnCB1(ClientGameServerDeny_t callback)
	{
		int num = 0;
	}

	private void OnCB2(SteamServerConnectFailure_t callback)
	{
		EResult eResult = callback.m_eResult;
		if (eResult == EResult.k_EResultInvalidPassword)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PlatformDisconnected);
		}
		int num = 0;
	}

	private void OnCB3(SteamServersConnected_t callback)
	{
		int num = 0;
	}

	private void OnCB4(SteamServersDisconnected_t callback)
	{
		EResult eResult = callback.m_eResult;
		if (eResult == EResult.k_EResultLoggedInElsewhere)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PlatformDisconnected);
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			Debug.Log("STEAM PLATFORM: The Steam overlay was just activated. Pausing the game");
			if (m_PauseCallBack != null)
			{
				m_PauseCallBack();
			}
		}
		else
		{
			Debug.Log("STEAM PLATFORM: The Steam overlay was just deactivated. Unpausing the game");
			if (m_UnPauseCallBack != null)
			{
				m_UnPauseCallBack();
			}
		}
	}

	public override void ShowGamerCard(Gamer requesterGamer, string requestedGamerId)
	{
		Debug.Log("STEAM PLATFORM: Show gamer card for " + requestedGamerId);
		ulong result = 0uL;
		if (ulong.TryParse(requestedGamerId, out result))
		{
			CSteamID steamID = new CSteamID(result);
			if (steamID.IsValid())
			{
				Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", steamID);
			}
		}
	}

	public override string GetPrimaryUserName()
	{
		return Steamworks.SteamFriends.GetPersonaName();
	}

	private void GamerCreatedHandler(Gamer gamer)
	{
		if (gamer.IsLocal())
		{
			gamer.UpdateGamer(gamer.m_iControllerIndex, gamer.m_PhotonID, gamer.m_NetViewID, null, null, bPrimarySet: false, bPrimary: false, SteamUser.GetSteamID().ToString());
		}
	}

	private void OnDeleteImminent(Gamer gamer)
	{
	}

	public static AppId_t GetAppId()
	{
		return new AppId_t(641990u);
	}

	public override string StreamingAssetsPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "Steam\\Windows");
	}

	public override void PostToLeaderboard(LevelScript.PRISON_ENUM ePrison, LeaderboardGameType lbGameType, LeaderboardPostComplete callback, int score, int extraData)
	{
		if (m_LeaderboardsImpl != null)
		{
			m_LeaderboardsImpl.PostToLeaderboard(ePrison, lbGameType, callback, score, extraData);
		}
	}

	public override void RequestLeaderboard(LeaderboardType lbType, LevelScript.PRISON_ENUM ePrison, LeaderboardGameType lbGameType, LeaderboardReadComplete callback, int firstRow = 1, int numRows = 100, bool bShowErrors = true)
	{
		if (m_LeaderboardsImpl != null)
		{
			m_LeaderboardsImpl.RequestLeaderboard(lbType, ePrison, lbGameType, callback, firstRow, numRows, bShowErrors);
		}
	}

	public override void CancelRequestLeaderboard(CancelRequestLeaderboardCallback callback)
	{
		if (m_LeaderboardsImpl != null)
		{
			m_LeaderboardsImpl.CancelRequestLeaderboard(callback);
		}
	}

	public override void SetAchievementProgress(string APIName, int progress)
	{
		if (m_AchievementsImpl != null)
		{
			m_AchievementsImpl.SetAchievementProgress(APIName, progress);
		}
	}

	public override void UnlockAchievement(string APIName)
	{
		if (m_AchievementsImpl != null)
		{
			m_AchievementsImpl.UnlockAchievement(APIName);
		}
	}

	public override void JoinedSession(string photonRoom, SessionType sessionType, bool isMaster, bool invitesEnabled, PrisonConfig.ConfigType configType)
	{
		if (m_MatchmakingImpl != null)
		{
			m_MatchmakingImpl.OnJoinedSession(photonRoom, sessionType, isMaster, invitesEnabled, configType);
		}
	}

	public override void LeaveSession()
	{
		if (m_MatchmakingImpl != null)
		{
			m_MatchmakingImpl.LeaveSession();
		}
	}

	public override void SessionTypeChange(SessionType newSessionType)
	{
		if (m_MatchmakingImpl != null)
		{
			m_MatchmakingImpl.OnSessionTypeChanged(newSessionType);
		}
	}

	public override void OpenInvitePicker()
	{
		if (m_MatchmakingImpl != null)
		{
			m_MatchmakingImpl.OpenInviteOverlay();
		}
	}

	public override void SendInvite(string OnlineID)
	{
		if (m_MatchmakingImpl != null)
		{
			m_MatchmakingImpl.SendInvite(OnlineID);
		}
	}

	public override void RequestFriendsList(RequestFriendsListCallback requestFriendsListCallback)
	{
		if (m_FriendsImpl != null)
		{
			m_FriendsImpl.RequestFriendsList(requestFriendsListCallback);
		}
	}

	public override void CancelFriendsListRequest()
	{
		if (m_FriendsImpl != null)
		{
			m_FriendsImpl.CancelFriendsListRequest();
		}
	}

	public override void SetPresenceTag(string text)
	{
	}

	public override void SetPresence(string text)
	{
	}

	public override string GetUserNameByControllerIndex(int controllerIndex)
	{
		return Steamworks.SteamFriends.GetPersonaName();
	}

	private IEnumerator QueryAvailableDLC()
	{
		while (true)
		{
			if (!Helpers.IsInGameplayScene())
			{
				bool pbAvailable = false;
				bool flag = false;
				int i = 0;
				for (int dLCCount = SteamApps.GetDLCCount(); i < dLCCount; i++)
				{
					SteamApps.BGetDLCDataByIndex(i, out var pAppID, out pbAvailable, out var pchName, 128);
					string text = pAppID.ToString();
					bool flag2 = false;
					for (int j = 0; j < m_DLCItems.Count; j++)
					{
						if (m_DLCItems[j].productId == text)
						{
							flag2 = true;
							if (!SteamApps.BIsDlcInstalled(pAppID))
							{
								m_DLCItems.RemoveAt(j);
								flag = true;
							}
							break;
						}
					}
					if (!flag2 && SteamApps.BIsDlcInstalled(pAppID))
					{
						DLCItem dLCItem = new DLCItem();
						dLCItem.name = pchName;
						dLCItem.productId = pAppID.ToString();
						m_DLCItems.Add(dLCItem);
						flag = true;
					}
				}
				if (flag && Platform.GetInstance().OnDLCUpdatedEvent != null)
				{
					Platform.GetInstance().OnDLCUpdatedEvent();
				}
			}
			yield return new WaitForSecondsRealtime(15f);
		}
	}

	private uint GetAppIDFromDLCID(string id)
	{
		switch (id)
		{
		case "DLC01":
		case "DLC1":
			return 666350u;
		case "DLC02":
			return 716580u;
		case "DLC04":
			return 784930u;
		case "DLC05":
			return 821170u;
		case "SeasonPass":
			return 701180u;
		default:
			Debug.LogWarning("STEAM PLATFORM: Unknown DLC id in IsDLCAvailable - " + id);
			return 0u;
		}
	}

	public override bool IsDLCAvailable(DLCFrontendData data)
	{
		if (!data.IsAvailableOnThisPlatform())
		{
			return false;
		}
		if (data.m_bFreeDLC)
		{
			return true;
		}
		AppId_t invalid = AppId_t.Invalid;
		bool result = false;
		string dLCID = data.m_DLCID;
		invalid.m_AppId = GetAppIDFromDLCID(dLCID);
		if (invalid.m_AppId != 0)
		{
			string text = invalid.ToString();
			for (int i = 0; i < m_DLCItems.Count; i++)
			{
				if (m_DLCItems[i].productId == text)
				{
					result = SteamApps.BIsDlcInstalled(invalid);
				}
			}
		}
		return result;
	}

	private void OnDLCInstalled(DlcInstalled_t pCallback)
	{
		Debug.Log("STEAM PLATFORM: On DLC Installed " + pCallback.m_nAppID.ToString());
		if (Platform.GetInstance().OnDLCUpdatedEvent != null)
		{
			Platform.GetInstance().OnDLCUpdatedEvent();
		}
	}

	public override bool ShowDLCStorePage(string dlcID)
	{
		uint appIDFromDLCID = GetAppIDFromDLCID(dlcID);
		if (appIDFromDLCID != 0)
		{
			Steamworks.SteamFriends.ActivateGameOverlayToWebPage("steam://url/StoreAppPage/" + appIDFromDLCID);
			return true;
		}
		return false;
	}

	public override void RegisterOnUGCChange(OnUserGeneratedContentUpdated ugcEvent)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.RegisterOnUGCChange(ugcEvent);
		}
	}

	public override void UnRegisterOnUGCChange(OnUserGeneratedContentUpdated ugcEvent)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.UnRegisterOnUGCChange(ugcEvent);
		}
	}

	public override void RegisterUGCUploadCallback(UserGeneratedContentUploadCallback callback)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.RegisterUGCUploadCallback(callback);
		}
	}

	public override void UnRegisterUGCUploadCallback(UserGeneratedContentUploadCallback callback)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.UnRegisterUGCUploadCallback(callback);
		}
	}

	public override void EnumerateUGCItems(ref List<UGCItem> ugcList, UGCType filter = UGCType.eNone)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.EnumerateUGCItems(ref ugcList, filter);
		}
	}

	public override bool EnumeratePublishedUGCItems(UGCType filter = UGCType.eNone, OnPublishedItemsRecieved callback = null)
	{
		if (m_UGCImpl != null)
		{
			return m_UGCImpl.EnumeratePublishedUGCItems(filter, callback);
		}
		return false;
	}

	public override int UploadUGCItem(UGCUploadData uploadData)
	{
		if (m_UGCImpl != null)
		{
			return m_UGCImpl.UploadUGCItem(uploadData);
		}
		return -1;
	}

	public override void RequestShowUGCTerms()
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.RequestShowUGCTerms();
		}
	}

	public override void RequestShowUGCHomePage()
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.RequestShowUGCHomePage();
		}
	}

	public override void UnsubscribedFromContent(UGCItem ugcToRemove)
	{
		if (m_UGCImpl != null)
		{
			m_UGCImpl.UnsubscribedFromContent(ugcToRemove);
		}
	}

	public override void CancelFilteringStringList()
	{
	}

	public override void FilterStringList(List<string> names, NameFilteringCallback callback)
	{
		if (callback == null)
		{
			return;
		}
		if (!BWFManager.isReady)
		{
			callback(bOK: false, names);
			return;
		}
		float value = 1f;
		GlobalSave.GetInstance().Get("Settings:ProfanityFilter", out value, 1f);
		if (value > 0.5f)
		{
			for (int i = 0; i < names.Count; i++)
			{
				List<string> all = BWFManager.GetAll(names[i], ManagerMask.BadWord, m_CachedBWFSources);
				if (all.Count != 0)
				{
					string value2 = BWFManager.Replace(names[i], all, ManagerMask.BadWord);
					names[i] = value2;
				}
			}
		}
		callback(bOK: true, names);
	}

	public override void FilterString(ref string theString, bool useUsersSettings = true)
	{
		if (BWFManager.isReady)
		{
			float value = 1f;
			GlobalSave.GetInstance().Get("Settings:ProfanityFilter", out value, 1f);
			if (!useUsersSettings || value > 0.5f)
			{
				theString = BWFManager.ReplaceAll(theString, ManagerMask.BadWord, m_CachedBWFSources);
			}
		}
	}

	public override bool SupportsDifferentQualitySettings()
	{
		return true;
	}

	private void CacheBWFLanguageSources()
	{
		switch (Localization.GetLanguageIndex())
		{
		case Localization.GameSupportedLanguages.English:
			m_CachedBWFSources = new string[2] { "Team17", "english" };
			break;
		case Localization.GameSupportedLanguages.German:
			m_CachedBWFSources = new string[3] { "Team17", "english", "german" };
			break;
		case Localization.GameSupportedLanguages.French:
			m_CachedBWFSources = new string[3] { "Team17", "english", "french" };
			break;
		case Localization.GameSupportedLanguages.Spanish:
			m_CachedBWFSources = new string[3] { "Team17", "english", "spanish" };
			break;
		case Localization.GameSupportedLanguages.Russian:
			m_CachedBWFSources = new string[3] { "Team17", "english", "russian" };
			break;
		case Localization.GameSupportedLanguages.Italian:
			m_CachedBWFSources = new string[3] { "Team17", "english", "italian" };
			break;
		case Localization.GameSupportedLanguages.Chinese:
			m_CachedBWFSources = new string[3] { "Team17", "english", "chinese" };
			break;
		default:
			m_CachedBWFSources = new string[2] { "Team17", "english" };
			break;
		}
	}

	public override bool IsReadyForPhoton()
	{
		if (Time.time - m_AuthTokenRetreiveTime > 3600f)
		{
			m_AuthSessionTicketReceived = false;
			for (int i = 0; i < m_AuthSessionTicketBuffer.Length; i++)
			{
				m_AuthSessionTicketBuffer[i] = 0;
			}
			m_AuthSessionTicket = null;
		}
		if (!m_AuthSessionTicketReceived)
		{
			RequestAppTicket();
		}
		return m_AuthSessionTicketReceived;
	}

	private void RequestAppTicket()
	{
		if (m_AuthTokenRequestTime <= 0f || Time.time - m_AuthTokenRequestTime >= 10f)
		{
			m_AuthTokenRequestTime = Time.time;
			SteamUser.GetAuthSessionTicket(m_AuthSessionTicketBuffer, m_AuthSessionTicketBuffer.Length, out m_AuthSessionTicketLength);
		}
	}

	private void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			m_AuthTokenRetreiveTime = Time.time;
			m_AuthSessionTicketReceived = true;
			m_AuthSessionTicket = new byte[m_AuthSessionTicketLength];
			Array.Copy(m_AuthSessionTicketBuffer, m_AuthSessionTicket, m_AuthSessionTicketLength);
			SignalReadyToConnectToPhoton();
		}
	}

	public static byte[] GetAuthSessionTicket()
	{
		return m_AuthSessionTicket;
	}

	public override void MakePlatformLobby()
	{
		m_MatchmakingImpl.CreatePlatformLobbyForJoinedSession();
	}

	public override void JoinPlatformLobby()
	{
		m_MatchmakingImpl.JoinPlatformLobby();
	}
}
