public class StayInRoomJob : BaseJob, IControlledUpdate
{
	public SpeechPODO m_TaunterInRoomSpeech;

	public float m_TaunterInterval = 5f;

	private float m_TimeUntilTaunterSpeech;

	private float m_EmployeeInRoomTimer;

	private bool m_bIsJobTimeActive;

	private FakeCharacter m_Taunter;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		UpdateManager.GetInstance().Register(this, UpdateCategory.ModeratePeriodic);
		if (base.RoomData.m_JobTaunters.Count > 0)
		{
			m_Taunter = base.RoomData.m_JobTaunters[0];
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.ModeratePeriodic);
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		m_EmployeeInRoomTimer = 0f;
		m_TimeUntilTaunterSpeech = m_TaunterInterval;
		m_bIsJobTimeActive = true;
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		m_bIsJobTimeActive = false;
	}

	public void ControlledUpdate()
	{
		if (!m_bIsJobTimeActive || !T17NetManager.IsMasterClient || base.QuotaAchieved >= base.QuotaTarget || !(base.Employee != null) || !(base.Employee.m_CurrentLocation == base.Room))
		{
			return;
		}
		m_EmployeeInRoomTimer += UpdateManager.deltaTime;
		if (m_EmployeeInRoomTimer >= 1f)
		{
			IncrementQuotaAchieved();
			m_EmployeeInRoomTimer -= 1f;
		}
		m_TimeUntilTaunterSpeech -= UpdateManager.deltaTime;
		if (m_TimeUntilTaunterSpeech <= 0f)
		{
			if (m_Taunter != null)
			{
				m_Taunter.SaySomethingRPC(m_TaunterInRoomSpeech);
			}
			m_TimeUntilTaunterSpeech += m_TaunterInterval;
		}
	}

	public void ControlledFixedUpdate()
	{
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
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
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
