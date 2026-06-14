using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Milestone", menuName = "Team17/Progress/Create Milestone")]
public class ProgressMilestone : ScriptableObject
{
	[Serializable]
	public class Criteria
	{
		[Localization]
		public string descriptionKey = "Text.MilestoneCriteria.Description";

		public StatsTracking.StatRule statRule = new StatsTracking.StatRule();
	}

	public int id;

	[Localization]
	public string nameKey = "Text.Milestone.Name";

	[Localization]
	public string description = "Text.Milestone.Description";

	public Sprite image;

	public Sprite imageLocked;

	public bool delayedUnlock;

	public StatsTracking.Combiner evaluationType;

	public Criteria[] criteria;

	public LevelScript.PRISON_ENUM m_prison;

	[Header("Rewards")]
	public string rewardName = string.Empty;

	public CustomisationSet rewards = new CustomisationSet();
}
