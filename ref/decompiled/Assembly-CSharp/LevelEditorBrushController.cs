using System.Collections.Generic;
using UnityEngine;

public class LevelEditorBrushController : MonoBehaviour
{
	public GameObject m_ElementPrefab;

	public GameObject m_VisualRep;

	private bool m_ElementVisability = true;

	public List<LevelEditorBrushElement> m_Elements = new List<LevelEditorBrushElement>();

	public List<LevelEditorBorderElement> m_BorderElements = new List<LevelEditorBorderElement>();

	public void Setup(int iBlockID)
	{
		if (m_ElementPrefab == null)
		{
			return;
		}
		m_Elements.Clear();
		LevelEditorBrushElement.EnviromentalLocation location = LevelEditorBrushElement.EnviromentalLocation.DoesntCare;
		BaseBuildingBlock block = BuildingBlockManager.GetBlock(iBlockID);
		if (block != null && block.m_Footprint != null)
		{
			if (block.BlockType != BaseBuildingBlock.BuildingBlockType.Object && block.BlockType != BaseBuildingBlock.BuildingBlockType.Decoration)
			{
				location = (((block.m_ValidLayers & 0x555) == 0) ? LevelEditorBrushElement.EnviromentalLocation.OutsideBlock : LevelEditorBrushElement.EnviromentalLocation.InsideBlock);
			}
			float num = block.m_Footprint.m_iLeft;
			float num2 = block.m_Footprint.m_iBottom;
			int num3 = block.m_Footprint.m_iH * block.m_Footprint.m_iW;
			int num4 = 0;
			bool requiresClearence = block.m_RequiresClearence;
			for (int i = 0; i < block.m_Footprint.m_iH; i++)
			{
				for (int j = 0; j < block.m_Footprint.m_iW; j++)
				{
					bool bRoomWallBlock = false;
					bool flag = false;
					BaseLevelManager.TileProperty[] array = new BaseLevelManager.TileProperty[6];
					bool bCheckForZone = false;
					if (block.m_Footprint.m_bMultiLevel)
					{
						for (int k = 1; k < 6; k++)
						{
							Footprint.BlockTypes blockTypes = block.m_Footprint.m_UsedTiles[num4 + num3 * k];
							if (blockTypes == Footprint.BlockTypes.None)
							{
								continue;
							}
							array[k] = GetProp(blockTypes, block.BlockType);
							if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Room || block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
							{
								int num5;
								BaseLevelManager.TileProperty[] array2;
								(array2 = array)[num5 = k] = array2[num5] | BaseLevelManager.TileProperty.RoomMask;
								if ((blockTypes & Footprint.BlockTypes.SolidWall) != 0)
								{
									int num6;
									(array2 = array)[num6 = k] = array2[num6] | BaseLevelManager.TileProperty.WallInRoomMask;
								}
							}
							flag = true;
						}
					}
					else
					{
						Footprint.BlockTypes blockTypes2 = block.m_Footprint.m_UsedTiles[num4];
						if (blockTypes2 != 0)
						{
							array[0] = GetProp(blockTypes2, block.BlockType);
							if (block.BlockType == BaseBuildingBlock.BuildingBlockType.Room || block.BlockType == BaseBuildingBlock.BuildingBlockType.Complex)
							{
								BaseLevelManager.TileProperty[] array2;
								(array2 = array)[0] = array2[0] | BaseLevelManager.TileProperty.RoomMask;
								if ((blockTypes2 & Footprint.BlockTypes.SolidWall) != 0)
								{
									(array2 = array)[0] = array2[0] | BaseLevelManager.TileProperty.WallInRoomMask;
								}
							}
							if ((blockTypes2 & Footprint.BlockTypes.Zone) == Footprint.BlockTypes.Zone)
							{
								bCheckForZone = true;
							}
							flag = true;
						}
					}
					if (flag)
					{
						GameObject gameObject = Object.Instantiate(m_ElementPrefab, base.transform);
						if (gameObject != null)
						{
							gameObject.transform.localPosition = new Vector3(num + (float)j, num2 + (float)i, -30f);
							gameObject.SetActive(value: true);
							m_Elements.Add(gameObject.GetComponent<LevelEditorBrushElement>());
							if (m_Elements[m_Elements.Count - 1] != null)
							{
								if (block.m_Footprint.m_bMultiLevel)
								{
									m_Elements[m_Elements.Count - 1].SetPropertiesToCheckInLayers(array, location);
								}
								else
								{
									m_Elements[m_Elements.Count - 1].SetPropertiesToCheck(array[0], location, bCheckForZone);
								}
								m_Elements[m_Elements.Count - 1].SetAttributes(bRoomWallBlock, requiresClearence);
							}
						}
					}
					num4++;
				}
			}
		}
		m_BorderElements.Clear();
		LevelEditorBorderElement.CreateBorderPieces(base.transform, ref block.m_Footprint, m_BorderElements);
	}

