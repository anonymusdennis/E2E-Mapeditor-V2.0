using UnityEngine;

public class LevelEditor_ZoneInvalidIndicator : MonoBehaviour
{
	public BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.GroundFloor;

	public T17Text m_TextControl;

	public GameObject m_Indicator;

	private LevelEditor_ZoneManager m_ZoneManager;

	private bool m_CurrentlyDisplayed;

	private int m_CurrentTotal;

	private int m_PendingChange;

	private void Start()
	{
		if ((int)m_Layer < 1 || (int)m_Layer > 5 || m_Indicator == null)
		{
			base.enabled = false;
		}
		else
		{
			m_CurrentlyDisplayed = m_Indicator.activeSelf;
		}
	}

	private void Update()
	{
		if (!(m_ZoneManager != null) && !((m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) != null))
		{
			return;
		}
		int totalInvalidZonesInLayer = m_ZoneManager.GetTotalInvalidZonesInLayer(m_Layer);
		bool flag = totalInvalidZonesInLayer != 0;
		if (flag != m_CurrentlyDisplayed && ++m_PendingChange > 2)
		{
			m_CurrentlyDisplayed = flag;
			m_Indicator.SetActive(m_CurrentlyDisplayed);
			m_PendingChange = 0;
		}
		if (totalInvalidZonesInLayer != m_CurrentTotal)
		{
			m_CurrentTotal = totalInvalidZonesInLayer;
			if (m_TextControl != null)
			{
				m_TextControl.SetNonLocalizedText(m_CurrentTotal.ToString());
			}
		}
	}

	public void OnWarningButtonPressed()
	{
		LevelEditor_Controller instance = LevelEditor_Controller.GetInstance();
		if (instance != null)
		{
			instance.GotoBadZone(m_Layer);
		}
	}
}
