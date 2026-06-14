using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class AIEventManager : T17MonoBehaviour
{
	public delegate bool AIEventStarted(AIEvent aiEvent);

	public struct SubscriptionData
	{
		public AIEventStarted callback;
	}

	public delegate void AIEventDataSetup();

	public enum EventHeight
	{
		Ground,
		Wall
	}

	private class VisionGrid
	{
		public List<EventManager>[,] m_EventGrid;
	}

	public class EventManagerPosition
	{
		public int floor = -1;

		public int row = -1;

		public int column = -1;
	}

	private static AIEventManager m_Instance;

	public T17NetView m_ManagerNetView;

	public GameObject m_StaticEventFloorPrefab;

	public GameObject m_StaticEventWallPrefab;

	[Header("Attack")]
	public AIEventData m_AttackedGuardEvent;

	public AIEventData m_AttackedInmateEvent;

	[Header("Items")]
	public AIEventData m_ItemContrabandOnFloor;

	[Header("Tile Damage")]
	public AIEventData m_TileDamaged;

	public AIEventData m_TileMissing;

	public AIEventData m_TileDugHole;

	public AIEventData m_TileFlooded;

	[Header("Investigate Location")]
	public AIEventData m_InvestigateLocation;

	public Dictionary<AIEvent.EventType, List<SubscriptionData>> m_GlobalEventStarted = new Dictionary<AIEvent.EventType, List<SubscriptionData>>();

	public AIEventDataSetup m_AIEventDataSetup;

	private Dictionary<uint, AIEvent> m_AIEventLookupTable = new Dictionary<uint, AIEvent>();

	private Dictionary<uint, EventManager> m_AIEventManagerLookupTable = new Dictionary<uint, EventManager>();

	public static int m_EventManID_iXBits = -1;

	public static int m_EventManID_iYBits = -1;

	private static BitField m_bitField = new BitField();

	private bool m_bDoneSetup;

	private VisionGrid[] m_FloorEventGrids;

	private Vector3 m_TileSystemOrigin;

	private const int BUCKET_SIZE = 8;

	private int m_BucketRows = -1;

	private int m_BucketColumns = -1;

	public const int ID_FULL_EVENTID_NETID_BITS = 19;

	public const int ID_FULL_EVENTID_POSID_BITS = 32;

	private const int ID_EVENTTYPE_BITS = 6;

	private const int ID_MANAGER_NETID_BITS = 13;

	private const int ID_MANAGER_POSITIONALID_BITS = 26;

	private const int NETID_BITS = 12;

	private const int POSITIONAL_BITS = 20;

	public static AIEventManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public static void CleanUp()
	{
		m_EventManID_iXBits = -1;
		m_EventManID_iYBits = -1;
		m_bitField.Reset();
	}

	public void SubscribeToGlobalCallback(AIEvent.EventType eventType, AIEventStarted callback)
	{
		m_GlobalEventStarted.TryGetValue(eventType, out var value);
		if (value == null)
		{
			value = new List<SubscriptionData>();
		}
		SubscriptionData item = default(SubscriptionData);
		item.callback = callback;
		value.Add(item);
		m_GlobalEventStarted[eventType] = value;
	}

	public void UnsubscribeToGlobalCallback(AIEvent.EventType eventType, AIEventStarted callback)
	{
		m_GlobalEventStarted.TryGetValue(eventType, out var value);
		if (value == null)
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num].callback == callback)
			{
				value.RemoveAt(num);
			}
		}
	}

	public void Initialise()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_AttackedGuardEvent = ConfigManager.GetInstance().ApplyAIEventOverride(m_AttackedGuardEvent);
			m_AttackedInmateEvent = ConfigManager.GetInstance().ApplyAIEventOverride(m_AttackedInmateEvent);
			m_ItemContrabandOnFloor = ConfigManager.GetInstance().ApplyAIEventOverride(m_ItemContrabandOnFloor);
			m_TileDamaged = ConfigManager.GetInstance().ApplyAIEventOverride(m_TileDamaged);
			m_TileMissing = ConfigManager.GetInstance().ApplyAIEventOverride(m_TileMissing);
			m_TileDugHole = ConfigManager.GetInstance().ApplyAIEventOverride(m_TileDugHole);
			m_TileFlooded = ConfigManager.GetInstance().ApplyAIEventOverride(m_TileFlooded);
			m_InvestigateLocation = ConfigManager.GetInstance().ApplyAIEventOverride(m_InvestigateLocation);
		}
		InitBuckets();
		m_bDoneSetup = false;
	}

	private void InitBuckets()
	{
		int currentMaxFloor = FloorManager.GetInstance().GetCurrentMaxFloor();
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorbyIndex(0);
		FloorManager.GetInstance().GetTileSystemBounds(floor, FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
		m_BucketRows = Mathf.CeilToInt((float)maxRows / 8f);
		m_BucketColumns = Mathf.CeilToInt((float)maxColumns / 8f);
		m_FloorEventGrids = new VisionGrid[currentMaxFloor];
		for (int i = 0; i < currentMaxFloor; i++)
		{
			m_FloorEventGrids[i] = new VisionGrid();
			m_FloorEventGrids[i].m_EventGrid = new List<EventManager>[m_BucketRows + 1, m_BucketColumns + 1];
			for (int j = 0; j < m_BucketRows + 1; j++)
			{
				for (int k = 0; k < m_BucketColumns + 1; k++)
				{
					m_FloorEventGrids[i].m_EventGrid[j, k] = new List<EventManager>();
				}
			}
		}
		m_TileSystemOrigin = FloorManager.GetInstance().GetWorldCoordinateForTileSystemOrigin();
	}

	public bool SetupDone()
	{
		return m_bDoneSetup;
	}

	public void RunAIEventDataSetup()
	{
		if (m_AIEventDataSetup != null)
		{
			m_AIEventDataSetup();
		}
		GlobalStart.TimedNetworkService();
		m_bDoneSetup = true;
	}

	public void RemoveManager(EventManager eventManager)
	{
		List<EventManagerPosition> list = eventManager.LockBucketPositions();
		for (int i = 0; i < list.Count; i++)
		{
			EventManagerPosition eventManagerPosition = list[i];
			if (eventManagerPosition != null)
			{
				RemoveManager(eventManager, eventManagerPosition.floor, eventManagerPosition.row, eventManagerPosition.column);
				eventManager.RemoveBucketPosition(eventManagerPosition);
			}
		}
		eventManager.UnlockBucketPositions();
	}

	private void RemoveManager(EventManager eventManager, int floor, int row, int column)
	{
		if (m_FloorEventGrids != null)
		{
			VisionGrid visionGrid = m_FloorEventGrids[floor];
			List<EventManager> list = visionGrid.m_EventGrid[row, column];
			list.Remove(eventManager);
			if (eventManager.m_bVisibleFromBelow && floor > 0)
			{
				VisionGrid visionGrid2 = m_FloorEventGrids[floor - 1];
				List<EventManager> list2 = visionGrid2.m_EventGrid[row, column];
				list2.Remove(eventManager);
			}
		}
	}

	private void AddEventManager(EventManager eventManager, int floor, int row, int column)
	{
		if (m_FloorEventGrids == null)
		{
			InitBuckets();
		}
		VisionGrid visionGrid = m_FloorEventGrids[floor];
		List<EventManager> list = visionGrid.m_EventGrid[row, column];
		list.Add(eventManager);
		if (eventManager.m_bVisibleFromBelow && floor > 0)
		{
			VisionGrid visionGrid2 = m_FloorEventGrids[floor - 1];
			List<EventManager> list2 = visionGrid2.m_EventGrid[row, column];
			list2.Add(eventManager);
		}
	}

	private void GetNearestGridBucket(Vector3 position, out short row, out short column)
	{
		Vector3 vector = position - m_TileSystemOrigin;
		vector /= 8f;
		vector.y *= -1f;
		row = (short)Mathf.Clamp(Mathf.FloorToInt(vector.x), 0, m_BucketRows);
		column = (short)Mathf.Clamp(Mathf.FloorToInt(vector.y), 0, m_BucketColumns);
	}

	public void UpdatePosition(EventManager eventManager, int newFloor = -1)
	{
		if (m_FloorEventGrids == null)
		{
			InitBuckets();
		}
		Vector3 position = eventManager.transform.position;
		AddEventManagerToPositionBucket(eventManager, position, newFloor);
		List<Vector3> targetOffsets = eventManager.GetTargetOffsets();
		for (int i = 0; i < targetOffsets.Count; i++)
		{
			AddEventManagerToPositionBucket(eventManager, position + targetOffsets[i], newFloor);
		}
	}

	public void GetBucketPosition(Vector3 position, out short row, out short column)
	{
		GetNearestGridBucket(position, out row, out column);
	}

	public void AddEventManagerToPositionBucket(EventManager eventManager, Vector3 eventPosition, int newFloor = -1)
	{
		short row = -1;
		short column = -1;
		GetNearestGridBucket(eventPosition, out row, out column);
		List<EventManagerPosition> list = eventManager.LockBucketPositions();
		int count = list.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				EventManagerPosition eventManagerPosition = list[i];
				if (eventManagerPosition != null && (newFloor == -1 || eventManagerPosition.floor == newFloor) && eventManagerPosition.row == row && eventManagerPosition.column == column)
				{
					eventManager.UnlockBucketPositions();
					return;
				}
				if (eventManagerPosition != null)
				{
					RemoveManager(eventManager, eventManagerPosition.floor, eventManagerPosition.row, eventManagerPosition.column);
					eventManager.RemoveBucketPosition(eventManagerPosition);
				}
				else
				{
					eventManagerPosition = new EventManagerPosition();
				}
				AddEventManager(eventManager, newFloor, row, column);
				eventManagerPosition.floor = newFloor;
				eventManagerPosition.row = row;
				eventManagerPosition.column = column;
				eventManager.AddBucketPosition(eventManagerPosition);
			}
		}
		else if (newFloor != -1)
		{
			EventManagerPosition eventManagerPosition2 = new EventManagerPosition();
			AddEventManager(eventManager, newFloor, row, column);
			eventManagerPosition2.floor = newFloor;
			eventManagerPosition2.row = row;
			eventManagerPosition2.column = column;
			eventManager.AddBucketPosition(eventManagerPosition2);
		}
		eventManager.UnlockBucketPositions();
	}

	public List<EventManager> GetEventManagers(int row, int column, int floor)
	{
		if (m_FloorEventGrids == null)
		{
			return null;
		}
		if (floor < 0 || floor >= m_FloorEventGrids.Length)
		{
			return null;
		}
		VisionGrid visionGrid = m_FloorEventGrids[floor];
		row = (short)Mathf.Clamp(row, 0, m_BucketRows);
		column = (short)Mathf.Clamp(column, 0, m_BucketColumns);
		return visionGrid.m_EventGrid[row, column];
	}

	public void SetTileDamagedEvent(Transform location, Transform target, Character characterResponsible, bool setActive, bool wallHeight)
	{
		GenericEventManager genericEventManager = FindParentsEventManager(location, wallHeight ? EventHeight.Wall : EventHeight.Ground, setActive);
		if (genericEventManager != null)
		{
			if (setActive)
			{
				genericEventManager.EnableEventVisability(m_TileDamaged, characterResponsible, target.gameObject);
			}
			else
			{
				genericEventManager.DisableEventVisability(m_TileDamaged);
			}
		}
	}

	public void SetTileMissingEvent(Transform location, Transform target, Character characterResponsible, bool setActive, bool wallHeight)
	{
		GenericEventManager genericEventManager = FindParentsEventManager(location, wallHeight ? EventHeight.Wall : EventHeight.Ground, setActive);
		if (genericEventManager != null)
		{
			if (setActive)
			{
				genericEventManager.EnableEventVisability(m_TileMissing, characterResponsible, target.gameObject);
			}
			else
			{
				genericEventManager.DisableEventVisability(m_TileMissing);
			}
		}
	}

	public void SetTileDugHoleEvent(Transform location, Transform target, Character characterResponsible, bool setActive)
	{
		GenericEventManager genericEventManager = FindParentsEventManager(location, EventHeight.Ground, setActive);
		if (genericEventManager != null)
		{
			if (setActive)
			{
				genericEventManager.EnableEventVisability(m_TileDugHole, characterResponsible, target.gameObject);
			}
			else
			{
				genericEventManager.DisableEventVisability(m_TileDugHole);
			}
		}
	}

	public void SetContrabandItemDropped(Transform parent, Character characterResponsible, bool setActive)
	{
		GenericEventManager genericEventManager = FindParentsEventManager(parent, EventHeight.Wall, setActive);
		if (genericEventManager != null)
		{
			if (setActive)
			{
				genericEventManager.EnableEventVisability(m_ItemContrabandOnFloor, characterResponsible, parent.gameObject);
			}
			else
			{
				genericEventManager.DisableEventVisability(m_ItemContrabandOnFloor);
			}
		}
	}

	public AIEvent GetInvestigateLocationEvent(Transform parent)
	{
		GenericEventManager genericEventManager = FindParentsEventManager(parent, EventHeight.Wall, createIfNeeded: true);
		return genericEventManager.EnableEventVisability(m_InvestigateLocation, null, parent.gameObject);
	}

	public void SetGroundTileCovered(Vector3 position, bool isCovered)
	{
		uint eventManagerIDForPosition = GetEventManagerIDForPosition(position, EventHeight.Ground);
		GenericEventManager genericEventManager = (GenericEventManager)GetEventManagerFromID(eventManagerIDForPosition, createIfNeeded: true);
		if (genericEventManager != null)
		{
			genericEventManager.SetIsCovered(isCovered);
		}
	}

	private GenericEventManager FindParentsEventManager(Transform targetTransform, EventHeight eventHeight, bool createIfNeeded)
	{
		Vector3 position = targetTransform.transform.position;
		uint eventManagerIDForPosition = GetEventManagerIDForPosition(position, eventHeight);
		return (GenericEventManager)GetEventManagerFromID(eventManagerIDForPosition, createIfNeeded);
	}

	public Vector2 GetBucketWorldPosition(int row, int column)
	{
		Vector3 tileSystemOrigin = m_TileSystemOrigin;
		tileSystemOrigin.x += (float)(8 * row) + 4f;
		tileSystemOrigin.y -= (float)(8 * column) + 4f;
		return tileSystemOrigin;
	}

	public void RegisterManager(EventManager aiEventManager)
	{
		if (!(aiEventManager == null) && !m_AIEventManagerLookupTable.ContainsKey(aiEventManager.GetEventManagerID()))
		{
			m_AIEventManagerLookupTable.Add(aiEventManager.GetEventManagerID(), aiEventManager);
		}
	}

	public void UnRegisterManager(EventManager aiEventManager)
	{
		uint eventManagerID = aiEventManager.GetEventManagerID();
		if (m_AIEventManagerLookupTable.ContainsKey(eventManagerID))
		{
			m_AIEventManagerLookupTable.Remove(eventManagerID);
		}
	}

	public void RegisterEvent(AIEvent aiEvent)
	{
		if (!m_AIEventLookupTable.ContainsKey(aiEvent.GetEventID()))
		{
			uint eventID = aiEvent.GetEventID();
			m_AIEventLookupTable.Add(eventID, aiEvent);
		}
	}

	public void UnRegisterEvent(AIEvent aiEvent)
	{
		m_AIEventLookupTable.Remove(aiEvent.GetEventID());
	}

	public AIEvent GetAIEventFromID(uint fullID)
	{
		AIEvent value = null;
		m_AIEventLookupTable.TryGetValue(fullID, out value);
		if (value == null)
		{
			int iBits = ((!FirstBitSet(fullID)) ? 26 : 13);
			m_bitField.Reset();
			m_bitField = fullID;
			uint uInt = m_bitField.GetUInt(iBits);
			uint uInt2 = m_bitField.GetUInt(6);
			EventManager eventManagerFromID = GetEventManagerFromID(uInt, createIfNeeded: true);
			AIEvent.EventType eventType = AIEvent.EventType.Event_Count;
			if (uInt2 < 32)
			{
				eventType = (AIEvent.EventType)uInt2;
			}
			if (eventManagerFromID != null && eventType != AIEvent.EventType.Event_Count)
			{
				return eventManagerFromID.GetEventByType(eventType);
			}
		}
		return value;
	}

	public static bool IsWellFormed(AIEvent aiEvent)
	{
		return aiEvent.m_Manager != null && aiEvent.m_Manager.m_NetView != null;
	}

	public static uint GetEventIDForEvent(AIEvent aiEvent)
	{
		uint eventManagerID = aiEvent.m_Manager.GetEventManagerID();
		uint eEventType = (uint)aiEvent.m_EventData.m_eEventType;
		int iBits = ((!FirstBitSet(eventManagerID)) ? 26 : 13);
		m_bitField.Reset();
		m_bitField.Set(iBits, eventManagerID);
		m_bitField.Set(6, eEventType);
		return (uint)(ulong)m_bitField;
	}

	public static bool FirstBitSet(uint id)
	{
		m_bitField.Reset();
		m_bitField = id;
		return m_bitField.GetBool();
	}

	public EventManager GetEventManagerFromID(uint eventManagerId, bool createIfNeeded)
	{
		EventManager value = null;
		m_AIEventManagerLookupTable.TryGetValue(eventManagerId, out value);
		if (value != null)
		{
			return value;
		}
		if (createIfNeeded)
		{
			value = RecoverEventManagerFromID(eventManagerId);
		}
		return value;
	}

	private EventManager RecoverEventManagerFromID(uint eventManagerID)
	{
		uint netId = 0u;
		uint row = 0u;
		uint column = 0u;
		uint floor = 0u;
		uint eventHeight = 0u;
		if (ReadEventManagerID(eventManagerID, out netId, out row, out column, out floor, out eventHeight))
		{
			EventManager eventManager = T17NetView.Find<EventManager>((int)netId);
			if (eventManager == null)
			{
			}
			return eventManager;
		}
		EventHeight eventHeight2 = (EventHeight)eventHeight;
		FloorManager.TileSystem_Type systemType = ((eventHeight2 != 0) ? FloorManager.TileSystem_Type.TileSystem_Wall : FloorManager.TileSystem_Type.TileSystem_Ground);
		Vector3 worldPosition = Vector3.zero;
		if (FloorManager.GetInstance().GetTileCentrePosition((int)floor, systemType, (int)row, (int)column, out worldPosition))
		{
			Object original = ((eventHeight2 != 0) ? m_StaticEventWallPrefab : m_StaticEventFloorPrefab);
			GameObject gameObject = Object.Instantiate(original, worldPosition, Quaternion.identity, base.transform) as GameObject;
			GenericEventManager component = gameObject.GetComponent<GenericEventManager>();
			component.m_NetView = m_ManagerNetView;
			component.m_EventHeight = eventHeight2;
			RegisterManager(component);
			return component;
		}
		return null;
	}

	public static uint GetEventManagerIDForNetObject(int netViewId)
	{
		return WriteEventManagerID(isNetId: true, (uint)netViewId, 0u, 0u, 0u, 0u);
	}

	public static uint GetEventManagerIDForPosition(Vector3 position, EventHeight eventHeight)
	{
		int row = 0;
		int column = 0;
		int num = 0;
		FloorManager instance = FloorManager.GetInstance();
		if (instance == null)
		{
			return 0u;
		}
		FloorManager.Floor floor = instance.FindFloorAtZ(position.z);
		num = floor.m_FloorIndex;
		FloorManager.TileSystem_Type systemType = ((eventHeight != 0) ? FloorManager.TileSystem_Type.TileSystem_Wall : FloorManager.TileSystem_Type.TileSystem_Ground);
		instance.GetTileGridPoint(floor, systemType, position, out row, out column);
		return WriteEventManagerID(isNetId: false, 0u, (uint)row, (uint)column, (uint)num, (uint)eventHeight);
	}

	public static bool ReadEventManagerID(uint eventManagerID, out uint netId, out uint row, out uint column, out uint floor, out uint eventHeight)
	{
		netId = 0u;
		row = 0u;
		column = 0u;
		floor = 0u;
		eventHeight = 0u;
		m_bitField.Reset();
		m_bitField = eventManagerID;
		if (m_bitField.GetBool())
		{
			netId = m_bitField.GetUInt(12);
			return true;
		}
		if (m_EventManID_iXBits == -1 || m_EventManID_iYBits == -1)
		{
			FloorManager instance = FloorManager.GetInstance();
			instance.GetFloorMetricsBitLength(0, 20, out m_EventManID_iXBits, out m_EventManID_iYBits);
		}
		row = m_bitField.GetUInt(m_EventManID_iYBits);
		column = m_bitField.GetUInt(m_EventManID_iXBits);
		floor = m_bitField.GetUInt(4);
		eventHeight = m_bitField.GetUInt(1);
		return false;
	}

	public static uint WriteEventManagerID(bool isNetId, uint netId, uint row, uint column, uint floor, uint eventHeight)
	{
		m_bitField.Reset();
		m_bitField.Set(isNetId);
		if (isNetId)
		{
			m_bitField.Set(12, netId);
		}
		else
		{
			if (m_EventManID_iXBits == -1 || m_EventManID_iYBits == -1)
			{
				FloorManager.GetInstance().GetFloorMetricsBitLength(0, 20, out m_EventManID_iXBits, out m_EventManID_iYBits);
			}
			m_bitField.Set(m_EventManID_iYBits, row);
			m_bitField.Set(m_EventManID_iXBits, column);
			m_bitField.Set(4, floor);
			m_bitField.Set(1, eventHeight);
		}
		return (uint)(ulong)m_bitField;
	}
}
