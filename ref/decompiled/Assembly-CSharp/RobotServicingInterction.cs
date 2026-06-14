public class RobotServicingInterction : ServiceItemMinigameInteractiveObject
{
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	public string m_RepCompletedSound = "Play_DLC_06_OilCan_Job";

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		ReadingMasher robotServicingMasher = trackedUIElements.GetRobotServicingMasher();
		robotServicingMasher.SetMasherSettings(m_MasherSettings);
		robotServicingMasher.m_bPlayRepComplimentarySoundEffect = false;
		return robotServicingMasher;
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
