using System.Collections.Generic;

public class RoomBlob_RollCall : RoomBlobData
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
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			BaseLevelManager.RoomObjectCollectionType<AICharacter_Guard> aICharacters = new BaseLevelManager.RoomObjectCollectionType<AICharacter_Guard>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomWaypoint, aICharacters);
			SetupFromDataBlob(ref roomWaypoint, ref aICharacters, ref blob);
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			BaseLevelManager.RoomObjectCollectionType<AICharacter_Guard> aICharacters = new BaseLevelManager.RoomObjectCollectionType<AICharacter_Guard>();
			instance.GetObjectsInZone(ref zone, roomWaypoint, aICharacters);
			SetupFromDataBlob(ref roomWaypoint, ref aICharacters, ref blob);
		}
	}

	public void SetupFromDataBlob(ref BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint, ref BaseLevelManager.RoomObjectCollectionType<AICharacter_Guard> aICharacters, ref RoomBlob blob)
	{
		blob.m_InmateRoomObjects.Clear();
		blob.m_GuardRoomObjects.Clear();
		blob.m_Waypoints.Clear();
		RoomBlob.OrderWaypoints(ref roomWaypoint.m_Contents, RoomBlob.WaypointSortType.HeadsFirst, bReversable: true);
		int count = roomWaypoint.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			blob.m_Waypoints.Add(roomWaypoint.m_Contents[i]);
		}
		List<AICharacter_Guard> contents = aICharacters.m_Contents;
		bool flag = true;
		int num = 0;
		for (int j = 0; j < contents.Count; j++)
		{
			AICharacter_Guard aICharacter_Guard = contents[j];
			if (aICharacter_Guard != null)
			{
				if (flag)
				{
					flag = false;
					aICharacter_Guard.m_RollCallStatus = AICharacter_Guard.RollCallStatus.DoesSpeech;
				}
				else if (num < 2)
				{
					num++;
					aICharacter_Guard.m_RollCallStatus = AICharacter_Guard.RollCallStatus.DoesShakeDown;
				}
				else
				{
					aICharacter_Guard.m_RollCallStatus = AICharacter_Guard.RollCallStatus.None;
				}
			}
		}
	}
}
