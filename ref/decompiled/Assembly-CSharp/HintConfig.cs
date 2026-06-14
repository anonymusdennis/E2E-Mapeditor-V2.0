using System;
using UnityEngine;

[CreateAssetMenu(fileName = "HintsConfig", menuName = "Team17/Config/Create Prison Hints")]
public class HintConfig : ScriptableObject
{
	[Serializable]
	public class HintData
	{
		public string m_VagueHint;

		public string m_FullHint;

		public int m_HintCost;
	}

	[Serializable]
	public class CraftHintData
	{
		public ItemData m_ItemToCraft;

		public int m_HintCost;
	}

	[Header("Hints for prison (max number of hints is 64)")]
	public HintData[] m_Hints = new HintData[32];

	public CraftHintData[] m_CraftHints = new CraftHintData[32];
}
