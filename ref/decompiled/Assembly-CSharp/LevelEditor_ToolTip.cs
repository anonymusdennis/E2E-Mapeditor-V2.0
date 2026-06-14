using UnityEngine;

public class LevelEditor_ToolTip : MonoBehaviour
{
	public class ToolTipData
	{
		public Vector2 m_Position = Vector2.zero;

		public Color m_TitleColor = Color.white;

		public string m_strTitle = string.Empty;

		public Color m_DescColor = Color.white;

		public string m_strDesc = string.Empty;

		public Color m_UsageColor = Color.white;

		public string m_strUsage = string.Empty;

		public float m_ExpireIn = 10f;

		public bool m_UpdatePositionAllowed = true;
	}

	public T17Text m_Title;

	public T17Text m_Desc;

	public T17Text m_Usage;

	private float m_ExpiryTime = -1f;

	private int m_iNextHandle;

	private int m_iCurrentHandle = -1;

	private bool m_UpdatePositionAllowed = true;

	private RectTransform m_MyRectTransform;

	private RectTransform m_MyParentTransform;

	private void Start()
	{
		m_MyRectTransform = base.transform as RectTransform;
	}

	private void Update()
	{
		m_ExpiryTime -= Time.deltaTime;
		if (m_ExpiryTime < 0f)
		{
			HideToolTip(m_iCurrentHandle);
		}
		else if (m_UpdatePositionAllowed)
		{
			UpdatePosition();
		}
	}

	public int DisplayToolTip(ref ToolTipData data)
	{
		m_iCurrentHandle = m_iNextHandle++;
		base.transform.localPosition = data.m_Position;
		if (m_Title != null)
		{
			m_Title.color = data.m_TitleColor;
			m_Title.SetNewLocalizationTag(data.m_strTitle);
		}
		if (m_Desc != null)
		{
			m_Desc.color = data.m_DescColor;
			m_Desc.SetNewLocalizationTag(data.m_strDesc);
		}
		if (m_Usage != null)
		{
			m_Usage.color = data.m_UsageColor;
			if (string.IsNullOrEmpty(data.m_strUsage))
			{
				m_Usage.SetNonLocalizedText(" ");
				m_Usage.gameObject.SetActive(value: false);
			}
			else
			{
				m_Usage.gameObject.SetActive(value: true);
				m_Usage.SetNonLocalizedText(data.m_strUsage);
			}
		}
		m_UpdatePositionAllowed = data.m_UpdatePositionAllowed;
		if (m_UpdatePositionAllowed)
		{
			UpdatePosition();
		}
		base.gameObject.SetActive(value: true);
		m_ExpiryTime = data.m_ExpireIn;
		return m_iCurrentHandle;
	}

	public void HideToolTip(int iHandle)
	{
		if (iHandle == m_iCurrentHandle)
		{
			m_iCurrentHandle = -1;
			m_ExpiryTime = -1f;
			base.gameObject.SetActive(value: false);
		}
	}

	public void UpdatePosition()
	{
		if (m_MyRectTransform == null)
		{
			m_MyRectTransform = base.transform as RectTransform;
		}
		if (m_MyParentTransform == null)
		{
			Canvas componentInParent = GetComponentInParent<Canvas>();
			if (componentInParent == null)
			{
				return;
			}
			m_MyParentTransform = componentInParent.transform as RectTransform;
		}
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null && m_MyRectTransform != null)
		{
			Vector2 rawMousePosition = instance.GetRawMousePosition();
			float num = 0f;
			float num2 = 0f;
			float num3 = Screen.width;
			float num4 = Screen.height;
			rawMousePosition = instance.GetRawMousePosition();
			float num5 = rawMousePosition.x / num3;
			float num6 = rawMousePosition.y / num4;
			float num7 = 40f / m_MyParentTransform.sizeDelta.y;
			float num8 = m_MyRectTransform.sizeDelta.x / m_MyParentTransform.sizeDelta.x + num7;
			float num9 = m_MyRectTransform.sizeDelta.y / m_MyParentTransform.sizeDelta.y + num7;
			num = ((!(num5 < 0.5f)) ? (num5 - num8 / 2f) : (num5 + num8 / 2f));
			num2 = ((!(num6 + num9 > 0.995f)) ? (num6 + num9 / 2f) : (num6 - num9 / 2f));
			rawMousePosition.x = (num - 0.5f) * m_MyParentTransform.sizeDelta.x;
			rawMousePosition.y = (num2 - 0.5f) * m_MyParentTransform.sizeDelta.y;
			m_MyRectTransform.anchoredPosition = rawMousePosition;
		}
	}
}
