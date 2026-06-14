using System;
using System.Collections.Generic;
using Rotorz.Tile;
using UnityEngine;

public abstract class BaseLevelManager : MonoBehaviour
{
	public enum LevelLayers : byte
	{
		Underground,
		GroundFloor,
		GroundFloor_Vent,
		FirstFloor,
		FirstFloor_Vent,
		Roof,
		TOTAL
	}

	protected enum TilePositions
	{
		Center,
		Bottom,
		BottomLeft,
		BottomRight,
		Left,
		Right,
		Top,
		TopLeft,
		TopRight,
		TOTAL
	}

	public class InterestingLocations
	{
		public enum LocationType
		{
			NowhereSpecial,
			OutsideDoor
		}

		public Vector3 m_Position;

		public int m_Value;

		public LocationType m_Type;

		public LevelLayers m_Layer;
	}

	[Flags]
	public enum TileProperty
	{
		EMPTY = 0,
		Blocked_Horizontal_Bits = 1,
		Blocked_Horizontal_Mask = 1,
		InverseBlocked_Horizontal_Mask = -2,
		Blocked_Vertical_Bits = 1,
		Blocked_Vertical_Shift = 1,
		Blocked_Vertical_Mask = 2,
		InverseBlocked_Vertical_Mask = -3,
		Blocked_All_Mask = 3,
		InverseBlocked_Blocked_All_Mask = -4,
		NoBlockingBits = 1,
		NoBlockingShift = 2,
		NoBlockingMask = 4,
		InverseNoBlockingMask = -5,
		WallInRoomBits = 1,
		WallInRoomShift = 3,
		WallInRoomMask = 8,
		InverseWallInRoomMask = -9,
		RoomBits = 1,
		RoomShift = 4,
		RoomMask = 0x10,
		InverseRoomMask = -17,
		ScannedBits = 1,
		ScannedShift = 5,
		ScannedMask = 0x20,
		InverseScannedMask = -33,
		ScanBlockedBits = 1,
		ScanBlockedShift = 6,
		ScanBlockedMask = 0x40,
		InverseScanBlockedMask = -65,
		AvailableBits = 1,
		AvailableShift = 7,
		AvailableMask = 0x80,
		InverseAvailableMask = -129,
		TileBit = 1,
		TileShift = 8,
		TileMask = 0x100,
		InverseTileMask = -257,
		WallBit = 1,
		WallShift = 9,
		WallMask = 0x200,
		InverseWallMask = -513,
		ObjectBit = 1,
		ObjectShift = 0xA,
		ObjectMask = 0x400,
		InverseObjectMask = -1025,
		DecorationBit = 1,
		DecorationShift = 0xB,
		DecorationMask = 0x800,
		InverseDecorationMask = -2049,
		ObjDecMask = 0xC00,
		InverseObjDecMask = -3073,
		BlockBitMask = 0xF00,
		InverseBlockBitMask = -3841,
		EnvironmentBits = 1,
		EnvironmentShift = 0xC,
		EnvironmentMask = 0x1000,
		InverseEnvironmentMask = -4097,
		ChangedBits = 1,
		ChangedShift = 0xD,
		ChangedMask = 0x2000,
		InverseChangedMask = -8193,
		DeleteTileBits = 1,
		DeleteTileShift = 0xE,
		DeleteTileMask = 0x4000,
		InverseDeleteTileMask = -16385,
		DeleteWallBits = 1,
		DeleteWallShift = 0xF,
		DeleteWallMask = 0x8000,
		InverseDeleteWallMask = -32769,
		DeleteObjBits = 1,
		DeleteObjShift = 0x10,
		DeleteObjMask = 0x10000,
		InverseDeleteObjMask = -65537,
		DeleteRoomBits = 1,
		DeleteRoomShift = 0x11,
		DeleteRoomMask = 0x20000,
		InverseDeleteRoomMask = -131073,
		DeleteFlags = 0x3C000,
		InverseDeleteFlags = -245761,
		ScanTileBits = 1,
		ScanTileShift = 0x12,
		ScanTileMask = 0x40000,
		InverseScanTileMask = -262145,
		ScanWallBits = 1,
		ScanWallShift = 0x13,
		ScanWallMask = 0x80000,
		InverseScanWallMask = -524289,
		ScanObjBits = 1,
		ScanObjShift = 0x14,
		ScanObjMask = 0x100000,
		InverseScanObjMask = -1048577,
		ScanRoomBits = 1,
		ScanRoom1Shift = 0x15,
		ScanRoom1Mask = 0x200000,
		InverseScanRoom1Mask = -2097153,
		ScanRoom2Shift = 0x16,
		ScanRoom2Mask = 0x400000,
		InverseScanRoom2Mask = -4194305,
		ScanRoom3Shift = 0x17,
		ScanRoom3Mask = 0x800000,
		InverseScanRoom3Mask = -8388609,
		ScanRoom4Shift = 0x18,
		ScanRoom4Mask = 0x1000000,
		InverseScanRoom4Mask = -16777217,
		CanReachRollCallBits = 1,
		CanReachRollCallShift = 0x19,
		CanReachRollCallMask = 0x2000000,
		InverseCanReachRollCallMask = -33554433,
		SafeBits = 1,
		SafeShift = 0x1A,
		SafeMask = 0x4000000,
		InverseSafeMask = -67108865,
		BlockingBits = 1,
		BlockingShift = 0x1B,
		BlockingMask = 0x8000000,
		InverseBlockingMask = -134217732,
		AlreadyCheckedOrBlocked = 0x42000100,
		AlreadySafeOrBlocked = 0x44000100,
		NaturalBlockage = 0x40000200,
		EntranceBits = 1,
		EntranceShift = 0x1C,
		EntranceMask = 0x10000000,
		InverseEntranceMask = -268435457,
		ExitBits = 1,
		ExitShift = 0x1D,
		ExitMask = 0x20000000,
		InverseExitMask = -536870913,
		TileBlockingBits = 1,
		TileBlockingShift = 0x1E,
		TileBlockingMask = 0x40000000,
		InverseTileBlockingMask = -1073741825,
		ItsADoorBits = 1,
		ItsADoorShift = 0x1F,
		ItsADoorMask = int.MinValue,
		InverseItsADoorMask = int.MaxValue,
		ScanFlags = 0x1FC0000,
		InverseScanFlags = -33292289,
		BlockExistsFlags = 0xF00,
		InverseBlockExistsFlags = -3841,
		TileExistsMask = 0x1100,
		TileInside = 0x1100,
		TileOutside = 0x100,
		WallAndObjects = 0xE00,
		ObjectsAndDecorations = 0xC00,
		LightMask = 0x1200
	}

