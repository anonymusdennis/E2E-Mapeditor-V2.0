public class GuardEventManager : CharacterEventManager
{
	public AIEventData m_IsBound;

	private AIEvent m_IsBoundEvent;

	private bool m_IsBoundVisible;

	public AIEventData m_IsKnockedOut;

	private AIEvent m_IsKnockedOutEvent;

	private bool m_IsKnockedOutVisible;

	public AIEventData m_IsAttacking;

	private AIEvent m_IsAttackingEvent;

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (ConfigManager.GetInstance() != null)
		{
			m_IsBound = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsBound);
			m_IsKnockedOut = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsKnockedOut);
			m_IsAttacking = ConfigManager.GetInstance().ApplyAIEventOverride(m_IsAttacking);
		}
		return base.StartInit();
	}

	public override void OnDestroy()
	{
		m_IsBoundEvent = null;
		m_IsKnockedOutEvent = null;
		m_IsAttackingEvent = null;
		base.OnDestroy();
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

	public override void OnCharacterDisabled()
	{
		IsKnockedOut(enable: false, null);
		IsBound(enable: false, null);
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
		default:
			return null;
		}
	}
}
