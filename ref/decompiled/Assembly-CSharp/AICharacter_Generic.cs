public class AICharacter_Generic : AICharacter
{
	public string m_SpeechID;

	protected override void OnStart()
	{
		if (m_SpeechID != string.Empty)
		{
			m_AIBlackboard.SetValue("SpeechID", m_SpeechID);
		}
	}
}
