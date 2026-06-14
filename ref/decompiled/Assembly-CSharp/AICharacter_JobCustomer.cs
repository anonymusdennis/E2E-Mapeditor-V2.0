using NodeCanvas.BehaviourTrees;
using UnityEngine;

public class AICharacter_JobCustomer : AICharacter
{
	public enum PatronTypes
	{
		Unassigned,
		GroupA,
		GroupB,
		GroupC,
		GroupD
	}

	[Header("AICharacter_JobCustomer")]
	public PatronTypes m_PatronType;

	public bool m_bWandersAtWaitPoint;

	public bool m_bWandersExactly;

	public float m_WanderingPrecision = 0.25f;

	public bool m_bAddToJobCustomerPool = true;

	public bool m_bRequestCustomisationOnInit;

	private BaseCustomerJob m_CustomerJob;

	private bool m_bIsBeingUsed;

	private bool m_bIsReadyToBeServed;

	private SpeechPODO m_RequestServiceProximitySpeech;

	private float m_RequestServiceSpeechDistance;

	private float m_RequestServiceSpeechCooldown;

	private float m_SquaredRequestServiceDistance;

	private float m_TimeUntilRequestServiceSpeech;

	private const string HAS_PROXIMITY_SPEECH_KEY = "m_HasProximitySpeech";

	private const string PROXIMITY_SPEECH_KEY = "m_ProximitySpeech";

	private const string PROXIMITY_SPEECH_DISTANCE_KEY = "m_ProximitySpeechDistance";

	private const string PROXIMITY_SPEECH_COOLDOWN_KEY = "m_ProximitySpeechCooldown";

	private const string JOB_CUSTOMER_BEHAVIOUR_KEY = "m_JobCustomerBehaviour";

	private const string ENACTING_JOB_KEY = "m_EnactingJob";

	private const string IS_BEING_USED_KEY = "m_IsBeingUsed";

	private const string NEEDS_CUSTOMISATION_RESET_KEY = "m_NeedsCustomisationReset";

	private const string EXIT_POINT_KEY = "m_ExitPoint";

	private const string HAS_EXIT_POINT_KEY = "m_HasExitPoint";

	private const string SHOULD_WANDER_AT_EXIT_POINT = "m_ShouldWanderAtWaitPosition";

	private const string SHOULD_WANDER_EXACTLY = "m_ShouldJobWanderExactly";

	private const string WANDER_PRECISION = "m_WanderPrecision";

	private const string HAS_ROUTINE_CHANGED_TARGET_POSITION = "m_HasRoutineGoalPosition";

	private const string ROUTINE_CHANGED_TARGET_POSITION = "m_RoutineGoalPosition";

	private const string SHOULD_RUN_TO_ROUTINE_TARGET_POSITION = "m_ShouldRunToRoutinePosition";

