using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Check if an AI Event is received and return true for one frame if it matches the required event types")]
[Category("★T17 Events")]
public class CheckAIEvent : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<AIEventMemory> saveEventValue;

	public bool b_AllEvents;

	public AIEvent.EventType[] checkEventTypes;

	private bool m_bHaveEvent;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty += '\n';
			if (b_AllEvents)
			{
				empty += "All Events";
			}
			else
			{
				for (int i = 0; i < checkEventTypes.Length; i++)
				{
					empty += checkEventTypes[i];
					if (i < checkEventTypes.Length - 1)
					{
						empty += ", ";
						empty += '\n';
					}
				}
			}
			return string.Concat("On Event: ", empty, '\n', '\n', "[", saveEventValue, "]");
		}
	}

	protected override string OnInit()
	{
		for (int i = 0; i < checkEventTypes.Length; i++)
		{
			base.agent.ListenForEvent(AIEventReceived, checkEventTypes[i]);
		}
		return null;
	}

	protected override bool OnCheck()
	{
		if (m_bHaveEvent)
		{
			m_bHaveEvent = false;
			return true;
		}
		return false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	private void AIEventReceived(AIEventMemory aiEvent)
	{
		saveEventValue.value = aiEvent;
		m_bHaveEvent = true;
	}
}
