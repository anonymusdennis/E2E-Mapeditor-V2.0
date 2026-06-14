using System.Collections.Generic;
using UnityEngine;

public class HazardousCarryableObjectInteraction : CarryObjectInteraction
{
	[Header("HazardousCarryableObjectInteraction")]
	public List<ItemData> m_RequiredItems;

	public SpeechPODO m_NoEquipmentSpeech;

	public override bool AllowedToInteract(Character localCharacter)
	{
		return base.AllowedToInteract(localCharacter) && localCharacter.HasItemsOnPerson(m_RequiredItems);
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			if (!localCharacter.HasItemsOnPerson(m_RequiredItems))
			{
				SpeechManager.GetInstance().SaySomething(localCharacter, m_NoEquipmentSpeech);
				return true;
			}
			return false;
		}
		return true;
	}
}
