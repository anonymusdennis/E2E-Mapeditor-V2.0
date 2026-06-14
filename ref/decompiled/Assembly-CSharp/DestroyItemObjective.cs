using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class DestroyItemObjective : BaseObjective
{
	public enum DestroyObjectiveMethod
	{
		Flush
	}

	public DestroyObjectiveMethod m_Method;

	public ItemObjective.ItemLocation m_ItemLocation = ItemObjective.ItemLocation.InRandomDesk;

	public bool m_bRandomItem;

	public ItemData m_ItemTarget;

	public RandomItemGroup m_RandomItemGroup;

	public ObjectiveSceneElement m_SceneItemLocation;

	public bool m_bResetOnItemsInContraband;

	public int m_IndexToResetTo = -1;

	private Item m_SpawnedItem;

	private ItemContainer m_ContainerToPutIn;

	private bool m_bDestroyedItem;

	private int m_SpawnedItemHUDPin = -1;

	private int m_QuestItemGroupID = -1;

	private const string TODESTROYTOKEN = "$ItemToDestroy";

	private const string LOCATIONTOKEN = "$ItemLocation";

	private int m_ItemSpawnEventID = -1;

	private bool m_bHudPinsOn;

	private ItemContainer m_ContrabandDeskItemContainer;

	protected override void Child_PickAllTargets()
	{
		string localized = "Text.Quest.Desk";
		Localization.Get("Text.Quest.Desk", out localized);
		switch (m_ItemLocation)
		{
		case ItemObjective.ItemLocation.OnRandomInmate:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate);
			break;
		case ItemObjective.ItemLocation.OnRandomGuard:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Guard);
			break;
		case ItemObjective.ItemLocation.OnInmateOfQuestGiversGang:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate);
			break;
		case ItemObjective.ItemLocation.OnInmateOfQuestGiversRivalGang:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Inmate);
			break;
		case ItemObjective.ItemLocation.InRandomDesk:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.Desk);
			if (!(m_ContainerToPutIn == null))
			{
				break;
			}
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
			if (m_ContainerToPutIn != null)
			{
				DeskInteraction component = m_ContainerToPutIn.GetComponent<DeskInteraction>();
				if (component != null && component.GetOwner() != null)
				{
					localized = component.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case ItemObjective.ItemLocation.InRandomInmateDesk:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
			if (m_ContainerToPutIn != null)
			{
				DeskInteraction component2 = m_ContainerToPutIn.GetComponent<DeskInteraction>();
				if (component2 != null && component2.GetOwner() != null)
				{
					localized = component2.GetOwner().m_CharacterCustomisation.m_DisplayName;
				}
			}
			break;
		case ItemObjective.ItemLocation.InRandomGuardDesk:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskGuard);
			break;
		case ItemObjective.ItemLocation.InDeskOfQuestGiversGang:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
			break;
		case ItemObjective.ItemLocation.InDeskOfQuestGiversRivalGang:
			m_ContainerToPutIn = ItemContainerManager.GetInstance().GetQuestableContainer(ItemContainer.ItemContainerType.DeskInmate);
			break;
		case ItemObjective.ItemLocation.InQuestGiversDesk:
		{
			RoomBlob myCell = m_QuestGiver.GetMyCell();
			if (myCell != null)
			{
				DeskInteraction deskInteraction = (DeskInteraction)myCell.GetRoomBlobData<RoomBlob_Cell>().GetCellObject(typeof(DeskInteraction), m_QuestGiver);
				if (deskInteraction != null)
				{
					m_ContainerToPutIn = deskInteraction.m_LinkedItemContainer;
				}
			}
			break;
		}
		case ItemObjective.ItemLocation.InPrison:
			if (m_SceneItemLocation != null && m_SceneItemLocation.m_LinksTo == ObjectiveSceneElement.ObjectiveSceneElementType.ItemContainer)
			{
				m_ContainerToPutIn = m_SceneItemLocation.GetComponent<ItemContainer>();
			}
			break;
		case ItemObjective.ItemLocation.Player:
			m_ContainerToPutIn = m_PlayerOwner.m_ItemContainer;
			break;
		}
		if (m_ContainerToPutIn == null)
		{
			m_ObjectiveStatus = ObjectiveStatus.Invalid;
			return;
		}
		Character characterOwner = m_ContainerToPutIn.GetCharacterOwner();
		InternalTokenUpdate("$ItemLocation", (!(characterOwner != null)) ? localized : characterOwner.m_CharacterCustomisation.m_DisplayName, string.Empty);
		if (ItemManager.GetInstance() != null && (m_bRandomItem || m_ItemTarget == null))
		{
			if (m_RandomItemGroup != null)
			{
				m_ItemTarget = m_RandomItemGroup.GetRandomItem(bUniqueItems: true);
			}
			else
			{
				m_ItemTarget = ItemManager.GetInstance().GetRandomItemFromAllowedList();
			}
		}
		string localized2 = string.Empty;
		Localization.Get(m_ItemTarget.m_ItemLocalizationTag, out localized2);
		InternalTokenUpdate("$ItemToDestroy", localized2, string.Empty);
	}

	protected override void Child_RegisterTokens(ref ObjectiveTree objectiveTree)
	{
		AddTokenInternal("$ItemToDestroy", Localization.TokenReplaceType.Item);
		AddTokenInternal("$ItemLocation", Localization.TokenReplaceType.ItemContainer);
	}

	protected override void Child_Reset()
	{
		m_SpawnedItem = null;
	}

	protected override void Child_Initialize()
	{
	}

	protected override void Child_PreAction()
	{
		if (!m_bDestroyedItem)
		{
			ItemManager instance = ItemManager.GetInstance();
			instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
			ItemManager instance2 = ItemManager.GetInstance();
			instance2.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Combine(instance2.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
			if (ItemManager.GetInstance() != null && !m_SpawnedItem)
			{
				ItemManager.GetInstance().AssignItemRPC(-1, m_ItemTarget.m_ItemDataID, OnItemMgrResponse, ref m_ItemSpawnEventID);
			}
		}
		if (m_bResetOnItemsInContraband)
		{
			m_ContrabandDeskItemContainer = ItemContainer.FindFirstContrabandDeskItemContainer();
		}
	}

	private void OnItemMgrResponse(Item newItem, int eventID)
	{
		if (!(newItem != null) || eventID != m_ItemSpawnEventID)
		{
			return;
		}
		if (m_ContainerToPutIn != null)
		{
			newItem.SetAsQuestItem(isQuestItem: true, m_PlayerOwner, ref m_QuestItemGroupID);
			ItemContainer.MakeRoomForQuestItem(m_ContainerToPutIn);
			if (!m_ContainerToPutIn.AddItemRPC(newItem))
			{
				newItem.DropItemInLevel(m_PlayerOwner, m_ContainerToPutIn.transform.position);
			}
		}
		m_SpawnedItem = newItem;
		m_ItemSpawnEventID = -1;
	}

	private void OnQuestItemDestroyed(Item item, int eventID)
	{
		if (!m_bInPostAction && m_SpawnedItem != null && m_SpawnedItem.m_NetView.viewID == item.m_NetView.viewID)
		{
			m_bDestroyedItem = true;
			m_SpawnedItem = null;
		}
	}

	protected override bool Child_EvaluateDependencies()
	{
		if (m_ObjectiveStatus == ObjectiveStatus.Done)
		{
			return true;
		}
		return false;
	}

	protected override bool Child_EvaluateStatus()
	{
		if (!m_bDestroyedItem && m_SpawnedItem != null && m_bHudPinsOn && m_SpawnedItem.m_ContainerViewID != -1)
		{
			ItemContainer itemContainer = m_SpawnedItem.m_ItemContainer;
			if (itemContainer != null)
			{
				GameObject gameObject = itemContainer.gameObject;
				if (itemContainer == m_PlayerOwner.m_ItemContainer)
				{
					if (m_SpawnedItemHUDPin != -1)
					{
						if (base.PlayerOwner != null)
						{
							base.PlayerOwner.CancelObjectiveArrow();
						}
						PinManager.GetInstance().RemovePin(m_SpawnedItemHUDPin);
						m_SpawnedItemHUDPin = -1;
					}
				}
				else
				{
					GameObject gameObject2 = gameObject.gameObject;
					if (m_SpawnedItem.m_ContainerViewID == LevelScript.LevelItemContainerViewID)
					{
						gameObject2 = m_SpawnedItem.gameObject;
					}
					if (m_SpawnedItemHUDPin == -1)
					{
						m_SpawnedItemHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, gameObject2, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(gameObject2.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
					}
					else if (m_SpawnedItemHUDPin != -1)
					{
						GameObject currentPinTarget = PinManager.GetInstance().GetCurrentPinTarget(m_SpawnedItemHUDPin);
						if (currentPinTarget != gameObject)
						{
							PinManager.GetInstance().UpdatePinTarget(m_SpawnedItemHUDPin, gameObject2);
						}
					}
				}
			}
			else if ((m_SpawnedItem == m_PlayerOwner.GetEquippedItem() || m_SpawnedItem == m_PlayerOwner.GetOutFit()) && m_SpawnedItemHUDPin != -1)
			{
				if (base.PlayerOwner != null)
				{
					base.PlayerOwner.CancelObjectiveArrow();
				}
				PinManager.GetInstance().RemovePin(m_SpawnedItemHUDPin);
				m_SpawnedItemHUDPin = -1;
			}
		}
		return m_bDestroyedItem;
	}

	protected override void Child_SetHUDPins(bool on)
	{
		m_bHudPinsOn = on;
		if (on)
		{
			if (m_SpawnedItem != null)
			{
				GameObject gameObject = null;
				if (T17NetManager.IsValidNetViewId(m_SpawnedItem.m_ContainerViewID) && m_SpawnedItem.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID)
				{
					gameObject = ((m_SpawnedItem.m_ContainerViewID == LevelScript.LevelItemContainerViewID) ? m_SpawnedItem.gameObject : PhotonView.Find(m_SpawnedItem.m_ContainerViewID).gameObject);
				}
				if (gameObject != null)
				{
					m_SpawnedItemHUDPin = PinManager.GetInstance().CreatePin(bForMainMap: true, bForMiniMap: true, gameObject, null, bUpdatePosition: true, FloorManager.GetInstance().FindFloorAtZ(gameObject.transform.position.z), new Player[1] { m_PlayerOwner }, PinManager.Pin.PinFilterType.Objectives, edgable: true, floorTrackable: true, directional: false, animation: ObjectiveManager.GetInstance().m_QuestTargetAnimation, toolTipTag: string.Empty);
				}
			}
		}
		else if (m_SpawnedItemHUDPin != -1)
		{
			PinManager.GetInstance().RemovePin(m_SpawnedItemHUDPin);
			m_SpawnedItemHUDPin = -1;
		}
	}

	protected override void Child_SetHUDArrow(bool on)
	{
		if (!(base.PlayerOwner != null))
		{
			return;
		}
		if (on)
		{
			if (m_SpawnedItem != null)
			{
				T17NetView t17NetView = null;
				if (T17NetManager.IsValidNetViewId(m_SpawnedItem.m_ContainerViewID) && m_SpawnedItem.m_ContainerViewID != LevelScript.LevelItemContainerViewID)
				{
					t17NetView = PhotonView.Find(m_SpawnedItem.m_ContainerViewID).GetComponent<T17NetView>();
				}
				if (t17NetView != null)
				{
					base.PlayerOwner.SetObjectiveArrowTarget(t17NetView);
				}
			}
		}
		else if (m_SpawnedItemHUDPin != -1)
		{
			base.PlayerOwner.CancelObjectiveArrow();
		}
	}

	protected override void Child_PostAction()
	{
		ItemManager instance = ItemManager.GetInstance();
		instance.OnQuestItemDestroyed = (ItemManager.ItemManagerEvent)Delegate.Remove(instance.OnQuestItemDestroyed, new ItemManager.ItemManagerEvent(OnQuestItemDestroyed));
		if (!(m_SpawnedItem != null) || !(m_PlayerOwner != null))
		{
			return;
		}
		bool flag = true;
		if (m_SpawnedItem.m_ContainerViewID != m_PlayerOwner.m_ItemContainer.NetView.viewID && m_SpawnedItem != m_PlayerOwner.GetEquippedItem() && m_SpawnedItem != m_PlayerOwner.GetOutFit())
		{
			PhotonView photonView = PhotonView.Find(m_SpawnedItem.m_ContainerViewID);
			if (photonView != null)
			{
				ItemContainer component = photonView.GetComponent<ItemContainer>();
				if (component != null)
				{
					component.RemoveItemRPC(m_SpawnedItem, releaseToManager: true);
					flag = false;
				}
			}
		}
		if (flag)
		{
			m_SpawnedItem.SetAsQuestItem(isQuestItem: false, m_PlayerOwner, ref m_QuestItemGroupID);
		}
	}

	protected override int Child_EvaluateResetCondition()
	{
		if (m_bResetOnItemsInContraband && m_SpawnedItem != null && m_ContrabandDeskItemContainer != null)
		{
			int num = m_ContrabandDeskItemContainer.FindItemIndex(m_SpawnedItem);
			if (num != -1)
			{
				return m_IndexToResetTo;
			}
		}
		return -1;
	}

	protected override string Child_Save(JObject baseObj, bool ingameSave)
	{
		baseObj.Add(new JProperty("DestroyMethod", (int)m_Method));
		if (ingameSave)
		{
			baseObj.Add(new JProperty("QuestItemGroupID", m_QuestItemGroupID));
			if (m_SpawnedItem != null)
			{
				baseObj.Add("SpawnedItem", m_SpawnedItem.m_NetView.viewID);
			}
			if (m_ContainerToPutIn != null)
			{
				baseObj.Add("ContainerToPutIn", m_ContainerToPutIn.NetView.viewID);
			}
			baseObj.Add("DestroyedItem", m_bDestroyedItem);
		}
		baseObj.Add(new JProperty("ItemLocation", (int)m_ItemLocation));
		baseObj.Add(new JProperty("Random", m_bRandomItem));
		if (m_ItemTarget != null)
		{
			baseObj.Add(new JProperty("ItemTarget", m_ItemTarget.m_ItemDataID));
		}
		if (m_RandomItemGroup != null)
		{
			baseObj.Add(new JProperty("ItemGroup", m_RandomItemGroup.m_RandomItemGroupID));
		}
		if (m_SceneItemLocation != null)
		{
			baseObj.Add(new JProperty("SceneLoc", m_SceneItemLocation.m_ObjectiveElementID));
			baseObj.Add(new JProperty("SceneLoc_Scene", m_SceneItemLocation.m_UsedInScene));
		}
		baseObj.Add(new JProperty("ResetOnItemsInContraband", m_bResetOnItemsInContraband));
		baseObj.Add(new JProperty("IndexToResetTo", m_IndexToResetTo));
		return GetType().ToString() + "_" + baseObj.ToString();
	}

	protected override void Child_Load(JObject json, bool ingameLoad)
	{
		m_Method = (DestroyObjectiveMethod)(int)json.Property("DestroyMethod").Value;
		if (ingameLoad)
		{
			JProperty jProperty = json.Property("QuestItemGroupID");
			if (jProperty != null)
			{
				m_QuestItemGroupID = (int)jProperty.Value;
			}
			JProperty jProperty2 = json.Property("SpawnedItem");
			if (jProperty2 != null)
			{
				int viewID = (int)jProperty2.Value;
				m_SpawnedItem = PhotonView.Find(viewID).GetComponent<Item>();
				m_SpawnedItem.LOADING_SetQuestGroupID(m_PlayerOwner, m_QuestItemGroupID);
			}
			JProperty jProperty3 = json.Property("ContainerToPutIn");
			if (jProperty3 != null)
			{
				int viewID2 = (int)jProperty3.Value;
				m_ContainerToPutIn = PhotonView.Find(viewID2).GetComponent<ItemContainer>();
			}
			JProperty jProperty4 = json.Property("DestroyedItem");
			if (jProperty4 != null)
			{
				m_bDestroyedItem = (bool)jProperty4.Value;
			}
		}
		ResourcesItemDataManager instance = ResourcesItemDataManager.GetInstance();
		JProperty jProperty5 = json.Property("ItemLocation");
		if (jProperty5 != null)
		{
			m_ItemLocation = (ItemObjective.ItemLocation)(int)jProperty5.Value;
		}
		JProperty jProperty6 = json.Property("Random");
		if (jProperty6 != null)
		{
			m_bRandomItem = (bool)jProperty6.Value;
		}
		JProperty jProperty7 = json.Property("ItemTarget");
		if (jProperty7 != null)
		{
			int num = (int)jProperty7.Value;
			m_ItemTarget = Resources.Load<ItemData>(instance.GetItemDataResourcePath(num));
			if (m_ItemTarget.m_ItemDataID != num)
			{
				Debug.LogError("[BLC] ERROR: THE ITEM LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
		}
		JProperty jProperty8 = json.Property("ItemGroup");
		if (jProperty8 != null)
		{
			int num2 = (int)jProperty8.Value;
			m_RandomItemGroup = Resources.Load<RandomItemGroup>(instance.GetRandomItemGroupResourcePath(num2));
			if (m_RandomItemGroup.m_RandomItemGroupID != num2)
			{
				Debug.LogError("[BLC] ERROR: THE RANDOM ITEM GROUP LOOK UP HAS FAILED!!! (Please speak to Brandon Calvert about the issue)");
			}
		}
		JProperty jProperty9 = json.Property("SceneLoc");
		if (jProperty9 != null)
		{
			string text = (string)json.Property("SceneLoc_Scene").Value;
			int id = (int)jProperty9.Value;
			if ((Application.isPlaying || !(text != SceneManagerHelper.ActiveSceneName)) && Application.isPlaying)
			{
				m_SceneItemLocation = ObjectiveSceneElement.FindSceneReference(id);
			}
		}
		JProperty jProperty10 = json.Property("ResetOnItemsInContraband");
		if (jProperty10 != null)
		{
			m_bResetOnItemsInContraband = (bool)jProperty10.Value;
		}
		JProperty jProperty11 = json.Property("IndexToResetTo");
		if (jProperty11 != null)
		{
			m_IndexToResetTo = (int)jProperty11.Value;
		}
	}

	public override ObjectiveType GetObjectiveType()
	{
		return ObjectiveType.DestroyItemObjective;
	}
}
