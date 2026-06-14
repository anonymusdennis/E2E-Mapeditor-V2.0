using System;
using UnityEngine;

public class LevelEditorHighLightManager : MonoBehaviour
{
	public enum HightLightFloorTypeEnum
	{
		DontCare,
		Inside,
		Outside,
		NoTile,
		Tile
	}

	private static LevelEditorHighLightManager m_Instance;

	public GameObject m_LevelBase;

	public GameObject[] m_MasterLayers = new GameObject[6];

	public GameObject m_HighlightPrefab;

	public GameObject m_LowerLevelPrefab;

	private GameObject m_MasterObject;

	private GameObject m_BorderObject;

	private GameObject m_LowerLevelObject;

	[NonSerialized]
	public HightLightFloorTypeEnum[] m_FloorHighLights = new HightLightFloorTypeEnum[6];

	private bool[] m_Map = new bool[14280];

	private int m_iCurrentBlockID = -5;

	private BaseLevelManager.LevelLayers m_eCurrentLayer = BaseLevelManager.LevelLayers.GroundFloor;

	private LevelEditorTileHighlight[] m_HighLighters = new LevelEditorTileHighlight[14160];

	private BaseLevelManager m_LevelManager;

	private bool m_bHightRescanPending;

	public static LevelEditorHighLightManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		base.enabled = IsSetupCorrectly();
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
		CreateHighLights();
	}

	private void Update()
	{
		if (BuildingInstructionManager.GetInstance() != null && BuildingInstructionManager.GetInstance().m_IgnoreChecks)
		{
			if (m_MasterObject.activeSelf)
			{
				m_MasterObject.SetActive(value: false);
			}
		}
		else if (!m_MasterObject.activeSelf)
		{
			m_MasterObject.SetActive(value: true);
		}
		bool flag = false;
		if (m_LevelManager.m_CurrentLayer != m_eCurrentLayer)
		{
			flag = true;
			m_eCurrentLayer = m_LevelManager.m_CurrentLayer;
			m_MasterObject.transform.parent = m_MasterLayers[(uint)m_eCurrentLayer].transform;
			m_MasterObject.transform.localPosition = new Vector3(-59.5f, -59.5f, -9f);
			if (m_LowerLevelObject != null)
			{
				if ((int)m_LevelManager.m_CurrentLayer < 2)
				{
					if (m_LowerLevelObject.activeSelf)
					{
						m_LowerLevelObject.SetActive(value: false);
					}
				}
				else
				{
					if (!m_LowerLevelObject.activeSelf)
					{
						m_LowerLevelObject.SetActive(value: true);
					}
					m_LowerLevelObject.transform.parent = m_MasterLayers[(uint)(m_eCurrentLayer - 1)].transform;
					m_LowerLevelObject.transform.localPosition = new Vector3(0f, -1f, -9f);
				}
			}
		}
		if (m_iCurrentBlockID != m_LevelManager.GetCurrentBuildingBlock())
		{
			m_iCurrentBlockID = m_LevelManager.GetCurrentBuildingBlock();
			flag = true;
		}
		flag |= m_bHightRescanPending;
		m_bHightRescanPending = false;
		if (!flag)
		{
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			m_FloorHighLights[i] = HightLightFloorTypeEnum.DontCare;
		}
		BaseBuildingBlock baseBuildingBlock = null;
		if (m_iCurrentBlockID != -1)
		{
			baseBuildingBlock = BuildingBlockManager.GetBlock(m_iCurrentBlockID);
			bool flag2 = (baseBuildingBlock.m_ValidLayers & 0x555) != 0;
			switch (baseBuildingBlock.BlockType)
			{
			case BaseBuildingBlock.BuildingBlockType.Tile:
			case BaseBuildingBlock.BuildingBlockType.Wall:
			{
				HightLightFloorTypeEnum hightLightFloorTypeEnum = HightLightFloorTypeEnum.DontCare;
				if (baseBuildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Wall && ((BuildingBlock_Wall)baseBuildingBlock).m_FloorTileID == -1)
				{
					hightLightFloorTypeEnum = (flag2 ? HightLightFloorTypeEnum.Inside : HightLightFloorTypeEnum.Outside);
				}
				if (flag2)
				{
					switch (m_eCurrentLayer)
					{
					case BaseLevelManager.LevelLayers.GroundFloor:
						m_FloorHighLights[1] = hightLightFloorTypeEnum;
						break;
					case BaseLevelManager.LevelLayers.GroundFloor_Vent:
						m_FloorHighLights[2] = hightLightFloorTypeEnum;
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor:
						m_FloorHighLights[3] = hightLightFloorTypeEnum;
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor_Vent:
						m_FloorHighLights[4] = hightLightFloorTypeEnum;
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.Roof:
						m_FloorHighLights[5] = hightLightFloorTypeEnum;
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						break;
					}
					break;
				}
				switch (m_eCurrentLayer)
				{
				case BaseLevelManager.LevelLayers.GroundFloor:
					m_FloorHighLights[1] = hightLightFloorTypeEnum;
					m_FloorHighLights[2] = HightLightFloorTypeEnum.NoTile;
					m_FloorHighLights[3] = HightLightFloorTypeEnum.NoTile;
					break;
				case BaseLevelManager.LevelLayers.GroundFloor_Vent:
					m_FloorHighLights[2] = hightLightFloorTypeEnum;
					m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
					m_FloorHighLights[3] = HightLightFloorTypeEnum.NoTile;
					break;
				case BaseLevelManager.LevelLayers.FirstFloor:
					m_FloorHighLights[3] = hightLightFloorTypeEnum;
					m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
					m_FloorHighLights[4] = HightLightFloorTypeEnum.NoTile;
					m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
					break;
				case BaseLevelManager.LevelLayers.FirstFloor_Vent:
					m_FloorHighLights[4] = hightLightFloorTypeEnum;
					m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
					m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
					break;
				case BaseLevelManager.LevelLayers.Roof:
					m_FloorHighLights[5] = hightLightFloorTypeEnum;
					m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
					break;
				}
				break;
			}
			case BaseBuildingBlock.BuildingBlockType.Decoration:
			case BaseBuildingBlock.BuildingBlockType.Object:
				m_FloorHighLights[(uint)m_eCurrentLayer] = HightLightFloorTypeEnum.Tile;
				break;
			case BaseBuildingBlock.BuildingBlockType.Complex:
			case BaseBuildingBlock.BuildingBlockType.Room:
			{
				bool flag3 = false;
				bool[] array = new bool[6];
				if (baseBuildingBlock.m_Footprint != null)
				{
					int num = baseBuildingBlock.m_Footprint.m_iW * baseBuildingBlock.m_Footprint.m_iH;
					int num2 = baseBuildingBlock.m_Footprint.m_UsedTiles.Length / num;
					int num3 = 0;
					for (int j = 0; j < num2; j++)
					{
						int num4 = j * num;
						for (int k = 0; k < num; k++)
						{
							if (baseBuildingBlock.m_Footprint.m_UsedTiles[num4++] != 0)
							{
								num3++;
								array[j] = true;
								break;
							}
						}
					}
					flag3 = num3 != 1;
				}
				if (flag3)
				{
					if (flag2)
					{
						if (array[5] || array[4])
						{
							m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						}
						if (array[3] || array[2])
						{
							m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						}
						break;
					}
					if (array[2] || array[4])
					{
					}
					if (array[1] && array[3] && array[5])
					{
						m_FloorHighLights[2] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[4] = HightLightFloorTypeEnum.NoTile;
					}
					else if (array[1] && array[3] && !array[5])
					{
						m_FloorHighLights[2] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[4] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
					}
					if (!array[1] || array[3] || array[5])
					{
					}
					if (!array[1] && array[3] && array[5])
					{
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						m_FloorHighLights[4] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
					}
				}
				else if (flag2)
				{
					switch (m_eCurrentLayer)
					{
					case BaseLevelManager.LevelLayers.GroundFloor_Vent:
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor:
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor_Vent:
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						break;
					case BaseLevelManager.LevelLayers.Roof:
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						break;
					}
				}
				else
				{
					switch (m_eCurrentLayer)
					{
					case BaseLevelManager.LevelLayers.GroundFloor:
						m_FloorHighLights[2] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[3] = HightLightFloorTypeEnum.NoTile;
						break;
					case BaseLevelManager.LevelLayers.GroundFloor_Vent:
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						m_FloorHighLights[3] = HightLightFloorTypeEnum.NoTile;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor:
						m_FloorHighLights[1] = HightLightFloorTypeEnum.Inside;
						m_FloorHighLights[4] = HightLightFloorTypeEnum.NoTile;
						m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
						break;
					case BaseLevelManager.LevelLayers.FirstFloor_Vent:
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						m_FloorHighLights[5] = HightLightFloorTypeEnum.NoTile;
						break;
					case BaseLevelManager.LevelLayers.Roof:
						m_FloorHighLights[3] = HightLightFloorTypeEnum.Inside;
						break;
					}
				}
				break;
			}
			}
		}
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = m_LevelManager.m_VentLayers[(uint)m_eCurrentLayer];
		if (flag6 && baseBuildingBlock != null && baseBuildingBlock.BlockType != BaseBuildingBlock.BuildingBlockType.Tile && baseBuildingBlock.BlockType != BaseBuildingBlock.BuildingBlockType.Complex)
		{
			flag6 = false;
		}
		for (int num5 = m_HighLighters.Length - 1; num5 >= 0; num5--)
		{
			if (m_HighLighters[num5] != null)
			{
				flag5 = m_HighLighters[num5].UpdateLook(flag6);
				m_Map[num5] = !flag5;
				flag4 = flag4 || flag5;
			}
		}
		CreateBorder(flag4);
	}

	private void CreateBorder(bool bMake)
	{
		if (m_LevelManager == null || m_MasterObject == null)
		{
			return;
		}
		if (!bMake)
		{
			if (m_BorderObject != null)
			{
				UnityEngine.Object.Destroy(m_BorderObject);
				m_BorderObject = null;
			}
			return;
		}
		if (m_BorderObject != null)
		{
			UnityEngine.Object.Destroy(m_BorderObject);
			m_BorderObject = null;
		}
		m_BorderObject = new GameObject("Border");
		m_BorderObject.transform.parent = m_MasterObject.transform;
		m_BorderObject.transform.localScale = Vector3.one;
		m_BorderObject.transform.localPosition = Vector3.zero;
		LevelEditorBorderElement.CreateBorderPiecesFromMap(m_BorderObject.transform, ref m_Map, null, 120, 0, 0, LevelEditorBorderElement.BorderState.Blackout);
	}

	private void CreateHighLights()
	{
		m_LevelManager = BaseLevelManager.GetInstance();
		if (m_LevelManager == null)
		{
			base.enabled = false;
		}
		else
		{
			if (m_MasterObject != null)
			{
				return;
			}
			m_MasterObject = new GameObject();
			m_MasterObject.SetActive(value: false);
			m_MasterObject.transform.localScale = Vector3.one;
			m_MasterObject.transform.parent = m_MasterLayers[(uint)m_eCurrentLayer].transform;
			m_MasterObject.transform.localPosition = new Vector3(-59.5f, -59.5f, -9f);
			int num = 0;
			for (int i = 0; i < 118; i++)
			{
				for (int j = 0; j < 120; j++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(m_HighlightPrefab, m_MasterObject.transform);
					if (gameObject != null)
					{
						LevelEditorTileHighlight component = gameObject.GetComponent<LevelEditorTileHighlight>();
						if (component != null)
						{
							if (component.Setup(m_LevelManager, this, j, i))
							{
								m_HighLighters[num] = component;
							}
							else
							{
								m_HighLighters[num] = null;
							}
						}
						gameObject.transform.localPosition = new Vector3(j, i, 0f);
					}
					num++;
				}
			}
			m_MasterObject.SetActive(value: true);
			if (m_LowerLevelPrefab != null)
			{
				m_LowerLevelObject = UnityEngine.Object.Instantiate(m_LowerLevelPrefab, base.transform);
				if (m_LowerLevelObject != null)
				{
					m_LowerLevelObject.SetActive(value: false);
					m_LowerLevelObject.transform.localPosition = new Vector3(0f, -1f, 0f);
					m_LowerLevelObject.transform.localScale = new Vector3(120f, 118f, 1f);
				}
			}
		}
	}

	public bool IsSetupCorrectly()
	{
		if (m_MasterLayers.Length != 6)
		{
			return false;
		}
		for (int num = m_MasterLayers.Length - 1; num >= 0; num--)
		{
			if (m_MasterLayers[num] == null)
			{
				return false;
			}
		}
		if (m_HighlightPrefab == null)
		{
			return false;
		}
		if (m_LevelBase == null)
		{
			return false;
		}
		if (m_LowerLevelPrefab == null)
		{
			return false;
		}
		return true;
	}

	public bool IsOkToDrawOn(int iX, int iY)
	{
		int num = iY * 120 + iX;
		if (m_HighLighters[num] != null)
		{
			return m_HighLighters[num].IsValid();
		}
		return false;
	}

	public void RequestRescan()
	{
		m_bHightRescanPending = true;
	}

	public BaseLevelManager.BrushError GetErrorForTile(int iX, int iY)
	{
		int num = iY * 120 + iX;
		if (m_HighLighters[num] != null)
		{
			return m_HighLighters[num].GetError();
		}
		return BaseLevelManager.BrushError.eInvalid;
	}
}
