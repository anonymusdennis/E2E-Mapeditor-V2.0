using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrisonConfig", menuName = "Team17/Config/Create Prison Config")]
public class PrisonConfig : ScriptableObject
{
	public enum ConfigType
	{
		Cooperative,
		Versus,
		Singleplayer
	}

	[Header("Config. Settings")]
	public ConfigType m_ConfigType;

	[Header("Required Settings")]
	public GlobalCombatConfig m_CombatConfig;

	public AIConfig m_AIConfig;

	public JobConfig m_JobConfig;

	[Header("Character Settings")]
	public CharacterConfig m_PlayerConfig;

	public CharacterConfig m_InmateConfig;

	public CharacterConfig m_GuardConfig;

	public CharacterConfig m_RiotGuardConfig;

	public CharacterConfig m_DogConfig;

	[Header("Desk Settings")]
	public ItemContainerConfig m_PlayerDeskConfig;

	public ItemContainerConfig m_InmateDeskConfig;

	public ItemContainerConfig m_GuardDeskConfig;

	[Header("Prison Settings")]
	public RoutineConfig m_RoutineConfig;

	public OpinionConfig m_OpinionConfig;

	public VendorConfig m_VendorConfig;

	public QuestConfig m_QuestConfig;

	public MinigameConfig m_MinigameConfig;

	public GeneralMinigameConfig m_GeneralPrisonMinigameConfig;

	public ScoreSystemConfig m_ScoreConfig;

	[Header("Versus Total Duration")]
	public int m_VersusDays = 1;

	public int m_VersusHours;

	public int m_VersusMinutes;

	[Header("Overrides")]
	public List<ItemDataConfig> m_ItemDataOverrides = new List<ItemDataConfig>();

	public List<AIEventData> m_AIEventOverrides = new List<AIEventData>();

	public List<QuestManager.QuestType> m_QuestOverrides = new List<QuestManager.QuestType>();
}
