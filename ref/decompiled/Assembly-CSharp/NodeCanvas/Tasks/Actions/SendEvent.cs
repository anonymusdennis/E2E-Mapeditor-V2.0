using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Utility")]
[Description("Send a graph event. If global is true, all graph owners in scene will receive this event. Use along with the 'Check Event' Condition")]
public class SendEvent : ActionTask<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName;

	public BBParameter<float> delay;

	public bool sendGlobal;

	protected override string info => string.Concat((!sendGlobal) ? string.Empty : "Global ", "Send Event [", eventName, "]", (!(delay.value > 0f)) ? string.Empty : string.Concat(" after ", delay, " sec."));

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= delay.value)
		{
			EventData eventData = new EventData(eventName.value);
			if (sendGlobal)
			{
				Graph.SendGlobalEvent(eventData);
			}
			else
			{
				base.agent.SendEvent(eventData);
			}
			EndAction();
		}
	}
}
[Description("Send a graph event with T value. If global is true, all graph owners in scene will receive this event. Use along with the 'Check Event' Condition")]
[Category("✫ Utility")]
public class SendEvent<T> : ActionTask<GraphOwner>
{
	[RequiredField]
	public BBParameter<string> eventName;

	public BBParameter<T> eventValue;

	public BBParameter<float> delay;

	public bool sendGlobal;

	protected override string info => string.Format("{0} Event [{1}] ({2}){3}", (!sendGlobal) ? string.Empty : "Global ", eventName, eventValue, (!(delay.value > 0f)) ? string.Empty : string.Concat(" after ", delay, " sec."));

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= delay.value)
		{
			EventData<T> eventData = new EventData<T>(eventName.value, eventValue.value);
			if (sendGlobal)
			{
				Graph.SendGlobalEvent(eventData);
			}
			else
			{
				base.agent.SendEvent(eventData);
			}
			EndAction();
		}
	}
}
