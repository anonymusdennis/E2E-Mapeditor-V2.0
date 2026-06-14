using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class HorseshoeMakingJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_ProcessorA;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_ProcessorB;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Collectors;

	[BlackboardOnly]
	public BBParameter<List<int>> m_ProcessorAAcceptedItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_ProcessorBAcceptedItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_CollectorAcceptedItems;

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
		BaseJob charactersJob = instance.GetCharactersJob(base.agent.m_Character);
		ExtendedProcessItemJob extendedProcessItemJob = charactersJob as ExtendedProcessItemJob;
		if (extendedProcessItemJob == null)
		{
			EndAction(false);
			return;
		}
		List<InteractiveObject> objects = roomBlobData.m_Dispensers;
		if (SanitiseInput(ref objects))
		{
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
		if (objects3 != null && objects3.Count > 0)
		{
			List<InteractiveObject> list = new List<InteractiveObject>();
			List<InteractiveObject> list2 = new List<InteractiveObject>();
			for (int i = 0; i < objects3.Count; i++)
			{
				ItemProcessorBase component = objects3[i].GetComponent<ItemProcessorBase>();
				if (component != null)
				{
					if (!component.m_bSecondaryProcessor)
					{
						list.Add(objects3[i]);
					}
					else
					{
						list2.Add(objects3[i]);
					}
				}
			}
			m_ProcessorA.value = list;
			m_ProcessorB.value = list2;
		}
		m_ProcessorAAcceptedItems.value = GetItemProcessorAcceptedItemList(m_ProcessorA.value);
		m_ProcessorBAcceptedItems.value = GetItemProcessorAcceptedItemList(m_ProcessorB.value);
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

	private List<int> GetItemProcessorAcceptedItemList(List<InteractiveObject> processors)
	{
		List<int> list = new List<int>();
		if (processors != null && processors.Count > 0)
		{
			for (int i = 0; i < processors.Count; i++)
			{
				InteractiveObject interactiveObject = processors[i];
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
		}
		return list;
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
			m_CollectorAcceptedItems.value = new List<int>();
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
		m_CollectorAcceptedItems.value = list;
	}
}
