using System;
using System.Collections.Generic;
using UnityEngine;

public class SwagBagMenu : GameMenuBehaviour
{
	public GameObject m_BagParent;

	private InventoryItem[] m_BagItems;

	private ItemContainer m_BagContainer;

	private BaseInventoryBehaviour m_BagInventoryBehaviour;

	private T17_PassThroughNavigation[] m_SwagPassThroughNavigations;

	private List<Item> m_PreviousItems = new List<Item>();

	private int m_CurrentSelectedObjectIndex = -1;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_BagParent != null)
		{
			m_BagItems = m_BagParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_BagInventoryBehaviour = m_BagParent.GetComponent<BaseInventoryBehaviour>();
		}
		ClearEntireSwagBag();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_SwagPassThroughNavigations != null)
		{
			for (int i = 0; i < m_SwagPassThroughNavigations.Length; i++)
			{
				m_SwagPassThroughNavigations[i].SetPassThrough();
			}
		}
		PopulateDrawerWithItemContainer(ref m_GameMenuInformation.m_MenuRepresentativeContainer, firstTimeInit: true);
		m_BagInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_Player);
		ItemContainer menuRepresentativeContainer = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(menuRepresentativeContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		ItemContainer menuRepresentativeContainer2 = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer2.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(menuRepresentativeContainer2.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToNothing();
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_Player);
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_BagContainer != null)
		{
			m_BagContainer.GetItems(ref m_PreviousItems);
		}
		m_BagContainer = null;
		ClearEntireSwagBag();
		DisableSmokesOnInventoryItems(m_BagItems);
		if (m_SwagPassThroughNavigations != null)
		{
			for (int i = 0; i < m_SwagPassThroughNavigations.Length; i++)
			{
				m_SwagPassThroughNavigations[i].RestorePassThrough();
			}
		}
		return true;
	}

	public void ClearEntireSwagBag()
	{
		ClearDrawer();
	}

	public void ClearDrawer()
	{
		m_CurrentSelectedObjectIndex = -1;
		if (m_BagItems == null)
		{
			return;
		}
		for (int i = 0; i < m_BagItems.Length; i++)
		{
			if (m_BagItems[i] != null)
			{
				m_BagItems[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
				m_BagItems[i].SetIndexOfRepresentation(-1);
				m_BagItems[i].OnItemSelected = null;
				m_BagItems[i].OnItemDeselected = null;
				if (m_BagItems[i].InteractableElement != null)
				{
					m_BagItems[i].InteractableElement.onClick.RemoveAllListeners();
				}
			}
		}
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
		if (m_BagContainer != null)
		{
			PopulateDrawerWithItemContainer(ref m_BagContainer, firstTimeInit: false);
		}
	}

	public void PopulateDrawerWithItemContainer(ref ItemContainer container, bool firstTimeInit)
	{
		int currentSelectedObjectIndex = m_CurrentSelectedObjectIndex;
		ClearEntireSwagBag();
		if (currentSelectedObjectIndex != -1)
		{
			m_CurrentSelectedObjectIndex = currentSelectedObjectIndex;
		}
		m_BagContainer = container;
		if (container == null || m_BagItems == null)
		{
			if (container == null)
			{
			}
			if (m_BagItems != null)
			{
			}
			return;
		}
		int num = m_BagItems.Length;
		for (int i = 0; i < num; i++)
		{
			m_BagItems[i].ResetBackgroundColor();
			m_BagItems[i].SetIndexOfRepresentation(i);
			InventoryItem obj = m_BagItems[i];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			InventoryItem obj2 = m_BagItems[i];
			obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
			Item item = container.GetItem(i);
			if (!(item != null))
			{
				continue;
			}
			m_BagItems[i].SetItemContentImage(item.m_ItemData.m_ItemUIImage);
			m_BagItems[i].SetItem(item);
			int index = i;
			if (m_BagInventoryBehaviour != null && m_BagItems[i].InteractableElement != null)
			{
				m_BagItems[i].InteractableElement.onClick.RemoveAllListeners();
				m_BagItems[i].InteractableElement.onClick.AddListener(delegate
				{
					m_BagInventoryBehaviour.OnItemClicked(index);
				});
			}
			if (m_BagInventoryBehaviour == null)
			{
			}
			if (!(m_BagItems[i].InteractableElement == null))
			{
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_BagItems, m_PreviousItems);
		}
		container.GetItems(ref m_PreviousItems);
	}

	protected override void Update()
	{
		base.Update();
	}

	private void InventoryItemWasSelected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		if (eventSystemForGamer != null && representationIndex >= 0 && representationIndex < m_BagItems.Length)
		{
			GameObject currentSelectedGameObject = eventSystemForGamer.currentSelectedGameObject;
			if (m_BagItems[representationIndex].gameObject == currentSelectedGameObject)
			{
				m_CurrentSelectedObjectIndex = representationIndex;
			}
		}
	}

	private void InventoryItemWasDeselected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
	}

	public BaseInventoryBehaviour GetBagInventoryBehaviour()
	{
		return m_BagInventoryBehaviour;
	}
}
