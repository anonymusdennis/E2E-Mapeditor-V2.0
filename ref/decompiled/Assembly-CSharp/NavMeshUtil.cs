using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class NavMeshUtil : MonoBehaviour
{
	public enum TransitionDirection
	{
		Up,
		Down
	}

	private static bool m_bIsCustomPrison = false;

	private static GraphNode m_CustomPrisonRollcallNode;

	private static NNConstraint ms_default = NNConstraint.Default;

	private static GraphHitInfo m_NavHitInfo = default(GraphHitInfo);

	public static void Init()
	{
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null)
		{
			m_bIsCustomPrison = currentLevelInfo.m_PrisonEnum == LevelScript.PRISON_ENUM.CustomPrison;
		}
		else
		{
			m_bIsCustomPrison = false;
		}
		m_CustomPrisonRollcallNode = null;
		if (!m_bIsCustomPrison)
		{
			return;
		}
		RoomManager instance = RoomManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		RoomBlob firstRoomByLocation = instance.GetFirstRoomByLocation(RoomBlob.eLocation.RollCall);
		if (!(firstRoomByLocation != null))
		{
			return;
		}
		Vector3 position = Vector3.zero;
		if (firstRoomByLocation.GetRandomPositionInRoom(CharacterRole.Inmate, ref position))
		{
			m_CustomPrisonRollcallNode = GetNearestGraphNode(position);
			if (m_CustomPrisonRollcallNode != null)
			{
			}
		}
	}

	protected void OnDestroy()
	{
		m_CustomPrisonRollcallNode = null;
	}

	public static Vector3 GetNearestValidPosition(Vector3 position)
	{
		Vector3 nodePos = default(Vector3);
		if (GetPositionOnNavMesh(position, out nodePos))
		{
		}
		return nodePos;
	}

	public static Vector3 GetNearestValidPosition(Vector3 startingPosition, Vector3 vDir)
	{
		List<Vector3> exitPositions = new List<Vector3>();
		return GetNearestValidPosition(startingPosition, vDir, Direction.AllDirections, ref exitPositions);
	}

	public static Vector3 GetNearestValid4DirectionPosition(Vector3 startingPosition, Vector3 vDir)
	{
		List<Vector3> exitPositions = new List<Vector3>();
		return GetNearestValidPosition(startingPosition, vDir, Direction.FourDirections, ref exitPositions);
	}

	public static Vector3 GetNearestValidPosition(Vector3 startingPosition, Vector3 vDir, Directionx8[] permittedDirections, ref List<Vector3> exitPositions, bool includeNodesOnDoors = true)
	{
		if (exitPositions == null)
		{
			PopulateExitLocations(startingPosition, permittedDirections, ref exitPositions, includeNodesOnDoors);
		}
		if (exitPositions.Count == 0)
		{
			return startingPosition;
		}
		float num = float.MaxValue;
		Vector3 result = startingPosition;
		Vector3 vector = Vector3.ClampMagnitude(vDir, 1f);
		for (int i = 0; i < exitPositions.Count; i++)
		{
			float num2 = Vector3.Distance(startingPosition + vector, exitPositions[i]);
			if (num2 < num)
			{
				num = num2;
				result = exitPositions[i];
			}
		}
		return result;
	}

	public static List<Vector3> GetEightSurroundingLocations(Vector3 startingPosition)
	{
		List<Vector3> exitPositions = null;
		PopulateExitLocations(startingPosition, Direction.AllDirections, ref exitPositions);
		return exitPositions;
	}

	private static void PopulateExitLocations(Vector3 startingPosition, Directionx8[] permittedDirections, ref List<Vector3> exitPositions, bool includeNodesOnDoors = true)
	{
		exitPositions = new List<Vector3>();
		FloorManager instance = FloorManager.GetInstance();
		FloorManager.Floor floor = null;
		if (instance != null)
		{
			floor = instance.FindFloorAtZ(startingPosition.z);
		}
		Vector3 physicsOverlapSize = new Vector3(0.1f, 0.1f, 0.1f);
		Vector3 nodePos = default(Vector3);
		if (TestExitLocation(startingPosition, physicsOverlapSize, includeNodesOnDoors, ref floor, out nodePos))
		{
			exitPositions.Add(nodePos);
		}
		foreach (Directionx8 direction in permittedDirections)
		{
			Vector3 testPosition = startingPosition + Direction.DirectionToVector(direction) * 0.75f;
			nodePos = default(Vector3);
			if (TestExitLocation(testPosition, physicsOverlapSize, includeNodesOnDoors, ref floor, out nodePos))
			{
				exitPositions.Add(nodePos);
			}
		}
	}

	private static bool TestExitLocation(Vector3 testPosition, Vector3 physicsOverlapSize, bool includeNodesOnDoors, ref FloorManager.Floor floor, out Vector3 nodePos)
	{
		FloorManager instance = FloorManager.GetInstance();
		int row = 0;
		int column = 0;
		bool flag = false;
		bool flag2 = false;
		uint num = ((m_CustomPrisonRollcallNode != null) ? m_CustomPrisonRollcallNode.Area : 0u);
		if (floor != null && instance.GetTileGridPoint(floor, FloorManager.TileSystem_Type.TileSystem_Wall, testPosition, out row, out column) && FloorManager.GetInstance().GetTile(floor.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Wall, row, column) != null)
		{
			flag2 = true;
		}
		if (GetPositionOnNavMesh(testPosition, out nodePos, out var nearest))
		{
			flag = true;
			if (!flag2)
			{
				if (m_bIsCustomPrison && nearest.node.Area != num)
				{
					flag = false;
				}
				if (flag && !includeNodesOnDoors)
				{
					int num2 = EscapistsRaycast.OverlapBoxNonAlloc(nodePos, physicsOverlapSize, -1);
					for (int i = 0; i < num2; i++)
					{
						if (EscapistsRaycast.ColliderOverlapList[i].GetComponent<Door>() != null)
						{
							flag = false;
							break;
						}
					}
				}
			}
		}
		return flag && !flag2;
	}

	public static bool GetPositionOnNavMesh(Vector3 pos, out Vector3 nodePos)
	{
		NNInfo nearest;
		return GetPositionOnNavMesh(pos, out nodePos, out nearest);
	}

	public static bool GetPositionOnNavMesh(Vector3 pos, out Vector3 nodePos, out NNInfo nearest)
	{
		nearest = AstarPath.active.GetNearest(pos, ms_default);
		if (nearest.node == null)
		{
			nodePos = pos;
			return false;
		}
		nodePos = new Vector3(nearest.node.position.x, nearest.node.position.y, nearest.node.position.z);
		nodePos /= 1000f;
		if (Vector3.Distance(pos, nodePos) > 0.7f)
		{
			return false;
		}
		return true;
	}

	public static GraphNode GetNearestGraphNode(Vector3 pos, bool onMesh = true)
	{
		return AstarPath.active.GetNearest(pos, (!onMesh) ? AstarPath.m_NNnone : ms_default).node;
	}

	public static void SetNodeTag(GraphNode node, uint newTag)
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			node.Tag = newTag;
		});
	}

	public static void SetDamagableTile(GraphNode node, DamagableTile damagableTile, bool wallTile)
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			if (wallTile)
			{
				node.m_DamagableWallTile = damagableTile;
			}
			else
			{
				node.m_DamagableGroundTile = damagableTile;
			}
		});
	}

	public static void SetNodeWalkable(GraphNode node, bool walkable, int penalty)
	{
		Vector3 center = new Vector3(node.position.x, node.position.y, node.position.z) * 0.001f;
		Vector3 one = Vector3.one;
		Bounds b = new Bounds(center, one);
		GraphUpdateObject graphUpdateObject = new GraphUpdateObject(b);
		graphUpdateObject.setWalkability = walkable;
		graphUpdateObject.resetPenalty = true;
		graphUpdateObject.addPenalty = penalty;
		graphUpdateObject.modifyWalkability = true;
		graphUpdateObject.updatePhysics = false;
		AstarPath.active.UpdateGraphs(graphUpdateObject);
	}

	public static void CreateManualConnection(Vector3 a, Vector3 b, uint connectionCost, bool biDirectional = true)
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			GraphNode node = AstarPath.active.GetNearest(a).node;
			GraphNode node2 = AstarPath.active.GetNearest(b).node;
			if (node != null && node2 != null)
			{
				if (node.ContainsConnection(node2))
				{
					node.RemoveConnection(node2);
				}
				node.AddConnection(node2, connectionCost);
				if (biDirectional)
				{
					if (node2.ContainsConnection(node))
					{
						node2.RemoveConnection(node);
					}
					node2.AddConnection(node, connectionCost);
				}
			}
		});
	}

	public static void RemoveConnection(Vector3 a, Vector3 b, bool biDirectional = true)
	{
		AstarPath.RegisterSafeUpdate(delegate
		{
			GraphNode node = AstarPath.active.GetNearest(a).node;
			GraphNode node2 = AstarPath.active.GetNearest(b).node;
			if (node != null && node2 != null)
			{
				if (node.ContainsConnection(node2))
				{
					node.RemoveConnection(node2);
				}
				if (biDirectional && node2.ContainsConnection(node))
				{
					node2.RemoveConnection(node);
				}
			}
		});
	}

	public static void CreateTransition(TransitionDirection direction, FloorManager.Floor currentFloor, Vector3 start, Vector2 offset, int connectionCost)
	{
		Vector3 destination = Vector3.zero;
		if (GetTransitionDestination(direction, currentFloor, start, offset, out destination))
		{
			CreateManualConnection(start, destination, (uint)connectionCost);
			AstarPath.active.AddWorkItem(new AstarPath.AstarWorkItem(delegate
			{
				AstarPath.active.QueueWorkItemFloodFill();
				return true;
			}));
		}
	}

	public static void RemoveTransition(TransitionDirection direction, FloorManager.Floor currentFloor, Vector3 start, Vector2 offset)
	{
		Vector3 destination = Vector3.zero;
		if (GetTransitionDestination(direction, currentFloor, start, offset, out destination))
		{
			RemoveConnection(start, destination);
		}
	}

	private static bool GetTransitionDestination(TransitionDirection direction, FloorManager.Floor currentFloor, Vector3 start, Vector2 offset, out Vector3 destination)
	{
		destination = start;
		FloorManager.Floor floor = null;
		floor = ((direction != 0) ? FloorManager.GetInstance().DownAFloor(currentFloor) : FloorManager.GetInstance().UpAFloor(currentFloor));
		if (floor != null)
		{
			if (!FloorManager.GetInstance().GetTileGridPoint(floor, FloorManager.TileSystem_Type.TileSystem_Ground, start + (Vector3)offset, out var row, out var column))
			{
				return false;
			}
			if (!FloorManager.GetInstance().GetTileCentrePosition(floor, FloorManager.TileSystem_Type.TileSystem_Ground, row, column, out destination))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool SameFloorCheck(float fromZ, float toZ)
	{
		if (Mathf.Abs(fromZ - toZ) > 1.5f)
		{
			return false;
		}
		return true;
	}

	public static bool NavigationLineOfSight(int graphIndex, Vector3 from, Vector3 to, ref GraphHitInfo hit, float lineWidth)
	{
		if (lineWidth <= 0f)
		{
			return NavigationLineOfSight(graphIndex, from, to, ref hit);
		}
		Vector2 normalized = ((Vector2)from - (Vector2)to).normalized;
		float num = normalized.x * lineWidth;
		float num2 = normalized.y * lineWidth;
		Vector3 vector = new Vector3(0f - num2, num, 0f);
		Vector3 vector2 = new Vector3(num2, 0f - num, 0f);
		if (!NavigationLineOfSight(graphIndex, from + vector, to + vector, ref m_NavHitInfo))
		{
			return false;
		}
		return NavigationLineOfSight(graphIndex, from + vector2, to + vector2, ref m_NavHitInfo);
	}

	public static bool NavigationLineOfSight(int graphIndex, Vector3 from, Vector3 to, ref GraphHitInfo hit)
	{
		if (!SameFloorCheck(from.z, to.z))
		{
			return false;
		}
		if (AstarPath.active == null || AstarPath.active.graphs == null || AstarPath.active.graphs.Length <= graphIndex || graphIndex < 0)
		{
			return false;
		}
		GridGraph gridGraph = (GridGraph)AstarPath.active.graphs[graphIndex];
		bool flag = gridGraph.Linecast(from, to, null, out hit);
		return !flag;
	}

	public static Vector3 GetNearbyPosition(Vector3 targetPosition, int graphIndex)
	{
		Vector3 result = targetPosition;
		float num = 0f;
		int num2 = Random.Range(0, 8);
		for (int i = 0; i < 8; i++)
		{
			int num3 = num2 + i;
			if (num3 >= 8)
			{
				num3 -= 8;
			}
			Vector3 vector = Direction.DirectionToVector(Direction.AllDirections[num3]);
			Vector3 vector2 = targetPosition + vector;
			if (NavigationLineOfSight(graphIndex, targetPosition, vector2, ref m_NavHitInfo))
			{
				NNInfo nearest = AstarPath.active.GetNearest(vector2, AstarPath.m_NNnone);
				if (nearest.node != null && !nearest.node.m_bHasDamagableTile && nearest.node.Tag == 0 && nearest.node.Penalty == 0 && nearest.node.Walkable)
				{
					result = vector2;
					break;
				}
			}
			else
			{
				float sqrMagnitude = (m_NavHitInfo.point - m_NavHitInfo.origin).sqrMagnitude;
				if (sqrMagnitude > num)
				{
					num = sqrMagnitude;
					result = m_NavHitInfo.point;
				}
			}
		}
		return result;
	}

	public static void Cleanup()
	{
		m_NavHitInfo = default(GraphHitInfo);
	}
}
