using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LevelEditor_ZoneManager : MonoBehaviour
{
	public class Zone
	{
		public class ObjectsInZone
		{
			public int m_X;

			public int m_Y;

			public int m_BlockID = -1;

			public GameObject m_Object;

			public int m_ComplexID;

			public List<int> m_InteractPoints = new List<int>();

			public int m_GoodInteractPoint = -1;

			public bool m_BeingBlocked;

			public bool m_OnlyPartiallyIn;
		}

		public class StillRequired
		{
			public int m_BlockGroupIndex = -1;

			public int m_Minimum;

			public int m_Maximum;

			public int m_CurrentTotal;

			public Errors m_Error;
		}

		public class Errors
		{
			public string m_StrError = string.Empty;

			public int m_BlockSetIndex = -1;
		}

		public bool m_bActive;

		public bool m_bValid;

		public bool m_bReachable = true;

		public int m_TotalBlocked;

		public ZoneDetailsManager.ZoneTypes m_ZoneType;

		public ZoneDetailsManager.ZoneDetails m_ZoneDetails;

		public int m_Value;

		public int m_ID = -1;

		public Vector2 m_IconPosition = Vector2.zero;

		public int m_Bottom;

		public int m_Left;

		public int m_Width;

		public int m_Height;

		public byte[] m_ZonePrint = new byte[0];

		public BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.TOTAL;

		public int m_TotalTiles;

		public int m_TotalIgnoredTiles;

		public string m_strErrors = string.Empty;

		public bool m_RequiresUpdate;

		public bool m_RequiresZonePrintUpdate;

		public List<ObjectsInZone> m_BlocksInZone = new List<ObjectsInZone>();

		public List<StillRequired> m_Required = new List<StillRequired>();

		public List<StillRequired> m_RequirementsMet = new List<StillRequired>();

		public int[] m_BlockGroupsUsed = new int[0];

		public int m_TotalInsideTiles;

		public int m_TotalOutsideTiles;

		public int m_TotalNoTiles;

		public int m_AllocatedRoomID = -1;

		public LevelEditor_Marquee m_ZoneGraphic;

		public LevelEditor_ZoneIconControl m_ZoneIcon;

		public bool m_bCreateUI;

		public int m_ZoneUpdateCount;

		public bool IsFullyValid()
		{
			return m_bValid & m_bReachable;
		}

		public bool IsGameObjectInZone(GameObject obj)
		{
			for (int num = m_BlocksInZone.Count - 1; num >= 0; num--)
			{
				if (m_BlocksInZone[num] != null && m_BlocksInZone[num].m_Object != null && object.ReferenceEquals(m_BlocksInZone[num].m_Object, obj))
				{
					return true;
				}
			}
			return false;
		}

		public bool[] GetMap()
		{
			bool[] array = new bool[m_Width * m_Height];
			int num = 0;
			byte b = 0;
			int num2 = 0;
			byte[] zonePrint = m_ZonePrint;
			for (int i = 0; i < m_Height; i++)
			{
				for (int j = 0; j < m_Width; j++)
				{
					array[num++] = (zonePrint[num2] & (1 << (int)b)) != 0;
					if (++b == 8)
					{
						b = 0;
						num2++;
					}
				}
			}
			return array;
		}

		public StillRequired GetRequireDataForBlockGroup(int iBlockGroup)
		{
			int count = m_RequirementsMet.Count;
			for (int i = 0; i < count; i++)
			{
				if (m_RequirementsMet[i].m_BlockGroupIndex == iBlockGroup)
				{
					return m_RequirementsMet[i];
				}
			}
			count = m_Required.Count;
			for (int j = 0; j < count; j++)
			{
				if (m_Required[j].m_BlockGroupIndex == iBlockGroup)
				{
					return m_Required[j];
				}
			}
			return null;
		}
	}

	public class ZoneMap
	{
		public int[] m_Map = new int[14400];
	}

	public enum ZonesInArea
	{
		JustOurs,
		Nothing,
		Others,
		OursAndOthers
	}

	private static LevelEditor_ZoneManager m_Instance;

	public const int INVALID_ZONE_ID = -1;

	public const int INVALID_REQUIREMENTS_ID = -1;

	public const int INVALID_ROOM_ID = -1;

	public const int INVALID_INTERACTPOINT = -1;

	[Tooltip("Is this component in the editor scene")]
	public bool m_InEditorScene = true;

	private Zone[] m_Zones = new Zone[0];

	private int m_TotalZonesInUse;

	private BuildingBlockGroupManager m_BlockGroupManager;

	private Zone m_LastCreatedZone;

	public UnityEngine.Object m_ZoneIconPrefab;

	public UnityEngine.Object m_ZoneMarqueePrefab;

	public UnityEngine.Object m_FlashingErrorMarqueePrefab;

	public UnityEngine.Object m_SolidErrorMarqueePrefab;

	private BaseLevelManager m_LevelManager;

	private BaseLevelManager.LevelLayers m_LastLevelLayer = BaseLevelManager.LevelLayers.TOTAL;

	private Stopwatch m_StopWatch = new Stopwatch();

	private ZoneMap[] m_ZoneMap = new ZoneMap[6];

	private float m_Scale;

	private int m_HighlightZone = -1;

	public static LevelEditor_ZoneManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		ResetZones();
		for (int i = 0; i < 6; i++)
		{
			m_ZoneMap[i] = new ZoneMap();
			for (int j = 0; j < 14400; j++)
			{
				m_ZoneMap[i].m_Map[j] = -1;
			}
		}
	}

	private void Start()
	{
		m_LevelManager = BaseLevelManager.GetInstance();
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Update()
	{
		if (!m_InEditorScene)
		{
			return;
		}
		int num = m_Zones.Length;
		if (num > 0)
		{
			m_StopWatch.Reset();
			m_StopWatch.Start();
			int num2 = 0;
			bool flag = false;
			while (m_StopWatch.ElapsedMilliseconds < 300 && num2 < num)
			{
				if (m_Zones[num2].m_RequiresZonePrintUpdate)
				{
					UpdateZonePrint(num2);
					flag = true;
				}
				else if (m_Zones[num2].m_RequiresUpdate)
				{
					ValidateZone(num2, ZoneRequirement.WhoForEnum.User);
					flag = true;
				}
				else
				{
					num2++;
				}
			}
			m_StopWatch.Stop();
			if (flag && LevelEditor_Controller.GetInstance() != null)
			{
				LevelEditor_Controller.GetInstance().ResetTimer();
			}
		}
		if (m_LastLevelLayer == m_LevelManager.m_CurrentLayer)
		{
			return;
		}
		m_LastLevelLayer = m_LevelManager.m_CurrentLayer;
		for (int i = 0; i < num; i++)
		{
			if (m_Zones[i] != null && m_Zones[i].m_bActive)
			{
				if (m_Zones[i].m_ZoneIcon != null)
				{
					m_Zones[i].m_ZoneIcon.ShowIcon(m_Zones[i].m_Layer == m_LastLevelLayer);
				}
				if (m_Zones[i].m_ZoneGraphic != null)
				{
					m_Zones[i].m_ZoneGraphic.ShowMarquee(m_Zones[i].m_Layer == m_LastLevelLayer);
				}
			}
		}
	}

	public void ResetLayerChecks()
	{
		m_LastLevelLayer = BaseLevelManager.LevelLayers.TOTAL;
	}

	public int GetTotalZones()
	{
		return m_Zones.Length;
	}

	public void ValidateAllZones()
	{
		int num = m_Zones.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Zones[i] != null && m_Zones[i].m_bActive)
			{
				UpdateZonePrint(i);
				ValidateZone(i, ZoneRequirement.WhoForEnum.Game);
			}
		}
	}

	private void ValidateZone(int iID, ZoneRequirement.WhoForEnum eWhoFor)
	{
		if (iID >= m_Zones.Length || m_Zones[iID] == null || !m_Zones[iID].m_bActive || (m_BlockGroupManager == null && (m_BlockGroupManager = BuildingBlockGroupManager.GetInstance()) == null))
		{
			return;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		Zone zone = m_Zones[iID];
		zone.m_RequiresUpdate = false;
		zone.m_ZoneUpdateCount++;
		zone.m_BlocksInZone.Clear();
		zone.m_Required.Clear();
		zone.m_RequirementsMet.Clear();
		zone.m_TotalTiles = 0;
		zone.m_TotalIgnoredTiles = 0;
		zone.m_TotalInsideTiles = 0;
		zone.m_TotalOutsideTiles = 0;
		zone.m_TotalNoTiles = 0;
		int totalGroups = m_BlockGroupManager.GetTotalGroups();
		zone.m_BlockGroupsUsed = new int[totalGroups];
		for (int i = 0; i < totalGroups; i++)
		{
			zone.m_BlockGroupsUsed[i] = 0;
		}
		int num = 0;
		BaseLevelManager.LayerDataCollection data = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer];
		ZoneMap zoneMap = m_ZoneMap[(uint)zone.m_Layer];
		List<int> list = new List<int>();
		int num2 = zone.m_ZoneDetails.m_Requirements.Length;
		for (int j = 0; j < num2; j++)
		{
			ZoneRequirement zoneRequirement = zone.m_ZoneDetails.m_Requirements[j];
			if (zoneRequirement != null && zoneRequirement.m_bValid && (zoneRequirement.m_WhoFor == ZoneRequirement.WhoForEnum.Both || zoneRequirement.m_WhoFor == eWhoFor))
			{
				list.Add(zoneRequirement.GetBlockSetIndex());
			}
		}
		int count = list.Count;
		List<Zone.ObjectsInZone> list2 = new List<Zone.ObjectsInZone>();
		num = zone.m_Bottom * 120 + zone.m_Left;
		int width = zone.m_Width;
		int height = zone.m_Height;
		int num3 = 120 - width;
		for (int k = 0; k < height; k++)
		{
			for (int l = 0; l < width; l++)
			{
				if (zoneMap.m_Map[num] == iID)
				{
					int roomNumberFromProperty = BaseLevelManager.GetRoomNumberFromProperty(ref data, num);
					if (roomNumberFromProperty != 0)
					{
						int blockIDFromComplexAllocation = m_LevelManager.GetBlockIDFromComplexAllocation(roomNumberFromProperty);
						if (blockIDFromComplexAllocation != -1)
						{
							BaseBuildingBlock block = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation);
							if (block != null && block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
							{
								BuildingBlock_Complex buildingBlock_Complex = block as BuildingBlock_Complex;
								if (buildingBlock_Complex.m_ZoneObject)
								{
									Zone.ObjectsInZone objectsInZone = new Zone.ObjectsInZone();
									objectsInZone.m_BlockID = blockIDFromComplexAllocation;
									objectsInZone.m_ComplexID = roomNumberFromProperty;
									objectsInZone.m_X = l;
									objectsInZone.m_Y = k;
									BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[num];
									if ((tileProperty & BaseLevelManager.TileProperty.ObjDecMask) != 0)
									{
										blockIDFromComplexAllocation = (int)(data.m_ObjectTileIDs[num] & BaseLevelManager.TileIDData.IDMask);
										if (blockIDFromComplexAllocation != 16383)
										{
											GameObject gameObject = data.m_ObjectTileObjects[num];
											if (gameObject != null)
											{
												BuildingBlock_Object buildingBlock_Object = BuildingBlockManager.GetBlock(blockIDFromComplexAllocation) as BuildingBlock_Object;
												if (buildingBlock_Object != null && buildingBlock_Object.m_ZoneObject)
												{
													int count2 = objectsInZone.m_InteractPoints.Count;
													if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractMarker) != 0)
													{
														if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirUp) != 0 && k < 119)
														{
															objectsInZone.m_InteractPoints.Add(num + 120);
														}
														if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirDown) != 0 && k > 0)
														{
															objectsInZone.m_InteractPoints.Add(num - 120);
														}
														if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirLeft) != 0 && l > 0)
														{
															objectsInZone.m_InteractPoints.Add(num - 1);
														}
														if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirRight) != 0 && l < 119)
														{
															objectsInZone.m_InteractPoints.Add(num + 1);
														}
													}
													else if ((buildingBlock_Object.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.KeepClearMarker) != 0)
													{
														objectsInZone.m_InteractPoints.Add(num);
													}
													if (count2 != objectsInZone.m_InteractPoints.Count)
													{
														objectsInZone.m_InteractPoints.Add(-1);
													}
												}
											}
										}
									}
									list2.Add(objectsInZone);
								}
							}
						}
					}
					zone.m_TotalTiles++;
					BaseLevelManager.TileProperty tileProperty2 = data.m_TileProperties[num];
					bool flag = false;
					if ((tileProperty2 & BaseLevelManager.TileProperty.ItsADoorMask) == BaseLevelManager.TileProperty.ItsADoorMask)
					{
						flag = true;
					}
					else if ((tileProperty2 & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
					{
						BuildingBlock_Wall buildingBlock_Wall = instance.GetBuildingBlock(data.m_WallTileIDs[num]) as BuildingBlock_Wall;
						if (buildingBlock_Wall.m_FloorTileID != -1)
						{
							flag = true;
						}
					}
					if (flag)
					{
						zone.m_TotalIgnoredTiles++;
					}
					else if ((tileProperty2 & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
					{
						if ((tileProperty2 & BaseLevelManager.TileProperty.EnvironmentMask) == BaseLevelManager.TileProperty.EnvironmentMask)
						{
							zone.m_TotalInsideTiles++;
						}
						else
						{
							zone.m_TotalOutsideTiles++;
						}
					}
					else
					{
						zone.m_TotalNoTiles++;
					}
					if ((tileProperty2 & BaseLevelManager.TileProperty.ObjDecMask) != 0)
					{
						int num4 = (int)(data.m_ObjectTileIDs[num] & BaseLevelManager.TileIDData.IDMask);
						if (num4 != 16383)
						{
							GameObject gameObject2 = data.m_ObjectTileObjects[num];
							if (gameObject2 != null)
							{
								BuildingBlock_Object buildingBlock_Object2 = BuildingBlockManager.GetBlock(num4) as BuildingBlock_Object;
								if (buildingBlock_Object2 != null && buildingBlock_Object2.m_ZoneObject)
								{
									Zone.ObjectsInZone objectsInZone2 = new Zone.ObjectsInZone();
									objectsInZone2.m_BlockID = num4;
									objectsInZone2.m_Object = gameObject2;
									objectsInZone2.m_X = l;
									objectsInZone2.m_Y = k;
									if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractMarker) != 0)
									{
										if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirUp) != 0 && k < 119)
										{
											objectsInZone2.m_InteractPoints.Add(num + 120);
										}
										if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirDown) != 0 && k > 0)
										{
											objectsInZone2.m_InteractPoints.Add(num - 120);
										}
										if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirLeft) != 0 && l > 0)
										{
											objectsInZone2.m_InteractPoints.Add(num - 1);
										}
										if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.InteractDirRight) != 0 && l < 119)
										{
											objectsInZone2.m_InteractPoints.Add(num + 1);
										}
									}
									else if ((buildingBlock_Object2.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.KeepClearMarker) != 0)
									{
										objectsInZone2.m_InteractPoints.Add(num);
									}
									list2.Add(objectsInZone2);
								}
							}
						}
					}
				}
				num++;
			}
			num += num3;
		}
		if (zone.m_TotalTiles == 0 && zone.m_TotalIgnoredTiles == 0)
		{
			if (zone.m_bValid && zone.m_Value != 0)
			{
				BuildingBlockManager.GetInstance().AdjustLimitationTotal(zone.m_ZoneDetails.m_LimitationGroup, zone.m_Value * -1);
			}
			ReleaseZone(iID);
			return;
		}
		int count3 = list2.Count;
		for (int m = 0; m < count3; m++)
		{
			if (list2[m] == null)
			{
				continue;
			}
			int blockID = list2[m].m_BlockID;
			BaseBuildingBlock block2 = BuildingBlockManager.GetBlock(blockID);
			if (block2 == null || block2.m_Footprint == null)
			{
				list2[m] = null;
				continue;
			}
			int totalTilesUsed = block2.m_Footprint.GetTotalTilesUsed();
			if (totalTilesUsed == 1)
			{
				continue;
			}
			int num5 = 1;
			for (int n = m + 1; n < count3; n++)
			{
				if (list2[n] != null && list2[n].m_BlockID == blockID && ((list2[m].m_ComplexID == 0 && object.ReferenceEquals(list2[n].m_Object, list2[m].m_Object)) || (list2[m].m_ComplexID != 0 && list2[m].m_ComplexID == list2[n].m_ComplexID)))
				{
					num5++;
					if (list2[n].m_InteractPoints.Count != 0)
					{
						list2[m].m_InteractPoints.AddRange(list2[n].m_InteractPoints);
					}
					list2[n] = null;
				}
			}
			if (num5 != totalTilesUsed)
			{
				list2[m].m_OnlyPartiallyIn = true;
			}
		}
		int num6 = 0;
		for (int num7 = list2.Count - 1; num7 >= 0; num7--)
		{
			if (list2[num7] != null)
			{
				BaseBuildingBlock block3 = BuildingBlockManager.GetBlock(list2[num7].m_BlockID);
				if (block3 != null)
				{
					if (block3.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
					{
						BuildingBlock_Complex buildingBlock_Complex2 = block3 as BuildingBlock_Complex;
						if (buildingBlock_Complex2 != null)
						{
							bool flag2 = false;
							int num8 = buildingBlock_Complex2.m_InBlockGroups.Count - 1;
							while (num8 >= 0 && !flag2)
							{
								for (int num9 = 0; num9 < count; num9++)
								{
									if (flag2)
									{
										break;
									}
									if (list[num9] == buildingBlock_Complex2.m_InBlockGroups[num8])
									{
										flag2 = true;
										break;
									}
								}
								num8--;
							}
							if (flag2)
							{
								for (int num10 = buildingBlock_Complex2.m_InBlockGroups.Count - 1; num10 >= 0; num10--)
								{
									zone.m_BlockGroupsUsed[buildingBlock_Complex2.m_InBlockGroups[num10]]++;
								}
								zone.m_BlocksInZone.Add(list2[num7]);
								if (list2[num7].m_OnlyPartiallyIn)
								{
									num6++;
								}
							}
						}
					}
					else
					{
						BuildingBlock_Object buildingBlock_Object3 = block3 as BuildingBlock_Object;
						if (buildingBlock_Object3 != null)
						{
							bool flag3 = false;
							int num11 = buildingBlock_Object3.m_InBlockGroups.Count - 1;
							while (num11 >= 0 && !flag3)
							{
								for (int num12 = 0; num12 < count; num12++)
								{
									if (flag3)
									{
										break;
									}
									if (list[num12] == buildingBlock_Object3.m_InBlockGroups[num11])
									{
										flag3 = true;
										break;
									}
								}
								num11--;
							}
							if (flag3)
							{
								for (int num13 = buildingBlock_Object3.m_InBlockGroups.Count - 1; num13 >= 0; num13--)
								{
									zone.m_BlockGroupsUsed[buildingBlock_Object3.m_InBlockGroups[num13]]++;
								}
								zone.m_BlocksInZone.Add(list2[num7]);
								if (list2[num7].m_OnlyPartiallyIn)
								{
									num6++;
								}
							}
						}
					}
				}
			}
		}
		if (zone.m_ZoneDetails == null)
		{
			return;
		}
		for (int num14 = 0; num14 < num2; num14++)
		{
			ZoneRequirement zoneRequirement2 = zone.m_ZoneDetails.m_Requirements[num14];
			if (zoneRequirement2 == null || !zoneRequirement2.m_bValid || (zoneRequirement2.m_WhoFor != ZoneRequirement.WhoForEnum.Both && zoneRequirement2.m_WhoFor != eWhoFor))
			{
				continue;
			}
			int blockSetIndex = zoneRequirement2.GetBlockSetIndex();
			if (blockSetIndex == -1)
			{
				continue;
			}
			int num15 = zone.m_BlockGroupsUsed[blockSetIndex];
			int num16 = 0;
			num16 = zoneRequirement2.m_Minimum.GetValue(ref zone);
			int value = zoneRequirement2.m_Maximum.GetValue(ref zone);
			Zone.Errors errors = null;
			bool flag4 = true;
			if (num16 > num15)
			{
				errors = new Zone.Errors();
				errors.m_BlockSetIndex = blockSetIndex;
				if (!Localization.Get(zoneRequirement2.m_TooFewError, out errors.m_StrError))
				{
					errors.m_StrError = "[" + zoneRequirement2.m_TooFewError + "]";
				}
				flag4 = false;
			}
			else if (value != 0 && num15 > value)
			{
				errors = new Zone.Errors();
				errors.m_BlockSetIndex = blockSetIndex;
				if (!Localization.Get(zoneRequirement2.m_TooManyError, out errors.m_StrError))
				{
					errors.m_StrError = "[" + zoneRequirement2.m_TooManyError + "]";
				}
				flag4 = false;
			}
			if (errors != null && !string.IsNullOrEmpty(errors.m_StrError))
			{
				string translatedName = m_BlockGroupManager.GetGroupByIndex(blockSetIndex).GetTranslatedName();
				errors.m_StrError = errors.m_StrError.Replace("$BlockGroup", translatedName);
			}
			if (!flag4)
			{
				Zone.StillRequired stillRequired = new Zone.StillRequired();
				stillRequired.m_BlockGroupIndex = blockSetIndex;
				stillRequired.m_CurrentTotal = num15;
				stillRequired.m_Minimum = num16;
				stillRequired.m_Maximum = value;
				stillRequired.m_Error = errors;
				zone.m_Required.Add(stillRequired);
			}
			else
			{
				Zone.StillRequired stillRequired2 = new Zone.StillRequired();
				stillRequired2.m_BlockGroupIndex = blockSetIndex;
				stillRequired2.m_CurrentTotal = num15;
				stillRequired2.m_Minimum = num16;
				stillRequired2.m_Maximum = value;
				zone.m_RequirementsMet.Add(stillRequired2);
			}
		}
		if (zone.m_TotalNoTiles != 0)
		{
			Zone.StillRequired stillRequired3 = new Zone.StillRequired();
			Zone.Errors errors2 = new Zone.Errors();
			string overNothingText = zone.m_ZoneDetails.GetOverNothingText();
			errors2.m_StrError = overNothingText.Replace("$Zone", zone.m_ZoneDetails.GetZoneNameText());
			stillRequired3.m_Error = errors2;
			zone.m_Required.Add(stillRequired3);
		}
		zone.m_bValid = zone.m_Required.Count == 0;
		zone.m_bValid &= num6 == 0;
		BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(zone.m_ZoneDetails.m_LimitationGroup);
		if (limitationGroup == null)
		{
			return;
		}
		int num17 = 0;
		if (zone.m_bValid)
		{
			switch (zone.m_ZoneDetails.m_LimitationType)
			{
			case ZoneDetailsManager.LimitationType.FixedSize:
				num17 = zone.m_ZoneDetails.m_LimitationCount;
				break;
			case ZoneDetailsManager.LimitationType.FromBlockGroup:
			{
				int limitationBlockGroupIndex = zone.m_ZoneDetails.GetLimitationBlockGroupIndex();
				if (limitationBlockGroupIndex != -1)
				{
					num17 = zone.m_BlockGroupsUsed[limitationBlockGroupIndex];
				}
				break;
			}
			}
		}
		if (zone.m_Value != num17)
		{
			BuildingBlockManager.GetInstance().AdjustLimitationTotal(zone.m_ZoneDetails.m_LimitationGroup, num17 - zone.m_Value);
			zone.m_Value = num17;
		}
	}

	private void UpdateZonePrint(int iID)
	{
		if (iID >= m_Zones.Length || m_Zones[iID] == null || !m_Zones[iID].m_bActive)
		{
			return;
		}
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		Zone zone = m_Zones[iID];
		zone.m_RequiresZonePrintUpdate = false;
		zone.m_TotalTiles = 0;
		zone.m_TotalIgnoredTiles = 0;
		zone.m_TotalInsideTiles = 0;
		zone.m_TotalOutsideTiles = 0;
		zone.m_TotalNoTiles = 0;
		zone.m_Bottom = 200;
		zone.m_Left = 200;
		int num = -1;
		int num2 = -1;
		zone.m_Width = 0;
		zone.m_Height = 0;
		int num3 = 200;
		int num4 = -1;
		int num5 = 0;
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer];
		ZoneMap zoneMap = m_ZoneMap[(uint)zone.m_Layer];
		for (int i = 0; i < 120; i++)
		{
			for (int j = 0; j < 120; j++)
			{
				if (zoneMap.m_Map[num5] == iID)
				{
					zone.m_TotalTiles++;
					if (j > num)
					{
						num = j;
					}
					if (j < zone.m_Left)
					{
						zone.m_Left = j;
					}
					if (i > num2)
					{
						num2 = i;
					}
					if (i < zone.m_Bottom)
					{
						zone.m_Bottom = i;
					}
					bool flag = false;
					BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[num5];
					if ((tileProperty & BaseLevelManager.TileProperty.ItsADoorMask) == BaseLevelManager.TileProperty.ItsADoorMask)
					{
						flag = true;
					}
					else if ((tileProperty & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
					{
						BuildingBlock_Wall buildingBlock_Wall = instance.GetBuildingBlock(layerDataCollection.m_WallTileIDs[num5]) as BuildingBlock_Wall;
						if (buildingBlock_Wall.m_FloorTileID != -1)
						{
							flag = true;
						}
					}
					if (flag)
					{
						zone.m_TotalIgnoredTiles++;
					}
					else if ((tileProperty & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
					{
						if ((tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) == BaseLevelManager.TileProperty.EnvironmentMask)
						{
							zone.m_TotalInsideTiles++;
						}
						else
						{
							zone.m_TotalOutsideTiles++;
						}
					}
					else
					{
						zone.m_TotalNoTiles++;
					}
				}
				num5++;
			}
		}
		if (zone.m_TotalTiles == 0 && zone.m_TotalIgnoredTiles == 0)
		{
			if (zone.m_bValid && zone.m_Value != 0)
			{
				BuildingBlockManager.GetInstance().AdjustLimitationTotal(zone.m_ZoneDetails.m_LimitationGroup, zone.m_Value * -1);
			}
			ReleaseZone(iID);
			return;
		}
		zone.m_Width = num - zone.m_Left + 1;
		zone.m_Height = num2 - zone.m_Bottom + 1;
		int num6 = (zone.m_Width * zone.m_Height + 7) / 8;
		zone.m_ZonePrint = new byte[num6];
		byte b = 0;
		int num7 = 0;
		num5 = zone.m_Bottom * 120 + zone.m_Left;
		int num8 = 120 - zone.m_Width;
		int num9 = 1000;
		for (int k = 0; k < zone.m_Height; k++)
		{
			for (int l = 0; l < zone.m_Width; l++)
			{
				if (zoneMap.m_Map[num5++] == iID)
				{
					int num10 = l + (zone.m_Height - k);
					if (num9 >= num10)
					{
						num9 = num10;
						num3 = l;
						num4 = k;
					}
					zone.m_ZonePrint[num7] |= (byte)(1 << (int)b);
				}
				if (++b == 8)
				{
					b = 0;
					num7++;
				}
			}
			num5 += num8;
		}
		zone.m_IconPosition.x = (float)num3 + (float)zone.m_Left;
		zone.m_IconPosition.y = (float)num4 + (float)zone.m_Bottom;
		if (!zone.m_bCreateUI)
		{
			return;
		}
		if (m_ZoneMarqueePrefab != null)
		{
			if (zone.m_ZoneGraphic != null)
			{
				zone.m_ZoneGraphic.RegenerateFromZone(zone);
			}
			else
			{
				zone.m_ZoneGraphic = LevelEditor_Marquee.CreateMarqueeForZone(m_ZoneMarqueePrefab, string.Empty, string.Empty, zone);
				if (m_LastLevelLayer != zone.m_Layer)
				{
					zone.m_ZoneGraphic.ShowMarquee(bShow: false);
				}
			}
		}
		if (zone.m_ZoneIcon == null)
		{
			if (m_ZoneIconPrefab != null)
			{
				zone.m_ZoneIcon = LevelEditor_ZoneIconControl.CreateZoneIcon(m_ZoneIconPrefab, zone, new Vector3(zone.m_IconPosition.x, zone.m_IconPosition.y - 119f, -30f), LevelEditor_ZoneIconControl.Mode.NotOverZone);
				if (m_LastLevelLayer != zone.m_Layer)
				{
					zone.m_ZoneIcon.ShowIcon(bShow: false);
				}
			}
		}
		else
		{
			zone.m_ZoneIcon.SetStandardPosition(new Vector3(zone.m_IconPosition.x, zone.m_IconPosition.y - 119f, -30f));
		}
		if (zone.m_ZoneIcon != null)
		{
			Vector3 localPosition = zone.m_ZoneGraphic.transform.localPosition;
			localPosition.z = zone.m_ZoneIcon.GetBorderZ();
			zone.m_ZoneGraphic.transform.localPosition = localPosition;
		}
	}

	public ZoneMap GetZoneMap(BaseLevelManager.LevelLayers layer)
	{
		if ((int)layer < 6)
		{
			return m_ZoneMap[(uint)layer];
		}
		return null;
	}

	public void ResetZones()
	{
		int num = m_Zones.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_Zones[i] == null)
			{
				m_Zones[i] = new Zone();
				m_Zones[i].m_ID = i;
			}
			else
			{
				ResetZone(m_Zones[i]);
			}
		}
		m_TotalZonesInUse = 0;
	}

	private void ResetZone(Zone zone)
	{
		if (zone == null)
		{
			return;
		}
		if (zone.m_bActive)
		{
			int[] map = GetZoneMap(zone.m_Layer).m_Map;
			int num = 0;
			for (int i = 0; i < 120; i++)
			{
				for (int j = 0; j < 120; j++)
				{
					if (map[num] == zone.m_ID)
					{
						map[num] = -1;
					}
					num++;
				}
			}
		}
		zone.m_bActive = false;
		zone.m_bValid = false;
		zone.m_bReachable = true;
		zone.m_Value = 0;
		zone.m_TotalTiles = 0;
		zone.m_TotalIgnoredTiles = 0;
		zone.m_TotalInsideTiles = 0;
		zone.m_TotalOutsideTiles = 0;
		zone.m_TotalNoTiles = 0;
		zone.m_RequiresUpdate = false;
		zone.m_RequiresZonePrintUpdate = false;
		zone.m_BlocksInZone.Clear();
		zone.m_Required.Clear();
		if (zone.m_ZoneGraphic != null)
		{
			UnityEngine.Object.Destroy(zone.m_ZoneGraphic.gameObject);
			zone.m_ZoneGraphic = null;
		}
		if (zone.m_ZoneIcon != null)
		{
			zone.m_ZoneIcon.DestroyZoneIcon();
			zone.m_ZoneIcon = null;
		}
	}

	public Zone GetZone(int iZoneIndex, bool bSupressWarning = false)
	{
		if (iZoneIndex < m_Zones.Length)
		{
			if (m_Zones[iZoneIndex].m_bActive)
			{
				return m_Zones[iZoneIndex];
			}
			if (bSupressWarning)
			{
			}
		}
		return null;
	}

	public Zone GetZoneAt(int iX, int iY, BaseLevelManager.LevelLayers eLayer)
	{
		ZoneMap zoneMap = GetZoneMap(eLayer);
		if (zoneMap != null)
		{
			int num = zoneMap.m_Map[iY * 120 + iX];
			if (num != -1)
			{
				return GetZone(num);
			}
		}
		return null;
	}

	public Zone GetFreeZone(ZoneDetailsManager.ZoneTypes zoneType, BaseLevelManager.LevelLayers layer, int iID = -1)
	{
		ZoneDetailsManager instance = ZoneDetailsManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		if (zoneType >= ZoneDetailsManager.ZoneTypes.TOTAL || zoneType == ZoneDetailsManager.ZoneTypes.INVALID)
		{
			return null;
		}
		if ((int)layer >= 6)
		{
			return null;
		}
		ZoneDetailsManager.ZoneDetails zoneDetails = instance.GetZoneDetails(zoneType);
		if (zoneDetails == null)
		{
			return null;
		}
		int num = m_Zones.Length;
		if (iID >= num)
		{
			while (iID >= num)
			{
				num += 25;
				Array.Resize(ref m_Zones, num);
				for (int i = m_TotalZonesInUse; i < num; i++)
				{
					m_Zones[i] = new Zone();
					m_Zones[i].m_ID = i;
				}
			}
		}
		int num2 = -1;
		if (iID != -1)
		{
			if (m_Zones[iID] == null || m_Zones[iID].m_bActive)
			{
				return null;
			}
			num2 = iID;
		}
		else if (m_TotalZonesInUse == num)
		{
			num += 25;
			Array.Resize(ref m_Zones, num);
			for (int j = m_TotalZonesInUse; j < num; j++)
			{
				m_Zones[j] = new Zone();
				m_Zones[j].m_ID = j;
			}
			num2 = m_TotalZonesInUse;
		}
		else
		{
			for (int k = 0; k < num; k++)
			{
				if (!m_Zones[k].m_bActive)
				{
					num2 = k;
					break;
				}
			}
		}
		m_TotalZonesInUse++;
		ResetZone(m_Zones[num2]);
		m_Zones[num2].m_bActive = true;
		m_Zones[num2].m_Layer = layer;
		m_Zones[num2].m_ZoneType = zoneType;
		m_Zones[num2].m_ZoneDetails = zoneDetails;
		string standardErrorText = zoneDetails.GetStandardErrorText();
		m_Zones[num2].m_strErrors = standardErrorText.Replace("%Object%", zoneDetails.GetZoneNameText());
		MarkAsChanged(num2);
		MarkAsZonePrintChanged(num2);
		return m_Zones[num2];
	}

	public void ReleaseZone(Zone releaseZone)
	{
		if (releaseZone != null && releaseZone.m_bActive)
		{
			ReleaseZone(releaseZone.m_ID);
		}
	}

	public void ReleaseZone(int iIndex)
	{
		if (iIndex < m_Zones.Length && iIndex >= 0 && m_Zones[iIndex].m_bActive)
		{
			if (m_Zones[iIndex].m_bValid)
			{
				BuildingBlockManager.GetInstance().AdjustLimitationTotal(m_Zones[iIndex].m_ZoneDetails.m_LimitationGroup, -m_Zones[iIndex].m_Value);
			}
			ResetHighlightZone(iIndex);
			ResetZone(m_Zones[iIndex]);
			m_TotalZonesInUse--;
		}
	}

	public void MarkAsChanged(Zone zone)
	{
		if (zone != null && zone.m_bActive)
		{
			MarkAsChanged(zone.m_ID);
		}
	}

	public void MarkAsChanged(int iZoneIndex)
	{
		if (iZoneIndex < m_Zones.Length && m_Zones[iZoneIndex].m_bActive)
		{
			m_Zones[iZoneIndex].m_RequiresUpdate = true;
		}
	}

	public void MarkAsZonePrintChanged(int iZoneIndex)
	{
		if (iZoneIndex < m_Zones.Length && m_Zones[iZoneIndex].m_bActive)
		{
			m_Zones[iZoneIndex].m_RequiresZonePrintUpdate = true;
		}
	}

	public void AddToZone(int iID, int iX, int iY, int iWidth, int iHeight, ref byte[] zonePrint)
	{
		Zone zone = GetZone(iID);
		if (zone != null && zone.m_bActive)
		{
			zone.m_RequiresUpdate = true;
			zone.m_RequiresZonePrintUpdate = true;
			BaseLevelManager.TileProperty[] tileProperties = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer].m_TileProperties;
			int num = (iWidth * iHeight + 7) / 8;
			if (zonePrint.Length != num)
			{
				ChangeFromToAndSetZonePrint(GetZoneMap(zone.m_Layer).m_Map, -1, iID, ref zonePrint, iX, iY, iWidth, iHeight, tileProperties);
			}
			else
			{
				ChangeToViaZonePrint(GetZoneMap(zone.m_Layer).m_Map, iID, ref zonePrint, iY * 120 + iX, iWidth, iHeight, tileProperties);
			}
		}
	}

	public void SubtractFromZone(int iID, int iX, int iY, int iWidth, int iHeight, ref byte[] zonePrint)
	{
		Zone zone = GetZone(iID);
		if (zone != null && zone.m_bActive)
		{
			zone.m_RequiresUpdate = true;
			zone.m_RequiresZonePrintUpdate = true;
			BaseLevelManager.TileProperty[] tileProperties = m_LevelManager.m_BuildingLayers[(uint)zone.m_Layer].m_TileProperties;
			int num = (iWidth * iHeight + 7) / 8;
			if (zonePrint.Length != num)
			{
				ChangeFromToAndSetZonePrint(GetZoneMap(zone.m_Layer).m_Map, iID, -1, ref zonePrint, iX, iY, iWidth, iHeight, tileProperties);
			}
			else
			{
				ChangeToViaZonePrint(GetZoneMap(zone.m_Layer).m_Map, -1, ref zonePrint, iY * 120 + iX, iWidth, iHeight, tileProperties);
			}
		}
	}

	private void ChangeToViaZonePrint(int[] map, int iNewValue, ref byte[] zonePrint, int iIndex, int iWidth, int iHeight, BaseLevelManager.TileProperty[] props)
	{
		int num = 120 - iWidth;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < iHeight; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				if ((zonePrint[num2] & (1 << num3)) != 0 && (props[iIndex] & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
				{
					map[iIndex] = iNewValue;
				}
				if (++num3 == 8)
				{
					num3 = 0;
					num2++;
				}
				iIndex++;
			}
			iIndex += num;
		}
	}

	private void ChangeFromToAndSetZonePrint(int[] map, int iOldValue, int iNewValue, ref byte[] zonePrint, int X, int Y, int iWidth, int iHeight, BaseLevelManager.TileProperty[] props)
	{
		int num = 120 - iWidth;
		int num2 = 0;
		byte b = 0;
		int num3 = Y * 120 + X;
		int num4 = (iHeight * iWidth + 7) / 8;
		zonePrint = new byte[num4];
		for (int i = 0; i < iHeight; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				if (map[num3] == iOldValue && (props[num3] & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask)
				{
					map[num3] = iNewValue;
					zonePrint[num2] |= (byte)(1 << (int)b);
				}
				if (++b == 8)
				{
					b = 0;
					num2++;
				}
				num3++;
			}
			num3 += num;
		}
	}

	public int CreateAZone(ZoneDetailsManager.ZoneTypes type, BaseLevelManager.LevelLayers floor, int iX, int iY, int iWidth, int iHeight, int iID = -1, bool bCreateUI = true)
	{
		int num = (iWidth * iHeight + 7) / 8;
		byte[] array = new byte[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = byte.MaxValue;
		}
		return CreateAZone(type, floor, iX, iY, iWidth, iHeight, array, iID, bCreateUI);
	}

	public int CreateAZone(ZoneDetailsManager.ZoneTypes type, BaseLevelManager.LevelLayers floor, int iX, int iY, int iWidth, int iHeight, byte[] zonePrint, int iID = -1, bool bCreateUI = true)
	{
		int num = (iWidth * iHeight + 7) / 8;
		if (zonePrint.Length == num)
		{
			Zone freeZone = GetFreeZone(type, floor, iID);
			if (freeZone != null)
			{
				ZoneMap zoneMap = m_ZoneMap[(uint)floor];
				int num2 = 120 - iWidth;
				int num3 = iY * 120 + iX;
				int num4 = 0;
				int num5 = 0;
				for (int i = 0; i < iHeight; i++)
				{
					for (int j = 0; j < iWidth; j++)
					{
						if ((zonePrint[num4] & (1 << num5)) != 0)
						{
							zoneMap.m_Map[num3] = freeZone.m_ID;
						}
						if (++num5 == 8)
						{
							num5 = 0;
							num4++;
						}
						num3++;
					}
					num3 += num2;
				}
				freeZone.m_Left = iX;
				freeZone.m_Bottom = iY;
				freeZone.m_Width = iWidth;
				freeZone.m_Height = iHeight;
				freeZone.m_ZonePrint = zonePrint;
				freeZone.m_bCreateUI = bCreateUI;
				m_LastCreatedZone = freeZone;
				return freeZone.m_ID;
			}
		}
		return -1;
	}

	private void HighlightZonesObjects()
	{
		if (m_HighlightZone == -1 || m_HighlightZone < 0 || m_HighlightZone >= m_Zones.Length)
		{
			return;
		}
		Zone zone = m_Zones[m_HighlightZone];
		m_Scale += Time.deltaTime * 0.5f;
		if (m_Scale > 1f)
		{
			m_Scale -= 1f;
		}
		float num = 1f;
		num = ((!(m_Scale >= 0.5f)) ? (num + m_Scale) : (num + (1f - m_Scale)));
		if (zone == null)
		{
			return;
		}
		for (int i = 0; i < zone.m_BlocksInZone.Count; i++)
		{
			if (zone.m_BlocksInZone[i] != null && zone.m_BlocksInZone[i].m_Object != null)
			{
				zone.m_BlocksInZone[i].m_Object.transform.localScale = new Vector3(num, num, 1f);
			}
		}
	}

	public void SetHighlightZone(int iZone)
	{
		ResetHighlightZone(iZone);
		if (iZone < m_Zones.Length)
		{
			m_HighlightZone = iZone;
		}
	}

	public void ResetHighlightZone(int iZone)
	{
		if (m_HighlightZone == -1 || iZone != m_HighlightZone || iZone < 0 || iZone >= m_Zones.Length)
		{
			return;
		}
		Zone zone = m_Zones[m_HighlightZone];
		for (int i = 0; i < zone.m_BlocksInZone.Count; i++)
		{
			if (zone.m_BlocksInZone[i] != null && zone.m_BlocksInZone[i].m_Object != null)
			{
				zone.m_BlocksInZone[i].m_Object.transform.localScale = Vector3.one;
			}
		}
		m_HighlightZone = -1;
	}

	public Zone GetZoneOfType(ref int iStartingIndex, ZoneDetailsManager.ZoneTypes zoneType)
	{
		if (iStartingIndex >= 0)
		{
			int num = m_Zones.Length;
			while (iStartingIndex < num)
			{
				if (m_Zones[iStartingIndex] != null && m_Zones[iStartingIndex].m_bValid && m_Zones[iStartingIndex].m_ZoneType == zoneType)
				{
					iStartingIndex++;
					return m_Zones[iStartingIndex - 1];
				}
				iStartingIndex++;
			}
		}
		return null;
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.1f, 1f, 0.5f);
		Vector3 vector = Vector3.zero;
		if (m_LevelManager != null)
		{
			vector = m_LevelManager.m_BuildingLayers[1].m_Tiles.transform.position;
			vector.z = 0f;
		}
		for (int i = 0; i < m_Zones.Length; i++)
		{
			if (m_Zones[i] == null || !m_Zones[i].m_bValid || !m_Zones[i].m_bActive)
			{
				continue;
			}
			int count = m_Zones[i].m_BlocksInZone.Count;
			for (int j = 0; j < count; j++)
			{
				int count2 = m_Zones[i].m_BlocksInZone[j].m_InteractPoints.Count;
				Gizmos.color = new Color(0.1f, 1f, 0.5f);
				for (int k = 0; k < count2; k++)
				{
					int num = m_Zones[i].m_BlocksInZone[j].m_InteractPoints[k] % 120;
					int num2 = m_Zones[i].m_BlocksInZone[j].m_InteractPoints[k] / 120 - 120;
					Gizmos.DrawCube(vector + new Vector3((float)num + 0.5f, (float)num2 + 0.5f, -33f), new Vector3(0.5f, 0.5f, 1f));
				}
				Gizmos.color = new Color(0f, 0f, 0f);
				if (m_Zones[i].m_BlocksInZone[j].m_GoodInteractPoint != -1)
				{
					int num3 = m_Zones[i].m_BlocksInZone[j].m_GoodInteractPoint % 120;
					int num4 = m_Zones[i].m_BlocksInZone[j].m_GoodInteractPoint / 120 - 120;
					Gizmos.DrawCube(vector + new Vector3((float)num3 + 0.5f, (float)num4 + 0.5f, -34f), new Vector3(0.7f, 0.7f, 1f));
				}
			}
		}
	}

	public void GetZoneValidationErrors(ref List<LevelDetailsManager.ErrorData> errorList)
	{
		for (int i = 0; i < m_Zones.Length; i++)
		{
			if (m_Zones[i] != null && m_Zones[i].m_bActive && !m_Zones[i].m_bValid)
			{
				LevelDetailsManager.ErrorData errorData = new LevelDetailsManager.ErrorData(LevelDetailsManager.ErrorData.Severity.Error, m_Zones[i].m_strErrors, (int)m_Zones[i].m_Layer, (int)m_Zones[i].m_IconPosition.x, (int)m_Zones[i].m_IconPosition.y, m_Zones[i].m_ZoneDetails.m_ErrorID);
				LevelDetailsManager.ErrorData.AddToErrorList(errorData, ref errorList);
			}
		}
	}

	public int GetTotalInvalidZonesInLayer(BaseLevelManager.LevelLayers eLayer)
	{
		int num = 0;
		for (int num2 = m_Zones.Length - 1; num2 >= 0; num2--)
		{
			Zone zone = m_Zones[num2];
			if (zone != null && zone.m_bActive && (!zone.m_bValid || zone.m_TotalBlocked != 0) && zone.m_Layer == eLayer)
			{
				num++;
			}
		}
		return num;
	}

	public Zone GetInvalidZonesInLayer(BaseLevelManager.LevelLayers eLayer, ref int iLastChecked)
	{
		int num = m_Zones.Length;
		for (int i = 0; i < num; i++)
		{
			iLastChecked = (iLastChecked + 1) % num;
			Zone zone = m_Zones[iLastChecked];
			if (zone != null && zone.m_bActive && (!zone.m_bValid || zone.m_TotalBlocked > 0) && zone.m_Layer == eLayer)
			{
				return zone;
			}
		}
		return null;
	}

	public void ClearAllReachableFlags()
	{
		int num = m_Zones.Length;
		for (int i = 0; i < num; i++)
		{
			Zone zone = m_Zones[i];
			if (zone != null && zone.m_bActive)
			{
				int count = zone.m_BlocksInZone.Count;
				for (int j = 0; j < count; j++)
				{
					zone.m_BlocksInZone[j].m_BeingBlocked = true;
				}
				if (zone.m_TotalBlocked != count)
				{
					zone.m_ZoneUpdateCount++;
				}
				zone.m_TotalBlocked = count;
			}
		}
	}

	public void SetEverythingAsReachable()
	{
		int num = m_Zones.Length;
		for (int i = 0; i < num; i++)
		{
			Zone zone = m_Zones[i];
			if (zone != null && zone.m_bActive)
			{
				zone.m_bReachable = true;
				int count = zone.m_BlocksInZone.Count;
				for (int j = 0; j < count; j++)
				{
					zone.m_BlocksInZone[j].m_BeingBlocked = false;
				}
				if (zone.m_TotalBlocked != 0)
				{
					zone.m_ZoneUpdateCount++;
				}
				zone.m_TotalBlocked = 0;
			}
		}
	}

	public Zone GetLastCreatedZone()
	{
		Zone lastCreatedZone = m_LastCreatedZone;
		m_LastCreatedZone = null;
		return lastCreatedZone;
	}

	public bool IsZoneWithinArea(int iZoneID, int iX, int iY, int iWidth, int iHeight, BaseLevelManager.LevelLayers layer)
	{
		if (iX < 0)
		{
			iWidth += iX;
			iX = 0;
		}
		else if (iX + iWidth > 120)
		{
			iWidth = 120 - iX;
		}
		if (iY < 0)
		{
			iHeight += iY;
			iY = 0;
		}
		else if (iY + iHeight > 118)
		{
			iHeight = 118 - iY;
		}
		if (iWidth >= 1 && iHeight >= 1)
		{
			int[] map = GetZoneMap(layer).m_Map;
			int num = iY * 120 + iX;
			int num2 = 120 - iWidth;
			for (int i = 0; i < iHeight; i++)
			{
				for (int j = 0; j < iWidth; j++)
				{
					if (map[num++] == iZoneID)
					{
						return true;
					}
				}
				num += num2;
			}
		}
		return false;
	}

	public ZonesInArea WhatIsInArea(int iZoneID, int iX, int iY, int iWidth, int iHeight, BaseLevelManager.LevelLayers layer)
	{
		if (iX < 0)
		{
			iWidth += iX;
			iX = 0;
		}
		else if (iX + iWidth > 120)
		{
			iWidth = 120 - iX;
		}
		if (iY < 0)
		{
			iHeight += iY;
			iY = 0;
		}
		else if (iY + iHeight > 118)
		{
			iHeight = 118 - iY;
		}
		bool flag = false;
		bool flag2 = false;
		if (iWidth >= 1 && iHeight >= 1)
		{
			int[] map = GetZoneMap(layer).m_Map;
			int num = iY * 120 + iX;
			int num2 = 120 - iWidth;
			for (int i = 0; i < iHeight; i++)
			{
				for (int j = 0; j < iWidth; j++)
				{
					if (map[num] == iZoneID)
					{
						flag = true;
						if (flag2)
						{
							return ZonesInArea.OursAndOthers;
						}
					}
					else if (map[num] != -1)
					{
						flag2 = true;
						if (flag)
						{
							return ZonesInArea.OursAndOthers;
						}
					}
					num++;
				}
				num += num2;
			}
		}
		if (flag2)
		{
			return ZonesInArea.Others;
		}
		if (flag)
		{
			return ZonesInArea.JustOurs;
		}
		return ZonesInArea.Nothing;
	}

	public void LimitationGroupChanged(int iLimitationGroup)
	{
		ZoneDetailsManager instance = ZoneDetailsManager.GetInstance();
		for (int i = 0; i < instance.m_Zones.Length; i++)
		{
			if (instance.m_Zones[i].DoesRequireLimitationGroup(iLimitationGroup))
			{
				DirtyAllZonesOfType(instance.m_Zones[i].m_ZoneType);
			}
		}
	}

	private void DirtyAllZonesOfType(ZoneDetailsManager.ZoneTypes zonetype)
	{
		for (int num = m_Zones.Length - 1; num >= 0; num--)
		{
			if (m_Zones[num] != null && m_Zones[num].m_bActive && m_Zones[num].m_ZoneType == zonetype)
			{
				m_Zones[num].m_RequiresUpdate = true;
			}
		}
	}
}
