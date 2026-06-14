using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Jobs")]
public class CarryItemJobBehaviourInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_CarriedObjectDispenser;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Processors;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Collectors;

	[BlackboardOnly]
	public BBParameter<List<GameObject>> m_BespokeObjects;

	[BlackboardOnly]
	public BBParameter<List<int>> m_DispenserItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_ProcessorAcceptedItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_CollectorAcceptedItem;

	protected override string OnInit()
	{
		m_ProcessorAcceptedItems.value = new List<int>();
		m_CollectorAcceptedItem.value = new List<int>();
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		RoomBlob jobRoom = base.agent.m_Character.GetJobRoom();
		if (jobRoom == null)
		{
			EndAction(false);
			return;
		}
		RoomBlob_JobRoom roomBlobData = jobRoom.GetRoomBlobData<RoomBlob_JobRoom>();
		if (roomBlobData == null)
		{
			EndAction(false);
			return;
		}
		JobsManager instance = JobsManager.GetInstance();
		if (instance == null)
		{
			EndAction(false);
			return;
		}
		List<InteractiveObject> objects = new List<InteractiveObject>(roomBlobData.m_Dispensers);
		if (SanitiseInput(ref objects))
		{
		}
		for (int num = objects.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = objects[num];
			CarriedObjectDispenser carriedObjectDispenser = interactiveObject as CarriedObjectDispenser;
			if (carriedObjectDispenser != null)
			{
				objects.RemoveAt(num);
				if (m_CarriedObjectDispenser.value == null)
				{
					m_CarriedObjectDispenser.value = interactiveObject;
					List<int> list = new List<int>();
					List<ItemData> requiredItems = carriedObjectDispenser.m_RequiredItems;
					for (int i = 0; i < requiredItems.Count; i++)
					{
						if (!(requiredItems[i] == null))
						{
							int itemDataID = requiredItems[i].m_ItemDataID;
							list.Add(itemDataID);
						}
					}
					m_DispenserItems.value = list;
				}
			}
		}
		m_Dispensers.value = objects;
		List<InteractiveObject> objects2 = roomBlobData.m_Collectors;
		if (SanitiseInput(ref objects2))
		{
		}
		m_Collectors.value = objects2;
		List<InteractiveObject> objects3 = roomBlobData.m_Processors;
		if (SanitiseInput(ref objects3))
		{
		}
		m_Processors.value = objects3;
		List<GameObject> objects4 = roomBlobData.m_BespokeJobObjects;
		if (SanitiseInput(ref objects4))
		{
		}
		m_BespokeObjects.value = objects4;
		SetupItemProcessorAcceptedItemList(roomBlobData);
		SetupItemCollectorAcceptedItemList(roomBlobData);
		EndAction(true);
	}

	private bool SanitiseInput<T>(ref List<T> objects)
	{
		bool result = false;
		if (objects == null)
		{
			return result;
		}
		for (int num = objects.Count - 1; num >= 0; num--)
		{
			if (objects[num] == null)
			{
				result = true;
				objects.RemoveAt(num);
			}
		}
		return result;
	}

	private void SetupItemProcessorAcceptedItemList(RoomBlob_JobRoom jobRoom)
	{
		if (jobRoom.m_Processors == null || jobRoom.m_Processors.Count <= 0)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < jobRoom.m_Processors.Count; i++)
		{
			InteractiveObject interactiveObject = jobRoom.m_Processors[i];
			if (interactiveObject == null)
			{
				continue;
			}
			ItemProcessorBase component = interactiveObject.GetComponent<ItemProcessorBase>();
			if (component == null)
			{
				continue;
			}
			ItemData[] inputItemTypes = component.GetInputItemTypes();
			m_ProcessorAcceptedItems.value = new List<int>();
			if (inputItemTypes == null)
			{
				continue;
			}
			foreach (ItemData itemData in inputItemTypes)
			{
				if (!(itemData == null) && !list.Contains(itemData.m_ItemDataID))
				{
					list.Add(itemData.m_ItemDataID);
				}
			}
		}
		m_ProcessorAcceptedItems.value = list;
	}

	private void SetupItemCollectorAcceptedItemList(RoomBlob_JobRoom jobRoom)
	{
		if (jobRoom.m_Collectors == null || jobRoom.m_Collectors.Count <= 0)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < jobRoom.m_Collectors.Count; i++)
		{
			InteractiveObject interactiveObject = jobRoom.m_Collectors[i];
			if (interactiveObject == null)
			{
				continue;
			}
			TransferItemsInteraction component = interactiveObject.GetComponent<TransferItemsInteraction>();
			if (component == null)
			{
				continue;
			}
			List<ItemData> transferItemTypes = component.GetTransferItemTypes();
			m_CollectorAcceptedItem.value = new List<int>();
			if (transferItemTypes == null)
			{
				continue;
			}
			for (int j = 0; j < transferItemTypes.Count; j++)
			{
				ItemData itemData = transferItemTypes[j];
				if (!(itemData == null) && !list.Contains(itemData.m_ItemDataID))
				{
					list.Add(itemData.m_ItemDataID);
				}
			}
		}
		m_CollectorAcceptedItem.value = list;
	}

	private List<int> GetCraftItems(int productID)
	{
		List<int> list = new List<int>();
		CraftManager.Recipe recipeForProduct = CraftManager.GetInstance().GetRecipeForProduct(productID);
		if (recipeForProduct == null || recipeForProduct.m_Ingredients == null)
		{
			return list;
		}
		for (int i = 0; i < recipeForProduct.m_Ingredients.Length; i++)
		{
			ItemData itemData = recipeForProduct.m_Ingredients[i];
			if (!(itemData == null))
			{
				list.Add(itemData.m_ItemDataID);
			}
		}
		return list;
	}
}
