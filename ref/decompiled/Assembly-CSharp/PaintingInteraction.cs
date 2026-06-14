using AUTOGEN_T17Wwise_Enums;

public class PaintingInteraction : HandymanInteraction
{
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	public string m_CentreSoundEvent = "Play_Jobs_Paint_Wall";

	protected SolitaryPotatoMasher m_Masher;

	private SolitaryPotatoMasher.SliderState m_PreviousSliderState;

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
		m_Masher = trackedUIElements.GetPaintingMasher();
		m_Masher.SetMasherSettings(m_MasherSettings);
		return m_Masher;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		m_Masher = null;
		m_PreviousSliderState = SolitaryPotatoMasher.SliderState.Center;
		base.OnStartInteraction(localCharacter);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
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
			if (sliderState != 0)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_CentreSoundEvent, base.gameObject);
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
