using System.Collections.Generic;
using UnityEngine;

public class LocationEventManager : EventManager
{
	public AIEventData m_InvestigateLocation;

	private AIEvent m_InvestigateLocationEvent;

	public GameObject m_TargetLocation;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_InvestigateLocation = ConfigManager.GetInstance().ApplyAIEventOverride(m_InvestigateLocation);
		}
		GetEventByType(AIEvent.EventType.InvestigateLocation);
		return base.StartInit();
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		return m_VisibleEvents;
	}

	public AIEvent GetInvestigateObjectEvent()
	{
		return GetEventByType(AIEvent.EventType.InvestigateLocation);
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		if (eventType == AIEvent.EventType.InvestigateLocation)
		{
			if (m_InvestigateLocationEvent == null)
			{
				m_InvestigateLocationEvent = new AIEvent(m_InvestigateLocation, null, null, m_TargetLocation, this);
			}
			return m_InvestigateLocationEvent;
		}
		return null;
	}
}