	[Flags]
	public enum RoomProperty
	{
		EMPTY = 0,
		Room1_Mask = 1,
		Room2_Mask = 2,
		Room3_Mask = 4,
		Room4_Mask = 8,
		InverseRoom1_Mask = -2,
		InverseRoom2_Mask = -3,
		InverseRoom3_Mask = -5,
		InverseRoom4_Mask = -9,
		TotalRoomsAllowed = 0x32
	}

	[Flags]
	public enum TileIDData
	{
		EMPTY = 0,
		IDBits = 0xE,
		IDMask = 0x3FFF,
		IDInvalid = 0x3FFF,
		InverseIDMask = -16384,
		VariantBits = 8,
		VariantShift = 0xE,
		VariantMask = 0x3FC000,
		VariantInvalid = 0x3FC000,
		InverseVariantMask = -4177921,
		SeedBits = 8,
		SeedShift = 0x16,
		SeedMask = 0x3FC00000,
		InverseSeedMask = -1069547521,
		ComplexBits = 1,
		ComplexShift = 0x1E,
		ComplexMask = 0x40000000,
		InverseComplexMask = -1069547521,
		ReservedBits = 1,
		ReservedShift = 0x1F
	}

	[Serializable]
	public class LayerDataCollection
	{
		public bool m_Changed;

		public TileProperty[] m_TileProperties;

		public RoomProperty[] m_RoomProperties;

		public RoomProperty[] m_RoomPropertiesMasks;

		public int[] m_RoomIDs;

		public TileIDData[] m_TileTileIDs;

		public GameObject[] m_TileTileObjects;

		public TileIDData[] m_WallTileIDs;

		public GameObject[] m_WallTileObjects;

		public TileIDData[] m_ObjectTileIDs;

		public GameObject[] m_ObjectTileObjects;

		public GameObject m_Tiles;

		public TileSystem m_Tiles_TileSystem;

		public GameObject m_Walls;

		public TileSystem m_Walls_TileSystem;

		public GameObject m_Objects;

		public GameObject m_Decorations;

		public void Init()
		{
			m_TileProperties = new TileProperty[14400];
			m_RoomPropertiesMasks = new RoomProperty[14400];
			m_RoomIDs = new int[57600];
			m_TileTileIDs = new TileIDData[14400];
			m_TileTileObjects = new GameObject[14400];
			m_WallTileIDs = new TileIDData[14400];
			m_WallTileObjects = new GameObject[14400];
			m_ObjectTileIDs = new TileIDData[14400];
			m_ObjectTileObjects = new GameObject[14400];
			m_Changed = false;
			for (int num = 14399; num >= 0; num--)
			{
				m_TileProperties[num] = TileProperty.EMPTY;
				m_RoomPropertiesMasks[num] = RoomProperty.EMPTY;
				m_RoomIDs[num] = 0;
				m_RoomIDs[num + 14400] = 0;
				m_RoomIDs[num + 14400 + 14400] = 0;
				m_RoomIDs[num + 14400 + 14400 + 14400] = 0;
				m_TileTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
				m_WallTileIDs[num] = TileIDData.IDMask | TileIDData.VariantMask;
				m_ObjectTileIDs[num] = TileIDData.IDMask;
			}
		}

		public void Release()
		{
			m_TileProperties = new TileProperty[0];
			m_RoomPropertiesMasks = new RoomProperty[0];
			m_RoomIDs = new int[0];
			m_TileTileIDs = new TileIDData[0];
			m_TileTileObjects = new GameObject[0];
			m_WallTileIDs = new TileIDData[0];
			m_WallTileObjects = new GameObject[0];
			m_ObjectTileIDs = new TileIDData[0];
			m_ObjectTileObjects = new GameObject[0];
			m_Changed = false;
		}
	}

	[Flags]
	public enum LayersEnvironment : byte
	{
		Inside = 0,
		Outside = 1
	}

	[Serializable]
	public class ComplexBlockRegistry
	{
		public enum State : byte
		{
			Inactive,
			Room,
			Complex
		}

		public int m_BlockID = -1;

		public State m_State;

		public int m_RoomManagerRoomNumber;

		public int m_RoomManagerRoomNumberPerm;

		public int m_RoomManagerRoomNumberSecondary;
	}

	[Flags]
	public enum BrushError
	{
		eNone = 0,
		eInvalid = 4,
		eNoClearance = 8,
		eOutOfStock = 0x10,
		eInsideRequired = 0x20,
		eInsideAboveRequired = 0x40,
		eInsideBelowRequired = 0x80,
		eOutsideRequired = 0x100,
		eOutsideAboveRequired = 0x200,
		eOutsideBelowRequired = 0x400,
		eBlocked = 0x800,
		eBlockedBelow = 0x1000,
		eBlockedAbove = 0x2000,
		eRoomBlocked = 0x4000,
		eOutOfBounds = 0x8000,
		eCantOverwriteZone = 0x10000,
		eZoneNoIslands = 0x20000,
		eZoneNoDoughnuts = 0x40000,
		eZoneOverEmptySpace = 0x80000,
		eZoneOverAnotherZone = 0x100000
	}

	public abstract class RoomObjectCollectionTypeBase
	{
		public abstract Type GetCollectionType();

		public abstract void AddToList(UnityEngine.Object obj);

		public abstract bool IsContentUsed(int iIndex);

		public abstract void SetContentUsedState(int iIndex, bool bUsed);
	}

	public class RoomObjectCollectionType<T> : RoomObjectCollectionTypeBase where T : UnityEngine.Object
	{
		public Type m_Type = typeof(T);

		public List<T> m_Contents = new List<T>();

		public List<bool> m_Used = new List<bool>();

		public override Type GetCollectionType()
		{
			return m_Type;
		}

		public override void AddToList(UnityEngine.Object obj)
		{
			T val = (T)obj;
			if (val != null && !m_Contents.Contains(val))
			{
				m_Contents.Add(val);
				m_Used.Add(item: false);
			}
		}

		public override void SetContentUsedState(int iIndex, bool bUsed)
		{
			if (iIndex >= 0 && iIndex < m_Used.Count)
			{
				m_Used[iIndex] = bUsed;
			}
		}

		public override bool IsContentUsed(int iIndex)
		{
			if (iIndex >= 0 && iIndex < m_Used.Count)
			{
				return m_Used[iIndex];
			}
			return false;
		}
	}

	private static BaseLevelManager m_Instance = null;

	protected const int INVALIDINDEX = -1;

	protected List<GameObject> m_ObjectsWithPhotonViews = new List<GameObject>();

	protected List<InterestingLocations> m_InterestingLocations = new List<InterestingLocations>();

	private List<KeyFunctionality.KeyColour> m_KeysInitialized = new List<KeyFunctionality.KeyColour>();

	protected LevelEditor_ZoneManager m_ZoneManager;

