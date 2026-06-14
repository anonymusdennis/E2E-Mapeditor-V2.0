using System;
using UnityEngine;

[Serializable]
public class LimitValue
{
	public enum LimitType
	{
		UNKNOWN,
		FixedValue,
		BasedOnLimitationGroup,
		BasedOnBlocksInZone
	}

	public LimitType m_Type;

	public string m_BlockGroup = string.Empty;

	private int m_BlockIndex = -1;

	public int m_LimitationGroup = -1;

	public int m_ValuePerBlock = 1;

	public int m_MinimumValue;

	public int m_MaximumValue;

	private BuildingBlockManager m_BlockManager;

	private BuildingBlockGroupManager m_GroupsManager;

	public LimitType GetLimitType()
	{
		return m_Type;
	}

	public static LimitValue GetNewFixedValueLimit()
	{
		LimitValue limitValue = new LimitValue();
		limitValue.m_Type = LimitType.FixedValue;
		limitValue.m_ValuePerBlock = 0;
		return limitValue;
	}

	public static LimitValue GetNewLimitationGroupLimit()
	{
		LimitValue limitValue = new LimitValue();
		limitValue.m_Type = LimitType.BasedOnLimitationGroup;
		return limitValue;
	}

	public static LimitValue GetNewBlockGroupInZoneLimit()
	{
		LimitValue limitValue = new LimitValue();
		limitValue.m_Type = LimitType.BasedOnBlocksInZone;
		return limitValue;
	}

	public int GetValue(ref LevelEditor_ZoneManager.Zone zone)
	{
		int num = 0;
		switch (m_Type)
		{
		case LimitType.FixedValue:
			num = m_ValuePerBlock;
			break;
		case LimitType.BasedOnLimitationGroup:
			if (m_LimitationGroup != -1 && (m_BlockManager != null || (m_BlockManager = BuildingBlockManager.GetInstance()) != null))
			{
				num = Mathf.Max(m_BlockManager.GetLimitationTotal(m_LimitationGroup) * m_ValuePerBlock, m_MinimumValue);
				if (m_MaximumValue != 0)
				{
					num = Mathf.Min(num, m_MaximumValue);
				}
			}
			break;
		case LimitType.BasedOnBlocksInZone:
			if ((m_GroupsManager != null || (m_GroupsManager = BuildingBlockGroupManager.GetInstance()) != null) && zone != null && (m_BlockIndex != -1 || (m_BlockIndex = m_GroupsManager.GetGroupIndexByName(m_BlockGroup)) != -1))
			{
				num = Mathf.Max(zone.m_BlockGroupsUsed[m_BlockIndex] * m_ValuePerBlock, m_MinimumValue);
			}
			if (m_MaximumValue != 0)
			{
				num = Mathf.Min(m_MaximumValue, num);
			}
			break;
		}
		return num;
	}
}
