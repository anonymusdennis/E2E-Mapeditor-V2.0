using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Category("★T17 Jobs")]
public class RelightTorchesJobInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_WallTorches;

	[BlackboardOnly]
	public BBParameter<int> m_LighterItem;

	[BlackboardOnly]
	public BBParameter<int> m_FuelItem;

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
		RelightTorchesJob relightTorchesJob = (RelightTorchesJob)instance.GetCharactersJob(base.agent.m_Character);
		if (relightTorchesJob == null)
		{
			EndAction(false);
			return;
		}
		List<WallTorch> wallTorches = relightTorchesJob.GetWallTorches();
		List<InteractiveObject> list = new List<InteractiveObject>();
		for (int i = 0; i < wallTorches.Count; i++)
		{
			list.Add(wallTorches[i]);
		}
		m_WallTorches.value = list;
		m_Dispensers.value = roomBlobData.m_Dispensers;
		m_LighterItem.value = relightTorchesJob.m_LighterItem.m_ItemDataID;
		m_FuelItem.value = relightTorchesJob.m_FuelItem.m_ItemDataID;
		EndAction(true);
	}
}
