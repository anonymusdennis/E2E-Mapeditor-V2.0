using UnityEngine;

public class LevelEditor_ValidZoneCard : LevelEditor_ZoneCard
{
	public GameObject m_WarningParent;

	public GameObject m_InsideWarning;

	public GameObject m_OutsideWarning;

	private LevelEditor_ZoneManager.Zone m_CurrentZone;

	private int m_CurrentZoneUpdateCount = -1;

	private void Start()
	{
		m_WarningParent.SetActive(value: false);
		m_InsideWarning.SetActive(value: false);
		m_OutsideWarning.SetActive(value: false);
	}

	private void Update()
	{
		if (m_CurrentZoneUpdateCount != m_CurrentZone.m_ZoneUpdateCount)
		{
			m_CurrentZoneUpdateCount = m_CurrentZone.m_ZoneUpdateCount;
			RefreshWarningMessages();
		}
	}

	public override void SetCardDataForZone(LevelEditor_ZoneManager.Zone newZone)
	{
		base.SetCardDataForZone(newZone);
		m_CurrentZone = newZone;
		m_CurrentZoneUpdateCount = m_CurrentZone.m_ZoneUpdateCount;
		RefreshWarningMessages();
	}

	public void RefreshWarningMessages()
	{
		if (m_CurrentZone.m_TotalInsideTiles > 0 && m_CurrentZone.m_TotalOutsideTiles > 0)
		{
			m_WarningParent.SetActive(value: true);
			if (m_CurrentZone.m_TotalInsideTiles >= m_CurrentZone.m_TotalOutsideTiles)
			{
				m_InsideWarning.SetActive(value: true);
				m_OutsideWarning.SetActive(value: false);
			}
			else
			{
				m_InsideWarning.SetActive(value: false);
				m_OutsideWarning.SetActive(value: true);
			}
		}
		else if (m_WarningParent.activeSelf)
		{
			m_WarningParent.SetActive(value: false);
		}
	}
}
