using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class JobDisruptionObjective : BaseObjective
{
	public JobType m_TypeToMakeLose;

	private Character m_TargetCharacter;

	private BaseJob m_TargetJobInfo;

	private bool m_bJobWasLost;

	private const string JOBTARGETTOKEN = "$JobTarget";

	private const string JOBTOKEN = "$Job";

	protected override void Child_PickAllTargets()
	{
		if (m_TypeToMakeLose != 0)
		{
			m_TargetJobInfo = JobsManager.GetInstance().GetJob(m_TypeToMakeLose);
			if (m_TargetJobInfo != null && m_TargetJobInfo.Employee != null)
			{
				m_TargetCharacter = m_TargetJobInfo.Employee;
			}
			else if (m_TargetJobInfo != null)
			{
				Character randomInmate = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver);
				JobsManager.GetInstance().AssignCharacterToJob(randomInmate, m_TargetJobInfo.m_Type);
				m_TargetCharacter = randomInmate;
			}
		}
		if (m_TypeToMakeLose == JobType.Invalid || m_TargetCharacter == null || m_TargetJobInfo == null)
		{
			List<BaseJob> currentJobList = JobsManager.GetInstance().GetCurrentJobList();
			List<BaseJob> list = new List<BaseJob>();
			for (int i = 0; i < currentJobList.Count; i++)
			{
				if (currentJobList[i].Employee != m_QuestGiver && currentJobList[i].Employee != m_PlayerOwner)
				{
					list.Add(currentJobList[i]);
				}
			}
			if (list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				m_TargetJobInfo = list[index];
				m_TypeToMakeLose = m_TargetJobInfo.m_Type;
			}
			if (m_TargetJobInfo != null && m_TargetJobInfo.Employee != null)
			{
				m_TargetCharacter = m_TargetJobInfo.Employee;
			}
			if (m_TargetCharacter == null)
			{
				Character randomInmate2 = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver);
				JobsManager.GetInstance().AssignCharacterToJob(randomInmate2, m_TargetJobInfo.m_Type);
				m_TargetCharacter = randomInmate2;
			}
		}
		if (m_TargetJobInfo == null || m_TargetCharacter == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
			return;
		}
		InternalTokenUpdate("$JobTarget", m_TargetCharacter.m_CharacterCustomisation.m_DisplayName, string.Empty);
		InternalTokenUpdate("$Job", m_TargetJobInfo.m_Info.m_NameTextID, string.Empty);
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$JobTarget", Localization.TokenReplaceType.Character);
		AddTokenInternal("$Job", Localization.TokenReplaceType.TextID);
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (m_TargetCharacter != null && !m_bJobWasLost)
		{
			JobsManager instance = JobsManager.GetInstance();
			instance.OnJobLost = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobLost, new JobsManager.JobEvent(OnJobLost));
			JobsManager instance2 = JobsManager.GetInstance();
			instance2.OnJobLost = (JobsManager.JobEvent)Delegate.Combine(instance2.OnJobLost, new JobsManager.JobEvent(OnJobLost));
		}
	}

	private void OnJobLost(Character employee, JobType jobType)
	{
		if (m_TargetJobInfo != null && m_TargetJobInfo.Employee == employee && m_TargetJobInfo.m_Type == jobType)
		{
			JobsManager instance = JobsManager.GetInstance();
			instance.OnJobLost = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobLost, new JobsManager.JobEvent(OnJobLost));
			m_bJobWasLost = true;
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return m_bJobWasLost;
	}

	protected override bool Child_EvaluateStatus()
	{
		return m_bJobWasLost;
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (on)
		{
			if (m_TargetCharacter != null)
			{
				m_TargetCharacter.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
			}
		}
		else if (m_TargetCharacter != null)
		{
			m_TargetCharacter.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (base.PlayerOwner != null)
		{
			if (on && m_TargetCharacter != null && m_TargetCharacter.m_NetView != null)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_TargetCharacter.m_NetView);
			}
			else
			{
				base.PlayerOwner.CancelObjectiveArrow();
			}
		}
	}

	protected override void Child_PostAction()
	{
		JobsManager instance = JobsManager.GetInstance();
		instance.OnJobLost = (JobsManager.JobEvent)Delegate.Remove(instance.OnJobLost, new JobsManager.JobEvent(OnJobLost));
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		Debug.Log("m_TypeToMakeLose : " + m_TypeToMakeLose);
		baseObj.Add(new JProperty("TypeToMakeLose", (int)m_TypeToMakeLose));
		if (ingameSave)
		{
			if (m_TargetCharacter != null)
			{
				baseObj.Add(new JProperty("TargetCharacter", m_TargetCharacter.m_NetView.viewID));
			}
			baseObj.Add(new JProperty("WasJobLost", m_bJobWasLost));
		}
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		m_TypeToMakeLose = (JobType)(int)json.Property("TypeToMakeLose").Value;
		if (!ingameLoad)
		{
			return;
		}
		JProperty jProperty = json.Property("TargetCharacter");
		if (jProperty != null)
		{
			m_TargetCharacter = PhotonView.Find((int)jProperty.Value).GetComponent<Character>();
		}
		JProperty jProperty2 = json.Property("WasJobLost");
		if (jProperty2 != null)
		{
			m_bJobWasLost = (bool)jProperty2.Value;
		}
		m_TargetJobInfo = JobsManager.GetInstance().GetJob(m_TypeToMakeLose);
		if (m_TargetJobInfo == null)
		{
			Debug.LogError("JobDisruptionObjective is null!!");
			return;
		}
		if (!(m_TargetJobInfo != null) || !(m_TargetJobInfo.Employee != null) || m_TargetJobInfo.Employee != m_TargetCharacter)
		{
		}
		InternalTokenUpdate("$JobTarget", m_TargetCharacter.m_CharacterCustomisation.m_DisplayName, string.Empty);
		InternalTokenUpdate("$Job", m_TargetJobInfo.m_Info.m_NameTextID, string.Empty);
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.JobDistruptionObjective;
	}
}
