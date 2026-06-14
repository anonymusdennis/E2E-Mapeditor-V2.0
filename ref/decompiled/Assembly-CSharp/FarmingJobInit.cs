using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class FarmingJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Processors;

	[BlackboardOnly]
	public BBParameter<int> m_TrowelID;

	[BlackboardOnly]
	public BBParameter<int> m_SeedsID;

	[BlackboardOnly]
	public BBParameter<int> m_PottedPlantID;

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
		GrowPlantJob growPlantJob = (GrowPlantJob)instance.GetCharactersJob(base.agent.m_Character);
		if (growPlantJob == null)
		{
			EndAction(false);
			return;
		}
		m_TrowelID.value = growPlantJob.m_Trowel.m_ItemDataID;
		m_SeedsID.value = growPlantJob.m_Seeds.m_ItemDataID;
		m_PottedPlantID.value = growPlantJob.m_PottedPlant.m_ItemDataID;
		m_Dispensers.value = roomBlobData.m_Dispensers;
		m_Processors.value = roomBlobData.m_Processors;
		EndAction(true);
	}
}
