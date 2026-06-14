using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class SolitaryPotatoesInteraction : AnimatedInteraction
{
	[Header("Minigame Settings")]
	public SolitaryPotatoMasher.MasherSettings m_MasherSettings = new SolitaryPotatoMasher.MasherSettings();

	[Range(0f, 10f)]
	public int m_StaminaLoss;

	private float m_StaminaLossModifier = 1f;

	private SolitaryPotatoMasher m_ButtonMasher;

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
		m_StaminaLossModifier = config.m_Solitary_StaminaLossModifier;
	}

	public override bool CanStartOrContinueInteraction(Character character)
	{
		return character != null && character.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaLoss) && character.m_bIsWantedForSolitary;
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
			SolitaryManager instance = SolitaryManager.GetInstance();
			if (instance == null || !instance.IsWantedForSolitary(character))
			{
				SpeechManager.GetInstance().SaySomething(character, "Text.Emote.NotNowSpeech", SpeechTone.Negative, 3f, 10);
			}
			else
			{
				SpeechManager.GetInstance().SaySomething(character, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
			}
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (!localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		Player player = (Player)localCharacter;
		if (player != null)
		{
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(player.m_PlayerCameraManagerBindingID);
			player.OnMinigameEntered();
			if (playerTrackedUIElements != null)
			{
				m_ButtonMasher = playerTrackedUIElements.GetSolitaryPotatoMasher();
			}
		}
		if (m_ButtonMasher != null)
		{
			m_ButtonMasher.gameObject.SetActive(value: true);
			m_ButtonMasher.SetupMasher(player, m_MasherSettings);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
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
				SolitaryManager.GetInstance().OnTaskCompleted(m_interactingCharacter);
			}
			if (m_ButtonMasher.GetShouldExpendStamina())
			{
				float amount = (float)m_StaminaLoss * m_StaminaLossModifier;
				m_interactingCharacter.m_CharacterStats.DecreaseEnergyRPC(amount);
				EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, GetEffectPositionForCharacterInInteraction(m_interactingCharacter));
			}
			if (!m_interactingCharacter.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaLoss))
			{
				SpeechManager.GetInstance().SaySomething(m_interactingCharacter, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				RequestStopInteraction(m_interactingCharacter);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rep_Fail, base.gameObject);
			}
			if (!m_interactingCharacter.m_bIsWantedForSolitary)
			{
				RequestStopInteraction(m_interactingCharacter);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Masher_Rep_Complete, base.gameObject);
			}
		}
	}
}
