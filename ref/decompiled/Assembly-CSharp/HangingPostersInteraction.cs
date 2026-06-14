public class HangingPostersInteraction : PaintingInteraction
{
	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		m_Masher = trackedUIElements.GetHangingPostersMasher();
		m_Masher.SetMasherSettings(m_MasherSettings);
		return m_Masher;
	}
}
