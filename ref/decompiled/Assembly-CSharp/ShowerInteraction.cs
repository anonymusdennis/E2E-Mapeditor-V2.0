using AUTOGEN_T17Wwise_Enums;

public class ShowerInteraction : AnimatedInteraction
{
	public float m_HealthIncrease = 1f;

	public float m_EnergyIncrease = 2f;

	public ParticleControl m_WaterParticles;

	public float m_IncreaseStatTime = 2f;

	private float m_IncreaseStatTimer;

	private float m_TimeWhenStartingShower = -1f;

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_IncreaseStatTimer = 0f;
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.Shower);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = (Player)m_interactingCharacter;
			if (player != null && player.m_Gamer.IsLocal())
			{
				m_TimeWhenStartingShower = RoutineManager.GetInstance().GetElapsedSeconds();
			}
		}
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.Shower);
	}

	public override void InteractionReadyUpdate()
	{
		m_IncreaseStatTimer += UpdateManager.deltaTime;
		if (m_IncreaseStatTimer > m_IncreaseStatTime)
		{
			m_IncreaseStatTimer = 0f;
			m_interactingCharacter.m_CharacterStats.IncreaseHealthRPC(m_HealthIncrease);
			m_interactingCharacter.m_CharacterStats.IncreaseEnergyRPC(m_EnergyIncrease);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (!(m_TimeWhenStartingShower > 0f))
		{
			return;
		}
		float num = RoutineManager.GetInstance().GetElapsedSeconds() - m_TimeWhenStartingShower;
		num /= 60f;
		if (num > 1f)
		{
			Player player = (Player)localCharacter;
			if (null != player)
			{
				StatSystem.GetInstance().IncStat(26, num, player.m_Gamer, string.Empty);
			}
		}
		m_TimeWhenStartingShower = -1f;
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		if (m_WaterParticles != null)
		{
			m_WaterParticles.Simulate(0f, bWithChildren: true, bRestart: true);
			m_WaterParticles.Play();
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Env_Shower_Loop, base.gameObject);
		base.InteractionStartedEvent(interactingCharacter);
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		if (m_WaterParticles != null && m_WaterParticles.isPlaying)
		{
			m_WaterParticles.Stop();
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Stop_Env_Shower_Loop, base.gameObject);
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
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
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + 0.05f);
		}
	}
}
