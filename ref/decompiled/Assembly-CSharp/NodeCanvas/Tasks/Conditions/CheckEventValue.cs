using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Conditions;

[Category("✫ Utility")]
[Description("Check if an event is received and it's value is equal to specified value, then return true for one frame")]
[EventReceiver(new string[] { "OnCustomEvent" })]
public class CheckEventValue<T> : ConditionTask<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName;

	public BBParameter<T> value;

	protected override string info => $"Event [{eventName}].value == {value}";

	protected override bool OnCheck()
	{
		return false;
	}

	public void OnCustomEvent(EventData receivedEvent)
	{
		if (receivedEvent is EventData<T> && base.isActive && receivedEvent.name.ToUpper() == eventName.value.ToUpper())
		{
			T val = ((EventData<T>)receivedEvent).value;
			if (val != null && val.Equals(value.value))
			{
				YieldReturn(value: true);
			}
		}
	}
}