	public const int c_LayerWidth = 120;

	public const int c_LayerHeight = 120;

	public const int c_LayerWidthHalf = 60;

	public const int c_LayerHeightHalf = 60;

	public const int c_LayerWidth_x_2 = 240;

	public const int c_LayerHeight_x_2 = 240;

	public const int c_LayerTileCount = 14400;

	public const int c_LayerTileCountx2 = 28800;

	public const int c_LayerTileCountx3 = 43200;

	public const int c_LayerTileCountx4 = 57600;

	public static readonly int[] c_LayerHeights = new int[6] { 118, 118, 118, 119, 119, 120 };

	protected int[] m_SurroundingBlocks = new int[32]
	{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1
	};

	protected int[][] m_SurroundOffsets = new int[9][]
	{
		new int[3] { 1, 120, 121 },
		new int[5] { -1, 1, 119, 120, 121 },
		new int[3] { -1, -121, -120 },
		new int[5] { -120, -119, 1, 120, 121 },
		new int[8] { -121, -120, -119, -1, 1, 119, 120, 121 },
		new int[5] { -121, -120, -1, 119, 120 },
		new int[3] { -120, -119, 1 },
		new int[5] { -121, -120, -119, -1, 1 },
		new int[3] { -121, -120, -1 }
	};

	public static string[] c_LayersNames = new string[6] { "Underground", "Ground floor", "Ground floor Vents", "First floor", "First floor Vents", "Roof" };

	[HideInInspector]
	public int m_CurrentComplexAllocation;

	[HideInInspector]
	public RoomProperty m_CurrentComplexProcessed;

	public ComplexBlockRegistry[] m_ComplexAllocations = new ComplexBlockRegistry[50];

	[HideInInspector]
	public LevelLayers m_CurrentLayer = LevelLayers.GroundFloor;

	[HideInInspector]
	public LayersEnvironment m_CurrentEnvironment = LayersEnvironment.Outside;

	[HideInInspector]
	public int m_CurrentBlockID = -1;

	public float[] m_fPositionOffsetsX = new float[7] { 0f, 0.5f, 0.5f, -59.5f, -59.5f, 0f, 0f };

	public float[] m_fPositionOffsetsY = new float[7] { 0f, -119.5f, -119.5f, -59.5f, -59.5f, 0f, 0f };

	public bool[] m_VentLayers = new bool[6] { false, false, true, false, true, false };

	private bool m_bInitializedData;

	public int m_UndoCount;

	public GameObject m_WaypointParent;

	public LayerDataCollection[] m_BuildingLayers = new LayerDataCollection[6];

	private float[] m_LayerZPositions = new float[6];

	private bool m_bLayerZOffsetSetup;

	public BuildingBlockManager m_BuildingBlockManager;

	public BuildingBlockGroupManager m_BuildingGroupManager;

	public static BaseLevelManager GetInstance()
	{
		return m_Instance;
	}

	protected virtual void Awake()
	{
		m_Instance = this;
		base.enabled = false;
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		if (m_BuildingBlockManager == null)
		{
			m_BuildingBlockManager = BuildingBlockManager.GetInstance();
		}
		if (m_BuildingGroupManager == null)
		{
			m_BuildingGroupManager = BuildingBlockGroupManager.GetInstance();
		}
		if (m_ZoneManager == null)
		{
			m_ZoneManager = LevelEditor_ZoneManager.GetInstance();
		}
		if (!IsEverythingSetUp())
		{
			base.enabled = false;
		}
		InitializeData();
	}

	public virtual void InitializeData()
	{
		if (m_bInitializedData)
		{
			return;
		}
		m_CurrentLayer = LevelLayers.GroundFloor;
		m_CurrentEnvironment = LayersEnvironment.Outside;
		m_CurrentComplexAllocation = 0;
		m_CurrentComplexProcessed = RoomProperty.EMPTY;
		m_CurrentBlockID = -1;
		for (int num = m_BuildingLayers.Length - 1; num >= 0; num--)
		{
			m_BuildingLayers[num].Init();
		}
		m_bInitializedData = true;
		for (int num2 = m_ComplexAllocations.Length - 1; num2 >= 0; num2--)
		{
			if (m_ComplexAllocations[num2] == null)
			{
				m_ComplexAllocations[num2] = new ComplexBlockRegistry();
			}
			m_ComplexAllocations[num2].m_State = ComplexBlockRegistry.State.Inactive;
			m_ComplexAllocations[num2].m_RoomManagerRoomNumber = 0;
			m_ComplexAllocations[num2].m_RoomManagerRoomNumberSecondary = 0;
		}
		m_ObjectsWithPhotonViews = new List<GameObject>();
		if (m_BuildingGroupManager == null || !m_BuildingGroupManager.UpdateBlocksWithGroupData())
		{
		}
		m_BuildingBlockManager.InitializeBlockInstructions();
	}

	public virtual bool IsEverythingSetUp()
	{
		bool result = true;
		if (m_BuildingBlockManager == null)
		{
			result = false;
		}
		if (m_BuildingGroupManager == null)
		{
			result = false;
		}
		if (m_ZoneManager == null)
		{
			result = false;
		}
		for (int i = 0; i < 6; i++)
		{
			if (m_BuildingLayers[i].m_Objects == null)
			{
				result = false;
			}
			if (m_BuildingLayers[i].m_Tiles == null)
			{
				result = false;
			}
			if (m_BuildingLayers[i].m_Walls == null)
			{
				result = false;
			}
			if (m_BuildingLayers[i].m_Decorations == null)
			{
				if (m_BuildingLayers[i].m_Objects == null)
				{
					result = false;
				}
				else
				{
					m_BuildingLayers[i].m_Decorations = m_BuildingLayers[i].m_Objects;
				}
			}
		}
		return result;
	}

	public LevelLayers GetCurrentLayer()
	{
		return m_CurrentLayer;
	}

	private int GetFreeComplexAllocationEntry()
	{
		int num = m_ComplexAllocations.Length;
		for (int i = 1; i < num; i++)
		{
			if (m_ComplexAllocations[i].m_State == ComplexBlockRegistry.State.Inactive)
			{
				return i;
			}
		}
		return 0;
	}

	public void ClearRoomNumbersComplexAllocation()
	{
		int num = m_ComplexAllocations.Length;
		for (int i = 1; i < num; i++)
		{
			m_ComplexAllocations[i].m_RoomManagerRoomNumber = 0;
			m_ComplexAllocations[i].m_RoomManagerRoomNumberSecondary = 0;
		}
	}

	public int GetComplexAllocationRoomNumber(int iIndex)
	{
		return m_ComplexAllocations[iIndex].m_RoomManagerRoomNumber;
	}

