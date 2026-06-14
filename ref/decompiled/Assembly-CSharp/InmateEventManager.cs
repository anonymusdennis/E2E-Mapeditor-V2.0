public class InmateEventManager : CharacterEventManager
{
	public AIEventData m_IsBound;

	private AIEvent m_IsBoundEvent;

	private bool m_IsBoundVisible;

	public AIEventData m_IsNaughtyLocation;

	private AIEvent m_IsNaughtyLocationEvent;

	private bool m_IsNaughtyLocationVisible;

	public AIEventData m_IsTardy;

	private AIEvent m_IsTardyEvent;

	private bool m_IsTardyVisible;

	public AIEventData m_IsStandingOnDesk;

	private AIEvent m_IsStandingOnDeskEvent;

	private bool m_IsStandingOnDeskVisible;

	public AIEventData m_IsNaked;

	private AIEvent m_IsNakedEvent;

	private bool m_IsNakedVisible;

	public AIEventData m_HasContraband;

	private AIEvent m_HasContrabandEvent;

	private bool m_HasContrabandVisible;

	public AIEventData m_IsDigging;

	private AIEvent m_IsDiggingEvent;

	private bool m_IsDiggingVisible;

	public AIEventData m_IsChipping;

	private AIEvent m_IsChippingEvent;

	private bool m_IsChippingVisible;

	public AIEventData m_IsCutting;

	private AIEvent m_IsCuttingEvent;

	private bool m_IsCuttingVisible;

	public AIEventData m_IsLooting;

	private AIEvent m_IsLootingEvent;

	private bool m_IsLootingVisible;

	public AIEventData m_IsSearchingDesk;

	private AIEvent m_IsSearchingDeskEvent;

	private bool m_IsSearchingDeskVisible;

	public AIEventData m_IsCarryingObject;

	private AIEvent m_IsCarryingObjectEvent;

	private bool m_IsCarryingObjectVisible;

	public AIEventData m_IsWanted;

	private AIEvent m_IsWantedEvent;

	private bool m_IsWantedVisible;

	public AIEventData m_IsSuspicious;

	private AIEvent m_IsSuspiciousEvent;

	private bool m_IsSuspiciousVisible;

	public AIEventData m_IsKnockedOut;

	private AIEvent m_IsKnockedOutEvent;

	private bool m_IsKnockedOutVisible;

	public AIEventData m_IsAttacking;

	private AIEvent m_IsAttackingEvent;

	private bool m_IsAttackingVisible;

	public AIEventData m_IsEscaping;

	private AIEvent m_IsEscapingEvent;

	public AIEventData m_IsDisguised;

	private AIEvent m_IsDisguisedEvent;

	private bool m_IsDisguisedVisible;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_IsBound = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsBound);
			m_IsNaughtyLocation = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsNaughtyLocation);
			m_IsTardy = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsTardy);
			m_IsStandingOnDesk = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsStandingOnDesk);
			m_IsNaked = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsNaked);
			m_HasContraband = ConfigManager.GetInstance().ApplyAIEventOverride(m_HasContraband);
			m_IsDigging = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsDigging);
			m_IsChipping = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsChipping);
			m_IsCutting = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsCutting);
			m_IsLooting = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsLooting);
			m_IsSearchingDesk = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsSearchingDesk);
			m_IsCarryingObject = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsCarryingObject);
			m_IsWanted = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsWanted);
			m_IsSuspicious = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsSuspicious);
			m_IsKnockedOut = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsKnockedOut);
			m_IsAttacking = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsAttacking);
			m_IsEscaping = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsEscaping);
			m_IsDisguised = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsDisguised);
		}
		return base.StartInit();
	}

	public override void IsKnockedOut(bool enable, Character characterResponsible)
	{
		if (enable)
		{
			m_IsKnockedOutEvent = GetEventByType(AIEvent.EventType.Character_KnockedOut);
			m_IsKnockedOutEvent.m_CharacterResponsible = characterResponsible;
		}
		SetVisible(ref m_IsKnockedOutEvent, ref m_IsKnockedOutVisible, enable);
	}

	public override void IsBound(bool enable, Character characterResponsible)
	{
		if (enable)
		{
			m_IsBoundEvent = GetEventByType(AIEvent.EventType.Character_Bound);
			m_IsBoundEvent.m_CharacterResponsible = characterResponsible;
		}
		SetVisible(ref m_IsBoundEvent, ref m_IsBoundVisible, enable);
	}

	public override void IsNaked(bool enable)
	{
		if (enable && m_IsNakedEvent == null)
		{
			m_IsNakedEvent = GetEventByType(AIEvent.EventType.Character_Naked);
		}
		SetVisible(ref m_IsNakedEvent, ref m_IsNakedVisible, enable);
	}

	public override void IsNaughtyLocation(bool enable)
	{
		if (enable && m_IsNaughtyLocationEvent == null)
		{
			m_IsNaughtyLocationEvent = GetEventByType(AIEvent.EventType.Character_NaughtyLocation);
		}
		SetVisible(ref m_IsNaughtyLocationEvent, ref m_IsNaughtyLocationVisible, enable);
	}

	public override void HasContraband(bool enable)
	{
		if (enable && m_HasContrabandEvent == null)
		{
			m_HasContrabandEvent = GetEventByType(AIEvent.EventType.Character_HasContraband);
		}
		SetVisible(ref m_HasContrabandEvent, ref m_HasContrabandVisible, enable);
	}

	public override void IsTardy(bool enable)
	{
		if (enable && m_IsTardyEvent == null)
		{
			m_IsTardyEvent = GetEventByType(AIEvent.EventType.Character_Tardy);
		}
		SetVisible(ref m_IsTardyEvent, ref m_IsTardyVisible, enable);
	}

	public override void IsStandingOnDesk(bool enable)
	{
		if (enable && m_IsStandingOnDeskEvent == null)
		{
			m_IsStandingOnDeskEvent = GetEventByType(AIEvent.EventType.Character_StandingOnDesk);
		}
		SetVisible(ref m_IsStandingOnDeskEvent, ref m_IsStandingOnDeskVisible, enable);
	}

	public override void IsCarryingObject(bool enable)
	{
		if (enable && m_IsCarryingObjectEvent == null)
		{
			m_IsCarryingObjectEvent = GetEventByType(AIEvent.EventType.Character_CarryingObject);
		}
		SetVisible(ref m_IsCarryingObjectEvent, ref m_IsCarryingObjectVisible, enable);
	}

	public override void IsWanted(bool enable)
	{
		if (enable && m_IsWantedEvent == null)
		{
			m_IsWantedEvent = GetEventByType(AIEvent.EventType.Character_Wanted);
		}
		SetVisible(ref m_IsWantedEvent, ref m_IsWantedVisible, enable);
	}

	public override void IsSuspicious(bool enable)
	{
		if (enable && m_IsSuspiciousEvent == null)
		{
			m_IsSuspiciousEvent = GetSuspiciousAIEvent();
		}
		SetVisible(ref m_IsSuspiciousEvent, ref m_IsSuspiciousVisible, enable);
	}

	public override void IsDigging(bool enable)
	{
		if (enable && m_IsDiggingEvent == null)
		{
			m_IsDiggingEvent = GetEventByType(AIEvent.EventType.Character_Digging);
		}
		SetVisible(ref m_IsDiggingEvent, ref m_IsDiggingVisible, enable);
	}

	public override void IsChipping(bool enable)
	{
		if (enable && m_IsChippingEvent == null)
		{
			m_IsChippingEvent = GetEventByType(AIEvent.EventType.Character_Chipping);
		}
		SetVisible(ref m_IsChippingEvent, ref m_IsChippingVisible, enable);
	}

	public override void IsCutting(bool enable)
	{
		if (enable && m_IsCuttingEvent == null)
		{
			m_IsCuttingEvent = GetEventByType(AIEvent.EventType.Character_Cutting);
		}
		SetVisible(ref m_IsCuttingEvent, ref m_IsCuttingVisible, enable);
	}

	public override void IsLooting(bool enable)
	{
		if (enable && m_IsLootingEvent == null)
		{
			m_IsLootingEvent = GetEventByType(AIEvent.EventType.Character_Looting);
		}
		SetVisible(ref m_IsLootingEvent, ref m_IsLootingVisible, enable);
	}

	public override void IsSearchingDesk(bool enable)
	{
		if (enable && m_IsSearchingDeskEvent == null)
		{
			m_IsSearchingDeskEvent = GetSearchingDeskAIEvent();
		}
		SetVisible(ref m_IsSearchingDeskEvent, ref m_IsSearchingDeskVisible, enable);
	}

	public override void IsAttacking(bool enable)
	{
		if (enable && m_IsAttackingEvent == null)
		{
			m_IsAttackingEvent = GetAttackingAIEvent();
		}
		SetVisible(ref m_IsAttackingEvent, ref m_IsAttackingVisible, enable);
	}

	public override void IsDisguised(bool enable)
	{
		if (enable && m_IsDisguisedEvent == null)
		{
			m_IsDisguisedEvent = GetDisguisedAIEvent();
		}
		SetVisible(ref m_IsDisguisedEvent, ref m_IsDisguisedVisible, enable);
	}

	public override void OnCharacterDisabled()
	{
		IsKnockedOut(enable: false, null);
		IsBound(enable: false, null);
		IsNaked(enable: false);
		IsNaughtyLocation(enable: false);
		HasContraband(enable: false);
		IsTardy(enable: false);
		IsStandingOnDesk(enable: false);
		IsCarryingObject(enable: false);
		IsWanted(enable: false);
		IsSuspicious(enable: false);
		IsDigging(enable: false);
		IsChipping(enable: false);
		IsCutting(enable: false);
		IsLooting(enable: false);
		IsSearchingDesk(enable: false);
		IsAttacking(enable: false);
		IsDisguised(enable: false);
	}

	public override AIEvent GetSearchingDeskAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_SearchingDesk);
	}

	public override AIEvent GetSuspiciousAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_Suspicious);
	}

	public override AIEvent GetEscapingAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_Escaping);
	}

	public override AIEvent GetAttackingAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_Attacking);
	}

	public override AIEvent GetBoundAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_Bound);
	}

	public override AIEvent GetKnockedOutAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_KnockedOut);
	}

	public override AIEvent GetDisguisedAIEvent()
	{
		return GetEventByType(AIEvent.EventType.Character_Disguised);
	}

	public override AIEvent GetEventByType(AIEvent.EventType eventType)
	{
		switch (eventType)
		{
		case AIEvent.EventType.Character_Bound:
			if (m_IsBoundEvent == null)
			{
				m_IsBoundEvent = new AIEvent(m_IsBound, null, m_Character, m_Character.gameObject, this);
			}
			return m_IsBoundEvent;
		case AIEvent.EventType.Character_KnockedOut:
			if (m_IsKnockedOutEvent == null)
			{
				m_IsKnockedOutEvent = new AIEvent(m_IsKnockedOut, null, m_Character, m_Character.gameObject, this);
			}
			return m_IsKnockedOutEvent;
		case AIEvent.EventType.Character_Attacking:
			if (m_IsAttackingEvent == null)
			{
				m_IsAttackingEvent = new AIEvent(m_IsAttacking, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsAttackingEvent;
		case AIEvent.EventType.Character_Escaping:
			if (m_IsEscapingEvent == null)
			{
				m_IsEscapingEvent = new AIEvent(m_IsEscaping, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsEscapingEvent;
		case AIEvent.EventType.Character_Disguised:
			if (m_IsDisguisedEvent == null)
			{
				m_IsDisguisedEvent = new AIEvent(m_IsDisguised, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsDisguisedEvent;
		case AIEvent.EventType.Character_Suspicious:
			if (m_IsSuspiciousEvent == null)
			{
				m_IsSuspiciousEvent = new AIEvent(m_IsSuspicious, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsSuspiciousEvent;
		case AIEvent.EventType.Character_SearchingDesk:
			if (m_IsSearchingDeskEvent == null)
			{
				m_IsSearchingDeskEvent = new AIEvent(m_IsSearchingDesk, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsSearchingDeskEvent;
		case AIEvent.EventType.Character_Naked:
			if (m_IsNakedEvent == null)
			{
				m_IsNakedEvent = new AIEvent(m_IsNaked, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsNakedEvent;
		case AIEvent.EventType.Character_NaughtyLocation:
			if (m_IsNaughtyLocationEvent == null)
			{
				m_IsNaughtyLocationEvent = new AIEvent(m_IsNaughtyLocation, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsNaughtyLocationEvent;
		case AIEvent.EventType.Character_HasContraband:
			if (m_HasContrabandEvent == null)
			{
				m_HasContrabandEvent = new AIEvent(m_HasContraband, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_HasContrabandEvent;
		case AIEvent.EventType.Character_Tardy:
			if (m_IsTardyEvent == null)
			{
				m_IsTardyEvent = new AIEvent(m_IsTardy, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsTardyEvent;
		case AIEvent.EventType.Character_StandingOnDesk:
			if (m_IsStandingOnDeskEvent == null)
			{
				m_IsStandingOnDeskEvent = new AIEvent(m_IsStandingOnDesk, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsStandingOnDeskEvent;
		case AIEvent.EventType.Character_CarryingObject:
			if (m_IsCarryingObjectEvent == null)
			{
				m_IsCarryingObjectEvent = new AIEvent(m_IsCarryingObject, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsCarryingObjectEvent;
		case AIEvent.EventType.Character_Wanted:
			if (m_IsWantedEvent == null)
			{
				m_IsWantedEvent = new AIEvent(m_IsWanted, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsWantedEvent;
		case AIEvent.EventType.Character_Digging:
			if (m_IsDiggingEvent == null)
			{
				m_IsDiggingEvent = new AIEvent(m_IsDigging, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsDiggingEvent;
		case AIEvent.EventType.Character_Chipping:
			if (m_IsChippingEvent == null)
			{
				m_IsChippingEvent = new AIEvent(m_IsChipping, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsChippingEvent;
		case AIEvent.EventType.Character_Cutting:
			if (m_IsCuttingEvent == null)
			{
				m_IsCuttingEvent = new AIEvent(m_IsCutting, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsCuttingEvent;
		case AIEvent.EventType.Character_Looting:
			if (m_IsLootingEvent == null)
			{
				m_IsLootingEvent = new AIEvent(m_IsLooting, m_Character, m_Character, m_Character.gameObject, this);
			}
			return m_IsLootingEvent;
		default:
			return null;
		}
	}
}
