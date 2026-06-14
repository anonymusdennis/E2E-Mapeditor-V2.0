using UnityEngine;

namespace Slate.ActionClips;

public abstract class SendGlobalMessage<T> : SendGlobalMessage
{
	public T value;

	public override string info => string.Format("Global Message\n'{0}'({1})", message, (value == null) ? "null" : value.ToString());

	protected override void OnEnter()
	{
		if (Application.isPlaying)
		{
			base.root.SendGlobalMessage(message, value);
		}
	}
}
[Description("Send a Unity Message to all actors of this Cutscene, including the Director Camera, as well as the Cutscene itself.")]
[Category("Events")]
public class SendGlobalMessage : DirectorActionClip
{
	[Required]
	public string message;

	public override string info => $"Global Message\n'{message}'";

	public override bool isValid => !string.IsNullOrEmpty(message);

	protected override void OnEnter()
	{
		if (Application.isPlaying)
		{
			base.root.SendGlobalMessage(message, null);
		}
	}
}
