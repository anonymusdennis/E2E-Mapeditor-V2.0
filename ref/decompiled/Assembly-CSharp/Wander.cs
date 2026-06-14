using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

[Category("★T17 Action")]
public class Wander : ActionTask<AICharacter>
{
	public int m_iMaxNodesPerRoom = 6;

	public float m_fMaxWaitTime = 10f;

	public float m_fSpinAroundTime = 2f;

	private float m_fSpinAroundTimer;

	public bool m_bWanderRoom;

	[BlackboardOnly]
	public BBParameter<RoomLabel> m_Label = RoomLabel.None;

	public BBParameter<bool> m_bShouldWanderExactly = false;

	public BBParameter<float> m_WanderingPrecision = 0.1f;

	private int m_iCurrentWaypoint;

	private List<Vector3> m_WaypointPath = new List<Vector3>();

	private bool m_bMovingToPosition;

	private float m_fWaitAtWaypointUntil;

	private float m_fOnErrorCooldownTime = 1f;

	private float m_fOnErrorCooldownTimer;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string info => "Wander";

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	private void GetNewPatrolPath()
	{
		m_iCurrentWaypoint = 0;
		m_WaypointPath.Clear();
		Vector3 pos = base.agent.m_Transform.position;
		if (!m_bWanderRoom)
		{
			if (!RoomManager.GetInstance().GetRandomPositionInWorld(base.agent.m_Character.m_CharacterRole, ref pos, m_Label.value))
			{
				EndAction(false);
				return;
			}
			m_WaypointPath.Add(pos);
		}
		RoomFloor floorFromZ = RoomManager.GetInstance().GetFloorFromZ(pos.z);
		Vector3 vector = RoomUtility.WorldToRoomGrid(pos, floorFromZ);
		RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom((int)vector.x, (int)vector.y, floorFromZ);
		if (roomBlob == null)
		{
			EndAction(false);
			return;
		}
		if (!roomBlob.HasPositionNodes(base.agent.m_Character.m_CharacterRole))
		{
			EndAction(false);
			return;
		}
		int num = Random.Range(0, m_iMaxNodesPerRoom);
		for (int i = 0; i < num; i++)
		{
			Vector3 position = base.agent.m_Transform.position;
			if (roomBlob.GetRandomPositionInRoom(base.agent.m_Character.m_CharacterRole, ref position))
			{
				m_WaypointPath.Add(position);
			}
		}
	}

	private void SetNextWaypoint()
	{
		m_iCurrentWaypoint++;
		if (m_iCurrentWaypoint >= m_WaypointPath.Count)
		{
			m_iCurrentWaypoint = 0;
			EndAction(true);
		}
	}

	public void OnTargetReached()
	{
		m_fWaitAtWaypointUntil = UpdateManager.time + Random.Range(2f, m_fMaxWaitTime);
		SetNextWaypoint();
		m_bMovingToPosition = false;
	}

	public void OnPathCancelled()
	{
		m_fOnErrorCooldownTimer = m_fOnErrorCooldownTime;
		SetNextWaypoint();
		m_bMovingToPosition = false;
	}

	private void MoveToWaypoint()
	{
		if (m_iCurrentWaypoint >= m_WaypointPath.Count)
		{
			EndAction(true);
			return;
		}
		Vector3 vector = m_WaypointPath[m_iCurrentWaypoint];
		if (!m_bShouldWanderExactly.value && Random.value > 0.1f)
		{
			GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(vector, onMesh: false);
			if (nearestGraphNode != null)
			{
				vector = NavMeshUtil.GetNearbyPosition(vector, (int)nearestGraphNode.GraphIndex);
			}
		}
		if (!m_bShouldWanderExactly.value)
		{
			vector.x += Random.value * 0.3f;
			vector.y += Random.value * 0.3f;
		}
		m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, vector, m_WanderingPrecision.value);
	}

	protected override void OnExecute()
	{
		GetNewPatrolPath();
	}

	protected override void OnStop()
	{
		m_fWaitAtWaypointUntil = 0f;
		m_bMovingToPosition = false;
	}

	protected override void OnUpdate()
	{
		if (m_fWaitAtWaypointUntil > UpdateManager.time)
		{
			if (m_fSpinAroundTimer < m_fSpinAroundTime)
			{
				m_fSpinAroundTimer += BehaviourTree.CurrentTimeSlicedDeltaTime;
				return;
			}
			m_fSpinAroundTimer = 0f;
			Directionx8 headAndBodyDirection = Direction.FourDirections[Random.Range(0, 4)];
			base.agent.m_Character.SetFaceDirection((Directionx4)headAndBodyDirection);
		}
		else if (!m_bMovingToPosition)
		{
			if (m_fOnErrorCooldownTimer > 0f)
			{
				m_fOnErrorCooldownTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
			}
			else
			{
				MoveToWaypoint();
			}
		}
	}
}
