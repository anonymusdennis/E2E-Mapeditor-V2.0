public class HorseshoeAnvilInteraction : MasherItemProcessorInteraction
{
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	public string m_StartLoopAudioEvent = "Play_DLC_05_Blacksmith_Sizzle";

	public string m_StopLoopAudioEvent = "Stop_DLC_05_Blacksmith_Sizzle";

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		ReadingMasher horseshoeAnvilMasher = trackedUIElements.GetHorseshoeAnvilMasher();
		horseshoeAnvilMasher.SetMasherSettings(m_MasherSettings);
		return horseshoeAnvilMasher;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (localCharacter.IsPlayer() && !string.IsNullOrEmpty(m_StartLoopAudioEvent) && !string.IsNullOrEmpty(m_StopLoopAudioEvent))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StartLoopAudioEvent, localCharacter.gameObject);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (localCharacter.IsPlayer() && !string.IsNullOrEmpty(m_StopLoopAudioEvent))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_StopLoopAudioEvent, localCharacter.gameObject);
		}
	}
}
