using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_RequirementsPopulator : MonoBehaviour
{
	public GameObject m_Grid;

	public GameObject m_Prefab;

	private LevelEditor_ZoneManager.Zone m_Zone;

	private List<LevelEditor_UIRequirement> m_UIButtons = new List<LevelEditor_UIRequirement>();

	public void SetZone(LevelEditor_ZoneManager.Zone zone)
	{
		if (object.ReferenceEquals(m_Zone, zone))
		{
			return;
		}
		m_Zone = zone;
		if (m_Zone == null || !(m_Grid != null) || !(m_Prefab != null))
		{
			return;
		}
		int i = 0;
		int num = m_Zone.m_ZoneDetails.m_Requirements.Length;
		for (int j = 0; j < num; j++)
		{
			if (m_Zone.m_ZoneDetails.m_Requirements[j].m_WhoFor != ZoneRequirement.WhoForEnum.Both && m_Zone.m_ZoneDetails.m_Requirements[j].m_WhoFor != 0)
			{
				continue;
			}
			if (i >= m_UIButtons.Count)
			{
				LevelEditor_UIRequirement levelEditor_UIRequirement = LevelEditor_UIRequirement.CreateUIRequirement(m_Prefab, m_Zone, j, m_Grid.transform);
				if (levelEditor_UIRequirement != null)
				{
					m_UIButtons.Add(levelEditor_UIRequirement);
					m_UIButtons[i++].m_VisualMaster.SetActive(value: true);
				}
			}
			else
			{
				m_UIButtons[i].SetZoneAndRequirement(m_Zone, j);
				m_UIButtons[i++].m_VisualMaster.SetActive(value: true);
			}
		}
		for (; i < m_UIButtons.Count; i++)
		{
			m_UIButtons[i].m_VisualMaster.SetActive(value: false);
		}
	}
}
