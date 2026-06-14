using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Waypoint : ActionTask<AICharacter>
{
	public RoomBlob.eLocation m_TargetLocation;

	public bool m_RandomWaypoint = true;

	public float m_fMinRunDistance = 5f;

	public float m_fMaxRunDistance = 10f;

	private float m_fRunDistance = 5f;

	[BlackboardOnly]
	public BBParameter<RoomWaypoint> m_Waypoint;

	private RoomBlob m_TargetRoom;

	private bool m_bMovingToPosition;

	private Directionx4 m_FaceDirection;

	private bool m_bHaveWaypoint;

	private Vector3 m_vNextWaypointPosition = Vector3.zero;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	private DoorManager m_CachedDoorManager;

	private RoutineManager m_CachedRoutineManager;

	protected override string info => (!m_RandomWaypoint) ? string.Concat("Waypoint[", m_Waypoint, "]: ", m_TargetLocation) : ("Random Waypoint: " + m_TargetLocation);

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	public void OnTargetReached()
	{
		base.agent.m_Character.SetFaceDirection(m_FaceDirection);
		if (!m_RandomWaypoint)
		{
			EndAction(true);
		}
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	private void MoveToWaypoint()
	{
		if (m_RandomWaypoint)
		{
			m_bHaveWaypoint = m_TargetRoom.GetRandomWaypoint(base.agent.m_Character, out m_vNextWaypointPosition, out m_FaceDirection);
		}
		else
		{
			m_bHaveWaypoint = m_TargetRoom.GetWaypoint(base.agent.m_Character, m_Waypoint.value, out m_vNextWaypointPosition, out m_FaceDirection);
		}
		if (!m_bHaveWaypoint)
		{
			EndAction(false);
		}
		else
		{
			m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, m_vNextWaypointPosition);
		}
	}

	protected override void OnExecute()
	{
		m_bMovingToPosition = false;
		m_bHaveWaypoint = false;
		if (m_TargetRoom == null)
		{
			List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(m_TargetLocation);
			if (allRoomsByLocation == null || allRoomsByLocation.Count == 0)
			{
				EndAction(false);
				return;
			}
			m_TargetRoom = allRoomsByLocation[0];
		}
		m_fRunDistance = Random.Range(m_fMinRunDistance, m_fMaxRunDistance);
		m_fRunDistance *= m_fRunDistance;
		m_CachedDoorManager = DoorManager.GetInstance();
		m_CachedRoutineManager = RoutineManager.GetInstance();
	}

	protected override void OnUpdate()
	{
		if (base.agent.m_Character.GetIsImmobilised())
		{
			if (m_bHaveWaypoint)
			{
				m_TargetRoom.ReturnWaypoint(base.agent.m_Character);
				m_bHaveWaypoint = false;
			}
			m_bMovingToPosition = false;
		}
		else if (!m_bMovingToPosition)
		{
			if (!base.agent.m_Character.IsPendingPurpleLockProcess() && !m_CachedDoorManager.IsPendingPurpleLockProcess() && !m_CachedRoutineManager.IsDuePurpleLockChange())
			{
				MoveToWaypoint();
			}
		}
		else
		{
			Vector3 vector = Vector3.Scale(m_vNextWaypointPosition - base.agent.m_Transform.position, FloorManager.FLOOR_SCALE);
			float num = vector.x * vector.x + vector.y * vector.y;
			if (num > m_fRunDistance)
			{
				base.agent.SetRunning(running: true);
			}
			else
			{
				base.agent.SetRunning(running: false);
			}
		}
	}

	protected override void OnStop()
	{
		if (m_bHaveWaypoint && m_TargetRoom != null)
		{
			m_TargetRoom.ReturnWaypoint(base.agent.m_Character);
			m_bHaveWaypoint = false;
		}
		base.agent.SetRunning(running: false);
		m_CachedDoorManager = null;
		m_CachedRoutineManager = null;
	}
}
