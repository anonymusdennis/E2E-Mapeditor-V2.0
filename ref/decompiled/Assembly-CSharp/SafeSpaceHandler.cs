using UnityEngine;

public class SafeSpaceHandler : MonoBehaviour
{
	private RectTransform m_SafeSpaceTransform;

	private RectTransform m_OriginalSSTransform;

	public Vector2 m_SplitscreenAnchorMin = new Vector2(0.1f, 0.1f);

	public Vector2 m_SplitscreenAnchorMax = new Vector2(0.9f, 0.9f);

	private void Awake()
	{
		m_SafeSpaceTransform = base.gameObject.GetComponent<RectTransform>();
		if (m_SafeSpaceTransform != null)
		{
			m_OriginalSSTransform = m_SafeSpaceTransform;
		}
	}

	private void Start()
	{
	}

	public void SetSafeSpaceForSplitscreen()
	{
		m_SafeSpaceTransform.anchorMin = m_SplitscreenAnchorMin;
		m_SafeSpaceTransform.anchorMax = m_SplitscreenAnchorMax;
		m_SafeSpaceTransform.anchoredPosition = default(Vector2);
	}

	public void ResetSafeSpace()
	{
		m_SafeSpaceTransform = m_OriginalSSTransform;
	}
}
