using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

[Category("★T17 Action")]
public class Leash : ActionTask<AICharacter>
{
	public BBParameter<AICharacter> m_FollowTarget;

	public float m_FollowDistance;

	public float m_fUpdateTime = 0.5f;

	public int m_iFutureWaypoint = 20;

	public float m_fMaxRunSqrDistance = 20f;

	public float m_fStopRunSqrDistance = 1f;

	private bool m_bMovingToPosition;

	private Vector3 m_TargetWaypoint;

	private GraphNode previousGraphNode;

	private bool m_bToggleRun;

	private float m_fUpdateTimer;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		if (m_FollowTarget.value == null)
		{
			EndAction(false);
			return;
		}
		m_bMovingToPosition = false;
		m_TargetWaypoint = base.agent.m_Transform.position;
	}

	protected override void OnUpdate()
	{
		m_fUpdateTimer += BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (!m_bMovingToPosition && m_fUpdateTimer > m_fUpdateTime)
		{
			m_fUpdateTimer = 0f;
			MoveCloser();
		}
		TryRuninng();
	}

	private void TryRuninng()
	{
		int currentGraphIndex = m_FollowTarget.value.m_AIMovement.GetCurrentGraphIndex();
		int currentGraphIndex2 = base.agent.m_AIMovement.GetCurrentGraphIndex();
		if (currentGraphIndex != currentGraphIndex2)
		{
			base.agent.SetRunning(running: true);
			return;
		}
		Vector3 vector = m_TargetWaypoint - base.agent.m_Transform.position;
		float num = Vector2.SqrMagnitude(vector);
		if (num >= m_fMaxRunSqrDistance)
		{
			m_bToggleRun = true;
		}
		else if (num <= m_fStopRunSqrDistance)
		{
			m_bToggleRun = false;
		}
		if (m_bToggleRun || (m_FollowTarget.value != null && m_FollowTarget.value.IsRunning()))
		{
			base.agent.SetRunning(running: true);
		}
		else
		{
			base.agent.SetRunning(running: false);
		}
	}

	private void MoveCloser()
	{
		if (m_FollowTarget.value == null || m_FollowTarget.value.m_Character.m_bIsKnockedOut)
		{
			EndAction(false);
			return;
		}
		int graphIndex = -1;
		Vector3 targetPosition = m_FollowTarget.value.m_AIMovement.CalculatePositionInFuture(m_iFutureWaypoint, out graphIndex);
		if (graphIndex != -1)
		{
			m_TargetWaypoint = NavMeshUtil.GetNearbyPosition(targetPosition, graphIndex);
		}
		else
		{
			GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(m_FollowTarget.value.m_Transform.position);
			if (nearestGraphNode != null && nearestGraphNode != previousGraphNode)
			{
				previousGraphNode = nearestGraphNode;
				m_TargetWaypoint = NavMeshUtil.GetNearbyPosition(m_FollowTarget.value.m_Transform.position, (int)nearestGraphNode.GraphIndex);
			}
		}
		m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, m_TargetWaypoint, m_FollowDistance);
	}

	public void OnTargetReached()
	{
		m_bMovingToPosition = false;
		AICharacter value = m_FollowTarget.value;
		if (value != null)
		{
			base.agent.m_Character.SetFaceDirection(value.m_Character.m_x4FacingDirection);
		}
		base.agent.m_Character.PauseMovement(0.5f);
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	protected override void OnStop()
	{
		base.agent.SetRunning(running: false);
	}
}
