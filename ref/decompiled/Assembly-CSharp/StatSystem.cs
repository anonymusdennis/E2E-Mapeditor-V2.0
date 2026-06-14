using System.Collections;
using UnityEngine;

public class StatSystem : MonoBehaviour
{
	private enum STAT_TYPE
	{
		ST_COUNTER,
		ST_ID_HOLDER
	}

	private class Stat
	{
		public string m_Name;

		public int m_ID;

		public float m_Value;

		public StatRule[] m_RefStats;

		public STAT_TYPE m_Type;

		public Hashtable m_IDHolder;

		public Stat()
		{
			m_Name = "STAT:NONAME";
			m_ID = 0;
			m_Value = 0f;
			m_RefStats = null;
			m_Type = STAT_TYPE.ST_COUNTER;
			m_IDHolder = null;
		}
	}

	public enum StatCompare
	{
		EQUAL,
		GREATER_THAN_EQUAL,
		HAS_ID
	}

	private class StatRule
	{
		public Stat m_Stat;

		public float m_RefValue;

		public StatCompare m_Compare;

		public Trophy m_RefTrophy;

		public Milestone m_RefMilestone;

		public StatRule()
		{
			m_Stat = null;
			m_RefValue = 0f;
		}

		public bool Check(ref bool bRuleJustMadeTrueByNewValue, float newValue)
		{
			bool result = false;
			if (m_Stat.m_Type == STAT_TYPE.ST_COUNTER)
			{
				switch (m_Compare)
				{
				case StatCompare.EQUAL:
					if (m_Stat.m_Value == m_RefValue)
					{
						result = true;
					}
					break;
				case StatCompare.GREATER_THAN_EQUAL:
					if (m_Stat.m_Value >= m_RefValue)
					{
						result = true;
					}
					break;
				}
			}
			else
			{
				switch (m_Compare)
				{
				case StatCompare.EQUAL:
					if ((float)m_Stat.m_IDHolder.Keys.Count == m_RefValue)
					{
						result = true;
					}
					break;
				case StatCompare.GREATER_THAN_EQUAL:
					if ((float)m_Stat.m_IDHolder.Keys.Count >= m_RefValue)
					{
						result = true;
					}
					break;
				case StatCompare.HAS_ID:
					if (m_Stat.m_IDHolder.ContainsKey((int)m_RefValue))
					{
						result = true;
						if ((int)m_RefValue == (int)newValue)
						{
							bRuleJustMadeTrueByNewValue = true;
						}
						else
						{
							bRuleJustMadeTrueByNewValue = false;
						}
					}
					break;
				}
			}
			return result;
		}
	}

	private enum Combiner
	{
		C_NA,
		C_AND,
		C_OR
	}

	private class Trophy
	{
		public string m_Name;

		public string m_APIName;

		public int m_TrophyID;

		public int m_TrophyProgress;

		public StatRule[] m_Rules;

		public int m_RuleCount;

		public Combiner m_Combiner;

		public Trophy()
		{
			m_Name = "XX";
			m_APIName = "XX";
			m_TrophyID = -1;
			m_TrophyProgress = 0;
		}

		public bool Check()
		{
			bool flag = false;
			switch (m_Combiner)
			{
			case Combiner.C_NA:
				flag = false;
				break;
			case Combiner.C_AND:
				flag = true;
				break;
			case Combiner.C_OR:
				flag = false;
				break;
			}
			for (uint num = 0u; num < m_Rules.Length; num++)
			{
				bool bRuleJustMadeTrueByNewValue = false;
				bool flag2 = m_Rules[num].Check(ref bRuleJustMadeTrueByNewValue, 0f);
				switch (m_Combiner)
				{
				case Combiner.C_NA:
					flag = flag2;
					break;
				case Combiner.C_AND:
					flag = flag && flag2;
					break;
				case Combiner.C_OR:
					flag = flag || flag2;
					break;
				}
			}
			return flag;
		}

		public int GetProgress()
		{
			m_TrophyProgress = 0;
			for (int i = 0; i < m_Rules.Length; i++)
			{
				bool bRuleJustMadeTrueByNewValue = false;
				m_TrophyProgress += (m_Rules[i].Check(ref bRuleJustMadeTrueByNewValue, 0f) ? 1 : 0);
			}
			return m_TrophyProgress;
		}
	}

