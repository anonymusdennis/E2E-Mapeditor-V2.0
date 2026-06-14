using UnityEngine;

namespace Slate.ActionClips;

public abstract class SendMessage<T> : SendMessage
{
	public T value;

	public override string info => string.Format("Message\n{0}({1})", message, (value == null) ? "null" : value.ToString());

	public override bool isValid => !string.IsNullOrEmpty(message);

	protected override void OnEnter()
	{
		if (Application.isPlaying)
		{
			Debug.Log($"<b>({base.actor.name}) Actor Message Send:</b> '{message}' ({value})");
			base.actor.SendMessage(message, value);
		}
	}
}
[Category("Events")]
[Description("Send a Unity Message to the actor")]
public class SendMessage : ActorActionClip
{
	[Required]
	public string message;

	public override string info => "Message\n" + message;

	public override bool isValid => !string.IsNullOrEmpty(message);

	protected override void OnEnter()
	{
		if (Application.isPlaying)
		{
			Debug.Log($"<b>({base.actor.name}) Actor Message Send:</b> '{message}'");
			base.actor.SendMessage(message);
		}
	}
}
