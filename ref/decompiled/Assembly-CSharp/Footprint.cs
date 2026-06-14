using System;
using UnityEngine;

[Serializable]
public class Footprint
{
	[Serializable]
	[Flags]
	public enum BlockTypes : byte
	{
		None = 0,
		Tiles = 1,
		Walls = 2,
		Objects = 4,
		Lights = 8,
		SolidWall = 0x10,
		All = 0x9F,
		NoBlockingBelow = 0x20,
		Blocking = 0x40,
		Zone = 0x80
	}

	public enum FoundState
	{
		NoHits,
		HitOnce,
		InAGap
	}

	[SerializeField]
	public int m_iLeft;

	[SerializeField]
	public int m_iBottom;

	[SerializeField]
	public int m_iW;

	[SerializeField]
	public int m_iH;

	[SerializeField]
	public bool m_bMultiLevel;

	[SerializeField]
	public BlockTypes[] m_UsedTiles;

	[SerializeField]
	public int m_iFirstX;

	[SerializeField]
	public int m_iFirstY;

	public Footprint()
	{
		m_iLeft = 0;
		m_iBottom = 0;
		m_bMultiLevel = false;
		InitUsedTiles(0, 0, BlockTypes.None);
	}

	public Footprint(BlockTypes mask, bool bMultiLevel = false)
	{
		m_iLeft = 0;
		m_iBottom = 0;
		m_bMultiLevel = bMultiLevel;
		InitUsedTiles(1, 1, mask);
	}

	public Footprint(Footprint footprint, int iXoffset, int iYoffset, bool bMultiLevel = false, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		m_iLeft = footprint.m_iLeft + iXoffset;
		m_iBottom = footprint.m_iBottom + iYoffset;
		m_iH = footprint.m_iH;
		m_iW = footprint.m_iW;
		m_bMultiLevel = bMultiLevel;
		int num = 0;
		int num2 = 0;
		int num3 = m_iW * m_iH;
		if (bMultiLevel)
		{
			m_UsedTiles = new BlockTypes[m_iW * m_iH * 6];
			num2 = 1;
			if (layer != BaseLevelManager.LevelLayers.TOTAL)
			{
				num2 = num3 * (int)layer;
			}
			if (footprint.m_bMultiLevel)
			{
				num = num2;
			}
		}
		else
		{
			m_UsedTiles = new BlockTypes[m_iW * m_iH];
		}
		for (int i = 0; i < num3; i++)
		{
			m_UsedTiles[num2 + i] = footprint.m_UsedTiles[num + i];
		}
		FindFirstObject();
	}

	public Footprint(int iWidth, int iHeight, BlockTypes mask, bool bMultiLevel = false)
	{
		m_iLeft = 0;
		m_iBottom = 0;
		m_bMultiLevel = bMultiLevel;
		InitUsedTiles(iWidth, iHeight, mask);
	}

