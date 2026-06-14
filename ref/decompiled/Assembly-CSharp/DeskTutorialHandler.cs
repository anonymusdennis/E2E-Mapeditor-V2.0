using System;
using System.Collections.Generic;
using UnityEngine;

public class DeskTutorialHandler : IGMTutorialArrowHandler
{
	private List<ItemData> m_TargetItems;

	private DeskMenu m_Desk;

	private Transform m_TutorialTargetTransform;

	public override IGMTutorialArrowController.IGMTutorial GetTutorialType()
	{
		return IGMTutorialArrowController.IGMTutorial.DeskMenu;
	}

	public override void TutorialInit()
	{
		m_Desk = GetComponent<DeskMenu>();
	}

	public override bool IsActive()
	{
		return base.gameObject.activeInHierarchy;
	}

	public override Transform GetTutorialTargetTransform()
	{
		return m_TutorialTargetTransform;
	}

	private void OnDeskItemsChanged()
	{
		LocateTutorialTargetPosition();
	}

	public override void SetTutorialTarget(List<ItemData> targets)
	{
		m_TargetItems = targets;
		if (m_TargetItems != null && m_TargetItems.Count > 0)
		{
			DeskMenu desk = m_Desk;
			desk.OnDeskContainerRegistered = (DeskMenu.ContainerRegisteredEvent)Delegate.Combine(desk.OnDeskContainerRegistered, new DeskMenu.ContainerRegisteredEvent(RegisterDeskDelegates));
			DeskMenu desk2 = m_Desk;
			desk2.OnDeskContainerDeregistered = (DeskMenu.ContainerRegisteredEvent)Delegate.Combine(desk2.OnDeskContainerDeregistered, new DeskMenu.ContainerRegisteredEvent(DeregisterDeskDelegates));
			RegisterDeskDelegates();
			LocateTutorialTargetPosition();
		}
	}

	public override void ClearData()
	{
		m_TargetItems = null;
		m_TutorialTargetTransform = null;
		DeskMenu desk = m_Desk;
		desk.OnDeskContainerRegistered = (DeskMenu.ContainerRegisteredEvent)Delegate.Remove(desk.OnDeskContainerRegistered, new DeskMenu.ContainerRegisteredEvent(RegisterDeskDelegates));
		DeskMenu desk2 = m_Desk;
		desk2.OnDeskContainerDeregistered = (DeskMenu.ContainerRegisteredEvent)Delegate.Remove(desk2.OnDeskContainerDeregistered, new DeskMenu.ContainerRegisteredEvent(DeregisterDeskDelegates));
		DeregisterDeskDelegates();
	}

	private void LocateTutorialTargetPosition()
	{
		if (m_Desk != null && m_TargetItems != null && m_TargetItems.Count > 0)
		{
			for (int i = 0; i < m_TargetItems.Count; i++)
			{
				InventoryItem inventoryItem = m_Desk.FindItemInDesk(m_TargetItems[i]);
				if (inventoryItem != null)
				{
					m_TutorialTargetTransform = inventoryItem.transform;
					return;
				}
			}
		}
		m_TutorialTargetTransform = null;
	}

	private void RegisterDeskDelegates()
	{
		ItemContainer drawerContainer = m_Desk.GetDrawerContainer();
		if (drawerContainer != null)
		{
			drawerContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(drawerContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnDeskItemsChanged));
		}
		ItemContainer hiddenCompartmentContainer = m_Desk.GetHiddenCompartmentContainer();
		if (hiddenCompartmentContainer != null)
		{
			hiddenCompartmentContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(hiddenCompartmentContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnDeskItemsChanged));
		}
		LocateTutorialTargetPosition();
	}

	private void DeregisterDeskDelegates()
	{
		ItemContainer drawerContainer = m_Desk.GetDrawerContainer();
		if (drawerContainer != null)
		{
			drawerContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(drawerContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnDeskItemsChanged));
		}
		ItemContainer hiddenCompartmentContainer = m_Desk.GetHiddenCompartmentContainer();
		if (hiddenCompartmentContainer != null)
		{
			hiddenCompartmentContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(hiddenCompartmentContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnDeskItemsChanged));
		}
	}
}
