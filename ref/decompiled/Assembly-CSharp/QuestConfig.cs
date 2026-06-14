using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestConfig", menuName = "Team17/Config/Create Quest Config")]
public class QuestConfig : ScriptableObject
{
	[Header("Overrides")]
	public List<QuestManager.QuestType> m_OverrideQuests = new List<QuestManager.QuestType>();

	[Range(1f, 100f)]
	[Tooltip("How much of the inmates can offer quests at the same time")]
	public int m_MaxPercentageQuestGivers = 20;

	[Tooltip("How long the game will wait with  giving inmatess quests, since the level started")]
	public int m_TimeInHoursBeforeInmatesHaveQuests = 1;

	[Header("Special Quests")]
	public bool m_bAllowSpecificQuests = true;
}
