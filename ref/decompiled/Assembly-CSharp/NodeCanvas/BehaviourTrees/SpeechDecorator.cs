using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

public class SpeechDecorator : StateDecorator
{
	public float m_fMinSpeakTime = 20f;

	public float m_fMaxSpeakTime = 50f;

	public BBParameter<string> m_TextID = string.Empty;

	public BBParameter<int> m_Variation = -1;

	public BBParameter<SpeechTone> m_Tone = SpeechTone.Positive;

	public BBParameter<float> m_Duration = 3f;

	public BBParameter<int> m_Priority = 0;

	public BBParameter<AIEventMemory> m_CurrentEvent;

	private float m_fSpeakTimer;

	protected override void OnEnter()
	{
		ResetTimer();
	}

	private void ResetTimer()
	{
		m_fSpeakTimer = Random.Range(m_fMinSpeakTime, m_fMaxSpeakTime);
	}

	protected override void OnUpdate()
	{
		m_fSpeakTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (!(m_fSpeakTimer <= 0f))
		{
			return;
		}
		if (m_AICharacter == null)
		{
			ResetTimer();
			return;
		}
		if (NPCManager.GetInstance().CanDoRoutineSpeech(m_AICharacter))
		{
			bool bAllowTextRecolour = false;
			if (m_AICharacter.m_Character.m_CharacterRole == CharacterRole.Guard && m_CurrentEvent.value != null && m_CurrentEvent.value.m_TargetCharacter != null && m_CurrentEvent.value.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
			{
				bAllowTextRecolour = true;
			}
			SpeechManager.GetInstance().SaySomething(m_AICharacter.m_Character, m_TextID.value, m_Tone.value, m_Duration.value, m_Priority.value, m_Variation.value, ignoreStatus: false, bAllowTextRecolour);
		}
		ResetTimer();
	}
}
