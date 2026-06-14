using System;
using UnityEngine;

namespace Slate;

public struct TimeOutPointer : IDirectableTimePointer
{
	private bool triggered;

	private IDirectable target;

	float IDirectableTimePointer.time => target.endTime;

	public TimeOutPointer(IDirectable target)
	{
		this.target = target;
		triggered = false;
	}

	void IDirectableTimePointer.TriggerForward(float currentTime, float previousTime)
	{
		if ((currentTime >= target.endTime || (currentTime == target.root.length && target.startTime < target.root.length)) && !triggered)
		{
			triggered = true;
			target.Update(target.endTime - target.startTime, Mathf.Max(0f, previousTime - target.startTime));
			target.Exit();
		}
	}

	void IDirectableTimePointer.Update(float currentTime, float previousTime)
	{
		throw new NotImplementedException();
	}

	void IDirectableTimePointer.TriggerBackward(float currentTime, float previousTime)
	{
		if ((currentTime < target.endTime || currentTime <= 0f) && currentTime != target.root.length && triggered)
		{
			triggered = false;
			target.ReverseEnter();
			target.Update(target.endTime - target.startTime, target.endTime - target.startTime);
		}
	}
}
