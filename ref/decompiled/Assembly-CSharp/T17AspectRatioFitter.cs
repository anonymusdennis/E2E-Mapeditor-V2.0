using UnityEngine;
using UnityEngine.UI;

public class T17AspectRatioFitter : AspectRatioFitter
{
	private float m_CurrentResolutionAspect;

	public Vector2 m_Size = Vector2.zero;

	public Vector2 m_Anchor = Vector2.zero;

	public Vector2 m_MinOffset = Vector2.zero;

	public Vector2 m_MaxOffset = Vector2.zero;

	public bool m_PosCached;

	private void Update()
	{
		CheckAspect();
	}

	private void CheckAspect()
	{
		if (base.aspectMode != AspectMode.HeightControlsWidth && base.aspectMode != AspectMode.WidthControlsHeight && base.aspectMode != 0)
		{
			return;
		}
		if (base.aspectMode == AspectMode.None && !m_PosCached)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null)
			{
				m_Size = rectTransform.sizeDelta;
				m_Anchor = rectTransform.anchoredPosition;
				m_MinOffset = rectTransform.anchorMin;
				m_MaxOffset = rectTransform.anchorMax;
				m_CurrentResolutionAspect = 0f;
				m_PosCached = true;
			}
		}
		float num = (float)Screen.width / (float)Screen.height;
		if (m_CurrentResolutionAspect == num)
		{
			return;
		}
		m_CurrentResolutionAspect = num;
		AspectMode aspectMode = AspectMode.None;
		aspectMode = ((!(num > 1.8f)) ? AspectMode.WidthControlsHeight : AspectMode.HeightControlsWidth);
		if (aspectMode == base.aspectMode)
		{
			return;
		}
		base.enabled = false;
		if (m_PosCached)
		{
			RectTransform rectTransform2 = base.transform as RectTransform;
			if (rectTransform2 != null)
			{
				rectTransform2.sizeDelta = m_Size;
				rectTransform2.anchoredPosition = m_Anchor;
				rectTransform2.anchorMin = m_MinOffset;
				rectTransform2.anchorMax = m_MaxOffset;
			}
		}
		base.aspectMode = aspectMode;
		base.enabled = true;
	}
}
