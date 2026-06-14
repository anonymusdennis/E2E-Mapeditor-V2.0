using Pathfinding;
using UnityEngine;

public class AIMovement : MonoBehaviour, IControlledUpdate
{
	public delegate void TargetReachedCallback();

	public Transform m_Transform;

	public Seeker m_Seeker;

	public Character m_Character;

	public CharacterMovement m_CharacterMovement;

	public AICharacter m_AICharacter;

	public float m_fAtWaypointProximity = 0.1f;

	private float m_fCloseEnoughDistance;

	private T17_ABPath.PathCallback m_OnTargetReached;

	private T17_ABPath.PathCallback m_OnTargetCancelled;

	private T17_ABPath m_CurrentPath;

	private int m_iTargetWaypoint;

	private Vector3 m_vTargetWaypoint;

	private Vector3 m_vTargetFinalWaypoint;

	private int m_iGraphIndex = -1;

	private float m_fGraphIndexZPos;

	private GraphNode m_LastTeleportNode;

	private uint m_KeyAccess = 1u;

	private TargetReachedCallback m_OnEventTargetReached;

	private Vector3? m_ChasePosition;

	private float m_fEventTargetSightedTime = 0.5f;

	private float m_fEventTargetSightedTimer;

	private DamagableTile m_DamagedWallTile;

	private DamagableTile m_DamagedGroundTile;

	private GraphNode m_DamageTileGraphNode;

	private GraphNode m_PreviousNode;

	private OnPathDelegate m_OnPathCompleteDel;

	private uint m_iVentTag = 1u;

	private uint m_iUndergroundTag = 2u;

	private Vector2 m_vZero = Vector2.zero;

	private GraphHitInfo m_NavHitInfo = default(GraphHitInfo);

	private bool m_bRequiresControlledUpdate = true;

	private static NNConstraint ms_default = NNConstraint.Default;

	public void AddDoor(Door door)
	{
		m_KeyAccess |= (uint)GetTagForKey(door.m_DoorKeyColour);
	}

	public void AddGhostKeyAccess()
	{
		m_KeyAccess |= (uint)GetTagForKey(KeyFunctionality.KeyColour.Ghost);
	}

	public void ClearAllowedDoors()
	{
		m_KeyAccess = 1u;
	}

	public static int GetKeyTag(KeyFunctionality.KeyColour keyColor)
	{
		return keyColor switch
		{
			KeyFunctionality.KeyColour.Black => 3, 
			KeyFunctionality.KeyColour.Cyan => 4, 
			KeyFunctionality.KeyColour.Red => 5, 
			KeyFunctionality.KeyColour.Green => 6, 
			KeyFunctionality.KeyColour.Yellow => 7, 
			KeyFunctionality.KeyColour.Purple => 8, 
			KeyFunctionality.KeyColour.Silver => 9, 
			KeyFunctionality.KeyColour.Solitary => 10, 
			KeyFunctionality.KeyColour.Ghost => 11, 
			_ => 0, 
		};
	}

	private static int GetTagForKey(KeyFunctionality.KeyColour keyColor)
	{
		return 1 << GetKeyTag(keyColor);
	}

	private static int GetTagMaskForTagIndex(int tagIndex)
	{
		return 1 << tagIndex;
	}

	private void Awake()
	{
		m_OnPathCompleteDel = OnPathComplete;
	}

	protected virtual void OnDestroy()
	{
		m_OnPathCompleteDel = null;
		m_OnTargetReached = null;
		m_OnTargetCancelled = null;
		m_OnEventTargetReached = null;
		if (m_CurrentPath != null)
		{
			m_CurrentPath.Release(this);
			m_CurrentPath = null;
		}
		if (m_Seeker != null)
		{
			m_Seeker.ReleaseClaimedPath();
		}
		m_Seeker = null;
		m_Character = null;
		m_CharacterMovement = null;
		m_AICharacter = null;
	}

	private string DEBUG_LogMyInfo(string headerReason)
	{
		string text = base.transform.name;
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		text += "\n\n";
		text = ((!(m_Transform == null)) ? (text + "m_Transform is " + m_Transform.name + "\n") : (text + "m_Transform is null \n"));
		if (m_Character == null)
		{
			return text + "Character is null \n";
		}
		string text2 = text;
		return text2 + "Character is " + m_Character.transform.name + " (" + m_Character.ToString() + " " + m_Character.m_CharacterRole.ToString() + ")\n";
	}

