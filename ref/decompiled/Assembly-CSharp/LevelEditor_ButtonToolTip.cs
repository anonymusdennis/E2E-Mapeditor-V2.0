using UnityEngine;

public class LevelEditor_ButtonToolTip : MonoBehaviour
{
	public T17Text m_Title;

	public Canvas m_Canvas;

	public Vector2 m_XClamp = Vector2.zero;

	public void Show(RectTransform callerTransform, string strTitle, bool bLeft)
	{
		base.gameObject.SetActive(value: true);
		if (callerTransform != null && m_Canvas != null)
		{
			if (bLeft)
			{
				if ((RectTransform)base.transform != null)
				{
					RectTransform rectTransform = (RectTransform)base.transform;
					Vector3 position = callerTransform.position + new Vector3(0f, callerTransform.rect.height * m_Canvas.scaleFactor * 0.5f, 0f);
					position.x = Mathf.Min(position.x, m_XClamp.y * m_Canvas.scaleFactor);
					base.transform.position = position;
				}
			}
			else
			{
				Vector3 position2 = callerTransform.position + new Vector3(0f, callerTransform.rect.height * m_Canvas.scaleFactor * 0.5f, 0f);
				position2.x = Mathf.Max(position2.x, m_XClamp.x * m_Canvas.scaleFactor);
				base.transform.position = position2;
			}
		}
		if (m_Title != null)
		{
			m_Title.SetLocalisedTextCatchAll(strTitle);
		}
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
