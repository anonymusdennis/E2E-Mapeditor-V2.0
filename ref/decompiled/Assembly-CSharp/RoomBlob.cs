using System;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using Pathfinding;
using UnityEngine;

[Serializable]
public class RoomBlob : T17MonoBehaviour
{
	public enum RoomAffinity
	{
		SuperPopular = 15,
		Interesting = 8,
		Meh = 5,
		Dull = 2,
		UtterlyBoring = 1
	}

	public enum eLocation : byte
	{
		NowhereSpecial,
		Corridor,
		InmateCell,
		MealHall,
		Gym,
		RollCall,
		Shower,
		Library,
		Solitary,
		Infirmary,
		InfirmaryStockRoom,
		JobOffice,
		ControlRoom,
		ContrabandRoom,
		Kitchen,
		Kennels,
		WardensOffice,
		GuardQuarters,
		SocialArea,
		Maintenance,
		JobRoom,
		BuildingBoundary,
		RoofArea,
		VisitorArea,
		CarPark,
		GuardRoom,
		GuardTower,
		WasteCollection,
		ShowTime,
		CrowdSeating
	}

	public enum RoomSubIdentity_Location
	{
		Indoors,
		Outdoors
	}

	public enum RoomSubIdentity_Rules
	{
		Inbounds,
		OffLimits
	}

	public enum WaypointSortType
	{
		HeadsFirst,
		TailsFirst
	}

	public eLocation location;

	public RoomSubIdentity_Location m_subLocation;

	public RoomSubIdentity_Rules m_subRules;

	public Player_Footsteps m_FloorMaterial;

	public int m_ID = -1;

	public Color colour = Color.white;

	public RoomAffinity m_RoomAffinity = RoomAffinity.Meh;

	public RoomAffinity m_RoomAffinityGuard = RoomAffinity.Meh;

	public RoomAffinity m_RoomAffinitySupport = RoomAffinity.Meh;

	public bool m_InmateSafeSpace = true;

	public bool m_GuardSafeSpace;

	public bool m_SupportSafeSpace;

	public bool m_AllowSniping = true;

	public RoomLabel m_RoomLabel;

	public bool m_bDisplayMapIcon;

	public bool m_bPrevDisplayMapIcon;

	public Sprite m_MapIcon;

	public string m_MapIconToolTipTag = string.Empty;

	public HashSet<Character> m_CharactersInRoom = new HashSet<Character>();

	public List<InteractiveObject> m_InmateRoomObjects = new List<InteractiveObject>();

	public List<InteractiveObject> m_GuardRoomObjects = new List<InteractiveObject>();

	public List<RoomWaypoint> m_Waypoints = new List<RoomWaypoint>();

	public List<CarryObjectInteraction> m_CarryObjectInteractions = new List<CarryObjectInteraction>();

	[ReadOnly]
	public Vector3 position;

	public bool m_bManualPosition;

	private bool m_bManualBuilderPosition;

	private Vector3 m_ManualBuilderArrowPosition = Vector3.one;

	[ReadOnly]
	public List<RoomBlob> m_ARoomConnections = new List<RoomBlob>();

	public bool m_AVisited;

	public ushort m_APathID;

	[ReadOnly]
	public float m_ACostSoFar = float.MaxValue;

	private List<RoomWaypoint> m_FreeWaypoints;

	private Dictionary<Character, RoomWaypoint> m_TakenWaypoints;

	private RoomBlobData m_BlobData;

	private RoomFloor m_RoomFloor;

	private const float m_fWalkableNodesPerRoom = 1f;

	public List<Vector3> m_TempRoomPositions = new List<Vector3>();

	public List<Vector3> m_FirstRoomTiles = new List<Vector3>();

	public Vector3 m_SomeRoomPosition;

	public Vector2 m_ScrollPosition;

	public bool m_RoomPositionsChanged;

	private bool m_bRoomObjectsFound;

	private List<GameObject> m_RoomObjects = new List<GameObject>();

	[ReadOnly]
	public int m_iInmateSafeSpaceStartIndex = -1;

	[ReadOnly]
	public int m_iInmateSafeSpaceEndIndex = -1;

	[ReadOnly]
	public int m_iGuardSafeSpaceStartIndex = -1;

