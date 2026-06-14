using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "StatsTracking", menuName = "Team17/Create StatsTracking")]
public class StatsTracking : ScriptableObject
{
	[Serializable]
	public enum STAT_TYPE
	{
		Counter,
		ID_Holder
	}

	[Serializable]
	public class Stat
	{
		public STAT_IDS m_ID;

		public STAT_TYPE m_StatType;
	}

	public enum StatCompare
	{
		EQUAL,
		GREATER_THAN_EQUAL,
		HAS_ID
	}

	[Serializable]
	public class StatRule
	{
		public STAT_IDS m_StatID;

		public float m_RefValue;

		public StatCompare m_Compare;
	}

	[Serializable]
	public enum Combiner
	{
		NA,
		AND,
		OR
	}

	[Serializable]
	public class Trophy
	{
		public string m_Name;

		public string m_APIName;

		public int m_TrophyID;

		public StatRule[] m_Rules;

		public Combiner m_CombineMode;
	}

	[Serializable]
	public class Milestone
	{
		public ProgressMilestone m_Milestone;
	}

	public ItemData m_TeaItemData;

	public ItemData m_EnergySwordItemData;

	public ItemData m_CakeItemData;

	private static StatsTracking m_Instance;

	public Stat[] m_Stats;

	public Trophy[] m_Tropies;

	public Milestone[] m_Milestones;

	public static int TEA_ITEM_ID => (!(m_Instance.m_TeaItemData != null)) ? (-1) : m_Instance.m_TeaItemData.m_ItemDataID;

	public static int ENERGYSWORD_ITEM_ID => (!(m_Instance.m_EnergySwordItemData != null)) ? (-1) : m_Instance.m_EnergySwordItemData.m_ItemDataID;

	public static int CAKE_ITEM_ID => (!(m_Instance.m_CakeItemData != null)) ? (-1) : m_Instance.m_CakeItemData.m_ItemDataID;

	private StatsTracking()
	{
		m_Instance = this;
	}
}
