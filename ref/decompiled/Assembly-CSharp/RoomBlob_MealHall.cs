using System.Collections.Generic;
using UnityEngine;

public class RoomBlob_MealHall : RoomBlobData
{
	[SerializeField]
	public List<InteractiveObject> Chairs = new List<InteractiveObject>();

	[SerializeField]
	public InteractiveObject Tray;

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction = new BaseLevelManager.RoomObjectCollectionType<TrayInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, chairInteraction, trayInteraction);
			SetupFromData(ref chairInteraction, ref trayInteraction);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction = new BaseLevelManager.RoomObjectCollectionType<TrayInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, chairInteraction, trayInteraction);
			SetupFromData(ref chairInteraction, ref trayInteraction);
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction, ref BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction)
	{
		m_RoomSpecificObjects = new List<InteractiveObject>();
		int count = chairInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			Chairs.Add(chairInteraction.m_Contents[i]);
		}
		m_RoomSpecificObjects.AddRange(Chairs);
		if (trayInteraction.m_Contents.Count == 1)
		{
			Tray = trayInteraction.m_Contents[0];
			m_RoomSpecificObjects.Add(Tray);
		}
		if (chairInteraction.m_Contents.Count != 0)
		{
		}
	}

	public override void AutoSetupRoomBlob(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction = new BaseLevelManager.RoomObjectCollectionType<TrayInteraction>();
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, chairInteraction, roomWaypoint, trayInteraction);
			SetupFromDataBlob(ref trayInteraction, ref roomWaypoint, ref chairInteraction, ref blob);
		}
	}

	public void SetupFromDataBlob(ref BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction, ref BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint, ref BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction, ref RoomBlob blob)
	{
		blob.m_InmateRoomObjects.Clear();
		blob.m_GuardRoomObjects.Clear();
		blob.m_Waypoints.Clear();
		for (int num = trayInteraction.m_Contents.Count - 1; num >= 0; num--)
		{
			blob.m_InmateRoomObjects.Add(trayInteraction.m_Contents[num]);
		}
		for (int num2 = chairInteraction.m_Contents.Count - 1; num2 >= 0; num2--)
		{
			blob.m_InmateRoomObjects.Add(chairInteraction.m_Contents[num2]);
		}
		RoomBlob.OrderWaypoints(ref roomWaypoint.m_Contents, RoomBlob.WaypointSortType.TailsFirst, bReversable: false);
		int count = roomWaypoint.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			blob.m_Waypoints.Add(roomWaypoint.m_Contents[i]);
		}
	}

	public override void AutoSetupZoneBlob(ref LevelEditor_ZoneManager.Zone zone, ref RoomBlob blob)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction = new BaseLevelManager.RoomObjectCollectionType<TrayInteraction>();
			BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint = new BaseLevelManager.RoomObjectCollectionType<RoomWaypoint>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZone(ref zone, chairInteraction, roomWaypoint, trayInteraction);
			SetupZoneFromDataBlob(ref trayInteraction, ref roomWaypoint, ref chairInteraction, ref blob);
		}
	}

	public void SetupZoneFromDataBlob(ref BaseLevelManager.RoomObjectCollectionType<TrayInteraction> trayInteraction, ref BaseLevelManager.RoomObjectCollectionType<RoomWaypoint> roomWaypoint, ref BaseLevelManager.RoomObjectCollectionType<ChairInteraction> chairInteraction, ref RoomBlob blob)
	{
		blob.m_InmateRoomObjects.Clear();
		blob.m_GuardRoomObjects.Clear();
		blob.m_Waypoints.Clear();
		for (int num = trayInteraction.m_Contents.Count - 1; num >= 0; num--)
		{
			blob.m_InmateRoomObjects.Add(trayInteraction.m_Contents[num]);
		}
		for (int num2 = chairInteraction.m_Contents.Count - 1; num2 >= 0; num2--)
		{
			blob.m_InmateRoomObjects.Add(chairInteraction.m_Contents[num2]);
		}
		int count = roomWaypoint.m_Contents.Count;
		for (int num3 = count - 1; num3 >= 0; num3--)
		{
			blob.m_Waypoints.Add(roomWaypoint.m_Contents[num3]);
		}
	}
}
