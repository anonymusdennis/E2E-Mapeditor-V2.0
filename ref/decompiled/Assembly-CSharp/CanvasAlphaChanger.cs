using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasAlphaChanger : TimedLerpingMonobehaviour
{
	private CanvasGroup m_Canvas;

	public bool m_bSkipLerpingUsingCurrentAlpha;

	protected override void Awake()
	{
		base.Awake();
		m_Canvas = GetComponent<CanvasGroup>();
	}

	protected override void DoWork()
	{
		m_Canvas.alpha = GetLerpValue();
	}

	public void FadeToOpaque(float time)
	{
		m_EffectTime = time;
		FadeToOpaque();
	}

	public void FadeToOpaque()
	{
		StartEffect(isReversed: false, disableOnFinish: true);
	}

	public void FadeToTransparent(float time)
	{
		m_EffectTime = time;
		FadeToTransparent();
	}

	public void FadeToTransparent()
	{
		StartEffect(isReversed: true, disableOnFinish: true);
	}

	public void ResetCanvas()
	{
		SetAlphaTo(1f);
	}

	public void SetAlphaTo(float alpha)
	{
		m_Canvas.alpha = alpha;
	}

	protected override void Reset()
	{
		base.Reset();
		ResetCanvas();
	}

	protected override void StartEffect(bool isReversed, bool disableOnFinish = false)
	{
		base.StartEffect(isReversed, disableOnFinish);
		if (m_bSkipLerpingUsingCurrentAlpha)
		{
			float a = ((!m_bIsReverse) ? 1f : 0f);
			float b = ((!m_bIsReverse) ? 0f : 1f);
			float num = Mathf.InverseLerp(a, b, m_Canvas.alpha);
			m_TimeUntilCompletion = m_EffectTime * num;
		}
	}

	public void Copy(CanvasAlphaChanger source)
	{
		m_EffectTime = source.m_EffectTime;
		if (m_Canvas == null)
		{
			m_Canvas = GetComponent<CanvasGroup>();
		}
		if (m_Canvas != null && source.m_Canvas != null)
		{
			m_Canvas.alpha = source.m_Canvas.alpha;
		}
		StartEffect(source.m_bIsReverse, source.m_bDisableOnFinish);
		m_TimeUntilCompletion = source.m_TimeUntilCompletion;
	}
}
