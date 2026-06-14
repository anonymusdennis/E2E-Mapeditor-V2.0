using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class ServiceItemJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Collectors;

	[BlackboardOnly]
	public BBParameter<List<int>> m_PostDispenserCraftItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_CollectorAcceptedItems;

	protected override string OnInit()
	{
		m_Dispensers.value = new List<InteractiveObject>();
		m_Collectors.value = new List<InteractiveObject>();
		m_CollectorAcceptedItems.value = new List<int>();
		m_PostDispenserCraftItems.value = new List<int>();
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		base.OnExecute();
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
		if (charactersJob is ServiceItemJob)
		{
			ServiceItemJob job = charactersJob as ServiceItemJob;
			SetupDispenserVariables(roomBlobData, job);
			SetupCollectorVariables(roomBlobData, job);
			EndAction(true);
		}
		else
		{
			EndAction(false);
		}
	}

	private void SetupDispenserVariables(RoomBlob_JobRoom jobRoom, ServiceItemJob job)
	{
		m_Dispensers.value.Clear();
		for (int num = jobRoom.m_Dispensers.Count - 1; num >= 0; num--)
		{
			if (jobRoom.m_Dispensers[num] != null)
			{
				m_Dispensers.value.Add(jobRoom.m_Dispensers[num]);
			}
		}
		for (int num2 = job.m_PossibleInputOutputs.Count - 1; num2 >= 0; num2--)
		{
			ServiceItemJob.ItemOptionPODO itemOptionPODO = job.m_PossibleInputOutputs[num2];
			if (itemOptionPODO.m_Ingredients.Count != 1 || itemOptionPODO.m_Ingredients[0].m_ItemDataID != itemOptionPODO.m_FinishedProduct.m_ItemDataID)
			{
				m_PostDispenserCraftItems.value.Add(itemOptionPODO.m_FinishedProduct.m_ItemDataID);
			}
		}
	}

	private void SetupCollectorVariables(RoomBlob_JobRoom jobRoom, ServiceItemJob job)
	{
		m_Collectors.value.Clear();
		List<InteractiveObject> collectors = jobRoom.m_Collectors;
		for (int num = collectors.Count - 1; num >= 0; num--)
		{
			ServiceItemInteractiveObject serviceItemInteractiveObject = ((!(collectors[num] == null)) ? (collectors[num] as ServiceItemInteractiveObject) : null);
			if (serviceItemInteractiveObject != null)
			{
				m_Collectors.value.Add(serviceItemInteractiveObject);
			}
		}
		m_CollectorAcceptedItems.value.Clear();
		for (int num2 = job.m_PossibleInputOutputs.Count - 1; num2 >= 0; num2--)
		{
			m_CollectorAcceptedItems.value.Add(job.m_PossibleInputOutputs[num2].m_FinishedProduct.m_ItemDataID);
		}
	}
}
