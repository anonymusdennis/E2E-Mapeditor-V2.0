using System;
using System.Collections.Generic;

public class SwagBagEventManager : EventManager
{
	public AIEventData m_SwagBagOnFloor;

	private AIEvent m_SwagBagOnFloorEvent;

	public float m_BagInvisibleTime = 60f;

	private float m_BagInvisbleTimer;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	private int m_FloorIndex;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_SwagBagOnFloor = ConfigManager.GetInstance().ApplyAIEventOverride(m_SwagBagOnFloor);
			if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().aiConfig != null)
			{
				m_BagInvisibleTime = ConfigManager.GetInstance().aiConfig.GetSwagBagInvisibleTime();
			}
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

	public void Setup()
	{
		GetEventByType(AIEvent.EventType.RemoveSwagBag);
		AIEventManager instance = AIEventManager.GetInstance();
		instance.m_AIEventDataSetup = (AIEventManager.AIEventDataSetup)Delegate.Remove(instance.m_AIEventDataSetup, new AIEventManager.AIEventDataSetup(Setup));
	}

	public void StartSwagBagTimer()
	{
		if (m_BagInvisibleTime > 0f)
		{
			m_BagInvisbleTimer = m_BagInvisibleTime;
		}
		else
		{
			m_BagInvisibleTime = 0.001f;
		}
	}

	private void Update()
	{
		if (m_BagInvisbleTimer > 0f)
		{
			m_BagInvisbleTimer -= UpdateManager.deltaTime;
			if (m_BagInvisbleTimer <= 0f)
			{
				m_VisibleEvents.Add(m_SwagBagOnFloorEvent);
				m_SwagBagOnFloorEvent.OnEventStarted();
				AIEventManager.GetInstance().UpdatePosition(this, m_FloorIndex);
			}
		}
	}

	public void ClearSwagBagEvent()
	{
		m_VisibleEvents.Remove(m_SwagBagOnFloorEvent);
		m_SwagBagOnFloorEvent.OnEventStopped();
		if (m_VisibleEvents.Count == 0)
		{
			AIEventManager.GetInstance().RemoveManager(this);
		}
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		return m_VisibleEvents;
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		if (eventType == AIEvent.EventType.RemoveSwagBag)
		{
			if (m_SwagBagOnFloorEvent == null)
			{
				m_SwagBagOnFloorEvent = new AIEvent(m_SwagBagOnFloor, null, null, base.gameObject, this);
			}
			return m_SwagBagOnFloorEvent;
		}
		return null;
	}
}
