using System;
using UnityEngine;

public class BuildingBlock_TMS : BuildingBlock_Single
{
	public enum TileSurround
	{
		TopLeft = 0,
		Top = 1,
		TopRight = 2,
		Left = 3,
		Right = 4,
		BottomLeft = 5,
		Bottom = 6,
		BottomRight = 7,
		TOTAL = 8,
		Wall_TopLeft = 8,
		Wall_Top = 9,
		Wall_TopRight = 10,
		Wall_Left = 11,
		Wall_Right = 12,
		Wall_BottomLeft = 13,
		Wall_Bottom = 14,
		Wall_BottomRight = 15,
		Object_TopLeft = 16,
		Object_Top = 17,
		Object_TopRight = 18,
		Object_Left = 19,
		Object_Right = 20,
		Object_BottomLeft = 21,
		Object_Bottom = 22,
		Object_BottomRight = 23,
		Group_TopLeft = 24,
		Group_Top = 25,
		Group_TopRight = 26,
		Group_Left = 27,
		Group_Right = 28,
		Group_BottomLeft = 29,
		Group_Bottom = 30,
		Group_BottomRight = 31,
		ALL_TOTAL = 32
	}

	public enum TMSPriority
	{
		Low,
		_1,
		_2,
		_3,
		_4,
		_5,
		_6,
		_7,
		_8,
		_9,
		High
	}

	[Serializable]
	public struct TMSEntry
	{
		public int[] m_SurroundingBlocks;

		public int[] m_SurroundingBlocksOrigin;

		public sbyte m_TotalSurroundingBlocks;

		public sbyte m_TotalSurroundingBlocksAlien;

		public sbyte m_TotalSurroundingBlocksGroups;

		public Material[] m_Materials;

		public Material[] m_DamagedMaterial;

		public Material m_FacadeMaterial;

		public Texture2D m_MapGraphic;

		public bool m_Damagable;

		public bool m_Valid;

		public bool m_DefaultEntry;

		public UnityEngine.Object m_Prefab;

		public UnityEngine.Object m_VisualPrefab;

		public TMSPriority m_Priority;

		public void Init()
		{
			m_SurroundingBlocks = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
			m_TotalSurroundingBlocks = 0;
			m_TotalSurroundingBlocksAlien = 0;
			m_TotalSurroundingBlocksGroups = 0;
			m_SurroundingBlocksOrigin = new int[8];
			m_Damagable = false;
			m_Valid = false;
			m_Materials = new Material[0];
			m_DamagedMaterial = new Material[0];
			m_FacadeMaterial = null;
			m_DefaultEntry = false;
			m_Priority = TMSPriority._5;
		}
	}

	public TMSEntry[] m_TMSEntries = new TMSEntry[0];

	public int m_DefaultEntry = -1;

	protected int m_Processing_TMSIndex = -1;

	protected int m_Processing_MatIndex = -1;

	public override CompletionState GetBlockCompletionState(ref string strProblems, bool bCreateErrorString = false)
	{
		CompletionState completionState = base.GetBlockCompletionState(ref strProblems, bCreateErrorString);
		if (m_TMSEntries == null || m_TMSEntries.Length == 0)
		{
			if (bCreateErrorString)
			{
				strProblems += "No TMS entries found\n";
			}
			completionState = CompletionState.Unfinished;
		}
		else
		{
			int num = m_TMSEntries.Length;
			for (int i = 0; i < num; i++)
			{
				int num2 = IsTMSDuplicated(i);
				if (num2 >= 0)
				{
					if (bCreateErrorString)
					{
						string text = strProblems;
						strProblems = text + "TMS(" + i + ") is a duplicate of " + num2 + "\n";
					}
					completionState = CompletionState.Unfinished;
				}
			}
			for (int j = 0; j < num; j++)
			{
				if (m_TMSEntries[j].m_Materials == null || m_TMSEntries[j].m_Materials.Length == 0)
				{
					if (bCreateErrorString)
					{
						strProblems = strProblems + "TMS(" + j + ") has no materials\n";
					}
					completionState = CompletionState.Unfinished;
					continue;
				}
				bool flag = false;
				int num3 = m_TMSEntries[j].m_Materials.Length;
				for (int k = 0; k < num3; k++)
				{
					if (flag)
					{
						break;
					}
					if (!(m_TMSEntries[j].m_Materials[k] == null))
					{
						continue;
					}
					if (bCreateErrorString)
					{
						strProblems = strProblems + "TMS(" + j + ") there is a material missing\n";
						if (completionState != CompletionState.Unfinished)
						{
							completionState = CompletionState.Nearly_Complete;
						}
					}
					completionState = CompletionState.Unfinished;
					flag = true;
				}
			}
		}
		return completionState;
	}