	public Footprint(byte[] bits, int iWidth, int iHeight, BlockTypes mask)
	{
		m_iLeft = 0;
		m_iBottom = 0;
		m_bMultiLevel = false;
		InitUsedTiles(iWidth, iHeight, BlockTypes.None);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < iHeight; i++)
		{
			for (int j = 0; j < iWidth; j++)
			{
				if ((bits[num] & (1 << num2)) != 0)
				{
					m_UsedTiles[num3] = mask;
				}
				num3++;
				if (num2 == 7)
				{
					num2 = 0;
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
	}

	public Footprint(int iLeft, int iBottom, int iWidth, int iHeight, BlockTypes mask, bool bMultiLevel = false)
	{
		m_iLeft = iLeft;
		m_iBottom = iBottom;
		m_bMultiLevel = bMultiLevel;
		InitUsedTiles(iWidth, iHeight, mask);
	}

	public int GetWidth()
	{
		return m_iW;
	}

	public int GetHeight()
	{
		return m_iH;
	}

	private void InitUsedTiles(int iWidth, int iHeight, BlockTypes mask)
	{
		if (iWidth < 0)
		{
			iWidth = 0;
		}
		if (iHeight < 0)
		{
			iHeight = 0;
		}
		m_iW = iWidth;
		m_iH = iHeight;
		if (m_iW * m_iH > 0)
		{
			int num = m_iW * m_iH;
			if (m_bMultiLevel)
			{
				num *= 6;
			}
			m_UsedTiles = new BlockTypes[num];
			for (int i = 0; i < num; i++)
			{
				m_UsedTiles[i] = mask;
			}
		}
		else
		{
			m_UsedTiles = null;
		}
		FindFirstObject();
	}

	public virtual bool DoesFootprintOverlap(Footprint footprint)
	{
		return DoesAreaOverlap(footprint.m_iLeft, footprint.m_iBottom, footprint.m_iW, footprint.m_iH);
	}

	public virtual bool DoesAreaOverlap(int iLeft, int iBottom, int iWidth, int iHeight)
	{
		if (iLeft + iWidth <= m_iLeft || iLeft >= m_iLeft + m_iW)
		{
			return false;
		}
		if (iBottom >= m_iBottom + m_iH || iBottom + m_iH <= m_iBottom)
		{
			return false;
		}
		return true;
	}

	public void NormaliseFootPrint(bool bChangedLayer)
	{
		if (!m_bMultiLevel || bChangedLayer)
		{
			return;
		}
		int num = m_iW * m_iH;
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < 6; i++)
		{
			num3 = num * i;
			for (int j = 0; j < num; j++)
			{
				if (m_UsedTiles[num3++] != 0)
				{
					if (num2 != -1)
					{
						return;
					}
					num2 = i;
					break;
				}
			}
		}
		BlockTypes[] array = new BlockTypes[num];
		num3 = num2 * num;
		for (int k = 0; k < num; k++)
		{
			if (num2 == -1)
			{
				array[k] = BlockTypes.None;
			}
			else
			{
				array[k] = m_UsedTiles[num3++];
			}
		}
		m_UsedTiles = array;
		m_bMultiLevel = false;
	}

	public BlockTypes GetBlocksUsedAt(int iX, int iY, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		if (m_UsedTiles == null || iX < 0 || iX >= m_iW || iY < 0 || iY >= m_iH)
		{
			return BlockTypes.None;
		}
		int num = iX + iY * m_iW;
		if (layer != BaseLevelManager.LevelLayers.TOTAL && m_bMultiLevel)
		{
			num += m_iW * m_iH * (int)layer;
		}
		return m_UsedTiles[iX + iY * m_iW];
	}

	public bool AddAreaToFootprint(int iLeft, int iBottom, int iWidth, int iHeight, BlockTypes mask, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		if (iWidth == 0 || iHeight == 0)
		{
			return false;
		}
		bool result = ExpandFootprint(iLeft, iBottom, iWidth, iHeight);
		if (mask != 0)
		{
			int num = iLeft - m_iLeft;
			int num2 = iBottom - m_iBottom;
			int num3 = num + iWidth;
			int num4 = num3 + iHeight;
			int num5 = num2 * m_iW;
			if (layer != BaseLevelManager.LevelLayers.TOTAL && m_bMultiLevel)
			{
				num5 += m_iW * m_iH * (int)layer;
			}
			for (int i = num2; i < num4; i++)
			{
				for (int j = num; j < num3; j++)
				{
					m_UsedTiles[j + num5] = m_UsedTiles[j + num5] | mask;
				}
				num5 += m_iW;
			}
		}
		FindFirstObject();
		return result;
	}

	public bool CombineFootprints(int iOffsetX, int iOffsetY, Footprint footprint, BaseLevelManager.LevelLayers layer = BaseLevelManager.LevelLayers.TOTAL)
	{
		if (footprint == null)
		{
			return false;
		}
		if (footprint.m_bMultiLevel)
		{
			return false;
		}
		bool result = ExpandFootprint(footprint.m_iLeft + iOffsetX, footprint.m_iBottom + iOffsetY, footprint.m_iW, footprint.m_iH);
		int num = footprint.m_iLeft + iOffsetX - m_iLeft;
		int num2 = footprint.m_iBottom + iOffsetY - m_iBottom;
		int num3 = num2 * m_iW;
		if (m_bMultiLevel)
		{
			num3 = ((layer == BaseLevelManager.LevelLayers.TOTAL) ? (num3 + m_iW * m_iH) : (num3 + m_iW * m_iH * (int)layer));
		}
		int num4 = 0;
		for (int i = 0; i < footprint.m_iH; i++)
		{
			for (int j = 0; j < footprint.m_iW; j++)
			{
				m_UsedTiles[num + j + num3] = m_UsedTiles[num + j + num3] | footprint.m_UsedTiles[j + num4];
			}
			num3 += m_iW;
			num4 += footprint.m_iW;
		}
		FindFirstObject();
		return result;
	}

	private bool ExpandFootprint(int iLeft, int iBottom, int iWidth, int iHeight)
	{
		int num = Mathf.Min(iLeft, m_iLeft);
		int num2 = Mathf.Max(iLeft + iWidth, m_iLeft + m_iW);
		int num3 = Mathf.Min(iBottom, m_iBottom);
		int num4 = Mathf.Max(iBottom + iHeight, m_iBottom + m_iH);
		if (num != m_iLeft || num2 != m_iLeft + m_iW || num4 != m_iBottom + m_iH || num3 != m_iBottom)
		{
			int num5 = (num2 - num) * (num4 - num3);
			int num6 = ((!m_bMultiLevel) ? num5 : (num5 * 6));
			BlockTypes[] array = new BlockTypes[num6];
			int num7 = m_iLeft - num;
			int num8 = m_iBottom - num3;
			int num9 = 0;
			int num10 = num8 * (num2 - num);
			for (int i = 0; i < m_iH; i++)
			{
				for (int j = 0; j < m_iW; j++)
				{
					if (m_bMultiLevel)
					{
						for (int k = 1; k < 6; k++)
						{
							int num11 = k * num5;
							int num12 = k * (m_iH * m_iW);
							array[num7 + j + num10 + num11] = m_UsedTiles[j + num9 + num12];
						}
					}
					else
					{
						array[num7 + j + num10] = m_UsedTiles[j + num9];
					}
				}
				num10 += num2 - num;
				num9 += m_iW;
			}
			m_UsedTiles = array;
			m_iLeft = num;
			m_iW = num2 - num;
			m_iBottom = num3;
			m_iH = num4 - num3;
			FindFirstObject();
			return true;
		}
		return false;
	}

	private void FindFirstObject()
	{
		m_iFirstX = -1;
		m_iFirstY = -1;
		if (m_bMultiLevel)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < m_iH; i++)
		{
			for (int j = 0; j < m_iW; j++)
			{
				if ((m_UsedTiles[num++] & BlockTypes.Objects) == BlockTypes.Objects)
				{
					m_iFirstX = j;
					m_iFirstY = i;
					return;
				}
			}
		}
	}

