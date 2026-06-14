using System;
using System.Collections.Generic;

[Serializable]
public class InteractionItemRequiremetHelper
{
	public ItemData m_EquippedItemRequirement;

	public SpeechPODO m_DontHaveItemsSpeech;

	private string m_ItemRequiredTag = string.Empty;

	public bool DoesCharacterSatasifysItemRequirement(Character localCharacter)
	{
		bool result = false;
		m_ItemRequiredTag = string.Empty;
		if (m_EquippedItemRequirement != null)
		{
			Item equippedItem = localCharacter.GetEquippedItem();
			if (equippedItem != null && equippedItem.ItemDataID == m_EquippedItemRequirement.m_ItemDataID)
			{
				result = true;
			}
			else
			{
				m_ItemRequiredTag = m_EquippedItemRequirement.m_ItemLocalizationTag;
			}
		}
		else
		{
			result = true;
		}
		return result;
	}

	public void DoNoEquippedItemDialog(Character localCharacter)
	{
		m_DontHaveItemsSpeech.m_TextId = "Text.Emote.ItemNotEquipped";
		List<SpeechManager.Token> list = new List<SpeechManager.Token>();
		list.Add(new SpeechManager.Token("$ItemToGet", m_ItemRequiredTag, bIsCharacterNetviewID: false));
		List<SpeechManager.Token> tokens = list;
		SpeechManager.GetInstance().SaySomething(localCharacter, m_DontHaveItemsSpeech.m_TextId, tokens, m_DontHaveItemsSpeech.m_SpeechTone, m_DontHaveItemsSpeech.m_Duration, m_DontHaveItemsSpeech.m_Priority);
	}
}
