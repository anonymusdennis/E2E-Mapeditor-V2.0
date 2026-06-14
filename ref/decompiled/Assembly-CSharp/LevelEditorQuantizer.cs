using System.Collections.Generic;
using UnityEngine;

public class LevelEditorQuantizer
{
	public class Information
	{
		public BaseLevelManager m_LevelManager;

		public List<BaseBuildInstruction> m_Instructions = new List<BaseBuildInstruction>();

		public BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.GroundFloor;

		public BaseLevelManager.LevelLayers m_LastLayerChangedTo = BaseLevelManager.LevelLayers.GroundFloor;

		public BaseLevelManager.TileProperty[] m_LayerPropsArray;

		public BaseLevelManager.RoomProperty[] m_RoomPropsArray;

		public BaseLevelManager.LayerDataCollection m_LayerData;

		public BaseLevelManager.TileIDData[] m_IDArray;

		public int m_Xpos;

		public int m_Ypos;

		public int m_iIndex;

		public bool m_bChangedLayer;
	}

	public enum Passes
	{
		Start,
		ScanFloorAndRooms,
		ScanWallsAndObjects,
		Finished
	}

	public static List<BaseBuildInstruction> QuantizeLevel(bool bSave = true)
	{
		Information data = new Information();
		data.m_LevelManager = BaseLevelManager.GetInstance();
		if (data.m_LevelManager == null)
		{
			return new List<BaseBuildInstruction>();
		}
		int num = 14400;
		for (int i = 1; i < 6; i++)
		{
			BaseLevelManager.TileProperty[] tileProperties = data.m_LevelManager.m_BuildingLayers[i].m_TileProperties;
			for (int j = 0; j < num; j++)
			{
				BaseLevelManager.TileProperty[] array;
				int num2;
				(array = tileProperties)[num2 = j] = array[num2] & BaseLevelManager.TileProperty.InverseScanFlags;
			}
		}
		BaseLevelManager.TileProperty tileProperty = BaseLevelManager.TileProperty.EMPTY;
		BaseLevelManager.RoomProperty roomProperty = BaseLevelManager.RoomProperty.EMPTY;
		int num3 = 0;
		int num4 = 0;
		BaseBuildingBlock baseBuildingBlock = null;
		for (int k = 1; k < 6; k++)
		{
			data.m_Layer = (BaseLevelManager.LevelLayers)k;
			data.m_bChangedLayer = k == 1;
			data.m_LayerData = data.m_LevelManager.m_BuildingLayers[k];
			data.m_LayerPropsArray = data.m_LayerData.m_TileProperties;
			data.m_RoomPropsArray = data.m_LayerData.m_RoomPropertiesMasks;
			Passes passes = Passes.Start;
			while (++passes != Passes.Finished)
			{
				data.m_iIndex = 0;
				num3 = 0;
				for (int l = 0; l < 120; l++)
				{
					data.m_Ypos = l;
					for (int m = 0; m < 120; m++)
					{
						data.m_Xpos = m;
						data.m_iIndex = num3;
						tileProperty = data.m_LayerPropsArray[num3];
						roomProperty = data.m_RoomPropsArray[num3];
						if (passes == Passes.ScanFloorAndRooms)
						{
							if ((tileProperty & BaseLevelManager.TileProperty.ScanTileMask) == 0 && (tileProperty & BaseLevelManager.TileProperty.TileMask) != 0)
							{
								num4 = (int)(data.m_LayerData.m_TileTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
								baseBuildingBlock = BuildingBlockManager.GetBlock(num4);
								if (baseBuildingBlock != null && baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Tile)
								{
									bool flag = true;
									if ((tileProperty & BaseLevelManager.TileProperty.WallMask) != 0)
									{
										int buildingBrickID = (int)(data.m_LayerData.m_WallTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
										baseBuildingBlock = BuildingBlockManager.GetBlock(buildingBrickID);
										if (baseBuildingBlock != null && baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Wall && ((BuildingBlock_Wall)baseBuildingBlock).m_FloorTileID != -1)
										{
											flag = false;
										}
									}
									if (flag)
									{
										ScanFloorForTile(ref data, num4);
									}
								}
							}
						}
						else if (roomProperty != 0)
						{
							num4 = data.m_LayerData.m_RoomIDs[num3];
							if ((tileProperty & BaseLevelManager.TileProperty.ScanRoom1Mask) == 0 && num4 != 0)
							{
								ScanRoom(ref data, num4);
							}
							num4 = data.m_LayerData.m_RoomIDs[num3 + 14400];
							if ((tileProperty & BaseLevelManager.TileProperty.ScanRoom2Mask) == 0 && num4 != 0)
							{
								ScanRoom(ref data, num4);
							}
							num4 = data.m_LayerData.m_RoomIDs[num3 + 28800];
							if ((tileProperty & BaseLevelManager.TileProperty.ScanRoom3Mask) == 0 && num4 != 0)
							{
								ScanRoom(ref data, num4);
							}
							num4 = data.m_LayerData.m_RoomIDs[num3 + 43200];
							if ((tileProperty & BaseLevelManager.TileProperty.ScanRoom4Mask) == 0 && num4 != 0)
							{
								ScanRoom(ref data, num4);
							}
						}
						else
						{
							if ((tileProperty & BaseLevelManager.TileProperty.ScanWallMask) == 0 && (tileProperty & BaseLevelManager.TileProperty.WallMask) != 0)
							{
								num4 = (int)(data.m_LayerData.m_WallTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
								baseBuildingBlock = BuildingBlockManager.GetBlock(num4);
								if (baseBuildingBlock != null && baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Wall)
								{
									ScanFloorForWall(ref data, num4);
								}
							}
							if ((tileProperty & BaseLevelManager.TileProperty.ScanObjMask) == 0 && (tileProperty & BaseLevelManager.TileProperty.ObjDecMask) != 0)
							{
								num4 = (int)(data.m_LayerData.m_ObjectTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
								baseBuildingBlock = BuildingBlockManager.GetBlock(num4);
								if (baseBuildingBlock != null && (baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Object || baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Decoration))
								{
									ScanFloorForObject(ref data, num4);
								}
							}
						}
						num3++;
					}
				}
			}
			ScanZones(ref data);
		}
		if (data.m_LastLayerChangedTo != BaseLevelManager.LevelLayers.GroundFloor)
		{
			BaseBuildInstruction item = BaseBuildInstruction.CreateLayerChange(BaseLevelManager.LevelLayers.GroundFloor);
			data.m_Instructions.Add(item);
		}
		if (data.m_Instructions.Count != 0 && bSave)
		{
			InstructionTransfer instructionTransfer = new InstructionTransfer();
			instructionTransfer.m_Instructions = data.m_Instructions;
			string text = JsonUtility.ToJson(instructionTransfer);
		}
		return data.m_Instructions;
	}

	private static void ScanZones(ref Information data)
	{
		LevelEditor_ZoneManager instance = LevelEditor_ZoneManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		int totalZones = instance.GetTotalZones();
		for (int i = 0; i < totalZones; i++)
		{
			LevelEditor_ZoneManager.Zone zone = instance.GetZone(i, bSupressWarning: true);
			if (zone != null && zone.m_bActive && zone.m_Layer == data.m_Layer)
			{
				BaseBuildInstruction item = BaseBuildInstruction.CreateZone(zone.m_ZoneType, (sbyte)zone.m_Left, (sbyte)zone.m_Bottom, zone.m_Width, zone.m_Height, zone.m_ZonePrint, zone.m_ID);
				data.m_Instructions.Add(item);
			}
		}
	}

	private static void ScanRoom(ref Information data, int iRoomIndex)
	{
		int blockIDFromComplexAllocation = data.m_LevelManager.GetBlockIDFromComplexAllocation(iRoomIndex);
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
		if (block == null || (block.BlockType != BaseBuildingBlock.BuildingBlockType.Room && block.BlockType != BaseBuildingBlock.BuildingBlockType.Complex) || !block.IsValidForLayer(data.m_Layer))
		{
			return;
		}
		int iX = 0;
		int iY = 0;
		if (!block.m_Footprint.GetPositionOfFirstOccupiedTile(ref iX, ref iY, data.m_Layer))
		{
			return;
		}
		if (!data.m_bChangedLayer)
		{
			AddLayerChange(ref data, data.m_Layer);
		}
		BaseBuildInstruction item = BaseBuildInstruction.CreateOnce((sbyte)(data.m_Xpos - iX - block.m_Footprint.m_iLeft), (sbyte)(data.m_Ypos - iY - block.m_Footprint.m_iBottom), blockIDFromComplexAllocation, Random.Range(0, 10000));
		data.m_Instructions.Add(item);
		int iW = block.m_Footprint.m_iW;
		int iH = block.m_Footprint.m_iH;
		int num = (data.m_Ypos - iY) * 120 + data.m_Xpos - iX;
		int num2 = 120 - iW;
		int num3 = 0;
		int num4 = (int)data.m_Layer;
		int num5 = num4 + 1;
		if (block.m_Footprint.m_bMultiLevel)
		{
			num4 = 1;
			num5 = 6;
			num3 = iH * iW;
		}
		for (int i = num4; i < num5; i++)
		{
			BaseLevelManager.TileProperty[] tileProperties = data.m_LevelManager.m_BuildingLayers[i].m_TileProperties;
			int num6 = num;
			for (int j = 0; j < iH; j++)
			{
				for (int k = 0; k < iW; k++)
				{
					if (block.m_Footprint.m_UsedTiles[num3++] != 0)
					{
						switch (BaseLevelManager.GetRoomPartFromProp(ref data.m_LevelManager.m_BuildingLayers[i], num6, iRoomIndex))
						{
						case 1:
						{
							BaseLevelManager.TileProperty[] array;
							int num10;
							(array = tileProperties)[num10 = num6] = array[num10] | BaseLevelManager.TileProperty.ScanRoom1Mask;
							break;
						}
						case 2:
						{
							BaseLevelManager.TileProperty[] array;
							int num9;
							(array = tileProperties)[num9 = num6] = array[num9] | BaseLevelManager.TileProperty.ScanRoom2Mask;
							break;
						}
						case 3:
						{
							BaseLevelManager.TileProperty[] array;
							int num8;
							(array = tileProperties)[num8 = num6] = array[num8] | BaseLevelManager.TileProperty.ScanRoom3Mask;
							break;
						}
						case 4:
						{
							BaseLevelManager.TileProperty[] array;
							int num7;
							(array = tileProperties)[num7 = num6] = array[num7] | BaseLevelManager.TileProperty.ScanRoom4Mask;
							break;
						}
						}
					}
					num6++;
				}
				num6 += num2;
			}
		}
	}

	private static void ScanFloorForTile(ref Information data, int iBlockID)
	{
		data.m_IDArray = data.m_LayerData.m_TileTileIDs;
		int iIndex = data.m_iIndex;
		int num = 0;
		int num2 = 1;
		int num3 = iIndex;
		for (int i = data.m_Xpos; i < 120; i++)
		{
			if ((data.m_IDArray[num3] & BaseLevelManager.TileIDData.IDMask) != (BaseLevelManager.TileIDData)iBlockID)
			{
				break;
			}
			if ((data.m_LayerPropsArray[num3] & BaseLevelManager.TileProperty.ScanTileMask) != 0)
			{
				break;
			}
			if ((data.m_LayerPropsArray[num3] & BaseLevelManager.TileProperty.WallMask) != 0)
			{
				int buildingBrickID = (int)(data.m_LayerData.m_WallTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(buildingBrickID);
				if (block != null && block.BlockType == BaseBuildingBlock.BuildingBlockType.Wall && ((BuildingBlock_Wall)block).m_FloorTileID != -1)
				{
					break;
				}
			}
			num++;
			num3++;
		}
		if (num == 0)
		{
			return;
		}
		int num4 = 120 - num;
		num3 = iIndex + 120;
		bool flag = false;
		for (int j = data.m_Ypos + 1; j < 120; j++)
		{
			for (int k = 0; k < num; k++)
			{
				if ((data.m_IDArray[num3] & BaseLevelManager.TileIDData.IDMask) != (BaseLevelManager.TileIDData)iBlockID || (data.m_LayerPropsArray[num3] & BaseLevelManager.TileProperty.ScanTileMask) != 0)
				{
					flag = true;
					break;
				}
				if ((data.m_LayerPropsArray[num3] & BaseLevelManager.TileProperty.WallMask) != 0)
				{
					int buildingBrickID2 = (int)(data.m_LayerData.m_WallTileIDs[num3] & BaseLevelManager.TileIDData.IDMask);
					BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(buildingBrickID2);
					if (block2 != null && block2.BlockType == BaseBuildingBlock.BuildingBlockType.Wall && ((BuildingBlock_Wall)block2).m_FloorTileID != -1)
					{
						flag = true;
						break;
					}
				}
				num3++;
			}
			if (flag)
			{
				break;
			}
			num3 += num4;
			num2++;
		}
		num3 = iIndex;
		for (int l = 0; l < num2; l++)
		{
			for (int m = 0; m < num; m++)
			{
				BaseLevelManager.TileProperty[] layerPropsArray;
				int num5;
				(layerPropsArray = data.m_LayerPropsArray)[num5 = num3++] = layerPropsArray[num5] | BaseLevelManager.TileProperty.ScanTileMask;
			}
			num3 += num4;
		}
		if (!data.m_bChangedLayer)
		{
			AddLayerChange(ref data, data.m_Layer);
		}
		if (num == 1 && num2 == 1)
		{
			BaseBuildInstruction item = BaseBuildInstruction.CreateOnce((sbyte)data.m_Xpos, (sbyte)data.m_Ypos, iBlockID, Random.Range(0, 10000));
			data.m_Instructions.Add(item);
		}
		else
		{
			BaseBuildInstruction item2 = BaseBuildInstruction.CreateArea((sbyte)data.m_Xpos, (sbyte)data.m_Ypos, (sbyte)num, (sbyte)num2, iBlockID, Random.Range(0, 10000));
			data.m_Instructions.Add(item2);
		}
	}

	private static void ScanFloorForWall(ref Information data, int iBlockID)
	{
		data.m_IDArray = data.m_LayerData.m_WallTileIDs;
		ScanFloorForBlock(ref data, iBlockID, BaseLevelManager.TileProperty.ScanWallMask);
	}

	private static void ScanFloorForBlock(ref Information data, int iBlockID, BaseLevelManager.TileProperty scanMask)
	{
		int iIndex = data.m_iIndex;
		int num = 0;
		int num2 = 1;
		int num3 = iIndex;
		for (int i = data.m_Xpos; i < 120; i++)
		{
			if ((data.m_IDArray[num3] & BaseLevelManager.TileIDData.IDMask) != (BaseLevelManager.TileIDData)iBlockID)
			{
				break;
			}
			if ((data.m_LayerPropsArray[num3] & scanMask) != 0)
			{
				break;
			}
			if (data.m_RoomPropsArray[num3] != 0)
			{
				break;
			}
			num++;
			num3++;
		}
		if (num == 0)
		{
			return;
		}
		int num4 = 120 - num;
		num3 = iIndex + 120;
		bool flag = false;
		for (int j = data.m_Ypos + 1; j < 120; j++)
		{
			for (int k = 0; k < num; k++)
			{
				if ((data.m_IDArray[num3] & BaseLevelManager.TileIDData.IDMask) != (BaseLevelManager.TileIDData)iBlockID || (data.m_LayerPropsArray[num3] & scanMask) != 0 || data.m_RoomPropsArray[num3] != 0)
				{
					flag = true;
					break;
				}
				num3++;
			}
			if (flag)
			{
				break;
			}
			num3 += num4;
			num2++;
		}
		num3 = iIndex;
		for (int l = 0; l < num2; l++)
		{
			for (int m = 0; m < num; m++)
			{
				BaseLevelManager.TileProperty[] layerPropsArray;
				int num5;
				(layerPropsArray = data.m_LayerPropsArray)[num5 = num3++] = layerPropsArray[num5] | scanMask;
			}
			num3 += num4;
		}
		if (!data.m_bChangedLayer)
		{
			AddLayerChange(ref data, data.m_Layer);
		}
		if (num == 1 && num2 == 1)
		{
			BaseBuildInstruction item = BaseBuildInstruction.CreateOnce((sbyte)data.m_Xpos, (sbyte)data.m_Ypos, iBlockID, Random.Range(0, 10000));
			data.m_Instructions.Add(item);
		}
		else
		{
			BaseBuildInstruction item2 = BaseBuildInstruction.CreateArea((sbyte)data.m_Xpos, (sbyte)data.m_Ypos, (sbyte)num, (sbyte)num2, iBlockID, Random.Range(0, 10000));
			data.m_Instructions.Add(item2);
		}
	}

	public static void ScanFloorForObject(ref Information data, int iBlockID)
	{
		GameObject gameObject = data.m_LayerData.m_ObjectTileObjects[data.m_iIndex];
		if (!(gameObject != null))
		{
			return;
		}
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
		if (!(block != null))
		{
			return;
		}
		float num = gameObject.transform.localPosition.x - data.m_LevelManager.m_fPositionOffsetsX[(int)block.BlockType];
		float num2 = gameObject.transform.localPosition.y - data.m_LevelManager.m_fPositionOffsetsY[(int)block.BlockType];
		num += (float)(block.m_Footprint.m_iW - 1) / 2f;
		num2 += (float)(block.m_Footprint.m_iH - 1) / 2f;
		int num3 = (int)num - (block.m_Footprint.m_iW - 1) / 2 + block.m_Footprint.m_iLeft;
		int num4 = (int)num2 - (block.m_Footprint.m_iH - 1) / 2 + block.m_Footprint.m_iBottom;
		int num5 = Mathf.Clamp(num3 + block.m_Footprint.m_iW, 0, 120);
		int num6 = Mathf.Clamp(num4 + block.m_Footprint.m_iH, 0, 120);
		int num7 = 0;
		for (int i = num4; i < num6; i++)
		{
			for (int j = num3; j < num5; j++)
			{
				if ((block.m_Footprint.m_UsedTiles[num7++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
				{
					BaseLevelManager.TileProperty[] layerPropsArray;
					int num8;
					(layerPropsArray = data.m_LayerPropsArray)[num8 = i * 120 + j] = layerPropsArray[num8] | BaseLevelManager.TileProperty.ScanObjMask;
				}
			}
		}
		if (!data.m_bChangedLayer)
		{
			AddLayerChange(ref data, data.m_Layer);
		}
		BaseBuildInstruction item = BaseBuildInstruction.CreateOnce((sbyte)(num3 - block.m_Footprint.m_iLeft), (sbyte)(num4 - block.m_Footprint.m_iBottom), iBlockID, Random.Range(0, 10000));
		data.m_Instructions.Add(item);
	}

	private static void AddLayerChange(ref Information data, BaseLevelManager.LevelLayers layer)
	{
		data.m_bChangedLayer = true;
		BaseBuildInstruction item = BaseBuildInstruction.CreateLayerChange(layer);
		data.m_Instructions.Add(item);
		data.m_LastLayerChangedTo = layer;
	}
}
