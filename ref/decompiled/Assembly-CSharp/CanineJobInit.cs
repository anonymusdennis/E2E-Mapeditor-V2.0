using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class CanineJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_BowlDispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_FoodDispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Collectors;

	[BlackboardOnly]
	public BBParameter<int> m_DogBowlID;

	[BlackboardOnly]
	public BBParameter<int> m_DogFoodID;

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
		DogFoodJob dogFoodJob = (DogFoodJob)instance.GetCharactersJob(base.agent.m_Character);
		if (dogFoodJob == null)
		{
			EndAction(false);
			return;
		}
		m_DogBowlID.value = dogFoodJob.m_FoodReceptacleItem.m_ItemDataID;
		m_DogFoodID.value = dogFoodJob.m_DogFoodItem.m_ItemDataID;
		List<TransferItemsInteraction> foodDispensers = dogFoodJob.GetFoodDispensers();
		List<InteractiveObject> list = new List<InteractiveObject>();
		for (int i = 0; i < foodDispensers.Count; i++)
		{
			list.Add(foodDispensers[i]);
		}
		m_BowlDispensers.value = roomBlobData.m_Dispensers;
		m_Collectors.value = roomBlobData.m_Collectors;
		m_FoodDispensers.value = list;
		EndAction(true);
	}
}
