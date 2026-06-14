using AUTOGEN_T17Wwise_Enums;

public class ElectricianInteraction : HandymanInteraction
{
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	private bool m_bPreviousIsGainingState;

	private AlternateButtonMasher.MasherState m_PreviousMasherState;

	private ReadingMasher m_ButtonMasher;

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		m_ButtonMasher = trackedUIElements.GetElectricianMasher();
		m_ButtonMasher.SetMasherSettings(m_MasherSettings);
		m_ButtonMasher.m_bPlayRepComplimentarySoundEffect = false;
		return m_ButtonMasher;
	}

	protected override void OnDestroy()
	{
		m_ButtonMasher = null;
		base.OnDestroy();
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		m_ButtonMasher = null;
		base.OnStartInteraction(localCharacter);
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Electrician_Power, base.gameObject);
		}
		m_bPreviousIsGainingState = false;
		m_PreviousMasherState = AlternateButtonMasher.MasherState.Invalid;
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		StopAudio();
	}

	private void StopAudio()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Electrician_Solder_Loop, base.gameObject);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Electrician_Loop, base.gameObject);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Electrician_Power, base.gameObject);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_ButtonMasher != null)
		{
			ProcessMasherGainSounds();
			ProcessMasherSweetspotSounds();
		}
	}

	private void ProcessMasherSweetspotSounds()
	{
		AlternateButtonMasher.MasherState masherState = m_ButtonMasher.GetMasherState();
		if (masherState != m_PreviousMasherState)
		{
			if (masherState == AlternateButtonMasher.MasherState.Valid)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Electrician_Solder_Loop, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Electrician_Solder_Loop, base.gameObject);
			}
			m_PreviousMasherState = masherState;
		}
	}

	private void ProcessMasherGainSounds()
	{
		bool flag = m_ButtonMasher.IsGainIncreasing();
		if (flag != m_bPreviousIsGainingState)
		{
			if (flag)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Electrician_Loop, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Electrician_Loop, base.gameObject);
			}
			m_bPreviousIsGainingState = flag;
		}
	}

	protected override void OnFixed()
	{
		base.OnFixed();
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Electrician_Spark, m_interactingCharacter.gameObject);
		StopAudio();
	}
}
