using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class TemporaryInventoryMenuBehaviour
{
	private Player CurrentGamePlayer;

	private InventoryItem[] m_InventoryItems;

	private PlayerInventoryMenu m_CurrentPlayerInventoryMenu;

	public TemporaryInventoryMenuBehaviour(Player player, InventoryItem[] inventoryItems, PlayerInventoryMenu currentPlayerInventoryMenu)
	{
		CurrentGamePlayer = player;
		m_InventoryItems = inventoryItems;
		m_CurrentPlayerInventoryMenu = currentPlayerInventoryMenu;
	}

	public void UpdatePlayerAndPlayerMenu(Player player, PlayerInventoryMenu currentPlayerInventoryMenu, bool bRefreshAlpha = true)
	{
		ClearItemSlots();
		RestorePlayerInventoryAlpha();
		CurrentGamePlayer = player;
		m_CurrentPlayerInventoryMenu = currentPlayerInventoryMenu;
		if (bRefreshAlpha)
		{
			RestorePlayerInventoryAlpha();
		}
	}

	public void RestorePlayerInventoryAlpha()
	{
		if (m_CurrentPlayerInventoryMenu != null)
		{
			m_CurrentPlayerInventoryMenu.SetAlphaOfAllInventoryObjectsTo(1f);
		}
	}

	public void OnInventoryItemClicked(ItemContainer container, int indexOfItem)
	{
		if (!(CurrentGamePlayer != null) || !(CurrentGamePlayer.m_ItemContainer != null))
		{
			return;
		}
		if (!CurrentGamePlayer.GetIsKnockedOut())
		{
			Item item = null;
			if (indexOfItem == -1)
			{
				if (CurrentGamePlayer != null)
				{
					item = CurrentGamePlayer.GetEquippedItem();
				}
			}
			else
			{
				item = CurrentGamePlayer.m_ItemContainer.GetItem(indexOfItem);
			}
			if (item.IsInUse())
			{
				return;
			}
			if (!IsItemAlreadyInInventory(item))
			{
				int i = 0;
				for (int num = m_InventoryItems.Length; i < num; i++)
				{
					if (m_InventoryItems[i] != null && m_InventoryItems[i].GetLinkedItem() == null)
					{
						SetItemForItemSlot(m_InventoryItems[i], item);
						break;
					}
				}
				return;
			}
			int j = 0;
			for (int num2 = m_InventoryItems.Length; j < num2; j++)
			{
				if (m_InventoryItems[j] != null && m_InventoryItems[j].GetLinkedItem() == item)
				{
					ClearSlotAndRestoreItemToPlayer(m_InventoryItems[j]);
					break;
				}
			}
		}
		else
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, AudioController.UI_Audio_GO);
		}
	}

	public void SetItemForItemSlot(InventoryItem slot, Item itemToUse)
	{
		if (m_InventoryItems.FindIndex((InventoryItem x) => x == slot) != -1)
		{
			slot.SetItemContentImage(itemToUse.m_ItemData.m_ItemUIImage);
			slot.SetItem(itemToUse);
			SetImageOnInventoryItem(ref slot, itemToUse.m_ItemData.m_ItemUIImage);
			if (m_CurrentPlayerInventoryMenu != null)
			{
				m_CurrentPlayerInventoryMenu.SetAlphaOfLinkedInventoryItem(itemToUse, 0.25f);
			}
		}
	}

	private void SetImageOnInventoryItem(ref InventoryItem itemslot, Sprite image, bool isFaded = false, bool isHidden = false)
	{
		if (!(image == null))
		{
			itemslot.SetItemContentImage(image, autoClearIfNull: true, autoDisableIfNull: true, isFaded, isHidden, resetVariation: false);
		}
	}

	private bool IsItemAlreadyInInventory(Item itemToCheck)
	{
		if (itemToCheck == null)
		{
			return false;
		}
		int i = 0;
		for (int num = m_InventoryItems.Length; i < num; i++)
		{
			if (m_InventoryItems[i].GetLinkedItem() == itemToCheck)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearSlot(int slotIndex)
	{
		if (slotIndex >= 0 && slotIndex < m_InventoryItems.Length)
		{
			ClearSlotAndRestoreItemToPlayer(m_InventoryItems[slotIndex]);
		}
	}

	private void ClearSlotAndRestoreItemToPlayer(InventoryItem slot)
	{
		if (m_CurrentPlayerInventoryMenu != null)
		{
			m_CurrentPlayerInventoryMenu.SetAlphaOfLinkedInventoryItem(slot.GetLinkedItem(), 1f);
		}
		slot.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
		slot.ResetBackgroundColor();
		slot.SetItem(null);
	}

	public ItemData[] GetItemDataCurrentlyInSlots()
	{
		ItemData[] array = new ItemData[m_InventoryItems.Length];
		int i = 0;
		for (int num = m_InventoryItems.Length; i < num; i++)
		{
			Item linkedItem = m_InventoryItems[i].GetLinkedItem();
			if (linkedItem != null)
			{
				array[i] = linkedItem.m_ItemData;
			}
		}
		return array;
	}

	public Item[] GetItemsCurrentlyInSlots(bool includeNulls = false)
	{
		Item[] array = new Item[m_InventoryItems.Length];
		int i = 0;
		for (int num = m_InventoryItems.Length; i < num; i++)
		{
			Item linkedItem = m_InventoryItems[i].GetLinkedItem();
			if (linkedItem != null || includeNulls)
			{
				array[i] = linkedItem;
			}
		}
		return array;
	}

	public void ClearItemSlots()
	{
		int i = 0;
		for (int num = m_InventoryItems.Length; i < num; i++)
		{
			if (m_InventoryItems[i] != null)
			{
				ClearSlotAndRestoreItemToPlayer(m_InventoryItems[i]);
			}
		}
	}

	public PlayerInventoryMenu GetLinkedMenu()
	{
		return m_CurrentPlayerInventoryMenu;
	}

	public bool HasQuestItems()
	{
		int i = 0;
		for (int num = m_InventoryItems.Length; i < num; i++)
		{
			if (m_InventoryItems[i] != null)
			{
				Item linkedItem = m_InventoryItems[i].GetLinkedItem();
				if (linkedItem != null && linkedItem.m_ItemData != null && linkedItem.IsQuestItem())
				{
					return true;
				}
			}
		}
		return false;
	}
}
