using System;
using UnityEngine;

public class JobProgressHUD : BaseIngamePassiveUI, IEventCleaner
{
	private BaseJob m_Job;

	public ProgressBarHUD m_ProgressBar;

	public GameObject m_JobQuotaFlash;

	public Color m_InProgressColour = Color.yellow;

	public Color m_CompletedColour = Color.green;

	public float m_QuotaMetFadeTime = 5f;

	private float m_TimeUntilHudDisappears;

	private bool m_IsInJobTime;

	private bool isVisible = true;

	private bool isUIActive;

	public override bool Init(Player owner)
	{
		bool flag = IsAlreadyInitialised();
		if (base.Init(owner))
		{
			if (flag)
			{
				UnRegisterAllEvents();
			}
			if (m_ProgressBar == null)
			{
			}
			if (m_JobQuotaFlash == null)
			{
			}
			JobsManager instance = JobsManager.GetInstance();
			if (instance != null)
			{
				instance.JobTimeStartedEvent += JobProgressHUD_JobTimeStartedEvent;
				instance.JobTimeEndedEvent += JobProgressHUD_JobTimeEndedEvent;
				instance.OnJobAssigned = (JobsManager.JobEvent)Delegate.Combine(instance.OnJobAssigned, new JobsManager.JobEvent(OnJobAssignedEvent));
				instance.OnJobLost = (JobsManager.JobEvent)Delegate.Combine(instance.OnJobLost, new JobsManager.JobEvent(OnJobLostEvent));
				m_Job = instance.GetCharactersJob(m_LinkedPlayer);
			}
			SolitaryManager instance2 = SolitaryManager.GetInstance();
			if (instance2 != null)
			{
				instance2.CharacterWantedForSolitaryEvent += SolitaryManager_CharacterWantedEvent;
			}
			if (JobsManager.GetInstance() != null)
			{
				m_Job = JobsManager.GetInstance().GetCharactersJob(m_LinkedPlayer);
				if (ShouldShowUI())
				{
					m_IsInJobTime = true;
					SetVisibleAndActiveTo(state: true);
				}
				else
				{
					SetVisibleAndActiveTo(state: false);
				}
			}
			else
			{
				SetVisibleAndActiveTo(state: false);
			}
			return true;
		}
		return false;
	}

	private void UnRegisterAllEvents()
	{
		JobsManager instance = JobsManager.GetInstance();
		if (instance != null)
		{
			instance.JobTimeStartedEvent -= JobProgressHUD_JobTimeStartedEvent;
			instance.JobTimeEndedEvent -= JobProgressHUD_JobTimeEndedEvent;
			instance.OnJobAssigned = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobAssigned, new JobsManager.JobEvent(OnJobAssignedEvent));
			instance.OnJobLost = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobLost, new JobsManager.JobEvent(OnJobLostEvent));
		}
		SolitaryManager instance2 = SolitaryManager.GetInstance();
		if (instance2 != null)
		{
			instance2.CharacterWantedForSolitaryEvent -= SolitaryManager_CharacterWantedEvent;
		}
	}

	protected override void OnDestroy()
	{
		UnRegisterAllEvents();
		base.OnDestroy();
	}

	private void OnJobAssignedEvent(Character employee, JobType type)
	{
		if (employee == m_LinkedPlayer)
		{
			m_Job = JobsManager.GetInstance().GetJob(type);
			if (m_IsInJobTime && m_Job.IsJobActive())
			{
				SetVisibleAndActiveTo(state: true);
			}
		}
	}

	private void OnJobLostEvent(Character employee, JobType type)
	{
		if (employee == m_LinkedPlayer && m_IsInJobTime)
		{
			SetVisibleAndActiveTo(state: false);
		}
	}

	private void SolitaryManager_CharacterWantedEvent(Character character, bool wanted)
	{
		if (!(character == m_LinkedPlayer))
		{
			return;
		}
		m_Job = JobsManager.GetInstance().GetCharactersJob(character);
		if (m_Job != null && m_IsInJobTime && m_Job.IsJobActive())
		{
			if (wanted)
			{
				SetVisibleAndActiveTo(state: false);
			}
			else
			{
				SetVisibleAndActiveTo(state: true);
			}
		}
	}

	private void JobProgressHUD_JobTimeEndedEvent()
	{
		m_IsInJobTime = false;
		SetVisibleAndActiveTo(state: false);
	}

	private void JobProgressHUD_JobTimeStartedEvent(bool isSaveRestore)
	{
		m_IsInJobTime = true;
		m_Job = JobsManager.GetInstance().GetCharactersJob(m_LinkedPlayer);
		if (m_Job != null && ShouldShowUI())
		{
			SetVisibleAndActiveTo(state: true);
		}
	}

	protected void Update()
	{
		if (!(m_Job != null))
		{
			return;
		}
		float normalizedProgress = m_Job.NormalizedProgress;
		m_ProgressBar.SetCurrentPercentage(normalizedProgress);
		if (normalizedProgress == 1f)
		{
			if (m_JobQuotaFlash != null && !m_JobQuotaFlash.activeSelf)
			{
				m_JobQuotaFlash.SetActive(value: true);
			}
			m_ProgressBar.m_SliderFillImage.color = m_CompletedColour;
			if (m_TimeUntilHudDisappears > 0f)
			{
				m_TimeUntilHudDisappears -= UpdateManager.deltaTime;
			}
			if (m_TimeUntilHudDisappears <= 0f)
			{
				SetVisibleAndActiveTo(state: false);
			}
		}
	}

	protected void SetVisibleAndActiveTo(bool state)
	{
		if (!(base.gameObject != null))
		{
			return;
		}
		isUIActive = state;
		if (isVisible)
		{
			base.gameObject.SetActive(state);
		}
		if (m_ProgressBar != null)
		{
			if (m_ProgressBar.m_SliderFillImage != null)
			{
				m_ProgressBar.m_SliderFillImage.color = m_InProgressColour;
			}
			m_ProgressBar.SetCurrentPercentage(0f);
			if (m_JobQuotaFlash != null)
			{
				m_JobQuotaFlash.SetActive(value: false);
			}
		}
		m_TimeUntilHudDisappears = m_QuotaMetFadeTime;
	}

	private bool ShouldShowUI()
	{
		if (m_Job != null && m_Job.IsJobActive() && !m_LinkedPlayer.m_bIsWantedForSolitary)
		{
			return true;
		}
		return false;
	}

	public void CleanUpEvents()
	{
		UnRegisterAllEvents();
	}

	public void SetVisibility(bool visible)
	{
		isVisible = visible;
		if (isUIActive && base.gameObject != null)
		{
			base.gameObject.SetActive(visible);
		}
	}
}
