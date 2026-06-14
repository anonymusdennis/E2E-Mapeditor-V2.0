using System.Collections.Generic;
using UnityEngine;

public class GenericEventManager : EventManager
{
	[ReadOnly]
	public List<AIEvent> m_VisibleAIEvents = new List<AIEvent>();

	public AIEventManager.EventHeight m_EventHeight = AIEventManager.EventHeight.Wall;

	private bool m_bIsCovered;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_EventHeight == AIEventManager.EventHeight.Wall)
		{
			m_TargetOffsets.Add(Direction.m_vUp * 0.6f);
			m_TargetOffsets.Add(Direction.m_vDown * 0.6f);
			m_TargetOffsets.Add(Direction.m_vLeft * 0.6f);
			m_TargetOffsets.Add(Direction.m_vRight * 0.6f);
		}
		if (m_bVisibleFromBelow)
		{
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
			if (floor != null)
			{
				FloorManager.Floor floor2 = FloorManager.GetInstance().DownAFloor(floor);
				float num = floor2.m_zPos - floor.m_zPos;
				Vector3 zero = Vector3.zero;
				zero.z = num - 0.9f;
				m_TargetOffsets.Add(zero);
			}
		}
		return base.StartInit();
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		if (m_bIsCovered)
		{
			return null;
		}
		return m_VisibleAIEvents;
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForPosition(base.transform.position, m_EventHeight);
	}

	public void SetIsCovered(bool isCovered)
	{
		m_bIsCovered = isCovered;
	}

	public AIEvent EnableEventVisability(AIEventData eventData, Character characterResponsible, GameObject target)
	{
		for (int i = 0; i < m_VisibleAIEvents.Count; i++)
		{
			if (m_VisibleAIEvents[i].m_EventData.m_eEventType == eventData.m_eEventType)
			{
				return m_VisibleAIEvents[i];
			}
		}
		AIEvent aIEvent = new AIEvent(eventData, characterResponsible, null, target, this);
		m_VisibleAIEvents.Add(aIEvent);
		aIEvent.OnEventStarted();
		int newFloor = FloorManager.GetInstance().FindFloorIndexAtZ(base.transform.position.z);
		AIEventManager.GetInstance().UpdatePosition(this, newFloor);
		return aIEvent;
	}

	public void DisableEventVisability(AIEventData eventData)
	{
		for (int num = m_VisibleAIEvents.Count - 1; num >= 0; num--)
		{
			AIEventData eventData2 = m_VisibleAIEvents[num].m_EventData;
			if (eventData2.m_eEventType == eventData.m_eEventType)
			{
				m_VisibleAIEvents[num].OnEventStopped();
				m_VisibleAIEvents.Remove(m_VisibleAIEvents[num]);
				break;
			}
		}
		if (m_VisibleAIEvents.Count == 0)
		{
			AIEventManager instance = AIEventManager.GetInstance();
			if (instance != null)
			{
				instance.RemoveManager(this);
			}
		}
	}

	protected override void OnManagerDestroyed()
	{
		if (m_VisibleAIEvents.Count > 0)
		{
			for (int num = m_VisibleAIEvents.Count - 1; num >= 0; num--)
			{
				DisableEventVisability(m_VisibleAIEvents[num].m_EventData);
			}
		}
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		for (int i = 0; i < m_VisibleAIEvents.Count; i++)
		{
			if (m_VisibleAIEvents[i].m_EventData.m_eEventType == eventType)
			{
				return m_VisibleAIEvents[i];
			}
		}
		return null;
	}
}
