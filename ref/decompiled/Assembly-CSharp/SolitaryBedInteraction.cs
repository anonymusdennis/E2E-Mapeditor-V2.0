public class SolitaryBedInteraction : BedInteraction
{
	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_interactingCharacter.RegainConsciousness();
		m_interactingCharacter.SetCharacterSleeping(isSleeping: true);
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.SetCharacterSleeping(isSleeping: false);
	}
}
