public class GuardComputerInteraction : AnimatedInteraction
{
	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		RequestStopInteraction(m_interactingCharacter);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}
