using UnityEngine;

public class UIAnimatedEffectController : MonoBehaviour
{
	public enum Effects
	{
		Unassigned,
		FadeToOpaque,
		FadeToTransparent,
		LetterboxIn,
		LetterboxOut,
		PixelWindowToPixel,
		PixelWindowToFullyClear,
		FadeToOpaque_Hold,
		DistortIn,
		DistortOut,
		FadeToOpaqueWhite
	}

	public UIAnimatedEffect m_FadeEffect;

	public UIAnimatedEffect m_LetterboxEffect;

	public UIAnimatedEffect m_PixelWindowEffect;

	public UIAnimatedEffect m_DistortEffect;

	private void Awake()
	{
		if (!(m_FadeEffect == null) && !(m_LetterboxEffect == null) && !(m_PixelWindowEffect == null) && !(m_DistortEffect == null))
		{
			return;
		}
		UIAnimatedEffect[] componentsInChildren = GetComponentsInChildren<UIAnimatedEffect>();
		foreach (UIAnimatedEffect uIAnimatedEffect in componentsInChildren)
		{
			if (m_FadeEffect == null && uIAnimatedEffect.gameObject.name == "Fade")
			{
				m_FadeEffect = uIAnimatedEffect;
			}
			if (m_LetterboxEffect == null && uIAnimatedEffect.gameObject.name == "Letterbox")
			{
				m_LetterboxEffect = uIAnimatedEffect;
			}
			if (m_PixelWindowEffect == null && uIAnimatedEffect.gameObject.name == "Square")
			{
				m_PixelWindowEffect = uIAnimatedEffect;
			}
			if (m_DistortEffect == null && uIAnimatedEffect.gameObject.name == "Wibbly")
			{
				m_DistortEffect = uIAnimatedEffect;
			}
		}
	}

	public void PlayEffect(Effects effect, float time)
	{
		switch (effect)
		{
		case Effects.FadeToOpaque:
		case Effects.FadeToOpaque_Hold:
			m_FadeEffect.TriggerStart(time, Color.black);
			break;
		case Effects.FadeToOpaqueWhite:
			m_FadeEffect.TriggerStart(time, Color.white);
			break;
		case Effects.FadeToTransparent:
			m_FadeEffect.TriggerReverse(time);
			break;
		case Effects.LetterboxIn:
			m_LetterboxEffect.TriggerStart(time);
			break;
		case Effects.LetterboxOut:
			m_LetterboxEffect.TriggerReverse(time);
			break;
		case Effects.PixelWindowToPixel:
			m_PixelWindowEffect.TriggerStart(time);
			break;
		case Effects.PixelWindowToFullyClear:
			m_PixelWindowEffect.TriggerReverse(time);
			break;
		case Effects.DistortIn:
			break;
		case Effects.DistortOut:
			break;
		}
	}

	public void ResetAllEffects()
	{
		if (m_FadeEffect != null)
		{
			m_FadeEffect.Reset();
		}
		if (m_LetterboxEffect != null)
		{
			m_LetterboxEffect.Reset();
		}
		if (m_PixelWindowEffect != null)
		{
			m_PixelWindowEffect.Reset();
		}
		if (m_DistortEffect != null)
		{
			m_DistortEffect.Reset();
		}
	}
}
