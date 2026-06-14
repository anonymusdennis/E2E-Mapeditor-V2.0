using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class ShopMenu : GameMenuBehaviour
{
	public GameObject m_ItemsParent;

	public T17Text[] m_ItemCostLabels;

	public T17Text m_VendorOpinionLabel;

	public T17Text m_OurMoneyLabel;

	public Animator m_OurMoneyLabelAnimator;

	public Animator m_OurMoneyAnimator;

	public Color m_PurchaseFontColour = Color.blue;

	public float m_CoinsPerSecond = 20f;

	public string m_MoneyLabelTriggerStart = "MoneyLarge";

	public string m_MoneyLabelTriggerStop = "MoneySmall";

	public string m_CoinStartTrigger = "CoinAnimate";

	public string m_CoinStopTrigger = "CoinIdle";

	private Color m_OriginalMoneyFontColour;

	private float m_TargetCoinValue;

	private float m_CurrentCoinValue;

	[Localization]
	public string m_VendorOpinionKeyPositive = string.Empty;

	[Localization]
	public string m_VendorOpinionKeyNegative = string.Empty;

	private static readonly Color32 m_VendorOpinionColorPositive = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static readonly Color32 m_VendorOpinionColorNegative = new Color32(152, 11, 0, byte.MaxValue);

	private InventoryItem[] m_InventoryItems;

	private BaseInventoryBehaviour m_InventoryBehaviour;

	private List<Item> m_PreviousItems = new List<Item>();

	private Vendor m_Vendor;

	private Player m_Player;

	private ItemContainer m_PlayerItemContainer;

	private bool m_bIsTickingDown;

	private static readonly Color m_DisabledButtonColour = new Color32(byte.MaxValue, 79, 95, byte.MaxValue);

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_ItemsParent != null)
		{
			m_InventoryItems = m_ItemsParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_InventoryBehaviour = m_ItemsParent.GetComponent<BaseInventoryBehaviour>();
		}
		m_InventoryBehaviour.SetItemClickToCallback();
		m_InventoryBehaviour.OnItemClickedEvent = OnVendorItemClicked;
		if (m_OurMoneyLabel != null)
		{
			m_OriginalMoneyFontColour = m_OurMoneyLabel.color;
		}
		ResetItems();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		bool success;
		Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(m_GameMenuInformation.m_MenuRepresentative, out success);
		SetupMenu(vendorForCharacter, m_GameMenuInformation.m_Player, m_GameMenuInformation.m_PlayerItemContainer);
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToCallback();
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
			BaseInventoryBehaviour playerInventoryBehaviour2 = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour2.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Combine(playerInventoryBehaviour2.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
		}
		ResetView();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
		}
		ResetView();
		if (!isTabSwitch)
		{
			ResetItems();
			if (m_Vendor != null && m_Vendor.GetItemContainer() != null)
			{
				ItemContainer itemContainer = m_Vendor.GetItemContainer();
				itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnVendorItemsChanged));
			}
			m_Vendor = null;
			m_Player = null;
			if (m_PlayerItemContainer != null)
			{
				m_PlayerItemContainer.GetItems(ref m_PreviousItems);
			}
			m_PlayerItemContainer = null;
			DisableSmokesOnInventoryItems(m_InventoryItems);
		}
		return true;
	}

	private void ResetView()
	{
		if (m_OurMoneyLabel != null)
		{
			m_OurMoneyLabel.color = m_OriginalMoneyFontColour;
		}
		if (m_Player != null)
		{
			m_TargetCoinValue = m_Player.m_CharacterStats.Money;
			m_CurrentCoinValue = m_TargetCoinValue;
			if (m_OurMoneyLabel != null)
			{
				m_OurMoneyLabel.SetNonLocalizedText(m_CurrentCoinValue.ToString("000"));
			}
		}
		if (m_OurMoneyLabelAnimator != null)
		{
			m_OurMoneyLabelAnimator.SetTrigger(m_MoneyLabelTriggerStop);
		}
		if (m_OurMoneyAnimator != null)
		{
			m_OurMoneyAnimator.SetTrigger(m_CoinStopTrigger);
		}
		m_bIsTickingDown = false;
	}

	private void OnInventoryItemClicked(ItemContainer container, int indexOfItem)
	{
	}

	public void SetupMenu(Vendor vendor, Player player, ItemContainer playerContainer)
	{
		m_Vendor = vendor;
		m_Player = player;
		m_PlayerItemContainer = playerContainer;
		ItemContainer itemContainer = vendor.GetItemContainer();
		itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnVendorItemsChanged));
		itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnVendorItemsChanged));
		m_InventoryBehaviour.SetItemContainerLinks(itemContainer, playerContainer, player);
		ResetView();
		RefreshUsable();
		RefreshItems(firstTimeInit: true);
	}

	private void DisplayItems(ItemContainer container, bool firstTimeInit, bool selectable = true)
	{
		if (container == null || m_InventoryItems == null)
		{
			return;
		}
		int num = Mathf.Min(container.GetItemCount(), m_InventoryItems.Length);
		for (int i = 0; i < num; i++)
		{
			Item item = container.GetItem(i);
			InventoryItem inventoryItem = m_InventoryItems[i];
			if (item == null || inventoryItem == null)
			{
				continue;
			}
			inventoryItem.ResetBackgroundColor();
			inventoryItem.SetItemContentImage(item.m_ItemData.m_ItemUIImage);
			inventoryItem.SetItem(item);
			inventoryItem.SetIndexOfRepresentation(i);
			Color32 backgroundColor = ((!selectable) ? m_DisabledButtonColour : Color.white);
			inventoryItem.SetBackgroundColor(backgroundColor);
			int index = i;
			if (m_InventoryBehaviour != null && inventoryItem.InteractableElement != null)
			{
				inventoryItem.InteractableElement.onClick.RemoveAllListeners();
				inventoryItem.InteractableElement.onClick.AddListener(delegate
				{
					m_InventoryBehaviour.OnItemClicked(index);
				});
			}
			if (i < m_ItemCostLabels.Length)
			{
				int modifiedItemCost = m_Vendor.GetModifiedItemCost(item);
				m_ItemCostLabels[i].SetNonLocalizedText(modifiedItemCost.ToString("000"));
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_InventoryItems, m_PreviousItems);
		}
		container.GetItems(ref m_PreviousItems);
	}

	private void OnVendorItemClicked(ItemContainer container, int indexOfItem)
	{
		if (m_Vendor == null || container != m_Vendor.GetItemContainer() || !m_Vendor.CanUseVendor(m_Player))
		{
			return;
		}
		float num = m_Player.m_CharacterStats.Money - (float)m_Vendor.GetModifiedItemCost(container.GetItem(indexOfItem));
		if (!m_Vendor.PurchaseItemRPC(indexOfItem, m_Player, m_PlayerItemContainer))
		{
			return;
		}
		if (m_TargetCoinValue != num)
		{
			m_TargetCoinValue = num;
			if (!m_bIsTickingDown)
			{
				m_bIsTickingDown = true;
				if (m_OurMoneyLabelAnimator != null)
				{
					m_OurMoneyLabelAnimator.ResetTrigger(m_MoneyLabelTriggerStop);
					m_OurMoneyLabelAnimator.SetTrigger(m_MoneyLabelTriggerStart);
				}
				if (m_OurMoneyLabel != null)
				{
					m_OurMoneyLabel.color = m_PurchaseFontColour;
				}
				if (m_OurMoneyAnimator != null)
				{
					m_OurMoneyAnimator.ResetTrigger(m_CoinStopTrigger);
					m_OurMoneyAnimator.SetTrigger(m_CoinStartTrigger);
				}
			}
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Purchase, base.gameObject);
	}

	private void OnVendorItemsChanged()
	{
		RefreshItems(firstTimeInit: false);
	}

	public void RefreshItems(bool firstTimeInit)
	{
		if (!(m_Vendor == null))
		{
			ResetItems();
			DisplayItems(m_Vendor.GetItemContainer(), firstTimeInit);
		}
	}

	public void RefreshUsable()
	{
		if (m_Player == null)
		{
			return;
		}
		bool flag = false;
		if (m_Vendor != null && m_Player != null)
		{
			flag = m_Vendor.CanUseVendor(m_Player);
		}
		if (m_VendorOpinionLabel != null)
		{
			string text = ((!flag) ? m_VendorOpinionKeyNegative : m_VendorOpinionKeyPositive);
			if (Localization.Get(text, out var localized))
			{
				Character character = m_Vendor.GetCharacter();
				localized = localized.Replace("$vendor", character.m_CharacterCustomisation.m_DisplayName);
				m_VendorOpinionLabel.m_bNeedsLocalization = false;
				m_VendorOpinionLabel.color = ((!flag) ? m_VendorOpinionColorNegative : m_VendorOpinionColorPositive);
				m_VendorOpinionLabel.text = localized;
			}
		}
	}

	private void ResetItems()
	{
		if (m_InventoryItems != null)
		{
			for (int i = 0; i < m_InventoryItems.Length; i++)
			{
				InventoryItem inventoryItem = m_InventoryItems[i];
				inventoryItem.SetItem(null);
				inventoryItem.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
				inventoryItem.SetIndexOfRepresentation(-1);
				if (inventoryItem.InteractableElement != null)
				{
					inventoryItem.InteractableElement.onClick.RemoveAllListeners();
				}
			}
		}
		for (int j = 0; j < m_ItemCostLabels.Length; j++)
		{
			m_ItemCostLabels[j].SetNonLocalizedText(string.Empty);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_TargetCoinValue != m_CurrentCoinValue)
		{
			m_CurrentCoinValue -= Time.deltaTime * m_CoinsPerSecond;
			if (m_CurrentCoinValue < m_TargetCoinValue)
			{
				m_CurrentCoinValue = m_TargetCoinValue;
				ResetView();
			}
			else if (m_OurMoneyLabel != null)
			{
				m_OurMoneyLabel.SetNonLocalizedText(Mathf.RoundToInt(m_CurrentCoinValue).ToString("D3"));
			}
		}
	}
}
