using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UIAnimatedEffect : MonoBehaviour
{
	public delegate void AnimationHandler(UIAnimatedEffect sender);

	protected Animator m_Animator;

	public string StartTrigger = "Start";

	public string ReverseEffectTrigger = "Stop";

	public string ResetTrigger = "Reset";

	private T17Image[] m_Images;

	public event AnimationHandler AnimationFinishedEvent;

	protected void Awake()
	{
		m_Animator = GetComponent<Animator>();
		m_Images = base.gameObject.GetComponentsInChildren<T17Image>(includeInactive: true);
	}

	public void PlayEffect(string triggerName, float time, Color? theColour = null)
	{
		if (m_Images != null && theColour.HasValue)
		{
			for (int i = 0; i < m_Images.Length; i++)
			{
				m_Images[i].color = theColour.Value;
			}
		}
		base.gameObject.SetActive(value: true);
		m_Animator.SetTrigger(triggerName);
		if (time <= 0f)
		{
			time = 0.01f;
		}
		m_Animator.speed = 1f / time;
	}

	public void TriggerStart(float time, Color? theColour = null)
	{
		m_Animator.ResetTrigger(ReverseEffectTrigger);
		PlayEffect(StartTrigger, time, theColour);
	}

	public void TriggerReverse(float time, Color? theColour = null)
	{
		m_Animator.ResetTrigger(StartTrigger);
		PlayEffect(ReverseEffectTrigger, time, theColour);
	}

	public void Reset()
	{
		m_Animator.ResetTrigger(StartTrigger);
		m_Animator.ResetTrigger(ReverseEffectTrigger);
		m_Animator.SetTrigger(ResetTrigger);
	}

	private void OnAnimationFinished(AnimationEvent animEvent)
	{
		if (this.AnimationFinishedEvent != null)
		{
			this.AnimationFinishedEvent(this);
		}
	}

	private void AnimationFinished(AnimationEvent animEvent)
	{
		OnAnimationFinished(animEvent);
	}
}
