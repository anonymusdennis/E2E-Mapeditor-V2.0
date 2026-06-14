using UnityEngine;

public class IconData : MonoBehaviour
{
	private bool m_activeUp;

	private bool m_activeDown;

	public T17Image UpArrow;

	public T17Image DownArrow;

	public void SetActive(bool activeUp, bool activeDown)
	{
		if (m_activeUp != activeUp)
		{
			m_activeUp = activeUp;
			UpArrow.gameObject.SetActive(activeUp);
		}
		if (m_activeDown != activeDown)
		{
			m_activeDown = activeDown;
			DownArrow.gameObject.SetActive(activeDown);
		}
	}
}
