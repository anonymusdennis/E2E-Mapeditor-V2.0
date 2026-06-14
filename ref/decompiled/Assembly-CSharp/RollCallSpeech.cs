using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class RollCallSpeech : ActionTask<AICharacter>
{
	public bool m_UseCurrentRoutine;

	public Routines m_Routine = Routines.UNASSIGNED;

	public bool m_bHavePatrol;

	public bool m_bRequirePatrol = true;

	public float m_fWaitBeforeSpeech = 4f;

	private float m_fSpeechWaitTimer;

	public float m_fSpeechLineduration = 4f;

	public float m_fShakedownAnnounceTime = 15f;

	private bool m_bSpeechStarted;

	private bool m_bSpeechEnded;

	private PatrolPath m_SpeechPath;

	private bool m_bReachedFirstWaypoint;

	private int m_iCurrentWaypoint;

	private PatrolPath.PathNode m_CurrentWaypoint;

	private bool m_bMovingToPosition;

	private float m_fWaitAtWaypointUntil;

	private Vector2 m_FacingDirection = Vector2.one;

	private bool m_bFaceDirection;

	private bool m_bUseOtherDirection;

	private float m_fOnErrorCooldownTime = 1f;

	private float m_fOnErrorCooldownTimer;

	private bool m_bInited;

	public string m_SpeechIntroMorning = "&Text.Guards.RollCallStart";

	public string m_SpeechIntro = "&Text.Guards.RollCallStart";

	public string m_SpeechMain = "&Text.Guards.RollCallMain";

	public string m_ShakedownIntro = "&Text.Guards.Shakedown";

	public string m_ShakedownNames = "&Text.Guards.ShakedownNames";

	public List<string> m_ShakedownTokens = new List<string>(2) { "$name", "$inmate" };

	public string m_DayIntroToken = "$day";

	public string m_RoutineIntroToken = "$routine";

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string info => "RollCallSpeech" + ((!m_UseCurrentRoutine) ? ("[" + m_Routine.ToString() + "]") : string.Empty);

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		InitRoutine();
		return base.OnInit();
	}

	private void InitRoutine()
	{
		if (m_bInited)
		{
			return;
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (!(instance != null) || !instance.RoutineManagerReady())
		{
			return;
		}
		instance.OnRoutineChanged -= RoutineChanged;
		instance.OnRoutineChanged += RoutineChanged;
		RoutinesData.Routine currentRoutine = instance.GetCurrentRoutine();
		if (currentRoutine != null)
		{
			if (currentRoutine.m_BaseRoutineType == m_Routine)
			{
				GetNewPatrolPath();
			}
			m_bInited = true;
		}
	}

	public void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType != m_Routine)
		{
			m_bSpeechEnded = false;
			m_bSpeechStarted = false;
			m_bReachedFirstWaypoint = false;
		}
	}

	private void GetNewPatrolPath()
	{
		AIPatrols aIPatrols = base.agent.m_AIPatrols;
		if (aIPatrols != null)
		{
			m_SpeechPath = base.agent.m_AIPatrols.GetRandomPatrolObject(m_Routine);
		}
		m_bHavePatrol = m_SpeechPath != null;
		SetRandomWaypoint();
	}

	private void SetRandomWaypoint()
	{
		if (m_SpeechPath != null)
		{
			if (m_SpeechPath.m_bBidirectional && Random.value > 0.5f)
			{
				m_bUseOtherDirection = true;
			}
			else
			{
				m_bUseOtherDirection = false;
			}
			if (m_SpeechPath.m_bStartAtFirstWaypoint)
			{
				m_iCurrentWaypoint = 0;
			}
			else
			{
				m_iCurrentWaypoint = Random.Range(0, m_SpeechPath.m_vPathNodes.Length);
			}
			m_CurrentWaypoint = m_SpeechPath.m_vPathNodes[m_iCurrentWaypoint];
			m_fWaitAtWaypointUntil = 0f;
			m_bMovingToPosition = false;
		}
		else if (m_bRequirePatrol)
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
				m_iCurrentWaypoint = m_SpeechPath.m_vPathNodes.Length - 1;
			}
		}
		else
		{
			m_iCurrentWaypoint++;
			if (m_iCurrentWaypoint >= m_SpeechPath.m_vPathNodes.Length)
			{
				m_iCurrentWaypoint = 0;
			}
		}
		m_CurrentWaypoint = m_SpeechPath.m_vPathNodes[m_iCurrentWaypoint];
	}

	public void OnTargetReached()
	{
		if (m_SpeechPath == null || m_CurrentWaypoint == null)
		{
			m_bMovingToPosition = false;
			return;
		}
		if (m_SpeechPath.m_vPathNodes.Length == 1)
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
		if (!m_bReachedFirstWaypoint)
		{
			m_bReachedFirstWaypoint = true;
			m_fSpeechWaitTimer = m_fWaitBeforeSpeech;
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
			m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, vNodePos);
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
		if (m_bRequirePatrol && !m_bHavePatrol)
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
			else if (m_bRequirePatrol)
			{
				MoveToWaypoint();
			}
			else if (!m_bReachedFirstWaypoint)
			{
				m_bReachedFirstWaypoint = true;
				m_fSpeechWaitTimer = m_fWaitBeforeSpeech;
			}
		}
		if (!(m_fSpeechWaitTimer > 0f) && m_bRequirePatrol)
		{
			return;
		}
		m_fSpeechWaitTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (!(m_fSpeechWaitTimer <= 0f) || !m_bReachedFirstWaypoint)
		{
			return;
		}
		float num = (float)RoutineManager.GetInstance().GetTimeLeftInRoutine().TotalMinutes;
		if (num - m_fSpeechLineduration > m_fShakedownAnnounceTime)
		{
			if (!m_bSpeechStarted)
			{
				SayStartingMessage();
				m_bSpeechStarted = true;
			}
			else
			{
				SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_SpeechMain, SpeechTone.Positive, m_fSpeechLineduration, 7);
				m_fSpeechWaitTimer += m_fSpeechLineduration;
			}
		}
		else
		{
			if (m_bSpeechEnded)
			{
				return;
			}
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_ShakedownIntro, SpeechTone.Positive, m_fSpeechLineduration, 15);
			List<int> namesOfInmatesToSearch = AICharacter_Guard.GetNamesOfInmatesToSearch();
			if (namesOfInmatesToSearch == null)
			{
				return;
			}
			List<SpeechManager.Token> list = new List<SpeechManager.Token>();
			for (int i = 0; i < namesOfInmatesToSearch.Count && i < m_ShakedownTokens.Count; i++)
			{
				if (namesOfInmatesToSearch[i] != -1 && !string.IsNullOrEmpty(m_ShakedownTokens[i]))
				{
					list.Add(new SpeechManager.Token(m_ShakedownTokens[i], namesOfInmatesToSearch[i], bIsCharacterNetviewID: true));
				}
			}
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_ShakedownNames, list, SpeechTone.Positive, m_fSpeechLineduration * 1.25f, 14);
			m_bSpeechEnded = true;
		}
	}

	private void SayStartingMessage()
	{
		bool flag = RoutineManager.GetInstance().GetCurrentRoutineSubType() == RoutineSubTypes.MorningRollCall;
		List<SpeechManager.Token> list = new List<SpeechManager.Token>();
		if (flag)
		{
			string replacementString = string.Empty;
			if (RoutineManager.GetInstance() != null)
			{
				replacementString = RoutineManager.GetInstance().GetDaysElapsed().ToString();
			}
			list.Add(new SpeechManager.Token(m_DayIntroToken, replacementString, bIsCharacterNetviewID: false, bIsRequireTranslating: false));
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_SpeechIntroMorning, list, SpeechTone.Positive, m_fSpeechLineduration, 7);
		}
		else
		{
			string localizationTag = RoutineManager.GetInstance().GetCurrentRoutine().m_LocalizationTag;
			list.Add(new SpeechManager.Token(m_RoutineIntroToken, localizationTag, bIsCharacterNetviewID: false, bIsRequireTranslating: true));
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_SpeechIntro, list, SpeechTone.Positive, m_fSpeechLineduration, 7);
		}
		m_fSpeechWaitTimer += m_fSpeechLineduration;
	}

	protected override void OnStop()
	{
		m_fWaitAtWaypointUntil = 0f;
		m_bMovingToPosition = false;
		base.agent.SetRunning(running: false);
	}
}
