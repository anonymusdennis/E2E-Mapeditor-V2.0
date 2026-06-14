using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AIEventMemory
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct AIEventMemoryComparer : IEqualityComparer<AIEventMemory>
	{
		public bool Equals(AIEventMemory x, AIEventMemory y)
		{
			return x.GetEventID() == y.GetEventID();
		}

		public int GetHashCode(AIEventMemory obj)
		{
			return obj.GetHashCode();
		}
	}

	public AICharacter m_Owner;

	public AIEvent m_AIEvent;

	public bool m_bEventValid = true;

	public bool m_bFlagToForget;

	public float m_fCreationTime;

	public Vector3 m_vEventLocation;

	public bool m_bForgettableEvent;

	public bool m_bForgettableInSight;

	public float m_SlotPosition;

	public float m_fEventForgetInSightTime;

	public float m_fEventForgetOutOfSightTime;

	public float m_fEventHeatReoccuranceTime;

	public float m_fEventOracleTime;

	public float m_fTimeSinceSeen;

	public float m_fTimeSinceMemoryStarted;

	public float m_fTimeUntilHeatReoccurance;

	public bool m_bAlwaysUpdateEventPosition;

	public GameObject m_Target;

	public Transform m_TargetTransform;

	public Character m_TargetCharacter;

	public Character m_CharacterResponsible;

	public AIEvent.EventType m_eEventType;

	public static AIEventMemoryComparer EventComparer = default(AIEventMemoryComparer);

	public AIEventMemory(AIEvent aiEvent, AICharacter owner, float slotPosition)
	{
		aiEvent.EventStopped += EventStoppped;
		m_AIEvent = aiEvent;
		m_fCreationTime = UpdateManager.time;
		m_eEventType = aiEvent.m_EventData.m_eEventType;
		m_SlotPosition = slotPosition;
		if (aiEvent.m_Target != null)
		{
			m_Target = aiEvent.m_Target;
			m_TargetTransform = m_Target.transform;
		}
		if (aiEvent.m_TargetCharacter != null)
		{
			m_TargetCharacter = aiEvent.m_TargetCharacter;
		}
		m_CharacterResponsible = aiEvent.m_CharacterResponsible;
		m_Owner = owner;
		CharacterRole characterRole = CharacterRole.Guard;
		if (m_Owner != null && m_Owner.m_Character != null)
		{
			characterRole = m_Owner.m_Character.m_CharacterRole;
		}
		bool flag = false;
		bool flag2 = false;
		switch (characterRole)
		{
		case CharacterRole.Guard:
		case CharacterRole.Dog:
			flag = aiEvent.m_EventData.m_bForgettableEventGuard;
			flag2 = flag & aiEvent.m_EventData.m_bForgettableInSightGuard;
			break;
		case CharacterRole.Inmate:
			flag = aiEvent.m_EventData.m_bForgettableEventInmate;
			flag2 = flag & aiEvent.m_EventData.m_bForgettableInSightInmate;
			break;
		}
		if (flag)
		{
			m_bForgettableEvent = true;
			m_fEventForgetOutOfSightTime = aiEvent.m_EventData.m_fEventOutOfSightForgetTime;
			if (flag2)
			{
				m_bForgettableInSight = true;
				m_fEventForgetInSightTime = aiEvent.m_EventData.m_fEventForgetInSight;
			}
			m_fEventOracleTime = aiEvent.m_EventData.m_fEventOracleTime;
			m_fEventHeatReoccuranceTime = aiEvent.m_EventData.m_ReoccuringHeatTime;
			m_fTimeUntilHeatReoccurance = m_fEventHeatReoccuranceTime;
			m_fTimeSinceSeen = 0f;
			m_Owner.TickMemory += TickForgettable;
		}
		if (aiEvent.m_EventData.m_bAlwaysUpdateEventPosition)
		{
			m_bAlwaysUpdateEventPosition = true;
			m_Owner.TickMemory += UpdateEventPosition;
		}
		m_vEventLocation = m_AIEvent.GetPosition();
	}

	public bool IsWellFormed()
	{
		return m_AIEvent != null && m_AIEvent.IsWellFormed();
	}

	public uint GetEventID()
	{
		if (m_AIEvent == null)
		{
			return 0u;
		}
		return m_AIEvent.GetEventID();
	}

	public void TickForgettable(float timeDelta)
	{
		bool flag = false;
		m_fTimeSinceMemoryStarted += timeDelta;
		if (m_bForgettableInSight && m_fTimeSinceMemoryStarted > m_fEventForgetInSightTime)
		{
			flag = true;
		}
		bool haveCollisionData = false;
		bool flag2 = m_Owner.m_CharacterUtil.LineOfSight(m_Target, out haveCollisionData);
		if (flag2 & (m_TargetCharacter == null || !m_TargetCharacter.m_bIsHidden))
		{
			m_vEventLocation = m_AIEvent.GetPosition();
			m_fTimeSinceSeen = 0f;
			if (m_fEventHeatReoccuranceTime > 0f)
			{
				m_fTimeUntilHeatReoccurance -= timeDelta;
				if (m_fTimeUntilHeatReoccurance <= 0f)
				{
					m_CharacterResponsible.m_CharacterStats.IncreaseHeat(m_AIEvent.m_EventData.m_GuardHeatIncrease);
					m_fTimeUntilHeatReoccurance = m_fEventHeatReoccuranceTime;
				}
			}
		}
		else
		{
			m_fTimeSinceSeen += timeDelta;
			if (m_fTimeSinceSeen < m_fEventOracleTime)
			{
				if (m_TargetCharacter == null || !m_TargetCharacter.m_bIsHidden)
				{
					m_vEventLocation = m_AIEvent.GetPosition();
				}
			}
			else if (m_fTimeSinceSeen > m_fEventForgetOutOfSightTime)
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_Owner.TickMemory -= TickForgettable;
			m_Owner.TickMemory -= UpdateEventPosition;
			m_Owner.ForgetEvent(this);
		}
	}

	public void UpdateEventPosition(float timeDelta)
	{
		if (m_bEventValid)
		{
			m_vEventLocation = m_AIEvent.GetPosition();
		}
	}

	public GameObject GetTarget()
	{
		return m_Target;
	}

	public static GameObject GetTarget(AIEventMemory memory)
	{
		return memory.m_Target;
	}

	public static Character GetCharacterTarget(AIEventMemory memory)
	{
		return memory.m_TargetCharacter;
	}

	public void EventStoppped(AIEvent aiEvent)
	{
		m_bEventValid = false;
	}

	public void OnForgetMemory()
	{
		m_Owner.TickMemory -= TickForgettable;
		m_Owner.TickMemory -= UpdateEventPosition;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is AIEventMemory aIEventMemory))
		{
			return false;
		}
		if (!aIEventMemory.IsWellFormed())
		{
			return false;
		}
		return GetEventID() == aIEventMemory.GetEventID();
	}

	public override int GetHashCode()
	{
		return GetEventID().GetHashCode();
	}

	public override string ToString()
	{
		return m_eEventType.ToString() + ((!(m_CharacterResponsible != null)) ? string.Empty : (":" + m_CharacterResponsible.name)) + ":" + $"{GetEventID():X}";
	}
}
