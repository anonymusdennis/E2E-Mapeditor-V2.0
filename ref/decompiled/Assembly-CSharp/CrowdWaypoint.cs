using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class CrowdWaypoint : ActionTask<AICharacter_CrowdNPC>
{
	public float m_fMinRunDistance = 5f;

	public float m_fMaxRunDistance = 10f;

	private float m_fRunDistance = 5f;

	public AnimState m_EnterAnimation = AnimState.SitEnter;

	public AnimState m_LoopAnimation = AnimState.IdleSit;

	private RoomWaypoint m_Waypoint;

	private bool m_bMovingToPosition;

	private Directionx4 m_FaceDirection;

	private bool m_bTargetReached;

	private Vector3 m_vNextWaypointPosition = Vector3.zero;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	[SerializeField]
	private float m_fLerpSpeed = 5f;

	private float m_fLerpTimer;

	private bool m_bLerpToPosition;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	public void OnTargetReached()
	{
		base.agent.m_Character.SetFaceDirection(m_FaceDirection);
		base.agent.m_Character.m_CharacterAnimator.StartAnimation(m_EnterAnimation);
		m_bTargetReached = true;
		m_bLerpToPosition = true;
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	protected override void OnExecute()
	{
		m_bMovingToPosition = false;
		m_bTargetReached = false;
		m_fRunDistance = Random.Range(m_fMinRunDistance, m_fMaxRunDistance);
		m_fRunDistance *= m_fRunDistance;
	}

	protected override void OnUpdate()
	{
		if (m_bTargetReached)
		{
			if (m_bLerpToPosition)
			{
				if (m_fLerpTimer >= 1f)
				{
					m_bLerpToPosition = false;
					base.agent.SetIsSeated(seated: true);
					m_fLerpTimer = 0f;
					base.agent.m_Character.m_CharacterAnimator.StartAnimation(m_LoopAnimation);
				}
				else
				{
					m_fLerpTimer += UpdateManager.deltaTime * m_fLerpSpeed;
					Vector3 pos = Vector3.Lerp(m_vNextWaypointPosition, m_Waypoint.GetPosition(), m_fLerpTimer);
					base.agent.m_Character.Teleport(pos, base.agent.m_Character.CurrentFloor, instantUpdate: false);
				}
			}
		}
		else if (!m_bMovingToPosition)
		{
			MoveToWaypoint();
		}
	}

	private void MoveToWaypoint()
	{
		m_Waypoint = base.agent.GetCrowdWaypoint();
		if (m_Waypoint == null)
		{
			EndAction(false);
			return;
		}
		m_vNextWaypointPosition = m_Waypoint.GetPosition();
		m_vNextWaypointPosition += 0.5f * Direction.DirectionToVector(m_Waypoint.m_WaypointSide);
		m_FaceDirection = m_Waypoint.m_FacingDirection;
		m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, m_vNextWaypointPosition);
	}

	protected override void OnStop()
	{
		base.agent.m_Character.Teleport(m_vNextWaypointPosition);
		base.agent.m_Character.m_CharacterAnimator.StopAnimation(m_LoopAnimation);
		base.agent.m_Character.m_CharacterAnimator.StopAnimation(m_EnterAnimation);
		base.agent.SetIsSeated(seated: false);
		m_bLerpToPosition = false;
		m_fLerpTimer = 0f;
	}
}
