using System.Collections.Generic;
using UnityEngine;

public class AICharacter_Medic : AICharacter
{
	private List<RoomBlob> m_InfirmaryRooms;

	private float m_fWaitForPickupTime = 3f;

	protected override void OnStart()
	{
		NPCManager.GetInstance().AddMedic(this);
		m_InfirmaryRooms = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.Infirmary);
		m_fWaitForPickupTime = ConfigManager.GetInstance().aiConfig.GetMedicWaitForPickUpTime();
	}

	protected override void OnAddingEventToMemory(AIEvent aiEvent, AIEventMemory memory, bool silent)
	{
		aiEvent.OnHandedOverToSupport();
		if (!silent)
		{
			PauseMedicOnFirstKOEvent();
		}
	}

	private void PauseMedicOnFirstKOEvent()
	{
		List<AIEventMemory> eventMemories = GetEventMemories(AIEvent.EventType.Character_KnockedOut);
		List<AIEventMemory> eventMemories2 = GetEventMemories(AIEvent.EventType.Character_Escaping);
		int num = 0;
		num += eventMemories?.Count ?? 0;
		num += eventMemories2?.Count ?? 0;
		if (num == 1)
		{
			bool flag = false;
			flag |= SearchForPlayerMemory(eventMemories);
			if (!(flag | SearchForPlayerMemory(eventMemories2)))
			{
				m_Character.PauseMovement(m_fWaitForPickupTime);
			}
			else
			{
				m_Character.PauseMovement(0f, force: true);
			}
		}
	}

	private bool SearchForPlayerMemory(List<AIEventMemory> memories)
	{
		if (memories != null)
		{
			for (int i = 0; i < memories.Count; i++)
			{
				AIEventMemory aIEventMemory = memories[i];
				if (aIEventMemory != null && aIEventMemory.m_TargetCharacter != null && aIEventMemory.m_TargetCharacter.m_CharacterStats != null && aIEventMemory.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnMemoryForgotten(AIEventMemory memory)
	{
		memory?.m_AIEvent?.HandBackToGuardsAndInmates();
	}

	public GameObject GetSolitaryBed(AIEventMemory memory)
	{
		Character character = null;
		if (memory.m_Target != null)
		{
			character = memory.m_Target.GetComponent<Character>();
			bool success;
			RoomBlob_Solitary cellForCharacter = SolitaryManager.GetInstance().GetCellForCharacter(character, out success);
			if (success)
			{
				return cellForCharacter.m_Bed.gameObject;
			}
		}
		return GetInfirmaryBed(memory);
	}

	public GameObject GetInfirmaryBed(AIEventMemory memory)
	{
		if (m_InfirmaryRooms == null)
		{
			return null;
		}
		InteractiveObject interactiveObject = null;
		if (LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport)
		{
			Character targetCharacter = memory.m_TargetCharacter;
			if (targetCharacter != null && targetCharacter.m_CharacterStats != null && targetCharacter.m_CharacterStats.m_bIsPlayer)
			{
				RoomBlob myCell = targetCharacter.GetMyCell();
				if (myCell != null)
				{
					RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
					if (roomBlobData != null)
					{
						interactiveObject = roomBlobData.GetCellObject(typeof(BedInteraction), targetCharacter);
					}
				}
			}
		}
		if (interactiveObject == null)
		{
			interactiveObject = GetNearestMedicBed();
		}
		if (interactiveObject == null)
		{
			return null;
		}
		if (!interactiveObject.ObjectReserved())
		{
			interactiveObject.ReserveObject(m_Character);
		}
		return interactiveObject.gameObject;
	}

	private InteractiveObject GetNearestMedicBed()
	{
		RoomBlob currentRoom = m_Character.m_CurrentLocation;
		m_InfirmaryRooms.Sort(delegate(RoomBlob a, RoomBlob b)
		{
			float distanceEstimate = RoomUtility.GetInstance().GetDistanceEstimate(ref currentRoom, ref a);
			float distanceEstimate2 = RoomUtility.GetInstance().GetDistanceEstimate(ref currentRoom, ref b);
			return distanceEstimate.CompareTo(distanceEstimate2);
		});
		InteractiveObject interactiveObject = null;
		for (int i = 0; i < m_InfirmaryRooms.Count; i++)
		{
			if (m_InfirmaryRooms[i] == null)
			{
				continue;
			}
			RoomBlob_Infirmary roomBlobData = m_InfirmaryRooms[i].GetRoomBlobData<RoomBlob_Infirmary>();
			if (roomBlobData == null || roomBlobData.m_MedicBeds == null)
			{
				continue;
			}
			int count = roomBlobData.m_MedicBeds.Count;
			for (int j = 0; j < count; j++)
			{
				InteractiveObject interactiveObject2 = roomBlobData.m_MedicBeds[j];
				if (!(interactiveObject2 == null))
				{
					if (interactiveObject == null)
					{
						interactiveObject = interactiveObject2;
					}
					if (!interactiveObject2.ObjectReserved())
					{
						return interactiveObject2;
					}
				}
			}
		}
		return interactiveObject;
	}

	public void StartSolitaryForCharacter(GameObject characterObject)
	{
		Character character = null;
		if (characterObject != null)
		{
			character = characterObject.GetComponent<Character>();
		}
		if (!(character == null) && SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().OnAI_CharacterPutInSolitary(character);
		}
	}

	public bool IsCharacterBound(AIEventMemory memory)
	{
		return memory?.m_TargetCharacter.m_bIsBound ?? false;
	}
}
