using UnityEngine;

public class LevelEditorBrushElement : MonoBehaviour
{
	public enum EnviromentalLocation
	{
		InsideBlock,
		OutsideBlock,
		DoesntCare
	}

	public enum ElementType
	{
		Block,
		Border
	}

	public MeshRenderer m_MeshRender;

	private bool m_bValid;

	private bool m_bOutOfStock;

	public bool m_bCheckJustCurrent;

	public BaseLevelManager.TileProperty[] m_PropertyToCheck = new BaseLevelManager.TileProperty[6];

	public bool m_bRequiresClearance;

	private bool m_bExternalValidation = true;

	public bool m_bCheckForZone;

	private BaseLevelManager m_LevelManager;

	private LevelEditor_ZoneManager m_ZoneManager;

	private int m_iLastCheckX = -1000;

	private int m_iLastCheckY = -1000;

	private Color m_TileColour = new Color(0f, 0f, 0f, 0.5f);

	private BaseLevelManager.BrushError m_CurrentError;

	public EnviromentalLocation m_Location = EnviromentalLocation.DoesntCare;

	private void Start()
	{
		if (m_MeshRender == null)
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		m_iLastCheckX = -1000;
	}

	private void Update()
	{
		ValidateElement(m_bOutOfStock);
	}

	public void SetExternalValidation(bool bValid)
	{
		m_bExternalValidation = bValid;
	}