	public bool TravelToPosition(T17_ABPath.PathCallback TargetReached, T17_ABPath.PathCallback TargetCancelled, Vector3 targetPosition, float closeEnoughDistance = 0.1f, bool throttled = false, bool allowTeleport = false, bool skipLastNode = false)
	{
		if (throttled && !m_Seeker.IsDone())
		{
			return false;
		}
		uint num = 1u;
		Vector3 cachedCurrentPosition = m_Character.m_CachedCurrentPosition;
		NNInfo nearest = AstarPath.active.GetNearest(targetPosition);
		NNInfo nearest2 = AstarPath.active.GetNearest(cachedCurrentPosition);
		if ((nearest.node.Tag & m_iVentTag) != 0 || (nearest2.node.Tag & m_iVentTag) != 0)
		{
			num |= (uint)GetTagMaskForTagIndex((int)m_iVentTag);
		}
		if ((nearest.node.Tag & m_iUndergroundTag) != 0 || (nearest2.node.Tag & m_iUndergroundTag) != 0)
		{
			num |= (uint)GetTagMaskForTagIndex((int)m_iUndergroundTag);
		}
		num |= m_KeyAccess;
		m_Seeker.traversableTags = (int)num;
		T17_ABPath t17_ABPath = T17_ABPath.Construct(cachedCurrentPosition, targetPosition);
		t17_ABPath.path_OnTargetReached = TargetReached;
		t17_ABPath.path_OnPathCancelled = TargetCancelled;
		t17_ABPath.path_fCloseEnoughDistance = closeEnoughDistance;
		t17_ABPath.path_bAllowTeleport = allowTeleport;
		t17_ABPath.path_RealTargetPos = targetPosition;
		t17_ABPath.path_bSkipLastNode = skipLastNode;
		t17_ABPath.calculatePartial = true;
		m_Seeker.StartPath(t17_ABPath, m_OnPathCompleteDel);
		return true;
	}

	public void OnPathComplete(Path p)
	{
		T17_ABPath t17_ABPath = (T17_ABPath)p;
		if (p.error)
		{
			if (t17_ABPath != null && t17_ABPath.path_bAllowTeleport && TeleportCheck(t17_ABPath.path_RealTargetPos))
			{
				CancelCurrentPath();
			}
			t17_ABPath.path_OnPathCancelled();
			Debug.DrawLine(m_Character.m_CachedCurrentPosition, t17_ABPath.originalEndPoint, Color.red, 4f, depthTest: false);
			return;
		}
		p.Claim(this);
		bool flag = InitialisePath(t17_ABPath);
		if (t17_ABPath.path_OnTargetReached != m_OnTargetReached)
		{
			m_OnTargetReached = t17_ABPath.path_OnTargetReached;
		}
		if (t17_ABPath.path_OnPathCancelled != m_OnTargetCancelled)
		{
			if (m_OnTargetCancelled != null)
			{
				m_OnTargetCancelled();
			}
			m_OnTargetCancelled = t17_ABPath.path_OnPathCancelled;
		}
		if (!flag)
		{
			EndOfPath(success: false);
		}
	}

	private bool InitialisePath(T17_ABPath p)
	{
		m_CurrentPath = p;
		m_iTargetWaypoint = 0;
		Vector3 vector = m_CurrentPath.vectorPath[0];
		vector.x = m_Character.m_CachedCurrentPosition.x;
		vector.y = m_Character.m_CachedCurrentPosition.y;
		m_CurrentPath.vectorPath[0] = vector;
		bool pathingFailure = false;
		SetTargetWaypoint(m_iTargetWaypoint, out pathingFailure);
		if (pathingFailure)
		{
			return false;
		}
		int count = m_CurrentPath.vectorPath.Count;
		int num = Mathf.Min(6, count - 2);
		for (int i = 0; i < num; i++)
		{
			Vector3 vector2 = m_CurrentPath.vectorPath[i];
			if (NavMeshUtil.NavigationLineOfSight(m_iGraphIndex, vector, vector2, ref m_NavHitInfo, 0.25f))
			{
				vector2.x = m_Character.m_CachedCurrentPosition.x;
				vector2.y = m_Character.m_CachedCurrentPosition.y;
				m_CurrentPath.vectorPath[i] = vector2;
				continue;
			}
			break;
		}
		int count2 = m_CurrentPath.vectorPath.Count;
		m_vTargetFinalWaypoint = m_CurrentPath.vectorPath[count2 - 1];
		m_fCloseEnoughDistance = p.path_fCloseEnoughDistance;
		return true;
	}