	[ReadOnly]
	public int m_iGuardSafeSpaceEndIndex = -1;

	[ReadOnly]
	public int m_iSupportSafeSpaceStartIndex = -1;

	[ReadOnly]
	public int m_iSupportSafeSpaceEndIndex = -1;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_Waypoints != null && m_Waypoints.Count > 0)
		{
			m_FreeWaypoints = new List<RoomWaypoint>();
			for (int i = 0; i < m_Waypoints.Count; i++)
			{
				RoomWaypoint roomWaypoint = m_Waypoints[i];
				if (roomWaypoint != null)
				{
					m_FreeWaypoints.Add(roomWaypoint);
				}
			}
			m_Waypoints.Reverse();
			m_TakenWaypoints = new Dictionary<Character, RoomWaypoint>(Character.CharacterTComparer);
		}
		return base.StartInit();
	}

	protected virtual void OnDestroy()
	{
		if (m_RoomFloor != null)
		{
			m_RoomFloor.RemoveRoom(m_ID);
		}
		m_RoomFloor = null;
		m_BlobData = null;
		m_CharactersInRoom.Clear();
		m_ARoomConnections.Clear();
		m_InmateRoomObjects.Clear();
		m_GuardRoomObjects.Clear();
		m_Waypoints.Clear();
		m_CarryObjectInteractions.Clear();
		if (m_TakenWaypoints != null)
		{
			m_TakenWaypoints.Clear();
			m_TakenWaypoints = null;
		}
		if (m_FreeWaypoints != null)
		{
			m_FreeWaypoints.Clear();
			m_FreeWaypoints = null;
		}
		if (m_RoomObjects != null)
		{
			m_RoomObjects.Clear();
			m_RoomObjects = null;
		}
	}

	public void SetBuilderArrowPosition(Vector3 arrowPosition)
	{
		m_ManualBuilderArrowPosition = arrowPosition;
		m_bManualBuilderPosition = true;
	}

	public Vector3 GetArrowPosition()
	{
		if (m_bManualBuilderPosition)
		{
			return m_ManualBuilderArrowPosition;
		}
		if (m_bManualPosition)
		{
			return base.transform.position;
		}
		return position;
	}

	public List<Character> GetCharactersInRoom()
	{
		return m_CharactersInRoom.ToList();
	}

	public void EnterRoom(Character character)
	{
		if (!m_CharactersInRoom.Contains(character))
		{
			m_CharactersInRoom.Add(character);
		}
	}

	public void ExitRoom(Character character)
	{
		if (m_CharactersInRoom.Contains(character))
		{
			m_CharactersInRoom.Remove(character);
		}
	}

	public List<InteractiveObject> FindObject(bool searchFreeTime, bool isInmate, Type InteractiveObjectType = null, string tag = null, Vector3? closeTo = null, bool filterReserved = false)
	{
		List<InteractiveObject> list = new List<InteractiveObject>();
		List<InteractiveObject> list2 = null;
		if (searchFreeTime)
		{
			list2 = ((!isInmate) ? m_GuardRoomObjects : m_InmateRoomObjects);
		}
		else
		{
			if (m_BlobData == null)
			{
				return null;
			}
			list2 = m_BlobData.GetRoomSpecificObjects();
		}
		if (InteractiveObjectType == null && string.IsNullOrEmpty(tag) && !closeTo.HasValue)
		{
			return list2;
		}
		if (list2 == null || list2.Count == 0)
		{
			return null;
		}
		if (filterReserved || InteractiveObjectType != null || string.IsNullOrEmpty(tag))
		{
			int count = list2.Count;
			for (int i = 0; i < count; i++)
			{
				if (!(list2[i] == null))
				{
					bool flag = string.IsNullOrEmpty(tag) || list2[i].CompareTag(tag);
					bool flag2 = InteractiveObjectType == null || list2[i].GetType() == InteractiveObjectType;
					bool flag3 = !filterReserved || !list2[i].ObjectReserved();
					if (flag2 && flag && flag3)
					{
						list.Add(list2[i]);
					}
				}
			}
		}
		else
		{
			list.AddRange(list2);
		}
		if (closeTo.HasValue)
		{
			Vector3 position = closeTo.Value;
			list.Sort(delegate(InteractiveObject a, InteractiveObject b)
			{
				Vector3 vector = a.transform.position - position;
				Vector3 vector2 = b.transform.position - position;
				float num = vector.x * vector.x + vector.y * vector.y;
				float value = vector2.x * vector2.x + vector2.y * vector2.y;
				return num.CompareTo(value);
			});
		}
		return list;
	}

	public bool HasPositionNodes(CharacterRole role)
	{
		switch (role)
		{
		case CharacterRole.Inmate:
			return m_iInmateSafeSpaceEndIndex - m_iInmateSafeSpaceStartIndex > 0;
		case CharacterRole.Guard:
		case CharacterRole.Dog:
			return m_iGuardSafeSpaceEndIndex - m_iGuardSafeSpaceStartIndex > 0;
		default:
			return m_iSupportSafeSpaceEndIndex - m_iSupportSafeSpaceStartIndex > 0;
		}
	}

	public bool GetRandomPositionInRoom(CharacterRole role, ref Vector3 position)
	{
		switch (role)
		{
		case CharacterRole.Inmate:
			return RoomUtility.GetInstance().GetRandomInmateNodePosition(m_iInmateSafeSpaceStartIndex, m_iInmateSafeSpaceEndIndex, ref position);
		case CharacterRole.Guard:
		case CharacterRole.Dog:
			return RoomUtility.GetInstance().GetRandomGuardNodePosition(m_iGuardSafeSpaceStartIndex, m_iGuardSafeSpaceEndIndex, ref position);
		default:
			return RoomUtility.GetInstance().GetRandomSupportNodePosition(m_iSupportSafeSpaceStartIndex, m_iSupportSafeSpaceEndIndex, ref position);
		}
	}

	public List<RoomWaypoint> GetWaypointList()
	{
		return m_Waypoints;
	}

	public bool GetWaypoint(Character character, RoomWaypoint waypoint, out Vector3 position, out Directionx4 direction)
	{
		position = character.m_CachedCurrentPosition;
		direction = Directionx4.Up;
		if (m_Waypoints == null || m_Waypoints.Count == 0)
		{
			return false;
		}
		int num = m_Waypoints.IndexOf(waypoint);
		if (num < 0 || num >= m_Waypoints.Count)
		{
			return false;
		}
		RoomWaypoint roomWaypoint = m_Waypoints[num];
		if (roomWaypoint == null)
		{
			return false;
		}
		position = roomWaypoint.GetPosition();
		direction = roomWaypoint.m_FacingDirection;
		if (!roomWaypoint.m_bReservable)
		{
			return true;
		}
		if (roomWaypoint.m_Reservation != null)
		{
			return false;
		}
		m_FreeWaypoints.Remove(roomWaypoint);
		roomWaypoint.m_Reservation = character;
		m_TakenWaypoints[character] = roomWaypoint;
		return true;
	}

	public bool GetRandomWaypoint(Character character, out Vector3 position, out Directionx4 direction)
	{
		position = character.m_CachedCurrentPosition;
		if (m_FreeWaypoints == null || m_FreeWaypoints.Count == 0)
		{
			direction = Directionx4.Up;
			return false;
		}
		RoomWaypoint value = null;
		m_TakenWaypoints.TryGetValue(character, out value);
		if (value != null)
		{
			position = value.GetPosition();
			direction = value.m_FacingDirection;
			return true;
		}
		int num = -1;
		for (int i = 0; i < m_FreeWaypoints.Count; i++)
		{
			Vector3 vector = m_FreeWaypoints[i].GetPosition() - position;
			if (vector.x * vector.x + vector.y * vector.y < 0.8f)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = (int)(Mathf.Abs(UnityEngine.Random.Range(-1f, 1f) + UnityEngine.Random.Range(-1f, 1f) + UnityEngine.Random.Range(-1f, 1f)) * (float)m_FreeWaypoints.Count / 3f);
		}
		RoomWaypoint roomWaypoint = m_FreeWaypoints[num];
		if (roomWaypoint == null)
		{
			direction = Directionx4.Up;
			return false;
		}
		position = roomWaypoint.GetPosition();
		direction = roomWaypoint.m_FacingDirection;
		if (!roomWaypoint.m_bReservable)
		{
			return true;
		}
		m_FreeWaypoints.RemoveAt(num);
		roomWaypoint.m_Reservation = character;
		m_TakenWaypoints[character] = roomWaypoint;
		return true;
	}

	public void ReturnWaypoint(Character character)
	{
		RoomWaypoint value = null;
		if (m_TakenWaypoints != null && m_FreeWaypoints != null)
		{
			m_TakenWaypoints.TryGetValue(character, out value);
			if (value != null)
			{
				m_FreeWaypoints.Add(value);
				value.m_Reservation = null;
				m_TakenWaypoints[character] = null;
			}
		}
	}

	public void ValidateRoomObjectLists()
	{
		for (int num = m_InmateRoomObjects.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = m_InmateRoomObjects[num];
			if (interactiveObject == null)
			{
				m_InmateRoomObjects.RemoveAt(num);
			}
		}
		for (int num2 = m_GuardRoomObjects.Count - 1; num2 >= 0; num2--)
		{
			InteractiveObject interactiveObject2 = m_GuardRoomObjects[num2];
			if (interactiveObject2 == null)
			{
				m_GuardRoomObjects.RemoveAt(num2);
			}
		}
	}

	public T GetRoomBlobData<T>()
	{
		if (m_BlobData == null || m_BlobData.GetType() != typeof(T))
		{
			return default(T);
		}
		return (T)(object)m_BlobData;
	}

	public void LoadBlob()
	{
		m_BlobData = GetComponent<RoomBlobData>();
	}

	public void SetFloor(RoomFloor floor)
	{
		m_RoomFloor = floor;
	}

	public RoomFloor GetFloor()
	{
		return m_RoomFloor;
	}

	public void AddLabelToMap()
	{
		if (m_bDisplayMapIcon && m_MapIcon != null)
		{
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = false;
			GameObject target = base.gameObject;
			Sprite mapIcon = m_MapIcon;
			FloorManager.Floor floor = m_RoomFloor.GetFloor();
			instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition: false, floor, null, PinManager.Pin.PinFilterType.All, edgable: false, floorTrackable: false, directional: false, m_MapIconToolTipTag);
		}
	}

	public void AutoSetupRoom(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		if (m_BlobData != null)
		{
			m_BlobData.AutoSetup(iLevelEditorRoomNumber, eLayer);
			RoomBlob blob = this;
			m_BlobData.AutoSetupRoomBlob(iLevelEditorRoomNumber, eLayer, ref blob);
			return;
		}
		switch (location)
		{
		case eLocation.Corridor:
			SetupCorridor(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.InfirmaryStockRoom:
			SetupInfirmaryStockRoom(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.Maintenance:
			SetupMaintenance(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.GuardRoom:
			SetupGuardRoom(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.NowhereSpecial:
			SetupNowhereSpecial(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.WardensOffice:
			SetupWardensOffice(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.SocialArea:
			SetupSocialArea(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.Library:
			SetupLibrary(iLevelEditorRoomNumber, eLayer);
			break;
		case eLocation.JobOffice:
		case eLocation.BuildingBoundary:
		case eLocation.RoofArea:
		case eLocation.VisitorArea:
		case eLocation.CarPark:
		case eLocation.GuardTower:
			break;
		case eLocation.InmateCell:
		case eLocation.MealHall:
		case eLocation.Gym:
		case eLocation.RollCall:
		case eLocation.Shower:
		case eLocation.Solitary:
		case eLocation.Infirmary:
		case eLocation.ControlRoom:
		case eLocation.ContrabandRoom:
		case eLocation.Kennels:
		case eLocation.GuardQuarters:
		case eLocation.JobRoom:
			break;
		case eLocation.Kitchen:
			break;
		}
	}

	public void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		if (m_BlobData != null)
		{
			m_BlobData.AutoSetupZone(ref zone);
			RoomBlob blob = this;
			m_BlobData.AutoSetupZoneBlob(ref zone, ref blob);
			return;
		}
		switch (location)
		{
		case eLocation.Corridor:
			SetupCorridorZone(ref zone);
			break;
		case eLocation.InfirmaryStockRoom:
			SetupInfirmaryStockRoomZone(ref zone);
			break;
		case eLocation.Maintenance:
			SetupMaintenanceZone(ref zone);
			break;
		case eLocation.GuardRoom:
			SetupGuardRoomZone(ref zone);
			break;
		case eLocation.NowhereSpecial:
			SetupNowhereSpecialZone(ref zone);
			break;
		case eLocation.WardensOffice:
			SetupWardensOfficeZone(ref zone);
			break;
		case eLocation.SocialArea:
			SetupSocialAreaZone(ref zone);
			break;
		case eLocation.Library:
			SetupLibraryZone(ref zone);
			break;
		case eLocation.JobOffice:
		case eLocation.BuildingBoundary:
		case eLocation.RoofArea:
		case eLocation.VisitorArea:
		case eLocation.CarPark:
		case eLocation.GuardTower:
			break;
		case eLocation.InmateCell:
		case eLocation.MealHall:
		case eLocation.Gym:
		case eLocation.RollCall:
		case eLocation.Shower:
		case eLocation.Solitary:
		case eLocation.Infirmary:
		case eLocation.ControlRoom:
		case eLocation.ContrabandRoom:
		case eLocation.Kennels:
		case eLocation.GuardQuarters:
		case eLocation.JobRoom:
			break;
		case eLocation.Kitchen:
			break;
		}
	}

	public void SetupLibrary(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupLibraryZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupSocialArea(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<StudyInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<StudyInteraction>();
			BaseLevelManager.RoomObjectCollectionType<InstrumentInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<InstrumentInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
		}
	}

	public void SetupSocialAreaZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<StudyInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<StudyInteraction>();
			BaseLevelManager.RoomObjectCollectionType<InstrumentInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<InstrumentInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
		}
	}

	public void SetupWardensOffice(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupWardensOfficeZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupNowhereSpecial(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupNowhereSpecialZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void SetupNowhereSpecialObject(GameObject obj)
	{
		BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
		GetCollectionInObject(obj, roomObjectCollectionType);
		for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
		{
			if (!m_GuardRoomObjects.Contains(roomObjectCollectionType.m_Contents[num]))
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}

	public void GetCollectionInObject(GameObject obj, params BaseLevelManager.RoomObjectCollectionTypeBase[] values)
	{
		if (values.Length == 0 || obj == null)
		{
		}
		Component[] componentsInChildren = obj.GetComponentsInChildren<Component>(includeInactive: true);
		int num = componentsInChildren.Length;
		int num2 = values.Length;
		for (int i = 0; i < num; i++)
		{
			Type type = componentsInChildren[i].GetType();
			for (int j = 0; j < num2; j++)
			{
				if (values[j].GetCollectionType().IsAssignableFrom(type))
				{
					values[j].AddToList(componentsInChildren[i]);
				}
			}
		}
	}

	public void SetupGuardRoom(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupGuardRoomZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupMaintenance(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupMaintenanceZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupInfirmaryStockRoom(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType, roomObjectCollectionType2);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
		}
	}

	public void SetupInfirmaryStockRoomZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType, roomObjectCollectionType2);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
		}
	}

	public void SetupCorridor(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<StudyInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<StudyInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupCorridorZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			m_InmateRoomObjects.Clear();
			m_GuardRoomObjects.Clear();
			m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<StudyInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<StudyInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetupCorridorObject(GameObject obj)
	{
		BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
		BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction>();
		BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
		BaseLevelManager.RoomObjectCollectionType<StudyInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<StudyInteraction>();
		GetCollectionInObject(obj, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3, roomObjectCollectionType4);
		for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
		{
			if (!m_InmateRoomObjects.Contains(roomObjectCollectionType.m_Contents[num]))
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			if (!m_GuardRoomObjects.Contains(roomObjectCollectionType.m_Contents[num]))
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
		for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
		{
			if (!m_InmateRoomObjects.Contains(roomObjectCollectionType2.m_Contents[num2]))
			{
				m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
		}
		for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
		{
			if (!m_GuardRoomObjects.Contains(roomObjectCollectionType3.m_Contents[num3]))
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
		}
		for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
		{
			if (!m_GuardRoomObjects.Contains(roomObjectCollectionType4.m_Contents[num4]))
			{
				m_GuardRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public void SetRoomLocationWithRoomUtility(ref RoomUtility roomUtility, eLocation setToLocation)
	{
		RoomUtility.LocationRoomBlobMap roomDataMap = roomUtility.GetDataMapForLocation(setToLocation);
		SetRoomLocationFromMap(ref roomDataMap, setToLocation);
	}

	public void SetRoomLocation(eLocation setToLocation)
	{
		RoomUtility.LocationRoomBlobMap roomDataMap = RoomUtility.GetInstance().GetDataMapForLocation(setToLocation);
		SetRoomLocationFromMap(ref roomDataMap, setToLocation);
	}

	private void SetRoomLocationFromMap(ref RoomUtility.LocationRoomBlobMap roomDataMap, eLocation setToLocation)
	{
		UnityEngine.Object @object = null;
		if (roomDataMap != null)
		{
			@object = roomDataMap.data;
		}
		if (@object != null)
		{
			Type type = Type.GetType(@object.name);
			RoomBlobData roomBlobData = (RoomBlobData)base.gameObject.GetComponent(type);
			if (roomBlobData == null || roomBlobData != m_BlobData)
			{
				UnityEngine.Object.DestroyImmediate(m_BlobData);
				m_BlobData = (RoomBlobData)base.gameObject.AddComponent(type);
			}
			if (m_MapIcon == null && roomDataMap != null && roomDataMap.m_ShowMapIcon)
			{
				m_bDisplayMapIcon = roomDataMap.m_ShowMapIcon;
				m_MapIcon = roomDataMap.m_DefaultMapIcon;
				m_MapIconToolTipTag = roomDataMap.m_DefaultMapToolTip;
			}
		}
		location = setToLocation;
	}

	public List<Vector3> EditorGenerateListOfWalkableNodes(NavGraph navGraph, CharacterRole role)
	{
		List<Vector3> list = new List<Vector3>();
		if (navGraph == null)
		{
			return list;
		}
		RoomAffinity roomAffinity = RoomAffinity.Meh;
		roomAffinity = role switch
		{
			CharacterRole.Inmate => m_RoomAffinity, 
			CharacterRole.Guard => m_RoomAffinityGuard, 
			_ => m_RoomAffinitySupport, 
		};
		List<Vector3> allWalkableNodes = GetAllWalkableNodes(navGraph);
		allWalkableNodes.Shuffle();
		for (int i = 0; i < allWalkableNodes.Count; i++)
		{
			float num = 1f;
			if (list.Count > 0)
			{
				num = 7f / (float)list.Count;
			}
			float num2 = num * ((float)roomAffinity / 15f);
			if (!(UnityEngine.Random.value > num2))
			{
				list.Add(allWalkableNodes[i]);
			}
		}
		return list;
	}

	public List<Vector3> GetAllWalkableNodes(NavGraph navGraph)
	{
		List<Vector3> list = new List<Vector3>();
		if (navGraph == null)
		{
			return list;
		}
		if (m_TempRoomPositions.Count == 0)
		{
			GridGraph gridGraph = (GridGraph)navGraph;
			int num = (int)m_SomeRoomPosition.x;
			int num2 = gridGraph.depth - 1 - (int)m_SomeRoomPosition.y;
			int depth = gridGraph.depth;
			int num3 = num + num2 * depth;
			GridNode gridNode = gridGraph.nodes[num3];
			float num4 = 1000f;
			list.Add(new Vector3((float)gridNode.position.x / num4, (float)gridNode.position.y / num4, (float)gridNode.position.z / num4));
		}
		for (int i = 0; i < m_TempRoomPositions.Count; i++)
		{
			GridGraph gridGraph2 = (GridGraph)navGraph;
			int num5 = (int)m_TempRoomPositions[i].x;
			int num6 = gridGraph2.depth - 1 - (int)m_TempRoomPositions[i].y;
			int width = gridGraph2.width;
			int num7 = num5 + num6 * width;
			if (num7 < gridGraph2.nodes.Length)
			{
				GridNode gridNode2 = gridGraph2.nodes[num7];
				if (gridNode2.Walkable)
				{
					float num8 = 1000f;
					list.Add(new Vector3((float)gridNode2.position.x / num8, (float)gridNode2.position.y / num8, (float)gridNode2.position.z / num8));
				}
			}
		}
		return list;
	}

	public List<GameObject> FindAllObjectsInRoom()
	{
		if (Application.isPlaying && m_bRoomObjectsFound)
		{
			return m_RoomObjects;
		}
		m_bRoomObjectsFound = true;
		for (int i = 0; i < m_TempRoomPositions.Count; i++)
		{
			Vector3 pos = m_TempRoomPositions[i];
			pos.x += 1f;
			pos.y += 0.5f;
			Vector3 vector = RoomUtility.RoomGridToWorld(pos, m_RoomFloor);
			RaycastHit[] array = Physics.SphereCastAll(vector + Vector3.forward * 2f, 0.95f, -Vector3.forward);
			int num = array.Length;
			for (int j = 0; j < num; j++)
			{
				Collider collider = array[j].collider;
				if (!(collider == null))
				{
					GameObject item = collider.gameObject;
					if (!m_RoomObjects.Contains(item))
					{
						m_RoomObjects.Add(item);
					}
				}
			}
		}
		return m_RoomObjects;
	}

	public void FindAllValidToiletPositionsInRoom()
	{
		List<GameObject> list = FindAllObjectsInRoom();
		for (int i = 0; i < list.Count; i++)
		{
			InteractiveObject component = list[i].GetComponent<InteractiveObject>();
			if (component != null && component.GetType() == typeof(ToiletInteraction))
			{
				((ToiletInteraction)component).SetToiletFloodPoints(ref m_TempRoomPositions, m_RoomFloor);
			}
		}
	}

	public void LoadAllInteractableObjects()
	{
		m_InmateRoomObjects.Clear();
		m_GuardRoomObjects.Clear();
		List<GameObject> list = FindAllObjectsInRoom();
		for (int i = 0; i < list.Count; i++)
		{
			InteractiveObject component = list[i].GetComponent<InteractiveObject>();
			if (component != null)
			{
				m_InmateRoomObjects.Add(component);
				m_GuardRoomObjects.Add(component);
			}
		}
	}

	public void AutoPositionRoom()
	{
		if (m_TempRoomPositions == null || m_TempRoomPositions.Count == 0)
		{
			m_TempRoomPositions = new List<Vector3>();
			m_TempRoomPositions.Add(m_SomeRoomPosition);
		}
		Vector3 centroid = GetCentroid();
		base.transform.position = RoomUtility.RoomGridToWorld(centroid, m_RoomFloor) + new Vector3(1f, -0.5f, 0f);
	}

	public Vector3 GetCentroid()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < m_TempRoomPositions.Count; i++)
		{
			zero += m_TempRoomPositions[i];
		}
		return zero / m_TempRoomPositions.Count;
	}

	public static int WhatPointsAtMe(ref List<RoomWaypoint> wayPoints, ref Vector3[] wayPositions, int iIndex)
	{
		for (int num = wayPoints.Count - 1; num >= 0; num--)
		{
			if (iIndex != num && wayPoints[num] != null)
			{
				float num2 = wayPositions[iIndex].x - wayPositions[num].x;
				float num3 = wayPositions[iIndex].y - wayPositions[num].y;
				if (num2 < -0.5f && num2 > -1.5f && num3 > -0.5f && num3 < 0.5f && wayPoints[num].m_FacingDirection == Directionx4.Left)
				{
					return num;
				}
				if (num2 > 0.5f && num2 < 1.5f && num3 > -0.5f && num3 < 0.5f && wayPoints[num].m_FacingDirection == Directionx4.Right)
				{
					return num;
				}
				if (num3 < -0.5f && num3 > -1.5f && num2 > -0.5f && num2 < 0.5f && wayPoints[num].m_FacingDirection == Directionx4.Down)
				{
					return num;
				}
				if (num3 > 0.5f && num3 < 1.5f && num2 > -0.5f && num2 < 0.5f && wayPoints[num].m_FacingDirection == Directionx4.Up)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public static List<int> MakeSnake(ref List<RoomWaypoint> wayPoints, ref Vector3[] wayPositions, int iIndex, ref int iTotalRemoved)
	{
		List<int> list = new List<int>();
		while (iIndex != -1)
		{
			int num = WhoDoIPointAt(ref wayPoints, ref wayPositions, iIndex);
			list.Add(iIndex);
			wayPoints[iIndex] = null;
			iTotalRemoved++;
			iIndex = num;
		}
		return list;
	}

	public static int WhoDoIPointAt(ref List<RoomWaypoint> wayPoints, ref Vector3[] wayPositions, int iIndex)
	{
		float num = wayPositions[iIndex].x;
		float num2 = wayPositions[iIndex].y;
		switch (wayPoints[iIndex].m_FacingDirection)
		{
		case Directionx4.Down:
			num2 -= 1f;
			break;
		case Directionx4.Up:
			num2 += 1f;
			break;
		case Directionx4.Left:
			num -= 1f;
			break;
		case Directionx4.Right:
			num += 1f;
			break;
		}
		for (int num3 = wayPoints.Count - 1; num3 >= 0; num3--)
		{
			if (wayPoints[num3] != null && num3 != iIndex)
			{
				float num4 = num - wayPositions[num3].x;
				float num5 = num2 - wayPositions[num3].y;
				if (num4 > -0.5f && num4 < 0.5f && num5 > -0.5f && num5 < 0.5f)
				{
					return num3;
				}
			}
		}
		return -1;
	}

	public static void OrderWaypoints(ref List<RoomWaypoint> wayPoints, WaypointSortType sortType, bool bReversable)
	{
		if (wayPoints == null || wayPoints.Count == 0)
		{
			return;
		}
		int count = wayPoints.Count;
		List<RoomWaypoint> wayPoints2 = new List<RoomWaypoint>(wayPoints);
		Vector3[] wayPositions = new Vector3[count];
		for (int i = 0; i < count; i++)
		{
			wayPoints[i].m_bReservable = bReversable;
			ref Vector3 reference = ref wayPositions[i];
			reference = wayPoints2[i].GetPosition();
		}
		List<List<int>> list = new List<List<int>>();
		int iTotalRemoved = 0;
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		while (iTotalRemoved < count)
		{
			if (wayPoints2[num] != null)
			{
				if (num2 == -1)
				{
					num2 = num;
				}
				int num4 = WhatPointsAtMe(ref wayPoints2, ref wayPositions, num);
				if (num4 == -1)
				{
					list.Add(MakeSnake(ref wayPoints2, ref wayPositions, num, ref iTotalRemoved));
					num2 = -1;
					num3 = 0;
				}
			}
			num++;
			if (num >= count)
			{
				num = 0;
			}
			num3++;
			if (num3 >= count)
			{
				num3 = 0;
				if (num2 != -1 && wayPoints2[num2] != null)
				{
					list.Add(MakeSnake(ref wayPoints2, ref wayPositions, num2, ref iTotalRemoved));
					num2 = -1;
				}
			}
		}
		switch (sortType)
		{
		case WaypointSortType.TailsFirst:
		{
			int num8 = 0;
			while (num8 < count)
			{
				for (int num9 = list.Count - 1; num9 >= 0; num9--)
				{
					int count3 = list[num9].Count;
					for (int j = 0; j < count3; j++)
					{
						if (list[num9][j] != -1)
						{
							wayPoints2[num8++] = wayPoints[list[num9][j]];
							list[num9][j] = -1;
							break;
						}
					}
				}
			}
			break;
		}
		case WaypointSortType.HeadsFirst:
		{
			int num5 = 0;
			while (num5 < count)
			{
				for (int num6 = list.Count - 1; num6 >= 0; num6--)
				{
					int count2 = list[num6].Count;
					for (int num7 = count2 - 1; num7 >= 0; num7--)
					{
						if (list[num6][num7] != -1)
						{
							wayPoints2[num5++] = wayPoints[list[num6][num7]];
							list[num6][num7] = -1;
							break;
						}
					}
				}
			}
			break;
		}
		}
		wayPoints = wayPoints2;
	}
}
