using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class StudyInteraction : AnimatedInteraction
{
	[Header("Minigame Settings")]
	public ReadingMasher.MasherSettings m_MasherSettings = new ReadingMasher.MasherSettings();

	[Range(0f, 99f)]
	public int m_IntellectReward;

	[Range(0f, 99f)]
	public int m_StaminaLoss;

	private float m_RewardMultiplier = 1f;

	private ReadingMasher m_ButtonMasher;

	protected override void Init()
	{
		base.Init();
		if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().minigameConfig != null)
		{
			ApplyConfigData(ConfigManager.GetInstance().minigameConfig);
		}
	}

	private void ApplyConfigData(MinigameConfig config)
	{
		m_RewardMultiplier = config.m_Reading_RewardModifier;
	}

	public override bool CanStartOrContinueInteraction(Character character)
	{
		return character != null && character.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaLoss);
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Sitting);
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Sitting);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override void OnCharacterFailedToStart(Character character)
	{
		base.OnCharacterFailedToStart(character);
		if (character.m_CharacterStats.m_bIsPlayer)
		{
			SpeechManager.GetInstance().SaySomething(character, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Book_Start, base.gameObject);
		if (!localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		Player player = (Player)localCharacter;
		if (player != null)
		{
			TutorialManager.GetInstance().StartTutorialRPC(player, TutorialSubject.Stats);
			player.OnMinigameEntered();
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(player.m_PlayerCameraManagerBindingID);
			if (playerTrackedUIElements != null)
			{
				m_ButtonMasher = playerTrackedUIElements.GetReadingMasher();
			}
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(player, TutorialSubject.IntellectMinigame);
			}
		}
		if (m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.SetActive(value: true);
			m_ButtonMasher.SetupMasher(player, m_MasherSettings);
			m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Intellect, 0f, 100f);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Book_End, base.gameObject);
		if (m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.SetActive(value: false);
			m_ButtonMasher.Reset();
		}
		m_ButtonMasher = null;
		if (localCharacter != null && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)localCharacter;
			if (player != null)
			{
				player.OnMinigameExited();
			}
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_ButtonMasher != null)
		{
			if (m_ButtonMasher.GetHasCompletedRep())
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
				float amount = (float)m_IntellectReward * m_RewardMultiplier;
				m_interactingCharacter.m_CharacterStats.IncreaseIntellectRPC(amount);
				EffectManager.PlayEffect(EffectManager.effectType.IntelligenceIncrease, GetEffectPositionForStatIncreaseEffect(m_interactingCharacter));
				m_ButtonMasher.UpdateProgress(m_interactingCharacter.m_CharacterStats.Intellect, 0f, 100f);
			}
			if (m_ButtonMasher.GetShouldExpendStamina())
			{
				m_interactingCharacter.m_CharacterStats.DecreaseEnergyRPC(m_StaminaLoss);
				EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, GetEffectPositionForCharacterInInteraction(m_interactingCharacter));
			}
			if (!m_interactingCharacter.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaLoss))
			{
				SpeechManager.GetInstance().SaySomething(m_interactingCharacter, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				RequestStopInteraction(m_interactingCharacter);
			}
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
}
