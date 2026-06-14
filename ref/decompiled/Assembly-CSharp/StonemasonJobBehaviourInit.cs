using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Jobs")]
public class StonemasonJobBehaviourInit : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Dispensers;

	[BlackboardOnly]
	public BBParameter<List<InteractiveObject>> m_Processors;

	[BlackboardOnly]
	public BBParameter<GameObject> m_Collector;

	[BlackboardOnly]
	public BBParameter<StonemasonDesk> m_StonemasonDesk;

	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_CarvingInteraction;

	[BlackboardOnly]
	public BBParameter<InteractiveObject> m_PickupStatueInteraction;

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
		m_Dispensers.value = objects;
		List<InteractiveObject> objects2 = roomBlobData.m_Processors;
		if (SanitiseInput(ref objects2))
		{
		}
		m_Processors.value = objects2;
		List<GameObject> objects3 = roomBlobData.m_BespokeJobObjects;
		if (SanitiseInput(ref objects3))
		{
		}
		if (objects3 != null && objects3.Count > 0)
		{
			m_Collector.value = objects3[0];
		}
		StonemasonDesk stonemasonDesk = FindStonemasonDesk();
		if (stonemasonDesk != null)
		{
			m_StonemasonDesk.value = stonemasonDesk;
			m_CarvingInteraction.value = stonemasonDesk.GetComponent<StonemasonCarvingInteraction>();
			m_PickupStatueInteraction.value = stonemasonDesk.GetComponent<StonemasonDeskDispenser>();
		}
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

	private StonemasonDesk FindStonemasonDesk()
	{
		if (m_Processors.value != null)
		{
			for (int i = 0; i < m_Processors.value.Count; i++)
			{
				if (!(m_Processors.value[i] == null))
				{
					StonemasonDesk component = m_Processors.value[i].GetComponent<StonemasonDesk>();
					if (component != null)
					{
						return component;
					}
				}
			}
		}
		return null;
	}
}
