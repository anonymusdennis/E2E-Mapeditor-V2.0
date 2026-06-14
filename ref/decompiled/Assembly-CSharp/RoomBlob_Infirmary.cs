using System.Collections.Generic;

public class RoomBlob_Infirmary : RoomBlobData
{
	public List<InteractiveObject> m_MedicBeds = new List<InteractiveObject>();

	public List<AICharacter> m_Medics = new List<AICharacter>();

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> medicBedInteraction = new BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<AICharacter> aICharacter = new BaseLevelManager.RoomObjectCollectionType<AICharacter>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, medicBedInteraction, aICharacter);
			SetupFromData(ref medicBedInteraction, ref aICharacter);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> medicBedInteraction = new BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<AICharacter> aICharacter = new BaseLevelManager.RoomObjectCollectionType<AICharacter>();
			instance.GetObjectsInZone(ref zone, medicBedInteraction, aICharacter);
			SetupFromData(ref medicBedInteraction, ref aICharacter);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<MedicBedInteraction> medicBedInteraction, ref BaseLevelManager.RoomObjectCollectionType<AICharacter> aICharacter)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = medicBedInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			m_MedicBeds.Add(medicBedInteraction.m_Contents[i]);
		}
		m_RoomSpecificObjects.AddRange(m_MedicBeds);
		count = aICharacter.m_Contents.Count;
		for (int j = 0; j < count; j++)
		{
			if (aICharacter.m_Contents[j].m_Character.m_CharacterRole == CharacterRole.Medic)
			{
				m_Medics.Add(aICharacter.m_Contents[j]);
			}
		}
		if (medicBedInteraction.m_Contents.Count == 0)
		{
		}
		if (aICharacter.m_Contents.Count != 0)
		{
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
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
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
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_GuardRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
		}
	}
}
