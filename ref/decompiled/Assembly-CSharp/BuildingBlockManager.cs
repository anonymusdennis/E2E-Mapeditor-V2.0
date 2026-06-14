using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BuildingBlockManager : MonoBehaviour
{
	[Serializable]
	public class LimitationGroup
	{
		public bool m_bPerminent;

		public string m_TextResourceName = string.Empty;

		public string m_GroupName = string.Empty;

		public int m_Min;

		public int m_Max;

		public bool m_bValid;

		public string m_ErrorResourceID = string.Empty;

		public string m_TooManyErrorResourceID = string.Empty;

		public string m_BlockedErrorResourceID = string.Empty;

		public int m_Hashcode;

		public Routines m_Routine = Routines.UNASSIGNED;

		public List<int> m_AutoMinimums = new List<int>();

		public int m_TotalAutoMinimums;

		public int m_MaximumAutoMinimum;

		public int m_PercentToDisplayAt;

		public ZoneDetailsManager.ZoneTypes m_ZoneType;

		public int m_ErrorID = -1;

		public bool m_RequiresUpdate;

		[NonSerialized]
		public int m_CurrentTotal;

		public bool HasMetRequirements()
		{
			if (m_Min == 0 || m_CurrentTotal >= m_Min)
			{
				return true;
			}
			return false;
		}

		public bool IsWithinLimits()
		{
			if (m_Max == 0 || m_CurrentTotal <= m_Max)
			{
				return true;
			}
			return false;
		}
	}

	public enum DefaultLimitationGroups
	{
		InmateCell,
		MealHall,
		Gym,
		RollCall,
		Shower,
		Library,
		Solitary,
		Infirmary,
		JobOffice,
		ControlRoom,
		ContrabandRoom,
		Kitchen,
		Kennels,
		WardensOffice,
		GuardQuarters,
		Maintenance,
		Visitor,
		GuardTower,
		WasteCollection,
		GuardRoom,
		Inmate,
		Guard,
		InfirmaryStockRoom,
		SocialArea,
		JobRoom,
		Generators,
		Job_Woodwork,
		Job_Shoemaker,
		Job_Blacksmith,
		Job_Mining,
		Job_Plumbing,
		Job_Electrician,
		Job_Kitchen,
		Job_Farming,
		Job_WasteDisposal,
		Job_MailSorting,
		Job_CanineCarer,
		Job_Painting,
		Job_PumpkinCarving,
		Job_VampireLaundry,
		Job_TrickOrTreat,
		TOTAL
	}

	[Flags]
	public enum DashedBorderEnum
	{
		L = 1,
		R = 2,
		T = 4,
		B = 8,
		invL = -2,
		invR = -3,
		invT = -5,
		invB = -9,
		Dash_Empty = 0,
		Dash_TL = 5,
		Dash_TR = 6,
		Dash_BL = 9,
		Dash_BR = 0xA,
		Dash_TBLR = 0xF,
		Dash_LR = 3,
		Dash_TB = 0xC,
		Dash_L = 1,
		Dash_R = 2,
		Dash_T = 4,
		Dash_B = 8,
		Dash_TBL = 0xD,
		Dash_TBR = 0xE,
		Dash_TLR = 7,
		Dash_BLR = 0xB,
		COUNT = 0x10
	}

	[Serializable]
	public class FamilyTypes
	{
		public bool m_Active;

		public string m_FamilyName = string.Empty;

		public int m_Order = -1;
	}

	[Serializable]
	public class BlockThemeData
	{
		public BaseBuildingBlock.BlockSet m_BlockSet;

		public string m_TextResourceTitle = string.Empty;

		public Sprite m_Sprite;
	}

	[Serializable]
	public struct DefaultBlocks
	{
		public int m_DefaultInsideBlock;

		public int m_DefaultOutsideBlock;

		public DefaultBlocks(int iInside, int iOutside)
		{
			m_DefaultInsideBlock = iInside;
			m_DefaultOutsideBlock = iOutside;
		}
	}

	public delegate void LimitationGroupChanged(int iGroup);

	public enum AudioBankType
	{
		Ambience,
		Music,
		UI
	}

	public BaseBuildingBlock[] m_BuildingBlocks = new BaseBuildingBlock[0];

	public LimitationGroup[] m_LimitationGroups = new LimitationGroup[0];

	private bool m_UpdateLimitationGroups;

	public Material[] m_DashLines_1 = new Material[16];

	public Material[] m_DashLines_2 = new Material[16];

	public Material[] m_DashLines_3 = new Material[16];

	public FamilyTypes[] m_FamilyTypes = new FamilyTypes[63];

	public List<BlockThemeData> m_BlockThemeData = new List<BlockThemeData>();

	[HideInInspector]
	public DefaultBlocks[] m_DefaultTileBlock = new DefaultBlocks[6]
	{
		new DefaultBlocks(-1, -1),
		new DefaultBlocks(-1, -1),
		new DefaultBlocks(-1, -1),
		new DefaultBlocks(-1, -1),
		new DefaultBlocks(-1, -1),
		new DefaultBlocks(-1, -1)
	};

	public List<string> m_MaterialPaths = new List<string>();

	public List<string> m_DoubleHeightMaterialPaths = new List<string>();

	public List<string> m_BlockIconPaths = new List<string>();

	public List<string> m_ZoneIconPaths = new List<string>();

	public List<string> m_StampPaths = new List<string>();

	public List<string> m_PrefabPaths = new List<string>();

	public Texture2D m_DefaultLevelImage;

	public Routines[] m_DefaultRoutines = new Routines[24];

	[HideInInspector]
	public GameObject m_BrushPrefab;

	[HideInInspector]
	public GameObject m_BorderPrefab;

	[HideInInspector]
	public GameObject m_FacadePrefab;

	public string m_MusicBank = "Prison_Editor_Music_01";

	public string m_AmbientBank = "Prison_Editor_Ambience_01";

	public string m_EffectsBank = "Prison_Editor_UI";

	private static BuildingBlockManager m_Instance;

	private int m_VisualBlockDataCurrentType;

	private int m_VisualBlockDataCurrentIndex;

	private bool m_VisualBlockDataStarted;

	private Stopwatch m_BlockTimesliceStopWatch = new Stopwatch();

	private int m_ActualBlockDataCurrentIndex;

	private bool m_ActualBlockDataStarted;

	private event LimitationGroupChanged OnLimitationChanged;

	public static BuildingBlockManager GetInstance()
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

	public void RebuildData()
	{
		BaseBuildingBlock[] componentsInChildren = GetComponentsInChildren<BaseBuildingBlock>(includeInactive: true);
		int num = componentsInChildren.Length;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			BaseBuildingBlock baseBuildingBlock = componentsInChildren[i];
			if (baseBuildingBlock.m_ID > num2)
			{
				num2 = baseBuildingBlock.m_ID;
			}
			if (baseBuildingBlock.m_ID == -1)
			{
				num3++;
			}
		}
		int num4 = Mathf.Max(num2 + num3 + 1, num);
		m_BuildingBlocks = new BaseBuildingBlock[num4];
		for (int j = 0; j < num; j++)
		{
			BaseBuildingBlock baseBuildingBlock2 = componentsInChildren[j];
			if (baseBuildingBlock2.m_ID == -1)
			{
				continue;
			}
			if (m_BuildingBlocks[baseBuildingBlock2.m_ID] == null)
			{
				m_BuildingBlocks[baseBuildingBlock2.m_ID] = baseBuildingBlock2;
				continue;
			}
			if (baseBuildingBlock2.m_MyInstanceID != baseBuildingBlock2.GetInstanceID())
			{
				baseBuildingBlock2.m_MyInstanceID = baseBuildingBlock2.GetInstanceID();
			}
			baseBuildingBlock2.m_ID = -1;
		}
		int k = 0;
		for (int l = 0; l < num; l++)
		{
			BaseBuildingBlock baseBuildingBlock3 = componentsInChildren[l];
			if (baseBuildingBlock3.m_ID != -1)
			{
				continue;
			}
			for (; k < num4; k++)
			{
				if (m_BuildingBlocks[k] == null)
				{
					baseBuildingBlock3.m_ID = k;
					m_BuildingBlocks[k++] = baseBuildingBlock3;
					baseBuildingBlock3.m_OrderNumber = 9999999;
					break;
				}
			}
		}
		PositionBuildingBlocks();
	}

	public void UpdateBlockData()
	{
		while (!GenerateVisualBlockData())
		{
		}
		while (!GenerateActualBlockData(bWithHouseKeeping: false))
		{
		}
	}

	public bool GenerateVisualBlockData(bool bWithHouseKeeping = true)
	{
		if (!m_VisualBlockDataStarted)
		{
			m_VisualBlockDataStarted = true;
			m_VisualBlockDataCurrentType = 1;
			m_VisualBlockDataCurrentIndex = 0;
		}
		bool flag = false;
		m_BlockTimesliceStopWatch.Reset();
		m_BlockTimesliceStopWatch.Start();
		do
		{
			if (m_VisualBlockDataCurrentIndex >= m_BuildingBlocks.Length)
			{
				m_VisualBlockDataCurrentType++;
				m_VisualBlockDataCurrentIndex = 0;
			}
			BaseBuildingBlock.BuildingBlockType visualBlockDataCurrentType = (BaseBuildingBlock.BuildingBlockType)m_VisualBlockDataCurrentType;
			if (visualBlockDataCurrentType == BaseBuildingBlock.BuildingBlockType.TOTAL)
			{
				flag = true;
				break;
			}
			BaseBuildingBlock baseBuildingBlock = m_BuildingBlocks[m_VisualBlockDataCurrentIndex];
			if (baseBuildingBlock != null && baseBuildingBlock.BlockType == visualBlockDataCurrentType && bWithHouseKeeping)
			{
				baseBuildingBlock.HouseKeeping();
			}
			m_VisualBlockDataCurrentIndex++;
		}
		while (m_BlockTimesliceStopWatch.ElapsedMilliseconds < 300);
		if (!flag)
		{
			return false;
		}
		RebuildData();
		m_VisualBlockDataStarted = false;
		return true;
	}

	public void CleanRoomReps()
	{
		int num = m_BuildingBlocks.Length;
		for (int i = 0; i < num; i++)
		{
			BaseBuildingBlock baseBuildingBlock = m_BuildingBlocks[m_VisualBlockDataCurrentIndex];
			if (!(baseBuildingBlock != null) || baseBuildingBlock.BlockType != BaseBuildingBlock.BuildingBlockType.Room)
			{
				continue;
			}
			int num2 = baseBuildingBlock.m_Representations.Length;
			int num3 = 0;
			while (num3 < num2)
			{
				if (baseBuildingBlock.m_Representations[num3] != null)
				{
					UnityEngine.Object.Destroy(baseBuildingBlock.m_Representations[num3]);
					baseBuildingBlock.m_Representations[num3] = null;
				}
				i++;
			}
			baseBuildingBlock.m_Representations = new GameObject[1];
			baseBuildingBlock.m_Footprint = null;
		}
	}

	public bool GenerateActualBlockData(bool bWithHouseKeeping = true)
	{
		if (!m_ActualBlockDataStarted)
		{
			m_ActualBlockDataStarted = true;
			m_ActualBlockDataCurrentIndex = 0;
		}
		m_BlockTimesliceStopWatch.Reset();
		m_BlockTimesliceStopWatch.Start();
		bool flag = false;
		do
		{
			if (m_ActualBlockDataCurrentIndex < m_BuildingBlocks.Length)
			{
				BaseBuildingBlock baseBuildingBlock = m_BuildingBlocks[m_ActualBlockDataCurrentIndex];
				if (baseBuildingBlock != null && bWithHouseKeeping)
				{
					baseBuildingBlock.HouseKeeping();
				}
				m_ActualBlockDataCurrentIndex++;
				continue;
			}
			flag = true;
			break;
		}
		while (m_BlockTimesliceStopWatch.ElapsedMilliseconds < 300);
		if (!flag)
		{
			return false;
		}
		RebuildData();
		m_ActualBlockDataStarted = false;
		return true;
	}

	public void AddNewBuildingBlock(BaseBuildingBlock obj)
	{
		if (!(obj != null) || obj.m_ID != -1)
		{
			return;
		}
		int num = m_BuildingBlocks.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_BuildingBlocks[i] == null)
			{
				obj.m_ID = i;
				m_BuildingBlocks[i++] = obj;
				PositionBuildingBlocks();
				return;
			}
		}
		RebuildData();
	}

	private void PositionBuildingBlocks()
	{
		int num = m_BuildingBlocks.Length;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 1f;
		for (int i = 0; i < num; i++)
		{
			if (m_BuildingBlocks[i] != null && m_BuildingBlocks[i].m_Footprint != null)
			{
				float x = num2 + (float)(m_BuildingBlocks[i].m_Footprint.m_iLeft * -1);
				float y = num3 + (float)(m_BuildingBlocks[i].m_Footprint.m_iBottom * -1);
				m_BuildingBlocks[i].transform.localPosition = new Vector3(x, y, 0.5f);
				num4 = Mathf.Max(num4, m_BuildingBlocks[i].m_Footprint.GetHeight());
				num2 += (float)m_BuildingBlocks[i].m_Footprint.GetWidth();
				if (num2 > 150f)
				{
					num2 = 0f;
					num3 += num4;
					num4 = 1f;
				}
			}
		}
	}

	public int FindBlockByObjectName(string strName)
	{
		int num = m_BuildingBlocks.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_BuildingBlocks[i] != null && m_BuildingBlocks[i].gameObject.name == strName)
			{
				return i;
			}
		}
		return -1;
	}

	public static string GetBlockName(BaseLevelManager.TileIDData blockID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null)
		{
			int num = (int)(blockID & BaseLevelManager.TileIDData.IDMask);
			if (num == 16383)
			{
				num = -1;
			}
			return GetBlockName(num);
		}
		return "UNKNOWN";
	}

	public string GetBuildingBlockName(BaseLevelManager.TileIDData blockID)
	{
		int num = (int)(blockID & BaseLevelManager.TileIDData.IDMask);
		if (num == 16383)
		{
			num = -1;
		}
		return GetBuildingBlockName(num);
	}

	public static string GetBlockName(int m_BuildingBrickID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null && m_BuildingBrickID >= 0 && m_BuildingBrickID < instance.m_BuildingBlocks.Length && instance.m_BuildingBlocks[m_BuildingBrickID] != null)
		{
			string empty = string.Empty;
			return instance.m_BuildingBlocks[m_BuildingBrickID].name;
		}
		return "UNKNOWN";
	}

	public string GetBuildingBlockName(int m_BuildingBrickID)
	{
		if (m_BuildingBrickID >= 0 && m_BuildingBrickID < m_BuildingBlocks.Length && m_BuildingBlocks[m_BuildingBrickID] != null)
		{
			string empty = string.Empty;
			return m_BuildingBlocks[m_BuildingBrickID].name;
		}
		return "UNKNOWN";
	}

	public static GameObject GetBlockSelectionObject(int m_BuildingBrickID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null && m_BuildingBrickID >= 0 && m_BuildingBrickID < instance.m_BuildingBlocks.Length && instance.m_BuildingBlocks[m_BuildingBrickID] != null)
		{
			return instance.m_BuildingBlocks[m_BuildingBrickID].m_SelectionImageObject;
		}
		return null;
	}

	public static GameObject GetBlockBrush(int buildingBrickID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null && buildingBrickID >= 0 && buildingBrickID < instance.m_BuildingBlocks.Length && instance.m_BuildingBlocks[buildingBrickID] != null)
		{
			if (instance.m_BuildingBlocks[buildingBrickID].m_Brush == null)
			{
				instance.m_BuildingBlocks[buildingBrickID].MakeRepresentations();
			}
			return instance.m_BuildingBlocks[buildingBrickID].m_Brush;
		}
		return null;
	}

	public static BaseBuildingBlock GetBlock(BaseLevelManager.TileIDData buildingBrickID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null)
		{
			int num = (int)(buildingBrickID & BaseLevelManager.TileIDData.IDMask);
			if (num >= 0 && num < instance.m_BuildingBlocks.Length && instance.m_BuildingBlocks[num] != null)
			{
				return instance.m_BuildingBlocks[num];
			}
		}
		return null;
	}

	public BaseBuildingBlock GetBuildingBlock(BaseLevelManager.TileIDData buildingBrickID)
	{
		int num = (int)(buildingBrickID & BaseLevelManager.TileIDData.IDMask);
		if (num >= 0 && num < m_BuildingBlocks.Length && m_BuildingBlocks[num] != null)
		{
			return m_BuildingBlocks[num];
		}
		return null;
	}

	public static BaseBuildingBlock GetBlock(int buildingBrickID)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null && buildingBrickID >= 0 && buildingBrickID < instance.m_BuildingBlocks.Length && instance.m_BuildingBlocks[buildingBrickID] != null)
		{
			return instance.m_BuildingBlocks[buildingBrickID];
		}
		return null;
	}

	public BaseBuildingBlock GetBuildingBlock(int buildingBrickID)
	{
		if (buildingBrickID >= 0 && buildingBrickID < m_BuildingBlocks.Length && m_BuildingBlocks[buildingBrickID] != null)
		{
			return m_BuildingBlocks[buildingBrickID];
		}
		return null;
	}

	public int GetTotalBlocks()
	{
		return m_BuildingBlocks.Length;
	}

	public int GetTotalBlocksOfType(BaseBuildingBlock.BuildingBlockType blockType)
	{
		int num = 0;
		for (int num2 = m_BuildingBlocks.Length - 1; num2 >= 0; num2--)
		{
			if (m_BuildingBlocks[num2] != null && m_BuildingBlocks[num2].BlockType == blockType && !m_BuildingBlocks[num2].m_AutomaticBlock)
			{
				num++;
			}
		}
		return num;
	}

	public int GetTotalValidBlocksOfType(BaseBuildingBlock.BuildingBlockType blockType, BaseLevelManager.LevelLayers layer, BaseLevelManager.LayersEnvironment environment)
	{
		int num = 0;
		int num2 = ((environment != 0) ? (1 << (int)layer * 2 + 1) : (1 << (int)layer * 2));
		for (int num3 = m_BuildingBlocks.Length - 1; num3 >= 0; num3--)
		{
			if (m_BuildingBlocks[num3] != null && m_BuildingBlocks[num3].BlockType == blockType && !m_BuildingBlocks[num3].m_AutomaticBlock && (m_BuildingBlocks[num3].m_ValidLayers & num2) != 0)
			{
				num++;
			}
		}
		return num;
	}

	public int GetBlocksOfType(ref List<int> blockList, BaseBuildingBlock.BuildingBlockType blockType, BaseLevelManager.LevelLayers layer, BaseLevelManager.LayersEnvironment environment, BaseBuildingBlock.BlockSet filterTheme = BaseBuildingBlock.BlockSet.ALL, BaseBuildingBlock.PurposeGroups filterPurpose = BaseBuildingBlock.PurposeGroups.ALL, long iFamily = -1L, bool automatic = true, BaseBuildingBlock.CompletionState validity = BaseBuildingBlock.CompletionState.Complete, bool bOnlySelectable = true)
	{
		int num = 0;
		bool flag = false;
		int num2 = ((layer == BaseLevelManager.LevelLayers.TOTAL || flag) ? ((environment != 0) ? 178956970 : 89478485) : ((environment != 0) ? (1 << (int)layer * 2 + 1) : (1 << (int)layer * 2)));
		bool flag2 = false;
		for (int num3 = m_BuildingBlocks.Length - 1; num3 >= 0; num3--)
		{
			BaseBuildingBlock baseBuildingBlock = m_BuildingBlocks[num3];
			if (baseBuildingBlock != null)
			{
				flag2 = baseBuildingBlock.m_EditorOnly;
				if (baseBuildingBlock.BlockType == blockType && (!flag2 || flag) && ((baseBuildingBlock.m_ValidLayers & num2) != 0 || layer == BaseLevelManager.LevelLayers.TOTAL) && (automatic || (!automatic && !baseBuildingBlock.m_AutomaticBlock)) && (baseBuildingBlock.m_Variation == -1 || baseBuildingBlock.m_VariationSelectable || !bOnlySelectable) && (baseBuildingBlock.m_OurBlockSets & filterTheme) != 0 && (filterPurpose == BaseBuildingBlock.PurposeGroups.ALL || (baseBuildingBlock.m_BlocksPurpose & filterPurpose) != 0) && (iFamily == -1 || (baseBuildingBlock.m_Family & iFamily) != 0))
				{
					int blockCompletionState = (int)baseBuildingBlock.GetBlockCompletionState();
					if (blockCompletionState <= (int)validity || flag)
					{
						num++;
						blockList.Add(num3);
					}
				}
			}
		}
		SortBlockList(ref blockList);
		return num;
	}

	public int CreateNewBlock(BaseBuildingBlock.BuildingBlockType blockType)
	{
		if (blockType != 0)
		{
			GameObject gameObject = new GameObject("New " + blockType);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = Vector3.zero;
			switch (blockType)
			{
			case BaseBuildingBlock.BuildingBlockType.Tile:
				gameObject.AddComponent<BuildingBlock_Tile>();
				break;
			case BaseBuildingBlock.BuildingBlockType.Wall:
				gameObject.AddComponent<BuildingBlock_Wall>();
				break;
			case BaseBuildingBlock.BuildingBlockType.Object:
				gameObject.AddComponent<BuildingBlock_Object>();
				break;
			case BaseBuildingBlock.BuildingBlockType.Decoration:
			{
				BuildingBlock_Decoration buildingBlock_Decoration = gameObject.AddComponent<BuildingBlock_Decoration>();
				if (buildingBlock_Decoration != null)
				{
					buildingBlock_Decoration.m_Solid = false;
				}
				break;
			}
			case BaseBuildingBlock.BuildingBlockType.Room:
				gameObject.AddComponent<BuildingBlock_Room>();
				break;
			case BaseBuildingBlock.BuildingBlockType.Complex:
				gameObject.AddComponent<BuildingBlock_Complex>();
				break;
			default:
				UnityEngine.Object.DestroyImmediate(gameObject);
				gameObject = null;
				break;
			}
			if (gameObject != null)
			{
				RebuildData();
				return gameObject.GetComponent<BaseBuildingBlock>().m_ID;
			}
		}
		return -1;
	}

	public static void CheckVariations()
	{
		BuildingBlockManager instance = GetInstance();
		if (!(instance != null))
		{
		}
	}

	public static int GetDefaultLayerBlock(BaseLevelManager.LevelLayers layer, BaseLevelManager.LayersEnvironment environment)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance == null || (int)layer < 0 || (int)layer >= instance.m_DefaultTileBlock.Length || (environment != 0 && environment != BaseLevelManager.LayersEnvironment.Outside))
		{
			return -1;
		}
		if (environment == BaseLevelManager.LayersEnvironment.Inside)
		{
			return instance.m_DefaultTileBlock[(uint)layer].m_DefaultInsideBlock;
		}
		return instance.m_DefaultTileBlock[(uint)layer].m_DefaultOutsideBlock;
	}

	private void ApplyChangeToAutoMinimums(int iGroup)
	{
		int num = m_LimitationGroups[iGroup].m_CurrentTotal;
		List<int> autoMinimums = m_LimitationGroups[iGroup].m_AutoMinimums;
		for (int num2 = autoMinimums.Count - 1; num2 >= 0; num2--)
		{
			if (m_LimitationGroups[autoMinimums[num2]].m_MaximumAutoMinimum != 0)
			{
				num = Mathf.Min(num, m_LimitationGroups[autoMinimums[num2]].m_MaximumAutoMinimum);
			}
			m_LimitationGroups[autoMinimums[num2]].m_Min = num;
		}
	}

	public void ClearLimitationTotals()
	{
		int num = m_LimitationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_LimitationGroups[i] == null)
			{
				continue;
			}
			if (this.OnLimitationChanged != null && m_LimitationGroups[i].m_CurrentTotal >= m_LimitationGroups[i].m_Max && m_LimitationGroups[i].m_Max != 0)
			{
				m_LimitationGroups[i].m_CurrentTotal = 0;
				if (m_LimitationGroups[i].m_TotalAutoMinimums != 0)
				{
					ApplyChangeToAutoMinimums(i);
				}
				this.OnLimitationChanged(i);
			}
			else
			{
				m_LimitationGroups[i].m_CurrentTotal = 0;
				if (m_LimitationGroups[i].m_TotalAutoMinimums != 0)
				{
					ApplyChangeToAutoMinimums(i);
				}
			}
		}
	}

	public static LimitationGroup GetLimitationGroup(int iIndex)
	{
		BuildingBlockManager instance = GetInstance();
		if (instance != null && iIndex >= 0 && iIndex < instance.m_LimitationGroups.Length)
		{
			return instance.m_LimitationGroups[iIndex];
		}
		return null;
	}

	public LimitationGroup GetTheLimitationGroup(int iIndex)
	{
		if (iIndex >= 0 && iIndex < m_LimitationGroups.Length)
		{
			return m_LimitationGroups[iIndex];
		}
		return null;
	}

	public static int GetTotalBlocksInLimitationGroup(int iIndex)
	{
		BuildingBlockManager instance = GetInstance();
		int num = 0;
		if (instance != null)
		{
			for (int num2 = instance.m_BuildingBlocks.Length - 1; num2 >= 0; num2--)
			{
				if (instance.m_BuildingBlocks[num2] != null && instance.m_BuildingBlocks[num2].m_LimitationGroup == iIndex)
				{
					num++;
				}
			}
		}
		return num;
	}

	public void AdjustLimitationTotal(BaseLevelManager.TileIDData block, bool bAdd)
	{
		int num = (int)(block & BaseLevelManager.TileIDData.IDMask);
		if (num == 16383)
		{
			return;
		}
		int limitationGroup = m_BuildingBlocks[num].m_LimitationGroup;
		if (limitationGroup == -1)
		{
			return;
		}
		bool flag = false;
		if (bAdd)
		{
			m_LimitationGroups[limitationGroup].m_CurrentTotal += m_BuildingBlocks[num].m_LimitationCount;
			if (m_LimitationGroups[limitationGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(limitationGroup);
				flag = true;
			}
			else if (this.OnLimitationChanged != null && m_LimitationGroups[limitationGroup].m_CurrentTotal >= m_LimitationGroups[limitationGroup].m_Max && m_LimitationGroups[limitationGroup].m_Max != 0)
			{
				flag = true;
			}
		}
		else
		{
			int currentTotal = m_LimitationGroups[limitationGroup].m_CurrentTotal;
			m_LimitationGroups[limitationGroup].m_CurrentTotal -= m_BuildingBlocks[num].m_LimitationCount;
			if (m_LimitationGroups[limitationGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(limitationGroup);
				flag = true;
			}
			else if (this.OnLimitationChanged != null && currentTotal == m_LimitationGroups[limitationGroup].m_Max && m_LimitationGroups[limitationGroup].m_Max != 0)
			{
				flag = true;
			}
		}
		if (!flag && m_LimitationGroups[limitationGroup].m_Min != 0)
		{
			flag = true;
		}
		if (!flag || this.OnLimitationChanged != null)
		{
		}
		m_LimitationGroups[limitationGroup].m_RequiresUpdate = true;
		m_UpdateLimitationGroups = true;
	}

	public void CheckForLimitationUpdate()
	{
		if (!m_UpdateLimitationGroups || this.OnLimitationChanged == null)
		{
			return;
		}
		int num = m_LimitationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_LimitationGroups[i] != null && m_LimitationGroups[i].m_RequiresUpdate)
			{
				m_LimitationGroups[i].m_RequiresUpdate = false;
				this.OnLimitationChanged(i);
			}
		}
	}

	public void AdjustLimitationTotal(int iBlockID, bool bAdd, bool bForceUpdate = false)
	{
		if (iBlockID == -1)
		{
			return;
		}
		int limitationGroup = m_BuildingBlocks[iBlockID].m_LimitationGroup;
		if (limitationGroup == -1)
		{
			return;
		}
		bool flag = false;
		if (bAdd)
		{
			m_LimitationGroups[limitationGroup].m_CurrentTotal += m_BuildingBlocks[iBlockID].m_LimitationCount;
			if (m_LimitationGroups[limitationGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(limitationGroup);
				flag = true;
			}
			else if (m_LimitationGroups[limitationGroup].m_Max != 0 && m_LimitationGroups[limitationGroup].m_CurrentTotal >= m_LimitationGroups[limitationGroup].m_Max)
			{
				flag = true;
			}
		}
		else
		{
			int currentTotal = m_LimitationGroups[limitationGroup].m_CurrentTotal;
			m_LimitationGroups[limitationGroup].m_CurrentTotal -= m_BuildingBlocks[iBlockID].m_LimitationCount;
			if (m_LimitationGroups[limitationGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(limitationGroup);
				flag = true;
			}
			else if (m_LimitationGroups[limitationGroup].m_Max != 0 && currentTotal == m_LimitationGroups[limitationGroup].m_Max)
			{
				flag = true;
			}
		}
		if (!flag && m_LimitationGroups[limitationGroup].m_Min != 0)
		{
			flag = true;
		}
		if (flag || bForceUpdate)
		{
		}
		m_LimitationGroups[limitationGroup].m_RequiresUpdate = true;
		m_UpdateLimitationGroups = true;
	}

	public void AdjustLimitationTotal(int iGroup, int iValue, bool bForceUpdate = false)
	{
		if (iGroup == -1)
		{
			return;
		}
		bool flag = false;
		if (iValue > 0)
		{
			m_LimitationGroups[iGroup].m_CurrentTotal += iValue;
			if (m_LimitationGroups[iGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(iGroup);
				flag = true;
			}
			else if (m_LimitationGroups[iGroup].m_Max != 0 && m_LimitationGroups[iGroup].m_CurrentTotal >= m_LimitationGroups[iGroup].m_Max)
			{
				flag = true;
			}
		}
		else
		{
			int currentTotal = m_LimitationGroups[iGroup].m_CurrentTotal;
			m_LimitationGroups[iGroup].m_CurrentTotal += iValue;
			if (m_LimitationGroups[iGroup].m_TotalAutoMinimums != 0)
			{
				ApplyChangeToAutoMinimums(iGroup);
				flag = true;
			}
			else if (m_LimitationGroups[iGroup].m_Max != 0 && currentTotal == m_LimitationGroups[iGroup].m_Max)
			{
				flag = true;
			}
		}
		if (!flag && m_LimitationGroups[iGroup].m_Min != 0)
		{
			flag = true;
		}
		if (flag || bForceUpdate)
		{
		}
		m_LimitationGroups[iGroup].m_RequiresUpdate = true;
		m_UpdateLimitationGroups = true;
	}

	public int GetLimitationTotal(int iIndex)
	{
		if (iIndex < m_LimitationGroups.Length && m_LimitationGroups[iIndex] != null && m_LimitationGroups[iIndex].m_bValid)
		{
			return m_LimitationGroups[iIndex].m_CurrentTotal;
		}
		return 0;
	}

	public bool AreRoutineRequirementsMet(Routines checkRoutine)
	{
		if (checkRoutine >= Routines.RollCall && checkRoutine < Routines.COUNT)
		{
			for (int num = m_LimitationGroups.Length - 1; num >= 0; num--)
			{
				if (m_LimitationGroups[num] != null && m_LimitationGroups[num].m_bValid && m_LimitationGroups[num].m_Routine == checkRoutine && m_LimitationGroups[num].m_CurrentTotal < m_LimitationGroups[num].m_Min)
				{
					return false;
				}
			}
		}
		return true;
	}

	public int GetLimitationTotal(string strName)
	{
		int namedLimitationIndex = GetNamedLimitationIndex(strName);
		if (namedLimitationIndex != -1)
		{
			return m_LimitationGroups[namedLimitationIndex].m_CurrentTotal;
		}
		return 0;
	}

	public int GetNamedLimitationIndex(string strName)
	{
		int hashCode = strName.GetHashCode();
		int num = m_LimitationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_LimitationGroups[i] != null && m_LimitationGroups[i].m_bValid && m_LimitationGroups[i].m_Hashcode == hashCode)
			{
				return i;
			}
		}
		return -1;
	}

	public void InitialiseDefaultLimitations()
	{
		int num = 41;
		int num2 = m_LimitationGroups.Length;
		for (int i = 0; i < num; i++)
		{
			DefaultLimitationGroups defaultLimitationGroups = (DefaultLimitationGroups)i;
			string text = defaultLimitationGroups.ToString();
			int namedLimitationIndex = GetNamedLimitationIndex(text);
			if (namedLimitationIndex != -1)
			{
				continue;
			}
			int num3 = -1;
			for (int j = 0; j < num2; j++)
			{
				if (m_LimitationGroups[j] == null || !m_LimitationGroups[j].m_bValid)
				{
					num3 = j;
					break;
				}
			}
			if (num3 == -1)
			{
				Array.Resize(ref m_LimitationGroups, num2 + 1);
				num3 = num2++;
			}
			LimitationGroup limitationGroup = new LimitationGroup();
			limitationGroup.m_GroupName = text;
			limitationGroup.m_bValid = true;
			limitationGroup.m_bPerminent = true;
			m_LimitationGroups[num3] = limitationGroup;
		}
		for (int k = 0; k < m_LimitationGroups.Length; k++)
		{
			if (m_LimitationGroups[k].m_Hashcode == 0)
			{
				m_LimitationGroups[k].m_Hashcode = m_LimitationGroups[k].m_GroupName.GetHashCode();
			}
		}
	}

	public void RegisterLimitationChange(LimitationGroupChanged limitationGroupChanged)
	{
		if (limitationGroupChanged != null)
		{
			OnLimitationChanged += limitationGroupChanged;
		}
	}

	public bool IsBlockVarientOfThisBlock(int iIsThis, int iInThis)
	{
		if (iIsThis == -1 || iInThis == -1)
		{
			return false;
		}
		BaseBuildingBlock block = GetBlock(iInThis);
		List<int> list = new List<int>();
		list.Add(iInThis);
		while (block != null && !list.Contains(block.m_Variation))
		{
			list.Add(block.m_Variation);
			block = GetBlock(block.m_Variation);
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == iIsThis)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyOtherVarientsHaveSameNumber(int iIsThis)
	{
		if (iIsThis == -1)
		{
			return false;
		}
		BaseBuildingBlock block = GetBlock(iIsThis);
		if (block == null || block.m_Variation == -1 || block.m_VariationNumber == -1)
		{
			return false;
		}
		int variationNumber = block.m_VariationNumber;
		block = GetBlock(block.m_Variation);
		while (block != null)
		{
			if (block.m_VariationNumber == variationNumber)
			{
				return true;
			}
			if (block.m_Variation == iIsThis)
			{
				return false;
			}
			block = GetBlock(block.m_Variation);
		}
		return false;
	}

	public Texture2D GetDefaultLevelImage()
	{
		return m_DefaultLevelImage;
	}

	public void ReOrder()
	{
		List<int> list = new List<int>();
		int num = m_BuildingBlocks.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_BuildingBlocks[i] != null)
			{
				list.Add(m_BuildingBlocks[i].m_ID);
			}
		}
		num = list.Count;
		SortBlockList(ref list);
		int num2 = 10;
		for (int j = 0; j < num; j++)
		{
			GetBlock(list[j]).m_OrderNumber = num2;
			num2 += 10;
		}
	}

	public static void SortBlockList(ref List<int> list)
	{
		list.Sort(delegate(int p1, int p2)
		{
			if (p1 == p2)
			{
				return 0;
			}
			int orderNumber = GetBlock(p1).m_OrderNumber;
			int orderNumber2 = GetBlock(p2).m_OrderNumber;
			if (orderNumber > orderNumber2)
			{
				return 1;
			}
			return (orderNumber == orderNumber2) ? Mathf.Clamp(p1 - p2, -1, 1) : (-1);
		});
	}

	public string GetAudioBanks(AudioBankType type)
	{
		return type switch
		{
			AudioBankType.Ambience => m_AmbientBank, 
			AudioBankType.Music => m_MusicBank, 
			AudioBankType.UI => m_EffectsBank, 
			_ => string.Empty, 
		};
	}

	public BaseBuildingBlock DuplicateBlock(int iID, string strName, bool bDeleteChilden = true)
	{
		BaseBuildingBlock block = GetBlock(iID);
		BaseBuildingBlock baseBuildingBlock = null;
		if (block != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(block.gameObject, block.transform.parent);
			if (gameObject != null)
			{
				gameObject.name = strName;
				baseBuildingBlock = gameObject.GetComponent<BaseBuildingBlock>();
				if (baseBuildingBlock == null)
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
				}
				else
				{
					baseBuildingBlock.m_OrderNumber = 9999999;
					if (bDeleteChilden)
					{
						Recursive_Delete(baseBuildingBlock.gameObject);
					}
				}
			}
		}
		return baseBuildingBlock;
	}

	private void Recursive_Delete(GameObject obj, bool bFirst = true)
	{
		int childCount = obj.transform.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			Transform child = obj.transform.GetChild(num);
			if (child != null)
			{
				GameObject obj2 = child.gameObject;
				Recursive_Delete(obj2, bFirst: false);
			}
		}
		if (!bFirst)
		{
			obj.SetActive(value: false);
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	[ContextMenu("Set up photon view limitations")]
	public void AutoAddToLimitationGroup()
	{
		int namedLimitationIndex = GetNamedLimitationIndex("InteractiveObjects");
		if (namedLimitationIndex == -1)
		{
			return;
		}
		for (int i = 0; i < m_BuildingBlocks.Length; i++)
		{
			BaseBuildingBlock baseBuildingBlock = m_BuildingBlocks[i];
			if (baseBuildingBlock != null && (baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Object || baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Decoration) && baseBuildingBlock.m_HasPhotonViewCount > 0 && baseBuildingBlock.m_LimitationGroup == -1)
			{
				baseBuildingBlock.m_LimitationGroup = namedLimitationIndex;
				baseBuildingBlock.m_LimitationCount = baseBuildingBlock.m_HasPhotonViewCount;
			}
		}
	}

	public string GetThemeTextResource(BaseBuildingBlock.BlockSet blockSet)
	{
		for (int num = m_BlockThemeData.Count - 1; num >= 0; num--)
		{
			if (m_BlockThemeData[num] != null && m_BlockThemeData[num].m_BlockSet == blockSet)
			{
				return m_BlockThemeData[num].m_TextResourceTitle;
			}
		}
		return "THEME NOT KNOWN";
	}

	public Sprite GetThemeSprite(BaseBuildingBlock.BlockSet blockSet)
	{
		for (int num = m_BlockThemeData.Count - 1; num >= 0; num--)
		{
			if (m_BlockThemeData[num] != null && m_BlockThemeData[num].m_BlockSet == blockSet)
			{
				return m_BlockThemeData[num].m_Sprite;
			}
		}
		return null;
	}

	public void ClearGroupTotals()
	{
		for (int num = m_BuildingBlocks.Length - 1; num >= 0; num--)
		{
			if (m_BuildingBlocks[num] != null && (m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Object || m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Decoration || m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Complex))
			{
				if (m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
				{
					BuildingBlock_Complex buildingBlock_Complex = (BuildingBlock_Complex)m_BuildingBlocks[num];
					buildingBlock_Complex.m_InBlockGroups.Clear();
				}
				else
				{
					BuildingBlock_Object buildingBlock_Object = (BuildingBlock_Object)m_BuildingBlocks[num];
					buildingBlock_Object.m_InBlockGroups.Clear();
				}
			}
		}
	}

	public void InitializeBlockInstructions(bool bOnlyIfNull = true)
	{
		for (int num = m_BuildingBlocks.Length - 1; num >= 0; num--)
		{
			if (m_BuildingBlocks[num] != null && (m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Room || m_BuildingBlocks[num].BlockType == BaseBuildingBlock.BuildingBlockType.Complex))
			{
				BuildingBlock_Room buildingBlock_Room = m_BuildingBlocks[num] as BuildingBlock_Room;
				if (buildingBlock_Room != null && (!bOnlyIfNull || buildingBlock_Room.m_BlockInstructions == null))
				{
					buildingBlock_Room.InitInstructionSet();
				}
			}
		}
	}

	public int GetFamilyValueFromString(params string[] arrayFamilies)
	{
		int num = 0;
		for (int num2 = arrayFamilies.Length - 1; num2 >= 0; num2--)
		{
			if (!string.IsNullOrEmpty(arrayFamilies[num2]))
			{
				for (int num3 = m_FamilyTypes.Length - 1; num3 >= 0; num3--)
				{
					if (m_FamilyTypes[num3].m_Active && string.CompareOrdinal(arrayFamilies[num2], m_FamilyTypes[num3].m_FamilyName) == 0)
					{
						num += 1 << num3;
						break;
					}
				}
			}
		}
		return num;
	}
}
