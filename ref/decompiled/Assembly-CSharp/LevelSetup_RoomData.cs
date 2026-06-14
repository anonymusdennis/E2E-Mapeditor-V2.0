using System.Collections.Generic;
using System.Diagnostics;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class LevelSetup_RoomData : BaseComponentSetup
{
	protected struct LabelEntries
	{
		public int m_iLimitGroup;

		public int m_X;

		public int m_Y;

		public int m_iFloor;
	}

	public enum RoomDataStageEnum
	{
		Start,
		ScanSafeAreas,
		CreatingRooms,
		CreateBorder,
		CreateEnvironment,
		AutoChunk,
		CreatingZones,
		ClearFlags,
		CheckNonZones,
		Finished
	}

	private enum StageResult
	{
		Finished,
		NeedMoreTime
	}

	public RoomManager m_RoomManager;

	public BaseLevelManager m_LevelManager;

	public RoomUtility m_RoomUtility;

	public GameObject m_LevelMaster;

	private LevelEditor_ZoneManager m_ZoneManager;

	private BaseLevelManager.LevelLayers m_Layer = BaseLevelManager.LevelLayers.GroundFloor;

	private int m_TileIndexLastProcessed;

	private Stopwatch m_StopWatch = new Stopwatch();

	private long m_TimeOut = 300L;

	private bool m_ClearRoomsBetweenFloors = true;

	private int[] m_DefaultLimitationHashcodes = new int[41];

	private int[] m_LargeInsideRoom = new int[41];

	private int[] m_LargeOutsideRoom = new int[41];

	private int[] m_LargeInsideRoomSafe = new int[41];

	private int[] m_LargeOutsideRoomSafe = new int[41];

	private int m_BoorderRoom;

	private int[][] m_ZoneMap = new int[6][];

	private LabelEntries[] m_LabelEntries = new LabelEntries[27];

	private RoomDataStageEnum m_State;

	public override SetupPriority GetPriority()
	{
		return SetupPriority.Priority_9;
	}

	public override SetupReturnState Setup()
	{
		if (m_RoomManager == null)
		{
			return FinishedAndRemove();
		}
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		if (m_RoomUtility == null)
		{
			return FinishedAndRemove();
		}
		if (m_LevelMaster == null)
		{
			return FinishedAndRemove();
		}
		if (BaseComponentSetup.m_FloorManager == null && !GetFloorManager())
		{
			return FinishedAndRemove();
		}
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			switch (m_State)
			{
			case RoomDataStageEnum.Start:
			{
				m_Layer = BaseLevelManager.LevelLayers.Underground;
				SpawnPoint.c_HighestSpawnPoint = 10;
				m_TileIndexLastProcessed = 99999999;
				m_State = RoomDataStageEnum.ScanSafeAreas;
				int num = 41;
				for (int i = 0; i < num; i++)
				{
					int[] defaultLimitationHashcodes = m_DefaultLimitationHashcodes;
					int num2 = i;
					BuildingBlockManager.DefaultLimitationGroups defaultLimitationGroups = (BuildingBlockManager.DefaultLimitationGroups)i;
					defaultLimitationHashcodes[num2] = defaultLimitationGroups.ToString().GetHashCode();
				}
				for (int num3 = m_LabelEntries.Length - 1; num3 >= 0; num3--)
				{
					m_LabelEntries[num3].m_iLimitGroup = -1;
				}
				break;
			}
			case RoomDataStageEnum.ScanSafeAreas:
				if (LevelDetailsManager.GetInstance() != null)
				{
					List<LevelDetailsManager.ErrorData> errorList = new List<LevelDetailsManager.ErrorData>();
					LevelDetailsManager.GetInstance().ValidateWalkableAreas(ref errorList, bTestForEscapes: false);
				}
				m_State = RoomDataStageEnum.CreatingRooms;
				break;
			case RoomDataStageEnum.CreatingRooms:
				switch (CreateRoomsStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_TileIndexLastProcessed = 0;
					m_Layer = BaseLevelManager.LevelLayers.FirstFloor;
					m_State = RoomDataStageEnum.CreateBorder;
					break;
				}
				break;
			case RoomDataStageEnum.CreateBorder:
				switch (CreateBorderStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_TileIndexLastProcessed = 0;
					m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
					m_State = RoomDataStageEnum.CreateEnvironment;
					break;
				}
				break;
			case RoomDataStageEnum.CreateEnvironment:
				switch (CreateEnvironmentStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
					m_State = RoomDataStageEnum.AutoChunk;
					break;
				}
				break;
			case RoomDataStageEnum.AutoChunk:
				switch (AutoChunk())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = RoomDataStageEnum.Finished;
					break;
				}
				break;
			case RoomDataStageEnum.Finished:
				m_RoomManager.transform.SetParent(m_LevelMaster.transform);
				m_RoomManager.transform.localPosition = Vector3.zero;
				m_StopWatch.Stop();
				return FinishedAndRemove();
			}
		}
		return TakeABreak();
	}

	public override SetupReturnState SetupV2()
	{
		if (m_RoomManager == null)
		{
			return FinishedAndRemove();
		}
		if (m_LevelManager == null)
		{
			return FinishedAndRemove();
		}
		if (m_RoomUtility == null)
		{
			return FinishedAndRemove();
		}
		if (m_LevelMaster == null)
		{
			return FinishedAndRemove();
		}
		if (BaseComponentSetup.m_FloorManager == null && !GetFloorManager())
		{
			return FinishedAndRemove();
		}
		if (m_ZoneManager == null && (m_ZoneManager = LevelEditor_ZoneManager.GetInstance()) == null)
		{
			return FinishedAndRemove();
		}
		m_StopWatch.Reset();
		m_StopWatch.Start();
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			switch (m_State)
			{
			case RoomDataStageEnum.Start:
			{
				m_Layer = BaseLevelManager.LevelLayers.Underground;
				SpawnPoint.c_HighestSpawnPoint = 10;
				m_TileIndexLastProcessed = 99999999;
				m_State = RoomDataStageEnum.ScanSafeAreas;
				int num = 41;
				for (int i = 0; i < num; i++)
				{
					int[] defaultLimitationHashcodes = m_DefaultLimitationHashcodes;
					int num2 = i;
					BuildingBlockManager.DefaultLimitationGroups defaultLimitationGroups = (BuildingBlockManager.DefaultLimitationGroups)i;
					defaultLimitationHashcodes[num2] = defaultLimitationGroups.ToString().GetHashCode();
				}
				for (int num3 = m_LabelEntries.Length - 1; num3 >= 0; num3--)
				{
					m_LabelEntries[num3].m_iLimitGroup = -1;
				}
				break;
			}
			case RoomDataStageEnum.ScanSafeAreas:
				if (LevelDetailsManager.GetInstance() != null)
				{
					List<LevelDetailsManager.ErrorData> errorList = new List<LevelDetailsManager.ErrorData>();
					LevelDetailsManager.GetInstance().ValidateWalkableAreas(ref errorList, bTestForEscapes: false);
				}
				m_State = RoomDataStageEnum.ClearFlags;
				break;
			case RoomDataStageEnum.ClearFlags:
				ClearScanFlags();
				m_TileIndexLastProcessed = 0;
				m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
				m_State = RoomDataStageEnum.CreatingZones;
				break;
			case RoomDataStageEnum.CreatingZones:
				switch (CreateZonesStageV2())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_TileIndexLastProcessed = 0;
					m_Layer = BaseLevelManager.LevelLayers.FirstFloor;
					m_State = RoomDataStageEnum.CreateBorder;
					break;
				}
				break;
			case RoomDataStageEnum.CreateBorder:
				switch (CreateBorderStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_TileIndexLastProcessed = 0;
					m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
					m_State = RoomDataStageEnum.CreateEnvironment;
					break;
				}
				break;
			case RoomDataStageEnum.CreateEnvironment:
				switch (CreateEnvironmentStage())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
					m_State = RoomDataStageEnum.AutoChunk;
					break;
				}
				break;
			case RoomDataStageEnum.AutoChunk:
				switch (AutoChunk())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = RoomDataStageEnum.CheckNonZones;
					m_Layer = BaseLevelManager.LevelLayers.GroundFloor;
					m_TileIndexLastProcessed = 0;
					break;
				}
				break;
			case RoomDataStageEnum.CheckNonZones:
				switch (CheckNonZones())
				{
				case StageResult.NeedMoreTime:
					m_StopWatch.Stop();
					return TakeABreak();
				case StageResult.Finished:
					m_State = RoomDataStageEnum.Finished;
					break;
				}
				break;
			case RoomDataStageEnum.Finished:
				m_RoomManager.transform.SetParent(m_LevelMaster.transform);
				m_RoomManager.transform.localPosition = Vector3.zero;
				m_StopWatch.Stop();
				return FinishedAndRemove();
			}
		}
		return TakeABreak();
	}

	private StageResult CreateRoomsStage()
	{
		BaseLevelManager.LayerDataCollection data = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
		int num = 14400;
		RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
		int num2 = m_TileIndexLastProcessed % 120;
		int num3 = m_TileIndexLastProcessed / 120;
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_TileIndexLastProcessed >= num)
			{
				if (m_Layer == BaseLevelManager.LevelLayers.Roof)
				{
					return StageResult.Finished;
				}
				m_Layer++;
				if (m_LevelManager.m_VentLayers[(uint)m_Layer])
				{
					m_Layer++;
				}
				m_TileIndexLastProcessed = 0;
				num2 = 0;
				num3 = 0;
				roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
				data = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
				if (m_ClearRoomsBetweenFloors)
				{
					m_LevelManager.ClearRoomNumbersComplexAllocation();
				}
			}
			BaseLevelManager.TileProperty tileProperty = data.m_TileProperties[m_TileIndexLastProcessed];
			BaseLevelManager.RoomProperty roomProperty = data.m_RoomPropertiesMasks[m_TileIndexLastProcessed];
			int tileIndexLastProcessed;
			BaseLevelManager.TileProperty[] tileProperties;
			(tileProperties = data.m_TileProperties)[tileIndexLastProcessed = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed] & BaseLevelManager.TileProperty.InverseScanTileMask;
			if (roomProperty != 0)
			{
				for (int i = 0; i < 4; i++)
				{
					int num4 = data.m_RoomIDs[m_TileIndexLastProcessed + i * 14400];
					if (num4 == 0 || num2 <= 0 || num3 >= 119 || !BaseLevelManager.IsRoomNumberInProperty(ref data, m_TileIndexLastProcessed - 1, num4) || !BaseLevelManager.IsRoomNumberInProperty(ref data, m_TileIndexLastProcessed + 120, num4) || !BaseLevelManager.IsRoomNumberInProperty(ref data, m_TileIndexLastProcessed - 1 + 120, num4))
					{
						continue;
					}
					int num5 = m_LevelManager.GetComplexAllocationRoomNumber(num4);
					if (num5 == 0)
					{
						BuildingBlock_Room buildingBlock_Room = BuildingBlockManager.GetBlock(m_LevelManager.GetBlockIDFromComplexAllocation(num4)) as BuildingBlock_Room;
						if (buildingBlock_Room != null)
						{
							RoomBlob roomBlob = m_RoomManager.CreateNewRoom(roomFloor);
							if ((tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) == 0)
							{
								roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Outdoors;
							}
							else
							{
								roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Indoors;
							}
							roomBlob.m_InmateSafeSpace = buildingBlock_Room.m_InmateSafeSpace;
							roomBlob.m_RoomAffinity = buildingBlock_Room.m_RoomAffinity;
							roomBlob.m_GuardSafeSpace = buildingBlock_Room.m_GuardSafeSpace;
							roomBlob.m_RoomAffinityGuard = buildingBlock_Room.m_RoomAffinityGuard;
							roomBlob.m_SupportSafeSpace = buildingBlock_Room.m_SupportSafeSpace;
							roomBlob.m_RoomAffinitySupport = buildingBlock_Room.m_RoomAffinitySupport;
							roomBlob.m_FloorMaterial = buildingBlock_Room.m_FloorMaterial;
							roomBlob.m_RoomLabel = GetRoomLabel(buildingBlock_Room.m_LabelType, buildingBlock_Room.m_LimitationGroup, num2, num3, (int)m_Layer);
							roomBlob.m_subRules = buildingBlock_Room.m_subRules;
							roomBlob.m_AllowSniping = buildingBlock_Room.m_bAllowSniping;
							m_LevelManager.SetComplexAllocationRoomNumber(num4, roomBlob.m_ID);
							num5 = roomBlob.m_ID;
							RoomBlob.eLocation setToLocation = RoomBlob.eLocation.NowhereSpecial;
							JobType jobType = JobType.Invalid;
							if (buildingBlock_Room.m_LimitationGroup != -1)
							{
								BuildingBlockManager.LimitationGroup limitationGroup = BuildingBlockManager.GetLimitationGroup(buildingBlock_Room.m_LimitationGroup);
								if (limitationGroup != null)
								{
									if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[10])
									{
										setToLocation = RoomBlob.eLocation.ContrabandRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[9])
									{
										setToLocation = RoomBlob.eLocation.ControlRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[14])
									{
										setToLocation = RoomBlob.eLocation.GuardQuarters;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[19])
									{
										setToLocation = RoomBlob.eLocation.GuardRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[17])
									{
										setToLocation = RoomBlob.eLocation.GuardTower;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[2])
									{
										setToLocation = RoomBlob.eLocation.Gym;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[7])
									{
										setToLocation = RoomBlob.eLocation.Infirmary;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[22])
									{
										setToLocation = RoomBlob.eLocation.InfirmaryStockRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[0])
									{
										setToLocation = RoomBlob.eLocation.InmateCell;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[8])
									{
										setToLocation = RoomBlob.eLocation.JobOffice;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[24])
									{
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[12])
									{
										setToLocation = RoomBlob.eLocation.Kennels;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[11])
									{
										setToLocation = RoomBlob.eLocation.Kitchen;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[5])
									{
										setToLocation = RoomBlob.eLocation.Library;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[15])
									{
										setToLocation = RoomBlob.eLocation.Maintenance;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[1])
									{
										setToLocation = RoomBlob.eLocation.MealHall;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[3])
									{
										setToLocation = RoomBlob.eLocation.RollCall;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[4])
									{
										setToLocation = RoomBlob.eLocation.Shower;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[23])
									{
										setToLocation = RoomBlob.eLocation.SocialArea;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[6])
									{
										setToLocation = RoomBlob.eLocation.Solitary;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[16])
									{
										setToLocation = RoomBlob.eLocation.VisitorArea;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[13])
									{
										setToLocation = RoomBlob.eLocation.WardensOffice;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[18])
									{
										setToLocation = RoomBlob.eLocation.WasteCollection;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[26])
									{
										jobType = JobType.Woodwork;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[27])
									{
										jobType = JobType.Shoemaker;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[28])
									{
										jobType = JobType.Blacksmith;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[29])
									{
										jobType = JobType.Mining;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[30])
									{
										jobType = JobType.Plumbing;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[31])
									{
										jobType = JobType.Electrician;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[32])
									{
										jobType = JobType.Kitchen;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[33])
									{
										jobType = JobType.Farming;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[34])
									{
										jobType = JobType.WasteDisposal;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[35])
									{
										jobType = JobType.MailSorting;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[36])
									{
										jobType = JobType.CanineCarer;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[37])
									{
										jobType = JobType.Painting;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[38])
									{
										jobType = JobType.PumpkinCarving;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[39])
									{
										jobType = JobType.VampireLaundry;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
									else if (limitationGroup.m_Hashcode == m_DefaultLimitationHashcodes[40])
									{
										jobType = JobType.TrickOrTreat;
										setToLocation = RoomBlob.eLocation.JobRoom;
									}
								}
							}
							roomBlob.SetRoomLocationWithRoomUtility(ref m_RoomUtility, setToLocation);
							if (jobType != 0)
							{
								RoomBlob_JobRoom roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_JobRoom>();
								if (roomBlobData != null)
								{
									roomBlobData.m_JobType = jobType;
								}
							}
							roomBlob.AutoSetupRoom(num4, m_Layer);
						}
					}
					int y = 119 - num3;
					roomFloor.SetTileBlob(num2, y, num5);
					int tileIndexLastProcessed2;
					(tileProperties = data.m_TileProperties)[tileIndexLastProcessed2 = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed2] | BaseLevelManager.TileProperty.ScanTileMask;
				}
			}
			m_TileIndexLastProcessed++;
			if (++num2 == 120)
			{
				num2 = 0;
				num3++;
			}
		}
		return StageResult.NeedMoreTime;
	}

	private void ClearScanFlags()
	{
		for (int i = 0; i < 6; i++)
		{
			m_ZoneMap[i] = new int[14400];
			int[] array = m_ZoneMap[i];
			int[] map = m_ZoneManager.GetZoneMap((BaseLevelManager.LevelLayers)i).m_Map;
			BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[i];
			for (int j = 0; j < 14400; j++)
			{
				BaseLevelManager.TileProperty[] tileProperties;
				int num;
				(tileProperties = layerDataCollection.m_TileProperties)[num = j] = tileProperties[num] & BaseLevelManager.TileProperty.InverseScanTileMask;
				if ((layerDataCollection.m_TileProperties[j] & BaseLevelManager.TileProperty.WallMask) == BaseLevelManager.TileProperty.WallMask)
				{
					array[j] = -1;
				}
				else
				{
					array[j] = map[j];
				}
			}
		}
		int totalZones = m_ZoneManager.GetTotalZones();
		for (int k = 0; k < totalZones; k++)
		{
			LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(k, bSupressWarning: true);
			if (zone != null)
			{
				zone.m_AllocatedRoomID = -1;
			}
		}
	}

	private StageResult CreateZonesStageV2()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
		int num = 14400;
		RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
		int num2 = m_TileIndexLastProcessed % 120;
		int num3 = m_TileIndexLastProcessed / 120;
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_TileIndexLastProcessed >= num)
			{
				if (m_Layer == BaseLevelManager.LevelLayers.Roof)
				{
					return StageResult.Finished;
				}
				m_Layer++;
				if (m_LevelManager.m_VentLayers[(uint)m_Layer])
				{
					m_Layer++;
				}
				m_TileIndexLastProcessed = 0;
				num2 = 0;
				num3 = 0;
				roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
				layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
			}
			BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[m_TileIndexLastProcessed];
			int[] array = m_ZoneMap[(uint)m_Layer];
			int num4 = array[m_TileIndexLastProcessed];
			if (num4 != -1)
			{
				if (num2 > 0 && num3 < 119)
				{
					bool flag = false;
					if ((tileProperty & BaseLevelManager.TileProperty.ItsADoorMask) == BaseLevelManager.TileProperty.ItsADoorMask)
					{
						if (array[m_TileIndexLastProcessed - 1 + 120] != -1)
						{
							flag = true;
							num4 = array[m_TileIndexLastProcessed - 1 + 120];
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						LevelEditor_ZoneManager.Zone zone = m_ZoneManager.GetZone(num4);
						int num5 = zone.m_AllocatedRoomID;
						if (num5 == -1)
						{
							num5 = CreateNewRoom(zone, roomFloor);
						}
						int y = 119 - num3;
						roomFloor.SetTileBlob(num2, y, num5);
						BaseLevelManager.TileProperty[] tileProperties;
						int tileIndexLastProcessed;
						(tileProperties = layerDataCollection.m_TileProperties)[tileIndexLastProcessed = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed] | BaseLevelManager.TileProperty.ScanTileMask;
					}
				}
			}
			else if (num2 > 0 && num3 < 119)
			{
				int num6 = -1;
				if ((layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1] & BaseLevelManager.TileProperty.ItsADoorMask) != BaseLevelManager.TileProperty.ItsADoorMask)
				{
					num6 = array[m_TileIndexLastProcessed - 1];
				}
				if (num6 == -1 && (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed + 120] & BaseLevelManager.TileProperty.ItsADoorMask) != BaseLevelManager.TileProperty.ItsADoorMask)
				{
					num6 = array[m_TileIndexLastProcessed + 120];
				}
				if (num6 == -1 && (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1 + 120] & BaseLevelManager.TileProperty.ItsADoorMask) != BaseLevelManager.TileProperty.ItsADoorMask)
				{
					num6 = array[m_TileIndexLastProcessed - 1 + 120];
				}
				if (num6 != -1)
				{
					LevelEditor_ZoneManager.Zone zone2 = m_ZoneManager.GetZone(num6);
					int num7 = zone2.m_AllocatedRoomID;
					if (num7 == -1)
					{
						num7 = CreateNewRoom(zone2, roomFloor);
					}
					int y2 = 119 - num3;
					roomFloor.SetTileBlob(num2, y2, num7);
					BaseLevelManager.TileProperty[] tileProperties;
					int tileIndexLastProcessed2;
					(tileProperties = layerDataCollection.m_TileProperties)[tileIndexLastProcessed2 = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed2] | BaseLevelManager.TileProperty.ScanTileMask;
				}
			}
			m_TileIndexLastProcessed++;
			if (++num2 == 120)
			{
				num2 = 0;
				num3++;
			}
		}
		return StageResult.NeedMoreTime;
	}

	private int CreateNewRoom(LevelEditor_ZoneManager.Zone zone, RoomFloor roomFloor)
	{
		ZoneDetailsManager.ZoneDetails zoneDetails = zone.m_ZoneDetails;
		if (zoneDetails != null)
		{
			int bottom = zone.m_Bottom;
			int left = zone.m_Left;
			RoomBlob roomBlob = m_RoomManager.CreateNewRoom(roomFloor);
			roomBlob.m_InmateSafeSpace = zoneDetails.m_InmateSafeSpace;
			roomBlob.m_RoomAffinity = zoneDetails.m_InmateRoomAffinity;
			roomBlob.m_GuardSafeSpace = zoneDetails.m_GuardSafeSpace;
			roomBlob.m_RoomAffinityGuard = zoneDetails.m_GuardRoomAffinity;
			roomBlob.m_SupportSafeSpace = zoneDetails.m_SupportSafeSpace;
			roomBlob.m_RoomAffinitySupport = zoneDetails.m_SupportRoomAffinity;
			roomBlob.m_RoomLabel = GetRoomLabel(zoneDetails.m_LabelType, zone.m_ID, left, bottom, (int)zone.m_Layer);
			roomBlob.m_subRules = zoneDetails.m_SubRules;
			roomBlob.m_AllowSniping = zoneDetails.m_bAllowSniping;
			zone.m_AllocatedRoomID = roomBlob.m_ID;
			if (zone.m_TotalInsideTiles >= zone.m_TotalOutsideTiles)
			{
				roomBlob.m_FloorMaterial = Player_Footsteps.Concrete;
				roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Indoors;
			}
			else
			{
				roomBlob.m_FloorMaterial = Player_Footsteps.Grass;
				roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Outdoors;
			}
			RoomBlob.eLocation blobLocation = zone.m_ZoneDetails.m_BlobLocation;
			JobType jobType = zone.m_ZoneDetails.m_JobType;
			roomBlob.SetRoomLocationWithRoomUtility(ref m_RoomUtility, blobLocation);
			if (jobType != 0)
			{
				RoomBlob_JobRoom roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_JobRoom>();
				if (roomBlobData != null)
				{
					roomBlobData.m_JobType = jobType;
				}
			}
			roomBlob.AutoSetupZone(ref zone);
			return roomBlob.m_ID;
		}
		return -1;
	}

	private RoomLabel GetRoomLabel(BuildingBlock_Room.LabelTypes labelType, int iLimitationGroup, int iX, int iY, int iFloor)
	{
		if (labelType == BuildingBlock_Room.LabelTypes.NoLabel)
		{
			return RoomLabel.None;
		}
		int num = m_LabelEntries.Length;
		for (int i = 1; i < num; i++)
		{
			switch (labelType)
			{
			case BuildingBlock_Room.LabelTypes.Unique:
				if (m_LabelEntries[i].m_iLimitGroup == -1)
				{
					m_LabelEntries[i].m_iLimitGroup = iLimitationGroup;
					m_LabelEntries[i].m_X = iX;
					m_LabelEntries[i].m_Y = iY;
					m_LabelEntries[i].m_iFloor = iFloor;
					return (RoomLabel)i;
				}
				break;
			case BuildingBlock_Room.LabelTypes.RoomsOfThisTypeShare:
				if (m_LabelEntries[i].m_iLimitGroup == iLimitationGroup)
				{
					return (RoomLabel)i;
				}
				break;
			case BuildingBlock_Room.LabelTypes.RoomsOfThisTypeNearShare:
				if (m_LabelEntries[i].m_iLimitGroup == iLimitationGroup && iFloor == m_LabelEntries[i].m_iFloor && Mathf.Abs(iX - m_LabelEntries[i].m_X) < 20 && Mathf.Abs(iY - m_LabelEntries[i].m_Y) < 20)
				{
					return (RoomLabel)i;
				}
				break;
			}
		}
		switch (labelType)
		{
		case BuildingBlock_Room.LabelTypes.Unique:
			return RoomLabel.Z;
		case BuildingBlock_Room.LabelTypes.RoomsOfThisTypeShare:
		case BuildingBlock_Room.LabelTypes.RoomsOfThisTypeNearShare:
			return GetRoomLabel(BuildingBlock_Room.LabelTypes.Unique, iLimitationGroup, iX, iY, iFloor);
		default:
			return RoomLabel.None;
		}
	}

	private StageResult CreateBorderStage()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
		int num = 14400;
		RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
		int num2 = m_TileIndexLastProcessed % 120;
		int num3 = m_TileIndexLastProcessed / 120;
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_TileIndexLastProcessed >= num)
			{
				if (m_Layer == BaseLevelManager.LevelLayers.Roof)
				{
					return StageResult.Finished;
				}
				m_Layer++;
				if (m_LevelManager.m_VentLayers[(uint)m_Layer])
				{
					m_Layer++;
				}
				m_TileIndexLastProcessed = 0;
				num2 = 0;
				num3 = 0;
				roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
				layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
				m_BoorderRoom = 0;
			}
			BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[m_TileIndexLastProcessed] & BaseLevelManager.TileProperty.TileMask;
			bool flag = false;
			if (num2 > 0)
			{
				if ((layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1] & BaseLevelManager.TileProperty.TileMask) != tileProperty)
				{
					flag = true;
				}
				else if (num3 < 119 && (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1 + 120] & BaseLevelManager.TileProperty.TileMask) != tileProperty)
				{
					flag = true;
				}
			}
			if (!flag && num3 < 119 && (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed + 120] & BaseLevelManager.TileProperty.TileMask) != tileProperty)
			{
				flag = true;
			}
			if (flag)
			{
				if (m_BoorderRoom == 0)
				{
					RoomBlob roomBlob = m_RoomManager.CreateNewRoom(roomFloor);
					roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Indoors;
					roomBlob.m_subRules = RoomBlob.RoomSubIdentity_Rules.Inbounds;
					roomBlob.m_InmateSafeSpace = false;
					roomBlob.m_RoomAffinity = RoomBlob.RoomAffinity.Meh;
					roomBlob.m_GuardSafeSpace = false;
					roomBlob.m_RoomAffinityGuard = RoomBlob.RoomAffinity.Meh;
					roomBlob.m_SupportSafeSpace = false;
					roomBlob.m_RoomAffinitySupport = RoomBlob.RoomAffinity.Meh;
					roomBlob.m_FloorMaterial = Player_Footsteps.Concrete;
					roomBlob.SetRoomLocationWithRoomUtility(ref m_RoomUtility, RoomBlob.eLocation.BuildingBoundary);
					m_BoorderRoom = roomBlob.m_ID;
				}
				int y = 119 - num3;
				roomFloor.SetTileBlob(num2, y, m_BoorderRoom);
				BaseLevelManager.TileProperty[] tileProperties;
				int tileIndexLastProcessed;
				(tileProperties = layerDataCollection.m_TileProperties)[tileIndexLastProcessed = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed] | BaseLevelManager.TileProperty.ScanTileMask;
			}
			m_TileIndexLastProcessed++;
			if (++num2 == 120)
			{
				num2 = 0;
				num3++;
			}
		}
		return StageResult.NeedMoreTime;
	}

	private StageResult CheckNonZones()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
		int[] map = m_ZoneManager.GetZoneMap(m_Layer).m_Map;
		int num = 14400;
		RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
		int num2 = m_TileIndexLastProcessed % 120;
		int num3 = m_TileIndexLastProcessed / 120;
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_TileIndexLastProcessed >= num)
			{
				if (m_Layer == BaseLevelManager.LevelLayers.Roof)
				{
					return StageResult.Finished;
				}
				m_Layer++;
				if (m_LevelManager.m_VentLayers[(uint)m_Layer])
				{
					m_Layer++;
				}
				m_TileIndexLastProcessed = 0;
				num2 = 0;
				num3 = 0;
				roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
				layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
				map = m_ZoneManager.GetZoneMap(m_Layer).m_Map;
			}
			if (map[m_TileIndexLastProcessed] == -1 && (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed] & BaseLevelManager.TileProperty.ObjectMask) != 0)
			{
				GameObject gameObject = layerDataCollection.m_ObjectTileObjects[m_TileIndexLastProcessed];
				if (gameObject != null)
				{
					RoomBlob roomBlob = roomFloor.LookUpRoom(num2, 119 - num3);
					if (roomBlob != null)
					{
						if (roomBlob.location == RoomBlob.eLocation.NowhereSpecial)
						{
							roomBlob.SetupNowhereSpecialObject(gameObject);
						}
						if (roomBlob.location == RoomBlob.eLocation.Corridor)
						{
							roomBlob.SetupCorridorObject(gameObject);
						}
					}
				}
			}
			m_TileIndexLastProcessed++;
			if (++num2 == 120)
			{
				num2 = 0;
				num3++;
			}
		}
		return StageResult.NeedMoreTime;
	}

	private StageResult CreateEnvironmentStage()
	{
		BaseLevelManager.LayerDataCollection layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
		int num = 14400;
		RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
		int num2 = m_TileIndexLastProcessed % 120;
		int num3 = m_TileIndexLastProcessed / 120;
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_TileIndexLastProcessed >= num)
			{
				if (m_Layer == BaseLevelManager.LevelLayers.Roof)
				{
					return StageResult.Finished;
				}
				m_Layer++;
				if (m_LevelManager.m_VentLayers[(uint)m_Layer])
				{
					m_Layer++;
				}
				m_TileIndexLastProcessed = 0;
				num2 = 0;
				num3 = 0;
				roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
				layerDataCollection = m_LevelManager.m_BuildingLayers[(uint)m_Layer];
			}
			BaseLevelManager.TileProperty tileProperty = layerDataCollection.m_TileProperties[m_TileIndexLastProcessed];
			if ((tileProperty & BaseLevelManager.TileProperty.TileMask) == BaseLevelManager.TileProperty.TileMask && (tileProperty & BaseLevelManager.TileProperty.ScanTileMask) == 0)
			{
				bool flag = false;
				bool flag2 = true;
				int num4 = 0;
				if ((tileProperty & BaseLevelManager.TileProperty.SafeMask) == 0 && (num2 == 0 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1] & BaseLevelManager.TileProperty.SafeMask) == 0) && (num3 >= 119 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed + 120] & BaseLevelManager.TileProperty.SafeMask) == 0) && (num2 == 0 || num3 >= 119 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1 + 120] & BaseLevelManager.TileProperty.SafeMask) == 0))
				{
					flag2 = false;
				}
				num4 = ((!flag2) ? m_LargeOutsideRoom[(uint)m_Layer] : m_LargeOutsideRoomSafe[(uint)m_Layer]);
				if ((tileProperty & BaseLevelManager.TileProperty.EnvironmentMask) != 0 && (num2 == 0 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1] & BaseLevelManager.TileProperty.EnvironmentMask) != 0) && (num3 >= 119 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed + 120] & BaseLevelManager.TileProperty.EnvironmentMask) != 0) && (num2 == 0 || num3 >= 119 || (layerDataCollection.m_TileProperties[m_TileIndexLastProcessed - 1 + 120] & BaseLevelManager.TileProperty.EnvironmentMask) != 0))
				{
					num4 = ((!flag2) ? m_LargeInsideRoom[(uint)m_Layer] : m_LargeInsideRoomSafe[(uint)m_Layer]);
					flag = true;
				}
				if (num4 == 0)
				{
					RoomBlob roomBlob = m_RoomManager.CreateNewRoom(roomFloor);
					num4 = roomBlob.m_ID;
					roomBlob.m_GuardSafeSpace = false;
					roomBlob.m_SupportSafeSpace = false;
					if (flag2)
					{
						roomBlob.m_subRules = RoomBlob.RoomSubIdentity_Rules.Inbounds;
						roomBlob.m_InmateSafeSpace = true;
					}
					else
					{
						roomBlob.m_subRules = RoomBlob.RoomSubIdentity_Rules.OffLimits;
						roomBlob.m_InmateSafeSpace = false;
					}
					if (flag)
					{
						roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Indoors;
						roomBlob.m_FloorMaterial = Player_Footsteps.Concrete;
						roomBlob.m_RoomAffinity = RoomBlob.RoomAffinity.Meh;
						roomBlob.m_RoomAffinityGuard = RoomBlob.RoomAffinity.Meh;
						roomBlob.m_RoomAffinitySupport = RoomBlob.RoomAffinity.Meh;
						roomBlob.SetRoomLocationWithRoomUtility(ref m_RoomUtility, RoomBlob.eLocation.Corridor);
						if (flag2)
						{
							m_LargeInsideRoomSafe[(uint)m_Layer] = num4;
						}
						else
						{
							m_LargeInsideRoom[(uint)m_Layer] = num4;
						}
					}
					else
					{
						roomBlob.m_subLocation = RoomBlob.RoomSubIdentity_Location.Outdoors;
						roomBlob.m_FloorMaterial = Player_Footsteps.Grass;
						roomBlob.m_RoomAffinity = RoomBlob.RoomAffinity.SuperPopular;
						roomBlob.m_RoomAffinityGuard = RoomBlob.RoomAffinity.Meh;
						roomBlob.m_RoomAffinitySupport = RoomBlob.RoomAffinity.Meh;
						roomBlob.SetRoomLocationWithRoomUtility(ref m_RoomUtility, RoomBlob.eLocation.NowhereSpecial);
						if (flag2)
						{
							m_LargeOutsideRoomSafe[(uint)m_Layer] = num4;
						}
						else
						{
							m_LargeOutsideRoom[(uint)m_Layer] = num4;
						}
					}
				}
				int y = 119 - num3;
				roomFloor.SetTileBlob(num2, y, num4);
				BaseLevelManager.TileProperty[] tileProperties;
				int tileIndexLastProcessed;
				(tileProperties = layerDataCollection.m_TileProperties)[tileIndexLastProcessed = m_TileIndexLastProcessed] = tileProperties[tileIndexLastProcessed] | BaseLevelManager.TileProperty.ScanTileMask;
			}
			m_TileIndexLastProcessed++;
			if (++num2 == 120)
			{
				num2 = 0;
				num3++;
			}
		}
		return StageResult.NeedMoreTime;
	}

	private StageResult AutoChunk()
	{
		while (m_StopWatch.ElapsedMilliseconds < m_TimeOut)
		{
			if (m_Layer == BaseLevelManager.LevelLayers.TOTAL)
			{
				return StageResult.Finished;
			}
			RoomFloor roomFloor = m_RoomManager.m_Floors[(int)m_Layer];
			if (m_LargeInsideRoom[(uint)m_Layer] != 0)
			{
				RoomBlob roomBlob = roomFloor.LookUpRoom(m_LargeInsideRoom[(uint)m_Layer]);
				if (roomBlob != null)
				{
					roomFloor.AutoChunkRoomBlob(roomBlob, ref BaseComponentSetup.m_FloorManager);
				}
			}
			if (m_LargeInsideRoomSafe[(uint)m_Layer] != 0)
			{
				RoomBlob roomBlob2 = roomFloor.LookUpRoom(m_LargeInsideRoomSafe[(uint)m_Layer]);
				if (roomBlob2 != null)
				{
					roomFloor.AutoChunkRoomBlob(roomBlob2, ref BaseComponentSetup.m_FloorManager);
				}
			}
			if (m_LargeOutsideRoom[(uint)m_Layer] != 0)
			{
				RoomBlob roomBlob3 = roomFloor.LookUpRoom(m_LargeOutsideRoom[(uint)m_Layer]);
				if (roomBlob3 != null)
				{
					roomFloor.AutoChunkRoomBlob(roomBlob3, ref BaseComponentSetup.m_FloorManager);
				}
			}
			if (m_LargeOutsideRoomSafe[(uint)m_Layer] != 0)
			{
				RoomBlob roomBlob4 = roomFloor.LookUpRoom(m_LargeOutsideRoomSafe[(uint)m_Layer]);
				if (roomBlob4 != null)
				{
					roomFloor.AutoChunkRoomBlob(roomBlob4, ref BaseComponentSetup.m_FloorManager);
				}
			}
			m_Layer++;
		}
		return StageResult.NeedMoreTime;
	}
}
