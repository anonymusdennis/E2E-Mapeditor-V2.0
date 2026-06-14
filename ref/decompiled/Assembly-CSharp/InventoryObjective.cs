using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class InventoryObjective : BaseObjective
{
	[SerializeField]
	public List<ItemData> m_ItemsNeeded = new List<ItemData>(1);

	public bool m_bFailIfItemDestroyed;

	private Dictionary<int, int> m_CachedItemsAndQuantities;

	private Dictionary<int, int> m_HasItemsCache;

	private ItemContainer m_ContainerToCheck;

	private List<Item> m_ActualItems = new List<Item>(1);

	private const string ITEMONETOKEN = "$InventoryItemOne";

	private const string ITEMTWOTOKEN = "$InventoryItemTwo";

	private const string ITEMTHREETOKEN = "$InventoryItemThree";

	~InventoryObjective()
	{
		if (m_PlayerOwner != null)
		{
			m_PlayerOwner.EquippedItemChangedEvent -= OnPlayerOwner_EquippedItemChangedEvent;
		}
	}

	protected override void Child_PickAllTargets()
	{
		m_ContainerToCheck = m_PlayerOwner.m_ItemContainer;
		if (m_ContainerToCheck != null)
		{
			ItemContainer containerToCheck = m_ContainerToCheck;
			containerToCheck.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Combine(containerToCheck.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnItemAddedToContainer));
		}
		m_PlayerOwner.EquippedItemChangedEvent += OnPlayerOwner_EquippedItemChangedEvent;
		UpdateTokens();
	}

	private void UpdateTokens()
	{
		for (int i = 0; i < m_ItemsNeeded.Count && i < 3; i++)
		{
			string localized = string.Empty;
			Localization.Get(m_ItemsNeeded[i].m_ItemLocalizationTag, out localized);
			switch (i)
			{
			case 0:
				InternalTokenUpdate("$InventoryItemOne", localized, string.Empty);
				break;
			case 1:
				InternalTokenUpdate("$InventoryItemTwo", localized, string.Empty);
				break;
			case 2:
				InternalTokenUpdate("$InventoryItemThree", localized, string.Empty);
				break;
			}
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$InventoryItemOne", Localization.TokenReplaceType.Item);
		AddTokenInternal("$InventoryItemTwo", Localization.TokenReplaceType.Item);
		AddTokenInternal("$InventoryItemThree", Localization.TokenReplaceType.Item);
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_PreAction()
	{
		m_ActualItems.Clear();
		List<Item> itemList = new List<Item>();
		for (int i = 0; i < m_ItemsNeeded.Count; i++)
		{
			if (!(m_ItemsNeeded[i] != null))
			{
				continue;
			}
			if (m_CachedItemsAndQuantities.ContainsKey(m_ItemsNeeded[i].m_ItemDataID))
			{
				m_CachedItemsAndQuantities[m_ItemsNeeded[i].m_ItemDataID]++;
			}
			else
			{
				m_CachedItemsAndQuantities[m_ItemsNeeded[i].m_ItemDataID] = 1;
			}
			if (m_ContainerToCheck != null)
			{
				m_ContainerToCheck.GetItemsWithItemID(ref itemList, m_ItemsNeeded[i].m_ItemDataID, m_ItemsNeeded.Count);
				for (int j = 0; j < itemList.Count; j++)
				{
					m_ActualItems.Add(itemList[i]);
				}
			}
			Item equippedItem = m_PlayerOwner.GetEquippedItem();
			if (equippedItem != null && equippedItem.m_ItemData.m_ItemDataID == m_ItemsNeeded[i].m_ItemDataID && !m_ActualItems.Contains(equippedItem))
			{
				m_ActualItems.Add(equippedItem);
			}
		}
		ItemManager instance = ItemManager.GetInstance();
		if (instance != null)
		{
			instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
			instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Combine(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		}
	}

	protected override void Child_Initialize()
	{
		if (m_HasItemsCache == null)
		{
			m_HasItemsCache = new Dictionary<int, int>();
		}
		m_HasItemsCache.Clear();
		if (m_CachedItemsAndQuantities == null)
		{
			m_CachedItemsAndQuantities = new Dictionary<int, int>();
		}
		m_CachedItemsAndQuantities.Clear();
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_ObjectiveStatus == ObjectiveStatus.Failed || m_ObjectiveStatus == ObjectiveStatus.Canceled)
		{
			return false;
		}
		bool result = true;
		int num = -1;
		if (m_PlayerOwner != null && m_PlayerOwner.GetEquippedItem() != null)
		{
			num = m_PlayerOwner.GetEquippedItem().ItemDataID;
		}
		if (m_ContainerToCheck != null && m_CachedItemsAndQuantities.Count > 0)
		{
			foreach (KeyValuePair<int, int> cachedItemsAndQuantity in m_CachedItemsAndQuantities)
			{
				int num2 = m_ContainerToCheck.HasItem(cachedItemsAndQuantity.Key);
				if (num == cachedItemsAndQuantity.Key)
				{
					num2++;
				}
				if (num2 >= cachedItemsAndQuantity.Value)
				{
					m_HasItemsCache[cachedItemsAndQuantity.Key] = cachedItemsAndQuantity.Value;
					continue;
				}
				result = false;
				m_HasItemsCache[cachedItemsAndQuantity.Key] = num2;
			}
		}
		return result;
	}

	private void OnQuestItemDestroyed(Item item, int eventID)
	{
		if (!m_bFailIfItemDestroyed || m_ActualItems == null)
		{
			return;
		}
		for (int i = 0; i < m_ActualItems.Count; i++)
		{
			if (m_ActualItems[i].m_NetView.viewID == item.m_NetView.viewID)
			{
				m_ActualItems.RemoveAt(i);
				break;
			}
		}
		int num = 0;
		for (int j = 0; j < m_ActualItems.Count; j++)
		{
			if (m_ActualItems[j].m_ItemData.m_ItemDataID == item.m_ItemData.m_ItemDataID)
			{
				num++;
			}
		}
		if (num < m_CachedItemsAndQuantities[item.m_ItemData.m_ItemDataID])
		{
			m_ObjectiveStatus = ObjectiveStatus.Failed;
		}
	}

	private void OnItemAddedToContainer(ItemContainer container, Item item, bool intoHidden)
	{
		if (container == m_ContainerToCheck && !m_ActualItems.Contains(item))
		{
			m_ActualItems.Add(item);
		}
	}

	private void OnPlayerOwner_EquippedItemChangedEvent(Character character, Item equippedItem)
	{
		if (m_PlayerOwner == character && equippedItem != null && !m_ActualItems.Contains(equippedItem))
		{
			m_ActualItems.Add(equippedItem);
		}
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
		if (m_ContainerToCheck != null)
		{
			ItemContainer containerToCheck = m_ContainerToCheck;
			containerToCheck.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(containerToCheck.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnItemAddedToContainer));
		}
		if (m_PlayerOwner != null)
		{
			m_PlayerOwner.EquippedItemChangedEvent -= OnPlayerOwner_EquippedItemChangedEvent;
		}
		ItemManager instance = ItemManager.GetInstance();
		instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		m_ActualItems.Clear();
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		JProperty jProperty = new JProperty("ItemsNeeded");
		JArray jArray = new JArray();
		for (int i = 0; i < m_ItemsNeeded.Count; i++)
		{
			jArray.Add(m_ItemsNeeded[i].m_ItemDataID);
		}
		jProperty.Add(jArray);
		baseObj.Add(jProperty);
		baseObj.Add(new JProperty("fail", m_bFailIfItemDestroyed));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		JProperty jProperty = json.Property("ItemsNeeded");
		if (jProperty == null || jProperty.Value.Type != JTokenType.Array)
		{
			return;
		}
		m_ItemsNeeded.Clear();
		JArray source = (JArray)jProperty.Value;
		List<int> itemsNeededIds = source.Select((JToken c) => (int)c).ToList();
		List<ItemData> source2 = Resources.LoadAll<ItemData>("Prefabs/Items").ToList();
		for (int i = 0; i < itemsNeededIds.Count; i++)
		{
			m_ItemsNeeded.Add(source2.FirstOrDefault((ItemData id) => id.m_ItemDataID == itemsNeededIds[i]));
		}
		if (ingameLoad)
		{
			m_ContainerToCheck = m_PlayerOwner.m_ItemContainer;
			UpdateTokens();
		}
		JProperty jProperty2 = json.Property("fail");
		if (jProperty2 != null)
		{
			m_bFailIfItemDestroyed = (bool)jProperty2.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.InventoryObjective;
	}
}
