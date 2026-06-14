using System.Collections.Generic;

public class RoomBlob_ContrabandRoom : RoomBlobData
{
	public InteractiveObject m_Desk;

	public InteractiveObject GetContrabandDesk()
	{
		return m_Desk;
	}

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteraction = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, deskInteraction);
			SetupFromData(ref deskInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteraction = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			instance.GetObjectsInZone(ref zone, deskInteraction);
			SetupFromData(ref deskInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (deskInteraction.m_Contents.Count == 1)
		{
			m_Desk = deskInteraction.m_Contents[0];
			m_RoomSpecificObjects.Add(m_Desk);
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
	}
}
