using System;
using System.Collections.Generic;

public class BedEventManager : EventManager
{
	public BedInteraction m_Bed;

	public AIEventData m_IsMissing;

	private AIEvent m_IsMissingEvent;

	private bool m_IsCharacterMissing;

	private bool m_HasBedDummy;

	private bool m_IsMissingVisible;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	private int m_FloorIndex;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_IsMissing = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsMissing);
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
		GetEventByType(AIEvent.EventType.Character_Missing);
		T17NetManager.OnBecameMasterClient += UpdateEventVisibility;
	}

	protected override void OnManagerDestroyed()
	{
		T17NetManager.OnBecameMasterClient -= UpdateEventVisibility;
	}

	public void IsMissing(bool missing)
	{
		if (m_IsCharacterMissing != missing)
		{
			m_IsCharacterMissing = missing;
			UpdateEventVisibility();
		}
	}

	public void HasDummy(bool haveDummy)
	{
		if (m_HasBedDummy != haveDummy)
		{
			m_HasBedDummy = haveDummy;
			UpdateEventVisibility();
		}
	}

	public void UpdateEventVisibility()
	{
		if (!T17NetManager.IsMasterClient || m_IsMissingEvent == null)
		{
			return;
		}
		bool flag = m_IsCharacterMissing && !m_HasBedDummy;
		if (flag && !m_IsMissingVisible)
		{
			m_IsMissingVisible = true;
			m_VisibleEvents.Add(m_IsMissingEvent);
			m_IsMissingEvent.OnEventStarted();
			AIEventManager.GetInstance().UpdatePosition(this, m_FloorIndex);
		}
		else if (!flag && m_IsMissingVisible)
		{
			m_IsMissingVisible = false;
			m_VisibleEvents.Remove(m_IsMissingEvent);
			m_IsMissingEvent.OnEventStopped();
			if (m_VisibleEvents.Count == 0)
			{
				AIEventManager.GetInstance().RemoveManager(this);
			}
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
		if (eventType == AIEvent.EventType.Character_Missing)
		{
			if (m_Bed != null && m_Bed.GetOwner() != null)
			{
				m_IsMissingEvent = new AIEvent(m_IsMissing, m_Bed.GetOwner(), m_Bed.GetOwner(), base.gameObject, this);
			}
			return m_IsMissingEvent;
		}
		return null;
	}
}
