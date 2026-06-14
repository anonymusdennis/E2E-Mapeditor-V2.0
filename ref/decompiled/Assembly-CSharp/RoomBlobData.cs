using System.Collections.Generic;

public abstract class RoomBlobData : T17MonoBehaviour
{
	public List<InteractiveObject> m_RoomSpecificObjects;

	public List<InteractiveObject> GetRoomSpecificObjects()
	{
		return m_RoomSpecificObjects;
	}

	public abstract void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer);

	public abstract void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone);

	public virtual void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
	}

	public virtual void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
	}
}
