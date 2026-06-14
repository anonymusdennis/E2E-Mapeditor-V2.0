using AUTOGEN_T17Wwise_Enums;

public class KennelInteraction : AnimatedInteraction
{
	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		if (null != m_interactingCharacter && m_interactingCharacter.m_CharacterRole != CharacterRole.Dog)
		{
			m_interactingCharacter.m_bIsHidden = true;
		}
	}

	public override void InteractionReadyEnd(bool interuption)
	{
		base.InteractionReadyEnd(interuption);
		if (null != m_interactingCharacter && m_interactingCharacter.m_CharacterRole != CharacterRole.Dog)
		{
			m_interactingCharacter.m_bIsHidden = false;
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, base.gameObject);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, base.gameObject);
		}
	}
}
