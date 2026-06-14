using System.Collections.Generic;
using DataHelpers;
using UnityEngine;

public class BuildingBlock_FilterManager : MonoBehaviour
{
	public delegate void BlockSetChanged(BaseBuildingBlock.BlockSet newSet, bool bUsingFilters);

	public delegate void FamilyChanged(int iFamily, bool bUsingFilters);

	public delegate void SmartFilterChanged(bool bSmartFilter);

	private static BuildingBlock_FilterManager m_Instance;

	private BaseBuildingBlock.BlockSet m_CurrentBlockSet = BaseBuildingBlock.BlockSet.ALL;

	private BaseBuildingBlock.BlockSet m_CurrentRoomBlockSet = BaseBuildingBlock.BlockSet.CentrePerks;

	private int m_CurrentFamily = -1;

	private bool m_SmartFilter = true;

	private bool m_FamilyFiltersOn = true;

	private bool m_NormalBlockSetsOn = true;

	private event BlockSetChanged OnBlockSetChanged;

	private event BlockSetChanged OnRoomBlockSetChanged;

	private event FamilyChanged OnFamilyChanged;

	private event SmartFilterChanged OnSmartFilterChanged;

	public static BuildingBlock_FilterManager GetInstance()
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

	private void Reset()
	{
		m_CurrentBlockSet = BaseBuildingBlock.BlockSet.CentrePerks;
		m_CurrentRoomBlockSet = BaseBuildingBlock.BlockSet.CentrePerks;
		m_CurrentFamily = -1;
		m_SmartFilter = true;
		m_FamilyFiltersOn = true;
		m_NormalBlockSetsOn = true;
	}

	public bool GetSmartFilter()
	{
		return m_SmartFilter;
	}

	public void SetSmartFilter(bool bSmart, bool bNotify = true)
	{
		if (m_SmartFilter != bSmart)
		{
			m_SmartFilter = bSmart;
			if (bNotify && this.OnSmartFilterChanged != null)
			{
				this.OnSmartFilterChanged(m_SmartFilter);
			}
		}
	}

	public int GetCurrentFamilyFilter()
	{
		return m_CurrentFamily;
	}

	public void SetCurrentFamilyFilter(int iFamily, bool bNotify = true)
	{
		if (m_CurrentFamily != iFamily)
		{
			m_CurrentFamily = iFamily;
			if (bNotify && this.OnFamilyChanged != null)
			{
				this.OnFamilyChanged(m_CurrentFamily, m_FamilyFiltersOn);
			}
		}
	}

	public bool AreWeUsingFamilyFilters()
	{
		return m_FamilyFiltersOn;
	}

	public void SetFamilyFiltersOn(bool bOn, bool bNotify = true)
	{
		if (m_FamilyFiltersOn != bOn)
		{
			m_FamilyFiltersOn = bOn;
			if (bNotify && this.OnFamilyChanged != null)
			{
				this.OnFamilyChanged(m_CurrentFamily, m_FamilyFiltersOn);
			}
		}
	}

	public bool AreWeUsingNormalBlockFilters()
	{
		return m_NormalBlockSetsOn;
	}

	public void SetNormalBlockFilters(bool bOn, bool bNotify = true)
	{
		if (m_NormalBlockSetsOn != bOn)
		{
			m_NormalBlockSetsOn = bOn;
			if (bNotify && this.OnBlockSetChanged != null)
			{
				this.OnBlockSetChanged(m_CurrentBlockSet, m_NormalBlockSetsOn);
			}
		}
	}

	public BaseBuildingBlock.BlockSet GetCurrentBlockSetFilter()
	{
		return m_CurrentBlockSet;
	}

	public void SetCurrentBlockSetFilter(BaseBuildingBlock.BlockSet newBlockSet, bool bNotify = true)
	{
		if (m_CurrentBlockSet != newBlockSet)
		{
			m_CurrentBlockSet = newBlockSet;
			if (bNotify && this.OnBlockSetChanged != null)
			{
				this.OnBlockSetChanged(m_CurrentBlockSet, m_NormalBlockSetsOn);
			}
		}
	}

