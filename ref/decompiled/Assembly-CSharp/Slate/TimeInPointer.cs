using UnityEngine;

namespace Slate;

public struct TimeInPointer : IDirectableTimePointer
{
	private bool triggered;

	private float lastTargetStartTime;

	private IDirectable target;

	float IDirectableTimePointer.time => target.startTime;

	public TimeInPointer(IDirectable target)
	{
		this.target = target;
		triggered = false;
		lastTargetStartTime = target.startTime;
	}

	void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
	{
		if (currentTime >= target.startTime && !triggered)
		{
			triggered = true;
			target.Enter();
			target.Update(0f, 0f);
		}
	}

	void IDirectableTimePointer.Update(float currentTime, float previousTime)
	{
		if (currentTime >= target.startTime && currentTime < target.endTime && currentTime > 0f && currentTime < target.root.length)
		{
			float value = currentTime - target.startTime;
			float value2 = previousTime - target.startTime + (target.startTime - lastTargetStartTime);
			value = Mathf.Clamp(value, 0f, target.endTime - target.startTime);
			value2 = Mathf.Clamp(value2, 0f, target.endTime - target.startTime);
			target.Update(value, value2);
			lastTargetStartTime = target.startTime;
		}
	}

	void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
	{
		if ((currentTime < target.startTime || currentTime <= 0f) && triggered)
		{
			triggered = false;
			target.Update(0f, Mathf.Min(target.endTime - target.startTime, previousTime - target.startTime));
			target.Reverse();
		}
	}
}
