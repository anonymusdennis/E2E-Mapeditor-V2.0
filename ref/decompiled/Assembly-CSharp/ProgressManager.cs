using System;
using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : T17MonoBehaviour
{
	[Serializable]
	public class TitleInfo
	{
		[Range(0f, 100f)]
		public float percentage;

		public string localizationTag = string.Empty;
	}

	public enum MilestoneStatus
	{
		Locked,
		Pending,
		Achieved
	}

	[Serializable]
	private class MilestoneSaveData
	{
		public int id = -1;

		public int status;
	}

	[Serializable]
	private class MilestoneCollectionSaveData
	{
		public MilestoneSaveData[] milestones;
	}

	private const string MILESTONE_SAVE_ID = "Progress:MilestoneStatus";

	[Header("Milestones")]
	public List<ProgressMilestone> m_Milestones = new List<ProgressMilestone>();

	[Header("Titles")]
	public List<TitleInfo> m_Titles = new List<TitleInfo>();

	private Dictionary<ProgressMilestone, MilestoneStatus> m_MilestoneStatus = new Dictionary<ProgressMilestone, MilestoneStatus>();

	private bool m_bShouldSaveMilestones;

	private static ProgressManager m_Instance;

	public static ProgressManager GetInstance()
	{
		return m_Instance;
	}

	protected override void Awake()
	{
		base.Awake();
		m_Instance = this;
		SetupMilestones();
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Update()
	{
		if (m_bShouldSaveMilestones)
		{
			m_bShouldSaveMilestones = false;
			SaveMilestoneData();
		}
	}

	private void SetupMilestones()
	{
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			ProgressMilestone progressMilestone = m_Milestones[i];
			if (progressMilestone == null)
			{
				continue;
			}
			ProgressMilestone.Criteria[] criteria = m_Milestones[i].criteria;
			foreach (ProgressMilestone.Criteria criteria2 in criteria)
			{
				if (criteria2 == null)
				{
				}
			}
			if (!m_MilestoneStatus.ContainsKey(progressMilestone))
			{
				m_MilestoneStatus.Add(progressMilestone, MilestoneStatus.Locked);
			}
		}
	}

	private ProgressMilestone FindMilestone(int id)
	{
		ProgressMilestone result = null;
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			if (m_Milestones[i] != null && m_Milestones[i].id == id)
			{
				result = m_Milestones[i];
				break;
			}
		}
		return result;
	}

	public void SetMilestoneAchieved(int milestoneID, bool achieved)
	{
		ProgressMilestone progressMilestone = FindMilestone(milestoneID);
		MilestoneStatus value = MilestoneStatus.Locked;
		if (!(progressMilestone == null) && m_MilestoneStatus.TryGetValue(progressMilestone, out value))
		{
			MilestoneStatus milestoneStatus = MilestoneStatus.Locked;
			milestoneStatus = (achieved ? (progressMilestone.delayedUnlock ? MilestoneStatus.Pending : MilestoneStatus.Achieved) : MilestoneStatus.Locked);
			SetMilestoneStatus_Internal(progressMilestone, milestoneStatus);
		}
	}

	public void ProcessPendingMilestones()
	{
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			ProgressMilestone progressMilestone = m_Milestones[i];
			MilestoneStatus value = MilestoneStatus.Locked;
			if (progressMilestone != null && m_MilestoneStatus.TryGetValue(progressMilestone, out value) && value == MilestoneStatus.Pending)
			{
				SetMilestoneStatus_Internal(progressMilestone, MilestoneStatus.Achieved);
			}
		}
	}

	private void SetMilestoneStatus_Internal(ProgressMilestone milestone, MilestoneStatus status)
	{
		if (m_MilestoneStatus[milestone] != status)
		{
			m_MilestoneStatus[milestone] = status;
			if (status == MilestoneStatus.Achieved)
			{
				OnMilestoneAchieved(milestone);
			}
			m_bShouldSaveMilestones = true;
		}
	}

	public bool GetMilestoneAchieved(int milestoneID)
	{
		ProgressMilestone progressMilestone = FindMilestone(milestoneID);
		MilestoneStatus value = MilestoneStatus.Locked;
		if (progressMilestone == null || !m_MilestoneStatus.TryGetValue(progressMilestone, out value))
		{
			return false;
		}
		return value != MilestoneStatus.Locked;
	}

	public int GetNumberOfPendingMilestones()
	{
		int num = 0;
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			ProgressMilestone progressMilestone = m_Milestones[i];
			MilestoneStatus value = MilestoneStatus.Locked;
			if (progressMilestone != null && m_MilestoneStatus.TryGetValue(progressMilestone, out value) && value == MilestoneStatus.Pending)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfPendingRewards()
	{
		int num = 0;
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			ProgressMilestone progressMilestone = m_Milestones[i];
			MilestoneStatus value = MilestoneStatus.Locked;
			if (progressMilestone != null && m_MilestoneStatus.TryGetValue(progressMilestone, out value) && value == MilestoneStatus.Pending)
			{
				num += progressMilestone.rewards.count;
			}
		}
		return num;
	}

	private void OnMilestoneAchieved(ProgressMilestone milestone)
	{
		UnlockManager instance = UnlockManager.GetInstance();
		if (instance != null)
		{
			instance.UnlockCustomisationSet(milestone.rewards);
		}
	}

	public string CalculateTitleFromCompletion(float percentage)
	{
		if (m_Titles.Count <= 0)
		{
			return string.Empty;
		}
		int num = Mathf.FloorToInt(percentage * 100f);
		int num2 = 0;
		for (int i = 1; i < m_Titles.Count && !(m_Titles[i].percentage > (float)num); i++)
		{
			num2++;
		}
		string result = string.Empty;
		if (num2 >= 0 && num2 < m_Titles.Count)
		{
			result = m_Titles[num2].localizationTag;
		}
		return result;
	}

	public float CalculateOverallProgress()
	{
		StatSystem instance = StatSystem.GetInstance();
		if (instance == null)
		{
			return 0f;
		}
		bool[] ruleStatuses = new bool[0];
		float[] statValues = new float[0];
		float[] refValues = new float[0];
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		LevelDataManager instance2 = LevelDataManager.GetInstance();
		for (int i = 0; i < m_Milestones.Count; i++)
		{
			if (instance2.IsLevelAvailable(m_Milestones[i].m_prison))
			{
				int totalNumberRules = 0;
				int totalNumberCompleted = 0;
				instance.GetProgressCountsForMilestone(m_Milestones[i].id, ref ruleStatuses, ref statValues, ref refValues, out totalNumberRules, out totalNumberCompleted);
				num2 += totalNumberRules;
				num3 += totalNumberCompleted;
			}
		}
		return Mathf.Clamp01((float)num3 / (float)num2);
	}

	public int GetHoursInPrison()
	{
		StatSystem instance = StatSystem.GetInstance();
		if (instance == null)
		{
			return 0;
		}
		return Mathf.FloorToInt(instance.GetStatValue(31));
	}

	public int GetTimedUnlocksUnlocked()
	{
		StatSystem instance = StatSystem.GetInstance();
		if (instance == null)
		{
			return 0;
		}
		return Mathf.FloorToInt(instance.GetStatValue(32));
	}

	private void ResetMilestones()
	{
		m_MilestoneStatus.Clear();
		SetupMilestones();
	}

	public bool LoadData()
	{
		ResetMilestones();
		MilestoneSaveData[] array = LoadMilestoneData();
		if (m_MilestoneStatus.Count > 0 && array != null && array.Length > 0)
		{
			foreach (MilestoneSaveData milestoneSaveData in array)
			{
				if (milestoneSaveData.id >= 0 && milestoneSaveData.status >= 0)
				{
					ProgressMilestone progressMilestone = FindMilestone(milestoneSaveData.id);
					if (progressMilestone != null && m_MilestoneStatus.ContainsKey(progressMilestone))
					{
						MilestoneStatus status = (MilestoneStatus)milestoneSaveData.status;
						m_MilestoneStatus[progressMilestone] = status;
					}
				}
			}
		}
		return true;
	}

	private bool SaveMilestoneData()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty("Progress:MilestoneStatus"))
		{
			List<ProgressMilestone> list = new List<ProgressMilestone>(m_MilestoneStatus.Keys);
			MilestoneCollectionSaveData milestoneCollectionSaveData = new MilestoneCollectionSaveData();
			milestoneCollectionSaveData.milestones = new MilestoneSaveData[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				MilestoneStatus milestoneStatus = m_MilestoneStatus[list[i]];
				if (milestoneStatus != 0)
				{
					milestoneCollectionSaveData.milestones[i] = new MilestoneSaveData();
					milestoneCollectionSaveData.milestones[i].id = list[i].id;
					milestoneCollectionSaveData.milestones[i].status = (int)milestoneStatus;
				}
			}
			string value = JsonUtility.ToJson(milestoneCollectionSaveData);
			if (!string.IsNullOrEmpty(value))
			{
				GlobalSave.GetInstance().Set("Progress:MilestoneStatus", value);
				GlobalSave.GetInstance().RequestSave();
				flag = true;
			}
		}
		if (!flag)
		{
		}
		return flag;
	}

	private MilestoneSaveData[] LoadMilestoneData()
	{
		MilestoneSaveData[] result = null;
		string value = string.Empty;
		GlobalSave.GetInstance().Get("Progress:MilestoneStatus", out value, string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			MilestoneCollectionSaveData milestoneCollectionSaveData = JsonUtility.FromJson<MilestoneCollectionSaveData>(value);
			if (milestoneCollectionSaveData != null && milestoneCollectionSaveData.milestones != null)
			{
				result = milestoneCollectionSaveData.milestones;
			}
		}
		else
		{
			result = new MilestoneSaveData[0];
		}
		return result;
	}
}
