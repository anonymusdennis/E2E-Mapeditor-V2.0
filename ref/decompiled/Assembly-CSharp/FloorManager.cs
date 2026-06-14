using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NetworkLoadable;
using Pathfinding;
using Rotorz.Tile;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class FloorManager : T17MonoBehaviour, IDeserializable, INetworkLoadable, Saveable
{
	public enum FLOOR_TYPE
	{
		Floor_Roof,
		Floor_Vent,
		Floor_Prison,
		Floor_UnderGround
	}

	public enum TileSystem_Type
	{
		TileSystem_Ground,
		TileSystem_Wall,
		TileSystem_GroundPlops,
		TileSystem_ObjectPlops,
		TileSystem_Lights,
		TileSystem_WallPlops
	}

	[Serializable]
	public class Floor
	{
		[ReadOnly]
		public string m_FloorName;

		[ReadOnly]
		public FLOOR_TYPE m_FloorType;

		[ReadOnly]
		public int m_zPos;

		[ReadOnly]
		public int m_FloorIndex;

		[ReadOnly]
		public bool m_bIsStartFloor;

		[ReadOnly]
		public Transform m_FloorRootObject;

		[ReadOnly]
		public TileSystem[] m_TileSystems;

		[ReadOnly]
		public RoomFloor m_RoomFloor;

		[ReadOnly]
		public bool m_bLocked;

		[ReadOnly]
		public Texture2D m_MapTexture;

		[SerializeField]
		public int m_FloorUINumber = -1;

		~Floor()
		{
			m_FloorRootObject = null;
			if (m_TileSystems == null)
			{
				return;
			}
			int num = m_TileSystems.Length - 1;
			while (num >= 0)
			{
				if (m_TileSystems[num] != null)
				{
					m_TileSystems[num] = null;
				}
				num--;
			}
			m_MapTexture = null;
		}

		public bool IsAboveUnderGround()
		{
			return m_FloorType == FLOOR_TYPE.Floor_Prison && m_FloorIndex == 1 && GetInstance().m_PrisonFloors[0].m_FloorType == FLOOR_TYPE.Floor_UnderGround;
		}

		public bool IsUnderGround()
		{
			return m_FloorType == FLOOR_TYPE.Floor_UnderGround;
		}

		public bool IsPrisonFloor()
		{
			return m_FloorType == FLOOR_TYPE.Floor_Prison;
		}

		public bool IsPrisonFloorOrRoof()
		{
			return m_FloorType == FLOOR_TYPE.Floor_Prison || m_FloorType == FLOOR_TYPE.Floor_Roof;
		}

		public bool IsTheGroundFloor()
		{
			for (int i = 0; i < GetInstance().currentMaxFloor; i++)
			{
				Floor floor = GetInstance().m_PrisonFloors[i];
				if (floor != null && floor.m_FloorType == FLOOR_TYPE.Floor_Prison)
				{
					return floor == this;
				}
			}
			return false;
		}

		public bool IsAboveVent()
		{
			return m_FloorType == FLOOR_TYPE.Floor_Prison && m_FloorIndex > 0 && GetInstance().m_PrisonFloors[m_FloorIndex - 1].m_FloorType == FLOOR_TYPE.Floor_Vent;
		}

		public bool IsVent()
		{
			return m_FloorType == FLOOR_TYPE.Floor_Vent;
		}
	}

	private class TileInfo
	{
		public int m_Row = -1;

		public int m_Column = -1;

		public GameObject m_GameObject;

		public TileInfo(int row, int column, GameObject obj)
		{
			m_Row = row;
			m_Column = column;
			m_GameObject = obj;
		}
	}

	[Serializable]
	private class SaveDataCollection
	{
		public List<FloorSaveData> Floors = new List<FloorSaveData>();

		public List<ObjectSaveData> Holes = new List<ObjectSaveData>();

		public List<ObjectSaveData> Rocks = new List<ObjectSaveData>();

		public List<ObjectSaveData> Braces = new List<ObjectSaveData>();
	}

	[Serializable]
	private class ObjectSaveData
	{
		public int row;

		public int column;

		public int floor;

		public int id;

		public string d;

		public ObjectSaveData(int r, int c, int f, int viewID, string data)
		{
			row = r;
			column = c;
			floor = f;
			id = viewID;
			d = data;
		}
	}

	[Serializable]
	private class FloorSaveData
	{
		public int i;

		public List<TileSystemSaveData> v = new List<TileSystemSaveData>();
	}

	[Serializable]
	private class TileSystemSaveData
	{
		public int t;

		public List<TileSaveData> v = new List<TileSaveData>();
	}

	[Serializable]
	private class TileSaveData
	{
		public int row;

		public int column;

		public List<SavableTileComponent> v = new List<SavableTileComponent>();

		public TileSaveData(int r, int c)
		{
			row = r;
			column = c;
		}
	}

	[Serializable]
	public class SavableTileComponent
	{
		public string t;

		public string d;

		public SavableTileComponent(string componentType, string data)
		{
			t = componentType;
			d = data;
		}
	}

	public const int NEXT_FLOOR_ROW_OFFSET = -1;

	public const int PREV_FLOOR_ROW_OFFSET = 1;

	public const float NEXT_FLOOR_Y_OFFSET = 1f;

	public const float PREV_FLOOR_Y_OFFSET = -1f;

	public static Vector3 FLOOR_SCALE = new Vector3(1f, 1f, 2f);

	private const int MAX_FLOORS = 16;

	private const int MAX_TILESYSTEMS_PER_FLOOR = 6;

	private const int MAX_SERIALISED_TILESYSTEMS_PER_FLOOR = 2;

	private const int RAYCAST_LIST_SIZE = 16;

	[ReadOnly]
	[Header("Floors")]
	public int m_FloorOffset = -3;

	[ReadOnly]
	public float m_HalfFloorOffset = 1.5f;

	public const float Mid_Floor_Offset = 0.5f;

	public Floor[] m_PrisonFloors = new Floor[16];

	[ReadOnly]
	public int currentMaxFloor;

	public float m_RockProbability = 0.4f;

	[Header("Prefabs")]
	public GameObject m_HolePrefab;

	public GameObject m_RockPrefab;

	public GameObject m_TunnelBracePrefab;

	public GameObject m_ItemCoverPrefab;

	[HideInInspector]
	public string m_HolePrefabName = string.Empty;

	[HideInInspector]
	public string m_RockPrefabName = string.Empty;

	[HideInInspector]
	public string m_TunnelBracePrefabName = string.Empty;

	[HideInInspector]
	public string m_ItemCoverPrefabName = string.Empty;

	private List<TileInfo>[,] m_ModifiedFloorTiles = new List<TileInfo>[16, 6];

	private T17NetView m_NetView;

	private RaycastHit[] m_LastRaycastHits = new RaycastHit[16];

	private List<Hole> m_LastHoleProximityCheck = new List<Hole>();

	private List<DamagableTile> m_LastDamagedTileProximityCheck = new List<DamagableTile>();

	[Header("Materials")]
	public UndergroundMaterialMapper m_UndergroundMaterialMapper;

	public Material m_UndergroundMissingWallMaterial;

	public Material m_UndergroundMissingGroundMaterial;

	public Material[] m_floorBakeMaterials;

	public Vector2[] m_FloorBakeTextureOffsets;

	public Texture[] m_AmbientLightMarkerTexs;

	private SaveDataRegister m_SaveData;

	private float m_CachedLastZ;

	private Floor m_CachedLastFloor;

	private float m_CachedLastZRenderer;

	private Floor m_CachedLastFloorRenderer;

	private float m_CachedLastRealZ;

	private Floor m_CachedLastRealFloor;

	private List<Hole> m_Holes = new List<Hole>();

	private List<Rock> m_Rocks = new List<Rock>();

	private List<VentCover> m_VentCovers = new List<VentCover>();

	private List<TunnelBrace> m_TunnelBraces = new List<TunnelBrace>();

	private List<StaticLadder> m_StaticLadders = new List<StaticLadder>();

	private static int m_CollisionLayersToIgnore;

	private ItemContainer m_ItemContainer;

	private static FloorManager m_Instance = null;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static readonly byte[] memSaveData = new byte[8192];

	public ItemContainer GetItemContainer()
	{
		if (m_ItemContainer == null)
		{
			m_ItemContainer = GetComponent<ItemContainer>();
		}
		return m_ItemContainer;
	}

	public static FloorManager GetInstance()
	{
		return m_Instance;
	}

	public static void SetToolsInstance(FloorManager instance)
	{
		m_Instance = instance;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_Instance != null)
		{
			UnityEngine.Object.Destroy(m_Instance);
		}
		m_Instance = this;
		m_CollisionLayersToIgnore = (1 << LayerMask.NameToLayer("Floor")) | (1 << LayerMask.NameToLayer("AudioTrigger")) | (1 << LayerMask.NameToLayer("CharacterCollision")) | (1 << LayerMask.NameToLayer("AI_Event"));
		m_ItemContainer = GetComponent<ItemContainer>();
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		if (m_NetView == null)
		{
		}
		m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 4);
		PhotonPeer.RegisterType(typeof(SaveDataCollection), 70, SerializeSaveDataCollection, DeserializeSaveDataCollection);
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
		if (LevelScript.GetInstance().m_PreBuildSwapPrefabRefs)
		{
			m_HolePrefab = Resources.Load(m_HolePrefabName) as GameObject;
			m_RockPrefab = Resources.Load(m_RockPrefabName) as GameObject;
			m_TunnelBracePrefab = Resources.Load(m_TunnelBracePrefabName) as GameObject;
			m_ItemCoverPrefab = Resources.Load(m_ItemCoverPrefabName) as GameObject;
		}
	}

	protected virtual void OnDestroy()
	{
		LevelScript instance = LevelScript.GetInstance();
		if (instance != null && instance.m_PreBuildSwapPrefabRefs)
		{
			m_HolePrefab = null;
			m_RockPrefab = null;
			m_TunnelBracePrefab = null;
			m_ItemCoverPrefab = null;
		}
		if (NetLoadManagerSync.m_AllNetworkLoadables != null)
		{
			NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		if (m_PrisonFloors != null)
		{
			for (int i = 0; i < currentMaxFloor; i++)
			{
				if (m_PrisonFloors[i] != null)
				{
					m_PrisonFloors[i].m_MapTexture = null;
				}
			}
		}
		if (m_AmbientLightMarkerTexs != null)
		{
			for (int num = m_AmbientLightMarkerTexs.Length - 1; num >= 0; num--)
			{
				if (m_AmbientLightMarkerTexs[num] != null)
				{
					m_AmbientLightMarkerTexs[num] = null;
				}
			}
		}
		m_UndergroundMaterialMapper = null;
		if (m_UndergroundMissingWallMaterial != null)
		{
			m_UndergroundMissingWallMaterial.mainTexture = null;
			m_UndergroundMissingWallMaterial = null;
		}
		if (m_UndergroundMissingGroundMaterial != null)
		{
			m_UndergroundMissingGroundMaterial.mainTexture = null;
			m_UndergroundMissingGroundMaterial = null;
		}
		if (m_PrisonFloors != null)
		{
			for (int num2 = m_PrisonFloors.Length - 1; num2 >= 0; num2--)
			{
				m_PrisonFloors[num2] = null;
			}
		}
		if (m_floorBakeMaterials != null)
		{
			for (int num3 = m_floorBakeMaterials.Length - 1; num3 >= 0; num3--)
			{
				if (m_floorBakeMaterials[num3] != null)
				{
					m_floorBakeMaterials[num3].mainTexture = null;
					m_floorBakeMaterials[num3] = null;
				}
			}
		}
		m_NetView = null;
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_floorBakeMaterials = null;
	}

	public string CreateSnapshot()
	{
		return Serialize();
	}

	public void StartedFromSnapshot()
	{
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			if (m_LoadState == LOADSTATE.Finished_OK)
			{
				SaveDataCollection saveDataCollection = SerializeBinary();
				m_NetView.RPC("RPC_RequestStateResponce_Yes_FloorManager", player, saveDataCollection);
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No_FloorManager", player);
			}
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_Yes_FloorManager(SaveDataCollection saveData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		DeserializeBinary(saveData, ref error);
		m_LoadState = LOADSTATE.Finished_OK;
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_FloorManager(PhotonMessageInfo info)
	{
		m_LoadError = "ItemManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	private SaveDataCollection SerializeBinary()
	{
		SaveDataCollection saveDataCollection = new SaveDataCollection();
		for (int i = 0; i < 16; i++)
		{
			FloorSaveData floorSaveData = new FloorSaveData();
			floorSaveData.i = i;
			for (int j = 0; j < 2; j++)
			{
				List<TileInfo> list = m_ModifiedFloorTiles[i, j];
				if (list == null || list.Count == 0)
				{
					continue;
				}
				TileSystemSaveData tileSystemSaveData = new TileSystemSaveData();
				tileSystemSaveData.t = j;
				int count = list.Count;
				for (int k = 0; k < count; k++)
				{
					TileInfo tileInfo = list[k];
					GameObject gameObject = tileInfo.m_GameObject;
					if (gameObject == null)
					{
						continue;
					}
					TileSaveData tileSaveData = new TileSaveData(tileInfo.m_Row, tileInfo.m_Column);
					MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
					if (components != null && components.Length > 0)
					{
						for (int l = 0; l < components.Length; l++)
						{
							if (components[l] is ISaveableTileComponent saveableTileComponent && saveableTileComponent.RequiresSaving())
							{
								tileSaveData.v.Add(new SavableTileComponent(saveableTileComponent.GetType().Name, saveableTileComponent.SerializeData()));
							}
						}
					}
					if (tileSaveData.v.Count > 0)
					{
						tileSystemSaveData.v.Add(tileSaveData);
					}
				}
				if (tileSystemSaveData.v.Count > 0)
				{
					floorSaveData.v.Add(tileSystemSaveData);
				}
			}
			if (floorSaveData.v.Count > 0)
			{
				saveDataCollection.Floors.Add(floorSaveData);
			}
		}
		for (int m = 0; m < m_Holes.Count; m++)
		{
			Hole hole = m_Holes[m];
			if (hole != null)
			{
				saveDataCollection.Holes.Add(new ObjectSaveData(hole.TileRow, hole.TileColumn, hole.TileFloor, hole.GetObjectNetID(), hole.SerializeData()));
			}
		}
		for (int n = 0; n < m_Rocks.Count; n++)
		{
			Rock rock = m_Rocks[n];
			if (rock != null && rock.RandomlySpawned)
			{
				saveDataCollection.Rocks.Add(new ObjectSaveData(rock.TileRow, rock.TileColumn, rock.TileFloor, 0, null));
			}
		}
		for (int num = 0; num < m_TunnelBraces.Count; num++)
		{
			TunnelBrace tunnelBrace = m_TunnelBraces[num];
			if (tunnelBrace != null)
			{
				saveDataCollection.Braces.Add(new ObjectSaveData(tunnelBrace.TileRow, tunnelBrace.TileColumn, tunnelBrace.TileFloor, 0, null));
			}
		}
		return saveDataCollection;
	}

	public string Serialize()
	{
		SaveDataCollection obj = SerializeBinary();
		return JsonUtility.ToJson(obj);
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	private bool DeserializeBinary(SaveDataCollection saveData, ref string error)
	{
		if (saveData.Floors != null)
		{
			for (int i = 0; i < saveData.Floors.Count; i++)
			{
				int i2 = saveData.Floors[i].i;
				List<TileSystemSaveData> v = saveData.Floors[i].v;
				if (i2 < 0 || i2 >= 16 || v == null)
				{
					continue;
				}
				for (int j = 0; j < v.Count; j++)
				{
					TileSystemSaveData tileSystemSaveData = v[j];
					if (tileSystemSaveData == null)
					{
						continue;
					}
					int t = tileSystemSaveData.t;
					List<TileSaveData> v2 = tileSystemSaveData.v;
					if (t < 0 || t >= 2 || v2 == null)
					{
						continue;
					}
					for (int k = 0; k < v2.Count; k++)
					{
						int row = v2[k].row;
						int column = v2[k].column;
						List<SavableTileComponent> v3 = v2[k].v;
						if (v3 == null)
						{
							continue;
						}
						TileData tile = GetTile(i2, (TileSystem_Type)t, row, column);
						if (tile == null || tile.gameObject == null)
						{
							continue;
						}
						for (int l = 0; l < v3.Count; l++)
						{
							ISaveableTileComponent saveableTileComponent = tile.gameObject.GetComponent(v3[l].t) as ISaveableTileComponent;
							if (saveableTileComponent == null)
							{
								Type type = Type.GetType(v3[l].t);
								if (type != null)
								{
									Component component = tile.gameObject.AddComponent(type);
									DamagableTile damagableTile = component as DamagableTile;
									if (damagableTile != null)
									{
										damagableTile.m_DamageAction = DamagableTile.DamageAction.Hole;
									}
									T17MonoBehaviour t17MonoBehaviour = component as T17MonoBehaviour;
									if (t17MonoBehaviour != null)
									{
										t17MonoBehaviour.StartInit();
									}
									saveableTileComponent = component as ISaveableTileComponent;
								}
							}
							saveableTileComponent?.DeserializeData(v3[l].d);
						}
						UpdateModifiedTiles(i2, t, row, column, tile.gameObject);
					}
				}
			}
		}
		if (saveData.Holes != null)
		{
			for (int m = 0; m < saveData.Holes.Count; m++)
			{
				ObjectSaveData objectSaveData = saveData.Holes[m];
				if (objectSaveData != null)
				{
					Hole hole = InstantiateHole(objectSaveData.row, objectSaveData.column, objectSaveData.floor);
					if (hole != null)
					{
						hole.m_NetView.viewID = objectSaveData.id;
						hole.DeserializeData(objectSaveData.d);
					}
				}
			}
		}
		if (saveData.Rocks != null)
		{
			for (int n = 0; n < saveData.Rocks.Count; n++)
			{
				ObjectSaveData objectSaveData2 = saveData.Rocks[n];
				if (objectSaveData2 != null)
				{
					InstantiateRock(objectSaveData2.row, objectSaveData2.column, objectSaveData2.floor);
				}
			}
		}
		if (saveData.Braces != null)
		{
			for (int num = 0; num < saveData.Braces.Count; num++)
			{
				ObjectSaveData objectSaveData3 = saveData.Braces[num];
				if (objectSaveData3 != null)
				{
					InstantiateTunnelBrace(objectSaveData3.row, objectSaveData3.column, objectSaveData3.floor);
				}
			}
		}
		return true;
	}

	public bool Deserialize(string serializedData, ref string error)
	{
		if (string.IsNullOrEmpty(serializedData))
		{
			return true;
		}
		SaveDataCollection saveDataCollection = null;
		try
		{
			saveDataCollection = JsonUtility.FromJson<SaveDataCollection>(serializedData);
		}
		catch
		{
			error = "Floor Manager could not parse JSON data, it is currupt.";
			return false;
		}
		if (saveDataCollection == null)
		{
			error = "FloorManager: JSON data returned null.";
			return false;
		}
		return DeserializeBinary(saveDataCollection, ref error);
	}

	[ContextMenu("Save")]
	private void Save()
	{
		string value = Serialize();
		if (!string.IsNullOrEmpty(value))
		{
			GlobalSave.GetInstance().Set("FloorManager:Serialization", value);
			GlobalSave.GetInstance().RequestSave();
		}
	}

	[ContextMenu("Load")]
	private void Load()
	{
		GlobalSave.GetInstance().Get("FloorManager:Serialization", out var value, string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			string error = string.Empty;
			Deserialize(value, ref error);
		}
	}

	public List<Floor> GetValidFloors()
	{
		List<Floor> list = new List<Floor>();
		for (int i = 0; i < currentMaxFloor; i++)
		{
			list.Add(m_PrisonFloors[i]);
		}
		return list;
	}

	public Floor GetStartFloor()
	{
		for (int i = 0; i < m_PrisonFloors.Length; i++)
		{
			if (m_PrisonFloors[i].m_bIsStartFloor)
			{
				return m_PrisonFloors[i];
			}
		}
		return null;
	}

	public Floor DownAFloor(Floor currentFloor)
	{
		int floorIndex = currentFloor.m_FloorIndex;
		if (floorIndex > 0)
		{
			floorIndex--;
			return m_PrisonFloors[floorIndex];
		}
		return currentFloor;
	}

	public Floor UpAFloor(Floor currentFloor)
	{
		int floorIndex = currentFloor.m_FloorIndex;
		if (floorIndex < currentMaxFloor - 1)
		{
			floorIndex++;
			return m_PrisonFloors[floorIndex];
		}
		return currentFloor;
	}

	public Floor GetUndergroundFloor()
	{
		if (m_PrisonFloors == null || m_PrisonFloors.Length == 0)
		{
			return null;
		}
		if (m_PrisonFloors[0].IsUnderGround())
		{
			return m_PrisonFloors[0];
		}
		for (int i = 0; i < m_PrisonFloors.Length; i++)
		{
			if (m_PrisonFloors[i].IsUnderGround())
			{
				return m_PrisonFloors[i];
			}
		}
		return null;
	}

	public Floor FindRealFloorAtZ(float posZ)
	{
		if (m_CachedLastRealFloor != null && Mathf.Abs(m_CachedLastRealZ - posZ) < 0.001f)
		{
			return m_CachedLastRealFloor;
		}
		Floor floor = m_PrisonFloors[0];
		float num = 0f;
		for (int i = 0; i < currentMaxFloor; i++)
		{
			Floor floor2 = m_PrisonFloors[i];
			num = floor2.m_zPos;
			if (floor2 != null && num >= posZ && posZ > num + (float)m_FloorOffset)
			{
				floor = floor2;
			}
		}
		m_CachedLastRealZ = posZ;
		m_CachedLastRealFloor = floor;
		return floor;
	}

	public Floor FindFloorAtZ(float posZ)
	{
		if (m_CachedLastFloor != null && Mathf.Abs(m_CachedLastZ - posZ) < 0.001f)
		{
			return m_CachedLastFloor;
		}
		Floor floor = m_PrisonFloors[0];
		if (floor != null)
		{
			float num = Mathf.Abs((float)floor.m_zPos - posZ);
			for (int i = 1; i < currentMaxFloor; i++)
			{
				Floor floor2 = m_PrisonFloors[i];
				if (floor2 != null)
				{
					float num2 = Mathf.Abs(posZ - (float)floor2.m_zPos);
					if (num2 < num)
					{
						floor = floor2;
						num = num2;
					}
				}
			}
		}
		m_CachedLastZ = posZ;
		m_CachedLastFloor = floor;
		return floor;
	}

	public int FindFloorIndexAtZ(float posZ)
	{
		int result = 0;
		Floor floor = FindFloorAtZ(posZ);
		if (floor != null)
		{
			result = floor.m_FloorIndex;
		}
		return result;
	}

	public Floor FindFloorForRendererZ(float posZ)
	{
		float num = 0.01f;
		if (m_CachedLastFloorRenderer != null && Mathf.Abs(m_CachedLastZRenderer - posZ) < num)
		{
			return m_CachedLastFloorRenderer;
		}
		Floor floor = m_PrisonFloors[0];
		if (posZ > (float)m_PrisonFloors[0].m_zPos + num)
		{
			floor = m_PrisonFloors[0];
		}
		else if (posZ < (float)m_PrisonFloors[currentMaxFloor - 1].m_zPos + num)
		{
			floor = m_PrisonFloors[currentMaxFloor - 1];
		}
		else
		{
			for (int i = 0; i < currentMaxFloor - 1; i++)
			{
				if (posZ >= (float)m_PrisonFloors[i + 1].m_zPos + num && posZ <= (float)m_PrisonFloors[i].m_zPos + num)
				{
					floor = m_PrisonFloors[i];
					break;
				}
			}
		}
		m_CachedLastZRenderer = posZ;
		m_CachedLastFloorRenderer = floor;
		return floor;
	}

	public int FindFloorIndexForRendererZ(float posZ)
	{
		return FindFloorForRendererZ(posZ).m_FloorIndex;
	}

	public int FindFloorIndex(Floor floor)
	{
		return floor.m_FloorIndex;
	}

	public Floor FindFloorbyIndex(int index)
	{
		if (index >= 0 && index < currentMaxFloor)
		{
			return m_PrisonFloors[index];
		}
		return null;
	}

	public Floor FindFloorByName(string name)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			if (m_PrisonFloors[i].m_FloorName == name)
			{
				return m_PrisonFloors[i];
			}
		}
		return null;
	}

	public Floor FindFloorByRootInst(Transform inst)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			if (m_PrisonFloors[i].m_FloorRootObject == inst)
			{
				return m_PrisonFloors[i];
			}
		}
		return null;
	}

	public bool GetSystemType(TileSystem system, out TileSystem_Type systemType)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			for (int j = 0; j < m_PrisonFloors[i].m_TileSystems.Length; j++)
			{
				if (m_PrisonFloors[i].m_TileSystems[j] == system)
				{
					systemType = (TileSystem_Type)j;
					return true;
				}
			}
		}
		systemType = TileSystem_Type.TileSystem_Lights;
		return false;
	}

	public FLOOR_TYPE GetFloorTypeForTileSystem(TileSystem system)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			for (int j = 0; j < m_PrisonFloors[i].m_TileSystems.Length; j++)
			{
				if (m_PrisonFloors[i].m_TileSystems[j] == system)
				{
					return m_PrisonFloors[i].m_FloorType;
				}
			}
		}
		return FLOOR_TYPE.Floor_Prison;
	}

	public bool IsUndergroundTileSystem(TileSystem system)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			if (!m_PrisonFloors[i].IsUnderGround())
			{
				continue;
			}
			for (int j = 0; j < m_PrisonFloors[i].m_TileSystems.Length; j++)
			{
				if (m_PrisonFloors[i].m_TileSystems[j] == system)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsFloorClear(Floor floor, int row, int column, out RaycastHit[] hitList, out int hitCount)
	{
		if (floor != null && GetTileCentrePosition(floor, TileSystem_Type.TileSystem_Wall, row, column, out var worldPosition))
		{
			float num = 1.5f;
			Vector3 vector = new Vector3(0f, 0f, 1f);
			worldPosition.z -= num;
			int num2 = Physics.RaycastNonAlloc(worldPosition, vector, m_LastRaycastHits, num, ~m_CollisionLayersToIgnore, QueryTriggerInteraction.Ignore);
			if (Debug.isDebugBuild)
			{
				Debug.DrawRay(worldPosition, vector * num, (num2 <= 0) ? Color.green : Color.red, 10f);
			}
			hitList = m_LastRaycastHits;
			hitCount = Mathf.Min(num2, m_LastRaycastHits.Length);
			return num2 == 0;
		}
		hitList = null;
		hitCount = 0;
		return false;
	}

	public bool IsFloorClear(Floor floor, int row, int column)
	{
		RaycastHit[] hitList = null;
		int hitCount = 0;
		return IsFloorClear(floor, row, column, out hitList, out hitCount);
	}

	public bool IsFloorClear(Floor floor, Vector3 position)
	{
		if (floor != null && GetTileGridPoint(floor, TileSystem_Type.TileSystem_Ground, position, out var row, out var column))
		{
			return IsFloorClear(floor, row, column);
		}
		return false;
	}

	public bool IsFloorAheadClear(Floor floor, int startRow, int startColumn, int deltaRow, int deltaColumn, CarryObjectInteraction objectToDrop = null)
	{
		TileSystem tileSystem = GetTileSystem(floor, TileSystem_Type.TileSystem_Wall);
		if (tileSystem != null && GetTileCentrePosition(floor, TileSystem_Type.TileSystem_Wall, startRow, startColumn, out var worldPosition))
		{
			int row = Mathf.Clamp(startRow + deltaRow, 0, tileSystem.RowCount - 1);
			int column = Mathf.Clamp(startColumn + deltaColumn, 0, tileSystem.ColumnCount - 1);
			if (GetTileCentrePosition(floor, TileSystem_Type.TileSystem_Wall, row, column, out var worldPosition2))
			{
				Vector3 vector = worldPosition2 - worldPosition;
				float magnitude = vector.magnitude;
				vector.Normalize();
				int num = Physics.RaycastNonAlloc(worldPosition, vector, m_LastRaycastHits, magnitude, ~m_CollisionLayersToIgnore, QueryTriggerInteraction.Ignore);
				bool flag;
				if (objectToDrop != null)
				{
					flag = true;
					for (int i = 0; i < num; i++)
					{
						ICarryableObjectConsumer component = m_LastRaycastHits[i].transform.GetComponent<ICarryableObjectConsumer>();
						if (component != null)
						{
							if (!component.WillAcceptInput(objectToDrop))
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
				}
				else
				{
					flag = num == 0;
				}
				if (Debug.isDebugBuild)
				{
					Debug.DrawRay(worldPosition, vector * magnitude, (!flag) ? Color.green : Color.red, 10f);
				}
				return flag;
			}
		}
		return false;
	}

	public Collider[] BoxCollideTileArea(Floor floor, int row, int column, bool checkTriggers = false)
	{
		if (floor != null && GetTileCentrePosition(floor, TileSystem_Type.TileSystem_Wall, row, column, out var worldPosition))
		{
			float num = 0.25f;
			QueryTriggerInteraction queryTriggerInteraction = ((!checkTriggers) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
			worldPosition.z -= num;
			return Physics.OverlapBox(halfExtents: new Vector3(num, num, num), center: worldPosition, orientation: Quaternion.identity, layerMask: ~m_CollisionLayersToIgnore, queryTriggerInteraction: queryTriggerInteraction);
		}
		return null;
	}

	public int BoxCollideTileAreaNonAlloc(Floor floor, int row, int column, TileSystem_Type tileSystem, bool checkTriggers = false, float halfBoxWidth = 0.25f)
	{
		if (floor != null && GetTileCentrePosition(floor, tileSystem, row, column, out var worldPosition))
		{
			QueryTriggerInteraction queryTriggerInteraction = ((!checkTriggers) ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
			worldPosition.z -= halfBoxWidth;
			return EscapistsRaycast.OverlapBoxNonAlloc(halfExtents: new Vector3(halfBoxWidth, halfBoxWidth, halfBoxWidth), center: worldPosition, orientation: Quaternion.identity, layerMask: ~m_CollisionLayersToIgnore, queryTriggerInteraction: queryTriggerInteraction);
		}
		return 0;
	}

	private TileSystem GetTileSystem(int floorIndex, TileSystem_Type systemType)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return GetTileSystem(m_PrisonFloors[floorIndex], systemType);
		}
		return null;
	}

	private TileSystem GetTileSystem(Floor floor, TileSystem_Type systemType)
	{
		if (floor != null)
		{
			TileSystem[] tileSystems = floor.m_TileSystems;
			if (tileSystems != null && (int)systemType < tileSystems.Length)
			{
				return tileSystems[(int)systemType];
			}
		}
		return null;
	}

	public TileData GetTile(int floorIndex, TileSystem_Type systemType, int row, int column)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return GetTile(m_PrisonFloors[floorIndex], systemType, row, column);
		}
		return null;
	}

	public TileData GetTile(Floor floor, TileSystem_Type systemType, int row, int column)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null && tileSystem.InBounds(row, column))
		{
			return tileSystem.GetTileOrNull(row, column);
		}
		return null;
	}

	public void GetTileSystemBounds(Floor floor, TileSystem_Type systemType, out int maxRows, out int maxColumns)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null)
		{
			maxRows = tileSystem.RowCount;
			maxColumns = tileSystem.ColumnCount;
		}
		else
		{
			maxRows = 0;
			maxColumns = 0;
		}
	}

	public bool CheckIsInBounds(int floorIndex, TileSystem_Type systemType, int row, int column)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return CheckIsInBounds(m_PrisonFloors[floorIndex], systemType, row, column);
		}
		return false;
	}

	public bool CheckIsInBounds(Floor floor, TileSystem_Type systemType, int row, int column)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null)
		{
			return tileSystem.InBounds(row, column);
		}
		return false;
	}

	public bool CheckTileExists(Floor floor, TileSystem_Type systemType, Vector3 worldPosition, bool bIncludeInactive = false)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null)
		{
			Vector3 vector = tileSystem.WorldPositionFromTileIndex(0, 0, center: false);
			Vector3 vector2 = worldPosition - vector;
			int column = (int)vector2.x;
			int row = -(int)vector2.y;
			if (tileSystem.InBounds(row, column))
			{
				TileData tileOrNull = tileSystem.GetTileOrNull(row, column);
				if (tileOrNull != null && tileOrNull.gameObject != null)
				{
					return bIncludeInactive || tileOrNull.gameObject.GetActive();
				}
			}
		}
		return false;
	}

	public bool CheckTileExists(int floorIndex, TileSystem_Type systemType, int row, int column, bool bIncludeInactive = false)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return CheckTileExists(m_PrisonFloors[floorIndex], systemType, row, column, bIncludeInactive);
		}
		return false;
	}

	public bool CheckTileExists(Floor floor, TileSystem_Type systemType, int row, int column, bool bIncludeInactive = false)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			return bIncludeInactive || tile.gameObject.GetActive();
		}
		return false;
	}

	public bool CheckClimbableTileExists(Floor floor, TileSystem_Type systemType, int row, int column, out ClimbableTile.ClimbAction climbAction)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
		{
			ClimbableTile component = tile.gameObject.GetComponent<ClimbableTile>();
			if (component != null)
			{
				climbAction = component.m_ClimbAction;
				return true;
			}
		}
		climbAction = ClimbableTile.ClimbAction.Invaid;
		return false;
	}

	public bool CheckDamagableTileExists(Floor floor, TileSystem_Type systemType, int row, int column, bool bIncludeInactive, out DamagableTile.DamageAction action, out float health)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && (bIncludeInactive || tile.gameObject.GetActive()))
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if ((bool)component)
			{
				action = component.m_DamageAction;
				health = component.Health;
				return true;
			}
		}
		action = DamagableTile.DamageAction.Dig;
		health = 0f;
		return false;
	}

	public T GetTileComponent<T>(Floor floor, TileSystem_Type systemType, int row, int column) where T : Component
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			T component = tile.gameObject.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
		}
		return (T)null;
	}

	public bool CheckTileHasTag(Floor floor, TileSystem_Type systemType, int row, int column, string hasTag)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
		{
			return tile.gameObject.CompareTag(hasTag);
		}
		return false;
	}

	public DamagableTile GetDamagableTile(Floor floor, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction action)
	{
		DamagableTile damagableTile = GetDamagableTile(floor, systemType, row, column);
		if (damagableTile != null && damagableTile.m_DamageAction == action)
		{
			return damagableTile;
		}
		return null;
	}

	public DamagableTile GetDamagableTile(Floor floor, TileSystem_Type systemType, int row, int column)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}

	public bool GetTileGridPoint(int floorIndex, TileSystem_Type systemType, Vector3 ourPosition, out int row, out int column)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return GetTileGridPoint(m_PrisonFloors[floorIndex], systemType, ourPosition, out row, out column);
		}
		row = 0;
		column = 0;
		return false;
	}

	public bool GetTileGridPoint(Floor floor, TileSystem_Type systemType, Vector3 ourPosition, out int row, out int column)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null)
		{
			Vector3 vector = tileSystem.WorldPositionFromTileIndex(0, 0, center: false);
			Vector3 vector2 = ourPosition - vector;
			column = (int)vector2.x;
			row = -(int)vector2.y;
			return tileSystem.InBounds(row, column);
		}
		row = 0;
		column = 0;
		return false;
	}

	public bool GetTileGridPoint(Vector3 ourPosition, TileSystem_Type systemType, out int row, out int column, out int floor)
	{
		Floor floor2 = FindFloorAtZ(ourPosition.z);
		floor = floor2.m_zPos;
		return GetTileGridPoint(floor2, systemType, ourPosition, out row, out column);
	}

	public bool GetTileGridPointAndFloorIndex(Vector3 ourPosition, TileSystem_Type systemType, out int row, out int column, out int floor)
	{
		Floor floor2 = FindFloorAtZ(ourPosition.z);
		floor = floor2.m_FloorIndex;
		return GetTileGridPoint(floor2, systemType, ourPosition, out row, out column);
	}

	public bool GetTileCentrePosition(int floorIndex, TileSystem_Type systemType, int row, int column, out Vector3 worldPosition)
	{
		worldPosition = Vector3.zero;
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return GetTileCentrePosition(m_PrisonFloors[floorIndex], systemType, row, column, out worldPosition);
		}
		return false;
	}

	public bool GetTileCentrePosition(Floor floor, TileSystem_Type systemType, int row, int column, out Vector3 worldPosition)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null && tileSystem.InBounds(row, column))
		{
			worldPosition = tileSystem.WorldPositionFromTileIndex(row, column);
			worldPosition.z = floor.m_zPos;
			return true;
		}
		worldPosition = Vector3.zero;
		return false;
	}

	public bool GetTileCentrePosition(Floor floor, TileSystem_Type systemType, Vector3 ourPosition, out Vector3 centredPosition)
	{
		TileSystem tileSystem = GetTileSystem(floor, systemType);
		if (tileSystem != null)
		{
			Vector3 vector = tileSystem.WorldPositionFromTileIndex(0, 0, center: false);
			Vector3 vector2 = ourPosition - vector;
			int column = (int)vector2.x;
			int row = -(int)vector2.y;
			if (tileSystem.InBounds(row, column))
			{
				centredPosition = tileSystem.WorldPositionFromTileIndex(row, column);
				centredPosition.z = floor.m_zPos;
				return true;
			}
		}
		centredPosition = Vector3.zero;
		return false;
	}

	public bool GetTileCentrePosition(int floorIndex, TileSystem_Type systemType, Vector3 ourPosition, out Vector3 centredPosition)
	{
		if (m_PrisonFloors != null && floorIndex < m_PrisonFloors.Length)
		{
			return GetTileCentrePosition(m_PrisonFloors[floorIndex], systemType, ourPosition, out centredPosition);
		}
		centredPosition = Vector3.zero;
		return false;
	}

	public Vector3 GetWorldCoordinateForTileSystemOrigin()
	{
		TileSystem tileSystem = GetTileSystem(0, TileSystem_Type.TileSystem_Ground);
		if (tileSystem != null)
		{
			return tileSystem.WorldPositionFromTileIndex(0, 0, center: false);
		}
		return Vector3.zero;
	}

	public void EnsureNonVentPosition(ref Vector3 target)
	{
		Floor floor = FindFloorAtZ(target.z);
		if (floor != null && floor.IsVent())
		{
			Floor floor2 = DownAFloor(floor);
			if (floor2 != null)
			{
				target.z = floor2.m_zPos;
			}
		}
	}

	public int FindGround(Floor startFloor, int startRow, int startColumn, out Floor groundFloor, out int groundRow, out int groundColumn, out bool groundIsClear)
	{
		if (GetInstance().CheckTileExists(startFloor, TileSystem_Type.TileSystem_Ground, startRow, startColumn, bIncludeInactive: true))
		{
			groundFloor = startFloor;
			groundRow = startRow;
			groundColumn = startColumn;
			groundIsClear = IsFloorClear(groundFloor, groundRow, groundColumn);
			return 0;
		}
		int num = 0;
		Floor floor = startFloor;
		Floor floor2 = null;
		int num2 = startRow;
		while (true)
		{
			floor2 = DownAFloor(floor);
			if (floor2 == floor)
			{
				break;
			}
			floor = floor2;
			if (!floor.IsVent())
			{
				num++;
				num2++;
				if (GetInstance().CheckTileExists(floor2, TileSystem_Type.TileSystem_Ground, num2, startColumn, bIncludeInactive: true))
				{
					break;
				}
			}
		}
		groundFloor = floor2;
		groundRow = num2;
		groundColumn = startColumn;
		groundIsClear = IsFloorClear(groundFloor, groundRow, groundColumn);
		return num;
	}

	public void GetUndergroundMaterials(int mask, ref Material[] wallMaterials, ref Material groundMaterial)
	{
		if (m_UndergroundMaterialMapper != null)
		{
			int count = m_UndergroundMaterialMapper.m_Map.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_UndergroundMaterialMapper.m_Map[i].m_Mask == mask)
				{
					int num = 0;
					int count2 = m_UndergroundMaterialMapper.m_Map[i].m_WallArtBrush.m_DamageStageMaterials.Count;
					if (wallMaterials == null || wallMaterials.Length != 1 + count2)
					{
						wallMaterials = new Material[1 + count2];
					}
					for (int num2 = count2 - 1; num2 >= 0; num2--)
					{
						wallMaterials[num++] = m_UndergroundMaterialMapper.m_Map[i].m_WallArtBrush.m_DamageStageMaterials[num2];
					}
					wallMaterials[num++] = m_UndergroundMaterialMapper.m_Map[i].m_WallArtBrush.m_MainMaterial;
					groundMaterial = null;
					return;
				}
			}
		}
		wallMaterials = new Material[1] { m_UndergroundMissingWallMaterial };
		groundMaterial = m_UndergroundMissingGroundMaterial;
	}

	public bool CanCoverTile(Floor floor, TileSystem_Type systemType, int row, int column, int coverItemID, DamagableTile.DamageAction coverDamageAction)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null && component.m_DamageAction == coverDamageAction)
			{
				return component.CanBeCovered(coverItemID);
			}
		}
		return false;
	}

	public bool CanSharpenOnTile(Floor floor, TileSystem_Type systemType, int row, int column)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			return true;
		}
		return false;
	}

	public bool CanDamageTile(Floor floor, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction action, int damagingItemID)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				return component.CanBeDamaged(action, damagingItemID);
			}
		}
		return false;
	}

	public bool WouldFullyDamageTile(Floor floor, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction damageAction, int damagingItemID, float fDamageAmount, out ItemData itemReclaimed)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				return component.WouldFullyDamage(damageAction, damagingItemID, fDamageAmount, out itemReclaimed);
			}
		}
		itemReclaimed = null;
		return false;
	}

	public void DamageTile(Floor floor, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction damageAction, int damagingItemID, float fDamageAmount, bool bAllowReclaimItem, Character character)
	{
		m_NetView.RPC("RPC_Master_DamageTile", NetTargets.MasterClient, floor.m_FloorIndex, systemType, row, column, damageAction, damagingItemID, fDamageAmount, bAllowReclaimItem, character.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_Master_DamageTile(int floorIndex, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction damageAction, int damagingItemID, float fDamageAmount, bool bAllowReclaimItem, int characterID, PhotonMessageInfo info)
	{
		Character character = null;
		if (characterID != -1)
		{
			character = T17NetView.Find<Character>(characterID);
		}
		if (!(character == null))
		{
			Floor floor = FindFloorbyIndex(floorIndex);
			if (floor != null)
			{
				DoDamageToTile(floor, systemType, row, column, damageAction, damagingItemID, fDamageAmount, bAllowReclaimItem, character);
			}
		}
	}

	private bool DoDamageToTile(Floor floor, TileSystem_Type systemType, int row, int column, DamagableTile.DamageAction action, int damagingItemID, float damageAmount, bool bAllowReclaimItem, Character character)
	{
		TileData tile = GetTile(floor, systemType, row, column);
		if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null && !component.IsFullyDamaged())
			{
				if (component.Damage(action, damagingItemID, damageAmount, bAllowReclaimItem, character))
				{
					UpdateModifiedTiles(floor.m_FloorIndex, (int)systemType, row, column, tile.gameObject);
					int viewID = character.m_NetView.viewID;
					m_NetView.RPC("RPC_Others_DamageTile", NetTargets.Others, floor.m_FloorIndex, systemType, row, column, component.Health, viewID);
					if (component.IsFullyDamaged())
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Tile " + action, LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " tile " + action.ToString() + " 100%", Gamer.GetGamerCount() + " Player", 0L);
						if (damageAmount > 0f && floor.IsUnderGround() && component.m_DamageAction == DamagableTile.DamageAction.Dig && component.m_AllowRandomRock && UnityEngine.Random.value <= m_RockProbability)
						{
							SpawnRock(row, column, floor.m_FloorIndex);
						}
						ScoreManager.EventRPC(ScoreManager.Events.PrisonTileDestroyed, character);
					}
					return true;
				}
				return false;
			}
		}
		return false;
	}

	[PunRPC]
	public void RPC_Others_DamageTile(int floorIndex, TileSystem_Type systemType, int row, int column, float newHealth, int characterViewId, PhotonMessageInfo info)
	{
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				component.SetHealth(newHealth, characterViewId);
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
			}
		}
	}

	public void RepairTile(Floor floor, TileSystem_Type systemType, int row, int column, float fHealthRestoreAmount, Character character)
	{
		m_NetView.RPC("RPC_Master_RepairTile", NetTargets.MasterClient, floor.m_FloorIndex, systemType, row, column, fHealthRestoreAmount, character.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_Master_RepairTile(int floorIndex, TileSystem_Type systemType, int row, int column, float fHealthRestoreAmount, int characterID, PhotonMessageInfo info)
	{
		Character character = null;
		PhotonView photonView = PhotonView.Find(characterID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		if (character == null)
		{
			return;
		}
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null && component.HasBeenDamaged() && component.Repair(fHealthRestoreAmount))
			{
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
				m_NetView.RPC("RPC_Others_RepairTile", NetTargets.Others, floorIndex, systemType, row, column, component.Health, characterID);
			}
		}
	}

	[PunRPC]
	public void RPC_Others_RepairTile(int floorIndex, TileSystem_Type systemType, int row, int column, float newHealth, int characterViewId, PhotonMessageInfo info)
	{
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				component.SetHealth(newHealth, characterViewId);
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
			}
		}
	}

	public void GiveTileItem(Floor floor, TileSystem_Type systemType, int row, int column, Item item, Character character)
	{
		if (!(item == null) && !(item.m_NetView == null))
		{
			int viewID = item.m_NetView.viewID;
			m_NetView.RPC("RPC_Master_GiveTileItem", NetTargets.MasterClient, floor.m_FloorIndex, systemType, row, column, viewID, character.m_NetView.viewID);
		}
	}

	[PunRPC]
	public void RPC_Master_GiveTileItem(int floorIndex, TileSystem_Type systemType, int row, int column, int itemViewId, int characterID, PhotonMessageInfo info)
	{
		Character character = null;
		if (characterID != -1)
		{
			character = T17NetView.Find<Character>(characterID);
		}
		if (character == null)
		{
			return;
		}
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile == null || !(tile.gameObject != null))
		{
			return;
		}
		DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
		if (component != null && component.IsFullyDamaged() && component.CanGiveItem())
		{
			Item item = T17NetView.Find<Item>(itemViewId);
			character.RemoveItemRPC(item, RPC_CallContexts.Master, release: false, addToOldInventory: false);
			if (component.GiveItem(itemViewId))
			{
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
				m_NetView.RPC("RPC_Others_GiveTileItem", NetTargets.Others, floorIndex, systemType, row, column, itemViewId);
			}
		}
	}

	[PunRPC]
	public void RPC_Others_GiveTileItem(int floorIndex, TileSystem_Type systemType, int row, int column, int itemViewId, PhotonMessageInfo info)
	{
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				component.SetItem(itemViewId);
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
			}
		}
	}

	public Item GetItemAtFloorTile(Floor floor, TileSystem_Type systemType, int row, int column)
	{
		GetTileCentrePosition(floor, systemType, row, column, out var worldPosition);
		int num = EscapistsRaycast.OverlapSphereNonAlloc(worldPosition, 0.4f, LayerMask.GetMask("Items"), QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			if (EscapistsRaycast.ColliderOverlapList[i] != null && EscapistsRaycast.ColliderOverlapList[i].gameObject != null)
			{
				Item component = EscapistsRaycast.ColliderOverlapList[i].GetComponent<Item>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	public void RemoveTileItem(Floor floor, TileSystem_Type systemType, int row, int column, ItemContainer toContainer = null)
	{
		int num = ((!(toContainer == null)) ? toContainer.NetView.viewID : (-1));
		m_NetView.RPC("RPC_Master_RemoveTileItem", NetTargets.MasterClient, floor.m_FloorIndex, systemType, row, column, num);
	}

	[PunRPC]
	public void RPC_Master_RemoveTileItem(int floorIndex, TileSystem_Type systemType, int row, int column, int itemContainerId, PhotonMessageInfo info)
	{
		ItemContainer itemContainer = null;
		if (itemContainerId != -1)
		{
			itemContainer = T17NetView.Find<ItemContainer>(itemContainerId);
			if (itemContainer == null)
			{
				return;
			}
		}
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile == null || !(tile.gameObject != null))
		{
			return;
		}
		DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
		if (component != null)
		{
			bool flag = false;
			if ((!(itemContainer == null)) ? component.TransferItemTo(itemContainer) : component.DestroyItem())
			{
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
				m_NetView.RPC("RPC_Others_RemoveTileItem", NetTargets.Others, floorIndex, systemType, row, column);
			}
		}
	}

	[PunRPC]
	public void RPC_Others_RemoveTileItem(int floorIndex, TileSystem_Type systemType, int row, int column, PhotonMessageInfo info)
	{
		TileData tile = GetTile(floorIndex, systemType, row, column);
		if (tile != null && tile.gameObject != null)
		{
			DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
			if (component != null)
			{
				component.SetItem(-1);
				UpdateModifiedTiles(floorIndex, (int)systemType, row, column, tile.gameObject);
			}
		}
	}

	public void PlaceTunnelBrace(Floor floor, int row, int column)
	{
		if (floor.IsUnderGround())
		{
			m_NetView.RPC("RPC_Master_PlaceTunnelBrace", NetTargets.MasterClient, floor.m_FloorIndex, row, column);
		}
	}

	[PunRPC]
	public void RPC_Master_PlaceTunnelBrace(int floorIndex, int row, int column, PhotonMessageInfo info)
	{
		SpawnTunnelBrace(row, column, floorIndex);
	}

	private void SpawnTunnelBrace(int row, int column, int floor)
	{
		TunnelBrace tunnelBrace = InstantiateTunnelBrace(row, column, floor);
		if (tunnelBrace != null)
		{
			m_NetView.RPC("RPC_Others_SpawnTunnelBrace", NetTargets.Others, row, column, floor);
		}
	}

	[PunRPC]
	public void RPC_Others_SpawnTunnelBrace(int row, int column, int floor, PhotonMessageInfo info)
	{
		InstantiateTunnelBrace(row, column, floor);
	}

	private TunnelBrace InstantiateTunnelBrace(int row, int column, int floor)
	{
		if (GetTileCentrePosition(m_PrisonFloors[floor], TileSystem_Type.TileSystem_Wall, row, column, out var worldPosition))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_TunnelBracePrefab, worldPosition, Quaternion.identity);
			if (gameObject != null)
			{
				if (m_PrisonFloors[floor].m_FloorRootObject != null)
				{
					gameObject.transform.parent = m_PrisonFloors[floor].m_FloorRootObject;
				}
				TunnelBrace component = gameObject.GetComponent<TunnelBrace>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	public void AddTunnelBrace(TunnelBrace brace)
	{
		m_TunnelBraces.Add(brace);
	}

	public TunnelBrace GetTunnelBrace(int row, int column, int floor)
	{
		int count = m_TunnelBraces.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_TunnelBraces[i].TileRow == row && m_TunnelBraces[i].TileColumn == column && m_TunnelBraces[i].TileFloor == floor)
			{
				return m_TunnelBraces[i];
			}
		}
		return null;
	}

	public bool CheckForBracedOrPremadeTunelWithinNTiles(Floor floor, int row, int column, int N, bool bIncludeSelf = true)
	{
		if (!floor.IsUnderGround())
		{
			return false;
		}
		if (N <= 0)
		{
			return false;
		}
		int num = 2;
		int num2 = 1;
		int num3 = row;
		int num4 = column - N;
		int num5 = N * 2 + 1;
		while (num5 > 0)
		{
			for (int i = 0; i < num2; i++)
			{
				bool flag = num3 == row && num4 == column;
				if (bIncludeSelf || !flag)
				{
					TileData tile = GetTile(floor, TileSystem_Type.TileSystem_Wall, num3, num4);
					if (tile == null || (tile != null && !tile.HasGameObject))
					{
						return true;
					}
					if (IsBraced(floor, num3, num4))
					{
						return true;
					}
				}
				num3++;
			}
			num5--;
			if (num5 == N)
			{
				num = -2;
			}
			num2 += num;
			num4++;
			num3 = row - (num2 - 1) / 2;
		}
		return false;
	}

	private bool IsBraced(Floor floor, int row, int column)
	{
		if (!floor.IsUnderGround())
		{
			return false;
		}
		Floor floor2 = UpAFloor(floor);
		if (floor2 != floor)
		{
			Hole hole = GetHole(row + -1, column, floor2.m_FloorIndex);
			if (hole != null && hole.IsFullyDug())
			{
				return true;
			}
		}
		return GetTunnelBrace(row, column, floor.m_FloorIndex) != null;
	}

	private void UpdateModifiedTiles(int floorIndex, int systemIndex, int row, int column, GameObject tile)
	{
		if (tile == null || floorIndex < 0 || floorIndex >= 16 || systemIndex < 0 || systemIndex >= 6)
		{
			return;
		}
		if (m_ModifiedFloorTiles[floorIndex, systemIndex] == null)
		{
			m_ModifiedFloorTiles[floorIndex, systemIndex] = new List<TileInfo>();
		}
		List<TileInfo> list = m_ModifiedFloorTiles[floorIndex, systemIndex];
		int count = list.Count;
		bool flag = false;
		MonoBehaviour[] components = tile.GetComponents<MonoBehaviour>();
		if (components != null && components.Length > 0)
		{
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] is ISaveableTileComponent saveableTileComponent && saveableTileComponent.RequiresSaving())
				{
					flag = true;
					break;
				}
			}
		}
		int j;
		for (j = 0; j < count && !(list[j].m_GameObject == tile); j++)
		{
		}
		if (flag)
		{
			if (j == count)
			{
				list.Add(new TileInfo(row, column, tile));
			}
		}
		else if (j != count)
		{
			list.RemoveAt(j);
		}
	}

	public void UpdateModifiedDamagedTile(DamagableTile damagableTile)
	{
		if (damagableTile == null)
		{
			return;
		}
		Floor currentFloor = damagableTile.CurrentFloor;
		if (currentFloor != null)
		{
			TileSystem_Type systemIndex = TileSystem_Type.TileSystem_Wall;
			if (damagableTile.m_DamageAction == DamagableTile.DamageAction.Hole || damagableTile.m_DamageAction == DamagableTile.DamageAction.Unscrew)
			{
				systemIndex = TileSystem_Type.TileSystem_Ground;
			}
			UpdateModifiedTiles(currentFloor.m_FloorIndex, (int)systemIndex, damagableTile.TileRow, damagableTile.TileColumn, damagableTile.gameObject);
		}
	}

	public void DigHole(int row, int column, int fromFloor, int toFloor, int damagingItemID, float digAmount, bool bAllowReclaimItem, Character digger)
	{
		m_NetView.RPC("RPC_Master_DigHole", NetTargets.MasterClient, row, column, fromFloor, toFloor, damagingItemID, digAmount, bAllowReclaimItem, digger.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_Master_DigHole(int row, int column, int fromFloor, int toFloor, int damagingItemID, float digAmount, bool bAllowReclaimItem, int diggerID, PhotonMessageInfo info)
	{
		Character character = null;
		PhotonView photonView = PhotonView.Find(diggerID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		if (character == null)
		{
			return;
		}
		Hole hole = GetHole(row, column, fromFloor);
		if (hole == null)
		{
			hole = SpawnHole(row, column, fromFloor);
		}
		if (hole != null && hole.Dig(digAmount, bAllowReclaimItem, character))
		{
			int row2 = row + 1;
			if (toFloor >= 0 && toFloor < currentMaxFloor && CheckTileExists(m_PrisonFloors[toFloor], TileSystem_Type.TileSystem_Wall, row2, column))
			{
				float damageAmount = -1f;
				DoDamageToTile(m_PrisonFloors[toFloor], TileSystem_Type.TileSystem_Wall, row2, column, DamagableTile.DamageAction.Dig, damagingItemID, damageAmount, bAllowReclaimItem: false, character);
			}
		}
	}

	public void FillHole(Hole hole, float healthRestore, bool bFakeCover, Character character)
	{
		m_NetView.RPC("RPC_Master_FillHole", NetTargets.MasterClient, hole.TileRow, hole.TileColumn, hole.TileFloor, healthRestore, bFakeCover, character.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_Master_FillHole(int row, int column, int floor, float healthAmount, bool bFakeCover, int characterID, PhotonMessageInfo info)
	{
		Character character = null;
		PhotonView photonView = PhotonView.Find(characterID);
		if (photonView != null)
		{
			character = photonView.GetComponent<Character>();
		}
		if (!(character == null))
		{
			Hole hole = GetHole(row, column, floor);
			if (!(hole == null))
			{
				hole.Fill(healthAmount, bFakeCover);
			}
		}
	}

	public bool WouldFullyDigHole(int row, int column, int floor, float fDamageAmount)
	{
		if (fDamageAmount >= 100f)
		{
			return true;
		}
		Hole hole = GetInstance().GetHole(row, column, floor);
		if (hole != null)
		{
			return hole.WouldBeFullyDug(fDamageAmount);
		}
		return false;
	}

	private Hole SpawnHole(int row, int column, int floor)
	{
		int num = T17NetManager.AllocateSceneViewID();
		Hole hole = InstantiateHole(row, column, floor);
		if (hole != null)
		{
			hole.m_NetView.viewID = num;
			m_NetView.RPC("RPC_Others_SpawnHole", NetTargets.Others, row, column, floor, num);
			return hole;
		}
		return null;
	}

	[PunRPC]
	public void RPC_Others_SpawnHole(int row, int column, int floor, int viewID, PhotonMessageInfo info)
	{
		Hole hole = InstantiateHole(row, column, floor);
		if (hole != null)
		{
			hole.m_NetView.viewID = viewID;
		}
	}

	private Hole InstantiateHole(int row, int column, int floor)
	{
		if (GetTileCentrePosition(m_PrisonFloors[floor], TileSystem_Type.TileSystem_Ground, row, column, out var worldPosition))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_HolePrefab, worldPosition, Quaternion.identity);
			if (gameObject != null)
			{
				if (m_PrisonFloors[floor].m_FloorRootObject != null)
				{
					gameObject.transform.parent = m_PrisonFloors[floor].m_FloorRootObject;
				}
				Hole component = gameObject.GetComponent<Hole>();
				if (component != null)
				{
					return component;
				}
			}
		}
		return null;
	}

	public void AddHole(Hole hole)
	{
		m_Holes.Add(hole);
	}

	public void RemoveHole(Hole hole)
	{
		m_Holes.Remove(hole);
	}

	public Hole GetHole(int row, int column, int floor, bool floorCheck = true)
	{
		int count = m_Holes.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Holes[i].TileRow == row && m_Holes[i].TileColumn == column && (!floorCheck || m_Holes[i].TileFloor == floor))
			{
				return m_Holes[i];
			}
		}
		return null;
	}

	public List<Hole> GetSortedHolesWithinProximity(Vector3 position, int radius, bool includeFullyDug, bool includeAboveWalls)
	{
		m_LastHoleProximityCheck.Clear();
		float num = radius * radius;
		int count = m_Holes.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Holes[i] != null && (includeFullyDug || !m_Holes[i].IsFullyDug()) && (includeAboveWalls || !m_Holes[i].IsAboveWall()) && (m_Holes[i].transform.position - position).sqrMagnitude <= num)
			{
				m_LastHoleProximityCheck.Add(m_Holes[i]);
			}
		}
		if (m_LastHoleProximityCheck.Count > 0)
		{
			m_LastHoleProximityCheck.Sort(delegate(Hole h1, Hole h2)
			{
				float sqrMagnitude = (h1.transform.position - position).sqrMagnitude;
				float sqrMagnitude2 = (h2.transform.position - position).sqrMagnitude;
				if (sqrMagnitude > sqrMagnitude2)
				{
					return 1;
				}
				return (sqrMagnitude != sqrMagnitude2) ? (-1) : 0;
			});
		}
		return m_LastHoleProximityCheck;
	}

	public List<DamagableTile> AdditiveDamagedTilesSearchWithinProximity(Vector3 position, int radius, bool clearCachedList, DamagableTile.DamageAction? actionFilter)
	{
		if (clearCachedList)
		{
			m_LastDamagedTileProximityCheck.Clear();
		}
		SearchForDamagedTilesAroundPosition(position, radius, actionFilter);
		return m_LastDamagedTileProximityCheck;
	}

	private void SearchForDamagedTilesAroundPosition(Vector3 position, int radius, DamagableTile.DamageAction? actionFilter)
	{
		Floor floor = FindFloorAtZ(position.z);
		TileSystem_Type[] array = new TileSystem_Type[2]
		{
			TileSystem_Type.TileSystem_Wall,
			TileSystem_Type.TileSystem_Ground
		};
		if (!GetTileGridPoint(floor, TileSystem_Type.TileSystem_Wall, position, out var row, out var column))
		{
			return;
		}
		for (int i = row - radius; i < row + radius; i++)
		{
			for (int j = column - radius; j < column + radius; j++)
			{
				for (int k = 0; k < array.Length; k++)
				{
					TileData tile = GetTile(floor, array[k], i, j);
					if (tile != null && tile.gameObject != null && tile.gameObject.GetActive())
					{
						DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
						if (component != null && ((component.HasBeenDamaged() && !component.IsFullyDamaged()) || component.IsHoldingItem()) && (!actionFilter.HasValue || actionFilter.Value == component.m_DamageAction))
						{
							m_LastDamagedTileProximityCheck.Add(component);
						}
					}
				}
			}
		}
	}

	public void SortMultiFlooredDamagedTileListForPosition(List<DamagableTile> list, Vector3 position)
	{
		Floor originFloor = FindFloorAtZ(position.z);
		if (!GetTileGridPoint(originFloor, TileSystem_Type.TileSystem_Wall, position, out var row, out var column))
		{
			return;
		}
		Vector3 originTilePosition = new Vector3(row, column, originFloor.m_FloorIndex);
		list.Sort(delegate(DamagableTile t1, DamagableTile t2)
		{
			int tileRow = t1.TileRow;
			int tileRow2 = t2.TileRow;
			int tileColumn = t1.TileColumn;
			int tileColumn2 = t2.TileColumn;
			tileRow += t1.CurrentFloor.m_FloorIndex - originFloor.m_FloorIndex;
			tileRow2 += t2.CurrentFloor.m_FloorIndex - originFloor.m_FloorIndex;
			Vector3 position2 = t1.transform.position;
			Vector3 position3 = t2.transform.position;
			position2.y += -1f * (float)(t1.CurrentFloor.m_FloorIndex - originFloor.m_FloorIndex);
			position3.y += -1f * (float)(t2.CurrentFloor.m_FloorIndex - originFloor.m_FloorIndex);
			if (tileRow == tileRow2 && tileColumn == tileColumn2)
			{
				if (t1.CurrentFloor == originFloor && t2.CurrentFloor == originFloor)
				{
					return 0;
				}
				if (t1.CurrentFloor == originFloor)
				{
					return -1;
				}
				return 1;
			}
			Vector3 vector = new Vector3(tileRow, tileColumn, t1.CurrentFloor.m_FloorIndex);
			Vector3 vector2 = new Vector3(tileRow2, tileColumn2, t2.CurrentFloor.m_FloorIndex);
			float sqrMagnitude = (vector - originTilePosition).sqrMagnitude;
			float sqrMagnitude2 = (vector2 - originTilePosition).sqrMagnitude;
			if (sqrMagnitude > sqrMagnitude2)
			{
				return 1;
			}
			if (sqrMagnitude == sqrMagnitude2)
			{
				sqrMagnitude = (t1.transform.position - position).sqrMagnitude;
				sqrMagnitude2 = (t2.transform.position - position).sqrMagnitude;
				if (sqrMagnitude > sqrMagnitude2)
				{
					return 1;
				}
				if (sqrMagnitude == sqrMagnitude2)
				{
					return 0;
				}
			}
			return -1;
		});
	}

	private void SpawnRock(int row, int column, int floor)
	{
		Rock rock = InstantiateRock(row, column, floor);
		if (rock != null)
		{
			m_NetView.RPC("RPC_Others_SpawnRock", NetTargets.Others, row, column, floor);
		}
	}

	[PunRPC]
	public void RPC_Others_SpawnRock(int row, int column, int floor, PhotonMessageInfo info)
	{
		InstantiateRock(row, column, floor);
	}

	private Rock InstantiateRock(int row, int column, int floor)
	{
		if (GetTileCentrePosition(m_PrisonFloors[0], TileSystem_Type.TileSystem_Wall, row, column, out var worldPosition))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_RockPrefab, worldPosition, Quaternion.identity);
			if (gameObject != null)
			{
				if (m_PrisonFloors[floor].m_FloorRootObject != null)
				{
					gameObject.transform.parent = m_PrisonFloors[0].m_FloorRootObject;
				}
				Rock component = gameObject.GetComponent<Rock>();
				if (component != null)
				{
					component.RandomlySpawned = true;
					return component;
				}
			}
		}
		return null;
	}

	public void AddRock(Rock rock)
	{
		m_Rocks.Add(rock);
	}

	public Rock GetRock(int row, int column, int floor)
	{
		int count = m_Rocks.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Rocks[i].TileRow == row && m_Rocks[i].TileColumn == column && m_Rocks[i].TileFloor == floor)
			{
				return m_Rocks[i];
			}
		}
		return null;
	}

	public void AddVentCover(VentCover ventCover)
	{
		m_VentCovers.Add(ventCover);
	}

	public VentCover GetVentCover(int row, int column, int floor)
	{
		int count = m_VentCovers.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_VentCovers[i].TileRow == row && m_VentCovers[i].TileColumn == column && m_VentCovers[i].TileFloor == floor)
			{
				return m_VentCovers[i];
			}
		}
		return null;
	}

	public void AddStaticLadder(StaticLadder ladder)
	{
		m_StaticLadders.Add(ladder);
	}

	public StaticLadder GetStaticLadder(int row, int column, int floor)
	{
		int count = m_StaticLadders.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_StaticLadders[i].TileRow == row && m_StaticLadders[i].TileColumn == column && m_StaticLadders[i].TileFloor == floor)
			{
				return m_StaticLadders[i];
			}
		}
		return null;
	}

	public void Init()
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			GlobalStart.TimedNetworkService();
			DamagableTile[] componentsInChildren = m_PrisonFloors[i].m_FloorRootObject.GetComponentsInChildren<DamagableTile>();
			int num = ((componentsInChildren != null) ? componentsInChildren.Length : 0);
			for (int j = 0; j < num; j++)
			{
				float initialHealth = componentsInChildren[j].m_InitialHealth;
				if (initialHealth >= 0f)
				{
					componentsInChildren[j].SetHealth(initialHealth);
				}
			}
		}
		for (int k = 0; k < currentMaxFloor; k++)
		{
			MapTextureInfo component = m_PrisonFloors[k].m_FloorRootObject.GetComponent<MapTextureInfo>();
			if (component != null && !string.IsNullOrEmpty(component.m_MapTexturePath))
			{
				m_PrisonFloors[k].m_MapTexture = component.m_MapTexture;
			}
		}
		GlobalStart.TimedNetworkService();
	}

	public void AddPrisonFloor(int width, int height, FLOOR_TYPE floorType)
	{
		if (m_PrisonFloors == null || m_PrisonFloors.Length == 0)
		{
			m_PrisonFloors = new Floor[16];
		}
		if (currentMaxFloor < m_PrisonFloors.Length)
		{
			m_PrisonFloors[currentMaxFloor] = new Floor();
			m_PrisonFloors[currentMaxFloor].m_FloorType = floorType;
			if (currentMaxFloor > 0)
			{
				m_PrisonFloors[currentMaxFloor].m_zPos = m_PrisonFloors[currentMaxFloor - 1].m_zPos + m_FloorOffset;
			}
			else
			{
				m_PrisonFloors[currentMaxFloor].m_zPos = 0;
			}
			m_PrisonFloors[currentMaxFloor].m_FloorIndex = currentMaxFloor;
			if (currentMaxFloor == 0 || (currentMaxFloor == 1 && m_PrisonFloors[0].m_FloorType == FLOOR_TYPE.Floor_UnderGround))
			{
				m_PrisonFloors[currentMaxFloor].m_bIsStartFloor = true;
			}
			Vector3 vector = new Vector3(0f, 0f, m_PrisonFloors[currentMaxFloor].m_zPos);
			string floorName = "Floor" + currentMaxFloor;
			switch (floorType)
			{
			case FLOOR_TYPE.Floor_Vent:
				floorName = "Vent" + currentMaxFloor;
				break;
			case FLOOR_TYPE.Floor_Roof:
				floorName = "Roof" + currentMaxFloor;
				break;
			}
			m_PrisonFloors[currentMaxFloor].m_FloorRootObject = new GameObject().transform;
			m_PrisonFloors[currentMaxFloor].m_FloorRootObject.gameObject.name = floorName;
			m_PrisonFloors[currentMaxFloor].m_FloorName = floorName;
			m_PrisonFloors[currentMaxFloor].m_FloorRootObject.position = vector;
			GameObject gameObject = new GameObject();
			gameObject.name = "Objects";
			gameObject.transform.parent = m_PrisonFloors[currentMaxFloor].m_FloorRootObject;
			gameObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "Building";
			gameObject2.transform.parent = m_PrisonFloors[currentMaxFloor].m_FloorRootObject;
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			AddPathfindingForFloor(floorName, width, height, vector, floorType, gameObject2.transform);
			AddTileSystemsForFloor(m_PrisonFloors[currentMaxFloor], floorName, width, height, vector, gameObject2.transform);
			currentMaxFloor++;
		}
	}

	public void AddUnderGroundFloor(int width, int height)
	{
		if (m_PrisonFloors == null || m_PrisonFloors.Length == 0)
		{
			m_PrisonFloors = new Floor[16];
		}
		if ((m_PrisonFloors.Length <= 0 || m_PrisonFloors[0].m_FloorType != FLOOR_TYPE.Floor_UnderGround) && currentMaxFloor < m_PrisonFloors.Length)
		{
			currentMaxFloor++;
			for (int num = currentMaxFloor; num > 0; num--)
			{
				m_PrisonFloors[num] = m_PrisonFloors[num - 1];
			}
			m_PrisonFloors[0] = new Floor();
			m_PrisonFloors[0].m_FloorType = FLOOR_TYPE.Floor_UnderGround;
			m_PrisonFloors[0].m_FloorIndex = 0;
			m_PrisonFloors[0].m_bIsStartFloor = false;
			m_PrisonFloors[0].m_zPos = ((currentMaxFloor > 1) ? (m_PrisonFloors[1].m_zPos - m_FloorOffset) : 0);
			string floorName = "Underground";
			Vector3 vector = new Vector3(0f, 0f, m_PrisonFloors[0].m_zPos);
			m_PrisonFloors[0].m_FloorRootObject = new GameObject().transform;
			m_PrisonFloors[0].m_FloorRootObject.gameObject.name = floorName;
			m_PrisonFloors[0].m_FloorName = floorName;
			m_PrisonFloors[0].m_FloorRootObject.position = vector;
			GameObject gameObject = new GameObject();
			gameObject.name = "Objects";
			gameObject.transform.parent = m_PrisonFloors[0].m_FloorRootObject;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			GameObject gameObject2 = new GameObject();
			gameObject2.name = "Building";
			gameObject2.transform.parent = m_PrisonFloors[0].m_FloorRootObject;
			gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
			AddPathfindingForFloor(floorName, width, height, vector, FLOOR_TYPE.Floor_UnderGround, m_PrisonFloors[0].m_FloorRootObject);
			AddTileSystemsForFloor(m_PrisonFloors[0], floorName, width, height, vector, gameObject2.transform);
		}
	}

	public void RemoveFloor(int index)
	{
		for (int i = index; i < currentMaxFloor - 1; i++)
		{
			MoveFloorUp(i);
		}
		if (m_PrisonFloors[currentMaxFloor - 1] != null && m_PrisonFloors[currentMaxFloor - 1].m_FloorRootObject != null)
		{
			RemovePathfindingForFloor(m_PrisonFloors[currentMaxFloor - 1].m_FloorName);
			GameObject gameObject = m_PrisonFloors[currentMaxFloor - 1].m_FloorRootObject.gameObject;
			m_PrisonFloors[currentMaxFloor - 1].m_FloorRootObject = null;
			m_PrisonFloors[currentMaxFloor - 1] = null;
			currentMaxFloor--;
		}
	}

	public void MoveFloorUp(int index)
	{
		if (index < currentMaxFloor - 1 && m_PrisonFloors[index].m_FloorType != FLOOR_TYPE.Floor_UnderGround)
		{
			int zPos = m_PrisonFloors[index].m_zPos;
			m_PrisonFloors[index].m_zPos = m_PrisonFloors[index + 1].m_zPos;
			m_PrisonFloors[index + 1].m_zPos = zPos;
			Floor floor = m_PrisonFloors[index];
			m_PrisonFloors[index] = m_PrisonFloors[index + 1];
			m_PrisonFloors[index + 1] = floor;
			m_PrisonFloors[index].m_FloorRootObject.position = new Vector3(m_PrisonFloors[index].m_FloorRootObject.position.x, m_PrisonFloors[index].m_FloorRootObject.position.y, m_PrisonFloors[index].m_zPos);
			m_PrisonFloors[index + 1].m_FloorRootObject.position = new Vector3(m_PrisonFloors[index + 1].m_FloorRootObject.position.x, m_PrisonFloors[index + 1].m_FloorRootObject.position.y, m_PrisonFloors[index + 1].m_zPos);
			UpdatePathfindingPositionForFloor(m_PrisonFloors[index].m_FloorName, m_PrisonFloors[index].m_FloorRootObject.position);
			UpdatePathfindingPositionForFloor(m_PrisonFloors[index + 1].m_FloorName, m_PrisonFloors[index + 1].m_FloorRootObject.position);
		}
	}

	public void MoveFloorDown(int index)
	{
		if (index - 1 > 0)
		{
			int zPos = m_PrisonFloors[index].m_zPos;
			m_PrisonFloors[index].m_zPos = m_PrisonFloors[index - 1].m_zPos;
			m_PrisonFloors[index - 1].m_zPos = zPos;
			Floor floor = m_PrisonFloors[index];
			m_PrisonFloors[index] = m_PrisonFloors[index - 1];
			m_PrisonFloors[index - 1] = floor;
			m_PrisonFloors[index].m_FloorRootObject.position = new Vector3(m_PrisonFloors[index].m_FloorRootObject.position.x, m_PrisonFloors[index].m_FloorRootObject.position.y, m_PrisonFloors[index].m_zPos);
			m_PrisonFloors[index - 1].m_FloorRootObject.position = new Vector3(m_PrisonFloors[index - 1].m_FloorRootObject.position.x, m_PrisonFloors[index - 1].m_FloorRootObject.position.y, m_PrisonFloors[index - 1].m_zPos);
			UpdatePathfindingPositionForFloor(m_PrisonFloors[index].m_FloorName, m_PrisonFloors[index].m_FloorRootObject.position);
			UpdatePathfindingPositionForFloor(m_PrisonFloors[index - 1].m_FloorName, m_PrisonFloors[index - 1].m_FloorRootObject.position);
		}
	}

	public void PushObjectIntoFloor(int index, Transform tran)
	{
		Transform transform = m_PrisonFloors[index].m_FloorRootObject.FindChild("Objects");
		if (transform != null)
		{
			tran.parent = transform;
			tran.position = new Vector3(tran.position.x, tran.position.y, transform.position.z);
		}
	}

	public int GetCurrentMaxFloor()
	{
		return currentMaxFloor;
	}

	public void ResetCurrentMaxFloor()
	{
		while (currentMaxFloor != m_PrisonFloors.Length && m_PrisonFloors[currentMaxFloor].m_FloorRootObject != null)
		{
			currentMaxFloor++;
		}
	}

	public static NavGraph GetAStarGridForFloor(string floorName, AstarPath astar)
	{
		if (astar == null)
		{
			return null;
		}
		astar.astarData.FindGraphTypes();
		NavGraph result = null;
		if (astar.astarData.graphs != null)
		{
			for (int i = 0; i < astar.astarData.graphs.Length; i++)
			{
				NavGraph navGraph = astar.astarData.graphs[i];
				if (navGraph != null && navGraph.name == floorName)
				{
					result = navGraph;
					break;
				}
			}
		}
		return result;
	}

	public void AddPathfindingForFloor(string floorName, int width, int height, Vector3 pos, FLOOR_TYPE floorType, Transform parent)
	{
		GameObject gameObject = GameObject.Find("A*");
		AstarPath component = gameObject.GetComponent<AstarPath>();
		string[] tagNames = new string[11]
		{
			"Prison", "Vent", "Underground", "Black", "Cyan", "Red", "Green", "Yellow", "Purple", "Silver",
			"Solitary"
		};
		component.SetTagNames(tagNames);
		NavGraph navGraph = GetAStarGridForFloor(floorName, component);
		if (navGraph == null)
		{
			navGraph = component.astarData.AddGraph(typeof(GridGraph));
			navGraph.name = floorName;
		}
		GridGraph gridGraph = (GridGraph)navGraph;
		gridGraph.Width = width;
		gridGraph.Depth = height;
		gridGraph.center = pos;
		gridGraph.rotation = new Vector3(-90f, 0f, 0f);
		gridGraph.neighbours = NumNeighbours.Eight;
		gridGraph.cutCorners = false;
		gridGraph.maxClimb = 0.4f;
		gridGraph.maxClimbAxis = 2;
		gridGraph.collision.collisionCheck = true;
		gridGraph.collision.type = Pathfinding.ColliderType.Ray;
		gridGraph.collision.height = 1.5f;
		gridGraph.collision.collisionOffset = 0f;
		string[] layerNames = new string[3] { "StaticMapObject", "Wall", "Fence" };
		gridGraph.collision.mask = LayerMask.GetMask(layerNames);
		gridGraph.collision.heightCheck = true;
		string[] layerNames2 = new string[1] { "Floor" };
		gridGraph.collision.heightMask = LayerMask.GetMask(layerNames2);
		gridGraph.collision.fromHeight = 1.1f;
		gridGraph.collision.thickRaycast = false;
		gridGraph.collision.unwalkableWhenNoGround = true;
		gridGraph.UpdateSizeFromWidthDepth();
		component.Scan();
	}

	private void RemovePathfindingForFloor(string floorName)
	{
		GameObject gameObject = GameObject.Find("A*");
		AstarPath component = gameObject.GetComponent<AstarPath>();
		component.astarData.FindGraphTypes();
		GridGraph gridGraph = null;
		IEnumerable enumerable = component.astarData.FindGraphsOfType(typeof(GridGraph));
		foreach (GridGraph item in enumerable)
		{
			if (item.name == floorName)
			{
				gridGraph = item;
			}
		}
		if (gridGraph != null)
		{
			component.astarData.RemoveGraph(gridGraph);
		}
	}

	private void UpdatePathfindingPositionForFloor(string floorName, Vector3 pos)
	{
		GameObject gameObject = GameObject.Find("A*");
		AstarPath component = gameObject.GetComponent<AstarPath>();
		component.astarData.FindGraphTypes();
		GridGraph gridGraph = null;
		IEnumerable enumerable = component.astarData.FindGraphsOfType(typeof(GridGraph));
		foreach (GridGraph item in enumerable)
		{
			if (item.name == floorName)
			{
				gridGraph = item;
			}
		}
		if (gridGraph != null)
		{
			gridGraph.center = pos;
			gridGraph.UpdateSizeFromWidthDepth();
		}
	}

	public void AddTileSystemsForFloor(Floor floor, string floorName, int width, int height, Vector3 pos, Transform parentTran)
	{
		int num = Enum.GetNames(typeof(TileSystem_Type)).Length;
		floor.m_TileSystems = new TileSystem[num];
		int num2 = -width / 2;
		float x = num2;
		int num3 = height / 2;
		float y = num3;
		floor.m_TileSystems[0] = CreateTileSystem(parentTran, floorName + "_GroundTiles", "GroundTiles", width, height, new Vector3(x, y, 0f));
		floor.m_TileSystems[2] = CreateTileSystem(parentTran, floorName + "_GroundPlops", "Untagged", width, height, new Vector3(x, y, -0.005f));
		floor.m_TileSystems[3] = CreateTileSystem(parentTran, floorName + "_ObjectPlops", "Untagged", width, height, new Vector3(x, y, -0.05f));
		floor.m_TileSystems[1] = CreateTileSystem(parentTran, floorName + "_WallTiles", "WallTiles", width, height, new Vector3(x, y, -0.025f));
		floor.m_TileSystems[5] = CreateTileSystem(parentTran, floorName + "_WallPlops", "Untagged", width, height, new Vector3(x, y, -0.0251f));
		floor.m_TileSystems[4] = CreateTileSystem(parentTran, floorName + "_Lights", "Untagged", width, height, new Vector3(x, y, 0f));
	}

	private TileSystem CreateTileSystem(Transform parentTran, string tilesysName, string tilesysTag, int width, int height, Vector3 pos)
	{
		TileSystem[] componentsInChildren = parentTran.GetComponentsInChildren<TileSystem>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].name == tilesysName)
			{
				componentsInChildren[i].transform.localPosition = pos;
				return componentsInChildren[i];
			}
			if (componentsInChildren[i].name == "GroundTiles" && componentsInChildren[i].CompareTag(tilesysTag))
			{
				componentsInChildren[i].name = tilesysName;
				componentsInChildren[i].transform.localPosition = pos;
				return componentsInChildren[i];
			}
			if (componentsInChildren[i].name == "WallTiles" && componentsInChildren[i].CompareTag(tilesysTag))
			{
				componentsInChildren[i].name = tilesysName;
				componentsInChildren[i].transform.localPosition = pos;
				return componentsInChildren[i];
			}
		}
		GameObject gameObject = new GameObject();
		TileSystem tileSystem = gameObject.AddComponent<TileSystem>();
		gameObject.transform.parent = parentTran;
		tileSystem.CreateSystem(1f, 1f, 1f, height, width);
		gameObject.transform.localPosition = pos;
		gameObject.tag = tilesysTag;
		gameObject.name = tilesysName;
		return tileSystem;
	}

	public string[] GetNameList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < currentMaxFloor; i++)
		{
			list.Add(m_PrisonFloors[i].m_FloorName);
		}
		return list.ToArray();
	}

	public void ShowOnlyThisFloor(string floorName)
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			if (floorName == m_PrisonFloors[i].m_FloorName)
			{
				m_PrisonFloors[i].m_FloorRootObject.gameObject.SetActive(value: true);
			}
			else
			{
				m_PrisonFloors[i].m_FloorRootObject.gameObject.SetActive(value: false);
			}
		}
	}

	public void ShowAllNonVentFloors()
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			if (!m_PrisonFloors[i].IsVent())
			{
				m_PrisonFloors[i].m_FloorRootObject.gameObject.SetActive(value: true);
			}
			else
			{
				m_PrisonFloors[i].m_FloorRootObject.gameObject.SetActive(value: false);
			}
		}
	}

	public Floor[] GetFloors()
	{
		List<Floor> list = new List<Floor>();
		for (int i = 0; i < currentMaxFloor; i++)
		{
			list.Add(m_PrisonFloors[i]);
		}
		return list.ToArray();
	}

	public bool GetFloorMetrics(int iFloor, out int iWidth, out int iHeight)
	{
		if (iFloor >= currentMaxFloor)
		{
			iWidth = 120;
			iHeight = 120;
			return false;
		}
		TileSystem tileSystem = GetTileSystem(iFloor, TileSystem_Type.TileSystem_Ground);
		if (tileSystem == null)
		{
			iWidth = 120;
			iHeight = 120;
			return false;
		}
		iWidth = tileSystem.ColumnCount;
		iHeight = tileSystem.RowCount;
		return true;
	}

	public bool GetFloorMetricsBitLength(int iFloor, int iMaxbits, out int uXBitLength, out int uYBitLength)
	{
		uXBitLength = 1;
		uYBitLength = 1;
		if (GetFloorMetrics(iFloor, out var iWidth, out var iHeight))
		{
			while ((iWidth >>= 1) != 0)
			{
				uXBitLength++;
			}
			while ((iHeight >>= 1) != 0)
			{
				uYBitLength++;
			}
			if (uXBitLength + uYBitLength > iMaxbits)
			{
				uYBitLength = iMaxbits / 2;
				uXBitLength = iMaxbits - uYBitLength;
			}
			return true;
		}
		return false;
	}

	public void FixMissingRefs()
	{
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			if (rootGameObjects == null)
			{
				continue;
			}
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				string text = rootGameObjects[j].name;
				if (text.StartsWith("Floor") || text.StartsWith("Underground") || text.StartsWith("Vent") || text.StartsWith("Roof"))
				{
					Floor floor = FindFloorByName(text);
					if (floor != null)
					{
						floor.m_FloorRootObject = rootGameObjects[j].transform;
						TileSystem componentInChildren = floor.m_FloorRootObject.GetComponentInChildren<TileSystem>();
						int columnCount = componentInChildren.ColumnCount;
						int rowCount = componentInChildren.RowCount;
						AddTileSystemsForFloor(pos: new Vector3(0f, 0f, floor.m_zPos), floor: floor, floorName: floor.m_FloorName, width: columnCount, height: rowCount, parentTran: componentInChildren.transform.parent);
					}
				}
			}
		}
	}

	public void EnableCollidersForUndergroundCaverns()
	{
		if (m_PrisonFloors[0].m_FloorType != FLOOR_TYPE.Floor_UnderGround)
		{
			return;
		}
		GetTileSystemBounds(m_PrisonFloors[0], TileSystem_Type.TileSystem_Wall, out var maxRows, out var maxColumns);
		for (int i = 0; i < maxRows; i++)
		{
			for (int j = 0; j < maxColumns; j++)
			{
				CheckTileForNearbyCavern(i, j);
			}
		}
	}

	private void CheckTileForNearbyCavern(int startRow, int startColumn)
	{
		Pair<int>[] array = new Pair<int>[8]
		{
			new Pair<int>(startRow - 1, startColumn),
			new Pair<int>(startRow + 1, startColumn),
			new Pair<int>(startRow, startColumn - 1),
			new Pair<int>(startRow, startColumn + 1),
			new Pair<int>(startRow - 1, startColumn - 1),
			new Pair<int>(startRow + 1, startColumn - 1),
			new Pair<int>(startRow - 1, startColumn + 1),
			new Pair<int>(startRow + 1, startColumn + 1)
		};
		for (int i = 0; i < array.Length; i++)
		{
			Pair<int> pair = array[i];
			TileData tile = GetTile(0, TileSystem_Type.TileSystem_Wall, pair.ValueOne, pair.ValueTwo);
			if (tile == null)
			{
				SetupDamagableTileForNearbyCavern(startRow, startColumn);
				break;
			}
		}
	}

	private void SetupDamagableTileForNearbyCavern(int row, int column)
	{
		TileData tile = GetTile(0, TileSystem_Type.TileSystem_Wall, row, column);
		if (tile == null || !(tile.gameObject != null))
		{
			return;
		}
		DamagableTile component = tile.gameObject.GetComponent<DamagableTile>();
		if (component != null)
		{
			Collider collider = component.GetCollider();
			if (!component.IsFullyDamaged() && collider != null && !collider.enabled)
			{
				collider.enabled = true;
				component.UpdateAppearance();
			}
		}
	}

	[ContextMenu("Delete Floor Collision")]
	public void DeleteFloorCollision()
	{
		for (int i = 0; i < currentMaxFloor; i++)
		{
			GetTileSystemBounds(m_PrisonFloors[i], TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
			GlobalStart.TimedNetworkService();
			for (int j = 0; j < maxRows; j++)
			{
				for (int k = 0; k < maxColumns; k++)
				{
					TileData tile = GetTile(m_PrisonFloors[i], TileSystem_Type.TileSystem_Ground, j, k);
					if (tile != null && tile.gameObject != null)
					{
						Collider component = tile.gameObject.GetComponent<Collider>();
						UnityEngine.Object.Destroy(component);
					}
				}
			}
		}
	}

	public void TurnOffUndergroundLayer()
	{
		m_PrisonFloors[0].m_FloorRootObject.gameObject.SetActive(value: false);
	}

	private static short SerializeSaveDataCollection(StreamBuffer outStream, object customobject)
	{
		int targetOffset = 0;
		SaveDataCollection saveDataCollection = (SaveDataCollection)customobject;
		lock (memSaveData)
		{
			byte[] bytes = memSaveData;
			List<FloorSaveData> floors = saveDataCollection.Floors;
			Protocol.Serialize(floors.Count, memSaveData, ref targetOffset);
			for (int i = 0; i < floors.Count; i++)
			{
				FloorSaveData floorSaveData = floors[i];
				Protocol.Serialize(floorSaveData.i, memSaveData, ref targetOffset);
				List<TileSystemSaveData> v = floorSaveData.v;
				Protocol.Serialize(v.Count, memSaveData, ref targetOffset);
				for (int j = 0; j < v.Count; j++)
				{
					TileSystemSaveData tileSystemSaveData = v[j];
					Protocol.Serialize(tileSystemSaveData.t, memSaveData, ref targetOffset);
					List<TileSaveData> v2 = tileSystemSaveData.v;
					Protocol.Serialize(v2.Count, memSaveData, ref targetOffset);
					for (int k = 0; k < v2.Count; k++)
					{
						TileSaveData tileSaveData = v2[k];
						Protocol.Serialize(tileSaveData.row, memSaveData, ref targetOffset);
						Protocol.Serialize(tileSaveData.column, memSaveData, ref targetOffset);
						List<SavableTileComponent> v3 = tileSaveData.v;
						Protocol.Serialize(v3.Count, memSaveData, ref targetOffset);
						for (int l = 0; l < v3.Count; l++)
						{
							SavableTileComponent savableTileComponent = v3[l];
							byte[] array = Protocol.Serialize(savableTileComponent.t);
							Protocol.Serialize(array.Length, memSaveData, ref targetOffset);
							if (array.Length > 0)
							{
								Array.Copy(array, 0, bytes, targetOffset, array.Length);
								targetOffset += array.Length;
							}
							byte[] array2 = Protocol.Serialize(savableTileComponent.d);
							Protocol.Serialize(array2.Length, memSaveData, ref targetOffset);
							if (array2.Length > 0)
							{
								Array.Copy(array2, 0, bytes, targetOffset, array2.Length);
								targetOffset += array2.Length;
							}
						}
					}
				}
			}
			SerializeObjectSaveData(ref saveDataCollection.Holes, ref bytes, ref targetOffset);
			SerializeObjectSaveData(ref saveDataCollection.Rocks, ref bytes, ref targetOffset);
			SerializeObjectSaveData(ref saveDataCollection.Braces, ref bytes, ref targetOffset);
			outStream.Write(bytes, 0, targetOffset);
		}
		return (short)targetOffset;
	}

	private static void SerializeObjectSaveData(ref List<ObjectSaveData> objects, ref byte[] bytes, ref int index)
	{
		Protocol.Serialize(objects.Count, memSaveData, ref index);
		for (int i = 0; i < objects.Count; i++)
		{
			ObjectSaveData objectSaveData = objects[i];
			Protocol.Serialize(objectSaveData.row, memSaveData, ref index);
			Protocol.Serialize(objectSaveData.column, memSaveData, ref index);
			Protocol.Serialize(objectSaveData.floor, memSaveData, ref index);
			Protocol.Serialize(objectSaveData.id, memSaveData, ref index);
			byte[] array = Protocol.Serialize(objectSaveData.d);
			Protocol.Serialize(array.Length, memSaveData, ref index);
			if (array.Length > 0)
			{
				Array.Copy(array, 0, bytes, index, array.Length);
				index += array.Length;
			}
		}
	}

	private static object DeserializeSaveDataCollection(StreamBuffer inStream, short length)
	{
		int offset = 0;
		SaveDataCollection saveDataCollection = new SaveDataCollection();
		lock (memSaveData)
		{
			inStream.Read(memSaveData, 0, length);
			byte[] bytes = memSaveData;
			int value = 0;
			Protocol.Deserialize(out value, memSaveData, ref offset);
			for (int i = 0; i < value; i++)
			{
				FloorSaveData floorSaveData = new FloorSaveData();
				saveDataCollection.Floors.Add(floorSaveData);
				Protocol.Deserialize(out floorSaveData.i, memSaveData, ref offset);
				int value2 = 0;
				Protocol.Deserialize(out value2, memSaveData, ref offset);
				for (int j = 0; j < value2; j++)
				{
					TileSystemSaveData tileSystemSaveData = new TileSystemSaveData();
					floorSaveData.v.Add(tileSystemSaveData);
					Protocol.Deserialize(out tileSystemSaveData.t, memSaveData, ref offset);
					int value3 = 0;
					Protocol.Deserialize(out value3, memSaveData, ref offset);
					for (int k = 0; k < value3; k++)
					{
						int value4 = 0;
						int value5 = 0;
						Protocol.Deserialize(out value4, memSaveData, ref offset);
						Protocol.Deserialize(out value5, memSaveData, ref offset);
						TileSaveData tileSaveData = new TileSaveData(value4, value5);
						tileSystemSaveData.v.Add(tileSaveData);
						int value6 = 0;
						Protocol.Deserialize(out value6, memSaveData, ref offset);
						for (int l = 0; l < value6; l++)
						{
							int value7 = 0;
							int value8 = 0;
							string componentType = string.Empty;
							string data = string.Empty;
							Protocol.Deserialize(out value7, memSaveData, ref offset);
							if (value7 > 0)
							{
								byte[] array = new byte[value7];
								Array.Copy(memSaveData, offset, array, 0, value7);
								componentType = (string)Protocol.Deserialize(array);
								offset += value7;
							}
							Protocol.Deserialize(out value8, memSaveData, ref offset);
							if (value8 > 0)
							{
								byte[] array2 = new byte[value8];
								Array.Copy(memSaveData, offset, array2, 0, value8);
								data = (string)Protocol.Deserialize(array2);
								offset += value8;
							}
							SavableTileComponent item = new SavableTileComponent(componentType, data);
							tileSaveData.v.Add(item);
						}
					}
				}
			}
			DeserializeObjectSaveData(ref saveDataCollection.Holes, ref bytes, ref offset);
			DeserializeObjectSaveData(ref saveDataCollection.Rocks, ref bytes, ref offset);
			DeserializeObjectSaveData(ref saveDataCollection.Braces, ref bytes, ref offset);
			return saveDataCollection;
		}
	}

	private static void DeserializeObjectSaveData(ref List<ObjectSaveData> objects, ref byte[] bytes, ref int index)
	{
		int value = 0;
		Protocol.Deserialize(out value, memSaveData, ref index);
		for (int i = 0; i < value; i++)
		{
			Protocol.Deserialize(out int value2, memSaveData, ref index);
			Protocol.Deserialize(out int value3, memSaveData, ref index);
			Protocol.Deserialize(out int value4, memSaveData, ref index);
			Protocol.Deserialize(out int value5, memSaveData, ref index);
			string data = string.Empty;
			Protocol.Deserialize(out int value6, memSaveData, ref index);
			if (value6 > 0)
			{
				byte[] array = new byte[value6];
				Array.Copy(memSaveData, index, array, 0, value6);
				data = (string)Protocol.Deserialize(array);
				index += value6;
			}
			ObjectSaveData item = new ObjectSaveData(value2, value3, value4, value5, data);
			objects.Add(item);
		}
	}

	public bool GetGroundTileBounds(ref Vector3 centrePosition, ref Vector2 dimensions)
	{
		for (int i = 0; i < m_PrisonFloors.Length; i++)
		{
			Floor floor = m_PrisonFloors[i];
			if (floor == null)
			{
				continue;
			}
			for (int j = 0; j < floor.m_TileSystems.Length; j++)
			{
				TileSystem tileSystem = floor.m_TileSystems[j];
				if (tileSystem != null)
				{
					centrePosition = floor.m_FloorRootObject.transform.position;
					centrePosition.z = 0f;
					dimensions.Set((float)tileSystem.ColumnCount * tileSystem.CellSize.x, (float)tileSystem.RowCount * tileSystem.CellSize.y);
					return true;
				}
			}
		}
		centrePosition = Vector3.zero;
		dimensions = Vector2.zero;
		return false;
	}

	public bool GetGroundTileExtents(ref Vector3 topLeft, ref Vector3 bottomRight)
	{
		Vector3 centrePosition = Vector3.zero;
		Vector2 dimensions = Vector2.zero;
		if (GetGroundTileBounds(ref centrePosition, ref dimensions))
		{
			Vector3 vector = new Vector3(dimensions.x / 2f, dimensions.y / 2f, 0f);
			topLeft.Set(centrePosition.x - vector.x, centrePosition.y + vector.y, centrePosition.z);
			bottomRight.Set(centrePosition.x + vector.x, centrePosition.y - vector.y, centrePosition.z);
			return true;
		}
		topLeft = Vector3.zero;
		bottomRight = Vector3.zero;
		return false;
	}
}
