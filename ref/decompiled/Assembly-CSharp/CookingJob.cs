using System;
using System.Collections.Generic;
using UnityEngine;

public class CookingJob : ProcessItemJob
{
	[Header("Cooking Job")]
	public ItemData m_OvercookedItemData;

	public float m_CookedLowerBound = 5f;

	public float m_CookedUpperBound = 10f;

	private List<CookingItemProcessor> m_CookingProcessors = new List<CookingItemProcessor>();

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		m_CookingProcessors.Clear();
		for (int num = base.RoomData.m_Processors.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Processors[num];
			if (interactiveObject != null)
			{
				CookingItemProcessor component = interactiveObject.GetComponent<CookingItemProcessor>();
				if (component != null)
				{
					component.m_OvercookedItem = m_OvercookedItemData;
					component.m_CookedLowerBound = m_CookedLowerBound;
					component.m_CookedUpperBound = m_CookedUpperBound;
					component.m_bIsInterruptable = true;
					component.SetupUIsForTotalProcessingTime();
					m_CookingProcessors.Add(component);
				}
			}
		}
		JobsManager instance = JobsManager.GetInstance();
		instance.OnJobAssigned = (JobsManager.JobEvent)Delegate.Combine(instance.OnJobAssigned, new JobsManager.JobEvent(EmployeeAssigned));
		JobsManager instance2 = JobsManager.GetInstance();
		instance2.OnJobAssigned = (JobsManager.JobEvent)Delegate.Combine(instance2.OnJobAssigned, new JobsManager.JobEvent(EmployeeLostJob));
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (JobsManager.GetInstance() != null)
		{
			JobsManager instance = JobsManager.GetInstance();
			instance.OnJobAssigned = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobAssigned, new JobsManager.JobEvent(EmployeeAssigned));
			JobsManager instance2 = JobsManager.GetInstance();
			instance2.OnJobAssigned = (JobsManager.JobEvent)Delegate.Remove(instance2.OnJobAssigned, new JobsManager.JobEvent(EmployeeLostJob));
		}
	}

	private void EmployeeAssigned(Character employee, JobType jobType)
	{
		if (jobType == JobType.Kitchen)
		{
			if (employee != null && employee.m_CharacterStats.m_bIsPlayer)
			{
				SetAllCookingUIsVisibleForPlayers(state: true);
			}
			else
			{
				SetAllCookingUIsVisibleForPlayers(state: false);
			}
		}
	}

	private void EmployeeLostJob(Character employee, JobType jobType)
	{
		if (jobType == JobType.Kitchen && (!(base.Employee != null) || !base.Employee.m_CharacterStats.m_bIsPlayer))
		{
			SetAllCookingUIsVisibleForPlayers(state: false);
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (base.Employee != null && base.Employee.m_CharacterStats.m_bIsPlayer)
		{
			SetAllCookingUIsVisibleForPlayers(state: true);
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		SetAllCookingUIsVisibleForPlayers(state: false);
	}

	private void SetAllCookingUIsVisibleForPlayers(bool state)
	{
		for (int num = m_CookingProcessors.Count - 1; num >= 0; num--)
		{
			m_CookingProcessors[num].SetUIVisibleForPlayers(state);
		}
	}
}
