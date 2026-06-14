using System;
using Rotorz.Tile;
using UnityEngine;

[RequireComponent(typeof(LevelDetailsManager))]
[RequireComponent(typeof(BuildingInstructionManager))]
public class GameLevelEditorManager : BaseLevelManager
{
	private enum RotorzAction
	{
		Leave,
		Remove,
		Add
	}

	protected bool m_DeleteingRoom;

	protected bool m_bIsEveryThingSetUp;

	protected int[] m_LayerOffset = new int[7] { 0, 0, 0, 1, 1, 2, 2 };

	protected int m_iCurrentLayerOffset_Y;

	protected int[][] m_DifferentLayerOffsets = new int[6][]
	{
		new int[6],
		new int[6] { -120, 0, 0, 120, 120, 240 },
		new int[6] { -120, 0, 0, 120, 120, 240 },
		new int[6] { -240, -120, -120, 0, 0, 120 },
		new int[6] { -240, -120, -120, 0, 0, 120 },
		new int[6] { -360, -240, -240, -120, -120, 0 }
	};

	public override bool IsEverythingSetUp()
	{
		bool flag = base.IsEverythingSetUp();
		for (int i = 0; i < 6; i++)
		{
			if (m_BuildingLayers[i].m_Tiles_TileSystem == null)
			{
				flag = false;
			}
			if (m_BuildingLayers[i].m_Walls_TileSystem == null)
			{
				flag = false;
			}
			if (m_BuildingLayers[i].m_Tiles == null)
			{
				flag = false;
			}
			if (m_BuildingLayers[i].m_Walls == null)
			{
				flag = false;
			}
			if (m_BuildingLayers[i].m_Objects == null)
			{
				flag = false;
			}
		}
		m_bIsEveryThingSetUp = flag;
		return flag;
	}