	private class Milestone
	{
		public int m_MilestoneID;

		public StatRule[] m_Rules;

		public int m_RuleCount;

		public Combiner m_Combiner;

		public Milestone()
		{
			m_MilestoneID = -1;
		}

		public bool Check()
		{
			bool flag = false;
			switch (m_Combiner)
			{
			case Combiner.C_NA:
				flag = false;
				break;
			case Combiner.C_AND:
				flag = true;
				break;
			case Combiner.C_OR:
				flag = false;
				break;
			}
			for (uint num = 0u; num < m_Rules.Length; num++)
			{
				bool bRuleJustMadeTrueByNewValue = false;
				bool flag2 = m_Rules[num].Check(ref bRuleJustMadeTrueByNewValue, 0f);
				switch (m_Combiner)
				{
				case Combiner.C_NA:
					flag = flag2;
					break;
				case Combiner.C_AND:
					flag = flag && flag2;
					break;
				case Combiner.C_OR:
					flag = flag || flag2;
					break;
				}
			}
			return flag;
		}

		public float GetProgress(ref bool[] completed, ref float[] values)
		{
			int numCompleted = 0;
			int numRules = 0;
			GetNumberOfCompletedRules(ref completed, ref values, out numCompleted, out numRules);
			float result = 1f;
			if (numRules > 0)
			{
				result = Mathf.Clamp01((float)numCompleted / (float)numRules);
			}
			return result;
		}

		public void GetNumberOfCompletedRules(ref bool[] completed, ref float[] values, out int numCompleted, out int numRules)
		{
			numCompleted = 0;
			numRules = m_Rules.Length;
			for (int i = 0; i < m_Rules.Length; i++)
			{
				bool bRuleJustMadeTrueByNewValue = false;
				bool flag = m_Rules[i].Check(ref bRuleJustMadeTrueByNewValue, 0f);
				if (flag)
				{
					numCompleted++;
				}
				if (i < completed.Length)
				{
					completed[i] = flag;
				}
				if (i >= values.Length)
				{
					continue;
				}
				switch (m_Rules[i].m_Stat.m_Type)
				{
				case STAT_TYPE.ST_COUNTER:
					values[i] = m_Rules[i].m_Stat.m_Value;
					break;
				case STAT_TYPE.ST_ID_HOLDER:
					switch (m_Rules[i].m_Compare)
					{
					case StatCompare.EQUAL:
						values[i] = m_Rules[i].m_Stat.m_IDHolder.Keys.Count;
						break;
					case StatCompare.GREATER_THAN_EQUAL:
						values[i] = m_Rules[i].m_Stat.m_IDHolder.Keys.Count;
						break;
					case StatCompare.HAS_ID:
						values[i] = 0f;
						break;
					}
					break;
				}
			}
		}

		public void GetRefValues(ref float[] values)
		{
			for (int i = 0; i < m_Rules.Length; i++)
			{
				if (i < values.Length)
				{
					values[i] = m_Rules[i].m_RefValue;
				}
			}
		}
	}

	private static StatSystem m_theInstance;

	private Stat[] m_Stats;

	private StatRule[] m_StatsRules;

	private Trophy[] m_Trophies;

	private Milestone[] m_Milestones;

	private uint m_StatCreateCount;

	private uint m_TrophyCreateCount;

	private uint m_MilestoneCreateCount;

	private bool m_bUnSavedStatChange;

	private float m_LastSaveTime;

	private T17NetView m_NetView;

	private const int kEscapedAllClassicPrisonsTrophyID = 1;

	private const int kEscapedAllTransportPrisonsTrophyID = 6;

	public bool statsLoaded { get; private set; }

	public static StatSystem GetInstance()
	{
		return m_theInstance;
	}

	private void Awake()
	{
		if (m_theInstance == null)
		{
			m_theInstance = this;
			Object.DontDestroyOnLoad(base.transform.gameObject);
			m_StatCreateCount = 0u;
			m_TrophyCreateCount = 0u;
			m_MilestoneCreateCount = 0u;
			m_bUnSavedStatChange = false;
			m_LastSaveTime = Time.realtimeSinceStartup;
			m_NetView = base.gameObject.GetComponent<T17NetView>();
			statsLoaded = false;
		}
	}

