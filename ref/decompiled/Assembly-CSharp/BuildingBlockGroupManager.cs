using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlockGroupManager : MonoBehaviour
{
	[Serializable]
	public class Group
	{
		public string m_GroupName = string.Empty;

		public string m_GroupDescription = string.Empty;

		public string m_GroupNameResource = string.Empty;

		public string m_TranslatedGroupName = string.Empty;

		public int m_NameHashcode;

		public int[] m_Blocks = new int[0];

		public string GetTranslatedName()
		{
			if (string.IsNullOrEmpty(m_TranslatedGroupName))
			{
				Localization.Get(m_GroupNameResource, out m_TranslatedGroupName);
			}
			return m_TranslatedGroupName;
		}
	}

	private static BuildingBlockGroupManager m_Instance;

	public const int INVALID_GROUP_ID = -1;

	public List<Group> m_Groups = new List<Group>();

	public static BuildingBlockGroupManager GetInstance()
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

	public Group GetGroupByName(string strName)
	{
		if (!string.IsNullOrEmpty(strName))
		{
			int hashCode = strName.GetHashCode();
			for (int num = m_Groups.Count - 1; num >= 0; num--)
			{
				if (m_Groups[num].m_NameHashcode == hashCode)
				{
					return m_Groups[num];
				}
			}
		}
		return null;
	}

	public Group GetGroupByIndex(int iIndex)
	{
		if (iIndex >= 0 && iIndex < m_Groups.Count)
		{
			return m_Groups[iIndex];
		}
		return null;
	}

	public int GetGroupIndexByName(string strName)
	{
		if (!string.IsNullOrEmpty(strName))
		{
			int hashCode = strName.GetHashCode();
			for (int num = m_Groups.Count - 1; num >= 0; num--)
			{
				if (m_Groups[num].m_NameHashcode == hashCode)
				{
					return num;
				}
			}
		}
		return -1;
	}

	public bool IsBlockInGroup(int iBlockID, string strGroupName)
	{
		int hashCode = strGroupName.GetHashCode();
		for (int num = m_Groups.Count - 1; num >= 0; num--)
		{
			if (m_Groups[num].m_NameHashcode == hashCode)
			{
				return IsBlockInGroup(iBlockID, num);
			}
		}
		return false;
	}

	public bool IsBlockInGroup(int iBlockID, int iIndex)
	{
		if (iIndex < m_Groups.Count)
		{
			for (int num = m_Groups[iIndex].m_Blocks.Length - 1; num >= 0; num--)
			{
				if (m_Groups[iIndex].m_Blocks[num] == iBlockID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetTotalGroups()
	{
		return m_Groups.Count;
	}

	public bool UpdateBlocksWithGroupData()
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		instance.ClearGroupTotals();
		int count = m_Groups.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_Groups[i] == null)
			{
				continue;
			}
			int num = m_Groups[i].m_Blocks.Length;
			for (int j = 0; j < num; j++)
			{
				BaseBuildingBlock buildingBlock = instance.GetBuildingBlock(m_Groups[i].m_Blocks[j]);
				if (buildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Object || buildingBlock.BlockType == BaseBuildingBlock.BuildingBlockType.Decoration)
				{
					BuildingBlock_Object buildingBlock_Object = buildingBlock as BuildingBlock_Object;
					if (buildingBlock_Object != null)
					{
						if (!buildingBlock_Object.m_ZoneObject)
						{
						}
						buildingBlock_Object.m_InBlockGroups.Add(i);
					}
				}
				else
				{
					if (buildingBlock.BlockType != BaseBuildingBlock.BuildingBlockType.Complex)
					{
						continue;
					}
					BuildingBlock_Complex buildingBlock_Complex = buildingBlock as BuildingBlock_Complex;
					if (buildingBlock_Complex != null)
					{
						if (!buildingBlock_Complex.m_ZoneObject)
						{
						}
						buildingBlock_Complex.m_InBlockGroups.Add(i);
					}
				}
			}
		}
		return true;
	}
}
