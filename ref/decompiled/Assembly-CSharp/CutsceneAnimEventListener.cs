public class CutsceneAnimEventListener : AnimEventListener
{
	private void Awake()
	{
	}

	private void LateUpdate()
	{
	}

	private void Attack(int animationEvent)
	{
	}

	private void HeavyAttack(int animationEvent)
	{
	}

	public override void SoundEvent(int animationEvent)
	{
		if (animationEvent >= 0 && animationEvent <= CharacterAudioEvents.m_Instance.m_AudioEvents.Length - 1)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, CharacterAudioEvents.m_Instance.m_AudioEvents[animationEvent], base.transform.parent.gameObject);
		}
	}
}
