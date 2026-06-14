using System.Collections.Generic;
using UnityEngine;

public class RoomBlob_ControlRoom : RoomBlobData
{
	[SerializeField]
	public InteractiveObject Computer;

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GuardComputerInteraction> guardComputerInteraction = new BaseLevelManager.RoomObjectCollectionType<GuardComputerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<InteractiveObject> interactiveObject = new BaseLevelManager.RoomObjectCollectionType<InteractiveObject>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, guardComputerInteraction, interactiveObject);
			SetupFromData(ref guardComputerInteraction, ref interactiveObject);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<GuardComputerInteraction> guardComputerInteraction = new BaseLevelManager.RoomObjectCollectionType<GuardComputerInteraction>();
			BaseLevelManager.RoomObjectCollectionType<InteractiveObject> interactiveObject = new BaseLevelManager.RoomObjectCollectionType<InteractiveObject>();
			instance.GetObjectsInZone(ref zone, guardComputerInteraction, interactiveObject);
			SetupFromData(ref guardComputerInteraction, ref interactiveObject);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<GuardComputerInteraction> guardComputerInteraction, ref BaseLevelManager.RoomObjectCollectionType<InteractiveObject> interactiveObject)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (guardComputerInteraction.m_Contents.Count > 0)
		{
			if (guardComputerInteraction.m_Contents.Count > 1)
			{
			}
			Computer = guardComputerInteraction.m_Contents[0];
		}
		if (interactiveObject.m_Contents.Count > 0)
		{
			if (interactiveObject.m_Contents.Count > 1)
			{
			}
			Computer = interactiveObject.m_Contents[0];
		}
		if (Computer != null)
		{
			m_RoomSpecificObjects.Add(Computer);
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
