using UnityEngine;
using UnityEngine.SceneManagement;

public class Helpers
{
	private static string m_cachedLoadedSceneName;

	private static int m_cachedLoadedSceneNameIndex = int.MaxValue;

	public static bool IsInFrontEndScene()
	{
		GlobalStart.GLOBALSTART_MODE mode = GlobalStart.GetInstance().GetMode();
		return mode == GlobalStart.GLOBALSTART_MODE.SHOW_FRONTEND;
	}

	public static bool IsInGameplayScene()
	{
		GlobalStart.GLOBALSTART_MODE mode = GlobalStart.GetInstance().GetMode();
		return mode == GlobalStart.GLOBALSTART_MODE.IN_LEVEL;
	}

	public static bool IsInResultsScene()
	{
		GlobalStart.GLOBALSTART_MODE mode = GlobalStart.GetInstance().GetMode();
		return mode == GlobalStart.GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS;
	}

	public static bool IsLoadingState(GlobalStart.GLOBALSTART_MODE state)
	{
		switch (state)
		{
		case GlobalStart.GLOBALSTART_MODE.START_LEVEL_LOAD:
		case GlobalStart.GLOBALSTART_MODE.KILL_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.LOADING_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.SETUP_AREA_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART2:
		case GlobalStart.GLOBALSTART_MODE.SETUP_ITEM_MANAGER:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART3:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_CUSTOMISATION:
		case GlobalStart.GLOBALSTART_MODE.LOAD_CUSTOMISATION:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS:
		case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.REQUEST_PLAYER_STARTING_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.NETWORK_INIT_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.INIT_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYERS:
		case GlobalStart.GLOBALSTART_MODE.CHECK_INVITES_DURING_LOAD:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PROFANITY_FILTER:
			return true;
		default:
			return false;
		}
	}

	public static bool IsMasterClientInFrontEnd()
	{
		switch (T17NetRoomListManager.Instance.GetRoomState())
		{
		case GlobalStart.GLOBALSTART_MODE.START_LEVEL_LOAD:
		case GlobalStart.GLOBALSTART_MODE.KILL_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_KILL_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.LOADING_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.LOADING_OTHER_INGAME_SCENES_HUD:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_HUD:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_IGM:
		case GlobalStart.GLOBALSTART_MODE.SETUP_AREA_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART2:
		case GlobalStart.GLOBALSTART_MODE.SETUP_ITEM_MANAGER:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_PART3:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_OTHER_SCENES_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_CUSTOMISATION:
		case GlobalStart.GLOBALSTART_MODE.LOAD_CUSTOMISATION:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_INIT_LEVEL_ITEMS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS:
		case GlobalStart.GLOBALSTART_MODE.SPAWN_LEVEL_PLAYER_OBJECTS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.REQUEST_PLAYER_STARTING_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYER_STARTING_ITEMS_WAITFORPLAYERS:
		case GlobalStart.GLOBALSTART_MODE.NETWORK_INIT_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.INIT_MANAGERS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PLAYERS:
		case GlobalStart.GLOBALSTART_MODE.CHECK_INVITES_DURING_LOAD:
		case GlobalStart.GLOBALSTART_MODE.IN_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.END_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_END_LEVEL:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_END_LEVEL_GC:
		case GlobalStart.GLOBALSTART_MODE.LOAD_RESULTS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_LOAD_RESULTS:
		case GlobalStart.GLOBALSTART_MODE.END_LEVEL_BEHIND_RESULTS:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_END_LEVEL_BEHIND_RESULTS:
		case GlobalStart.GLOBALSTART_MODE.RELOAD_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_RELOAD_FRONTEND:
		case GlobalStart.GLOBALSTART_MODE.LOADING_FLOW_DUMMY_STATE:
		case GlobalStart.GLOBALSTART_MODE.WAIT_FOR_PROFANITY_FILTER:
			return false;
		default:
			return true;
		}
	}

	public static int GetLoadedSceneIndex()
	{
		return SceneManager.GetActiveScene().buildIndex;
	}

	public static string GetLoadedSceneName()
	{
		if (GetLoadedSceneIndex() != m_cachedLoadedSceneNameIndex)
		{
			m_cachedLoadedSceneName = SceneManager.GetActiveScene().name;
		}
		return m_cachedLoadedSceneName;
	}

	public static T FindFirstComponentInScenes<T>() where T : Component
	{
		int sceneCount = SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (!sceneAt.isLoaded || !sceneAt.IsValid())
			{
				continue;
			}
			GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
			for (int num = rootGameObjects.Length - 1; num >= 0; num--)
			{
				T componentInChildren = rootGameObjects[num].GetComponentInChildren<T>(includeInactive: true);
				if (componentInChildren != null)
				{
					return componentInChildren;
				}
			}
		}
		return (T)null;
	}
}