	public int GetComplexAllocationFromRoomNumber(int iRoomNumber)
	{
		int num = m_ComplexAllocations.Length;
		for (int i = 1; i < num; i++)
		{
			if (m_ComplexAllocations[i].m_State != 0 && m_ComplexAllocations[i].m_RoomManagerRoomNumberPerm == iRoomNumber)
			{
				return i;
			}
		}
		return 0;
	}

	public int GetComplexAllocationRoomNumberSecondary(int iIndex)
	{
		return m_ComplexAllocations[iIndex].m_RoomManagerRoomNumberSecondary;
	}

	public void SetComplexAllocationRoomNumber(int iIndex, int iRoomNumber)
	{
		m_ComplexAllocations[iIndex].m_RoomManagerRoomNumber = iRoomNumber;
		m_ComplexAllocations[iIndex].m_RoomManagerRoomNumberPerm = iRoomNumber;
	}

	public void SetComplexAllocationRoomNumberSecondary(int iIndex, int iRoomNumber)
	{
		m_ComplexAllocations[iIndex].m_RoomManagerRoomNumberSecondary = iRoomNumber;
	}

	public ComplexBlockRegistry.State GetComplexAllocationType(int iIndex)
	{
		return m_ComplexAllocations[iIndex].m_State;
	}

