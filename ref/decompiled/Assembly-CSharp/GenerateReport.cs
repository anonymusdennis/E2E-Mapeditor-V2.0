using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Action")]
public class GenerateReport : ActionTask<AICharacter_Guard>
{
	public bool m_bReturnStatus;

	public AIEvent.EventType[] checkEventTypes;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty += '\n';
			empty += '\n';
			for (int i = 0; i < checkEventTypes.Length; i++)
			{
				empty += checkEventTypes[i];
				if (i < checkEventTypes.Length - 1)
				{
					empty += ", ";
					empty += '\n';
				}
			}
			return "Generate Reports: " + empty;
		}
	}

	protected override void OnExecute()
	{
		EndAction(m_bReturnStatus);
	}

	protected override string OnInit()
	{
		for (int i = 0; i < checkEventTypes.Length; i++)
		{
			base.agent.AddReportableEvent(checkEventTypes[i], AIEventReceived);
		}
		return base.OnInit();
	}

	private void AIEventReceived(AIEventMemory aiEventMemory)
	{
		Character character = base.agent.m_Character;
		if (!character.GetIsKnockedOut() && !character.GetIsDisabled() && !character.GetIsSleeping())
		{
			base.agent.GenerateReport(aiEventMemory);
		}
	}
}
