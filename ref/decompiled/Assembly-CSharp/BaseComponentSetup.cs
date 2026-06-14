using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseComponentSetup : MonoBehaviour
{
	public enum SetupPriority
	{
		Priority_0_First,
		Priority_1,
		Priority_2,
		Priority_3,
		Priority_4,
		Priority_5,
		Priority_6,
		Priority_7,
		Priority_8,
		Priority_9,
		Priority_10_Last,
		Priority_ReservedForPostprocess,
		COUNT
	}

	public enum SetupReturnState
	{
		OnGoing,
		Finished,
		TakeABreak
	}

	public static FloorManager m_FloorManager = null;

	public static List<object> m_ClassInstanceCache = new List<object>();

	public abstract SetupReturnState Setup();

	public abstract SetupReturnState SetupV2();

	public abstract SetupPriority GetPriority();

	protected SetupReturnState FinishedAndRemove()
	{
		if (Application.isPlaying)
		{
			Object.Destroy(this);
		}
		return SetupReturnState.Finished;
	}

	protected SetupReturnState Finished()
	{
		return SetupReturnState.Finished;
	}

	protected SetupReturnState OnGoing()
	{
		return SetupReturnState.OnGoing;
	}

	protected SetupReturnState TakeABreak()
	{
		return SetupReturnState.TakeABreak;
	}

	protected bool GetLayerAndPosition(ref int X, ref int Y, ref BaseLevelManager.LevelLayers layer, FloorManager.TileSystem_Type tileType = FloorManager.TileSystem_Type.TileSystem_Wall)
	{
		if (m_FloorManager == null && !GetFloorManager())
		{
			return false;
		}
		int row = 0;
		int column = 0;
		int floor = 0;
		if (m_FloorManager.GetTileGridPointAndFloorIndex(base.transform.position, tileType, out row, out column, out floor))
		{
			Y = row;
			X = column;
			layer = (BaseLevelManager.LevelLayers)floor;
			return true;
		}
		return false;
	}

	protected bool GetLayerAndZoneMapIndex(ref int iIndex, ref BaseLevelManager.LevelLayers layer, FloorManager.TileSystem_Type tileType = FloorManager.TileSystem_Type.TileSystem_Wall)
	{
		if (m_FloorManager == null && !GetFloorManager())
		{
			return false;
		}
		int row = 0;
		int column = 0;
		int floor = 0;
		if (m_FloorManager.GetTileGridPointAndFloorIndex(base.transform.position, tileType, out row, out column, out floor))
		{
			iIndex = (119 - row) * 120 + column;
			layer = (BaseLevelManager.LevelLayers)floor;
			return true;
		}
		return false;
	}

	protected bool GetFloorManager()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			if (!(m_FloorManager == null))
			{
				break;
			}
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded && sceneAt.IsValid())
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				int num = rootGameObjects.Length - 1;
				while (num >= 0 && m_FloorManager == null)
				{
					m_FloorManager = rootGameObjects[num].GetComponentInChildren<FloorManager>(includeInactive: true);
					num--;
				}
			}
		}
		return m_FloorManager != null;
	}

	protected T GetClassInstance<T>() where T : Component
	{
		int count = m_ClassInstanceCache.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_ClassInstanceCache[i] != null && m_ClassInstanceCache[i].GetType() is T)
			{
				return (T)m_ClassInstanceCache[i];
			}
		}
		T val = (T)null;
		for (int j = 0; j < SceneManager.sceneCount; j++)
		{
			if (!(val == null))
			{
				break;
			}
			Scene sceneAt = SceneManager.GetSceneAt(j);
			if (sceneAt.isLoaded && sceneAt.IsValid())
			{
				GameObject[] rootGameObjects = sceneAt.GetRootGameObjects();
				int num = rootGameObjects.Length - 1;
				while (num >= 0 && val == null)
				{
					val = rootGameObjects[num].GetComponentInChildren<T>(includeInactive: true);
					num--;
				}
			}
		}
		m_ClassInstanceCache.Add(val);
		return val;
	}

	protected void CleanUp()
	{
		m_FloorManager = null;
		m_ClassInstanceCache.Clear();
	}
}