	private bool SetTargetWaypoint(int waypoint, out bool pathingFailure)
	{
		m_iTargetWaypoint = waypoint;
		pathingFailure = false;
		Vector3 position = m_CurrentPath.vectorPath[m_iTargetWaypoint];
		Vector3 position2 = position;
		if (m_iTargetWaypoint > 0)
		{
			position2 = m_CurrentPath.vectorPath[m_iTargetWaypoint - 1];
		}
		m_vTargetWaypoint = position;
		GraphNode graphNode = GetGraphNode(ref position);
		GraphNode graphNode2 = GetGraphNode(ref position2);
		if (m_AICharacter.TryingToUseItem())
		{
			return false;
		}
		if ((m_DamagedGroundTile != null || m_DamagedWallTile != null) && m_DamageTileGraphNode == m_PreviousNode && graphNode2 != m_PreviousNode && graphNode != m_PreviousNode && !m_AICharacter.MagicItemInUse())
		{
			if (m_DamagedGroundTile != null)
			{
				m_AICharacter.RepairTile(m_DamagedGroundTile.transform.position, AIEventManager.EventHeight.Ground);
				m_DamagedGroundTile = null;
			}
			if (m_DamagedWallTile != null)
			{
				m_AICharacter.RepairTile(m_DamagedWallTile.transform.position, AIEventManager.EventHeight.Wall);
				m_DamagedWallTile = null;
			}
			return false;
		}
		m_AICharacter.QueryCurrentNode(graphNode2);
		if (graphNode == null)
		{
			CancelCurrentPath();
			return false;
		}
		int graphIndex = (int)graphNode.GraphIndex;
		bool flag = graphIndex != m_iGraphIndex;
		if (graphNode.m_bHasDamagableTile || (graphNode2 != null && graphNode2.m_bHasDamagableTile))
		{
			if (!flag)
			{
				if (graphNode.m_DamagableWallTile != null && !graphNode.m_DamagableWallTile.IsFullyDamaged())
				{
					if (m_Character.m_CharacterRole == CharacterRole.Inmate)
					{
						pathingFailure = true;
						return false;
					}
					if (!m_AICharacter.MagicItemInUse())
					{
						m_DamagedWallTile = graphNode.m_DamagableWallTile;
						m_AICharacter.DestroyTile(m_DamagedWallTile.transform.position, AIEventManager.EventHeight.Wall);
						m_DamageTileGraphNode = graphNode;
					}
					return false;
				}
				if (graphNode.m_DamagableWallTile != null && graphNode.m_DamagableWallTile.IsHoldingItem())
				{
					if (m_Character.m_CharacterRole == CharacterRole.Inmate)
					{
						pathingFailure = true;
						return false;
					}
					m_Character.PauseMovement(1f);
					graphNode.m_DamagableWallTile.DestroyItem();
					return false;
				}
			}
			else
			{
				bool flag2 = false;
				if (graphNode2 != null)
				{
					flag2 = GetZPosFromGraphIndex((int)graphNode2.GraphIndex) < GetZPosFromGraphIndex((int)graphNode.GraphIndex);
				}
				GraphNode graphNode3 = ((!flag2) ? graphNode : graphNode2);
				if (graphNode3.m_DamagableGroundTile != null && graphNode3.m_DamagableGroundTile.IsHoldingItem())
				{
					if (m_Character.m_CharacterRole == CharacterRole.Inmate)
					{
						pathingFailure = true;
						return false;
					}
					graphNode3.m_DamagableGroundTile.DestroyItem();
				}
				if (flag2)
				{
					if (graphNode2.m_DamagableGroundTile != null && !graphNode2.m_DamagableGroundTile.IsFullyDamaged())
					{
						if (m_Character.m_CharacterRole == CharacterRole.Inmate)
						{
							pathingFailure = true;
							return false;
						}
						if (!m_AICharacter.MagicItemInUse())
						{
							m_DamagedGroundTile = graphNode2.m_DamagableGroundTile;
							m_AICharacter.DestroyTile(m_DamagedGroundTile.transform.position, AIEventManager.EventHeight.Ground);
							m_DamageTileGraphNode = graphNode2;
						}
						return false;
					}
				}
				else
				{
					if ((graphNode.m_DamagableWallTile == null || graphNode.m_DamagableWallTile.IsFullyDamaged()) && graphNode.m_DamagableGroundTile != null && !graphNode.m_DamagableGroundTile.IsFullyDamaged())
					{
						if (m_Character.m_CharacterRole == CharacterRole.Inmate)
						{
							pathingFailure = true;
							return false;
						}
						if (!m_AICharacter.MagicItemInUse())
						{
							m_DamagedGroundTile = graphNode.m_DamagableGroundTile;
							m_AICharacter.DestroyTile(m_DamagedGroundTile.transform.position, AIEventManager.EventHeight.Ground);
							m_DamageTileGraphNode = graphNode;
						}
						return false;
					}
					if (graphNode.m_DamagableWallTile != null && !graphNode.m_DamagableWallTile.IsFullyDamaged() && (graphNode.m_DamagableGroundTile == null || graphNode.m_DamagableGroundTile.IsFullyDamaged()))
					{
						if (m_Character.m_CharacterRole == CharacterRole.Inmate)
						{
							pathingFailure = true;
							return false;
						}
						if (!m_AICharacter.MagicItemInUse())
						{
							m_DamagedGroundTile = graphNode.m_DamagableGroundTile;
							m_AICharacter.DestroyTile(m_DamagedGroundTile.transform.position, AIEventManager.EventHeight.Ground);
							m_DamageTileGraphNode = graphNode;
						}
						return false;
					}
					if (graphNode.m_DamagableWallTile != null && !graphNode.m_DamagableWallTile.IsFullyDamaged() && graphNode.m_DamagableGroundTile != null && !graphNode.m_DamagableGroundTile.IsFullyDamaged())
					{
						if (m_Character.m_CharacterRole == CharacterRole.Inmate)
						{
							pathingFailure = true;
							return false;
						}
						if (!m_AICharacter.MagicItemInUse())
						{
							m_DamagedGroundTile = graphNode.m_DamagableGroundTile;
							m_DamagedWallTile = graphNode.m_DamagableWallTile;
							m_AICharacter.DestroyTile(m_DamagedGroundTile.transform.position, AIEventManager.EventHeight.Ground);
							m_AICharacter.DestroyTile(m_DamagedWallTile.transform.position, AIEventManager.EventHeight.Wall);
							m_DamageTileGraphNode = graphNode;
						}
						return false;
					}
				}
			}
		}
		if (flag)
		{
			m_iGraphIndex = graphIndex;
			m_fGraphIndexZPos = GetZPosFromGraphIndex(graphIndex);
			Vector3 newPosition = new Vector3(position.x, position.y, m_fGraphIndexZPos);
			m_Character.Teleport(newPosition);
		}
		m_PreviousNode = graphNode2;
		return true;
	}

