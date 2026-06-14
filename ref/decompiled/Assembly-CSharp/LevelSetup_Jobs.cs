using System;
using System.Collections.Generic;

public class LevelSetup_Jobs : BaseComponentSetup
{
	[Serializable]
	public class JobInfo
	{
		public BuildingBlockManager.DefaultLimitationGroups m_LimitationGroup = BuildingBlockManager.DefaultLimitationGroups.TOTAL;

		public BaseJob m_JobPrefabRef;

		public bool m_EmployNPCOnStart;

		public int m_DaysKeptVacant = 2;
	}

	public JobsManager m_JobsManager;

	public JobInfo[] m_JobsTolookFor = new JobInfo[0];

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_8;
	}

	public override SetupReturnState Setup()
	{
		if (m_JobsManager == null)
		{
			return FinishedAndRemove();
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return FinishedAndRemove();
		}
		List<JobsManager.PrisonJobInfo> list = new List<JobsManager.PrisonJobInfo>();
		int num = m_JobsTolookFor.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_JobsTolookFor[i] != null && m_JobsTolookFor[i].m_LimitationGroup != BuildingBlockManager.DefaultLimitationGroups.TOTAL && m_JobsTolookFor[i].m_JobPrefabRef != null && m_JobsTolookFor[i].m_DaysKeptVacant >= 0 && instance.GetLimitationTotal(m_JobsTolookFor[i].m_LimitationGroup.ToString()) > 0)
			{
				JobsManager.PrisonJobInfo prisonJobInfo = new JobsManager.PrisonJobInfo();
				prisonJobInfo.m_DaysKeptVacant = m_JobsTolookFor[i].m_DaysKeptVacant;
				prisonJobInfo.m_EmployNPCOnStart = m_JobsTolookFor[i].m_EmployNPCOnStart;
				prisonJobInfo.m_JobPrefabRef = m_JobsTolookFor[i].m_JobPrefabRef;
				list.Add(prisonJobInfo);
			}
		}
		m_JobsManager.m_PrisonJobsInfo = list.ToArray();
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		return Setup();
	}
}
