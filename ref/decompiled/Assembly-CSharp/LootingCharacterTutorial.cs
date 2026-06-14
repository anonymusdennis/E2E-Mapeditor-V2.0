using System;
using System.Collections.Generic;
using UnityEngine;

public class LootingCharacterTutorial : IGMTutorialArrowHandler
{
	private List<ItemData> m_TargetItems;

	private LootingMenu m_LootingMenu;

	private Transform m_TutorialTargetTransform;

	public override IGMTutorialArrowController.IGMTutorial GetTutorialType()
	{
		return IGMTutorialArrowController.IGMTutorial.LootingCharacter;
	}

	public override void TutorialInit()
	{
		m_LootingMenu = GetComponent<LootingMenu>();
	}

	public override bool IsActive()
	{
		return base.gameObject.activeInHierarchy;
	}

	public override Transform GetTutorialTargetTransform()
	{
		return m_TutorialTargetTransform;
	}

	private void OnCharacterItemsChanged()
	{
		LocateTutorialTargetPosition();
	}

	public override void SetTutorialTarget(List<ItemData> targets)
	{
		m_TargetItems = targets;
		LootingMenu lootingMenu = m_LootingMenu;
		lootingMenu.OnCharacterContainerRegistered = (LootingMenu.ContainerRegisteredEvent)Delegate.Combine(lootingMenu.OnCharacterContainerRegistered, new LootingMenu.ContainerRegisteredEvent(RegisterLootingDelegate));
		LootingMenu lootingMenu2 = m_LootingMenu;
		lootingMenu2.OnCharacterContainerDeregistered = (LootingMenu.ContainerRegisteredEvent)Delegate.Combine(lootingMenu2.OnCharacterContainerDeregistered, new LootingMenu.ContainerRegisteredEvent(DeregisterLootingDelegate));
		RegisterLootingDelegate();
	}

	public override void ClearData()
	{
		m_TargetItems = null;
		m_TutorialTargetTransform = null;
		LootingMenu lootingMenu = m_LootingMenu;
		lootingMenu.OnCharacterContainerRegistered = (LootingMenu.ContainerRegisteredEvent)Delegate.Remove(lootingMenu.OnCharacterContainerRegistered, new LootingMenu.ContainerRegisteredEvent(RegisterLootingDelegate));
		LootingMenu lootingMenu2 = m_LootingMenu;
		lootingMenu2.OnCharacterContainerDeregistered = (LootingMenu.ContainerRegisteredEvent)Delegate.Remove(lootingMenu2.OnCharacterContainerDeregistered, new LootingMenu.ContainerRegisteredEvent(DeregisterLootingDelegate));
		DeregisterLootingDelegate();
	}

	private void LocateTutorialTargetPosition()
	{
		if (m_LootingMenu != null && m_TargetItems != null && m_TargetItems.Count > 0)
		{
			for (int i = 0; i < m_TargetItems.Count; i++)
			{
				InventoryItem inventoryItem = m_LootingMenu.FindItemInContainer(m_TargetItems[i]);
				if (inventoryItem != null)
				{
					m_TutorialTargetTransform = inventoryItem.transform;
					return;
				}
			}
		}
		m_TutorialTargetTransform = null;
	}

	private void RegisterLootingDelegate()
	{
		if (m_TargetItems != null && m_TargetItems.Count > 0)
		{
			ItemContainer itemContainer = m_LootingMenu.ItemContainer;
			if (itemContainer != null)
			{
				itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnCharacterItemsChanged));
			}
			LocateTutorialTargetPosition();
		}
	}

	private void DeregisterLootingDelegate()
	{
		ItemContainer itemContainer = m_LootingMenu.ItemContainer;
		if (itemContainer != null)
		{
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnCharacterItemsChanged));
		}
	}
}
