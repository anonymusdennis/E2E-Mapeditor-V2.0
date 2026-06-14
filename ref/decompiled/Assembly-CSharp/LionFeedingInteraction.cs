public class LionFeedingInteraction : BeckonAndMinigameServeCustomerInteraction
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		SolitaryPotatoMasher lionTamingMasher = trackedUIElements.GetLionTamingMasher();
		lionTamingMasher.SetMasherSettings(m_MasherSettings);
		return lionTamingMasher;
	}
}
