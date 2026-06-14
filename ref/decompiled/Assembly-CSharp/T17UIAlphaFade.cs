using UnityEngine;

public class T17UIAlphaFade : MonoBehaviour
{
	public LevelEditor_Controller m_Controller;

	public CanvasRenderer m_CanvasRenderer;

	public float m_CanvasAlphaMax;

	public float m_CanvasAlphaMin;

	private bool m_MouseOver;

	private float m_CurrentAlpha = -1f;

	private void Update()
	{
		float num = ((!m_MouseOver && !m_Controller.m_MarqueeBrushActive) ? m_CanvasAlphaMax : m_CanvasAlphaMin);
		if (num != m_CurrentAlpha)
		{
			m_CurrentAlpha = num;
			m_CanvasRenderer.SetAlpha(m_CurrentAlpha);
		}
	}

	public void SetMouseOver(bool bMouseOver)
	{
		m_MouseOver = bMouseOver;
	}
}