	public bool ValidateElement(bool bOutOfStock = false, bool bForceUpdate = false)
	{
		float num = Time.realtimeSinceStartup / 2f;
		num -= (float)(int)num;
		if (num > 0.5f)
		{
			num = 1f - num;
		}
		if (m_LevelManager == null)
		{
			if (BuildingInstructionManager.GetInstance() != null)
			{
				m_LevelManager = BuildingInstructionManager.GetInstance().m_LevelManager;
			}
			if (m_LevelManager == null)
			{
				return false;
			}
		}
		if (m_ZoneManager == null && (m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
			return false;
		}
		Vector3 position = base.transform.position;
		int num2 = (int)(position.x + 60f - 0.5f);
		int num3 = (int)(position.y + 60f - 0.5f);
		if (num2 != m_iLastCheckX || num3 != m_iLastCheckY || m_bOutOfStock != bOutOfStock || bForceUpdate)
		{
			m_CurrentError = BaseLevelManager.BrushError.eNone;
			m_bOutOfStock = bOutOfStock;
			m_iLastCheckX = num2;
			m_iLastCheckY = num3;
			if (!m_bOutOfStock)
			{
				if (m_iLastCheckX < 0 || m_iLastCheckY < 0 || m_iLastCheckX >= 120 || m_iLastCheckY >= 118)
				{
					m_bValid = false;
					m_CurrentError |= BaseLevelManager.BrushError.eOutOfBounds;
				}
				else if (LevelEditorHighLightManager.GetInstance() != null && !LevelEditorHighLightManager.GetInstance().IsOkToDrawOn(m_iLastCheckX, m_iLastCheckY) && !BuildingInstructionManager.GetInstance().m_IgnoreChecks)
				{
					m_bValid = false;
					m_CurrentError |= LevelEditorHighLightManager.GetInstance().GetErrorForTile(m_iLastCheckX, m_iLastCheckY);
				}
				else
				{
					m_bValid = true;
					int num4 = num2 + 120 * num3;
					int[] array = new int[4] { -1, -1, -1, -1 };
					if (m_bRequiresClearance)
					{
						if (num2 > 0)
						{
							array[0] = num2 + 120 * num3 - 1;
						}
						if (num2 < 119)
						{
							array[1] = num2 + 120 * num3 + 1;
						}
						if (num3 > 0)
						{
							array[2] = num2 + 120 * (num3 - 1);
						}
						if (num3 < 119)
						{
							array[3] = num2 + 120 * (num3 + 1);
						}
					}
					int num5 = (int)m_LevelManager.m_CurrentLayer;
					int num6 = num5 + 1;
					int num7 = 0;
					if (!m_bCheckJustCurrent)
					{
						num5 = 1;
						num6 = 6;
						num7 = 1;
					}
					if (m_LevelManager.m_CurrentLayer != BaseLevelManager.LevelLayers.GroundFloor)
					{
						int num8 = 1;
						if (!m_LevelManager.m_VentLayers[(uint)m_LevelManager.m_CurrentLayer])
						{
							num8++;
						}
						BaseLevelManager.TileProperty tileProperty = m_LevelManager.m_BuildingLayers[(int)m_LevelManager.m_CurrentLayer - num8].m_TileProperties[num4];
						if ((tileProperty & BaseLevelManager.TileProperty.TileExistsMask) != BaseLevelManager.TileProperty.TileExistsMask)
						{
							m_bValid = false;
							m_CurrentError |= BaseLevelManager.BrushError.eInsideRequired;
						}
					}
					for (int i = num5; i < num6; i++)
					{
						if (!m_bValid)
						{
							break;
						}
						BaseLevelManager.TileProperty tileProperty2 = m_LevelManager.m_BuildingLayers[i].m_TileProperties[num4];
						if ((tileProperty2 & BaseLevelManager.TileProperty.WallMask) != 0)
						{
							int buildingBrickID = (int)(m_LevelManager.m_BuildingLayers[i].m_WallTileIDs[num4] & BaseLevelManager.TileIDData.IDMask);
							BaseBuildingBlock block = BuildingBlockManager.GetBlock(buildingBrickID);
							if (block != null && block.m_AutomaticBlock)
							{
								tileProperty2 &= BaseLevelManager.TileProperty.InverseWallMask;
							}
						}
						if (m_bRequiresClearance)
						{
							int num9 = array.Length - 1;
							while (num9 >= 0 && m_bValid)
							{
								if (array[num9] != -1)
								{
									if ((m_LevelManager.m_BuildingLayers[i].m_TileProperties[array[num9]] & BaseLevelManager.TileProperty.WallMask) != 0)
									{
										int buildingBrickID2 = (int)(m_LevelManager.m_BuildingLayers[i].m_WallTileIDs[array[num9]] & BaseLevelManager.TileIDData.IDMask);
										BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(buildingBrickID2);
										if (block2 != null && block2.m_RequiresClearence)
										{
											m_bValid = false;
											m_CurrentError |= BaseLevelManager.BrushError.eNoClearance;
											break;
										}
									}
									if ((m_LevelManager.m_BuildingLayers[i].m_TileProperties[array[num9]] & BaseLevelManager.TileProperty.TileMask) != 0)
									{
										int buildingBrickID3 = (int)(m_LevelManager.m_BuildingLayers[i].m_TileTileIDs[array[num9]] & BaseLevelManager.TileIDData.IDMask);
										BaseBuildingBlock block3 = BuildingBlockManager.GetBlock(buildingBrickID3);
										if (block3 != null && block3.m_RequiresClearence)
										{
											m_bValid = false;
											m_CurrentError |= BaseLevelManager.BrushError.eNoClearance;
											break;
										}
									}
									if ((m_LevelManager.m_BuildingLayers[i].m_TileProperties[array[num9]] & BaseLevelManager.TileProperty.ObjDecMask) != 0)
									{
										int buildingBrickID4 = (int)(m_LevelManager.m_BuildingLayers[i].m_ObjectTileIDs[array[num9]] & BaseLevelManager.TileIDData.IDMask);
										BaseBuildingBlock block4 = BuildingBlockManager.GetBlock(buildingBrickID4);
										if (block4 != null && block4.m_RequiresClearence)
										{
											m_bValid = false;
											m_CurrentError |= BaseLevelManager.BrushError.eNoClearance;
											break;
										}
									}
								}
								num9--;
							}
						}
						bool flag = BuildingInstructionManager.GetInstance().m_IgnoreChecks;
						if (flag && (m_PropertyToCheck[num7] & BaseLevelManager.TileProperty.ObjDecMask) != 0 && (tileProperty2 & BaseLevelManager.TileProperty.ObjDecMask) != 0)
						{
							flag = false;
						}
						BaseLevelManager.TileProperty tileProperty3 = m_PropertyToCheck[num7++];
						bool flag2 = (tileProperty3 & BaseLevelManager.TileProperty.WallInRoomMask) != 0;
						bool flag3 = (tileProperty3 & BaseLevelManager.TileProperty.RoomMask) != 0;
						if (flag2)
						{
							tileProperty3 &= BaseLevelManager.TileProperty.InverseWallInRoomMask;
						}
						BaseLevelManager.TileProperty tileProperty4 = tileProperty3 & BaseLevelManager.TileProperty.InverseBlockingMask & BaseLevelManager.TileProperty.InverseNoBlockingMask;
						if (tileProperty4 == BaseLevelManager.TileProperty.EMPTY || flag)
						{
							continue;
						}
						if (m_bCheckForZone && m_ZoneManager.GetZoneMap((BaseLevelManager.LevelLayers)i).m_Map[num4] != -1)
						{
							m_bValid = false;
							m_CurrentError |= BaseLevelManager.BrushError.eCantOverwriteZone;
							break;
						}
						if (flag2)
						{
							if ((tileProperty2 & BaseLevelManager.TileProperty.WallMask) != 0)
							{
								int buildingBrickID5 = (int)(m_LevelManager.m_BuildingLayers[i].m_WallTileIDs[num4] & BaseLevelManager.TileIDData.IDMask);
								BuildingBlock_Wall buildingBlock_Wall = BuildingBlockManager.GetBlock(buildingBrickID5) as BuildingBlock_Wall;
								if (buildingBlock_Wall != null && buildingBlock_Wall.m_FloorTileID != -1)
								{
									tileProperty4 = BaseLevelManager.TileProperty.EMPTY;
								}
							}
						}
						else if (m_Location != EnviromentalLocation.DoesntCare && (tileProperty4 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask && (tileProperty2 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask && !flag3)
						{
							int buildingBrickID6 = (int)(m_LevelManager.m_BuildingLayers[i].m_WallTileIDs[num4] & BaseLevelManager.TileIDData.IDMask);
							BuildingBlock_Wall buildingBlock_Wall2 = BuildingBlockManager.GetBlock(buildingBrickID6) as BuildingBlock_Wall;
							if (buildingBlock_Wall2 != null && buildingBlock_Wall2.m_FloorTileID == -1)
							{
								if (m_Location == EnviromentalLocation.InsideBlock)
								{
									if ((tileProperty2 & BaseLevelManager.TileProperty.EnvironmentMask) == BaseLevelManager.TileProperty.EnvironmentMask)
									{
										tileProperty4 &= BaseLevelManager.TileProperty.InverseWallMask;
									}
								}
								else if ((tileProperty2 & BaseLevelManager.TileProperty.EnvironmentMask) != BaseLevelManager.TileProperty.EnvironmentMask)
								{
									tileProperty4 &= BaseLevelManager.TileProperty.InverseWallMask;
								}
							}
						}
						if ((tileProperty2 & tileProperty4) != 0)
						{
							m_bValid = false;
							m_CurrentError |= BaseLevelManager.BrushError.eBlocked;
							break;
						}
						if (tileProperty4 != 0 && m_LevelManager.m_BuildingLayers[i].m_RoomPropertiesMasks[num4] != 0)
						{
							m_bValid = false;
							m_CurrentError |= BaseLevelManager.BrushError.eRoomBlocked;
							break;
						}
						if ((tileProperty3 & (BaseLevelManager.TileProperty.NoBlockingMask | BaseLevelManager.TileProperty.BlockingMask)) != 0 && (tileProperty2 & BaseLevelManager.TileProperty.NoBlockingMask) == BaseLevelManager.TileProperty.NoBlockingMask)
						{
							m_bValid = false;
							m_CurrentError |= BaseLevelManager.BrushError.eBlocked;
							break;
						}
						if ((tileProperty3 & BaseLevelManager.TileProperty.NoBlockingMask) != BaseLevelManager.TileProperty.NoBlockingMask)
						{
							continue;
						}
						if (i > 1)
						{
							BaseLevelManager.TileProperty tileProperty5 = m_LevelManager.m_BuildingLayers[i - 1].m_TileProperties[num4];
							if ((tileProperty5 & BaseLevelManager.TileProperty.ObjectMask) == BaseLevelManager.TileProperty.ObjectMask)
							{
								BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(m_LevelManager.m_BuildingLayers[i - 1].m_ObjectTileIDs[num4]) as BuildingBlock_Object;
								if (buildingBlock_Object != null && buildingBlock_Object.m_Solid)
								{
									m_bValid = false;
									m_CurrentError |= BaseLevelManager.BrushError.eInsideRequired;
									break;
								}
							}
							if ((tileProperty5 & BaseLevelManager.TileProperty.TileBlockingMask) == BaseLevelManager.TileProperty.TileBlockingMask)
							{
								m_bValid = false;
								m_CurrentError |= BaseLevelManager.BrushError.eBlockedBelow;
								break;
							}
							if ((tileProperty5 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
							{
								BuildingBlock_Wall buildingBlock_Wall3 = BuildingBlockManager.GetBlock(m_LevelManager.m_BuildingLayers[i - 1].m_WallTileIDs[num4]) as BuildingBlock_Wall;
								if (buildingBlock_Wall3 != null && !buildingBlock_Wall3.m_AutomaticBlock)
								{
									m_bValid = false;
									m_CurrentError |= BaseLevelManager.BrushError.eBlockedBelow;
								}
								break;
							}
							if ((tileProperty5 & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
							{
								BaseBuildingBlock block5 = BuildingBlockManager.GetBlock(m_LevelManager.m_BuildingLayers[i - 1].m_TileTileIDs[num4]);
								if (block5 != null && block5.m_NoBlockingBelow)
								{
									m_bValid = false;
									m_CurrentError |= BaseLevelManager.BrushError.eBlockedBelow;
								}
								break;
							}
						}
						if (i >= 5)
						{
							continue;
						}
						BaseLevelManager.TileProperty tileProperty6 = m_LevelManager.m_BuildingLayers[i + 1].m_TileProperties[num4];
						if ((tileProperty6 & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
						{
							BaseBuildingBlock block6 = BuildingBlockManager.GetBlock(m_LevelManager.m_BuildingLayers[i + 1].m_TileTileIDs[num4]);
							if (block6 != null && block6.m_NoBlockingBelow)
							{
								m_bValid = false;
								m_CurrentError |= BaseLevelManager.BrushError.eBlockedAbove;
							}
							break;
						}
					}
				}
			}
			else
			{
				m_bValid = false;
			}
			if (m_MeshRender != null)
			{
				if (!Application.isPlaying)
				{
					m_MeshRender.enabled = !m_bValid;
				}
				else
				{
					if (bOutOfStock)
					{
						float num10 = 0.7f;
						m_TileColour.r = num10;
						m_TileColour.g = num10;
						m_TileColour.b = num10;
					}
					else
					{
						m_TileColour.r = 1f;
						m_TileColour.g = 0f;
						m_TileColour.b = 0f;
					}
					m_MeshRender.enabled = true;
				}
			}
		}
		if (m_MeshRender != null && m_MeshRender.enabled)
		{
			if (m_bValid && m_bExternalValidation)
			{
				m_TileColour.a = 0f;
			}
			else if (!m_bValid || bOutOfStock)
			{
				m_TileColour.a = 0.35f + num / 2f;
			}
			else
			{
				m_TileColour.a = 0.25f;
			}
			m_MeshRender.material.color = m_TileColour;
		}
		return m_bValid;
	}

	public void SetAttributes(bool bRoomWallBlock, bool bRequiresClearance)
	{
		m_bRequiresClearance = bRequiresClearance;
	}

	public bool AreWeValid()
	{
		return m_bValid & !m_bOutOfStock;
	}

	public BaseLevelManager.BrushError GetCurrentError()
	{
		return m_CurrentError;
	}

	public void SetPropertiesToCheckInLayers(BaseLevelManager.TileProperty[] props, EnviromentalLocation location)
	{
		m_PropertyToCheck = props;
		m_bCheckJustCurrent = false;
		m_Location = location;
	}

	public void SetPropertiesToCheck(BaseLevelManager.TileProperty props, EnviromentalLocation location, bool bCheckForZone)
	{
		m_PropertyToCheck = new BaseLevelManager.TileProperty[1];
		m_PropertyToCheck[0] = props;
		m_bCheckJustCurrent = true;
		m_Location = location;
		m_bCheckForZone = bCheckForZone;
	}
}
