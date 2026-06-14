using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerManager : MonoBehaviour
{
	public SpawnItemLimits m_SpawnItemLimits;

	private Dictionary<ItemContainer.ItemContainerType, List<ItemContainer>> m_AllItemContainers;

	private Dictionary<int, List<ItemContainer>> m_CachedItemIDToDeskMap = new Dictionary<int, List<ItemContainer>>();

	private static ItemContainerManager m_Instance;

	public static ItemContainerManager GetInstance()
	{
		return m_Instance;
	}

	private void Awake()
	{
		m_Instance = this;
		if (m_AllItemContainers == null)
		{
			m_AllItemContainers = new Dictionary<ItemContainer.ItemContainerType, List<ItemContainer>>();
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_Instance == this)
		{
			m_Instance = null;
		}
	}

	private void Start()
	{
	}

	public void AddItemContainer(ItemContainer itemContainer, ItemContainer.ItemContainerType type)
	{
		if (m_AllItemContainers.ContainsKey(type))
		{
			m_AllItemContainers[type].Add(itemContainer);
			return;
		}
		m_AllItemContainers.Add(type, new List<ItemContainer>());
		m_AllItemContainers[type].Add(itemContainer);
	}

	public void RemoveItemContainer(ItemContainer itemContainer, ItemContainer.ItemContainerType type)
	{
		if (m_AllItemContainers.ContainsKey(type))
		{
			m_AllItemContainers[type].Remove(itemContainer);
		}
	}

	public void RecordDeskItem(int itemID, ItemContainer itemContainer)
	{
		if (itemContainer.IsDesk())
		{
			if (!m_CachedItemIDToDeskMap.ContainsKey(itemID))
			{
				m_CachedItemIDToDeskMap.Add(itemID, new List<ItemContainer>());
			}
			m_CachedItemIDToDeskMap[itemID].Add(itemContainer);
		}
	}

	public void RefreshAllItemContainers(bool stagger, bool bUpdateNetworkService = false)
	{
		if (!stagger)
		{
			PreRefreshItemContainers();
			foreach (KeyValuePair<ItemContainer.ItemContainerType, List<ItemContainer>> allItemContainer in m_AllItemContainers)
			{
				for (int i = 0; i < allItemContainer.Value.Count; i++)
				{
					if (bUpdateNetworkService)
					{
						GlobalStart.TimedNetworkService();
					}
					if (allItemContainer.Value[i] != null)
					{
						allItemContainer.Value[i].ConsiderRefreshingItems();
					}
				}
			}
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			if (T17NetManager.IsMasterClient)
			{
				ConstructEndgameInteraction.SpawnAllNecessaryItems_Classic();
			}
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
			PostRefreshItemContainers();
			if (bUpdateNetworkService)
			{
				GlobalStart.TimedNetworkService();
			}
		}
		else
		{
			StartCoroutine(StaggeredRefreshAllItemContainers());
		}
	}

	private IEnumerator StaggeredRefreshAllItemContainers()
	{
		PreRefreshItemContainers();
		int numberRefreshesDone = 0;
		foreach (KeyValuePair<ItemContainer.ItemContainerType, List<ItemContainer>> entry in m_AllItemContainers)
		{
			for (int i = 0; i < entry.Value.Count; i++)
			{
				if (entry.Value[i] != null && entry.Value[i].ConsiderRefreshingItems())
				{
					numberRefreshesDone++;
					if (numberRefreshesDone >= 50)
					{
						numberRefreshesDone = 0;
						yield return new WaitForSecondsRealtime(2f);
					}
				}
			}
		}
		if (T17NetManager.IsMasterClient)
		{
			ConstructEndgameInteraction.SpawnAllNecessaryItems_Classic();
		}
		PostRefreshItemContainers();
	}

	private void PreRefreshItemContainers()
	{
		m_CachedItemIDToDeskMap.Clear();
	}

	private void PostRefreshItemContainers()
	{
		if (!(m_SpawnItemLimits != null) || m_SpawnItemLimits.m_ItemLimits == null)
		{
			return;
		}
		for (int i = 0; i < m_SpawnItemLimits.m_ItemLimits.Count; i++)
		{
			ItemData item = m_SpawnItemLimits.m_ItemLimits[i].m_Item;
			if (!(item != null))
			{
				continue;
			}
			List<ItemContainer> list = null;
			if (m_CachedItemIDToDeskMap.ContainsKey(item.m_ItemDataID))
			{
				list = m_CachedItemIDToDeskMap[item.m_ItemDataID];
			}
			int num = list?.Count ?? 0;
			if (num < m_SpawnItemLimits.m_ItemLimits[i].m_Min)
			{
				int num2 = m_SpawnItemLimits.m_ItemLimits[i].m_Min - num;
				for (int j = 0; j < num2; j++)
				{
					int requestID = -1;
					ItemManager.GetInstance().AssignItemRPC(0, item.m_ItemDataID, OnSpawnLimitedItem, ref requestID);
				}
			}
			else
			{
				if (num <= m_SpawnItemLimits.m_ItemLimits[i].m_Max)
				{
					continue;
				}
				int num3 = num - m_SpawnItemLimits.m_ItemLimits[i].m_Max;
				for (int k = 0; k < num3; k++)
				{
					int index = Random.Range(0, list.Count);
					Item firstItemWithItemID = list[index].GetFirstItemWithItemID(item.m_ItemDataID);
					if (firstItemWithItemID != null && !firstItemWithItemID.IsQuestItem())
					{
						list[index].RemoveItemRPC(firstItemWithItemID, releaseToManager: true);
					}
					list.RemoveAt(index);
				}
			}
		}
	}

	private void OnSpawnLimitedItem(Item item, int eventID)
	{
		if (!(item != null))
		{
			return;
		}
		ItemContainer randomNonFullDeskContainer = GetRandomNonFullDeskContainer();
		if (randomNonFullDeskContainer != null)
		{
			if (randomNonFullDeskContainer.AddItemRPC(item))
			{
				RecordDeskItem(item.ItemDataID, randomNonFullDeskContainer);
			}
			else
			{
				ItemManager.GetInstance().RequestReleaseItem(item);
			}
		}
	}

	public void PostSpawnPlayers_ApplyConfigs()
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		foreach (KeyValuePair<ItemContainer.ItemContainerType, List<ItemContainer>> allItemContainer in m_AllItemContainers)
		{
			GlobalStart.TimedNetworkService();
			for (int i = 0; i < allItemContainer.Value.Count; i++)
			{
				if (allItemContainer.Value[i] != null)
				{
					ItemContainerConfig itemContainerOverride = instance.GetItemContainerOverride(allItemContainer.Value[i]);
					if (itemContainerOverride != null)
					{
						allItemContainer.Value[i].ApplyContainerConfig(itemContainerOverride);
					}
				}
			}
		}
	}

	private ItemContainer GetRandomNonFullDeskContainer()
	{
		List<ItemContainer> list = new List<ItemContainer>();
		list.AddRange(GetNonFullContainersOfType(ItemContainer.ItemContainerType.Desk));
		list.AddRange(GetNonFullContainersOfType(ItemContainer.ItemContainerType.DeskInmate));
		list.AddRange(GetNonFullContainersOfType(ItemContainer.ItemContainerType.DeskGuard));
		if (list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}

	private List<ItemContainer> GetNonFullContainersOfType(ItemContainer.ItemContainerType type)
	{
		List<ItemContainer> list = new List<ItemContainer>();
		if (m_AllItemContainers.ContainsKey(type))
		{
			for (int i = 0; i < m_AllItemContainers[type].Count; i++)
			{
				ItemContainer itemContainer = m_AllItemContainers[type][i];
				if (itemContainer != null && !itemContainer.IsVisibleFull())
				{
					list.Add(itemContainer);
				}
			}
		}
		return list;
	}

	public ItemContainer GetAnyQuestableDeskContainer(List<ItemContainer> exclude = null)
	{
		ItemContainer itemContainer = null;
		List<ItemContainer> list = new List<ItemContainer>();
		itemContainer = GetQuestableContainer(ItemContainer.ItemContainerType.Desk, exclude);
		if (itemContainer != null)
		{
			list.Add(itemContainer);
		}
		itemContainer = GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, exclude);
		if (itemContainer != null)
		{
			list.Add(itemContainer);
		}
		itemContainer = GetQuestableContainer(ItemContainer.ItemContainerType.DeskGuard, exclude);
		if (itemContainer != null)
		{
			list.Add(itemContainer);
		}
		if (list.Count > 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}

	public ItemContainer GetQuestableContainer(ItemContainer.ItemContainerType type, List<ItemContainer> exclude = null)
	{
		if (m_AllItemContainers.ContainsKey(type))
		{
			List<int> list = new List<int>();
			for (int i = 0; i < m_AllItemContainers[type].Count; i++)
			{
				ItemContainer itemContainer = m_AllItemContainers[type][i];
				switch (type)
				{
				case ItemContainer.ItemContainerType.Guard:
					if (itemContainer.GetCharacterOwner() != null)
					{
						AICharacter_Guard component = itemContainer.GetCharacterOwner().GetComponent<AICharacter_Guard>();
						if (component != null && (int)component.m_ActiveAlertness >= 6)
						{
							continue;
						}
					}
					break;
				case ItemContainer.ItemContainerType.DeskInmate:
					if (itemContainer.GetCharacterOwner() == null)
					{
						continue;
					}
					break;
				}
				if (itemContainer.m_CanBeUsedForQuests && !itemContainer.IsVisibleFull() && (exclude == null || !exclude.Contains(itemContainer)))
				{
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				int index = Random.Range(0, list.Count);
				return m_AllItemContainers[type][list[index]];
			}
		}
		return null;
	}

	public List<ItemContainer> DebugFindContainersWithItem(int itemID)
	{
		List<ItemContainer> list = new List<ItemContainer>();
		List<Item> items = new List<Item>();
		foreach (KeyValuePair<ItemContainer.ItemContainerType, List<ItemContainer>> allItemContainer in m_AllItemContainers)
		{
			for (int i = 0; i < allItemContainer.Value.Count; i++)
			{
				ItemContainer itemContainer = allItemContainer.Value[i];
				itemContainer.GetItems(ref items);
				for (int j = 0; j < items.Count; j++)
				{
					if (items[j].ItemDataID == itemID)
					{
						list.Add(itemContainer);
					}
				}
			}
		}
		return list;
	}
}
