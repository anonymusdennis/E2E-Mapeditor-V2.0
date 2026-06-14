using UnityEngine;

public class T17_RawImage_UVScroller : MonoBehaviour
{
	[Range(-1f, 1f)]
	public float m_HorizontalSpeed;

	[Range(-1f, 1f)]
	public float m_VerticalSpeed;

	private T17RawImage m_RawImage;

	private Rect m_UVRect;

	private Vector2 m_ScrollPosition = default(Vector2);

	private void Start()
	{
		m_RawImage = GetComponent<T17RawImage>();
		if (m_RawImage == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (m_RawImage != null)
		{
			m_UVRect = m_RawImage.uvRect;
			m_ScrollPosition = m_UVRect.position;
			m_ScrollPosition.x += m_HorizontalSpeed * Time.deltaTime;
			m_ScrollPosition.y += m_VerticalSpeed * Time.deltaTime;
			m_UVRect.position = m_ScrollPosition;
			m_RawImage.uvRect = m_UVRect;
		}
	}
}
