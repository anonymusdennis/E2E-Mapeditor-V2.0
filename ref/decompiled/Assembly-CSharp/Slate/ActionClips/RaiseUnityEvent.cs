using UnityEngine;
using UnityEngine.Events;

namespace Slate.ActionClips;

[Description("Raise a Unity Event when the time is moving forwards and another when the time is moving backwards. This is helpfull if you want to use these events with UI to reverse their state, both in runtime and in editor.")]
[Category("Events")]
public class RaiseUnityEvent : DirectorActionClip
{
	public string customLabel;

	public UnityEvent forwardEvent = new UnityEvent();

	public UnityEvent reverseEvent = new UnityEvent();

	public override string info
	{
		get
		{
			if (!string.IsNullOrEmpty(customLabel))
			{
				return customLabel;
			}
			int persistentEventCount = forwardEvent.GetPersistentEventCount();
			int persistentEventCount2 = reverseEvent.GetPersistentEventCount();
			string arg = ((persistentEventCount <= 0) ? "No Event" : string.Empty);
			string arg2 = ((persistentEventCount2 <= 0) ? "No Event" : string.Empty);
			if (persistentEventCount > 0)
			{
				Object persistentTarget = forwardEvent.GetPersistentTarget(0);
				string persistentMethodName = forwardEvent.GetPersistentMethodName(0);
				arg = string.Format("{0}: {1}", (!(persistentTarget != null)) ? "null" : persistentTarget.name, persistentMethodName);
			}
			if (persistentEventCount2 > 0)
			{
				Object persistentTarget2 = reverseEvent.GetPersistentTarget(0);
				string persistentMethodName2 = reverseEvent.GetPersistentMethodName(0);
				arg2 = string.Format("{0}: {1}", (!(persistentTarget2 != null)) ? "null" : persistentTarget2.name, persistentMethodName2);
			}
			return $"> {arg}\n< {arg2}";
		}
	}

	protected override void OnEnter()
	{
		forwardEvent.Invoke();
	}

	protected override void OnReverse()
	{
		reverseEvent.Invoke();
	}
}
