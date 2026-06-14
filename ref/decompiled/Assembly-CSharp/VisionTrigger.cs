using System.Collections.Generic;
using UnityEngine;

public class VisionTrigger : MonoBehaviour, IControlledUpdate
{
	public AICharacter m_AICharacter;

	public CharacterUtil m_CharacterUtil;

	public Transform m_Transform;

	public Character m_Character;

	private CharacterRole m_Role;

	public void Start()
	{
		m_Role = m_Character.m_CharacterRole;
		if (m_Role == CharacterRole.Inmate)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.AI_Events_Slow);
		}
		else
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.AI_Events);
		}
	}

	protected virtual void OnDestroy()
	{
		if (UpdateManager.GetInstance() != null)
		{
			if (m_Role == CharacterRole.Inmate)
			{
				UpdateManager.GetInstance().Unregister(this, UpdateCategory.AI_Events_Slow);
			}
			else
			{
				UpdateManager.GetInstance().Unregister(this, UpdateCategory.AI_Events);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledLateUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledUpdate()
	{
		if (m_Character.GetIsKnockedOut() || m_Character.GetIsDisabled() || m_Character.GetIsMedicalSleeping() || m_AICharacter.IsTemporaryBlind())
		{
			return;
		}
		short row = 0;
		short column = 0;
		short floor = 0;
		if (m_Character.CurrentFloor != null)
		{
			floor = (short)m_Character.CurrentFloor.m_FloorIndex;
		}
		Vector3 position = base.transform.position;
		AIEventManager instance = AIEventManager.GetInstance();
		instance.GetBucketPosition(position, out row, out column);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				List<EventManager> eventManagers = instance.GetEventManagers(row + i, column + j, floor);
				if (eventManagers == null)
				{
					continue;
				}
				for (int k = 0; k < eventManagers.Count; k++)
				{
					EventManager eventManager = eventManagers[k];
					if (eventManager != null)
					{
						CheckEventManager(eventManager);
					}
				}
			}
		}
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	private void CheckEventManager(EventManager eventManager)
	{
		if (eventManager == null)
		{
			return;
		}
		List<AIEvent> visibleEvents = eventManager.GetVisibleEvents();
		int num = visibleEvents?.Count ?? 0;
		if (visibleEvents == null || num <= 0)
		{
			return;
		}
		bool haveCollisionData = false;
		List<Vector3> targetOffsets = eventManager.GetTargetOffsets();
		if (targetOffsets == null)
		{
			return;
		}
		Vector3 position = eventManager.transform.position;
		int count = targetOffsets.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 toPosition = position + targetOffsets[i];
			if (!m_CharacterUtil.LineOfSight(toPosition, out haveCollisionData))
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				AIEvent aIEvent = visibleEvents[j];
				Character targetCharacter = aIEvent.m_TargetCharacter;
				if (!(targetCharacter != null) || (!targetCharacter.m_bIsKnockedOut && !targetCharacter.m_bIsHidden && !targetCharacter.GetIsDisabled()))
				{
					AIEventData eventData = aIEvent.m_EventData;
					if (!(eventData != null) || ((m_Character.m_CharacterRole != 0 || (eventData.m_eEventType != AIEvent.EventType.Character_Attacking && eventData.m_eEventType != AIEvent.EventType.Character_SearchingDesk)) && (m_Character.m_CharacterRole == CharacterRole.Dog || eventData.m_eEventType != AIEvent.EventType.Character_HasContraband)))
					{
						m_AICharacter.AddEvent(aIEvent);
					}
				}
			}
			break;
		}
	}
}
