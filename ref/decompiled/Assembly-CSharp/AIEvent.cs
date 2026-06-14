using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AIEvent
{
	public enum EventType
	{
		Character_Bound,
		Character_NaughtyLocation,
		Character_StandingOnDesk,
		Character_Naked,
		Character_HasContraband,
		Character_Digging,
		Character_Chipping,
		Character_Cutting,
		Character_Looting,
		Character_CarryingObject,
		Character_Attacking,
		Character_Wanted,
		Character_KnockedOut,
		Item_ContrabandOnFloor,
		Item_ContrabandInContainer,
		Tile_DamagedTile,
		Tile_MissingTile,
		Tile_DugHole,
		Tile_Flooded,
		NOT_USED,
		NOT_USED_EITHER,
		Character_Suspicious,
		Character_SearchingDesk,
		Character_Escaping,
		Character_Tardy,
		InvestigateObject,
		InvestigateLocation,
		Character_Disguised,
		ItemMissing,
		Character_Missing,
		Bars_Covered,
		RemoveSwagBag,
		Event_Count
	}

	public delegate void AIEventResolvedCB(AIEvent aiEvent);

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct EventTypeComparer : IEqualityComparer<EventType>
	{
		public bool Equals(EventType x, EventType y)
		{
			return x == y;
		}

		public int GetHashCode(EventType obj)
		{
			return (int)obj;
		}
	}

	public AIEventData m_EventData;

	public Character m_CharacterResponsible;

	public EventManager m_Manager;

	public GameObject m_Target;

	public Character m_TargetCharacter;

	public List<AICharacter> m_InmateResponders;

	public List<AICharacter> m_GuardResponders;

	public List<AICharacter> m_SupportResponders;

	public List<AICharacter> m_DogResponders;

	private float m_fInmateCooldownEpoch;

	private float m_fGuardCooldownEpoch;

	private float m_fDogCooldownEpoch;

	private uint m_EventID;

	private bool m_bFirstResponderSpeechClaimed;

	public static EventTypeComparer EventTComparer = default(EventTypeComparer);

	public event AIEventResolvedCB EventStopped;

	public AIEvent(AIEventData eventData, Character characterResponsible, Character targetCharacter, GameObject target, EventManager manager)
	{
		m_EventData = eventData;
		m_CharacterResponsible = characterResponsible;
		m_Manager = manager;
		m_Target = target;
		m_TargetCharacter = targetCharacter;
		InitResponderLists();
	}

	public Vector3 GetPosition()
	{
		if (m_EventData != null && m_EventData.m_eEventType == EventType.ItemMissing)
		{
			return m_Manager.GetWorldPosition();
		}
		if (m_Target != null)
		{
			return m_Target.transform.position;
		}
		if (m_Manager != null)
		{
			return m_Manager.GetWorldPosition();
		}
		return Vector3.zero;
	}

	private void InitResponderLists()
	{
		if (m_InmateResponders == null && m_EventData.m_MaxInmateResponders > 0)
		{
			m_InmateResponders = new List<AICharacter>();
		}
		if (m_GuardResponders == null && m_EventData.m_MaxGuardResponders > 0)
		{
			m_GuardResponders = new List<AICharacter>();
		}
		if (m_SupportResponders == null && m_EventData.m_MaxSupportResponders > 0)
		{
			m_SupportResponders = new List<AICharacter>();
		}
		if (m_DogResponders == null && m_EventData.m_MaxDogResponders > 0)
		{
			m_DogResponders = new List<AICharacter>();
		}
	}

	public void OnEventStarted()
	{
		AIEventManager.GetInstance().RegisterEvent(this);
		AIEventManager.GetInstance().m_GlobalEventStarted.TryGetValue(m_EventData.m_eEventType, out var value);
		if (value != null)
		{
			for (int i = 0; i < value.Count; i++)
			{
				value[i].callback(this);
			}
		}
	}

	public void OnEventStopped()
	{
		AIEventManager instance = AIEventManager.GetInstance();
		if (instance != null)
		{
			instance.UnRegisterEvent(this);
		}
		m_bFirstResponderSpeechClaimed = false;
		if (this.EventStopped != null)
		{
			this.EventStopped(this);
		}
	}

	public bool IsWellFormed()
	{
		return AIEventManager.IsWellFormed(this);
	}

	public uint GetEventID()
	{
		if (m_EventID == 0)
		{
			m_EventID = AIEventManager.GetEventIDForEvent(this);
		}
		return m_EventID;
	}

	public bool SlotsAvaliable(CharacterRole role)
	{
		if (CooldownActive(role))
		{
			return false;
		}
		int maxSlots = 0;
		List<AICharacter> slotList = GetSlotList(role, out maxSlots);
		if (slotList != null)
		{
			return slotList.Count < maxSlots;
		}
		return false;
	}

	public float TakeSlot(CharacterRole role, AICharacter character)
	{
		int maxSlots;
		List<AICharacter> slotList = GetSlotList(role, out maxSlots);
		slotList?.Add(character);
		if (slotList == null || maxSlots == 0)
		{
			return 0f;
		}
		return (float)slotList.Count / (float)maxSlots;
	}

	public void ReturnSlot(CharacterRole role, AICharacter character)
	{
		List<AICharacter> slotList = GetSlotList(role);
		if (slotList != null && slotList.Contains(character))
		{
			slotList.Remove(character);
		}
	}

	public List<AICharacter> GetSlotList(CharacterRole role)
	{
		int maxSlots;
		return GetSlotList(role, out maxSlots);
	}

	private List<AICharacter> GetSlotList(CharacterRole role, out int maxSlots)
	{
		switch (role)
		{
		case CharacterRole.Guard:
			maxSlots = m_EventData.m_MaxGuardResponders;
			return m_GuardResponders;
		case CharacterRole.Dog:
			maxSlots = m_EventData.m_MaxDogResponders;
			return m_DogResponders;
		case CharacterRole.Inmate:
			maxSlots = m_EventData.m_MaxInmateResponders;
			return m_InmateResponders;
		case CharacterRole.Medic:
		case CharacterRole.Maintenance:
			maxSlots = m_EventData.m_MaxSupportResponders;
			return m_SupportResponders;
		default:
			maxSlots = 0;
			return null;
		}
	}

	private bool CooldownActive(CharacterRole role)
	{
		return role switch
		{
			CharacterRole.Inmate => m_fInmateCooldownEpoch > UpdateManager.time, 
			CharacterRole.Guard => m_fGuardCooldownEpoch > UpdateManager.time, 
			CharacterRole.Dog => m_fDogCooldownEpoch > UpdateManager.time, 
			_ => false, 
		};
	}

	public void StartCooldown(float cooldownTime, CharacterRole role)
	{
		switch (role)
		{
		case CharacterRole.Inmate:
			m_fInmateCooldownEpoch = Mathf.Max(UpdateManager.time + cooldownTime, m_fInmateCooldownEpoch);
			break;
		case CharacterRole.Guard:
			m_fGuardCooldownEpoch = Mathf.Max(UpdateManager.time + cooldownTime, m_fGuardCooldownEpoch);
			break;
		case CharacterRole.Dog:
			m_fDogCooldownEpoch = Mathf.Max(UpdateManager.time + cooldownTime, m_fDogCooldownEpoch);
			break;
		}
	}

	public void OnHandedOverToSupport()
	{
		if (m_GuardResponders != null)
		{
			for (int num = m_GuardResponders.Count - 1; num >= 0; num--)
			{
				if (!(m_GuardResponders[num] == null))
				{
					m_GuardResponders[num].ForgetEvent(this);
				}
			}
			m_GuardResponders = null;
		}
		if (m_InmateResponders != null)
		{
			for (int num2 = m_InmateResponders.Count - 1; num2 >= 0; num2--)
			{
				if (!(m_InmateResponders[num2] == null))
				{
					m_InmateResponders[num2].ForgetEvent(this);
				}
			}
			m_InmateResponders = null;
		}
		if (m_DogResponders == null)
		{
			return;
		}
		for (int num3 = m_DogResponders.Count - 1; num3 >= 0; num3--)
		{
			if (!(m_DogResponders[num3] == null))
			{
				m_DogResponders[num3].ForgetEvent(this);
			}
		}
		m_DogResponders = null;
	}

	public void HandBackToGuardsAndInmates()
	{
		if (m_SupportResponders != null)
		{
			for (int num = m_SupportResponders.Count - 1; num >= 0; num--)
			{
				if (!(m_SupportResponders[num] == null))
				{
					m_SupportResponders[num].ForgetEvent(this);
				}
			}
			m_SupportResponders = null;
		}
		InitResponderLists();
	}

	public bool IsFirstResponderSpeechClaimed()
	{
		return m_bFirstResponderSpeechClaimed;
	}

	public void ClaimFirstResponderSpeech()
	{
		m_bFirstResponderSpeechClaimed = true;
	}

	public override string ToString()
	{
		string empty = string.Empty;
		empty += m_EventData.m_eEventType;
		empty = empty + " Resp " + m_CharacterResponsible;
		empty = empty + " Targ " + m_Target;
		if (m_GuardResponders != null)
		{
			empty = empty + " G:" + m_GuardResponders.Count;
		}
		if (m_InmateResponders != null)
		{
			empty = empty + " I:" + m_InmateResponders.Count;
		}
		if (m_SupportResponders != null)
		{
			empty = empty + " S:" + m_SupportResponders.Count;
		}
		if (m_DogResponders != null)
		{
			empty = empty + " D:" + m_DogResponders.Count;
		}
		return empty;
	}
}
