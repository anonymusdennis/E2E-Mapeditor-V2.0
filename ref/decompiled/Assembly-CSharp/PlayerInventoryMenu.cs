using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BaseInventoryBehaviour))]
public class PlayerInventoryMenu : BaseMenuBehaviour
{
	public Text m_InventoryTitle;

	public Text m_MoneyLabel;

	private InventoryItem[] m_InventoryItems;

	private ItemContainer m_CurrentContainer;

	private ItemContainer m_AlternatedHiddenContainer;

	private BaseInventoryBehaviour m_InventoryBehaviour;

	private int m_CurrentSelectedObjectIndex = -1;

	private List<Item> m_PreviousItems = new List<Item>();

	public const int EQUIPED_ITEM_INDEX = 0;

	public int CurrentHighlightedItemIndex => m_CurrentSelectedObjectIndex;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_InventoryItems == null)
		{
			m_InventoryItems = GetComponentsInChildren<InventoryItem>(includeInactive: true);
		}
		SetAlphaOfAllInventoryObjectsTo(1f);
		m_InventoryBehaviour = GetComponent<BaseInventoryBehaviour>();
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_CurrentContainer != null)
		{
			m_CurrentContainer.GetItems(ref m_PreviousItems);
		}
		DisableSmokesOnInventoryItems(m_InventoryItems);
		m_CurrentContainer = null;
		m_AlternatedHiddenContainer = null;
		ClearInventoryData();
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentRewiredPlayer == null || !(m_CurrentContainer != null) || !(m_AlternatedHiddenContainer != null) || !base.CurrentRewiredPlayer.GetButtonDown("UI_PutIntoHidden"))
		{
			return;
		}
		int num = m_CurrentSelectedObjectIndex;
		int num2 = -1;
		if (base.CachedEventSystem != null && !T17RewiredStandaloneInputModule.IsControllerDrivingInput(base.CurrentGamer))
		{
			GameObject currentMouseOverGO = base.CachedEventSystem.GetCurrentPointerOverGameobject();
			num2 = m_InventoryItems.FindIndex((InventoryItem item) => item.gameObject == currentMouseOverGO);
			num = num2;
		}
		switch (num)
		{
		case 0:
			base.CurrentGamePlayer.m_NetView.RPC("RPC_MASTER_PutEquipedItemIntoContainer", NetTargets.MasterClient, base.CurrentGamePlayer.m_NetView.viewID, m_AlternatedHiddenContainer.GetObjectNetID(), true);
			return;
		case -1:
			return;
		}
		Item item2 = m_CurrentContainer.GetItem(num - 1);
		if (item2 != null)
		{
			base.CurrentGamePlayer.m_bPendingRequest = true;
			base.CurrentGamePlayer.m_NetView.RPC("RPC_MASTER_SelectInventoryItem", NetTargets.MasterClient, m_CurrentContainer.GetObjectNetID(), m_AlternatedHiddenContainer.GetObjectNetID(), item2.m_NetView.viewID, false, true);
		}
	}

	private void InventoryItemWasSelected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		if (eventSystemForGamer != null && representationIndex >= 0 && representationIndex < m_InventoryItems.Length)
		{
			GameObject currentSelectedGameObject = eventSystemForGamer.currentSelectedGameObject;
			if (m_InventoryItems[representationIndex].gameObject == currentSelectedGameObject)
			{
				m_CurrentSelectedObjectIndex = representationIndex;
			}
		}
	}

	private void InventoryItemWasDeselected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
	}

	private void InventoryItemClicked(int index)
	{
		if (!(base.CurrentGamePlayer != null) || !(base.CurrentGamePlayer.m_ItemContainer != null))
		{
			return;
		}
		Item item = base.CurrentGamePlayer.m_ItemContainer.GetItem(index);
		if (item != null)
		{
			if (item.OutfitData != null)
			{
				base.CurrentGamePlayer.SetOutFit(item);
			}
			else
			{
				base.CurrentGamePlayer.SetEquippedItem(item);
			}
		}
	}

	public void SetInventoryTitle(string title)
	{
		if (m_InventoryTitle != null)
		{
			if (string.IsNullOrEmpty(title))
			{
				m_InventoryTitle.gameObject.SetActive(value: false);
				return;
			}
			m_InventoryTitle.gameObject.SetActive(value: true);
			m_InventoryTitle.text = title;
		}
	}

	public void SetInventoryMoney(float value)
	{
		if (m_MoneyLabel != null)
		{
			string text = value.ToString("F0");
			if (string.IsNullOrEmpty(text))
			{
				m_MoneyLabel.gameObject.SetActive(value: false);
				return;
			}
			m_MoneyLabel.gameObject.SetActive(value: true);
			m_MoneyLabel.text = text;
		}
	}

	public void PopulateWithItemContainer(ref ItemContainer container, Player player, bool firstTimeInit)
	{
		if (!HasPerformedFirstTimeInitialise())
		{
			DoSingleTimeInitialize();
		}
		int currentSelectedObjectIndex = m_CurrentSelectedObjectIndex;
		ClearInventoryData();
		if (currentSelectedObjectIndex != -1)
		{
			m_CurrentSelectedObjectIndex = currentSelectedObjectIndex;
		}
		m_CurrentContainer = container;
		if (container == null || m_InventoryItems == null)
		{
			if (container == null)
			{
			}
			if (m_InventoryItems != null)
			{
			}
			return;
		}
		int num = m_InventoryItems.Length;
		int num2 = 0;
		if (player != null)
		{
			player.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Remove(player.OnEquipedItemChanged, new Character.CharacterEvent(RefreshAllSlotsWithCurrentContainer));
			player.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Combine(player.OnEquipedItemChanged, new Character.CharacterEvent(RefreshAllSlotsWithCurrentContainer));
		}
		for (int i = 0; i < num; i++)
		{
			if (m_InventoryItems[i] == null)
			{
				continue;
			}
			m_InventoryItems[i].ResetBackgroundColor();
			Item item = null;
			if (i == 0)
			{
				num2 = -1;
				if (player != null)
				{
					item = player.GetEquippedItem();
				}
			}
			else
			{
				num2 = i - 1;
				item = container.GetItem(num2);
			}
			m_InventoryItems[i].SetIndexOfRepresentation(i);
			InventoryItem obj = m_InventoryItems[i];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			InventoryItem obj2 = m_InventoryItems[i];
			obj2.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			InventoryItem obj3 = m_InventoryItems[i];
			obj3.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj3.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
			InventoryItem obj4 = m_InventoryItems[i];
			obj4.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj4.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
			if (item != null)
			{
				if (!(item.m_ItemData == null))
				{
					m_InventoryItems[i].SetItemContentImage(item.m_ItemData.m_ItemUIImage);
					m_InventoryItems[i].SetItem(item);
				}
				int index = num2;
				if (!(m_InventoryBehaviour != null) || !(m_InventoryItems[i].InteractableElement != null))
				{
					continue;
				}
				m_InventoryItems[i].InteractableElement.onClick.RemoveAllListeners();
				m_InventoryItems[i].InteractableElement.onClick.AddListener(delegate
				{
					if (index == -1)
					{
						m_InventoryBehaviour.OnEquipedItemClicked();
					}
					else if (!m_InventoryBehaviour.OnItemClicked(index))
					{
						InventoryItemClicked(index);
					}
				});
			}
			else
			{
				m_InventoryItems[i].StopSmoke();
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_InventoryItems, m_PreviousItems);
		}
		m_CurrentContainer.GetItems(ref m_PreviousItems);
		if (player != null && player.GetEquippedItem() != null && m_PreviousItems != null)
		{
			m_PreviousItems.Add(player.GetEquippedItem());
		}
	}

	public void SetAlternateHiddenContainer(ref ItemContainer container)
	{
		m_AlternatedHiddenContainer = container;
	}

	public BaseInventoryBehaviour GetInventoryBehaviour()
	{
		return m_InventoryBehaviour;
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
		if (m_CurrentContainer != null)
		{
			PopulateWithItemContainer(ref m_CurrentContainer, base.CurrentGamePlayer, firstTimeInit: false);
		}
	}

	public void ClearInventoryData()
	{
		m_CurrentSelectedObjectIndex = -1;
		for (int i = 0; i < m_InventoryItems.Length; i++)
		{
			m_InventoryItems[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
			m_InventoryItems[i].OnItemSelected = null;
			m_InventoryItems[i].OnItemDeselected = null;
			if (m_InventoryItems[i].InteractableElement != null)
			{
				m_InventoryItems[i].InteractableElement.onClick.RemoveAllListeners();
			}
		}
	}

	public void SetLinkFromInventoryToObject(Selectable objectToGoTo, int inventorySlot)
	{
		if (inventorySlot >= 0 && inventorySlot < m_InventoryItems.Length && m_InventoryItems[inventorySlot].InteractableElement != null)
		{
			Navigation navigation = m_InventoryItems[inventorySlot].InteractableElement.navigation;
			navigation.selectOnUp = objectToGoTo;
			m_InventoryItems[inventorySlot].InteractableElement.navigation = navigation;
		}
	}

	public Selectable GetLinkToInventoryObject(int inventorySlot)
	{
		if (inventorySlot >= 0 && inventorySlot < m_InventoryItems.Length && m_InventoryItems[inventorySlot].InteractableElement != null)
		{
			return m_InventoryItems[inventorySlot].InteractableElement;
		}
		return null;
	}

	public void SetInventoryItemSetups(InventoryItem.ContainerSetup setup)
	{
		for (int i = 0; i < m_InventoryItems.Length; i++)
		{
			m_InventoryItems[i].ChangeDisplaySetup(setup);
		}
	}

	public void SetAlphaOfLinkedInventoryItem(Item theItem, float alpha)
	{
		for (int i = 0; i < m_InventoryItems.Length; i++)
		{
			if (m_InventoryItems[i].GetLinkedItem() == theItem)
			{
				SetAlphaOfInventoryItem(m_InventoryItems[i], alpha);
				break;
			}
		}
	}

	public void SetAlphaOfInventoryItem(InventoryItem inventoryItem, float alpha)
	{
		Color color = inventoryItem.m_ContentObject.color;
		color.a = alpha;
		inventoryItem.m_ContentObject.color = color;
	}

	public void SetAlphaOfAllInventoryObjectsTo(float value)
	{
		for (int i = 0; i < m_InventoryItems.Length; i++)
		{
			SetAlphaOfInventoryItem(m_InventoryItems[i], value);
		}
	}
}
