using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ServiceItemJob : ServiceCustomerViaProxyJob
{
	[Serializable]
	public class ItemOptionPODO
	{
		public List<ItemData> m_Ingredients;

		public ItemData m_FinishedProduct;

		public SpeechPODO m_NpcResponse;
	}

	[Header("ServiceItemJob")]
	public List<ItemOptionPODO> m_PossibleInputOutputs;

	[FormerlySerializedAs("m_NoFinishedFoodEquipped")]
	public SpeechPODO m_NoFinishedItemEquipped;

	public SpeechPODO m_PlayerSpeechWhenServing;

	public bool m_bGuidePlayerToServicePoint;

	[Header("Minigame Requirements")]
	public MinigameCompletionHelper m_MinigameSetup;

	protected List<ServiceItemInteractiveObject> m_ServiceItemObjects = new List<ServiceItemInteractiveObject>();

	protected Dictionary<ServiceCustomer, ServiceItemInteractiveObject> m_ServiceMaps = new Dictionary<ServiceCustomer, ServiceItemInteractiveObject>();

	private List<TransferItemsInteraction> m_Dispensers = new List<TransferItemsInteraction>();

	public int m_NumSetsOfItemsDispensed = 1;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		if (!(base.RoomData != null))
		{
			return;
		}
		List<ItemData> list = new List<ItemData>();
		List<ItemData> list2 = new List<ItemData>();
		for (int num = m_PossibleInputOutputs.Count - 1; num >= 0; num--)
		{
			ItemOptionPODO itemOptionPODO = m_PossibleInputOutputs[num];
			if (itemOptionPODO != null)
			{
				list.AddRange(itemOptionPODO.m_Ingredients);
				list2.Add(itemOptionPODO.m_FinishedProduct);
			}
			else
			{
				m_PossibleInputOutputs.RemoveAt(num);
			}
		}
		ItemData[] transferItemTypes = list.ToArray();
		ItemData[] transferItemTypes2 = list2.ToArray();
		for (int num2 = base.RoomData.m_Dispensers.Count - 1; num2 >= 0; num2--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Dispensers[num2];
			if (interactiveObject != null)
			{
				TransferItemsInteraction component = interactiveObject.GetComponent<TransferItemsInteraction>();
				if (component != null)
				{
					component.SetTransferDirection(TransferItemsInteraction.TransferDirection.ToCharacter);
					component.SetTransferItemTypes(transferItemTypes);
					component.GetItemContainer().m_MaxSize = 12;
					ItemContainer itemContainer = component.GetItemContainer();
					itemContainer.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(itemContainer.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(OnDispenserItemRemoved));
					m_Dispensers.Add(component);
				}
			}
		}
		for (int num3 = base.RoomData.m_Collectors.Count - 1; num3 >= 0; num3--)
		{
			InteractiveObject interactiveObject2 = base.RoomData.m_Collectors[num3];
			if (interactiveObject2 != null)
			{
				ServiceItemInteractiveObject component2 = interactiveObject2.GetComponent<ServiceItemInteractiveObject>();
				if (component2 != null)
				{
					component2.SetTransferDirection(TransferItemsInteraction.TransferDirection.FromCharacter);
					component2.SetTransferItemTypes(transferItemTypes2);
					component2.GetItemContainer().m_MaxSize = 2;
					component2.m_NoEquippedItemSpeech = m_NoFinishedItemEquipped;
					component2.m_PlayerSpeechWhenServing = m_PlayerSpeechWhenServing;
					component2.LinkToJob(this);
					component2.SetCanBeUsedOutsideJobTime(canBeUsed: false);
					m_ServiceItemObjects.Add(component2);
					ServiceCustomer component3 = component2.GetComponent<ServiceCustomer>();
					component3.LinkToJob(this);
					if (!m_ServiceMaps.ContainsKey(component3))
					{
						m_ServiceMaps.Add(component3, component2);
					}
				}
			}
		}
	}

	protected override List<ItemData> OneTimeCalculateJobRelatedItems()
	{
		List<ItemData> list = new List<ItemData>();
		int i = 0;
		for (int count = m_PossibleInputOutputs.Count; i < count; i++)
		{
			list.Add(m_PossibleInputOutputs[i].m_FinishedProduct);
			list.AddRange(m_PossibleInputOutputs[i].m_Ingredients);
		}
		return list;
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore && T17NetManager.IsMasterClient)
		{
			for (int num = m_Dispensers.Count - 1; num >= 0; num--)
			{
				RefillItemContainer();
			}
			for (int num2 = m_ServiceItemObjects.Count - 1; num2 >= 0; num2--)
			{
				m_ServiceItemObjects[num2].GetItemContainer().RemoveAllItems(releaseToManager: true);
			}
			CreateNewCustomerForJobTimeStarted();
		}
	}

	public override void OnJobTimeEnded()
	{
		base.OnJobTimeEnded();
		if (T17NetManager.IsMasterClient)
		{
			for (int num = m_Dispensers.Count - 1; num >= 0; num--)
			{
				m_Dispensers[num].GetItemContainer().RemoveAllItems(releaseToManager: true);
			}
		}
	}

	protected virtual void CreateNewCustomerForJobTimeStarted()
	{
		CreateNewCustomer(m_CustomerType, m_bCustomerWantsRandomCustomisation);
	}

	protected override void Local_ServiceWaitingCustomer(ServiceCustomer sender, ItemData itemGiven, Character servicingCharacter)
	{
		Character character = null;
		CustomerViaProxy customerToService = GetCustomerToService(sender, servicingCharacter);
		if (customerToService != null)
		{
			character = customerToService.m_AiCustomer.m_Character;
		}
		base.Local_ServiceWaitingCustomer(sender, itemGiven, servicingCharacter);
		if (character != null && T17NetManager.IsMasterClient)
		{
			ItemOptionPODO itemOptionPODO = FindItemOptionForFinishedProduct(itemGiven.m_ItemDataID);
			if (itemOptionPODO != null)
			{
				SpeechManager.GetInstance().SaySomething(character, itemOptionPODO.m_NpcResponse);
			}
			ItemContainer component = sender.GetComponent<ItemContainer>();
			component.RemoveAllItems(releaseToManager: true);
		}
	}

	protected override void OnCustomerServiced(CustomerViaProxy customer, ServiceCustomer sender, Character servicingCharacter)
	{
		base.OnCustomerServiced(customer, sender, servicingCharacter);
		if (T17NetManager.IsMasterClient && (m_bInfiniteCustomers || base.QuotaAchieved < base.QuotaTarget))
		{
			CreateNewCustomerForCustomerJustServiced();
		}
	}

	protected virtual void CreateNewCustomerForCustomerJustServiced()
	{
		CreateNewCustomer(m_CustomerType, m_bCustomerWantsRandomCustomisation);
	}

	private ItemOptionPODO FindItemOptionForFinishedProduct(int itemDataId)
	{
		for (int num = m_PossibleInputOutputs.Count - 1; num >= 0; num--)
		{
			if (m_PossibleInputOutputs[num] != null && m_PossibleInputOutputs[num].m_FinishedProduct != null && m_PossibleInputOutputs[num].m_FinishedProduct.m_ItemDataID == itemDataId)
			{
				return m_PossibleInputOutputs[num];
			}
		}
		return null;
	}

	private void OnDispenserItemRemoved(ItemContainer container, Item item)
	{
		if (container.GetItemCount() == 0)
		{
			RefillItemContainer();
		}
	}

	private void RefillItemContainer()
	{
		if (m_PossibleInputOutputs.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_NumSetsOfItemsDispensed; i++)
		{
			ItemOptionPODO itemOptionPODO = m_PossibleInputOutputs[UnityEngine.Random.Range(0, m_PossibleInputOutputs.Count)];
			for (int num = itemOptionPODO.m_Ingredients.Count - 1; num >= 0; num--)
			{
				RequestItemCreation(0, itemOptionPODO.m_Ingredients[num].m_ItemDataID);
			}
		}
	}

	protected override void OnItemManagerCreatedItemForUs(Item item, int eventId)
	{
		base.OnItemManagerCreatedItemForUs(item, eventId);
		for (int num = m_Dispensers.Count - 1; num >= 0; num--)
		{
			if (m_Dispensers[num].GetItemContainer().GetFreeSpaceCount() > 0 && !m_Dispensers[num].GetItemContainer().AddItemRPC(item))
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
	}

	protected ServiceItemInteractiveObject GetFirstValidServiceInteraction()
	{
		int i = 0;
		for (int count = m_ServiceItemObjects.Count; i < count; i++)
		{
			if (m_ServiceItemObjects[i] != null)
			{
				return m_ServiceItemObjects[i];
			}
		}
		return null;
	}

	public override void SetRoutineInformationForCharacter(Character character)
	{
		if (!m_bGuidePlayerToServicePoint)
		{
			base.SetRoutineInformationForCharacter(character);
		}
		else
		{
			if (character == null || !character.IsPlayer())
			{
				return;
			}
			Player player = (Player)character;
			if (base.QuotaAchieved < base.QuotaTarget && !DoesCharacterHaveAnyFinishedProducts(player, checkEquippedOnly: false))
			{
				base.SetRoutineInformationForCharacter(character);
				return;
			}
			CustomerViaProxy waitingCustomer = GetWaitingCustomer();
			if (waitingCustomer != null)
			{
				player.SetRoutineArrowTarget(GetFirstValidServiceInteraction().m_NetViewID);
			}
			else
			{
				player.SetRoutineArrowTarget((RoomBlob)null);
			}
		}
	}

	public override bool DoesEmployeeHaveToReportToJobRoom()
	{
		return !DoesCharacterHaveAnyFinishedProducts(base.Employee, checkEquippedOnly: false);
	}

	public bool DoesCharacterHaveAnyFinishedProducts(Character character, bool checkEquippedOnly)
	{
		for (int num = m_PossibleInputOutputs.Count - 1; num >= 0; num--)
		{
			if (checkEquippedOnly)
			{
				Item equippedItem = character.GetEquippedItem();
				if (equippedItem != null && equippedItem.ItemDataID == m_PossibleInputOutputs[num].m_FinishedProduct.m_ItemDataID)
				{
					return true;
				}
			}
			else if (character.HasItemOnPerson(m_PossibleInputOutputs[num].m_FinishedProduct))
			{
				return true;
			}
		}
		return false;
	}
}
