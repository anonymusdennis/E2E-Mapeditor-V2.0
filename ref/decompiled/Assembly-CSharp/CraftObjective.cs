using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class CraftObjective : BaseObjective
{
	public enum CraftDelivery
	{
		KeepYourself,
		GiveToQuestGiver,
		GiveToRandomInmate,
		GiveToRandomGuard,
		PutInRandomDesk
	}

	public enum State
	{
		Setup,
		CollectItems,
		WaitForCollectItems,
		CraftItem,
		WaitForCraftItems,
		DeliverItem,
		WaitForTickComplete,
		WaitForDeliverItem,
		Finished
	}

	public bool m_bRandomItem;

	public bool m_bSpawnItemsForCrafting = true;

	public ItemData m_ItemToCraft;

	public RandomItemGroup m_RandomItemGroup;

	public string m_CollectIngrOneText = "Text.Quest.Collect";

	public string m_CollectIngrTwoText = "Text.Quest.Collect";

	public string m_CollectIngrThreeText = "Text.Quest.Collect";

	public string m_CraftObjectiveText = "Text.Quest.Craft";

	public string m_DeliverObjectiveText = "Text.Quest.Deliver";

	public bool m_bResetOnFail;

	public bool m_bResetOnItemsInContraband;

	public int m_IndexToResetTo = -1;

	public CraftDelivery m_CraftDeliveryType;

	private State m_State;

	private bool m_bCraftedItem;

	private bool m_bDeliveredItem;

	private bool m_bHudPinsOn;

	private bool m_bHUDArrowsOn;

	private Item m_CurrentArrowItem;

	private int m_CurrentArrowItemContainerID = -1;

	private Item m_CraftedItem;

	private CraftManager.Recipe m_CraftingRecipe;

	private Dictionary<Item, int> m_ItemContainers;

	private ItemContainer m_TargetToDeliverTo;

	private int m_WaitForFailFrames = -1;

	private int m_DeliverTargetPin = -1;

	private bool m_bDeliverPinCreated;

	private bool m_bDeliveredToTarget;

	private ObjectiveSubGoalHUD[] m_HUDRefList;

	private Dictionary<int, int> m_HUDInfoBindings;

	private Dictionary<int, int> m_HUDPinBindings;

	private const string TOCRAFTTOKEN = "$ItemToCraft";

	private const string DELIVERTOTOKEN = "$DeliverTo";

	private const string INGRBASETOKEN = "$Ingr";

	private const string INGRONETOKEN = "$IngrOne";

	private const string INGRTWOTOKEN = "$IngrTwo";

	private const string INGRTHREETOKEN = "$IngrThree";

	private int m_QuestItemGroupID = -1;

	private List<int> m_SafetyCheck = new List<int>();

	private List<int> m_SafetyCheckResponse = new List<int>();

	private int m_ImmediateSpawnCheck = -1;

	private ItemContainer m_ContrabandDeskItemContainer;

	protected override void Child_PickAllTargets()
	{
		string localized = "Text.Quest.Desk";
		Localization.Get("Text.Quest.Desk", out localized);
		Player playerOwner = m_PlayerOwner;
		playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
		switch (m_CraftDeliveryType)
		{
		case CraftDelivery.KeepYourself:
			m_TargetToDeliverTo = m_PlayerOwner.m_ItemContainer;
			break;
		case CraftDelivery.GiveToQuestGiver:
			m_TargetToDeliverTo = m_QuestGiver.m_ItemContainer;
			break;
		case CraftDelivery.GiveToRandomInmate:
			m_TargetToDeliverTo = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver).m_ItemContainer;
			m_bHasRandomInformation = true;
			break;
		case CraftDelivery.GiveToRandomGuard:
			m_TargetToDeliverTo = QuestManager.GetInstance().GetRandomGuard().m_ItemContainer;
			m_bHasRandomInformation = true;
			break;
		case CraftDelivery.PutInRandomDesk:
			m_TargetToDeliverTo = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Desk);
			if (m_TargetToDeliverTo == null)
			{
				m_TargetToDeliverTo = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
				if (m_TargetToDeliverTo != null)
				{
					DeskInteraction component = m_TargetToDeliverTo.GetComponent<DeskInteraction>();
					if (component != null && component.GetOwner() != null)
					{
						localized = component.GetOwner().m_CharacterCustomisation.m_DisplayName;
					}
				}
			}
			m_bHasRandomInformation = true;
			break;
		}
		Character characterOwner = m_TargetToDeliverTo.GetCharacterOwner();
		InternalTokenUpdate("$DeliverTo", (!(characterOwner != null)) ? localized : characterOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
		if (ItemManager.GetInstance() != null)
		{
			if (m_bRandomItem || m_ItemToCraft == null)
			{
				if (m_RandomItemGroup != null)
				{
					m_ItemToCraft = m_RandomItemGroup.GetRandomItem(bUniqueItems: true);
					m_CraftingRecipe = CraftManager.GetInstance().GetRecipeForProduct(m_ItemToCraft);
				}
				else
				{
					m_CraftingRecipe = CraftManager.GetInstance().GetRandomAllowedRecipe();
					m_ItemToCraft = m_CraftingRecipe.m_Product;
				}
				m_bHasRandomInformation = true;
			}
			else
			{
				m_CraftingRecipe = CraftManager.GetInstance().GetRecipeForProduct(m_ItemToCraft);
			}
		}
		if (m_CraftingRecipe == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
		UpdateRecipeTokens();
	}

	private void UpdateRecipeTokens()
	{
		string localized = "NO RECIPE FOUND";
		if (m_CraftingRecipe != null)
		{
			Localization.Get(m_CraftingRecipe.m_Product.m_ItemLocalizationTag, out localized);
			InternalTokenUpdate("$ItemToCraft", localized, m_CraftingRecipe.m_Product.m_ItemLocalizationTag);
			for (int i = 0; i < m_CraftingRecipe.m_Ingredients.Length; i++)
			{
				if (m_CraftingRecipe.m_Ingredients[i] != null)
				{
					string localized2 = string.Empty;
					Localization.Get(m_CraftingRecipe.m_Ingredients[i].m_ItemLocalizationTag, out localized2);
					switch (i)
					{
					case 0:
						InternalTokenUpdate("$IngrOne", localized2, m_CraftingRecipe.m_Ingredients[i].m_ItemLocalizationTag);
						break;
					case 1:
						InternalTokenUpdate("$IngrTwo", localized2, m_CraftingRecipe.m_Ingredients[i].m_ItemLocalizationTag);
						break;
					case 2:
						InternalTokenUpdate("$IngrThree", localized2, m_CraftingRecipe.m_Ingredients[i].m_ItemLocalizationTag);
						break;
					}
				}
			}
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$ItemToCraft", Localization.TokenReplaceType.Item);
		AddTokenInternal("$IngrOne", Localization.TokenReplaceType.Item);
		AddTokenInternal("$IngrTwo", Localization.TokenReplaceType.Item);
		AddTokenInternal("$IngrThree", Localization.TokenReplaceType.Item);
		AddTokenInternal("$DeliverTo", Localization.TokenReplaceType.ItemContainer);
	}

	protected override void Child_Reset()
	{
		ClearCraftingItems();
		m_bCraftedItem = false;
		m_bDeliveredItem = false;
		m_bHudPinsOn = false;
		m_bHUDArrowsOn = false;
		m_State = State.Setup;
		m_CraftedItem = null;
		m_ItemContainers = null;
		m_CurrentArrowItem = null;
		m_CurrentArrowItemContainerID = -1;
		m_WaitForFailFrames = -1;
		if (m_HUDInfoBindings != null)
		{
			ClearHUDInfo();
		}
	}

	protected override void Child_Initialize()
	{
	}

	public void OnItemCrafted(Item item, Character crafter)
	{
		if (m_State != State.WaitForCraftItems || !(crafter == m_PlayerOwner) || !(m_ItemToCraft != null) || !(item != null) || item.m_ItemData.m_ItemDataID != m_ItemToCraft.m_ItemDataID || (item.IsQuestItem() && (item.m_QuestItemOwnerID != m_PlayerOwner.m_NetView.viewID || item.m_QuestItemGroupID != m_QuestItemGroupID)) || (m_bSpawnItemsForCrafting && m_WaitForFailFrames == -1))
		{
			return;
		}
		m_WaitForFailFrames = -1;
		m_CraftedItem = item;
		m_CraftedItem.SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
		m_bCraftedItem = true;
		if (m_ItemContainers != null)
		{
			foreach (KeyValuePair<Item, int> itemContainer in m_ItemContainers)
			{
				if (itemContainer.Key != null && itemContainer.Key.m_ItemData != null)
				{
					itemContainer.Key.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
				}
			}
			m_ItemContainers.Clear();
		}
		m_State = State.DeliverItem;
		if (m_HUDRefList != null)
		{
			m_HUDRefList[0].PlayTick();
		}
	}

	protected override void Child_PreAction()
	{
		Player playerOwner = m_PlayerOwner;
		playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
		if (!m_bDeliveredItem)
		{
			switch (m_CraftDeliveryType)
			{
			case CraftDelivery.GiveToQuestGiver:
			case CraftDelivery.GiveToRandomInmate:
			case CraftDelivery.GiveToRandomGuard:
			{
				Player playerOwner2 = m_PlayerOwner;
				playerOwner2.TryDeliverItem = (Player.DeliverItem)Delegate.Combine(playerOwner2.TryDeliverItem, new Player.DeliverItem(DeliverItem));
				break;
			}
			}
			ItemManager instance = ItemManager.GetInstance();
			instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
			ItemManager instance2 = ItemManager.GetInstance();
			instance2.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Combine(instance2.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
			if (m_bSpawnItemsForCrafting && m_CraftedItem == null && m_ItemContainers == null && ItemManager.GetInstance() != null)
			{
				m_ItemContainers = new Dictionary<Item, int>();
				if (m_CraftingRecipe != null)
				{
					for (int i = 0; i < m_CraftingRecipe.m_Ingredients.Length; i++)
					{
						if (m_CraftingRecipe.m_Ingredients[i] != null)
						{
							m_SafetyCheck.Add(ItemManager.GetInstance().AssignItemRPC(-1, m_CraftingRecipe.m_Ingredients[i].m_ItemDataID, OnItemMgrResponse, ref m_ImmediateSpawnCheck));
						}
					}
				}
			}
			if (m_CraftedItem == null)
			{
				CraftManager.OnItemCrafted = (CraftManager.CraftEvent)Delegate.Remove(CraftManager.OnItemCrafted, new CraftManager.CraftEvent(OnItemCrafted));
				CraftManager.OnItemCrafted = (CraftManager.CraftEvent)Delegate.Combine(CraftManager.OnItemCrafted, new CraftManager.CraftEvent(OnItemCrafted));
			}
		}
		m_State = State.CollectItems;
		if (m_bResetOnItemsInContraband)
		{
			m_ContrabandDeskItemContainer = ItemContainer.FindFirstContrabandDeskItemContainer();
		}
	}

	private void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (!(newItem != null) || (m_ImmediateSpawnCheck != eventID && !m_SafetyCheck.Contains(eventID)))
		{
			return;
		}
		ItemContainer itemContainer = null;
		int num = 4;
		do
		{
			num--;
			itemContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
		}
		while ((itemContainer == null || m_ItemContainers.ContainsValue(itemContainer.NetView.viewID)) && num > 0);
		if (num <= 0)
		{
			T17NetManager.LogGoogleException("CraftObjective has problems finding enough questable ItemContainers! INVESTIGATE!");
		}
		if (itemContainer != null)
		{
			newItem.SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
			if (!itemContainer.AddItemRPC(newItem))
			{
				newItem.DropItemInLevel(m_PlayerOwner, itemContainer.transform.position);
			}
			int viewID = itemContainer.NetView.viewID;
			int viewID2 = newItem.m_NetView.viewID;
			m_ItemContainers.Add(newItem, viewID);
			if (m_HUDInfoBindings != null && !m_HUDInfoBindings.ContainsKey(viewID2))
			{
				m_HUDInfoBindings.Add(viewID2, m_HUDInfoBindings.Count);
				if (m_bPinsOn)
				{
					SetHUDPins(on: false);
					SetHUDPins(on: true);
				}
			}
		}
		m_SafetyCheckResponse.Add(eventID);
		m_ImmediateSpawnCheck = -1;
	}

	private bool DeliverItem(Player player, Character tryDeliverTo, bool onlyCheck)
	{
		bool result = false;
		if (player == m_PlayerOwner && tryDeliverTo.m_ItemContainer == m_TargetToDeliverTo && m_CraftedItem != null)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (m_PlayerOwner.m_ItemContainer.HasSpecificItem(m_CraftedItem.m_NetView.viewID, m_CraftedItem.m_ItemData.m_ItemDataID, isQuestItem: true, lookIntoHidden: false))
			{
				flag = true;
			}
			if (m_PlayerOwner.GetEquippedItem() == m_CraftedItem)
			{
				flag2 = true;
			}
			else if (m_PlayerOwner.GetOutFit() == m_CraftedItem)
			{
				flag3 = true;
			}
			if (flag || flag2 || flag3)
			{
				if (!onlyCheck)
				{
					ItemManager instance = ItemManager.GetInstance();
					instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
					if (flag)
					{
						m_PlayerOwner.m_ItemContainer.RemoveItemRPC(m_CraftedItem, releaseToManager: true);
					}
					if (flag2)
					{
						m_PlayerOwner.SetEquippedItem(null, bTellOthers: true, bAddOldToItemContainer: false);
						ItemManager.GetInstance().RequestReleaseItem(m_CraftedItem);
					}
					if (flag3)
					{
						m_PlayerOwner.SetOutFit(null, bTellOthers: true, bAddOldToInventory: false);
						ItemManager.GetInstance().RequestReleaseItem(m_CraftedItem);
					}
					m_bDeliveredToTarget = true;
				}
				result = true;
			}
		}
		return result;
	}

	private void OnQuestItemDestroyed(Item item, int eventID)
	{
		if (!m_bInPostAction && !(item == null))
		{
			if (m_CraftedItem != null && m_CraftedItem.m_NetView.viewID == item.m_NetView.viewID)
			{
				m_bCraftedItem = false;
				FailObjective();
			}
			if (m_ItemContainers != null && m_ItemContainers.ContainsKey(item))
			{
				m_WaitForFailFrames = 3;
				m_ItemContainers.Remove(item);
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (m_TargetToDeliverTo != null && m_TargetToDeliverTo.HasSpecificItem(m_CraftedItem.m_NetView.viewID, m_CraftedItem.m_ItemData.m_ItemDataID, isQuestItem: true, lookIntoHidden: false))
		{
			return true;
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_WaitForFailFrames > -1)
		{
			if (m_WaitForFailFrames == 0)
			{
				FailObjective();
				foreach (KeyValuePair<Item, int> itemContainer2 in m_ItemContainers)
				{
					if (itemContainer2.Key != null)
					{
						itemContainer2.Key.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
					}
				}
				m_ItemContainers.Clear();
			}
			m_WaitForFailFrames--;
			return false;
		}
		if (m_ObjectiveStatus == ObjectiveStatus.Failed || m_ObjectiveStatus == ObjectiveStatus.Reset)
		{
			return false;
		}
		switch (m_State)
		{
		case State.CollectItems:
			if (m_bCraftedItem)
			{
				m_State = State.DeliverItem;
			}
			else if (SafetyCheckResult())
			{
				SetHUDInfo();
				m_State = State.WaitForCollectItems;
				m_SafetyCheck.Clear();
				m_SafetyCheckResponse.Clear();
			}
			break;
		case State.WaitForCollectItems:
			if (m_ItemContainers != null)
			{
				int num = 0;
				List<Item> list2 = m_ItemContainers.Keys.ToList();
				for (int j = 0; j < list2.Count; j++)
				{
					Item item2 = list2[j];
					if (m_PlayerOwner.GetEquippedItem() == item2 || m_PlayerOwner.m_ItemContainer.HasSpecificItem(item2.m_NetView.viewID))
					{
						num++;
					}
					int num2 = m_ItemContainers[item2];
					int containerViewID = item2.m_ContainerViewID;
					if (num2 == containerViewID)
					{
						continue;
					}
					m_ItemContainers[item2] = containerViewID;
					if (T17NetManager.IsValidNetViewId(containerViewID))
					{
						if (containerViewID == LevelScript.LevelItemContainerViewID)
						{
							if (m_HUDRefList != null && m_HUDInfoBindings.Count > 0)
							{
								int num3 = m_HUDInfoBindings[item2.m_NetView.viewID];
								m_HUDRefList[num3].ResetTick();
							}
							if (!m_bHudPinsOn || m_HUDPinBindings == null)
							{
								continue;
							}
							int value = -1;
							if (!m_HUDPinBindings.TryGetValue(item2.m_NetView.viewID, out value) || value == -1)
							{
								AddPin(item2, containerViewID);
								m_HUDPinBindings.TryGetValue(item2.m_NetView.viewID, out value);
							}
							if (value > -1)
							{
								GameObject gameObject2 = item2.gameObject;
								if (gameObject2 != null)
								{
									PinManager.GetInstance().UpdatePinTarget(value, gameObject2);
								}
							}
							continue;
						}
						if (m_PlayerOwner.m_ItemContainer.NetView.viewID == containerViewID)
						{
							if (m_HUDRefList != null && m_HUDInfoBindings.Count > 0)
							{
								int num4 = m_HUDInfoBindings[item2.m_NetView.viewID];
								m_HUDRefList[num4].PlayTick();
							}
							RemovePin(item2.m_NetView.viewID);
						}
						else
						{
							if (m_HUDRefList != null && m_HUDInfoBindings.Count > 0)
							{
								int num5 = m_HUDInfoBindings[item2.m_NetView.viewID];
								m_HUDRefList[num5].ResetTick();
							}
							AddPin(item2, containerViewID);
						}
						if (!m_bHudPinsOn || m_HUDPinBindings == null || m_HUDPinBindings.Count <= 0)
						{
							continue;
						}
						int value2 = -1;
						m_HUDPinBindings.TryGetValue(item2.m_NetView.viewID, out value2);
						if (value2 != -1)
						{
							ItemContainer itemContainer = T17NetView.Find<ItemContainer>(containerViewID);
							if (itemContainer != null && itemContainer != m_PlayerOwner.gameObject)
							{
								PinManager.GetInstance().UpdatePinTarget(value2, itemContainer.gameObject);
							}
						}
					}
					else
					{
						if (m_HUDRefList != null && m_HUDInfoBindings.Count > 0)
						{
							int num6 = m_HUDInfoBindings[item2.m_NetView.viewID];
							m_HUDRefList[num6].PlayTick();
						}
						RemovePin(item2.m_NetView.viewID);
					}
				}
				if (num == m_ItemContainers.Count)
				{
					m_State = State.CraftItem;
				}
			}
			else
			{
				m_State = State.CraftItem;
			}
			break;
		case State.CraftItem:
			SetHUDInfo();
			m_State = State.WaitForCraftItems;
			break;
		case State.WaitForCraftItems:
			if (m_bCraftedItem)
			{
				if (m_HUDRefList != null)
				{
					m_HUDRefList[0].PlayTick();
				}
				m_State = State.DeliverItem;
			}
			else
			{
				if (m_ItemContainers == null)
				{
					break;
				}
				bool flag3 = true;
				if (!InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerOwner.m_PlayerCameraManagerBindingID))
				{
					List<Item> list = m_ItemContainers.Keys.ToList();
					for (int i = 0; i < list.Count; i++)
					{
						Item item = list[i];
						if (item.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID && item != m_PlayerOwner.GetEquippedItem())
						{
							flag3 = false;
							break;
						}
					}
				}
				if (!flag3)
				{
					m_State = State.CollectItems;
				}
			}
			break;
		case State.DeliverItem:
			if (m_CraftDeliveryType == CraftDelivery.KeepYourself)
			{
				m_State = State.Finished;
				break;
			}
			if (m_DeliverTargetPin != -1)
			{
				PinManager.GetInstance().RemovePin(m_DeliverTargetPin);
			}
			m_State = State.WaitForTickComplete;
			break;
		case State.WaitForTickComplete:
		{
			bool flag2 = true;
			if (m_HUDRefList != null)
			{
				if (m_HUDRefList[0].isActiveAndEnabled && m_HUDRefList[0].ElapsedTimeAfterTick < 1f)
				{
					flag2 = false;
				}
			}
			else
			{
				flag2 = true;
			}
			if (flag2)
			{
				SetHUDInfo();
				m_State = State.WaitForDeliverItem;
			}
			break;
		}
		case State.WaitForDeliverItem:
		{
			bool flag = true;
			if (m_TargetToDeliverTo != null)
			{
				flag = m_bDeliveredToTarget || (m_TargetToDeliverTo != null && m_TargetToDeliverTo.HasSpecificItem(m_CraftedItem.m_NetView.viewID, m_CraftedItem.m_ItemData.m_ItemDataID, isQuestItem: true, lookIntoHidden: false));
			}
			if (flag)
			{
				if (m_DeliverTargetPin != -1)
				{
					PinManager.GetInstance().RemovePin(m_DeliverTargetPin);
					m_DeliverTargetPin = -1;
				}
				if (m_HUDRefList != null)
				{
					m_HUDRefList[0].PlayTick();
				}
				m_State = State.Finished;
				m_ObjectiveStatus = ObjectiveStatus.Done;
			}
			else if (m_bHudPinsOn)
			{
				if (!m_bDeliverPinCreated)
				{
					Character characterOwner = m_TargetToDeliverTo.GetCharacterOwner();
					DeskInteraction deskInteraction = m_TargetToDeliverTo.GetDeskInteraction();
					if (characterOwner == null || deskInteraction != null)
					{
						m_DeliverTargetPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_TargetToDeliverTo.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_TargetToDeliverTo.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
					}
					else
					{
						characterOwner.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
					}
					m_bDeliverPinCreated = true;
				}
			}
			else
			{
				if (!m_bDeliverPinCreated)
				{
					break;
				}
				if (m_DeliverTargetPin == -1)
				{
					Character characterOwner2 = m_TargetToDeliverTo.GetCharacterOwner();
					if (characterOwner2 != null)
					{
						characterOwner2.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
					}
				}
				m_bDeliverPinCreated = false;
			}
			break;
		}
		}
		if (m_bHUDArrowsOn)
		{
			Child_SetHUDArrow(m_bHUDArrowsOn);
		}
		return m_State == State.Finished;
	}

	protected override int Child_EvaluateResetCondition()
	{
		if (m_ObjectiveStatus == ObjectiveStatus.Reset && m_bResetOnFail)
		{
			return m_IndexToResetTo;
		}
		if (m_bResetOnItemsInContraband && m_ContrabandDeskItemContainer != null)
		{
			if (m_State < State.DeliverItem)
			{
				Dictionary<Item, int>.KeyCollection keys = m_ItemContainers.Keys;
				IEnumerator<Item> enumerator = keys.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Item current = enumerator.Current;
					if (!(current == null))
					{
						int num = m_ContrabandDeskItemContainer.FindItemIndex(current);
						if (num != -1)
						{
							return m_IndexToResetTo;
						}
					}
				}
			}
			else if (m_bCraftedItem && m_CraftedItem != null)
			{
				int num2 = m_ContrabandDeskItemContainer.FindItemIndex(m_CraftedItem);
				if (num2 != -1)
				{
					return m_IndexToResetTo;
				}
			}
		}
		return -1;
	}

	public override int SetHUDInfo(ref ObjectiveSubGoalHUD[] infoList)
	{
		m_HUDRefList = infoList;
		if (m_HUDInfoBindings == null)
		{
			m_HUDInfoBindings = new Dictionary<int, int>();
		}
		if (m_HUDInfoBindings.Count == 0 && m_ItemContainers != null)
		{
			int num = 0;
			foreach (KeyValuePair<Item, int> itemContainer in m_ItemContainers)
			{
				m_HUDInfoBindings.Add(itemContainer.Key.m_NetView.viewID, num);
				num++;
			}
		}
		SetHUDInfo();
		return Mathf.Max(m_HUDInfoBindings.Count, 1);
	}

	private void SetHUDInfo()
	{
		if (m_State == State.CollectItems || m_State == State.WaitForCollectItems)
		{
			if (m_ItemContainers == null)
			{
				return;
			}
			List<Item> list = m_ItemContainers.Keys.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				Item item = list[i];
				int value = -1;
				if (m_HUDInfoBindings == null || m_HUDRefList == null || !m_HUDInfoBindings.TryGetValue(item.m_NetView.viewID, out value))
				{
					continue;
				}
				m_HUDRefList[value].Show(m_PlayerOwner);
				m_HUDRefList[value].ResetTick();
				if (m_PlayerOwner.m_ItemContainer.NetView.viewID == item.m_ContainerViewID || m_PlayerOwner.GetEquippedItem() == item)
				{
					m_HUDRefList[value].PlayTick();
				}
				string localized = null;
				Localization.TokenInfo token = null;
				switch (value)
				{
				case 0:
				{
					if (m_AvailableTokens.TryGetValue("$IngrOne", out var value4) && m_ParentObjectiveTree.GetObjectiveToken(value4, out token))
					{
						Localization.Get(m_CollectIngrOneText, out localized);
						localized = localized.Replace("$Ingr", token.m_TextID);
					}
					break;
				}
				case 1:
				{
					if (m_AvailableTokens.TryGetValue("$IngrTwo", out var value3) && m_ParentObjectiveTree.GetObjectiveToken(value3, out token))
					{
						Localization.Get(m_CollectIngrTwoText, out localized);
						localized = localized.Replace("$Ingr", token.m_TextID);
					}
					break;
				}
				case 2:
				{
					if (m_AvailableTokens.TryGetValue("$IngrThree", out var value2) && m_ParentObjectiveTree.GetObjectiveToken(value2, out token))
					{
						Localization.Get(m_CollectIngrThreeText, out localized);
						localized = localized.Replace("$Ingr", token.m_TextID);
					}
					break;
				}
				}
				m_HUDRefList[value].m_Info.text = localized;
			}
		}
		else if (m_State == State.CraftItem || m_State == State.WaitForCraftItems)
		{
			if (m_HUDRefList != null)
			{
				for (int j = 0; j < m_HUDRefList.Length; j++)
				{
					m_HUDRefList[j].Hide();
				}
				m_HUDRefList[0].Show(m_PlayerOwner);
				m_HUDRefList[0].ResetTick();
				m_HUDRefList[0].m_Info.text = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_CraftObjectiveText);
			}
		}
		else if (m_State == State.DeliverItem || m_State == State.WaitForDeliverItem || m_State == State.WaitForTickComplete)
		{
			if (m_HUDRefList != null)
			{
				for (int k = 0; k < m_HUDRefList.Length; k++)
				{
					m_HUDRefList[k].Hide();
				}
				m_HUDRefList[0].Show(m_PlayerOwner);
				m_HUDRefList[0].ResetTick();
				m_HUDRefList[0].m_Info.text = m_ParentObjectiveTree.GetTokenizedLocalization(this, m_DeliverObjectiveText);
			}
		}
		else if (m_HUDRefList != null)
		{
			for (int l = 0; l < m_HUDRefList.Length; l++)
			{
				m_HUDRefList[l].Hide();
			}
		}
	}

	public override void ClearHUDInfo()
	{
		base.ClearHUDInfo();
		m_HUDRefList = null;
		m_HUDInfoBindings.Clear();
	}

	protected override void Child_SetHUDPins(bool on)
	{
		if (on)
		{
			m_bHudPinsOn = on;
			if (m_HUDPinBindings == null)
			{
				m_HUDPinBindings = new Dictionary<int, int>();
			}
			if (m_HUDPinBindings.Count == 0 && m_State < State.CraftItem && m_ItemContainers != null)
			{
				List<Item> list = m_ItemContainers.Keys.ToList();
				for (int i = 0; i < list.Count; i++)
				{
					Item item = list[i];
					if (!T17NetManager.IsValidNetViewId(item.m_ContainerViewID))
					{
						continue;
					}
					if (item.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID && item != m_PlayerOwner.GetEquippedItem())
					{
						AddPin(item, item.m_ContainerViewID);
					}
					if (item.m_ContainerViewID != LevelScript.LevelItemContainerViewID)
					{
						continue;
					}
					int value = -1;
					m_HUDPinBindings.TryGetValue(item.m_NetView.viewID, out value);
					if (value > -1)
					{
						GameObject gameObject = item.gameObject;
						if (gameObject != null)
						{
							PinManager.GetInstance().UpdatePinTarget(value, gameObject);
						}
					}
				}
			}
			if (m_TargetToDeliverTo.gameObject != null && m_State >= State.DeliverItem)
			{
				Character characterOwner = m_TargetToDeliverTo.GetCharacterOwner();
				DeskInteraction deskInteraction = m_TargetToDeliverTo.GetDeskInteraction();
				if (characterOwner == null || deskInteraction != null)
				{
					m_DeliverTargetPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_TargetToDeliverTo.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_TargetToDeliverTo.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
				}
				else
				{
					characterOwner.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
				}
				m_bDeliverPinCreated = true;
			}
		}
		else
		{
			if (m_HUDPinBindings != null)
			{
				foreach (KeyValuePair<int, int> hUDPinBinding in m_HUDPinBindings)
				{
					PinManager.GetInstance().RemovePin(hUDPinBinding.Value);
				}
				m_HUDPinBindings.Clear();
			}
			if (m_DeliverTargetPin != -1)
			{
				PinManager.GetInstance().RemovePin(m_DeliverTargetPin);
				m_DeliverTargetPin = -1;
			}
			if (m_TargetToDeliverTo != null)
			{
				Character characterOwner2 = m_TargetToDeliverTo.GetCharacterOwner();
				if (characterOwner2 != null)
				{
					characterOwner2.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
				}
			}
			m_bDeliverPinCreated = false;
		}
		m_bHudPinsOn = on;
	}

	private void AddPin(Item theItem, int containerViewId)
	{
		if (m_bHudPinsOn)
		{
			ItemContainer component = PhotonView.Find(containerViewId).gameObject.GetComponent<ItemContainer>();
			if (component != null && !m_HUDPinBindings.ContainsKey(theItem.m_NetView.viewID))
			{
				float posZ = ((containerViewId != LevelScript.LevelItemContainerViewID) ? component.transform.position.z : theItem.transform.position.z);
				m_HUDPinBindings.Add(theItem.m_NetView.viewID, PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, component.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(posZ), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty));
			}
		}
	}

	private void RemovePin(int itemNetViewId)
	{
		if (m_bHudPinsOn)
		{
			int value = -1;
			m_HUDPinBindings.TryGetValue(itemNetViewId, out value);
			if (value != -1)
			{
				PinManager.GetInstance().RemovePin(value);
				m_HUDPinBindings.Remove(itemNetViewId);
			}
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		m_bHUDArrowsOn = on;
		if (!(base.PlayerOwner != null))
		{
			return;
		}
		if (on)
		{
			if (m_State < State.CraftItem)
			{
				if (m_ItemContainers != null)
				{
					List<Item> list = m_ItemContainers.Keys.ToList();
					for (int i = 0; i < list.Count; i++)
					{
						Item item = list[i];
						if (!T17NetManager.IsValidNetViewId(item.m_ContainerViewID) || item.m_ContainerViewID == m_PlayerOwner.m_ItemContainer.NetView.viewID || !(item != m_PlayerOwner.GetEquippedItem()))
						{
							continue;
						}
						if (!object.ReferenceEquals(item, m_CurrentArrowItem) || m_CurrentArrowItemContainerID != item.m_ContainerViewID)
						{
							if (item.m_ContainerViewID == LevelScript.LevelItemContainerViewID)
							{
								base.PlayerOwner.SetObjectiveArrowTarget(item.m_NetView);
							}
							else
							{
								ItemContainer component = PhotonView.Find(item.m_ContainerViewID).gameObject.GetComponent<ItemContainer>();
								base.PlayerOwner.SetObjectiveArrowTarget(component.NetView);
							}
							m_CurrentArrowItem = item;
							m_CurrentArrowItemContainerID = item.m_ContainerViewID;
						}
						return;
					}
				}
			}
			else if (m_State > State.WaitForCraftItems && m_TargetToDeliverTo.NetView != null && m_TargetToDeliverTo.GetCharacterOwner() != m_PlayerOwner)
			{
				if (base.PlayerOwner.ObjectiveArrowTargetNetViewID != m_TargetToDeliverTo.NetView.viewID)
				{
					base.PlayerOwner.SetObjectiveArrowTarget(m_TargetToDeliverTo.NetView);
				}
			}
			else
			{
				base.PlayerOwner.CancelObjectiveArrow();
			}
			m_CurrentArrowItem = null;
		}
		else
		{
			base.PlayerOwner.CancelObjectiveArrow();
			m_CurrentArrowItem = null;
		}
	}

	protected override void Child_PostAction()
	{
		ItemManager instance = ItemManager.GetInstance();
		instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		CraftManager.OnItemCrafted = (CraftManager.CraftEvent)Delegate.Remove(CraftManager.OnItemCrafted, new CraftManager.CraftEvent(OnItemCrafted));
		if (m_CraftedItem != null && m_PlayerOwner != null)
		{
			m_CraftedItem.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
			switch (m_CraftDeliveryType)
			{
			case CraftDelivery.GiveToQuestGiver:
			case CraftDelivery.GiveToRandomInmate:
			case CraftDelivery.GiveToRandomGuard:
			{
				Player playerOwner = m_PlayerOwner;
				playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
				if (m_TargetToDeliverTo != null)
				{
					m_TargetToDeliverTo.RemoveItemRPC(m_CraftedItem, releaseToManager: true);
					m_CraftedItem = null;
				}
				break;
			}
			default:
				m_CraftedItem = null;
				break;
			}
		}
		ClearCraftingItems();
	}

	private void FailObjective()
	{
		if (m_bResetOnFail && m_IndexToResetTo != -1)
		{
			m_ObjectiveStatus = ObjectiveStatus.Reset;
		}
		else
		{
			m_ObjectiveStatus = ObjectiveStatus.Failed;
		}
	}

	private void ClearCraftingItems()
	{
		if (m_ItemContainers == null || m_ItemContainers.Count <= 0 || !(m_PlayerOwner != null))
		{
			return;
		}
		List<Item> list = m_ItemContainers.Keys.ToList();
		foreach (Item item in list)
		{
			if (!(item != null))
			{
				continue;
			}
			if (item.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID && item != m_PlayerOwner.GetEquippedItem() && item != m_PlayerOwner.GetOutFit())
			{
				ItemContainer itemContainer = T17NetView.Find<ItemContainer>(item.m_ContainerViewID);
				if (itemContainer != null)
				{
					itemContainer.RemoveItemRPC(item, releaseToManager: true);
				}
			}
			else
			{
				item.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
			}
		}
		m_ItemContainers.Clear();
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (m_ItemToCraft != null)
		{
			baseObj.Add(new JProperty("ItemToCraft", m_ItemToCraft.m_ItemDataID));
		}
		if (ingameSave)
		{
			baseObj.Add(new JProperty("HasCraftedItem", m_bCraftedItem));
			baseObj.Add(new JProperty("DeliveredItem", m_bDeliveredItem));
			baseObj.Add(new JProperty("State", (int)m_State));
			if (m_TargetToDeliverTo != null)
			{
				baseObj.Add(new JProperty("TargetToDeliverTo", m_TargetToDeliverTo.NetView.viewID));
			}
			baseObj.Add(new JProperty("QuestItemGroupID", m_QuestItemGroupID));
			if (m_ItemContainers != null)
			{
				JProperty jProperty = new JProperty("SpawnedItems");
				JArray jArray = new JArray();
				foreach (KeyValuePair<Item, int> itemContainer in m_ItemContainers)
				{
					jArray.Add(itemContainer.Key.m_NetView.viewID);
				}
				jProperty.Add(jArray);
				baseObj.Add(jProperty);
			}
			if (m_CraftedItem != null)
			{
				baseObj.Add(new JProperty("CraftedItem", m_CraftedItem.m_NetView.viewID));
			}
		}
		baseObj.Add(new JProperty("RandomItem", m_bRandomItem));
		if (m_RandomItemGroup != null)
		{
			baseObj.Add(new JProperty("ItemGroup", m_RandomItemGroup.m_RandomItemGroupID));
		}
		baseObj.Add(new JProperty("DeliverTo", (int)m_CraftDeliveryType));
		baseObj.Add(new JProperty("SpawnItemsToCraft", m_bSpawnItemsForCrafting));
		baseObj.Add(new JProperty("CollectIngrOneText", m_CollectIngrOneText));
		baseObj.Add(new JProperty("CollectIngrTwoText", m_CollectIngrTwoText));
		baseObj.Add(new JProperty("CollectIngrThreeText", m_CollectIngrThreeText));
		baseObj.Add(new JProperty("CraftObjectiveText", m_CraftObjectiveText));
		baseObj.Add(new JProperty("DeliverObjectiveText", m_DeliverObjectiveText));
		baseObj.Add(new JProperty("ResetOnFail", m_bResetOnFail));
		baseObj.Add(new JProperty("ResetOnItemsInContraband", m_bResetOnItemsInContraband));
		baseObj.Add(new JProperty("IndexToResetTo", m_IndexToResetTo));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		ResourcesItemDataManager instance = ResourcesItemDataManager.GetInstance();
		JProperty jProperty = json.Property("ItemToCraft");
		if (jProperty != null)
		{
			int num = (int)jProperty.Value;
			m_ItemToCraft = Resources.Load<ItemData>(instance.GetItemDataResourcePath(num));
			if (m_ItemToCraft.m_ItemDataID != num)
			{
				Debug.LogError("[BLC] ERROR: THE RANDOM ITEM GROUP LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
		}
		if (ingameLoad)
		{
			if (m_ItemToCraft != null)
			{
				m_CraftingRecipe = CraftManager.GetInstance().GetRecipeForProduct(m_ItemToCraft);
			}
			if (json.Property("HasCraftedItem") != null)
			{
				m_bCraftedItem = (bool)json.Property("HasCraftedItem").Value;
			}
			if (json.Property("DeliveredItem") != null)
			{
				m_bDeliveredItem = (bool)json.Property("DeliveredItem").Value;
			}
			if (json.Property("State") != null)
			{
				m_State = (State)(int)json.Property("State").Value;
			}
			if (json.Property("TargetToDeliverTo") != null)
			{
				int viewID = (int)json.Property("TargetToDeliverTo").Value;
				m_TargetToDeliverTo = PhotonView.Find(viewID).GetComponent<ItemContainer>();
				Character characterOwner = m_TargetToDeliverTo.GetCharacterOwner();
				if (characterOwner != null)
				{
					InternalTokenUpdate("$DeliverTo", (!(characterOwner != null)) ? "TBR: A Desk" : characterOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
				}
			}
			JProperty jProperty2 = json.Property("QuestItemGroupID");
			if (jProperty2 != null)
			{
				m_QuestItemGroupID = (int)jProperty2.Value;
			}
			JProperty jProperty3 = json.Property("SpawnedItems");
			if (jProperty3 != null)
			{
				m_ItemContainers = new Dictionary<Item, int>();
				if (jProperty3.Value.Type == JTokenType.Array)
				{
					JArray jArray = (JArray)jProperty3.Value;
					for (int i = 0; i < jArray.Count; i++)
					{
						if (jArray[i] != null)
						{
							JToken value = jArray[i];
							int netviewID = value.Value<int>();
							Item itemFromUsedListByNetView = ItemManager.GetInstance().GetItemFromUsedListByNetView(netviewID);
							m_ItemContainers.Add(itemFromUsedListByNetView, itemFromUsedListByNetView.m_ContainerViewID);
							itemFromUsedListByNetView.LOADING_SetQuestGroupID(m_PlayerOwner, m_QuestItemGroupID);
						}
					}
				}
			}
			JProperty jProperty4 = json.Property("CraftedItem");
			if (jProperty4 != null)
			{
				int netviewID2 = (int)jProperty4.Value;
				m_CraftedItem = ItemManager.GetInstance().GetItemFromUsedListByNetView(netviewID2);
				m_CraftedItem.LOADING_SetQuestGroupID(m_PlayerOwner, m_QuestItemGroupID);
			}
			UpdateRecipeTokens();
		}
		if (json.Property("RandomItem") != null)
		{
			m_bRandomItem = (bool)json.Property("RandomItem").Value;
		}
		JProperty jProperty5 = json.Property("ItemGroup");
		if (jProperty5 != null)
		{
			int num2 = (int)jProperty5.Value;
			m_RandomItemGroup = Resources.Load<RandomItemGroup>(instance.GetRandomItemGroupResourcePath(num2));
			if (m_RandomItemGroup.m_RandomItemGroupID != num2)
			{
				Debug.LogError("[BLC] ERROR: THE RANDOM ITEM GROUP LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
		}
		if (json.Property("DeliverTo") != null)
		{
			m_CraftDeliveryType = (CraftDelivery)(int)json.Property("DeliverTo").Value;
		}
		if (json.Property("SpawnItemsToCraft") != null)
		{
			m_bSpawnItemsForCrafting = (bool)json.Property("SpawnItemsToCraft").Value;
		}
		if (json.Property("CollectIngrOneText") != null)
		{
			m_CollectIngrOneText = (string)json.Property("CollectIngrOneText").Value;
		}
		if (json.Property("CollectIngrTwoText") != null)
		{
			m_CollectIngrTwoText = (string)json.Property("CollectIngrTwoText").Value;
		}
		if (json.Property("CollectIngrThreeText") != null)
		{
			m_CollectIngrThreeText = (string)json.Property("CollectIngrThreeText").Value;
		}
		if (json.Property("CraftObjectiveText") != null)
		{
			m_CraftObjectiveText = (string)json.Property("CraftObjectiveText").Value;
		}
		if (json.Property("DeliverObjectiveText") != null)
		{
			m_DeliverObjectiveText = (string)json.Property("DeliverObjectiveText").Value;
		}
		if (json.Property("ResetOnFail") != null)
		{
			m_bResetOnFail = (bool)json.Property("ResetOnFail").Value;
		}
		if (json.Property("ResetOnItemsInContraband") != null)
		{
			m_bResetOnItemsInContraband = (bool)json.Property("ResetOnItemsInContraband").Value;
		}
		if (json.Property("IndexToResetTo") != null)
		{
			m_IndexToResetTo = (int)json.Property("IndexToResetTo").Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.CraftObjective;
	}

	private bool SafetyCheckResult()
	{
		for (int i = 0; i < m_SafetyCheck.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < m_SafetyCheckResponse.Count; j++)
			{
				if (m_SafetyCheck[i] == m_SafetyCheckResponse[j])
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}
}
