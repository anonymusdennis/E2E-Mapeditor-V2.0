using System;
using System.Collections.Generic;
using System.Linq;
using NetworkLoadable;
using SaveHelpers;
using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class ItemManager : T17MonoBehaviour, INetworkLoadable, Saveable, IDeserializable
{
	public delegate void ItemManagerEvent(Item item, int eventID);

	[Serializable]
	public class NetItemSaveData
	{
		public bool m_DataInitialised;

		public List<long> m_ItemSerializedData = new List<long>();

		public List<int> m_ItemContainerPairData = new List<int>();

		public List<int> m_GroundItemOwners = new List<int>();
	}

	private enum ItemLocationType
	{
		Ground,
		Container,
		Outfit,
		Equipped,
		ManagedKey
	}

	private static ItemManager s_Instance;

	public int m_ItemPoolSize = 1000;

	public GameObject m_EmptyItemPrefab;

	private int m_LastIndexOfFreeItem;

	private T17NetView m_NetView;

	private List<ItemData> m_ExistingItemData = new List<ItemData>();

	private List<ItemData> m_AllowedItems = new List<ItemData>();

	private List<ItemData> m_KeyItems = new List<ItemData>();

	public List<ItemData> m_CoolItems = new List<ItemData>();

	private static int REQUEST_ID;

	private Dictionary<int, ItemManagerEvent> OnItemRequestResponseList = new Dictionary<int, ItemManagerEvent>();

	public ItemData m_MagicAiRepairWallItemData;

	public ItemData m_MagicAiRepairGroundItemData;

	public ItemData m_MagicAiDestroyWallItemData;

	public ItemData m_MagicAiDestroyGroundItemData;

	public ItemData m_MagicAiDestroyVentGroundItemData;

	public ItemManagerEvent OnQuestItemDestroyed;

	private bool m_bInited;

	public bool m_DataInitialised;

	private Dictionary<int, int> m_TrackedItems_ItemToContainer = new Dictionary<int, int>();

	private Dictionary<int, List<int>> m_TrackedItems_ContainerToItems = new Dictionary<int, List<int>>();

	private Item[] m_ItemPool;

	private Item[] m_UsedItemPool;

	private SaveDataRegister m_SaveData;

	private string m_LoadError = string.Empty;

	private LOADSTATE m_LoadState;

	public int RequestsInProgress => OnItemRequestResponseList.Count;

	public bool DataInitialised
	{
		get
		{
			return m_DataInitialised;
		}
		set
		{
			m_DataInitialised = value;
		}
	}

	public bool m_bItemsUpdated { get; private set; }

	public static ItemManager GetInstance()
	{
		return s_Instance;
	}

	protected override void Awake()
	{
		if (s_Instance != null)
		{
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			s_Instance = this;
		}
		base.Awake();
	}

	private void Start()
	{
		m_NetView = GetComponent<T17NetView>();
		if (m_NetView != null)
		{
			PhotonView component = GetComponent<PhotonView>();
			component.viewID = 0;
			m_NetView.viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.LevelManager);
			m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: true, 2);
		}
		NetLoadManagerSync.m_AllNetworkLoadables.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		for (int i = 0; i < m_ItemPoolSize; i++)
		{
			Item item = m_UsedItemPool[i];
			if (item != null)
			{
				UpdateManager.GetInstance().Unregister(item, UpdateCategory.Items);
				item.m_ItemData = null;
				UnityEngine.Object.DestroyObject(item);
				m_UsedItemPool[i] = null;
			}
		}
		for (int i = 0; i < m_ItemPoolSize; i++)
		{
			Item item2 = m_ItemPool[i];
			if (item2 != null)
			{
				UnityEngine.Object.DestroyObject(item2);
				m_ItemPool[i] = null;
			}
		}
		if (s_Instance == this)
		{
			s_Instance = null;
		}
		if (NetLoadManagerSync.m_AllNetworkLoadables != null)
		{
			NetLoadManagerSync.m_AllNetworkLoadables.Remove(this);
		}
		m_NetView = null;
	}

	public bool SpecialInit()
	{
		if (LevelScript.GetInstance().m_PreBuildItemCreatePool)
		{
			Debug.Log(" ++++ SpecialInit    m_PreBuildItemCreatePool ");
			LevelScript.ListOfItemDatas allowedItems = LevelScript.GetInstance().m_AllowedItems;
			int num = allowedItems.m_Datas.Length;
			for (int i = 0; i < num; i++)
			{
				m_AllowedItems.Add(allowedItems.m_Datas[i]);
			}
			allowedItems = LevelScript.GetInstance().m_KeyItems;
			num = allowedItems.m_Datas.Length;
			for (int i = 0; i < num; i++)
			{
				m_KeyItems.Add(allowedItems.m_Datas[i]);
			}
			m_ItemPool = new Item[m_ItemPoolSize];
			m_UsedItemPool = new Item[m_ItemPoolSize];
			CreatePool2(bUpdateNetworkService: true);
			Debug.Log(" ++++ SpecialInit    m_PreBuildItemCreatePool ");
			Debug.Log(" ++++ SpecialInit    m_PreBuildItemCreatePool ");
			Debug.Log(" ++++ SpecialInit    m_PreBuildItemCreatePool ");
		}
		else
		{
			CreatePool();
		}
		return true;
	}

	private void CreatePool()
	{
		m_bInited = false;
		m_ExistingItemData = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		m_ItemPool = new Item[m_ItemPoolSize];
		m_UsedItemPool = new Item[m_ItemPoolSize];
		CraftManager instance = CraftManager.GetInstance();
		if (m_ExistingItemData != null && m_ExistingItemData.Count > 0)
		{
			int i = 0;
			try
			{
				for (i = 0; i < m_ExistingItemData.Count; i++)
				{
					if (!(m_ExistingItemData[i] == null))
					{
						m_ExistingItemData[i].Init();
						KeyFunctionality keyFunctionality = (KeyFunctionality)m_ExistingItemData[i].HasFunctionality(BaseItemFunctionality.Functionality.Key);
						if (instance != null && instance.HaveRecipeWithItem(m_ExistingItemData[i]))
						{
							m_ExistingItemData[i].SetComponent();
						}
						if (keyFunctionality != null)
						{
							m_KeyItems.Add(m_ExistingItemData[i]);
						}
						else if (LevelScript.IsPrisonEnumInMask(m_ExistingItemData[i].m_PrisonMask, LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum))
						{
							m_AllowedItems.Add(m_ExistingItemData[i]);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log("Exception while trying to cache key data at index: " + i + " Msg: " + ex.Message);
			}
		}
		CreatePool2();
	}

	public void BuildCreatePool(List<CraftManager.Recipe> recipes)
	{
		Debug.LogError(" ****  ItemManager  BuildCreatePool " + Time.realtimeSinceStartup);
		m_bInited = false;
		m_ExistingItemData = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		m_ItemPool = new Item[m_ItemPoolSize];
		m_UsedItemPool = new Item[m_ItemPoolSize];
		if (m_ExistingItemData == null || m_ExistingItemData.Count <= 0)
		{
			return;
		}
		int num = 0;
		int i = -1;
		for (num = 0; num < m_ExistingItemData.Count; num++)
		{
			if (m_ExistingItemData[num] == null)
			{
				continue;
			}
			if (m_ExistingItemData[num].m_ItemDataID == 235)
			{
				Debug.Log(" **** ");
			}
			if (LevelScript.IsPrisonEnumInMask(m_ExistingItemData[num].m_PrisonMask, LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum))
			{
				m_AllowedItems.Add(m_ExistingItemData[num]);
				m_ExistingItemData[num].Init();
			}
			KeyFunctionality keyFunctionality = (KeyFunctionality)m_ExistingItemData[num].HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (recipes != null)
			{
				try
				{
					for (i = 0; i < recipes.Count; i++)
					{
						if (recipes[i] == null || !(recipes[i].m_Product != null))
						{
							continue;
						}
						bool flag = false;
						for (int j = 0; j < recipes[i].GetIngredientCount(); j++)
						{
							if (recipes[i].m_Ingredients[j] != null && recipes[i].m_Ingredients[j].m_ItemDataID == m_ExistingItemData[num].m_ItemDataID)
							{
								m_ExistingItemData[num].SetComponent();
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				catch (Exception)
				{
					Debug.LogError(" ****  ItemManager  BuildCreatePool   ERROR ERROR   problem with  recipe " + i);
				}
			}
			else
			{
				Debug.LogError(" ****  ItemManager  BuildCreatePool   No recipes recipes recipes recipes recipes ");
			}
			if (keyFunctionality != null)
			{
				m_ExistingItemData[num].Init();
				m_KeyItems.Add(m_ExistingItemData[num]);
			}
		}
	}

	private void CreatePool2(bool bUpdateNetworkService = false)
	{
		if (m_bInited)
		{
			return;
		}
		for (int i = 0; i < m_ItemPoolSize; i++)
		{
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			int viewID = T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart) + i;
			string text = "Item_Unused_" + i;
			m_ItemPool[i] = CreateNewItem_Internal(text, viewID);
		}
		m_bInited = true;
	}

	private Item CreateNewItem_Internal(string name, int viewID)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_EmptyItemPrefab);
		if (gameObject == null)
		{
			return null;
		}
		gameObject.name = name;
		gameObject.transform.SetParent(base.transform);
		Item component = gameObject.GetComponent<Item>();
		if (component.m_NetView != null)
		{
			component.m_NetView.viewID = viewID;
		}
		if (component.MeshRendererProp != null)
		{
			component.MeshRendererProp.enabled = false;
		}
		if (component.BoxColliderProp != null)
		{
			component.BoxColliderProp.enabled = false;
		}
		return component;
	}

	public Item GetMagicAIRepairWallItem()
	{
		return GetMagicItem(T17NetConfig.ReservedNetID.MagicAIRepairWallItem);
	}

	public Item GetMagicAIDestroyWallItem()
	{
		return GetMagicItem(T17NetConfig.ReservedNetID.MagicAIDestroyWallItem);
	}

	public Item GetMagicAIRepairGroundItem()
	{
		return GetMagicItem(T17NetConfig.ReservedNetID.MagicAIRepairGroundItem);
	}

	public Item GetMagicAIDestroyGroundItem()
	{
		return GetMagicItem(T17NetConfig.ReservedNetID.MagicAIDestroyGroundItem);
	}

	public Item GetMagicAIDestroyVentGroundItem()
	{
		return GetMagicItem(T17NetConfig.ReservedNetID.MagicAIDestroyVentGroundItem);
	}

	private Item GetMagicItem(T17NetConfig.ReservedNetID id)
	{
		int reservedNetID = T17NetConfig.GetReservedNetID(id);
		Item item = T17NetView.Find<Item>(reservedNetID);
		if (item == null)
		{
			string text = "AI_MagicItem_" + id;
			item = CreateNewItem_Internal(text, reservedNetID);
			if (item != null)
			{
				int itemDataID = -1;
				switch (id)
				{
				case T17NetConfig.ReservedNetID.MagicAIRepairWallItem:
					itemDataID = m_MagicAiRepairWallItemData.m_ItemDataID;
					break;
				case T17NetConfig.ReservedNetID.MagicAIRepairGroundItem:
					itemDataID = m_MagicAiRepairGroundItemData.m_ItemDataID;
					break;
				case T17NetConfig.ReservedNetID.MagicAIDestroyWallItem:
					itemDataID = m_MagicAiDestroyWallItemData.m_ItemDataID;
					break;
				case T17NetConfig.ReservedNetID.MagicAIDestroyGroundItem:
					itemDataID = m_MagicAiDestroyGroundItemData.m_ItemDataID;
					break;
				case T17NetConfig.ReservedNetID.MagicAIDestroyVentGroundItem:
					itemDataID = m_MagicAiDestroyVentGroundItemData.m_ItemDataID;
					break;
				}
				AssignDataToItem(ref item, ref itemDataID, 0);
				item.m_bIsAMagicItem = true;
				if (item != null)
				{
					UpdateManager.GetInstance().Register(item, UpdateCategory.Items);
				}
			}
		}
		return item;
	}

	public int AssignKeyRPC(int ownerID, KeyFunctionality.KeyColour keyColour, ItemManagerEvent responseEvent, ref int requestID)
	{
		if (m_KeyItems != null && m_KeyItems.Count > 0)
		{
			for (int i = 0; i < m_KeyItems.Count; i++)
			{
				KeyFunctionality keyFunctionality = (KeyFunctionality)m_KeyItems[i].HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (keyFunctionality != null && keyFunctionality.m_KeyColour == keyColour && keyFunctionality.IsDurable)
				{
					if (ownerID == -1)
					{
						ownerID = m_NetView.ownerId;
					}
					return AssignItemRPC(ownerID, m_KeyItems[i].m_ItemDataID, responseEvent, ref requestID);
				}
			}
		}
		return -1;
	}

	public int GetNextRequestID()
	{
		return REQUEST_ID + 1;
	}

	public int AssignItemRPC(int ownerID, int itemDataID, ItemManagerEvent responseEvent, ref int requestID, int trackingContainerId = -1)
	{
		if (responseEvent != null)
		{
			REQUEST_ID++;
			OnItemRequestResponseList.Add(REQUEST_ID, responseEvent);
			requestID = REQUEST_ID;
		}
		m_NetView.RPCQuestion("RPC_AssignItemRPC", NetTargets.MasterClient, ownerID, itemDataID, REQUEST_ID, trackingContainerId);
		return REQUEST_ID;
	}

	[PunRPC]
	public void RPC_AssignItemRPC(int RPCID, int ownerID, int itemDataID, int requestID, int trackingContainerId, PhotonMessageInfo info)
	{
		if (RequestFreeItem(out var netviewID, out var listIndex))
		{
			CreateItemRPC(netviewID, ownerID, listIndex, itemDataID, trackingContainerId, RPCID, requestID);
		}
		else
		{
			T17NetManager.LogGoogleException("ItemManager::RPC_AssignItemRPC has run out of items! INVESTIGATE!");
			m_NetView.RPCResponse("RPC_ItemResponse", RPCID, listIndex, requestID);
		}
		if (listIndex == -1)
		{
			Debug.Log(" ERROR RPC_AssignItemRPC  ERROR ");
		}
	}

	[PunRPC]
	private void RPC_ItemResponse(int indexInList, int eventID)
	{
		if (OnItemRequestResponseList.ContainsKey(eventID))
		{
			Item item = ((indexInList != -1) ? m_UsedItemPool[indexInList] : null);
			if (item == null && indexInList == -1)
			{
				T17NetManager.LogGoogleException("ItemManager::RPC_ItemResponse got a response with an invalid index (-1)");
			}
			ItemManagerEvent itemManagerEvent = OnItemRequestResponseList[eventID];
			OnItemRequestResponseList.Remove(eventID);
			itemManagerEvent(item, eventID);
		}
	}

	private bool RequestFreeItem(out int netviewID, out int listIndex)
	{
		netviewID = -1;
		listIndex = -1;
		for (int i = m_LastIndexOfFreeItem; i < m_ItemPoolSize; i++)
		{
			if (m_ItemPool[i] != null && !m_ItemPool[i].MarkedForUse)
			{
				netviewID = m_ItemPool[i].m_NetView.viewID;
				m_ItemPool[i].MarkedForUse = true;
				listIndex = i;
				m_LastIndexOfFreeItem = i + 1;
				return true;
			}
		}
		for (int j = 0; j < m_LastIndexOfFreeItem; j++)
		{
			if (m_ItemPool[j] != null && !m_ItemPool[j].MarkedForUse)
			{
				netviewID = m_ItemPool[j].m_NetView.viewID;
				m_ItemPool[j].MarkedForUse = true;
				listIndex = j;
				m_LastIndexOfFreeItem = j + 1;
				return true;
			}
		}
		T17NetManager.LogGoogleException("ItemManager::RequestFreeItem - ItemManager has run out of free items.");
		return false;
	}

	private bool TakeFreeItem(int iIndex)
	{
		if (m_UsedItemPool[iIndex] != null)
		{
		}
		if (m_ItemPool[iIndex] != null)
		{
			m_UsedItemPool[iIndex] = m_ItemPool[iIndex];
			m_ItemPool[iIndex] = null;
		}
		if (m_UsedItemPool[iIndex] != null)
		{
			m_UsedItemPool[iIndex].MarkedForUse = false;
			m_UsedItemPool[iIndex].gameObject.SetActive(value: true);
			return true;
		}
		return false;
	}

	public void RequestReleaseItem(Item item, RPC_CallContexts callContext = RPC_CallContexts.Unknown)
	{
		if (item != null)
		{
			RequestReleaseItem(item.m_NetView.viewID, callContext);
		}
	}

	public void RequestReleaseItem(int itemviewID, RPC_CallContexts callContext = RPC_CallContexts.Unknown)
	{
		if (itemviewID == -1)
		{
			return;
		}
		if (callContext == RPC_CallContexts.All)
		{
			if (T17NetManager.IsMasterClient)
			{
				RPC_MASTER_Release(itemviewID);
			}
		}
		else if (T17NetManager.IsMasterClient)
		{
			RPC_MASTER_Release(itemviewID);
		}
		else
		{
			m_NetView.RPC("RPC_MASTER_Release", NetTargets.MasterClient, itemviewID);
		}
	}

	[PunRPC]
	private void RPC_MASTER_Release(int netviewID)
	{
		MASTER_ReleaseItemRPC(netviewID);
	}

	public bool MASTER_ReleaseItemRPC(int netviewID)
	{
		int num = netviewID - T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart);
		if (num < 0 || num >= m_UsedItemPool.Length)
		{
			T17NetManager.LogGoogleException("Trying to release an item where the netviewID is " + netviewID + " and correlates to array index: " + num + " while the list length is: " + m_UsedItemPool.Length);
			return false;
		}
		if (m_UsedItemPool[num] == null)
		{
			return false;
		}
		bool flag = Master_CheckTrackedItemsForReleased(netviewID);
		Item item = T17NetView.Find<Item>(netviewID);
		AIEventManager.GetInstance().SetContrabandItemDropped(item.transform, null, setActive: false);
		if (item.m_CoveringTile != null)
		{
			item.m_CoveringTile.SetItem(-1);
			item.m_CoveringTile = null;
		}
		if (flag)
		{
			return true;
		}
		if (m_UsedItemPool[num] != null && m_UsedItemPool[num].m_NetView.viewID == netviewID)
		{
			m_NetView.PostLevelLoadRPC("RPC_ReleaseItem", NetTargets.All, netviewID, num, m_NetView.ownerId);
		}
		return true;
	}

	public void Local_ReleaseItem(int netviewID)
	{
		int num = netviewID - T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart);
		if (m_UsedItemPool[num] != null && m_UsedItemPool[num].m_NetView.viewID == netviewID)
		{
			RPC_ReleaseItem(netviewID, num, m_NetView.ownerId);
		}
	}

	public bool Master_CheckTrackedItemsForReleased(int netviewID)
	{
		if (!T17NetManager.IsMasterClient)
		{
			return false;
		}
		bool result = false;
		if (m_TrackedItems_ItemToContainer.ContainsKey(netviewID))
		{
			int viewID = m_TrackedItems_ItemToContainer[netviewID];
			ItemContainer itemContainer = T17NetView.Find<ItemContainer>(viewID);
			if (!(itemContainer == null))
			{
				Item item = T17NetView.Find<Item>(netviewID);
				if (!(item == null))
				{
					if (itemContainer.HasSpecificItem(netviewID))
					{
						result = true;
					}
					else
					{
						if (itemContainer.IsVisibleFull() && !itemContainer.TryDestroyOneItem())
						{
							itemContainer.TryDestroyOneItem(excludeQuestItems: false);
						}
						if (!itemContainer.IsVisibleFull())
						{
							if ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Key) || (bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Keycard))
							{
								HandleKeyRespawnedInContainer(itemContainer);
							}
							itemContainer.AddItemRPC(item);
							item.HandleRespawnedRPC();
							result = true;
						}
					}
				}
			}
		}
		return result;
	}

	private static void HandleKeyRespawnedInContainer(ItemContainer container)
	{
		Character characterOwner = container.GetCharacterOwner();
		if (characterOwner != null)
		{
			AICharacter_Guard component = characterOwner.GetComponent<AICharacter_Guard>();
			if (component != null && (characterOwner.GetIsKnockedOut() || component.IsPendingMedicBedKeyCheck()))
			{
				component.SetReleasedKeyRespawnedOnUs(wasRespawned: true);
			}
		}
	}

	public Item GetItemFromUsedList(int indexInList)
	{
		if (m_UsedItemPool != null && indexInList >= 0 && indexInList < m_ItemPoolSize)
		{
			return m_UsedItemPool[indexInList];
		}
		return null;
	}

	public Item GetItemFromUsedListByNetView(int netviewID)
	{
		int num = netviewID - T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart);
		if (m_UsedItemPool != null && num >= 0 && num < m_ItemPoolSize)
		{
			return m_UsedItemPool[num];
		}
		return null;
	}

	public ItemData GetRandomItemFromAllowedList()
	{
		int index = UnityEngine.Random.Range(0, m_AllowedItems.Count);
		return m_AllowedItems[index];
	}

	public bool IsItemInAllowedList(ItemData data)
	{
		return LevelScript.IsPrisonEnumInMask(data.m_PrisonMask, LevelScript.GetInstance().m_LevelSetup.m_LevelInfo.m_PrisonEnum);
	}

	public ItemData GetItemDataWithID(int id)
	{
		if (m_ExistingItemData != null && m_ExistingItemData.Count > 0)
		{
			for (int num = m_ExistingItemData.Count - 1; num >= 0; num--)
			{
				if (m_ExistingItemData[num].m_ItemDataID == id)
				{
					return m_ExistingItemData[num];
				}
			}
		}
		else
		{
			for (int num2 = m_AllowedItems.Count - 1; num2 >= 0; num2--)
			{
				if (m_AllowedItems[num2].m_ItemDataID == id)
				{
					return m_AllowedItems[num2];
				}
			}
		}
		return null;
	}

	private bool AssignDataToItem(ref Item item, ref int itemDataID, int ownerID, bool isLateJoin = false)
	{
		if (item != null)
		{
			if (m_ExistingItemData != null && m_ExistingItemData.Count > 0)
			{
				for (int i = 0; i < m_ExistingItemData.Count; i++)
				{
					if (m_ExistingItemData[i].m_ItemDataID != itemDataID)
					{
						continue;
					}
					item.m_ItemData = ScriptableObject.CreateInstance<ItemData>();
					item.m_ItemData.CopyData(m_ExistingItemData[i]);
					item.m_ItemData.SetParentItem(item);
					item.MeshRendererProp.material = item.m_ItemData.m_ItemWorldMaterial;
					if (ConfigManager.GetInstance() != null)
					{
						ItemDataConfig itemOverrideConfig = ConfigManager.GetInstance().GetItemOverrideConfig(itemDataID);
						if (itemOverrideConfig != null)
						{
							item.m_ItemData.ApplyConfigData(itemOverrideConfig);
						}
					}
					return true;
				}
			}
			else
			{
				for (int j = 0; j < m_AllowedItems.Count; j++)
				{
					if (m_AllowedItems[j].m_ItemDataID != itemDataID)
					{
						continue;
					}
					item.m_ItemData = ScriptableObject.CreateInstance<ItemData>();
					item.m_ItemData.CopyData(m_AllowedItems[j]);
					item.m_ItemData.SetParentItem(item);
					item.MeshRendererProp.material = item.m_ItemData.m_ItemWorldMaterial;
					if (ConfigManager.GetInstance() != null)
					{
						ItemDataConfig itemOverrideConfig2 = ConfigManager.GetInstance().GetItemOverrideConfig(itemDataID);
						if (itemOverrideConfig2 != null)
						{
							item.m_ItemData.ApplyConfigData(itemOverrideConfig2);
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	[PunRPC]
	public void RPC_ReleaseItem(int netviewID, int indexInList, int ownerID)
	{
		if (m_UsedItemPool != null && indexInList >= 0 && indexInList < m_UsedItemPool.Length && m_UsedItemPool[indexInList] != null && m_UsedItemPool[indexInList].m_NetView.viewID == netviewID)
		{
			if (m_UsedItemPool[indexInList].IsQuestItem() && OnQuestItemDestroyed != null)
			{
				OnQuestItemDestroyed(m_UsedItemPool[indexInList], -1);
			}
			m_ItemPool[indexInList] = m_UsedItemPool[indexInList];
			m_UsedItemPool[indexInList] = null;
			m_ItemPool[indexInList].m_ItemData = null;
			m_ItemPool[indexInList].ResetItem();
			m_ItemPool[indexInList].gameObject.name = "Item_Unused_" + indexInList;
			m_ItemPool[indexInList].gameObject.SetActive(value: false);
			UpdateManager.GetInstance().Unregister(m_ItemPool[indexInList], UpdateCategory.Items);
		}
	}

	private void CreateItemRPC(int netviewID, int ownerID, int indexInList, int itemDataID, int trackingContainerID, int rpcid, int requestID)
	{
		if (T17NetManager.IsMasterClient && !DataInitialised)
		{
			RPC_CreateItem(netviewID, ownerID, indexInList, itemDataID, trackingContainerID, rpcid, requestID);
			return;
		}
		m_NetView.PostLevelLoadRPC("RPC_CreateItem", NetTargets.All, netviewID, ownerID, indexInList, itemDataID, trackingContainerID, rpcid, requestID);
	}

	[PunRPC]
	public void RPC_CreateItem(int netviewID, int ownerID, int indexInList, int itemDataID, int trackingContainerID, int rpcid, int requestID)
	{
		if (m_UsedItemPool != null && indexInList >= 0 && indexInList < m_UsedItemPool.Length && ConfigManager.GetInstance().HasActiveConfig())
		{
			TakeFreeItem(indexInList);
			Item item = m_UsedItemPool[indexInList];
			if (item != null)
			{
				AssignDataToItem(ref item, ref itemDataID, ownerID);
				if (item.m_ItemData != null)
				{
					item.gameObject.name = item.m_ItemData.m_ItemLocalizationTag + "_" + indexInList;
					TrackItem(netviewID, trackingContainerID);
					UpdateManager.GetInstance().Register(item, UpdateCategory.Items);
				}
				else
				{
					m_ItemPool[indexInList] = m_UsedItemPool[indexInList];
					m_UsedItemPool[indexInList] = null;
					m_ItemPool[indexInList].ResetItem();
					m_ItemPool[indexInList].gameObject.name = "Item_Unused_" + indexInList;
					m_ItemPool[indexInList].gameObject.SetActive(value: false);
				}
			}
		}
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.RPCResponse("RPC_ItemResponse", rpcid, indexInList, requestID);
		}
	}

	public void GiftEquipedItemRPC(Character gifter, Character receiver, int money)
	{
		if (!(gifter == null) && !(receiver == null))
		{
			m_NetView.RPC("RPC_MASTER_GiftEquipedItem", NetTargets.MasterClient, gifter.m_NetView.viewID, receiver.m_NetView.viewID, money);
		}
	}

	[PunRPC]
	private void RPC_MASTER_GiftEquipedItem(int gifterViewID, int receiverViewID, int money)
	{
		Character character = T17NetView.Find<Character>(gifterViewID);
		Character character2 = T17NetView.Find<Character>(receiverViewID);
		if (!(character != null) || !(character2 != null))
		{
			return;
		}
		int num = 0;
		Item equippedItem = character.GetEquippedItem();
		int num2 = -1;
		if (equippedItem != null)
		{
			num2 = equippedItem.m_ItemData.m_ItemDataID;
			if (character.m_CharacterStats.m_bIsPlayer && equippedItem.m_ItemData.m_ItemDataID == StatsTracking.TEA_ITEM_ID)
			{
				StatSystem.GetInstance().IncStat(23, 1f, ((Player)character).m_Gamer, string.Empty);
			}
			num += equippedItem.GetGiftValue(character2);
			MASTER_ReleaseItemRPC(equippedItem.m_NetView.viewID);
			character.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
		}
		if (money > 0)
		{
			character.m_CharacterStats.DecreaseMoney(money);
			character2.m_CharacterStats.IncreaseMoney(money);
		}
		if (OpinionManager.GetInstance() != null)
		{
			num = Mathf.FloorToInt((float)num * OpinionManager.GetInstance().GetItemGiftValueModifier());
		}
		num += money;
		if (num > 0)
		{
			character2.m_CharacterOpinions.IncreaseOpinionOf(character, num);
			EffectManager.PlayEffect(EffectManager.effectType.OpinionIncrease, character2.GetStatChangeEffectPosition());
		}
		else if (num < 0)
		{
			character2.m_CharacterOpinions.DecreaseOpinionOf(character, Mathf.Abs(num));
			EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, character2.GetStatChangeEffectPosition());
		}
		m_NetView.RPC("RPC_ReceiveGiftItems", NetTargets.All, gifterViewID, receiverViewID, new int[1] { num2 }, money);
	}

	public void GiftItemsRPC(Character gifter, Character receiver, ItemContainer sourceContainer, List<Item> items, int money)
	{
		if (gifter == null || receiver == null || sourceContainer == null || (items.Count <= 0 && money <= 0))
		{
			return;
		}
		int viewID = gifter.m_NetView.viewID;
		int viewID2 = receiver.m_NetView.viewID;
		int objectNetID = sourceContainer.GetObjectNetID();
		int count = items.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = -1;
			if (items[i] != null)
			{
				array[i] = items[i].m_NetView.viewID;
			}
		}
		m_NetView.RPC("RPC_GiftItems", NetTargets.MasterClient, viewID, viewID2, objectNetID, array, money);
	}

	[PunRPC]
	public void RPC_GiftItems(int gifterID, int receiverID, int containerID, int[] itemViewIDs, int money)
	{
		Character character = T17NetView.Find<Character>(gifterID);
		Character character2 = T17NetView.Find<Character>(receiverID);
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(containerID);
		if (!(character != null) || !(character2 != null) || !(itemContainer != null))
		{
			return;
		}
		int num = 0;
		int num2 = itemViewIDs.Length;
		List<int> list = new List<int>();
		for (int num3 = num2 - 1; num3 >= 0; num3--)
		{
			Item itemByViewID = itemContainer.GetItemByViewID(itemViewIDs[num3]);
			if (itemByViewID != null)
			{
				list.Add(itemByViewID.ItemDataID);
				if (character.m_CharacterStats.m_bIsPlayer && itemByViewID.m_ItemData.m_ItemDataID == StatsTracking.TEA_ITEM_ID)
				{
					StatSystem.GetInstance().IncStat(23, 1f, ((Player)character).m_Gamer, string.Empty);
				}
				num += itemByViewID.GetGiftValue(character2);
				itemContainer.RemoveItemRPC(itemByViewID, releaseToManager: true);
			}
		}
		int num4 = Math.Min(Mathf.RoundToInt(character.m_CharacterStats.Money), money);
		if (num4 > 0)
		{
			character.m_CharacterStats.DecreaseMoney(num4);
			character2.m_CharacterStats.IncreaseMoney(num4);
		}
		if (OpinionManager.GetInstance() != null)
		{
			num = Mathf.FloorToInt((float)num * OpinionManager.GetInstance().GetItemGiftValueModifier());
		}
		num += num4;
		if (num > 0)
		{
			character2.m_CharacterOpinions.IncreaseOpinionOf(character, num);
			EffectManager.PlayEffect(EffectManager.effectType.OpinionIncrease, character2.GetStatChangeEffectPosition());
		}
		else if (num < 0)
		{
			character2.m_CharacterOpinions.DecreaseOpinionOf(character, Mathf.Abs(num));
			EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, character2.GetStatChangeEffectPosition());
		}
		if (list.Count > 0 || num4 > 0)
		{
			m_NetView.RPC("RPC_ReceiveGiftItems", NetTargets.All, gifterID, receiverID, list.ToArray(), num4);
		}
	}

	[PunRPC]
	public void RPC_ReceiveGiftItems(int gifterID, int receiverID, int[] itemDataIDs, int money)
	{
		Character character = T17NetView.Find<Character>(gifterID);
		Character character2 = T17NetView.Find<Character>(receiverID);
		if (character != null && character2 != null)
		{
			character2.ReceivedGift(character, itemDataIDs, money);
		}
	}

	private bool TrackItem(int itemViewID, int containerViewId)
	{
		if (containerViewId == -1 || itemViewID == -1)
		{
			return false;
		}
		bool result = false;
		if (!m_TrackedItems_ItemToContainer.TryGetValue(itemViewID, out var value))
		{
			m_TrackedItems_ItemToContainer.Add(itemViewID, containerViewId);
		}
		else if (containerViewId != value)
		{
			result = true;
		}
		if (!m_TrackedItems_ContainerToItems.TryGetValue(containerViewId, out var value2))
		{
			m_TrackedItems_ContainerToItems.Add(containerViewId, new List<int>());
			m_TrackedItems_ContainerToItems[containerViewId].Add(itemViewID);
		}
		else
		{
			if (value2 == null)
			{
				value2 = new List<int>();
			}
			bool flag = false;
			for (int i = 0; i < value2.Count; i++)
			{
				if (value2[i] == itemViewID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				value2.Add(itemViewID);
			}
			m_TrackedItems_ContainerToItems[containerViewId] = value2;
		}
		return result;
	}

	public List<int> GetTrackedItemsForContainer(ItemContainer itemContainer)
	{
		if (itemContainer == null || itemContainer.NetView == null)
		{
			return null;
		}
		int viewID = itemContainer.NetView.viewID;
		List<int> value = null;
		m_TrackedItems_ContainerToItems.TryGetValue(viewID, out value);
		return value;
	}

	public void ResetLoadState()
	{
		if (T17NetManager.IsMasterClient)
		{
			DataInitialised = true;
			m_LoadState = LOADSTATE.Finished_OK;
			m_LoadError = string.Empty;
		}
		else
		{
			m_LoadState = LOADSTATE.NotStarted;
			m_LoadError = string.Empty;
		}
	}

	public LOADSTATE GetLoadState()
	{
		return m_LoadState;
	}

	public string GetLoadError()
	{
		return m_LoadError;
	}

	public void SendLoadDataToClientRPC(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient && !player.IsLocal)
		{
			if (DataInitialised && m_LoadState == LOADSTATE.Finished_OK && player != PhotonNetwork.player)
			{
				NetItemSaveData netItemSaveData = Serialize(isForLocalSave: false);
				m_NetView.RPC("RPC_RequestStateResponce_Yes", player, netItemSaveData.m_ItemSerializedData.ToArray(), netItemSaveData.m_ItemContainerPairData.ToArray());
			}
			else
			{
				m_NetView.RPC("RPC_RequestStateResponce_No", player);
			}
		}
	}

	[PunRPC]
	public void RPC_RequestStateResponce_Yes(long[] itemData, int[] trackedItemData, PhotonMessageInfo info)
	{
		string error = string.Empty;
		List<long> serializedData = itemData.ToList();
		DeserializeItems(serializedData, ref error);
		List<int> itemContainerPairData = trackedItemData.ToList();
		DeserializeTrackedItems(itemContainerPairData, ref error);
		m_DataInitialised = true;
		m_LoadState = LOADSTATE.Finished_OK;
	}

	[PunRPC]
	public void RPC_RequestStateResponce_No(PhotonMessageInfo info)
	{
		m_LoadError = "ItemManager RPC_RequestStateResponce_No - Error on MasterClient";
		m_LoadState = LOADSTATE.Finished_Error;
	}

	public string CreateSnapshot()
	{
		return JsonUtility.ToJson(Serialize(isForLocalSave: true));
	}

	public void StartedFromSnapshot()
	{
	}

	public NetItemSaveData Serialize(bool isForLocalSave)
	{
		int uXBitLength = 9;
		int uYBitLength = 9;
		FloorManager instance = FloorManager.GetInstance();
		if (instance == null)
		{
			return null;
		}
		instance.GetFloorMetricsBitLength(0, 18, out uXBitLength, out uYBitLength);
		NetItemSaveData saveData = new NetItemSaveData();
		saveData.m_ItemSerializedData.Clear();
		saveData.m_GroundItemOwners.Clear();
		saveData.m_DataInitialised = m_DataInitialised;
		int questItemOwnerIdFilter = -1;
		if (isForLocalSave)
		{
			Player playerObject = Gamer.GetPrimaryGamer().m_PlayerObject;
			if (playerObject != null)
			{
				questItemOwnerIdFilter = playerObject.m_NetView.viewID;
			}
		}
		List<KeyValuePair<Item, int>> list = new List<KeyValuePair<Item, int>>();
		for (int i = 0; i < m_ItemPoolSize; i++)
		{
			Item item = m_UsedItemPool[i];
			if (!(item != null))
			{
				continue;
			}
			T17NetView component = item.GetComponent<T17NetView>();
			if (component != null)
			{
				LevelScript instance2 = LevelScript.GetInstance();
				if ((!(null != item.m_ItemContainer) || !(null != instance2) || !(item.m_ItemContainer == instance2.m_LevelItemContainer)) && T17NetManager.IsValidNetViewId(item.m_ContainerViewID))
				{
					ItemContainer itemContainer = T17NetView.Find<ItemContainer>(item.m_ContainerViewID);
					if (itemContainer != null)
					{
						list.Add(new KeyValuePair<Item, int>(item, itemContainer.FindItemIndex(item)));
						continue;
					}
				}
			}
			SerializeStoreItem(ref saveData, item, instance, uXBitLength, uYBitLength, questItemOwnerIdFilter);
		}
		list.Sort((KeyValuePair<Item, int> x, KeyValuePair<Item, int> y) => x.Value.CompareTo(y.Value));
		for (int j = 0; j < list.Count; j++)
		{
			Item key = list[j].Key;
			if (key != null)
			{
				SerializeStoreItem(ref saveData, key, instance, uXBitLength, uYBitLength, questItemOwnerIdFilter);
			}
		}
		saveData.m_ItemContainerPairData.Clear();
		foreach (KeyValuePair<int, int> item2 in m_TrackedItems_ItemToContainer)
		{
			int key2 = item2.Key;
			int value = item2.Value;
			saveData.m_ItemContainerPairData.Add(key2);
			saveData.m_ItemContainerPairData.Add(value);
		}
		return saveData;
	}

	private void SerializeStoreItem(ref NetItemSaveData saveData, Item thisItem, FloorManager refFloorManager, int iXBits, int iYBits, int questItemOwnerIdFilter)
	{
		ItemLocationType itemLocationType = ItemLocationType.Container;
		T17NetView component = thisItem.GetComponent<T17NetView>();
		if (!(component != null))
		{
			return;
		}
		Character owner = thisItem.GetOwner();
		Item item = null;
		Item item2 = null;
		if (null != owner)
		{
			item = owner.GetOutFit();
			item2 = owner.GetEquippedItem();
		}
		KeyFunctionality keyFunctionality = (KeyFunctionality)thisItem.HasFunctionality(BaseItemFunctionality.Functionality.Key);
		bool flag = keyFunctionality != null;
		LevelScript instance = LevelScript.GetInstance();
		if (null != thisItem.m_ItemContainer && null != instance && thisItem.m_ItemContainer == instance.m_LevelItemContainer)
		{
			itemLocationType = ItemLocationType.Ground;
		}
		else if (T17NetManager.IsValidNetViewId(thisItem.m_ContainerViewID))
		{
			itemLocationType = ItemLocationType.Container;
		}
		else if (null != item && item == thisItem)
		{
			itemLocationType = ItemLocationType.Outfit;
		}
		else if (null != item2 && item2 == thisItem)
		{
			itemLocationType = ItemLocationType.Equipped;
		}
		else
		{
			if (!flag)
			{
				return;
			}
			itemLocationType = ItemLocationType.ManagedKey;
		}
		BitField bitField = new BitField();
		bitField.Set(12, (uint)component.viewID);
		bitField.Set(11, (uint)thisItem.m_ItemData.m_ItemDataID);
		bitField.Set(7, (uint)thisItem.Health);
		bool flag2 = false;
		if (questItemOwnerIdFilter != -1 && thisItem.m_QuestItemOwnerID != questItemOwnerIdFilter)
		{
			flag2 = true;
		}
		bitField.Set(thisItem.m_bIsAQuestItem && !flag2);
		bitField.Set(flag);
		if (flag)
		{
			bitField.Set(6, (uint)keyFunctionality.SubCode);
			bitField.Set(keyFunctionality.IsHidden);
		}
		bitField.Set(3, (uint)itemLocationType);
		switch (itemLocationType)
		{
		case ItemLocationType.Ground:
		{
			int column = 0;
			int row = 0;
			int num = refFloorManager.FindFloorAtZ(thisItem.transform.position.z).m_FloorIndex;
			if (!refFloorManager.GetTileGridPoint(num, FloorManager.TileSystem_Type.TileSystem_Ground, thisItem.transform.position, out row, out column))
			{
				column = 0;
				row = 0;
				num = 0;
			}
			bitField.Set(iXBits, (uint)column);
			bitField.Set(iYBits, (uint)row);
			bitField.Set(4, (uint)num);
			BitField bitField2 = new BitField();
			bitField2.Set(12, component.viewID);
			bitField2.Set(12, thisItem.m_DroppedByCharacterViewID);
			saveData.m_GroundItemOwners.Add((int)(long)bitField2);
			break;
		}
		case ItemLocationType.Container:
			if (0 >= thisItem.m_ContainerViewID)
			{
			}
			bitField.Set(12, (uint)thisItem.m_ContainerViewID);
			bitField.Set(thisItem.m_bHidden);
			break;
		case ItemLocationType.Outfit:
		case ItemLocationType.Equipped:
		{
			int uValue = -1;
			if (null != owner && null != owner.m_NetView)
			{
				uValue = owner.m_NetView.viewID;
			}
			bitField.Set(12, (uint)uValue);
			break;
		}
		}
		saveData.m_ItemSerializedData.Add((long)bitField);
	}

	public static void ValidateItems()
	{
		int num = 0;
		int num2 = 0;
		FloorManager instance = FloorManager.GetInstance();
		if (instance == null || s_Instance == null)
		{
			return;
		}
		ItemLocationType itemLocationType = ItemLocationType.Container;
		LevelScript instance2 = LevelScript.GetInstance();
		for (int i = 0; i < s_Instance.m_ItemPoolSize; i++)
		{
			Item item = s_Instance.m_UsedItemPool[i];
			if (!(item != null))
			{
				continue;
			}
			num2++;
			T17NetView netView = item.m_NetView;
			string itemName = s_Instance.m_UsedItemPool[i].ItemName;
			if (netView != null)
			{
				Character owner = item.GetOwner();
				Item item2 = null;
				Item item3 = null;
				if (null != owner)
				{
					item2 = owner.GetOutFit();
					item3 = owner.GetEquippedItem();
				}
				KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				bool flag = keyFunctionality != null;
				if (null != item.m_ItemContainer && null != instance2 && item.m_ItemContainer == instance2.m_LevelItemContainer)
				{
					itemLocationType = ItemLocationType.Ground;
				}
				else if (T17NetManager.IsValidNetViewId(item.m_ContainerViewID))
				{
					itemLocationType = ItemLocationType.Container;
				}
				else if (null != item2 && item2 == item)
				{
					itemLocationType = ItemLocationType.Outfit;
				}
				else if (null != item3 && item3 == item)
				{
					itemLocationType = ItemLocationType.Equipped;
				}
				else
				{
					if (!flag)
					{
						num++;
						continue;
					}
					itemLocationType = ItemLocationType.ManagedKey;
				}
				switch (itemLocationType)
				{
				case ItemLocationType.Ground:
				{
					int column = 0;
					int row = 0;
					int floorIndex = instance.FindFloorAtZ(s_Instance.m_UsedItemPool[i].transform.position.z).m_FloorIndex;
					if (!instance.GetTileGridPoint(floorIndex, FloorManager.TileSystem_Type.TileSystem_Ground, s_Instance.m_UsedItemPool[i].transform.position, out row, out column))
					{
						num++;
					}
					break;
				}
				case ItemLocationType.Container:
					if (0 >= item.m_ContainerViewID)
					{
						num++;
					}
					break;
				case ItemLocationType.Outfit:
				case ItemLocationType.Equipped:
					if (null == owner || null == owner.m_NetView)
					{
						num++;
					}
					break;
				}
			}
			else
			{
				num++;
			}
		}
		if (num <= 0)
		{
		}
	}

	public string GetSerializationData()
	{
		return m_SaveData.GetSaveData();
	}

	public bool Deserialize(string data, ref string error)
	{
		DeserializeSave(data);
		return true;
	}

	public void DeserializeSave(string serializedData)
	{
		if (!string.IsNullOrEmpty(serializedData))
		{
			NetItemSaveData netItemSaveData = null;
			try
			{
				netItemSaveData = JsonUtility.FromJson<NetItemSaveData>(serializedData);
			}
			catch
			{
				return;
			}
			string error = string.Empty;
			DeserializeItems(netItemSaveData.m_ItemSerializedData, ref error);
			DeserializeTrackedItems(netItemSaveData.m_ItemContainerPairData, ref error);
			DeserializeGroundItems(netItemSaveData.m_GroundItemOwners, ref error);
		}
	}

	public bool DeserializeItems(List<long> serializedData, ref string error)
	{
		bool result = true;
		if (serializedData != null)
		{
			int uXBitLength = 10;
			int uYBitLength = 10;
			FloorManager instance = FloorManager.GetInstance();
			if (instance == null)
			{
				return false;
			}
			instance.GetFloorMetricsBitLength(0, 20, out uXBitLength, out uYBitLength);
			int keySubCode = 0;
			bool keyHidden = false;
			int column = 0;
			int row = 0;
			int index = 0;
			int viewID = 0;
			bool bHidden = false;
			MeshRenderer meshRenderer = null;
			BoxCollider boxCollider = null;
			int viewID2 = -1;
			for (int i = 0; i < serializedData.Count; i++)
			{
				long num = serializedData[i];
				if (num == 0)
				{
					continue;
				}
				BitField bitField = new BitField((ulong)num);
				int uInt = (int)bitField.GetUInt(12);
				int itemDataID = (int)bitField.GetUInt(11);
				int uInt2 = (int)bitField.GetUInt(7);
				bool @bool = bitField.GetBool();
				bool bool2 = bitField.GetBool();
				if (bool2)
				{
					keySubCode = (int)bitField.GetUInt(6);
					keyHidden = bitField.GetBool();
				}
				ItemLocationType uInt3 = (ItemLocationType)bitField.GetUInt(3);
				switch (uInt3)
				{
				case ItemLocationType.Container:
					viewID = (int)bitField.GetUInt(12);
					bHidden = bitField.GetBool();
					break;
				case ItemLocationType.Outfit:
				case ItemLocationType.Equipped:
					viewID2 = (int)bitField.GetUInt(12);
					break;
				case ItemLocationType.Ground:
					column = (int)bitField.GetUInt(uXBitLength);
					row = (int)bitField.GetUInt(uYBitLength);
					index = (int)bitField.GetUInt(4);
					break;
				}
				int num2 = uInt - T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart);
				Item item = null;
				if (m_UsedItemPool[num2] == null)
				{
					TakeFreeItem(num2);
				}
				item = m_UsedItemPool[num2];
				if (!(null != item))
				{
					continue;
				}
				item.transform.SetParent(base.transform);
				AssignDataToItem(ref item, ref itemDataID, 0, isLateJoin: true);
				if (!(item != null))
				{
					continue;
				}
				item.gameObject.name = item.m_ItemData.m_ItemLocalizationTag + "_" + num2;
				item.m_ItemData.m_ItemHealth = uInt2;
				item.m_bIsAQuestItem = @bool;
				if (bool2)
				{
					KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
					if (keyFunctionality != null)
					{
						keyFunctionality.SetKeySubCode(keySubCode);
						keyFunctionality.SetKeyHidden(keyHidden);
					}
				}
				meshRenderer = item.MeshRendererProp;
				boxCollider = item.BoxColliderProp;
				switch (uInt3)
				{
				case ItemLocationType.Ground:
				{
					instance.GetTileCentrePosition(instance.FindFloorbyIndex(index), FloorManager.TileSystem_Type.TileSystem_Ground, row, column, out var worldPosition);
					item.transform.position = worldPosition + new Vector3(0f, 0f, -0.1f);
					LevelScript.GetInstance().m_LevelItemContainer.LOCAL_AddItemToContainer(item, bHidden);
					if (item.TrackableUIElementReporter == null)
					{
						TrackableUIElementsReporter trackableUIElementReporter = item.gameObject.AddComponent<TrackableUIElementsReporter>();
						item.SetTrackableUIElementReporter(trackableUIElementReporter);
					}
					if (item.TrackableUIElementReporter != null)
					{
						string localized = string.Empty;
						Localization.Get(item.ItemName, out localized);
						item.TrackableUIElementReporter.SetDisplayName((!string.IsNullOrEmpty(localized)) ? localized : item.ItemName);
					}
					item.AddToCullingSystem();
					item.ItemDropped();
					break;
				}
				case ItemLocationType.Container:
				{
					ItemContainer itemContainer = T17NetView.Find<ItemContainer>(viewID);
					if (itemContainer != null)
					{
						itemContainer.LOCAL_AddItemToContainer(item, bHidden);
					}
					else
					{
						result = false;
					}
					break;
				}
				case ItemLocationType.Outfit:
				{
					Character character2 = T17NetView.Find<Character>(viewID2);
					if (null != character2)
					{
						character2.SetOutFit(item, bTellOthers: false, bAddOldToInventory: false);
					}
					else
					{
						result = false;
					}
					break;
				}
				case ItemLocationType.Equipped:
				{
					Character character = T17NetView.Find<Character>(viewID2);
					if (null != character)
					{
						character.SetEquippedItem(item, bTellOthers: false, bAddOldToInventory: false);
					}
					else
					{
						result = false;
					}
					break;
				}
				}
				bool flag = uInt3 == ItemLocationType.Ground;
				if (meshRenderer != null)
				{
					meshRenderer.enabled = flag;
				}
				if (boxCollider != null)
				{
					boxCollider.enabled = flag;
				}
				UpdateManager.GetInstance().Register(item, UpdateCategory.Items);
			}
		}
		else
		{
			result = false;
			error = "ItemManager: JSON data returned a null object.";
		}
		return result;
	}

	private void DeserializeTrackedItems(List<int> itemContainerPairData, ref string error)
	{
		bool flag = false;
		if (itemContainerPairData != null)
		{
			for (int i = 0; i < itemContainerPairData.Count; i += 2)
			{
				int itemViewID = itemContainerPairData[i];
				int containerViewId = itemContainerPairData[i + 1];
				flag &= TrackItem(itemViewID, containerViewId);
			}
		}
		if (flag)
		{
			error += "ItemManager: JSON data contained invalid item tracking data";
		}
	}

	private void DeserializeGroundItems(List<int> groundData, ref string error)
	{
		if (groundData == null)
		{
			return;
		}
		for (int i = 0; i < groundData.Count; i++)
		{
			int num = groundData[i];
			if (num != 0)
			{
				BitField bitField = new BitField((ulong)num);
				int @int = bitField.GetInt(12);
				int int2 = bitField.GetInt(12);
				int num2 = @int - T17NetConfig.GetReservedNetID(T17NetConfig.ReservedNetID.ItemMgrStart);
				Item item = m_UsedItemPool[num2];
				if (null != item)
				{
					item.SetContrabandItemDropped(int2);
				}
			}
		}
	}

	public List<ItemData> GetAllowedList()
	{
		return m_AllowedItems;
	}

	public List<ItemData> GetKeyList()
	{
		return m_KeyItems;
	}

	public Item[] GetItems()
	{
		return m_ItemPool;
	}
}