	private BaseLevelManager.TileProperty GetProp(Footprint.BlockTypes types, BaseBuildingBlock.BuildingBlockType blockType)
	{
		BaseLevelManager.TileProperty tileProperty = BaseLevelManager.TileProperty.EMPTY;
		switch (blockType)
		{
		case BaseBuildingBlock.BuildingBlockType.Tile:
			tileProperty = BaseLevelManager.TileProperty.WallMask;
			break;
		case BaseBuildingBlock.BuildingBlockType.Wall:
			tileProperty = BaseLevelManager.TileProperty.ObjDecMask;
			break;
		case BaseBuildingBlock.BuildingBlockType.Decoration:
		case BaseBuildingBlock.BuildingBlockType.Object:
			tileProperty = BaseLevelManager.TileProperty.WallAndObjects;
			break;
		case BaseBuildingBlock.BuildingBlockType.Room:
			tileProperty = BaseLevelManager.TileProperty.WallAndObjects;
			break;
		case BaseBuildingBlock.BuildingBlockType.Complex:
			tileProperty = BaseLevelManager.TileProperty.WallAndObjects;
			break;
		}
		if ((types & Footprint.BlockTypes.Blocking) == Footprint.BlockTypes.Blocking)
		{
			tileProperty |= BaseLevelManager.TileProperty.BlockingMask;
		}
		if ((types & Footprint.BlockTypes.NoBlockingBelow) == Footprint.BlockTypes.NoBlockingBelow)
		{
			tileProperty |= BaseLevelManager.TileProperty.NoBlockingMask;
		}
		return tileProperty;
	}

	public bool AreWeValid()
	{
		bool flag = true;
		for (int num = m_Elements.Count - 1; num >= 0; num--)
		{
			if (m_Elements[num] != null && !m_Elements[num].AreWeValid())
			{
				flag = false;
				break;
			}
		}
		for (int num2 = m_Elements.Count - 1; num2 >= 0; num2--)
		{
			if (m_Elements[num2] != null)
			{
				m_Elements[num2].SetExternalValidation(flag);
			}
		}
		return flag;
	}

	public void ValidateElements(bool bOutOfStock = false, bool bForceUpdate = false)
	{
		bool flag = true;
		for (int num = m_Elements.Count - 1; num >= 0; num--)
		{
			if (m_Elements[num] != null)
			{
				flag &= m_Elements[num].ValidateElement(bOutOfStock, bForceUpdate);
			}
		}
		for (int num2 = m_Elements.Count - 1; num2 >= 0; num2--)
		{
			if (m_Elements[num2] != null)
			{
				m_Elements[num2].SetExternalValidation(flag);
			}
		}
		for (int num3 = m_BorderElements.Count - 1; num3 >= 0; num3--)
		{
			if (m_BorderElements[num3] != null)
			{
				if (flag)
				{
					m_BorderElements[num3].SetState(LevelEditorBorderElement.BorderState.Freeze);
				}
				else
				{
					m_BorderElements[num3].SetState(LevelEditorBorderElement.BorderState.Red);
				}
			}
		}
	}

	public void SetElementVisability(bool bVisible)
	{
		if (m_ElementVisability == bVisible)
		{
			return;
		}
		m_ElementVisability = bVisible;
		for (int num = m_Elements.Count - 1; num >= 0; num--)
		{
			if (m_Elements[num] != null)
			{
				m_Elements[num].gameObject.SetActive(m_ElementVisability);
			}
		}
		for (int num2 = m_BorderElements.Count - 1; num2 >= 0; num2--)
		{
			if (m_BorderElements[num2] != null)
			{
				m_BorderElements[num2].gameObject.SetActive(m_ElementVisability);
			}
		}
	}

	public BaseLevelManager.BrushError GetAllErrors()
	{
		BaseLevelManager.BrushError brushError = BaseLevelManager.BrushError.eNone;
		for (int i = 0; i < m_Elements.Count; i++)
		{
			brushError |= m_Elements[i].GetCurrentError();
		}
		return brushError;
	}
}
