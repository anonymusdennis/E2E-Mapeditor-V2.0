using System.Collections.Generic;

public class RoomBlob_ShowTime : RoomBlobData
{
	public List<InteractiveObject> m_PerformanceInteractions = new List<InteractiveObject>();

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		base.AutoSetupRoomBlob(iLevelEditorRoomNumber, eLayer, ref blob);
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		base.AutoSetupZoneBlob(ref zone, ref blob);
	}
}
