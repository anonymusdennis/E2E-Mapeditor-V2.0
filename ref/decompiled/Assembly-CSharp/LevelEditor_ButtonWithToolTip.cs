using UnityEngine;

public class LevelEditor_ButtonWithToolTip : MonoBehaviour
{
	public LevelEditor_ButtonToolTip m_ToolTip;

	public string m_ToolTipTitle = "Default Title";

	public bool m_Left;

	public void ShowToolTip()
	{
		if (m_ToolTip != null)
		{
			RectTransform rectTransform = (RectTransform)base.transform;
			if (rectTransform != null)
			{
				m_ToolTip.Show(rectTransform, m_ToolTipTitle, m_Left);
			}
		}
	}

	public void HideToolTip()
	{
		if (m_ToolTip != null)
		{
			m_ToolTip.Hide();
		}
	}
}