	public BaseBuildingBlock.BlockSet GetCurrentRoomBlockSetFilter()
	{
		return m_CurrentRoomBlockSet;
	}

	public void SetCurrentRoomBlockSetFilter(BaseBuildingBlock.BlockSet newBlockSet, bool bNotify = true)
	{
		if (m_CurrentRoomBlockSet != newBlockSet)
		{
			m_CurrentRoomBlockSet = newBlockSet;
			if (bNotify && this.OnRoomBlockSetChanged != null)
			{
				this.OnRoomBlockSetChanged(m_CurrentRoomBlockSet, bUsingFilters: true);
			}
		}
	}

	public void RegisterForSmartChange(SmartFilterChanged smartFilterDelegate)
	{
		if (smartFilterDelegate != null)
		{
			OnSmartFilterChanged += smartFilterDelegate;
		}
	}

	public void RegisterForFamilyChange(FamilyChanged familyChangedDelegate)
	{
		if (familyChangedDelegate != null)
		{
			OnFamilyChanged += familyChangedDelegate;
		}
	}

	public void RegisterForBlockSetChange(BlockSetChanged blockSetChangedDelegate)
	{
		if (blockSetChangedDelegate != null)
		{
			OnBlockSetChanged += blockSetChangedDelegate;
		}
	}

	public void RegisterForRoomBlockSetChange(BlockSetChanged blockSetChangedDelegate)
	{
		if (blockSetChangedDelegate != null)
		{
			OnRoomBlockSetChanged += blockSetChangedDelegate;
		}
	}

	public void SerializeOurData(ref List<byte> dataCollection)
	{
		dataCollection.Add(17);
		int iIndex = dataCollection.Count;
		ByteArrayConversion.AddInt(0, ref dataCollection);
		ByteArrayConversion.AddInt((int)m_CurrentBlockSet, ref dataCollection);
		ByteArrayConversion.AddInt((int)m_CurrentRoomBlockSet, ref dataCollection);
		ByteArrayConversion.AddInt(m_CurrentFamily, ref dataCollection);
		byte b = 0;
		if (m_SmartFilter)
		{
			b = (byte)(b | 1u);
		}
		if (m_FamilyFiltersOn)
		{
			b = (byte)(b | 2u);
		}
		if (m_NormalBlockSetsOn)
		{
			b = (byte)(b | 4u);
		}
		dataCollection.Add(b);
		dataCollection.Add(102);
		ByteArrayConversion.StoreInt(dataCollection.Count - iIndex - 4, ref dataCollection, ref iIndex);
	}

	public bool DeserializeOurData(ref List<byte> dataCollection, ref int iIndex)
	{
		Reset();
		if (dataCollection[iIndex] != 17)
		{
			return false;
		}
		iIndex++;
		ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
		m_CurrentBlockSet = (BaseBuildingBlock.BlockSet)ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
		m_CurrentRoomBlockSet = (BaseBuildingBlock.BlockSet)ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
		m_CurrentFamily = ByteArrayConversion.GetInt(ref dataCollection, ref iIndex);
		byte b = dataCollection[iIndex++];
		if ((b & 1) == 0)
		{
			m_SmartFilter = false;
		}
		if ((b & 2) == 0)
		{
			m_FamilyFiltersOn = false;
		}
		if ((b & 4) == 0)
		{
			m_NormalBlockSetsOn = false;
		}
		if (dataCollection[iIndex++] != 102)
		{
			return false;
		}
		return true;
	}

	public string GetThemeTextResource(BaseBuildingBlock.BlockSet blockSet)
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance != null)
		{
			return instance.GetThemeTextResource(blockSet);
		}
		return "INVALID";
	}

	public Sprite GetThemeSprite(BaseBuildingBlock.BlockSet blockSet)
	{
		BuildingBlockManager instance = BuildingBlockManager.GetInstance();
		if (instance != null)
		{
			return instance.GetThemeSprite(blockSet);
		}
		return null;
	}
}
