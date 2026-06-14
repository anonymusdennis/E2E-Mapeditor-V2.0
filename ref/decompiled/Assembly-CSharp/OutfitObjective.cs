using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class OutfitObjective : BaseObjective
{
	public enum OutfitObjectiveType
	{
		HaveInInventory,
		WearOutfit,
		CleanOutfit
	}

	public OutfitObjectiveType m_OutfitObjectiveType = OutfitObjectiveType.WearOutfit;

	public ItemData m_OutfitItemData;

	public RandomItemGroup m_RandomItemGroup;

	public bool m_bSpawnOutfitInPlayerInventory;

	private Item m_SpawnedOutfit;

	private const string OUTFITTOKEN = "$Outfit";

	private int m_QuestItemGroupID = -1;

	private int m_ItemSpawnEventID = -1;

	protected override void Child_PickAllTargets()
	{
		if ((m_RandomItemGroup != null || m_OutfitItemData == null) && ItemManager.GetInstance() != null)
		{
			if (m_RandomItemGroup != null)
			{
				m_OutfitItemData = m_RandomItemGroup.GetRandomItem(bUniqueItems: true);
			}
			else
			{
				m_OutfitItemData = ItemManager.GetInstance().GetRandomItemFromAllowedList();
			}
		}
		Localization.Get(m_OutfitItemData.m_ItemLocalizationTag, out var localized);
		InternalTokenUpdate("$Outfit", localized, string.Empty);
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$Outfit", Localization.TokenReplaceType.Item);
	}

	protected override void Child_Reset()
	{
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		ItemManager instance = ItemManager.GetInstance();
		instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		ItemManager instance2 = ItemManager.GetInstance();
		instance2.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Combine(instance2.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		if (m_PlayerOwner.m_ItemContainer != null && m_bSpawnOutfitInPlayerInventory && m_SpawnedOutfit == null)
		{
			ItemManager.GetInstance().AssignItemRPC(-1, m_OutfitItemData.m_ItemDataID, OnItemMgrResponse, ref m_ItemSpawnEventID);
		}
	}

	private void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (newItem != null && m_ItemSpawnEventID == eventID)
		{
			newItem.SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
			m_SpawnedOutfit = newItem;
			if (!m_PlayerOwner.m_ItemContainer.AddItemRPC(newItem))
			{
				newItem.DropItemInLevel(m_PlayerOwner, m_PlayerOwner.transform.position);
			}
			m_ItemSpawnEventID = -1;
		}
	}

	private void OnQuestItemDestroyed(Item item, int eventID)
	{
		if (!m_bInPostAction && m_SpawnedOutfit != null && m_SpawnedOutfit.m_NetView.viewID == item.m_NetView.viewID)
		{
			SetHUDPins(on: false);
			SetHUDArrow(on: false);
			m_ObjectiveStatus = ObjectiveStatus.Failed;
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		switch (m_OutfitObjectiveType)
		{
		case OutfitObjectiveType.HaveInInventory:
			if (m_PlayerOwner != null && m_PlayerOwner.m_ItemContainer != null && m_PlayerOwner.m_ItemContainer.HasItem(m_OutfitItemData.m_ItemDataID) > 0)
			{
				return true;
			}
			break;
		case OutfitObjectiveType.WearOutfit:
			if (m_PlayerOwner != null)
			{
				Item outFit = m_PlayerOwner.GetOutFit();
				if (outFit != null && outFit.m_ItemData.m_ItemDataID == m_OutfitItemData.m_ItemDataID)
				{
					return true;
				}
			}
			break;
		}
		return false;
	}

	protected override void Child_SetHUDPins(bool on)
	{
	}

	protected override void Child_SetHUDArrow(bool on)
	{
	}

	protected override void Child_PostAction()
	{
		ItemManager instance = ItemManager.GetInstance();
		instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		if (!(m_SpawnedOutfit != null) || !(m_PlayerOwner != null))
		{
			return;
		}
		if (m_SpawnedOutfit.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID && m_SpawnedOutfit != m_PlayerOwner.GetEquippedItem() && m_SpawnedOutfit != m_PlayerOwner.GetOutFit())
		{
			ItemContainer component = PhotonView.Find(m_SpawnedOutfit.m_ContainerViewID).GetComponent<ItemContainer>();
			if (component != null)
			{
				component.RemoveItemRPC(m_SpawnedOutfit, releaseToManager: true);
			}
		}
		else
		{
			m_SpawnedOutfit.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
		}
		m_SpawnedOutfit = null;
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("QuestItemGroupID", m_QuestItemGroupID));
			if (m_SpawnedOutfit != null)
			{
				baseObj.Add(new JProperty("SpawnedOutfit", m_SpawnedOutfit.m_NetView.viewID));
			}
		}
		baseObj.Add(new JProperty("OutfitObjType", (int)m_OutfitObjectiveType));
		if (m_OutfitItemData != null)
		{
			baseObj.Add(new JProperty("OutfitItemData", m_OutfitItemData.m_ItemDataID));
		}
		baseObj.Add(new JProperty("SpawnInPlayerInventory", m_bSpawnOutfitInPlayerInventory));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("QuestItemGroupID");
			if (jProperty != null)
			{
				m_QuestItemGroupID = (int)jProperty.Value;
			}
			JProperty jProperty2 = json.Property("SpawnedOutfit");
			if (jProperty2 != null)
			{
				int viewID = (int)jProperty2.Value;
				m_SpawnedOutfit = PhotonView.Find(viewID).GetComponent<Item>();
				m_SpawnedOutfit.LOADING_SetQuestGroupID(m_PlayerOwner, m_QuestItemGroupID);
			}
			Localization.Get(m_SpawnedOutfit.m_ItemData.m_ItemLocalizationTag, out var localized);
			InternalTokenUpdate("$Outfit", localized, string.Empty);
		}
		m_OutfitObjectiveType = (OutfitObjectiveType)(int)json.Property("OutfitObjType").Value;
		JProperty jProperty3 = json.Property("OutfitItemData");
		if (jProperty3 != null)
		{
			int itemID = (int)jProperty3.Value;
			m_OutfitItemData = Resources.LoadAll<ItemData>("Prefabs/Items").ToList().FirstOrDefault((ItemData id) => id.m_ItemDataID == itemID);
		}
		m_bSpawnOutfitInPlayerInventory = (bool)json.Property("SpawnInPlayerInventory").Value;
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.OutfitObjective;
	}
}
