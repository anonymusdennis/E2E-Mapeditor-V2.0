using System.Collections.Generic;

public class RoomBlob_Solitary : RoomBlobData
{
	public static int c_SolitarySubKey;

	public Door m_Door;

	public InteractiveObject m_Bed;

	public InteractiveObject m_TaskObject;

	private Character m_CharacterInSolitary;

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager levelManager = BaseLevelManager.GetInstance();
		if (levelManager != null)
		{
			BaseLevelManager.RoomObjectCollectionType<Door> door = new BaseLevelManager.RoomObjectCollectionType<Door>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<SolitaryPotatoesInteraction> solitaryPotatoesInteraction = new BaseLevelManager.RoomObjectCollectionType<SolitaryPotatoesInteraction>();
			levelManager.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, door, bedInteraction, solitaryPotatoesInteraction);
			SetupFromData(ref levelManager, ref door, ref bedInteraction, ref solitaryPotatoesInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager levelManager = BaseLevelManager.GetInstance();
		if (levelManager != null)
		{
			BaseLevelManager.RoomObjectCollectionType<Door> door = new BaseLevelManager.RoomObjectCollectionType<Door>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<SolitaryPotatoesInteraction> solitaryPotatoesInteraction = new BaseLevelManager.RoomObjectCollectionType<SolitaryPotatoesInteraction>();
			levelManager.GetObjectsInZone(ref zone, door, bedInteraction, solitaryPotatoesInteraction);
			SetupFromData(ref levelManager, ref door, ref bedInteraction, ref solitaryPotatoesInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager levelManager, ref BaseLevelManager.RoomObjectCollectionType<Door> door, ref BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction, ref BaseLevelManager.RoomObjectCollectionType<SolitaryPotatoesInteraction> solitaryPotatoesInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (door.m_Contents.Count == 1)
		{
			m_Door = door.m_Contents[0];
			if (m_Door.m_DoorKeyColour == KeyFunctionality.KeyColour.Solitary && !levelManager.IsKeyInitialized(KeyFunctionality.KeyColour.Solitary))
			{
				c_SolitarySubKey = 0;
			}
			m_Door.m_DoorKeySubCode = ++c_SolitarySubKey;
		}
		if (bedInteraction.m_Contents.Count == 1)
		{
			m_Bed = bedInteraction.m_Contents[0];
			m_RoomSpecificObjects.Add(m_Bed);
		}
		if (solitaryPotatoesInteraction.m_Contents.Count == 1)
		{
			m_TaskObject = solitaryPotatoesInteraction.m_Contents[0];
			m_RoomSpecificObjects.Add(m_TaskObject);
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
		}
	}

	public void LockToCharacter(Character character)
	{
		if (SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().LockToCharacterRPC(character, this);
		}
	}

	public void ReleaseCharacter(Character character)
	{
		if (SolitaryManager.GetInstance() != null)
		{
			SolitaryManager.GetInstance().UnlockForCharacterRPC(character, this);
		}
	}

	public void SetCharacterAssignment(Character character)
	{
		m_CharacterInSolitary = character;
	}

	public bool IsCellAssigned()
	{
		return m_CharacterInSolitary != null;
	}

	public Character GetAssignedCharacter()
	{
		return m_CharacterInSolitary;
	}
}
