using System.Collections.Generic;
using SaveHelpers;
using UnityEngine;

public class StoneMasonJob : BaseJob
{
	private List<StonemasonDesk> m_StonemasonDesks = new List<StonemasonDesk>();

	private List<CarriedObjectDispenser> m_StoneDispensers = new List<CarriedObjectDispenser>();

	private List<CarryableObjectConsumer> m_StoneConsumers = new List<CarryableObjectConsumer>();

	private List<CarriedObjectDispenser> m_StatueDispensers = new List<CarriedObjectDispenser>();

	private List<CarryableObjectConsumer> m_StatueConsumers = new List<CarryableObjectConsumer>();

	public bool m_bCanBeUsedOutsideJobTime;

	private const int NUM_BITS_PER_ENTRY = 16;

	private const int NUM_BITS_HEADER = 5;

	private const int NUM_BITS_FOR_DATA = 59;

	private const int MAX_ENTRIES_PER_LONG = 3;

	public override void Init(RoomBlob jobRoom)
	{
		base.Init(jobRoom);
		InitStoneDispensers();
		GlobalStart.TimedNetworkService();
		InitStonemasonDesks();
		GlobalStart.TimedNetworkService();
		InitStatueCollectors();
		GlobalStart.TimedNetworkService();
		RoutineManager.GetInstance().OnRoutineChanged += OnRoutineChanged;
	}

	protected override void OnDestroy()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= OnRoutineChanged;
		}
		base.OnDestroy();
	}

	private void InitStoneDispensers()
	{
		if (base.RoomData.m_Dispensers == null)
		{
			return;
		}
		int count = base.RoomData.m_Dispensers.Count;
		for (int i = 0; i < count; i++)
		{
			CarriedObjectDispenser component = base.RoomData.m_Dispensers[i].GetComponent<CarriedObjectDispenser>();
			if (component != null)
			{
				m_StoneDispensers.Add(component);
				component.JobManager_Init();
			}
		}
	}

	private void InitStatueCollectors()
	{
		if (base.RoomData.m_BespokeJobObjects == null)
		{
			return;
		}
		int count = base.RoomData.m_BespokeJobObjects.Count;
		for (int i = 0; i < count; i++)
		{
			CarryableObjectConsumer component = base.RoomData.m_BespokeJobObjects[i].GetComponent<CarryableObjectConsumer>();
			if (component != null)
			{
				m_StatueConsumers.Add(component);
				component.InputDroppedOnUsEvent += Statue_AcceptedInputEvent;
			}
		}
	}

	private void InitStonemasonDesks()
	{
		for (int num = base.RoomData.m_Processors.Count - 1; num >= 0; num--)
		{
			InteractiveObject interactiveObject = base.RoomData.m_Processors[num];
			if (interactiveObject != null)
			{
				StonemasonDesk component = interactiveObject.GetComponent<StonemasonDesk>();
				if (component != null)
				{
					m_StonemasonDesks.Add(component);
					component.InitInteractions(m_bCanBeUsedOutsideJobTime);
					CarriedObjectDispenser component2 = component.GetComponent<CarriedObjectDispenser>();
					component2.JobManager_Init();
					m_StatueDispensers.Add(component2);
					CarryableObjectConsumer component3 = component.GetComponent<CarryableObjectConsumer>();
					m_StoneConsumers.Add(component3);
					component3.InputDroppedOnUsEvent += Stone_AcceptedInputEvent;
				}
			}
		}
	}

	private void Stone_AcceptedInputEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		for (int i = 0; i < m_StoneDispensers.Count; i++)
		{
			m_StoneDispensers[i].AddObjectBackToSpawnPool(theObject);
		}
	}

	private void Statue_AcceptedInputEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		if (T17NetManager.IsMasterClient && consumer.m_ProcessingTags.Contains(theObject.m_Tag))
		{
			IncrementQuotaAchieved();
		}
		for (int i = 0; i < m_StatueDispensers.Count; i++)
		{
			m_StatueDispensers[i].AddObjectBackToSpawnPool(theObject);
		}
	}

	private void OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (newRoutine.m_BaseRoutineType == Routines.LightsOut)
		{
			for (int num = m_StonemasonDesks.Count - 1; num >= 0; num--)
			{
				m_StonemasonDesks[num].RPC_SetState(StonemasonDesk.State.NoStone, silent: true);
			}
			base.RequiresSerialization = true;
		}
	}

	public override void OnJobTimeStarted(bool isSaveRestore)
	{
		base.OnJobTimeStarted(isSaveRestore);
		if (!isSaveRestore)
		{
			if (T17NetManager.IsMasterClient)
			{
				ResetStoneMasonDesks();
			}
			for (int i = 0; i < m_StatueDispensers.Count; i++)
			{
				m_StatueDispensers[i].ReleaseAllActiveObjectsWithEffect();
			}
			for (int j = 0; j < m_StoneDispensers.Count; j++)
			{
				m_StoneDispensers[j].ReleaseAllActiveObjectsWithEffect();
			}
		}
	}

	private void ResetStoneMasonDesks()
	{
		for (int num = m_StonemasonDesks.Count - 1; num >= 0; num--)
		{
			m_StonemasonDesks[num].SetStateRPC(StonemasonDesk.State.NoStone, silent: true);
		}
	}

	public override void Deserialize(ulong[] jobData)
	{
		base.Deserialize(jobData);
		for (int i = 1; i < jobData.Length; i++)
		{
			BitField bitField = new BitField(jobData[i]);
			int uInt = (int)bitField.GetUInt(5);
			for (int j = 0; j < uInt; j++)
			{
				int netViewId = (int)bitField.GetUInt(12);
				StonemasonDesk.State uInt2 = (StonemasonDesk.State)bitField.GetUInt(4);
				StonemasonDesk stonemasonDesk = m_StonemasonDesks.Find((StonemasonDesk x) => x.m_NetViewID.viewID == netViewId);
				if (stonemasonDesk != null)
				{
					stonemasonDesk.RPC_SetState(uInt2, silent: true);
				}
			}
		}
	}

	public override List<ulong> Serialize()
	{
		List<ulong> list = base.Serialize();
		List<StonemasonDesk> list2 = m_StonemasonDesks.FindAll((StonemasonDesk x) => x.GetState() != StonemasonDesk.State.NoStone);
		int num = list2.Count;
		if (num == 0)
		{
			return list;
		}
		while (num > 0)
		{
			BitField bitField = new BitField();
			int num2 = Mathf.Min(3, num);
			bitField.Set(5, (uint)num2);
			for (int i = 0; i < num2; i++)
			{
				bitField.Set(12, (uint)list2[i].m_NetViewID.viewID);
				bitField.Set(4, (uint)list2[i].GetState());
				num--;
			}
			list.Add((ulong)bitField);
		}
		return list;
	}
}
