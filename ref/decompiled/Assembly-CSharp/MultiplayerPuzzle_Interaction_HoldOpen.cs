using UnityEngine;

public class MultiplayerPuzzle_Interaction_HoldOpen : MultiplayerPuzzle_Interaction
{
	private const float VALID_TIME_DELAY = 1f;

	[Header("Masher Settings")]
	public AlternateButtonMasher.AlternateMasherSettings m_MasherSettings;

	public float m_StaminaDrain;

	private AlternateButtonMasher m_ButtonMasher;

	private AlternateButtonMasher.MasherState m_MasherState;

	private float m_TimeUntilValid;

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_TimeUntilValid = 0f;
		if (!(m_interactingCharacter != null))
		{
			return;
		}
		Player player = (Player)m_interactingCharacter;
		if (player != null)
		{
			PerPlayerTrackedUIElements myTrackedUIElements = player.GetMyTrackedUIElements();
			if (myTrackedUIElements != null)
			{
				m_ButtonMasher = myTrackedUIElements.GetButtonMasher();
				m_ButtonMasher.SetMasherSettings(ref m_MasherSettings);
				myTrackedUIElements.ShowButtonMasher(player);
			}
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		m_MasherState = AlternateButtonMasher.MasherState.Idle;
		SetInteractionStateRPC(valid: false);
		if (m_interactingCharacter != null)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null)
			{
				PerPlayerTrackedUIElements myTrackedUIElements = player.GetMyTrackedUIElements();
				if (myTrackedUIElements != null)
				{
					myTrackedUIElements.HideButtonMasher();
				}
			}
		}
		base.OnExitInteraction(localCharacter);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		AlternateButtonMasher.MasherState masherState = m_ButtonMasher.GetMasherState();
		if (m_MasherState != masherState)
		{
			m_MasherState = masherState;
			if (masherState == AlternateButtonMasher.MasherState.Valid)
			{
				m_TimeUntilValid = 1f;
			}
			else
			{
				m_TimeUntilValid = 0f;
				SetInteractionStateRPC(valid: false);
			}
		}
		if (m_TimeUntilValid > 0f)
		{
			m_TimeUntilValid -= UpdateManager.deltaTime;
			if (m_TimeUntilValid <= 0f)
			{
				m_TimeUntilValid = 0f;
				SetInteractionStateRPC(valid: true);
			}
		}
		if (m_interactingCharacter != null)
		{
			if (m_ButtonMasher.StaminaSpent())
			{
				m_interactingCharacter.m_CharacterStats.DecreaseEnergyRPC(m_StaminaDrain);
				EffectManager.PlayEffect(EffectManager.effectType.StaminaDecrease, GetEffectPositionForCharacterInInteraction(m_interactingCharacter));
			}
			if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer && !m_interactingCharacter.m_CharacterStats.HasEnoughEnergyForTask(m_StaminaDrain))
			{
				SpeechManager.GetInstance().SaySomething(m_interactingCharacter, "Text.Player.NoStamina", SpeechTone.Negative, 3f, 10);
				RequestStopInteraction(m_interactingCharacter);
			}
		}
	}
}
