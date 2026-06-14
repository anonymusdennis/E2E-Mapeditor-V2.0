using System;
using System.Collections.Generic;

public class DeskEventManager : EventManager
{
	public AIEventData m_InvestigateObject;

	private AIEvent m_InvestigateObjectEvent;

	public AIEventData m_ContrabandInDesk;

	private AIEvent m_ContrabandInDeskEvent;

	public DeskInteraction m_Desk;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_InvestigateObject = ConfigManager.GetInstance().ApplyAIEventOverride(m_InvestigateObject);
			m_ContrabandInDesk = ConfigManager.GetInstance().ApplyAIEventOverride(m_ContrabandInDesk);
		}
		m_TargetOffsets.Add(Direction.m_vUp * 0.6f);
		m_TargetOffsets.Add(Direction.m_vDown * 0.6f);
		m_TargetOffsets.Add(Direction.m_vLeft * 0.6f);
		m_TargetOffsets.Add(Direction.m_vRight * 0.6f);
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
		m_Desk = null;
		m_InvestigateObject = null;
		m_InvestigateObjectEvent = null;
		m_ContrabandInDesk = null;
		m_ContrabandInDeskEvent = null;
	}

	public void Setup()
	{
		GetEventByType(AIEvent.EventType.InvestigateObject);
		GetEventByType(AIEvent.EventType.Item_ContrabandInContainer);
	}

	public override uint GetEventManagerID()
	{
		if (m_NetView != null)
		{
			return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
		}
		return 0u;
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		return m_VisibleEvents;
	}

	public AIEvent GetInvestigateObjectEvent()
	{
		return GetEventByType(AIEvent.EventType.InvestigateObject);
	}

	public AIEvent GetContrabandInDeskEvent()
	{
		return GetEventByType(AIEvent.EventType.Item_ContrabandInContainer);
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		switch (eventType)
		{
		case AIEvent.EventType.InvestigateObject:
			if (m_InvestigateObjectEvent == null)
			{
				m_InvestigateObjectEvent = new AIEvent(m_InvestigateObject, null, null, base.gameObject, this);
			}
			return m_InvestigateObjectEvent;
		case AIEvent.EventType.Item_ContrabandInContainer:
			if (m_ContrabandInDeskEvent == null)
			{
				Character owner = m_Desk.GetOwner();
				m_ContrabandInDeskEvent = new AIEvent(m_ContrabandInDesk, owner, null, base.gameObject, this);
			}
			return m_ContrabandInDeskEvent;
		default:
			return null;
		}
	}
}
