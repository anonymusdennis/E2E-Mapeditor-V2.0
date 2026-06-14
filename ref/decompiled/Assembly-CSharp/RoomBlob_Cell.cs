using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomBlob_Cell : RoomBlobData
{
	public static int c_CellSubKey;

	public List<SpawnPoint> m_SpawnPoints = new List<SpawnPoint>();

	public Door m_Door;

	public override void AutoSetup(int iLevelEditorRoomNumber, BaseLevelManager.LevelLayers eLayer)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		m_RoomSpecificObjects = new List<InteractiveObject>();
		if (instance != null)
		{
			BaseLevelManager.RoomObjectCollectionType<SpawnPoint> spawnPoint = new BaseLevelManager.RoomObjectCollectionType<SpawnPoint>();
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteractions = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> toiletInteraction = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			BaseLevelManager.RoomObjectCollectionType<Door> doors = new BaseLevelManager.RoomObjectCollectionType<Door>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, spawnPoint, deskInteractions, bedInteraction, toiletInteraction, doors);
			SetupFromData(ref spawnPoint, ref deskInteractions, ref bedInteraction, ref toiletInteraction, ref doors);
		}
	}

	public override void AutoSetupZone(ref LevelEditor_ZoneManager.Zone zone)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		BaseLevelManager.RoomObjectCollectionType<SpawnPoint> spawnPoint = new BaseLevelManager.RoomObjectCollectionType<SpawnPoint>();
		BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteractions = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
		BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
		BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> toiletInteraction = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
		BaseLevelManager.RoomObjectCollectionType<Door> doors = new BaseLevelManager.RoomObjectCollectionType<Door>();
		instance.GetObjectsInZone(ref zone, deskInteractions, bedInteraction, toiletInteraction, doors);
		int count = bedInteraction.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			int reachablePointForObject = instance.GetReachablePointForObject(ref zone, bedInteraction.m_Contents[i].gameObject);
			Vector3 localPosition = Vector3.zero;
			if (reachablePointForObject != -1)
			{
				localPosition.x = reachablePointForObject % 120;
				localPosition.y = reachablePointForObject / 120;
				localPosition.x -= 60f;
				localPosition.y -= 60f;
			}
			else
			{
				localPosition = bedInteraction.m_Contents[i].transform.localPosition;
			}
			GameObject gameObject = new GameObject();
			gameObject.transform.parent = bedInteraction.m_Contents[i].transform.parent;
			gameObject.transform.localPosition = localPosition;
			SpawnPoint obj = gameObject.AddComponent<SpawnPoint>();
			spawnPoint.AddToList(obj);
		}
		SetupFromDataForZone(ref spawnPoint, ref deskInteractions, ref bedInteraction, ref toiletInteraction, ref doors);
	}

	public void SetupFromDataForZone(ref BaseLevelManager.RoomObjectCollectionType<SpawnPoint> spawnPoint, ref BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteractions, ref BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction, ref BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> toiletInteraction, ref BaseLevelManager.RoomObjectCollectionType<Door> doors)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		m_RoomSpecificObjects = new List<InteractiveObject>();
		m_Door = null;
		if (doors.m_Contents.Count == 1)
		{
			m_Door = doors.m_Contents[0];
			if (m_Door.m_DoorKeyColour == KeyFunctionality.KeyColour.Yellow && !instance.IsKeyInitialized(KeyFunctionality.KeyColour.Yellow))
			{
				c_CellSubKey = 0;
			}
			m_Door.m_DoorKeySubCode = ++c_CellSubKey;
		}
		int count = spawnPoint.m_Contents.Count;
		for (int i = 0; i < count; i++)
		{
			float num = float.PositiveInfinity;
			Vector3 position = bedInteraction.m_Contents[i].transform.position;
			int num2 = -1;
			if (deskInteractions.m_Contents.Count > 0)
			{
				for (int num3 = deskInteractions.m_Contents.Count - 1; num3 >= 0; num3--)
				{
					if (!deskInteractions.IsContentUsed(num3))
					{
						float sqrMagnitude = (position - deskInteractions.m_Contents[num3].transform.position).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							num2 = num3;
						}
					}
				}
			}
			if (num2 != -1)
			{
				spawnPoint.m_Contents[i].m_AttachedDesk = deskInteractions.m_Contents[num2];
				deskInteractions.SetContentUsedState(num2, bUsed: true);
			}
			spawnPoint.m_Contents[i].m_AttachedBed = bedInteraction.m_Contents[i];
			bedInteraction.SetContentUsedState(num2, bUsed: true);
			num = float.PositiveInfinity;
			num2 = -1;
			if (toiletInteraction.m_Contents.Count > 0)
			{
				int count2 = toiletInteraction.m_Contents.Count;
				bool flag = false;
				for (int j = 0; j < count2; j++)
				{
					if (!toiletInteraction.IsContentUsed(j))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					for (int k = 0; k < count2; k++)
					{
						toiletInteraction.SetContentUsedState(k, bUsed: false);
					}
				}
				for (int num4 = toiletInteraction.m_Contents.Count - 1; num4 >= 0; num4--)
				{
					if (!toiletInteraction.IsContentUsed(num4))
					{
						float sqrMagnitude2 = (position - toiletInteraction.m_Contents[num4].transform.position).sqrMagnitude;
						if (sqrMagnitude2 < num)
						{
							num = sqrMagnitude2;
							num2 = num4;
						}
					}
				}
			}
			if (num2 != -1)
			{
				spawnPoint.m_Contents[i].m_AttachedToilet = toiletInteraction.m_Contents[num2];
				toiletInteraction.SetContentUsedState(num2, bUsed: true);
			}
			spawnPoint.m_Contents[i].m_SpawnPointID = SpawnPoint.c_HighestSpawnPoint++;
			m_SpawnPoints.Add(spawnPoint.m_Contents[i]);
		}
		for (int l = 0; l < m_SpawnPoints.Count; l++)
		{
			SpawnPoint spawnPoint2 = m_SpawnPoints[l];
			if (spawnPoint2 != null)
			{
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedBed);
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedDesk);
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedToilet);
			}
		}
	}

	public void SetupFromData(ref BaseLevelManager.RoomObjectCollectionType<SpawnPoint> spawnPoint, ref BaseLevelManager.RoomObjectCollectionType<DeskInteraction> deskInteractions, ref BaseLevelManager.RoomObjectCollectionType<BedInteraction> bedInteraction, ref BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> toiletInteraction, ref BaseLevelManager.RoomObjectCollectionType<Door> doors)
	{
		BaseLevelManager instance = BaseLevelManager.GetInstance();
		m_RoomSpecificObjects = new List<InteractiveObject>();
		m_Door = null;
		if (doors.m_Contents.Count == 1)
		{
			m_Door = doors.m_Contents[0];
			if (m_Door.m_DoorKeyColour == KeyFunctionality.KeyColour.Yellow && !instance.IsKeyInitialized(KeyFunctionality.KeyColour.Yellow))
			{
				c_CellSubKey = 0;
			}
			m_Door.m_DoorKeySubCode = ++c_CellSubKey;
		}
		for (int num = spawnPoint.m_Contents.Count - 1; num >= 0; num--)
		{
			float num2 = float.PositiveInfinity;
			Vector3 position = spawnPoint.m_Contents[num].transform.position;
			int num3 = -1;
			if (deskInteractions.m_Contents.Count > 0)
			{
				for (int num4 = deskInteractions.m_Contents.Count - 1; num4 >= 0; num4--)
				{
					float sqrMagnitude = (position - deskInteractions.m_Contents[num4].transform.position).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						num2 = sqrMagnitude;
						num3 = num4;
					}
				}
			}
			if (num3 != -1)
			{
				spawnPoint.m_Contents[num].m_AttachedDesk = deskInteractions.m_Contents[num3];
				deskInteractions.SetContentUsedState(num3, bUsed: true);
			}
			num2 = float.PositiveInfinity;
			num3 = -1;
			if (bedInteraction.m_Contents.Count > 0)
			{
				for (int num5 = bedInteraction.m_Contents.Count - 1; num5 >= 0; num5--)
				{
					float sqrMagnitude2 = (position - bedInteraction.m_Contents[num5].transform.position).sqrMagnitude;
					if (sqrMagnitude2 < num2)
					{
						num2 = sqrMagnitude2;
						num3 = num5;
						spawnPoint.m_Contents[num].m_AttachedBed = bedInteraction.m_Contents[num5];
					}
				}
			}
			if (num3 != -1)
			{
				spawnPoint.m_Contents[num].m_AttachedBed = bedInteraction.m_Contents[num3];
				bedInteraction.SetContentUsedState(num3, bUsed: true);
			}
			num2 = float.PositiveInfinity;
			if (toiletInteraction.m_Contents.Count > 0)
			{
				for (int num6 = toiletInteraction.m_Contents.Count - 1; num6 >= 0; num6--)
				{
					float sqrMagnitude3 = (position - toiletInteraction.m_Contents[num6].transform.position).sqrMagnitude;
					if (sqrMagnitude3 < num2)
					{
						num2 = sqrMagnitude3;
						num3 = num6;
					}
				}
			}
			if (num3 != -1)
			{
				spawnPoint.m_Contents[num].m_AttachedToilet = toiletInteraction.m_Contents[num3];
				toiletInteraction.SetContentUsedState(num3, bUsed: true);
			}
			else if (toiletInteraction.m_Contents.Count > 0)
			{
				spawnPoint.m_Contents[num].m_AttachedToilet = toiletInteraction.m_Contents[0];
			}
			spawnPoint.m_Contents[num].m_SpawnPointID = SpawnPoint.c_HighestSpawnPoint++;
			m_SpawnPoints.Add(spawnPoint.m_Contents[num]);
		}
		for (int i = 0; i < m_SpawnPoints.Count; i++)
		{
			SpawnPoint spawnPoint2 = m_SpawnPoints[i];
			if (spawnPoint2 != null)
			{
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedBed);
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedDesk);
				m_RoomSpecificObjects.Add(spawnPoint2.m_AttachedToilet);
			}
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
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInRoom(iLevelEditorRoomNumber, eLayer, roomObjectCollectionType4, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
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
			BaseLevelManager.RoomObjectCollectionType<DeskInteraction> roomObjectCollectionType = new BaseLevelManager.RoomObjectCollectionType<DeskInteraction>();
			BaseLevelManager.RoomObjectCollectionType<BedInteraction> roomObjectCollectionType2 = new BaseLevelManager.RoomObjectCollectionType<BedInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ToiletInteraction> roomObjectCollectionType3 = new BaseLevelManager.RoomObjectCollectionType<ToiletInteraction>();
			BaseLevelManager.RoomObjectCollectionType<ChairInteraction> roomObjectCollectionType4 = new BaseLevelManager.RoomObjectCollectionType<ChairInteraction>();
			instance.GetObjectsInZoneIncludingNonRequired(ref zone, roomObjectCollectionType4, roomObjectCollectionType, roomObjectCollectionType2, roomObjectCollectionType3);
			for (int num = roomObjectCollectionType.m_Contents.Count - 1; num >= 0; num--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType.m_Contents[num]);
			}
			for (int num2 = roomObjectCollectionType2.m_Contents.Count - 1; num2 >= 0; num2--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType2.m_Contents[num2]);
			}
			for (int num3 = roomObjectCollectionType3.m_Contents.Count - 1; num3 >= 0; num3--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType3.m_Contents[num3]);
			}
			for (int num4 = roomObjectCollectionType4.m_Contents.Count - 1; num4 >= 0; num4--)
			{
				blob.m_InmateRoomObjects.Add(roomObjectCollectionType4.m_Contents[num4]);
			}
		}
	}

	public SpawnPoint GetSpawnPointForCharacter(Character owner)
	{
		for (int i = 0; i < m_SpawnPoints.Count; i++)
		{
			SpawnPoint spawnPoint = m_SpawnPoints[i];
			if (spawnPoint != null && spawnPoint.GetCharacterOwner() == owner)
			{
				return spawnPoint;
			}
		}
		return null;
	}

	public InteractiveObject GetCellObject(Type type, Character owner = null)
	{
		for (int i = 0; i < m_SpawnPoints.Count; i++)
		{
			SpawnPoint spawnPoint = m_SpawnPoints[i];
			if (!(owner != null) || !(spawnPoint.GetCharacterOwner() != owner))
			{
				if (spawnPoint.m_AttachedBed != null && spawnPoint.m_AttachedBed.GetType() == type)
				{
					return spawnPoint.m_AttachedBed;
				}
				if (spawnPoint.m_AttachedDesk != null && spawnPoint.m_AttachedDesk.GetType() == type)
				{
					return spawnPoint.m_AttachedDesk;
				}
				if (spawnPoint.m_AttachedToilet != null && spawnPoint.m_AttachedToilet.GetType() == type)
				{
					return spawnPoint.m_AttachedToilet;
				}
			}
		}
		return null;
	}
}
