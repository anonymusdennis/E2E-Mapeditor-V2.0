using T17.UI.Carousel;
using UnityEngine;

public class UIAnimatedPagination : MonoBehaviour
{
	public UICarouselBase m_CarouselPage;

	public Animator m_PageAnimator;

	public CanvasAlphaChanger m_PageToFade;

	public string m_AnimateFromLeftTrigger = "SlideLeft";

	public string m_AnimateFromRightTrigger = "SlideRight";

	public float m_FadeTime = 0.2f;

	private SelectionDirections m_SelectedDirection;

	protected virtual void Awake()
	{
		if (m_CarouselPage == null || m_PageAnimator == null || m_PageToFade == null)
		{
		}
		m_CarouselPage.m_bDeferUpdatingUI = true;
		m_CarouselPage.IndexSelectedEvent += CarouselPage_IndexSelectedEvent;
		m_PageToFade.LerpFinishedEvent += CanvasFading_LerpFinishedEvent;
	}

	protected void OnDestroy()
	{
		m_CarouselPage.IndexSelectedEvent -= CarouselPage_IndexSelectedEvent;
		m_PageToFade.LerpFinishedEvent -= CanvasFading_LerpFinishedEvent;
	}

	private void CarouselPage_IndexSelectedEvent(int index, SelectionDirections directionTravelledIn)
	{
		m_CarouselPage.m_bBLockedInput = true;
		m_SelectedDirection = directionTravelledIn;
		m_PageToFade.FadeToTransparent(m_FadeTime);
	}

	private void CanvasFading_LerpFinishedEvent(TimedLerpingMonobehaviour sender)
	{
		m_PageToFade.ResetCanvas();
		m_CarouselPage.UpdateUI();
		switch (m_SelectedDirection)
		{
		case SelectionDirections.Next:
			m_PageAnimator.SetTrigger(m_AnimateFromRightTrigger);
			break;
		case SelectionDirections.Previous:
			m_PageAnimator.SetTrigger(m_AnimateFromLeftTrigger);
			break;
		default:
			AnimationFinished();
			break;
		}
	}

	private void AnimationFinished()
	{
		m_CarouselPage.m_bBLockedInput = false;
	}
}
