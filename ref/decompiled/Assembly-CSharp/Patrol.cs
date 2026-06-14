using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Patrol : ActionTask<AICharacter>
{
	public bool m_UseCurrentRoutine;

	public Routines m_Routine = Routines.UNASSIGNED;

	public bool m_bHavePatrol;

	private PatrolPath m_PatrolPath;

	public Routines m_CurrentlyRunningRoutine = Routines.UNASSIGNED;

	private int m_iCurrentWaypoint;

	private PatrolPath.PathNode m_CurrentWaypoint;

	private bool m_bMovingToPosition;

	private float m_fWaitAtWaypointUntil;

	private Vector2 m_FacingDirection = Vector2.one;

	private bool m_bFaceDirection;

	private bool m_bUseOtherDirection;

	private float m_fOnErrorCooldownTime = 1f;

	private float m_fOnErrorCooldownTimer;

	private bool m_bNeedToRepath;

	private bool m_bInited;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelled;

	protected override string info => "Patrol" + ((!m_UseCurrentRoutine) ? ("[" + m_Routine.ToString() + "]") : string.Empty);

	protected override string OnInit()
	{
		InitRoutine();
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelled = OnPathCancelled;
		return base.OnInit();
	}

	private void InitRoutine()
	{
		if (!m_bInited && RoutineManager.GetInstance().RoutineManagerReady())
		{
			RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
			if (currentRoutine != null)
			{
				RoutineManager.GetInstance().OnRoutineChanged += RoutineChanged;
				RoutineChanged(null, currentRoutine, forceEnd: false);
				m_bInited = true;
			}
		}
	}

	public void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		PatrolPath patrolPath = null;
		if (newRoutine != null)
		{
			patrolPath = base.agent.m_AIPatrols.GetRandomPatrolObject(newRoutine.m_BaseRoutineType);
		}
		m_bHavePatrol = patrolPath != null;
		m_bNeedToRepath = true;
		if (m_PatrolPath != null && newRoutine != null && base.agent.m_AIPatrols.RoutineHasPatrol(newRoutine.m_BaseRoutineType, m_PatrolPath))
		{
			m_bNeedToRepath = false;
		}
	}

	private void GetNewPatrolPath()
	{
		m_CurrentlyRunningRoutine = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
		if (m_UseCurrentRoutine || m_Routine == m_CurrentlyRunningRoutine)
		{
			m_PatrolPath = base.agent.m_AIPatrols.GetRandomPatrolObject(m_CurrentlyRunningRoutine);
			SetRandomWaypoint();
		}
	}

	private void SetRandomWaypoint()
	{
		if (m_PatrolPath != null)
		{
			if (m_PatrolPath.m_bBidirectional && Random.value > 0.5f)
			{
				m_bUseOtherDirection = true;
			}
			else
			{
				m_bUseOtherDirection = false;
			}
			if (m_PatrolPath.m_bStartAtFirstWaypoint)
			{
				m_iCurrentWaypoint = 0;
			}
			else
			{
				m_iCurrentWaypoint = Random.Range(0, m_PatrolPath.m_vPathNodes.Length);
			}
			m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
			m_fWaitAtWaypointUntil = 0f;
			m_bMovingToPosition = false;
		}
		else
		{
			EndAction(false);
		}
	}

	private void SetNextWaypoint()
	{
		if (m_bUseOtherDirection)
		{
			m_iCurrentWaypoint--;
			if (m_iCurrentWaypoint < 0)
			{
				m_iCurrentWaypoint = m_PatrolPath.m_vPathNodes.Length - 1;
			}
		}
		else
		{
			m_iCurrentWaypoint++;
			if (m_iCurrentWaypoint >= m_PatrolPath.m_vPathNodes.Length)
			{
				m_iCurrentWaypoint = 0;
			}
		}
		m_CurrentWaypoint = m_PatrolPath.m_vPathNodes[m_iCurrentWaypoint];
	}

	public void OnTargetReached()
	{
		if (m_PatrolPath == null || m_CurrentWaypoint == null)
		{
			m_bMovingToPosition = false;
			return;
		}
		if (m_PatrolPath.m_vPathNodes.Length == 1)
		{
			m_fWaitAtWaypointUntil = float.MaxValue;
		}
		else
		{
			m_fWaitAtWaypointUntil = UpdateManager.time + m_CurrentWaypoint.m_fWaitTimer + Random.Range(0f, m_CurrentWaypoint.m_fWaitVariance);
		}
		if (m_CurrentWaypoint.m_bSetDirection)
		{
			m_bFaceDirection = true;
			Vector3 vector = m_CurrentWaypoint.m_FacingDirection * Vector3.forward;
			m_FacingDirection.x = vector.x;
			m_FacingDirection.y = vector.y;
		}
		else
		{
			m_bFaceDirection = false;
		}
		if (SpeechPODO.IsValid(m_CurrentWaypoint.m_CharacterSpeech))
		{
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_CurrentWaypoint.m_CharacterSpeech);
		}
		SetNextWaypoint();
		m_bMovingToPosition = false;
	}

	public void OnPathCancelled()
	{
		m_fOnErrorCooldownTimer = m_fOnErrorCooldownTime;
		m_bMovingToPosition = false;
	}

	private void MoveToWaypoint()
	{
		if (m_CurrentWaypoint != null)
		{
			Vector3 vNodePos = m_CurrentWaypoint.m_vNodePos;
			base.agent.SetRunning(m_CurrentWaypoint.m_bRunToNode);
			m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelled, vNodePos);
		}
		else
		{
			EndAction(false);
		}
	}

	protected override void OnExecute()
	{
		if (!m_bInited)
		{
			InitRoutine();
		}
		if (!m_bHavePatrol)
		{
			EndAction(false);
		}
		else
		{
			GetNewPatrolPath();
		}
	}

	protected override void OnUpdate()
	{
		if (m_bNeedToRepath)
		{
			m_bNeedToRepath = false;
			base.agent.m_AIMovement.CancelCurrentPath();
			GetNewPatrolPath();
		}
		if (m_CurrentWaypoint != null)
		{
			base.agent.SetRunning(m_CurrentWaypoint.m_bRunToNode);
		}
		if (m_bFaceDirection || m_fWaitAtWaypointUntil > UpdateManager.time)
		{
			if (m_bFaceDirection)
			{
				base.agent.m_Character.CalcFaceDirection(m_FacingDirection);
				m_bFaceDirection = false;
			}
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

	protected override void OnStop()
	{
		m_fWaitAtWaypointUntil = 0f;
		m_bMovingToPosition = false;
		base.agent.SetRunning(running: false);
	}
}
