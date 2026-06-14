using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class Speak : ActionTask<AICharacter>
{
	public BBParameter<string> m_TextID = string.Empty;

	public BBParameter<int> m_Variation = -1;

	public BBParameter<SpeechTone> m_Tone = SpeechTone.Positive;

	public BBParameter<float> m_Duration = 3f;

	public BBParameter<int> m_Priority = 0;

	public BBParameter<bool> m_WaitUntilDone = false;

	public BBParameter<AIEventMemory> m_CurrentEvent;

	private float m_SpeechTime;

	protected override string info => "Speak \n" + m_TextID.value + "\n" + ((m_Variation.value == -1) ? string.Empty : ("[variation: " + m_Variation.value + "]\n")) + "[tone: " + m_Tone.value.ToString() + "]\n" + "[duration: " + m_Duration.value + "]\n" + ((m_Priority.value <= 0) ? string.Empty : ("[priority: " + m_Priority.value + "]\n"));

	protected override void OnExecute()
	{
		if (!string.IsNullOrEmpty(m_TextID.value))
		{
			bool bAllowTextRecolour = false;
			if (base.agent.m_Character.m_CharacterRole == CharacterRole.Guard && m_CurrentEvent.value != null && m_CurrentEvent.value.m_TargetCharacter != null && m_CurrentEvent.value.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
			{
				bAllowTextRecolour = true;
			}
			SpeechManager.GetInstance().SaySomething(base.agent.m_Character, m_TextID.value, m_Tone.value, m_Duration.value, m_Priority.value, m_Variation.value, ignoreStatus: false, bAllowTextRecolour);
			if (!m_WaitUntilDone.value)
			{
				EndAction(true);
			}
			else
			{
				m_SpeechTime = m_Duration.value + UpdateManager.deltaTime;
			}
		}
		else
		{
			EndAction(true);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!m_WaitUntilDone.value)
		{
			EndAction(true);
			return;
		}
		m_SpeechTime -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_SpeechTime < 0f)
		{
			EndAction(true);
		}
	}
}
