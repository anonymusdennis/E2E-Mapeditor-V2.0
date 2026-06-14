using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class ItemObjective : BaseObjective
{
	public enum ItemLocation
	{
		OnRandomInmate,
		OnRandomGuard,
		OnInmateOfQuestGiversGang,
		OnInmateOfQuestGiversRivalGang,
		InRandomDesk,
		InRandomInmateDesk,
		InRandomGuardDesk,
		InDeskOfQuestGiversGang,
		InDeskOfQuestGiversRivalGang,
		InQuestGiversDesk,
		InPrison,
		Player,
		PlayerDesk
	}

	public enum DeliveryTargetType
	{
		QuestGiver,
		RandomInmate,
		RandomGuard,
		InmateOfQuestGiversGang,
		InmateOfQuestGiversRivalGang,
		RandomDesk,
		RandomInmateDesk,
		RandomGuardDesk,
		DeskOfQuestGiversGang,
		DeskOfQuestGiversRivalGang,
		InPrison,
		Player
	}

	public ItemLocation m_ItemLocation = ItemLocation.InRandomDesk;

	public DeliveryTargetType m_TargetType;

	public bool m_bTrackItems;

	public List<ItemData> m_ItemTargets = new List<ItemData>();

	public bool m_bRandomItem;

	public RandomItemGroup m_RandomItemGroup;

	public int m_QuantityNeeded = 1;

	public ObjectiveSceneElement m_SceneItemLocation;

	public ObjectiveSceneElement m_SceneTarget;

	private List<Item> m_SpawnedItems;

	private List<Item> m_DeliveredItems;

	private ItemContainer m_SpawnContainer;

	private ItemContainer m_DeliverContainer;

	private bool m_bHudPinsOn;

	private bool m_bTargetPinCreated;

	private Dictionary<int, int> m_HUDPinBindings;

	private int m_DeliverTargetPin = -1;

	private const string SPAWNLOCATIONTOKEN = "$ItemSpawnLocation";

	private const string ITEMTOGETTOKEN = "$ItemToGet";

	private const string DELIVERTOKEN = "$DeliverItemTo";

	private int m_QuestItemGroupID = -1;

	private List<int> m_SafetySpawnCheck = new List<int>();

	private int m_ImmediateSpawnCheck = -1;

	protected override void Child_PickAllTargets()
	{
		List<ItemContainer> list = new List<ItemContainer>();
		DeskInteraction myDesk = m_PlayerOwner.GetMyDesk();
		if (myDesk != null)
		{
			list.Add(myDesk.m_LinkedItemContainer);
		}
		if (m_QuestGiver != null)
		{
			list.Add(m_QuestGiver.m_ItemContainer);
			myDesk = m_QuestGiver.GetMyDesk();
			if (myDesk != null)
			{
				list.Add(myDesk.m_LinkedItemContainer);
			}
		}
		if (RoomManager.GetContrabandDesks().Count > 0)
		{
			list.AddRange(RoomManager.GetContrabandDesks());
		}
		Player playerOwner = m_PlayerOwner;
		playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
		string localized = "Text.Quest.Desk";
		Localization.Get("Text.Quest.Desk", out localized);
		switch (m_TargetType)
		{
		case DeliveryTargetType.QuestGiver:
			m_DeliverContainer = m_QuestGiver.m_ItemContainer;
			break;
		case DeliveryTargetType.RandomInmate:
			m_DeliverContainer = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver).m_ItemContainer;
			break;
		case DeliveryTargetType.RandomGuard:
			m_DeliverContainer = QuestManager.GetInstance().GetRandomGuard().m_ItemContainer;
			break;
		case DeliveryTargetType.InmateOfQuestGiversGang:
			m_DeliverContainer = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver).m_ItemContainer;
			break;
		case DeliveryTargetType.InmateOfQuestGiversRivalGang:
			m_DeliverContainer = QuestManager.GetInstance().GetRandomInmate(m_QuestGiver).m_ItemContainer;
			break;
		case DeliveryTargetType.RandomDesk:
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Desk, list);
			if (!(m_DeliverContainer == null))
			{
				break;
			}
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			if (m_DeliverContainer != null)
			{
				DeskInteraction component2 = m_DeliverContainer.GetComponent<DeskInteraction>();
				if (component2 != null && component2.GetOwner() != null)
				{
					localized = component2.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case DeliveryTargetType.RandomInmateDesk:
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			if (m_DeliverContainer != null)
			{
				DeskInteraction component3 = m_DeliverContainer.GetComponent<DeskInteraction>();
				if (component3 != null && component3.GetOwner() != null)
				{
					localized = component3.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case DeliveryTargetType.RandomGuardDesk:
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskGuard, list);
			if (m_DeliverContainer != null)
			{
				DeskInteraction component = m_DeliverContainer.GetComponent<DeskInteraction>();
				if (component != null && component.GetOwner() != null)
				{
					localized = component.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case DeliveryTargetType.DeskOfQuestGiversGang:
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			break;
		case DeliveryTargetType.DeskOfQuestGiversRivalGang:
			m_DeliverContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			break;
		case DeliveryTargetType.InPrison:
			if (m_SceneTarget != null && m_SceneTarget.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.ItemContainer)
			{
				m_DeliverContainer = m_SceneTarget.GetComponent<ItemContainer>();
			}
			break;
		case DeliveryTargetType.Player:
			m_DeliverContainer = m_PlayerOwner.m_ItemContainer;
			break;
		}
		if (m_DeliverContainer == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
			return;
		}
		list.Add(m_DeliverContainer);
		Character characterOwner = m_DeliverContainer.GetCharacterOwner();
		InternalTokenUpdate("$DeliverItemTo", (!(characterOwner != null)) ? localized : characterOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
		string localized2 = "Text.Quest.Desk";
		Localization.Get("Text.Quest.Desk", out localized2);
		switch (m_ItemLocation)
		{
		case ItemLocation.OnRandomInmate:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate, list);
			break;
		case ItemLocation.OnRandomGuard:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Guard, list);
			break;
		case ItemLocation.OnInmateOfQuestGiversGang:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate, list);
			break;
		case ItemLocation.OnInmateOfQuestGiversRivalGang:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate, list);
			break;
		case ItemLocation.InRandomDesk:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Desk, list);
			if (!(m_SpawnContainer == null))
			{
				break;
			}
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			if (m_SpawnContainer != null)
			{
				DeskInteraction component6 = m_SpawnContainer.GetComponent<DeskInteraction>();
				if (component6 != null && component6.GetOwner() != null)
				{
					localized2 = component6.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case ItemLocation.InRandomInmateDesk:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			if (m_SpawnContainer != null)
			{
				DeskInteraction component4 = m_SpawnContainer.GetComponent<DeskInteraction>();
				if (component4 != null && component4.GetOwner() != null)
				{
					localized2 = component4.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case ItemLocation.InRandomGuardDesk:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskGuard, list);
			if (m_SpawnContainer != null)
			{
				DeskInteraction component5 = m_SpawnContainer.GetComponent<DeskInteraction>();
				if (component5 != null && component5.GetOwner() != null)
				{
					localized2 = component5.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case ItemLocation.InDeskOfQuestGiversGang:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			break;
		case ItemLocation.InDeskOfQuestGiversRivalGang:
			m_SpawnContainer = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate, list);
			break;
		case ItemLocation.InQuestGiversDesk:
		{
			DeskInteraction myDesk3 = m_QuestGiver.GetMyDesk();
			if (myDesk3 != null)
			{
				m_SpawnContainer = myDesk3.m_LinkedItemContainer;
			}
			break;
		}
		case ItemLocation.InPrison:
			if (m_SceneItemLocation != null && m_SceneItemLocation.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.ItemContainer)
			{
				m_SpawnContainer = m_SceneItemLocation.GetComponent<ItemContainer>();
			}
			break;
		case ItemLocation.PlayerDesk:
		{
			DeskInteraction myDesk2 = m_PlayerOwner.GetMyDesk();
			if (myDesk2 != null)
			{
				m_SpawnContainer = myDesk2.m_LinkedItemContainer;
			}
			break;
		}
		case ItemLocation.Player:
			m_SpawnContainer = m_PlayerOwner.m_ItemContainer;
			break;
		}
		if (m_SpawnContainer == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
			return;
		}
		Character characterOwner2 = m_SpawnContainer.GetCharacterOwner();
		InternalTokenUpdate("$ItemSpawnLocation", (!(characterOwner2 != null)) ? localized2 : characterOwner2.m_CharacterCustomisation.m_DisplayName, string.Empty);
		if (ItemManager.GetInstance() != null && (m_bRandomItem || m_ItemTargets.Count <= 0))
		{
			ItemData itemData = null;
			itemData = ((!(m_RandomItemGroup != null)) ? ItemManager.GetInstance().GetRandomItemFromAllowedList() : m_RandomItemGroup.GetRandomItem(bUniqueItems: true));
			for (int i = 0; i < m_QuantityNeeded; i++)
			{
				m_ItemTargets.Add(itemData);
			}
		}
		if (m_ItemTargets.Count > 0 && m_ItemTargets[0] != null)
		{
			string localized3 = string.Empty;
			Localization.Get(m_ItemTargets[0].m_ItemLocalizationTag, out localized3);
			InternalTokenUpdate("$ItemToGet", localized3, m_ItemTargets[0].m_ItemLocalizationTag);
		}
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$ItemSpawnLocation", Localization.TokenReplaceType.ItemContainer);
		AddTokenInternal("$ItemToGet", Localization.TokenReplaceType.Item);
		AddTokenInternal("$DeliverItemTo", Localization.TokenReplaceType.ItemContainer);
	}

	protected override void Child_Reset()
	{
		m_SpawnedItems = null;
		m_DeliveredItems = null;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		Player playerOwner = m_PlayerOwner;
		playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
		switch (m_TargetType)
		{
		case DeliveryTargetType.QuestGiver:
		case DeliveryTargetType.RandomInmate:
		case DeliveryTargetType.RandomGuard:
		case DeliveryTargetType.InmateOfQuestGiversGang:
		case DeliveryTargetType.InmateOfQuestGiversRivalGang:
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
		if (!(ItemManager.GetInstance() != null) || m_SpawnedItems != null)
		{
			return;
		}
		m_SpawnedItems = new List<Item>();
		m_DeliveredItems = new List<Item>();
		for (int i = 0; i < m_ItemTargets.Count; i++)
		{
			if (!(m_ItemTargets[i] == null))
			{
				int trackingContainerId = ((!m_bTrackItems || !(m_SpawnContainer != null)) ? (-1) : m_SpawnContainer.NetView.viewID);
				m_SafetySpawnCheck.Add(ItemManager.GetInstance().AssignItemRPC(-1, m_ItemTargets[i].m_ItemDataID, OnItemMgrResponse, ref m_ImmediateSpawnCheck, trackingContainerId));
			}
		}
	}

	private void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (!(newItem != null) || (eventID != m_ImmediateSpawnCheck && !m_SafetySpawnCheck.Contains(eventID)))
		{
			return;
		}
		newItem.SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
		if (m_SpawnContainer != null)
		{
			ItemContainer.MakeRoomForQuestItem(m_SpawnContainer);
			if (!m_SpawnContainer.AddItemRPC(newItem))
			{
				Character characterOwner = m_SpawnContainer.GetCharacterOwner();
				if (characterOwner != null)
				{
					newItem.DropItemInLevel(characterOwner, characterOwner.transform.position);
				}
			}
		}
		m_SpawnedItems.Add(newItem);
		m_ImmediateSpawnCheck = -1;
	}

	private bool DoNULLCheckWithException(object toTest, string variableName)
	{
		if (toTest == null)
		{
			string arg = "UNKNOWN";
			if (m_ParentObjectiveTree != null && m_ParentObjectiveTree.MainBranch != null)
			{
				QuestIntroObjective questDescription = m_ParentObjectiveTree.MainBranch.GetQuestDescription();
				if (questDescription != null)
				{
					arg = questDescription.QuestLocalizedObjectiveName;
				}
			}
			T17NetManager.LogGoogleException($"ItemObjective::DeliverItem - [{arg}] {variableName} is NULL");
			return true;
		}
		return false;
	}

	private bool DeliverItem(Player player, Character tryDeliverTo, bool onlyCheck)
	{
		bool flag = false;
		flag |= DoNULLCheckWithException(m_PlayerOwner, "m_PlayerOwner");
		if (m_PlayerOwner != null)
		{
			flag |= DoNULLCheckWithException(m_PlayerOwner.m_ItemContainer, "m_PlayerOwner.m_ItemContainer");
		}
		flag |= DoNULLCheckWithException(player, "player");
		if (player != null)
		{
			flag |= DoNULLCheckWithException(player.m_ItemContainer, "player.m_ItemContainer");
		}
		flag |= DoNULLCheckWithException(tryDeliverTo, "tryDeliverTo");
		if (tryDeliverTo != null)
		{
			flag |= DoNULLCheckWithException(tryDeliverTo.m_ItemContainer, "tryDeliverTo.m_ItemContainer");
		}
		flag |= DoNULLCheckWithException(ItemManager.GetInstance(), "ItemManager.GetInstance( )");
		flag |= DoNULLCheckWithException(m_SpawnedItems, "m_SpawnedItems");
		if (m_SpawnedItems != null)
		{
			for (int i = 0; i < m_SpawnedItems.Count; i++)
			{
				Item item = m_SpawnedItems[i];
				flag |= DoNULLCheckWithException(item, $"m_SpawnedItems[{i}]");
				if (item != null)
				{
					flag |= DoNULLCheckWithException(item.m_NetView, $"m_SpawnedItems[{i}].m_NetView");
					flag |= DoNULLCheckWithException(item.m_ItemData, $"m_SpawnedItems[{i}].m_ItemData");
				}
			}
		}
		if (flag)
		{
			return false;
		}
		bool result = false;
		if (player == m_PlayerOwner && tryDeliverTo.m_ItemContainer == m_DeliverContainer)
		{
			for (int j = 0; j < m_SpawnedItems.Count; j++)
			{
				Item item2 = m_SpawnedItems[j];
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				if (m_PlayerOwner.m_ItemContainer.HasSpecificItem(item2.m_NetView.viewID, item2.m_ItemData.m_ItemDataID, isQuestItem: true, lookIntoHidden: false))
				{
					flag2 = true;
				}
				if (m_PlayerOwner.GetEquippedItem() == item2)
				{
					flag3 = true;
				}
				else if (m_PlayerOwner.GetOutFit() == item2)
				{
					flag4 = true;
				}
				if (!flag2 && !flag3 && !flag4)
				{
					continue;
				}
				if (!onlyCheck)
				{
					ItemManager instance = ItemManager.GetInstance();
					instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
					m_DeliveredItems.Add(item2);
					m_SpawnedItems.Remove(item2);
					if (flag2)
					{
						m_PlayerOwner.m_ItemContainer.RemoveItemRPC(item2, releaseToManager: true);
					}
					if (flag3)
					{
						m_PlayerOwner.SetEquippedItem(null, bTellOthers: true, bAddOldToItemContainer: false);
						ItemManager.GetInstance().RequestReleaseItem(item2);
					}
					if (flag4)
					{
						m_PlayerOwner.SetOutFit(null, bTellOthers: true, bAddOldToInventory: false);
						ItemManager.GetInstance().RequestReleaseItem(item2);
					}
				}
				result = true;
			}
		}
		return result;
	}

	private void OnQuestItemDestroyed(Item item, int eventID)
	{
		if (m_bInPostAction || m_SpawnedItems == null)
		{
			return;
		}
		for (int num = m_SpawnedItems.Count - 1; num >= 0; num--)
		{
			if (m_SpawnedItems[num].m_NetView.viewID == item.m_NetView.viewID)
			{
				SetHUDPins(on: false);
				SetHUDArrow(on: false);
				m_ObjectiveStatus = ObjectiveStatus.Failed;
				m_SpawnedItems.RemoveAt(num);
				break;
			}
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		return Child_EvaluateStatus();
	}

	protected override bool Child_EvaluateStatus()
	{
		if (m_ObjectiveStatus != ObjectiveStatus.Done || m_bIsADependency)
		{
			if (m_SpawnedItems != null && m_SpawnedItems.Count > 0)
			{
				if (m_bHudPinsOn && m_HUDPinBindings != null)
				{
					int num = 0;
					for (int i = 0; i < m_SpawnedItems.Count; i++)
					{
						if (!(m_SpawnedItems[i] != null) || !m_SpawnedItems[i].m_NetView)
						{
							continue;
						}
						Item item = m_SpawnedItems[i];
						int containerViewID = item.m_ContainerViewID;
						if (T17NetManager.IsValidNetViewId(containerViewID))
						{
							if (containerViewID == LevelScript.LevelItemContainerViewID)
							{
								int value = -1;
								if (!m_HUDPinBindings.TryGetValue(item.m_NetView.viewID, out value) || value == -1)
								{
									AddPin(item, containerViewID);
									m_HUDPinBindings.TryGetValue(item.m_NetView.viewID, out value);
								}
								if (value > -1)
								{
									GameObject gameObject = item.gameObject;
									if (gameObject != null)
									{
										PinManager.GetInstance().UpdatePinTarget(value, gameObject);
									}
								}
								continue;
							}
							if (m_PlayerOwner.m_ItemContainer.NetView.viewID == containerViewID)
							{
								RemovePin(item.m_NetView.viewID);
								num++;
							}
							else
							{
								AddPin(item, containerViewID);
							}
							int value2 = -1;
							m_HUDPinBindings.TryGetValue(item.m_NetView.viewID, out value2);
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
							RemovePin(item.m_NetView.viewID);
							num++;
						}
					}
					if (num >= m_SpawnedItems.Count)
					{
						if (!m_bTargetPinCreated)
						{
							if (m_DeliverContainer != null && m_PlayerOwner != null && (bool)m_PlayerOwner.m_ItemContainer && m_DeliverContainer != m_PlayerOwner.m_ItemContainer)
							{
								Character characterOwner = m_DeliverContainer.GetCharacterOwner();
								DeskInteraction deskInteraction = m_DeliverContainer.GetDeskInteraction();
								if (characterOwner == null || deskInteraction != null)
								{
									m_DeliverTargetPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_DeliverContainer.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_DeliverContainer.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
								}
								else
								{
									characterOwner.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
								}
							}
							m_bTargetPinCreated = true;
						}
					}
					else if (m_bTargetPinCreated)
					{
						if (m_DeliverTargetPin != -1)
						{
							PinManager.GetInstance().RemovePin(m_DeliverTargetPin);
							m_DeliverTargetPin = -1;
						}
						if (m_DeliverContainer != null)
						{
							Character characterOwner2 = m_DeliverContainer.GetCharacterOwner();
							if (characterOwner2 != null)
							{
								characterOwner2.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
							}
						}
						m_bTargetPinCreated = false;
					}
				}
				if (m_DeliverContainer != null)
				{
					InGameMenuFlow instance = InGameMenuFlow.Instance;
					if (instance != null)
					{
						InGameMenuFlow.PlayerIGMData data = null;
						instance.GetCorrectIGMData(m_PlayerOwner.m_PlayerCameraManagerBindingID, out data);
						if (data != null && !data.AnyMenusOpen && !m_DeliverContainer.IsNetObjectLocked())
						{
							int num2 = 0;
							for (int j = 0; j < m_SpawnedItems.Count; j++)
							{
								Item item2 = m_SpawnedItems[j];
								if (m_DeliverContainer.HasSpecificItem(item2.m_NetView.viewID, item2.m_ItemData.m_ItemDataID, isQuestItem: true, lookIntoHidden: false))
								{
									num2++;
									continue;
								}
								Character characterOwner3 = m_DeliverContainer.GetCharacterOwner();
								if (characterOwner3 != null && (characterOwner3.GetEquippedItem() == item2 || characterOwner3.GetOutFit() == item2))
								{
									num2++;
								}
							}
							if (num2 >= m_SpawnedItems.Count)
							{
								return true;
							}
						}
					}
				}
			}
			else if (m_DeliveredItems != null && m_DeliveredItems.Count > 0)
			{
				return true;
			}
			return false;
		}
		return m_ObjectiveStatus == ObjectiveStatus.Done;
	}

	protected override void Child_SetHUDPins(bool on)
	{
		m_bHudPinsOn = on;
		if (on)
		{
			if (m_HUDPinBindings == null)
			{
				m_HUDPinBindings = new Dictionary<int, int>();
			}
			if (m_HUDPinBindings.Count == 0 && m_SpawnedItems != null)
			{
				for (int i = 0; i < m_SpawnedItems.Count; i++)
				{
					Item item = m_SpawnedItems[i];
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
			if (m_DeliverContainer != null && m_PlayerOwner != null && m_DeliverContainer != m_PlayerOwner.m_ItemContainer)
			{
				Character characterOwner = m_DeliverContainer.GetCharacterOwner();
				DeskInteraction deskInteraction = m_DeliverContainer.GetDeskInteraction();
				if (characterOwner == null || deskInteraction != null)
				{
					m_DeliverTargetPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, m_DeliverContainer.gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(m_DeliverContainer.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
				}
				else
				{
					characterOwner.SetPinImage(null, PinManager.Pin.PinFilterType.Objectives, ObjectiveManager.GetInstance().m_QuestTargetAnimation, edgeable: true, floorTrackable: true);
				}
			}
			m_bTargetPinCreated = true;
			return;
		}
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
		if (m_DeliverContainer != null)
		{
			Character characterOwner2 = m_DeliverContainer.GetCharacterOwner();
			if (characterOwner2 != null)
			{
				characterOwner2.ResetPinImage(PinManager.Pin.PinFilterType.Objectives);
			}
		}
		m_bTargetPinCreated = false;
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
		if (m_PlayerOwner != null)
		{
			if (on && m_SpawnContainer != null && m_SpawnContainer.NetView != null)
			{
				base.PlayerOwner.SetObjectiveArrowTarget(m_SpawnContainer.NetView);
			}
			else
			{
				base.PlayerOwner.CancelObjectiveArrow();
			}
		}
	}

	protected override void Child_PostAction()
	{
		ItemManager instance = ItemManager.GetInstance();
		if (instance != null)
		{
			instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		}
		if (m_PlayerOwner != null)
		{
			Player playerOwner = m_PlayerOwner;
			playerOwner.TryDeliverItem = (Player.DeliverItem)Delegate.Remove(playerOwner.TryDeliverItem, new Player.DeliverItem(DeliverItem));
		}
		if (m_SpawnedItems == null || !(m_PlayerOwner != null))
		{
			return;
		}
		for (int i = 0; i < m_SpawnedItems.Count; i++)
		{
			Item item = m_SpawnedItems[i];
			if (!(item != null) || !(item.m_ItemData != null))
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
		m_SpawnedItems.Clear();
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		if (ingameSave)
		{
			baseObj.Add(new JProperty("QuestItemGroupID", m_QuestItemGroupID));
			if (m_SpawnedItems != null)
			{
				JProperty jProperty = new JProperty("SpawnedItems");
				JArray jArray = new JArray();
				for (int i = 0; i < m_SpawnedItems.Count; i++)
				{
					jArray.Add(m_SpawnedItems[i].m_NetView.viewID);
				}
				jProperty.Add(jArray);
				baseObj.Add(jProperty);
			}
			if (m_DeliveredItems != null && m_DeliveredItems.Count > 0)
			{
				JProperty jProperty2 = new JProperty("DeliveredItems");
				JArray jArray2 = new JArray();
				for (int j = 0; j < m_DeliveredItems.Count; j++)
				{
					jArray2.Add(m_DeliveredItems[j].m_NetView.viewID);
				}
				jProperty2.Add(jArray2);
				baseObj.Add(jProperty2);
			}
			if (m_SpawnContainer != null)
			{
				baseObj.Add(new JProperty("ContainerToPutIn", m_SpawnContainer.NetView.viewID));
			}
			if (m_DeliverContainer != null)
			{
				baseObj.Add(new JProperty("TargetToDeliverTo", m_DeliverContainer.NetView.viewID));
			}
		}
		baseObj.Add(new JProperty("TargetType", (int)m_TargetType));
		baseObj.Add(new JProperty("ItemLocation", (int)m_ItemLocation));
		baseObj.Add(new JProperty("Random", m_bRandomItem));
		baseObj.Add(new JProperty("Tracked", m_bTrackItems));
		JProperty jProperty3 = new JProperty("ItemTargets");
		JArray jArray3 = new JArray();
		for (int k = 0; k < m_ItemTargets.Count; k++)
		{
			jArray3.Add(m_ItemTargets[k].m_ItemDataID);
		}
		jProperty3.Add(jArray3);
		baseObj.Add(jProperty3);
		if (m_RandomItemGroup != null)
		{
			baseObj.Add(new JProperty("ItemGroup", m_RandomItemGroup.m_RandomItemGroupID));
		}
		baseObj.Add(new JProperty("Quantity", m_QuantityNeeded));
		if (m_SceneItemLocation != null)
		{
			baseObj.Add(new JProperty("SceneLoc", m_SceneItemLocation.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneLoc_Scene", m_SceneItemLocation.m_UsedInScene));
		}
		if (m_SceneTarget != null)
		{
			baseObj.Add(new JProperty("SceneTarget", m_SceneTarget.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneTarget_Scene", m_SceneTarget.m_UsedInScene));
		}
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
			JProperty jProperty2 = json.Property("SpawnedItems");
			if (jProperty2 != null)
			{
				m_SpawnedItems = new List<Item>();
				if (jProperty2.Value.Type == JTokenType.Array)
				{
					JArray jArray = (JArray)jProperty2.Value;
					for (int i = 0; i < jArray.Count; i++)
					{
						if (jArray[i] != null)
						{
							JToken value = jArray[i];
							int viewID = value.Value<int>();
							Item item = T17NetView.Find<Item>(viewID);
							m_SpawnedItems.Add(item);
							item.LOADING_SetQuestGroupID(m_PlayerOwner, m_QuestItemGroupID);
						}
					}
				}
			}
			m_DeliveredItems = new List<Item>();
			JProperty jProperty3 = json.Property("DeliveredItems");
			if (jProperty3 != null && jProperty3.Value.Type == JTokenType.Array)
			{
				JArray jArray2 = (JArray)jProperty3.Value;
				for (int j = 0; j < jArray2.Count; j++)
				{
					if (jArray2[j] != null)
					{
						JToken value2 = jArray2[j];
						int viewID2 = value2.Value<int>();
						Item item2 = T17NetView.Find<Item>(viewID2);
						m_DeliveredItems.Add(item2);
					}
				}
			}
			JProperty jProperty4 = json.Property("ContainerToPutIn");
			if (jProperty4 != null)
			{
				int viewID3 = (int)jProperty4.Value;
				m_SpawnContainer = T17NetView.Find<ItemContainer>(viewID3);
			}
			JProperty jProperty5 = json.Property("TargetToDeliverTo");
			if (jProperty5 != null)
			{
				int viewID4 = (int)jProperty5.Value;
				m_DeliverContainer = T17NetView.Find<ItemContainer>(viewID4);
			}
		}
		JProperty jProperty6 = json.Property("TargetType");
		if (jProperty6 != null)
		{
			m_TargetType = (DeliveryTargetType)(int)jProperty6.Value;
		}
		JProperty jProperty7 = json.Property("ItemLocation");
		if (jProperty7 != null)
		{
			m_ItemLocation = (ItemLocation)(int)jProperty7.Value;
		}
		JProperty jProperty8 = json.Property("Random");
		if (jProperty8 != null)
		{
			m_bRandomItem = (bool)jProperty8.Value;
		}
		JProperty jProperty9 = json.Property("Tracked");
		if (jProperty9 != null)
		{
			m_bTrackItems = (bool)jProperty9.Value;
		}
		ResourcesItemDataManager instance = ResourcesItemDataManager.GetInstance();
		JProperty jProperty10 = json.Property("ItemTarget");
		if (jProperty10 != null)
		{
			int num = (int)jProperty10.Value;
			ItemData itemData = Resources.Load<ItemData>(instance.GetItemDataResourcePath(num));
			if (itemData.m_ItemDataID != num)
			{
				Debug.LogError("[BLC] ERROR: THE ITEM LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
			m_ItemTargets.Clear();
			m_ItemTargets.Add(itemData);
		}
		else
		{
			JProperty jProperty11 = json.Property("ItemTargets");
			if (jProperty11 != null)
			{
				m_ItemTargets.Clear();
				JArray jArray3 = (JArray)jProperty11.Value;
				if (jArray3 != null)
				{
					List<int> list = jArray3.Select((JToken c) => (int)c).ToList();
					for (int k = 0; k < list.Count; k++)
					{
						ItemData itemData2 = Resources.Load<ItemData>(instance.GetItemDataResourcePath(list[k]));
						if (itemData2.m_ItemDataID != list[k])
						{
							Debug.LogError("[BLC] ERROR: THE ITEM LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
						}
						m_ItemTargets.Add(itemData2);
					}
				}
			}
		}
		JProperty jProperty12 = json.Property("ItemGroup");
		if (jProperty12 != null)
		{
			m_RandomItemGroup = null;
			int num2 = (int)jProperty12.Value;
			m_RandomItemGroup = Resources.Load<RandomItemGroup>(instance.GetRandomItemGroupResourcePath(num2));
			if (m_RandomItemGroup.m_RandomItemGroupID != num2)
			{
				Debug.LogError("[BLC] ERROR: THE RANDOM ITEM GROUP LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
		}
		JProperty jProperty13 = json.Property("Quantity");
		if (jProperty13 != null)
		{
			m_QuantityNeeded = (int)jProperty13.Value;
		}
		JProperty jProperty14 = json.Property("SceneLoc");
		if (jProperty14 != null)
		{
			string text = (string)json.Property("SceneLoc_Scene").Value;
			int id = (int)jProperty14.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_SceneItemLocation = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty15 = json.Property("SceneTarget");
		if (jProperty15 != null)
		{
			string text2 = (string)json.Property("SceneTarget_Scene").Value;
			int id2 = (int)jProperty15.Value;
			if ((Application.isPlaying || !(text2 != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_SceneTarget = ObjectiveSceneElement.FindSceneReference(id2);
			}
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.ItemObjective;
	}
}
