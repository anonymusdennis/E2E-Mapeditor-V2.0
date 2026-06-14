using UnityEngine;

[RequireComponent(typeof(CharacterSpeechBubbleHandler))]
public class CutsceneSpeechBubbleResponder : T17MonoBehaviour, ICutsceneStartEndResponder
{
	private CharacterSpeechBubbleHandler m_SpeechBubbleHandler;

	private bool m_bAwakeDone;

	protected override void Awake()
	{
		base.Awake();
		if (!m_bAwakeDone)
		{
			m_SpeechBubbleHandler = GetComponent<CharacterSpeechBubbleHandler>();
			m_bAwakeDone = true;
		}
	}

	protected virtual void OnDestroy()
	{
		m_SpeechBubbleHandler = null;
	}

	public void CutsceneStarted()
	{
		Awake();
		if (m_SpeechBubbleHandler != null)
		{
			m_SpeechBubbleHandler.EnableSpeechBubble();
		}
	}

	public void CutsceneEnded()
	{
		if (m_SpeechBubbleHandler != null)
		{
			m_SpeechBubbleHandler.ClearSpeechBuffer();
			return;
		}
		T17NetManager.LogGoogleException(string.Concat("Cutscene Speech Bubble Reponder doesn't have a handler: ", base.transform.name, " child of ", base.transform.parent.name, " in level ", LevelScript.GetCurrentLevelInfo().m_PrisonEnum, " playing cutscene ", CutsceneManagerBase.GetInstance().GetCurrentPlayingCutscene().name));
	}
}
