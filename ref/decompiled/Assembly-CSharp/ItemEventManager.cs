using System.Collections.Generic;
using UnityEngine;

public class ItemEventManager : EventManager
{
	public AIEventData m_ItemMissing;

	private AIEvent m_ItemMissingEvent;

	public Item m_Item;

	private List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_ItemMissing = ConfigManager.GetInstance().ApplyAIEventOverride(m_ItemMissing);
		}
		GetEventByType(AIEvent.EventType.ItemMissing);
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

	public AIEvent GetItemMissingEvent()
	{
		return GetEventByType(AIEvent.EventType.ItemMissing);
	}

	public override Vector3 GetWorldPosition()
	{
		Vector3 worldPos = base.transform.position;
		if (m_Item != null)
		{
			m_Item.GetWorldPosition(out worldPos);
		}
		return worldPos;
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		if (eventType == AIEvent.EventType.ItemMissing)
		{
			if (m_ItemMissingEvent == null)
			{
				m_ItemMissingEvent = new AIEvent(m_ItemMissing, null, null, base.gameObject, this);
			}
			return m_ItemMissingEvent;
		}
		return null;
	}
}
