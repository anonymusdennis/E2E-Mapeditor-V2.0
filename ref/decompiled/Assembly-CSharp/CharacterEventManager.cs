using System.Collections.Generic;

public abstract class CharacterEventManager : EventManager, IControlledUpdate
{
	public Character m_Character;

	[ReadOnly]
	public List<AIEvent> m_VisibleEvents = new List<AIEvent>();

	public virtual void IsKnockedOut(bool enable, Character characterResponsible)
	{
	}

	public virtual void IsBound(bool enable, Character characterResponsible)
	{
	}

	public virtual void IsNaked(bool enable)
	{
	}

	public virtual void IsNaughtyLocation(bool enable)
	{
	}

	public virtual void HasContraband(bool enable)
	{
	}

	public virtual void IsTardy(bool enable)
	{
	}

	public virtual void IsStandingOnDesk(bool enable)
	{
	}

	public virtual void IsCarryingObject(bool enable)
	{
	}

	public virtual void IsWanted(bool enable)
	{
	}

	public virtual void IsSuspicious(bool enable)
	{
	}

	public virtual void IsDigging(bool enable)
	{
	}

	public virtual void IsChipping(bool enable)
	{
	}

	public virtual void IsCutting(bool enable)
	{
	}

	public virtual void IsLooting(bool enable)
	{
	}

	public virtual void IsSearchingDesk(bool enable)
	{
	}

	public virtual void IsAttacking(bool enable)
	{
	}

	public virtual void IsDisguised(bool enable)
	{
	}

	public override uint GetEventManagerID()
	{
		return AIEventManager.GetEventManagerIDForNetObject(m_NetView.viewID);
	}

	public override List<AIEvent> GetVisibleEvents()
	{
		return m_VisibleEvents;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		UpdateManager.GetInstance().Register(this, UpdateCategory.AI_Events);
		return base.StartInit();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (null != UpdateManager.GetInstance())
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.AI_Events);
		}
		m_VisibleEvents.Clear();
	}

	public void ControlledUpdate()
	{
		if (m_VisibleEvents.Count > 0)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		int newFloor = 0;
		if (m_Character.CurrentFloor != null)
		{
			newFloor = m_Character.CurrentFloor.m_FloorIndex;
		}
		AIEventManager.GetInstance().UpdatePosition(this, newFloor);
	}

	public void ControlledFixedUpdate()
	{
	}

	protected void SetVisible(ref AIEvent aiEvent, ref bool toggle, bool enable)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (enable && !toggle)
		{
			if (!m_Character.GetIsDisabled())
			{
				toggle = true;
				aiEvent.OnEventStarted();
				m_VisibleEvents.Add(aiEvent);
				UpdatePosition();
			}
		}
		else
		{
			if (enable || !toggle)
			{
				return;
			}
			toggle = false;
			aiEvent.OnEventStopped();
			m_VisibleEvents.Remove(aiEvent);
			if (m_VisibleEvents.Count == 0)
			{
				AIEventManager instance = AIEventManager.GetInstance();
				if (instance != null)
				{
					instance.RemoveManager(this);
				}
			}
		}
	}

	public abstract void OnCharacterDisabled();

	public abstract AIEvent GetAttackingAIEvent();

	public virtual AIEvent GetSearchingDeskAIEvent()
	{
		return null;
	}

	public virtual AIEvent GetSuspiciousAIEvent()
	{
		return null;
	}

	public virtual AIEvent GetEscapingAIEvent()
	{
		return null;
	}

	public virtual AIEvent GetKnockedOutAIEvent()
	{
		return null;
	}

	public virtual AIEvent GetBoundAIEvent()
	{
		return null;
	}

	public virtual AIEvent GetDisguisedAIEvent()
	{
		return null;
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
}
