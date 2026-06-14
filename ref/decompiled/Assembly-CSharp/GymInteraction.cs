using AUTOGEN_T17Wwise_Enums;
using MinigameMashers;
using UnityEngine;

public class GymInteraction : AnimatedInteraction
{
	public enum GymInteractionType
	{
		WeightLifting,
		PullUp,
		Kettlebells,
		Threadmill,
		ExerciseBike,
		PommelHorse,
		Footbag
	}

	public GymInteractionType m_GymEquipmentType;

	[Header("General Settings")]
	public float m_StaminaDecreasePerTriggerUse = 1f;

	[Range(0f, 10f)]
	public int m_StrengthChange;

	[Range(0f, 10f)]
	public int m_EnergyChange;

	[Range(0f, 10f)]
	public int m_CardioChange;

	[Header("WeightLifting")]
	public AlternateButtonMasher.AlternateMasherSettings m_MasherSettings;

	public float m_TimeToKeepInThreshold = 3f;

	[Header("KettleBells")]
	public GymMasher_KettleBelts.HoldingMasherSettings m_KettleHolderMasherSettings;

	[Header("PullUps")]
	public GymMasher_Pullup.PullupMasherSettings m_PullUpsMasherSettings;

	[Header("ExerciseBike")]
	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_ExerciseBikeMasherSettings;

	[Header("Threadmill")]
	public GymMasher_Threadmill_ExerciseBike.ThreadMillMasherSettings m_ThreadmillMasherSettings;

	[Header("PommelHorse")]
	public GymMasher_Pommel_Footbag.PommelMasherSettings m_PommelHorseMasherSettings;

	[Header("Footbag")]
	public GymMasher_Pommel_Footbag.PommelMasherSettings m_FootbagMasherSettings;

	private GymMasherBase m_ButtonMasher;

	private float m_RewardMultiplier = 1f;

	[Header("Rep Setup")]
	public AnimState m_RepFirstStage = AnimState.INVALID;

	private bool m_bIsRepAOneshot;

	public AnimState m_RepSecondStage = AnimState.INVALID;

	private bool m_bIsRepBOneShot;

	public float m_AICharacter_RepMinTime = 1.5f;

	public float m_AICharacter_RepMaxTime = 3f;

	private bool m_bRepToggle;

	private float m_AICharacterTimeBetweenReps;

	private float m_AICharacterTimeWaitingForRep;

	[Tooltip("If set to invalid, the system will try to work out what one to display based off what strength/fitness is gained")]
	public StylePreset m_MasherPreset;

	private float HACK_PommelHorseTimeUntilSwish;

	private GymMasher_WeightLifting m_WeightMasher;

