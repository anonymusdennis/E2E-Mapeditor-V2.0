using UnityEngine;

public class LevelEditor_CreateTooltip : MonoBehaviour
{
	public string m_TitleID = string.Empty;

	public string m_MessageID = string.Empty;

	public float m_DelayBeforeShowing;

	private float m_ToolTipTimer = -1f;

	private void Update()
	{
		if (m_ToolTipTimer >= 0f)
		{
			m_ToolTipTimer -= Time.deltaTime;
			if (m_ToolTipTimer < 0f)
			{
				DisplayToolTip();
			}
		}
	}

	public void MouseOver()
	{
		m_ToolTipTimer = m_DelayBeforeShowing;
	}

	public void MouseLeave()
	{
		HideToolTip();
	}

	private void HideToolTip()
	{
		m_ToolTipTimer = -1f;
		if (LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().HideToolTip();
		}
	}

	private void DisplayToolTip()
	{
		if (LevelEditor_UIController.GetInstance() != null)
		{
			LevelEditor_UIController.GetInstance().DisplayToolTip(m_TitleID, m_MessageID);
		}
	}
}
