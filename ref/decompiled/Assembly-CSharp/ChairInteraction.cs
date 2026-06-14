using AUTOGEN_T17Wwise_Enums;

public class ChairInteraction : AnimatedInteraction
{
	public float m_HealthIncrease = 1f;

	public float m_EnergyIncrease = 2f;

	public float m_EnergyIncreaseWithTray = 6f;

	public float m_IncreaseStatTime = 2f;

	private float m_IncreaseStatTimer;

	private bool m_bIsOccuppied;

	protected override void Init()
	{
		base.Init();
		T17NetManager.OnBecameMasterClient += UnoccupyChair;
	}

	protected override void OnDestroy()
	{
		T17NetManager.OnBecameMasterClient -= UnoccupyChair;
		base.OnDestroy();
	}

	protected void UnoccupyChair()
	{
		m_bIsOccuppied = false;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter && localCharacter.GetHasTray() && localCharacter.m_CharacterStats != null && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			localCharacter.SetHasTray(hasTray: false);
		}
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_IncreaseStatTimer = 0f;
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Sitting);
		m_bIsOccuppied = true;
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_bIsOccuppied = false;
		if (null != m_interactingCharacter && null != m_interactingCharacter.m_CharacterStats)
		{
			m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Sitting);
			if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
			}
		}
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		m_bIsOccuppied = true;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		m_bIsOccuppied = false;
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
			m_interactingCharacter.m_CharacterStats.IncreaseHealthRPC(m_HealthIncrease);
			float amount = ((!m_interactingCharacter.GetHasTray()) ? m_EnergyIncrease : m_EnergyIncreaseWithTray);
			m_interactingCharacter.m_CharacterStats.IncreaseEnergyRPC(amount);
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return !m_bIsOccuppied && base.AllowedToInteract(localCharacter);
	}
}
