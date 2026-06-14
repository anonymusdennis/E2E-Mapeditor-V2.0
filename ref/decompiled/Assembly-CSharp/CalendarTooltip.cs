using UnityEngine;

public class CalendarTooltip : MonoBehaviour
{
	public T17Text m_DayName;

	public T17Text m_DayDescription;

	public RectTransform m_Rect;

	public float m_XOffset;

	public float m_YOffset;

	public void SetPositionWithOffset(Vector3 position)
	{
		base.transform.position = position;
		Vector2 anchoredPosition = m_Rect.anchoredPosition;
		anchoredPosition.x += m_XOffset;
		anchoredPosition.y += m_YOffset;
		m_Rect.anchoredPosition = anchoredPosition;
	}
}
