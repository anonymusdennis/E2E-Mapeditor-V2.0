using UnityEngine;

public class FadeManager : MonoBehaviour
{
	public delegate void FadeHandler();

	public enum CurtainStates
	{
		Raised,
		Lowering,
		Lowered,
		Rising
	}

	private FadeHandler OnCurtainRaisedCallback;

	private FadeHandler OnCurtainLoweredCallback;

	public T17Image m_FadeUIImage;

	private static FadeManager m_Instance;

	public float m_FadeDuration = 0.5f;

	public UIAnimatedEffect m_FadingEffect;

	private CurtainStates m_FadeStatus;

	public static FadeManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = this;
			m_FadingEffect.AnimationFinishedEvent += m_FadingEffect_AnimationFinishedEvent;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
			m_FadingEffect.AnimationFinishedEvent -= m_FadingEffect_AnimationFinishedEvent;
		}
	}

	public void StartCurtainLower(FadeHandler onLoweredCallback = null)
	{
		if (m_FadeStatus == CurtainStates.Raised)
		{
			m_FadeStatus = CurtainStates.Lowering;
			if (m_FadingEffect != null)
			{
				m_FadingEffect.TriggerStart(m_FadeDuration);
			}
			OnCurtainLoweredCallback = onLoweredCallback;
		}
		else if (m_FadeStatus == CurtainStates.Lowered)
		{
			onLoweredCallback?.Invoke();
		}
	}

	public void StartCurtainRaise(FadeHandler onRaisedCallback = null)
	{
		if (m_FadeStatus == CurtainStates.Lowered)
		{
			m_FadeStatus = CurtainStates.Rising;
			if (m_FadingEffect != null)
			{
				m_FadingEffect.TriggerReverse(m_FadeDuration);
			}
			OnCurtainRaisedCallback = onRaisedCallback;
		}
	}

	public void HideCurtain(int simulateStateResolvedDepth)
	{
		for (int i = 0; i < simulateStateResolvedDepth; i++)
		{
			ResolveCurrentStatus();
		}
		m_FadeStatus = CurtainStates.Raised;
		if (m_FadingEffect != null)
		{
			m_FadingEffect.Reset();
		}
	}

	private void m_FadingEffect_AnimationFinishedEvent(UIAnimatedEffect sender)
	{
		ResolveCurrentStatus();
	}

	private void ResolveCurrentStatus()
	{
		switch (m_FadeStatus)
		{
		case CurtainStates.Lowering:
			m_FadeStatus = CurtainStates.Lowered;
			if (OnCurtainLoweredCallback != null)
			{
				OnCurtainLoweredCallback();
			}
			break;
		case CurtainStates.Rising:
			m_FadeStatus = CurtainStates.Raised;
			if (OnCurtainRaisedCallback != null)
			{
				OnCurtainRaisedCallback();
			}
			break;
		}
	}
}
