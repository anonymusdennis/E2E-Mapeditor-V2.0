using System;
using System.Collections.Generic;
using System.IO;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class Platform : MonoBehaviour
{
	public class DisplayableFriend : IComparable
	{
		public enum ActivityState
		{
			Unknown,
			Offline,
			Online,
			Away,
			Ingame_Public,
			Ingame_Private,
			Ingame_Menus,
			Ingame
		}

		public Gamer m_Gamer;

		public string m_Name = " - ";

		public string m_OnlineID = string.Empty;

		public string m_Presence = string.Empty;

		public ActivityState m_ActivityState = ActivityState.Offline;

		public int CompareTo(object obj)
		{
			DisplayableFriend displayableFriend = obj as DisplayableFriend;
			if (m_ActivityState == ActivityState.Online && displayableFriend.m_ActivityState != ActivityState.Online)
			{
				return -1;
			}
			if (displayableFriend.m_ActivityState == ActivityState.Online && m_ActivityState != ActivityState.Online)
			{
				return 1;
			}
			if (m_ActivityState == ActivityState.Away && displayableFriend.m_ActivityState != ActivityState.Away)
			{
				return -1;
			}
			if (displayableFriend.m_ActivityState == ActivityState.Away && m_ActivityState != ActivityState.Away)
			{
				return 1;
			}
			if (m_ActivityState == ActivityState.Offline && displayableFriend.m_ActivityState != ActivityState.Offline)
			{
				return -1;
			}
			if (displayableFriend.m_ActivityState == ActivityState.Offline && m_ActivityState != ActivityState.Offline)
			{
				return 1;
			}
			return 0;
		}
	}

	public class DisplayableRank
	{
		public string m_Name = " - ";

		public string m_OnlineID = string.Empty;

		public int m_Rank = -1;

		public ulong m_Score;

		public int m_ExtraData = -1;

		public bool m_bMyScore;
	}

	public class VoiceChatGamer
	{
		public Gamer m_Gamer;

		public string m_GamerName;

		public bool m_bIsTalking;

		public bool m_bIsMuted;
	}

	public class DLCItem
	{
		public string name;

		public string description;

		public string productId;

		public string path = string.Empty;

		public string bundlePath;

		public AssetBundle bundle;

		public void Save()
		{
		}
	}

	[Serializable]
	public class RumbleController
	{
		public enum Motors
		{
			LeftMain = 1,
			RightMain = 2,
			LeftTrigger = 4,
			RightTrigger = 8
		}

		public enum RumbleStrength
		{
			Weak,
			Strong
		}

		public float m_Duration = 0.5f;

		public RumbleStrength m_Strength;

		public Motors m_Motors = (Motors)3;
	}

	[Serializable]
	public class LightBarEffect
	{
		public Color m_EffectColor = Color.white;

		public float m_EffectDuration = 0.5f;

		public bool m_bIsStrobe;

		public Color m_EffectColor2 = Color.white;

		public float m_StrobeDuration = 0.1f;

		[HideInInspector]
		public bool m_bCurrentColorIsSecond;
	}

	[Serializable]
	public class LightController
	{
		public int rewiredPlayerId = -1;

		public Color m_DefaultColor;

		public LightBarEffect m_LightBarEffect;
	}

	public enum SessionType
	{
		SESSION_TYPE_PUBLIC,
		SESSION_TYPE_PRIVATE
	}

	public enum PlatformError
	{
		None,
		CouldNotStartAsyncOperation,
		CouldNotOpenAcountPicker,
		UserAlreadySignedIn,
		UserAlreadyInUse,
		UserCancelled
	}

	public enum PlatformNetworkStatus
	{
		Unknown,
		Disconnected,
		Connected
	}

	public enum LeaderboardType
	{
		Overall,
		MyScore,
		Friends,
		COUNT
	}

	public enum LeaderboardGameType
	{
		SinglePlayer,
		Multiplayer,
		Versus,
		COUNT
	}

	public class LeaderboardCachedData
	{
		public DisplayableRank[] Ranks;

		public float TimeLastRequested;
	}

	public delegate void OnlineAreaEntryCheckCallback(bool bAllowedToProgress, OnlineAccessErrorCode returnCode, bool failureHandledPlatformside);

	public delegate void OnlineAreaNewUserEvent();

	public delegate void OnNetworkEvent(PlatformNetworkStatus status);

	public delegate void RequestFriendsListCallback(List<DisplayableFriend> friendsList);

	public delegate void CancelRequestLeaderboardCallback();

	public delegate void DLCUpdatedEvent();

	public delegate void OnReadyToConnect();

	public delegate void OnlineAreaEntryResponderCallback(OnlineAccessErrorCode error, bool bShowSystemDialogues);

	public delegate void OnUserGeneratedContentUpdated();

	public delegate void UserGeneratedContentUploadCallback(UGCUploadState uploadData);

	public delegate void OnPublishedItemsRecieved(List<UGCItem> publishedItems);

	public delegate void OnResumeFromSuspendedHandler();

	protected class ControllerDisconnectionInfo
	{
		public T17DialogBox DialogBox;

		public int RewiredPlayerIndex = -1;

		public ControllerType DisconnectedControllerType = ControllerType.Joystick;

		public int ControllerID = -1;

		public Gamer AttachedGamer;

		public PlayerMapsSnapshot playerMapsSnapshot;

		public void Reset()
		{
			DialogBox = null;
			RewiredPlayerIndex = -1;
			DisconnectedControllerType = ControllerType.Joystick;
			ControllerID = -1;
			AttachedGamer = null;
			playerMapsSnapshot = null;
		}
	}

	protected class SignOutInfo
	{
		public int m_ControllerID = -1;

		public bool m_bIsPrimary;

		public bool m_IsSignOut = true;

		public string m_UserName = string.Empty;
	}

	public enum OnlineAccessErrorCode
	{
		OnlineAccessOK,
		SignedOutOfPlatformService,
		NotConnectedToNet,
		MissingPatch,
		MissingPatchNotInstalled,
		UnderAge,
		NotPremiumPlatformService,
		MiscError,
		UserNoLongerLoggedIn
	}

	public delegate void LeaderboardReadComplete(bool bOK, DisplayableRank[] ranks, int totalRows, bool bShowError, bool bContentBlocked);

	public delegate void LeaderboardPostComplete(bool bScoreWasBetter, bool bOk);

	public delegate void NameFilteringCallback(bool bOK, List<string> names);

	public delegate void PlatformPauseRequest();

	public enum UGCType
	{
		eNone,
		eCustomLevel,
		eMax
	}

	public class UGCItem
	{
		public ulong m_ID;

		public UGCType m_Type;

		public string m_AbsolutePath = string.Empty;
	}

	public class UGCUploadData
	{
		public enum UGCVisibility
		{
			ePublic,
			eFriendsOnly,
			eHidden
		}

		public ulong m_PublishID;

		public int m_UploadID = -1;

		public string m_strName = string.Empty;

		public string m_strDescription = string.Empty;

		public string m_strContentPath = string.Empty;

		public string m_strPreviewPath = string.Empty;

		public UGCType m_ugcType;

		public UGCVisibility m_eVisibility = UGCVisibility.eHidden;
	}

	public class UGCUploadState
	{
		public int m_UploadID = -1;

		public bool m_bCompleted;

		public bool m_bError;

		public ulong m_ulBytesProcessed;

		public ulong m_ulBytesTotal;

		public ulong m_ulFinalPublishedID;
	}

	public enum PlatformOverride
	{
		None,
		Standalone,
		PS4,
		XBoxOne,
		SwitchHandheld,
		SwitchDocked
	}

	public enum OSKGetTextResponses
	{
		Undefined,
		StillActive,
		Cancelled,
		Accepted
	}

	public class OSKInfo
	{
		public int CharacterLimit = -1;

		public InputField.ContentType ContentType;

		public InputField.LineType LineType;

		public InputField.CharacterValidation CharacterValidation;

		public string Title = string.Empty;

		public string Description = string.Empty;
	}

	public class LobbyData
	{
		public class MemberData
		{
			public Gamer m_Gamer;

			public string m_Name;

			public int m_NetViewID;

			public bool m_bLocalPlayer;

			public int m_LocalPlayerIndex;

			public string m_Address;

			public bool m_bNewPlayer;

			public int m_ID;

			public bool m_IsValid;

			public MemberData()
			{
				m_bNewPlayer = false;
				m_ID = -1;
			}
		}

		public const int MAXPLAYERS = 4;

		public int m_MemberCount;

		public MemberData[] m_Members = new MemberData[4];

		public LobbyData()
		{
			m_MemberCount = 0;
			for (int i = 0; i < 4; i++)
			{
				m_Members[i] = new MemberData();
			}
		}

		public int GetNumberRemoteMembers()
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				if (!m_Members[i].m_bLocalPlayer && m_Members[i].m_IsValid)
				{
					num++;
				}
			}
			return num;
		}
	}

	private bool m_bCheckedAge;

	private Dictionary<int, Controller> m_DisconnectedPlayerControllers = new Dictionary<int, Controller>();

	protected List<LightController> m_LightControllers = new List<LightController>();

	public Dictionary<string, LeaderboardCachedData> m_CachedOverallScores = new Dictionary<string, LeaderboardCachedData>();

	public Dictionary<string, LeaderboardCachedData> m_CachedMyScores = new Dictionary<string, LeaderboardCachedData>();

	public Dictionary<string, LeaderboardCachedData> m_CachedFriendsScores = new Dictionary<string, LeaderboardCachedData>();

	private static Platform m_TheInstance = null;

	private StatsTracking m_StatsTracking;

	private StatSystem m_StatSystem;

	private bool m_bPlatformCreated;

	protected PlatformError m_Error;

	private bool m_bWasMasterBeforeSuspend;

	protected const int FRONTEND_CONTROLLERINFO_INDEX = 4;

	protected ControllerDisconnectionInfo[] m_ControllerDisconnectionInfo = new ControllerDisconnectionInfo[5];

	protected T17DialogBox m_UserSwitchDialog;

	protected int m_ControllersPollingDueToDisconnect;

	protected int m_SignedOutControllerIndex = -1;

	private bool m_bHavePrimarySignOutDialog;

	private List<SignOutInfo> m_SignoutRequests = new List<SignOutInfo>();

	protected List<OnlineAreaEntryCheckCallback> m_OnlineAreaEntryCheckCallbacks = new List<OnlineAreaEntryCheckCallback>();

	protected OnlineAreaEntryCheckCallback m_VoiceChatCheckCallback;

	protected OnlineAreaEntryCheckCallback m_UGCCheckCallback;

	public OnlineAreaNewUserEvent m_OnlineAreaNewDisallowedUserCallback;

	public RequestFriendsListCallback m_RequestFriendsListCallback;

	public OnNetworkEvent m_OnNetworkChangedEvent;

	public NameFilteringCallback OnNameFilteringCallback;

	protected bool m_bUGCRestricted;

	protected bool m_bChatRestricted;

	public OnlineAreaNewUserEvent OnParentalsChanged;

	protected List<DLCItem> m_DLCItems = new List<DLCItem>();

	public DLCUpdatedEvent OnDLCUpdatedEvent;

	public PlatformPauseRequest m_PauseCallBack;

	public PlatformPauseRequest m_UnPauseCallBack;

	protected bool m_bInitialized;

	protected bool m_bControllerVibrationEnabled = true;

	public static readonly string m_MatchmakingString = "Escapists2_Desktop_Crossplay_Final_1.0." + BuildVersion.m_ChangeList;

	public CrossplayLobbyManager m_CrossplayLobbyManager;

	protected bool m_bProgressedPastISS;

	public string InvitedRoomNameInvalid => "T17_Invalid_Invited_RoomName";

	public string InvitedRoomNameAlreadyFull => "T17_Full_Invited_RoomName";

	public bool IsInitialized => m_bInitialized;

	public bool IsPassedISS => m_bProgressedPastISS;

	public static bool controllerVibrationEnabled
	{
		get
		{
			return IsPlatformCreated() && m_TheInstance.IsControllerVibrationEnabled();
		}
		set
		{
			if (IsPlatformCreated())
			{
				m_TheInstance.SetControllerVibration(value);
			}
		}
	}

	public static event OnReadyToConnect OnReadyToConnectToPhoton;

	public static event OnResumeFromSuspendedHandler OnResumeFromSuspended;

	public static event LeaderboardReadComplete OnLeaderboardReadComplete;

	public static event LeaderboardPostComplete OnLeaderboardPostComplete;

	protected virtual void Awake()
	{
		Debug.Log("  ************  Platform       Awake *******  ");
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	protected virtual void Start()
	{
		m_StatsTracking = Resources.Load<StatsTracking>("StatsTracking");
		m_StatSystem = base.gameObject.GetComponent<StatSystem>();
		m_bPlatformCreated = true;
		m_OnNetworkChangedEvent = (OnNetworkEvent)Delegate.Remove(m_OnNetworkChangedEvent, new OnNetworkEvent(OnNetworkStatusChanged));
		m_OnNetworkChangedEvent = (OnNetworkEvent)Delegate.Combine(m_OnNetworkChangedEvent, new OnNetworkEvent(OnNetworkStatusChanged));
		ReInput.ControllerDisconnectedEvent += ReInput_ControllerDisconnectedEvent;
		ReInput.ControllerPreDisconnectEvent += ReInput_ControllerPreDisconnectEvent;
		ReInput.ControllerConnectedEvent += ReInput_ControllerConnectedEvent;
		m_CrossplayLobbyManager = base.gameObject.AddComponent<CrossplayLobbyManager>();
	}

	protected virtual void OnDestroy()
	{
		ReInput.ControllerDisconnectedEvent -= ReInput_ControllerDisconnectedEvent;
		ReInput.ControllerPreDisconnectEvent -= ReInput_ControllerPreDisconnectEvent;
		ReInput.ControllerConnectedEvent -= ReInput_ControllerConnectedEvent;
	}

	public void SetupStatSystem()
	{
		if (!m_StatsTracking)
		{
			Debug.Log("  ************  Platform       SetupStatSystem           StatsTracking     is   null   *******  ");
		}
		if (!(m_StatsTracking != null) || !(m_StatSystem != null))
		{
			return;
		}
		m_StatSystem.InitStats(m_StatsTracking.m_Stats.Length, m_StatsTracking.m_Tropies.Length, m_StatsTracking.m_Milestones.Length);
		for (int i = 0; i < m_StatsTracking.m_Stats.Length; i++)
		{
			m_StatSystem.CreateStat(m_StatsTracking.m_Stats[i].m_ID.ToString(), (int)m_StatsTracking.m_Stats[i].m_ID, (int)m_StatsTracking.m_Stats[i].m_StatType);
		}
		for (int i = 0; i < m_StatsTracking.m_Tropies.Length; i++)
		{
			m_StatSystem.CreateTrophy(m_StatsTracking.m_Tropies[i].m_Name, m_StatsTracking.m_Tropies[i].m_APIName, m_StatsTracking.m_Tropies[i].m_TrophyID, m_StatsTracking.m_Tropies[i].m_Rules.Length, (int)m_StatsTracking.m_Tropies[i].m_CombineMode);
			for (int j = 0; j < m_StatsTracking.m_Tropies[i].m_Rules.Length; j++)
			{
				m_StatSystem.SetUpTrophy(m_StatsTracking.m_Tropies[i].m_TrophyID, (int)m_StatsTracking.m_Tropies[i].m_Rules[j].m_StatID, m_StatsTracking.m_Tropies[i].m_Rules[j].m_RefValue, (int)m_StatsTracking.m_Tropies[i].m_Rules[j].m_Compare);
			}
		}
		for (int i = 0; i < m_StatsTracking.m_Milestones.Length; i++)
		{
			m_StatSystem.CreateMilestone(m_StatsTracking.m_Milestones[i].m_Milestone.id, m_StatsTracking.m_Milestones[i].m_Milestone.criteria.Length, (int)m_StatsTracking.m_Milestones[i].m_Milestone.evaluationType);
			for (int j = 0; j < m_StatsTracking.m_Milestones[i].m_Milestone.criteria.Length; j++)
			{
				m_StatSystem.SetUpMilestone(m_StatsTracking.m_Milestones[i].m_Milestone.id, (int)m_StatsTracking.m_Milestones[i].m_Milestone.criteria[j].statRule.m_StatID, m_StatsTracking.m_Milestones[i].m_Milestone.criteria[j].statRule.m_RefValue, (int)m_StatsTracking.m_Milestones[i].m_Milestone.criteria[j].statRule.m_Compare);
			}
		}
		m_StatSystem.InitDone();
	}

	public static bool CreatePlatform(GameObject parent)
	{
		if (parent == null)
		{
			parent = new GameObject("Platform");
		}
		parent.AddComponent<StatSystem>();
		parent.AddComponent<LinuxSteamPlatform>();
		parent.AddComponent<PCPlatformIO>();
		return true;
	}

	public static Platform GetInstance()
	{
		return m_TheInstance;
	}

	public static bool IsPlatformCreated()
	{
		if (m_TheInstance == null || PlatformIO.GetInstance() == null || !m_TheInstance.m_bPlatformCreated)
		{
			return false;
		}
		return true;
	}

	protected virtual void Update()
	{
		if (!(GlobalStart.GetInstance() != null))
		{
			return;
		}
		GlobalStart.GLOBALSTART_MODE currentGlobalStartMode = GlobalStart.GetInstance().CurrentGlobalStartMode;
		if ((currentGlobalStartMode != GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND && currentGlobalStartMode != GlobalStart.GLOBALSTART_MODE.IN_LEVEL && currentGlobalStartMode != GlobalStart.GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS && currentGlobalStartMode != GlobalStart.GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR) || m_bHavePrimarySignOutDialog)
		{
			return;
		}
		for (int i = 0; i < m_SignoutRequests.Count; i++)
		{
			if (m_SignoutRequests[i].m_bIsPrimary)
			{
				ProcessSignOutRequest(m_SignoutRequests[i]);
				m_SignoutRequests.RemoveAt(i);
				m_bHavePrimarySignOutDialog = true;
				break;
			}
		}
		if (m_bHavePrimarySignOutDialog)
		{
			return;
		}
		bool flag = false;
		if (m_ControllersPollingDueToDisconnect > 0)
		{
			for (int j = 0; j < m_ControllerDisconnectionInfo.Length; j++)
			{
				if (m_ControllerDisconnectionInfo[j] == null)
				{
					continue;
				}
				if (m_ControllerDisconnectionInfo[j].DialogBox == null)
				{
					if (j == 4)
					{
						m_ControllerDisconnectionInfo[j].DialogBox = T17DialogBoxManager.GetDialog(forSingleUser: false);
					}
					else
					{
						m_ControllerDisconnectionInfo[j].DialogBox = T17DialogBoxManager.GetDialog(forSingleUser: false, m_ControllerDisconnectionInfo[j].AttachedGamer.m_PlayerObject, showOverPauseMenu: true);
					}
					if (m_ControllerDisconnectionInfo[j].DialogBox != null)
					{
						m_ControllerDisconnectionInfo[j].DialogBox.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.ControllerDisconnect", "Text.Dialog.ControllerDisconnect.Body", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						m_ControllerDisconnectionInfo[j].DialogBox.Show();
						m_ControllerDisconnectionInfo[j].DialogBox.m_Message.m_bAlwaysUseControllerIcons = true;
						m_ControllerDisconnectionInfo[j].DialogBox.m_Message.CheckMarkup();
						m_ControllerDisconnectionInfo[j].DialogBox.m_Message.SetVerticesDirty();
						T17DialogBox dialogBox = m_ControllerDisconnectionInfo[j].DialogBox;
						dialogBox.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialogBox.OnConfirm, new T17DialogBox.DialogEvent(OnControllerDisconnectedConfirmedStandAlone));
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
			}
		}
		if (!flag && m_SignedOutControllerIndex == -1 && m_SignoutRequests.Count > 0)
		{
			ProcessSignOutRequest(m_SignoutRequests[0]);
			m_SignoutRequests.RemoveAt(0);
		}
	}

	private void OnControllerDisconnectedConfirmed(T17DialogBox dialog)
	{
		dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Remove(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnControllerDisconnectedConfirmed));
		for (int i = 0; i < m_ControllerDisconnectionInfo.Length; i++)
		{
			if (m_ControllerDisconnectionInfo[i] == null || !(m_ControllerDisconnectionInfo[i].DialogBox == dialog))
			{
				continue;
			}
			PauseMenu openPauseMenuInstance = PauseMenu.GetOpenPauseMenuInstance();
			if (T17NetManager.OfflineMode && m_ControllersPollingDueToDisconnect == 1 && openPauseMenuInstance == null)
			{
				Time.timeScale = 1f;
			}
			if (!m_ControllerDisconnectionInfo[i].AttachedGamer.m_bPrimaryLocal)
			{
				if (openPauseMenuInstance != null && openPauseMenuInstance.GetOwner() == m_ControllerDisconnectionInfo[i].AttachedGamer.m_PlayerObject)
				{
					openPauseMenuInstance.ResumeGame();
				}
				if (m_ControllerDisconnectionInfo[i].AttachedGamer.m_bActive)
				{
					T17NetworkManager.GetInstance().DeleteGamer(m_ControllerDisconnectionInfo[i].AttachedGamer.m_NetViewID);
				}
				m_ControllerDisconnectionInfo[i].DialogBox.Hide();
				m_ControllerDisconnectionInfo[i].Reset();
				m_ControllerDisconnectionInfo[i] = null;
				m_ControllersPollingDueToDisconnect--;
			}
		}
	}

	public virtual void OnControllerDisconnectedConfirmedStandAlone(T17DialogBox dialogBox)
	{
		if (PauseMenu.GetOpenPauseMenuInstance() == null)
		{
			Time.timeScale = 1f;
		}
		if (dialogBox == null)
		{
			return;
		}
		dialogBox.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Remove(dialogBox.OnConfirm, new T17DialogBox.DialogEvent(OnControllerDisconnectedConfirmedStandAlone));
		T17RewiredStandaloneInputModule.ForceTextRefresh();
		int i = 0;
		for (int num = m_ControllerDisconnectionInfo.Length; i < num; i++)
		{
			if (m_ControllerDisconnectionInfo[i] != null && m_ControllerDisconnectionInfo[i].DialogBox != null && m_ControllerDisconnectionInfo[i].DialogBox == dialogBox)
			{
				m_ControllerDisconnectionInfo[i].Reset();
				m_ControllerDisconnectionInfo[i] = null;
				m_ControllersPollingDueToDisconnect--;
				break;
			}
		}
	}

	public void ProgressedPastIIS()
	{
		m_bProgressedPastISS = true;
		if (TextIconManager.Instance != null)
		{
			TextIconManager.CacheControllerMaps();
		}
		SetAgeChecked(bValue: false);
		SignalReadyToConnectToPhoton();
	}

	public virtual bool IsReadyForPhoton()
	{
		return true;
	}

	public void SignalReadyToConnectToPhoton()
	{
		if (Platform.OnReadyToConnectToPhoton != null)
		{
			Platform.OnReadyToConnectToPhoton();
		}
	}

	public void StartingToLoadLevel()
	{
		m_bWasMasterBeforeSuspend = T17NetManager.IsMasterClient;
	}

	public void ExitingLevel()
	{
		m_bWasMasterBeforeSuspend = T17NetManager.IsMasterClient;
	}

	public void PreApplicationSuspend()
	{
		m_bWasMasterBeforeSuspend = T17NetManager.IsMasterClient;
	}

	public void SignalResumeFromSuspended()
	{
		if (Platform.OnResumeFromSuspended != null)
		{
			Platform.OnResumeFromSuspended();
		}
	}

	public bool WasMasterClientPreviously()
	{
		return m_bWasMasterBeforeSuspend;
	}

	public virtual void CleanupBeforeResetToBoot()
	{
		m_Error = PlatformError.None;
		Platform.OnLeaderboardReadComplete = null;
		Platform.OnLeaderboardPostComplete = null;
		OnNameFilteringCallback = null;
		m_ControllerDisconnectionInfo = new ControllerDisconnectionInfo[5];
		m_ControllersPollingDueToDisconnect = 0;
		m_SignedOutControllerIndex = -1;
		m_bHavePrimarySignOutDialog = false;
		m_UserSwitchDialog = null;
		m_SignoutRequests.Clear();
		m_OnlineAreaEntryCheckCallbacks.Clear();
		m_VoiceChatCheckCallback = null;
		m_UGCCheckCallback = null;
		m_OnlineAreaNewDisallowedUserCallback = null;
		m_RequestFriendsListCallback = null;
		OnParentalsChanged = null;
		m_bUGCRestricted = false;
		m_bChatRestricted = false;
		m_DLCItems.Clear();
		OnDLCUpdatedEvent = null;
		m_bControllerVibrationEnabled = true;
		m_bProgressedPastISS = false;
		m_CachedOverallScores.Clear();
		m_CachedMyScores.Clear();
		m_CachedFriendsScores.Clear();
	}

	public virtual bool StartDiscovery(bool bMainUser)
	{
		return false;
	}

	public virtual bool EndDiscovery(int padIndex, bool bIsPrimary)
	{
		return true;
	}

	public virtual bool FinishedDiscovery(int gamePadIndex)
	{
		return false;
	}

	public virtual void SetPresence(string text)
	{
	}

	public virtual void SetPresenceTag(string text)
	{
	}

	public virtual void SignalUserSignedOut(bool isPrimary, int controllerIndex, bool isSignOut = true)
	{
		if (m_SignoutRequests.Exists((SignOutInfo r) => r.m_ControllerID == controllerIndex))
		{
			return;
		}
		if (isPrimary)
		{
			PlatformIO.GetInstance().CancelAllIORequests();
			GlobalSave.GetInstance().ResetSave();
			if (GlobalStart.GetInstance().CurrentGlobalStartMode == GlobalStart.GLOBALSTART_MODE.SHOW_BOOT)
			{
				OnPrimaryUserSignedOutConfirmed(null);
				return;
			}
		}
		SignOutInfo signOutInfo = new SignOutInfo();
		signOutInfo.m_bIsPrimary = isPrimary;
		signOutInfo.m_ControllerID = controllerIndex;
		signOutInfo.m_IsSignOut = isSignOut;
		Gamer gamerByRewiredId = Gamer.GetGamerByRewiredId(controllerIndex);
		if (gamerByRewiredId != null)
		{
			signOutInfo.m_UserName = gamerByRewiredId.m_GamerName;
		}
		else
		{
			signOutInfo.m_UserName = "No Username";
		}
		m_SignoutRequests.Add(signOutInfo);
	}

	private void ProcessSignOutRequest(SignOutInfo info)
	{
		m_SignedOutControllerIndex = info.m_ControllerID;
		T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
		if (!(dialog != null))
		{
			return;
		}
		string localized = string.Empty;
		if (info.m_bIsPrimary)
		{
			if (info.m_IsSignOut)
			{
				Localization.Get("Text.Dialog.PrimaryUserSignOut.Body", out localized);
				if (localized.Contains("[User]"))
				{
					localized = localized.Replace("[User]", info.m_UserName);
				}
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.PrimaryUserSignOut", localized, "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: false);
			}
			else
			{
				Localization.Get("Text.Dialog.PrimaryUserChanged.Body", out localized);
				if (localized.Contains("[User]"))
				{
					localized = localized.Replace("[User]", info.m_UserName);
				}
				dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.PrimaryUserChanged", localized, "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: false);
			}
			dialog.Show();
			dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnPrimaryUserSignedOutConfirmed));
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
			return;
		}
		if (info.m_IsSignOut)
		{
			Localization.Get("Text.Dialog.SecondaryUserSignOut.Body", out localized);
			if (localized.Contains("[User]"))
			{
				localized = localized.Replace("[User]", info.m_UserName);
			}
			dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.SecondaryUserSignOut", localized, "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: false);
		}
		else
		{
			Localization.Get("Text.Dialog.SecondaryUserChanged.Body", out localized);
			if (localized.Contains("[User]"))
			{
				localized = localized.Replace("[User]", info.m_UserName);
			}
			dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.SecondaryUserChanged", localized, "Text.Dialog.Prompt.Ok", string.Empty, string.Empty, T17DialogBox.Symbols.Warning, bLocalizeTitle: true, bLocalizeMessage: false);
		}
		dialog.Show();
		dialog.OnConfirm = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnConfirm, new T17DialogBox.DialogEvent(OnSecondaryUserSignedOutConfirmed));
	}

	private void OnPrimaryUserSignedOutConfirmed(T17DialogBox box)
	{
		GlobalStart.GetInstance().ResetBackToPressAToStart();
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			Rewired.Player player = ReInput.players.GetPlayer(i);
			if (player != null)
			{
				T17EventSystem.ApplyCategories(player, T17EventSystem.InputCateogryStates.Disabled);
			}
		}
		m_SignedOutControllerIndex = -1;
	}

	private void OnSecondaryUserSignedOutConfirmed(T17DialogBox box)
	{
		if (m_SignedOutControllerIndex == -1)
		{
			return;
		}
		if (FrontEndFlow.Instance != null)
		{
			FrontendMenuBehaviour frontendMenuBehaviour = FrontEndFlow.Instance.InMultiUserMenu();
			if (frontendMenuBehaviour != null && frontendMenuBehaviour.GetType() == typeof(CoopFrontEndMenu))
			{
				((CoopFrontEndMenu)frontendMenuBehaviour).RemoveGamer(m_SignedOutControllerIndex);
			}
		}
		else
		{
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int i = 0; i < allGamers.Length; i++)
			{
				if (allGamers[i] != null && allGamers[i].IsLocal() && allGamers[i].m_iControllerIndex == m_SignedOutControllerIndex)
				{
					if (allGamers[i].m_RewiredPlayer != null)
					{
						T17EventSystem.ApplyCategories(allGamers[i].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
					}
					Gamer.DeleteGamer(i, clearRewiredMaps: true);
					break;
				}
			}
		}
		m_SignedOutControllerIndex = -1;
	}

	public virtual string GetUniqueID()
	{
		return "UNSET";
	}

	public PlatformError GetPlatformError()
	{
		PlatformError error = m_Error;
		m_Error = PlatformError.None;
		return error;
	}

	public virtual bool StartOSK(int controllerIndex, string placeholderText, OSKInfo setupInfo = null)
	{
		return false;
	}

	public virtual void DismissOSK(int controllerIndex)
	{
	}

	public virtual OSKGetTextResponses GetOSKText(out string currentText)
	{
		currentText = null;
		return OSKGetTextResponses.Undefined;
	}

	public virtual string GetUserNameByControllerIndex(int controllerIndex)
	{
		return "UNKNOWN";
	}

	public string GetClippedUserNameByControllerIndex(int controllerIndex)
	{
		string text = GetUserNameByControllerIndex(controllerIndex);
		if (text.Length > 16)
		{
			text = text.Substring(0, 16);
			text += "...";
		}
		return text;
	}

	public virtual string GetPrimaryUserName()
	{
		return Environment.UserName;
	}

	public string GetClippedPrimaryUserName()
	{
		string text = GetPrimaryUserName();
		if (text.Length > 16)
		{
			text = text.Substring(0, 16);
			text += "...";
		}
		return text;
	}

	public virtual void PostToFeed(ACTIVITY_FEED_IDS subType, string capt, string conCapt, uint AFID)
	{
	}

	protected virtual void ReInput_ControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
	{
		if (!m_bProgressedPastISS)
		{
			return;
		}
		for (int i = 0; i < m_ControllerDisconnectionInfo.Length; i++)
		{
			if (m_ControllerDisconnectionInfo[i] == null || m_ControllerDisconnectionInfo[i].ControllerID != obj.controllerId || m_ControllerDisconnectionInfo[i].DisconnectedControllerType != obj.controllerType)
			{
				continue;
			}
			Rewired.Player player = null;
			player = GetRewiredPlayerFrom(obj.controllerId, obj.controllerType);
			if (player != null)
			{
				if (player.controllers.hasMouse && T17RewiredStandaloneInputModule.GetCurrentPCKeyboardMode() == ControlSetting.Keyboard)
				{
					IList<Rewired.Player> players = ReInput.players.Players;
					int j = 0;
					for (int count = players.Count; j < count; j++)
					{
						Rewired.Player player2 = players[j];
						if (player2 != player && player2.controllers.joystickCount == 0)
						{
							player2.controllers.AddController(player.controllers.GetController(obj.controllerType, obj.controllerId), removeFromOtherPlayers: true);
							player = player2;
							break;
						}
					}
				}
				if (m_ControllerDisconnectionInfo[i].RewiredPlayerIndex != player.id && m_ControllerDisconnectionInfo[i].AttachedGamer != null)
				{
					bool flag = false;
					if (obj.controllerType != ControllerType.Joystick)
					{
						flag = true;
					}
					else
					{
						Rewired.Player player3 = ReInput.players.GetPlayer(m_ControllerDisconnectionInfo[i].RewiredPlayerIndex);
						if (!player3.controllers.hasMouse)
						{
							flag = true;
						}
						else if (m_ControllerDisconnectionInfo[i].DialogBox != null && m_ControllerDisconnectionInfo[i].DialogBox.IsActive)
						{
							Controller controller = player.controllers.GetController(obj.controllerType, obj.controllerId);
							Controller controller2 = player3.controllers.Joysticks[0];
							player.controllers.RemoveController(controller);
							player3.controllers.RemoveController(controller2);
							player3.controllers.AddController(controller, removeFromOtherPlayers: true);
							player.controllers.AddController(controller2, removeFromOtherPlayers: true);
							T17EventSystem.ReApplyCurrentState(player3.id);
							T17EventSystem.ReApplyCurrentState(player.id);
						}
					}
					if (flag)
					{
						m_ControllerDisconnectionInfo[i].AttachedGamer.UpdateGamer(player.id, PhotonNetwork.player.ID, m_ControllerDisconnectionInfo[i].AttachedGamer.m_NetViewID, null, null, bPrimarySet: false, bPrimary: false, null);
					}
				}
				AddRewiredPlayer(player.id);
			}
			if (player != null && m_DisconnectedPlayerControllers.ContainsKey(player.id))
			{
				m_DisconnectedPlayerControllers.Remove(player.id);
			}
			if (m_ControllerDisconnectionInfo[i].DialogBox != null)
			{
				m_ControllerDisconnectionInfo[i].DialogBox.Hide();
			}
			else if (player != null)
			{
				T17EventSystem.ApplyLastRequestedStateSinceDialogboxWasUp(player);
			}
			if (m_ControllerDisconnectionInfo[i].playerMapsSnapshot != null)
			{
				m_ControllerDisconnectionInfo[i].playerMapsSnapshot.RestoreControllerMaps();
			}
			m_ControllerDisconnectionInfo[i].Reset();
			m_ControllerDisconnectionInfo[i] = null;
			m_ControllersPollingDueToDisconnect--;
			PauseMenu openPauseMenuInstance = PauseMenu.GetOpenPauseMenuInstance();
			if (T17NetManager.OfflineMode && m_ControllersPollingDueToDisconnect == 0 && openPauseMenuInstance == null)
			{
				Time.timeScale = 1f;
			}
			return;
		}
		Rewired.Player player4 = GetRewiredPlayerFrom(obj.controllerId, obj.controllerType);
		if (player4 == null)
		{
			return;
		}
		if (player4.controllers.hasMouse && T17RewiredStandaloneInputModule.GetCurrentPCKeyboardMode() == ControlSetting.Keyboard)
		{
			IList<Rewired.Player> players2 = ReInput.players.Players;
			int k = 0;
			for (int count2 = players2.Count; k < count2; k++)
			{
				Rewired.Player player5 = players2[k];
				if (player5 != player4 && player5.controllers.joystickCount == 0)
				{
					player5.controllers.AddController(player4.controllers.GetController(obj.controllerType, obj.controllerId), removeFromOtherPlayers: true);
					player4 = player5;
					break;
				}
			}
		}
		T17EventSystem.ReApplyCurrentState(player4);
	}

	protected virtual void SetGamerControllerDisconnected(Gamer gamer, bool isPrimary)
	{
	}

	public Controller GetDisconnectedControllerForRewiredPlayer(int rewiredPlayerID)
	{
		if (m_DisconnectedPlayerControllers.ContainsKey(rewiredPlayerID))
		{
			m_DisconnectedPlayerControllers.TryGetValue(rewiredPlayerID, out var value);
			return value;
		}
		return null;
	}

	protected virtual void ReInput_ControllerPreDisconnectEvent(ControllerStatusChangedEventArgs obj)
	{
		if (TextIconManager.Instance != null)
		{
			TextIconManager.CacheControllerMaps();
		}
		if (!m_bProgressedPastISS)
		{
			return;
		}
		Rewired.Player rewiredPlayerFrom = GetRewiredPlayerFrom(obj.controllerId, obj.controllerType);
		Gamer gamer = null;
		bool flag = false;
		if (rewiredPlayerFrom == null)
		{
			return;
		}
		if (m_DisconnectedPlayerControllers.ContainsKey(rewiredPlayerFrom.id))
		{
			m_DisconnectedPlayerControllers.Remove(rewiredPlayerFrom.id);
		}
		if (rewiredPlayerFrom.controllers.ContainsController(obj.controllerType, obj.controllerId))
		{
			Controller controller = rewiredPlayerFrom.controllers.GetController(obj.controllerType, obj.controllerId);
			if (controller != null)
			{
				m_DisconnectedPlayerControllers.Add(rewiredPlayerFrom.id, controller);
			}
		}
		if (FrontEndFlow.Instance != null)
		{
			FrontendMenuBehaviour frontendMenuBehaviour = FrontEndFlow.Instance.InMultiUserMenu();
			if (frontendMenuBehaviour != null && frontendMenuBehaviour.GetType() == typeof(CoopFrontEndMenu))
			{
				Gamer[] allGamers = Gamer.GetAllGamers();
				int num = allGamers.Length;
				for (int i = 0; i < num; i++)
				{
					if (allGamers[i] != null && allGamers[i].IsLocal() && allGamers[i].m_iControllerIndex == rewiredPlayerFrom.id)
					{
						if (allGamers[i] == Gamer.GetPrimaryGamer())
						{
							flag = true;
							break;
						}
						RemoveRewiredPlayer(rewiredPlayerFrom.id);
						((CoopFrontEndMenu)frontendMenuBehaviour).RemoveGamer(rewiredPlayerFrom.id);
						break;
					}
				}
			}
			if (m_UserSwitchDialog != null && m_ControllerDisconnectionInfo[4] != null && m_ControllerDisconnectionInfo[4].AttachedGamer.m_iControllerIndex == rewiredPlayerFrom.id)
			{
				m_UserSwitchDialog.Decline();
				m_UserSwitchDialog = null;
			}
			if ((flag || Gamer.GetPrimaryGamer().m_iControllerIndex == rewiredPlayerFrom.id) && m_ControllerDisconnectionInfo[4] == null)
			{
				flag = true;
				gamer = Gamer.GetPrimaryGamer();
			}
		}
		else
		{
			Gamer[] allGamers2 = Gamer.GetAllGamers();
			int num2 = allGamers2.Length;
			for (int j = 0; j < num2; j++)
			{
				int id = rewiredPlayerFrom.id;
				if (allGamers2[j] != null && allGamers2[j].IsLocal() && allGamers2[j].m_iControllerIndex == rewiredPlayerFrom.id && m_ControllerDisconnectionInfo[id] == null)
				{
					gamer = allGamers2[j];
					break;
				}
			}
		}
		if (gamer == null)
		{
			return;
		}
		ControllerDisconnectionInfo controllerDisconnectionInfo = new ControllerDisconnectionInfo();
		controllerDisconnectionInfo.RewiredPlayerIndex = rewiredPlayerFrom.id;
		controllerDisconnectionInfo.ControllerID = obj.controllerId;
		controllerDisconnectionInfo.DisconnectedControllerType = obj.controllerType;
		controllerDisconnectionInfo.AttachedGamer = gamer;
		if (flag)
		{
			m_ControllerDisconnectionInfo[4] = controllerDisconnectionInfo;
			m_ControllerDisconnectionInfo[4].playerMapsSnapshot = PlayerMapsSnapshot.CreateSnapshotForGamer(gamer, disableAllMaps: false, bFullSnapshot: true);
			if (T17EventSystem.GetStateForRewiredPlayer(gamer.m_RewiredPlayer) == T17EventSystem.InputCateogryStates.Dialogbox)
			{
				m_ControllerDisconnectionInfo[4].playerMapsSnapshot.OverrideCapturedSelected(T17EventSystem.GetPrevStateForRewiredPlayer(gamer.m_RewiredPlayer));
				m_ControllerDisconnectionInfo[4].playerMapsSnapshot.OverrideCapturedSelected(T17EventSystem.GetLastReqStateForRewiredPlayer(gamer.m_RewiredPlayer));
			}
		}
		else
		{
			m_ControllerDisconnectionInfo[rewiredPlayerFrom.id] = controllerDisconnectionInfo;
			if (T17NetManager.OfflineMode)
			{
				Time.timeScale = 0f;
			}
			m_ControllerDisconnectionInfo[rewiredPlayerFrom.id].playerMapsSnapshot = PlayerMapsSnapshot.CreateSnapshotForGamer(gamer, disableAllMaps: false, bFullSnapshot: true);
			if (T17EventSystem.GetStateForRewiredPlayer(gamer.m_RewiredPlayer) == T17EventSystem.InputCateogryStates.Dialogbox)
			{
				m_ControllerDisconnectionInfo[rewiredPlayerFrom.id].playerMapsSnapshot.OverrideCapturedSelected(T17EventSystem.GetPrevStateForRewiredPlayer(gamer.m_RewiredPlayer));
				m_ControllerDisconnectionInfo[rewiredPlayerFrom.id].playerMapsSnapshot.OverrideCapturedSelected(T17EventSystem.GetLastReqStateForRewiredPlayer(gamer.m_RewiredPlayer));
			}
		}
		SetGamerControllerDisconnected(gamer, isPrimary: false);
		m_ControllersPollingDueToDisconnect++;
		T17EventSystem.ApplyCategories(rewiredPlayerFrom, T17EventSystem.InputCateogryStates.Assignment);
	}

	public virtual bool ReInput_IsAnyControllerDisconnected()
	{
		return m_ControllersPollingDueToDisconnect > 0;
	}

	public virtual bool ReInput_IsRePlayerControllerDisconnected(Rewired.Player player)
	{
		if (player == null)
		{
			return true;
		}
		int i = 0;
		for (int num = m_ControllerDisconnectionInfo.Length; i < num; i++)
		{
			if (m_ControllerDisconnectionInfo[i] != null && m_ControllerDisconnectionInfo[i].RewiredPlayerIndex == player.id)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool ReInput_IsGamerControllerDisconnected(Gamer gamer)
	{
		if (gamer == null)
		{
			return false;
		}
		int i = 0;
		for (int num = m_ControllerDisconnectionInfo.Length; i < num; i++)
		{
			if (m_ControllerDisconnectionInfo[i] != null && m_ControllerDisconnectionInfo[i].AttachedGamer == gamer)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ReInput_SetControllerReconnectedPlayerMapsSnapshotState(Gamer gamer, T17EventSystem.InputCateogryStates newState, T17EventSystem.InputCateogryStates newPreviousState)
	{
		if (gamer == null)
		{
			return;
		}
		int i = 0;
		for (int num = m_ControllerDisconnectionInfo.Length; i < num; i++)
		{
			ControllerDisconnectionInfo controllerDisconnectionInfo = m_ControllerDisconnectionInfo[i];
			if (controllerDisconnectionInfo != null && controllerDisconnectionInfo.playerMapsSnapshot != null && controllerDisconnectionInfo.AttachedGamer == gamer)
			{
				controllerDisconnectionInfo.playerMapsSnapshot.OverrideCapturedSelected(newPreviousState);
				controllerDisconnectionInfo.playerMapsSnapshot.OverrideCapturedSelected(newState);
				break;
			}
		}
	}

	protected virtual void ReInput_ControllerDisconnectedEvent(ControllerStatusChangedEventArgs obj)
	{
		if (m_bProgressedPastISS)
		{
		}
	}

	private Rewired.Player GetRewiredPlayerFrom(int controllerID, ControllerType controllerType)
	{
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			Rewired.Player player = ReInput.players.GetPlayer(i);
			if (player != null && player.controllers.ContainsController(controllerType, controllerID))
			{
				return ReInput.players.GetPlayer(i);
			}
		}
		return null;
	}

	public virtual void RemoveAllSecondaryUsers()
	{
	}

	public virtual void RemoveUnusedUsers()
	{
	}

	public virtual void ShowGamerCard(Gamer requesterGamer, string requestedGamerID)
	{
	}

	public virtual bool OnlineCheck()
	{
		return true;
	}

	public virtual void GetOnlineAccessCode(OnlineAreaEntryResponderCallback callback, bool bShowSystemDialogues)
	{
		callback(OnlineAccessErrorCode.OnlineAccessOK, bShowSystemDialogues);
	}

	public virtual void OnlineAreaEntryCheckRequest(bool isLeaderboard, OnlineAreaEntryCheckCallback callback, bool isModal = true, bool bShowSystemErrors = true)
	{
		callback?.Invoke(bAllowedToProgress: true, OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: false);
	}

	protected virtual void OnlineAreaEntryCheckResponse(OnlineAccessErrorCode error, bool bShowSystemDialogues)
	{
	}

	public virtual void RefreshParentals()
	{
	}

	public virtual bool IsChatRestrictedRequest(OnlineAreaEntryCheckCallback callback)
	{
		return false;
	}

	public virtual bool IsUGCRestrictedRequest(OnlineAreaEntryCheckCallback callback)
	{
		callback?.Invoke(bAllowedToProgress: false, OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: false);
		return false;
	}

	public virtual void EnterOnlineArea(bool bIsLeaderboard, OnlineAreaNewUserEvent NewDisallowedUserCallback)
	{
	}

	public virtual bool DisplayNativeDialogForOnlineAccessCode(OnlineAccessErrorCode error)
	{
		return false;
	}

	public virtual void ExitOnlineArea()
	{
	}

	public virtual void OnNetworkStatusChanged(PlatformNetworkStatus status)
	{
		if (status != PlatformNetworkStatus.Connected)
		{
			ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PlatformDisconnected);
		}
		if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom() && status == PlatformNetworkStatus.Disconnected)
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, T17NetManager.SilentErrorDialogMode);
		}
	}

	public void ForceGameOffline()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode);
		ExitOnlineArea();
	}

	public virtual void SendInvite(string OnlineID)
	{
	}

	public virtual void OpenInvitePicker()
	{
	}

	public virtual void RequestFriendsList(RequestFriendsListCallback requestFriendsListCallback)
	{
		requestFriendsListCallback(new List<DisplayableFriend>());
	}

	public virtual void CancelFriendsListRequest()
	{
	}

	public virtual void JoinedSession(string photonRoom, SessionType sessionType, bool isMaster, bool bInvitesEnabled, PrisonConfig.ConfigType configType)
	{
	}

	public virtual void LeaveSession()
	{
	}

	public virtual void SessionTypeChange(SessionType newSessionType)
	{
	}

	public virtual void MakePlatformLobby()
	{
	}

	public virtual void JoinPlatformLobby()
	{
	}

	public virtual void RequestLeaderboard(LeaderboardType lbType, LevelScript.PRISON_ENUM ePrison, LeaderboardGameType lbGameType, LeaderboardReadComplete callback, int firstRow = 1, int numRows = 100, bool bShowErrors = true)
	{
		if (callback != null)
		{
			Platform.OnLeaderboardReadComplete = callback;
			Platform.OnLeaderboardReadComplete(bOK: false, null, 0, bShowError: false, bContentBlocked: false);
			Platform.OnLeaderboardReadComplete = null;
		}
	}

	public virtual void PostToLeaderboard(LevelScript.PRISON_ENUM ePrison, LeaderboardGameType lbGameType, LeaderboardPostComplete callback, int score, int extraData)
	{
		if (callback != null)
		{
			Platform.OnLeaderboardPostComplete = callback;
			Platform.OnLeaderboardPostComplete(bScoreWasBetter: false, bOk: false);
			Platform.OnLeaderboardPostComplete = null;
		}
	}

	public virtual void CancelRequestLeaderboard(CancelRequestLeaderboardCallback callback)
	{
		callback?.Invoke();
	}

	public virtual void CancelFilteringStringList()
	{
	}

	public virtual void FilterStringList(List<string> names, NameFilteringCallback callback)
	{
		callback(bOK: true, names);
	}

	public virtual void FilterString(ref string theString, bool useUsersSettings = true)
	{
	}

	public virtual void VoiceChatEntryPoint()
	{
	}

	public virtual void DisableVoiceChat()
	{
	}

	public virtual void GetTalkingGamers(ref List<VoiceChatGamer> voiceChatGamers)
	{
	}

	public virtual bool ToggleMuteForGamer(string uniquePlatformID)
	{
		return false;
	}

	public virtual bool IsGamerMuted(string uniquePlatformID)
	{
		return false;
	}

	public virtual void KickGamer(string uniquePlatformID)
	{
	}

	public virtual void SetSessionLocked(bool shouldLock)
	{
	}

	public virtual void JoinInvitedSessionRoom(bool isSessionPrivate)
	{
	}

	public virtual void GetSessionJoinInformation(out bool hasPassword)
	{
		hasPassword = false;
	}

	public virtual void UpdateJoinedSessionInformation(bool isSessionPrivate)
	{
	}

	public virtual void PhotonPlayerRequestsAccessToRoom(PhotonPlayer player)
	{
	}

	public virtual void JoinSessionViaSearch(string photonRoomName)
	{
	}

	public virtual bool IsUserDiscoveryInProgress()
	{
		return false;
	}

	public virtual bool HasRanMainDiscoveryBefore()
	{
		return true;
	}

	public virtual void DebugPrintInfoForNativeSession()
	{
	}

	public virtual void SetNativeVoiceChatEnabled(bool state)
	{
	}

	public bool IsAnyControllerConnectionEventsPending()
	{
		return false;
	}

	public bool GetAgeHasBeenChecked()
	{
		return m_bCheckedAge;
	}

	public void SetAgeChecked(bool bValue)
	{
		m_bCheckedAge = bValue;
	}

	protected virtual void SetControllerVibration(bool enable)
	{
		m_bControllerVibrationEnabled = enable;
	}

	protected virtual bool IsControllerVibrationEnabled()
	{
		return m_bControllerVibrationEnabled;
	}

	public void LoadData()
	{
		float value = 1f;
		GlobalSave.GetInstance().Get("Settings:Vibration", out value, 1f);
		m_bControllerVibrationEnabled = !(value < 0.5f);
		int value2 = 0;
		GlobalSave.GetInstance().Get("Settings:ControlOption", out value2, 0);
		T17RewiredStandaloneInputModule.SetPCKeyboardMode((ControlSetting)value2);
	}

	public virtual Rewired.Player CheckForAPress()
	{
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			if (ReInput.players.GetPlayer(i).GetButtonDown("Start"))
			{
				return ReInput.players.GetPlayer(i);
			}
		}
		return null;
	}

	public virtual bool CheckForAnyUserPress()
	{
		for (int i = 0; i < ReInput.players.playerCount; i++)
		{
			if (ReInput.players.GetPlayer(i).GetButtonDown("Start"))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void RemoveRewiredPlayer(int id)
	{
	}

	protected void OnApplicationPause(bool state)
	{
		if (state)
		{
			CheckAllLoggedInUsers();
		}
	}

	protected virtual void CheckAllLoggedInUsers()
	{
	}

	public virtual void AddRewiredPlayer(int id)
	{
	}

	public void DoControllerRumble(RumbleController rumble, int rewiredPlayerId)
	{
		try
		{
			if (m_bControllerVibrationEnabled)
			{
				Rewired.Player player = ReInput.players.GetPlayer(rewiredPlayerId);
				if (player != null && rumble != null)
				{
					rumble.m_Duration = Mathf.Clamp(rumble.m_Duration, 0.01f, 5f);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void AddDLCItem(DLCItem item)
	{
		bool flag = false;
		for (int i = 0; i < m_DLCItems.Count; i++)
		{
			if (m_DLCItems[i].productId == item.productId)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_DLCItems.Add(item);
		}
	}

	public virtual bool IsDLCAvailable(DLCFrontendData data)
	{
		return true;
	}

	public virtual void RefreshDLC()
	{
	}

	public virtual bool ShowDLCStorePage(string dlcEntitlementID)
	{
		return false;
	}

	public virtual void StartLightBarEffect(LightBarEffect light, int rewiredPlayerId)
	{
	}

	public virtual void StopLightBarEffect(int rewiredPlayerId)
	{
	}

	public virtual void SetLightBarData(int rewiredID, Color defaultColor)
	{
	}

	public virtual void ResetLightBar()
	{
	}

	public virtual void ResetLightBar(int rewiredID)
	{
	}

	public virtual void NotifyInMultiplayer()
	{
	}

	public virtual void SetAchievementProgress(string APIName, int progress)
	{
	}

	public virtual void UnlockAchievement(string APIName)
	{
	}

	public virtual string GetMatchmakingVersionNumber()
	{
		return m_MatchmakingString;
	}

	public virtual void RegisterOnUGCChange(OnUserGeneratedContentUpdated ugcEvent)
	{
	}

	public virtual void UnRegisterOnUGCChange(OnUserGeneratedContentUpdated ugcEvent)
	{
	}

	public virtual void RegisterUGCUploadCallback(UserGeneratedContentUploadCallback callback)
	{
	}

	public virtual void UnRegisterUGCUploadCallback(UserGeneratedContentUploadCallback callback)
	{
	}

	public virtual void EnumerateUGCItems(ref List<UGCItem> ugcList, UGCType filter = UGCType.eNone)
	{
	}

	public virtual bool EnumeratePublishedUGCItems(UGCType filter = UGCType.eNone, OnPublishedItemsRecieved callback = null)
	{
		return false;
	}

	public virtual int UploadUGCItem(UGCUploadData uploadData)
	{
		return -1;
	}

	public virtual void RequestShowUGCTerms()
	{
	}

	public virtual void RequestShowUGCHomePage()
	{
	}

	public virtual void UnsubscribedFromContent(UGCItem ugcToRemove)
	{
	}

	public virtual void ResetPlaytogether()
	{
	}

	protected void RaiseOnlineAreaEntryCheckCallbacks(bool bAllowedToProgress, OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		int count = m_OnlineAreaEntryCheckCallbacks.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_OnlineAreaEntryCheckCallbacks[i] != null)
			{
				m_OnlineAreaEntryCheckCallbacks[i](bAllowedToProgress, returnCode, failureHandledPlatformside);
			}
		}
		m_OnlineAreaEntryCheckCallbacks.Clear();
	}

	protected bool AddNewOnlineAreaEntryCheckCallback(OnlineAreaEntryCheckCallback newCallback)
	{
		m_OnlineAreaEntryCheckCallbacks.Add(newCallback);
		return m_OnlineAreaEntryCheckCallbacks.Count == 1;
	}

	public virtual bool SupportsDifferentQualitySettings()
	{
		return false;
	}

	public static string GetStreamingAssetsPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "Linux");
	}

	public virtual string StreamingAssetsPath()
	{
		return Application.streamingAssetsPath;
	}

	public virtual void InjectADisconnectEvent()
	{
	}

	public virtual SystemLanguage GetDesiredLanguage()
	{
		return Application.systemLanguage;
	}

	public virtual bool RegisterNetworkEncryptionKey(byte[] keyData, byte[] keyInitVector, int keyId)
	{
		return true;
	}

	public virtual void UnregisterNetworkEncryptionKey(int keyId)
	{
	}

	public virtual bool ActivateNetworkEncryptionKey(int keyId)
	{
		return true;
	}

	public virtual byte[] EncryptForNetwork(byte[] inputData)
	{
		return inputData;
	}

	public virtual byte[] DecryptFromNetwork(byte[] inputData, out int keyIdUsed, out int numBytesDecrypted)
	{
		keyIdUsed = 0;
		numBytesDecrypted = inputData.Length;
		return inputData;
	}

	public virtual string GetAuthToken()
	{
		return string.Empty;
	}

	public static void DEBUG_LogNativeSessionInformation()
	{
		GetInstance().DebugPrintInfoForNativeSession();
	}

	public virtual bool GetSaveDirectory()
	{
		return true;
	}
}
