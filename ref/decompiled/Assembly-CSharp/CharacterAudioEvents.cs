using UnityEngine;

public class CharacterAudioEvents : MonoBehaviour
{
	public string[] m_AudioEvents;

	public static CharacterAudioEvents m_Instance;

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}
}
