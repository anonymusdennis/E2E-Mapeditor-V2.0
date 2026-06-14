using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class HandyManJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Processors;

	[BlackboardOnly]
	public BBParameter<List<int>> m_RequiredDispensedItems;

	[BlackboardOnly]
	public BBParameter<List<int>> m_RequiredCraftableItems;

	[BlackboardOnly]
	public BBParameter<int> m_RequiredEquippedItemID;

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
		HandymanJob handymanJob = (HandymanJob)instance.GetCharactersJob(base.agent.m_Character);
		if (handymanJob == null)
		{
			EndAction(false);
			return;
		}
		handymanJob.SetupProcessors();
		m_RequiredDispensedItems.value = new List<int>();
		for (int i = 0; i < handymanJob.m_DispensedStartingItems.Length; i++)
		{
			m_RequiredDispensedItems.value.Add(handymanJob.m_DispensedStartingItems[i].m_ItemDataID);
		}
		m_RequiredCraftableItems.value = new List<int>();
		for (int j = 0; j < handymanJob.m_CraftableItemRequirements.Count; j++)
		{
			m_RequiredCraftableItems.value.Add(handymanJob.m_CraftableItemRequirements[j].m_ItemDataID);
		}
		m_Dispensers.value = roomBlobData.m_Dispensers;
		m_Processors.value = roomBlobData.m_Processors;
		if (handymanJob.m_MinigameRequiredEquippedItem != null)
		{
			m_RequiredEquippedItemID.value = handymanJob.m_MinigameRequiredEquippedItem.m_ItemDataID;
		}
		EndAction(true);
	}
}
