using System;
using System.Collections;
using System.Collections.Generic;
using BitStream;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AICharacter : T17MonoBehaviour, IControlledUpdate
{
	public delegate void OnAIEventCallback(AIEventMemory aiEvent);

	public delegate void AIEventMemoryUpdate(float deltaTime);

	[Serializable]
	public class CharacterSaveData
	{
		public byte[] m_EventMemories;

		public byte[] m_EventsToReport;

		public int m_Personality;

		public bool m_bReleasedKeySpawnedOnUs;

		public bool m_bIsDueMedicBedMissingKeyCheck;
	}

	public Transform m_Transform;

	public Character m_Character;

	public ItemContainer m_ItemContainer;

	public CharacterMovement m_CharacterMovement;

	public CharacterUtil m_CharacterUtil;

	public CharacterStats m_CharacterStats;

	public AIMovement m_AIMovement;

	public AIPatrols m_AIPatrols;

	public T17NetView m_NetView;

	[FormerlySerializedAs("m_GuardActiveAlertness")]
	public PrisonAlertness m_ActiveAlertness;

	private Dictionary<AIEvent.EventType, List<AIEventMemory>> m_EventMemory = new Dictionary<AIEvent.EventType, List<AIEventMemory>>(AIEvent.EventTComparer);

	private Dictionary<AIEvent.EventType, List<OnAIEventCallback>> m_OnEventListeners = new Dictionary<AIEvent.EventType, List<OnAIEventCallback>>(AIEvent.EventTComparer);

	protected List<AICharacter_Guard.ReportData> m_EventsToReport = new List<AICharacter_Guard.ReportData>();

	protected Vector3 m_vStartingLocation;

	protected RoomBlob m_StartingRoom;

	protected Personality.PersonalityType m_CharacterPersonality;

	protected Blackboard m_AIBlackboard;

	private static ItemContainer m_ContrabandDeskItemContainer = null;

	private bool m_bRunning;

	private float m_fSprintTimer;

	private Item m_MagicItemInUse;

	private bool m_bTryingToUseItem;

	private bool m_bTryingToUseMultipleItems;

	protected static Item m_MagicRepairWallItem = null;

	protected static Item m_MagicRepairGroundItem = null;

	protected static Item m_MagicDestroyWallItem = null;

	protected static Item m_MagicDestroyGroundItem = null;

	protected static Item m_MagicDestroyVentGroundItem = null;

	private static WaitForSeconds m_WaitForItemDelay = new WaitForSeconds(0.3f);

	private const float m_kWaitTime = 0.3f;

	public bool m_bBTStateDirty;

	public bool m_bBTStateReset;

	protected bool m_bWasKeyRespawnedOnUs;

	protected bool m_bIsDueMedicBedMissingKeyCheck;

	public bool m_bEnteredCombat;

	private const string COMBAT_BEHAVIOUR = "CombatBehaviour";

	private float m_fTemporaryBlindnessTimer = 2f;

	public float m_fTemporaryBlindnessTime = 1f;

	private int m_DefaultOutfitId = -1;

	private CharacterSaveData m_CharacterSaveData = new CharacterSaveData();

	protected bool m_bRequiresSerialization;

	private static BitStreamWriter ms_bitWriter = null;

	private static FastList<byte> ms_bitWriterList = null;

	private static bool m_bAIDebugOn = GlobalStart.m_bShowDebugElements;

	private static bool m_bAIBehaviourDebugOn = GlobalStart.m_bShowDebugElements;

	private static bool m_bAIHeatDebugOn = GlobalStart.m_bShowDebugElements;

	private const int NUMBER_LOCAL_PLAYERS_TO_DO_LIMIT = 3;

	private static Dictionary<string, Type> m_CachedSystemTypeDict = new Dictionary<string, Type>();

	private List<Character> m_RemoveAllItemsForSolitaryQueue = new List<Character>();

	public Image debug_image;

	public string debug_text;

	private Texture2D _tex;

	private Texture2D tex
	{
		get
		{
			if (_tex == null)
			{
				_tex = new Texture2D(1, 1);
				_tex.SetPixel(0, 0, Color.white);
				_tex.Apply();
			}
			return _tex;
		}
	}

	public event AIEventMemoryUpdate TickMemory;

	public Vector3 GetStartingLocation()
	{
		return m_vStartingLocation;
	}

	public RoomBlob GetStartingRoom()
	{
		return m_StartingRoom;
	}

	public RoomLabel GetSpawnRoomLabel()
	{
		if (m_StartingRoom == null)
		{
			return RoomLabel.None;
		}
		return m_StartingRoom.m_RoomLabel;
	}

	public void SetRunning(bool running)
	{
		m_bRunning = running;
	}

	public void SetSprintTimer(float time)
	{
		m_fSprintTimer = time;
	}

	public bool IsRunning()
	{
		return m_bRunning || m_fSprintTimer > 0f;
	}

	public bool IsSleeping()
	{
		StatModifierEnum characterState = m_CharacterStats.GetCharacterState();
		return characterState == StatModifierEnum.Sleeping || characterState == StatModifierEnum.SleepingInOwnBed || characterState == StatModifierEnum.MedicalSleeping;
	}

	protected override void Awake()
	{
		base.Awake();
		m_AIBlackboard = GetComponent<Blackboard>();
		OnAwake();
	}

	protected virtual void OnDestroy()
	{
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		ForgetEverything();
		CleanUp();
		this.TickMemory = null;
		m_Character = null;
		m_ItemContainer = null;
		m_EventMemory.Clear();
		m_StartingRoom = null;
		m_AIBlackboard = null;
		m_ContrabandDeskItemContainer = null;
		m_MagicItemInUse = null;
		if (m_OnEventListeners != null)
		{
			m_OnEventListeners.Clear();
		}
		if (m_EventsToReport != null)
		{
			m_EventsToReport.Clear();
		}
		m_NetView = null;
		m_CharacterMovement = null;
		m_CharacterUtil = null;
		m_CharacterStats = null;
		m_AIMovement = null;
		m_AIPatrols = null;
	}

	public static void CleanUp()
	{
		m_ContrabandDeskItemContainer = null;
		m_MagicRepairWallItem = null;
		m_MagicRepairGroundItem = null;
		m_MagicDestroyWallItem = null;
		m_MagicDestroyGroundItem = null;
		m_MagicDestroyVentGroundItem = null;
		if (ms_bitWriter != null)
		{
			ms_bitWriterList.Clear();
			ms_bitWriterList = null;
			ms_bitWriter.Reset(null);
			ms_bitWriter = null;
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_vStartingLocation = base.transform.position;
		bool flag = true;
		if (LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison && LevelDetailsManager.c_CurrentLevelDataVersionNumber != LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			flag = false;
		}
		if (!flag)
		{
			flag = true;
			int num = -1;
			LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
			OwnedByZone component = GetComponent<OwnedByZone>();
			if (component != null)
			{
				LevelEditor_ZoneManager.Zone zone = instance.GetZone(component.m_ZoneIndex);
				if (zone != null)
				{
					num = zone.m_AllocatedRoomID;
				}
				UnityEngine.Object.Destroy(component);
			}
			if (num == -1)
			{
				FloorManager instance2 = FloorManager.GetInstance();
				if (instance != null && instance2 != null)
				{
					int row = 0;
					int column = 0;
					int floor = 0;
					Vector3 vStartingLocation = m_vStartingLocation;
					vStartingLocation.x = Mathf.Floor(vStartingLocation.x);
					if (instance2.GetTileGridPointAndFloorIndex(vStartingLocation, FloorManager.TileSystem_Type.TileSystem_ObjectPlops, out row, out column, out floor))
					{
						BaseLevelManager.LevelLayers eLayer = (BaseLevelManager.LevelLayers)floor;
						LevelEditor_ZoneManager.Zone zoneAt = instance.GetZoneAt(column, 119 - row, eLayer);
						if (zoneAt != null)
						{
							num = zoneAt.m_AllocatedRoomID;
						}
					}
				}
			}
			if (num != -1)
			{
				m_StartingRoom = RoomManager.GetInstance().LookUpRoom(num);
				flag = false;
			}
		}
		if (flag)
		{
			m_StartingRoom = RoomManager.GetInstance().LookUpRoom(m_vStartingLocation);
		}
		GetMagicItems();
		NPCManager.GetInstance().AddAICharacter(this);
		if (!PrisonSnapshotIO.IsThereSaveData())
		{
			m_CharacterPersonality = (Personality.PersonalityType)UnityEngine.Random.Range(0, 4);
			m_bRequiresSerialization = true;
			UpdateCombatBehaviour();
		}
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
		ItemData defaultStartingItem = m_Character.GetDefaultStartingItem(outfit: true);
		m_DefaultOutfitId = ((!(defaultStartingItem == null)) ? defaultStartingItem.m_ItemDataID : (-1));
		OnStart();
		return base.StartInit();
	}

	private void GetMagicItems()
	{
		m_MagicRepairWallItem = m_MagicRepairWallItem ?? ItemManager.GetInstance().GetMagicAIRepairWallItem();
		m_MagicRepairGroundItem = m_MagicRepairGroundItem ?? ItemManager.GetInstance().GetMagicAIRepairGroundItem();
		m_MagicDestroyWallItem = m_MagicDestroyWallItem ?? ItemManager.GetInstance().GetMagicAIDestroyWallItem();
		m_MagicDestroyGroundItem = m_MagicDestroyGroundItem ?? ItemManager.GetInstance().GetMagicAIDestroyGroundItem();
		m_MagicDestroyVentGroundItem = m_MagicDestroyVentGroundItem ?? ItemManager.GetInstance().GetMagicAIDestroyVentGroundItem();
		if (m_MagicRepairWallItem == null || m_MagicRepairGroundItem == null || m_MagicDestroyWallItem == null || m_MagicDestroyGroundItem == null || m_MagicDestroyVentGroundItem == null)
		{
			T17NetManager.LogGoogleException("One of the critical AI items was NULL, this will likely result in stuck NPCs");
		}
	}

	public virtual void ControlledUpdate()
	{
		if (!IsInited())
		{
			return;
		}
		if (this.TickMemory != null)
		{
			this.TickMemory(UpdateManager.deltaTime);
		}
		if (m_fSprintTimer > 0f)
		{
			m_fSprintTimer -= UpdateManager.deltaTime;
		}
		if (m_fTemporaryBlindnessTimer > 0f)
		{
			GlobalStart instance = GlobalStart.GetInstance();
			if (instance != null && instance.IsWithinLevel())
			{
				m_fTemporaryBlindnessTimer -= UpdateManager.deltaTime;
			}
		}
		ProcessRemoveAllItemsForSolitaryQueue();
		OnUpdate();
		if (m_bRequiresSerialization)
		{
			RequiresSerialization();
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void OnPostBTTick()
	{
		if (m_bBTStateDirty)
		{
			m_bBTStateDirty = false;
		}
		if (m_bBTStateReset)
		{
			m_bBTStateReset = false;
			m_bBTStateDirty = true;
		}
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnAddingEventToMemory(AIEvent aiEvent, AIEventMemory memory, bool silent)
	{
	}

	protected virtual void OnMemoryForgotten(AIEventMemory memory)
	{
	}

	protected virtual void OnForgetEverything()
	{
	}

	public virtual void OnMedicBedInteractionStarted()
	{
	}

	public virtual void OnMedicBedInteractionEnded()
	{
	}

	protected virtual void OnBecameMasterClientBody()
	{
	}

	public void OnBecameMasterClient()
	{
		if (m_Character.IsInteracting())
		{
			InteractiveObject interactiveObject = m_Character.GetInteractiveObject();
			if (interactiveObject == null)
			{
				interactiveObject = m_Character.GetRemoteInteractiveObject();
			}
			interactiveObject.RaiseInteractionEndedForHostMigration();
			m_Character.SetRemoteInteractiveObject(0, 0);
		}
		OnBecameMasterClientBody();
	}

	public void ForgetEvent(AIEvent aiEvent)
	{
		AIEventMemory aIEventMemory = FindEventInMemory(aiEvent);
		if (aIEventMemory != null)
		{
			ForgetEvent(aIEventMemory);
		}
	}

	public void ForgetEvent(AIEventMemory aiEventMemory)
	{
		if (aiEventMemory != null)
		{
			aiEventMemory.m_AIEvent.ReturnSlot(m_Character.m_CharacterRole, this);
			aiEventMemory.OnForgetMemory();
			OnMemoryForgotten(aiEventMemory);
			m_bRequiresSerialization = true;
			AIEvent.EventType eEventType = aiEventMemory.m_eEventType;
			if (m_EventMemory.ContainsKey(eEventType) && m_EventMemory[eEventType].Contains(aiEventMemory))
			{
				m_EventMemory[eEventType].Remove(aiEventMemory);
			}
		}
	}

	public void OnKnockedOut()
	{
		m_bEnteredCombat = false;
		if (m_Character != null && (m_Character.m_CharacterRole == CharacterRole.Guard || m_Character.m_CharacterRole == CharacterRole.Inmate))
		{
			ForgetEverything();
		}
	}

	public void ForgetEverything()
	{
		foreach (KeyValuePair<AIEvent.EventType, List<AIEventMemory>> item in m_EventMemory)
		{
			List<AIEventMemory> value = item.Value;
			if (value != null)
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					ForgetEvent(value[num]);
				}
				item.Value.Clear();
			}
		}
		OnForgetEverything();
		m_bRequiresSerialization = true;
		m_EventMemory.Clear();
	}

	public void FlagEventsToForget(Character character, List<AIEvent.EventType> eventTypes)
	{
		if (character == null || eventTypes == null)
		{
			return;
		}
		for (int i = 0; i < eventTypes.Count; i++)
		{
			AIEvent.EventType eventType = eventTypes[i];
			List<AIEventMemory> eventMemories = GetEventMemories(eventType);
			if (eventMemories == null)
			{
				continue;
			}
			for (int j = 0; j < eventMemories.Count; j++)
			{
				AIEventMemory aIEventMemory = eventMemories[j];
				if (aIEventMemory != null && aIEventMemory.m_TargetCharacter == character)
				{
					aIEventMemory.m_bEventValid = false;
					aIEventMemory.m_bFlagToForget = true;
				}
			}
		}
	}

	public bool RememberEvent(ref List<AIEventMemory> events, AIEvent aiEvent, out AIEventMemory eventMemory)
	{
		if (aiEvent.SlotsAvaliable(m_Character.m_CharacterRole))
		{
			float slotPosition = aiEvent.TakeSlot(m_Character.m_CharacterRole, this);
			eventMemory = new AIEventMemory(aiEvent, this, slotPosition);
			if (aiEvent.m_CharacterResponsible != null && aiEvent.m_CharacterResponsible.m_bIsDisguised)
			{
				AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
				if (aiConfig.DisguiseableEvents.Contains(aiEvent.m_EventData.m_eEventType))
				{
					return false;
				}
			}
			if (events.Count > 1 && m_Character.m_CharacterRole == CharacterRole.Medic && (aiEvent.m_EventData.m_eEventType == AIEvent.EventType.Character_KnockedOut || aiEvent.m_EventData.m_eEventType == AIEvent.EventType.Character_Escaping) && aiEvent.m_TargetCharacter != null && aiEvent.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
			{
				events.Insert(1, eventMemory);
			}
			else
			{
				events.Add(eventMemory);
			}
			return true;
		}
		eventMemory = null;
		return false;
	}

	public void AddEvent(AIEvent aiEvent, bool silent = false)
	{
		if (!T17NetManager.IsMasterClient || aiEvent == null || aiEvent.m_EventData == null)
		{
			return;
		}
		AIEventMemory aIEventMemory = null;
		if (m_EventMemory.TryGetValue(aiEvent.m_EventData.m_eEventType, out var value))
		{
			aIEventMemory = FindEventInMemory(aiEvent);
		}
		else
		{
			value = new List<AIEventMemory>();
			m_EventMemory[aiEvent.m_EventData.m_eEventType] = value;
		}
		if (aIEventMemory != null)
		{
			return;
		}
		AIEventMemory eventMemory = null;
		if (!RememberEvent(ref value, aiEvent, out eventMemory))
		{
			return;
		}
		OnAddingEventToMemory(aiEvent, eventMemory, silent);
		m_bRequiresSerialization |= !silent;
		List<OnAIEventCallback> value2 = null;
		m_OnEventListeners.TryGetValue(aiEvent.m_EventData.m_eEventType, out value2);
		if (value2 != null)
		{
			int count = value2.Count;
			for (int i = 0; i < count; i++)
			{
				value2[i](eventMemory);
			}
		}
	}

	protected AIEventMemory FindEventInMemory(AIEvent aiEvent)
	{
		List<AIEventMemory> value = null;
		if (!m_EventMemory.TryGetValue(aiEvent.m_EventData.m_eEventType, out value))
		{
			return null;
		}
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i].GetEventID() == aiEvent.GetEventID())
			{
				return value[i];
			}
		}
		return null;
	}

	public void ListenForEvent(OnAIEventCallback action, AIEvent.EventType eventType)
	{
		List<OnAIEventCallback> value = null;
		m_OnEventListeners.TryGetValue(eventType, out value);
		if (value == null)
		{
			value = new List<OnAIEventCallback>();
			m_OnEventListeners[eventType] = value;
		}
		m_OnEventListeners[eventType].Add(action);
	}

	public bool KnownEvent(AIEvent.EventType eventType)
	{
		return m_EventMemory != null && m_EventMemory.ContainsKey(eventType) && m_EventMemory[eventType].Count > 0;
	}

	public List<AIEventMemory> GetEventMemories(AIEvent.EventType eventType)
	{
		if (m_EventMemory == null || !m_EventMemory.ContainsKey(eventType) || m_EventMemory[eventType].Count == 0)
		{
			return null;
		}
		return m_EventMemory[eventType];
	}

	public bool KnownEvents(AIEvent.EventType[] eventTypes, out AIEvent.EventType eventTypeFound)
	{
		eventTypeFound = AIEvent.EventType.Event_Count;
		if (m_EventMemory == null || eventTypes == null)
		{
			return false;
		}
		foreach (AIEvent.EventType eventType in eventTypes)
		{
			if (m_EventMemory.ContainsKey(eventType) && m_EventMemory[eventType].Count > 0)
			{
				eventTypeFound = eventType;
				return true;
			}
		}
		return false;
	}

	public AIEventMemory GetLastKnownEvent(AIEvent.EventType eventType)
	{
		List<AIEventMemory> knownEvents = GetKnownEvents(eventType);
		if (knownEvents != null && knownEvents.Count > 0)
		{
			return knownEvents[knownEvents.Count - 1];
		}
		return null;
	}

	public AIEventMemory GetFirstKnownEvent(AIEvent.EventType eventType)
	{
		List<AIEventMemory> knownEvents = GetKnownEvents(eventType);
		if (knownEvents != null && knownEvents.Count > 0)
		{
			return knownEvents[0];
		}
		return null;
	}

	public List<AIEventMemory> GetKnownEvents(AIEvent.EventType eventType)
	{
		if (!m_EventMemory.TryGetValue(eventType, out var value))
		{
			value = new List<AIEventMemory>();
			m_EventMemory[eventType] = value;
		}
		return value;
	}

	public int GetKnownEventCount(AIEvent.EventType eventType)
	{
		if (!m_EventMemory.TryGetValue(eventType, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public bool CanSeeEvent(AIEventMemory aiEvent)
	{
		GameObject target = aiEvent.GetTarget();
		bool haveCollisionData = false;
		return m_CharacterUtil.LineOfSight(target.transform.position, out haveCollisionData);
	}

	public GameObject GetTarget(AIEventMemory aiEvent)
	{
		return aiEvent.m_Target;
	}

	public bool HasCharacterResponsible(AIEventMemory aiEvent)
	{
		return aiEvent.m_CharacterResponsible != null;
	}

	public bool CharacterResponsibleAlive(AIEventMemory aiEvent)
	{
		return aiEvent.m_CharacterResponsible != null && !aiEvent.m_CharacterResponsible.m_bIsKnockedOut && !aiEvent.m_CharacterResponsible.GetIsDisabled();
	}

	public RoutinesData.Routine GetCurrentRoutine()
	{
		return RoutineManager.GetInstance().GetCurrentRoutine();
	}

	public bool CombatEnded()
	{
		bool bEnteredCombat = m_bEnteredCombat;
		m_bEnteredCombat = false;
		if (bEnteredCombat)
		{
			m_Character.CombatBlock(doBlock: false);
			m_Character.ResetChargeAttack();
			m_Character.SetCharacterTarget(null);
		}
		return bEnteredCombat;
	}

	public bool HasDefaultOutfit()
	{
		Item outFit = m_Character.GetOutFit();
		if (outFit != null)
		{
			return m_DefaultOutfitId == outFit.ItemDataID;
		}
		return m_DefaultOutfitId <= 0;
	}

	public static Type GetSystemType(string name)
	{
		if (m_CachedSystemTypeDict.ContainsKey(name))
		{
			return m_CachedSystemTypeDict[name];
		}
		Type type = Type.GetType(name, throwOnError: false, ignoreCase: true);
		m_CachedSystemTypeDict.Add(name, type);
		return type;
	}

	public InteractiveObject FindObject(RoomBlob.eLocation location, string interactionType, bool findFreeTimeObjects, bool filterReserved = true)
	{
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(location);
		if (location == RoomBlob.eLocation.ShowTime && CameraManager.GetInstance().GetUsedCameraCount() > 3 && !NPCManager.GetInstance().AllowToTakePartInShowTime(m_Character.m_CharacterID))
		{
			return null;
		}
		if (allRoomsByLocation.Count == 0)
		{
			return null;
		}
		List<InteractiveObject> list = new List<InteractiveObject>();
		bool flag = m_Character.m_CharacterRole == CharacterRole.Inmate;
		Type systemType = GetSystemType(interactionType);
		for (int num = allRoomsByLocation.Count - 1; num >= 0; num--)
		{
			RoomBlob roomBlob = allRoomsByLocation[num];
			Vector3 position = roomBlob.transform.position;
			bool searchFreeTime = findFreeTimeObjects;
			bool isInmate = flag;
			Type interactiveObjectType = systemType;
			Vector3? closeTo = position;
			List<InteractiveObject> list2 = roomBlob.FindObject(searchFreeTime, isInmate, interactiveObjectType, null, closeTo, filterReserved);
			if (list2.Count > 0)
			{
				list.AddRange(list2);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		int num2 = list.Count - 1;
		int index = (int)(Mathf.Abs(UnityEngine.Random.Range(-1f, 1f) + UnityEngine.Random.Range(-1f, 1f) + UnityEngine.Random.Range(-1f, 1f)) * (float)num2 / 3f);
		return list[index];
	}

	public List<RoomWaypoint> GetRoomWaypoints(RoomBlob.eLocation location)
	{
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(location);
		if (allRoomsByLocation == null || allRoomsByLocation.Count == 0)
		{
			return null;
		}
		return allRoomsByLocation[0].GetWaypointList();
	}

	public virtual void OnRegainConsciousness()
	{
		m_fTemporaryBlindnessTimer = m_fTemporaryBlindnessTime;
	}

	public virtual void OnEscapeBindings()
	{
	}

	public bool IsTemporaryBlind()
	{
		return m_fTemporaryBlindnessTimer > 0f;
	}

	public bool HasOutfit()
	{
		return m_Character.GetOutFit() != null;
	}

	public virtual void EquipDefaultOutfit()
	{
		m_Character.EquipStartingOutfit();
	}

	public void RemoveAllNonJobRelatedItems(bool considerFluffOnly = true, List<int> ignoreItems = null, bool removeEquippedItem = false)
	{
		if (ignoreItems == null)
		{
			ignoreItems = new List<int>();
		}
		BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(m_Character);
		if (charactersJob != null)
		{
			ItemData[] jobRelatedItems = charactersJob.GetJobRelatedItems();
			if (jobRelatedItems != null)
			{
				for (int i = 0; i < jobRelatedItems.Length; i++)
				{
					ignoreItems.Add(jobRelatedItems[i].m_ItemDataID);
				}
			}
		}
		RemoveAllInventoryItems(considerFluffOnly, ignoreItems, removeEquippedItem);
	}

	public void RemoveAllInventoryItems(bool considerFluffOnly = true, List<int> ignoreItems = null, bool removeEquippedItem = false)
	{
		if (removeEquippedItem)
		{
			Item equippedItem = m_Character.GetEquippedItem();
			if (equippedItem != null)
			{
				m_Character.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
				if (!equippedItem.IsMagicItem())
				{
					ItemManager.GetInstance().RequestReleaseItem(equippedItem);
				}
			}
		}
		EnsureWeHaveNFreeInventorySlots(considerFluffOnly, ignoreItems);
	}

	public void EnsureWeHaveNFreeInventorySlots(bool considerFluffOnly = true, List<int> ignoreItems = null, int numberOfWantedFreeSlots = -1)
	{
		if (!(m_ItemContainer != null))
		{
			return;
		}
		int itemCount = m_ItemContainer.GetItemCount();
		for (int num = itemCount - 1; num >= 0; num--)
		{
			if (numberOfWantedFreeSlots != -1)
			{
				int num2 = m_ItemContainer.m_MaxSize - m_ItemContainer.GetItemCount();
				if (num2 >= numberOfWantedFreeSlots)
				{
					break;
				}
			}
			Item item = m_ItemContainer.GetItem(num);
			if (!(item == null))
			{
				bool flag = true;
				flag &= ignoreItems == null || !ignoreItems.Contains(item.ItemDataID);
				if (flag & (considerFluffOnly && !item.IsQuestItem()))
				{
					m_ItemContainer.RemoveItemRPC(item, releaseToManager: true);
				}
			}
		}
	}

	public bool MoveItemToContrabandDesk(Item item)
	{
		if (m_ContrabandDeskItemContainer == null)
		{
			GameObject gameObject = null;
			List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.ContrabandRoom);
			if (allRoomsByLocation == null)
			{
				return false;
			}
			for (int i = 0; i < allRoomsByLocation.Count; i++)
			{
				RoomBlob roomBlob = allRoomsByLocation[i];
				if (!(roomBlob == null))
				{
					RoomBlob_ContrabandRoom roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_ContrabandRoom>();
					if (!(roomBlobData == null) && !(roomBlobData.m_Desk == null))
					{
						gameObject = roomBlobData.m_Desk.gameObject;
						break;
					}
				}
			}
			if (gameObject != null)
			{
				m_ContrabandDeskItemContainer = gameObject.GetComponent<ItemContainer>();
			}
		}
		if (m_ContrabandDeskItemContainer == null)
		{
			return false;
		}
		bool flag = m_ContrabandDeskItemContainer.AddItemRPC(item);
		if (!flag)
		{
		}
		return flag;
	}

	public void RemoveAllContraband(AIEventMemory m_EventMemory)
	{
		if (m_EventMemory == null)
		{
			return;
		}
		GameObject target = m_EventMemory.GetTarget();
		if (target == null || (m_EventMemory.m_TargetCharacter != null && m_EventMemory.m_TargetCharacter.m_CharacterRole != 0))
		{
			return;
		}
		ItemContainer componentInChildren = target.GetComponentInChildren<ItemContainer>();
		if (componentInChildren == null || GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum() == LevelScript.PRISON_ENUM.Tutorial)
		{
			return;
		}
		int alertnessToIncrease;
		bool flag = RemoveAllContrabandFromContainer(componentInChildren, out alertnessToIncrease);
		if (flag | RemoveContrabandFromCharacter(m_EventMemory.m_TargetCharacter))
		{
			bool flag2 = ((m_EventMemory.m_TargetCharacter != null && m_EventMemory.m_TargetCharacter.m_CharacterStats.m_bIsPlayer) ? true : false);
			int num = ((!flag2) ? 5 : 10);
			SpeechManager instance = SpeechManager.GetInstance();
			Character character = m_Character;
			string textID = "Text.Guard.SentToContrabandDesk";
			SpeechTone tone = SpeechTone.Negative;
			float duration = 1f;
			bool bAllowTextRecolour = flag2;
			int priority = num;
			instance.SaySomething(character, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
			if (alertnessToIncrease > 0)
			{
				Character characterResponsible = ((!(m_EventMemory.m_TargetCharacter != null)) ? componentInChildren.GetCharacterOwner() : m_EventMemory.m_TargetCharacter);
				PrisonAlertnessManager.AlertnessReason reason = ((componentInChildren.m_ContainerType != ItemContainer.ItemContainerType.Inmate) ? PrisonAlertnessManager.AlertnessReason.ContrabandInContainer : PrisonAlertnessManager.AlertnessReason.HasContraband);
				PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(alertnessToIncrease, characterResponsible, reason, punishCharacter: false);
			}
		}
	}

	private bool RemoveAllContrabandFromContainer(ItemContainer itemContainer, out int alertnessToIncrease)
	{
		bool result = false;
		alertnessToIncrease = 0;
		if (itemContainer != null)
		{
			List<Item> contrabandItems = null;
			if (itemContainer.HasContrabandItems(ref contrabandItems))
			{
				alertnessToIncrease = PrisonAlertnessManager.GetMaxAlertnessIncreaseForContrabandItems(contrabandItems);
				result = true;
				for (int num = contrabandItems.Count - 1; num >= 0; num--)
				{
					Item item = contrabandItems[num];
					bool flag = MoveItemToContrabandDesk(item);
					itemContainer.RemoveItemRPC(item, !flag);
				}
			}
		}
		return result;
	}

	private bool RemoveContrabandFromCharacter(Character targetCharacter, bool removeEquippedItem = true, bool removeOutfit = true)
	{
		bool result = false;
		if (targetCharacter != null)
		{
			if (removeEquippedItem)
			{
				Item equippedItem = targetCharacter.GetEquippedItem();
				if (equippedItem != null && equippedItem.m_ItemData != null && equippedItem.m_ItemData.IsContraband() && !equippedItem.IsMagicItem())
				{
					result = true;
					targetCharacter.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
					if (!MoveItemToContrabandDesk(equippedItem))
					{
						ItemManager.GetInstance().RequestReleaseItem(equippedItem.m_NetView.viewID);
					}
				}
			}
			if (removeOutfit)
			{
				Item outFit = targetCharacter.GetOutFit();
				if (outFit != null && outFit.m_ItemData != null && outFit.m_ItemData.IsContraband() && targetCharacter.m_CharacterRole == CharacterRole.Inmate)
				{
					result = true;
					targetCharacter.SetOutFit(null, bTellOthers: true, bAddOldToInventory: false);
					if (!MoveItemToContrabandDesk(outFit))
					{
						ItemManager.GetInstance().RequestReleaseItem(outFit.m_NetView.viewID);
					}
				}
			}
		}
		return result;
	}

	public void RemoveAllItemsForSolitary(AIEventMemory m_EventMemory)
	{
		Character targetCharacter = m_EventMemory.m_TargetCharacter;
		if (!(targetCharacter == null))
		{
			m_RemoveAllItemsForSolitaryQueue.Add(targetCharacter);
			UpdateManager.AquireHeavyCpuLock();
		}
	}

	private void ProcessRemoveAllItemsForSolitaryQueue()
	{
		if (m_RemoveAllItemsForSolitaryQueue.Count == 0 || !UpdateManager.AquireHeavyCpuLock())
		{
			return;
		}
		Character character = m_RemoveAllItemsForSolitaryQueue[0];
		m_RemoveAllItemsForSolitaryQueue.RemoveAt(0);
		ItemContainer componentInChildren = character.GetComponentInChildren<ItemContainer>();
		if (componentInChildren == null)
		{
			return;
		}
		List<Item> items = new List<Item>(componentInChildren.GetItemCount());
		componentInChildren.GetItems(ref items);
		bool flag = items.Count > 0;
		int num = PrisonAlertnessManager.GetMaxAlertnessIncreaseForContrabandItems(items);
		int num2 = items.Count - 1;
		for (int num3 = num2; num3 >= 0; num3--)
		{
			Item item = items[num3];
			if (item.m_bIsAMagicItem)
			{
				componentInChildren.RemoveItemRPC(item);
			}
			else
			{
				bool flag2 = false;
				ItemData itemData = item.m_ItemData;
				if (itemData != null && itemData.IsContraband())
				{
					flag2 = MoveItemToContrabandDesk(item);
				}
				componentInChildren.RemoveItemRPC(item, !flag2);
			}
		}
		Item equippedItem = character.GetEquippedItem();
		if (equippedItem != null)
		{
			ItemData itemData2 = equippedItem.m_ItemData;
			if (itemData2 != null)
			{
				flag = true;
				character.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
				if (itemData2.IsContraband())
				{
					num = Mathf.Max(itemData2.m_AlertnessIncreaseWhenFound, num);
					if (!MoveItemToContrabandDesk(equippedItem))
					{
						ItemManager.GetInstance().RequestReleaseItem(equippedItem.m_NetView.viewID);
					}
				}
			}
		}
		bool flag3 = false;
		DeskInteraction myDesk = character.GetMyDesk();
		if (myDesk != null)
		{
			int alertnessToIncrease = 0;
			flag3 = RemoveAllContrabandFromContainer(myDesk.m_LinkedItemContainer, out alertnessToIncrease);
			num = Mathf.Max(alertnessToIncrease, num);
		}
		Item outFit = character.GetOutFit();
		if (outFit != null && outFit.m_ItemData.IsContraband())
		{
			num = Mathf.Max(outFit.m_ItemData.m_AlertnessIncreaseWhenFound, num);
		}
		flag3 |= RemoveContrabandFromCharacter(character, removeEquippedItem: false);
		if (flag || flag3)
		{
			bool bIsPlayer = character.m_CharacterStats.m_bIsPlayer;
			int num4 = ((!bIsPlayer) ? 5 : 10);
			SpeechManager instance = SpeechManager.GetInstance();
			Character character2 = m_Character;
			string textID = "Text.Guard.SentToContrabandDesk";
			SpeechTone tone = SpeechTone.Negative;
			float duration = 1f;
			bool bAllowTextRecolour = bIsPlayer;
			int priority = num4;
			instance.SaySomething(character2, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
		}
		PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(num, character, PrisonAlertnessManager.AlertnessReason.HasContraband, punishCharacter: false);
	}

	public void DestroyAllItems(AIEventMemory m_EventMemory)
	{
		GameObject target = m_EventMemory.GetTarget();
		if (target == null)
		{
			return;
		}
		ItemContainer componentInChildren = target.GetComponentInChildren<ItemContainer>();
		if (componentInChildren == null)
		{
			return;
		}
		componentInChildren.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		if (m_EventMemory.m_TargetCharacter != null)
		{
			Item equippedItem = m_EventMemory.m_TargetCharacter.GetEquippedItem();
			if (equippedItem != null && equippedItem.m_ItemData != null)
			{
				m_EventMemory.m_TargetCharacter.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
				ItemManager.GetInstance().RequestReleaseItem(equippedItem.m_NetView.viewID);
			}
		}
	}

	protected void RandomAttack()
	{
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.LightsOut || RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.RollCall || m_CharacterStats.Energy < 30f || m_CharacterStats.Health < 30f)
		{
			return;
		}
		RoomBlob currentLocation = m_Character.m_CurrentLocation;
		if (!(currentLocation != null))
		{
			return;
		}
		List<Character> charactersInRoom = currentLocation.GetCharactersInRoom();
		for (int i = 0; i < charactersInRoom.Count; i++)
		{
			Character character = charactersInRoom[i];
			if (!(character == m_Character) && character.m_CharacterRole == CharacterRole.Inmate && !character.m_CharacterStats.m_bIsPlayer && !character.GetIsKnockedOut() && !character.m_bIsBound)
			{
				CharacterEventManager characterEventManager = character.m_CharacterEventManager;
				if (characterEventManager != null)
				{
					AIEvent attackingAIEvent = characterEventManager.GetAttackingAIEvent();
					AddEvent(attackingAIEvent, silent: true);
					break;
				}
			}
		}
	}

	public bool HasTray()
	{
		return m_Character.GetHasTray();
	}

	public void RepairTile(Vector3 position, AIEventManager.EventHeight eventHeight)
	{
		bool wallHeight = eventHeight == AIEventManager.EventHeight.Wall;
		UseMagicItem(position, wallHeight, repair: true);
	}

	public void RepairTileHoleThenWall(Vector3 position)
	{
		TargetTilePosition(position);
		Item magicItem = GetMagicItem(wallHeight: false, repair: true);
		Item magicItem2 = GetMagicItem(wallHeight: true, repair: true);
		StartCoroutine(WaitThenUseItems(magicItem, magicItem2));
	}

	public void DestroyTile(Vector3 position, AIEventManager.EventHeight eventHeight)
	{
		bool wallHeight = eventHeight == AIEventManager.EventHeight.Wall;
		UseMagicItem(position, wallHeight, repair: false);
	}

	private void UseMagicItem(Vector3 position, bool wallHeight, bool repair)
	{
		TargetTilePosition(position);
		DamagableTile.DamageAction tileDamageAction = GetTileDamageAction(ref position, wallHeight);
		Item magicItem = GetMagicItem(wallHeight, repair, tileDamageAction);
		if (!(magicItem == null))
		{
			StartCoroutine(WaitThenUseItem(magicItem));
		}
	}

	private DamagableTile.DamageAction GetTileDamageAction(ref Vector3 position, bool wallHeight)
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(position.z);
		FloorManager.TileSystem_Type systemType = (wallHeight ? FloorManager.TileSystem_Type.TileSystem_Wall : FloorManager.TileSystem_Type.TileSystem_Ground);
		FloorManager.GetInstance().GetTileGridPoint(floor, systemType, position, out var row, out var column);
		DamagableTile damagableTile = FloorManager.GetInstance().GetDamagableTile(floor, systemType, row, column);
		if (damagableTile != null)
		{
			return damagableTile.m_DamageAction;
		}
		return DamagableTile.DamageAction.Dig;
	}

	private IEnumerator WaitThenUseItems(params Item[] items)
	{
		m_bTryingToUseMultipleItems = true;
		foreach (Item itemToUse in items)
		{
			yield return StartCoroutine(WaitThenUseItem(itemToUse));
		}
		m_bTryingToUseMultipleItems = false;
	}

	private void TargetTilePosition(Vector3 position)
	{
		FloorManager.GetInstance().GetTileGridPoint(m_Character.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, position, out var row, out var column);
		m_Character.SetTargetTile(row, column);
		m_Character.CalcFaceDirection(position - base.transform.position);
	}

	private Item GetMagicItem(bool wallHeight, bool repair, DamagableTile.DamageAction damageAction = DamagableTile.DamageAction.Dig)
	{
		GetMagicItems();
		Item item = null;
		item = (wallHeight ? ((!repair) ? m_MagicDestroyWallItem : m_MagicRepairWallItem) : (repair ? m_MagicRepairGroundItem : ((damageAction != DamagableTile.DamageAction.Hole) ? m_MagicDestroyVentGroundItem : m_MagicDestroyGroundItem)));
		if (item == null)
		{
		}
		return item;
	}

	public bool MagicItemInUse()
	{
		return m_MagicItemInUse != null;
	}

	public bool ImmobilisingMagicItemInUse()
	{
		return m_MagicItemInUse != null && m_MagicItemInUse.IsImmobilisingOwner();
	}

	public bool TryingToUseItem()
	{
		return m_bTryingToUseItem && m_bTryingToUseMultipleItems;
	}

	private IEnumerator WaitThenUseItem(Item itemWeWantToUse)
	{
		m_bTryingToUseItem = true;
		while ((m_MagicItemInUse != null && m_MagicItemInUse.IsInUse()) || (itemWeWantToUse != null && itemWeWantToUse.IsInUse()))
		{
			m_Character.PauseMovement(0.3f);
			yield return m_WaitForItemDelay;
		}
		bool equippedItemIsMagic2 = EquippedItemIsMagicItem();
		m_Character.SetEquippedItem(null, bTellOthers: true, !equippedItemIsMagic2);
		m_MagicItemInUse = itemWeWantToUse;
		m_MagicItemInUse.SetOwner(m_Character);
		m_Character.SetEquippedItem(m_MagicItemInUse, bTellOthers: true, bAddOldToInventory: false);
		m_MagicItemInUse.Use();
		while (itemWeWantToUse.IsInUse())
		{
			yield return null;
		}
		equippedItemIsMagic2 = EquippedItemIsMagicItem();
		m_Character.SetEquippedItem(null, bTellOthers: true, !equippedItemIsMagic2);
		m_MagicItemInUse = null;
		m_bTryingToUseItem = false;
	}

	private bool EquippedItemIsMagicItem()
	{
		Item equippedItem = m_Character.GetEquippedItem();
		return equippedItem != null && equippedItem.m_bIsAMagicItem;
	}

	public InteractiveObject PickRandomInteractiveObject(List<InteractiveObject> interactiveObjects)
	{
		if (interactiveObjects == null || interactiveObjects.Count == 0)
		{
			return null;
		}
		return interactiveObjects[UnityEngine.Random.Range(0, interactiveObjects.Count)];
	}

	public void UpdateCombatBehaviour()
	{
		if ((m_Character.m_CharacterRole != 0 && m_Character.m_CharacterRole != CharacterRole.Guard) || m_AIBlackboard == null)
		{
			return;
		}
		AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
		if (!(aiConfig == null))
		{
			BehaviourTree combatBehaviour = aiConfig.GetCombatBehaviour(m_CharacterPersonality);
			if (!(combatBehaviour == null))
			{
				m_AIBlackboard.SetValue("CombatBehaviour", combatBehaviour);
			}
		}
	}

	public void SetRobinsonWanderingSpeech()
	{
		if (m_Character.m_CharacterRole == CharacterRole.Inmate && !(m_AIBlackboard == null))
		{
			m_AIBlackboard.SetValue("WanderSpeechTag", "&Text.Robinson.Banter");
		}
	}

	public void TakeAllItemsFromOpenContainer()
	{
		if (!(m_Character.m_OpenContainer == null))
		{
			m_Character.m_OpenContainer.MoveItemsToAnotherContainer(m_ItemContainer, includeHidden: false);
		}
	}

	public bool IsInCombatState()
	{
		return m_bEnteredCombat;
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public void SetReleasedKeyRespawnedOnUs(bool wasRespawned)
	{
		m_bWasKeyRespawnedOnUs = wasRespawned;
	}

	public virtual void QueryCurrentNode(GraphNode node)
	{
	}

	public void RequiresSerialization()
	{
		m_bRequiresSerialization = true;
		NPCManager.GetInstance().TriggerAICharacterSerialization();
	}

	public CharacterSaveData SerialiseCharacter()
	{
		if (!m_bRequiresSerialization)
		{
			return m_CharacterSaveData;
		}
		if (ms_bitWriter == null)
		{
			ms_bitWriterList = new FastList<byte>();
			ms_bitWriter = new BitStreamWriter(ms_bitWriterList);
		}
		else
		{
			ms_bitWriterList.Clear();
			ms_bitWriter.Reset(ms_bitWriterList);
		}
		foreach (KeyValuePair<AIEvent.EventType, List<AIEventMemory>> item in m_EventMemory)
		{
			List<AIEventMemory> value = item.Value;
			if (value != null)
			{
				for (int i = 0; i < value.Count; i++)
				{
					AIEventMemory aIEventMemory = value[i];
					WriteEventMemory(ref ms_bitWriter, ref aIEventMemory.m_AIEvent);
				}
			}
		}
		m_CharacterSaveData.m_EventMemories = ms_bitWriterList.ToArray();
		m_CharacterSaveData.m_bReleasedKeySpawnedOnUs = m_bWasKeyRespawnedOnUs;
		m_CharacterSaveData.m_bIsDueMedicBedMissingKeyCheck = m_bIsDueMedicBedMissingKeyCheck;
		ms_bitWriterList.Clear();
		ms_bitWriter.Reset(ms_bitWriterList);
		if (m_EventsToReport != null)
		{
			for (int j = 0; j < m_EventsToReport.Count; j++)
			{
				AIEvent aiEvent = m_EventsToReport[j].m_Event;
				if (aiEvent != null)
				{
					WriteEventMemory(ref ms_bitWriter, ref aiEvent);
				}
			}
		}
		m_CharacterSaveData.m_EventsToReport = ms_bitWriterList.ToArray();
		m_CharacterSaveData.m_Personality = (int)m_CharacterPersonality;
		m_bRequiresSerialization = false;
		return m_CharacterSaveData;
	}

	public void DeserialiseCharacter(CharacterSaveData data)
	{
		if (data == null)
		{
			return;
		}
		m_CharacterSaveData = data;
		m_bWasKeyRespawnedOnUs = m_CharacterSaveData.m_bReleasedKeySpawnedOnUs;
		m_bIsDueMedicBedMissingKeyCheck = m_CharacterSaveData.m_bIsDueMedicBedMissingKeyCheck;
		byte[] eventMemories = m_CharacterSaveData.m_EventMemories;
		AIEventManager instance = AIEventManager.GetInstance();
		BitStreamReader bitReader = null;
		if (eventMemories != null && eventMemories.Length > 0 && instance != null)
		{
			bitReader = new BitStreamReader(eventMemories);
			int num = eventMemories.Length * 8;
			uint smallestRead = 0u;
			while (num >= smallestRead)
			{
				AIEvent aiEvent = null;
				num -= ReadEventMemory(ref bitReader, out aiEvent, out smallestRead);
				if (aiEvent != null)
				{
					AddEvent(aiEvent, silent: true);
				}
			}
		}
		else
		{
			m_CharacterSaveData.m_EventMemories = null;
		}
		byte[] eventsToReport = m_CharacterSaveData.m_EventsToReport;
		if (eventsToReport != null && eventsToReport.Length > 0 && instance != null)
		{
			if (bitReader != null)
			{
				bitReader.Reset(eventsToReport);
			}
			else
			{
				bitReader = new BitStreamReader(eventsToReport);
			}
			AICharacter_Guard component = GetComponent<AICharacter_Guard>();
			if (component != null)
			{
				int num2 = eventsToReport.Length * 8;
				uint smallestRead2 = 0u;
				while (num2 >= smallestRead2)
				{
					AIEvent aiEvent2 = null;
					num2 -= ReadEventMemory(ref bitReader, out aiEvent2, out smallestRead2);
					if (aiEvent2 != null)
					{
						component.GenerateReport(aiEvent2);
					}
				}
			}
		}
		else
		{
			m_CharacterSaveData.m_EventsToReport = null;
		}
		m_CharacterPersonality = (Personality.PersonalityType)m_CharacterSaveData.m_Personality;
		UpdateCombatBehaviour();
	}

	private void WriteEventMemory(ref BitStreamWriter writer, ref AIEvent aiEvent)
	{
		uint eventID = aiEvent.GetEventID();
		bool isNetID = AIEventManager.FirstBitSet(eventID);
		WriteEventID(ref writer, isNetID, eventID);
	}

	private int ReadEventMemory(ref BitStreamReader bitReader, out AIEvent aiEvent, out uint smallestRead)
	{
		aiEvent = null;
		int bitsRead = 0;
		uint fullID = ReadEventID(ref bitReader, out bitsRead, out smallestRead);
		if (AIEventManager.GetInstance() != null)
		{
			aiEvent = AIEventManager.GetInstance().GetAIEventFromID(fullID);
			if (aiEvent != null)
			{
			}
		}
		return bitsRead;
	}

	public static uint ReadEventID(ref BitStreamReader bitReader, out int bitsRead, out uint smallestRead)
	{
		int num = ((!bitReader.ReadBit()) ? 32 : 19);
		uint result = bitReader.ReadUInt32(num);
		bitsRead = 1 + num;
		smallestRead = 19u;
		return result;
	}

	public static void WriteEventID(ref BitStreamWriter writer, bool isNetID, uint eventID)
	{
		writer.Write(isNetID);
		int countOfBits = ((!isNetID) ? 32 : 19);
		writer.Write(eventID, countOfBits);
	}

	public static bool AIDebug(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bAIDebugOn = bPos;
		}
		return m_bAIDebugOn;
	}

	public static bool AIBehaviourDebug(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bAIBehaviourDebugOn = bPos;
		}
		return m_bAIBehaviourDebugOn;
	}

	public static bool AIHeatDebug(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			m_bAIHeatDebugOn = bPos;
		}
		return m_bAIHeatDebugOn;
	}

	public bool IsPendingMedicBedKeyCheck()
	{
		return m_bIsDueMedicBedMissingKeyCheck;
	}

	public virtual void Post_RealDeserialize(bool isFromSaveFile)
	{
	}

	private void OnDisable()
	{
		if (m_Character.m_IconHandler != null)
		{
			m_Character.m_IconHandler.HideCharacterIcon(hide: true);
		}
	}

	private void OnEnable()
	{
		if (m_Character.m_IconHandler != null)
		{
			m_Character.m_IconHandler.HideCharacterIcon(hide: false);
		}
	}
}
