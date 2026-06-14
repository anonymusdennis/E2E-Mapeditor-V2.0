using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class RoomBlob_JobRoom : RoomBlobData
{
	public static int c_JobRoomSubKey;

	public JobType m_JobType;

	public Door m_Door;

	public List<InteractiveObject> m_Dispensers = new List<InteractiveObject>();

	public List<InteractiveObject> m_Processors = new List<InteractiveObject>();

	public List<InteractiveObject> m_Collectors = new List<InteractiveObject>();

	public InteractiveObject m_CustomerWaitObject;

	public GameObject m_CustomerWaitPosition;

	public AICharacter_JobCustomer.PatronTypes m_CustomerPatronType;

	public List<CustomerServicePointLinker> m_CustomerServicePointLinks = new List<CustomerServicePointLinker>();

	public List<RoomBlob> m_CustomerIdleActiveRooms = new List<RoomBlob>();

	public GameObject m_CustomerNotActiveRoutinePosition;

	public List<GameObject> m_BespokeJobObjects = new List<GameObject>();

	public List<FakeCharacter> m_JobTaunters = new List<FakeCharacter>();

	public JobTutorialBoardInteraction m_TutorialBoard;

	public BehaviourTree m_JobBehaviour;

	protected virtual void OnDestroy()
	{
		if (m_TutorialBoard != null)
		{
			m_TutorialBoard = null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_JobBehaviour != null)
		{
			string assetName = ((Object)m_JobBehaviour).name;
			object assetFromBundle = AssetManager.instance.GetAssetFromBundle("aibehaviours", assetName);
			if (assetFromBundle != null && assetFromBundle is BehaviourTree)
			{
				m_JobBehaviour = assetFromBundle as BehaviourTree;
			}
		}
	}

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager levelManager = BaseLevelManager.GetInstance();
		if (levelManager != null)
		{
			BaseLevelManager.RoomObjectCollectionType<Door> door = new BaseLevelManager.RoomObjectCollectionType<Door>();
			BaseLevelManager.RoomObjectCollectionType<JobTutorialBoardInteraction> jobTutorialBoardInteraction = new BaseLevelManager.RoomObjectCollectionType<JobTutorialBoardInteraction>();
			BaseLevelManager.RoomObjectCollectionType<JobRoom_ContentsMarker> jobRoom_ContentsMarker = new BaseLevelManager.RoomObjectCollectionType<JobRoom_ContentsMarker>();
			levelManager.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, door, jobTutorialBoardInteraction, jobRoom_ContentsMarker);
			SetupFromData(ref levelManager, ref door, ref jobTutorialBoardInteraction, ref jobRoom_ContentsMarker);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager levelManager = BaseLevelManager.GetInstance();
		if (levelManager != null)
		{
			BaseLevelManager.RoomObjectCollectionType<Door> door = new BaseLevelManager.RoomObjectCollectionType<Door>();
			BaseLevelManager.RoomObjectCollectionType<JobTutorialBoardInteraction> jobTutorialBoardInteraction = new BaseLevelManager.RoomObjectCollectionType<JobTutorialBoardInteraction>();
			BaseLevelManager.RoomObjectCollectionType<JobRoom_ContentsMarker> jobRoom_ContentsMarker = new BaseLevelManager.RoomObjectCollectionType<JobRoom_ContentsMarker>();
			levelManager.GetObjectsInZone(ref zone, door, jobTutorialBoardInteraction, jobRoom_ContentsMarker);
			SetupFromData(ref levelManager, ref door, ref jobTutorialBoardInteraction, ref jobRoom_ContentsMarker);
		}
	}

	private void SetupFromData(ref BaseLevelManager levelManager, ref BaseLevelManager.RoomObjectCollectionType<Door> door, ref BaseLevelManager.RoomObjectCollectionType<JobTutorialBoardInteraction> jobTutorialBoardInteraction, ref BaseLevelManager.RoomObjectCollectionType<JobRoom_ContentsMarker> jobRoom_ContentsMarker)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (door.m_Contents.Count == 1)
		{
			m_Door = door.m_Contents[0];
			if (m_Door.m_DoorKeyColour == KeyFunctionality.KeyColour.Green && !levelManager.IsKeyInitialized(KeyFunctionality.KeyColour.Green))
			{
				c_JobRoomSubKey = 0;
			}
			m_Door.m_DoorKeySubCode = ++c_JobRoomSubKey;
		}
		if (jobTutorialBoardInteraction.m_Contents.Count == 1)
		{
			m_TutorialBoard = jobTutorialBoardInteraction.m_Contents[0];
		}
		int count = jobRoom_ContentsMarker.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			switch (jobRoom_ContentsMarker.m_Contents[i].m_ContentsType)
			{
			case JobRoom_ContentsMarker.ContentsType.Behaviour:
				if (jobRoom_ContentsMarker.m_Contents[i].m_Behaviour != null && m_JobBehaviour == null)
				{
					m_JobBehaviour = jobRoom_ContentsMarker.m_Contents[i].m_Behaviour;
				}
				break;
			case JobRoom_ContentsMarker.ContentsType.Collectors:
				if (jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject != null)
				{
					m_Collectors.Add(jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject);
				}
				break;
			case JobRoom_ContentsMarker.ContentsType.Processors:
				if (jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject != null)
				{
					m_Processors.Add(jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject);
				}
				break;
			case JobRoom_ContentsMarker.ContentsType.Dispencer:
				if (jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject != null)
				{
					m_Dispensers.Add(jobRoom_ContentsMarker.m_Contents[i].m_InteractiveObject);
				}
				break;
			case JobRoom_ContentsMarker.ContentsType.Objects:
				m_BespokeJobObjects.Add(jobRoom_ContentsMarker.m_Contents[i].gameObject);
				break;
			}
		}
		m_RoomSpecificObjects = new List<InteractiveObject>();
		m_RoomSpecificObjects.AddRange(m_Dispensers);
		m_RoomSpecificObjects.AddRange(m_Collectors);
		m_RoomSpecificObjects.AddRange(m_Processors);
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}
}
