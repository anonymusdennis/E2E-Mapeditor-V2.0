using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
	public enum ReponseType
	{
		RT_CLOSE_CONTAINER,
		RT_ITEM_SELECT,
		RT_CRAFT_ITEM
	}

	private Player m_Player;

	private int m_ResponseObjectID;

	private ReponseType m_ResponseType;

	private const float HELD_BUTTON_TIME = 2f;

	private Dictionary<int, CraftManager.CraftInfo> m_CraftResponsesToBeHandled = new Dictionary<int, CraftManager.CraftInfo>();

	private void Start()
	{
		m_Player = base.gameObject.GetComponent<Player>();
	}

	[PunRPC]
	public void RPC_CloseContainer(PhotonMessageInfo info)
	{
		m_ResponseType = ReponseType.RT_CLOSE_CONTAINER;
		if (null != m_Player && null != m_Player.m_OpenContainer)
		{
			m_Player.m_OpenContainer.ReleaseLock();
		}
		m_Player.m_NetView.RPC("RPC_Response", info.sender, m_ResponseType, -1, 0);
	}

	[PunRPC]
	public void RPC_MASTER_SelectInventoryItem(int sourceID, int destID, int itemViewId, bool fromHidden, bool intoHidden, PhotonMessageInfo info)
	{
		if (sourceID == -1)
		{
			return;
		}
		PhotonView photonView = PhotonView.Find(sourceID);
		if (null == photonView)
		{
			return;
		}
		ItemContainer component = photonView.GetComponent<ItemContainer>();
		if (component == null)
		{
			return;
		}
		ItemContainer itemContainer = null;
		if (destID == -1)
		{
			return;
		}
		PhotonView photonView2 = PhotonView.Find(destID);
		if (null == photonView2)
		{
			return;
		}
		itemContainer = photonView2.GetComponent<ItemContainer>();
		if (null == itemContainer)
		{
			return;
		}
		Item item = ((!fromHidden) ? component.GetItemByViewID(itemViewId) : component.GetHiddenItemByViewID(itemViewId));
		if (null == item)
		{
			return;
		}
		m_ResponseType = ReponseType.RT_ITEM_SELECT;
		bool flag = false;
		Character characterOwner = itemContainer.GetCharacterOwner();
		if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && (itemContainer.m_ContainerType == ItemContainer.ItemContainerType.Inmate || itemContainer.m_ContainerType == ItemContainer.ItemContainerType.Guard) && characterOwner.GetEquippedItem() == null && characterOwner.CanEquipItem(item))
		{
			component.MoveItemToCharacterEquipedSlot(item.m_NetView.viewID, characterOwner.m_NetView.viewID);
			flag = true;
		}
		if (!flag)
		{
			if ((intoHidden && itemContainer.IsHiddenFull()) || (!intoHidden && itemContainer.IsVisibleFull()))
			{
				TutorialManager.GetInstance().StartTutorialRPC(m_Player, TutorialSubject.DiscardItem);
				SpeechManager.GetInstance().SaySomething(characterOwner, "Text.Player.FullInventory", SpeechTone.Negative, 3f, 10);
			}
			else
			{
				component.MoveItemToAnotherContainerRPC(item.m_NetView.viewID, itemContainer.NetView.viewID, intoHidden);
				m_ResponseObjectID = item.m_NetView.viewID;
			}
		}
		m_Player.m_NetView.RPC("RPC_Response", info.sender, m_ResponseType, m_ResponseObjectID, 0);
	}

	[PunRPC]
	public void RPC_MASTER_PutEquipedItemIntoContainer(int sourceCharacterID, int destID, bool bIntoHidden)
	{
		Character character = T17NetView.Find<Character>(sourceCharacterID);
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(destID);
		if (!(character == null) && !(itemContainer == null))
		{
			if ((bIntoHidden && itemContainer.IsHiddenFull()) || (!bIntoHidden && itemContainer.IsVisibleFull()))
			{
				TutorialManager.GetInstance().StartTutorialRPC(m_Player, TutorialSubject.DiscardItem);
				return;
			}
			m_Player.m_NetView.RPC("RPC_MoveEquipedItemToContainer", NetTargets.All, sourceCharacterID, destID, bIntoHidden);
		}
	}

	[PunRPC]
	private void RPC_MoveEquipedItemToContainer(int sourceCharacterID, int destID, bool bIntoHidden)
	{
		Character character = T17NetView.Find<Character>(sourceCharacterID);
		if (!(character != null))
		{
			return;
		}
		Item equippedItem = character.GetEquippedItem();
		if (!(equippedItem != null))
		{
			return;
		}
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(destID);
		if (itemContainer != null)
		{
			character.SetEquippedItem(null, bTellOthers: false, bAddOldToInventory: false, RPC_CallContexts.All);
			if (((bIntoHidden && !itemContainer.IsHiddenFull()) || (!bIntoHidden && !itemContainer.IsVisibleFull())) && !itemContainer.LOCAL_AddItem(equippedItem, bIntoHidden))
			{
				ItemManager.GetInstance().RequestReleaseItem(equippedItem, RPC_CallContexts.All);
			}
		}
	}

	[PunRPC]
	public void RPC_CraftItem(int itemContainerID, int recipeIndex, int[] preferredIndices, int requestingPlayerId, PhotonMessageInfo info)
	{
		m_ResponseObjectID = -1;
		ItemContainer itemContainer = null;
		if (itemContainerID != -1)
		{
			itemContainer = T17NetView.Find<ItemContainer>(itemContainerID);
		}
		if (!(itemContainer != null) || itemContainer.GetItemCount() <= 0)
		{
			return;
		}
		int[] usingItemIndices = new int[8];
		if (CraftManager.GetInstance().HasItemsForRecipe(recipeIndex, itemContainer, ref usingItemIndices))
		{
			CraftManager.Recipe recipe = CraftManager.GetInstance().GetCurrentRecipes()[recipeIndex];
			if (preferredIndices != null)
			{
				int destroyableIngredientCount = recipe.GetDestroyableIngredientCount();
				CraftManager.GetInstance().ReplaceWithPreferredItems(itemContainer, destroyableIngredientCount, ref usingItemIndices, preferredIndices);
			}
			CraftManager.CraftInfo value = default(CraftManager.CraftInfo);
			value.RemovalInfo = new CraftManager.CraftItemRemovalInfo[1];
			value.RemovalInfo[0].IndicesToRemove = new int[8];
			usingItemIndices.CopyTo(value.RemovalInfo[0].IndicesToRemove, 0);
			value.RemovalInfo[0].ItemContainer = itemContainer;
			value.RemovalInfo[0].NumIndiciesToRemove = recipe.GetDestroyableIngredientCount();
			value.DestinationContainer = itemContainer;
			value.Recipe = recipe;
			value.PhotonPlayer = info.sender;
			value.GamePlayerViewId = requestingPlayerId;
			m_CraftResponsesToBeHandled.Add(ItemManager.GetInstance().GetNextRequestID(), value);
			int requestID = -1;
			ItemManager.GetInstance().AssignItemRPC(-1, recipe.m_Product.m_ItemDataID, OnItemMgrResponse, ref requestID);
		}
		else
		{
			m_ResponseObjectID = -1;
		}
	}

	[PunRPC]
	private void Master_CraftItemFromContainers(int recipeId, int destinationContainerId, int[] otherSourceContainerIds, int requestingPlayerId, PhotonMessageInfo info)
	{
		if (ItemManager.GetInstance() == null || CraftManager.GetInstance() == null)
		{
			return;
		}
		ItemContainer itemContainer = T17NetView.Find<ItemContainer>(destinationContainerId);
		CraftManager.Recipe recipeByID = CraftManager.GetInstance().GetRecipeByID(recipeId);
		Character character = T17NetView.Find<Character>(destinationContainerId);
		if (!(itemContainer != null) || recipeByID == null)
		{
			return;
		}
		bool flag = false;
		if (itemContainer.GetFreeSpaceCount() > 0)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < recipeByID.m_Ingredients.Length; i++)
			{
				Item item = ((!(character != null)) ? null : character.GetEquippedItem());
				ItemData itemData = recipeByID.m_Ingredients[i];
				if (recipeByID.m_IngredientsToBeDestroyed[i] && itemData != null && (itemContainer.HasItem(itemData.m_ItemDataID) > 0 || (item != null && item.ItemDataID == itemData.m_ItemDataID)))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		List<CraftManager.CraftItemRemovalInfo> removalInfos = new List<CraftManager.CraftItemRemovalInfo>();
		List<ItemContainer> list = new List<ItemContainer>();
		list.Add(itemContainer);
		for (int j = 0; j < otherSourceContainerIds.Length; j++)
		{
			ItemContainer itemContainer2 = T17NetView.Find<ItemContainer>(otherSourceContainerIds[j]);
			if (itemContainer2 != null)
			{
				list.Add(itemContainer2);
			}
		}
		if (CraftManager.GetInstance().GetCraftingIndiciesForRecipe(recipeByID, ref removalInfos, list))
		{
			if (removalInfos != null && itemContainer.NetView != null && recipeByID.m_Product != null)
			{
				CraftManager.CraftInfo value = default(CraftManager.CraftInfo);
				value.RemovalInfo = removalInfos.ToArray();
				value.DestinationContainer = itemContainer;
				value.Recipe = recipeByID;
				value.PhotonPlayer = info.sender;
				value.GamePlayerViewId = requestingPlayerId;
				m_CraftResponsesToBeHandled.Add(ItemManager.GetInstance().GetNextRequestID(), value);
				int requestID = -1;
				ItemManager.GetInstance().AssignItemRPC(itemContainer.NetView.ownerId, recipeByID.m_Product.m_ItemDataID, OnItemMgrResponse, ref requestID);
			}
		}
		else
		{
			m_Player.m_NetView.RPC("RPC_Response", info.sender, ReponseType.RT_CRAFT_ITEM, -1, 0);
		}
	}

	private void OnItemMgrResponse(Item item, int eventID)
	{
		if (!m_CraftResponsesToBeHandled.ContainsKey(eventID))
		{
			return;
		}
		CraftManager.CraftInfo craftInfo = m_CraftResponsesToBeHandled[eventID];
		Player player = T17NetView.Find<Player>(craftInfo.GamePlayerViewId);
		CraftManager.GetInstance().HaveCraftedRecipe(craftInfo.Recipe);
		for (int i = 0; i < craftInfo.RemovalInfo.Length; i++)
		{
			CraftManager.CraftItemRemovalInfo craftItemRemovalInfo = craftInfo.RemovalInfo[i];
			Character characterOwner = craftItemRemovalInfo.ItemContainer.GetCharacterOwner();
			if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer)
			{
				for (int j = 0; j < craftItemRemovalInfo.IndicesToRemove.Length; j++)
				{
					if (craftItemRemovalInfo.IndicesToRemove[j] == -1)
					{
						Item equippedItem = characterOwner.GetEquippedItem();
						if (equippedItem != null)
						{
							characterOwner.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
							ItemManager.GetInstance().RequestReleaseItem(equippedItem);
						}
						break;
					}
				}
			}
			craftItemRemovalInfo.ItemContainer.RemoveItems(craftItemRemovalInfo.NumIndiciesToRemove, ref craftItemRemovalInfo.IndicesToRemove, releaseToManager: true);
		}
		if (item != null)
		{
			if (!craftInfo.DestinationContainer.AddItemRPC(item) && player != null)
			{
				bool flag = false;
				if (player.GetEquippedItem() == null)
				{
					flag = player.SetEquippedItem(item, bTellOthers: true, bAddOldToItemContainer: false, RPC_CallContexts.Master);
				}
				if (!flag)
				{
					item.DropItemInLevel(player, player.transform.position);
				}
			}
			m_ResponseObjectID = item.m_NetView.viewID;
			GoogleAnalyticsV3.LogCommericalAnalyticEvent("Items Crafted", item.m_ItemData.m_ItemLocalizationTag + " Crafted", string.Empty, 0L);
		}
		else
		{
			m_ResponseObjectID = -1;
		}
		m_Player.m_NetView.RPC("RPC_Response", craftInfo.PhotonPlayer, ReponseType.RT_CRAFT_ITEM, m_ResponseObjectID, 0);
		m_CraftResponsesToBeHandled.Remove(eventID);
	}

	[PunRPC]
	private void RPC_Response(ReponseType type, int objectID, InGameRootMenu.InGameMenuTypeToOpen menuType)
	{
		m_ResponseType = type;
		m_ResponseObjectID = objectID;
		if (!m_Player.m_bPendingRequest)
		{
			return;
		}
		m_Player.m_bPendingRequest = false;
		switch (m_ResponseType)
		{
		case ReponseType.RT_CLOSE_CONTAINER:
			if (m_Player.IsBrowsingPauseMenu)
			{
				m_Player.SetCloseInventoryOnPauseMenuHide();
				break;
			}
			m_Player.CloseInventory();
			m_Player.m_OpenContainer = null;
			break;
		case ReponseType.RT_ITEM_SELECT:
			if (m_ResponseObjectID != -1)
			{
				InGameMenuFlow.Instance.OnSelectItem(m_Player.m_PlayerCameraManagerBindingID);
			}
			break;
		case ReponseType.RT_CRAFT_ITEM:
			if (m_ResponseObjectID != -1)
			{
				PhotonView photonView = PhotonView.Find(m_ResponseObjectID);
				if (photonView != null)
				{
					Item component = photonView.GetComponent<Item>();
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Craft_Complete, m_Player.gameObject);
					if (LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial)
					{
						StatSystem.GetInstance().AddIDStat(6, component.ItemDataID, m_Player.m_Gamer);
					}
					if (component != null && CraftManager.OnItemCrafted != null)
					{
						CraftManager.OnItemCrafted(component, m_Player);
					}
				}
			}
			InGameMenuFlow.Instance.OnSelectItem(m_Player.m_PlayerCameraManagerBindingID);
			break;
		}
	}
}
