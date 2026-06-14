using UnityEngine;

public class WardenScreen : FakeCharacter
{
	public float m_FirstDelayVar = 5f;

	public float m_RepeatDelayVar = 5f;

	public float m_GapInSpeechTime = 10f;

	public float m_SpeechTime = 3f;

	private float m_TalkTimer;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		T17BehaviourManager.INITSTATE result = base.StartInit();
		SetNextTalkTime(bFirstTime: true);
		return result;
	}

	public override void ControlledUpdate()
	{
		if (!(m_SpeechBubbleHandler != null) || m_SpeechBubbleHandler.IsProcessingSpeech())
		{
			return;
		}
		m_TalkTimer -= UpdateManager.deltaTime;
		if (m_TalkTimer <= 0f)
		{
			string localized = string.Empty;
			if (Localization.Get("Text.NPC.Warden", out localized))
			{
				m_SpeechBubbleHandler.NewSpeech(localized, SpeechTone.Computer_Negative, m_SpeechTime, 10, bAllowTextColourControl: false);
			}
			SetNextTalkTime(bFirstTime: false);
		}
	}

	private void SetNextTalkTime(bool bFirstTime)
	{
		if (bFirstTime)
		{
			m_TalkTimer = Random.Range(0f, m_FirstDelayVar);
		}
		else
		{
			m_TalkTimer = m_GapInSpeechTime + Random.Range(0f, m_RepeatDelayVar);
		}
	}
}
