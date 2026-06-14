using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Events")]
[Description("Have if an AI Event is received and return true for one frame if it matches the required event types")]
public class HaveAIEvent : ConditionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<AIEventMemory> saveEventValue;

	public bool getFirstKnownEvent = true;

	public AIEvent.EventType[] haveEventTypes;

	public bool m_bCheckAndForget;

	public bool m_bCheckKOandForget;

	private AIEventMemory m_previousMemory;

	protected override string info
	{
		get
		{
			string empty = string.Empty;
			empty += '\n';
			if (haveEventTypes != null)
			{
				for (int i = 0; i < haveEventTypes.Length; i++)
				{
					empty += haveEventTypes[i];
					if (i < haveEventTypes.Length - 1)
					{
						empty += ", ";
						empty += '\n';
					}
				}
			}
			if (m_bCheckAndForget)
			{
				empty += '\n';
				empty += '\n';
				empty += "Check and Forget";
			}
			empty += '\n';
			empty += '\n';
			empty += ((!getFirstKnownEvent) ? "Get Last" : "Get First");
			return string.Concat("Have Event: ", empty, '\n', '\n', "[", saveEventValue, "]");
		}
	}

	protected override string OnInit()
	{
		return null;
	}

	protected override bool OnCheck()
	{
		AIEvent.EventType eventTypeFound = AIEvent.EventType.Event_Count;
		bool flag = base.agent.KnownEvents(haveEventTypes, out eventTypeFound);
		if (flag)
		{
			AIEventMemory aIEventMemory = ((!getFirstKnownEvent) ? base.agent.GetLastKnownEvent(eventTypeFound) : base.agent.GetFirstKnownEvent(eventTypeFound));
			saveEventValue.value = aIEventMemory;
			if (aIEventMemory != null)
			{
				bool flag2 = m_bCheckAndForget && !aIEventMemory.m_bEventValid;
				if (!flag2)
				{
					flag2 |= m_bCheckKOandForget && aIEventMemory.m_bFlagToForget;
				}
				if (flag2)
				{
					base.agent.ForgetEvent(aIEventMemory);
					saveEventValue.value = null;
					return false;
				}
			}
		}
		else
		{
			saveEventValue.value = null;
		}
		if (saveEventValue.value != m_previousMemory && saveEventValue.value != null)
		{
			bool flag3 = m_previousMemory != null;
			m_previousMemory = saveEventValue.value;
			if (flag3)
			{
				InteractiveObject interactiveObject = base.agent.m_Character.GetInteractiveObject();
				if (interactiveObject != null)
				{
					if (interactiveObject.GetInteractionClassType() == InteractiveObject.InteractionType.AnimatedInteractiveObject)
					{
						base.agent.m_Character.RequestStopInteraction();
					}
					else
					{
						base.agent.m_Character.ForceStopInteraction();
					}
				}
				return false;
			}
		}
		return flag;
	}
}
