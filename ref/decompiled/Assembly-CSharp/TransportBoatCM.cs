using System.Collections.Generic;
using Slate;

public class TransportBoatCM : CutsceneManager<TransportBoatCM>
{
	public Cutscene m_EscapeSinglePlayerSecond;

	public Cutscene m_EscapeMultiplayerSecond;

	protected override void AddLevelSpecificScenesToList(List<Cutscene> cutscenes)
	{
		if (m_EscapeSinglePlayerSecond != null)
		{
			cutscenes.Add(m_EscapeSinglePlayerSecond);
		}
		if (m_EscapeMultiplayerSecond != null)
		{
			cutscenes.Add(m_EscapeMultiplayerSecond);
		}
	}
}