	public int StartComplexObjectAllocation(int iBlockID)
	{
		if (m_CurrentComplexAllocation != 0)
		{
			return m_CurrentComplexAllocation;
		}
		int num = GetFreeComplexAllocationEntry();
		if (num == 0)
		{
			int num2 = m_ComplexAllocations.Length;
			Array.Resize(ref m_ComplexAllocations, num2 + 50);
			for (int num3 = m_ComplexAllocations.Length - 1; num3 >= num2; num3--)
			{
				if (m_ComplexAllocations[num3] == null)
				{
					m_ComplexAllocations[num3] = new ComplexBlockRegistry();
				}
				m_ComplexAllocations[num3].m_State = ComplexBlockRegistry.State.Inactive;
				m_ComplexAllocations[num3].m_BlockID = -1;
			}
			num = num2;
		}
		if (num > 0)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
			if (block != null)
			{
				m_BuildingBlockManager.AdjustLimitationTotal(iBlockID, bAdd: true, bForceUpdate: true);
				if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
				{
					m_ComplexAllocations[num].m_State = ComplexBlockRegistry.State.Complex;
				}
				else
				{
					m_ComplexAllocations[num].m_State = ComplexBlockRegistry.State.Room;
				}
				m_ComplexAllocations[num].m_BlockID = iBlockID;
				m_CurrentComplexAllocation = num;
				m_CurrentComplexProcessed = (RoomProperty)num;
				return m_CurrentComplexAllocation;
			}
			return 0;
		}
		return 0;
	}

	public int GetBlockIDFromComplexAllocation(int iIndex)
	{
		if (iIndex >= m_ComplexAllocations.Length)
		{
			return -1;
		}
		if (m_ComplexAllocations[iIndex].m_State == ComplexBlockRegistry.State.Inactive)
		{
			return -1;
		}
		return m_ComplexAllocations[iIndex].m_BlockID;
	}

	public void CancelComplexObjectAllocation(int iComplexObjectAllocationIndex)
	{
		if (iComplexObjectAllocationIndex < m_ComplexAllocations.Length && m_ComplexAllocations[iComplexObjectAllocationIndex].m_State != 0)
		{
			m_BuildingBlockManager.AdjustLimitationTotal(m_ComplexAllocations[iComplexObjectAllocationIndex].m_BlockID, bAdd: false, bForceUpdate: true);
			m_ComplexAllocations[iComplexObjectAllocationIndex].m_State = ComplexBlockRegistry.State.Inactive;
			m_CurrentComplexAllocation = 0;
			m_CurrentComplexProcessed = RoomProperty.EMPTY;
		}
	}

	public void EndComplexObjectAllocation()
	{
		if (m_CurrentComplexAllocation == 0)
		{
		}
		m_CurrentComplexAllocation = 0;
		m_CurrentComplexProcessed = RoomProperty.EMPTY;
	}

	public void SetComplexObjectAllocation(int iComplexObjectAllocationIndex)
	{
		if (m_CurrentComplexAllocation == 0)
		{
			m_CurrentComplexAllocation = iComplexObjectAllocationIndex;
			m_CurrentComplexProcessed = (RoomProperty)iComplexObjectAllocationIndex;
		}
	}

	public void SetAreaAsChanged(int X, int Y, int W, int H)
	{
		W = Mathf.Clamp(W + X, 0, 120);
		H = Mathf.Clamp(H + Y, 0, 120);
		X = Mathf.Clamp(X, 0, 120);
		Y = Mathf.Clamp(Y, 0, 120);
		int num = X + Y * 120;
		int num2 = 120 - (W - X);
		for (int num3 = H - Y; num3 > 0; num3--)
		{
			for (int num4 = W - X; num4 > 0; num4--)
			{
				TileProperty[] tileProperties;
				int num5;
				(tileProperties = m_BuildingLayers[(uint)m_CurrentLayer].m_TileProperties)[num5 = num++] = tileProperties[num5] | TileProperty.ChangedMask;
			}
			num += num2;
		}
		m_BuildingLayers[(uint)m_CurrentLayer].m_Changed = true;
	}

	public bool SetCurrentBuildingBlock(int iBlockID)
	{
		int num = iBlockID;
		if (iBlockID != -1)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
			if (block == null)
			{
				iBlockID = -1;
			}
		}
		if (m_CurrentBlockID != iBlockID)
		{
			m_CurrentBlockID = iBlockID;
		}
		return m_CurrentBlockID == num;
	}

	public int GetCurrentBuildingBlock()
	{
		return m_CurrentBlockID;
	}

	protected int GetTheTileIDAt(int iLayer, int iIndex, ref BaseBuildingBlock.GroupFlags groupInfo)
	{
		int num = (int)(m_BuildingLayers[iLayer].m_TileTileIDs[iIndex] & TileIDData.IDMask);
		if (num == 16383)
		{
			return -1;
		}
		groupInfo |= BuildingBlockManager.GetBlock(num).m_GroupFlags;
		return num;
	}

	protected int GetTheWallIDAt(int iLayer, int iIndex, ref BaseBuildingBlock.GroupFlags groupInfo)
	{
		int num = (int)(m_BuildingLayers[iLayer].m_WallTileIDs[iIndex] & TileIDData.IDMask);
		if (num == 16383)
		{
			return -1;
		}
		groupInfo |= BuildingBlockManager.GetBlock(num).m_GroupFlags;
		return num;
	}

	protected int GetTheObjectIDAt(int iLayer, int iIndex, ref BaseBuildingBlock.GroupFlags groupInfo)
	{
		int num = (int)(m_BuildingLayers[iLayer].m_ObjectTileIDs[iIndex] & TileIDData.IDMask);
		if (num == 16383)
		{
			return -1;
		}
		groupInfo |= BuildingBlockManager.GetBlock(num).m_GroupFlags;
		return num;
	}

	[ContextMenu("CleanUp")]
	public void CleanLevel()
	{
		if (m_ComplexAllocations == null || m_ComplexAllocations.Length != 50)
		{
			m_ComplexAllocations = new ComplexBlockRegistry[50];
		}
		if (m_BuildingBlockManager == null)
		{
			m_BuildingBlockManager = BuildingBlockManager.GetInstance();
		}
		m_bInitializedData = false;
		int num = 14400;
		m_CurrentLayer = LevelLayers.GroundFloor;
		m_CurrentEnvironment = LayersEnvironment.Outside;
		m_CurrentComplexAllocation = 0;
		m_CurrentComplexProcessed = RoomProperty.EMPTY;
		m_CurrentBlockID = -1;
		if (m_BuildingBlockManager != null)
		{
			m_BuildingBlockManager.ClearLimitationTotals();
		}
		for (int num2 = m_ComplexAllocations.Length - 1; num2 >= 0; num2--)
		{
			if (m_ComplexAllocations[num2] == null)
			{
				m_ComplexAllocations[num2] = new ComplexBlockRegistry();
			}
			m_ComplexAllocations[num2].m_State = ComplexBlockRegistry.State.Inactive;
			m_ComplexAllocations[num2].m_BlockID = -1;
		}
		for (int i = 1; i < 6; i++)
		{
			int num3 = m_BuildingLayers[i].m_TileTileIDs.Length;
			int num4 = m_BuildingLayers[i].m_WallTileIDs.Length;
			int num5 = m_BuildingLayers[i].m_ObjectTileIDs.Length;
			int num6 = m_BuildingLayers[i].m_TileProperties.Length;
			int num7 = m_BuildingLayers[i].m_RoomPropertiesMasks.Length;
			int num8 = m_BuildingLayers[i].m_TileTileObjects.Length;
			int num9 = m_BuildingLayers[i].m_ObjectTileObjects.Length;
			int num10 = m_BuildingLayers[i].m_WallTileObjects.Length;
			m_BuildingLayers[i].m_Changed = false;
			for (int j = 0; j < num; j++)
			{
				if (num3 > j)
				{
					m_BuildingLayers[i].m_TileTileIDs[j] = TileIDData.IDMask | TileIDData.VariantMask;
				}
				if (num4 > j)
				{
					m_BuildingLayers[i].m_WallTileIDs[j] = TileIDData.IDMask | TileIDData.VariantMask;
				}
				if (num5 > j)
				{
					m_BuildingLayers[i].m_ObjectTileIDs[j] = TileIDData.IDMask;
				}
				if (num6 > j)
				{
					m_BuildingLayers[i].m_TileProperties[j] = TileProperty.EMPTY;
				}
				if (num7 > j)
				{
					m_BuildingLayers[i].m_RoomPropertiesMasks[j] = RoomProperty.EMPTY;
					m_BuildingLayers[i].m_RoomIDs[j] = 0;
					m_BuildingLayers[i].m_RoomIDs[j + 14400] = 0;
					m_BuildingLayers[i].m_RoomIDs[j + 14400 + 14400] = 0;
					m_BuildingLayers[i].m_RoomIDs[j + 14400 + 14400 + 14400] = 0;
				}
				if (num8 > j && m_BuildingLayers[i].m_TileTileObjects[j] != null)
				{
					UnityEngine.Object.Destroy(m_BuildingLayers[i].m_TileTileObjects[j]);
					m_BuildingLayers[i].m_TileTileObjects[j] = null;
				}
				if (num10 > j && m_BuildingLayers[i].m_WallTileObjects[j] != null)
				{
					UnityEngine.Object.Destroy(m_BuildingLayers[i].m_WallTileObjects[j]);
					m_BuildingLayers[i].m_WallTileObjects[j] = null;
				}
				if (num9 > j && m_BuildingLayers[i].m_ObjectTileObjects[j] != null)
				{
					UnityEngine.Object.Destroy(m_BuildingLayers[i].m_ObjectTileObjects[j]);
					m_BuildingLayers[i].m_ObjectTileObjects[j] = null;
				}
			}
			if (m_BuildingLayers[i].m_Tiles_TileSystem != null)
			{
				int columnCount = m_BuildingLayers[i].m_Tiles_TileSystem.ColumnCount;
				int rowCount = m_BuildingLayers[i].m_Tiles_TileSystem.RowCount;
				for (int k = 0; k < rowCount; k++)
				{
					for (int l = 0; l < columnCount; l++)
					{
						TileData tile = m_BuildingLayers[i].m_Tiles_TileSystem.GetTile(l, k);
						if (tile != null)
						{
							tile.gameObject = null;
						}
					}
				}
			}
			if (!(m_BuildingLayers[i].m_Walls_TileSystem != null))
			{
				continue;
			}
			int columnCount2 = m_BuildingLayers[i].m_Walls_TileSystem.ColumnCount;
			int rowCount2 = m_BuildingLayers[i].m_Walls_TileSystem.RowCount;
			for (int m = 0; m < rowCount2; m++)
			{
				for (int n = 0; n < columnCount2; n++)
				{
					TileData tile2 = m_BuildingLayers[i].m_Walls_TileSystem.GetTile(n, m);
					if (tile2 != null)
					{
						tile2.gameObject = null;
					}
				}
			}
		}
	}

	public void SetInitialLevel()
	{
		int num = BuildingBlockManager.GetDefaultLayerBlock(LevelLayers.GroundFloor, LayersEnvironment.Outside);
		if (BuildingBlockManager.GetBlock(num) == null)
		{
			num = -1;
		}
		CleanLevel();
		InitializeData();
		if (BuildingInstructionManager.GetInstance() != null)
		{
			BuildingInstructionManager.GetInstance().ResetContents();
			if (num != -1)
			{
				BuildingInstructionManager.GetInstance().AddBlockArea(num, 0, 0, 120, 118, UnityEngine.Random.Range(0, 10000));
				BuildingInstructionManager.GetInstance().AddToCurrentList(0, BaseBuildInstruction.InstructionTypeEnum.PreventUndo);
			}
			UpdateTiles();
		}
	}

	public List<GameObject> GetPhotonViewList()
	{
		return m_ObjectsWithPhotonViews;
	}

	public void ClearPhotonViewList()
	{
		m_ObjectsWithPhotonViews.Clear();
	}

	public void ClearInterestingLocationsList()
	{
		m_InterestingLocations.Clear();
	}

	public List<InterestingLocations> GetInterestingLocationsList()
	{
		return m_InterestingLocations;
	}

	public void GetObjectsInRoom(int iRoomNumber, LevelLayers eLayer, params RoomObjectCollectionTypeBase[] values)
	{
		LayerDataCollection layerDataCollection = m_BuildingLayers[(uint)eLayer];
		if (values.Length == 0)
		{
		}
		int num = values.Length;
		int num2 = 14400;
		for (int i = 0; i < num2; i++)
		{
			GameObject gameObject = layerDataCollection.m_ObjectTileObjects[i];
			if (!(gameObject != null) || layerDataCollection.m_RoomPropertiesMasks[i] == RoomProperty.EMPTY || (layerDataCollection.m_RoomIDs[i] != iRoomNumber && layerDataCollection.m_RoomIDs[i + 14400] != iRoomNumber && layerDataCollection.m_RoomIDs[i + 28800] != iRoomNumber && layerDataCollection.m_RoomIDs[i + 43200] != iRoomNumber))
			{
				continue;
			}
			Component[] componentsInChildren = gameObject.GetComponentsInChildren<Component>(includeInactive: true);
			int num3 = componentsInChildren.Length;
			for (int j = 0; j < num3; j++)
			{
				Type type = componentsInChildren[j].GetType();
				for (int k = 0; k < num; k++)
				{
					if (values[k].GetCollectionType().IsAssignableFrom(type))
					{
						values[k].AddToList(componentsInChildren[j]);
					}
				}
			}
		}
	}

	public void GetObjectsInZone(ref LevelEditor_ZoneManager.Zone zone, params RoomObjectCollectionTypeBase[] values)
	{
		if (zone == null)
		{
			return;
		}
		if (values.Length == 0)
		{
		}
		int num = values.Length;
		int count = zone.m_BlocksInZone.Count;
		for (int i = 0; i < count; i++)
		{
			if (zone.m_BlocksInZone[i] == null)
			{
				continue;
			}
			GameObject @object = zone.m_BlocksInZone[i].m_Object;
			if (!(@object != null))
			{
				continue;
			}
			Component[] componentsInChildren = @object.GetComponentsInChildren<Component>(includeInactive: true);
			int num2 = componentsInChildren.Length;
			for (int j = 0; j < num2; j++)
			{
				Type type = componentsInChildren[j].GetType();
				for (int k = 0; k < num; k++)
				{
					if (values[k].GetCollectionType().IsAssignableFrom(type))
					{
						values[k].AddToList(componentsInChildren[j]);
					}
				}
			}
		}
	}

	public void GetObjectsInZoneIncludingNonRequired(ref LevelEditor_ZoneManager.Zone zone, params RoomObjectCollectionTypeBase[] values)
	{
		if (zone == null)
		{
			return;
		}
		if (values.Length == 0)
		{
		}
		int num = zone.m_Left + zone.m_Bottom * 120;
		int num2 = 120 - zone.m_Width;
		LayerDataCollection layerDataCollection = m_BuildingLayers[(uint)zone.m_Layer];
		int num3 = 0;
		int num4 = 0;
		int num5 = values.Length;
		for (int i = 0; i < zone.m_Height; i++)
		{
			for (int j = 0; j < zone.m_Width; j++)
			{
				byte b = (byte)(1 << num4);
				if ((zone.m_ZonePrint[num3] & b) != 0 && (layerDataCollection.m_TileProperties[num] & TileProperty.ObjDecMask) != 0 && layerDataCollection.m_ObjectTileObjects[num] != null)
				{
					Component[] componentsInChildren = layerDataCollection.m_ObjectTileObjects[num].GetComponentsInChildren<Component>(includeInactive: true);
					int num6 = componentsInChildren.Length;
					for (int k = 0; k < num6; k++)
					{
						Type type = componentsInChildren[k].GetType();
						for (int l = 0; l < num5; l++)
						{
							if (values[l].GetCollectionType().IsAssignableFrom(type))
							{
								values[l].AddToList(componentsInChildren[k]);
							}
						}
					}
				}
				if (num4 == 7)
				{
					num4 = 0;
					num3++;
				}
				else
				{
					num4++;
				}
				num++;
			}
			num += num2;
		}
	}

	public int GetReachablePointForObject(ref LevelEditor_ZoneManager.Zone zone, GameObject reachableObject)
	{
		if (zone == null || reachableObject == null)
		{
			return -1;
		}
		int count = zone.m_BlocksInZone.Count;
		int instanceID = reachableObject.GetInstanceID();
		for (int i = 0; i < count; i++)
		{
			if (zone.m_BlocksInZone[i] != null)
			{
				GameObject @object = zone.m_BlocksInZone[i].m_Object;
				if (@object != null && @object.GetInstanceID() == instanceID)
				{
					return zone.m_BlocksInZone[i].m_GoodInteractPoint;
				}
			}
		}
		return -1;
	}

	public static int GetRoomNumberFromProperty(ref LayerDataCollection data, int iIndex)
	{
		RoomProperty roomProperty = data.m_RoomPropertiesMasks[iIndex];
		int result = 0;
		if (roomProperty != 0)
		{
			if ((roomProperty & RoomProperty.Room1_Mask) != 0)
			{
				result = data.m_RoomIDs[iIndex];
			}
			else if ((roomProperty & RoomProperty.Room2_Mask) != 0)
			{
				result = data.m_RoomIDs[iIndex + 14400];
			}
			else if ((roomProperty & RoomProperty.Room3_Mask) != 0)
			{
				result = data.m_RoomIDs[iIndex + 28800];
			}
			else if ((roomProperty & RoomProperty.Room4_Mask) != 0)
			{
				result = data.m_RoomIDs[iIndex + 43200];
			}
		}
		return result;
	}

	public static bool IsRoomNumberInProperty(ref LayerDataCollection data, int iIndex, int iRoomLookingFor)
	{
		RoomProperty roomProperty = data.m_RoomPropertiesMasks[iIndex];
		if (roomProperty != 0 && iRoomLookingFor != 0)
		{
			if ((roomProperty & RoomProperty.Room1_Mask) != 0 && data.m_RoomIDs[iIndex] == iRoomLookingFor)
			{
				return true;
			}
			if ((roomProperty & RoomProperty.Room2_Mask) != 0 && data.m_RoomIDs[iIndex + 14400] == iRoomLookingFor)
			{
				return true;
			}
			if ((roomProperty & RoomProperty.Room3_Mask) != 0 && data.m_RoomIDs[iIndex + 28800] == iRoomLookingFor)
			{
				return true;
			}
			if ((roomProperty & RoomProperty.Room4_Mask) != 0 && data.m_RoomIDs[iIndex + 43200] == iRoomLookingFor)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetRoomPartFromProp(ref LayerDataCollection data, int iIndex, int iRoomLookingFor)
	{
		RoomProperty roomProperty = data.m_RoomPropertiesMasks[iIndex];
		if (roomProperty != 0 && iRoomLookingFor != 0)
		{
			if ((roomProperty & RoomProperty.Room1_Mask) == RoomProperty.Room1_Mask && data.m_RoomIDs[iIndex] == iRoomLookingFor)
			{
				return 1;
			}
			if ((roomProperty & RoomProperty.Room2_Mask) == RoomProperty.Room2_Mask && data.m_RoomIDs[iIndex + 14400] == iRoomLookingFor)
			{
				return 2;
			}
			if ((roomProperty & RoomProperty.Room3_Mask) == RoomProperty.Room3_Mask && data.m_RoomIDs[iIndex + 28800] == iRoomLookingFor)
			{
				return 3;
			}
			if ((roomProperty & RoomProperty.Room4_Mask) == RoomProperty.Room4_Mask && data.m_RoomIDs[iIndex + 43200] == iRoomLookingFor)
			{
				return 4;
			}
		}
		return 0;
	}

	public static RoomProperty RemoveRoomNumberInProperty(ref LayerDataCollection data, int iIndex, int iRoomLookingFor)
	{
		RoomProperty roomProperty = data.m_RoomPropertiesMasks[iIndex];
		if (roomProperty != 0 && iRoomLookingFor != 0)
		{
			if ((roomProperty & RoomProperty.Room1_Mask) == RoomProperty.Room1_Mask && data.m_RoomIDs[iIndex] == iRoomLookingFor)
			{
				RoomProperty[] roomPropertiesMasks;
				int num;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num = iIndex] = roomPropertiesMasks[num] & RoomProperty.InverseRoom1_Mask;
				data.m_RoomIDs[iIndex] = 0;
			}
			else if ((roomProperty & RoomProperty.Room2_Mask) == RoomProperty.Room2_Mask && data.m_RoomIDs[iIndex + 14400] == iRoomLookingFor)
			{
				RoomProperty[] roomPropertiesMasks;
				int num2;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num2 = iIndex] = roomPropertiesMasks[num2] & RoomProperty.InverseRoom2_Mask;
				data.m_RoomIDs[iIndex + 14400] = 0;
			}
			else if ((roomProperty & RoomProperty.Room3_Mask) == RoomProperty.Room3_Mask && data.m_RoomIDs[iIndex + 28800] == iRoomLookingFor)
			{
				RoomProperty[] roomPropertiesMasks;
				int num3;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num3 = iIndex] = roomPropertiesMasks[num3] & RoomProperty.InverseRoom3_Mask;
				data.m_RoomIDs[iIndex + 28800] = 0;
			}
			else if ((roomProperty & RoomProperty.Room4_Mask) == RoomProperty.Room4_Mask && data.m_RoomIDs[iIndex + 43200] == iRoomLookingFor)
			{
				RoomProperty[] roomPropertiesMasks;
				int num4;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num4 = iIndex] = roomPropertiesMasks[num4] & RoomProperty.InverseRoom4_Mask;
				data.m_RoomIDs[iIndex + 43200] = 0;
			}
		}
		return data.m_RoomPropertiesMasks[iIndex];
	}

	public static int TotalRoomNumbersInProperty(RoomProperty lookinInRoomProp)
	{
		int num = 0;
		if (lookinInRoomProp != 0)
		{
			if ((lookinInRoomProp & RoomProperty.Room1_Mask) == RoomProperty.Room1_Mask)
			{
				num++;
			}
			if ((lookinInRoomProp & RoomProperty.Room2_Mask) == RoomProperty.Room2_Mask)
			{
				num++;
			}
			if ((lookinInRoomProp & RoomProperty.Room3_Mask) == RoomProperty.Room3_Mask)
			{
				num++;
			}
			if ((lookinInRoomProp & RoomProperty.Room4_Mask) == RoomProperty.Room4_Mask)
			{
				num++;
			}
		}
		return num;
	}

	public static RoomProperty AddRoomNumberToProperty(ref LayerDataCollection data, int iIndex, int iRoomLookingFor)
	{
		if (iRoomLookingFor == 0 || !IsRoomNumberInProperty(ref data, iIndex, iRoomLookingFor))
		{
			RoomProperty roomProperty = data.m_RoomPropertiesMasks[iIndex];
			if ((roomProperty & RoomProperty.Room1_Mask) == 0)
			{
				data.m_RoomIDs[iIndex] = iRoomLookingFor;
				RoomProperty[] roomPropertiesMasks;
				int num;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num = iIndex] = roomPropertiesMasks[num] | RoomProperty.Room1_Mask;
			}
			else if ((roomProperty & RoomProperty.Room2_Mask) == 0)
			{
				data.m_RoomIDs[iIndex + 14400] = iRoomLookingFor;
				RoomProperty[] roomPropertiesMasks;
				int num2;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num2 = iIndex] = roomPropertiesMasks[num2] | RoomProperty.Room2_Mask;
			}
			else if ((roomProperty & RoomProperty.Room3_Mask) == 0)
			{
				data.m_RoomIDs[iIndex + 28800] = iRoomLookingFor;
				RoomProperty[] roomPropertiesMasks;
				int num3;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num3 = iIndex] = roomPropertiesMasks[num3] | RoomProperty.Room3_Mask;
			}
			else if ((roomProperty & RoomProperty.Room4_Mask) == 0)
			{
				data.m_RoomIDs[iIndex + 43200] = iRoomLookingFor;
				RoomProperty[] roomPropertiesMasks;
				int num4;
				(roomPropertiesMasks = data.m_RoomPropertiesMasks)[num4 = iIndex] = roomPropertiesMasks[num4] | RoomProperty.Room4_Mask;
			}
		}
		return data.m_RoomPropertiesMasks[iIndex];
	}

	public bool IsKeyInitialized(KeyFunctionality.KeyColour keyColour)
	{
		if (m_KeysInitialized.Contains(keyColour))
		{
			return true;
		}
		m_KeysInitialized.Add(keyColour);
		return false;
	}

	public LevelLayers WhatLayerIsThisZIn(float fZ)
	{
		if (!m_bLayerZOffsetSetup)
		{
			for (int i = 0; i < 6; i++)
			{
				m_LayerZPositions[i] = m_BuildingLayers[i].m_Tiles.transform.position.z;
			}
			m_bLayerZOffsetSetup = true;
		}
		for (int j = 0; j < 6; j++)
		{
			if (fZ >= m_LayerZPositions[j])
			{
				return (LevelLayers)j;
			}
		}
		return LevelLayers.GroundFloor;
	}

	public void CopyBase()
	{
		BaseLevelManager[] components = GetComponents<BaseLevelManager>();
		for (int i = 0; i < components.Length; i++)
		{
			if (!(components[i] != this))
			{
				continue;
			}
			BaseLevelManager baseLevelManager = components[i];
			baseLevelManager.m_ObjectsWithPhotonViews.Clear();
			baseLevelManager.m_ObjectsWithPhotonViews.AddRange(m_ObjectsWithPhotonViews);
			baseLevelManager.m_InterestingLocations.Clear();
			baseLevelManager.m_InterestingLocations.AddRange(m_InterestingLocations);
			baseLevelManager.m_ComplexAllocations = new ComplexBlockRegistry[m_ComplexAllocations.Length];
			baseLevelManager.m_CurrentLayer = m_CurrentLayer;
			baseLevelManager.m_CurrentEnvironment = m_CurrentEnvironment;
			baseLevelManager.m_UndoCount = m_UndoCount;
			baseLevelManager.m_WaypointParent = m_WaypointParent;
			int num = m_BuildingLayers.Length;
			baseLevelManager.m_BuildingLayers = new LayerDataCollection[num];
			int num2 = 0;
			for (int j = 0; j < num; j++)
			{
				baseLevelManager.m_BuildingLayers[j] = new LayerDataCollection();
				baseLevelManager.m_BuildingLayers[j].m_Changed = m_BuildingLayers[j].m_Changed;
				baseLevelManager.m_BuildingLayers[j].m_Decorations = m_BuildingLayers[j].m_Decorations;
				baseLevelManager.m_BuildingLayers[j].m_Objects = m_BuildingLayers[j].m_Objects;
				baseLevelManager.m_BuildingLayers[j].m_ObjectTileIDs = new TileIDData[m_BuildingLayers[j].m_ObjectTileIDs.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_ObjectTileIDs.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_ObjectTileIDs[num2] = m_BuildingLayers[j].m_ObjectTileIDs[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_ObjectTileObjects = new GameObject[m_BuildingLayers[j].m_ObjectTileObjects.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_ObjectTileObjects.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_ObjectTileObjects[num2] = m_BuildingLayers[j].m_ObjectTileObjects[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_RoomIDs = new int[m_BuildingLayers[j].m_RoomIDs.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_RoomIDs.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_RoomIDs[num2] = m_BuildingLayers[j].m_RoomIDs[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_RoomProperties = new RoomProperty[m_BuildingLayers[j].m_RoomProperties.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_RoomProperties.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_RoomProperties[num2] = m_BuildingLayers[j].m_RoomProperties[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_RoomPropertiesMasks = new RoomProperty[m_BuildingLayers[j].m_RoomPropertiesMasks.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_RoomPropertiesMasks.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_RoomPropertiesMasks[num2] = m_BuildingLayers[j].m_RoomPropertiesMasks[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_TileProperties = new TileProperty[m_BuildingLayers[j].m_TileProperties.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_TileProperties.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_TileProperties[num2] = m_BuildingLayers[j].m_TileProperties[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_Tiles = m_BuildingLayers[j].m_Tiles;
				baseLevelManager.m_BuildingLayers[j].m_Tiles_TileSystem = m_BuildingLayers[j].m_Tiles_TileSystem;
				baseLevelManager.m_BuildingLayers[j].m_TileTileIDs = new TileIDData[m_BuildingLayers[j].m_TileTileIDs.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_TileTileIDs.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_TileTileIDs[num2] = m_BuildingLayers[j].m_TileTileIDs[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_TileTileObjects = new GameObject[m_BuildingLayers[j].m_TileTileObjects.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_TileTileObjects.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_TileTileObjects[num2] = m_BuildingLayers[j].m_TileTileObjects[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_Walls = m_BuildingLayers[j].m_Walls;
				baseLevelManager.m_BuildingLayers[j].m_Walls_TileSystem = m_BuildingLayers[j].m_Walls_TileSystem;
				baseLevelManager.m_BuildingLayers[j].m_WallTileIDs = new TileIDData[m_BuildingLayers[j].m_WallTileIDs.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_WallTileIDs.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_WallTileIDs[num2] = m_BuildingLayers[j].m_WallTileIDs[num2];
				}
				baseLevelManager.m_BuildingLayers[j].m_WallTileObjects = new GameObject[m_BuildingLayers[j].m_WallTileObjects.Length];
				for (num2 = 0; num2 < m_BuildingLayers[j].m_WallTileObjects.Length; num2++)
				{
					baseLevelManager.m_BuildingLayers[j].m_WallTileObjects[num2] = m_BuildingLayers[j].m_WallTileObjects[num2];
				}
			}
			baseLevelManager.m_BuildingBlockManager = m_BuildingBlockManager;
			break;
		}
	}

	public void ExternalAddPhotonViewObject(GameObject obj)
	{
		m_ObjectsWithPhotonViews.Add(obj);
	}

	public abstract void AddSingle(ref BuildingInstructionManager.InstructionOnceElement obj, bool bStorePrevious = true);

	public virtual void RemoveSingle(ref BuildingInstructionManager.InstructionOnceElement obj)
	{
	}

	public abstract void AddSingleWall(ref BuildingInstructionManager.InstructionOnceWallElement obj, bool bStorePrevious = true);

	public virtual void RemoveSingleWall(ref BuildingInstructionManager.InstructionOnceWallElement obj)
	{
	}

	public abstract void AddArea(ref BuildingInstructionManager.InstructionAreaElement obj, bool bStorePrevious = true);

	public virtual void RemoveArea(ref BuildingInstructionManager.InstructionAreaElement obj)
	{
	}

	public abstract void AddAreaWall(ref BuildingInstructionManager.InstructionAreaWallElement obj, bool bStorePrevious = true);

	public virtual void RemoveAreaWall(ref BuildingInstructionManager.InstructionAreaWallElement obj)
	{
	}

	public abstract void CreateZone(ref BuildingInstructionManager.InstructionZoneElement obj);

	public abstract void DeleteZone(ref BuildingInstructionManager.InstructionZoneElement obj);

	public abstract void AddToZone(ref BuildingInstructionManager.InstructionZoneElement obj);

	public abstract void SubtractFromZone(ref BuildingInstructionManager.InstructionZoneElement obj);

	public abstract void AddCommand(ref BuildingInstructionManager.InstructionCommandElement obj, bool bStorePrevious = true);

	public virtual void RemoveCommand(ref BuildingInstructionManager.InstructionCommandElement obj)
	{
	}

	public abstract void AddDelete(ref BuildingInstructionManager.InstructionDeleteElement obj, bool bStorePrevious = true);

	public virtual void RestoreDelete(ref BuildingInstructionManager.InstructionDeleteElement obj)
	{
	}

	public abstract void UpdateTiles();
}
