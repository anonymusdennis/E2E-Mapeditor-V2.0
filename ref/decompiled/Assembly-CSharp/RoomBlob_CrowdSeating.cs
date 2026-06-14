public class RoomBlob_CrowdSeating : RoomBlobData
{
	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			blob.m_InmateRoomObjects.Clear();
			blob.m_GuardRoomObjects.Clear();
			blob.m_Waypoints.Clear();
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			int count = roomObjectCollectionType.m_Contents.Count;
			for (int i = 0; i < count; i++)
			{
				blob.m_Waypoints.Add(roomObjectCollectionType.m_Contents[i]);
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
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			int count = roomObjectCollectionType.m_Contents.Count;
			for (int i = 0; i < count; i++)
			{
				blob.m_Waypoints.Add(roomObjectCollectionType.m_Contents[i]);
			}
		}
	}
}
