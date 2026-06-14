using AUTOGEN_T17Wwise_Enums;

public class PlumberInteraction : HandymanInteraction
{
	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_MasherSettings = default(GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings);

	private ToiletInteraction m_ToiletInteraction;

	private bool m_bIsToiletInteractionSet;

	protected override void Init()
	{
		base.Init();
		m_bCanDoWithoutPlayerJob = true;
	}

	protected override void Awake()
	{
		base.Awake();
		CheckSelfForToiletInteraction();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_ToiletInteraction = null;
		m_bIsToiletInteractionSet = false;
	}

	public void CheckSelfForToiletInteraction()
	{
		m_ToiletInteraction = GetComponent<ToiletInteraction>();
		m_bIsToiletInteractionSet = m_ToiletInteraction != null;
	}

	public void LoadDefaultConfigIfInvalid()
	{
		if (!m_MasherSettings.IsValid())
		{
			GeneralMinigameConfig generalMinigameConfigs = ConfigManager.GetInstance().GeneralMinigameConfigs;
			if (generalMinigameConfigs != null)
			{
				m_MasherSettings = generalMinigameConfigs.m_ToiletUncloggingSettings;
			}
			if (!m_MasherSettings.IsValid())
			{
				m_MasherSettings.m_DecayPerSecond = 0.23f;
				m_MasherSettings.m_GainPerAlternate = 0.1f;
			}
		}
	}

	protected override IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements)
	{
		GymMasher_Threadmill_ExerciseBike plumberMasher = trackedUIElements.GetPlumberMasher();
		LoadDefaultConfigIfInvalid();
		plumberMasher.SetMasherSettings(ref m_MasherSettings);
		return plumberMasher;
	}

	public override bool InteractionVisibility()
	{
		if (m_bIsToiletInteractionSet)
		{
			return m_ToiletInteraction.GetFloodingStatus() == ToiletInteraction.FloodingStatus.Flooded;
		}
		return false;
	}

	protected override bool ShouldUpdateInteraction()
	{
		if (base.ShouldUpdateInteraction())
		{
			return true;
		}
		return m_bIsToiletInteractionSet && m_ToiletInteraction.GetFloodingStatus() == ToiletInteraction.FloodingStatus.Flooded;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Plumber_Plunge, base.gameObject);
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Plumber_Plunge, base.gameObject);
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			return true;
		}
		bool flag = localCharacter.m_CharacterRole == CharacterRole.Maintenance;
		return m_bIsToiletInteractionSet && m_ToiletInteraction.GetFloodingStatus() == ToiletInteraction.FloodingStatus.Flooded && (flag || DoesCharacterSatasifysItemRequirement(localCharacter));
	}

	public override bool IsPossibleToInteractWith()
	{
		if (!base.IsPossibleToInteractWith())
		{
			return m_bIsToiletInteractionSet && m_ToiletInteraction.GetFloodingStatus() == ToiletInteraction.FloodingStatus.Flooded;
		}
		return true;
	}

	protected override void OnFixed()
	{
		base.OnFixed();
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Jobs_Plumber_Unblocked, base.gameObject);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Jobs_Plumber_Plunge, base.gameObject);
		if (m_bIsToiletInteractionSet)
		{
			m_ToiletInteraction.UnClogToilet();
		}
	}

	protected override void UpdateInteractionZ_PreTransitionStart()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_Interacting()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_PostTransitionEnd()
	{
		SetInteractionZ();
	}

	private void SetInteractionZ()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_InteractingZOffset);
		}
	}
}
