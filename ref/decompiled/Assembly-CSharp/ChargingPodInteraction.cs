using AUTOGEN_T17Wwise_Enums;

public class ChargingPodInteraction : AnimatedInteraction
{
	public float m_HealthIncrease = 1f;

	public float m_EnergyIncrease = 1f;

	public float m_IncreaseStatTime = 2f;

	private float m_IncreaseStatTimer;

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.m_bIsHidden = true;
			m_IncreaseStatTimer = 0f;
			if (null != m_interactingCharacter.m_CharacterStats)
			{
				m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Sitting);
			}
		}
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		if (null != m_interactingCharacter)
		{
			m_interactingCharacter.m_bIsHidden = false;
			if (null != m_interactingCharacter.m_CharacterStats)
			{
				m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Sitting);
			}
		}
	}

	public override void InteractionReadyEndEvent(Character interactingCharacter)
	{
		base.InteractionReadyEndEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Locker_Exit.ToString(), interactingCharacter.gameObject);
		}
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Locker_Enter.ToString(), interactingCharacter.gameObject);
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override void InteractionReadyUpdate()
	{
		m_IncreaseStatTimer += UpdateManager.deltaTime;
		if (m_IncreaseStatTimer > m_IncreaseStatTime)
		{
			m_IncreaseStatTimer = 0f;
			if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats)
			{
				m_interactingCharacter.m_CharacterStats.IncreaseHealthRPC(m_HealthIncrease);
				m_interactingCharacter.m_CharacterStats.IncreaseEnergyRPC(m_EnergyIncrease);
			}
		}
	}
}
