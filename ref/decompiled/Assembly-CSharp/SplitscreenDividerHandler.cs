using System;
using UnityEngine;

public class SplitscreenDividerHandler : MonoBehaviour
{
	public float m_LineThicknessPixels = 10f;

	public RectTransform m_HorizontalDivider;

	public RectTransform m_VerticalDivider;

	private void Awake()
	{
		CameraManager.CreatedEvent += CameraManager_CreatedEvent;
		CameraManager.DestroyedEvent += CameraManager_DestroyedEvent;
	}

	private void CameraManager_CreatedEvent(CameraManager sender)
	{
		sender.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(sender.OnTargetsUpdated, new CameraManager.CameraManagerHandler(SetScreenDividers));
	}

	private void CameraManager_DestroyedEvent(CameraManager sender)
	{
		sender.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(sender.OnTargetsUpdated, new CameraManager.CameraManagerHandler(SetScreenDividers));
		if (m_VerticalDivider != null)
		{
			m_VerticalDivider.gameObject.SetActive(value: false);
		}
		if (m_HorizontalDivider != null)
		{
			m_HorizontalDivider.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OnDestroy()
	{
		CameraManager.CreatedEvent -= CameraManager_CreatedEvent;
		CameraManager.DestroyedEvent -= CameraManager_DestroyedEvent;
	}

	private void SetScreenDividers()
	{
		if (m_HorizontalDivider == null || m_HorizontalDivider.gameObject == null || m_VerticalDivider == null || m_VerticalDivider.gameObject == null)
		{
			return;
		}
		int usedCameraCount = CameraManager.GetInstance().GetUsedCameraCount();
		Rect rect = ((RectTransform)base.gameObject.transform).rect;
		float num = 1f / rect.height * m_LineThicknessPixels;
		float num2 = 1f / rect.width * m_LineThicknessPixels;
		switch (usedCameraCount)
		{
		case 2:
			if (m_VerticalDivider != null)
			{
				m_VerticalDivider.anchorMin = new Vector2(0.5f - num2 / 2f, 0f);
				m_VerticalDivider.anchorMax = new Vector2(0.5f + num2 / 2f, 1f);
				m_VerticalDivider.anchoredPosition = Vector2.zero;
				m_VerticalDivider.gameObject.SetActive(value: true);
			}
			if (m_HorizontalDivider != null)
			{
				m_HorizontalDivider.gameObject.SetActive(value: false);
			}
			break;
		case 3:
			if (m_HorizontalDivider != null)
			{
				m_HorizontalDivider.anchorMin = new Vector2(0.5f, 0.5f - num / 2f);
				m_HorizontalDivider.anchorMax = new Vector2(1f, 0.5f + num / 2f);
				m_HorizontalDivider.anchoredPosition = Vector2.zero;
				m_HorizontalDivider.gameObject.SetActive(value: true);
			}
			if (m_VerticalDivider != null)
			{
				m_VerticalDivider.anchorMin = new Vector2(0.5f - num2 / 2f, 0f);
				m_VerticalDivider.anchorMax = new Vector2(0.5f + num2 / 2f, 1f);
				m_VerticalDivider.anchoredPosition = Vector2.zero;
				m_VerticalDivider.gameObject.SetActive(value: true);
			}
			break;
		case 4:
			if (m_HorizontalDivider != null)
			{
				m_HorizontalDivider.anchorMin = new Vector2(0f, 0.5f - num2 / 2f);
				m_HorizontalDivider.anchorMax = new Vector2(1f, 0.5f + num2 / 2f);
				m_HorizontalDivider.anchoredPosition = Vector2.zero;
				m_HorizontalDivider.gameObject.SetActive(value: true);
			}
			if (m_VerticalDivider != null)
			{
				m_VerticalDivider.anchorMin = new Vector2(0.5f - num2 / 2f, 0f);
				m_VerticalDivider.anchorMax = new Vector2(0.5f + num2 / 2f, 1f);
				m_VerticalDivider.anchoredPosition = Vector2.zero;
				m_VerticalDivider.gameObject.SetActive(value: true);
			}
			break;
		default:
			if (m_HorizontalDivider != null)
			{
				m_HorizontalDivider.gameObject.SetActive(value: false);
			}
			if (m_VerticalDivider != null)
			{
				m_VerticalDivider.gameObject.SetActive(value: false);
			}
			break;
		}
	}
}
