using UnityEngine;
using UnityEngine.Serialization;

public class AICharacter_SpecificQuestGiver : AICharacter_Generic
{
	[Localization]
	[Header("Character Information strings")]
	public string m_JobString = "Text.Game.Roles.NoJob";

	[FormerlySerializedAs("m_TimeServedString")]
	[Localization]
	public string m_TimeServedLabelString = "Text.Stats.DaysWorked";

	[Localization]
	public string m_TimeServedValueString;

	protected override void OnStart()
	{
		base.OnStart();
		if ((bool)m_Character)
		{
			QuestManager.GetInstance().RegisterSpecificQuestableCharacter(m_Character);
		}
	}

	public string GetLocalisedDaysServedString()
	{
		int num = (int)m_Character.m_CharacterStats.m_SentenceBaseLine - m_Character.m_CharacterStats.RemainingSentence;
		int value = num + 1;
		if (string.IsNullOrEmpty(m_TimeServedValueString))
		{
			return value.ToString();
		}
		Localization.GetWithKeySwap(m_TimeServedValueString, out var localised, "$days", value);
		return localised;
	}
}
