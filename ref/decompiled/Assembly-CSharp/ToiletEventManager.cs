using System;
using System.Collections.Generic;
using UnityEngine;

public class ToiletEventManager : EventManager
{
	public AIEventData m_ToiletFlood;

	private AIEvent m_ToiletFloodedEvent;

	public AIEventData m_ContrabandInToilet;

	private AIEvent m_ContrabandInToiletEvent;

	public ToiletInteraction m_Toilet;

	private bool m_isToiletFloded;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	private int m_FloorIndex;

	private BaseJob m_PlumbingJobCache;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_ToiletFlood = ConfigManager.GetInstance().ApplyAIEventOverride(m_ToiletFlood);
			m_ContrabandInToilet = ConfigManager.GetInstance().ApplyAIEventOverride(m_ContrabandInToilet);
		}
		m_FloorIndex = FloorManager.GetInstance().FindFloorIndexAtZ(base.transform.position.z);
		if (AIEventManager.GetInstance().SetupDone())
		{
			Setup();
		}
		else
		{
			AIEventManager instance = AIEventManager.GetInstance();
			instance.m_AIEventDataSetup = (AIEventManager.AIEventDataSetup)Delegate.Combine(instance.m_AIEventDataSetup, new AIEventManager.AIEventDataSetup(Setup));
		}
		return base.StartInit();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		AIEventManager instance = AIEventManager.GetInstance();
		if (instance != null)
		{
			instance.m_AIEventDataSetup = (AIEventManager.AIEventDataSetup)Delegate.Remove(instance.m_AIEventDataSetup, new AIEventManager.AIEventDataSetup(Setup));
		}
	}

	public void Setup()
	{
		GetEventByType(AIEvent.EventType.Tile_Flooded);
		GetEventByType(AIEvent.EventType.Item_ContrabandInContainer);
		if (JobsManager.GetInstance() != null)
		{
			m_PlumbingJobCache = JobsManager.GetInstance().GetJob(JobType.Plumbing);
		}
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		if (m_PlumbingJobCache != null && RoutineManager.GetInstance() != null && RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.JobTime && m_PlumbingJobCache.IsJobActive())
		{
			return null;
		}
		return m_VisibleEvents;
	}

	public AIEvent GetToiletFloodEvent()
	{
		return m_ToiletFloodedEvent;
	}

	public AIEvent GetContrabandInToiletEvent()
	{
		return m_ContrabandInToiletEvent;
	}

	public void ClearOffsets()
	{
		m_TargetOffsets.Clear();
	}

	public void AddOffet(Vector3 newOffset)
	{
		m_TargetOffsets.Add(newOffset);
	}

	public void UpdateFloodEventVisibility(bool isFlooded)
	{
		if (isFlooded && !m_isToiletFloded)
		{
			m_isToiletFloded = true;
			m_VisibleEvents.Add(m_ToiletFloodedEvent);
			m_ToiletFloodedEvent.OnEventStarted();
			AIEventManager.GetInstance().UpdatePosition(this, m_FloorIndex);
		}
		else if (!isFlooded && m_isToiletFloded)
		{
			m_isToiletFloded = false;
			m_VisibleEvents.Remove(m_ToiletFloodedEvent);
			m_ToiletFloodedEvent.OnEventStopped();
			m_ToiletFloodedEvent = new AIEvent(m_ToiletFlood, null, null, base.gameObject, this);
			if (m_VisibleEvents.Count == 0)
			{
				AIEventManager.GetInstance().RemoveManager(this);
			}
		}
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		switch (eventType)
		{
		case AIEvent.EventType.Tile_Flooded:
			if (m_ToiletFloodedEvent == null)
			{
				m_ToiletFloodedEvent = new AIEvent(m_ToiletFlood, null, null, base.gameObject, this);
			}
			return m_ToiletFloodedEvent;
		case AIEvent.EventType.Item_ContrabandInContainer:
			if (m_ContrabandInToiletEvent == null)
			{
				m_ContrabandInToiletEvent = new AIEvent(m_ContrabandInToilet, null, null, base.gameObject, this);
			}
			return m_ContrabandInToiletEvent;
		default:
			return null;
		}
	}
}
