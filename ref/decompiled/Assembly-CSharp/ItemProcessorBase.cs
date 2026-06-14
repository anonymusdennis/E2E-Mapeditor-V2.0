using System.Collections.Generic;
using System.Linq;
using SaveHelpers;
using UnityEngine;

[RequireComponent(typeof(ItemContainer))]
public abstract class ItemProcessorBase : T17MonoBehaviour
{
	public bool m_bSecondaryProcessor;

	protected Dictionary<ItemData, ItemData> m_InputOutputItems;

	protected ItemContainer m_ItemContainer;

	protected T17NetView m_NetView;

	private int m_ItemMgrResponseID = -1;

	protected abstract void OnItemManagerCreatedItemForUs(Item item, int eventId);

	public abstract bool IsIdle();

	public abstract bool IsFinishedCreatingItem();

	protected override void Awake()
	{
		base.Awake();
		m_ItemContainer = GetComponent<ItemContainer>();
		m_NetView = GetComponent<T17NetView>();
	}

	private void OnDestroy()
	{
		if (m_InputOutputItems != null)
		{
			m_InputOutputItems.Clear();
			m_InputOutputItems = null;
		}
		m_ItemContainer = null;
		m_NetView = null;
	}

	protected int RequestItemCreation(int ownerID, int itemDataID)
	{
		return ItemManager.GetInstance().AssignItemRPC(ownerID, itemDataID, OnItemMgrResponseAddOutputItem, ref m_ItemMgrResponseID);
	}

	private void OnItemMgrResponseAddOutputItem(Item item, int eventID)
	{
		if (item != null && eventID == m_ItemMgrResponseID)
		{
			OnItemManagerCreatedItemForUs(item, eventID);
			m_ItemMgrResponseID = -1;
		}
	}

	public void SetInputOutputItemTypes(ItemData[] inputItems, ItemData[] outputItems)
	{
		m_InputOutputItems = null;
		if (inputItems == null || outputItems == null || inputItems.Length <= 0 || outputItems.Length <= 0 || inputItems.Length != outputItems.Length)
		{
			return;
		}
		for (int i = 0; i < inputItems.Length && i < outputItems.Length; i++)
		{
			if (inputItems[i] != null && outputItems[i] != null)
			{
				if (m_InputOutputItems == null)
				{
					m_InputOutputItems = new Dictionary<ItemData, ItemData>();
				}
				if (!m_InputOutputItems.ContainsKey(inputItems[i]))
				{
					m_InputOutputItems.Add(inputItems[i], outputItems[i]);
				}
			}
		}
	}

	public bool WillAcceptInput(Item item)
	{
		return GetOutputItem(item) != null;
	}

	public ItemData[] GetInputItemTypes()
	{
		if (m_InputOutputItems != null)
		{
			return m_InputOutputItems.Keys.ToArray();
		}
		return null;
	}

	protected virtual List<ItemData> GetPossibleOutputs()
	{
		return m_InputOutputItems.Values.ToList();
	}

	public List<ItemData> GetOutputItemTypes()
	{
		if (m_InputOutputItems != null)
		{
			return GetPossibleOutputs();
		}
		return null;
	}

	protected ItemData GetOutputItem(Item inputItem)
	{
		if (inputItem == null)
		{
			return null;
		}
		return GetOutputItem(inputItem.ItemDataID);
	}

	protected ItemData GetOutputItem(int inputItemDataId)
	{
		if (m_InputOutputItems == null)
		{
			return null;
		}
		foreach (KeyValuePair<ItemData, ItemData> inputOutputItem in m_InputOutputItems)
		{
			if (inputOutputItem.Key == null)
			{
				T17NetManager.LogGoogleException("Item process " + base.transform.name + " in scene " + LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " input/output items has a null key. Was it set up correctly?");
			}
			else if (inputOutputItem.Key.m_ItemDataID == inputItemDataId)
			{
				return inputOutputItem.Value;
			}
		}
		return null;
	}

	public virtual bool NeedsSaving()
	{
		return false;
	}

	public bool IsItemSpawnInProgress()
	{
		return m_ItemMgrResponseID != -1;
	}

	public virtual void Serialise(ref BitField bitfield)
	{
		bitfield.Set(12, (uint)GetComponent<T17NetView>().viewID);
	}

	public static void Deserialise(BitField bitfield)
	{
		int uInt = (int)bitfield.GetUInt(12);
		ItemProcessorBase itemProcessorBase = T17NetView.Find<ItemProcessorBase>(uInt);
		if (itemProcessorBase != null)
		{
			itemProcessorBase.DeserialiseWithBitfield(ref bitfield);
		}
	}

	protected virtual void DeserialiseWithBitfield(ref BitField bitfield)
	{
	}

	public virtual int GetBitsPerEntry()
	{
		return 12;
	}
}
