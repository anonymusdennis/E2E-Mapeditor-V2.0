using System.Collections.Generic;
using UnityEngine;

public class InventoryTutorialHandler : HUDTutorialArrowHandler
{
	private List<ItemData> m_TargetItems;

	private PlayerInventoryHUD m_InventoryHUD;

	private Transform m_TutorialTargetTransform;

	public override bool IsActive()
	{
		return m_InventoryHUD.m_ExpandParent.activeInHierarchy;
	}

	public override HUDTutorialArrowController.HUDTutorial GetTutorialType()
	{
		return HUDTutorialArrowController.HUDTutorial.ItemSelection;
	}

	public override void TutorialInit()
	{
		m_InventoryHUD = GetComponent<PlayerInventoryHUD>();
	}

	public override Transform GetTutorialTargetTransform()
	{
		LocateTutorialTargetPosition();
		return m_TutorialTargetTransform;
	}

	public override void ClearData()
	{
		m_TargetItems = null;
		m_TutorialTargetTransform = null;
		if (m_InventoryHUD.CurrentGamePlayer != null)
		{
			LocateTutorialTargetPosition();
		}
	}

	public override void SetTutorialTarget(List<ItemData> targets)
	{
		m_TargetItems = targets;
		if (m_TargetItems != null && m_TargetItems.Count > 0 && m_InventoryHUD.CurrentGamePlayer != null)
		{
			LocateTutorialTargetPosition();
		}
	}

	private void LocateTutorialTargetPosition()
	{
		if (m_InventoryHUD != null && m_TargetItems != null && m_TargetItems.Count > 0)
		{
			for (int i = 0; i < m_TargetItems.Count; i++)
			{
				InventoryItem inventoryItemForItem = m_InventoryHUD.GetInventoryItemForItem(m_TargetItems[i]);
				if (inventoryItemForItem != null)
				{
					m_TutorialTargetTransform = inventoryItemForItem.transform;
					return;
				}
			}
		}
		m_TutorialTargetTransform = null;
	}
}
