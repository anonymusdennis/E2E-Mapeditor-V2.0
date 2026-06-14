using UnityEngine;

public class FrontendEventListener : MonoBehaviour
{
	public delegate void AnimationEventHandler(FrontendEventListener sender, bool transitionStarted);

	public event AnimationEventHandler AnimationEvent;

	private void TransitionAnimationStarted()
	{
		if (this.AnimationEvent != null)
		{
			this.AnimationEvent(this, transitionStarted: true);
		}
	}

	private void TransitionAnimationFinished()
	{
		if (this.AnimationEvent != null)
		{
			this.AnimationEvent(this, transitionStarted: false);
		}
	}
}
