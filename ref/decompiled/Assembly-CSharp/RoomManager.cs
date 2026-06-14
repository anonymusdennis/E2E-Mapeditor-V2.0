using System;
using System.Collections.Generic;
using NetworkLoadable;
using UnityEngine;

public class RoomManager : T17MonoBehaviour, INetworkLoadable, IDeserializable, Saveable
{
	[Serializable]
	private class SaveData
	{
		public int[] v;

		public int[] s;
	}

	public List<RoomFloor> m_Floors = new List<RoomFloor>();

	public RoomUtility roomUtil;

	public int nextRoomID = 1;

	public int defaultWidth = 121;

	public int defaultHeight = 121;

	public bool m_AllowBuildingBoundariesToGenerateMeshes;

	[ReadOnly]
	public int[] m_iInmateSafeSpaceStartIndex;

	[ReadOnly]
	public int[] m_iInmateSafeSpaceEndIndex;

	[ReadOnly]
	public int[] m_iGuardSafeSpaceStartIndex;

	[ReadOnly]
	public int[] m_iGuardSafeSpaceEndIndex;

	[ReadOnly]
	public int[] m_iSupportSafeSpaceStartIndex;

	[ReadOnly]
	public int[] m_iSupportSafeSpaceEndIndex;

	private static List<Character> m_CharactersForRoomAssignment = new List<Character>(32);

	private List<int> m_SpawnPointsKeyForRandom = new List<int>();

	private Dictionary<int, SpawnPoint> m_InmateSpawnPoints = new Dictionary<int, SpawnPoint>(32);

	private Dictionary<int, SpawnPoint> m_SpawnPointForCharacter = new Dictionary<int, SpawnPoint>(32);

	private T17NetView m_Netview;

	private List<ItemContainer> contrabandDesks = new List<ItemContainer>();

	private SaveDataRegister m_SaveData;

	private static RoomManager m_Instance = null;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public static RoomManager GetInstance()
	{
		return m_Instance;
	}

	public static void SetToolsInstance(RoomManager instance)
	{
		m_Instance = instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	private void Start()
	{
		m_Netview = GetComponent<T17NetView>();
		m_Netview.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.RoomManager);
		m_SaveData = new SaveDataRegister(this, m_Netview.viewID, bIsMajorManagerComponent: true, 5);
	}

	protected virtual void OnDestroy()
	{
		NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		if (m_Instance == this)
		{
			m_Instance = null;
		}
		m_Netview = null;
		roomUtil = null;
	}

