using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomProcessingTool
{
	private enum SetupStage
	{
		Setup,
		WanderWaypoints,
		RoomDistanceGraph,
		RoomMeshes,
		ToiletData,
		Finished
	}

	private static AstarPath m_Astar = null;

	private static List<GameObject> m_DeactivatedFloors = new List<GameObject>();

	private static bool m_bForRunTime = false;

	private static string m_CurrentScenePath = null;

	private static SetupStage m_SetupStage = SetupStage.Setup;

	private static int m_iStartPathCounter = 0;

	private static float m_fDistance = 0f;

	private static RoomBlob m_startRoom = null;

	private static RoomBlob m_endRoom = null;

	public static void Init(bool bForRunTime, string strScenePath = null)
	{
		m_CurrentScenePath = null;
		m_CurrentScenePath = strScenePath;
		m_bForRunTime = bForRunTime;
		m_DeactivatedFloors.Clear();
		if (AstarPath.active != null)
		{
			m_Astar = AstarPath.active;
		}
		else
		{
			GameObject gameObject = GameObject.Find("A*");
			if (gameObject != null)
			{
				m_Astar = gameObject.GetComponent<AstarPath>();
			}
		}
		m_SetupStage = SetupStage.Setup;
	}

	public static bool StartProcessing()
	{
		switch (m_SetupStage)
		{
		case SetupStage.Setup:
			Setup();
			m_SetupStage = SetupStage.WanderWaypoints;
			return false;
		case SetupStage.WanderWaypoints:
			SetUpWanderWaypoints();
			m_SetupStage = SetupStage.RoomDistanceGraph;
			return false;
		case SetupStage.RoomDistanceGraph:
			SetUpRoomDistanceGraph();
			m_SetupStage = SetupStage.RoomMeshes;
			return false;
		case SetupStage.RoomMeshes:
			SetUpRoomMeshes(m_CurrentScenePath);
			m_SetupStage = SetupStage.ToiletData;
			return false;
		case SetupStage.ToiletData:
			if (m_bForRunTime)
			{
				RoomManager.GetInstance().FixAllToiletData();
			}
			m_SetupStage = SetupStage.Finished;
			return true;
		default:
			return true;
		}
	}

	public static void Finished()
	{
		for (int i = 0; i < m_DeactivatedFloors.Count; i++)
		{
			m_DeactivatedFloors[i].SetActive(value: false);
		}
		m_CurrentScenePath = null;
		m_DeactivatedFloors.Clear();
	}

	private static void Setup()
	{
		Transform transform = null;
		if (AstarPath.active != null)
		{
			m_Astar = AstarPath.active;
			transform = m_Astar.transform;
		}
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			if (rootGameObjects == null)
			{
				continue;
			}
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				if (transform == null)
				{
					transform = ((!rootGameObjects[j].name.Equals("A*")) ? rootGameObjects[j].transform.Find("A*") : rootGameObjects[j].transform);
				}
				string name = rootGameObjects[j].name;
				if (name.StartsWith("Floor") || name.StartsWith("Underground") || name.StartsWith("Vent") || name.StartsWith("Roof"))
				{
					FloorManager.Floor floor = FloorManager.GetInstance().FindFloorByName(name);
					if (floor != null)
					{
						floor.m_FloorRootObject = rootGameObjects[j].transform;
					}
					if (!rootGameObjects[j].activeInHierarchy)
					{
						rootGameObjects[j].SetActive(value: true);
						m_DeactivatedFloors.Add(rootGameObjects[j]);
					}
				}
				if (m_bForRunTime && name.Equals("LevelMaster") && !rootGameObjects[j].GetActive())
				{
					rootGameObjects[j].SetActive(value: true);
					m_DeactivatedFloors.Add(rootGameObjects[j]);
				}
			}
		}
		if (transform != null)
		{
			transform.gameObject.SetActive(value: true);
			if (m_Astar == null)
			{
				m_Astar = transform.GetComponent<AstarPath>();
			}
			m_Astar.Scan();
			while (m_Astar.isScanning)
			{
			}
		}
		RoomManager.GetInstance().LoadFloors(FloorManager.GetInstance().GetFloors());
	}

	private static void SetUpWanderWaypoints()
	{
		RoomUtility.GetInstance().m_InmateSafeSpaces.Clear();
		RoomUtility.GetInstance().m_GuardSafeSpaces.Clear();
		RoomUtility.GetInstance().m_SupportSafeSpaces.Clear();
		RoomManager.GetInstance().m_iInmateSafeSpaceStartIndex = new int[28];
		RoomManager.GetInstance().m_iGuardSafeSpaceStartIndex = new int[28];
		RoomManager.GetInstance().m_iSupportSafeSpaceStartIndex = new int[28];
		RoomManager.GetInstance().m_iInmateSafeSpaceEndIndex = new int[28];
		RoomManager.GetInstance().m_iGuardSafeSpaceEndIndex = new int[28];
		RoomManager.GetInstance().m_iSupportSafeSpaceEndIndex = new int[28];
		int inmateIndex = 0;
		int guardIndex = 0;
		int supportIndex = 0;
		for (int i = 0; i <= 27; i++)
		{
			RoomManager.GetInstance().m_iInmateSafeSpaceStartIndex[i] = inmateIndex;
			RoomManager.GetInstance().m_iGuardSafeSpaceStartIndex[i] = guardIndex;
			RoomManager.GetInstance().m_iSupportSafeSpaceStartIndex[i] = supportIndex;
			SetUpWanderPointsForLabel(ref inmateIndex, ref guardIndex, ref supportIndex, (RoomLabel)i);
			RoomManager.GetInstance().m_iInmateSafeSpaceEndIndex[i] = inmateIndex;
			RoomManager.GetInstance().m_iGuardSafeSpaceEndIndex[i] = guardIndex;
			RoomManager.GetInstance().m_iSupportSafeSpaceEndIndex[i] = supportIndex;
		}
	}

	private static void SetUpWanderPointsForLabel(ref int inmateIndex, ref int guardIndex, ref int supportIndex, RoomLabel label)
	{
		int count = RoomManager.GetInstance().m_Floors.Count;
		for (int i = 0; i < count; i++)
		{
			RoomFloor roomFloor = RoomManager.GetInstance().m_Floors[i];
			if (roomFloor == null)
			{
				continue;
			}
			roomFloor.PopulateRoomTempNodes();
			if (m_bForRunTime)
			{
				roomFloor.AutoPositionAllRooms();
			}
			NavGraph aStarGridForFloor = FloorManager.GetAStarGridForFloor(roomFloor.name, m_Astar);
			foreach (KeyValuePair<int, RoomBlob> room in roomFloor.m_Rooms)
			{
				RoomBlob value = room.Value;
				if (!(value == null) && value.m_RoomLabel == label)
				{
					value.m_iInmateSafeSpaceStartIndex = inmateIndex;
					value.m_iGuardSafeSpaceStartIndex = guardIndex;
					value.m_iSupportSafeSpaceStartIndex = supportIndex;
					if (value.m_InmateSafeSpace)
					{
						List<Vector3> list = value.EditorGenerateListOfWalkableNodes(aStarGridForFloor, CharacterRole.Inmate);
						RoomUtility.GetInstance().m_InmateSafeSpaces.AddRange(list);
						inmateIndex += list.Count;
					}
					if (value.m_GuardSafeSpace)
					{
						List<Vector3> list2 = value.EditorGenerateListOfWalkableNodes(aStarGridForFloor, CharacterRole.Guard);
						RoomUtility.GetInstance().m_GuardSafeSpaces.AddRange(list2);
						guardIndex += list2.Count;
					}
					if (value.m_SupportSafeSpace)
					{
						List<Vector3> list3 = value.EditorGenerateListOfWalkableNodes(aStarGridForFloor, CharacterRole.COUNT);
						RoomUtility.GetInstance().m_SupportSafeSpaces.AddRange(list3);
						supportIndex += list3.Count;
					}
					value.m_iInmateSafeSpaceEndIndex = inmateIndex;
					value.m_iGuardSafeSpaceEndIndex = guardIndex;
					value.m_iSupportSafeSpaceEndIndex = supportIndex;
					value.ValidateRoomObjectLists();
				}
			}
		}
	}

	private static void SetUpRoomDistanceGraph()
	{
		RoomManager.GetInstance().LoadFloors(FloorManager.GetInstance().GetFloors());
		if (RoomUtility.GetInstance().m_ConnectionData != null)
		{
			RoomUtility.GetInstance().m_ConnectionData.Clear();
		}
		else
		{
			RoomUtility.GetInstance().m_ConnectionData = new List<ConnectionData>();
		}
		int count = RoomManager.GetInstance().m_Floors.Count;
		for (int i = 0; i < count; i++)
		{
			RoomFloor roomFloor = RoomManager.GetInstance().m_Floors[i];
			if (!(roomFloor == null))
			{
				RoomBlob[] componentsInChildren = roomFloor.GetComponentsInChildren<RoomBlob>();
				foreach (RoomBlob roomBlob in componentsInChildren)
				{
					roomBlob.m_ARoomConnections.Clear();
					roomBlob.m_ACostSoFar = float.MaxValue;
				}
			}
		}
		for (int k = 0; k < count; k++)
		{
			RoomFloor roomFloor2 = RoomManager.GetInstance().m_Floors[k];
			if (roomFloor2 == null)
			{
				continue;
			}
			roomFloor2.PopulateRoomTempNodes();
			roomFloor2.GenerateRoomGraph();
			RoomBlob[] array = roomFloor2.m_Rooms.Values.ToArray();
			for (int l = 0; l < array.Length; l++)
			{
				RoomBlob room = array[l];
				if (!(room == null))
				{
					Vector3? start = SetRoomGraphPosition(ref room);
					List<RoomBlob> aRoomConnections = room.m_ARoomConnections;
					for (int m = 0; m < aRoomConnections.Count; m++)
					{
						RoomBlob room2 = aRoomConnections[m];
						Vector3? end = SetRoomGraphPosition(ref room2);
						float num = 0f;
						num = ((!(room.GetFloor() == room2.GetFloor())) ? 200f : GetDistance(start, end, room, room2));
						RoomUtility.GetInstance().SetConnection(ref room, ref room2, num);
					}
				}
			}
		}
		RoomUtility.GetInstance().SerialiseConnections();
	}

	private static Vector3? GetBestWalkableNode(RoomBlob room, NavGraph navGraph)
	{
		List<Vector3> allWalkableNodes = room.GetAllWalkableNodes(navGraph);
		if (allWalkableNodes == null || allWalkableNodes.Count == 0)
		{
			return null;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < allWalkableNodes.Count; i++)
		{
			zero += allWalkableNodes[i];
		}
		zero /= (float)allWalkableNodes.Count;
		Vector3 value = allWalkableNodes[0];
		float num = float.MaxValue;
		for (int j = 0; j < allWalkableNodes.Count; j++)
		{
			float num2 = Vector3.Distance(zero, allWalkableNodes[j]);
			if (num2 < num)
			{
				value = allWalkableNodes[j];
				num = num2;
			}
		}
		return value;
	}

	private static Vector3? SetRoomGraphPosition(ref RoomBlob room)
	{
		RoomFloor floor = room.GetFloor();
		if (floor.m_AssociatedNavGraph == null)
		{
			string floorName = floor.GetFloor().m_FloorName;
			floor.m_AssociatedNavGraph = FloorManager.GetAStarGridForFloor(floorName, m_Astar);
		}
		Vector3? bestWalkableNode = GetBestWalkableNode(room, floor.m_AssociatedNavGraph);
		if (!bestWalkableNode.HasValue)
		{
			room.position = room.GetCentroid();
		}
		else
		{
			room.position = bestWalkableNode.Value;
		}
		return bestWalkableNode;
	}

	private static float GetDistance(Vector3? start, Vector3? end, RoomBlob startRoom, RoomBlob endRoom)
	{
		m_iStartPathCounter = 1;
		m_fDistance = 500f;
		m_startRoom = startRoom;
		m_endRoom = endRoom;
		if (start.HasValue && end.HasValue)
		{
			Vector3 value = start.Value;
			Vector3 value2 = end.Value;
			ABPath p = ABPath.Construct(value, value2, OnPathComplete);
			AstarPath.StartPath(p);
			MethodInfo method = m_Astar.GetType().GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
			while (m_iStartPathCounter > 0)
			{
				method.Invoke(m_Astar, new object[0]);
			}
		}
		m_startRoom = null;
		m_endRoom = null;
		return m_fDistance;
	}

	private static void OnPathComplete(Path p)
	{
		m_iStartPathCounter--;
		if (!p.error)
		{
			m_fDistance = p.GetTotalLength();
		}
	}

	public static void SetUpRoomMeshes(string scenePath, bool forceRegenerate = false)
	{
		RoomManager.GetInstance().LoadFloors(FloorManager.GetInstance().GetFloors());
		int count = RoomManager.GetInstance().m_Floors.Count;
		for (int i = 0; i < count; i++)
		{
			RoomFloor roomFloor = RoomManager.GetInstance().m_Floors[i];
			if (roomFloor == null)
			{
				continue;
			}
			RoomBlob[] componentsInChildren = roomFloor.GetComponentsInChildren<RoomBlob>();
			foreach (RoomBlob roomBlob in componentsInChildren)
			{
				if (!RoomMeshGenerator.HasRoomGotOcclusionMesh(roomBlob) || roomBlob.m_RoomPositionsChanged || forceRegenerate)
				{
					roomBlob.m_RoomPositionsChanged = false;
					if (roomBlob.m_subLocation != RoomBlob.RoomSubIdentity_Location.Outdoors && (roomBlob.location != RoomBlob.eLocation.BuildingBoundary || RoomManager.GetInstance().m_AllowBuildingBoundariesToGenerateMeshes))
					{
						RoomOcclusionMesh roomOcclusionMesh = RoomMeshGenerator.GenerateRoomMesh(roomBlob, roomFloor);
					}
				}
			}
		}
	}
}
