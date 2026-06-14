using System.Collections.Generic;

public class RoomBlob_GuardQuarters : RoomBlobData
{
	public List<InteractiveObject> m_Crates = new List<InteractiveObject>();

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GuardOutfitCrateInteraction> guardOutfitCrateInteraction = new BaseLevelManager.RoomObjectCollectionType<GuardOutfitCrateInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, guardOutfitCrateInteraction);
			SetupFromData(ref guardOutfitCrateInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GuardOutfitCrateInteraction> guardOutfitCrateInteraction = new BaseLevelManager.RoomObjectCollectionType<GuardOutfitCrateInteraction>();
			instance.GetObjectsInZone(ref zone, guardOutfitCrateInteraction);
			SetupFromData(ref guardOutfitCrateInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<GuardOutfitCrateInteraction> guardOutfitCrateInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = guardOutfitCrateInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			m_Crates.Add(guardOutfitCrateInteraction.m_Contents[i]);
		}
		m_RoomSpecificObjects.AddRange(m_Crates);
		if (guardOutfitCrateInteraction.m_Contents.Count != 0)
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
