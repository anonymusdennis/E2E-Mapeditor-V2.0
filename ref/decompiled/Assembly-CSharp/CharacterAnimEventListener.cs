using AUTOGEN_T17Wwise_Enums;

public class CharacterAnimEventListener : AnimEventListener
{
	private Character m_Character;

	private bool m_bAttackEvent;

	private int m_LocalStepsTaken;

	private void Awake()
	{
		m_Character = GetComponentInParent<Character>();
		if (m_Character != null)
		{
			m_Owner = m_Character.gameObject;
		}
	}

	private void LateUpdate()
	{
		m_bAttackEvent = false;
	}

	private void Footstep(int value)
	{
		if (!m_Character.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Footsteps, m_Character.gameObject);
		if (m_Character.m_NetView.isMine)
		{
			m_LocalStepsTaken++;
			if (m_LocalStepsTaken >= 20)
			{
				ScoreManager.EventRPC(ScoreManager.Events.FootstepsTaken, m_Character);
				m_LocalStepsTaken = 0;
			}
		}
	}

	public override void SoundEvent(int animationEvent)
	{
		if (animationEvent >= 0 && animationEvent <= CharacterAudioEvents.m_Instance.m_AudioEvents.Length - 1 && !m_bAttackEvent)
		{
			m_bAttackEvent = true;
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, CharacterAudioEvents.m_Instance.m_AudioEvents[animationEvent], m_Character.gameObject);
		}
	}

	private void Attack(int animationEvent)
	{
		if (!m_bAttackEvent)
		{
			m_bAttackEvent = true;
			PlayAttackSoundEffects(isNormalAttack: true);
		}
	}

	private void HeavyAttack(int animationEvent)
	{
		if (!m_bAttackEvent)
		{
			m_bAttackEvent = true;
			PlayAttackSoundEffects(isNormalAttack: false);
		}
	}

	private void PlayAttackSoundEffects(bool isNormalAttack)
	{
		m_Character.AttackAnimationDoDamageEvent(isNormalAttack, Character.GamelogicRunModes.AudioOnly);
	}
}
