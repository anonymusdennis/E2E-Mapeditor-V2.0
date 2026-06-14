using UnityEngine;

public class LevelEditorTileHighlight : MonoBehaviour
{
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

	protected const int INVALIDINDEX = -1;

	private int[] m_iTileindexes = new int[9];

	private BaseLevelManager m_LevelManager;

	private bool m_bCurrentlyEnabled;

	public MeshRenderer m_MeshRender;

	private LevelEditorHighLightManager m_HighlightManager;

	private BaseLevelManager.BrushError m_CurrentError;

	public bool UpdateLook(bool bIncludeSurrounds = false)
	{
		int num = ((!bIncludeSurrounds) ? 1 : 9);
		m_CurrentError = BaseLevelManager.BrushError.eNone;
		BaseLevelManager.LevelLayers currentLayer = m_LevelManager.GetCurrentLayer();
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			if (flag)
			{
				break;
			}
			if (m_iTileindexes[i] == -1)
			{
				continue;
			}
			for (int j = 1; j < 6; j++)
			{
				LevelEditorHighLightManager.HightLightFloorTypeEnum hightLightFloorTypeEnum = m_HighlightManager.m_FloorHighLights[j];
				if (hightLightFloorTypeEnum == LevelEditorHighLightManager.HightLightFloorTypeEnum.DontCare)
				{
					continue;
				}
				BaseLevelManager.TileProperty tileProperty = m_LevelManager.m_BuildingLayers[j].m_TileProperties[m_iTileindexes[i]];
				switch (hightLightFloorTypeEnum)
				{
				case LevelEditorHighLightManager.HightLightFloorTypeEnum.Inside:
					if ((tileProperty & BaseLevelManager.TileProperty.TileExistsMask) != BaseLevelManager.TileProperty.TileExistsMask)
					{
						flag = true;
						if (j < (int)currentLayer)
						{
							m_CurrentError = BaseLevelManager.BrushError.eInsideBelowRequired;
						}
						else if (j > (int)currentLayer)
						{
							m_CurrentError = BaseLevelManager.BrushError.eInsideAboveRequired;
						}
						else
						{
							m_CurrentError = BaseLevelManager.BrushError.eInsideRequired;
						}
						break;
					}
					continue;
				case LevelEditorHighLightManager.HightLightFloorTypeEnum.Outside:
					if ((tileProperty & BaseLevelManager.TileProperty.TileExistsMask) != BaseLevelManager.TileProperty.TileMask)
					{
						flag = true;
						if (j < (int)currentLayer)
						{
							m_CurrentError = BaseLevelManager.BrushError.eOutsideBelowRequired;
						}
						else if (j > (int)currentLayer)
						{
							m_CurrentError = BaseLevelManager.BrushError.eOutsideAboveRequired;
						}
						else
						{
							m_CurrentError = BaseLevelManager.BrushError.eOutsideRequired;
						}
						break;
					}
					continue;
				case LevelEditorHighLightManager.HightLightFloorTypeEnum.Tile:
					if ((tileProperty & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
					{
						flag = true;
						m_CurrentError = BaseLevelManager.BrushError.eInvalid;
						break;
					}
					continue;
				case LevelEditorHighLightManager.HightLightFloorTypeEnum.NoTile:
					if ((tileProperty & BaseLevelManager.TileProperty.TileMask) != BaseLevelManager.TileProperty.TileMask)
					{
						continue;
					}
					flag = true;
					if (j < (int)currentLayer)
					{
						m_CurrentError = BaseLevelManager.BrushError.eBlockedBelow;
					}
					else if (j > (int)currentLayer)
					{
						m_CurrentError = BaseLevelManager.BrushError.eBlockedAbove;
					}
					else
					{
						m_CurrentError = BaseLevelManager.BrushError.eBlocked;
					}
					break;
				default:
					continue;
				}
				break;
			}
		}
		if (m_bCurrentlyEnabled != flag)
		{
			m_bCurrentlyEnabled = flag;
			m_MeshRender.enabled = m_bCurrentlyEnabled;
		}
		return flag;
	}

	public bool Setup(BaseLevelManager manager, LevelEditorHighLightManager highlightManager, int iX, int iY)
	{
		if (m_MeshRender == null)
		{
			m_MeshRender = GetComponent<MeshRenderer>();
			if (m_MeshRender == null)
			{
				return false;
			}
		}
		m_MeshRender.enabled = false;
		m_bCurrentlyEnabled = false;
		m_LevelManager = manager;
		m_HighlightManager = highlightManager;
		m_iTileindexes[0] = 120 * iY + iX;
		if (iY > 0)
		{
			m_iTileindexes[1] = m_iTileindexes[0] - 120;
			if (iX > 0)
			{
				m_iTileindexes[2] = m_iTileindexes[0] - 120 - 1;
			}
			else
			{
				m_iTileindexes[2] = -1;
			}
			if (iX < 119)
			{
				m_iTileindexes[3] = m_iTileindexes[0] - 120 + 1;
			}
			else
			{
				m_iTileindexes[3] = -1;
			}
		}
		else
		{
			m_iTileindexes[1] = -1;
			m_iTileindexes[2] = -1;
			m_iTileindexes[3] = -1;
		}
		if (iX > 0)
		{
			m_iTileindexes[4] = m_iTileindexes[0] - 1;
		}
		else
		{
			m_iTileindexes[4] = -1;
		}
		if (iX < 119)
		{
			m_iTileindexes[5] = m_iTileindexes[0] + 120 + 1;
		}
		else
		{
			m_iTileindexes[5] = -1;
		}
		if (iY < 117)
		{
			m_iTileindexes[6] = m_iTileindexes[0] + 120;
			if (iX > 0)
			{
				m_iTileindexes[7] = m_iTileindexes[0] + 120 - 1;
			}
			else
			{
				m_iTileindexes[7] = -1;
			}
			if (iX < 119)
			{
				m_iTileindexes[8] = m_iTileindexes[0] + 120 + 1;
			}
			else
			{
				m_iTileindexes[8] = -1;
			}
		}
		else
		{
			m_iTileindexes[6] = -1;
			m_iTileindexes[7] = -1;
			m_iTileindexes[8] = -1;
		}
		return true;
	}

	public bool IsValid()
	{
		return !m_bCurrentlyEnabled;
	}

	public BaseLevelManager.BrushError GetError()
	{
		return m_CurrentError;
	}
}