	private GraphNode GetGraphNode(int index)
	{
		if (m_CurrentPath == null)
		{
			return null;
		}
		if (index < 0 || index >= m_CurrentPath.path.Count)
		{
			return null;
		}
		return m_CurrentPath.path[index];
	}

	private GraphNode GetGraphNode(ref Vector3 position)
	{
		int index = Mathf.FloorToInt(position.z);
		return m_CurrentPath.path[index];
	}

	private float GetZPosFromGraphIndex(int GraphIndex)
	{
		GridGraph gridGraph = (GridGraph)AstarPath.active.graphs[GraphIndex];
		return gridGraph.center.z - 1f;
	}

	public int GetCurrentGraphIndex()
	{
		return m_iGraphIndex;
	}

	public void ControlledUpdate()
	{
	}

	public void ControlledFixedUpdate()
	{
		if ((m_Character.GetIsImmobilised() || m_AICharacter.ImmobilisingMagicItemInUse()) && m_CurrentPath != null)
		{
			m_Character.Walk(m_vZero, CharacterSpeed.Stand);
			return;
		}
		if ((m_Character.m_bIsKnockedOut || m_Character.m_bIsBound) && m_CurrentPath != null)
		{
			EndOfPath(success: false);
		}
		if (m_Character.m_bIsDashing)
		{
			m_Character.Walk(m_vZero, CharacterSpeed.Run);
		}
		Vector3? chasePosition = m_ChasePosition;
		if (chasePosition.HasValue && m_Transform != null)
		{
			Vector3? chasePosition2 = m_ChasePosition;
			Vector3 value = chasePosition2.Value;
			float num = Vector2.Distance(m_Character.m_CachedCurrentPosition, value);
			if (NavMeshUtil.SameFloorCheck(m_Character.m_CachedCurrentPosition.z, value.z) && num <= m_fCloseEnoughDistance)
			{
				if (m_OnEventTargetReached != null)
				{
					m_OnEventTargetReached();
					m_OnEventTargetReached = null;
				}
				m_Character.Walk(m_vZero, CharacterSpeed.Stand);
				return;
			}
			if (NavMeshUtil.NavigationLineOfSight(m_iGraphIndex, m_Character.m_CachedCurrentPosition, value, ref m_NavHitInfo, 0.25f))
			{
				Vector2 vector = value - m_Character.m_CachedCurrentPosition;
				m_Character.CalcFaceDirection(vector);
				if (!(m_fEventTargetSightedTimer < m_fEventTargetSightedTime))
				{
					CancelCurrentPath();
					m_Character.Walk(vector, CharacterSpeed.Run);
					return;
				}
				m_fEventTargetSightedTimer += UpdateManager.fixedDeltaTime;
			}
			else
			{
				m_fEventTargetSightedTimer = 0f;
				if (m_CurrentPath == null && m_NavHitInfo.node != null)
				{
					Vector2 vector2 = new Vector2((float)m_NavHitInfo.node.position.x * 0.001f, (float)m_NavHitInfo.node.position.y * 0.001f);
					Vector2 vector3 = vector2 - (Vector2)m_Character.m_CachedCurrentPosition;
					if (Vector2.Dot(vector3, value) > 0f)
					{
						m_Character.Walk(vector3, CharacterSpeed.Run);
						return;
					}
				}
			}
		}
		if (m_CurrentPath == null)
		{
			m_Character.Walk(m_vZero, CharacterSpeed.Stand);
			return;
		}
		float num2 = Vector3.Distance(m_Character.m_CachedCurrentPosition, m_vTargetFinalWaypoint);
		if (num2 <= m_fCloseEnoughDistance)
		{
			m_Character.Walk(m_vZero, CharacterSpeed.Stand);
			EndOfPath();
			return;
		}
		if (CloseEnoughToTargetWaypoint() && !AdvanceWaypoint())
		{
			m_Character.Walk(m_vZero, CharacterSpeed.Stand);
			return;
		}
		Vector2 vector4 = m_Character.m_CachedCurrentPosition;
		Vector2 vector5 = m_vTargetWaypoint;
		CharacterSpeed characterSpeed = ((!m_AICharacter.IsRunning()) ? CharacterSpeed.Walk : CharacterSpeed.Run);
		float num3 = m_CharacterMovement.GetTravelDistance(characterSpeed) * UpdateManager.fixedDeltaTime;
		Vector2 vector6 = vector5 - vector4;
		float sqrMagnitude = vector6.sqrMagnitude;
		float num4 = num3 * num3;
		Vector2 vector7 = vector5;
		if (num4 >= sqrMagnitude)
		{
			while (num4 >= sqrMagnitude)
			{
				vector7 = vector5;
				if (!AdvanceWaypoint())
				{
					m_Character.Walk(m_vZero, CharacterSpeed.Stand);
					return;
				}
				vector5 = m_vTargetWaypoint;
				num4 -= sqrMagnitude;
				vector6 = vector5 - vector7;
				sqrMagnitude = vector6.sqrMagnitude;
			}
			if (num4 > 0f)
			{
				vector7 += vector6 / Mathf.Sqrt(sqrMagnitude) * Mathf.Sqrt(num4);
			}
		}
		else
		{
			vector7 = vector4 + num3 * (vector6 / Mathf.Sqrt(sqrMagnitude));
		}
		Vector2 desiredVelocity = vector7 - vector4;
		m_Character.Walk(desiredVelocity, characterSpeed);
	}

