using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class MedicBedInteraction : BedInteraction
{
	public float m_fInmateConvalesceMinTime = 5f;

	public float m_fInmateConvalesceMaxTime = 20f;

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.MedicalSleeping);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
		}
	}

	public override void InteractionReadyStart()
	{
		base.InteractionReadyStart();
		m_interactingCharacter.m_CharacterStats.SetCharacterState(StatModifierEnum.MedicalSleeping);
		m_interactingCharacter.RegainConsciousness();
		if (!m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			m_interactingCharacter.m_CharacterStats.RestoreHealthRPC();
			float inmateConvalesceMinTime = ConfigManager.GetInstance().aiConfig.GetInmateConvalesceMinTime();
			float inmateConvalesceMaxTime = ConfigManager.GetInstance().aiConfig.GetInmateConvalesceMaxTime();
			m_interactingCharacter.PauseMovement(Random.Range(inmateConvalesceMinTime, inmateConvalesceMaxTime));
			AICharacter component = m_interactingCharacter.GetComponent<AICharacter>();
			if ((bool)component)
			{
				component.OnMedicBedInteractionStarted();
			}
		}
	}

	public override void InteractionReadyEnd(bool interruption = false)
	{
		base.InteractionReadyEnd(interruption);
		m_interactingCharacter.m_CharacterStats.UnSetCharacterState(StatModifierEnum.MedicalSleeping);
		m_interactingCharacter.PauseMovement(0f, force: true);
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Rest, m_interactingCharacter.gameObject);
			return;
		}
		AICharacter component = m_interactingCharacter.GetComponent<AICharacter>();
		component.OnMedicBedInteractionEnded();
	}
}
