using System;
using System.Collections.Generic;

public class MailSortJob : CarriedInputJob
{
	[Serializable]
	public class PostBoxTagMap
	{
		public uint m_Tag;

		public ItemData m_ItemData;
	}

	public List<PostBoxTagMap> m_SortingMachineMaps = new List<PostBoxTagMap>();

	public int m_SortingMachineNumDispensedItems;

	private List<MailSortMachine> m_SortingMachines = new List<MailSortMachine>();

	private List<CarryableObjectConsumer> m_SortingMachineConsumers = new List<CarryableObjectConsumer>();

	private List<ItemContainer> m_Collectors = new List<ItemContainer>();

	private List<TransferItemsInteraction> m_CollectorTransferInteractions = new List<TransferItemsInteraction>();

	private List<ItemData> m_SortingMachineDispensedItems;

	protected override void OnDestroy()
	{
		for (int num = m_CollectorTransferInteractions.Count - 1; num >= 0; num--)
		{
			m_CollectorTransferInteractions[num].m_OnTransferComplete += OnDepositFinishedItemEvent;
		}
		for (int num2 = m_SortingMachineConsumers.Count - 1; num2 >= 0; num2--)
		{
			m_SortingMachineConsumers[num2].InputDroppedOnUsEvent -= CarryableConsumer_AcceptedInputEvent;
		}
		base.OnDestroy();
	}

	public override void Init(RoomBlob jobRoom)
	{
		m_SortingMachineDispensedItems = new List<ItemData>();
		for (int num = m_SortingMachineMaps.Count - 1; num >= 0; num--)
		{
			if (!m_SortingMachineDispensedItems.Contains(m_SortingMachineMaps[num].m_ItemData))
			{
				m_SortingMachineDispensedItems.Add(m_SortingMachineMaps[num].m_ItemData);
			}
		}
		if (m_SortingMachineDispensedItems.Count == 0)
		{
		}
		base.Init(jobRoom);
		SetupCollectors();
	}

	protected override void PreSetupDispenser()
	{
		base.PreSetupDispenser();
		SetupProcessors();
	}

	private void SetupProcessors()
	{
		m_SortingMachines.Clear();
		m_SortingMachineConsumers.Clear();
		for (int num = base.RoomData.m_Processors.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Processors[num];
			if (interactiveObject != null)
			{
				MailSortMachine component = interactiveObject.GetComponent<MailSortMachine>();
				if (component != null)
				{
					m_SortingMachines.Add(component);
					CarryableObjectConsumer component2 = interactiveObject.GetComponent<CarryableObjectConsumer>();
					m_SortingMachineConsumers.Add(component2);
					component2.InputDroppedOnUsEvent += CarryableConsumer_AcceptedInputEvent;
					component.m_PossibleItemsToDispense = m_SortingMachineDispensedItems;
					component.m_NumItemsToDispense = m_SortingMachineNumDispensedItems;
					component.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
					component.SetTransferItemTypes(m_SortingMachineDispensedItems.ToArray());
					if (component2.m_AcceptedTags.Count != 0)
					{
					}
					break;
				}
			}
		}
		if (m_SortingMachines.Count == 0)
		{
		}
		if (m_SortingMachineConsumers.Count != 0)
		{
		}
	}

	private void CarryableConsumer_AcceptedInputEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		for (int i = 0; i < m_CarriedObjectDispenser.Length; i++)
		{
			m_CarriedObjectDispenser[i].AddObjectBackToSpawnPool(theObject);
		}
	}

	protected void SetupCollectors()
	{
		m_Collectors.Clear();
		m_CollectorTransferInteractions.Clear();
		for (int i = 0; i < base.RoomData.m_Collectors.Count; i++)
		{
			if (base.RoomData.m_Collectors[i] == null)
			{
				continue;
			}
			ItemContainer component = base.RoomData.m_Collectors[i].GetComponent<ItemContainer>();
			if (component != null)
			{
				component.m_MaxSize = m_SortingMachineNumDispensedItems;
				component.m_MaxHiddenSize = 0;
				m_Collectors.Add(component);
			}
			TransferItemsInteraction transferInteraction = base.RoomData.m_Collectors[i] as TransferItemsInteraction;
			if (transferInteraction != null)
			{
				PostBoxTagMap postBoxTagMap = m_SortingMachineMaps.Find((PostBoxTagMap x) => x.m_Tag == transferInteraction.m_Tag);
				if (postBoxTagMap != null)
				{
					m_CollectorTransferInteractions.Add(transferInteraction);
					transferInteraction.SetTransferDirection(TransferItemsInteraction.TransferDirection.FromCharacter);
					transferInteraction.SetTransferItemTypes(new ItemData[1] { postBoxTagMap.m_ItemData });
					transferInteraction.m_bTransferEquippedItemsOnly = true;
					transferInteraction.SetCanBeUsedOutsideJobTime(m_bCanDispensersBeUsedOutsideJobTime);
					transferInteraction.m_OnTransferComplete += OnDepositFinishedItemEvent;
				}
			}
		}
		if (m_CollectorTransferInteractions.Count == 0)
		{
		}
		if (m_Collectors.Count != 0)
		{
		}
	}

	protected override List<uint> GetPossibleSpawnTags()
	{
		return CarryableObjectConsumer.GetAllPossibleSpawnTags(m_SortingMachineConsumers);
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		EmptyAllCollectors();
	}

	private void EmptyAllCollectors()
	{
		if (T17NetManager.OfflineMode || (T17NetManager.NetOnlineMode && T17NetManager.IsMasterClient))
		{
			for (int num = m_Collectors.Count - 1; num >= 0; num--)
			{
				m_Collectors[num].RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
			}
		}
	}

	private void OnDepositFinishedItemEvent(Item item, ItemContainer to, ItemContainer from)
	{
		if (item != null && to != null)
		{
			IncrementQuotaAchieved();
			to.RemoveAllItems(releaseToManager: true, exemptQuestItems: false);
		}
	}
}