	public Material GetVersionMaterial(int iIndex)
	{
		if (m_TMSEntries != null)
		{
			int num = m_TMSEntries.Length;
			for (int i = 0; i < num; i++)
			{
				if (m_TMSEntries[i].m_Materials != null)
				{
					int num2 = m_TMSEntries[i].m_Materials.Length;
					if (num2 > iIndex)
					{
						return m_TMSEntries[i].m_Materials[iIndex];
					}
					iIndex -= num2;
				}
				else
				{
					if (iIndex == 0)
					{
						return null;
					}
					iIndex--;
				}
			}
		}
		return null;
	}

	public override int GetNumberOfVersionsRequired()
	{
		if (m_TMSEntries == null)
		{
			return base.GetNumberOfVersionsRequired();
		}
		int num = 0;
		if (m_TMSEntries.Length == 0)
		{
			num = 1;
		}
		else
		{
			for (int num2 = m_TMSEntries.Length - 1; num2 >= 0; num2--)
			{
				num = ((m_TMSEntries[num2].m_Materials != null) ? (num + m_TMSEntries[num2].m_Materials.Length) : (num + 1));
			}
		}
		return num;
	}

	public void GetEntryIndexsFromVersion(int iVersionIndex, ref int iTMSIndex, ref int iMatIndex)
	{
		iTMSIndex = -1;
		iMatIndex = -1;
		if (m_TMSEntries == null)
		{
			return;
		}
		int num = m_TMSEntries.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_TMSEntries[i].m_Materials != null)
			{
				int num2 = m_TMSEntries[i].m_Materials.Length;
				if (num2 > iVersionIndex)
				{
					iTMSIndex = i;
					iMatIndex = iVersionIndex;
					break;
				}
				iVersionIndex -= num2;
			}
			else
			{
				if (iVersionIndex == 0)
				{
					iTMSIndex = i;
					break;
				}
				iVersionIndex--;
			}
		}
	}

	public override void HouseKeeping()
	{
		base.HouseKeeping();
		int num = m_TMSEntries.Length;
		for (int i = 0; i < num; i++)
		{
			UpdateSurroundingBlocksOrigin(i);
		}
		for (int j = 0; j < num - 1; j++)
		{
			if (m_TMSEntries[j].m_Priority >= m_TMSEntries[j + 1].m_Priority && m_TMSEntries[j].m_TotalSurroundingBlocks >= m_TMSEntries[j + 1].m_TotalSurroundingBlocks && (m_TMSEntries[j].m_TotalSurroundingBlocks != m_TMSEntries[j + 1].m_TotalSurroundingBlocks || m_TMSEntries[j].m_TotalSurroundingBlocksAlien >= m_TMSEntries[j + 1].m_TotalSurroundingBlocksAlien) && (m_TMSEntries[j].m_TotalSurroundingBlocks != m_TMSEntries[j + 1].m_TotalSurroundingBlocks || m_TMSEntries[j].m_TotalSurroundingBlocksGroups >= m_TMSEntries[j + 1].m_TotalSurroundingBlocksGroups))
			{
				continue;
			}
			Array.Sort(m_TMSEntries, delegate(TMSEntry Entry1, TMSEntry Entry2)
			{
				int num2 = Mathf.Clamp(Entry2.m_Priority - Entry1.m_Priority, -1, 1);
				if (num2 == 0)
				{
					num2 = Mathf.Clamp(Entry2.m_TotalSurroundingBlocks - Entry1.m_TotalSurroundingBlocks, -1, 1);
					if (num2 == 0)
					{
						num2 = Mathf.Clamp(Entry2.m_TotalSurroundingBlocksAlien + Entry2.m_TotalSurroundingBlocksGroups - (Entry1.m_TotalSurroundingBlocksAlien + Entry1.m_TotalSurroundingBlocksGroups), -1, 1);
						if (num2 == 0)
						{
							num2 = Mathf.Clamp(Entry2.m_TotalSurroundingBlocksAlien - Entry1.m_TotalSurroundingBlocksAlien, -1, 1);
							if (num2 == 0)
							{
								num2 = Mathf.Clamp(Entry2.m_TotalSurroundingBlocksGroups - Entry1.m_TotalSurroundingBlocksGroups, -1, 1);
							}
						}
					}
				}
				return num2;
			});
			break;
		}
		m_DefaultEntry = -1;
		for (int k = 0; k < num; k++)
		{
			if (m_TMSEntries[k].m_DefaultEntry)
			{
				if (m_DefaultEntry == -1)
				{
					m_DefaultEntry = k;
				}
				else
				{
					m_TMSEntries[k].m_DefaultEntry = false;
				}
			}
		}
		if (m_DefaultEntry == -1 && num > 0)
		{
			m_DefaultEntry = 0;
			m_TMSEntries[0].m_DefaultEntry = true;
		}
	}

	public void UpdateSurroundingBlocksOrigin(int iIndex)
	{
		int num = 8;
		m_TMSEntries[iIndex].m_TotalSurroundingBlocks = 0;
		m_TMSEntries[iIndex].m_TotalSurroundingBlocksAlien = 0;
		m_TMSEntries[iIndex].m_TotalSurroundingBlocksGroups = 0;
		if (m_TMSEntries[iIndex].m_SurroundingBlocksOrigin.Length != num)
		{
			m_TMSEntries[iIndex].m_SurroundingBlocksOrigin = new int[num];
		}
		for (int i = 0; i < num; i++)
		{
			if (m_TMSEntries[iIndex].m_SurroundingBlocks[i] == -1)
			{
				continue;
			}
			BuildingBlockType buildingBlockType = BuildingBlockType.Tile;
			if (m_TMSEntries[iIndex].m_SurroundingBlocks[i] <= -18 && m_TMSEntries[iIndex].m_SurroundingBlocks[i] >= -49)
			{
				buildingBlockType = BuildingBlockType.Complex;
				ref TMSEntry reference = ref m_TMSEntries[iIndex];
				reference.m_TotalSurroundingBlocksGroups++;
			}
			else
			{
				switch (m_TMSEntries[iIndex].m_SurroundingBlocks[i])
				{
				case -8:
				case -5:
				{
					buildingBlockType = BuildingBlockType.Wall;
					ref TMSEntry reference3 = ref m_TMSEntries[iIndex];
					reference3.m_TotalSurroundingBlocksAlien++;
					break;
				}
				case -17:
				{
					buildingBlockType = BuildingBlockType.Tile;
					ref TMSEntry reference4 = ref m_TMSEntries[iIndex];
					reference4.m_TotalSurroundingBlocksAlien++;
					break;
				}
				default:
				{
					BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_TMSEntries[iIndex].m_SurroundingBlocks[i]);
					if (block == null)
					{
						m_TMSEntries[iIndex].m_SurroundingBlocks[i] = -1;
						continue;
					}
					buildingBlockType = block.BlockType;
					if (m_ID != block.m_ID)
					{
						ref TMSEntry reference2 = ref m_TMSEntries[iIndex];
						reference2.m_TotalSurroundingBlocksAlien++;
					}
					break;
				}
				}
			}
			ref TMSEntry reference5 = ref m_TMSEntries[iIndex];
			reference5.m_TotalSurroundingBlocks++;
			switch (buildingBlockType)
			{
			case BuildingBlockType.Tile:
				m_TMSEntries[iIndex].m_SurroundingBlocksOrigin[i] = i;
				break;
			case BuildingBlockType.Wall:
				m_TMSEntries[iIndex].m_SurroundingBlocksOrigin[i] = i + num;
				break;
			case BuildingBlockType.Decoration:
			case BuildingBlockType.Object:
				m_TMSEntries[iIndex].m_SurroundingBlocksOrigin[i] = i + num + num;
				break;
			case BuildingBlockType.Complex:
				m_TMSEntries[iIndex].m_SurroundingBlocksOrigin[i] = i + num + num + num;
				break;
			}
		}
	}

	public override void MakeActualObject(int iIndex)
	{
		base.MakeActualObject(iIndex);
		GetEntryIndexsFromVersion(iIndex, ref m_Processing_TMSIndex, ref m_Processing_MatIndex);
		TextureStamp[] array = m_RealObjects[iIndex].GetComponentsInChildren<TextureStamp>(includeInactive: true);
		if (array.Length > 1)
		{
		}
		bool flag = true;
		if (m_Processing_TMSIndex == -1 || m_TMSEntries[m_Processing_TMSIndex].m_MapGraphic == null)
		{
			flag = false;
		}
		if (!flag)
		{
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(array[i]);
			}
			return;
		}
		if (array.Length == 0)
		{
			array = new TextureStamp[1] { m_RealObjects[iIndex].AddComponent<TextureStamp>() };
		}
		for (int j = 0; j < array.Length; j++)
		{
			array[j].m_Stamp = m_TMSEntries[m_Processing_TMSIndex].m_MapGraphic;
		}
	}

	public override UnityEngine.Object GetPrefab(int iIndex, bool bVisual = false)
	{
		int iTMSIndex = 0;
		int iMatIndex = 0;
		GetEntryIndexsFromVersion(iIndex, ref iTMSIndex, ref iMatIndex);
		if (iTMSIndex < 0 || m_TMSEntries.Length <= iTMSIndex)
		{
			return base.GetPrefab(iIndex);
		}
		if (bVisual && m_TMSEntries[iTMSIndex].m_VisualPrefab != null)
		{
			return m_TMSEntries[iTMSIndex].m_VisualPrefab;
		}
		if (m_TMSEntries[iTMSIndex].m_Prefab != null)
		{
			return m_TMSEntries[iTMSIndex].m_Prefab;
		}
		return base.GetPrefab(iIndex);
	}

	protected override void ProcessComponent(GameObject masterGameObject, Component comp, Type compType, ref bool bKeep, ref bool bClear, int iVersionIndex = 0)
	{
		bool flag = false;
		if (compType == typeof(Renderer) || compType.IsSubclassOf(typeof(Renderer)))
		{
			MeshRenderer meshRenderer = comp as MeshRenderer;
			if (!(meshRenderer == null) && comp.gameObject == masterGameObject)
			{
				Material versionMaterial = GetVersionMaterial(iVersionIndex);
				if (versionMaterial != null)
				{
					flag = true;
					meshRenderer.material = versionMaterial;
				}
				bClear = true;
				bKeep = true;
			}
		}
		if (compType == typeof(ThisIsMyRenderer) || compType.IsSubclassOf(typeof(ThisIsMyRenderer)))
		{
			flag = true;
			bClear = true;
			bKeep = false;
			ThisIsMyRenderer thisIsMyRenderer = comp as ThisIsMyRenderer;
			if (thisIsMyRenderer != null && thisIsMyRenderer.m_Renderer != null)
			{
				Material versionMaterial2 = GetVersionMaterial(iVersionIndex);
				if (versionMaterial2 != null)
				{
					thisIsMyRenderer.m_Renderer.material = versionMaterial2;
				}
			}
		}
		if (!flag)
		{
			base.ProcessComponent(masterGameObject, comp, compType, ref bKeep, ref bClear, iVersionIndex);
		}
	}

	public int IsTMSDuplicated(int iTMSIndex)
	{
		int num = m_TMSEntries.Length;
		for (int i = 0; i < num; i++)
		{
			if (i == iTMSIndex)
			{
				continue;
			}
			bool flag = true;
			int num2 = m_TMSEntries[iTMSIndex].m_SurroundingBlocks.Length;
			for (int j = 0; j < num2; j++)
			{
				if (m_TMSEntries[iTMSIndex].m_SurroundingBlocks[j] != m_TMSEntries[i].m_SurroundingBlocks[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetApplicableTMSVariant(int[] BlocksSurround, int seed, bool bAllowNoFloor = true)
	{
		int num = 0;
		int num2 = m_TMSEntries.Length;
		bool flag = true;
		if (num2 > 0)
		{
			for (int i = 0; i < num2; i++)
			{
				TMSEntry tMSEntry = m_TMSEntries[i];
				flag = true;
				for (int j = 0; j < 8; j++)
				{
					int num3 = tMSEntry.m_SurroundingBlocks[j];
					if (num3 == -1)
					{
						continue;
					}
					if (num3 < 0)
					{
						if (num3 <= -18 && num3 >= -49)
						{
							int num4 = 31 + (-49 - num3);
							int num5 = 1 << num4;
							flag = (BlocksSurround[tMSEntry.m_SurroundingBlocksOrigin[j]] & num5) != 0;
						}
						else
						{
							switch (num3)
							{
							case -5:
								flag = BlocksSurround[tMSEntry.m_SurroundingBlocksOrigin[j]] != -1;
								break;
							case -8:
								flag = BlocksSurround[tMSEntry.m_SurroundingBlocksOrigin[j]] == -1;
								break;
							case -17:
								flag = BlocksSurround[tMSEntry.m_SurroundingBlocksOrigin[j]] == -1 && bAllowNoFloor;
								break;
							}
						}
					}
					else
					{
						flag = num3 == BlocksSurround[tMSEntry.m_SurroundingBlocksOrigin[j]];
					}
					if (!flag)
					{
						num = ((tMSEntry.m_Materials != null && tMSEntry.m_Materials.Length >= 2) ? (num + tMSEntry.m_Materials.Length) : (num + 1));
						break;
					}
				}
				if (flag)
				{
					if (tMSEntry.m_Materials == null || tMSEntry.m_Materials.Length < 2)
					{
						return num;
					}
					System.Random random = new System.Random(seed);
					return num + random.Next(tMSEntry.m_Materials.Length);
				}
			}
			return m_DefaultEntry;
		}
		return -1;
	}

	public override GameObject GetDefaultRepresentation()
	{
		int num = m_DefaultEntry;
		if (num == -1 || num >= m_TMSEntries.Length)
		{
			num = 0;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += Mathf.Max(m_TMSEntries[i].m_Materials.Length, 1);
		}
		return GetVisualRep(num2);
	}

	public override bool IsVariantDefault(int iVariant)
	{
		int num = m_DefaultEntry;
		if (num == -1 || num >= m_TMSEntries.Length)
		{
			num = 0;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += Mathf.Max(m_TMSEntries[i].m_Materials.Length, 1);
		}
		return iVariant == num2;
	}

	[ContextMenu("Check TMS")]
	public void CheckTMS()
	{
		for (int num = m_TMSEntries.Length - 1; num >= 0; num--)
		{
			if (m_TMSEntries[num].m_SurroundingBlocks != null && m_TMSEntries[num].m_SurroundingBlocks.Length == 8)
			{
				for (int i = 0; i < 8; i++)
				{
					if (m_TMSEntries[num].m_SurroundingBlocks[i] != -1)
					{
						BaseBuildingBlock block = BuildingBlockManager.GetBlock(m_TMSEntries[num].m_SurroundingBlocks[i]);
						if (!(block == null) && block.BlockType != BuildingBlockType.Complex && block.BlockType != BuildingBlockType.Room)
						{
						}
					}
				}
			}
		}
	}
}
