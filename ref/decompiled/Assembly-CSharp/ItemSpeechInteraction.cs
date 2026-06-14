using System.Collections.Generic;

public class ItemSpeechInteraction : AnimatedInteraction
{
	public SpeechPODO m_NoItemSpeech;

	public SpeechPODO m_ItemSpeech;

	public List<ItemData> m_RequiredItems;

	private bool m_bRequestStopInteraction;

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (localCharacter.HasItemsOnPerson(m_RequiredItems))
		{
			SpeechManager.GetInstance().SaySomething(localCharacter, m_ItemSpeech);
		}
		else
		{
			SpeechManager.GetInstance().SaySomething(localCharacter, m_NoItemSpeech);
		}
		m_bRequestStopInteraction = true;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_bRequestStopInteraction)
		{
			m_bRequestStopInteraction = false;
			if (m_interactingCharacter != null)
			{
				m_interactingCharacter.RequestStopInteraction();
			}
		}
	}
}
