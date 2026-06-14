using UnityEngine;

public class LevelSetup_DoorSetup : BaseComponentSetup
{
	public enum Rule_Room
	{
		Dont_Care,
		Room_to_NotRoom,
		NotRoom_to_Room
	}

	public enum Rule_Danger
	{
		Dont_Care,
		Safe_to_Danger,
		Danger_to_Safe
	}

	public enum Rule_Environment
	{
		Dont_Care,
		In_to_Out,
		Out_to_In
	}

	public enum Rule_Node
	{
		None,
		Locked_Side,
		Unlocked_Side,
		Both
	}

	public enum LockFound
	{
		None,
		First_to_Second,
		Second_to_First
	}

	public Door m_Door;

	[Tooltip("If the door is part of a room, which direction can the door be opened")]
	public Rule_Room m_Room_Rule;

	[Tooltip("If the door is NOT part of a room, test which side is dangerous and if the door will open")]
	public Rule_Danger m_Danger_Rule;

	[Tooltip("If the door is NOT part of a room and NOT between Safe and Danger, test which side is inside and which outside")]
	public Rule_Environment m_Environment_Rule;

	[Tooltip("Should there be a node created and if so which side")]
	public Rule_Node m_NodeCreation = Rule_Node.Locked_Side;

	private BaseLevelManager m_LevelManager;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_5;
	}

	public override SetupReturnState Setup()
	{
		m_Door.m_OneWayWhenLocked = Door.DoorOneWayDirection.None;
		LockFound lockFound = LockFound.None;
		if (m_Door == null)
		{
			return FinishedAndRemove();
		}
		m_LevelManager = BaseLevelManager.GetInstance();
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		int X = 0;
		int Y = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.FirstFloor;
		if (!GetLayerAndPosition(ref X, ref Y, ref layer))
		{
			return FinishedAndRemove();
		}
		Y = 119 - Y;
		if (X == 0 || Y == 0 || X >= 119 || Y >= BaseLevelManager.c_LayerHeights[(uint)layer] - 1)
		{
			return FinishedAndRemove();
		}
		int num = 120 * Y + X;
		int num2 = num;
		int num3 = num;
		if (m_Door.IsVerticalDoor)
		{
			num2--;
			num3++;
		}
		else
		{
			num2 += 120;
			num3 -= 120;
		}
		bool flag = true;
		BaseLevelManager.LayerDataCollection data = m_LevelManager.m_BuildingLayers[(uint)layer];
		BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[num2];
		BaseLevelManager.TileProperty tileProperty2 = data.m_TileProperties[num3];
		if ((tileProperty & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask || (tileProperty2 & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
		{
			flag = false;
		}
		if ((tileProperty & BaseLevelManager.TileProperty.TileBlockingMask) == BaseLevelManager.TileProperty.TileBlockingMask || (tileProperty2 & BaseLevelManager.TileProperty.TileBlockingMask) == BaseLevelManager.TileProperty.TileBlockingMask || (tileProperty & BaseLevelManager.TileProperty.BlockBitMask) == BaseLevelManager.TileProperty.BlockBitMask || (tileProperty2 & BaseLevelManager.TileProperty.BlockBitMask) == BaseLevelManager.TileProperty.BlockBitMask)
		{
			m_Door.m_OneWayWhenLocked = Door.DoorOneWayDirection.None;
			return FinishedAndRemove();
		}
		int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref data, num);
		if (m_Room_Rule != 0 && roomNumberFromProperty != 0)
		{
			bool flag2 = BaseLevelManager.IsRoomNumberInProperty(ref data, num2, roomNumberFromProperty);
			bool flag3 = BaseLevelManager.IsRoomNumberInProperty(ref data, num3, roomNumberFromProperty);
			if (flag2 != flag3)
			{
				lockFound = ((m_Room_Rule != Rule_Room.NotRoom_to_Room) ? (flag2 ? LockFound.First_to_Second : LockFound.Second_to_First) : ((!flag2) ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound == LockFound.None && m_Danger_Rule != 0)
		{
			bool flag4 = (tileProperty & BaseLevelManager.TileProperty.SafeMask) != 0;
			bool flag5 = (tileProperty2 & BaseLevelManager.TileProperty.SafeMask) != 0;
			if (flag4 != flag5)
			{
				lockFound = ((m_Danger_Rule != Rule_Danger.Safe_to_Danger) ? ((!flag4) ? LockFound.First_to_Second : LockFound.Second_to_First) : (flag4 ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound == LockFound.None && m_Environment_Rule != 0)
		{
			bool flag6 = (tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) != 0;
			bool flag7 = (tileProperty2 & BaseLevelManager.TileProperty.EnvironmentMask) != 0;
			if (flag6 != flag7)
			{
				lockFound = ((m_Environment_Rule != Rule_Environment.Out_to_In) ? (flag6 ? LockFound.First_to_Second : LockFound.Second_to_First) : ((!flag6) ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound != 0)
		{
			if (lockFound == LockFound.First_to_Second)
			{
				m_Door.m_OneWayWhenLocked = (m_Door.IsVerticalDoor ? Door.DoorOneWayDirection.LeftToRight : Door.DoorOneWayDirection.TopToBottom);
			}
			else
			{
				m_Door.m_OneWayWhenLocked = (m_Door.IsVerticalDoor ? Door.DoorOneWayDirection.RightToLeft : Door.DoorOneWayDirection.BottomToTop);
			}
			if (flag)
			{
				CreateInterestingLocation(X, Y, layer, roomNumberFromProperty);
			}
		}
		return FinishedAndRemove();
	}

	public override SetupReturnState SetupV2()
	{
		LevelEditor_ZoneManager zoneManager = LevelEditor_ZoneManager.GetInstance();
		if (zoneManager == null)
		{
			return FinishedAndRemove();
		}
		m_Door.m_OneWayWhenLocked = Door.DoorOneWayDirection.None;
		LockFound lockFound = LockFound.None;
		if (m_Door == null)
		{
			return FinishedAndRemove();
		}
		m_LevelManager = BaseLevelManager.GetInstance();
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		int num = 0;
		int num2 = 0;
		int iIndex = 0;
		BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.GroundFloor;
		if (!GetLayerAndZoneMapIndex(ref iIndex, ref layer))
		{
			return FinishedAndRemove();
		}
		num = iIndex % 120;
		num2 = iIndex / 120;
		if ((num <= 0 || num >= 119) && m_Door.IsVerticalDoor)
		{
			return FinishedAndRemove();
		}
		if ((num2 <= 0 || num2 >= BaseLevelManager.c_LayerHeights[(uint)layer] - 1) && !m_Door.IsVerticalDoor)
		{
			return FinishedAndRemove();
		}
		int num3 = iIndex;
		int num4 = iIndex;
		if (m_Door.IsVerticalDoor)
		{
			num3--;
			num4++;
		}
		else
		{
			num3 += 120;
			num4 -= 120;
		}
		bool flag = true;
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)layer];
		LevelEditor_ZoneManager.ZoneMap zoneMap = zoneManager.GetZoneMap(layer);
		BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num3];
		BaseLevelManager.TileProperty tileProperty2 = layerDataCollection.m_TileProperties[num4];
		if ((tileProperty & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask || (tileProperty2 & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
		{
			flag = false;
		}
		if ((tileProperty & BaseLevelManager.TileProperty.TileBlockingMask) == BaseLevelManager.TileProperty.TileBlockingMask || (tileProperty2 & BaseLevelManager.TileProperty.TileBlockingMask) == BaseLevelManager.TileProperty.TileBlockingMask || (tileProperty & BaseLevelManager.TileProperty.BlockBitMask) == BaseLevelManager.TileProperty.BlockBitMask || (tileProperty2 & BaseLevelManager.TileProperty.BlockBitMask) == BaseLevelManager.TileProperty.BlockBitMask)
		{
			m_Door.m_OneWayWhenLocked = Door.DoorOneWayDirection.None;
			return FinishedAndRemove();
		}
		int num5 = zoneMap.m_Map[iIndex];
		if (m_Room_Rule != 0 && num5 != -1)
		{
			bool flag2 = zoneMap.m_Map[num3] == num5;
			bool flag3 = zoneMap.m_Map[num4] == num5;
			if (flag2 != flag3)
			{
				lockFound = ((m_Room_Rule != Rule_Room.NotRoom_to_Room) ? (flag2 ? LockFound.First_to_Second : LockFound.Second_to_First) : ((!flag2) ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound == LockFound.None && m_Danger_Rule != 0)
		{
			bool flag4 = (tileProperty & BaseLevelManager.TileProperty.SafeMask) != 0;
			bool flag5 = (tileProperty2 & BaseLevelManager.TileProperty.SafeMask) != 0;
			if (flag4 != flag5)
			{
				lockFound = ((m_Danger_Rule != Rule_Danger.Safe_to_Danger) ? ((!flag4) ? LockFound.First_to_Second : LockFound.Second_to_First) : (flag4 ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound == LockFound.None && m_Environment_Rule != 0)
		{
			bool flag6 = (tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) != 0;
			bool flag7 = (tileProperty2 & BaseLevelManager.TileProperty.EnvironmentMask) != 0;
			if (flag6 != flag7)
			{
				lockFound = ((m_Environment_Rule != Rule_Environment.Out_to_In) ? (flag6 ? LockFound.First_to_Second : LockFound.Second_to_First) : ((!flag6) ? LockFound.First_to_Second : LockFound.Second_to_First));
			}
		}
		if (lockFound != 0)
		{
			if (lockFound == LockFound.First_to_Second)
			{
				m_Door.m_OneWayWhenLocked = (m_Door.IsVerticalDoor ? Door.DoorOneWayDirection.LeftToRight : Door.DoorOneWayDirection.TopToBottom);
			}
			else
			{
				m_Door.m_OneWayWhenLocked = (m_Door.IsVerticalDoor ? Door.DoorOneWayDirection.RightToLeft : Door.DoorOneWayDirection.BottomToTop);
			}
			if (flag)
			{
				CreateInterestingLocationV2(num, num2, layer, num5, ref zoneManager);
			}
		}
		return FinishedAndRemove();
	}

	private void CreateInterestingLocation(int iX, int iY, BaseLevelManager.LevelLayers elayer, int iRoomNumber)
	{
		if (m_NodeCreation == Rule_Node.None || m_Door.m_OneWayWhenLocked == Door.DoorOneWayDirection.None)
		{
			return;
		}
		int iValue = -1;
		Vector3 localPosition = base.transform.localPosition;
		if (iRoomNumber != 0)
		{
			int blockIDFromComplexAllocation = m_LevelManager.GetBlockIDFromComplexAllocation(iRoomNumber);
			if (blockIDFromComplexAllocation != -1)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
				if (block != null)
				{
					iValue = block.m_LimitationGroup;
				}
			}
		}
		if (m_NodeCreation == Rule_Node.Locked_Side || m_NodeCreation == Rule_Node.Both)
		{
			switch (m_Door.m_OneWayWhenLocked)
			{
			case Door.DoorOneWayDirection.BottomToTop:
				AddLocation(localPosition + new Vector3(0f, -1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.TopToBottom:
				AddLocation(localPosition + new Vector3(0f, 1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.LeftToRight:
				AddLocation(localPosition + new Vector3(-1f, 0f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.RightToLeft:
				AddLocation(localPosition + new Vector3(1f, 0f, 0f), iValue, elayer);
				break;
			}
		}
		if (m_NodeCreation == Rule_Node.Unlocked_Side || m_NodeCreation == Rule_Node.Both)
		{
			switch (m_Door.m_OneWayWhenLocked)
			{
			case Door.DoorOneWayDirection.BottomToTop:
				AddLocation(localPosition + new Vector3(0f, 1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.TopToBottom:
				AddLocation(localPosition + new Vector3(0f, -1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.LeftToRight:
				AddLocation(localPosition + new Vector3(1f, 0f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.RightToLeft:
				AddLocation(localPosition + new Vector3(-1f, 0f, 0f), iValue, elayer);
				break;
			}
		}
	}

	private void CreateInterestingLocationV2(int iX, int iY, BaseLevelManager.LevelLayers elayer, int iZoneIndex, ref LevelEditor_ZoneManager zoneManager)
	{
		if (m_NodeCreation == Rule_Node.None || m_Door.m_OneWayWhenLocked == Door.DoorOneWayDirection.None)
		{
			return;
		}
		int iValue = -1;
		Vector3 localPosition = base.transform.localPosition;
		if (iZoneIndex != -1)
		{
			LevelEditor_ZoneManager.Zone zone = zoneManager.GetZone(iZoneIndex);
			if (zone != null)
			{
				iValue = (int)zone.m_ZoneType;
			}
		}
		if (m_NodeCreation == Rule_Node.Locked_Side || m_NodeCreation == Rule_Node.Both)
		{
			switch (m_Door.m_OneWayWhenLocked)
			{
			case Door.DoorOneWayDirection.BottomToTop:
				AddLocation(localPosition + new Vector3(0f, -1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.TopToBottom:
				AddLocation(localPosition + new Vector3(0f, 1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.LeftToRight:
				AddLocation(localPosition + new Vector3(-1f, 0f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.RightToLeft:
				AddLocation(localPosition + new Vector3(1f, 0f, 0f), iValue, elayer);
				break;
			}
		}
		if (m_NodeCreation == Rule_Node.Unlocked_Side || m_NodeCreation == Rule_Node.Both)
		{
			switch (m_Door.m_OneWayWhenLocked)
			{
			case Door.DoorOneWayDirection.BottomToTop:
				AddLocation(localPosition + new Vector3(0f, 1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.TopToBottom:
				AddLocation(localPosition + new Vector3(0f, -1f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.LeftToRight:
				AddLocation(localPosition + new Vector3(1f, 0f, 0f), iValue, elayer);
				break;
			case Door.DoorOneWayDirection.RightToLeft:
				AddLocation(localPosition + new Vector3(-1f, 0f, 0f), iValue, elayer);
				break;
			}
		}
	}

	private void AddLocation(Vector3 vPos, int iValue, BaseLevelManager.LevelLayers elayer)
	{
		BaseLevelManager.InterestingLocations interestingLocations = new BaseLevelManager.InterestingLocations();
		interestingLocations.m_Value = iValue;
		interestingLocations.m_Position = vPos;
		interestingLocations.m_Type = BaseLevelManager.InterestingLocations.LocationType.OutsideDoor;
		interestingLocations.m_Layer = elayer;
		m_LevelManager.GetInterestingLocationsList()?.Add(interestingLocations);
	}
}
