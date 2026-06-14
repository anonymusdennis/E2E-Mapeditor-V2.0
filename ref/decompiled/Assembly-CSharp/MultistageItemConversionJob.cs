using System;
using System.Collections.Generic;
using UnityEngine;

public class MultistageItemConversionJob : BaseJob
{
	[Header("Dispenser Settings")]
	public int m_NumItemsPerDispenser = 1;

	public bool m_DispenserUsableOutsideJobTime = true;

	public bool m_InfintelyDispenseDuringJobTime;

	protected List<ItemContainer> m_DispenserContainers = new List<ItemContainer>();

	[Header("Converter Settings")]
	public List<MultistageItemConverter.ItemConverterConversions> m_InputOutputs;

	public bool m_CanConvertersBeUsedOutsideJobTime;

	public int m_MaxConverterInputs = 1;

	private List<ItemData> m_ValidInputs;

	private int m_DispensedItemIndex;

	protected override void Awake()
	{
		base.Awake();
		if (JobsManager.GetJobCategory(m_Type) != JobCategory.ProcessItem)
		{
			throw new Exception("The behaviour of this class is not applicable to this type of job");
		}
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		if (!(base.RoomData != null))
		{
			return;
		}
		m_ValidInputs = new List<ItemData>();
		for (int num = m_InputOutputs.Count - 1; num >= 0; num--)
		{
			m_ValidInputs.Add(m_InputOutputs[num].m_Input);
		}
		ItemData[] transferItemTypes = GetDispenserTransactionItems().ToArray();
		for (int num2 = base.RoomData.m_Dispensers.Count - 1; num2 >= 0; num2--)
		{
			if (!(base.RoomData.m_Dispensers[num2] == null))
			{
				ItemContainer component = base.RoomData.m_Dispensers[num2].GetComponent<ItemContainer>();
				if (component != null)
				{
					component.m_MaxSize = m_NumItemsPerDispenser;
					component.m_MaxHiddenSize = 0;
					component.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(component.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(Dispenser_OnItemRemoved));
					m_DispenserContainers.Add(component);
				}
				TransferItemsInteraction component2 = base.RoomData.m_Dispensers[num2].GetComponent<TransferItemsInteraction>();
				if (component2 != null)
				{
					component2.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
					component2.SetTransferItemTypes(transferItemTypes);
					component2.SetCanBeUsedOutsideJobTime(m_DispenserUsableOutsideJobTime);
				}
			}
		}
		for (int num3 = base.RoomData.m_Processors.Count - 1; num3 >= 0; num3--)
		{
			if (!(base.RoomData.m_Processors[num3] == null))
			{
				MultiStageTransferInteraction component3 = base.RoomData.m_Processors[num3].GetComponent<MultiStageTransferInteraction>();
				if (component3 != null)
				{
					component3.SetTransferDirection(TransferItemsInteraction.TransferDirection.Invalid);
					component3.SetTransferItemTypes(null);
					component3.SetCanBeUsedOutsideJobTime(m_CanConvertersBeUsedOutsideJobTime);
				}
				MultistageItemConverter component4 = base.RoomData.m_Processors[num3].GetComponent<MultistageItemConverter>();
				if (component3 != null)
				{
					component4.SetInputOutputItemTypes(m_InputOutputs.ToArray());
				}
				ItemContainer component5 = base.RoomData.m_Processors[num3].GetComponent<ItemContainer>();
				if (component5 != null)
				{
					component5.m_MaxSize = m_MaxConverterInputs * 2;
					component5.m_MaxHiddenSize = 0;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		for (int num = m_DispenserContainers.Count - 1; num >= 0; num--)
		{
			ItemContainer itemContainer = m_DispenserContainers[num];
			itemContainer.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(itemContainer.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(Dispenser_OnItemRemoved));
		}
		base.OnDestroy();
	}

	protected virtual List<ItemData> GetDispenserTransactionItems()
	{
		return m_ValidInputs;
	}

	protected virtual int CalculateDispenserMaxSize()
	{
		return m_NumItemsPerDispenser;
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			FillDispensers();
		}
	}

	protected void FillDispensers()
	{
		if (!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient))
		{
			return;
		}
		List<ItemData> dispenserContentsForFilling = GetDispenserContentsForFilling();
		if (dispenserContentsForFilling == null || dispenserContentsForFilling.Count <= 0 || m_DispenserContainers.Count <= 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_DispenserContainers.Count; i++)
		{
			num += m_DispenserContainers[i].GetFreeSpaceCount();
		}
		m_ItemMgrResponseIDs.Clear();
		for (int j = 0; j < num; j++)
		{
			ItemData itemData = null;
			for (int k = 0; k < dispenserContentsForFilling.Count; k++)
			{
				if (!(itemData == null))
				{
					break;
				}
				itemData = dispenserContentsForFilling[m_DispensedItemIndex];
				if (++m_DispensedItemIndex >= dispenserContentsForFilling.Count)
				{
					m_DispensedItemIndex = 0;
				}
			}
			if (itemData == null)
			{
				break;
			}
			SpawnItemIntoDispenser(itemData);
		}
	}

	protected void SpawnItemIntoDispenser(ItemData itemData)
	{
		m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(0, itemData.m_ItemDataID, OnItemMgrResponseAddToDispenser, ref m_ImmediateItemMgrResponseID));
	}

	public virtual List<ItemData> GetDispenserContentsForFilling()
	{
		return m_ValidInputs;
	}

	private void OnItemMgrResponseAddToDispenser(Item item, int eventID)
	{
		if (!(item != null) || (eventID != m_ImmediateItemMgrResponseID && !m_ItemMgrResponseIDs.Contains(eventID)))
		{
			return;
		}
		ItemContainer itemContainer = null;
		for (int i = 0; i < m_DispenserContainers.Count; i++)
		{
			if (m_DispenserContainers[i].GetFreeSpaceCount() > 0)
			{
				itemContainer = m_DispenserContainers[i];
				break;
			}
		}
		if (itemContainer != null)
		{
			if (!itemContainer.AddItemRPC(item))
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
		else
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	private void Dispenser_OnItemRemoved(ItemContainer container, Item item)
	{
		if (T17NetManager.IsMasterClient && m_InfintelyDispenseDuringJobTime && container.GetItemCount() == 0 && IsJobActive())
		{
			FillDispensers();
		}
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>();
		if (m_ValidInputs != null)
		{
			for (int num = m_ValidInputs.Count - 1; num >= 0; num--)
			{
				list.Add(m_InputOutputs[num].m_Input);
				list.Add(m_InputOutputs[num].m_Output);
			}
		}
		return list;
	}
}
