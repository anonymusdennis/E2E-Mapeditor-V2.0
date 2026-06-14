using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditor_GridCellPopulator : MonoBehaviour
{
	public delegate void LevelEditor_GridCellPopulatorDelegate(LevelEditor_GridCellPopulator gridCellPopulator);

	public LevelEditor_GridCellPopulatorDelegate OnCellsUpdated;

	[SerializeField]
	private GameObject m_CellPrefab;

	[SerializeField]
	private LevelEditor_UIController.BuildingBlockCategory m_Category = LevelEditor_UIController.BuildingBlockCategory.Max;

	[SerializeField]
	private long m_Family;

	[NonSerialized]
	public List<BuildingBlock_UIButton> m_CellUIButtons = new List<BuildingBlock_UIButton>();

	private BaseLevelManager m_LevelMan;

	private BuildingBlockManager m_BlockMan;

	private BuildingBlock_FilterManager m_FilterMan;

	private bool m_bHasStarted;

	public long Family
	{
		get
		{
			return m_Family;
		}
		set
		{
			m_Family = value;
			if (Application.isPlaying)
			{
				UpdateCells();
			}
		}
	}

	private void Start()
	{
		if (!m_bHasStarted)
		{
			m_LevelMan = BaseLevelManager.GetInstance();
			m_BlockMan = BuildingBlockManager.GetInstance();
			m_FilterMan = BuildingBlock_FilterManager.GetInstance();
			if (m_FilterMan != null)
			{
				m_FilterMan.RegisterForRoomBlockSetChange(OnThemeChanged);
			}
			m_bHasStarted = true;
			UpdateCells();
		}
	}

	private void OnThemeChanged(BaseBuildingBlock.BlockSet newBlockSet, bool bNotify)
	{
		UpdateCells();
	}

	public void UpdateCells()
	{
		long iFamily = -1L;
		if (!m_bHasStarted)
		{
			Start();
		}
		if (m_BlockMan == null)
		{
			m_BlockMan = BuildingBlockManager.GetInstance();
		}
		if (m_LevelMan == null)
		{
			m_LevelMan = BaseLevelManager.GetInstance();
		}
		if (m_FilterMan == null)
		{
			m_FilterMan = BuildingBlock_FilterManager.GetInstance();
		}
		if (m_BlockMan == null || m_LevelMan == null)
		{
			return;
		}
		List<int> blockList = new List<int>();
		List<int> list = new List<int>();
		BaseBuildingBlock.BlockSet filterTheme = BaseBuildingBlock.BlockSet.ALL;
		BaseBuildingBlock.BlockSet filterTheme2 = BaseBuildingBlock.BlockSet.CentrePerks;
		m_FilterMan = BuildingBlock_FilterManager.GetInstance();
		if (m_FilterMan != null)
		{
			filterTheme = m_FilterMan.GetCurrentBlockSetFilter();
			filterTheme2 = m_FilterMan.GetCurrentRoomBlockSetFilter();
		}
		BaseLevelManager.LevelLayers currentLayer = m_LevelMan.GetCurrentLayer();
		if (m_Family != 0)
		{
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Tile, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Tile, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Wall, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Wall, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Room, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme2, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Room, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme2, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Decoration, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Decoration, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Object, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Object, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Complex, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
			m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Complex, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, m_Family);
		}
		else
		{
			switch (m_Category)
			{
			case LevelEditor_UIController.BuildingBlockCategory.Outside:
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Tile, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Wall, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				break;
			case LevelEditor_UIController.BuildingBlockCategory.Inside:
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Tile, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Wall, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				break;
			case LevelEditor_UIController.BuildingBlockCategory.Room:
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Room, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme2, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Room, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme2, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				break;
			case LevelEditor_UIController.BuildingBlockCategory.Object:
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Decoration, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Object, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Complex, currentLayer, BaseLevelManager.LayersEnvironment.Inside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Decoration, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Object, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				m_BlockMan.GetBlocksOfType(ref blockList, BaseBuildingBlock.BuildingBlockType.Complex, currentLayer, BaseLevelManager.LayersEnvironment.Outside, filterTheme, BaseBuildingBlock.PurposeGroups.ALL, iFamily);
				break;
			}
		}
		int count = blockList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!list.Contains(blockList[i]))
			{
				list.Add(blockList[i]);
			}
		}
		int j = 0;
		for (int count2 = m_CellUIButtons.Count; j < count2; j++)
		{
			m_CellUIButtons[j].SetBlockID(-1);
			m_CellUIButtons[j].gameObject.SetActive(value: false);
		}
		int count3 = list.Count;
		while (count3 > m_CellUIButtons.Count)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_CellPrefab, base.transform);
			if (gameObject != null)
			{
				gameObject.transform.localScale = Vector3.one;
				BuildingBlock_UIButton component = gameObject.GetComponent<BuildingBlock_UIButton>();
				if (component != null)
				{
					component.SetBlockID(-1);
					m_CellUIButtons.Add(component);
					gameObject.SetActive(value: false);
				}
			}
		}
		for (int k = 0; k < count3; k++)
		{
			BaseBuildingBlock block = BuildingBlockManager.GetBlock(list[k]);
			if (block != null)
			{
				m_CellUIButtons[k].SetBlockID(list[k]);
				m_CellUIButtons[k].gameObject.SetActive(value: true);
				m_CellUIButtons[k].LimitationGroupChanged(block.m_LimitationGroup);
			}
		}
		if (OnCellsUpdated != null)
		{
			OnCellsUpdated(this);
		}
	}

	public void AddFamily(int familyIndex)
	{
		m_Family &= familyIndex;
		UpdateCells();
	}

	public void RemoveFamily(int familyIndex)
	{
		m_Family &= 0xFFFFFFFFu ^ familyIndex;
		UpdateCells();
	}
}
