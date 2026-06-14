using UnityEngine;

public class AnimationEffectCompletdEchoer : MonoBehaviour
{
	public delegate void AnimationFinishedHandler(AnimationEffectCompletdEchoer sender);

	public event AnimationFinishedHandler AnimationFinishedEvent;

	protected void AnimationFinished()
	{
		if (this.AnimationFinishedEvent != null)
		{
			this.AnimationFinishedEvent(this);
		}
	}
}