	public override void AddSingle(ref BuildingInstructionManager.InstructionOnceElement obj, bool bStorePrevious = true)
	{
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID);
		if (!(block == null))
		{
			switch (block.BlockType)
			{
			case BaseBuildingBlock.BuildingBlockType.Tile:
				PlaceTile(obj.m_XPosition, obj.m_YPosition + m_iCurrentLayerOffset_Y, (BuildingBlock_Tile)block, obj.m_iRandomSeed);
				break;
			case BaseBuildingBlock.BuildingBlockType.Decoration:
			case BaseBuildingBlock.BuildingBlockType.Object:
				PlaceObject(obj.m_XPosition, obj.m_YPosition + m_iCurrentLayerOffset_Y, (BuildingBlock_Object)block);
				break;
			case BaseBuildingBlock.BuildingBlockType.Wall:
				break;
			}
		}
	}

	public override void AddSingleWall(ref BuildingInstructionManager.InstructionOnceWallElement obj, bool bStorePrevious = true)
	{
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID);
		if (block == null)
		{
			return;
		}
		BaseBuildingBlock.BuildingBlockType blockType = block.BlockType;
		if (blockType != BaseBuildingBlock.BuildingBlockType.Wall)
		{
			return;
		}
		PlaceWall(obj.m_XPosition, obj.m_YPosition + m_iCurrentLayerOffset_Y, (BuildingBlock_Wall)block, obj.m_iRandomSeed);
		int floorTileID = ((BuildingBlock_Wall)block).m_FloorTileID;
		if (floorTileID != -1)
		{
			BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(floorTileID);
			if (block2 != null)
			{
				PlaceTile(obj.m_XPosition, obj.m_YPosition + m_iCurrentLayerOffset_Y, (BuildingBlock_Tile)block2, obj.m_iRandomSeed, bMarkAsChanged: false);
			}
		}
	}

	public override void AddArea(ref BuildingInstructionManager.InstructionAreaElement obj, bool bStorePrevious = true)
	{
		BuildingBlock_Tile buildingBlock_Tile = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID) as BuildingBlock_Tile;
		if (!(buildingBlock_Tile == null) && buildingBlock_Tile.BlockType != BaseBuildingBlock.BuildingBlockType.Tile)
		{
			return;
		}
		BuildingBlockManager.LimitationGroup limitationGroup = null;
		if (buildingBlock_Tile != null && buildingBlock_Tile.m_LimitationGroup != -1 && m_BuildingBlockManager.m_LimitationGroups[buildingBlock_Tile.m_LimitationGroup].m_Max != 0)
		{
			limitationGroup = m_BuildingBlockManager.m_LimitationGroups[buildingBlock_Tile.m_LimitationGroup];
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (buildingBlock_Tile != null)
		{
			flag = (buildingBlock_Tile.m_ValidLayers & 0x555) != 0;
			flag2 = buildingBlock_Tile.m_BlockingTile;
			flag3 = buildingBlock_Tile.m_NoBlockingBelow;
		}
		TileProperty tileProperty = TileProperty.WallMask;
		System.Random random = new System.Random(obj.m_iRandomSeed);
		if (bStorePrevious)
		{
			obj.m_Previous = new TileIDData[obj.m_XCount * obj.m_YCount];
		}
		int num = obj.m_XPosition;
		int num2 = num + obj.m_XCount;
		int num3 = obj.m_YPosition + m_iCurrentLayerOffset_Y;
		int num4 = num3 + obj.m_YCount;
		int num5 = 0;
		int num6 = num3 * 120 + num;
		int num7 = 120 - obj.m_XCount;
		int num8 = 0;
		bool flag4 = m_VentLayers[(uint)m_CurrentLayer];
		if (m_CurrentLayer != LevelLayers.GroundFloor)
		{
			num8 = (flag4 ? 1 : 2);
		}
		int num9 = 0;
		if (m_CurrentLayer != LevelLayers.Roof && (!flag || buildingBlock_Tile == null))
		{
			num9 = (flag4 ? 1 : 2);
		}
		bool flag5 = true;
		int[] array = new int[9];
		int num10 = ((!flag4) ? 1 : 9);
		for (int i = num3; i < num4; i++)
		{
			for (int j = num; j < num2; j++)
			{
				array[0] = num6;
				if (flag4)
				{
					if (i > 0)
					{
						array[1] = array[0] - 120;
						if (j > 0)
						{
							array[2] = array[0] - 120 - 1;
						}
						else
						{
							array[2] = -1;
						}
						if (j < 119)
						{
							array[3] = array[0] - 120 + 1;
						}
						else
						{
							array[3] = -1;
						}
					}
					else
					{
						array[1] = -1;
						array[2] = -1;
						array[3] = -1;
					}
					if (j > 0)
					{
						array[4] = array[0] - 1;
					}
					else
					{
						array[4] = -1;
					}
					if (j < 119)
					{
						array[5] = array[0] + 1;
					}
					else
					{
						array[5] = -1;
					}
					if (i < 117)
					{
						array[6] = array[0] + 120;
						if (j > 0)
						{
							array[7] = array[0] + 120 - 1;
						}
						else
						{
							array[7] = -1;
						}
						if (j < 119)
						{
							array[8] = array[0] + 120 + 1;
						}
						else
						{
							array[8] = -1;
						}
					}
					else
					{
						array[6] = -1;
						array[7] = -1;
						array[8] = -1;
					}
				}
				flag5 = true;
				for (int k = 0; k < num10; k++)
				{
					if (!flag5)
					{
						break;
					}
					int num11 = array[k];
					if (num11 == -1)
					{
						continue;
					}
					TileProperty tileProperty2 = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num11];
					if ((tileProperty2 & TileProperty.WallMask) != 0)
					{
						BuildingBlock_Wall buildingBlock_Wall = BuildingBlockManager.GetBlock(m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num11]) as BuildingBlock_Wall;
						if (buildingBlock_Wall != null)
						{
							if (buildingBlock_Wall.m_AutomaticBlock)
							{
								tileProperty2 &= TileProperty.InverseWallMask;
							}
							else if (buildingBlock_Wall.m_FloorTileID == -1 && buildingBlock_Tile != null && (buildingBlock_Tile.m_ValidLayers & buildingBlock_Wall.m_ValidLayers) != 0)
							{
								tileProperty2 &= TileProperty.InverseWallMask;
							}
						}
					}
					if (num8 != 0)
					{
						flag5 = (m_BuildingLayers[(int)m_CurrentLayer - num8].m_TileProperties[num11 + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(int)m_CurrentLayer - num8]] & TileProperty.TileExistsMask) == TileProperty.TileExistsMask;
					}
					if (num9 != 0 && flag5)
					{
						for (int l = 1; l <= num9; l++)
						{
							if (!flag5)
							{
								break;
							}
							flag5 = (m_BuildingLayers[(int)m_CurrentLayer + l].m_TileProperties[num11 + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(int)m_CurrentLayer + l]] & TileProperty.TileMask) != TileProperty.TileMask;
						}
					}
					if (!flag5)
					{
						continue;
					}
					if (k == 0 && m_CurrentComplexAllocation == 0 && m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num11] != 0)
					{
						flag5 = false;
					}
					else if (limitationGroup != null && limitationGroup.m_Max <= limitationGroup.m_CurrentTotal)
					{
						flag5 = false;
					}
					else if ((tileProperty2 & tileProperty) != 0)
					{
						flag5 = false;
					}
					else if (flag2 && k == 0 && (tileProperty2 & TileProperty.NoBlockingMask) == TileProperty.NoBlockingMask)
					{
						flag5 = false;
					}
					if (flag3 && (int)m_CurrentLayer > 1 && k == 0)
					{
						TileProperty tileProperty3 = m_BuildingLayers[(uint)(m_CurrentLayer - 1)].m_TileProperties[num11 + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(uint)(m_CurrentLayer - 1)]];
						if ((tileProperty3 & (TileProperty.BlockingMask | TileProperty.TileBlockingMask)) != 0)
						{
							flag5 = false;
						}
					}
				}
				if (flag5)
				{
					PlaceTile(j, i, buildingBlock_Tile, random.Next());
				}
				num6++;
				num5++;
			}
			num6 += num7;
		}
	}

	public override void AddAreaWall(ref BuildingInstructionManager.InstructionAreaWallElement obj, bool bStorePrevious = true)
	{
		BuildingBlock_Wall buildingBlock_Wall = BuildingBlockManager.GetBlock(obj.m_BuildingBlockID) as BuildingBlock_Wall;
		BuildingBlock_Tile buildingBlock_Tile = null;
		if (buildingBlock_Wall == null || buildingBlock_Wall.BlockType != BaseBuildingBlock.BuildingBlockType.Wall)
		{
			return;
		}
		int floorTileID = buildingBlock_Wall.m_FloorTileID;
		if (floorTileID != -1)
		{
			buildingBlock_Tile = BuildingBlockManager.GetBlock(floorTileID) as BuildingBlock_Tile;
		}
		bool flag = (buildingBlock_Wall.m_ValidLayers & 0x555) != 0;
		BuildingBlockManager.LimitationGroup limitationGroup = null;
		if (buildingBlock_Wall.m_LimitationGroup != -1 && m_BuildingBlockManager.m_LimitationGroups[buildingBlock_Wall.m_LimitationGroup].m_Max != 0)
		{
			limitationGroup = m_BuildingBlockManager.m_LimitationGroups[buildingBlock_Wall.m_LimitationGroup];
		}
		TileProperty tileProperty = TileProperty.WallAndObjects;
		System.Random random = new System.Random(obj.m_iRandomSeed);
		if (bStorePrevious)
		{
			obj.m_Previous = new TileIDData[obj.m_XCount * obj.m_YCount];
			obj.m_PreviousTile = new TileIDData[obj.m_XCount * obj.m_YCount];
		}
		int num = obj.m_XPosition;
		int num2 = num + obj.m_XCount;
		int num3 = obj.m_YPosition + m_iCurrentLayerOffset_Y;
		int num4 = num3 + obj.m_YCount;
		int num5 = 0;
		int num6 = num3 * 120 + num;
		int num7 = 120 - obj.m_XCount;
		int num8 = 0;
		if (m_CurrentLayer != LevelLayers.GroundFloor)
		{
			num8 = (m_VentLayers[(uint)m_CurrentLayer] ? 1 : 2);
		}
		int num9 = 0;
		if (m_CurrentLayer != LevelLayers.Roof && !flag)
		{
			num9 = (m_VentLayers[(uint)m_CurrentLayer] ? 1 : 2);
		}
		bool flag2 = true;
		for (int i = num3; i < num4; i++)
		{
			for (int j = num; j < num2; j++)
			{
				TileProperty tileProperty2 = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num6];
				flag2 = true;
				if (num8 != 0)
				{
					flag2 = (m_BuildingLayers[(int)m_CurrentLayer - num8].m_TileProperties[num6 + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(int)m_CurrentLayer - num8]] & TileProperty.TileExistsMask) == TileProperty.TileExistsMask;
				}
				if (num9 != 0 && flag2)
				{
					for (int k = 1; k <= num9; k++)
					{
						if (!flag2)
						{
							break;
						}
						flag2 = (m_BuildingLayers[(int)m_CurrentLayer + k].m_TileProperties[num6 + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(int)m_CurrentLayer + k]] & TileProperty.TileMask) != TileProperty.TileMask;
					}
				}
				if (flag2 && buildingBlock_Tile == null)
				{
					flag2 = ((!flag) ? ((tileProperty2 & TileProperty.TileExistsMask) == TileProperty.TileMask) : ((tileProperty2 & TileProperty.TileExistsMask) == TileProperty.TileExistsMask));
				}
				if (flag2)
				{
					flag2 = (tileProperty2 & TileProperty.NoBlockingMask) == 0;
				}
				bool flag3 = (tileProperty2 & tileProperty) != 0;
				if (m_CurrentComplexAllocation != 0)
				{
					flag3 = false;
				}
				if (flag2 && (m_CurrentComplexAllocation != 0 || m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num6] == RoomProperty.EMPTY) && (limitationGroup == null || limitationGroup.m_Max > limitationGroup.m_CurrentTotal) && !flag3)
				{
					PlaceWall(j, i, buildingBlock_Wall, random.Next());
					if (buildingBlock_Tile != null)
					{
						PlaceTile(j, i, buildingBlock_Tile, random.Next(), bMarkAsChanged: false);
					}
				}
				num5++;
				num6++;
			}
			num6 += num7;
		}
	}

	public override void AddCommand(ref BuildingInstructionManager.InstructionCommandElement obj, bool bStorePrevious = true)
	{
		switch (obj.m_Commmand)
		{
		case BuildingInstructionManager.CommandsEnum.Set_Layer:
			if (bStorePrevious)
			{
				obj.m_PreviousValue = (int)m_CurrentLayer;
			}
			m_CurrentLayer = (LevelLayers)obj.m_Value;
			m_iCurrentLayerOffset_Y = m_LayerOffset[(uint)m_CurrentLayer];
			break;
		case BuildingInstructionManager.CommandsEnum.Inc_Layer:
			m_CurrentLayer++;
			break;
		case BuildingInstructionManager.CommandsEnum.Dec_Layer:
			m_CurrentLayer--;
			break;
		case BuildingInstructionManager.CommandsEnum.Set_Environment:
			if (bStorePrevious)
			{
				obj.m_PreviousValue = (int)m_CurrentEnvironment;
			}
			m_CurrentEnvironment = (LayersEnvironment)obj.m_Value;
			break;
		case BuildingInstructionManager.CommandsEnum.Start_Room:
			if (bStorePrevious)
			{
				obj.m_PreviousValue = 0;
			}
			StartComplexObjectAllocation(obj.m_Value);
			break;
		case BuildingInstructionManager.CommandsEnum.End_Room:
			obj.m_PreviousValue = m_CurrentComplexAllocation;
			EndComplexObjectAllocation();
			break;
		case BuildingInstructionManager.CommandsEnum.Start_DeleteRoom:
		{
			int commandPreviousValue = BuildingInstructionManager.GetInstance().GetCommandPreviousValue(obj.m_Value);
			if (bStorePrevious)
			{
				obj.m_PreviousValue = m_ComplexAllocations[commandPreviousValue].m_BlockID;
			}
			CancelComplexObjectAllocation(commandPreviousValue);
			SetComplexObjectAllocation(commandPreviousValue);
			m_DeleteingRoom = true;
			break;
		}
		case BuildingInstructionManager.CommandsEnum.End_DeleteRoom:
			BuildingInstructionManager.GetInstance().SetCommandPreviousValue(obj.m_Value, -1);
			if (bStorePrevious)
			{
				obj.m_PreviousValue = m_ComplexAllocations[m_CurrentComplexAllocation].m_BlockID;
			}
			EndComplexObjectAllocation();
			m_DeleteingRoom = false;
			break;
		case BuildingInstructionManager.CommandsEnum.Set_Inside:
		case BuildingInstructionManager.CommandsEnum.Set_Outside:
		{
			int value = obj.m_Value;
			int num = value & 0xFF;
			value >>= 8;
			int num2 = value & 0xFF;
			value >>= 8;
			int num3 = value & 0xFF;
			value >>= 8;
			int num4 = value & 0xFF;
			int num5 = 120 * num3 + num4;
			int num6 = 120 - num2;
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					if (obj.m_Commmand == BuildingInstructionManager.CommandsEnum.Set_Outside)
					{
						TileProperty[] tileProperties;
						int num7;
						(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num7 = num5++] = tileProperties[num7] & TileProperty.InverseEnvironmentMask;
					}
					else
					{
						TileProperty[] tileProperties;
						int num8;
						(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num8 = num5++] = tileProperties[num8] | TileProperty.EnvironmentMask;
					}
				}
				num5 += num6;
			}
			break;
		}
		}
	}

	public override void UpdateTiles()
	{
		if (!m_bIsEveryThingSetUp)
		{
			return;
		}
		TileData tile = m_BuildingLayers[0].m_Tiles_TileSystem.GetTile(10, 10);
		if (tile == null)
		{
			return;
		}
		tile.brush = null;
		tile.gameObject = null;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num = 14400;
		int num2 = -1;
		int num3 = -1;
		bool flag5 = false;
		bool flag6 = false;
		int num4 = -1;
		bool flag7 = false;
		RotorzAction rotorzAction = RotorzAction.Leave;
		RotorzAction rotorzAction2 = RotorzAction.Leave;
		LevelLayers currentLayer = m_CurrentLayer;
		for (int i = 1; i < 6; i++)
		{
			m_CurrentLayer = (LevelLayers)i;
			if (!m_BuildingLayers[i].m_Changed)
			{
				continue;
			}
			m_BuildingLayers[i].m_Changed = false;
			bool flag8 = false;
			num2 = BuildingBlockManager.GetDefaultLayerBlock((LevelLayers)i, LayersEnvironment.Inside);
			num3 = BuildingBlockManager.GetDefaultLayerBlock((LevelLayers)i, LayersEnvironment.Outside);
			if (num2 != -1)
			{
				BaseBuildingBlock block = BuildingBlockManager.GetBlock(num2);
				if (block == null || !block.m_AutomaticBlock)
				{
					num2 = -1;
				}
				else
				{
					flag5 = block.BlockType == BaseBuildingBlock.BuildingBlockType.Tile;
					flag8 = true;
				}
			}
			if (num3 != -1)
			{
				BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(num3);
				if (block2 == null || !block2.m_AutomaticBlock)
				{
					num3 = -1;
				}
				else
				{
					flag6 = block2.BlockType == BaseBuildingBlock.BuildingBlockType.Tile;
					flag8 = true;
				}
			}
			int num5 = 0;
			if (flag8)
			{
				for (int j = 0; j < 120; j++)
				{
					for (int k = 0; k < 120; k++)
					{
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.ChangedMask) != 0)
						{
							bool flag9 = (m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.EnvironmentMask) != 0;
							if (flag9 && num2 != -1)
							{
								num4 = num2;
								flag7 = flag5;
							}
							else
							{
								if (flag9 || num3 == -1)
								{
									num5++;
									continue;
								}
								num4 = num3;
								flag7 = flag6;
							}
							int num6 = 0;
							if (k > 0)
							{
								num6 = ((k < 119) ? 1 : 2);
							}
							if (j > 0)
							{
								num6 = ((j >= 119) ? (num6 + 6) : (num6 + 3));
							}
							TileProperty tileProperty = m_BuildingLayers[i].m_TileProperties[num5];
							if (flag7)
							{
								if ((tileProperty & TileProperty.TileMask) != 0 && num4 == (int)(m_BuildingLayers[i].m_TileTileIDs[num5] & TileIDData.IDMask))
								{
									if ((tileProperty & TileProperty.WallAndObjects) != 0)
									{
										TileProperty[] tileProperties;
										int num7;
										(tileProperties = m_BuildingLayers[i].m_TileProperties)[num7 = num5] = tileProperties[num7] & TileProperty.InverseTileMask;
									}
									else
									{
										bool flag10 = false;
										int num8 = m_SurroundOffsets[num6].Length - 1;
										while (num8 >= 0 && !flag10)
										{
											TileProperty tileProperty2 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num8]];
											if ((tileProperty2 & TileProperty.WallAndObjects) != 0)
											{
												flag10 = true;
											}
											else if ((tileProperty2 & TileProperty.TileMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_TileTileIDs[num5 + m_SurroundOffsets[num6][num8]] & TileIDData.IDMask))
											{
												flag10 = true;
											}
											num8--;
										}
										if (!flag10)
										{
											TileProperty[] tileProperties;
											int num9;
											(tileProperties = m_BuildingLayers[i].m_TileProperties)[num9 = num5] = tileProperties[num9] & TileProperty.InverseTileMask;
										}
									}
								}
								else if ((tileProperty & TileProperty.BlockBitMask) == 0)
								{
									bool flag11 = false;
									int num10 = m_SurroundOffsets[num6].Length - 1;
									while (num10 >= 0 && !flag11)
									{
										TileProperty tileProperty3 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num10]];
										if ((tileProperty3 & TileProperty.WallAndObjects) != 0)
										{
											flag11 = true;
										}
										else if ((tileProperty3 & TileProperty.TileMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_TileTileIDs[num5 + m_SurroundOffsets[num6][num10]] & TileIDData.IDMask))
										{
											flag11 = true;
										}
										num10--;
									}
									if (flag11)
									{
										BuildingBlock_Tile obj = BuildingBlockManager.GetBlock(num4) as BuildingBlock_Tile;
										PlaceTile(k, j, obj, UnityEngine.Random.Range(0, 10000), bMarkAsChanged: false);
									}
								}
							}
							else if ((tileProperty & TileProperty.WallMask) != 0 && num4 == (int)(m_BuildingLayers[i].m_WallTileIDs[num5] & TileIDData.IDMask))
							{
								if ((tileProperty & (TileProperty.ObjDecMask | TileProperty.TileMask)) != 0)
								{
									TileProperty[] tileProperties;
									int num11;
									(tileProperties = m_BuildingLayers[i].m_TileProperties)[num11 = num5] = tileProperties[num11] & ~(TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
								}
								else
								{
									bool flag12 = false;
									int num12 = m_SurroundOffsets[num6].Length - 1;
									while (num12 >= 0 && !flag12)
									{
										TileProperty tileProperty4 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num12]];
										if ((tileProperty4 & (TileProperty.ObjDecMask | TileProperty.TileMask)) != 0)
										{
											flag12 = true;
										}
										else if ((tileProperty4 & TileProperty.WallMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_WallTileIDs[num5 + m_SurroundOffsets[num6][num12]] & TileIDData.IDMask))
										{
											flag12 = true;
										}
										num12--;
									}
									if (!flag12)
									{
										TileProperty[] tileProperties;
										int num13;
										(tileProperties = m_BuildingLayers[i].m_TileProperties)[num13 = num5] = tileProperties[num13] & ~(TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
									}
								}
							}
							else if ((tileProperty & TileProperty.BlockBitMask) == 0)
							{
								bool flag13 = false;
								int num14 = m_SurroundOffsets[num6].Length - 1;
								while (num14 >= 0 && !flag13)
								{
									int num15 = num5 + m_SurroundOffsets[num6][num14];
									if (num15 >= 0 && num15 < num)
									{
										TileProperty tileProperty5 = m_BuildingLayers[i].m_TileProperties[num15];
										TileProperty tileProperty6 = TileProperty.ObjDecMask | TileProperty.TileMask;
										if ((tileProperty5 & tileProperty6) != 0)
										{
											flag13 = true;
										}
										else if ((tileProperty5 & TileProperty.WallMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_WallTileIDs[num15] & TileIDData.IDMask))
										{
											flag13 = true;
										}
									}
									num14--;
								}
								if (flag13)
								{
									BuildingBlock_Wall obj2 = BuildingBlockManager.GetBlock(num4) as BuildingBlock_Wall;
									PlaceWall(k, j, obj2, UnityEngine.Random.Range(0, 10000), bMarkAsChanged: false);
								}
							}
						}
						num5++;
					}
				}
			}
			num5 = 0;
			for (int l = 0; l < 120; l++)
			{
				for (int m = 0; m < 120; m++)
				{
					rotorzAction = RotorzAction.Leave;
					rotorzAction2 = RotorzAction.Leave;
					if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.ChangedMask) != 0)
					{
						TileProperty[] tileProperties;
						int num16;
						(tileProperties = m_BuildingLayers[i].m_TileProperties)[num16 = num5] = tileProperties[num16] & TileProperty.InverseChangedMask;
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.TileMask) == 0 && m_BuildingLayers[i].m_TileTileObjects[num5] != null)
						{
							UnityEngine.Object.Destroy(m_BuildingLayers[i].m_TileTileObjects[num5]);
							m_BuildingLayers[i].m_TileTileObjects[num5] = null;
							m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_TileTileIDs[num5], bAdd: false);
							TileIDData[] tileTileIDs;
							int num17;
							(tileTileIDs = m_BuildingLayers[i].m_TileTileIDs)[num17 = num5] = tileTileIDs[num17] | TileIDData.IDMask;
							rotorzAction = RotorzAction.Remove;
						}
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.WallMask) == 0 && m_BuildingLayers[i].m_WallTileObjects[num5] != null)
						{
							UnityEngine.Object.Destroy(m_BuildingLayers[i].m_WallTileObjects[num5]);
							m_BuildingLayers[i].m_WallTileObjects[num5] = null;
							m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_WallTileIDs[num5], bAdd: false);
							TileIDData[] tileTileIDs;
							int num18;
							(tileTileIDs = m_BuildingLayers[i].m_WallTileIDs)[num18 = num5] = tileTileIDs[num18] | TileIDData.IDMask;
							rotorzAction2 = RotorzAction.Remove;
						}
						int num19 = 0;
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.WallMask) != 0 || (m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.TileMask) != 0)
						{
							flag = m != 0;
							flag2 = m < 119;
							flag3 = l < 119;
							flag4 = l != 0;
							BaseBuildingBlock.GroupFlags groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
							if (flag)
							{
								groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
								num19 = num5 - 1;
								m_SurroundingBlocks[3] = GetTheTileIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[11] = GetTheWallIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[19] = GetTheObjectIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[27] = (int)groupFlags;
								if (flag3)
								{
									groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
									num19 = num5 - 1 + 120;
									m_SurroundingBlocks[0] = GetTheTileIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[8] = GetTheWallIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[16] = GetTheObjectIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[24] = (int)groupFlags;
								}
								else
								{
									m_SurroundingBlocks[0] = -1;
									m_SurroundingBlocks[8] = -1;
									m_SurroundingBlocks[16] = -1;
									m_SurroundingBlocks[24] = 0;
								}
								if (flag4)
								{
									groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
									num19 = num5 - 1 - 120;
									m_SurroundingBlocks[5] = GetTheTileIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[13] = GetTheWallIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[21] = GetTheObjectIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[29] = (int)groupFlags;
								}
								else
								{
									m_SurroundingBlocks[5] = -1;
									m_SurroundingBlocks[13] = -1;
									m_SurroundingBlocks[21] = -1;
									m_SurroundingBlocks[29] = 0;
								}
							}
							else
							{
								m_SurroundingBlocks[3] = -1;
								m_SurroundingBlocks[11] = -1;
								m_SurroundingBlocks[19] = -1;
								m_SurroundingBlocks[27] = 0;
								m_SurroundingBlocks[0] = -1;
								m_SurroundingBlocks[8] = -1;
								m_SurroundingBlocks[16] = -1;
								m_SurroundingBlocks[24] = 0;
								m_SurroundingBlocks[5] = -1;
								m_SurroundingBlocks[13] = -1;
								m_SurroundingBlocks[21] = -1;
								m_SurroundingBlocks[29] = 0;
							}
							if (flag2)
							{
								groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
								num19 = num5 + 1;
								m_SurroundingBlocks[4] = GetTheTileIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[12] = GetTheWallIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[20] = GetTheObjectIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[28] = (int)groupFlags;
								if (flag3)
								{
									groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
									num19 = num5 + 1 + 120;
									m_SurroundingBlocks[2] = GetTheTileIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[10] = GetTheWallIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[18] = GetTheObjectIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[26] = (int)groupFlags;
								}
								else
								{
									m_SurroundingBlocks[2] = -1;
									m_SurroundingBlocks[10] = -1;
									m_SurroundingBlocks[18] = -1;
									m_SurroundingBlocks[26] = 0;
								}
								if (flag4)
								{
									groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
									num19 = num5 + 1 - 120;
									m_SurroundingBlocks[7] = GetTheTileIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[15] = GetTheWallIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[23] = GetTheObjectIDAt(i, num19, ref groupFlags);
									m_SurroundingBlocks[31] = (int)groupFlags;
								}
								else
								{
									m_SurroundingBlocks[7] = -1;
									m_SurroundingBlocks[15] = -1;
									m_SurroundingBlocks[23] = -1;
									m_SurroundingBlocks[31] = 0;
								}
							}
							else
							{
								m_SurroundingBlocks[4] = -1;
								m_SurroundingBlocks[12] = -1;
								m_SurroundingBlocks[20] = -1;
								m_SurroundingBlocks[28] = 0;
								m_SurroundingBlocks[2] = -1;
								m_SurroundingBlocks[10] = -1;
								m_SurroundingBlocks[18] = -1;
								m_SurroundingBlocks[26] = 0;
								m_SurroundingBlocks[7] = -1;
								m_SurroundingBlocks[15] = -1;
								m_SurroundingBlocks[23] = -1;
								m_SurroundingBlocks[31] = 0;
							}
							if (flag3)
							{
								groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
								num19 = num5 + 120;
								m_SurroundingBlocks[1] = GetTheTileIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[9] = GetTheWallIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[17] = GetTheObjectIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[25] = (int)groupFlags;
							}
							else
							{
								m_SurroundingBlocks[1] = -1;
								m_SurroundingBlocks[9] = -1;
								m_SurroundingBlocks[17] = -1;
								m_SurroundingBlocks[25] = 0;
							}
							if (flag4)
							{
								groupFlags = BaseBuildingBlock.GroupFlags.EMPTY;
								num19 = num5 - 120;
								m_SurroundingBlocks[6] = GetTheTileIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[14] = GetTheWallIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[22] = GetTheObjectIDAt(i, num19, ref groupFlags);
								m_SurroundingBlocks[30] = (int)groupFlags;
							}
							else
							{
								m_SurroundingBlocks[6] = -1;
								m_SurroundingBlocks[14] = -1;
								m_SurroundingBlocks[22] = -1;
								m_SurroundingBlocks[30] = 0;
							}
							int num20 = (int)(m_BuildingLayers[i].m_TileTileIDs[num5] & TileIDData.IDMask);
							if (num20 != 16383)
							{
								BuildingBlock_Tile buildingBlock_Tile = (BuildingBlock_Tile)BuildingBlockManager.GetBlock(num20);
								if (buildingBlock_Tile != null)
								{
									int num21 = (int)(m_BuildingLayers[i].m_TileTileIDs[num5] & TileIDData.SeedMask);
									num21 >>= 22;
									int applicableTMSVariant = buildingBlock_Tile.GetApplicableTMSVariant(m_SurroundingBlocks, num21);
									if (applicableTMSVariant != -1)
									{
										int num22 = (int)(m_BuildingLayers[i].m_TileTileIDs[num5] & TileIDData.VariantMask) >> 14;
										if (applicableTMSVariant != num22 || m_BuildingLayers[i].m_TileTileObjects[num5] == null)
										{
											int num23;
											TileIDData[] tileTileIDs;
											(tileTileIDs = m_BuildingLayers[i].m_TileTileIDs)[num23 = num5] = tileTileIDs[num23] & TileIDData.InverseVariantMask;
											int num24;
											(tileTileIDs = m_BuildingLayers[i].m_TileTileIDs)[num24 = num5] = (TileIDData)((int)tileTileIDs[num24] | (applicableTMSVariant << 14));
											GameObject realObject = buildingBlock_Tile.GetRealObject(applicableTMSVariant);
											if (realObject == null)
											{
												return;
											}
											GameObject gameObject = UnityEngine.Object.Instantiate(realObject, m_BuildingLayers[i].m_Tiles.transform);
											float x = (float)m + m_fPositionOffsetsX[(int)buildingBlock_Tile.BlockType];
											float y = (float)l + m_fPositionOffsetsY[(int)buildingBlock_Tile.BlockType];
											gameObject.transform.localPosition = new Vector3(x, y, 0f);
											gameObject.SetActive(value: true);
											GameObject gameObject2 = m_BuildingLayers[i].m_TileTileObjects[num5];
											if (gameObject2 != null)
											{
												UnityEngine.Object.Destroy(gameObject2);
											}
											m_BuildingLayers[i].m_TileTileObjects[num5] = gameObject;
											rotorzAction = RotorzAction.Add;
										}
									}
								}
								else
								{
									m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_TileTileIDs[num5], bAdd: false);
									TileIDData[] tileTileIDs;
									int num25;
									(tileTileIDs = m_BuildingLayers[i].m_TileTileIDs)[num25 = num5] = tileTileIDs[num25] | TileIDData.IDMask;
								}
							}
							num20 = (int)(m_BuildingLayers[i].m_WallTileIDs[num5] & TileIDData.IDMask);
							if (num20 != 16383)
							{
								BuildingBlock_Wall buildingBlock_Wall = (BuildingBlock_Wall)BuildingBlockManager.GetBlock(num20);
								if (buildingBlock_Wall != null)
								{
									int num26 = (int)(m_BuildingLayers[i].m_WallTileIDs[num5] & TileIDData.SeedMask);
									num26 >>= 22;
									int applicableTMSVariant2 = buildingBlock_Wall.GetApplicableTMSVariant(m_SurroundingBlocks, num26);
									if (applicableTMSVariant2 != -1)
									{
										int num27 = (int)(m_BuildingLayers[i].m_WallTileIDs[num5] & TileIDData.VariantMask) >> 14;
										if (applicableTMSVariant2 != num27 || m_BuildingLayers[i].m_WallTileObjects[num5] == null)
										{
											int num28;
											TileIDData[] tileTileIDs;
											(tileTileIDs = m_BuildingLayers[i].m_WallTileIDs)[num28 = num5] = tileTileIDs[num28] & TileIDData.InverseVariantMask;
											int num29;
											(tileTileIDs = m_BuildingLayers[i].m_WallTileIDs)[num29 = num5] = (TileIDData)((int)tileTileIDs[num29] | (applicableTMSVariant2 << 14));
											GameObject realObject2 = buildingBlock_Wall.GetRealObject(applicableTMSVariant2);
											if (realObject2 == null)
											{
												return;
											}
											GameObject gameObject3 = UnityEngine.Object.Instantiate(realObject2, m_BuildingLayers[i].m_Walls.transform);
											float x2 = (float)m + m_fPositionOffsetsX[(int)buildingBlock_Wall.BlockType];
											float y2 = (float)l + m_fPositionOffsetsY[(int)buildingBlock_Wall.BlockType] + (gameObject3.transform.localScale.y - 1f) / 2f;
											gameObject3.transform.localPosition = new Vector3(x2, y2, 0f);
											gameObject3.SetActive(value: true);
											GameObject gameObject4 = m_BuildingLayers[i].m_WallTileObjects[num5];
											if (gameObject4 != null)
											{
												UnityEngine.Object.Destroy(gameObject4);
											}
											m_BuildingLayers[i].m_WallTileObjects[num5] = gameObject3;
											rotorzAction2 = RotorzAction.Add;
											if (buildingBlock_Wall.m_HasPhotonViewCount > 0)
											{
												m_ObjectsWithPhotonViews.Add(gameObject3);
											}
										}
									}
								}
								else
								{
									m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_WallTileIDs[num5], bAdd: false);
									TileIDData[] tileTileIDs;
									int num30;
									(tileTileIDs = m_BuildingLayers[i].m_WallTileIDs)[num30 = num5] = tileTileIDs[num30] | TileIDData.IDMask;
								}
							}
						}
					}
					int row = 119 - l;
					if (rotorzAction != 0)
					{
						TileData tile2 = m_BuildingLayers[i].m_Tiles_TileSystem.GetTile(row, m);
						switch (rotorzAction)
						{
						case RotorzAction.Add:
						{
							if (tile2 != null)
							{
								tile2.gameObject = m_BuildingLayers[i].m_TileTileObjects[num5];
								break;
							}
							TileData tileData = new TileData();
							tileData.SetFrom(tile);
							tileData.gameObject = m_BuildingLayers[i].m_TileTileObjects[num5];
							m_BuildingLayers[i].m_Tiles_TileSystem.SetTileFrom(row, m, tileData);
							break;
						}
						case RotorzAction.Remove:
							if (tile2 != null)
							{
								tile2.gameObject = null;
							}
							break;
						}
					}
					if (rotorzAction2 != 0)
					{
						TileData tile3 = m_BuildingLayers[i].m_Walls_TileSystem.GetTile(row, m);
						switch (rotorzAction2)
						{
						case RotorzAction.Add:
						{
							if (tile3 != null)
							{
								tile3.gameObject = m_BuildingLayers[i].m_WallTileObjects[num5];
								break;
							}
							TileData tileData2 = new TileData();
							tileData2.SetFrom(tile);
							tileData2.gameObject = m_BuildingLayers[i].m_WallTileObjects[num5];
							m_BuildingLayers[i].m_Walls_TileSystem.SetTile(row, m, tileData2);
							break;
						}
						case RotorzAction.Remove:
							if (tile3 != null)
							{
								tile3.gameObject = null;
							}
							break;
						}
					}
					num5++;
				}
			}
		}
		m_CurrentLayer = currentLayer;
	}

	public override void AddDelete(ref BuildingInstructionManager.InstructionDeleteElement obj, bool bStorePrevious = true)
	{
		int num = (obj.m_YPosition + m_iCurrentLayerOffset_Y) * 120 + obj.m_XPosition;
		switch (obj.m_DeleteType)
		{
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Tile:
		{
			bool flag = true;
			if (m_CurrentComplexAllocation != 0 && BaseLevelManager.IsRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation) && BaseLevelManager.TotalRoomNumbersInProperty(m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num]) != 1)
			{
				flag = false;
			}
			GameObject gameObject2 = m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num];
			if (gameObject2 != null)
			{
				UnityEngine.Object.Destroy(gameObject2);
				m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num] = null;
			}
			if (flag)
			{
				int num10;
				TileProperty[] tileProperties;
				(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num10 = num] = tileProperties[num10] & TileProperty.InverseTileMask;
				m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num], bAdd: false);
				m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
				if ((int)m_CurrentLayer > 1)
				{
					int num11;
					(tileProperties = m_BuildingLayers[(uint)(m_CurrentLayer - 1)].m_TileProperties)[num11 = num] = tileProperties[num11] & TileProperty.InverseNoBlockingMask;
				}
			}
			if (m_CurrentComplexAllocation != 0)
			{
				BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
			}
			SetAreaAsChanged(obj.m_XPosition - 1, obj.m_YPosition - 1 + m_iCurrentLayerOffset_Y, 3, 3);
			break;
		}
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Wall:
		{
			bool flag2 = true;
			if (m_CurrentComplexAllocation != 0 && BaseLevelManager.IsRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation) && BaseLevelManager.TotalRoomNumbersInProperty(m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num]) != 1)
			{
				flag2 = false;
			}
			GameObject gameObject3 = m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num];
			if (gameObject3 != null)
			{
				UnityEngine.Object.Destroy(gameObject3);
				m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num] = null;
			}
			if (flag2)
			{
				TileProperty[] tileProperties;
				int num12;
				(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num12 = num] = tileProperties[num12] & ~(TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
				m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num], bAdd: false);
				m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
			}
			if (m_CurrentComplexAllocation != 0)
			{
				BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
			}
			SetAreaAsChanged(obj.m_XPosition - 1, obj.m_YPosition - 1 + m_iCurrentLayerOffset_Y, 3, 3);
			break;
		}
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object:
		{
			TileIDData tileIDData = ValidatePrevious(m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num]);
			BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock((int)(tileIDData & TileIDData.IDMask)) as BuildingBlock_Object;
			if (buildingBlock_Object == null)
			{
				break;
			}
			int num2 = obj.m_XPosition - buildingBlock_Object.m_Footprint.m_iFirstX;
			int num3 = obj.m_YPosition - buildingBlock_Object.m_Footprint.m_iFirstY + m_iCurrentLayerOffset_Y;
			int num4 = Mathf.Clamp(num2 + buildingBlock_Object.m_Footprint.m_iW, 0, 120);
			int num5 = Mathf.Clamp(num3 + buildingBlock_Object.m_Footprint.m_iH, 0, 120);
			num2 = Mathf.Clamp(num2, 0, 120);
			num3 = Mathf.Clamp(num3, 0, 120);
			TileProperty tileProperty = ((buildingBlock_Object.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? TileProperty.InverseDecorationMask : TileProperty.InverseObjectMask);
			if (buildingBlock_Object.m_Solid)
			{
				tileProperty &= TileProperty.InverseBlockingMask;
			}
			if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) == BuildingBlock_Object.SpecialFlagsEnum.Entrance)
			{
				tileProperty &= TileProperty.InverseEntranceMask;
			}
			if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) == BuildingBlock_Object.SpecialFlagsEnum.Exit)
			{
				tileProperty &= TileProperty.InverseExitMask;
			}
			if (buildingBlock_Object.m_ItsADoor)
			{
				tileProperty &= TileProperty.InverseItsADoorMask;
			}
			m_BuildingBlockManager.AdjustLimitationTotal(buildingBlock_Object.m_ID, bAdd: false);
			int num6 = num3 * 120 + num2;
			int num7 = 120 - buildingBlock_Object.m_Footprint.m_iW;
			int num8 = 0;
			for (int i = num3; i < num5; i++)
			{
				for (int j = num2; j < num4; j++)
				{
					if ((buildingBlock_Object.m_Footprint.m_UsedTiles[num8++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
					{
						num6 = i * 120 + j;
						m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num6] = TileIDData.IDMask;
						TileProperty[] tileProperties;
						int num9;
						(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num9 = num6] = tileProperties[num9] & tileProperty;
						if (m_CurrentComplexProcessed != 0)
						{
							BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num6, m_CurrentComplexAllocation);
						}
						GameObject gameObject = m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num6];
						if (gameObject != null)
						{
							UnityEngine.Object.Destroy(gameObject);
						}
					}
					num6++;
				}
				num6 += num7;
			}
			SetAreaAsChanged(num2 - 1, num3 - 1, num4 - num2 + 2, num5 - num3 + 2);
			break;
		}
		}
	}

	private void PlaceTile(int X, int Y, BuildingBlock_Tile obj, int seed, bool bMarkAsChanged = true, bool bReplaceSame = false)
	{
		int num = Y * 120 + X;
		TileIDData tileIDData = (TileIDData)((seed << 22) & 0x3FC00000);
		TileIDData tileIDData2 = m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num];
		TileIDData tileIDData3 = tileIDData2;
		TileProperty tileProperty = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num];
		TileProperty tileProperty2 = TileProperty.EMPTY;
		TileIDData tileIDData4;
		if (obj != null)
		{
			tileIDData4 = (TileIDData)obj.m_ID;
			if (((uint)obj.m_ValidLayers & 0x555u) != 0)
			{
				tileProperty2 = TileProperty.EnvironmentMask;
			}
		}
		else
		{
			tileIDData4 = TileIDData.IDMask;
		}
		if ((tileIDData2 & TileIDData.IDMask) != tileIDData4)
		{
			GameObject gameObject = m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num] = null;
			}
			tileIDData2 = tileIDData4 | TileIDData.VariantMask | tileIDData;
		}
		else
		{
			tileIDData2 = (tileIDData2 & TileIDData.InverseSeedMask) | tileIDData;
		}
		int num2;
		TileProperty[] tileProperties;
		(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num2 = num] = tileProperties[num2] & ~(TileProperty.EnvironmentMask | TileProperty.TileBlockingMask);
		int num3;
		(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num3 = num] = tileProperties[num3] | (TileProperty.TileMask | tileProperty2);
		if (obj.m_BlockingTile)
		{
			int num4;
			(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num4 = num] = tileProperties[num4] | TileProperty.TileBlockingMask;
		}
		if ((int)m_CurrentLayer > 1)
		{
			if (obj.m_NoBlockingBelow)
			{
				int num5;
				(tileProperties = m_BuildingLayers[(uint)(m_CurrentLayer - 1)].m_TileProperties)[num5 = num + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(uint)(m_CurrentLayer - 1)]] = tileProperties[num5] | TileProperty.NoBlockingMask;
			}
			else
			{
				int num6;
				(tileProperties = m_BuildingLayers[(uint)(m_CurrentLayer - 1)].m_TileProperties)[num6 = num + m_DifferentLayerOffsets[(uint)m_CurrentLayer][(uint)(m_CurrentLayer - 1)]] = tileProperties[num6] & TileProperty.InverseNoBlockingMask;
			}
		}
		if (!m_DeleteingRoom)
		{
			if (m_CurrentComplexAllocation != 0)
			{
				BaseLevelManager.AddRoomNumberToProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
				tileIDData2 |= TileIDData.ComplexMask;
			}
		}
		else
		{
			BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
			tileIDData2 &= TileIDData.InverseSeedMask;
		}
		if (obj == null)
		{
			int num7;
			(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num7 = num] = tileProperties[num7] & TileProperty.InverseTileMask;
		}
		bool flag = true;
		if (!bReplaceSame && m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num] == tileProperty && (tileIDData2 & TileIDData.InverseSeedMask) == (tileIDData3 & TileIDData.InverseSeedMask))
		{
			flag = false;
		}
		if (flag)
		{
			m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num], bAdd: false);
			m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num] = tileIDData2;
			m_BuildingBlockManager.AdjustLimitationTotal(tileIDData2, bAdd: true);
		}
		if (bMarkAsChanged)
		{
			SetAreaAsChanged(X - 1, Y - 1, 3, 3);
		}
	}

	protected virtual void PlaceObject(int X, int Y, BuildingBlock_Object obj, bool bMarkAsChanged = true)
	{
		GameObject realObject = obj.GetRealObject(0);
		if (realObject == null)
		{
			return;
		}
		GameObject gameObject = null;
		gameObject = ((obj.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? UnityEngine.Object.Instantiate(realObject, m_BuildingLayers[(uint)m_CurrentLayer].m_Decorations.transform) : UnityEngine.Object.Instantiate(realObject, m_BuildingLayers[(uint)m_CurrentLayer].m_Objects.transform));
		float x = (float)X + m_fPositionOffsetsX[(int)obj.BlockType] + realObject.transform.localPosition.x;
		float y = (float)Y + m_fPositionOffsetsY[(int)obj.BlockType] + realObject.transform.localPosition.y;
		gameObject.transform.localPosition = new Vector3(x, y, 0f);
		if (obj.m_HasPhotonViewCount > 0)
		{
			m_ObjectsWithPhotonViews.Add(gameObject);
		}
		int num = X + obj.m_Footprint.m_iLeft;
		int num2 = Y + obj.m_Footprint.m_iBottom;
		int num3 = Mathf.Clamp(num + obj.m_Footprint.m_iW, 0, 120);
		int num4 = Mathf.Clamp(num2 + obj.m_Footprint.m_iH, 0, 120);
		num = Mathf.Clamp(num, 0, 120);
		num2 = Mathf.Clamp(num2, 0, 120);
		TileProperty tileProperty = ((obj.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? TileProperty.DecorationMask : TileProperty.ObjectMask);
		if (obj.m_Solid)
		{
			tileProperty |= TileProperty.BlockingMask;
			if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksAll)
			{
				tileProperty |= TileProperty.Blocked_All_Mask;
			}
			else if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksHorizontal)
			{
				tileProperty |= TileProperty.Blocked_Horizontal_Bits;
			}
			else if (obj.m_BlockingDirection == BuildingBlock_Object.BlockingDirection.BlocksVerticle)
			{
				tileProperty |= TileProperty.Blocked_Vertical_Mask;
			}
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) == BuildingBlock_Object.SpecialFlagsEnum.Entrance)
		{
			tileProperty |= TileProperty.EntranceMask;
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) == BuildingBlock_Object.SpecialFlagsEnum.Exit)
		{
			tileProperty |= TileProperty.ExitMask;
		}
		TileIDData tileIDData = TileIDData.EMPTY;
		tileIDData = ((obj.m_ID != -1) ? ((TileIDData)(obj.m_ID & 0x3FFF)) : TileIDData.IDMask);
		if (m_CurrentComplexAllocation != 0)
		{
			tileIDData |= TileIDData.ComplexMask;
		}
		m_BuildingBlockManager.AdjustLimitationTotal(tileIDData, bAdd: true);
		int num5 = num2 * 120 + num;
		int num6 = 120 - obj.m_Footprint.m_iW;
		int num7 = 0;
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				if ((obj.m_Footprint.m_UsedTiles[num7++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
				{
					m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num5] = tileIDData;
					TileProperty[] tileProperties;
					int num8;
					(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num8 = num5] = tileProperties[num8] | tileProperty;
					if (m_CurrentComplexAllocation != 0)
					{
						BaseLevelManager.AddRoomNumberToProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num5, m_CurrentComplexAllocation);
					}
					GameObject gameObject2 = m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num5];
					if (gameObject2 != null)
					{
						UnityEngine.Object.Destroy(gameObject2);
					}
					m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num5] = gameObject;
				}
				num5++;
			}
			num5 += num6;
		}
		if (bMarkAsChanged)
		{
			SetAreaAsChanged(num - 1, num2 - 1, num3 - num + 2, num4 - num2 + 2);
		}
		gameObject.SetActive(value: true);
	}

	private void PlaceWall(int X, int Y, BuildingBlock_Wall obj, int seed, bool bMarkAsChanged = true, bool bReplaceSame = false)
	{
		int num = Y * 120 + X;
		TileIDData tileIDData = (TileIDData)((seed << 22) & 0x3FC00000);
		TileIDData tileIDData2 = m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num];
		TileIDData tileIDData3 = tileIDData2;
		TileProperty tileProperty = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num];
		TileIDData iD = (TileIDData)obj.m_ID;
		if ((tileIDData2 & TileIDData.IDMask) != iD)
		{
			GameObject gameObject = m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num] = null;
			}
			tileIDData2 = iD | TileIDData.VariantMask | tileIDData;
		}
		else
		{
			tileIDData2 = (tileIDData2 & TileIDData.InverseSeedMask) | tileIDData;
		}
		if (m_CurrentComplexAllocation != 0)
		{
			tileIDData2 |= TileIDData.ComplexMask;
		}
		TileProperty[] tileProperties;
		int num2;
		(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num2 = num] = tileProperties[num2] | (TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
		if (m_CurrentComplexAllocation != 0)
		{
			BaseLevelManager.AddRoomNumberToProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
		}
		bool flag = true;
		if (!bReplaceSame && m_CurrentComplexAllocation == 0 && m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties[num] == tileProperty && (tileIDData2 & TileIDData.InverseSeedMask) == (tileIDData3 & TileIDData.InverseSeedMask))
		{
			flag = false;
		}
		if (flag)
		{
			m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num], bAdd: false);
			m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num] = tileIDData2;
			m_BuildingBlockManager.AdjustLimitationTotal(tileIDData2, bAdd: true);
		}
		if (bMarkAsChanged)
		{
			SetAreaAsChanged(X - 1, Y - 1, 3, 3);
		}
	}

	public TileIDData ValidatePrevious(TileIDData iPreviousTile)
	{
		int num = (int)(iPreviousTile & TileIDData.IDMask);
		if (num == 16383)
		{
			return iPreviousTile;
		}
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(num);
		if (block == null || block.m_AutomaticBlock)
		{
			return TileIDData.IDMask | TileIDData.VariantMask;
		}
		return iPreviousTile;
	}

	[ContextMenu("Copy To Other")]
	public void Copy()
	{
		CopyBase();
	}

	public override void CreateZone(ref BuildingInstructionManager.InstructionZoneElement obj)
	{
		if (!(m_ZoneManager == null) || !((m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null))
		{
			obj.m_iID = m_ZoneManager.CreateAZone(obj.m_ZoneType, m_CurrentLayer, obj.m_Left, obj.m_Bottom + m_iCurrentLayerOffset_Y, obj.m_Width, obj.m_Height, obj.m_ZonePrint, obj.m_iID, bCreateUI: false);
		}
	}

	public override void DeleteZone(ref BuildingInstructionManager.InstructionZoneElement obj)
	{
		if (!(m_ZoneManager == null) || !((m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null))
		{
			LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(obj.m_iID);
			if (zone != null)
			{
				m_ZoneManager.ReleaseZone(zone.m_ID);
			}
		}
	}

	public override void AddToZone(ref BuildingInstructionManager.InstructionZoneElement obj)
	{
		if (!(m_ZoneManager == null) || !((m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null))
		{
			m_ZoneManager.AddToZone(obj.m_iID, obj.m_Left, obj.m_Bottom + m_iCurrentLayerOffset_Y, obj.m_Width, obj.m_Height, ref obj.m_ZonePrint);
		}
	}

	public override void SubtractFromZone(ref BuildingInstructionManager.InstructionZoneElement obj)
	{
		if (!(m_ZoneManager == null) || !((m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null))
		{
			m_ZoneManager.SubtractFromZone(obj.m_iID, obj.m_Left, obj.m_Bottom + m_iCurrentLayerOffset_Y, obj.m_Width, obj.m_Height, ref obj.m_ZonePrint);
		}
	}
}
