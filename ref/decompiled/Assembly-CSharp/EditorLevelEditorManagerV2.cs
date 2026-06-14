using UnityEngine;

public class EditorLevelEditorManagerV2 : EditorLevelEditorManager
{
	public override bool IsEverythingSetUp()
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			return base.IsEverythingSetUp();
		}
		return base.IsEverythingSetUp();
	}

	protected override void PlaceObject(int X, int Y, BuildingBlock_Object obj, bool bMarkAsChanged = true)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease || !obj.m_ZoneObject)
		{
			base.PlaceObject(X, Y, obj, bMarkAsChanged);
			return;
		}
		GameObject visualRep = obj.GetVisualRep(0);
		if (visualRep == null)
		{
			return;
		}
		GameObject gameObject = Object.Instantiate(visualRep, m_BuildingLayers[(uint)m_CurrentLayer].m_Objects.transform);
		float x = (float)X + m_fPositionOffsetsX[(int)obj.BlockType] + visualRep.transform.localPosition.x;
		float y = (float)Y + m_fPositionOffsetsY[(int)obj.BlockType] + visualRep.transform.localPosition.y;
		float z = 0f - (float)(120 - Y) / 240f;
		gameObject.transform.localPosition = new Vector3(x, y, z);
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
		if (obj.m_ItsADoor)
		{
			tileProperty |= TileProperty.ItsADoorMask;
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
						Object.Destroy(gameObject2);
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
	}

	protected override void RemoveObject(int X, int Y, BuildingBlock_Object obj, bool bMarkAsChanged = true)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease || !obj.m_ZoneObject)
		{
			base.RemoveObject(X, Y, obj, bMarkAsChanged);
			return;
		}
		int num = X + obj.m_Footprint.m_iLeft;
		int num2 = Y + obj.m_Footprint.m_iBottom;
		int num3 = Mathf.Clamp(num + obj.m_Footprint.m_iW, 0, 120);
		int num4 = Mathf.Clamp(num2 + obj.m_Footprint.m_iH, 0, 120);
		num = Mathf.Clamp(num, 0, 120);
		num2 = Mathf.Clamp(num2, 0, 120);
		TileProperty tileProperty = ((obj.BlockType != BaseBuildingBlock.BuildingBlockType.Object) ? TileProperty.InverseDecorationMask : TileProperty.InverseObjectMask);
		if (obj.m_Solid)
		{
			tileProperty &= TileProperty.InverseBlockingMask;
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) == BuildingBlock_Object.SpecialFlagsEnum.Entrance)
		{
			tileProperty &= TileProperty.InverseEntranceMask;
		}
		if ((obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) == BuildingBlock_Object.SpecialFlagsEnum.Exit)
		{
			tileProperty &= TileProperty.InverseExitMask;
		}
		if (obj.m_ItsADoor)
		{
			tileProperty &= TileProperty.InverseItsADoorMask;
		}
		m_BuildingBlockManager.AdjustLimitationTotal(obj.m_ID, bAdd: false);
		int num5 = num2 * 120 + num;
		int num6 = 120 - obj.m_Footprint.m_iW;
		int num7 = 0;
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				if ((obj.m_Footprint.m_UsedTiles[num7++] & Footprint.BlockTypes.Objects) == Footprint.BlockTypes.Objects)
				{
					m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num5] = TileIDData.IDMask;
					TileProperty[] tileProperties;
					int num8;
					(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num8 = num5] = tileProperties[num8] & tileProperty;
					if (m_CurrentComplexProcessed != 0)
					{
						BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num5, (int)m_CurrentComplexProcessed);
					}
					GameObject gameObject = m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileObjects[num5];
					if (gameObject != null)
					{
						Object.Destroy(gameObject);
					}
				}
				num5++;
			}
			num5 += num6;
		}
		if (bMarkAsChanged)
		{
			SetAreaAsChanged(num - 1, num2 - 1, num3 - num + 2, num4 - num2 + 2);
		}
	}

	public override void AddDelete(ref BuildingInstructionManager.InstructionDeleteElement obj, bool bStorePrevious = true)
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			base.AddDelete(ref obj, bStorePrevious);
			return;
		}
		int num = obj.m_YPosition * 120 + obj.m_XPosition;
		switch (obj.m_DeleteType)
		{
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Tile:
		{
			bool flag2 = true;
			if (m_CurrentComplexAllocation != 0 && BaseLevelManager.IsRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation) && BaseLevelManager.TotalRoomNumbersInProperty(m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num]) != 1)
			{
				flag2 = false;
			}
			TileIDData previous2 = ValidatePrevious(m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num]);
			if (bStorePrevious)
			{
				obj.m_Previous = previous2;
			}
			GameObject gameObject3 = m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num];
			if (gameObject3 != null)
			{
				Object.Destroy(gameObject3);
				m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileObjects[num] = null;
			}
			if (flag2)
			{
				int num11;
				TileProperty[] tileProperties;
				(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num11 = num] = tileProperties[num11] & TileProperty.InverseTileMask;
				m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num], bAdd: false);
				m_BuildingLayers[(uint)m_CurrentLayer].m_TileTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
				if ((int)m_CurrentLayer > 1)
				{
					int num12;
					(tileProperties = m_BuildingLayers[(uint)(m_CurrentLayer - 1)].m_TileProperties)[num12 = num] = tileProperties[num12] & TileProperty.InverseNoBlockingMask;
				}
			}
			if (m_CurrentComplexAllocation != 0)
			{
				BaseLevelManager.RemoveRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation);
			}
			SetAreaAsChanged(obj.m_XPosition - 1, obj.m_YPosition - 1, 3, 3);
			break;
		}
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Wall:
		{
			bool flag = true;
			if (m_CurrentComplexAllocation != 0 && BaseLevelManager.IsRoomNumberInProperty(ref m_BuildingLayers[(uint)m_CurrentLayer], num, m_CurrentComplexAllocation) && BaseLevelManager.TotalRoomNumbersInProperty(m_BuildingLayers[(uint)m_CurrentLayer].m_RoomPropertiesMasks[num]) != 1)
			{
				flag = false;
			}
			TileIDData previous = ValidatePrevious(m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num]);
			if (bStorePrevious)
			{
				obj.m_Previous = previous;
			}
			GameObject gameObject2 = m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num];
			if (gameObject2 != null)
			{
				Object.Destroy(gameObject2);
				m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileObjects[num] = null;
			}
			if (flag)
			{
				TileProperty[] tileProperties;
				int num10;
				(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num10 = num] = tileProperties[num10] & ~(TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
				m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num], bAdd: false);
				m_BuildingLayers[(uint)m_CurrentLayer].m_WallTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
			}
			SetAreaAsChanged(obj.m_XPosition - 1, obj.m_YPosition - 1, 3, 3);
			break;
		}
		case BuildingInstructionManager.InstructionDeleteElement.DeleteType.Object:
		{
			TileIDData tileIDData = ValidatePrevious(m_BuildingLayers[(uint)m_CurrentLayer].m_ObjectTileIDs[num]);
			if (bStorePrevious)
			{
				obj.m_Previous = tileIDData;
			}
			BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock((int)(tileIDData & TileIDData.IDMask)) as BuildingBlock_Object;
			if (buildingBlock_Object == null)
			{
				break;
			}
			int num2 = obj.m_XPosition - buildingBlock_Object.m_Footprint.m_iFirstX;
			int num3 = obj.m_YPosition - buildingBlock_Object.m_Footprint.m_iFirstY;
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
							Object.Destroy(gameObject);
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

	[ContextMenu("Update")]
	public override void UpdateTiles()
	{
		if (LevelDetailsManager.c_CurrentLevelDataVersionNumber == LevelDetailsManager.LevelEditorDataVersion.V1_InitialRelease)
		{
			base.UpdateTiles();
			return;
		}
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
		LevelLayers currentLayer = m_CurrentLayer;
		for (int i = 1; i < 6; i++)
		{
			m_CurrentLayer = (LevelLayers)i;
			if (!m_BuildingLayers[i].m_Changed)
			{
				continue;
			}
			int[] map = m_ZoneManager.GetZoneMap(m_CurrentLayer).m_Map;
			bool flag8 = !m_VentLayers[i];
			m_BuildingLayers[i].m_Changed = false;
			bool flag9 = false;
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
					flag9 = true;
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
					flag9 = true;
				}
			}
			int num5 = 0;
			if (flag9)
			{
				for (int j = 0; j < 120; j++)
				{
					for (int k = 0; k < 120; k++)
					{
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.ChangedMask) != 0)
						{
							bool flag10 = (m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.EnvironmentMask) != 0;
							if (flag10 && num2 != -1)
							{
								num4 = num2;
								flag7 = flag5;
							}
							else
							{
								if (flag10 || num3 == -1)
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
										bool flag11 = false;
										int num8 = m_SurroundOffsets[num6].Length - 1;
										while (num8 >= 0 && !flag11)
										{
											TileProperty tileProperty2 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num8]];
											if ((tileProperty2 & TileProperty.WallAndObjects) != 0)
											{
												flag11 = true;
											}
											else if ((tileProperty2 & TileProperty.TileMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_TileTileIDs[num5 + m_SurroundOffsets[num6][num8]] & TileIDData.IDMask))
											{
												flag11 = true;
											}
											num8--;
										}
										if (!flag11)
										{
											TileProperty[] tileProperties;
											int num9;
											(tileProperties = m_BuildingLayers[i].m_TileProperties)[num9 = num5] = tileProperties[num9] & TileProperty.InverseTileMask;
										}
									}
								}
								else if ((tileProperty & TileProperty.BlockBitMask) == 0)
								{
									bool flag12 = false;
									int num10 = m_SurroundOffsets[num6].Length - 1;
									while (num10 >= 0 && !flag12)
									{
										TileProperty tileProperty3 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num10]];
										if ((tileProperty3 & TileProperty.WallAndObjects) != 0)
										{
											flag12 = true;
										}
										else if ((tileProperty3 & TileProperty.TileMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_TileTileIDs[num5 + m_SurroundOffsets[num6][num10]] & TileIDData.IDMask))
										{
											flag12 = true;
										}
										num10--;
									}
									if (flag12)
									{
										BuildingBlock_Tile obj = BuildingBlockManager.GetBlock(num4) as BuildingBlock_Tile;
										PlaceTile(k, j, obj, Random.Range(0, 10000), bMarkAsChanged: false);
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
									bool flag13 = false;
									int num12 = m_SurroundOffsets[num6].Length - 1;
									while (num12 >= 0 && !flag13)
									{
										TileProperty tileProperty4 = m_BuildingLayers[i].m_TileProperties[num5 + m_SurroundOffsets[num6][num12]];
										if ((tileProperty4 & (TileProperty.ObjDecMask | TileProperty.TileMask)) != 0)
										{
											flag13 = true;
										}
										else if ((tileProperty4 & TileProperty.WallMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_WallTileIDs[num5 + m_SurroundOffsets[num6][num12]] & TileIDData.IDMask))
										{
											flag13 = true;
										}
										num12--;
									}
									if (!flag13)
									{
										TileProperty[] tileProperties;
										int num13;
										(tileProperties = m_BuildingLayers[i].m_TileProperties)[num13 = num5] = tileProperties[num13] & ~(TileProperty.Blocked_All_Mask | TileProperty.WallMask | TileProperty.BlockingMask);
									}
								}
							}
							else if ((tileProperty & TileProperty.BlockBitMask) == 0)
							{
								bool flag14 = false;
								int num14 = m_SurroundOffsets[num6].Length - 1;
								while (num14 >= 0 && !flag14)
								{
									int num15 = num5 + m_SurroundOffsets[num6][num14];
									if (num15 >= 0 && num15 < num)
									{
										TileProperty tileProperty5 = m_BuildingLayers[i].m_TileProperties[num15];
										TileProperty tileProperty6 = TileProperty.ObjDecMask | TileProperty.TileMask;
										if ((tileProperty5 & tileProperty6) != 0)
										{
											flag14 = true;
										}
										else if ((tileProperty5 & TileProperty.WallMask) != 0 && num4 != (int)(m_BuildingLayers[i].m_WallTileIDs[num15] & TileIDData.IDMask))
										{
											flag14 = true;
										}
									}
									num14--;
								}
								if (flag14)
								{
									BuildingBlock_Wall obj2 = BuildingBlockManager.GetBlock(num4) as BuildingBlock_Wall;
									PlaceWall(k, j, obj2, Random.Range(0, 10000), bMarkAsChanged: false);
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
					if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.ChangedMask) != 0)
					{
						if (flag8 && map[num5] != -1)
						{
							m_ZoneManager.MarkAsChanged(map[num5]);
						}
						TileProperty[] tileProperties;
						int num16;
						(tileProperties = m_BuildingLayers[i].m_TileProperties)[num16 = num5] = tileProperties[num16] & TileProperty.InverseChangedMask;
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.TileMask) == 0 && m_BuildingLayers[i].m_TileTileObjects[num5] != null)
						{
							Object.Destroy(m_BuildingLayers[i].m_TileTileObjects[num5]);
							m_BuildingLayers[i].m_TileTileObjects[num5] = null;
							m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_TileTileIDs[num5], bAdd: false);
							TileIDData[] tileTileIDs;
							int num17;
							(tileTileIDs = m_BuildingLayers[i].m_TileTileIDs)[num17 = num5] = tileTileIDs[num17] | TileIDData.IDMask;
						}
						if ((m_BuildingLayers[i].m_TileProperties[num5] & TileProperty.WallMask) == 0 && m_BuildingLayers[i].m_WallTileObjects[num5] != null)
						{
							Object.Destroy(m_BuildingLayers[i].m_WallTileObjects[num5]);
							m_BuildingLayers[i].m_WallTileObjects[num5] = null;
							m_BuildingBlockManager.AdjustLimitationTotal(m_BuildingLayers[i].m_WallTileIDs[num5], bAdd: false);
							TileIDData[] tileTileIDs;
							int num18;
							(tileTileIDs = m_BuildingLayers[i].m_WallTileIDs)[num18 = num5] = tileTileIDs[num18] | TileIDData.IDMask;
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
											GameObject visualRep = buildingBlock_Tile.GetVisualRep(applicableTMSVariant);
											if (visualRep == null)
											{
												return;
											}
											GameObject gameObject = Object.Instantiate(visualRep, m_BuildingLayers[i].m_Tiles.transform);
											float x = (float)m + m_fPositionOffsetsX[(int)buildingBlock_Tile.BlockType];
											float y = (float)l + m_fPositionOffsetsY[(int)buildingBlock_Tile.BlockType];
											float z = 0f - (float)(120 - l) / 240f;
											gameObject.transform.localPosition = new Vector3(x, y, z);
											gameObject.SetActive(value: true);
											GameObject gameObject2 = m_BuildingLayers[i].m_TileTileObjects[num5];
											if (gameObject2 != null)
											{
												Object.Destroy(gameObject2);
											}
											m_BuildingLayers[i].m_TileTileObjects[num5] = gameObject;
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
											GameObject visualRep2 = buildingBlock_Wall.GetVisualRep(applicableTMSVariant2);
											if (visualRep2 == null)
											{
												return;
											}
											GameObject gameObject3 = Object.Instantiate(visualRep2, m_BuildingLayers[i].m_Walls.transform);
											float x2 = (float)m + m_fPositionOffsetsX[(int)buildingBlock_Wall.BlockType];
											float y2 = (float)l + m_fPositionOffsetsY[(int)buildingBlock_Wall.BlockType] + (gameObject3.transform.localScale.y - 1f) / 2f;
											float z2 = 0f - (float)(120 - l) / 240f;
											gameObject3.transform.localPosition = new Vector3(x2, y2, z2);
											gameObject3.SetActive(value: true);
											GameObject gameObject4 = m_BuildingLayers[i].m_WallTileObjects[num5];
											if (gameObject4 != null)
											{
												Object.Destroy(gameObject4);
											}
											m_BuildingLayers[i].m_WallTileObjects[num5] = gameObject3;
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
					num5++;
				}
			}
		}
		m_CurrentLayer = currentLayer;
	}
}
