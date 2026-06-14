using UnityEngine;

public class CaptureAnimatorSoundEvent : MonoBehaviour
{
	private void Start()
	{
		GlobalStart.EndLevelEvent += OnLevelEnd;
	}

	private void SoundEvent(AnimationEvent animationEvent)
	{
		if (GlobalStart.GetInstance() != null && GlobalStart.GetInstance().GetMode() != GlobalStart.GLOBALSTART_MODE.WAIT_FOR_LOADING_LEVEL)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, animationEvent.stringParameter, base.gameObject);
		}
	}

	private void OnDestroy()
	{
		OnLevelEnd();
		GlobalStart.EndLevelEvent -= OnLevelEnd;
	}

	private void OnLevelEnd()
	{
		AkSoundEngine.UnregisterGameObj(base.gameObject);
	}
}
