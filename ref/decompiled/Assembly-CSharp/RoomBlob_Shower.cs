using System.Collections.Generic;

public class RoomBlob_Shower : RoomBlobData
{
	public List<InteractiveObject> Showers = new List<InteractiveObject>();

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, showerInteraction);
			SetupFromData(ref showerInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			instance.GetObjectsInZone(ref zone, showerInteraction);
			SetupFromData(ref showerInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = showerInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			Showers.Add(showerInteraction.m_Contents[i]);
		}
		m_RoomSpecificObjects.AddRange(Showers);
		if (showerInteraction.m_Contents.Count != 0)
		{
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<LockerInteraction> lockerInteraction = new BaseLevelManager.RoomObjectCollectionType<LockerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, chairInteraction, showerInteraction, lockerInteraction);
			SetupFromDataBlob(ref showerInteraction, ref lockerInteraction, ref chairInteraction, ref blob);
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction = new BaseLevelManager.RoomObjectCollectionType<ShowerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<LockerInteraction> lockerInteraction = new BaseLevelManager.RoomObjectCollectionType<LockerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, chairInteraction, showerInteraction, lockerInteraction);
			SetupFromDataBlob(ref showerInteraction, ref lockerInteraction, ref chairInteraction, ref blob);
		}
	}

	public void SetupFromDataBlob(ref BaseLevelManager.RoomObjectCollectionType<ShowerInteraction> showerInteraction, ref BaseLevelManager.RoomObjectCollectionType<LockerInteraction> lockerInteraction, ref BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction, ref RoomBlob blob)
	{
		blob.m_InmateRoomObjects.Clear();
		blob.m_GuardRoomObjects.Clear();
		blob.m_Waypoints.Clear();
		for (int num = lockerInteraction.m_Contents.Count - 1; num >= 0; num--)
		{
			blob.m_InmateRoomObjects.Add(lockerInteraction.m_Contents[num]);
		}
		for (int num2 = showerInteraction.m_Contents.Count - 1; num2 >= 0; num2--)
		{
			blob.m_InmateRoomObjects.Add(showerInteraction.m_Contents[num2]);
		}
		for (int num3 = chairInteraction.m_Contents.Count - 1; num3 >= 0; num3--)
		{
			blob.m_InmateRoomObjects.Add(chairInteraction.m_Contents[num3]);
		}
	}
}
