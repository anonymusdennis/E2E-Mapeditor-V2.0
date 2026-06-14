public class GuardOutfitCrateInteraction : AnimatedInteraction
{
	public override bool AllowedToInteract(Character localCharacter)
	{
		if (localCharacter.m_CharacterRole != CharacterRole.Guard)
		{
			return false;
		}
		return true;
	}

	public override bool AllowOtherPlayerHUDInteractions()
	{
		return false;
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
