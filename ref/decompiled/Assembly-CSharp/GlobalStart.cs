using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AUTOGEN_T17Wwise_Enums;
using CodingJar;
using CodingJar.MultiScene;
using NetworkLoadable;
using ParadoxNotion.Serialization;
using Pathfinding;
using Rewired;
using Slate;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalStart : T17MonoBehaviour
{
	public enum ReturnToFrontendRoutes
	{
		None,
		Versus,
		VersusLobby
	}

	public enum GLOBALSTART_GAME_MODES
	{
		SINGLE,
		LOCAL,
		ONLINE
	}

	public delegate void StateChangedHandler();

	public enum GLOBALSTART_MODE
	{
		INIT,
		START_LOAD_REWIRED,
		WAIT_FOR_LOAD_REWIRED,
		START_LOAD_BOOT,
		WAIT_FOR_LOAD_BOOT,
		SHOW_BOOT,
		KILL_BOOT,
		WAIT_FOR_KILL_BOOT,
		START_LOAD_FRONTEND,
		WAIT_FOR_LOAD_FRONTEND,
		CHECK_INVITES,
		WAIT_FOR_INVITES_ONLINE_CHECK,
		PROCESSING_INVITE,
		SHOW_FRONTEND,
		START_LEVEL_LOAD,
		KILL_FRONTEND,
		WAIT_FOR_KILL_FRONTEND,
		LOADING_LEVEL,
		WAIT_FOR_LOADING_LEVEL,
		LOADING_OTHER_INGAME_SCENES_HUD,
		WAIT_FOR_OTHER_SCENES_HUD,
		WAIT_FOR_OTHER_SCENES_IGM,
		SETUP_AREA_MANAGERS,
		WAIT_FOR_OTHER_SCENES_PART2,
		SETUP_ITEM_MANAGER,
		WAIT_FOR_OTHER_SCENES_PART3,
		WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS,
		WAIT_FOR_CUSTOMISATION,
		LOAD_CUSTOMISATION,
		WAIT_FOR_INIT_LEVEL_ITEMS,
		WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS,
		SPAWN_LEVEL_PLAYER_OBJECTS,
		SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS,
		REQUEST_PLAYER_STARTING_ITEMS,
		WAIT_FOR_PLAYER_STARTING_ITEMS,
		WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS,
		NETWORK_INIT_MANAGERS,
		INIT_MANAGERS,
		WAIT_FOR_PLAYERS,
		CHECK_INVITES_DURING_LOAD,
		IN_LEVEL,
		END_LEVEL,
		WAIT_FOR_END_LEVEL,
		WAIT_FOR_END_LEVEL_GC,
		LOAD_RESULTS,
		WAIT_FOR_LOAD_RESULTS,
		END_LEVEL_BEHIND_RESULTS,
		WAIT_FOR_END_LEVEL_BEHIND_RESULTS,
		RELOAD_FRONTEND,
		WAIT_FOR_RELOAD_FRONTEND,
		LOADING_FLOW_DUMMY_STATE,
		WAIT_FOR_PROFANITY_FILTER,
		SHOW_CREDITS_START,
		WAIT_FOR_CREDITS_LOAD,
		CREDITS,
		HIDE_CREDITS_BACK_TO_FRONTEND,
		LEVEL_EDITOR__ENTER__START_LEVEL_EDITOR,
		LEVEL_EDITOR__ENTER__KILL_FRONTEND,
		LEVEL_EDITOR__ENTER__WAIT_KILL_FRONTEND,
		LEVEL_EDITOR__ENTER__LOAD_LEVEL_EDITOR,
		LEVEL_EDITOR__ENTER__WAIT_LEVEL_EDITOR,
		LEVEL_EDITOR__ENTER__WAIT_LEVEL_CREATION,
		LEVEL_EDITOR__ENTER__KILL_LOADING,
		LEVEL_EDITOR__IN_EDITOR,
		LEVEL_EDITOR__EXIT__START_EXIT,
		LEVEL_EDITOR__EXIT__KILL_LEVEL_EDITOR,
		LEVEL_EDITOR__EXIT__WAIT_KILL_LEVEL_EDITOR,
		LEVEL_EDITOR__EXIT__RELOAD_FRONTEND,
		LEVEL_EDITOR__EXIT__WAIT_FOR_RELOAD_FRONTEND
	}

	public delegate void FinishedFiltering();

	public class PersistentScriptComponent<T> where T : MonoBehaviour
	{
		private T m_value;

		public void Awake(Component parent)
		{
			m_value = parent.GetComponent<T>();
		}

		public T value(bool silentFail = false)
		{
			if (m_value == null && !silentFail && !IsApplicationQuitting)
			{
				UnityEngine.Debug.LogErrorFormat("PersistentScripts.{0} - value is null", typeof(T).Name);
			}
			return m_value;
		}

		public static explicit operator T(PersistentScriptComponent<T> psc)
		{
			return psc.value();
		}
	}

	public static float START_TIME = 0f;

	public static string m_VersionString = BuildVersion.m_VersionString;

	public static bool m_bShowDebugElements = false;

	public static bool m_bShowDebugFPSText = false;

	public static bool m_bShowDebugText = false;

	public static bool m_bDebugElementOverride = true;

	public static string m_DebugElementOverride = string.Empty;

	private static GlobalStart m_TheInstance = null;

	private YieldInstruction m_Async;

	private bool m_bRewiredLoaded;

	private bool m_bBootLoaded;

	private bool m_bLoadedLoadingScene;

	private bool m_bStartedLoadingSceneLoad;

	private bool m_bFrontEndLoaded;

	private bool m_bStartedFrontEndSceneLoad;

	private bool m_bLevelLoaded;

	private bool m_bLevelFailedToLoad;

	private bool m_bHUDMenuLoaded;

	private bool m_bLevelEditorLoaded;

	private string m_strCustomLevelFile = string.Empty;

	private bool m_bInGameMenusLoaded;

	private bool m_bResultsLoaded;

	private bool m_bCreditsLoaded;

	private bool m_bGarbageCollected;

	private List<Transform> m_LevelSceneRoots = new List<Transform>();

	private bool m_bLoadBackToBootFlow;

	private bool m_bProcessedLoadBackToBootFlow;

	private bool m_bLoadError;

	private string m_LoadErrorDescription;

	private bool m_bDARTTest;

	private const float DELAY_TIME = 5f;

	private static Stopwatch m_TimedNetworkServiceStopWatch = null;

	private static bool m_bTimedNetworkServiceStopWatchCreated = false;

	private Dictionary<BaseFlowBehaviour.FlowType, BaseFlowBehaviour> m_RegisteredFlows;

	private string m_CurrentLevelSceneName;

	private int m_CurrentLevelConfigID;

	private PlaylistData.NetPrisonSetup m_CurrentPrisonSetup;

	private PlaylistData m_CurrentPlaylistData;

	private GLOBALSTART_GAME_MODES m_GameMode = GLOBALSTART_GAME_MODES.LOCAL;

	public ReturnToFrontendRoutes m_ReturnToFrontendRoute;

	public GameObject m_ManagersGORef;

	public float[] m_SupportedAspectRatios;

	public const string NO_DATA = "Intentionally Left Blank";

	private float m_fSerializeTime;

	private NetBluePrintDetails m_netBluePrintDetails;

	[NonSerialized]
	public List<byte> m_CustomLevelData = new List<byte>();

	[NonSerialized]
	public bool m_CustomLevel;

	[NonSerialized]
	public bool m_PreviewEditorLevel;

	private List<string> m_LoadedSceneNamesToDelete = new List<string>();

	public LevelEditor_Settings m_EditorSettings;

	private static GameObject m_googleAnalyticsV3GO = null;

	private const string m_googleAnalyticsV3PreFabName = "Prefabs/Network/GoogleAnalyticsV3/GAv3";

	private static bool g_bGoogleAnalyticsEnabled = true;

	private static GameObject m_googleAnalyticsV3PreFab;

	private bool m_wasMasterClientAtStartOfLevelLoad;

	private Coroutine m_CountTimeInEditorRoutine;

	private List<int> m_NameFilteringIndexCharacterIdMap;

	private static bool m_Debug_NoRandomPlaylists = false;

	private bool m_bWaitForSettingsToCloseForInvite;

	private PauseMenu m_OpenPauseMenu;

	private bool m_inviteAcceptedDuringCredits;

	private bool m_bWaitingForEditorInviteResponse;

	private bool m_bLockInWithLevelReturn;

	private Coroutine m_DelayedHideLoadingScreenRoutine;

	public Mesh m_StencilClearQuad;

	public Material m_StencilClearMaterial;

	private int m_SlicedSetupLoop;

	private GLOBALSTART_MODE _m_LastGlobalStartMode;

	private GLOBALSTART_MODE _m_GlobalStartMode;

	public Canvas m_DebugCanvas;

	public Text m_DebugText;

	public Text m_DebugFPSText;

	public Text m_DebugVersionText;

	public float m_DebugNetNextUpdateTime;

	public Image m_DebugGarbageCollectImage;

	public Image m_DebugExceptionSinceBootImage;

	public Image m_DebugExceptionImage;

	public string m_DebugForceLoadLevel = string.Empty;

	public Camera[] m_CameraToHideDuringGame = new Camera[2];

	private int m_FramesToSkipForControllerCheck;

	private static bool m_bPostLevelLoad = false;

	private FinishedFiltering m_FinishedFiltering;

	private bool m_bProfanityFilteringSuccessfullyCompleted;

	private bool m_bProfanityFilteringFinished;

	public Color32 m_PCInputTextColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private GameObject m_BWFPrefab;

	private bool m_bProfanityFilterEnabled = true;

	public static PersistentScriptComponent<PrimitiveDrawer> PrimitiveDrawer = new PersistentScriptComponent<PrimitiveDrawer>();

	private static int fixedTimeStep = 0;

	private static int maxDeltaTime = 0;

	public static bool IsApplicationQuitting { get; set; }

	public bool LoadingBackToStart => m_bLoadBackToBootFlow;

	public GLOBALSTART_MODE CurrentGlobalStartMode => _m_GlobalStartMode;

	private GLOBALSTART_MODE m_GlobalStartMode
	{
		get
		{
			return _m_GlobalStartMode;
		}
		set
		{
			_m_GlobalStartMode = value;
			UnityEngine.Debug.Log("**DART** GLOBALSTART_MODE " + _m_GlobalStartMode);
			if (T17NetManager.IsMasterClient)
			{
				T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.GameState, (int)_m_GlobalStartMode);
			}
			if (value == GLOBALSTART_MODE.SHOW_FRONTEND || value == GLOBALSTART_MODE.IN_LEVEL)
			{
				ErrorDialogHandler.ConsiderShowingDialog();
			}
		}
	}

	public bool ProfanityFilteringComplete => m_bProfanityFilteringSuccessfullyCompleted;

	public bool ProfanityFilterEnabled => m_bProfanityFilterEnabled;

	public static event StateChangedHandler EnteredLevelEvent;

	public event StateChangedHandler InitManagersCompletedEvent;

	public static event StateChangedHandler EndLevelEvent;

	public void SkipFrames(int num)
	{
		m_FramesToSkipForControllerCheck += num;
	}

	protected override void Awake()
	{
		base.Awake();
		UnityEngine.Debug.Log("===== Graphics Device Info ======");
		UnityEngine.Debug.Log(SystemInfo.deviceName);
		UnityEngine.Debug.Log(SystemInfo.deviceModel);
		UnityEngine.Debug.Log(SystemInfo.deviceUniqueIdentifier);
		UnityEngine.Debug.Log(SystemInfo.graphicsDeviceName);
		UnityEngine.Debug.Log(SystemInfo.graphicsDeviceVendor);
		UnityEngine.Debug.Log(SystemInfo.graphicsDeviceID);
		UnityEngine.Debug.Log("Device Version: " + SystemInfo.graphicsDeviceVersion);
		UnityEngine.Debug.Log("Memory Size:" + SystemInfo.graphicsMemorySize);
		UnityEngine.Debug.Log("Multithreaded Enabled: " + SystemInfo.graphicsMultiThreaded);
		UnityEngine.Debug.Log("Shader Model Version: " + SystemInfo.graphicsShaderLevel);
		UnityEngine.Debug.Log("Max Texture Size:" + SystemInfo.maxTextureSize);
		UnityEngine.Debug.Log("Supports Image Effects:" + SystemInfo.supportsImageEffects);
		UnityEngine.Debug.Log("===== Graphics Device Info End ======");
		SomeMemoryHacks();
		Localization.Init();
		UnityEngine.Debug.Log(" +++++   MAINLINE   +++++++  ");
		NumberToStringCache.Init();
		RenderTargetManager.Init();
		if (m_TheInstance == null)
		{
			m_TheInstance = this;
		}
		if (m_BWFPrefab == null)
		{
			m_BWFPrefab = Resources.Load<GameObject>("BWF");
		}
		if (m_BWFPrefab != null && m_ManagersGORef != null)
		{
			UnityEngine.Object.Instantiate(m_BWFPrefab, m_ManagersGORef.transform);
		}
		Display main = Display.main;
		if (Screen.width > main.systemWidth || Screen.height > main.systemHeight)
		{
			QualityManager.SetResolution(main.systemWidth, main.systemHeight, Screen.fullScreen);
		}
	}

	public static GlobalStart GetInstance()
	{
		return m_TheInstance;
	}

	protected virtual void OnDestroy()
	{
		if (null != NetAnalytics.Instance)
		{
			NetAnalytics.Instance.StopSession();
		}
		T17NetLoadSync.c_OnTimedOut -= OnLoadTimedOut;
		T17NetLoadSync.c_OnDisconnected -= OnLoadDisconnected;
		if (m_TheInstance == this)
		{
			m_TheInstance = null;
			if (m_googleAnalyticsV3GO != null)
			{
				UnityEngine.Object.Destroy(m_googleAnalyticsV3GO);
				m_googleAnalyticsV3GO = null;
			}
			m_googleAnalyticsV3PreFab = null;
		}
	}

	public GLOBALSTART_MODE GetMode()
	{
		return m_GlobalStartMode;
	}

	public string GetModeAsString()
	{
		return m_GlobalStartMode.ToString();
	}

	private void Start()
	{
		m_RegisteredFlows = new Dictionary<BaseFlowBehaviour.FlowType, BaseFlowBehaviour>();
		m_bRewiredLoaded = false;
		m_bBootLoaded = false;
		m_bLoadedLoadingScene = false;
		m_bStartedLoadingSceneLoad = false;
		m_bStartedFrontEndSceneLoad = false;
		m_bFrontEndLoaded = false;
		m_bResultsLoaded = false;
		m_GlobalStartMode = GLOBALSTART_MODE.INIT;
		T17NetLoadSync.c_OnTimedOut += OnLoadTimedOut;
		T17NetLoadSync.c_OnDisconnected += OnLoadDisconnected;
		if (m_DebugCanvas != null)
		{
			m_DebugCanvas.gameObject.SetActive(value: false);
		}
		if (m_DebugGarbageCollectImage != null)
		{
			m_DebugGarbageCollectImage.gameObject.SetActive(value: false);
		}
		if (null != m_DebugExceptionSinceBootImage)
		{
			m_DebugExceptionSinceBootImage.gameObject.SetActive(value: false);
		}
		if (null != m_DebugExceptionImage)
		{
			m_DebugExceptionImage.gameObject.SetActive(value: false);
		}
		T17Text.m_sPCInputTextColorHex = ColorUtility.ToHtmlStringRGBA(m_PCInputTextColor);
	}

	private void HandleException(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Exception)
		{
			if (null != m_DebugExceptionSinceBootImage && m_bShowDebugText)
			{
				m_DebugExceptionSinceBootImage.gameObject.SetActive(value: true);
			}
			if (null != m_DebugExceptionImage && m_bShowDebugText)
			{
				m_DebugExceptionImage.gameObject.SetActive(value: true);
			}
		}
	}

	private void OnRoomEvent(T17NetConfig.NetEventTypes roomSignal, object payload, int senderId, bool isUs)
	{
		if (roomSignal == T17NetConfig.NetEventTypes.CustomLevelData && payload != null)
		{
			byte[] collection = (byte[])payload;
			m_CustomLevelData.Clear();
			m_CustomLevelData.AddRange(collection);
		}
	}

	private void Update()
	{
		ResetTimedNetworkService();
		if (Input.GetKey(KeyCode.D) && Input.GetKeyUp(KeyCode.I))
		{
			ToggleDebugText();
		}
		if (!m_bProcessedLoadBackToBootFlow && m_bLoadBackToBootFlow)
		{
			if (m_GlobalStartMode == GLOBALSTART_MODE.SHOW_BOOT)
			{
				if (IsLoadingFlowLoaded())
				{
					ShowLoadingScreen(delegate
					{
						StartCoroutine(DeleteBootScene());
						m_bProcessedLoadBackToBootFlow = true;
						m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_KILL_BOOT;
					});
				}
			}
			else if (m_GlobalStartMode == GLOBALSTART_MODE.IN_LEVEL)
			{
				m_bProcessedLoadBackToBootFlow = true;
				EndLevel(bShowResults: false);
			}
			else if (m_GlobalStartMode == GLOBALSTART_MODE.SHOW_FRONTEND)
			{
				if (IsLoadingFlowLoaded())
				{
					ShowLoadingScreen(delegate
					{
						m_bProcessedLoadBackToBootFlow = true;
						if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
						{
							((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).HideMenus();
						}
						m_GlobalStartMode = GLOBALSTART_MODE.KILL_FRONTEND;
					});
				}
			}
			else if (m_GlobalStartMode == GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS)
			{
				m_bProcessedLoadBackToBootFlow = true;
			}
		}
		switch (m_GlobalStartMode)
		{
		case GLOBALSTART_MODE.INIT:
			Platform.CreatePlatform(m_ManagersGORef);
			StartCoroutine(LoadRewiredScene());
			if (TextIconManager.Instance != null)
			{
				TextIconManager.CacheControllerMaps();
			}
			if (g_bGoogleAnalyticsEnabled)
			{
				if (m_googleAnalyticsV3PreFab == null)
				{
					m_googleAnalyticsV3PreFab = Resources.Load("Prefabs/Network/GoogleAnalyticsV3/GAv3") as GameObject;
					if (m_googleAnalyticsV3PreFab == null)
					{
						UnityEngine.Debug.LogErrorFormat("Bootstrap.Awake - failed to load prefab ({0}) for m_googleAnalyticsV3GO", "Prefabs/Network/GoogleAnalyticsV3/GAv3");
					}
				}
				if (m_googleAnalyticsV3GO == null && m_googleAnalyticsV3PreFab != null)
				{
					m_googleAnalyticsV3GO = UnityEngine.Object.Instantiate(m_googleAnalyticsV3PreFab);
					if (m_googleAnalyticsV3GO != null)
					{
						m_googleAnalyticsV3GO.name = "NetGoogleAnalyticsV3";
						UnityEngine.Object.DontDestroyOnLoad(m_googleAnalyticsV3GO);
					}
					else
					{
						UnityEngine.Debug.LogErrorFormat("Failed to instantiate prefab ({0}) for m_googleAnalyticsV3GO", "Prefabs/Network/GoogleAnalyticsV3/GAv3");
					}
				}
			}
			if (T17NetRoomGameView.Instance != null)
			{
				T17NetRoomGameView.OnRoomSignalEvent -= OnRoomEvent;
				T17NetRoomGameView.OnRoomSignalEvent += OnRoomEvent;
			}
			NetAnalytics.Instance.InitialiseTracker();
			NetAnalytics.Instance.StartSession();
			m_GlobalStartMode = GLOBALSTART_MODE.START_LOAD_REWIRED;
			break;
		case GLOBALSTART_MODE.START_LOAD_REWIRED:
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOAD_REWIRED;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_LOAD_REWIRED:
			if (m_bRewiredLoaded)
			{
				StartCoroutine(LoadBootScene());
				m_GlobalStartMode = GLOBALSTART_MODE.START_LOAD_BOOT;
			}
			break;
		case GLOBALSTART_MODE.START_LOAD_BOOT:
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOAD_BOOT;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_LOAD_BOOT:
			if (m_bBootLoaded && m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Boot))
			{
				AudioController.LoadBank("GamePlay");
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_BOOT;
			}
			break;
		case GLOBALSTART_MODE.SHOW_BOOT:
			if (!m_bLoadedLoadingScene && !m_bStartedLoadingSceneLoad)
			{
				m_bStartedLoadingSceneLoad = true;
				StartCoroutine(LoadLoadingScene());
			}
			if (m_bLoadBackToBootFlow && m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
			{
				((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).Reset();
				m_bLoadBackToBootFlow = false;
				m_bProcessedLoadBackToBootFlow = false;
			}
			if (!m_bFrontEndLoaded && !m_bStartedFrontEndSceneLoad)
			{
				m_bStartedFrontEndSceneLoad = true;
				StartCoroutine(LoadFrontendScene());
			}
			break;
		case GLOBALSTART_MODE.KILL_BOOT:
			if (!m_bDARTTest)
			{
				StartCoroutine(DeleteBootScene());
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_KILL_BOOT;
			}
			else
			{
				m_GlobalStartMode = GLOBALSTART_MODE.START_LEVEL_LOAD;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_KILL_BOOT:
			if (!m_bBootLoaded)
			{
				if (m_bLoadBackToBootFlow)
				{
					m_GlobalStartMode = GLOBALSTART_MODE.KILL_FRONTEND;
				}
				else
				{
					m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOAD_FRONTEND;
				}
			}
			break;
		case GLOBALSTART_MODE.START_LOAD_FRONTEND:
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOAD_FRONTEND;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_LOAD_FRONTEND:
			if (m_bFrontEndLoaded)
			{
				Platform.GetInstance().ResetLightBar();
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
				{
					((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).StartFrontEnd();
					m_GlobalStartMode = GLOBALSTART_MODE.CHECK_INVITES;
				}
			}
			break;
		case GLOBALSTART_MODE.CHECK_INVITES:
		{
			FrontEndFlow instance12 = FrontEndFlow.Instance;
			if (instance12 != null && instance12.m_MainMenu != null)
			{
				BaseMenuBehaviour currentOpenMenu = instance12.m_MainMenu.GetCurrentOpenMenu();
				if (currentOpenMenu != null && currentOpenMenu as SettingsFrontendMenu != null)
				{
					break;
				}
			}
			if (T17DialogBoxManager.HasAnyOpenDialogs())
			{
				break;
			}
			bool flag3 = false;
			if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
			{
				flag3 = ((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).IsTransitionInProgress;
			}
			if (!flag3)
			{
				if (T17NetInvites.HasInvite() || T17NetInvites.IsPlayTogetherHost())
				{
					InviteOnlineAreaCheckCallBack(bResult: true, Platform.OnlineAccessErrorCode.OnlineAccessOK, failureHandledPlatformside: false);
					break;
				}
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
				T17NetInvites.Region = CloudRegionCode.none;
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
			}
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_INVITES_ONLINE_CHECK:
			break;
		case GLOBALSTART_MODE.PROCESSING_INVITE:
		{
			bool flag5 = false;
			switch (T17NetInvites.m_Result)
			{
			case T17NetInvites.InviteResult.JoinSucceeded:
			{
				T17NetRoomGameView.GameRoomType outValue = T17NetRoomGameView.GameRoomType.Undefined;
				PrisonConfig.ConfigType outValue2 = PrisonConfig.ConfigType.Cooperative;
				GLOBALSTART_MODE outValue3 = GLOBALSTART_MODE.INIT;
				if (T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.RoomType, ref outValue) && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.ConfigType, ref outValue2) && T17NetRoomGameView.GetCustomPropertyAsEnum(T17NetRoomGameView.CustomProperty.GameState, ref outValue3))
				{
					if (outValue == T17NetRoomGameView.GameRoomType.Private && outValue2 == PrisonConfig.ConfigType.Versus && outValue3 == GLOBALSTART_MODE.SHOW_FRONTEND)
					{
						if (FrontEndFlow.Instance.m_CurrentMenuType == FrontEndFlow.MenuType.LevelEditor)
						{
							FrontEndFlow.Instance.HideCurrentMainMenu();
							FrontEndFlow.Instance.ShowMenus();
							FrontEndFlow.Instance.ExternalForceFlowToRunning();
						}
						FrontEndFlow.Instance.SwitchToFrontEndMenuType(FrontendRootMenu.FrontendMenuTypeToOpen.Versus);
						FrontEndFlow.Instance.OpenChildOnTopOfMenu(3);
						NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
						T17NetInvites.Region = CloudRegionCode.none;
						m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
					}
					else if (outValue == T17NetRoomGameView.GameRoomType.Public && outValue2 == PrisonConfig.ConfigType.Versus)
					{
						T17NetManager.LogGoogleException("We've managed to accept an invite into a public versus game!  Leaving...");
						NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState);
						T17NetInvites.Clear();
						NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
						T17NetInvites.Region = CloudRegionCode.none;
						m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
					}
					else
					{
						T17NetInvites.Region = CloudRegionCode.none;
						SetSelectedLevelToNetRoomCurrent();
						StartGameWithModeAndCurrentConfig(GLOBALSTART_GAME_MODES.ONLINE);
					}
				}
				else
				{
					flag5 = true;
				}
				T17NetInvites.Clear();
				break;
			}
			case T17NetInvites.InviteResult.JoinFailed:
				flag5 = true;
				break;
			case T17NetInvites.InviteResult.JoinFailedNoPhotonConnectionError:
			case T17NetInvites.InviteResult.JoinCancelled:
				T17NetInvites.Clear();
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
				T17NetInvites.Region = CloudRegionCode.none;
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
				break;
			}
			if (flag5)
			{
				T17NetInvites.Clear();
				ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.InviteFailedToConnect);
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
				T17NetInvites.Region = CloudRegionCode.none;
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
			}
			break;
		}
		case GLOBALSTART_MODE.SHOW_FRONTEND:
			break;
		case GLOBALSTART_MODE.START_LEVEL_LOAD:
			if (!IsLoadingFlowLoaded() || ((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).IsTransitionInProgress)
			{
				break;
			}
			ShowLoadingScreen(delegate
			{
				PrisonSnapshotIO.ResetIOData();
				if (T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
				{
					Platform.GetInstance().EnterOnlineArea(bIsLeaderboard: false, Platform.GetInstance().ForceGameOffline);
				}
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
				{
					((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).HideMenus();
				}
				m_bLoadError = false;
				int numPlayers = 1;
				if (PhotonNetwork.room != null)
				{
					numPlayers = PhotonNetwork.room.PlayerCount;
				}
				ResetLoadStates();
				NetLoadSync.RecordPreLoadRoomVars(numPlayers);
				NetLoadSync.StartLevelLoad();
				if (m_PreviewEditorLevel)
				{
					HideLoadingScreen(null, setStateToDummy: false);
					m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND;
				}
				else
				{
					m_GlobalStartMode = GLOBALSTART_MODE.KILL_FRONTEND;
				}
				m_GlobalStartMode = GLOBALSTART_MODE.KILL_FRONTEND;
			});
			break;
		case GLOBALSTART_MODE.KILL_FRONTEND:
			StartCoroutine(DeleteFrontEndScene());
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND:
			if (!m_bFrontEndLoaded)
			{
				if (m_bLoadBackToBootFlow)
				{
					CleanupBeforeGoingToBoot();
					break;
				}
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_LEVEL;
				Platform.GetInstance().StartingToLoadLevel();
			}
			break;
		case GLOBALSTART_MODE.LOADING_LEVEL:
		{
			Character.TOTAL_INMATE_COUNT = 0;
			GlobalLevelCleanUp();
			AudioController.SetState(State_Group.Sfx_Mix, Sfx_Mix.Volume_Down.ToString());
			CullingBuckets.Init();
			T17BehaviourManager.GetInstance().PreScan();
			Platform.GetInstance().SetNativeVoiceChatEnabled(state: false);
			float value = 1f;
			GlobalSave.GetInstance().Get("Settings:ProfanityFilter", out value, 1f);
			m_bProfanityFilterEnabled = value > 0.5f;
			ObjectiveTree.HIGHEST_ACTIVE_TREE_ID = 0;
			LoadSaveGame();
			if (m_CustomLevel && T17NetManager.IsMasterClient)
			{
				if (m_CustomLevelData.Count > 0)
				{
					BroadcastCustomLevelData();
				}
				else
				{
					m_bLevelFailedToLoad = true;
				}
			}
			StartCoroutine(LoadLevelScene());
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL;
			SaveManager instance18 = SaveManager.GetInstance();
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Save Mode", "Level loaded - Save Mode " + instance18.CurrentSaveMode, string.Empty, 0L);
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL:
			if (m_bLevelFailedToLoad)
			{
			}
			if (m_bLevelLoaded || m_bLevelFailedToLoad)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_OTHER_INGAME_SCENES_HUD;
			}
			break;
		case GLOBALSTART_MODE.LOADING_OTHER_INGAME_SCENES_HUD:
			StartCoroutine(LoadHUDMenuScene());
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_HUD;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_HUD:
			if (m_bHUDMenuLoaded)
			{
				StartCoroutine(LoadInGameMenusScene());
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_IGM;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_IGM:
		{
			if (m_CustomLevel && m_bHUDMenuLoaded && m_bInGameMenusLoaded)
			{
				LevelScript instance6 = LevelScript.GetInstance();
				if (instance6 != null)
				{
					LevelDetailsManager instance7 = LevelDetailsManager.GetInstance();
					LevelDataManager instance8 = LevelDataManager.GetInstance();
					if (instance7 != null && instance8 != null)
					{
						PrisonData prisonDataForPrison = instance8.GetPrisonDataForPrison(instance7.GetOutfitType());
						PrisonData prisonDataForPrison2 = instance8.GetPrisonDataForPrison(LevelScript.PRISON_ENUM.Centre_Perks);
						PrisonData prisonData = UnityEngine.Object.Instantiate(prisonDataForPrison2);
						prisonData.m_bAddRobinsonCharacter = false;
						prisonData.m_Configs.Clear();
						if (m_PreviewEditorLevel)
						{
							for (int num2 = 0; num2 < LevelDataManager.GetInstance().m_CustomPrisonConfigs.Length; num2++)
							{
								PrisonConfig prisonConfig = UnityEngine.Object.Instantiate(LevelDataManager.GetInstance().m_CustomPrisonConfigs[num2]);
								prisonConfig.m_ConfigType = PrisonConfig.ConfigType.Singleplayer;
								prisonData.m_Configs.Add(prisonConfig);
							}
						}
						else
						{
							prisonData.m_Configs.AddRange(instance8.m_CustomPrisonConfigs);
						}
						for (int num3 = 0; num3 < prisonData.m_Configs.Count; num3++)
						{
							prisonData.m_Configs[num3].m_ItemDataOverrides.Clear();
							prisonData.m_Configs[num3].m_ItemDataOverrides.AddRange(prisonDataForPrison.m_Configs[0].m_ItemDataOverrides);
						}
						prisonData.m_NameLocalizationKey = instance7.GetLevelName();
						prisonData.m_DescriptionLocalizationKey = instance7.GetLevelDecription();
						prisonData.m_LevelInfo.m_PrisonType = LevelScript.PRISON_TYPE.Normal;
						prisonData.m_LevelInfo.m_PrisonEnum = LevelScript.PRISON_ENUM.CustomPrison;
						prisonData.m_CustomisableRoles = new int[2];
						prisonData.m_CustomisableRoles[0] = instance7.GetNumberOfInmates();
						prisonData.m_CustomisableRoles[1] = instance7.GetNumberOfGuards();
						for (int num4 = 0; num4 < prisonData.m_Configs.Count; num4++)
						{
							prisonData.m_Configs[num4].m_VendorConfig.m_MaxVendors = prisonData.m_CustomisableRoles[0] / 2;
						}
						for (int num5 = 0; num5 < prisonData.m_InfluencerWeights.Length; num5++)
						{
							prisonData.m_InfluencerWeights[num5].min = 0;
							prisonData.m_InfluencerWeights[num5].max = 0;
						}
						PrisonConfig prisonConfig2 = prisonData.m_Configs[GetCurrentSelectedConfigID()];
						if (prisonConfig2 == null)
						{
						}
						LevelDetailsManager.DiffecultyLevel levelDifficulty = instance7.GetLevelDifficulty();
						ItemContainerConfig itemContainerConfig = new ItemContainerConfig();
						itemContainerConfig.m_KeepOldStartingItems = false;
						itemContainerConfig.m_KeepOldTrackedItems = false;
						itemContainerConfig.m_ReplaceRandomGroups = true;
						itemContainerConfig.m_AllowRefresh = true;
						itemContainerConfig.m_StartingItems.Clear();
						itemContainerConfig.m_RandomGroups.Clear();
						DifficultySettings difficultySettings = m_EditorSettings.m_DifficultySettings[(int)levelDifficulty];
						itemContainerConfig.m_RandomPercentages = new int[difficultySettings.InmateDeskConfig.m_RandomPercentages.Length + 5];
						itemContainerConfig.m_StartingItems.AddRange(difficultySettings.InmateDeskConfig.m_StartingItems);
						itemContainerConfig.m_RandomGroups.AddRange(difficultySettings.InmateDeskConfig.m_RandomGroups);
						int num6 = 0;
						int num7 = 0;
						while (num7 < difficultySettings.InmateDeskConfig.m_RandomPercentages.Length)
						{
							itemContainerConfig.m_RandomPercentages[num6] = difficultySettings.InmateDeskConfig.m_RandomPercentages[num7];
							num7++;
							num6++;
						}
						for (int num8 = 0; num8 < 5; num8++)
						{
							int randomItemGroup = instance7.GetRandomItemGroup(num8);
							ItemGroupSetting itemGroupSetting = m_EditorSettings.m_ItemGroupSettingsList[randomItemGroup];
							itemContainerConfig.m_StartingItems.AddRange(itemGroupSetting.ItemConfig.m_StartingItems);
							itemContainerConfig.m_RandomGroups.AddRange(itemGroupSetting.ItemConfig.m_RandomGroups);
							int num9 = 0;
							while (num9 < itemGroupSetting.ItemConfig.m_RandomGroups.Count)
							{
								itemContainerConfig.m_RandomPercentages[num6] = itemGroupSetting.ItemConfig.m_RandomPercentages[num9];
								num9++;
								num6++;
							}
						}
						prisonConfig2.m_InmateDeskConfig = itemContainerConfig;
						prisonConfig2.m_PlayerDeskConfig = itemContainerConfig;
						prisonConfig2.m_GuardDeskConfig = m_EditorSettings.m_DifficultySettings[(int)levelDifficulty].GuardDeskConfig;
						instance6.m_LevelSetup = prisonData;
						if (m_PreviewEditorLevel && (bool)CustomisationManager.GetInstance())
						{
							CustomisationManager.GetInstance().RandomiseCustomisations(prisonData);
						}
					}
				}
			}
			TimedNetworkService();
			ConfigManager instance9 = ConfigManager.GetInstance();
			if (!(instance9 != null) || !m_bLevelLoaded || !m_bHUDMenuLoaded || !m_bInGameMenusLoaded)
			{
				break;
			}
			PlaylistData.NetPrisonSetup netPrisonSetup = null;
			if (!m_bDARTTest)
			{
				netPrisonSetup = NetBluePrintDetails.Instance.CurrentPrisonConfig;
				instance9.SetActiveConfig(netPrisonSetup.m_ConfigIndex);
			}
			else
			{
				UnityEngine.Debug.Log(" ****** DARTTEST CONFIG 0  ****** ");
				instance9.SetActiveConfig(0);
			}
			LevelScript instance10 = LevelScript.GetInstance();
			if (instance10.m_LevelSetup == null)
			{
				PrisonData prisonDataForPrison3 = LevelDataManager.GetInstance().GetPrisonDataForPrison(LevelScript.PRISON_ENUM.Centre_Perks);
				if (prisonDataForPrison3 == null || prisonDataForPrison3.m_LevelInfo == null)
				{
				}
				instance10.m_LevelSetup = prisonDataForPrison3;
			}
			AudioController.LoadBank("Ambience_Prison_01");
			TimedNetworkService();
			AudioController.LoadBank("Music_Prison_01");
			TimedNetworkService();
			AudioController.LoadBank("Player_SFX");
			TimedNetworkService();
			Localization.SetKeyOverride(instance10.m_LocalizationKeyOverride);
			instance10.PreInit();
			if (!instance10.m_PreBuildBehaviourLists)
			{
				GameObject gameObject = GameObject.Find("LevelParent");
				m_LevelSceneRoots.Clear();
				UnityEngine.Debug.LogError(" **** Time Add Classes Start " + Time.unscaledTime);
				T17BehaviourManager.GetInstance().AddClassesFromRoot(gameObject.transform);
				m_LevelSceneRoots.Add(gameObject.transform);
				for (int num10 = 0; num10 < instance10.m_SubLevels.Length; num10++)
				{
					if (!(instance10.m_SubLevels[num10].m_Root != null))
					{
						continue;
					}
					instance10.m_SubLevels[num10].m_Root.gameObject.SetActive(value: true);
					m_LevelSceneRoots.Add(instance10.m_SubLevels[num10].m_Root.transform);
					int childCount = instance10.m_SubLevels[num10].m_Root.transform.childCount;
					for (int num11 = 0; num11 < childCount; num11++)
					{
						if (instance10.m_SubLevels[num10].m_Root.GetChild(num11) != null)
						{
							instance10.m_SubLevels[num10].m_Root.GetChild(num11).gameObject.SetActive(value: true);
						}
					}
					Transform transform = instance10.m_SubLevels[num10].m_Root.Find("Building");
					if (transform != null)
					{
						childCount = transform.transform.childCount;
						for (int num12 = 0; num12 < childCount; num12++)
						{
							if (transform.GetChild(num12) != null)
							{
								transform.GetChild(num12).gameObject.SetActive(value: true);
							}
						}
					}
					Transform transform2 = instance10.m_SubLevels[num10].m_Root.Find("Text");
					if (transform2 != null)
					{
						transform2.gameObject.SetActive(value: false);
					}
					T17BehaviourManager.GetInstance().AddClassesFromRoot(instance10.m_SubLevels[num10].m_Root);
				}
			}
			else
			{
				UnityEngine.Debug.LogError(" ****   MADNESS OF Pre Made behaviours list  " + Time.unscaledTime);
				m_LevelSceneRoots.Clear();
				GameObject gameObject2 = GameObject.Find("LevelParent");
				m_LevelSceneRoots.Add(gameObject2.transform);
				for (int num13 = 0; num13 < instance10.m_SubLevels.Length; num13++)
				{
					if (instance10.m_SubLevels[num13].m_Root != null)
					{
						m_LevelSceneRoots.Add(instance10.m_SubLevels[num13].m_Root.transform);
					}
				}
				T17BehaviourManager.GetInstance().InjectClasses(instance10.m_NetworkBehaviourClasses, instance10.m_MonoBehaviourClasses);
			}
			TimedNetworkService();
			m_GlobalStartMode = GLOBALSTART_MODE.SETUP_AREA_MANAGERS;
			break;
		}
		case GLOBALSTART_MODE.SETUP_AREA_MANAGERS:
			TimedNetworkService();
			if (LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.DLC02)
			{
				GhostNode[] array = null;
				for (int m = 0; m < LevelScript.GetInstance().m_SubLevels.Length; m++)
				{
					if (LevelScript.GetInstance().m_SubLevels[m].m_Root != null)
					{
						array = LevelScript.GetInstance().m_SubLevels[m].m_Root.GetComponentsInChildren<GhostNode>();
						break;
					}
				}
				for (int n = 0; n < array.Length; n++)
				{
					array[n].EnableColliders(bOn: false);
				}
				if (AstarPath.active != null)
				{
					AstarPath.active.Scan(bUpdateNetworkService: true);
				}
				for (int n = 0; n < array.Length; n++)
				{
					array[n].EnableColliders(bOn: true);
				}
			}
			else if (AstarPath.active != null)
			{
				AstarPath.active.Scan(bUpdateNetworkService: true);
				TimedNetworkService();
			}
			SetupAreaManagers();
			m_SlicedSetupLoop = 0;
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART2;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART2:
		{
			int num19 = ((ConfigManager.GetInstance() != null && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Singleplayer) ? 1 : 4);
			if (m_SlicedSetupLoop < num19)
			{
				int reservedNetID = T17NetConfig.GetReservedNetID((T17NetConfig.ReservedNetID)(26 + m_SlicedSetupLoop));
				T17NetworkManager.GetInstance().SpawnPlayerObject(reservedNetID, m_SlicedSetupLoop);
				m_SlicedSetupLoop++;
			}
			if (m_SlicedSetupLoop != num19)
			{
				break;
			}
			List<Player> allPlayers2 = Player.GetAllPlayers();
			for (int num20 = 0; num20 < allPlayers2.Count; num20++)
			{
				if (allPlayers2[num20] != null)
				{
					T17BehaviourManager.GetInstance().AddClassesFromRoot(allPlayers2[num20].transform);
				}
			}
			m_GlobalStartMode = GLOBALSTART_MODE.SETUP_ITEM_MANAGER;
			break;
		}
		case GLOBALSTART_MODE.SETUP_ITEM_MANAGER:
		{
			if (!(ItemManager.GetInstance() != null) || !ItemManager.GetInstance().SpecialInit())
			{
				break;
			}
			Transform transform3 = ItemManager.GetInstance().transform;
			for (int num17 = 0; num17 < transform3.childCount; num17++)
			{
				Transform child = transform3.GetChild(num17);
				if (child != null)
				{
					T17BehaviourManager.GetInstance().AddClassesFromRoot(child);
				}
			}
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART3;
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART3:
			T17BehaviourManager.GetInstance().PostScan();
			UnityEngine.Debug.LogError(" **** Time Add Classes Done " + Time.unscaledTime);
			if (m_CameraToHideDuringGame != null)
			{
				for (int num15 = 0; num15 < m_CameraToHideDuringGame.Length; num15++)
				{
					if (m_CameraToHideDuringGame[num15] != null)
					{
						m_CameraToHideDuringGame[num15].gameObject.SetActive(value: false);
					}
				}
			}
			NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_LevelLoad_Done);
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS;
			break;
		case GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || (NetLoadSync.AllClientsLevelLoaded() && NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.LevelLoad_Done))
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_LevelInit_InProgress);
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_CUSTOMISATION;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_CUSTOMISATION:
			if (!T17NetManager.NetOfflineMode && !NetLoadSync.HasTimedOut() && !NetLoadSync.HasDisconnected() && !T17NetManager.AnyPlayersEscaped() && !(null != PrisonCustomisationManager.GetInstance()))
			{
				break;
			}
			if (PrisonCustomisationManager.GetInstance() != null)
			{
				if (T17NetManager.IsMasterClient)
				{
					if (PrisonSnapshotIO.IsThereSaveData())
					{
						string errorStr = string.Empty;
						T17NetworkManager.Deserialize(PrisonCustomisationManager.GetInstance(), ref errorStr);
					}
					PrisonCustomisationManager.m_bNpcCustomisationsInit = true;
				}
				else
				{
					PrisonCustomisationManager.m_bNpcCustomisationsInit = false;
					PrisonCustomisationManager.GetInstance().RequestState_CustomisationRPC();
				}
			}
			m_GlobalStartMode = GLOBALSTART_MODE.LOAD_CUSTOMISATION;
			break;
		case GLOBALSTART_MODE.LOAD_CUSTOMISATION:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || PrisonCustomisationManager.m_bNpcCustomisationsInit)
			{
				CharacterCustomisation.SetRandomSeed(PrisonCustomisationManager.m_CustomisationSeed);
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS:
			if ((T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.LevelInit_InProgress) && !T17NetConfig.NetForceLoadHang_LevelInit)
			{
				if (T17BehaviourManager.GetInstance().RunClassInit())
				{
					T17BehaviourManager.GetInstance().PurgeClasses();
					T17NetManager.Service();
					SetUpManagers_StartInit();
					T17NetManager.Service();
					NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_LevelInit_Done);
					m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS;
				}
				else
				{
					T17NetManager.Service();
				}
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || (NetLoadSync.AllClientsLevelInit() && NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.LevelInit_Done))
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_Inventory_InProgress);
				m_GlobalStartMode = GLOBALSTART_MODE.REQUEST_PLAYER_STARTING_ITEMS;
			}
			break;
		case GLOBALSTART_MODE.REQUEST_PLAYER_STARTING_ITEMS:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.Inventory_InProgress)
			{
				NetworkingPeer.m_bDiscardSerializeOnViews = false;
				CharacterSerializer instance15 = CharacterSerializer.GetInstance();
				if (instance15 != null)
				{
					instance15.EnableSerialization();
				}
				T17NetRoomGameView.OnRoomSignalEvent += CharacterNetEvents.OnEvent;
				List<Player> allPlayers = Player.GetAllPlayers();
				for (int num16 = 0; num16 < allPlayers.Count; num16++)
				{
					m_LevelSceneRoots.Add(allPlayers[num16].transform);
				}
				if (!T17NetManager.IsMasterClient)
				{
					T17NetworkManager.GetInstance().RequestPhotonSerialiseViewRPC();
				}
				m_fSerializeTime = UpdateManager.time + 1f;
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS:
		{
			if (T17NetConfig.NetForceLoadHang_Inventory)
			{
				break;
			}
			bool flag = true;
			List<Character> allCharacters = Character.GetAllCharacters();
			for (int i = 0; i < allCharacters.Count; i++)
			{
				Character character = allCharacters[i];
				if (null == character)
				{
					m_bLoadError = true;
					m_LoadErrorDescription = "Null character in GetAllCharacters array.";
					NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, T17NetManager.SilentErrorDialogMode);
					m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS;
					flag = false;
					break;
				}
				if (!character.m_NetView.isMine && character.m_CharacterRole != CharacterRole.Crowd && !character.m_bSerialiseInit && !character.m_bSpawnPointInit)
				{
					flag = false;
					if (UpdateManager.time >= m_fSerializeTime)
					{
						character.ForcePhotonSerialiseViewRPC();
						m_fSerializeTime = UpdateManager.time + 0.1f;
					}
				}
			}
			ItemManager instance2 = ItemManager.GetInstance();
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || (flag && null != instance2 && instance2.RequestsInProgress == 0))
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_Inventory_Done);
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS;
			}
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || (NetLoadSync.AllClientsInventoryInit() && NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.Inventory_Done))
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_Spawn_InProgress);
				m_GlobalStartMode = GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS;
			}
			break;
		case GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS:
		{
			if ((!T17NetManager.NetOfflineMode && !NetLoadSync.HasTimedOut() && !NetLoadSync.HasDisconnected() && !T17NetManager.AnyPlayersEscaped() && NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) != PlayerLoadState.Spawn_InProgress) || T17NetConfig.NetForceLoadHang_Spawn)
			{
				break;
			}
			bool bWait = false;
			if (!m_bLoadError)
			{
				m_LoadErrorDescription = "Deserialize error spawning level objects";
				string errorInformation = string.Empty;
				T17NetworkManager.GetInstance().DeserializeWorld(out m_bLoadError, out bWait, ref errorInformation);
				if (!m_bLoadError)
				{
					m_LoadErrorDescription = string.Empty;
				}
				else if (!string.IsNullOrEmpty(errorInformation))
				{
					m_LoadErrorDescription = "Deserialize error: '" + errorInformation + "'\n";
				}
			}
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || !bWait)
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_Spawn_Done);
				StartNameFiltering(FilteringDone);
				m_GlobalStartMode = GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS;
			}
			break;
		}
		case GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS:
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || (NetLoadSync.AllClientsPlayerSpawned() && NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.Spawn_Done))
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_Managers_InProgress);
				for (int k = 0; k < NetLoadManagerSync.m_AllNetworkLoadables.Count; k++)
				{
					INetworkLoadable networkLoadable = NetLoadManagerSync.m_AllNetworkLoadables[k];
					networkLoadable.ResetLoadState();
				}
				NetLoadManagerSync.Instance.RequestLoadDataRPC();
				m_GlobalStartMode = GLOBALSTART_MODE.NETWORK_INIT_MANAGERS;
			}
			break;
		case GLOBALSTART_MODE.NETWORK_INIT_MANAGERS:
		{
			if (!T17NetManager.NetOfflineMode && !NetLoadSync.HasTimedOut() && !NetLoadSync.HasDisconnected() && !T17NetManager.AnyPlayersEscaped() && NetLoadManagerSync.Instance.m_eLoadState != NetLoadManagerSync.LOADSTATE.RequestConfirmed)
			{
				break;
			}
			bool flag6 = true;
			if (!T17NetManager.IsMasterClient)
			{
				for (int num18 = 0; num18 < NetLoadManagerSync.m_AllNetworkLoadables.Count; num18++)
				{
					INetworkLoadable networkLoadable2 = NetLoadManagerSync.m_AllNetworkLoadables[num18];
					if (networkLoadable2.GetLoadState() == LOADSTATE.Finished_Error)
					{
						m_bLoadError = true;
						m_LoadErrorDescription += networkLoadable2.GetLoadError();
						m_GlobalStartMode = GLOBALSTART_MODE.INIT_MANAGERS;
					}
					else if (networkLoadable2.GetLoadState() != LOADSTATE.Finished_OK)
					{
						flag6 = false;
					}
				}
			}
			if (T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || flag6)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.INIT_MANAGERS;
			}
			break;
		}
		case GLOBALSTART_MODE.INIT_MANAGERS:
			if ((T17NetManager.NetOfflineMode || NetLoadSync.HasTimedOut() || NetLoadSync.HasDisconnected() || T17NetManager.AnyPlayersEscaped() || NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) == PlayerLoadState.Managers_InProgress) && !T17NetConfig.NetForceLoadHang_Managers)
			{
				InitialiseManagers();
				LevelScript instance3 = LevelScript.GetInstance();
				if (instance3 != null)
				{
					instance3.FinalInit();
				}
				CraftManager.GetInstance().CreateLiveList();
				SetUpManagers();
				if (!T17NetManager.IsMasterClient)
				{
					ItemContainerManager.GetInstance().PostSpawnPlayers_ApplyConfigs();
				}
				AIEventManager instance4 = AIEventManager.GetInstance();
				if (instance4 != null)
				{
					instance4.RunAIEventDataSetup();
				}
				string error2 = string.Empty;
				if (!NPCManager.GetInstance().Real_Deserialize(ref error2))
				{
				}
				NPCManager.GetInstance().PostRealDeserialize(PrisonSnapshotIO.IsThereSaveData());
				PrisonSnapshotIO.NotifySnapshotOfStart();
				if (PrisonAlterationSaveFixer.GetInstance() != null)
				{
					PrisonAlterationSaveFixer.GetInstance().RunAllChecks();
				}
				ObjectiveManager instance5 = ObjectiveManager.GetInstance();
				if (instance5 != null)
				{
					instance5.StartEvaluating();
				}
				ResetTimedNetworkService();
				T17NetManager.Service();
				if (Bootstrap.Instance != null)
				{
					Bootstrap.Instance.InToLevel();
				}
				UnityEngine.Debug.Log("**DART** IN_LEVEL");
				List<Character> allCharacters2 = Character.GetAllCharacters();
				for (int j = 0; j < allCharacters2.Count; j++)
				{
					allCharacters2[j].ProcessPins();
				}
				RoomManager.GetInstance().SetUpPins();
				FloorManager.GetInstance().DeleteFloorCollision();
				TimedNetworkService();
				FloorManager.GetInstance().EnableCollidersForUndergroundCaverns();
				TimedNetworkService();
				if (TutorialManager.GetInstance() != null)
				{
					TutorialManager.GetInstance().OnLevelStart();
				}
				ConstructEndgameInteraction.SpawnAllNecessaryItems_Transport();
				NavMeshUtil.Init();
				TimedNetworkService();
				if (this.InitManagersCompletedEvent != null)
				{
					this.InitManagersCompletedEvent();
				}
				TimedNetworkService();
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_PROFANITY_FILTER;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_PROFANITY_FILTER:
			if (m_bProfanityFilteringFinished)
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_ReadyToPlay);
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_PLAYERS;
			}
			TimedNetworkService();
			break;
		case GLOBALSTART_MODE.WAIT_FOR_PLAYERS:
		{
			if (!T17NetManager.NetOfflineMode && !NetLoadSync.HasTimedOut() && !NetLoadSync.HasDisconnected() && !T17NetManager.AnyPlayersEscaped() && (!NetLoadSync.AllClientsReadyToPlay() || NetLoadSync.GetLoadStateForPhotonId(T17NetManager.PhotonPlayerID) != PlayerLoadState.Success))
			{
				break;
			}
			bool silentErrorDialogMode = T17NetManager.SilentErrorDialogMode;
			if (m_GameMode == GLOBALSTART_GAME_MODES.ONLINE && !T17NetInvites.HasInvite())
			{
				if (NetLoadSync.HasTimedOut())
				{
					T17NetManager.SilentErrorDialogMode = false;
					ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected, m_wasMasterClientAtStartOfLevelLoad);
				}
				else if (NetLoadSync.HasDisconnected())
				{
					bool wasMasterClientAtStartOfLoad = m_wasMasterClientAtStartOfLevelLoad;
					Platform.GetInstance().GetOnlineAccessCode(delegate(Platform.OnlineAccessErrorCode error, bool bShowSystemDialogue)
					{
						T17NetManager.SilentErrorDialogMode = false;
						if (error == Platform.OnlineAccessErrorCode.OnlineAccessOK)
						{
							ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.DisconnectedDuringLoad, wasMasterClientAtStartOfLoad, true);
						}
						else
						{
							ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected, wasMasterClientAtStartOfLoad, true);
						}
					}, bShowSystemDialogues: false);
				}
				if (NetLoadSync.HaveAllPlayersLeftSinceLoad())
				{
					T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
					if (dialog != null)
					{
						dialog.Initialize(hasConfirm: true, hasDecline: true, hasCancel: false, "Text.Dialog.Gameplay.PlayersLeftDuringLoadTitle", "Text.Dialog.Gameplay.PlayersLeftDuringLoadBody", "Text.Dialog.Prompt.Yes", "Text.Dialog.Prompt.No", string.Empty);
						dialog.OnDecline = (T17DialogBox.DialogEvent)Delegate.Combine(dialog.OnDecline, (T17DialogBox.DialogEvent)delegate
						{
							DisconnectAndEndLevel();
						});
					}
					else
					{
						DisconnectAndEndLevel();
					}
				}
			}
			if (m_bLoadError && !T17NetInvites.HasInvite())
			{
				T17NetManager.SilentErrorDialogMode = false;
				ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.PhotonDisconnected, wasOriginalMasterClient: false);
			}
			T17NetManager.SilentErrorDialogMode = silentErrorDialogMode;
			m_bPostLevelLoad = true;
			NetLoadSync.StopLevelLoad();
			if (!T17NetManager.IsMasterClient)
			{
				RoutineManager instance11 = RoutineManager.GetInstance();
				if (instance11 != null)
				{
					instance11.InitalSyncClient();
				}
			}
			AudioController.SetState(State_Group.Sfx_Mix, Sfx_Mix.Volume_Up.ToString());
			m_GlobalStartMode = GLOBALSTART_MODE.CHECK_INVITES_DURING_LOAD;
			break;
		}
		case GLOBALSTART_MODE.CHECK_INVITES_DURING_LOAD:
		{
			bool flag4 = ErrorDialogHandler.HasErrorsToShow() && !PhotonNetwork.isMasterClient && !PhotonNetwork.offlineMode;
			m_GlobalStartMode = GLOBALSTART_MODE.IN_LEVEL;
			if (m_CustomLevel && !m_PreviewEditorLevel)
			{
				m_CountTimeInEditorRoutine = StartCoroutine(CountTimeInCustomPrisonRoutine());
			}
			if (flag4)
			{
				m_bWaitForSettingsToCloseForInvite = true;
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
				{
					((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).DecrementRefCountForImminentLoadingCall();
				}
				break;
			}
			Platform.GetInstance().SetNativeVoiceChatEnabled(state: true);
			if (T17NetInvites.HasInvite() || T17NetInvites.IsPlayTogetherHost())
			{
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
				{
					((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).DecrementRefCountForImminentLoadingCall();
				}
				DisconnectAndEndLevel();
				break;
			}
			if (T17NetManager.AnyPlayersEscaped())
			{
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
				{
					((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).DecrementRefCountForImminentLoadingCall();
				}
				EndLevel(bShowResults: false);
				ConfigManager instance14 = ConfigManager.GetInstance();
				if (null != instance14 && instance14.gameType == PrisonConfig.ConfigType.Versus)
				{
					m_ReturnToFrontendRoute = ReturnToFrontendRoutes.VersusLobby;
				}
				else
				{
					ErrorDialogHandler.ShowError(ErrorDialogHandler.Type.DisconnectedDuringLoad);
				}
				break;
			}
			if (PrisonSnapshotIO.IsThereSaveData())
			{
				m_DelayedHideLoadingScreenRoutine = StartCoroutine(HideLoadScreenAfterDelay(0.75f));
			}
			else if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
			{
				((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).HideLoadingScreen(delegate
				{
					RenderTargetManager.CheckForLostRTs();
				});
			}
			if (GlobalStart.EnteredLevelEvent != null)
			{
				StateChangedHandler enteredLevelEvent = GlobalStart.EnteredLevelEvent;
				enteredLevelEvent();
			}
			break;
		}
		case GLOBALSTART_MODE.IN_LEVEL:
		{
			CheckForDropInPlayer();
			bool flag2 = false;
			Gamer[] allGamers2 = Gamer.GetAllGamers();
			if (allGamers2 != null)
			{
				for (int l = 0; l < allGamers2.Length; l++)
				{
					if (allGamers2[l] != null && !allGamers2[l].IsLocal())
					{
						flag2 = true;
						break;
					}
				}
			}
			if (T17NetManager.NetOnlineMode && CutsceneManagerBase.GetState() == CutsceneManagerBase.States.Idle && (T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Public || flag2))
			{
				Platform.GetInstance().NotifyInMultiplayer();
			}
			if (!m_bWaitForSettingsToCloseForInvite)
			{
				break;
			}
			if (m_OpenPauseMenu != null)
			{
				if (!m_OpenPauseMenu.IsSettingsMenuOpen() && !m_OpenPauseMenu.IsSaveSlotMenuOpen())
				{
					m_OpenPauseMenu = null;
				}
			}
			else if (!T17DialogBoxManager.HasAnyOpenDialogs())
			{
				DisconnectAndEndLevel();
			}
			break;
		}
		case GLOBALSTART_MODE.END_LEVEL:
			Platform.GetInstance().SetNativeVoiceChatEnabled(state: false);
			if (IsLoadingFlowLoaded())
			{
				ShowLoadingScreen(delegate
				{
					Process_EndLevelState();
				});
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_END_LEVEL:
		{
			if (m_bLevelLoaded || m_bHUDMenuLoaded || m_bInGameMenusLoaded || m_bResultsLoaded)
			{
				break;
			}
			Process_PostEndLevelState();
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int num = allGamers.Length - 1; num >= 0; num--)
			{
				Gamer gamer = allGamers[num];
				if (gamer != null && gamer.IsLocal())
				{
					gamer.ForceNullOfPlayerObject();
				}
			}
			StartCoroutine(GarbageCollect());
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_END_LEVEL_GC;
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_END_LEVEL_GC:
			if (m_bGarbageCollected)
			{
				if (m_bLoadBackToBootFlow)
				{
					CleanupBeforeGoingToBoot();
				}
				else if (m_PreviewEditorLevel)
				{
					m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__LOAD_LEVEL_EDITOR;
					m_PreviewEditorLevel = false;
					m_strCustomLevelFile = SaveManager.GetInstance().GetCustomLevelFilePath(m_CurrentLevelSceneName, bWithoutFinal: true);
				}
				else
				{
					m_GlobalStartMode = GLOBALSTART_MODE.RELOAD_FRONTEND;
				}
			}
			break;
		case GLOBALSTART_MODE.LOAD_RESULTS:
			if (!m_bResultsLoaded)
			{
				StartCoroutine(LoadResultsScene());
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_LOAD_RESULTS;
			}
			break;
		case GLOBALSTART_MODE.WAIT_FOR_LOAD_RESULTS:
		{
			if (!m_bResultsLoaded)
			{
				break;
			}
			EscapePrisonFunctionality instance16 = EscapePrisonFunctionality.GetInstance();
			if (CutsceneManagerBase.GetInstance() != null && instance16 != null && instance16.DidPlayEscapeCutscene())
			{
				CutsceneManagerBase.GetInstance().EffectHoldFinalBit();
			}
			if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Results))
			{
				if (FadeManager.GetInstance() != null)
				{
					FadeManager.GetInstance().HideCurtain(3);
				}
				HUDMenuFlow instance17 = HUDMenuFlow.Instance;
				if (instance17 != null)
				{
					instance17.PlayGlobalEffect(UIAnimatedEffectController.Effects.PixelWindowToFullyClear, 0.5f);
				}
				((ResultsFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Results]).StartResults();
				m_GlobalStartMode = GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS;
			}
			break;
		}
		case GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS:
		{
			if (!((ResultsFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Results]).ExitResultsRequested() && !m_bLoadBackToBootFlow)
			{
				break;
			}
			ConfigManager instance13 = ConfigManager.GetInstance();
			if (null != instance13 && instance13.gameType == PrisonConfig.ConfigType.Cooperative)
			{
				NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, T17NetManager.NetOfflineMode);
			}
			else
			{
				Platform.GetInstance().SetNativeVoiceChatEnabled(state: false);
			}
			if (IsLoadingFlowLoaded())
			{
				ShowLoadingScreen(delegate
				{
					Process_EndLevelState();
				});
			}
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_END_LEVEL_BEHIND_RESULTS:
			break;
		case GLOBALSTART_MODE.RELOAD_FRONTEND:
		{
			if (!m_bFrontEndLoaded && !m_bStartedFrontEndSceneLoad)
			{
				m_bStartedFrontEndSceneLoad = true;
				StartCoroutine(LoadFrontendScene());
			}
			Platform.GetInstance().ResetLightBar();
			if (ErrorDialogHandler.bErrorFromBeingKicked)
			{
				NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
				ErrorDialogHandler.bErrorFromBeingKicked = false;
			}
			int count = m_LevelSceneRoots.Count;
			for (int num14 = 0; num14 < count; num14++)
			{
				m_LevelSceneRoots[num14] = null;
			}
			m_LevelSceneRoots.Clear();
			m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_RELOAD_FRONTEND;
			break;
		}
		case GLOBALSTART_MODE.WAIT_FOR_RELOAD_FRONTEND:
			if (!m_bFrontEndLoaded || !IsLoadingFlowLoaded())
			{
				break;
			}
			HideLoadingScreen(delegate
			{
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
				{
					((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).ShowMenus();
				}
				m_GlobalStartMode = GLOBALSTART_MODE.CHECK_INVITES;
				Platform.GetInstance().SetNativeVoiceChatEnabled(state: true);
			});
			break;
		case GLOBALSTART_MODE.SHOW_CREDITS_START:
			m_inviteAcceptedDuringCredits = false;
			ShowBlackScreenOnlyLoadingScreen(delegate
			{
				if (!m_bCreditsLoaded)
				{
					StartCoroutine(LoadCreditsScene());
				}
				m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_CREDITS_LOAD;
			}, setStateToDummy: false);
			break;
		case GLOBALSTART_MODE.WAIT_FOR_CREDITS_LOAD:
			if (!m_bCreditsLoaded)
			{
				break;
			}
			Credits.GetInstance().ShowAndReset();
			HideBlackScreenOnlyLoadingScreen(delegate
			{
				m_GlobalStartMode = GLOBALSTART_MODE.CREDITS;
				Gamer[] allGamers3 = Gamer.GetAllGamers();
				for (int num21 = 0; num21 < allGamers3.Length; num21++)
				{
					if (allGamers3[num21] != null && allGamers3[num21].m_RewiredPlayer != null)
					{
						T17EventSystem.ApplyCategories(allGamers3[num21].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Frontend);
					}
				}
			}, setStateToDummy: false);
			break;
		case GLOBALSTART_MODE.CREDITS:
			break;
		case GLOBALSTART_MODE.HIDE_CREDITS_BACK_TO_FRONTEND:
			ShowBlackScreenOnlyLoadingScreen(delegate
			{
				Credits.GetInstance().Hide();
				HideBlackScreenOnlyLoadingScreen(delegate
				{
					Gamer[] allGamers4 = Gamer.GetAllGamers();
					for (int num22 = 0; num22 < allGamers4.Length; num22++)
					{
						if (allGamers4[num22] != null && allGamers4[num22].m_RewiredPlayer != null)
						{
							T17EventSystem.ApplyCategories(allGamers4[num22].m_RewiredPlayer, T17EventSystem.InputCateogryStates.Frontend);
						}
					}
					if (m_inviteAcceptedDuringCredits)
					{
						m_GlobalStartMode = GLOBALSTART_MODE.CHECK_INVITES;
						FrontEndFlow instance19 = FrontEndFlow.Instance;
						if (instance19 != null && instance19.m_MainMenu != null)
						{
							BaseMenuBehaviour currentOpenMenu2 = instance19.m_MainMenu.GetCurrentOpenMenu();
							if (currentOpenMenu2 != null)
							{
								SettingsFrontendMenu settingsFrontendMenu = currentOpenMenu2 as SettingsFrontendMenu;
								if (settingsFrontendMenu != null)
								{
									settingsFrontendMenu.CallCancel();
								}
							}
						}
					}
					else
					{
						m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
					}
					m_inviteAcceptedDuringCredits = false;
				}, setStateToDummy: false);
			}, setStateToDummy: false);
			break;
		case GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE:
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__START_LEVEL_EDITOR:
			if (!IsLoadingFlowLoaded())
			{
				break;
			}
			ShowLoadingScreen(delegate
			{
				if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
				{
					((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).HideMenus();
				}
				m_bLoadError = false;
				m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__KILL_FRONTEND;
			});
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__KILL_FRONTEND:
			StartCoroutine(DeleteFrontEndScene());
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_KILL_FRONTEND;
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_KILL_FRONTEND:
			if (!m_bFrontEndLoaded)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__LOAD_LEVEL_EDITOR;
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__LOAD_LEVEL_EDITOR:
			StartCoroutine(LoadLevelEditorScene());
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_LEVEL_EDITOR;
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_LEVEL_EDITOR:
			if (m_bLevelEditorLoaded && LevelDetailsManager.GetInstance() != null)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_LEVEL_CREATION;
				if (!string.IsNullOrEmpty(m_strCustomLevelFile))
				{
					LevelDetailsManager.GetInstance().SetupFromSave(m_strCustomLevelFile, EditorLevelCreated);
				}
				else
				{
					LevelDetailsManager.GetInstance().CreateNewLevel(EditorLevelCreated);
				}
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__WAIT_LEVEL_CREATION:
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__KILL_LOADING:
			if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
			{
				((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).HideLoadingScreen(null);
			}
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR;
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR:
			if (m_bWaitingForEditorInviteResponse)
			{
				LevelEditor_UIController instance = LevelEditor_UIController.GetInstance();
				if (instance != null && instance.GetEditorInviteResponse())
				{
					m_bWaitingForEditorInviteResponse = false;
					EndEditorLevel();
				}
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__START_EXIT:
			if (IsLoadingFlowLoaded())
			{
				ShowLoadingScreen(delegate
				{
					m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__KILL_LEVEL_EDITOR;
				});
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__KILL_LEVEL_EDITOR:
			if (m_bLevelEditorLoaded)
			{
				StartCoroutine(DeleteLevelEditorScene());
				m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__WAIT_KILL_LEVEL_EDITOR;
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__WAIT_KILL_LEVEL_EDITOR:
			if (!m_bLevelEditorLoaded)
			{
				StartCoroutine(GarbageCollect());
				m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__RELOAD_FRONTEND;
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__RELOAD_FRONTEND:
			if (m_bGarbageCollected)
			{
				if (m_PreviewEditorLevel)
				{
					StartGameWithModeAndCurrentConfig(GLOBALSTART_GAME_MODES.SINGLE);
				}
				else
				{
					m_GlobalStartMode = GLOBALSTART_MODE.RELOAD_FRONTEND;
				}
			}
			break;
		case GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__WAIT_FOR_RELOAD_FRONTEND:
			break;
		}
	}

	private void EditorLevelCreated(LevelDetailsManager.RequestResultEnum eResult)
	{
		if (eResult == LevelDetailsManager.RequestResultEnum.Success)
		{
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__KILL_LOADING;
		}
	}

	public void InviteOnlineAreaCheckCallBack(bool bResult, Platform.OnlineAccessErrorCode returnCode, bool failureHandledPlatformside)
	{
		if (bResult)
		{
			if (T17NetInvites.HasInvite())
			{
				if (T17NetInvites.CheckPlatformErrors(T17NetInvites.RoomName))
				{
					T17NetInvites.Region = CloudRegionCode.none;
					NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
					T17NetInvites.Clear();
					m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
				}
				else
				{
					T17NetInvites.JoinInvitedRoom();
					m_GlobalStartMode = GLOBALSTART_MODE.PROCESSING_INVITE;
				}
			}
			else if (T17NetInvites.IsPlayTogetherHost())
			{
				T17NetInvites.Region = CloudRegionCode.none;
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
				List<PlaylistData> list = new List<PlaylistData>(LevelDataManager.GetInstance().GetCampaignPlaylists());
				list.RemoveAll(delegate(PlaylistData playlist)
				{
					if (playlist == null || playlist.m_Prisons[0].m_PrisonData.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.Tutorial)
					{
						return true;
					}
					bool flag = false;
					if (playlist.m_UnlockMilestone != null)
					{
						flag = !ProgressManager.GetInstance().GetMilestoneAchieved(playlist.m_UnlockMilestone.id);
					}
					if (KeyAwardManager.AreAllPrisonsUnlocked)
					{
						flag = false;
					}
					return false;
				});
				PlaylistData playlist2 = list[UnityEngine.Random.Range(0, list.Count)];
				SetSelectedPlaylist(playlist2);
				NetGoOnlineHelper.GoOnline(showConnectionFailedPrompt: true, bProfanityCheck: false, delegate(bool isConnected)
				{
					if (isConnected)
					{
						NetCreateRoomHelper.RequestCreateRoom(T17NetRoomGameView.GameRoomType.Private, PrisonConfig.ConfigType.Cooperative, delegate(bool roomSetupOk)
						{
							if (roomSetupOk)
							{
								T17NetInvites.Clear();
								StartGameWithModeAndCurrentConfig(GLOBALSTART_GAME_MODES.ONLINE);
							}
							else
							{
								T17DialogBox dialog2 = T17DialogBoxManager.GetDialog(forSingleUser: false);
								dialog2.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.CreateRoom.FailedTitle", "Text.Dialog.Net.CreateRoom.FailedBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
								dialog2.SetSymbol(T17DialogBox.Symbols.Error);
								dialog2.Show();
								T17NetInvites.Clear();
							}
						}, showDialogs: true, string.Empty);
					}
					else
					{
						T17DialogBox dialog = T17DialogBoxManager.GetDialog(forSingleUser: false);
						dialog.Initialize(hasConfirm: true, hasDecline: false, hasCancel: false, "Text.Dialog.Net.CreateRoom.FailedTitle", "Text.Dialog.Net.CreateRoom.FailedBody", "Text.Dialog.Prompt.Ok", string.Empty, string.Empty);
						dialog.SetSymbol(T17DialogBox.Symbols.Error);
						dialog.Show();
						T17NetInvites.Clear();
					}
				}, delegate
				{
					Platform.GetInstance().ResetPlaytogether();
					T17NetInvites.Clear();
				});
			}
			else
			{
				NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
				T17NetInvites.Region = CloudRegionCode.none;
				T17NetInvites.Clear();
				m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
			}
		}
		else
		{
			NetLoadSync.EventSend(T17NetConfig.NetEventTypes.Load_NotStarted);
			T17NetInvites.Region = CloudRegionCode.none;
			T17NetInvites.Clear();
			m_GlobalStartMode = GLOBALSTART_MODE.SHOW_FRONTEND;
		}
	}

	private IEnumerator HideLoadScreenAfterDelay(float delay)
	{
		yield return new WaitForSecondsRealtime(delay);
		WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
		bool waiting = true;
		while (waiting)
		{
			if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
			{
				waiting = false;
				((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).HideLoadingScreen(delegate
				{
					RenderTargetManager.CheckForLostRTs();
				});
			}
			else
			{
				yield return endOfFrame;
			}
		}
		m_DelayedHideLoadingScreenRoutine = null;
	}

	private void Process_EndLevelState()
	{
		InGameMenuFlow instance = InGameMenuFlow.Instance;
		if (instance != null)
		{
			instance.EnableSplitScreenEffects(bOn: false);
		}
		TutorialManager instance2 = TutorialManager.GetInstance();
		if (instance2 != null)
		{
			instance2.OnLevelEnd();
		}
		T17NetworkManager instance3 = T17NetworkManager.GetInstance();
		if (instance3 != null)
		{
			instance3.Stop();
		}
		CraftManager instance4 = CraftManager.GetInstance();
		if (instance4 != null)
		{
			instance4.DestroyLiveList();
		}
		UpdateManager instance5 = UpdateManager.GetInstance();
		if (instance5 != null)
		{
			UpdateManager.GetInstance().LevelUnload();
		}
		m_bProfanityFilteringSuccessfullyCompleted = false;
		m_bProfanityFilteringFinished = false;
		UnityEngine.Debug.Log("     +++++++++++++   Out call  to    DeleteLevelScene ");
		GlobalLevelCleanUp();
		T17BehaviourManager.GetInstance().PurgeClasses();
		if (m_bResultsLoaded)
		{
			StartCoroutine(DeleteResultsScene());
		}
		if (m_bInGameMenusLoaded)
		{
			StartCoroutine(DeleteInGameMenusScene());
		}
		if (m_bHUDMenuLoaded)
		{
			StartCoroutine(DeleteHUDScene());
		}
		if (m_bLevelLoaded)
		{
			StartCoroutine(DeleteLevelScene());
		}
		m_GlobalStartMode = GLOBALSTART_MODE.WAIT_FOR_END_LEVEL;
	}

	private void Process_PostEndLevelState()
	{
		Resources.UnloadUnusedAssets();
		if (m_ReturnToFrontendRoute != ReturnToFrontendRoutes.VersusLobby)
		{
			Gamer.DeleteLocalNonPrimaryGamers();
		}
		if (m_CameraToHideDuringGame != null)
		{
			for (int i = 0; i < m_CameraToHideDuringGame.Length; i++)
			{
				if (m_CameraToHideDuringGame[i] != null)
				{
					m_CameraToHideDuringGame[i].gameObject.SetActive(value: true);
				}
			}
		}
		Localization.RemoveKeyOverride();
		if (GlobalSave.GetInstance() != null)
		{
			GlobalSave.GetInstance().RequestSave();
		}
		if (ProgressManager.GetInstance() != null)
		{
			ProgressManager.GetInstance().ProcessPendingMilestones();
		}
		PrisonCustomisationManager.ClearCache();
		if (m_ReturnToFrontendRoute == ReturnToFrontendRoutes.VersusLobby)
		{
			NetBluePrintDetails instance = NetBluePrintDetails.Instance;
			if (instance != null)
			{
				instance.NextPrison();
			}
		}
	}

	private bool IsLoadingFlowLoaded()
	{
		return m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading);
	}

	private void ShowLoadingScreen(LoadingFlow.LoadingFlowHandler callback, bool setStateToDummy = true)
	{
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
		{
			if (!((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).ShowLoadingScreen(callback))
			{
				callback?.Invoke();
			}
			else if (setStateToDummy)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE;
			}
		}
	}

	private void HideLoadingScreen(LoadingFlow.LoadingFlowHandler callback, bool setStateToDummy = true)
	{
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
		{
			((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).HideLoadingScreen(callback);
			if (setStateToDummy)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE;
			}
		}
	}

	private void ShowBlackScreenOnlyLoadingScreen(LoadingFlow.LoadingFlowHandler callback, bool setStateToDummy = true)
	{
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
		{
			((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).DoBlackScreenOnlyLoad(callback);
			if (setStateToDummy)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE;
			}
		}
	}

	private void HideBlackScreenOnlyLoadingScreen(LoadingFlow.LoadingFlowHandler callback, bool setStateToDummy = true)
	{
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Loading))
		{
			((LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading]).DoHideBlackScreenOnlyLoad(callback);
			if (setStateToDummy)
			{
				m_GlobalStartMode = GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE;
			}
		}
	}

	public bool ShowTheFrontEndBackGroundVideoOnly()
	{
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Frontend))
		{
			((FrontEndFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Frontend]).SpecialFrontEndShowVideoBackGroundOnly();
			return true;
		}
		return false;
	}

	public void DisconnectAndEndLevel()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		EndLevel(bShowResults: false);
	}

	public void DisconnectAndEndResults()
	{
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Results))
		{
			((ResultsFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Results]).ReturnToMainMenu();
		}
	}

	public void EndResultsAndBackToDefault()
	{
		NetConnectAndJoinRoom.RequestConnectionState(T17NetManager.m_DefaultConnectionState, silentErrorDialogMode: true);
		if (m_RegisteredFlows.ContainsKey(BaseFlowBehaviour.FlowType.Results))
		{
			((ResultsFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Results]).ReturnToMainMenu();
		}
	}

	public void ResetBackToPressAToStart()
	{
		m_bLoadBackToBootFlow = true;
		m_bProcessedLoadBackToBootFlow = false;
		Time.timeScale = 1f;
		T17DialogBoxManager.ReleaseAll();
		PlatformIO.GetInstance().CancelAllIORequests();
		SaveManager.GetInstance().ResetBackToIdle();
	}

	private void CleanupBeforeGoingToBoot()
	{
		Platform.GetInstance().CleanupBeforeResetToBoot();
		T17EventSystemsManager.Instance.ResetAll();
		GlobalSave.GetInstance().ResetSave();
		AudioController.Reset();
		Platform.controllerVibrationEnabled = true;
		NetConnectAndJoinRoom.PhotonRegion = CloudRegionCode.none;
		m_bWaitForSettingsToCloseForInvite = false;
		m_OpenPauseMenu = null;
		m_inviteAcceptedDuringCredits = false;
		if (m_bBootLoaded)
		{
			return;
		}
		Gamer[] allGamers = Gamer.GetAllGamers();
		for (int i = 0; i < allGamers.Length; i++)
		{
			if (allGamers[i] != null)
			{
				Gamer.DeleteGamer(i, clearRewiredMaps: true);
			}
		}
		StartCoroutine(LoadBootScene());
		m_GlobalStartMode = GLOBALSTART_MODE.START_LOAD_BOOT;
	}

	private void SetupAreaManagers()
	{
		if (RoomManager.GetInstance() != null)
		{
			TimedNetworkService();
			RoomManager.GetInstance().Init();
		}
		if (FacadesManager.GetInstance() != null)
		{
			TimedNetworkService();
			FacadesManager.GetInstance().Init();
		}
		if (LightingManager.GetInstance() != null)
		{
			TimedNetworkService();
			LightingManager.GetInstance().Init();
		}
	}

	private void SetUpManagers_StartInit()
	{
		if (AIEventManager.GetInstance() != null)
		{
			TimedNetworkService();
			AIEventManager.GetInstance().Initialise();
		}
		if (JobsManager.GetInstance() != null)
		{
			TimedNetworkService();
			JobsManager.GetInstance().Init();
		}
		if (OpinionManager.GetInstance() != null)
		{
			TimedNetworkService();
			OpinionManager.GetInstance().Initialise();
		}
		if (VendorManager.GetInstance() != null)
		{
			TimedNetworkService();
			VendorManager.GetInstance().Initialise();
		}
		if (NPCManager.GetInstance() != null)
		{
			TimedNetworkService();
			NPCManager.GetInstance().Init();
		}
		List<Character> allCharacters = Character.GetAllCharacters();
		if (T17NetManager.IsMasterClient && !PrisonSnapshotIO.IsThereSaveData())
		{
			for (int i = 0; i < allCharacters.Count; i++)
			{
				TimedNetworkService();
				Character character = allCharacters[i];
				if (null == character)
				{
					m_bLoadError = true;
					m_LoadErrorDescription = "SetUpManagers_PrePlayers - Null character in GetAllCharacters array.";
					break;
				}
				allCharacters[i].RequestStartingItemsRPC();
				allCharacters[i].ProcessStartingItems();
			}
			RoutineManager.GetInstance().AllItemContainerRefresh(staggerItemRefresh: false, bUpdateNetworkService: true);
		}
		if (PlayerDataManager.GetInstance() != null)
		{
			TimedNetworkService();
			PlayerDataManager.GetInstance().Initialise();
		}
		if (TagManager.GetInstance() != null)
		{
			TimedNetworkService();
			TagManager.GetInstance().Initialise();
		}
		if (CutsceneManagerBase.GetInstance() != null)
		{
			TimedNetworkService();
			CutsceneManagerBase.GetInstance().RegisterWithCullingObject();
		}
		if (QuestManager.GetInstance() != null)
		{
			TimedNetworkService();
			QuestManager.GetInstance().Initialise();
		}
		GC.Collect();
		TimedNetworkService();
	}

	private void SetUpManagers()
	{
		if (CullingObjectCollector.GetInstance() != null)
		{
			TimedNetworkService();
			CullingObjectCollector.GetInstance().Init(FloorManager.GetInstance().currentMaxFloor, m_LevelSceneRoots);
		}
		if (CutsceneManagerBase.GetInstance() != null)
		{
			TimedNetworkService();
			CutsceneManagerBase.GetInstance().RegisterWithCullingObject();
		}
		if (ObjectiveManager.GetInstance() != null)
		{
			TimedNetworkService();
			ObjectiveManager.GetInstance().RegisterToSaveSystem();
		}
	}

	private void GlobalLevelCleanUp()
	{
		if (AstarPath.active != null)
		{
			AstarPath.active.BlockUntilPathQueueBlocked();
			PathPool.Clear();
		}
		else if (PathPool.GetSize(typeof(T17_ABPath)) > 0)
		{
			UnityEngine.Debug.LogError("MEMORY LEAK: We can't clean up pathfinding pool without a astar path!!");
		}
		if (NPCManager.GetInstance() != null)
		{
			NPCManager.GetInstance().Cleanup();
		}
		if (SpeechManager.GetInstance() != null)
		{
			SpeechManager.GetInstance().CleanUp();
		}
		Character.CleanUp();
		JobOfficerScreen.Cleanup();
		AICharacter_JobOfficer.Cleanup();
		AICharacter_Guard.CleanUp();
		AICharacter.CleanUp();
		Character.CleanUp();
		CCTVCamera.Cleanup();
		CarryObjectInteraction.CleanUp();
		DeskInteraction.CleanUp();
		ToiletInteraction.Cleanup();
		ConstructEndgameInteraction.CleanUp();
		AIEventManager.CleanUp();
		Character.CleanUp();
		TransitionPoint.Cleanup();
		RoomManager.CleanUp();
		ObjectiveSceneElement.Cleanup();
		WorldCanvasTrackedUIElements.Cleanup();
		NavMeshUtil.Cleanup();
		EscapistsRaycast.Cleanup();
		CustomLightRenderer.Cleanup();
		ChatFeedManager.ForceCleanup();
	}

	private IEnumerator LoadRewiredScene()
	{
		m_bRewiredLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("RewiredInputSettings", LoadSceneMode.Additive);
		yield return m_Async;
		m_bRewiredLoaded = true;
	}

	private IEnumerator LoadBootScene()
	{
		m_bBootLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("Boot", LoadSceneMode.Additive);
		yield return m_Async;
		m_bBootLoaded = true;
	}

	private IEnumerator LoadLoadingScene()
	{
		m_bLoadedLoadingScene = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("loading", LoadSceneMode.Additive);
		yield return m_Async;
		m_bLoadedLoadingScene = true;
	}

	private IEnumerator LoadFrontendScene()
	{
		m_bFrontEndLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("Frontend", LoadSceneMode.Additive);
		yield return m_Async;
		m_Async = AssetManager.instance.LoadSceneAsync("FrontendEditor", LoadSceneMode.Additive);
		yield return m_Async;
		m_bFrontEndLoaded = true;
	}

	private IEnumerator LoadLevelScene()
	{
		m_bLockInWithLevelReturn = false;
		m_bLevelFailedToLoad = false;
		m_bLevelLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		bool bCustom = m_CustomLevel;
		if (bCustom && T17NetManager.IsMasterClient && (m_CustomLevelData == null || m_CustomLevelData.Count == 0))
		{
			bCustom = false;
		}
		if (bCustom)
		{
			if (!T17NetManager.IsMasterClient)
			{
				while (m_CustomLevelData == null || m_CustomLevelData.Count == 0)
				{
					yield return new WaitForSeconds(0.25f);
				}
			}
			else if (m_CustomLevelData == null || m_CustomLevelData.Count == 0)
			{
				m_bLevelFailedToLoad = true;
				yield break;
			}
			if (m_LoadedSceneNamesToDelete.Count > 0)
			{
				for (int num = m_LoadedSceneNamesToDelete.Count - 1; num >= 0; num--)
				{
				}
			}
			if (!BuildVersion.m_UseAssetBundles && SceneUtility.GetBuildIndexByScenePath("LevelEditorGameScene") <= 0)
			{
				m_bLevelFailedToLoad = true;
				yield break;
			}
			m_LoadedSceneNamesToDelete.Add("LevelEditorGameScene");
			m_Async = AssetManager.instance.LoadSceneAsync("LevelEditorGameScene", LoadSceneMode.Additive, alsoLoadIngameBundles: true);
			yield return m_Async;
			UnityEngine.Debug.Log("Loaded Game Scene - Getting block scene");
			if (LevelDetailsManager.GetInstance() == null)
			{
				m_bLevelFailedToLoad = true;
				yield break;
			}
			yield return null;
			if (!LevelDetailsManager.GetInstance().LoadUserLevel(ref m_CustomLevelData))
			{
				m_bLevelFailedToLoad = true;
				yield break;
			}
			string strBlockSceneName = LevelDetailsManager.GetInstance().GetBlockSceneName();
			if (string.IsNullOrEmpty(strBlockSceneName))
			{
				m_bLevelFailedToLoad = true;
				yield break;
			}
			if (!m_PreviewEditorLevel)
			{
				if (!BuildVersion.m_UseAssetBundles && SceneUtility.GetBuildIndexByScenePath(strBlockSceneName) <= 0)
				{
					m_bLevelFailedToLoad = true;
					yield break;
				}
				AssetManager.instance.LoadScene(strBlockSceneName, LoadSceneMode.Additive, alsoLoadBundleOverrides: true);
				yield return null;
			}
			if (BaseLevelManager.GetInstance() == null)
			{
				m_bLevelFailedToLoad = true;
				yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync(strBlockSceneName));
				yield break;
			}
			BaseLevelManager.GetInstance().enabled = true;
			yield return null;
			while (!BuildingBlockManager.GetInstance().GenerateActualBlockData())
			{
				yield return null;
			}
			UnityEngine.Debug.Log("Loaded Block Scene - Initializing and building level");
			if (!LevelDetailsManager.GetInstance().StartMakingLevel())
			{
				m_bLevelFailedToLoad = true;
				yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync(strBlockSceneName));
				yield break;
			}
			UnityEngine.Debug.Log("Started creating the level");
			while (LevelDetailsManager.GetInstance().IsDetailsManagerBusy())
			{
				yield return null;
			}
			UnityEngine.Debug.Log("Finished running all the instructions to build the level.");
			LevelDetailsManager.GetInstance().StartRemoveSelf();
			yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync(strBlockSceneName));
			m_bLevelLoaded = true;
			if (LevelScript.GetInstance() != null && LevelScript.GetCurrentLevelInfo() != null && !PrisonSnapshotIO.IsThereSaveData())
			{
				Localization.Get("Text.ActivityFeed.LockedUp.Cap", out var localized);
				string localized2 = "level_script";
				LevelScript instance = LevelScript.GetInstance();
				if (instance != null)
				{
					Localization.Get(instance.m_LevelSetup.m_NameLocalizationKey, out localized2);
				}
				localized = localized.Replace("%NAME%", localized2);
				Localization.Get("Text.ActivityFeed.LockedUp.Con", out var localized3);
				Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Locked_Up_In, localized, localized3, (uint)LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
			}
		}
		else
		{
			string sceneName = string.Empty;
			if (!m_DebugForceLoadLevel.Equals(string.Empty))
			{
				string debugForceLoadLevel = m_DebugForceLoadLevel;
				LevelScript.PRISON_ENUM prisonEnum = LevelScript.PRISON_ENUM.Centre_Perks;
				int num2 = 12;
				bool flag = false;
				for (int i = 0; i <= num2; i++)
				{
					LevelScript.PRISON_ENUM pRISON_ENUM = (LevelScript.PRISON_ENUM)i;
					string text = pRISON_ENUM.ToString();
					if (text.Equals(debugForceLoadLevel))
					{
						prisonEnum = (LevelScript.PRISON_ENUM)i;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					PlaylistData playlistData = LevelDataManager.GetInstance().GetCampaignForPrison(prisonEnum);
					if (playlistData == null)
					{
						playlistData = LevelDataManager.GetInstance().GetCampaignPlaylists()[0];
					}
					SetSelectedPlaylist(playlistData);
					SetSelectedLevelToPlaylistIndex(0);
					PrisonData prisonData = playlistData.m_Prisons[0].m_PrisonData;
					Customisation[] array = CustomisationManager.GetInstance().GenerateDefaultCustomisations(prisonData);
					PrisonCustomisationManager.m_NpcCustomisations = new Customisation[array.Length];
					for (int j = 0; j < array.Length; j++)
					{
						PrisonCustomisationManager.m_NpcCustomisations[j] = new Customisation(array[j]);
					}
					sceneName = prisonEnum.ToString();
				}
				else
				{
					sceneName = debugForceLoadLevel;
				}
			}
			else if (m_netBluePrintDetails != null)
			{
				sceneName = m_netBluePrintDetails.LevelSceneName;
				if (!T17NetManager.IsMasterClient)
				{
					if (DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetBluePrintDetails))
					{
					}
					while (string.IsNullOrEmpty(m_netBluePrintDetails.LevelSceneName))
					{
						yield return new WaitForSeconds(0.25f);
					}
					sceneName = m_netBluePrintDetails.LevelSceneName;
				}
			}
			if (m_LoadedSceneNamesToDelete.Count > 0)
			{
				for (int num3 = m_LoadedSceneNamesToDelete.Count - 1; num3 >= 0; num3--)
				{
				}
			}
			m_LoadedSceneNamesToDelete.Add(sceneName);
			m_Async = AssetManager.instance.LoadSceneAsync(sceneName, LoadSceneMode.Additive, alsoLoadIngameBundles: true);
			yield return m_Async;
			m_bLevelLoaded = true;
			if (LevelScript.GetInstance() != null && LevelScript.GetCurrentLevelInfo() != null && !PrisonSnapshotIO.IsThereSaveData())
			{
				Localization.Get("Text.ActivityFeed.LockedUp.Cap", out var localized4);
				string localized5 = "level_script";
				LevelScript instance2 = LevelScript.GetInstance();
				if (instance2 != null)
				{
					Localization.Get(instance2.m_LevelSetup.m_NameLocalizationKey, out localized5);
				}
				localized4 = localized4.Replace("%NAME%", localized5);
				Localization.Get("Text.ActivityFeed.LockedUp.Con", out var localized6);
				Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Locked_Up_In, localized4, localized6, (uint)LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
			}
		}
		if (!Platform.GetInstance().GetSaveDirectory())
		{
			StartCoroutine(LoadLevelScene());
		}
	}

	private IEnumerator LoadLevelEditorScene()
	{
		UnityEngine.Debug.Log(" ***** Start Loading Level editor");
		m_bLevelEditorLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("LevelEditorUIScene", LoadSceneMode.Additive);
		yield return m_Async;
		m_bLevelEditorLoaded = true;
	}

	private IEnumerator DeleteLevelEditorScene()
	{
		if (!m_PreviewEditorLevel)
		{
			LevelDetailsManager levelDetailsMan = LevelDetailsManager.GetInstance();
			if (levelDetailsMan != null)
			{
				m_Async = SceneManager.UnloadSceneAsync(levelDetailsMan.GetBlockSceneName());
				yield return m_Async;
			}
		}
		m_Async = AssetManager.instance.UnloadSceneCoroutine("LevelEditorUIScene");
		yield return m_Async;
		m_bLevelEditorLoaded = false;
	}

	private IEnumerator LoadHUDMenuScene()
	{
		UnityEngine.Debug.Log(" ***** Start Loading  HUD Menu Scene");
		m_bHUDMenuLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("Hud", LoadSceneMode.Additive);
		UnityEngine.Debug.Log("Loading HUD Menu Scene");
		yield return m_Async;
		UnityEngine.Debug.Log("Loaded HUD Menu Scene");
		if (HUDLoadScript.GetInstance() == null || !HUDLoadScript.GetInstance().m_PreBuildBehaviourLists)
		{
			GameObject gameObject = GameObject.Find("HUDParent");
			T17BehaviourManager.GetInstance().AddClassesFromRoot(gameObject.transform);
		}
		else
		{
			T17BehaviourManager.GetInstance().InjectClasses(HUDLoadScript.GetInstance().m_NetworkBehaviourClasses, HUDLoadScript.GetInstance().m_MonoBehaviourClasses);
		}
		m_bHUDMenuLoaded = true;
	}

	private IEnumerator LoadInGameMenusScene()
	{
		UnityEngine.Debug.Log(" ***** Start Loading  Ingame Scene");
		m_bInGameMenusLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("InGameMenus", LoadSceneMode.Additive);
		yield return m_Async;
		if (IGMLoadScript.GetInstance() == null || !IGMLoadScript.GetInstance().m_PreBuildBehaviourLists)
		{
			GameObject gameObject = GameObject.Find("InGameMenuParent");
			T17BehaviourManager.GetInstance().AddClassesFromRoot(gameObject.transform);
		}
		else
		{
			T17BehaviourManager.GetInstance().InjectClasses(IGMLoadScript.GetInstance().m_NetworkBehaviourClasses, IGMLoadScript.GetInstance().m_MonoBehaviourClasses);
		}
		m_bInGameMenusLoaded = true;
	}

	private IEnumerator LoadResultsScene()
	{
		m_bResultsLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("RoundResults", LoadSceneMode.Additive);
		yield return m_Async;
		m_bResultsLoaded = true;
	}

	private IEnumerator LoadCreditsScene()
	{
		m_bCreditsLoaded = false;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		m_Async = AssetManager.instance.LoadSceneAsync("Credits", LoadSceneMode.Additive);
		yield return m_Async;
		m_bCreditsLoaded = true;
	}

	private IEnumerator DeleteRewiredScene()
	{
		UnityEngine.Debug.Log("  ******  DeleteRewiredScene  ***");
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("RewiredInputSettings"));
		UnityEngine.Debug.Log("  ******  DeleteRewiredScene  ** 1 *");
		GC.Collect();
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		yield return null;
		Resources.UnloadUnusedAssets();
		yield return null;
		GC.Collect();
		UnityEngine.Debug.Log("  ******  DeleteRewiredScene  ** 2 *");
		m_bRewiredLoaded = false;
	}

	private IEnumerator DeleteBootScene()
	{
		UnityEngine.Debug.Log("  ******  DeleteBootScene  ***");
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("Boot"));
		UnityEngine.Debug.Log("  ******  DeleteBootScene  ** 1 *");
		GC.Collect();
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		yield return null;
		Resources.UnloadUnusedAssets();
		yield return null;
		GC.Collect();
		UnityEngine.Debug.Log("  ******  DeleteBootScene  ** 2 *");
		m_bBootLoaded = false;
	}

	private IEnumerator DeleteFrontEndScene()
	{
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("Frontend"));
		yield return AssetManager.instance.UnloadSceneAsync("FrontendEditor");
		if (m_bCreditsLoaded)
		{
			yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("Credits"));
		}
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		GC.Collect();
		yield return null;
		Resources.UnloadUnusedAssets();
		yield return null;
		GC.Collect();
		m_bFrontEndLoaded = false;
		m_bStartedFrontEndSceneLoad = false;
		m_bCreditsLoaded = false;
	}

	private IEnumerator DeleteLevelScene()
	{
		while (m_bHUDMenuLoaded)
		{
			yield return null;
		}
		UnityEngine.Debug.Log("     +++++++++++++   DeleteLevelScene  ");
		if (CullingObjectCollector.GetInstance() != null)
		{
			CullingObjectCollector.GetInstance().ForceCleanup();
		}
		ConfigManager configMan = ConfigManager.GetInstance();
		bool bJustPlayedAVersusMatch = false;
		if (configMan != null && configMan.gameType == PrisonConfig.ConfigType.Versus)
		{
			UnityEngine.Debug.Log("   *******   bJustPlayedAVersusMatch *******");
			bJustPlayedAVersusMatch = true;
		}
		for (int iDeleteScene = m_LoadedSceneNamesToDelete.Count - 1; iDeleteScene >= 0; iDeleteScene--)
		{
			string sceneName = m_LoadedSceneNamesToDelete[iDeleteScene];
			UnityEngine.Debug.Log("  ** UNLOADING SCENE   " + sceneName);
			yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync(sceneName));
		}
		m_LoadedSceneNamesToDelete.Clear();
		m_bLevelLoaded = false;
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView instance = T17NetRoomGameView.Instance;
			if (instance != null)
			{
				if (bJustPlayedAVersusMatch)
				{
					instance.ClearPrisonProperties();
				}
				else
				{
					instance.ClearCustomProperties();
				}
			}
		}
		NetPrisonViewDetails netPrisonViewDetails = NetPrisonViewDetails.Instance;
		if (netPrisonViewDetails != null)
		{
			netPrisonViewDetails.ResetPrisonView();
		}
	}

	private IEnumerator DeleteHUDScene()
	{
		while (m_bInGameMenusLoaded)
		{
			yield return null;
		}
		GameObject HUDParent = GameObject.Find("HUDParent");
		if ((bool)HUDParent)
		{
			UnityEngine.Object.Destroy(HUDParent);
		}
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("Hud"));
		m_bHUDMenuLoaded = false;
	}

	private IEnumerator DeleteInGameMenusScene()
	{
		while (m_bResultsLoaded)
		{
			yield return null;
		}
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("InGameMenus"));
		m_bInGameMenusLoaded = false;
	}

	private IEnumerator DeleteResultsScene()
	{
		yield return StartCoroutine(AssetManager.instance.UnloadSceneAsync("RoundResults"));
		yield return m_Async;
		m_bResultsLoaded = false;
	}

	private IEnumerator GarbageCollect()
	{
		m_bGarbageCollected = false;
		GameObjectEx.ForceCleanup();
		AmsMultiSceneSetup.ForceCleanup();
		JSONSerializer.ForceCleanup();
		DirectorCamera.ForceCleanup();
		GC.Collect();
		yield return null;
		m_Async = Resources.UnloadUnusedAssets();
		yield return m_Async;
		GC.Collect();
		yield return null;
		AudioController.UnloadBank("Ambience_Prison_01");
		AudioController.UnloadBank("Music_Prison_01");
		AudioController.UnloadBank("Player_SFX");
		yield return null;
		m_bGarbageCollected = true;
	}

	public bool HookUpFlow(BaseFlowBehaviour flow, BaseFlowBehaviour.FlowType flowType)
	{
		if (m_RegisteredFlows.ContainsKey(flowType))
		{
			if (flow != null)
			{
				UnityEngine.Object.Destroy(flow);
			}
			return false;
		}
		m_RegisteredFlows.Add(flowType, flow);
		return true;
	}

	public bool DoneWithFlow(BaseFlowBehaviour.FlowType flowType, bool fromDestroy = false)
	{
		if (m_RegisteredFlows.ContainsKey(flowType))
		{
			switch (flowType)
			{
			case BaseFlowBehaviour.FlowType.Boot:
				if (!m_bLoadBackToBootFlow)
				{
					m_GlobalStartMode = GLOBALSTART_MODE.KILL_BOOT;
				}
				break;
			}
			if (fromDestroy)
			{
				m_RegisteredFlows.Remove(flowType);
			}
			return true;
		}
		return false;
	}

	public void ResetLoadStates()
	{
		if (T17NetManager.NetOfflineMode || T17NetManager.IsMasterClient)
		{
			T17NetLoadSync.Instance.Reset();
			T17NetLoadSync.Instance.UpdateStatesFromGamers();
		}
	}

	public void SetupLoadStatesAndStartGameWithConfig(GLOBALSTART_GAME_MODES config = GLOBALSTART_GAME_MODES.SINGLE)
	{
		StartGameWithModeAndCurrentConfig(config);
	}

	public void SetupLoadStatesAndNewCampaignPrisonSetup()
	{
		switch (FrontEndFlow.Instance.GetCurrentMenuType())
		{
		case FrontEndFlow.MenuType.GameFrontend:
			FrontEndFlow.Instance.SwitchToFrontEndMenuType(FrontendRootMenu.FrontendMenuTypeToOpen.PrisonSetupMenu);
			break;
		case FrontEndFlow.MenuType.LevelEditor:
			FrontEndFlow.Instance.SwitchToFrontEndMenuType(EditorRootMenu.EditorMenuTypeToOpen.PrisonSetupMenu);
			break;
		}
	}

	public void StartGameWithModeAndCurrentConfig(GLOBALSTART_GAME_MODES mode)
	{
		StartLevel(mode, GetCurrentSelectedLevel(), GetCurrentSelectedConfigID());
	}

	[ContextMenu("Save")]
	public void SaveGameFromContextMenu()
	{
		SaveGame();
	}

	public static bool SaveGame()
	{
		if (SaveManager.GetInstance() != null && SaveManager.GetInstance().CanWeSaveNow(bRequestIsARetryFromWithin: false))
		{
			SaveManager.GetInstance().SaveGame(null);
			return true;
		}
		return false;
	}

	public bool LoadSaveGame()
	{
		string text = string.Empty;
		if (SaveManager.GetInstance() != null)
		{
			text = SaveManager.GetInstance().GetLoadedGame();
		}
		bool flag = PrisonSnapshotIO.RestoreSnapshot(text, ref m_CustomLevelData);
		string empty = string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			string text2 = NetConnectAndJoinRoom.GetLobbyName(PrisonConfig.ConfigType.Cooperative) + "_" + Platform.GetInstance().GetPrimaryUserName() + UnityEngine.Random.Range(1, 8) + "_" + DateTime.UtcNow.ToString();
			empty = text2.GetHashCode().ToString();
		}
		else
		{
			empty = PrisonSnapshotIO.GetHostKeyHash();
		}
		if (flag && T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.GamerCount, Gamer.m_GamerCount);
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.HostName, Platform.GetInstance().GetPrimaryUserName().ToLowerInvariant());
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.HostKey, empty);
		}
		return flag;
	}

	public void StartLevel(GLOBALSTART_GAME_MODES gameMode, string sceneName, int configID)
	{
		if ((gameMode == GLOBALSTART_GAME_MODES.SINGLE || gameMode == GLOBALSTART_GAME_MODES.LOCAL) && NetConnectAndJoinRoom.GetRequestedConnectionState() != 0)
		{
			NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
		}
		m_GameMode = gameMode;
		StartLevelLoad(sceneName, configID);
	}

	public void StartLevelLoad(string sceneName, int configID)
	{
		m_netBluePrintDetails = NetBluePrintDetails.Instance;
		m_wasMasterClientAtStartOfLevelLoad = T17NetManager.IsMasterClient;
		if (m_netBluePrintDetails != null)
		{
			if (T17NetManager.IsMasterClient)
			{
				if (m_CurrentPlaylistData != null)
				{
					int mapRotationIndex = NetBluePrintDetails.Instance.MapRotationIndex;
					if (mapRotationIndex >= 0)
					{
						PrisonData currentSelectedPrisonData = GetCurrentSelectedPrisonData();
						if (currentSelectedPrisonData != null && currentSelectedPrisonData.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
						{
							m_CustomLevel = true;
							m_CustomLevelData.Clear();
							string customLevelFilePath = SaveManager.GetInstance().GetCustomLevelFilePath(sceneName);
							if (File.Exists(customLevelFilePath))
							{
								try
								{
									byte[] array = File.ReadAllBytes(customLevelFilePath);
									if (array != null && array.Length > 0)
									{
										m_CustomLevelData.AddRange(array);
									}
									else
									{
										m_bLoadError = true;
									}
								}
								catch (Exception)
								{
									m_bLoadError = true;
								}
							}
							else
							{
								m_bLoadError = true;
							}
						}
						Customisation[] customisableNpcsForPrison = CustomisationManager.GetInstance().GetCustomisableNpcsForPrison(currentSelectedPrisonData, generateIfNotFound: true, m_PreviewEditorLevel);
						if (customisableNpcsForPrison != null && customisableNpcsForPrison.Length > 0)
						{
							if (!m_CustomLevel)
							{
								CustomisationManager.EnforceRobinson(currentSelectedPrisonData, customisableNpcsForPrison);
							}
							PrisonCustomisationManager.m_NpcCustomisations = new Customisation[customisableNpcsForPrison.Length];
							for (int i = 0; i < customisableNpcsForPrison.Length; i++)
							{
								PrisonCustomisationManager.m_NpcCustomisations[i] = new Customisation(customisableNpcsForPrison[i]);
							}
						}
					}
				}
				PrisonCustomisationManager.m_CustomisationSeed = UnityEngine.Random.state;
			}
			Gamer[] allGamers = Gamer.GetAllGamers();
			for (int num = allGamers.Length - 1; num >= 0; num--)
			{
				Gamer gamer = allGamers[num];
				if (gamer != null && gamer.IsLocal())
				{
					gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.PreMenu;
					gamer.UpdateGamer(gamer.m_iControllerIndex, gamer.m_PhotonID, -1, gamer.m_GamerName, gamer.m_PlayerObject, bPrimarySet: false, bPrimary: false, null);
				}
			}
		}
		if (T17NetManager.IsMasterClient)
		{
			T17NetRoomGameView.Instance.SetCustomProperty(T17NetRoomGameView.CustomProperty.EscapeState, 0);
			Gamer.InvalidateOnlineIds(bNetView: true, bPlayerId: false);
		}
		START_TIME = Time.realtimeSinceStartup;
		m_GlobalStartMode = GLOBALSTART_MODE.START_LEVEL_LOAD;
		T17NetManager.SetTimePingIntervalToLoadingRate();
	}

	public void ShowCreditsScreen()
	{
		if (m_GlobalStartMode != GLOBALSTART_MODE.CREDITS && m_GlobalStartMode != GLOBALSTART_MODE.SHOW_CREDITS_START && m_GlobalStartMode != GLOBALSTART_MODE.WAIT_FOR_CREDITS_LOAD && m_GlobalStartMode != GLOBALSTART_MODE.HIDE_CREDITS_BACK_TO_FRONTEND)
		{
			m_GlobalStartMode = GLOBALSTART_MODE.SHOW_CREDITS_START;
		}
	}

	public void HideCreditsScreen()
	{
		m_GlobalStartMode = GLOBALSTART_MODE.HIDE_CREDITS_BACK_TO_FRONTEND;
	}

	public void EnterLevelEditor(string strLevelFile = "")
	{
		if (!string.IsNullOrEmpty(strLevelFile) && !File.Exists(strLevelFile))
		{
			strLevelFile = string.Empty;
		}
		m_strCustomLevelFile = strLevelFile;
		m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__ENTER__START_LEVEL_EDITOR;
	}

	public void EndEditorLevel()
	{
		if (m_GlobalStartMode == GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR)
		{
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__START_EXIT;
		}
	}

	public void PreviewEditorLevel()
	{
		if (!m_PreviewEditorLevel)
		{
			m_PreviewEditorLevel = true;
			m_GlobalStartMode = GLOBALSTART_MODE.LEVEL_EDITOR__EXIT__START_EXIT;
		}
	}

	public void EndLevel(bool bShowResults)
	{
		if (m_DelayedHideLoadingScreenRoutine != null)
		{
			StopCoroutine(m_DelayedHideLoadingScreenRoutine);
		}
		m_bLockInWithLevelReturn = false;
		if (MatchingGames.GetInstance() != null && m_GameMode == GLOBALSTART_GAME_MODES.ONLINE)
		{
			MatchingGames.GetInstance().SaveGame();
		}
		Time.timeScale = 1f;
		m_OpenPauseMenu = null;
		m_bWaitForSettingsToCloseForInvite = false;
		T17DialogBoxManager.ReleaseAll();
		if (GlobalStart.EndLevelEvent != null)
		{
			GlobalStart.EndLevelEvent();
			GlobalStart.EndLevelEvent = null;
		}
		InGameMenuFlow instance = InGameMenuFlow.Instance;
		if (instance != null)
		{
			instance.CloseAllMenusOnAllPlayers();
			instance.gameObject.SetActive(value: false);
		}
		m_CustomLevel = false;
		m_CustomLevelData.Clear();
		if (m_PreviewEditorLevel)
		{
			bShowResults = false;
		}
		if (m_CountTimeInEditorRoutine != null)
		{
			StopCoroutine(m_CountTimeInEditorRoutine);
			m_CountTimeInEditorRoutine = null;
		}
		if (m_GlobalStartMode == GLOBALSTART_MODE.IN_LEVEL)
		{
			CutsceneManagerBase.GetInstance().StopAllCutscenes();
			if (!bShowResults)
			{
				m_bLockInWithLevelReturn = false;
				m_GlobalStartMode = GLOBALSTART_MODE.END_LEVEL;
			}
			else
			{
				ConfigManager instance2 = ConfigManager.GetInstance();
				if (null != instance2 && instance2.gameType == PrisonConfig.ConfigType.Cooperative && T17NetManager.IsMasterClient && T17NetManager.IsConnectedOnline() && T17NetRoomManager.IsInRoom())
				{
					T17NetRoomManager instance3 = T17NetRoomManager.Instance;
					if (null != instance3)
					{
						instance3.SetPropertiesForGameroomType(T17NetRoomGameView.GameRoomType.Private);
					}
				}
				LevelScript instance4 = LevelScript.GetInstance();
				if (instance4 != null)
				{
					string localized = string.Empty;
					PrisonData.LevelInfo levelInfo = instance4.m_LevelSetup.m_LevelInfo;
					if (levelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Normal)
					{
						Localization.Get(instance4.m_LevelSetup.m_EscapedActivityFeedKey, out var localized2);
						Localization.Get("Text.ActivityFeed.EscapedPrison.Con", out var localized3);
						Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Prison_Escape, localized2, localized3, (uint)levelInfo.m_PrisonEnum);
						localized = "Text.Presence.EscapedPrison" + (int)LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
					}
					else if (levelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Transport)
					{
						if (RoutineManager.GetInstance().IsTimedPrisonTimeUp())
						{
							localized = "Text.Presence.TimedOutPrison";
						}
						else
						{
							Localization.Get(instance4.m_LevelSetup.m_EscapedActivityFeedKey, out var localized4);
							Localization.Get("Text.ActivityFeed.EscapedTransport.Con", out var localized5);
							Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Transport_Escape, localized4, localized5, (uint)levelInfo.m_PrisonEnum);
							localized = "Text.Presence.EscapedTransport" + (int)LevelScript.GetCurrentLevelInfo().m_PrisonEnum;
						}
					}
					else if (levelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Tutorial)
					{
						Localization.Get(instance4.m_LevelSetup.m_EscapedActivityFeedKey, out var localized6);
						Localization.Get("Text.ActivityFeed.EscapedPrison.Con", out var localized7);
						Platform.GetInstance().PostToFeed(ACTIVITY_FEED_IDS.Activity_Feed_Prison_Escape, localized6, localized7, (uint)levelInfo.m_PrisonEnum);
						localized = "Text.Presence.EscapedTutorial";
					}
					Localization.Get(localized, out localized);
					localized = localized.Replace("%DAY%", (RoutineManager.GetInstance().GetDaysSinceInitialTime() + 1).ToString());
					Platform.GetInstance().SetPresence(localized);
				}
				m_bLockInWithLevelReturn = false;
				m_GlobalStartMode = GLOBALSTART_MODE.LOAD_RESULTS;
			}
		}
		Gamer.InvalidateOnlineIds(bNetView: true, bPlayerId: false);
		NetworkingPeer.m_bDiscardSerializeOnViews = false;
		CharacterSerializer instance5 = CharacterSerializer.GetInstance();
		if (instance5 != null)
		{
			instance5.DisableSerialization();
		}
		T17NetRoomGameView.OnRoomSignalEvent -= CharacterNetEvents.OnEvent;
		Platform.GetInstance().ExitingLevel();
		T17NetManager.SetTimePingIntervalToConnectingRate();
	}

	public void SetSelectedLevelToNetRoomCurrent()
	{
		int mapRotationIndex = NetBluePrintDetails.Instance.MapRotationIndex;
		if (mapRotationIndex >= 0)
		{
			SetSelectedLevelToPlaylistIndex(mapRotationIndex);
		}
	}

	public void SetSelectedLevelToPlaylistIndex(int index)
	{
		PlaylistData.NetPlaylistData playlist = NetBluePrintDetails.Instance.Playlist;
		if (!T17NetManager.IsMasterClient && (playlist == null || playlist.m_PrisonSetups == null))
		{
			StartCoroutine(SetSelectedLevelToPlayIndexWhenPlaylistIsSet(index));
		}
		else if (playlist != null && playlist.m_PrisonSetups != null)
		{
			PlaylistData.NetPrisonSetup currentSelectedLevel = playlist.m_PrisonSetups[index];
			SetCurrentSelectedLevel(currentSelectedLevel);
		}
	}

	private IEnumerator SetSelectedLevelToPlayIndexWhenPlaylistIsSet(int index)
	{
		while (!T17NetManager.IsMasterClient && (NetBluePrintDetails.Instance.Playlist == null || NetBluePrintDetails.Instance.Playlist.m_PrisonSetups == null))
		{
			yield return new WaitForEndOfFrame();
		}
		if (!T17NetManager.IsMasterClient)
		{
			SetSelectedLevelToPlaylistIndex(index);
		}
	}

	public void SetSelectedPlaylist(PlaylistData playlist, bool randomiseOrder = false)
	{
		m_CurrentPlaylistData = playlist;
		if (T17NetManager.IsMasterClient)
		{
			NetBluePrintDetails.Instance.MapRotationIndex = 0;
			PlaylistData.NetPlaylistData netPlaylistData = playlist.StripForNet();
			if (randomiseOrder && !m_Debug_NoRandomPlaylists)
			{
				netPlaylistData.m_PrisonSetups.Shuffle();
			}
			string text = "Playlist Order: ";
			for (int i = 0; i < netPlaylistData.m_PrisonSetups.Count; i++)
			{
				text = text + netPlaylistData.m_PrisonSetups[i].m_PrisonInfo.m_PrisonEnum.ToString() + ", ";
			}
			NetBluePrintDetails.Instance.Playlist = netPlaylistData;
			SetSelectedLevelToPlaylistIndex(0);
			LevelDataManager.GetInstance().NotifyOthersToSelectPlaylistRPC(playlist);
		}
	}

	private void SetCurrentSelectedLevel(PlaylistData.NetPrisonSetup setup)
	{
		m_CurrentPrisonSetup = setup;
		SetCurrentSelectedLevel(m_CurrentPrisonSetup.m_PrisonInfo.m_AssociatedFile, m_CurrentPrisonSetup.m_ConfigIndex, m_CurrentPrisonSetup.m_PrisonInfo.m_PrisonEnum);
	}

	public void SetCurrentSelectedLevel(string sceneName, int configID, LevelScript.PRISON_ENUM prisonEnum = LevelScript.PRISON_ENUM.Centre_Perks)
	{
		m_CurrentLevelSceneName = sceneName;
		m_CurrentLevelConfigID = configID;
		if (!(SaveManager.GetInstance() != null))
		{
			return;
		}
		if (prisonEnum == LevelScript.PRISON_ENUM.CustomPrison)
		{
			m_CustomLevel = true;
			if (!T17NetManager.IsMasterClient)
			{
				return;
			}
			Regex regex = new Regex("(ESC2UGC+.[^\\\\\\/]*)");
			Match match = regex.Match(sceneName);
			if (match.Groups.Count == 2)
			{
				SaveManager.GetInstance().SetCurrentPrison(match.Groups[0].Value, SaveManager.PrisonsSaveInformation.PrisonData.PrisonType.eUGCLevel);
				return;
			}
			regex = new Regex("(ESC2U+.[^\\\\\\/]*)");
			match = regex.Match(sceneName);
			if (match.Groups.Count == 2)
			{
				SaveManager.GetInstance().SetCurrentPrison(match.Groups[0].Value, SaveManager.PrisonsSaveInformation.PrisonData.PrisonType.eCustomLevel);
			}
		}
		else
		{
			m_CustomLevel = false;
			SaveManager.GetInstance().SetCurrentPrison(prisonEnum.ToString(), SaveManager.PrisonsSaveInformation.PrisonData.PrisonType.eDefault);
		}
	}

	public LevelScript.PRISON_ENUM GetCurrentSelectedPrisonEnum()
	{
		if (m_CurrentPrisonSetup == null)
		{
			return LevelScript.PRISON_ENUM.Centre_Perks;
		}
		return m_CurrentPrisonSetup.m_PrisonInfo.m_PrisonEnum;
	}

	public string GetCurrentSelectedLevel()
	{
		if (m_CurrentPrisonSetup == null)
		{
			return m_CurrentLevelSceneName;
		}
		return m_CurrentPrisonSetup.m_PrisonInfo.m_AssociatedFile;
	}

	public int GetCurrentSelectedConfigID()
	{
		if (m_CurrentPrisonSetup == null)
		{
			return m_CurrentLevelConfigID;
		}
		return m_CurrentPrisonSetup.m_ConfigIndex;
	}

	public PrisonConfig.ConfigType GetCurrentSelectedConfigType()
	{
		PrisonData currentSelectedPrisonData = GetCurrentSelectedPrisonData();
		if (currentSelectedPrisonData != null)
		{
			int currentSelectedConfigID = GetCurrentSelectedConfigID();
			if (currentSelectedConfigID >= 0 && currentSelectedConfigID < currentSelectedPrisonData.m_Configs.Count && currentSelectedPrisonData.m_Configs[currentSelectedConfigID] != null)
			{
				return currentSelectedPrisonData.m_Configs[currentSelectedConfigID].m_ConfigType;
			}
		}
		return PrisonConfig.ConfigType.Cooperative;
	}

	public PrisonData GetCurrentSelectedPrisonData()
	{
		PlaylistData.NetPrisonSetup currentPrisonConfig = NetBluePrintDetails.Instance.CurrentPrisonConfig;
		if (m_CurrentPlaylistData != null && currentPrisonConfig != null)
		{
			return m_CurrentPlaylistData.GetPrisonDataForLevelInfo(currentPrisonConfig.m_PrisonInfo);
		}
		return null;
	}

	public bool IsWithinLevel()
	{
		Gamer gamer = null;
		if (m_bLockInWithLevelReturn)
		{
			return true;
		}
		if (m_GlobalStartMode == GLOBALSTART_MODE.IN_LEVEL)
		{
			Gamer[] allGamers = Gamer.GetAllGamers();
			foreach (Gamer gamer2 in allGamers)
			{
				if (gamer2 != null && T17NetManager.MasterClientID == gamer2.m_PhotonID)
				{
					gamer = gamer2;
					break;
				}
			}
			bool flag = gamer != null && null != gamer.m_PlayerObject && -1 != gamer.m_NetViewID;
			if (flag)
			{
				m_bLockInWithLevelReturn = true;
			}
			return flag;
		}
		return false;
	}

	private void SomeMemoryHacks()
	{
		object[] array = new object[2048];
		for (int i = 0; i < 2048; i++)
		{
			array[i] = new byte[2048];
		}
		array = null;
		GC.Collect();
	}

	private void TrackMemoryHacks()
	{
	}

	public static void ToggleDebugText()
	{
		m_bShowDebugText = !m_bShowDebugText;
		m_bShowDebugFPSText = m_bShowDebugText;
		GetInstance().m_DebugText.gameObject.SetActive(m_bShowDebugText);
		GetInstance().m_DebugFPSText.gameObject.SetActive(m_bShowDebugFPSText);
		GetInstance().m_DebugVersionText.gameObject.SetActive(m_bShowDebugText);
		if (!m_bShowDebugText)
		{
			GetInstance().m_DebugGarbageCollectImage.gameObject.SetActive(m_bShowDebugText);
			GetInstance().m_DebugExceptionSinceBootImage.gameObject.SetActive(m_bShowDebugText);
			GetInstance().m_DebugExceptionImage.gameObject.SetActive(m_bShowDebugText);
		}
	}

	public void DARTTestMoveON()
	{
		UnityEngine.Debug.Log("  ******  DARTTestMoveON  ***");
		m_bDARTTest = true;
		Gamer.UpsertGamer(0, T17NetManager.PhotonPlayerID, -1, Platform.GetInstance().GetPrimaryUserName(), null, bPrimarySet: true, bPrimary: true);
		StartCoroutine(DeleteBootScene());
		m_GlobalStartMode = GLOBALSTART_MODE.START_LEVEL_LOAD;
	}

	private void InitialiseManagers()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int i = 0; i < allPlayers.Count; i++)
		{
			if (allPlayers[i] != null)
			{
				DoorManager.GetInstance().SetUpCharacterKeys(allPlayers[i]);
			}
		}
		if (ObjectiveManager.GetInstance() != null)
		{
			TimedNetworkService();
			ObjectiveManager.GetInstance().Init(allPlayers.Count);
		}
		if (QuestManager.GetInstance() != null)
		{
			TimedNetworkService();
			QuestManager.GetInstance().Begin();
		}
		TimedNetworkService();
	}

	public static int SetFixedTimeStep(int val, bool bJustRead)
	{
		if (!bJustRead)
		{
			fixedTimeStep = val;
			if (fixedTimeStep == 0)
			{
				Time.fixedDeltaTime = 1f / 60f;
			}
			else if (fixedTimeStep == 1)
			{
				Time.fixedDeltaTime = 1f / 30f;
			}
			else if (fixedTimeStep == 2)
			{
				Time.fixedDeltaTime = 1f / 15f;
			}
		}
		return fixedTimeStep;
	}

	public static int SetMaxDeltaTime(int val, bool bJustRead)
	{
		if (!bJustRead)
		{
			maxDeltaTime = val;
			if (maxDeltaTime == 0)
			{
				Time.maximumDeltaTime = 1f / 3f;
			}
			else if (maxDeltaTime == 1)
			{
				Time.maximumDeltaTime = 0.1f;
			}
		}
		return maxDeltaTime;
	}

	public static int SetSolverIterations(int val, bool bJustRead)
	{
		if (!bJustRead)
		{
			Physics.defaultSolverIterations = val;
		}
		return Physics.defaultSolverIterations;
	}

	public static void DisableCCD()
	{
		int num = 0;
		Rigidbody[] array = UnityEngine.Object.FindObjectsOfType<Rigidbody>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collisionDetectionMode != 0)
			{
				array[i].collisionDetectionMode = CollisionDetectionMode.Discrete;
				num++;
			}
		}
	}

	public void CheckForDropInPlayer()
	{
		if (m_FramesToSkipForControllerCheck <= 0)
		{
			ConfigManager instance = ConfigManager.GetInstance();
			if (m_bPostLevelLoad && !CutsceneManagerBase.IsACutscenePlaying() && ReInput.players != null && !T17DialogBoxManager.HasAnyOpenDialogs() && instance != null && (instance.gameType == PrisonConfig.ConfigType.Cooperative || (instance.gameType == PrisonConfig.ConfigType.Versus && !T17NetManager.IsConnectedOnline() && NetConnectAndJoinRoom.GetRequestedConnectionState() == NetConnectionState.OfflineMode)) && !IsPauseMenuOpen() && !Platform.GetInstance().IsAnyControllerConnectionEventsPending())
			{
				Rewired.Player player = Platform.GetInstance().CheckForAPress();
				if (player != null)
				{
					T17NetworkManager.GetInstance().RequestSplitscreenPlayer(player);
				}
			}
		}
		else
		{
			m_FramesToSkipForControllerCheck--;
		}
	}

	private void TempForTryingLoadCode()
	{
		SaveManager.GetInstance().NewGameSelected(prisonTutorial: false);
		SaveManager.GetInstance().SetSlotSelected(0);
	}

	private bool IsPauseMenuOpen()
	{
		List<Player> allPlayers = Player.GetAllPlayers();
		for (int num = allPlayers.Count - 1; num >= 0; num--)
		{
			if (allPlayers[num] != null && allPlayers[num].m_Gamer != null && allPlayers[num].m_Gamer.IsLocal())
			{
				CameraManager.PlayerBindingID playerCameraManagerBindingID = allPlayers[num].m_PlayerCameraManagerBindingID;
				InGameMenuFlow.PlayerIGMData data = null;
				if (InGameMenuFlow.Instance.GetCorrectIGMData(playerCameraManagerBindingID, out data) && data != null && data.m_PauseMenu != null && data.m_PauseMenu.gameObject.activeSelf)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void StartNameFiltering(FinishedFiltering filteringcallback)
	{
		float value = 1f;
		GlobalSave.GetInstance().Get("Settings:ProfanityFilter", out value, 1f);
		m_bProfanityFilterEnabled = value > 0.5f;
		List<Character> allCharacters = Character.GetAllCharacters();
		List<string> list = new List<string>();
		m_NameFilteringIndexCharacterIdMap = new List<int>(allCharacters.Count);
		for (int i = 0; i < allCharacters.Count; i++)
		{
			if (allCharacters[i].m_CharacterRole != CharacterRole.Crowd && allCharacters[i].m_CharacterRole != CharacterRole.Invisible && (allCharacters[i].m_CharacterCustomisation.m_Mode == CharacterCustomisation.Mode.Blueprint || (allCharacters[i].m_CharacterStats != null && allCharacters[i].m_CharacterStats.m_bIsPlayer)))
			{
				list.Add(allCharacters[i].m_CharacterCustomisation.m_RealName);
				m_NameFilteringIndexCharacterIdMap.Add(allCharacters[i].m_NetView.viewID);
			}
		}
		Customisation[] playerPresets = CustomisationManager.GetInstance().GetPlayerPresets();
		for (int j = 0; j < playerPresets.Length; j++)
		{
			list.Add(playerPresets[j].name);
		}
		m_FinishedFiltering = filteringcallback;
		Platform.GetInstance().FilterStringList(list, NameFilteringCallback);
	}

	public void NameFilteringCallback(bool bOK, List<string> names)
	{
		m_bProfanityFilteringFinished = true;
		Customisation[] playerPresets = CustomisationManager.GetInstance().GetPlayerPresets();
		if (bOK)
		{
			int count = m_NameFilteringIndexCharacterIdMap.Count;
			for (int i = 0; i < names.Count; i++)
			{
				if (i < count)
				{
					Character character = T17NetView.Find<Character>(m_NameFilteringIndexCharacterIdMap[i]);
					if (character != null && character.m_CharacterCustomisation.m_Mode == CharacterCustomisation.Mode.Blueprint)
					{
						Customisation nPCDetails = PrisonCustomisationManager.GetNPCDetails(character.m_CharacterCustomisation.m_BlueprintIdentifier);
						if (nPCDetails != null)
						{
							if (m_bProfanityFilterEnabled)
							{
								nPCDetails.filteredName = names[i];
							}
							else
							{
								nPCDetails.name = names[i];
								nPCDetails.filteredName = string.Empty;
							}
						}
					}
					if (character != null)
					{
						character.m_CharacterCustomisation.SetRealName(names[i]);
					}
				}
				else if (m_bProfanityFilterEnabled)
				{
					playerPresets[i - count].filteredName = names[i];
				}
				else
				{
					playerPresets[i - count].name = names[i];
					playerPresets[i - count].filteredName = string.Empty;
				}
			}
			m_bProfanityFilteringSuccessfullyCompleted = true;
		}
		else if (!T17NetManager.NetOfflineMode)
		{
			for (int j = 0; j < playerPresets.Length; j++)
			{
				playerPresets[j].name = playerPresets[j].safeName;
			}
		}
		m_FinishedFiltering();
	}

	private void FilteringDone()
	{
	}

	public void InviteReceived()
	{
		if (m_GlobalStartMode == GLOBALSTART_MODE.SHOW_FRONTEND)
		{
			m_GlobalStartMode = GLOBALSTART_MODE.CHECK_INVITES;
			FrontEndFlow instance = FrontEndFlow.Instance;
			if (!(instance != null) || !(instance.m_MainMenu != null))
			{
				return;
			}
			BaseMenuBehaviour currentOpenMenu = instance.m_MainMenu.GetCurrentOpenMenu();
			if (currentOpenMenu != null)
			{
				SettingsFrontendMenu settingsFrontendMenu = currentOpenMenu as SettingsFrontendMenu;
				if (settingsFrontendMenu != null)
				{
					settingsFrontendMenu.CallCancel();
				}
			}
		}
		else if (m_GlobalStartMode == GLOBALSTART_MODE.IN_LEVEL)
		{
			if (T17DialogBoxManager.HasAnyOpenDialogs())
			{
				m_bWaitForSettingsToCloseForInvite = true;
				m_OpenPauseMenu = null;
			}
			else
			{
				PauseMenu openPauseMenuInstance = PauseMenu.GetOpenPauseMenuInstance();
				if (openPauseMenuInstance != null)
				{
					if (openPauseMenuInstance.IsSettingsMenuOpen())
					{
						openPauseMenuInstance.OnSettingsClose();
						m_bWaitForSettingsToCloseForInvite = true;
						m_OpenPauseMenu = openPauseMenuInstance;
					}
					if (openPauseMenuInstance.IsSaveSlotMenuOpen())
					{
						openPauseMenuInstance.OnSaveSlotsClose();
						m_bWaitForSettingsToCloseForInvite = true;
						m_OpenPauseMenu = openPauseMenuInstance;
					}
				}
				m_PreviewEditorLevel = false;
			}
			if (!m_bWaitForSettingsToCloseForInvite)
			{
				DisconnectAndEndLevel();
			}
		}
		else if (m_GlobalStartMode == GLOBALSTART_MODE.CREDITS)
		{
			m_inviteAcceptedDuringCredits = true;
			m_GlobalStartMode = GLOBALSTART_MODE.HIDE_CREDITS_BACK_TO_FRONTEND;
		}
		else if (m_GlobalStartMode == GLOBALSTART_MODE.LEVEL_EDITOR__IN_EDITOR)
		{
			m_bWaitingForEditorInviteResponse = true;
			LevelEditor_UIController instance2 = LevelEditor_UIController.GetInstance();
			if (instance2 != null)
			{
				instance2.InviteRecieved();
			}
		}
	}

	public void CancelOnlineGameLoad()
	{
		Platform.GetInstance().CancelFilteringStringList();
		NetConnectAndJoinRoom.RequestConnectionState(NetConnectionState.OfflineMode, silentErrorDialogMode: true);
	}

	public void OnLoadDisconnected()
	{
		UnityEngine.Debug.Log("Loading has been disconnected");
		CancelOnlineGameLoad();
	}

	public void OnLoadTimedOut(PlayerLoadState state)
	{
		UnityEngine.Debug.Log("Loading has been timed out in state " + state);
		UnityEngine.Debug.Log("**DART** Loading has been timed out in state " + state);
		CancelOnlineGameLoad();
	}

	public PlaylistData GetCurrentPlaylistData()
	{
		return m_CurrentPlaylistData;
	}

	public void BroadcastCustomLevelData()
	{
		if (m_CustomLevel && T17NetManager.IsMasterClient && m_CustomLevelData.Count > 0)
		{
			T17NetRoomGameView.Instance.SignalToRoomEvent(T17NetConfig.NetEventTypes.CustomLevelData, T17NetConfig.NetSequenceChannel.UserLevel, m_CustomLevelData.ToArray(), viaServer: true, bCached: true);
			PhotonNetwork.networkingPeer.SendOutgoingCommands();
		}
	}

	private IEnumerator CountTimeInCustomPrisonRoutine()
	{
		while (true)
		{
			yield return new WaitForSecondsRealtime(240f);
			EC2AnalyticsHelper.UpdatePlaytimeRecord(4, "Analytics_MinutesInCustomPrison", "Any Custom Prison playtime", "Custom Prison playtime ");
		}
	}

	public bool IsLoadingFlowActive()
	{
		LoadingFlow loadingFlow = (LoadingFlow)m_RegisteredFlows[BaseFlowBehaviour.FlowType.Loading];
		if (loadingFlow != null)
		{
			return loadingFlow.m_LoadingCanvas.gameObject.activeSelf;
		}
		return false;
	}

	public static void ResetTimedNetworkService()
	{
		if (!m_bTimedNetworkServiceStopWatchCreated)
		{
			m_TimedNetworkServiceStopWatch = new Stopwatch();
			m_bTimedNetworkServiceStopWatchCreated = true;
		}
		m_TimedNetworkServiceStopWatch.Reset();
		m_TimedNetworkServiceStopWatch.Start();
	}

	public static void TimedNetworkService()
	{
		if (!m_bTimedNetworkServiceStopWatchCreated)
		{
			m_TimedNetworkServiceStopWatch = new Stopwatch();
			m_TimedNetworkServiceStopWatch.Reset();
			m_TimedNetworkServiceStopWatch.Start();
			m_bTimedNetworkServiceStopWatchCreated = true;
		}
		else if (!m_TimedNetworkServiceStopWatch.IsRunning)
		{
			m_TimedNetworkServiceStopWatch.Reset();
			m_TimedNetworkServiceStopWatch.Start();
		}
		else if (m_TimedNetworkServiceStopWatch.ElapsedMilliseconds > 40)
		{
			T17NetManager.Service();
			m_TimedNetworkServiceStopWatch.Reset();
			m_TimedNetworkServiceStopWatch.Start();
		}
	}

	public static bool Debug_NoRandomPlaylists(bool bEnable, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_Debug_NoRandomPlaylists = bEnable;
		}
		return m_Debug_NoRandomPlaylists;
	}

	public static void DEBUG_IncrementMapRotationIndex()
	{
		NetBluePrintDetails.Instance.NextPrison();
	}
}
