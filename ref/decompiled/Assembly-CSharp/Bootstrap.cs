using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExitGames.Client.Photon;
using GameAnalytics;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
	public class CommandLineOptions
	{
		[Commando.Name("ActiveLogGroup")]
		public readonly List<DebugHelpers.LogGroup> ActiveLogGroups = new List<DebugHelpers.LogGroup>();

		[Commando.Name("ActiveLogNetGroup")]
		public readonly List<DebugHelpers.LogNetGroup> ActiveLogNetGroups = new List<DebugHelpers.LogNetGroup>();

		[Commando.Name("ActivePrefix")]
		public readonly List<DebugHelpers.Prefix> ActivePrefixes = new List<DebugHelpers.Prefix>();

		[Commando.Name("AnalyticTracker")]
		public readonly List<NetAnalytics.Tracker> AnalyticsTrackers = new List<NetAnalytics.Tracker>();

		[Commando.Name("AnalyticEvent")]
		public readonly List<NetAnalytics.EventCategory> AnalyticsEvents = new List<NetAnalytics.EventCategory>();

		public readonly string NetLoginID;

		public readonly int NetDebugResendCount = 10;

		public readonly PhotonLogLevel NetLogLevel;

		public readonly bool TrafficStatsCaptureDisabled;

		public readonly bool SkipBootFlow;

		public readonly bool UsingPatchables = true;

		public readonly string InviteRoomName;

		public readonly bool NetLagSimulationEnabled;

		public readonly bool NetLagSimulationGuiEnabled;

		public readonly float NetLagSimulationLag = 100f;

		public readonly float NetLagSimulationJitter;

		public readonly float NetLagSimulationLoss = 1f;

		public readonly bool NetTrafficEnabled;

		public readonly bool NetTrafficGuiEnabled;

		public readonly bool NetTrafficShowTraffic;

		public readonly bool NetTrafficShowHealth;

		public readonly bool NetConfigRpcTTY;

		public readonly bool NetDebugGuiPanel;

		public readonly bool NetDrawPlayerPos;

		public readonly string TestScript = string.Empty;

		public readonly int Quit;

		public readonly int QuitExitCode;

		public readonly string LoadLevel = string.Empty;

		public readonly int LoadLevelConfigID;

		public readonly string RequestConnectionState = string.Empty;

		public readonly string VersionOverride = string.Empty;

		public readonly bool UnlockAllLevels;
	}

	private static Bootstrap m_instance = null;

	public static readonly PhotonTestEvents m_PhotonEvents = new PhotonTestEvents();

	public static readonly CommandLineOptions CmdLineOptions = new CommandLineOptions();

	private const string COMMANDO_FILE_PREFIX = "-GoCommando:";

	private const string COMMANDO_FILE_COMMENT_PREFIX = "#";

	private const string COMMANDO_ARGS_PREFIX = "-Commando:";

	private const char COMMANDO_ARGS_SEPARATOR = ';';

	private static bool m_sHasProcessedCommandLine = false;

	private static float m_QuitTimer = 0f;

	private static int m_QuitTimerExitCode = 0;

	private static string m_LoadLevel = string.Empty;

	private static int m_LoadLevelConfigID = 0;

	public static NetConnectionState m_StartupConnectionState = NetConnectionState.OfflineMode;

	private static GameObject TestScriptGO;

	private const string m_netPersistentScriptsPreFabName = "Prefabs/Network/PersistentScripts";

	private static GameObject m_netGlobalRoomViewGO = null;

	private const string m_netGlobalRoomViewPreFabName = "Prefabs/Network/NetGlobalRoomGameView";

	private static bool m_applicationHasFocus = true;

	private GameObject m_PersistentScriptsGO;

	public static Bootstrap Instance => m_instance;

	public static bool ApplicationHasFocus
	{
		get
		{
			return m_applicationHasFocus;
		}
		private set
		{
			m_applicationHasFocus = value;
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_instance == this)
		{
			m_instance = null;
			if (m_netGlobalRoomViewGO != null)
			{
				UnityEngine.Object.Destroy(m_netGlobalRoomViewGO);
				m_netGlobalRoomViewGO = null;
			}
		}
	}

	private void ProcessCommandLine()
	{
		if (m_sHasProcessedCommandLine)
		{
			return;
		}
		m_sHasProcessedCommandLine = true;
		string[] commandLineArgs = GetCommandLineArgs();
		if (commandLineArgs == null)
		{
			return;
		}
		string text = string.Join(" ", commandLineArgs);
		Debug.LogFormat("Bootstrap.ProcessCommandLine - Processing command line \"{0}\"", text);
		Debug.Log(" *1* command line " + text);
		string[] array = null;
		if (text.Contains("-GoCommando:"))
		{
			string empty = string.Empty;
			try
			{
				empty = commandLineArgs.Where((string row) => row.Contains("-GoCommando:")).Single();
				empty = empty.Replace("-GoCommando:", string.Empty);
				Debug.LogFormat("Bootstrap.ProcessCommandLine - Found GoCommando directive \"{0}\"", empty);
				array = ReadCommandoArgsFromFile($"{Application.dataPath}/Resources/GoCommando/{empty}");
			}
			catch (Exception)
			{
			}
		}
		else if (text.Contains("-Commando:"))
		{
			array = ReadCommandoArgsFromCommandLine(commandLineArgs);
		}
		if (array != null)
		{
			Debug.Log(" *2* command line " + array);
			Commando commando = new Commando(CmdLineOptions, "-Commando:", "-GoCommando:");
			if (commando.ParseCommandLine(array))
			{
				ApplyCommandLineOptions();
			}
			else
			{
				Application.Quit();
			}
		}
	}

	private string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	private string[] ReadCommandoArgsFromCommandLine(string[] args)
	{
		string[] result = null;
		string text = string.Empty;
		try
		{
			text = args.Where((string row) => row.Contains("-Commando:")).Single();
		}
		catch (Exception)
		{
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Replace("-Commando:", string.Empty);
			Debug.LogFormat("Bootstrap.ReadCommandoArgsFromCommandLine - Found Commando args \"{0}\"", text);
			result = text.Split(';');
		}
		return result;
	}

	private string[] ReadCommandoArgsFromFile(string fullPath)
	{
		string[] result = null;
		if (File.Exists(fullPath))
		{
			List<string> list = new List<string>();
			using (StreamReader streamReader = File.OpenText(fullPath))
			{
				string text = streamReader.ReadLine();
				while (!string.IsNullOrEmpty(text))
				{
					if (!text.StartsWith("#"))
					{
						list.Add(text);
					}
					text = streamReader.ReadLine();
				}
			}
			result = list.ToArray();
		}
		else
		{
			Debug.LogErrorFormat("Bootstrap.ProcessCommandLine - Cannot find file \"{0}\"", fullPath);
		}
		return result;
	}

	private void ApplyCommandLineOptions()
	{
		Debug.Log("   *******  ApplyCommandLineOptions");
		if (CmdLineOptions.ActiveLogGroups.Count > 0)
		{
			DebugHelpers.ClearActiveLogGroups();
			foreach (DebugHelpers.LogGroup activeLogGroup in CmdLineOptions.ActiveLogGroups)
			{
				DebugHelpers.LogGroupActive(activeLogGroup, active: true);
			}
		}
		if (CmdLineOptions.ActiveLogNetGroups.Count > 0)
		{
			DebugHelpers.ClearActiveLogNetGroups();
			foreach (DebugHelpers.LogNetGroup activeLogNetGroup in CmdLineOptions.ActiveLogNetGroups)
			{
				DebugHelpers.LogGroupActive(activeLogNetGroup, active: true);
			}
		}
		if (CmdLineOptions.AnalyticsTrackers.Count > 0)
		{
			GAHelpers.ActiveTrackerHelper.ClearActiveTrackers();
			foreach (NetAnalytics.Tracker analyticsTracker in CmdLineOptions.AnalyticsTrackers)
			{
				GAHelpers.TrackerActive(analyticsTracker, active: true);
			}
		}
		if (CmdLineOptions.AnalyticsEvents.Count > 0)
		{
			GAHelpers.ActiveCategoriesHelper.ClearActiveCategories();
			foreach (NetAnalytics.EventCategory analyticsEvent in CmdLineOptions.AnalyticsEvents)
			{
				GAHelpers.CategoryActive(analyticsEvent, active: true);
			}
		}
		if (CmdLineOptions.ActivePrefixes.Count > 0)
		{
			DebugHelpers.ClearActivePrefixes();
			foreach (DebugHelpers.Prefix activePrefix in CmdLineOptions.ActivePrefixes)
			{
				DebugHelpers.PrefixActive(activePrefix, active: true);
			}
		}
		PhotonPeer networkingPeer = PhotonNetwork.networkingPeer;
		if (networkingPeer != null)
		{
			T17NetPhotonLagSimulationGui.Instance.NetLagSimulationGuiOn = CmdLineOptions.NetLagSimulationGuiEnabled;
			networkingPeer.IsSimulationEnabled = CmdLineOptions.NetLagSimulationEnabled;
			float num = Mathf.Clamp(CmdLineOptions.NetLagSimulationLag, 0f, 500f);
			networkingPeer.NetworkSimulationSettings.IncomingLag = (int)num;
			networkingPeer.NetworkSimulationSettings.OutgoingLag = (int)num;
			float num2 = Mathf.Clamp(CmdLineOptions.NetLagSimulationJitter, 0f, 100f);
			networkingPeer.NetworkSimulationSettings.IncomingJitter = (int)num2;
			networkingPeer.NetworkSimulationSettings.OutgoingJitter = (int)num2;
			float num3 = Mathf.Clamp(CmdLineOptions.NetLagSimulationLoss, 0f, 10f);
			networkingPeer.NetworkSimulationSettings.IncomingLossPercentage = (int)num3;
			networkingPeer.NetworkSimulationSettings.OutgoingLossPercentage = (int)num3;
		}
		T17NetConfig.NetPhotonRpcTTY = CmdLineOptions.NetConfigRpcTTY;
		T17NetConfig.NetDebugGuiPanel = CmdLineOptions.NetDebugGuiPanel;
		if (CmdLineOptions.NetDrawPlayerPos)
		{
			DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetworkPlayerPos, active: true);
		}
		if (!string.IsNullOrEmpty(CmdLineOptions.TestScript))
		{
			TestScript testScript = TestScript.Instance;
			if (testScript == null)
			{
				TestScriptGO = new GameObject("TestScript");
				if (TestScriptGO != null)
				{
					testScript = TestScriptGO.AddComponent<TestScript>();
					UnityEngine.Object.DontDestroyOnLoad(TestScriptGO);
				}
			}
			if (testScript != null)
			{
				StartCoroutine(PerformTestScriptAfterDelay(5f));
			}
		}
		if (CmdLineOptions.Quit > 0)
		{
			m_QuitTimer = CmdLineOptions.Quit;
			Debug.Log("   *******  ApplyCommandLineOptions      QUIT  " + m_QuitTimer);
		}
		if (CmdLineOptions.QuitExitCode > 0)
		{
			m_QuitTimerExitCode = CmdLineOptions.QuitExitCode;
			Debug.Log("   *******  ApplyCommandLineOptions      QuitExitCode  " + m_QuitTimerExitCode);
		}
		if (CmdLineOptions.LoadLevel != string.Empty)
		{
			m_LoadLevel = CmdLineOptions.LoadLevel;
			Debug.Log("   *******  ApplyCommandLineOptions      LoadLevel  " + m_LoadLevel);
		}
		if (CmdLineOptions.LoadLevelConfigID >= 0)
		{
			m_LoadLevelConfigID = CmdLineOptions.LoadLevelConfigID;
			Debug.Log("   *******  ApplyCommandLineOptions      LoadLevelConfigID  " + m_LoadLevel);
		}
		if (!string.IsNullOrEmpty(CmdLineOptions.RequestConnectionState))
		{
			object obj = Enum.Parse(typeof(NetConnectionState), CmdLineOptions.RequestConnectionState, ignoreCase: true);
			if (obj != null)
			{
				m_StartupConnectionState = (NetConnectionState)obj;
			}
		}
		if (!string.IsNullOrEmpty(CmdLineOptions.VersionOverride))
		{
			GlobalStart.m_VersionString = CmdLineOptions.VersionOverride;
		}
	}

	private IEnumerator PerformTestScriptAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		PerformTestScript(CmdLineOptions.TestScript);
	}

	internal static void PerformTestScript(string scriptName)
	{
		TestScript.Instance.Execute(scriptName, TestScriptCompleteCallback);
	}

	private static void TestScriptCompleteCallback(TestScript.ResultCode result)
	{
		if (result == TestScript.ResultCode.Success)
		{
			Debug.Log("Bootstrap.TestScriptCompleteCallback - SUCCESS.");
			return;
		}
		Debug.LogErrorFormat("Bootstrap.TestScriptCompleteCallback - FAILED, returnValue = {0}", result.ToString());
	}

	public void OnApplicationFocus(bool focus)
	{
		ApplicationHasFocus = focus;
	}

	private void Awake()
	{
		if (m_instance != null)
		{
			Debug.LogError("More than one Bootstrap instance has been created, it expects to be a singleton.", this);
			return;
		}
		m_instance = this;
		m_PersistentScriptsGO = GameObject.Find("NetPersistentScripts");
		if (m_PersistentScriptsGO == null)
		{
			GameObject gameObject = Resources.Load("Prefabs/Network/PersistentScripts") as GameObject;
			if (gameObject != null)
			{
				m_PersistentScriptsGO = UnityEngine.Object.Instantiate(gameObject);
				if (m_PersistentScriptsGO != null)
				{
					m_PersistentScriptsGO.SetNetViewID(T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.PersistentScripts));
					m_PersistentScriptsGO.name = "NetPersistentScripts";
					UnityEngine.Object.DontDestroyOnLoad(m_PersistentScriptsGO);
				}
				else
				{
					Debug.LogErrorFormat("Bootstrap.Awake - Failed to Instantiate Prefab {0}", "Prefabs/Network/PersistentScripts");
				}
			}
			else
			{
				Debug.LogErrorFormat("Bootstrap.Awake - Failed to load Prefab {0}", "Prefabs/Network/PersistentScripts");
			}
		}
		else
		{
			Debug.LogErrorFormat("Bootstrap.Awake - Already Exists -- NetPersistentScripts");
		}
		if (m_netGlobalRoomViewGO == null)
		{
			GameObject gameObject2 = Resources.Load("Prefabs/Network/NetGlobalRoomGameView") as GameObject;
			if (gameObject2 != null)
			{
				m_netGlobalRoomViewGO = UnityEngine.Object.Instantiate(gameObject2);
				if (m_netGlobalRoomViewGO != null)
				{
					m_netGlobalRoomViewGO.SetNetViewID(T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.GlobalRoomView));
					m_netGlobalRoomViewGO.name = "NetGlobalRoomView";
					UnityEngine.Object.DontDestroyOnLoad(m_netGlobalRoomViewGO);
				}
				else
				{
					Debug.LogErrorFormat("Bootstrap.Awake - failed to instantiate prefab ({0}) for m_netGlobalRoomViewGO", "Prefabs/Network/NetGlobalRoomGameView");
				}
			}
			else
			{
				Debug.LogErrorFormat("Bootstrap.Awake - failed to load prefab ({0}) for m_netGlobalRoomViewGO", "Prefabs/Network/NetGlobalRoomGameView");
			}
		}
		ProcessCommandLine();
	}

	private void Start()
	{
	}

	public static void Quit(int exitCode)
	{
		Environment.ExitCode = exitCode;
		Debug.LogFormat("BootStrap.Update - UNITY_STANDALONE_WIN -- ExitCode={0}", Environment.ExitCode);
		Debug.LogFormat("BootStrap.Update - UNITY_STANDALONE_WIN -- Calling Application.Quit");
		Application.Quit();
		Debug.LogFormat("BootStrap.Update - UNITY_STANDALONE_WIN -- After Application.Quit");
	}

	private void Update()
	{
		if (m_QuitTimer > 0f)
		{
			m_QuitTimer -= UpdateManager.deltaTime;
			if (m_QuitTimer <= 0f)
			{
				Quit(m_QuitTimerExitCode);
			}
		}
	}

	public static void RequestStartupConnectionState()
	{
		NetConnectAndJoinRoom.RequestConnectionState(m_StartupConnectionState);
	}

	public void CheckAndExecStartUpCommand()
	{
		Debug.Log("********  CheckAndExecStartUpCommand   ***** ");
		if (m_LoadLevel != string.Empty && m_LoadLevelConfigID >= 0)
		{
			GlobalStart.GetInstance().m_DebugForceLoadLevel = m_LoadLevel;
			GlobalStart.GetInstance().StartLevelLoad(m_LoadLevel, m_LoadLevelConfigID);
			GlobalStart.GetInstance().DARTTestMoveON();
		}
	}

	public void InToLevel()
	{
	}
}
