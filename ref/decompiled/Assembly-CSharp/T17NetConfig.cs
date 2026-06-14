using UnityEngine;

public class T17NetConfig
{
	public enum NetPlatformType
	{
		Undefined,
		PS4,
		XB1,
		Switch,
		Win,
		Linux,
		OSX,
		SteamWin,
		SteamLinux,
		SteamOSX,
		Steam,
		GOG,
		Origin,
		DesktopCrossplay,
		EpicGames
	}

	public enum NetEventTypes : byte
	{
		INVALID,
		Load_NotStarted,
		Load_LevelLoad_Done,
		Load_LevelInit_InProgress,
		Load_LevelInit_Done,
		Load_Inventory_InProgress,
		Load_Inventory_Done,
		Load_Spawn_InProgress,
		Load_Spawn_Done,
		Load_Managers_InProgress,
		Load_ReadyToPlay,
		GamerInfo,
		LobbyCountdownStarted,
		LobbyCountdownStopped,
		LobbyCountdownSync,
		StartLoading,
		ReturnToLobby,
		NPCManager,
		CharacterSerialization,
		InteractiveObjectEvent,
		CombatEvent,
		Ping,
		CharacterEvent,
		CustomLevelData
	}

	public enum NetSequenceChannel : byte
	{
		CombatAndInteractoins,
		CharacterSerialization0,
		CharacterSerialization1,
		CharacterSerialization2,
		CharacterSerialization3,
		CharacterSerialization4,
		CharacterSerialization5,
		CharacterSerialization6,
		CharacterSerialization7,
		CharacterSerialization8,
		CharacterSerialization9,
		CharacterSerialization10,
		CharacterSerialization11,
		CharacterSerialization12,
		CharacterSerialization13,
		CharacterSerialization14,
		CharacterSerialization15,
		Ping,
		UserLevel,
		COUNT
	}

	public enum ReservedNetID
	{
		T17NetworkManager = 1,
		LevelManager = 2,
		RoomManager = 3,
		JobsManager = 4,
		GlobalRoomView = 5,
		PersistentScripts = 6,
		MagicAIRepairWallItem = 7,
		MagicAIDestroyWallItem = 8,
		MagicAIRepairGroundItem = 9,
		MagicAIDestroyGroundItem = 10,
		MagicAIDestroyVentGroundItem = 11,
		HUDParentView = 12,
		CutsceneManager = 13,
		CrossplayLobbyManager = 14,
		PlayerStart = 26,
		PlayerEnd = 29,
		JobInstanceEnd = 30,
		JobInstanceStart = 40,
		ItemMgrEnd = 41,
		ItemMgrStart = 961,
		CarriedObjectDispenserEnd = 962,
		CarriedObjectDispenserStart = 1012,
		COUNT = 1013
	}

	public const int CharacterSerialization_TOTAL = 7;

	public const int MaxGameItemsAllowed = 920;

	public const int MaxGameDispenserCarriedObjects = 50;

	public const int MaxJobsPerLevel = 10;

	public static bool FeatureCampaignMatchmaking;

	public const string OFFLINE_ROOM_NAME = "offline room";

	private static bool m_networkDebugGuiPanel;

	private static bool m_pressedOnlineMenuButton;

	private static bool m_netForce_JoinRoom_Error;

	private static bool m_netForce_CoRoutineVisibility_Error;

	private static bool m_netForce_IgnoreQuestions;

	private static bool m_netPhotonRpcTTY;

	private static bool m_serverPatchingControlOverride;

	private static bool m_serverPatchingEnabled;

	private static bool m_localPatchingControlOverride;

	private static bool m_localPatchingEnabled;

	public static bool NetForceLoadHang_LevelInit;

	public static bool NetForceLoadHang_Spawn;

	public static bool NetForceLoadHang_Inventory;

	public static bool NetForceLoadHang_Managers;

	public static string MatchingVersionString => Platform.GetInstance().GetMatchmakingVersionNumber();

	public static bool ServerPatchingMode
	{
		get
		{
			if (m_serverPatchingControlOverride && m_serverPatchingEnabled)
			{
				return true;
			}
			return false;
		}
	}

	public static bool LocalPatchingMode
	{
		get
		{
			if (m_localPatchingControlOverride && m_localPatchingEnabled)
			{
				return true;
			}
			return false;
		}
	}

	public static bool PatchingMode => LocalPatchingMode || ServerPatchingMode;

	public static bool PressedOnlineMenuButton
	{
		get
		{
			return m_pressedOnlineMenuButton;
		}
		set
		{
			m_pressedOnlineMenuButton = value;
		}
	}

	public static bool NetForceCreateJoinError
	{
		get
		{
			return m_netForce_JoinRoom_Error;
		}
		set
		{
			m_netForce_JoinRoom_Error = value;
		}
	}

	public static bool NetForceQuestionIgnore
	{
		get
		{
			return m_netForce_IgnoreQuestions;
		}
		set
		{
			m_netForce_IgnoreQuestions = value;
		}
	}

	public static bool NetForceCoRoutineVisibiltyError
	{
		get
		{
			return m_netForce_CoRoutineVisibility_Error;
		}
		set
		{
			m_netForce_CoRoutineVisibility_Error = value;
		}
	}

	public static bool NetPhotonRpcTTY
	{
		get
		{
			return m_netPhotonRpcTTY;
		}
		set
		{
			m_netPhotonRpcTTY = value;
			PhotonNetwork.PhotonRpcTTY = value;
			DebugHelpers.LogGroupActive(DebugHelpers.LogNetGroup.NetPhotonRpc, value);
		}
	}

	public static bool NetDebugGuiPanel
	{
		get
		{
			return m_networkDebugGuiPanel;
		}
		set
		{
			m_networkDebugGuiPanel = value;
		}
	}

	public static bool PauseSerializeView
	{
		get
		{
			return NetworkingPeer.m_bDiscardSerializeOnViews;
		}
		set
		{
			NetworkingPeer.m_bDiscardSerializeOnViews = value;
			CharacterSerializer instance = CharacterSerializer.GetInstance();
			if (instance != null)
			{
				if (value)
				{
					instance.DisableSerialization();
				}
				else
				{
					instance.EnableSerialization();
				}
			}
		}
	}

	public static int GetReservedNetID(ReservedNetID reservedNetID)
	{
		return GetReservedNetID((int)reservedNetID);
	}

	public static int GetReservedNetID(int reservedNetID)
	{
		return PhotonNetwork.MAX_VIEW_IDS - reservedNetID;
	}

	public static int GetPlayerMaxViewIds()
	{
		return PhotonNetwork.MAX_VIEW_IDS;
	}

	public static string GetPlatformType()
	{
		return NetPlatformType.DesktopCrossplay.ToString();
	}

	public static void SetServerPatchingControl(bool patching)
	{
		m_serverPatchingControlOverride = true;
		m_serverPatchingEnabled = patching;
		Debug.LogWarningFormat("SetServerPatchingControl: SERVER - Patching Control = {0}", m_serverPatchingEnabled);
	}

	public static void SetLocalPatchingControl(bool patching)
	{
		m_localPatchingControlOverride = true;
		m_localPatchingEnabled = patching;
		Debug.LogWarningFormat("SetLocalPatchingControl: LOCAL - Patching Control = {0}", m_localPatchingEnabled);
	}

	public static bool NetDebugGui(bool bEnable, bool bJustRead)
	{
		if (!bJustRead)
		{
			NetDebugGuiPanel = bEnable;
		}
		return NetDebugGuiPanel;
	}
}
