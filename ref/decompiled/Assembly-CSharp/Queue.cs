using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Queue : ActionTask<AICharacter>
{
	private float m_fRepathTimer;

	public float m_fRepathMinTime = 0.5f;

	public float m_fRepathMaxTime = 1f;

	private float m_fRunDistance = 2f;

	private Directionx4 m_FacingDirection;

	private Vector3 m_TargetPosition;

	protected override string info => "Queue";

	protected override void OnExecute()
	{
		m_fRepathTimer = 0f;
		m_fRunDistance = 1.5f + Random.Range(0f, 2f);
		m_FacingDirection = base.agent.m_Character.m_x4FacingDirection;
	}

	protected override void OnUpdate()
	{
		m_fRepathTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_fRepathTimer <= 0f)
		{
			m_fRepathTimer = Random.Range(m_fRepathMinTime, m_fRepathMaxTime);
			Vector3 cachedCurrentPosition = base.agent.m_Character.m_CachedCurrentPosition;
			if (NPCManager.GetInstance().GetQueuePosition(base.agent.m_Character, out m_TargetPosition, out m_FacingDirection))
			{
				base.agent.m_Character.SetFaceDirection(m_FacingDirection);
				EndAction(true);
				return;
			}
			base.agent.m_AIMovement.TravelToPosition(OnTargetReached, OnPathCancelled, m_TargetPosition);
		}
		float sqrMagnitude = (m_TargetPosition - base.agent.m_Character.m_CachedCurrentPosition).sqrMagnitude;
		float num = m_TargetPosition.z - base.agent.m_Character.m_CachedCurrentPosition.z;
		bool running = sqrMagnitude > m_fRunDistance || num > 1f;
		base.agent.SetRunning(running);
	}

	public void OnTargetReached()
	{
		base.agent.m_Character.SetFaceDirection(m_FacingDirection);
	}

	public void OnPathCancelled()
	{
	}

	protected override void OnStop()
	{
		if (NPCManager.GetInstance() != null)
		{
			NPCManager.GetInstance().RemoveFromQueue(base.agent.m_Character);
		}
		base.agent.SetRunning(running: false);
	}
}
