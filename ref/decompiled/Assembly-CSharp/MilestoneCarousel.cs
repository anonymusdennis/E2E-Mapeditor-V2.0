using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MilestoneCarousel : UICarousel<MilestonePage>
{
	[Header("Settings")]
	public T17Text m_PageHeaderLabel;

	public T17Text m_PageCountLabel;

	public T17GridLayoutGroup m_PageLayoutGroup;

	private MilestoneDisplayObject[] m_MilestoneDisplays = new MilestoneDisplayObject[0];

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void UpdateUIForSelectedIndex(int index)
	{
		MilestonePage milestonePage = m_Options[index];
		if (m_PageHeaderLabel != null)
		{
			m_PageHeaderLabel.SetLocalisedTextCatchAll(milestonePage.m_HeaderTag);
		}
		if (m_PageCountLabel != null)
		{
			int num = index + 1;
			int count = m_Options.Count;
			string text = string.Format("{0} / {1}", num.ToString("##"), count.ToString("##"));
			m_PageCountLabel.m_bNeedsLocalization = false;
			m_PageCountLabel.text = text;
		}
		for (int i = 0; i < m_MilestoneDisplays.Length; i++)
		{
			if (!(m_MilestoneDisplays[i] == null))
			{
				Object.Destroy(m_MilestoneDisplays[i].gameObject);
			}
		}
		m_MilestoneDisplays = SpawnDisplayObjects(milestonePage.m_MilestonePrefabs);
		for (int j = 0; j < m_MilestoneDisplays.Length; j++)
		{
			MilestoneDisplayObject milestoneDisplayObject = m_MilestoneDisplays[j];
			if (!(milestoneDisplayObject != null))
			{
				continue;
			}
			if (j >= milestonePage.m_Milestones.Count)
			{
				if (milestoneDisplayObject.gameObject != null)
				{
					milestoneDisplayObject.gameObject.SetActive(value: false);
				}
				continue;
			}
			if (milestoneDisplayObject.gameObject != null)
			{
				m_MilestoneDisplays[j].gameObject.SetActive(value: true);
			}
			m_MilestoneDisplays[j].ShowMilestone(milestonePage.m_Milestones[j]);
		}
	}

	private MilestoneDisplayObject[] SpawnDisplayObjects(List<GameObject> prefabs)
	{
		MilestoneDisplayObject[] array = null;
		if (m_PageLayoutGroup != null && prefabs.Count > 0)
		{
			int count = prefabs.Count;
			array = new MilestoneDisplayObject[count];
			for (int i = 0; i < count; i++)
			{
				if (!(prefabs[i] == null))
				{
					GameObject gameObject = Object.Instantiate(prefabs[i], m_PageLayoutGroup.transform);
					if (gameObject != null)
					{
						gameObject.transform.localScale = Vector3.one;
						array[i] = gameObject.GetComponent<MilestoneDisplayObject>();
					}
				}
			}
		}
		else
		{
			array = new MilestoneDisplayObject[0];
		}
		return array;
	}
}
