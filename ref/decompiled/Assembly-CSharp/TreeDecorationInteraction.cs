using AUTOGEN_T17Wwise_Enums;

public class TreeDecorationInteraction : HandymanInteraction
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	private SolitaryPotatoMasher m_Masher;

	private SolitaryPotatoMasher.SliderState m_PreviousSliderState;

	public string m_MinigameCentreSoundEffect;

	protected override void Init()
	{
		base.Init();
		ConfigManager instance = ConfigManager.GetInstance();
		if (instance != null && instance.gameType == PrisonConfig.ConfigType.Versus && m_NetObjectLock != null)
		{
			m_NetObjectLock.m_bIsVisibleToProximityDetector = false;
		}
	}

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		m_Masher = trackedUIElements.GetTreeDecorationMasherMasher();
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
		if (!(m_Masher != null) || string.IsNullOrEmpty(m_MinigameCentreSoundEffect))
		{
			return;
		}
		SolitaryPotatoMasher.SliderState sliderState = m_Masher.GetSliderState();
		if (sliderState != m_PreviousSliderState)
		{
			if (sliderState != 0)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_MinigameCentreSoundEffect, base.gameObject);
			}
			sliderState = m_PreviousSliderState;
		}
	}

	protected override void OnFixed()
	{
		base.OnFixed();
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
		}
	}
}