	private bool CloseEnoughToTargetWaypoint()
	{
		GraphNode graphNode = GetGraphNode(ref m_vTargetWaypoint);
		if (graphNode == null)
		{
			return false;
		}
		if (m_iGraphIndex != (int)graphNode.GraphIndex)
		{
			return false;
		}
		return Vector2.Distance(m_Character.m_CachedCurrentPosition, m_vTargetWaypoint) < m_fAtWaypointProximity;
	}

	private bool AdvanceWaypoint()
	{
		m_iTargetWaypoint++;
		if (m_iTargetWaypoint >= m_CurrentPath.vectorPath.Count)
		{
			EndOfPath();
			return false;
		}
		bool pathingFailure = false;
		bool flag = SetTargetWaypoint(m_iTargetWaypoint, out pathingFailure);
		if (pathingFailure)
		{
			EndOfPath(success: false);
			return false;
		}
		if (!flag)
		{
			m_iTargetWaypoint--;
			m_Character.PauseMovement(2f);
		}
		return flag;
	}

	public Vector3 CalculatePositionInFuture(int future, out int graphIndex)
	{
		if (m_CurrentPath == null || m_CurrentPath.vectorPath == null || m_CurrentPath.vectorPath.Count == 0)
		{
			graphIndex = -1;
			return m_Character.m_CachedCurrentPosition;
		}
		int index = Mathf.Min(m_iTargetWaypoint + future, m_CurrentPath.vectorPath.Count - 1);
		Vector3 position = m_CurrentPath.vectorPath[index];
		int graphIndex2 = (int)GetGraphNode(ref position).GraphIndex;
		position.z = GetZPosFromGraphIndex(graphIndex2);
		graphIndex = graphIndex2;
		return position;
	}

