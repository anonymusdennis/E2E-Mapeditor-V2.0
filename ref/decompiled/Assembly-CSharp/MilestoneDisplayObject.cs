using System;
using UnityEngine;

public class MilestoneDisplayObject : MonoBehaviour
{
	[Serializable]
	public class CriteriaDisplay
	{
		public Transform parent;

		public T17Text description;

		public T17Image tick;

		public T17Image key;
	}

	public T17Text m_MilestoneTitle;

	public T17Text m_MilestoneDescription;

	public CriteriaDisplay[] m_CriteriaDisplayObjects = new CriteriaDisplay[0];

	public T17Text m_RewardName;

	public T17Image m_RewardImage;

	public Sprite m_KeySprite;

	public Sprite m_KeyDisabledSprite;

	public void Reset()
	{
		if (m_MilestoneTitle != null)
		{
			m_MilestoneTitle.SetLocalisedTextCatchAll(string.Empty);
		}
		if (m_MilestoneDescription != null)
		{
			m_MilestoneDescription.SetLocalisedTextCatchAll(string.Empty);
		}
		if (m_RewardName != null)
		{
			m_RewardName.SetLocalisedTextCatchAll(string.Empty);
		}
		if (m_RewardImage != null)
		{
			m_RewardImage.sprite = null;
		}
		if (m_CriteriaDisplayObjects == null || m_CriteriaDisplayObjects.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < m_CriteriaDisplayObjects.Length; i++)
		{
			CriteriaDisplay criteriaDisplay = m_CriteriaDisplayObjects[i];
			criteriaDisplay.parent.gameObject.SetActive(value: true);
			if (criteriaDisplay.description != null)
			{
				criteriaDisplay.description.SetLocalisedTextCatchAll(string.Empty);
			}
			if (criteriaDisplay.tick != null)
			{
				criteriaDisplay.tick.enabled = false;
			}
			if (criteriaDisplay.key != null)
			{
				criteriaDisplay.key.sprite = m_KeyDisabledSprite;
			}
		}
	}

	public void ShowMilestone(ProgressMilestone milestone)
	{
		if (milestone == null)
		{
			Reset();
			return;
		}
		if (m_MilestoneTitle != null)
		{
			m_MilestoneTitle.SetLocalisedTextCatchAll(milestone.nameKey);
		}
		if (m_MilestoneDescription != null)
		{
			m_MilestoneDescription.SetLocalisedTextCatchAll(milestone.description);
		}
		if (m_RewardName != null)
		{
			m_RewardName.SetLocalisedTextCatchAll(milestone.rewardName);
		}
		if (m_RewardImage != null)
		{
			bool flag = false;
			ProgressManager instance = ProgressManager.GetInstance();
			if (instance != null)
			{
				flag = instance.GetMilestoneAchieved(milestone.id);
			}
			m_RewardImage.sprite = ((!flag) ? milestone.imageLocked : milestone.image);
		}
		if (m_CriteriaDisplayObjects == null || m_CriteriaDisplayObjects.Length <= 0)
		{
			return;
		}
		int num = Mathf.Min(milestone.criteria.Length, m_CriteriaDisplayObjects.Length);
		bool[] ruleStatuses = new bool[num];
		float[] statValues = new float[num];
		float[] refValues = new float[num];
		StatSystem instance2 = StatSystem.GetInstance();
		if (instance2 != null)
		{
			instance2.GetProgressDataForMilestone(milestone.id, ref ruleStatuses, ref statValues, ref refValues);
		}
		for (int i = 0; i < m_CriteriaDisplayObjects.Length; i++)
		{
			CriteriaDisplay criteriaDisplay = m_CriteriaDisplayObjects[i];
			if (i >= num)
			{
				criteriaDisplay.parent.gameObject.SetActive(value: false);
				continue;
			}
			ProgressMilestone.Criteria criteria = milestone.criteria[i];
			criteriaDisplay.parent.gameObject.SetActive(value: true);
			if (criteriaDisplay.description != null)
			{
				criteriaDisplay.description.SetLocalisedTextCatchAll(criteria.descriptionKey);
			}
			if (criteriaDisplay.tick != null)
			{
				criteriaDisplay.tick.enabled = ruleStatuses[i];
			}
			if (criteriaDisplay.key != null)
			{
				criteriaDisplay.key.sprite = null;
				criteriaDisplay.key.color = Color.clear;
			}
		}
	}
}
