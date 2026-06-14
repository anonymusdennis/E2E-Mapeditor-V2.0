using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[EventReceiver(new string[] { "OnCustomEvent" })]
[Description("Check if an event is received and return true for one frame")]
[Category("✫ Utility")]
public class CheckEvent : ConditionTask<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName;

	protected override string info => "[" + eventName.ToString() + "]";

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnCustomEvent(EventData receivedEvent)
	{
		if (base.isActive && receivedEvent.name.ToUpper() == eventName.value.ToUpper())
		{
			YieldReturn(value: true);
		}
	}
}
[EventReceiver(new string[] { "OnCustomEvent" })]
[Description("Check if an event is received and return true for one frame. Optionaly save the received event's value")]
[Category("✫ Utility")]
public class CheckEvent<T> : ConditionTask<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName;

	[BlackboardOnly]
	public BBParameter<T> saveEventValue;

	protected override string info => $"Event [{eventName}]\n{saveEventValue} = EventValue";

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnCustomEvent(EventData receivedEvent)
	{
		if (base.isActive && receivedEvent.name.ToUpper() == eventName.value.ToUpper())
		{
			if (receivedEvent.value is T)
			{
				saveEventValue.value = (T)receivedEvent.value;
			}
			YieldReturn(value: true);
		}
	}
}