	public bool FindEdges(ref int iLeft, ref int iRight, ref int iTop, ref int iBottom)
	{
		iLeft = 1000;
		iRight = -1000;
		iTop = -1000;
		iBottom = 1000;
		int num = 1;
		if (m_bMultiLevel)
		{
			num = 6;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < m_iH; j++)
			{
				for (int k = 0; k < m_iW; k++)
				{
					if ((m_UsedTiles[num2] & BlockTypes.Tiles) == BlockTypes.Tiles && (m_UsedTiles[num2] & BlockTypes.Walls) != BlockTypes.Walls)
					{
						if (k < iLeft)
						{
							iLeft = k;
						}
						if (k > iRight)
						{
							iRight = k;
						}
						if (j < iBottom)
						{
							iBottom = j;
						}
						if (j > iTop)
						{
							iTop = j;
						}
					}
					num2++;
				}
			}
		}
		if (iLeft == 1000 || iRight == -1000 || iTop == -1000 || iBottom == 1000)
		{
			return false;
		}
		return true;
	}

	public void CreateZonePrint(ref int iLeft, ref int iRight, ref int iTop, ref int iBottom, ref byte[] zonePrint)
	{
		iLeft = 0;
		iRight = 0;
		iTop = 0;
		iBottom = 0;
		if (!FindEdges(ref iLeft, ref iRight, ref iTop, ref iBottom))
		{
			return;
		}
		int num = iRight - iLeft + 1;
		int num2 = iTop - iBottom + 1;
		int num3 = (num * num2 + 7) / 8;
		zonePrint = new byte[num3];
		if (!m_bMultiLevel)
		{
			int num4 = 0;
			int num5 = 0;
			int num6 = iBottom * m_iW + iLeft;
			int num7 = m_iW - num;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					byte b = (byte)(1 << num4);
					if ((m_UsedTiles[num6] & BlockTypes.Tiles) == BlockTypes.Tiles && (m_UsedTiles[num6] & BlockTypes.Walls) != BlockTypes.Walls)
					{
						zonePrint[num5] |= b;
					}
					else
					{
						zonePrint[num5] &= (byte)(~b);
					}
					num6++;
					if (++num4 == 8)
					{
						num4 = 0;
						num5++;
					}
				}
				num6 += num7;
			}
		}
		iLeft += m_iLeft;
		iRight += m_iLeft;
		iBottom += m_iBottom;
		iTop += m_iBottom;
	}

	public int GetTotalTilesUsed()
	{
		int num = m_UsedTiles.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (m_UsedTiles[i] != 0)
			{
				num2++;
			}
		}
		return num2;
	}

	public bool GetPositionOfFirstOccupiedTile(ref int iX, ref int iY, BaseLevelManager.LevelLayers eLayer)
	{
		int num = 0;
		if (m_bMultiLevel)
		{
			num = m_iW * m_iH * (int)eLayer;
		}
		for (iY = 0; iY < m_iH; iY++)
		{
			for (iX = 0; iX < m_iW; iX++)
			{
				if (m_UsedTiles[num++] != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void DrawGizmo(Vector3 position)
	{
		if (m_UsedTiles == null || m_UsedTiles.Length == 0 || (m_iH * m_iW != m_UsedTiles.Length && m_iH * m_iW * 6 != m_UsedTiles.Length))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(position + new Vector3((float)m_iLeft - 0.5f, (float)m_iBottom + 0.5f, -10f), position + new Vector3((float)m_iLeft + 0.5f, (float)m_iBottom - 0.5f, -10f));
			Gizmos.DrawLine(position + new Vector3((float)m_iLeft - 0.5f, (float)m_iBottom - 0.5f, -10f), position + new Vector3((float)m_iLeft + 0.5f, (float)m_iBottom + 0.5f, -10f));
			return;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < m_iH; i++)
		{
			for (int j = 0; j < m_iW; j++)
			{
				DrawTileArea(position + new Vector3((float)(m_iLeft + j) + num, (float)(m_iBottom + i) + num2, -10f), j, i);
			}
		}
	}

	public void DrawTileArea(Vector3 position, int iX, int iY)
	{
		float num = 0.9f;
		float num2 = 4f;
		float num3 = 5f;
		float num4 = num / (num2 + 1f);
		float num5 = (num - num4 * num2) / (num2 - 1f);
		float num6 = num5 / 3f + num4;
		float num7 = num / (num3 + 1f);
		float num8 = (num - num7 * num3) / (num3 - 1f);
		float num9 = num8 / 3f + num7;
		Gizmos.color = Color.white;
		if (iX == m_iFirstX && iY == m_iFirstY)
		{
			Gizmos.color = Color.yellow;
		}
		Gizmos.DrawWireCube(position + new Vector3(0f, 0f, -1f), new Vector3(1f, 1f, 1f));
		int num10 = m_iH * m_iW;
		int num11 = 0;
		int num12 = 1;
		int num13 = iY * m_iW + iX;
		float num14 = num / 2f - num7 / 2f;
		float num15 = 0f - num / 2f + num4 / 2f;
		float num16 = num15;
		if (m_bMultiLevel)
		{
			num11 = 1;
			num12 = 6;
		}
		for (int i = num11; i < num12; i++)
		{
			int num17 = num13 + num10 * i;
			num15 = num16;
			BlockTypes blockTypes = m_UsedTiles[num17];
			if (blockTypes == BlockTypes.None)
			{
				num14 -= num7 + num8;
				continue;
			}
			if ((blockTypes & BlockTypes.Tiles) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6, num9, 1f));
				Gizmos.color = Color.white;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num4, num7, 1f));
			}
			if ((blockTypes & BlockTypes.Zone) != 0)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num4 / 2f, num7 / 2f, 1f));
			}
			num15 += num4 + num5;
			if ((blockTypes & BlockTypes.Walls) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6, num9, 1f));
				Gizmos.color = Color.red;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num4, num7, 1f));
			}
			if ((blockTypes & BlockTypes.SolidWall) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6 / 2f, num9 / 2f, 1f));
			}
			num15 += num4 + num5;
			if ((blockTypes & BlockTypes.Objects) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6, num9, 1f));
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num4, num7, 1f));
			}
			num15 += num4 + num5;
			if ((blockTypes & BlockTypes.NoBlockingBelow) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6, num9, 1f));
				Gizmos.color = Color.cyan;
				Gizmos.DrawCube(position + new Vector3(num15, num14 + num7 / 4f, -1f), new Vector3(num4, num7 / 2f, 1f));
			}
			if ((blockTypes & BlockTypes.Blocking) != 0)
			{
				Gizmos.color = Color.black;
				Gizmos.DrawCube(position + new Vector3(num15, num14, -1f), new Vector3(num6, num9, 1f));
				Gizmos.color = Color.yellow;
				Gizmos.DrawCube(position + new Vector3(num15, num14 - num7 / 4f, -1f), new Vector3(num4, num7 / 2f, 1f));
			}
			num14 -= num7 + num8;
		}
	}

	public bool AreThereSatelites()
	{
		int num = 1;
		int num2 = 6;
		if (!m_bMultiLevel)
		{
			num = 0;
			num2 = 1;
		}
		FoundState foundState = FoundState.NoHits;
		for (int i = num; i < num2; i++)
		{
			int num3 = m_iW * m_iH * i;
			foundState = FoundState.NoHits;
			for (int j = 0; j < m_iW; j++)
			{
				bool flag = false;
				for (int k = 0; k < m_iH; k++)
				{
					if (m_UsedTiles[num3 + k * m_iW + j] != 0)
					{
						flag = true;
						break;
					}
				}
				switch (foundState)
				{
				case FoundState.NoHits:
					if (flag)
					{
						foundState = FoundState.HitOnce;
					}
					break;
				case FoundState.HitOnce:
					if (!flag)
					{
						foundState = FoundState.InAGap;
					}
					break;
				case FoundState.InAGap:
					if (flag)
					{
						return true;
					}
					break;
				}
			}
			for (int l = 0; l < m_iH; l++)
			{
				bool flag2 = false;
				for (int m = 0; m < m_iW; m++)
				{
					if (m_UsedTiles[num3 + l * m_iW + m] != 0)
					{
						flag2 = true;
						break;
					}
				}
				switch (foundState)
				{
				case FoundState.NoHits:
					if (flag2)
					{
						foundState = FoundState.HitOnce;
					}
					break;
				case FoundState.HitOnce:
					if (!flag2)
					{
						foundState = FoundState.InAGap;
					}
					break;
				case FoundState.InAGap:
					if (flag2)
					{
						return true;
					}
					break;
				}
			}
		}
		return false;
	}
}