	protected virtual void OnDestroy()
	{
		m_NetView = null;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_bUnSavedStatChange && Time.realtimeSinceStartup - m_LastSaveTime > 300f)
		{
			m_LastSaveTime = Time.realtimeSinceStartup;
			m_bUnSavedStatChange = false;
			GlobalSave.GetInstance().RequestSave();
		}
	}

	public void InitStats(int statCount, int trophyCount, int milestoneCount)
	{
		m_Stats = new Stat[statCount];
		for (uint num = 0u; num < m_Stats.Length; num++)
		{
			m_Stats[num] = new Stat();
		}
		m_Trophies = new Trophy[trophyCount];
		for (uint num = 0u; num < m_Trophies.Length; num++)
		{
			m_Trophies[num] = new Trophy();
		}
		m_Milestones = new Milestone[milestoneCount];
		for (uint num = 0u; num < m_Milestones.Length; num++)
		{
			m_Milestones[num] = new Milestone();
		}
		m_StatCreateCount = 0u;
		m_TrophyCreateCount = 0u;
		m_MilestoneCreateCount = 0u;
	}

	public void CreateStat(string statName, int ID, int statType)
	{
		m_Stats[m_StatCreateCount].m_Name = statName;
		m_Stats[m_StatCreateCount].m_ID = ID;
		m_Stats[m_StatCreateCount].m_Type = (STAT_TYPE)statType;
		if (m_Stats[m_StatCreateCount].m_Type == STAT_TYPE.ST_ID_HOLDER)
		{
			m_Stats[m_StatCreateCount].m_IDHolder = new Hashtable();
		}
		m_StatCreateCount++;
	}

	public void CreateTrophy(string trophyName, string apiName, int trophyID, int numberRules, int combiner)
	{
		m_Trophies[m_TrophyCreateCount].m_Name = trophyName;
		m_Trophies[m_TrophyCreateCount].m_APIName = apiName;
		m_Trophies[m_TrophyCreateCount].m_TrophyID = trophyID;
		m_Trophies[m_TrophyCreateCount].m_Rules = new StatRule[numberRules];
		m_Trophies[m_TrophyCreateCount].m_RuleCount = 0;
		m_Trophies[m_TrophyCreateCount].m_Combiner = (Combiner)combiner;
		m_TrophyCreateCount++;
	}

	public void SetUpTrophy(int trophyID, int statID, float refValue, int compareFunc)
	{
		if (FindTrophy(trophyID, out var trophy))
		{
			if (FindStat(statID, out var stat))
			{
				trophy.m_Rules[trophy.m_RuleCount] = new StatRule();
				trophy.m_Rules[trophy.m_RuleCount].m_Stat = stat;
				trophy.m_Rules[trophy.m_RuleCount].m_RefValue = refValue;
				trophy.m_Rules[trophy.m_RuleCount].m_Compare = (StatCompare)compareFunc;
				trophy.m_RuleCount++;
			}
			else
			{
				Debug.Log(" *** INVALID STAT ID  " + statID);
			}
		}
		else
		{
			Debug.Log(" *** INVALID TROPHY ID   " + trophyID);
		}
	}

	public void CreateMilestone(int milestoneID, int numberRules, int combiner)
	{
		m_Milestones[m_MilestoneCreateCount].m_MilestoneID = milestoneID;
		m_Milestones[m_MilestoneCreateCount].m_Rules = new StatRule[numberRules];
		m_Milestones[m_MilestoneCreateCount].m_RuleCount = 0;
		m_Milestones[m_MilestoneCreateCount].m_Combiner = (Combiner)combiner;
		m_MilestoneCreateCount++;
	}

	public void SetUpMilestone(int milestoneID, int statID, float refValue, int compareFunc)
	{
		if (FindMilestone(milestoneID, out var milestone))
		{
			if (FindStat(statID, out var stat))
			{
				milestone.m_Rules[milestone.m_RuleCount] = new StatRule();
				milestone.m_Rules[milestone.m_RuleCount].m_Stat = stat;
				milestone.m_Rules[milestone.m_RuleCount].m_RefValue = refValue;
				milestone.m_Rules[milestone.m_RuleCount].m_Compare = (StatCompare)compareFunc;
				milestone.m_RuleCount++;
			}
			else
			{
				Debug.Log(" *** INVALID STAT ID  " + statID);
			}
		}
		else
		{
			Debug.Log(" *** INVALID MILESTONE ID   " + milestoneID);
		}
	}

	public void InitDone()
	{
		int num = 0;
		for (uint num2 = 0u; num2 < m_Trophies.Length; num2++)
		{
			num += m_Trophies[num2].m_Rules.Length;
		}
		for (uint num2 = 0u; num2 < m_Milestones.Length; num2++)
		{
			num += m_Milestones[num2].m_Rules.Length;
		}
		m_StatsRules = new StatRule[num];
		for (uint num3 = 0u; num3 < m_Stats.Length; num3++)
		{
			int num4 = 0;
			for (uint num2 = 0u; num2 < m_Trophies.Length; num2++)
			{
				for (uint num5 = 0u; num5 < m_Trophies[num2].m_Rules.Length; num5++)
				{
					if (m_Trophies[num2].m_Rules[num5].m_Stat == m_Stats[num3])
					{
						num4++;
					}
				}
			}
			for (uint num2 = 0u; num2 < m_Milestones.Length; num2++)
			{
				for (uint num5 = 0u; num5 < m_Milestones[num2].m_Rules.Length; num5++)
				{
					if (m_Milestones[num2].m_Rules[num5].m_Stat == m_Stats[num3])
					{
						num4++;
					}
				}
			}
			m_Stats[num3].m_RefStats = new StatRule[num4];
		}
		num = 0;
		for (uint num2 = 0u; num2 < m_Trophies.Length; num2++)
		{
			for (uint num5 = 0u; num5 < m_Trophies[num2].m_Rules.Length; num5++)
			{
				m_StatsRules[num] = m_Trophies[num2].m_Rules[num5];
				m_StatsRules[num].m_RefTrophy = m_Trophies[num2];
				num++;
			}
		}
		for (uint num2 = 0u; num2 < m_Milestones.Length; num2++)
		{
			for (uint num5 = 0u; num5 < m_Milestones[num2].m_Rules.Length; num5++)
			{
				m_StatsRules[num] = m_Milestones[num2].m_Rules[num5];
				m_StatsRules[num].m_RefMilestone = m_Milestones[num2];
				num++;
			}
		}
		for (uint num3 = 0u; num3 < m_Stats.Length; num3++)
		{
			int num4 = 0;
			for (uint num2 = 0u; num2 < m_StatsRules.Length; num2++)
			{
				if (m_StatsRules[num2].m_Stat == m_Stats[num3])
				{
					m_Stats[num3].m_RefStats[num4] = m_StatsRules[num2];
					num4++;
				}
			}
		}
	}

	public void LoadStats()
	{
		for (uint num = 0u; num < m_Stats.Length; num++)
		{
			Stat stat = m_Stats[num];
			if (stat.m_Type == STAT_TYPE.ST_COUNTER)
			{
				float value = 0f;
				if (GlobalSave.GetInstance().Get("S_" + stat.m_Name + "_V_" + stat.m_ID, out value, 0f))
				{
					stat.m_Value = value;
				}
			}
			else
			{
				if (!GlobalSave.GetInstance().Get("S_" + stat.m_Name + "_HS_" + stat.m_ID, out var value2, 0))
				{
					continue;
				}
				stat.m_IDHolder = new Hashtable();
				for (int i = 0; i < value2; i++)
				{
					if (GlobalSave.GetInstance().Get("S_" + stat.m_Name + "_KEY_" + stat.m_ID + "_" + i.ToString(), out var value3, 0) && GlobalSave.GetInstance().Get("S_" + stat.m_Name + "_V_" + stat.m_ID + "_" + i.ToString(), out var value4, 0))
					{
						stat.m_IDHolder[value3] = value4;
					}
				}
			}
		}
		statsLoaded = true;
		CheckForPrisonMilestonesChanged();
	}

	public void ResetSaveTime()
	{
		m_bUnSavedStatChange = false;
		m_LastSaveTime = Time.realtimeSinceStartup;
	}

	public void ResetStat(int ID)
	{
		Stat stat = null;
		if (FindStat(ID, out stat))
		{
			stat.m_Value = 0f;
			Debug.Log("****  " + stat.m_Name + "  " + stat.m_Value);
			GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_V_" + stat.m_ID, stat.m_Value);
			GlobalSave.GetInstance().RequestSave();
			m_bUnSavedStatChange = false;
			m_LastSaveTime = Time.realtimeSinceStartup;
		}
	}

	public void SetStat(int ID, float value, Gamer gamer)
	{
		if (m_NetView == null || T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Undefined)
		{
			InternalSetStat(ID, value, gamer.m_NetViewID);
		}
		else if (!gamer.IsLocal())
		{
			m_NetView.RPC("RPC_SetStat", NetTargets.All, ID, value, gamer.m_NetViewID);
		}
		else
		{
			InternalSetStat(ID, value, gamer.m_NetViewID);
		}
	}

	[PunRPC]
	public void RPC_SetStat(int ID, float value, int netView, PhotonMessageInfo info)
	{
		InternalSetStat(ID, value, netView);
	}

	public void InternalSetStat(int ID, float value, int netView)
	{
		Stat stat = null;
		Gamer gamerByViewID = Gamer.GetGamerByViewID(netView);
		if (gamerByViewID.IsLocal() && FindStat(ID, out stat))
		{
			stat.m_Value = value;
			CheckStatRulesLinkedToStat(stat, stat.m_Value);
			Debug.Log("****  " + stat.m_Name + "  " + stat.m_Value);
			GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_V_" + stat.m_ID, stat.m_Value);
			m_bUnSavedStatChange = true;
		}
	}

	public void IncStat(int ID, float inc, Gamer gamer, string progressIDString = "")
	{
		if (m_NetView == null || T17NetRoomManager.CurrentGameRoomType == T17NetRoomGameView.GameRoomType.Undefined)
		{
			InternalIncStat(ID, inc, gamer.m_NetViewID);
			return;
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo == null || currentLevelInfo.m_PrisonEnum != LevelScript.PRISON_ENUM.CustomPrison)
		{
			if (!gamer.IsLocal())
			{
				m_NetView.RPC("RPC_IncStat", NetTargets.All, ID, inc, gamer.m_NetViewID);
			}
			else
			{
				InternalIncStat(ID, inc, gamer.m_NetViewID);
			}
		}
	}

	[PunRPC]
	public void RPC_IncStat(int ID, float value, int netView, PhotonMessageInfo info)
	{
		InternalIncStat(ID, value, netView);
	}

	private void InternalIncStat(int ID, float value, int netView)
	{
		Stat stat = null;
		Gamer gamerByViewID = Gamer.GetGamerByViewID(netView);
		if (gamerByViewID != null && gamerByViewID.IsLocal() && FindStat(ID, out stat))
		{
			stat.m_Value += value;
			CheckStatRulesLinkedToStat(stat, stat.m_Value);
			GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_V_" + stat.m_ID, stat.m_Value);
			m_bUnSavedStatChange = true;
		}
	}

	public int AddIDStat(int ID, int itemID, Gamer gamer)
	{
		Stat stat = null;
		if (!gamer.IsLocal())
		{
			return 0;
		}
		if (FindStat(ID, out stat) && stat.m_Type == STAT_TYPE.ST_ID_HOLDER)
		{
			if (!stat.m_IDHolder.ContainsKey(itemID))
			{
				stat.m_IDHolder[itemID] = 1;
			}
			else
			{
				int num = (int)stat.m_IDHolder[itemID];
				num++;
				stat.m_IDHolder[itemID] = num;
			}
			CheckStatRulesLinkedToStat(stat, itemID);
			GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_HS_" + stat.m_ID, stat.m_IDHolder.Keys.Count);
			int num2 = 0;
			foreach (DictionaryEntry item in stat.m_IDHolder)
			{
				GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_KEY_" + stat.m_ID + "_" + num2.ToString(), (int)item.Key);
				GlobalSave.GetInstance().Set("S_" + stat.m_Name + "_V_" + stat.m_ID + "_" + num2.ToString(), (int)item.Value);
				num2++;
			}
			GlobalSave.GetInstance().RequestSave();
			m_bUnSavedStatChange = false;
			m_LastSaveTime = Time.realtimeSinceStartup;
			return stat.m_IDHolder.Keys.Count;
		}
		return -1;
	}

	private bool FindStat(int ID, out Stat stat)
	{
		for (uint num = 0u; num < m_Stats.Length; num++)
		{
			if (m_Stats[num].m_ID == ID)
			{
				stat = m_Stats[num];
				return true;
			}
		}
		stat = null;
		return false;
	}

	private bool FindTrophy(int ID, out Trophy trophy)
	{
		for (uint num = 0u; num < m_Trophies.Length; num++)
		{
			if (m_Trophies[num].m_TrophyID == ID)
			{
				trophy = m_Trophies[num];
				return true;
			}
		}
		trophy = null;
		return false;
	}

	private bool FindMilestone(int ID, out Milestone milestone)
	{
		for (uint num = 0u; num < m_Milestones.Length; num++)
		{
			if (m_Milestones[num].m_MilestoneID == ID)
			{
				milestone = m_Milestones[num];
				return true;
			}
		}
		milestone = null;
		return false;
	}

	private void CheckStatRulesLinkedToStat(Stat stat, float newValue)
	{
		bool flag = false;
		bool flag2 = false;
		for (uint num = 0u; num < stat.m_RefStats.Length; num++)
		{
			flag2 = true;
			flag |= stat.m_RefStats[num].Check(ref flag2, newValue);
			if (flag && flag2)
			{
				CheckTrophyLinkedToStatRule(stat.m_RefStats[num]);
				CheckMilestoneLinkedToStatRule(stat.m_RefStats[num]);
			}
		}
	}

	private void CheckTrophyLinkedToStatRule(StatRule statRule)
	{
		if (statRule.m_RefTrophy != null && statRule.m_RefTrophy.Check())
		{
			Platform.GetInstance().UnlockAchievement(statRule.m_RefTrophy.m_APIName);
		}
	}

	private void CheckMilestoneLinkedToStatRule(StatRule statRule)
	{
		if (statRule.m_RefMilestone != null && statRule.m_RefMilestone.Check())
		{
			ProgressManager instance = ProgressManager.GetInstance();
			if (instance != null)
			{
				instance.SetMilestoneAchieved(statRule.m_RefMilestone.m_MilestoneID, achieved: true);
			}
		}
	}

	private void CheckForPrisonMilestonesChanged()
	{
		Stat stat = null;
		Gamer primaryGamer = Gamer.GetPrimaryGamer();
		if (primaryGamer != null && primaryGamer.IsLocal() && FindStat(44, out stat))
		{
			for (int i = 0; i < stat.m_RefStats.Length; i++)
			{
				CheckMilestoneLinkedToStatRule(stat.m_RefStats[i]);
			}
		}
	}

	public float GetProgressDataForMilestone(int ID, ref bool[] ruleStatuses, ref float[] statValues, ref float[] refValues)
	{
		float result = 0f;
		Milestone milestone = null;
		if (FindMilestone(ID, out milestone))
		{
			result = milestone.GetProgress(ref ruleStatuses, ref statValues);
			milestone.GetRefValues(ref refValues);
		}
		return result;
	}

	public void GetProgressCountsForMilestone(int ID, ref bool[] ruleStatuses, ref float[] statValues, ref float[] refValues, out int totalNumberRules, out int totalNumberCompleted)
	{
		Milestone milestone = null;
		if (FindMilestone(ID, out milestone))
		{
			milestone.GetNumberOfCompletedRules(ref ruleStatuses, ref statValues, out totalNumberCompleted, out totalNumberRules);
			milestone.GetRefValues(ref refValues);
		}
		else
		{
			totalNumberCompleted = 0;
			totalNumberRules = 0;
		}
	}

	public float GetStatValue(int ID)
	{
		Stat stat = null;
		float result = 0f;
		if (FindStat(ID, out stat) && stat.m_Type == STAT_TYPE.ST_COUNTER)
		{
			result = stat.m_Value;
		}
		return result;
	}

	public int GetIDStatTotal(int ID)
	{
		Stat stat = null;
		int result = 0;
		if (FindStat(ID, out stat) && stat.m_Type == STAT_TYPE.ST_ID_HOLDER)
		{
			result = stat.m_IDHolder.Keys.Count;
		}
		return result;
	}

	public int GetIDStatIDFromIndex(int ID, int index)
	{
		Stat stat = null;
		int num = 0;
		int result = -1;
		if (FindStat(ID, out stat) && stat.m_Type == STAT_TYPE.ST_ID_HOLDER)
		{
			foreach (DictionaryEntry item in stat.m_IDHolder)
			{
				if (num == index)
				{
					result = (int)item.Key;
					break;
				}
				num++;
			}
		}
		return result;
	}
}
