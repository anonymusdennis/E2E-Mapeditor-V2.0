using System.Collections.Generic;

public class RoomBlob_Gym : RoomBlobData
{
	public List<InteractiveObject> GymEquipment = new List<InteractiveObject>();

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GymInteraction> gymInteraction = new BaseLevelManager.RoomObjectCollectionType<GymInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, gymInteraction);
			SetupFromData(ref gymInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GymInteraction> gymInteraction = new BaseLevelManager.RoomObjectCollectionType<GymInteraction>();
			instance.GetObjectsInZone(ref zone, gymInteraction);
			SetupFromData(ref gymInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<GymInteraction> gymInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = gymInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			GymEquipment.Add(gymInteraction.m_Contents[i]);
		}
		m_RoomSpecificObjects.AddRange(GymEquipment);
		if (gymInteraction.m_Contents.Count != 0)
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
			BaseLevelManager.RoomObjectCollectionType<GymInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<GymInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType2, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
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
			BaseLevelManager.RoomObjectCollectionType<GymInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<GymInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType2, roomObjectCollectionType);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
		}
	}
}
