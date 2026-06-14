using System;
using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class ProcessItemJob : BaseJob
{
	[Header("Dispenser Settings")]
	public ItemData[] m_DispensedItems;

	public int m_NumItemsPerDispenser = 1;

	public bool m_DispenserUsableOutsideJobTime = true;

	public bool m_InfintelyDispenseDuringJobTime;

	[Header("Processor Settings")]
	public ItemData[] m_ProcessorInputItems;

	public ItemData[] m_ProcessorOutputItems;

	public float m_ProcessingTime = 5f;

	public bool m_ProcessorUsableOutsideJobTime;

	public MinigameMasherSettingsContainer m_MinigameMasherContainer;

	[Header("Collector Settings")]
	public ItemData[] m_CollectedItems;

	public bool m_CollectorUsableOutsideJobTime;

	[Header("Other Settings")]
	public ItemData[] m_PreCraftItems;

	private List<ItemContainer> m_DispenserContainers = new List<ItemContainer>();

	private List<ItemContainer> m_CollectorContainers = new List<ItemContainer>();

	private List<ItemContainer> m_ProcessorContainers = new List<ItemContainer>();

	private List<ItemProcessorBase> m_Processors = new List<ItemProcessorBase>();

	private int m_DispensedItemIndex;

	private const int NUM_BITS_HEADER = 5;

	private const int NUM_BITS_FOR_DATA = 59;

	protected override void Awake()
	{
		base.Awake();
		if (JobsManager.GetJobCategory(m_Type) == JobCategory.ProcessItem)
		{
		}
	}

	protected override void OnDestroy()
	{
		if (RoutineManager.GetInstance() != null)
		{
			RoutineManager.GetInstance().OnRoutineChanged -= ProcessItemJob_OnRoutineChanged;
		}
		for (int num = m_DispenserContainers.Count - 1; num >= 0; num--)
		{
			ItemContainer itemContainer = m_DispenserContainers[num];
			itemContainer.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(itemContainer.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(Dispenser_OnItemRemoved));
		}
		for (int num2 = m_CollectorContainers.Count - 1; num2 >= 0; num2--)
		{
			if (m_CollectorContainers[num2] != null)
			{
				TransferItemsInteraction component = m_CollectorContainers[num2].GetComponent<TransferItemsInteraction>();
				component.m_OnTransferComplete -= OnDepositFinishedItemEvent;
			}
		}
		base.OnDestroy();
	}

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		if (base.RoomData != null)
		{
			for (int i = 0; i < base.RoomData.m_Dispensers.Count; i++)
			{
				if (!(base.RoomData.m_Dispensers[i] == null))
				{
					ItemContainer component = base.RoomData.m_Dispensers[i].GetComponent<ItemContainer>();
					if (component != null)
					{
						component.m_MaxSize = m_NumItemsPerDispenser;
						component.m_MaxHiddenSize = 0;
						component.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(component.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(Dispenser_OnItemRemoved));
						m_DispenserContainers.Add(component);
					}
					TransferItemsInteraction component2 = base.RoomData.m_Dispensers[i].GetComponent<TransferItemsInteraction>();
					if (component2 != null)
					{
						component2.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
						component2.SetTransferItemTypes(m_DispensedItems);
						component2.SetCanBeUsedOutsideJobTime(m_DispenserUsableOutsideJobTime);
					}
				}
			}
			for (int j = 0; j < base.RoomData.m_Collectors.Count; j++)
			{
				if (!(base.RoomData.m_Collectors[j] == null))
				{
					int maxSize = 10;
					ItemContainer component3 = base.RoomData.m_Collectors[j].GetComponent<ItemContainer>();
					if (component3 != null)
					{
						component3.m_MaxSize = maxSize;
						component3.m_MaxHiddenSize = 0;
						m_CollectorContainers.Add(component3);
					}
					TransferItemsInteraction transferItemsInteraction = base.RoomData.m_Collectors[j] as TransferItemsInteraction;
					if (transferItemsInteraction != null)
					{
						transferItemsInteraction.SetTransferDirection(TransferItemsInteraction.TransferDirection.FromCharacter);
						transferItemsInteraction.SetTransferItemTypes(m_CollectedItems);
						transferItemsInteraction.SetCanBeUsedOutsideJobTime(m_CollectorUsableOutsideJobTime);
						transferItemsInteraction.m_OnTransferComplete -= OnDepositFinishedItemEvent;
						transferItemsInteraction.m_OnTransferComplete += OnDepositFinishedItemEvent;
					}
				}
			}
			for (int k = 0; k < base.RoomData.m_Processors.Count; k++)
			{
				if (base.RoomData.m_Processors[k] == null)
				{
					continue;
				}
				ItemContainer component4 = base.RoomData.m_Processors[k].GetComponent<ItemContainer>();
				if (component4 != null)
				{
					component4.m_MaxSize = 2;
					m_ProcessorContainers.Add(component4);
				}
				ItemProcessorBase component5 = base.RoomData.m_Processors[k].GetComponent<ItemProcessorBase>();
				if (component5 != null)
				{
					SetProcessorInputItem(component5, m_ProcessorInputItems, m_ProcessorOutputItems);
					DelayedPassiveItemProcessor delayedPassiveItemProcessor = component5 as DelayedPassiveItemProcessor;
					if (delayedPassiveItemProcessor != null)
					{
						delayedPassiveItemProcessor.SetProcessingTime(m_ProcessingTime);
					}
					MasherItemProcessorInteraction component6 = component5.GetComponent<MasherItemProcessorInteraction>();
					if (component6 != null)
					{
						component6.m_MinigameSettingsContainer = m_MinigameMasherContainer;
					}
					m_Processors.Add(component5);
				}
				TransferItemsInteraction component7 = base.RoomData.m_Processors[k].GetComponent<TransferItemsInteraction>();
				if (component7 != null)
				{
					component7.SetTransferDirection(TransferItemsInteraction.TransferDirection.Invalid);
					component7.SetTransferItemTypes(null);
					component7.SetCanBeUsedOutsideJobTime(m_ProcessorUsableOutsideJobTime);
				}
			}
		}
		if (!PrisonSnapshotIO.IsThereSaveData())
		{
			FillDispensers();
		}
		RoutineManager.GetInstance().OnRoutineChanged += ProcessItemJob_OnRoutineChanged;
	}

	private void ProcessItemJob_OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.LightsOut || (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime && newRoutine.m_BaseRoutineType != Routines.JobTime))
		{
			for (int num = m_ProcessorContainers.Count - 1; num >= 0; num--)
			{
				m_ProcessorContainers[num].RemoveAllItems(releaseToManager: true, exemptQuestItems: true, bLeaveKeys: false, includeHidden: true);
			}
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			FillDispensers();
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		EmptyAllCollectors();
	}

	private void OnDepositFinishedItemEvent(Item item, ItemContainer to, ItemContainer from)
	{
		if (!(item != null) || !(to != null))
		{
			return;
		}
		bool flag = false;
		if (m_CollectedItems != null)
		{
			for (int i = 0; i < m_CollectedItems.Length; i++)
			{
				if (m_CollectedItems[i] != null && m_CollectedItems[i].m_ItemDataID == item.ItemDataID)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			IncrementQuotaAchieved();
		}
		to.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
	}

	private void FillDispensers()
	{
		if ((!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient)) || m_DispensedItems == null || m_DispensedItems.Length <= 0 || m_DispenserContainers.Count <= 0)
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
			for (int k = 0; k < m_DispensedItems.Length; k++)
			{
				if (!(itemData == null))
				{
					break;
				}
				itemData = m_DispensedItems[m_DispensedItemIndex];
				if (++m_DispensedItemIndex >= m_DispensedItems.Length)
				{
					m_DispensedItemIndex = 0;
				}
			}
			if (itemData == null)
			{
				break;
			}
			m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(0, itemData.m_ItemDataID, OnItemMgrResponseAddToDispenser, ref m_ImmediateItemMgrResponseID));
		}
	}

	private void EmptyAllCollectors()
	{
		if (!T17NetManager.OfflineMode && (!T17NetManager.NetOnlineMode || !T17NetManager.IsMasterClient))
		{
			return;
		}
		for (int i = 0; i < m_CollectorContainers.Count; i++)
		{
			if (m_CollectorContainers[i] != null)
			{
				m_CollectorContainers[i].RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
			}
		}
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
		if (T17NetManager.IsMasterClient && base.Employee != null && (!base.Employee.m_CharacterStats.m_bIsPlayer || m_InfintelyDispenseDuringJobTime) && container.GetItemCount() == 0 && IsJobActive())
		{
			FillDispensers();
		}
	}

	protected virtual void SetProcessorInputItem(ItemProcessorBase itemProcessor, ItemData[] inputItems, ItemData[] outputItems)
	{
		itemProcessor.SetInputOutputItemTypes(inputItems, outputItems);
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>();
		list.AddRange(m_DispensedItems);
		list.AddRange(m_ProcessorInputItems);
		list.AddRange(m_ProcessorOutputItems);
		list.AddRange(m_CollectedItems);
		list.AddRange(m_PreCraftItems);
		return list;
	}

	public override void Deserialize(ulong[] jobData)
	{
		base.Deserialize(jobData);
		for (int i = 1; i < jobData.Length; i++)
		{
			BitField bitField = new BitField(jobData[i]);
			int uInt = (int)bitField.GetUInt(5);
			for (int j = 0; j < uInt; j++)
			{
				ItemProcessorBase.Deserialise(bitField);
			}
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		List<ItemProcessorBase> list2 = m_Processors.FindAll((ItemProcessorBase x) => x.NeedsSaving());
		int num = list2.Count;
		if (num == 0)
		{
			return list;
		}
		while (num > 0)
		{
			BitField bitfield = new BitField();
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < num && num2 + list2[i].GetBitsPerEntry() < 59; i++)
			{
				num2 += list2[i].GetBitsPerEntry();
				num3++;
			}
			bitfield.Set(5, (uint)num3);
			for (int j = 0; j < num3; j++)
			{
				list2[j].Serialise(ref bitfield);
				num--;
			}
			list.Add((ulong)bitfield);
		}
		return list;
	}
}
