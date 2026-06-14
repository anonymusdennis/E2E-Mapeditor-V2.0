using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemContainer : T17MonoBehaviour
{
	public delegate void ItemContainerChangedEvent();

	public delegate void ItemContainerEvent(ItemContainer container, Item item);

	public delegate void ItemContainerAddedHandler(ItemContainer container, Item item, bool intoHidden);

	public enum ItemContainerType
	{
		Desk,
		DeskGuard,
		DeskInmate,
		Inmate,
		Guard,
		Dog,
		Toilet,
		Vendor,
		Job,
		SwagBag,
		Level,
		LevelObject,
		SolitaryKeys,
		Cutlrey,
		Bed
	}

	public int m_MaxSize = 4;

	public int m_MaxHiddenSize = 2;

	public ItemContainerType m_ContainerType;

	public bool m_CanBeUsedForQuests;

	public bool m_bCanLoot;

	[FormerlySerializedAs("m_Items")]
	public List<ItemData> m_StartingItems = new List<ItemData>();

	public List<ItemData> m_TrackedItems = new List<ItemData>();

	public List<RandomItemGroup> m_RandomGroups = new List<RandomItemGroup>();

	public int m_NumberFromGroups = 1;

	public bool m_UniqueFromGroup = true;

	public int[] m_RandomPercentages = new int[0];

	protected T17NetView m_NetView;

	protected List<Item> m_ItemObjects = new List<Item>();

	protected string m_ContainerName = string.Empty;

	private List<Item> m_HiddenItemObjects = new List<Item>();

	private Character m_CharacterOwner;

	private bool m_bIsLocked;

	private DeskInteraction m_DeskInteraction;

	public bool m_bShouldConsiderItemRefresh = true;

	private List<int> m_ItemMgrResponseIDs = new List<int>();

	private int m_ImmediateItemMgrResponseID = -1;

	public ItemContainerAddedHandler OnItemAddedEvent;

	public ItemContainerEvent OnItemRemovedEvent;

	public ItemContainerChangedEvent OnItemsChangedEvent;

	private Player m_Player;

	public List<Transform> m_LevelItemsSpawnPos = new List<Transform>();

	private int m_CurrentItemIndexToAdd;

	private Dictionary<int, int> m_bedStartItemCounts;

	private Dictionary<int, int> m_bedCurrentItemCounts;

	private NetObjectLock m_NetObjectLock;

	public T17NetView NetView => m_NetView;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = GetComponent<T17NetView>();
		m_CharacterOwner = null;
		m_bIsLocked = false;
		m_NetObjectLock = GetComponent<NetObjectLock>();
	}

	protected virtual void OnDestroy()
	{
		if (m_NetObjectLock != null)
		{
			if (m_NetObjectLock.IsLocked() && m_NetObjectLock.m_NetView != null)
			{
				m_NetObjectLock.ReleaseLock();
			}
			m_NetObjectLock = null;
		}
		ItemContainerManager instance = ItemContainerManager.GetInstance();
		if (instance != null)
		{
			instance.RemoveItemContainer(this, m_ContainerType);
		}
		m_NetView = null;
		m_DeskInteraction = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_CharacterOwner == null && GetComponent<Character>() != null)
		{
			return T17BehaviourManager.INITSTATE.IS_DEPS;
		}
		if (T17NetManager.IsMasterClient)
		{
			if (m_DeskInteraction == null)
			{
				m_DeskInteraction = GetComponent<DeskInteraction>();
			}
			RoomManager instance = RoomManager.GetInstance();
			if (instance != null && !instance.IsInited() && m_DeskInteraction != null)
			{
				return T17BehaviourManager.INITSTATE.IS_DEPS;
			}
		}
		m_bIsLocked = false;
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
		ItemContainerManager instance2 = ItemContainerManager.GetInstance();
		if (instance2 != null)
		{
			instance2.AddItemContainer(this, m_ContainerType);
		}
		if (m_ContainerName == string.Empty && base.gameObject != null)
		{
			m_ContainerName = base.gameObject.name;
		}
		m_DeskInteraction = GetComponent<DeskInteraction>();
		m_Player = GetComponent<Player>();
		if (m_NetView != null)
		{
			ItemManager instance3 = ItemManager.GetInstance();
			if (T17NetManager.IsMasterClient && instance3 != null)
			{
				ConfigManager instance4 = ConfigManager.GetInstance();
				if (instance4 != null)
				{
					ItemContainerConfig itemContainerOverride = instance4.GetItemContainerOverride(this);
					if (itemContainerOverride != null)
					{
						ApplyContainerConfig(itemContainerOverride);
					}
				}
				if (!PrisonSnapshotIO.IsThereSaveData())
				{
					m_ItemMgrResponseIDs.Clear();
					for (int i = 0; i < m_TrackedItems.Count; i++)
					{
						if (!(m_TrackedItems[i] == null))
						{
							m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(m_NetView.ownerId, m_TrackedItems[i].m_ItemDataID, OnItemMgrResponseAddToInventory, ref m_ImmediateItemMgrResponseID, m_NetView.viewID));
						}
					}
					for (int j = 0; j < m_StartingItems.Count; j++)
					{
						if (!(m_StartingItems[j] == null))
						{
							m_CurrentItemIndexToAdd = j;
							m_ItemMgrResponseIDs.Add(ItemManager.GetInstance().AssignItemRPC(m_NetView.ownerId, m_StartingItems[j].m_ItemDataID, OnItemMgrResponseAddToInventory, ref m_ImmediateItemMgrResponseID));
						}
					}
				}
			}
		}
		return base.StartInit();
	}

	private void OnItemMgrResponseAddToInventory(Item item, int eventID)
	{
		if (!(item != null) || (eventID != m_ImmediateItemMgrResponseID && !m_ItemMgrResponseIDs.Contains(eventID)))
		{
			return;
		}
		if (m_ContainerType != ItemContainerType.Level)
		{
			if (!AddItemRPC(item))
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
		else
		{
			Vector3 spawnPos = Vector3.zero;
			LevelScript.GetInstance().m_LevelItemContainer.GetItemSpawnPos(item.m_ItemData, ref spawnPos);
			item.DropItemInLevel(null, spawnPos);
		}
	}

	public void ApplyContainerConfig(ItemContainerConfig config)
	{
		if (!config.m_KeepOldStartingItems)
		{
			m_StartingItems.Clear();
		}
		for (int i = 0; i < config.m_StartingItems.Count; i++)
		{
			m_StartingItems.Add(config.m_StartingItems[i]);
		}
		if (!config.m_KeepOldTrackedItems)
		{
			m_TrackedItems.Clear();
		}
		for (int j = 0; j < config.m_TrackedItems.Count; j++)
		{
			m_TrackedItems.Add(config.m_TrackedItems[j]);
		}
		if (config.m_ReplaceRandomGroups)
		{
			m_RandomGroups.Clear();
			m_RandomPercentages = new int[0];
		}
		int num = m_RandomGroups.Count + config.m_RandomGroups.Count;
		int count = m_RandomGroups.Count;
		Array.Resize(ref m_RandomPercentages, num);
		for (int k = count; k < num; k++)
		{
			int num2 = k - count;
			m_RandomGroups.Add(config.m_RandomGroups[num2]);
			m_RandomPercentages[k] = config.m_RandomPercentages[num2];
		}
		m_bShouldConsiderItemRefresh = config.m_AllowRefresh;
	}

	public void GetItemSpawnPos(ItemData itemData, ref Vector3 spawnPos)
	{
		if (m_StartingItems != null && m_CurrentItemIndexToAdd >= 0 && m_CurrentItemIndexToAdd < m_StartingItems.Count && m_StartingItems[m_CurrentItemIndexToAdd] != null && m_LevelItemsSpawnPos != null && m_CurrentItemIndexToAdd >= 0 && m_CurrentItemIndexToAdd < m_LevelItemsSpawnPos.Count && m_LevelItemsSpawnPos[m_CurrentItemIndexToAdd] != null)
		{
			spawnPos = m_LevelItemsSpawnPos[m_CurrentItemIndexToAdd].position;
		}
	}

	public bool IsDesk()
	{
		return m_ContainerType == ItemContainerType.Desk || m_ContainerType == ItemContainerType.DeskGuard || m_ContainerType == ItemContainerType.DeskInmate;
	}

	public bool ConsiderRefreshingItems()
	{
		bool flag = false;
		if (m_NetView != null && T17NetManager.IsMasterClient)
		{
			switch (m_ContainerType)
			{
			case ItemContainerType.Desk:
			{
				bool flag2 = true;
				if (m_DeskInteraction != null)
				{
					flag2 = m_DeskInteraction.AllowRefresh();
				}
				if (flag2 && m_bShouldConsiderItemRefresh)
				{
					flag = RefreshItems();
				}
				break;
			}
			default:
				if (m_bShouldConsiderItemRefresh)
				{
					flag = RefreshItems();
				}
				break;
			case ItemContainerType.Level:
				break;
			}
			if (IsDesk() && !flag)
			{
				ItemContainerManager instance = ItemContainerManager.GetInstance();
				int count = m_ItemObjects.Count;
				for (int i = 0; i < count; i++)
				{
					if (m_ItemObjects[i] != null)
					{
						instance.RecordDeskItem(m_ItemObjects[i].ItemDataID, this);
					}
				}
			}
		}
		return flag;
	}

	private bool RefreshItems()
	{
		if (T17NetManager.IsMasterClient)
		{
			if (IsNetObjectLocked() && m_ContainerType != ItemContainerType.Bed)
			{
				return false;
			}
			ItemContainerManager instance = ItemContainerManager.GetInstance();
			ItemManager instance2 = ItemManager.GetInstance();
			m_ItemMgrResponseIDs.Clear();
			int ownerId = m_NetView.ownerId;
			switch (m_ContainerType)
			{
			default:
			{
				RemoveAllItems(releaseToManager: true, exemptQuestItems: true, bLeaveKeys: true);
				int count = m_ItemObjects.Count;
				for (int k = 0; k < count; k++)
				{
					if (m_ItemObjects[k] != null)
					{
						instance.RecordDeskItem(m_ItemObjects[k].ItemDataID, this);
					}
				}
				if (m_RandomGroups.Count > 0 && null != instance2)
				{
					for (int l = 0; l < m_NumberFromGroups; l++)
					{
						int num = UnityEngine.Random.Range(0, 100);
						int num2 = 0;
						int num3 = 0;
						for (int m = 0; m < m_RandomGroups.Count; m++)
						{
							num3 += m_RandomPercentages[m];
							if (num >= num2 && num < num3 && m_ItemObjects.Count < m_MaxSize && m_RandomGroups[m] != null)
							{
								ItemData randomItem = m_RandomGroups[m].GetRandomItem(m_UniqueFromGroup);
								if (null != randomItem)
								{
									m_ItemMgrResponseIDs.Add(instance2.AssignItemRPC(ownerId, randomItem.m_ItemDataID, OnItemMgrResponseAddToInventory, ref m_ImmediateItemMgrResponseID));
									if (IsDesk())
									{
										instance.RecordDeskItem(randomItem.m_ItemDataID, this);
									}
								}
							}
							num2 = num3;
						}
					}
				}
				return true;
			}
			case ItemContainerType.Bed:
			{
				if (GetFreeSpaceCount() <= 1)
				{
					return false;
				}
				if (m_bedStartItemCounts == null)
				{
					m_bedStartItemCounts = new Dictionary<int, int>();
				}
				if (m_bedCurrentItemCounts == null)
				{
					m_bedCurrentItemCounts = new Dictionary<int, int>();
				}
				m_bedStartItemCounts.Clear();
				m_bedCurrentItemCounts.Clear();
				for (int i = 0; i < m_StartingItems.Count; i++)
				{
					if (m_StartingItems[i] != null)
					{
						int itemDataID = m_StartingItems[i].m_ItemDataID;
						if (!m_bedStartItemCounts.ContainsKey(itemDataID))
						{
							m_bedStartItemCounts.Add(itemDataID, 1);
							m_bedCurrentItemCounts.Add(itemDataID, HasItem(itemDataID));
						}
						else
						{
							m_bedStartItemCounts[itemDataID]++;
						}
					}
				}
				for (int j = 0; j < m_StartingItems.Count; j++)
				{
					if (m_StartingItems[j] != null && GetFreeSpaceCount() > 0)
					{
						int itemDataID2 = m_StartingItems[j].m_ItemDataID;
						if (m_bedCurrentItemCounts[itemDataID2] < m_bedStartItemCounts[itemDataID2])
						{
							m_bedCurrentItemCounts[itemDataID2]++;
							m_ItemMgrResponseIDs.Add(instance2.AssignItemRPC(ownerId, itemDataID2, OnItemMgrResponseAddToInventory, ref m_ImmediateItemMgrResponseID));
						}
					}
				}
				return true;
			}
			case ItemContainerType.Vendor:
				break;
			}
		}
		return false;
	}

	public void SetName(string newName)
	{
		m_ContainerName = newName;
	}

	public bool IsVisibleFull()
	{
		return m_ItemObjects.Count >= m_MaxSize;
	}

	public bool IsHiddenFull()
	{
		return m_HiddenItemObjects.Count >= m_MaxHiddenSize;
	}

	public bool HasOneOfEachItem(List<ItemData> itemsToLookFor, List<ItemData> itemsFound = null)
	{
		bool result = true;
		itemsFound?.Clear();
		for (int num = itemsToLookFor.Count - 1; num >= 0; num--)
		{
			int itemDataID = itemsToLookFor[num].m_ItemDataID;
			bool flag = false;
			for (int num2 = m_ItemObjects.Count - 1; num2 >= 0; num2--)
			{
				if (m_ItemObjects[num2].ItemDataID == itemDataID)
				{
					itemsFound?.Add(itemsToLookFor[num]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				result = false;
			}
		}
		return result;
	}

	public int HasItem(int itemID, bool lookIntoHidden = false)
	{
		int num = 0;
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.m_ItemDataID == itemID)
			{
				num++;
			}
		}
		if (lookIntoHidden)
		{
			for (int j = 0; j < m_HiddenItemObjects.Count; j++)
			{
				if (m_HiddenItemObjects[j] != null && m_HiddenItemObjects[j].m_ItemData != null && m_HiddenItemObjects[j].m_ItemData.m_ItemDataID == itemID)
				{
					num++;
				}
			}
		}
		return num;
	}

	public int HasItemWithFunctionality(BaseItemFunctionality.Functionality functionalityType)
	{
		int num = 0;
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && (bool)m_ItemObjects[i].HasFunctionality(functionalityType))
			{
				num++;
			}
		}
		return num;
	}

	public bool HasSpecificItem(int itemViewID)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_NetView.viewID == itemViewID)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSpecificItem(int itemViewID, int itemDataID, bool isQuestItem, bool lookIntoHidden)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_NetView.viewID == itemViewID && m_ItemObjects[i].IsQuestItem() == isQuestItem && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.m_ItemDataID == itemDataID)
			{
				return true;
			}
		}
		if (lookIntoHidden)
		{
			for (int j = 0; j < m_HiddenItemObjects.Count; j++)
			{
				if (m_HiddenItemObjects[j] != null && m_HiddenItemObjects[j].m_NetView.viewID == itemViewID && m_HiddenItemObjects[j].IsQuestItem() == isQuestItem && m_HiddenItemObjects[j].m_ItemData != null && m_HiddenItemObjects[j].m_ItemData.m_ItemDataID == itemDataID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasContrabandItems(ref List<Item> contrabandItems)
	{
		bool result = false;
		contrabandItems = new List<Item>();
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.IsContraband())
			{
				contrabandItems.Add(m_ItemObjects[i]);
				result = true;
			}
		}
		return result;
	}

	public bool HasContrabandItems()
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.IsContraband())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasQuestItems()
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].IsQuestItem())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasKeyItem()
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && (m_ItemObjects[i].m_ItemData.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null || m_ItemObjects[i].m_ItemData.HasFunctionality(BaseItemFunctionality.Functionality.Keycard) != null))
			{
				return true;
			}
		}
		return false;
	}

	public void MoveItemsToAnotherContainer(ItemContainer destCon, bool includeHidden)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (destCon != null)
		{
			for (int num = m_ItemObjects.Count - 1; num >= 0; num--)
			{
				if (m_ItemObjects[num] != null)
				{
					list.Add(m_ItemObjects[num].m_NetView.viewID);
				}
			}
			if (includeHidden)
			{
				for (int num2 = m_HiddenItemObjects.Count - 1; num2 >= 0; num2--)
				{
					if (m_HiddenItemObjects[num2] != null)
					{
						list2.Add(m_HiddenItemObjects[num2].m_NetView.viewID);
					}
				}
			}
		}
		if (list.Count > 0 || list2.Count > 0)
		{
			m_NetView.RPC("RPC_MoveItemsToAnotherContainer", NetTargets.All, destCon.m_NetView.viewID, list.ToArray(), list2.ToArray());
		}
	}

	[PunRPC]
	public void RPC_MoveItemsToAnotherContainer(int iItemContainerViewId, int[] iItemViewIds, int[] iInToHiddenViewIds)
	{
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(iItemContainerViewId);
		if (!(itemContainer != null))
		{
			return;
		}
		Character characterOwner = itemContainer.GetCharacterOwner();
		bool flag = false;
		if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && characterOwner.GetEquippedItem() == null)
		{
			flag = true;
		}
		for (int num = iItemViewIds.Length - 1; num >= 0; num--)
		{
			Item item = T17NetView.Find<Item>(iItemViewIds[num]);
			if (item != null)
			{
				if (flag)
				{
					flag = false;
					RPC_MoveItemToCharacterEquipedSlot(item.m_NetView.viewID, characterOwner.m_NetView.viewID);
				}
				else
				{
					RPC_MoveItemToAnotherContainer(item.m_NetView.viewID, itemContainer.m_NetView.viewID, bInToHidden: false);
				}
			}
		}
		for (int num2 = iInToHiddenViewIds.Length - 1; num2 >= 0; num2--)
		{
			Item item2 = T17NetView.Find<Item>(iInToHiddenViewIds[num2]);
			if (item2 != null)
			{
				RPC_MoveItemToAnotherContainer(item2.m_NetView.viewID, itemContainer.m_NetView.viewID, bInToHidden: true);
			}
		}
	}

	public void MoveItemToAnotherContainerRPC(int iItemViewId, int iItemContainerViewId, bool bInToHidden = false)
	{
		m_NetView.PostLevelLoadRPC("RPC_MoveItemToAnotherContainer", NetTargets.All, iItemViewId, iItemContainerViewId, bInToHidden);
	}

	[PunRPC]
	public void RPC_MoveItemToAnotherContainer(int iItemViewId, int iItemContainerViewId, bool bInToHidden)
	{
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(iItemContainerViewId);
		if (!(null == itemContainer))
		{
			Item item = T17NetView.Find<Item>(iItemViewId);
			if (!(null == item) && itemContainer.LOCAL_AddItem(item, bInToHidden))
			{
				RemoveItemRPC(item, releaseToManager: false, RPC_CallContexts.All);
			}
		}
	}

	public void MoveItemToCharacterEquipedSlot(int itemViewID, int characterViewID)
	{
		m_NetView.PostLevelLoadRPC("RPC_MoveItemToCharacterEquipedSlot", NetTargets.All, itemViewID, characterViewID);
	}

	[PunRPC]
	private void RPC_MoveItemToCharacterEquipedSlot(int itemViewID, int characterViewID)
	{
		Item item = T17NetView.Find<Item>(itemViewID);
		if (!(null == item))
		{
			Character character = T17NetView.Find<Character>(characterViewID);
			if (!(null == character) && character.SetEquippedItem(item, bTellOthers: false, bAddOldToInventory: true, RPC_CallContexts.All))
			{
				RemoveItemRPC(item, releaseToManager: false, RPC_CallContexts.All);
			}
		}
	}

	public virtual bool AddItemRPC(Item item, bool intoHidden = false, RPC_CallContexts context = RPC_CallContexts.Unknown)
	{
		if (item == null)
		{
			T17NetManager.LogGoogleException("ItemContainer::AddItemRPC was called with a NULL item!");
		}
		bool flag = false;
		if (item.m_NetView.viewID == 0)
		{
			throw new Exception(" You are trying to add an item that is still a Prefab to an item Container");
		}
		KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
		if (keyFunctionality != null && keyFunctionality.IsHidden)
		{
			intoHidden = true;
		}
		if (!intoHidden)
		{
			if (m_ItemObjects.Count < m_MaxSize)
			{
				flag = true;
			}
		}
		else if (m_HiddenItemObjects.Count < m_MaxHiddenSize)
		{
			flag = true;
		}
		if (flag)
		{
			ItemManager instance = ItemManager.GetInstance();
			if (T17NetManager.IsMasterClient && null != instance && !instance.DataInitialised)
			{
				AddItem(item.m_NetView.viewID, intoHidden);
			}
			else if (m_NetView != null)
			{
				if (context == RPC_CallContexts.All)
				{
					if (T17NetManager.IsMasterClient)
					{
						m_NetView.PostLevelLoadRPC("RPC_AddItem", NetTargets.All, item.m_NetView.viewID, intoHidden);
					}
				}
				else
				{
					m_NetView.PostLevelLoadRPC("RPC_AddItem", NetTargets.All, item.m_NetView.viewID, intoHidden);
				}
			}
			else
			{
				flag = false;
			}
		}
		return flag;
	}

	[PunRPC]
	private void RPC_AddItem(int itemViewID, bool bInToHidden, PhotonMessageInfo info)
	{
		AddItem(itemViewID, bInToHidden);
	}

	private bool AddItem(int itemViewID, bool bInToHidden)
	{
		Item item = T17NetView.Find<Item>(itemViewID);
		if (null != item)
		{
			return LOCAL_AddItem(item, bInToHidden);
		}
		return false;
	}

	public bool LOCAL_AddItem(Item item, bool bInToHidden)
	{
		if (null == item)
		{
			return false;
		}
		if (item.TrackableUIElementReporter != null)
		{
			UnityEngine.Object.Destroy(item.TrackableUIElementReporter);
			item.SetTrackableUIElementReporter(null);
		}
		bool flag = LOCAL_AddItemToContainer(item, bInToHidden);
		if (flag)
		{
			if (OnItemAddedEvent != null)
			{
				OnItemAddedEvent(this, item, bInToHidden);
			}
			if (OnItemsChangedEvent != null)
			{
				OnItemsChangedEvent();
			}
		}
		return flag;
	}

	public bool LOCAL_AddItemToContainer(Item theItem, bool bHidden = false)
	{
		bool flag = false;
		if (!bHidden)
		{
			if (m_ItemObjects.Count < m_MaxSize)
			{
				Item item = m_ItemObjects.Find((Item x) => x.m_NetView.viewID == theItem.m_NetView.viewID);
				if (null == item)
				{
					m_ItemObjects.Add(theItem);
				}
				flag = true;
			}
		}
		else if (m_HiddenItemObjects.Count < m_MaxHiddenSize)
		{
			Item item2 = m_HiddenItemObjects.Find((Item x) => x.m_NetView.viewID == theItem.m_NetView.viewID);
			if (null == item2)
			{
				m_HiddenItemObjects.Add(theItem);
			}
			flag = true;
		}
		if (flag)
		{
			theItem.m_bHidden = bHidden;
			theItem.m_ContainerViewID = m_NetView.viewID;
			if (theItem.IsQuestItem())
			{
			}
			if (m_CharacterOwner != null && m_CharacterOwner.m_CharacterStats != null && m_CharacterOwner.m_CharacterStats.m_bIsPlayer)
			{
				TutorialManager instance = TutorialManager.GetInstance();
				if (instance != null && m_Player != null && m_Player.m_Gamer != null && theItem.m_ItemData != null)
				{
					if (theItem.m_ItemData.IsOutfit())
					{
						instance.StartTutorialRPC(m_Player, TutorialSubject.Outfits);
					}
					else if (!bHidden && instance.CheckTutorialNeeded(m_Player, TutorialSubject.Inventory) && theItem.m_ItemData.m_CanBeEquiped)
					{
						instance.StartTutorialRPC(m_Player, TutorialSubject.Inventory);
					}
					else if (instance.CheckTutorialNeeded(m_Player, TutorialSubject.Crafting))
					{
						CraftManager instance2 = CraftManager.GetInstance();
						if (instance2 != null && instance2.HasItemsForAnyRecipe(this))
						{
							instance.StartTutorialRPC(m_Player, TutorialSubject.Crafting);
						}
					}
				}
			}
			theItem.SetOwner(null);
			DoorManager instance3 = DoorManager.GetInstance();
			if (instance3 != null && m_CharacterOwner != null && theItem.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null)
			{
				instance3.SetUpCharacterKeys(m_CharacterOwner);
			}
		}
		if (bHidden != theItem.m_bHidden)
		{
		}
		return flag;
	}

	public void RemoveItemRPC(Item item, bool releaseToManager = false, RPC_CallContexts context = RPC_CallContexts.Unknown)
	{
		if (!(null != item) || !(null != item.m_NetView))
		{
			return;
		}
		ItemManager instance = ItemManager.GetInstance();
		if (T17NetManager.IsMasterClient && null != instance && !instance.DataInitialised)
		{
			RemoveItem(item.m_NetView.viewID, releaseToManager);
		}
		else if (context == RPC_CallContexts.All)
		{
			if (T17NetManager.IsMasterClient)
			{
				m_NetView.PostLevelLoadRPC("RPC_RemoveItem", NetTargets.All, item.m_NetView.viewID, releaseToManager);
			}
		}
		else
		{
			m_NetView.PostLevelLoadRPC("RPC_RemoveItem", NetTargets.All, item.m_NetView.viewID, releaseToManager);
		}
	}

	public void RemoveAllItems(bool releaseToManager = false, bool exemptQuestItems = true, bool bLeaveKeys = false, bool includeHidden = false)
	{
		int count = m_ItemObjects.Count;
		int count2 = m_HiddenItemObjects.Count;
		int num = count;
		if (includeHidden)
		{
			num += m_HiddenItemObjects.Count;
		}
		if (num == 0)
		{
			return;
		}
		int[] array = new int[num];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
			if ((!bLeaveKeys || (!(m_ItemObjects[i].HasFunctionality(BaseItemFunctionality.Functionality.Key) != null) && !(m_ItemObjects[i].HasFunctionality(BaseItemFunctionality.Functionality.Keycard) != null))) && m_ItemObjects[i] != null && (!exemptQuestItems || (exemptQuestItems && !m_ItemObjects[i].m_bIsAQuestItem)))
			{
				array[i] = m_ItemObjects[i].m_NetView.viewID;
			}
		}
		if (includeHidden)
		{
			for (int j = 0; j < count2; j++)
			{
				array[count + j] = -1;
				if ((!bLeaveKeys || (!(m_ItemObjects[j].HasFunctionality(BaseItemFunctionality.Functionality.Key) != null) && !(m_ItemObjects[j].HasFunctionality(BaseItemFunctionality.Functionality.Keycard) != null))) && m_HiddenItemObjects[j] != null && (!exemptQuestItems || (exemptQuestItems && !m_HiddenItemObjects[j].m_bIsAQuestItem)))
				{
					array[count + j] = m_HiddenItemObjects[j].m_NetView.viewID;
				}
			}
		}
		ItemManager instance = ItemManager.GetInstance();
		if (T17NetManager.IsMasterClient && null != instance && !instance.DataInitialised)
		{
			for (int k = 0; k < num; k++)
			{
				if (array[k] != -1)
				{
					RemoveItem(array[k], releaseToManager);
				}
			}
		}
		else
		{
			m_NetView.PostLevelLoadRPC("RPC_RemoveArrayOfItemViewIds", NetTargets.All, array, releaseToManager);
		}
	}

	public void RemoveItems(int count, ref int[] indicesToRemove, bool releaseToManager = false)
	{
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
			int num = indicesToRemove[i];
			if (num >= 0 && num < m_ItemObjects.Count && m_ItemObjects[num] != null)
			{
				array[i] = m_ItemObjects[num].m_NetView.viewID;
			}
		}
		for (int j = 0; j < count; j++)
		{
			if (array[j] != -1)
			{
				m_NetView.PostLevelLoadRPC("RPC_RemoveItem", NetTargets.All, array[j], releaseToManager);
			}
		}
	}

	[PunRPC]
	private void RPC_RemoveArrayOfItemViewIds(int[] itemViewIDs, bool releaseToManager, PhotonMessageInfo info)
	{
		for (int i = 0; i < itemViewIDs.Length; i++)
		{
			if (itemViewIDs[i] != -1)
			{
				RemoveItem(itemViewIDs[i], releaseToManager);
			}
		}
	}

	[PunRPC]
	private void RPC_RemoveItem(int itemViewID, bool releaseToManager, PhotonMessageInfo info)
	{
		RemoveItem(itemViewID, releaseToManager);
	}

	private void RemoveItem(int itemViewID, bool releaseToManager)
	{
		PhotonView photonView = PhotonView.Find(itemViewID);
		if (null == photonView)
		{
			return;
		}
		Item component = photonView.gameObject.GetComponent<Item>();
		bool flag = false;
		if (m_ItemObjects.Contains(component))
		{
			m_ItemObjects.Remove(component);
			flag = true;
		}
		else if (m_HiddenItemObjects.Contains(component))
		{
			m_HiddenItemObjects.Remove(component);
			flag = true;
		}
		if (flag)
		{
			if (component.m_ContainerViewID == m_NetView.viewID)
			{
				component.m_ContainerViewID = 0;
				component.m_bHidden = false;
			}
			if (OnItemRemovedEvent != null)
			{
				OnItemRemovedEvent(this, component);
			}
			if (OnItemsChangedEvent != null)
			{
				OnItemsChangedEvent();
			}
			if (m_CharacterOwner != null && component.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null)
			{
				DoorManager.GetInstance().SetUpCharacterKeys(m_CharacterOwner);
			}
			if (releaseToManager && T17NetManager.IsMasterClient)
			{
				ItemManager.GetInstance().RequestReleaseItem(itemViewID);
			}
		}
	}

	public bool SwitchItemCompartmentToMainRPC(Item item)
	{
		if (m_HiddenItemObjects.Contains(item) && m_ItemObjects.Count + 1 <= m_MaxSize)
		{
			m_NetView.RPC("RPC_SwitchItemCompartmentToHidden", NetTargets.All, item.m_NetView.viewID, false);
			return true;
		}
		return false;
	}

	public bool SwitchItemCompartmentToHiddenRPC(Item item)
	{
		if (m_ItemObjects.Contains(item) && m_HiddenItemObjects.Count + 1 <= m_MaxHiddenSize)
		{
			m_NetView.RPC("RPC_SwitchItemCompartmentToHidden", NetTargets.All, item.m_NetView.viewID, true);
			return true;
		}
		return false;
	}

	[PunRPC]
	private void RPC_SwitchItemCompartmentToHidden(int itemId, bool intoHidden)
	{
		List<Item> list;
		List<Item> list2;
		if (intoHidden)
		{
			list = m_ItemObjects;
			list2 = m_HiddenItemObjects;
		}
		else
		{
			list = m_HiddenItemObjects;
			list2 = m_ItemObjects;
		}
		Item item = list.Find((Item x) => x.m_NetView.viewID == itemId);
		if (item != null)
		{
			list.Remove(item);
			list2.Add(item);
			item.m_bHidden = intoHidden;
			if (OnItemsChangedEvent != null)
			{
				OnItemsChangedEvent();
			}
		}
	}

	public bool TryDestroyOneItem(bool excludeQuestItems = true, RPC_CallContexts context = RPC_CallContexts.Unknown)
	{
		Item itemToDestroy = GetItemToDestroy(excludeQuestItems);
		if (itemToDestroy != null)
		{
			RemoveItemRPC(itemToDestroy, releaseToManager: true, context);
			return true;
		}
		return false;
	}

	private Item GetItemToDestroy(bool excludeQuestItems)
	{
		List<int> trackedItemsForContainer = ItemManager.GetInstance().GetTrackedItemsForContainer(this);
		for (int i = 0; i < GetItemCount(); i++)
		{
			Item item = m_ItemObjects[i];
			if (!(item == null) && !(item.m_CoveringTile != null) && (trackedItemsForContainer == null || !trackedItemsForContainer.Contains(item.m_NetView.viewID)) && (!excludeQuestItems || !item.IsQuestItem()))
			{
				return item;
			}
		}
		return null;
	}

	private void DisableRendering(Item item)
	{
		SpriteRenderer component = item.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public bool GrabLock(int grabberID)
	{
		bool flag = false;
		if (m_bIsLocked)
		{
			return false;
		}
		m_bIsLocked = true;
		return true;
	}

	public void ReleaseLock()
	{
		m_bIsLocked = false;
	}

	public bool CanPickUp()
	{
		return true;
	}

	public int GetItemCount()
	{
		return m_ItemObjects.Count;
	}

	public int GetFreeSpaceCount()
	{
		return m_MaxSize - m_ItemObjects.Count;
	}

	public Item GetItem(int index)
	{
		if (index >= 0 && index < m_ItemObjects.Count)
		{
			return m_ItemObjects[index];
		}
		return null;
	}

	public Item GetItemByViewID(int viewID)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_NetView.viewID == viewID)
			{
				return m_ItemObjects[i];
			}
		}
		return null;
	}

	public Item GetFirstItemWithItemID(int itemID)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.m_ItemDataID == itemID)
			{
				return m_ItemObjects[i];
			}
		}
		return null;
	}

	public Item GetFirstItemWithItemFunctionality(BaseItemFunctionality.Functionality functionalityType)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && (bool)m_ItemObjects[i].HasFunctionality(functionalityType))
			{
				return m_ItemObjects[i];
			}
		}
		return null;
	}

	public int GetIndexOfItem(Item item)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public Item GetItemWithItemDataId(int itemDataId)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.m_ItemDataID == itemDataId)
			{
				return m_ItemObjects[i];
			}
		}
		return null;
	}

	public void GetItemsWithItemID(ref List<Item> itemList, int itemID, int count)
	{
		itemList.Clear();
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] != null && m_ItemObjects[i].m_ItemData != null && m_ItemObjects[i].m_ItemData.m_ItemDataID == itemID)
			{
				itemList.Add(m_ItemObjects[i]);
				count--;
				if (count <= 0)
				{
					break;
				}
			}
		}
	}

	public void GetItems(ref List<Item> items)
	{
		items.Clear();
		items.AddRange(m_ItemObjects);
	}

	public void GetHiddenItems(ref List<Item> items)
	{
		items.Clear();
		items.AddRange(m_HiddenItemObjects);
	}

	public int GetHiddenItemCount()
	{
		return m_HiddenItemObjects.Count;
	}

	public int GetHiddenFreeSpaceCount()
	{
		return m_MaxHiddenSize - m_HiddenItemObjects.Count;
	}

	public Item GetHiddenItem(int index)
	{
		if (index >= 0 && index < m_HiddenItemObjects.Count)
		{
			return m_HiddenItemObjects[index];
		}
		return null;
	}

	public Item GetHiddenItemByViewID(int viewID)
	{
		for (int i = 0; i < m_HiddenItemObjects.Count; i++)
		{
			if (m_HiddenItemObjects[i] != null && m_HiddenItemObjects[i].m_NetView.viewID == viewID)
			{
				return m_HiddenItemObjects[i];
			}
		}
		return null;
	}

	public void SetCharacterOwner(Character owner)
	{
		m_CharacterOwner = owner;
	}

	public Character GetCharacterOwner()
	{
		return m_CharacterOwner;
	}

	public int GetObjectNetID()
	{
		return m_NetView.viewID;
	}

	public void ItemAddedToFloor(ItemContainer container, Item item, bool intoHidden)
	{
		item.ItemDropped();
	}

	public void ItemRemovedFromFloor(ItemContainer container, Item item)
	{
		item.ItemPickedUp();
	}

	public bool IsNetObjectLocked()
	{
		if (m_NetObjectLock != null)
		{
			return m_NetObjectLock.IsLocked();
		}
		return false;
	}

	public DeskInteraction GetDeskInteraction()
	{
		return m_DeskInteraction;
	}

	public void OnDrawGizmos()
	{
		bool flag = false;
		KeyFunctionality.KeyColour keyColour = KeyFunctionality.KeyColour.None;
		for (int i = 0; i < m_TrackedItems.Count; i++)
		{
			if (!(m_TrackedItems[i] == null))
			{
				KeyFunctionality keyFunctionality = (KeyFunctionality)m_TrackedItems[i].HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (keyFunctionality != null)
				{
					flag = true;
					keyColour = keyFunctionality.m_KeyColour;
					break;
				}
			}
		}
		if (flag)
		{
			string value = string.Empty;
			switch (keyColour)
			{
			case KeyFunctionality.KeyColour.Black:
				value = "Gizmo_Key.png";
				break;
			case KeyFunctionality.KeyColour.Cyan:
				value = "Gizmo_KeyCyan.png";
				break;
			case KeyFunctionality.KeyColour.Red:
				value = "Gizmo_KeyRed.png";
				break;
			case KeyFunctionality.KeyColour.Green:
				value = "Gizmo_KeyGreen.png";
				break;
			case KeyFunctionality.KeyColour.Yellow:
				value = "Gizmo_KeyYellow.png";
				break;
			case KeyFunctionality.KeyColour.Purple:
				value = "Gizmo_KeyPurple.png";
				break;
			}
			if (!string.IsNullOrEmpty(value))
			{
				Gizmos.DrawIcon(base.transform.position - new Vector3(0.5f, -0.5f, 1.5f), value, allowScaling: true);
			}
		}
		if (!Application.isPlaying || m_ItemObjects == null || m_ItemObjects.Count <= 0)
		{
			return;
		}
		bool flag2 = false;
		for (int j = 0; j < m_ItemObjects.Count; j++)
		{
			if (m_ItemObjects[j] != null && m_ItemObjects[j].IsQuestItem())
			{
				flag2 = true;
				break;
			}
		}
		if (flag2)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawCube(base.transform.position - ((!flag) ? new Vector3(0.5f, -0.5f, 1.5f) : new Vector3(-0.5f, -0.5f, 1.5f)), new Vector3(0.5f, 0.5f, 0.5f));
		}
	}

	public void CyclicalAdd_AllRPC(Item item, bool intoHidden)
	{
		if (item != null)
		{
			m_NetView.RPC("AllRPC_CyclicalAdd", NetTargets.All, item.m_NetView.viewID, intoHidden);
		}
	}

	[PunRPC]
	private void AllRPC_CyclicalAdd(int itemViewId, bool intoHidden)
	{
		List<Item> list;
		int num;
		if (!intoHidden)
		{
			list = m_ItemObjects;
			num = m_MaxSize;
		}
		else
		{
			list = m_HiddenItemObjects;
			num = m_MaxHiddenSize;
		}
		bool flag = false;
		bool flag2 = false;
		Item item = T17NetView.Find<Item>(itemViewId);
		if (!(item != null))
		{
			return;
		}
		bool flag3 = true;
		if (list.Count == num)
		{
			Item itemToDestroy = GetItemToDestroy(excludeQuestItems: true);
			if (itemToDestroy != null)
			{
				ItemManager.GetInstance().Master_CheckTrackedItemsForReleased(itemToDestroy.m_NetView.viewID);
				if (!list.Remove(itemToDestroy))
				{
					flag3 = false;
				}
				else
				{
					flag2 = true;
					if (T17NetManager.IsMasterClient)
					{
						AIEventManager.GetInstance().SetContrabandItemDropped(itemToDestroy.transform, null, setActive: false);
					}
					ItemManager.GetInstance().Local_ReleaseItem(itemToDestroy.m_NetView.viewID);
					if (flag2 && OnItemRemovedEvent != null)
					{
						OnItemRemovedEvent(this, itemToDestroy);
					}
				}
			}
			else
			{
				flag3 = false;
			}
		}
		if (flag3)
		{
			list.Add(item);
			item.m_ContainerViewID = m_NetView.viewID;
			flag = true;
			item.m_bHidden = intoHidden;
			if (OnItemAddedEvent != null)
			{
				OnItemAddedEvent(this, item, intoHidden);
			}
		}
		if ((flag2 || flag) && OnItemsChangedEvent != null)
		{
			OnItemsChangedEvent();
		}
	}

	public int FindItemIndex(Item item)
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			if (m_ItemObjects[i] == item)
			{
				return i;
			}
		}
		for (int j = 0; j < m_HiddenItemObjects.Count; j++)
		{
			if (m_HiddenItemObjects[j] == item)
			{
				if (!m_HiddenItemObjects[j].m_bHidden)
				{
					T17NetManager.LogGoogleException("WHY IS THERE AN ITEM IN THE HIDDEN LIST THAT IS NOT FLAGGED AS HIDDEN?");
				}
				return j;
			}
		}
		return -1;
	}

	public void LogItemInfomation()
	{
		for (int i = 0; i < m_ItemObjects.Count; i++)
		{
			Item item = m_ItemObjects[i];
		}
		for (int j = 0; j < m_HiddenItemObjects.Count; j++)
		{
			Item item2 = m_HiddenItemObjects[j];
			if (!item2.m_bHidden)
			{
				T17NetManager.LogGoogleException("THIS ITEM IS IN THE HIDDEN COMPARTMENT BUT IT IS NOT FLAGED AS HIDDEN");
			}
		}
	}

	public static void MakeRoomForQuestItem(ItemContainer targetContainer)
	{
		if (targetContainer == null || !targetContainer.IsVisibleFull())
		{
			return;
		}
		int itemCount = targetContainer.GetItemCount();
		Item item = null;
		for (int num = itemCount - 1; num >= 0; num--)
		{
			Item item2 = targetContainer.GetItem(num);
			if (!(item2 == null) && !item2.IsQuestItem())
			{
				item = item2;
				if (item2.m_ItemData != null && !item2.m_ItemData.IsContraband())
				{
					break;
				}
			}
		}
		if (item != null)
		{
			targetContainer.RemoveItemRPC(item, releaseToManager: true);
		}
	}

	public static ItemContainer FindFirstContrabandDeskItemContainer()
	{
		GameObject gameObject = null;
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.ContrabandRoom);
		if (allRoomsByLocation != null)
		{
			for (int i = 0; i < allRoomsByLocation.Count; i++)
			{
				RoomBlob roomBlob = allRoomsByLocation[i];
				if (!(roomBlob == null))
				{
					RoomBlob_ContrabandRoom roomBlobData = roomBlob.GetRoomBlobData<RoomBlob_ContrabandRoom>();
					if (!(roomBlobData == null) && !(roomBlobData.m_Desk == null))
					{
						gameObject = roomBlobData.m_Desk.gameObject;
						break;
					}
				}
			}
			if (gameObject != null)
			{
				return gameObject.GetComponent<ItemContainer>();
			}
		}
		return null;
	}
}
