using System;

[Serializable]
public class SpeechPODO
{
	public string m_TextId;

	public SpeechTone m_SpeechTone;

	public float m_Duration = -1f;

	public int m_Priority;

	public int m_ForcedVariation = -1;

	private SpeechPODO()
	{
	}

	public SpeechPODO(string textID, SpeechTone tone, float duration = -1f, int priority = 0, int forcedVariation = -1)
	{
		m_TextId = textID;
		m_SpeechTone = tone;
		m_Duration = duration;
		m_Priority = priority;
		m_ForcedVariation = forcedVariation;
	}

	public SpeechPODO(SpeechPODO other)
	{
		m_TextId = other.m_TextId;
		m_SpeechTone = other.m_SpeechTone;
		m_Duration = other.m_Duration;
		m_Priority = other.m_Priority;
		m_ForcedVariation = other.m_ForcedVariation;
	}

	public bool IsSet()
	{
		return !string.IsNullOrEmpty(m_TextId);
	}

	public static bool IsValid(SpeechPODO speech)
	{
		return speech?.IsSet() ?? false;
	}
}
