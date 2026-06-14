public class PumpkinCarvingInteraction : MasherItemProcessorInteraction
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	public string m_RepCompletedAudioEvent;

	private SolitaryPotatoMasher m_Masher;

	private SolitaryPotatoMasher.SliderState m_PreviousSliderState;

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		m_Masher = trackedUIElements.GetPumpkinCarvingMasher();
		if (m_MinigameSettingsContainer != null)
		{
			m_MasherSettings = m_MinigameSettingsContainer.m_SolitaryMasherSettings;
		}
		m_Masher.SetMasherSettings(m_MasherSettings);
		return m_Masher;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		m_Masher = null;
		m_PreviousSliderState = SolitaryPotatoMasher.SliderState.Center;
		base.OnStartInteraction(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (!(m_Masher != null))
		{
			return;
		}
		SolitaryPotatoMasher.SliderState sliderState = m_Masher.GetSliderState();
		if (sliderState != m_PreviousSliderState)
		{
			if (sliderState != 0 && !string.IsNullOrEmpty(m_RepCompletedAudioEvent))
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_RepCompletedAudioEvent, base.gameObject);
			}
			sliderState = m_PreviousSliderState;
		}
	}
}