	private Vector2 WaypointFuture(int future)
	{
		if (m_CurrentPath == null || m_CurrentPath.vectorPath == null || m_CurrentPath.vectorPath.Count == 0)
		{
			return m_Character.m_CachedCurrentPosition;
		}
		int index = Mathf.Min(m_iTargetWaypoint + future, m_CurrentPath.vectorPath.Count - 1);
		return m_CurrentPath.vectorPath[index];
	}

	public int GetCurrentWaypointPos()
	{
		return m_iTargetWaypoint;
	}

	public void CancelCurrentPath()
	{
		EndOfPath(success: false);
	}

	private void EndOfPath(bool success = true)
	{
		if (m_CurrentPath != null)
		{
			if (success && m_CurrentPath.path_bAllowTeleport && TeleportCheck(m_CurrentPath.path_RealTargetPos))
			{
				success = false;
			}
			m_CurrentPath.Release(this);
			m_CurrentPath = null;
		}
		if (success && m_OnTargetReached != null)
		{
			m_OnTargetReached();
		}
		m_OnTargetReached = null;
		if (!success && m_OnTargetCancelled != null)
		{
			m_OnTargetCancelled();
		}
		m_OnTargetCancelled = null;
		m_iTargetWaypoint = -1;
	}

	public bool GetDistanceToTarget(GameObject target, T17_ABPath.PathDistanceCallback distCallback)
	{
		if (!m_Seeker.IsDone())
		{
			return false;
		}
		T17_ABPath t17_ABPath = T17_ABPath.Construct(m_Character.m_CachedCurrentPosition, target.transform.position);
		t17_ABPath.path_Distance = distCallback;
		t17_ABPath.path_TargetGO = target;
		m_Seeker.StartPath(t17_ABPath, OnPathDistComplete);
		return true;
	}

	public void OnPathDistComplete(Path p)
	{
		T17_ABPath t17_ABPath = (T17_ABPath)p;
		int distance = int.MaxValue;
		if (p.error)
		{
			t17_ABPath.path_Distance(distance, t17_ABPath.path_TargetGO, m_AICharacter);
			return;
		}
		p.Claim(this);
		distance = t17_ABPath.vectorPath.Count;
		t17_ABPath.path_Distance(distance, t17_ABPath.path_TargetGO, m_AICharacter);
		p.Release(this);
	}

	public void SetChaseTarget(Vector3? chasePosition, TargetReachedCallback OnTargetReached = null)
	{
		m_OnEventTargetReached = OnTargetReached;
		m_ChasePosition = chasePosition;
	}

	private bool TeleportCheck(Vector3 target)
	{
		NNInfo nearest = AstarPath.active.GetNearest(target, ms_default);
		NNInfo nearest2 = AstarPath.active.GetNearest(m_Character.m_CachedCurrentPosition);
		if (nearest2.node == null || nearest.node == null || nearest2.node.Area != nearest.node.Area)
		{
			if (nearest.node != null)
			{
				m_iGraphIndex = (int)nearest.node.GraphIndex;
			}
			Vector3 newPosition = target;
			if (m_LastTeleportNode != null && nearest.node != null && m_LastTeleportNode.Area == nearest.node.Area)
			{
				Int3 position = m_LastTeleportNode.position;
				newPosition = new Vector3(position.x / 1000, position.y / 1000, position.z / 1000);
			}
			m_Character.Teleport(newPosition);
			m_LastTeleportNode = nearest2.node;
			return true;
		}
		return false;
	}

	public void SetRequiresControlledUpdate(bool value)
	{
		m_bRequiresControlledUpdate = value;
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
		return false;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return m_bRequiresControlledUpdate;
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
}
