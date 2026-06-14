using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrisonData", menuName = "Team17/PrisonData and Playlists/Create Prison Data")]
public class PrisonData : ScriptableObject
{
	public enum PrisonDifficulty
	{
		Invalid = -1,
		Easy,
		Medium,
		Hard,
		Count
	}

	[Serializable]
	public class LevelInfo
	{
		public LevelScript.PRISON_ENUM m_PrisonEnum;

		public LevelScript.PRISON_TYPE m_PrisonType;

		[Tooltip("For our prisons, these will correspond to a scene name. For custom prisons, they will correspond to the appropiate save file")]
		public string m_AssociatedFile = string.Empty;

		public bool InfoMatches(LevelInfo other)
		{
			return m_PrisonEnum == other.m_PrisonEnum && m_PrisonType == other.m_PrisonType && m_AssociatedFile.Equals(other.m_AssociatedFile);
		}
	}

	[Localization]
	[Header("Front End")]
	public string m_NameLocalizationKey = string.Empty;

	[Localization]
	public string m_DescriptionLocalizationKey = string.Empty;

	public string m_ImagePath;

	public string m_PrisonSetupImagePath;

	public string m_ImageLockedPath;

	public string m_RoundResultsImagePath;

	public bool m_bIsDLC;

	public DLCFrontendData m_DLCData;

	public bool m_bIsDebug;

	public PrisonDifficulty m_PrisonDifficulty;

	public bool m_bAddRobinsonCharacter;

	[Header("Customisation")]
	public CustomisationConfig m_DefaultPlayerCustomisations;

	public CustomisationConstraint m_DefaultPlayerConstraint;

	[ReadOnly]
	public int[] m_CustomisableRoles = new int[0];

	[ReadOnly]
	public ItemData[] m_RoleStartingOutfitData = new ItemData[0];

	public CustomisationConfig[] m_CustomisationPools = new CustomisationConfig[0];

	public CustomisationConstraint[] m_CustomisationConstraints = new CustomisationConstraint[0];

	[Header("Influencers")]
	public InfluencerWeights[] m_InfluencerWeights = new InfluencerWeights[0];

	[Header("Configurations")]
	public List<PrisonConfig> m_Configs = new List<PrisonConfig>();

	[Header("Prison Info")]
	public LevelInfo m_LevelInfo;

	[Header("Platform Info")]
	[Localization]
	public string m_EscapedActivityFeedKey = string.Empty;

	[Header("Unlock")]
	public ProgressMilestone m_UnlockMilestone;

	public CustomisationSet m_UnlockRewards = new CustomisationSet();

	public PrisonData()
	{
		m_LevelInfo = new LevelInfo();
	}
}
