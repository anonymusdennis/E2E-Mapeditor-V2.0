using System.Collections.Generic;
using Slate;

public class OilRigCM : CutsceneManager<OilRigCM>
{
	public Cutscene m_SinglePlayerSecret;

	protected override void AddLevelSpecificScenesToList(List<Cutscene> cutscenes)
	{
		if (m_SinglePlayerSecret != null)
		{
			cutscenes.Add(m_SinglePlayerSecret);
		}
	}
}
