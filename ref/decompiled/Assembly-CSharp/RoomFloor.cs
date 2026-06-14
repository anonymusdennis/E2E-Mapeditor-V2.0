using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class RoomFloor : MonoBehaviour
{
	public const int kFloorWidth = 121;

	public const int kFloorHeight = 121;

	public Dictionary<int, RoomBlob> m_Rooms = new Dictionary<int, RoomBlob>();

	public Dictionary<RoomBlob.eLocation, List<RoomBlob>> m_RoomTypeLookup = new Dictionary<RoomBlob.eLocation, List<RoomBlob>>();

	[ReadOnly]
	public int[] m_FloorMap = new int[14641];

	[ReadOnly]
	public int m_FloorWidth = 121;

	[ReadOnly]
	public int m_FloorHeight = 121;

	[ReadOnly]
	public int m_FloorIndex = -1;

	public NavGraph m_AssociatedNavGraph;

	private bool m_TempNodesGenerated;

	private bool m_bRoomsLoaded;

	private bool[] m_bRoomHits = new bool[14641];

	private bool m_bMaxXSizeReached;

	private bool m_bMaxYSizeReached;

	private int m_iMinXPosition;

	private int m_iMinYPosition;

	private int m_iMaxXPosition;

	private int m_iMaxYPosition;

	private int m_iCurrentWidth;

	private int m_iCurrentHeight;

	private const int c_iRoomMaxSize = 16;

	protected virtual void OnDestroy()
	{
		Dictionary<RoomBlob.eLocation, List<RoomBlob>>.Enumerator enumerator = m_RoomTypeLookup.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Clear();
		}
		m_RoomTypeLookup.Clear();
		m_Rooms.Clear();
		m_AssociatedNavGraph = null;
	}

	public void SetDims(int width, int height)
	{
		m_FloorMap = new int[width * height];
		m_FloorWidth = width;
		m_FloorHeight = height;
	}

	public FloorManager.Floor GetFloor()
	{
		return FloorManager.GetInstance().FindFloorbyIndex(m_FloorIndex);
	}

	public RoomBlob LookUpRoom(int key)
	{
		RoomBlob value = null;
		m_Rooms.TryGetValue(key, out value);
		return value;
	}

	public RoomBlob LookUpRoom(Vector2 gridPos)
	{
		return LookUpRoom(FloorMap((int)gridPos.x, (int)gridPos.y));
	}

	public RoomBlob LookUpRoom(int x, int y)
	{
		return LookUpRoom(FloorMap(x, y));
	}

	public int LookUpKey(int x, int y)
	{
		return FloorMap(x, y);
	}

	public int FloorMap(int x, int y)
	{
		int num = x * m_FloorHeight + y;
		if (num > 0 && num < m_FloorMap.Length)
		{
			return m_FloorMap[num];
		}
		return 0;
	}

	public void LoadRooms(ref int nextRoomID)
	{
		if (Application.isPlaying && m_bRoomsLoaded)
		{
			return;
		}
		m_bRoomsLoaded = true;
		m_Rooms.Clear();
		RoomBlob[] componentsInChildren = GetComponentsInChildren<RoomBlob>();
		foreach (RoomBlob roomBlob in componentsInChildren)
		{
			roomBlob.LoadBlob();
			roomBlob.SetFloor(this);
			if (!m_Rooms.ContainsKey(roomBlob.m_ID))
			{
				m_Rooms.Add(roomBlob.m_ID, roomBlob);
				List<RoomBlob> value = null;
				m_RoomTypeLookup.TryGetValue(roomBlob.location, out value);
				if (value == null)
				{
					value = new List<RoomBlob>();
				}
				if (!value.Contains(roomBlob))
				{
					value.Add(roomBlob);
				}
				m_RoomTypeLookup[roomBlob.location] = value;
			}
			else
			{
				roomBlob.m_ID = nextRoomID;
				nextRoomID++;
				m_Rooms.Add(roomBlob.m_ID, roomBlob);
			}
		}
	}

	public List<RoomBlob> GetAllRoomsOnThisFloorByLocation(RoomBlob.eLocation location)
	{
		List<RoomBlob> value = null;
		m_RoomTypeLookup.TryGetValue(location, out value);
		return value;
	}

	public void SetupPins()
	{
		List<int> roomKeys = GetRoomKeys();
		for (int i = 0; i < roomKeys.Count; i++)
		{
			RoomBlob roomBlob = LookUpRoom(roomKeys[i]);
			roomBlob.AddLabelToMap();
		}
	}

	public List<int> GetRoomKeys()
	{
		return new List<int>(m_Rooms.Keys);
	}

	public void SetTileBlob(int x, int y, int key)
	{
		if (x * m_FloorHeight + y < m_FloorMap.Length)
		{
			m_FloorMap[x * m_FloorHeight + y] = key;
		}
	}

	public void AddRoom(int roomKey, RoomBlob room)
	{
		m_bRoomsLoaded = false;
		m_Rooms.Add(roomKey, room);
	}

	public void RemoveRoom(int roomKey)
	{
		m_bRoomsLoaded = false;
		m_Rooms.Remove(roomKey);
		for (int i = 0; i < m_FloorMap.Length; i++)
		{
			if (m_FloorMap[i] == roomKey)
			{
				m_FloorMap[i] = 0;
			}
		}
	}

	public void AutoChunkRoomBlob(RoomBlob room, ref FloorManager floorManager)
	{
		if (room == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_FloorHeight; i++)
		{
			for (int j = 0; j < m_FloorWidth; j++)
			{
				m_bRoomHits[num++] = (room == null && LookUpRoom(j, i) == null) || room == LookUpRoom(j, i);
			}
		}
		int iCount = 0;
		num = 0;
		for (int k = 0; k < m_FloorHeight; k++)
		{
			for (int l = 0; l < m_FloorWidth; l++)
			{
				if (m_bRoomHits[num++])
				{
					m_bMaxXSizeReached = false;
					m_bMaxYSizeReached = false;
					m_iMinXPosition = l;
					m_iMinYPosition = k;
					m_iMaxXPosition = l;
					m_iMaxYPosition = k;
					m_iCurrentWidth = 1;
					m_iCurrentHeight = 1;
					RoomBlob roomBlob = CreateNewRoom(room, ref iCount);
					Flood(l, k, num - 1, 0, roomBlob.m_ID);
				}
			}
		}
		if (room != null)
		{
			RoomManager.GetInstance().DeleteRoomFromFloor(room.m_ID, this);
		}
	}

	private RoomBlob CreateNewRoom(RoomBlob source, ref int iCount)
	{
		RoomBlob roomBlob = RoomManager.GetInstance().CreateNewRoom(this);
		iCount++;
		if (source != null)
		{
			roomBlob.m_RoomAffinity = source.m_RoomAffinity;
			roomBlob.m_RoomAffinityGuard = source.m_RoomAffinityGuard;
			roomBlob.m_RoomAffinitySupport = source.m_RoomAffinitySupport;
			roomBlob.m_GuardSafeSpace = source.m_GuardSafeSpace;
			roomBlob.m_InmateSafeSpace = source.m_InmateSafeSpace;
			roomBlob.m_SupportSafeSpace = source.m_SupportSafeSpace;
			roomBlob.m_subLocation = source.m_subLocation;
			roomBlob.SetRoomLocation(source.location);
			roomBlob.m_FloorMaterial = source.m_FloorMaterial;
			roomBlob.m_subRules = source.m_subRules;
			roomBlob.colour = source.colour;
		}
		Color.RGBToHSV(roomBlob.colour, out var H, out var S, out var V);
		H += (float)(iCount % 3) * 0.01f;
		roomBlob.colour = Color.HSVToRGB(H, S, V);
		return roomBlob;
	}

	private void Flood(int X, int Y, int iIndex, int iDirection, int iID)
	{
		m_bRoomHits[iIndex] = false;
		SetTileBlob(X, Y, iID);
		if (X < m_iMinXPosition)
		{
			m_iMinXPosition = X;
			m_iCurrentWidth++;
			if (m_iCurrentWidth >= 16)
			{
				m_bMaxXSizeReached = true;
			}
		}
		else if (X > m_iMaxXPosition)
		{
			m_iMaxXPosition = X;
			m_iCurrentWidth++;
			if (m_iCurrentWidth >= 16)
			{
				m_bMaxXSizeReached = true;
			}
		}
		if (Y < m_iMinYPosition)
		{
			m_iMinYPosition = Y;
			m_iCurrentHeight++;
			if (m_iCurrentHeight >= 16)
			{
				m_bMaxYSizeReached = true;
			}
		}
		else if (Y > m_iMaxYPosition)
		{
			m_iMaxYPosition = Y;
			m_iCurrentHeight++;
			if (m_iCurrentHeight >= 16)
			{
				m_bMaxYSizeReached = true;
			}
		}
		int num = iDirection;
		do
		{
			switch (num)
			{
			case 0:
				if ((!m_bMaxYSizeReached || Y < m_iMaxYPosition) && Y + 1 < 121 && m_bRoomHits[iIndex + 121])
				{
					Flood(X, Y + 1, iIndex + 121, 1, iID);
				}
				break;
			case 1:
				if ((!m_bMaxXSizeReached || X < m_iMaxXPosition) && X + 1 <= 121 && m_bRoomHits[iIndex + 1])
				{
					Flood(X + 1, Y, iIndex + 1, 2, iID);
				}
				break;
			case 2:
				if ((!m_bMaxYSizeReached || Y > m_iMinYPosition) && Y > 0 && m_bRoomHits[iIndex - 121])
				{
					Flood(X, Y - 1, iIndex - 121, 3, iID);
				}
				break;
			case 3:
				if ((!m_bMaxXSizeReached || X > m_iMinXPosition) && X > 0 && m_bRoomHits[iIndex - 1])
				{
					Flood(X - 1, Y, iIndex - 1, 0, iID);
				}
				break;
			}
			num = (num + 1) % 4;
		}
		while (num != iDirection);
	}

	public void PopulateRoomTempNodes()
	{
		if (Application.isPlaying && m_TempNodesGenerated)
		{
			return;
		}
		m_TempNodesGenerated = true;
		List<int> roomKeys = GetRoomKeys();
		for (int i = 0; i < roomKeys.Count; i++)
		{
			RoomBlob roomBlob = LookUpRoom(roomKeys[i]);
			roomBlob.m_TempRoomPositions.Clear();
			roomBlob.m_FirstRoomTiles.Clear();
		}
		RoomBlob roomBlob2 = null;
		for (int j = 0; j < m_FloorWidth - 1; j++)
		{
			for (int k = 0; k < m_FloorHeight - 1; k++)
			{
				int num = FloorMap(j, k);
				if (num == 0)
				{
					continue;
				}
				RoomBlob roomBlob3 = LookUpRoom(num);
				Vector3 vector = (roomBlob3.m_SomeRoomPosition = new Vector3(j, k, base.transform.position.z));
				if (roomBlob3 != roomBlob2 && roomBlob3.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors)
				{
					if (roomBlob3.m_FirstRoomTiles.Count == 0)
					{
						roomBlob3.m_FirstRoomTiles.Add(vector);
					}
					else
					{
						bool flag = true;
						for (int l = 0; l < roomBlob3.m_FirstRoomTiles.Count; l++)
						{
							Vector3 roomStartPos = roomBlob3.m_FirstRoomTiles[l];
							if (RoomMeshGenerator.CanTraceBetweenTilesInRoom(roomStartPos, vector, roomBlob3.GetFloor()))
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							roomBlob3.m_FirstRoomTiles.Add(vector);
						}
					}
				}
				bool flag2 = true;
				flag2 &= num == FloorMap(j + 1, k);
				flag2 &= num == FloorMap(j + 1, k + 1);
				if (flag2 & (num == FloorMap(j, k + 1)))
				{
					roomBlob3.m_TempRoomPositions.Add(vector);
				}
				roomBlob2 = roomBlob3;
			}
		}
	}

	public void LoadAllInteractableObjectsForAllRooms()
	{
		List<int> roomKeys = GetRoomKeys();
		for (int i = 0; i < roomKeys.Count; i++)
		{
			RoomBlob roomBlob = LookUpRoom(roomKeys[i]);
			roomBlob.LoadAllInteractableObjects();
		}
	}

	public void SetTileBlobInFloor(int x, int y, int key)
	{
		FloorMapSet(x, y, key);
	}

	public void FloorMapSet(int x, int y, int val)
	{
		if (x * m_FloorHeight + y < m_FloorMap.Length)
		{
			m_FloorMap[x * m_FloorHeight + y] = val;
		}
	}

	public void AutoPositionAllRooms()
	{
		List<int> roomKeys = GetRoomKeys();
		for (int i = 0; i < roomKeys.Count; i++)
		{
			RoomBlob roomBlob = LookUpRoom(roomKeys[i]);
			roomBlob.AutoPositionRoom();
		}
	}

	public void GenerateRoomGraph()
	{
		RoomManager roomManager = RoomManager.GetInstance();
		if (roomManager == null)
		{
			GameObject gameObject = GameObject.Find("RoomManager");
			if (gameObject != null)
			{
				roomManager = gameObject.GetComponent<RoomManager>();
			}
		}
		int num = 2;
		for (int i = 0; i < m_FloorWidth; i++)
		{
			for (int j = 0; j < m_FloorHeight; j++)
			{
				int num2 = FloorMap(i, j);
				if (num2 == 0)
				{
					continue;
				}
				RoomBlob room = LookUpRoom(num2);
				for (int k = -num; k <= num; k++)
				{
					for (int l = -num; l <= num; l++)
					{
						if (k == 0 && l == 0)
						{
							continue;
						}
						int x = Mathf.Clamp(i + k, 0, m_FloorWidth);
						int y = Mathf.Clamp(j + l, 0, m_FloorHeight);
						int num3 = FloorMap(x, y);
						if (num3 != num2)
						{
							RoomBlob roomBlob = LookUpRoom(num3);
							if (!(roomBlob == null))
							{
								CreateConnection(roomBlob, room);
							}
						}
					}
				}
			}
		}
		NodeLink[] array = Object.FindObjectsOfType<NodeLink>();
		foreach (NodeLink nodeLink in array)
		{
			Vector3 position = nodeLink.transform.position;
			Transform end = nodeLink.End;
			if (!(end == null))
			{
				Vector3 position2 = end.position;
				RoomFloor floorFromZ = roomManager.GetFloorFromZ(position.z);
				Vector3 vector = RoomUtility.WorldToRoomGrid(position, floorFromZ);
				RoomBlob roomBlob2 = floorFromZ.LookUpRoom(vector);
				RoomFloor floorFromZ2 = roomManager.GetFloorFromZ(position2.z);
				Vector3 vector2 = RoomUtility.WorldToRoomGrid(position2, floorFromZ2);
				RoomBlob roomBlob3 = floorFromZ2.LookUpRoom(vector2);
				if (!(roomBlob2 == null) && !(roomBlob3 == null))
				{
					CreateConnection(roomBlob3, roomBlob2);
				}
			}
		}
	}

	private void CreateConnection(RoomBlob otherRoom, RoomBlob room)
	{
		if (!(otherRoom == room))
		{
			if (!room.m_ARoomConnections.Contains(otherRoom))
			{
				room.m_ARoomConnections.Add(otherRoom);
			}
			if (!otherRoom.m_ARoomConnections.Contains(room))
			{
				otherRoom.m_ARoomConnections.Add(room);
			}
			if (!RoomUtility.GetInstance().HasConnection(ref room, ref otherRoom))
			{
				RoomUtility.GetInstance().SetConnection(ref room, ref otherRoom, 500f);
			}
		}
	}
}
