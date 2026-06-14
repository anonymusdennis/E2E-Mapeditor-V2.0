using System.Collections.Generic;
using UnityEngine;

public class SwagBagManager : MonoBehaviour
{
	private struct tilePos
	{
		public int x;

		public int y;

		public tilePos(int xpos, int ypos)
		{
			x = xpos;
			y = ypos;
		}
	}

	private static SwagBagManager m_Instance;

	public List<SwagBagInteraction> m_SwagBags = new List<SwagBagInteraction>();

	public static SwagBagManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	public SwagBagInteraction GetSwagBag(int index)
	{
		if (index < m_SwagBags.Count && index >= 0)
		{
			return m_SwagBags[index];
		}
		return null;
	}

	public Vector3 FindCleanestPositionInRoom(FloorManager.Floor targetFloor, Vector3 startPosition, RoomBlob targetRoom)
	{
		List<tilePos> list = new List<tilePos>();
		List<tilePos> list2 = new List<tilePos>();
		FloorManager instance = FloorManager.GetInstance();
		RoomManager instance2 = RoomManager.GetInstance();
		if (instance != null && instance2 != null && instance.GetTileGridPoint(targetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, startPosition, out var row, out var column))
		{
			tilePos tilePos = new tilePos(-1, -1);
			int num = int.MaxValue;
			instance.GetTileSystemBounds(targetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
			list.Add(new tilePos(row, column));
			int num2 = 0;
			int num3 = maxRows * maxColumns / 2;
			while (list.Count > 0 && num2 < num3)
			{
				int index = 0;
				int num4 = ScoreTileForBagPlacement(targetFloor, list[index].x, list[index].y);
				if (num4 == 0)
				{
					Vector3 worldPosition = Vector3.zero;
					instance.GetTileCentrePosition(targetFloor.m_FloorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, list[index].x, list[index].y, out worldPosition);
					return worldPosition;
				}
				if (num4 < num)
				{
					num = num4;
					tilePos.x = list[index].x;
					tilePos.y = list[index].y;
				}
				tilePos[] array = FindNeighbours(list[index]);
				for (int i = 0; i < array.Length; i++)
				{
					if (!CompareTileToList(array[i], list2) && !CompareTileToList(array[i], list))
					{
						instance.GetTileCentrePosition(targetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, array[i].x, array[i].y, out var worldPosition2);
						RoomBlob roomBlob = instance2.LookUpRoom(worldPosition2, targetFloor);
						if (roomBlob != null && roomBlob == targetRoom)
						{
							list.Add(array[i]);
						}
					}
				}
				list2.Add(list[index]);
				list.RemoveAt(index);
			}
			if (num != int.MaxValue && tilePos.x != -1 && tilePos.y != -1)
			{
				Vector3 worldPosition3 = Vector3.zero;
				instance.GetTileCentrePosition(targetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, tilePos.x, tilePos.y, out worldPosition3);
				return worldPosition3;
			}
			num2++;
		}
		return startPosition;
	}

	public int ScoreTileForBagPlacement(FloorManager.Floor targetFloor, int targetRow, int targetColumn)
	{
		FloorManager instance = FloorManager.GetInstance();
		int num = 0;
		if (!instance.IsFloorClear(targetFloor, targetRow, targetColumn))
		{
			num += 100;
		}
		if (instance.GetHole(targetRow, targetColumn, targetFloor.m_FloorIndex) != null)
		{
			num += 10;
		}
		if (instance.GetItemAtFloorTile(targetFloor, FloorManager.TileSystem_Type.TileSystem_Ground, targetRow, targetColumn) != null)
		{
			num++;
		}
		return num;
	}

	private tilePos[] FindNeighbours(tilePos target)
	{
		return new tilePos[8]
		{
			new tilePos(target.x - 1, target.y - 1),
			new tilePos(target.x, target.y - 1),
			new tilePos(target.x + 1, target.y - 1),
			new tilePos(target.x - 1, target.y),
			new tilePos(target.x + 1, target.y),
			new tilePos(target.x - 1, target.y + 1),
			new tilePos(target.x, target.y + 1),
			new tilePos(target.x + 1, target.y + 1)
		};
	}

	private bool CompareTileToList(tilePos toCheck, List<tilePos> checkPoints)
	{
		for (int i = 0; i < checkPoints.Count; i++)
		{
			if (toCheck.x == checkPoints[i].x && toCheck.y == checkPoints[i].y)
			{
				return true;
			}
		}
		return false;
	}
}
