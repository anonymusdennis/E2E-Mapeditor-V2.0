public class FacePaintingInteraction : ServiceItemMinigameInteractiveObject
{
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	public string m_RepCompletedSound = "Play_PaintFace";

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		ReadingMasher facePaintingMasher = trackedUIElements.GetFacePaintingMasher();
		facePaintingMasher.SetMasherSettings(m_MasherSettings);
		facePaintingMasher.m_bPlayRepComplimentarySoundEffect = false;
		return facePaintingMasher;
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_ButtonMasher != null && m_ButtonMasher.HasCompletedRep())
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_RepCompletedSound, base.gameObject);
		}
	}
}
