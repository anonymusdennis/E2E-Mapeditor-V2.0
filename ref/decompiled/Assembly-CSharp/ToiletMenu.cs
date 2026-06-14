using System;
using UnityEngine;

public class ToiletMenu : GameMenuBehaviour
{
	public GameObject m_BowlParent;

	public GameObject m_FlushParent;

	public T17Slider m_FlushSlider;

	public T17Text m_FlushButtonLabel;

	private InventoryItem[] m_InventoryItems;

	private BaseInventoryBehaviour m_InventoryBehaviour;

	private ToiletInteraction m_LinkedToilet;

	private TemporaryInventoryMenuBehaviour m_TemporaryToiletInventory;

	public static bool DEBUG_ALWAYS_CLOG;

	public static bool AlwaysClogToilet(bool bPos, bool bJustRead)
	{
		if (!bJustRead)
		{
			DEBUG_ALWAYS_CLOG = bPos;
		}
		return DEBUG_ALWAYS_CLOG;
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_BowlParent != null)
		{
			m_InventoryItems = m_BowlParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_InventoryBehaviour = m_BowlParent.GetComponent<BaseInventoryBehaviour>();
			m_TemporaryToiletInventory = new TemporaryInventoryMenuBehaviour(null, m_InventoryItems, null);
			m_TemporaryToiletInventory.ClearItemSlots();
		}
		ClearToilet();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToCallback();
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryToiletInventory.OnInventoryItemClicked));
			BaseInventoryBehaviour playerInventoryBehaviour2 = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour2.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Combine(playerInventoryBehaviour2.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryToiletInventory.OnInventoryItemClicked));
		}
		if (base.CurrentGamePlayer != null)
		{
			m_TemporaryToiletInventory.UpdatePlayerAndPlayerMenu(base.CurrentGamePlayer, InGameMenuFlow.Instance.GetInventoryMenu(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID));
		}
		ClearToilet();
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		ClearToilet();
		DisableSmokesOnInventoryItems(m_InventoryItems);
		m_LinkedToilet = null;
		if (m_FlushParent != null)
		{
			m_FlushParent.SetActive(value: false);
		}
		if (m_FlushButtonLabel != null)
		{
			m_FlushButtonLabel.gameObject.SetActive(value: true);
		}
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryToiletInventory.OnInventoryItemClicked));
		}
		m_TemporaryToiletInventory.RestorePlayerInventoryAlpha();
		return true;
	}

	protected override void Update()
	{
		base.Update();
		if (m_LinkedToilet != null && m_LinkedToilet.IsFlushing)
		{
			if (m_FlushSlider != null)
			{
				m_FlushSlider.value = 1f - m_LinkedToilet.FlushPercentage;
			}
		}
		else if (m_FlushParent != null && m_FlushParent.activeInHierarchy)
		{
			m_FlushParent.SetActive(value: false);
			if (m_FlushSlider != null)
			{
				m_FlushSlider.value = 1f;
			}
			if (m_FlushButtonLabel != null)
			{
				m_FlushButtonLabel.gameObject.SetActive(value: true);
			}
		}
	}

	public void FlushToilet()
	{
		if (m_LinkedToilet.IsFlushing || m_LinkedToilet.IsClogged || !(m_LinkedToilet != null))
		{
			return;
		}
		Item[] itemsCurrentlyInSlots = m_TemporaryToiletInventory.GetItemsCurrentlyInSlots();
		bool flag = m_TemporaryToiletInventory.HasQuestItems();
		m_TemporaryToiletInventory.ClearItemSlots();
		int num = UnityEngine.Random.Range(0, 100);
		bool flag2 = itemsCurrentlyInSlots.Length > 0;
		bool flag3 = m_LinkedToilet.MustFloodForPlayer(m_GameMenuInformation.m_Player);
		if (!flag && (DEBUG_ALWAYS_CLOG || ((flag3 || (float)num < m_LinkedToilet.m_FloodingChance) && flag2)))
		{
			int i = 0;
			for (int num2 = itemsCurrentlyInSlots.Length; i < num2; i++)
			{
				if (itemsCurrentlyInSlots[i] != null)
				{
					int viewID = itemsCurrentlyInSlots[i].m_NetView.viewID;
					if (base.CurrentGamePlayer.m_ItemContainer.HasSpecificItem(viewID))
					{
						base.CurrentGamePlayer.m_ItemContainer.MoveItemToAnotherContainerRPC(viewID, m_LinkedToilet.m_LinkedItemContainer.NetView.viewID);
					}
					else if (base.CurrentGamePlayer.GetEquippedItem() == itemsCurrentlyInSlots[i] && m_LinkedToilet.m_LinkedItemContainer.AddItemRPC(base.CurrentGamePlayer.GetEquippedItem()))
					{
						base.CurrentGamePlayer.SetEquippedItem(null, bTellOthers: true, bAddOldToItemContainer: false);
					}
				}
			}
			m_LinkedToilet.FloodToilet();
		}
		else
		{
			int[] array = new int[itemsCurrentlyInSlots.Length];
			int j = 0;
			for (int num3 = itemsCurrentlyInSlots.Length; j < num3; j++)
			{
				if (itemsCurrentlyInSlots[j] != null && itemsCurrentlyInSlots[j].m_NetView != null)
				{
					array[j] = itemsCurrentlyInSlots[j].m_NetView.viewID;
				}
				else
				{
					array[j] = -1;
				}
			}
			m_LinkedToilet.FlushToilet(array, base.CurrentGamePlayer.m_NetView.viewID);
			if (m_FlushParent != null)
			{
				m_FlushParent.SetActive(value: true);
			}
			if (m_FlushButtonLabel != null)
			{
				m_FlushButtonLabel.gameObject.SetActive(value: false);
			}
			if (m_FlushSlider != null)
			{
				m_FlushSlider.value = 1f;
			}
		}
		if (base.CurrentGamePlayer != null)
		{
			if (m_LinkedToilet.m_StatUponFlushing != STAT_IDS.NoneStat)
			{
				StatSystem.GetInstance().IncStat((int)m_LinkedToilet.m_StatUponFlushing, 1f, base.CurrentGamePlayer.m_Gamer, string.Empty);
			}
			base.CurrentGamePlayer.RequestStopInteraction();
			base.CurrentGamePlayer.RequestCloseContainer();
		}
	}

	public void ClearToiletSlot(int slotIndex)
	{
		m_TemporaryToiletInventory.ClearSlot(slotIndex - 1);
	}

	public void ClearToilet()
	{
		m_TemporaryToiletInventory.ClearItemSlots();
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
	}

	public BaseInventoryBehaviour GetToiletInventoryBehaviour()
	{
		return m_InventoryBehaviour;
	}

	public void SetToiletInteraction(ToiletInteraction interaction)
	{
		m_LinkedToilet = interaction;
		if (m_LinkedToilet.IsFlushing)
		{
			if (m_FlushParent != null)
			{
				m_FlushParent.SetActive(value: true);
			}
			if (m_FlushButtonLabel != null)
			{
				m_FlushButtonLabel.gameObject.SetActive(value: false);
			}
			if (m_FlushSlider != null)
			{
				m_FlushSlider.value = 1f - m_LinkedToilet.FlushPercentage;
			}
		}
	}
}
