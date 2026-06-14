using System.Collections.Generic;
using UnityEngine;

public class AICharacter_MaintenanceMan : AICharacter
{
	public ItemData m_plunger;

	private float m_remainingRepairTime;

	protected override void OnStart()
	{
		NPCManager.GetInstance().AddMaintenanceMan(this);
	}

	protected override void OnAddingEventToMemory(AIEvent aiEvent, AIEventMemory memory, bool silent)
	{
		aiEvent.OnHandedOverToSupport();
	}

	protected override void OnMemoryForgotten(AIEventMemory memory)
	{
		memory?.m_AIEvent?.HandBackToGuardsAndInmates();
	}

	public void RepairEvent(AIEventMemory eventMemory)
	{
		AIEvent aIEvent = eventMemory.m_AIEvent;
		GameObject DamagedObject = aIEvent.m_Target;
		if (DamagedObject == null)
		{
			eventMemory.m_bEventValid = false;
			return;
		}
		if (aIEvent == null)
		{
		}
		if (aIEvent.m_Manager == null)
		{
			return;
		}
		EventManager component = aIEvent.m_Manager.GetComponent<EventManager>();
		if (component == null)
		{
			return;
		}
		List<AIEvent> visibleEvents = component.GetVisibleEvents();
		if (visibleEvents != null && visibleEvents.Contains(aIEvent))
		{
			if (aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_DamagedTile || aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_DugHole || aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_MissingTile)
			{
				TeleportToCorrectFloor(ref DamagedObject);
				RepairTileHoleThenWall(DamagedObject.transform.position);
				m_remainingRepairTime = 2f;
			}
			else if (aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_Flooded)
			{
				TeleportToCorrectFloor(ref DamagedObject);
				RepairToilet(DamagedObject);
				m_remainingRepairTime = 5f;
			}
		}
		else
		{
			eventMemory.m_bEventValid = false;
		}
	}

	public bool SkipLastNodeWhenFixing(AIEventMemory eventMemory)
	{
		bool result = true;
		if (eventMemory != null && eventMemory.m_AIEvent != null)
		{
			AIEvent aIEvent = eventMemory.m_AIEvent;
			if (aIEvent.m_EventData != null)
			{
				if (aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_Flooded)
				{
					result = false;
				}
				else if (aIEvent.m_EventData.m_eEventType == AIEvent.EventType.Tile_DamagedTile)
				{
					GenericEventManager genericEventManager = aIEvent.m_Manager as GenericEventManager;
					if (genericEventManager != null && genericEventManager.m_EventHeight == AIEventManager.EventHeight.Wall)
					{
						result = false;
					}
				}
			}
		}
		return result;
	}

	private void TeleportToCorrectFloor(ref GameObject DamagedObject)
	{
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(DamagedObject.transform.position.z);
		if (floor.IsVent())
		{
			floor = FloorManager.GetInstance().DownAFloor(floor);
		}
		m_Character.Teleport(m_Character.m_CachedCurrentPosition, floor);
	}

	public void RepairToilet(GameObject toiletObject)
	{
		Item equippedItem = m_Character.GetEquippedItem();
		if (equippedItem == null || equippedItem.ItemDataID != m_plunger.m_ItemDataID)
		{
			for (int i = 0; i < m_ItemContainer.GetItemCount(); i++)
			{
				Item item = m_ItemContainer.GetItem(i);
				if (item != null && item.ItemDataID == m_plunger.m_ItemDataID)
				{
					m_Character.SetEquippedItem(item);
					break;
				}
			}
		}
		PlumberInteraction component = toiletObject.GetComponent<PlumberInteraction>();
		if (component != null && component.AllowedToInteract(m_Character))
		{
			component.Interact(m_Character);
		}
	}

	public float GetRepairTime()
	{
		return m_remainingRepairTime;
	}
}