	protected override void Init()
	{
		base.Init();
		if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().minigameConfig != null)
		{
			ApplyConfigData(ConfigManager.GetInstance().minigameConfig);
		}
		if (m_GymEquipmentType == GymInteractionType.WeightLifting)
		{
			m_NormalizedAnimStateHash = Animator.StringToHash("RepA");
		}
	}

	private void ApplyConfigData(MinigameConfig config)
	{
		switch (m_GymEquipmentType)
		{
		case GymInteractionType.WeightLifting:
			m_RewardMultiplier = config.m_Weights_RewardModifier;
			break;
		case GymInteractionType.Kettlebells:
			m_RewardMultiplier = config.m_KettleBells_RewardModifier;
			break;
		case GymInteractionType.PullUp:
			m_RewardMultiplier = config.m_Pullups_RewardModifier;
			break;
		case GymInteractionType.ExerciseBike:
			m_RewardMultiplier = config.m_ExcersiseBike_RewardModifier;
			break;
		case GymInteractionType.Threadmill:
			m_RewardMultiplier = config.m_Threadmill_RewardModifier;
			break;
		case GymInteractionType.PommelHorse:
			m_RewardMultiplier = config.m_PommelHorse_RewardModifier;
			break;
		case GymInteractionType.Footbag:
			m_RewardMultiplier = config.m_Footbag_RewardModifier;
			break;
		}
	}

	public override bool CanStartOrContinueInteraction(Character character)
	{
		return character != null && character.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaDecreasePerTriggerUse);
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Exercising);
		if (!m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			m_AICharacterTimeBetweenReps = 0f;
			m_bRepToggle = false;
			m_AICharacterTimeBetweenReps = Random.Range(m_AICharacter_RepMinTime, m_AICharacter_RepMaxTime);
		}
		else
		{
			Player component = m_interactingCharacter.GetComponent<Player>();
			TutorialManager instance = TutorialManager.GetInstance();
			if (component != null && instance != null)
			{
				instance.StartTutorialRPC(component, TutorialSubject.Gym);
			}
			if (m_GymEquipmentType == GymInteractionType.WeightLifting)
			{
				m_interactingCharacter.m_CharacterAnimator.BeginControllingNormalizedTime();
			}
		}
		m_bIsRepAOneshot = m_interactingCharacter.m_CharacterAnimator.IsAnimStateOneShot(m_RepFirstStage);
		m_bIsRepBOneShot = m_interactingCharacter.m_CharacterAnimator.IsAnimStateOneShot(m_RepSecondStage);
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Exercising);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (localCharacter.m_CharacterStats == null)
		{
		}
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			TutorialManager.GetInstance().StartTutorialRPC((Player)localCharacter, TutorialSubject.Stats);
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(((Player)localCharacter).m_PlayerCameraManagerBindingID);
			((Player)localCharacter).OnMinigameEntered();
			if (playerTrackedUIElements == null)
			{
			}
			switch (m_GymEquipmentType)
			{
			case GymInteractionType.WeightLifting:
			{
				GymMasher_WeightLifting weightLiftingButtonMasher = playerTrackedUIElements.GetWeightLiftingButtonMasher();
				if (weightLiftingButtonMasher != null)
				{
					weightLiftingButtonMasher.SetMasherSettings(ref m_MasherSettings, m_TimeToKeepInThreshold);
					weightLiftingButtonMasher.SetPlayerToCheck((Player)localCharacter);
					weightLiftingButtonMasher.gameObject.SetActive(value: true);
					m_ButtonMasher = weightLiftingButtonMasher;
					m_WeightMasher = weightLiftingButtonMasher;
				}
				break;
			}
			case GymInteractionType.PullUp:
			{
				GymMasher_Pullup pullupMasher = playerTrackedUIElements.GetPullupMasher();
				if (pullupMasher != null)
				{
					pullupMasher.SetMasherSettings(ref m_PullUpsMasherSettings);
					pullupMasher.SetPlayerToCheck((Player)localCharacter);
					pullupMasher.gameObject.SetActive(value: true);
					m_ButtonMasher = pullupMasher;
				}
				break;
			}
			case GymInteractionType.Kettlebells:
			{
				GymMasher_KettleBelts kettleBellsMasher = playerTrackedUIElements.GetKettleBellsMasher();
				if (kettleBellsMasher != null)
				{
					kettleBellsMasher.SetMasherSettings(ref m_KettleHolderMasherSettings);
					kettleBellsMasher.SetPlayerToCheck((Player)localCharacter);
					kettleBellsMasher.gameObject.SetActive(value: true);
					m_ButtonMasher = kettleBellsMasher;
				}
				break;
			}
			case GymInteractionType.Threadmill:
			{
				GymMasher_Threadmill_ExerciseBike threadmillAndExerciseBikeMasher2 = playerTrackedUIElements.GetThreadmillAndExerciseBikeMasher();
				if (threadmillAndExerciseBikeMasher2 != null)
				{
					threadmillAndExerciseBikeMasher2.SetMasherSettings(ref m_ThreadmillMasherSettings);
					threadmillAndExerciseBikeMasher2.SetPlayerToCheck((Player)localCharacter);
					threadmillAndExerciseBikeMasher2.gameObject.SetActive(value: true);
					m_ButtonMasher = threadmillAndExerciseBikeMasher2;
				}
				break;
			}
			case GymInteractionType.ExerciseBike:
			{
				GymMasher_Threadmill_ExerciseBike threadmillAndExerciseBikeMasher = playerTrackedUIElements.GetThreadmillAndExerciseBikeMasher();
				if (threadmillAndExerciseBikeMasher != null)
				{
					threadmillAndExerciseBikeMasher.SetMasherSettings(ref m_ExerciseBikeMasherSettings);
					threadmillAndExerciseBikeMasher.SetPlayerToCheck((Player)localCharacter);
					threadmillAndExerciseBikeMasher.gameObject.SetActive(value: true);
					m_ButtonMasher = threadmillAndExerciseBikeMasher;
				}
				break;
			}
			case GymInteractionType.PommelHorse:
			{
				GymMasher_Pommel_Footbag pommelAndFootbagMasher2 = playerTrackedUIElements.GetPommelAndFootbagMasher();
				if (pommelAndFootbagMasher2 != null)
				{
					pommelAndFootbagMasher2.SetMasherSettings(ref m_PommelHorseMasherSettings);
					pommelAndFootbagMasher2.SetPlayerToCheck((Player)localCharacter);
					pommelAndFootbagMasher2.gameObject.SetActive(value: true);
					m_ButtonMasher = pommelAndFootbagMasher2;
				}
				break;
			}
			case GymInteractionType.Footbag:
			{
				GymMasher_Pommel_Footbag pommelAndFootbagMasher = playerTrackedUIElements.GetPommelAndFootbagMasher();
				if (pommelAndFootbagMasher != null)
				{
					pommelAndFootbagMasher.SetMasherSettings(ref m_FootbagMasherSettings);
					pommelAndFootbagMasher.SetPlayerToCheck((Player)localCharacter);
					pommelAndFootbagMasher.gameObject.SetActive(value: true);
					m_ButtonMasher = pommelAndFootbagMasher;
				}
				break;
			}
			}
			StylePreset stylePreset = m_MasherPreset;
			if (stylePreset == StylePreset.Invalid)
			{
				stylePreset = ((m_StrengthChange > m_EnergyChange && m_StrengthChange > m_CardioChange) ? StylePreset.Stength : StylePreset.Fitness);
			}
			if (stylePreset == StylePreset.Stength)
			{
				m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Strength, 0f, 100f);
			}
			else if (m_EnergyChange > m_CardioChange)
			{
				m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Energy, 0f, 100f);
			}
			else
			{
				m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Cardio, 0f, 100f);
			}
			m_ButtonMasher.SetStyle(stylePreset);
		}
		if (m_GymEquipmentType == GymInteractionType.PommelHorse)
		{
			HACK_PommelHorseTimeUntilSwish = m_PommelHorseMasherSettings.m_ExpectedPlayerAnimationTime / 2f;
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter)
		{
			if (null != localCharacter.m_CharacterAnimator)
			{
				localCharacter.m_CharacterAnimator.StopAnimation(m_RepFirstStage);
				localCharacter.m_CharacterAnimator.StopAnimation(m_RepSecondStage);
				localCharacter.m_CharacterAnimator.FinishControllingNormalizedTime();
			}
			if (null != localCharacter.m_CharacterStats && localCharacter.m_CharacterStats.m_bIsPlayer)
			{
				if (m_ButtonMasher != null)
				{
					m_ButtonMasher.gameObject.SetActive(value: false);
					m_ButtonMasher.Reset();
					m_ButtonMasher = null;
				}
				Player player = (Player)localCharacter;
				if (player != null)
				{
					player.OnMinigameExited();
				}
			}
		}
		SetInteractionObjectAnimatorState(0);
		ResetNormalizedAnimTime();
		SendEvent(InteractiveEventType.InteractionEnded);
	}

	public override void OnCharacterFailedToStart(Character character)
	{
		base.OnCharacterFailedToStart(character);
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			SpeechManager.GetInstance().SaySomething(character, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_ButtonMasher != null)
		{
			if (m_ButtonMasher.GetMasherState() == AlternateButtonMasher.MasherState.Valid)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
				if (m_StrengthChange > 0)
				{
					float amount = (float)m_StrengthChange * m_RewardMultiplier;
					m_interactingCharacter.m_CharacterStats.IncreaseStrengthRPC(amount);
					EffectManager.PlayEffect(EffectManager.effectType.StrengthIncrease, GetEffectPositionForStatIncreaseEffect(m_interactingCharacter));
					m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Strength, 0f, 100f);
				}
				if (m_CardioChange > 0)
				{
					float amount2 = (float)m_CardioChange * m_RewardMultiplier;
					m_interactingCharacter.m_CharacterStats.IncreaseCardioRPC(amount2);
					EffectManager.PlayEffect(EffectManager.effectType.CardioIncrease, GetEffectPositionForStatIncreaseEffect(m_interactingCharacter));
					m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Cardio, 0f, 100f);
				}
				if (m_EnergyChange > 0)
				{
					float amount3 = (float)m_EnergyChange * m_RewardMultiplier;
					m_interactingCharacter.m_CharacterStats.IncreaseEnergyRPC(amount3);
					EffectManager.PlayEffect(EffectManager.effectType.StaminaIncrease, GetEffectPositionForStatIncreaseEffect(m_interactingCharacter));
					m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Energy, 0f, 100f);
				}
			}
			if (m_ButtonMasher.StaminaSpent())
			{
				m_interactingCharacter.m_CharacterStats.DecreaseEnergyRPC(m_StaminaDecreasePerTriggerUse);
				EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, GetEffectPositionForCharacterInInteraction(m_interactingCharacter));
			}
			if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
			{
				if ((m_interactingCharacter as Player).m_Gamer.IsLocal() && m_GymEquipmentType == GymInteractionType.PommelHorse)
				{
					HACK_PommelHorseTimeUntilSwish -= UpdateManager.deltaTime;
					if (HACK_PommelHorseTimeUntilSwish <= 0f)
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Gym_Swish, base.gameObject);
						HACK_PommelHorseTimeUntilSwish = m_PommelHorseMasherSettings.m_ExpectedPlayerAnimationTime + HACK_PommelHorseTimeUntilSwish;
					}
				}
				if (!m_interactingCharacter.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaDecreasePerTriggerUse))
				{
					SpeechManager.GetInstance().SaySomething(m_interactingCharacter, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
					RequestStopInteraction(m_interactingCharacter);
				}
				else
				{
					bool flag = false;
					if (m_ButtonMasher.ConsumeIsRepACompleted() && m_RepFirstStage != AnimState.INVALID)
					{
						flag = true;
					}
					if (m_ButtonMasher.ConsumeIsRepBCompleted() && m_RepSecondStage != AnimState.INVALID)
					{
						flag = true;
					}
					if (flag)
					{
						HandleNewRepDone();
					}
				}
			}
			if (m_GymEquipmentType != 0 || !(m_WeightMasher != null))
			{
				return;
			}
			if (m_InteractionObjectAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == m_NormalizedAnimStateHash)
			{
				if (m_interactingCharacter.m_CharacterAnimator.GetAnimState() != m_RepFirstStage)
				{
					m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_RepFirstStage);
					m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_RepFirstStage);
				}
				if (m_InteractionObjectAnimator != null && base.CurrentAnimState != 4)
				{
					SetInteractionObjectAnimatorState(4);
				}
				m_interactingCharacter.m_CharacterAnimator.SetNormaisedTime(m_WeightMasher.m_Slider.value);
			}
			else
			{
				ResetNormalizedAnimTime();
			}
		}
		else if (m_RepFirstStage != AnimState.INVALID && m_RepSecondStage != AnimState.INVALID && m_interactingCharacter != null)
		{
			m_AICharacterTimeWaitingForRep += UpdateManager.deltaTime;
			if (m_AICharacterTimeWaitingForRep > m_AICharacterTimeBetweenReps)
			{
				HandleNewRepDone();
				m_AICharacterTimeWaitingForRep = 0f;
			}
		}
	}

	private void HandleNewRepDone()
	{
		if (m_bRepToggle)
		{
			PlayRepB();
		}
		else
		{
			PlayRepA();
		}
		m_bRepToggle = !m_bRepToggle;
	}

	private void PlayRepA()
	{
		if (m_GymEquipmentType != 0 || !m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			if (m_interactingCharacter != null)
			{
				if (!m_bIsRepBOneShot)
				{
					m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_RepSecondStage);
				}
				m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_RepFirstStage);
			}
			if (m_InteractionObjectAnimator != null)
			{
				SetInteractionObjectAnimatorState(4);
			}
		}
		else if (base.CurrentAnimState != 4)
		{
			SetInteractionObjectAnimatorState(4);
		}
	}

	private void PlayRepB()
	{
		if (m_GymEquipmentType != 0 || !m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			if (m_interactingCharacter != null)
			{
				if (!m_bIsRepAOneshot)
				{
					m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_RepFirstStage);
				}
				m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_RepSecondStage);
			}
			if (m_InteractionObjectAnimator != null)
			{
				SetInteractionObjectAnimatorState(5);
			}
		}
		else if (base.CurrentAnimState != 4)
		{
			SetInteractionObjectAnimatorState(4);
		}
	}

	private Vector3 GetEffectPositionForStatIncreaseEffect(Character character)
	{
		if (m_ButtonMasher != null && m_ButtonMasher.m_EffectOriginTransform != null)
		{
			return m_ButtonMasher.GetEffectSpawnPosition();
		}
		return GetEffectPositionForCharacterInInteraction(character);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		SetInteractionObjectAnimatorState(0);
		ResetNormalizedAnimTime();
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}
}