	public void Init()
	{
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			LoadFloors(instance.GetFloors());
			RoomManager instance2 = GetInstance();
			if (instance2 != null)
			{
				List<RoomBlob> allRoomsByLocation = instance2.GetAllRoomsByLocation(RoomBlob.eLocation.InmateCell);
				for (int i = 0; i < allRoomsByLocation.Count; i++)
				{
					RoomBlob roomBlob = allRoomsByLocation[i];
					if (roomBlob != null)
					{
						RoomBlob_Cell roomBlobData = allRoomsByLocation[i].GetRoomBlobData<RoomBlob_Cell>();
						if (!(roomBlobData != null) || roomBlobData.m_SpawnPoints == null)
						{
							continue;
						}
						for (int j = 0; j < roomBlobData.m_SpawnPoints.Count; j++)
						{
							SpawnPoint spawnPoint = roomBlobData.m_SpawnPoints[j];
							if (spawnPoint != null)
							{
								m_InmateSpawnPoints.Add(spawnPoint.m_SpawnPointID, spawnPoint);
								spawnPoint.SetRoomblobIBelongTo(roomBlob);
								m_SpawnPointsKeyForRandom.Add(spawnPoint.m_SpawnPointID);
							}
						}
					}
					else
					{
						Debug.LogError("ERROR: RoomBlob " + i + " is null!");
					}
				}
			}
			else
			{
				Debug.LogError("ERROR: RoomManager is null!");
			}
		}
		else
		{
			Debug.LogError("ERROR: FloorManager is NULL");
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (T17NetManager.IsMasterClient && !PrisonSnapshotIO.IsThereSaveData())
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (!allPlayers[i].IsInited())
				{
					return T17BehaviourManager.INITSTATE.IS_DEPS;
				}
			}
			for (int j = 0; j < m_CharactersForRoomAssignment.Count; j++)
			{
				if (!m_CharactersForRoomAssignment[j].IsInited())
				{
					return T17BehaviourManager.INITSTATE.IS_DEPS;
				}
			}
			for (int k = 0; k < allPlayers.Count; k++)
			{
				if (allPlayers[k] != null)
				{
					AssignSpawnPointForPlayer(allPlayers[k].m_NetView.viewID);
				}
			}
			AssignNonPlayerCharactersToRoomSpawnPoints();
		}
		return base.StartInit();
	}

	public static List<ItemContainer> GetContrabandDesks()
	{
		if (m_Instance == null)
		{
			throw new Exception("Room Manager: Trying to use GetContrabandDesks() function when Room Manager is null");
		}
		if (m_Instance.contrabandDesks != null && m_Instance.contrabandDesks.Count > 0)
		{
			return m_Instance.contrabandDesks;
		}
		List<RoomBlob> allRoomsByLocation = m_Instance.GetAllRoomsByLocation(RoomBlob.eLocation.ContrabandRoom);
		m_Instance.contrabandDesks = new List<ItemContainer>();
		if (allRoomsByLocation == null)
		{
			return m_Instance.contrabandDesks;
		}
		for (int i = 0; i < allRoomsByLocation.Count; i++)
		{
			RoomBlob roomBlob = allRoomsByLocation[i];
			if (!(roomBlob == null))
			{
				RoomBlob_ContrabandRoom roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_ContrabandRoom>();
				DeskInteraction deskInteraction = roomBlobData.m_Desk as DeskInteraction;
				if (!(roomBlobData == null) && !(deskInteraction == null) && !(deskInteraction.m_LinkedItemContainer == null))
				{
					m_Instance.contrabandDesks.Add(deskInteraction.m_LinkedItemContainer);
				}
			}
		}
		return m_Instance.contrabandDesks;
	}

	public void LoadFloors(FloorManager.Floor[] floors)
	{
		bool flag = false;
		int width = 0;
		int height = 0;
		m_Floors = new List<RoomFloor>();
		RoomFloor[] componentsInChildren = base.gameObject.GetComponentsInChildren<RoomFloor>();
		for (int i = 0; i < floors.Length; i++)
		{
			if (!flag && floors[i].m_TileSystems != null && floors[i].m_TileSystems.Length > 1 && floors[i].m_TileSystems[0] != null)
			{
				width = floors[i].m_TileSystems[0].ColumnCount + 1;
				height = floors[i].m_TileSystems[0].RowCount + 1;
				flag = true;
			}
			bool flag2 = false;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].name == floors[i].m_FloorName)
				{
					int num = nextRoomID;
					componentsInChildren[j].LoadRooms(ref nextRoomID);
					if (num != nextRoomID)
					{
					}
					componentsInChildren[j].transform.localPosition = floors[i].m_FloorRootObject.position;
					componentsInChildren[j].m_FloorIndex = floors[i].m_FloorIndex;
					m_Floors.Add(componentsInChildren[j]);
					flag2 = true;
				}
			}
			if (flag2)
			{
				continue;
			}
			GameObject gameObject = new GameObject();
			gameObject.name = floors[i].m_FloorName;
			gameObject.transform.parent = base.transform;
			if (floors[i].m_FloorRootObject != null)
			{
				gameObject.transform.localPosition = floors[i].m_FloorRootObject.position;
				RoomFloor roomFloor = gameObject.AddComponent<RoomFloor>();
				if (flag)
				{
					roomFloor.SetDims(width, height);
					defaultWidth = width;
					defaultHeight = height;
				}
				roomFloor.m_FloorIndex = floors[i].m_FloorIndex;
				m_Floors.Add(roomFloor);
			}
		}
		if (roomUtil != null)
		{
			roomUtil.Load();
		}
	}

	public void SetUpPins()
	{
		for (int i = 0; i < m_Floors.Count; i++)
		{
			m_Floors[i].SetupPins();
		}
	}

	public RoomBlob LookUpRoom(int key)
	{
		int count = GetInstance().m_Floors.Count;
		for (int i = 0; i < count; i++)
		{
			RoomFloor roomFloor = GetInstance().m_Floors[i];
			RoomBlob value = null;
			roomFloor.m_Rooms.TryGetValue(key, out value);
			if (value != null)
			{
				return value;
			}
		}
		return null;
	}

	public RoomBlob LookUpRoom(int key, RoomFloor floor)
	{
		RoomBlob value = null;
		floor.m_Rooms.TryGetValue(key, out value);
		return value;
	}

	public RoomBlob LookUpRoom(Vector2 gridPos, RoomFloor floor)
	{
		return LookUpRoom(floor.FloorMap((int)gridPos.x, (int)gridPos.y), floor);
	}

	public RoomBlob LookUpRoom(int x, int y, RoomFloor floor)
	{
		if (floor != null)
		{
			return LookUpRoom(floor.FloorMap(x, y), floor);
		}
		return null;
	}

	public RoomBlob LookUpRoom(Vector3 pos)
	{
		RoomFloor floorFromZ = GetFloorFromZ(pos.z);
		pos = RoomUtility.WorldToRoomGrid(pos, floorFromZ);
		return LookUpRoom(floorFromZ.FloorMap((int)pos.x, (int)pos.y), floorFromZ);
	}

	public RoomBlob LookUpRoom(Vector3 pos, FloorManager.Floor floor)
	{
		RoomFloor floorFromFloorManFloor = GetFloorFromFloorManFloor(floor);
		if (floorFromFloorManFloor == null)
		{
			return null;
		}
		pos = RoomUtility.WorldToRoomGrid(pos, floorFromFloorManFloor);
		return LookUpRoom(floorFromFloorManFloor.FloorMap((int)pos.x, (int)pos.y), floorFromFloorManFloor);
	}

	public int LookUpKey(int x, int y, RoomFloor floor)
	{
		return floor.FloorMap(x, y);
	}

	public RoomFloor GetFloorFromZ(float zPos)
	{
		RoomFloor roomFloor = null;
		if (m_Floors != null)
		{
			for (int num = m_Floors.Count - 1; num >= 0; num--)
			{
				roomFloor = m_Floors[num];
				if (roomFloor != null && zPos <= roomFloor.transform.position.z)
				{
					break;
				}
			}
		}
		return roomFloor;
	}

	public RoomFloor GetFloorFromFloorManFloor(FloorManager.Floor floor)
	{
		if (floor.m_RoomFloor != null)
		{
			return floor.m_RoomFloor;
		}
		RoomFloor roomFloor = null;
		for (int num = m_Floors.Count - 1; num >= 0; num--)
		{
			if (m_Floors[num].m_FloorIndex == floor.m_FloorIndex)
			{
				roomFloor = m_Floors[num];
				break;
			}
		}
		floor.m_RoomFloor = roomFloor;
		return roomFloor;
	}

	public RoomFloor GetFloorFromIndex(int index)
	{
		if (index >= 0 && index < m_Floors.Count)
		{
			return m_Floors[index];
		}
		return null;
	}

	public RoomBlob GetFirstRoomByLocation(RoomBlob.eLocation location)
	{
		List<RoomBlob> allRoomsByLocation = GetAllRoomsByLocation(location);
		if (allRoomsByLocation == null || allRoomsByLocation.Count == 0)
		{
			return null;
		}
		return allRoomsByLocation[0];
	}

	public List<RoomBlob> GetAllRoomsByLocation(RoomBlob.eLocation location)
	{
		List<RoomBlob> list = new List<RoomBlob>();
		for (int i = 0; i < m_Floors.Count; i++)
		{
			List<RoomBlob> allRoomsOnThisFloorByLocation = m_Floors[i].GetAllRoomsOnThisFloorByLocation(location);
			if (allRoomsOnThisFloorByLocation != null && allRoomsOnThisFloorByLocation.Count != 0)
			{
				list.AddRange(allRoomsOnThisFloorByLocation);
			}
		}
		return list;
	}

	public void GetNameandKeyLists(out string[] nameList, out int[] keyList, RoomFloor floor)
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		if (floor != null && floor.m_Rooms.Count > 0)
		{
			foreach (KeyValuePair<int, RoomBlob> room in floor.m_Rooms)
			{
				list.Add(room.Key + "_" + room.Value.location);
				list2.Add(room.Key);
			}
		}
		nameList = list.ToArray();
		keyList = list2.ToArray();
	}

	public bool GetRandomPositionInWorld(CharacterRole role, ref Vector3 pos, RoomLabel label = RoomLabel.None)
	{
		int num = 0;
		int num2 = 27;
		if (label != 0)
		{
			num = (int)label;
			num2 = (int)label;
		}
		switch (role)
		{
		case CharacterRole.Inmate:
			return RoomUtility.GetInstance().GetRandomInmateNodePosition(m_iInmateSafeSpaceStartIndex[num], m_iInmateSafeSpaceEndIndex[num2], ref pos);
		case CharacterRole.Guard:
		case CharacterRole.Dog:
			return RoomUtility.GetInstance().GetRandomGuardNodePosition(m_iGuardSafeSpaceStartIndex[num], m_iGuardSafeSpaceEndIndex[num2], ref pos);
		default:
			return RoomUtility.GetInstance().GetRandomSupportNodePosition(m_iSupportSafeSpaceStartIndex[num], m_iSupportSafeSpaceEndIndex[num2], ref pos);
		}
	}

	public string[] GetNameList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < m_Floors.Count; i++)
		{
			list.Add(m_Floors[i].name);
		}
		return list.ToArray();
	}

	public static void RegisterForRoomAssignment(Character character)
	{
		m_CharactersForRoomAssignment.Add(character);
	}

	public static void UnregisterForRoomAssignment(Character character)
	{
		m_CharactersForRoomAssignment.Remove(character);
	}

	public static void CleanUp()
	{
		if (m_CharactersForRoomAssignment != null)
		{
			m_CharactersForRoomAssignment.Clear();
		}
	}

	public string CreateSnapshot()
	{
		SaveData saveData = new SaveData();
		int count = m_SpawnPointForCharacter.Count;
		int num = 0;
		saveData.v = new int[count];
		saveData.s = new int[count];
		foreach (KeyValuePair<int, SpawnPoint> item in m_SpawnPointForCharacter)
		{
			saveData.v[num] = item.Key;
			saveData.s[num] = item.Value.m_SpawnPointID;
			num++;
		}
		return JsonUtility.ToJson(saveData);
	}

	public void StartedFromSnapshot()
	{
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool Deserialize(string serializedData, ref string error)
	{
		if (string.IsNullOrEmpty(serializedData))
		{
			return true;
		}
		SaveData saveData = null;
		try
		{
			saveData = JsonUtility.FromJson<SaveData>(serializedData);
		}
		catch
		{
			error = "Room Manager could not parse JSON data, it is corrupt.";
			return false;
		}
		if (saveData == null)
		{
			error = "RoomManager: JSON data returned null.";
			return false;
		}
		if (saveData.v.Length != saveData.s.Length)
		{
			error = "Deserialize received invalid params: " + saveData.v.Length + " " + saveData.s.Length;
			return false;
		}
		for (int i = 0; i < saveData.v.Length; i++)
		{
			InsertSpawnPointForCharacter(saveData.v[i], saveData.s[i]);
		}
		return true;
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
		if (!T17NetManager.IsMasterClient || player.IsLocal)
		{
			return;
		}
		if (m_LoadState == LOADSTATE.Finished_OK)
		{
			int count = m_SpawnPointForCharacter.Count;
			int num = 0;
			int[] array = new int[count];
			int[] array2 = new int[count];
			foreach (KeyValuePair<int, SpawnPoint> item in m_SpawnPointForCharacter)
			{
				array[num] = item.Key;
				array2[num] = item.Value.m_SpawnPointID;
				num++;
			}
			m_Netview.RPC("RPC_SetSpawnPointsForCharacters", player, array, array2);
		}
		else
		{
			m_Netview.RPC("RPC_RequestStateResponce_No_RoomManager", player);
		}
	}

	[PunRPC]
	public void RPC_SetSpawnPointsForCharacters(int[] characterViewIds, int[] spawnPointIds, PhotonMessageInfo info)
	{
		if (characterViewIds.Length == spawnPointIds.Length)
		{
			for (int i = 0; i < characterViewIds.Length; i++)
			{
				InsertSpawnPointForCharacter(characterViewIds[i], spawnPointIds[i]);
			}
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
	}

	[PunRPC]
	private void RPC_RequestStateResponce_No_RoomManager(PhotonMessageInfo info)
	{
		m_LoadError = "RoomManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	public void AssignSpawnPointForPlayer(int playerViewID)
	{
		int num = -1;
		if (m_SpawnPointForCharacter.ContainsKey(playerViewID))
		{
			num = m_SpawnPointForCharacter[playerViewID].m_SpawnPointID;
		}
		if (num == -1 && m_SpawnPointsKeyForRandom.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, m_SpawnPointsKeyForRandom.Count);
			num = m_SpawnPointsKeyForRandom[index];
			m_SpawnPointsKeyForRandom.RemoveAt(index);
		}
		if (num != -1)
		{
			InsertSpawnPointForCharacter(playerViewID, num);
		}
	}

	public void AssignNonPlayerCharactersToRoomSpawnPoints()
	{
		if (T17NetManager.IsMasterClient && m_SpawnPointsKeyForRandom.Count > 0 && m_CharactersForRoomAssignment.Count > 0)
		{
			for (int i = 0; i < m_CharactersForRoomAssignment.Count; i++)
			{
				if (m_CharactersForRoomAssignment[i].m_CharacterRole == CharacterRole.Inmate && !m_CharactersForRoomAssignment[i].m_CharacterStats.m_bIsPlayer)
				{
					int viewID = m_CharactersForRoomAssignment[i].m_NetView.viewID;
					int index = UnityEngine.Random.Range(0, m_SpawnPointsKeyForRandom.Count);
					int spawnPointID = m_SpawnPointsKeyForRandom[index];
					m_SpawnPointsKeyForRandom.RemoveAt(index);
					InsertSpawnPointForCharacter(viewID, spawnPointID);
					if (m_SpawnPointsKeyForRandom.Count <= 0)
					{
						break;
					}
				}
			}
		}
		m_CharactersForRoomAssignment.Clear();
	}

	private void InsertSpawnPointForCharacter(int characterViewID, int spawnPointID)
	{
		if (characterViewID == -1)
		{
			return;
		}
		Character character = T17NetView.Find<Character>(characterViewID);
		if (!(character != null))
		{
			return;
		}
		character.m_bSpawnPointInit = true;
		if (m_SpawnPointForCharacter.ContainsKey(characterViewID))
		{
			return;
		}
		SpawnPoint spawnPoint = m_InmateSpawnPoints[spawnPointID];
		spawnPoint.SetCharacterOwner(character);
		spawnPoint.gameObject.name = "SpawnPoint_" + character.m_CharacterCustomisation.m_DisplayName;
		character.SetMyCell(spawnPoint.MyRoomBlob);
		m_SpawnPointForCharacter.Add(characterViewID, spawnPoint);
		if (character.m_NetView.isMine && spawnPoint != null)
		{
			if (character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
			{
				Vector3 newPosition = new Vector3(Player.HidePlayerPosition.x, Player.HidePlayerPosition.y - 2f * (float)spawnPointID, spawnPoint.transform.position.z);
				character.Teleport(newPosition);
			}
			else
			{
				character.Teleport(spawnPoint.transform.position);
			}
		}
		spawnPoint.AddStartingItems();
		if (spawnPoint.m_AttachedBed != null && T17NetManager.IsMasterClient && !PrisonSnapshotIO.IsThereSaveData() && (character.m_CharacterStats == null || !character.m_CharacterStats.m_bIsPlayer))
		{
			Vector3 newPosition2 = spawnPoint.m_AttachedBed.FindClosestInteractionNode(character);
			character.Teleport(newPosition2);
			spawnPoint.m_AttachedBed.m_bOnLevelEnteredInteraction = true;
			spawnPoint.m_AttachedBed.Interact(character);
		}
	}

	public RoomBlob CreateNewRoom(RoomFloor floor)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = nextRoomID + "_" + floor.name;
		gameObject.transform.parent = floor.transform;
		gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
		RoomBlob roomBlob = gameObject.AddComponent<RoomBlob>();
		roomBlob.m_ID = nextRoomID;
		roomBlob.colour = Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f);
		roomBlob.LoadBlob();
		floor.AddRoom(nextRoomID, roomBlob);
		nextRoomID++;
		return roomBlob;
	}

	public void DeleteRoomFromFloor(int roomKey, RoomFloor floor)
	{
		RoomBlob roomBlob = LookUpRoom(roomKey, floor);
		if (roomBlob != null)
		{
			UnityEngine.Object.Destroy(roomBlob.gameObject);
			floor.RemoveRoom(roomKey);
		}
	}

	public void FixAllToiletData()
	{
		for (int i = 0; i < m_Floors.Count; i++)
		{
			m_Floors[i].PopulateRoomTempNodes();
			foreach (KeyValuePair<int, RoomBlob> room in m_Floors[i].m_Rooms)
			{
				room.Value.FindAllValidToiletPositionsInRoom();
			}
		}
	}
}