	private const string WAITING_FOR_SERVICE_TEXT = "m_WaitingForServiceText";

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		base.StartInit();
		if (m_bAddToJobCustomerPool)
		{
			JobCustomerRequester instance = JobCustomerRequester.GetInstance();
			if (instance != null)
			{
				instance.RegisterCustomerWithSystem(this);
			}
		}
		if (m_AIBlackboard != null)
		{
			m_AIBlackboard.SetValue("m_ShouldWanderAtWaitPosition", m_bWandersAtWaitPoint);
			m_AIBlackboard.SetValue("m_ShouldJobWanderExactly", m_bWandersExactly);
			m_AIBlackboard.SetValue("m_WanderPrecision", m_WanderingPrecision);
		}
		if (T17NetManager.IsMasterClient && m_bRequestCustomisationOnInit)
		{
			SetJobCustomerCustomisation.ApplyCustomisation(this);
		}
		return T17BehaviourManager.INITSTATE.IS_FINISHED;
	}

	public void ClearNonGenericJobBlackboardValues()
	{
		if (m_AIBlackboard != null)
		{
			m_AIBlackboard.SetValue("m_HasProximitySpeech", false);
		}
	}

	public void SetupForJob(BaseCustomerJob job, BehaviourTree jobBehaviour)
	{
		m_CustomerJob = job;
		if (m_AIBlackboard != null)
		{
			m_AIBlackboard.SetValue("m_JobCustomerBehaviour", jobBehaviour);
			m_AIBlackboard.SetValue("m_EnactingJob", job);
			m_AIBlackboard.SetValue("m_WaitingForServiceText", job.m_CustomerWaitingForServiceLines);
		}
	}

	public void SetIsBeingUsed(bool isbeingUsed, bool needsCustomisationReset)
	{
		m_bIsBeingUsed = isbeingUsed;
		m_bIsReadyToBeServed = false;
		if (m_AIBlackboard != null)
		{
			m_AIBlackboard.SetValue("m_IsBeingUsed", isbeingUsed);
			if (isbeingUsed)
			{
				m_AIBlackboard.SetValue("m_NeedsCustomisationReset", needsCustomisationReset);
				CancelRoutineChangedTargetPosition();
			}
		}
	}

	public void SetWaitingPoint(GameObject waitingPoint)
	{
		if (m_AIBlackboard != null)
		{
			if (waitingPoint != null)
			{
				m_AIBlackboard.SetValue("m_ExitPoint", waitingPoint);
			}
			m_AIBlackboard.SetValue("m_HasExitPoint", waitingPoint != null);
		}
	}

	public void CancelRoutineChangedTargetPosition()
	{
		m_AIBlackboard.SetValue("m_HasRoutineGoalPosition", false);
	}

	public void SetRoutineChangedTargetPosition(Vector3 position, bool shouldRun)
	{
		if (m_AIBlackboard != null)
		{
			m_AIBlackboard.SetValue("m_HasRoutineGoalPosition", true);
			m_AIBlackboard.SetValue("m_RoutineGoalPosition", position);
			m_AIBlackboard.SetValue("m_ShouldRunToRoutinePosition", shouldRun);
		}
	}

	public void ReadyToBeServed()
	{
		if (!m_bIsReadyToBeServed)
		{
			SoftSetReadyToBeServed();
			if (m_CustomerJob != null && m_CustomerJob is ServiceCustomerViaProxyJob)
			{
				ServiceCustomerViaProxyJob serviceCustomerViaProxyJob = m_CustomerJob as ServiceCustomerViaProxyJob;
				serviceCustomerViaProxyJob.SetCustomerReadyToBeServedRPC(this);
			}
		}
	}

	public void SoftSetReadyToBeServed()
	{
		m_bIsReadyToBeServed = true;
		m_TimeUntilRequestServiceSpeech = 0f;
	}

	public void SetProximitySpeech(SpeechPODO speech, float distanceFromOwner, float speechCooldown)
	{
		if (m_AIBlackboard != null && speech.IsSet())
		{
			m_AIBlackboard.SetValue("m_HasProximitySpeech", true);
			m_RequestServiceSpeechDistance = distanceFromOwner;
			m_SquaredRequestServiceDistance = distanceFromOwner * distanceFromOwner;
			m_RequestServiceSpeechCooldown = speechCooldown;
			m_RequestServiceProximitySpeech = speech;
		}
	}

	public override void ControlledUpdate()
	{
		base.ControlledUpdate();
		if (T17NetManager.IsMasterClient && m_bIsBeingUsed && m_bIsReadyToBeServed && m_RequestServiceProximitySpeech != null && m_RequestServiceProximitySpeech.IsSet())
		{
			CheckForEmployeeProximitySpeech();
		}
	}

	private void CheckForEmployeeProximitySpeech()
	{
		if (m_TimeUntilRequestServiceSpeech <= 0f)
		{
			Character employee = m_CustomerJob.Employee;
			if (employee != null && !employee.IsInteracting())
			{
				float sqrMagnitude = (employee.m_CachedCurrentPosition - m_Character.m_CachedCurrentPosition).sqrMagnitude;
				if (sqrMagnitude < m_SquaredRequestServiceDistance)
				{
					SpeechManager.GetInstance().SaySomething(m_Character, m_RequestServiceProximitySpeech);
					m_TimeUntilRequestServiceSpeech += m_RequestServiceSpeechCooldown;
				}
			}
		}
		else
		{
			m_TimeUntilRequestServiceSpeech -= UpdateManager.deltaTime;
		}
	}

	public bool IsBeingUsedOrServed()
	{
		return m_bIsBeingUsed || m_bIsReadyToBeServed;
	}
}
