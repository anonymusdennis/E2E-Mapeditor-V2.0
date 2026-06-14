using System.Collections.Generic;

public class NetLoadSync
{
	private static int m_PreLoadNumPlayers = 0;

	private static List<PlayerLoadState> m_LevelLoadedStates = new List<PlayerLoadState>();

	private static bool m_LevelLoadedStatesInit = false;

	private static List<PlayerLoadState> m_LevelInitStates = new List<PlayerLoadState>();

	private static bool m_LevelInitStatesInit = false;

	private static List<PlayerLoadState> m_InventoryStates = new List<PlayerLoadState>();

	private static bool m_InventoryStatesInit = false;

	private static List<PlayerLoadState> m_SpawnedStates = new List<PlayerLoadState>();

	private static bool m_SpawnedStatesInit = false;

	private static List<PlayerLoadState> m_ManagerStates = new List<PlayerLoadState>();

	private static bool m_ManagerStatesInit = false;

	public static void RecordPreLoadRoomVars(int numPlayers)
	{
		m_PreLoadNumPlayers = numPlayers;
	}

	public static bool HaveAllPlayersLeftSinceLoad()
	{
		if (m_PreLoadNumPlayers > 1 && Gamer.GetNumRemoteGamers() == 0)
		{
			return true;
		}
		return false;
	}

	public static void StartLevelLoad()
	{
		T17NetLoadSync.Instance.StartLevelLoad();
	}

	public static void StopLevelLoad()
	{
		T17NetLoadSync.Instance.StopLevelLoad();
	}

	public static bool HasTimedOut()
	{
		return T17NetLoadSync.TimedOut;
	}

	public static bool HasDisconnected()
	{
		return T17NetLoadSync.m_bDisconnected || !T17NetManager.ConnectedAndReady;
	}

	public static void EventSend(T17NetConfig.NetEventTypes eventType)
	{
		T17NetLoadSync.Instance.EventSend(eventType);
	}

	public static string GetLoadStatesDescription()
	{
		return string.Concat("Time out occured in ", T17NetLoadSync.Instance.TimedOutState, "\n", T17NetLoadSync.Instance.NetPlayerStateMapping);
	}

	public static PlayerLoadState GetLoadStateForPhotonId(int iPhotonId)
	{
		return T17NetLoadSync.Instance.GetLoadStateForPhotonId(iPhotonId);
	}

	public static bool AllClientsLevelLoaded()
	{
		if (!m_LevelLoadedStatesInit)
		{
			m_LevelLoadedStates.Add(PlayerLoadState.LevelLoad_Done);
			m_LevelLoadedStates.Add(PlayerLoadState.LevelInit_InProgress);
			m_LevelLoadedStates.Add(PlayerLoadState.LevelInit_Done);
			m_LevelLoadedStates.Add(PlayerLoadState.Inventory_InProgress);
			m_LevelLoadedStates.Add(PlayerLoadState.Inventory_Done);
			m_LevelLoadedStates.Add(PlayerLoadState.Spawn_InProgress);
			m_LevelLoadedStates.Add(PlayerLoadState.Spawn_Done);
			m_LevelLoadedStates.Add(PlayerLoadState.Managers_InProgress);
			m_LevelLoadedStates.Add(PlayerLoadState.Success);
			m_LevelLoadedStatesInit = true;
		}
		return T17NetLoadSync.Instance.AreAllClientsInState(m_LevelLoadedStates);
	}

	public static bool AllClientsLevelInit()
	{
		if (!m_LevelInitStatesInit)
		{
			m_LevelInitStates.Add(PlayerLoadState.LevelInit_Done);
			m_LevelInitStates.Add(PlayerLoadState.Inventory_InProgress);
			m_LevelInitStates.Add(PlayerLoadState.Inventory_Done);
			m_LevelInitStates.Add(PlayerLoadState.Spawn_InProgress);
			m_LevelInitStates.Add(PlayerLoadState.Spawn_Done);
			m_LevelInitStates.Add(PlayerLoadState.Managers_InProgress);
			m_LevelInitStates.Add(PlayerLoadState.Success);
			m_LevelInitStatesInit = true;
		}
		return T17NetLoadSync.Instance.AreAllClientsInState(m_LevelInitStates);
	}

	public static bool AllClientsInventoryInit()
	{
		if (!m_InventoryStatesInit)
		{
			m_InventoryStates.Add(PlayerLoadState.Inventory_Done);
			m_InventoryStates.Add(PlayerLoadState.Spawn_InProgress);
			m_InventoryStates.Add(PlayerLoadState.Spawn_Done);
			m_InventoryStates.Add(PlayerLoadState.Managers_InProgress);
			m_InventoryStates.Add(PlayerLoadState.Success);
			m_InventoryStatesInit = true;
		}
		return T17NetLoadSync.Instance.AreAllClientsInState(m_InventoryStates);
	}

	public static bool AllClientsPlayerSpawned()
	{
		if (!m_SpawnedStatesInit)
		{
			m_SpawnedStates.Add(PlayerLoadState.Spawn_Done);
			m_SpawnedStates.Add(PlayerLoadState.Managers_InProgress);
			m_SpawnedStates.Add(PlayerLoadState.Success);
			m_SpawnedStatesInit = true;
		}
		return T17NetLoadSync.Instance.AreAllClientsInState(m_SpawnedStates);
	}

	public static bool AllClientsManagersInit()
	{
		if (!m_ManagerStatesInit)
		{
			m_ManagerStates.Add(PlayerLoadState.Success);
			m_ManagerStatesInit = true;
		}
		return T17NetLoadSync.Instance.AreAllClientsInState(m_ManagerStates);
	}

	public static bool AllClientsReadyToPlay()
	{
		return T17NetLoadSync.Instance.AreAllClientsInState(PlayerLoadState.Success);
	}

	public static bool IsMasterClientLevelLoaded()
	{
		return T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.LevelLoad_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.LevelInit_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Spawn_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Inventory_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Success);
	}

	public static bool IsMasterClientLevelInit()
	{
		return T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.LevelInit_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Spawn_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Inventory_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Success);
	}

	public static bool IsMasterClientPlayerSpawned()
	{
		return T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Spawn_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Inventory_Done) || T17NetLoadSync.Instance.IsMasterClientInState(PlayerLoadState.Success);
	}

	public static void RegisterOnReadyToPlayInterest(T17NetLoadSync.OnSuccess_PlayerReadyToPlay callback, bool bAdd = true)
	{
		if (bAdd)
		{
			T17NetLoadSync.c_OnPlayerReadyToPlay += callback;
		}
		else
		{
			T17NetLoadSync.c_OnPlayerReadyToPlay -= callback;
		}
	}
}
