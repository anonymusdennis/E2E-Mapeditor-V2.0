using System;
using System.Collections.Generic;
using UnityEngine;

public class LootingMenu : GameMenuBehaviour
{
	public delegate void ContainerRegisteredEvent();

	public GameObject m_PocketsParent;

	private InventoryItem[] m_PocketItems;

	private List<Item> m_PreviousItems = new List<Item>();

	private ItemContainer m_ItemContainer;

	private BaseInventoryBehaviour m_PocketInventoryBehaviour;

	public ContainerRegisteredEvent OnCharacterContainerRegistered;

	public ContainerRegisteredEvent OnCharacterContainerDeregistered;

	private int m_CurrentSelectedObjectIndex;

	private List<CraftManager.Recipe> m_CachedRecipies = new List<CraftManager.Recipe>();

	public string m_MouldKeyLocalisationKey = "Text.Interaction.Mould";

	public string m_CloneKeycardLocalisationKey = "Text.Interaction.Clone";

	private List<CraftManager.CraftItemRemovalInfo> m_DummyRemovalInfos = new List<CraftManager.CraftItemRemovalInfo>();

	private List<ItemContainer> m_CraftingContainerSources = new List<ItemContainer>();

	public InventoryItem.ContainerSetup m_PocketDefaultSetup;

	public ItemContainer ItemContainer => m_ItemContainer;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_PocketsParent != null)
		{
			m_PocketItems = m_PocketsParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_PocketInventoryBehaviour = m_PocketsParent.GetComponent<BaseInventoryBehaviour>();
		}
		ClearEntireDesk();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		PopulatePocketsWithItemContainer(ref m_GameMenuInformation.m_MenuRepresentativeContainer, firstTimeInit: true);
		m_PocketInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_Player);
		ItemContainer menuRepresentativeContainer = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(menuRepresentativeContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		ItemContainer menuRepresentativeContainer2 = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer2.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(menuRepresentativeContainer2.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		ItemContainer menuRepresentativeContainer3 = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer3.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Remove(menuRepresentativeContainer3.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(OnItemRemovedEvent));
		ItemContainer menuRepresentativeContainer4 = m_GameMenuInformation.m_MenuRepresentativeContainer;
		menuRepresentativeContainer4.OnItemRemovedEvent = (ItemContainer.ItemContainerEvent)Delegate.Combine(menuRepresentativeContainer4.OnItemRemovedEvent, new ItemContainer.ItemContainerEvent(OnItemRemovedEvent));
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToSwitchIventory();
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_Player);
		}
		if (m_GameMenuInformation.m_Player != null)
		{
			m_GameMenuInformation.m_Player.OnContainerViewed(m_GameMenuInformation.m_MenuRepresentativeContainer);
		}
		if (OnCharacterContainerRegistered != null)
		{
			OnCharacterContainerRegistered();
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_ItemContainer != null)
		{
			m_ItemContainer.GetItems(ref m_PreviousItems);
		}
		m_ItemContainer = null;
		DisableSmokesOnInventoryItems(m_PocketItems);
		ClearEntireDesk();
		if (m_GameMenuInformation.m_Player != null)
		{
			m_GameMenuInformation.m_Player.OnContainerClosed(m_GameMenuInformation.m_MenuRepresentativeContainer);
		}
		if (OnCharacterContainerDeregistered != null)
		{
			OnCharacterContainerDeregistered();
		}
		return true;
	}

	public void ClearEntireDesk()
	{
		ClearPockets();
	}

	public void ClearPockets()
	{
		if (m_PocketItems == null)
		{
			return;
		}
		for (int i = 0; i < m_PocketItems.Length; i++)
		{
			if (m_PocketItems[i] != null)
			{
				m_PocketItems[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
				m_PocketItems[i].SetIndexOfRepresentation(-1);
				if (m_PocketItems[i].InteractableElement != null)
				{
					m_PocketItems[i].InteractableElement.onClick.RemoveAllListeners();
				}
				InventoryItem obj = m_PocketItems[i];
				obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			}
		}
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
		if (m_ItemContainer != null)
		{
			PopulatePocketsWithItemContainer(ref m_ItemContainer, firstTimeInit: false);
		}
	}

	public void OnItemRemovedEvent(ItemContainer container, Item item)
	{
		SetPlayerLooting();
	}

	private void SetPlayerLooting()
	{
		if (m_GameMenuInformation.m_Player != null)
		{
			CharacterNetEvents.SendSetLootingEvent(m_GameMenuInformation.m_Player);
		}
	}

	public void PopulatePocketsWithItemContainer(ref ItemContainer container, bool firstTimeInit)
	{
		ClearEntireDesk();
		m_ItemContainer = container;
		m_CraftingContainerSources.Clear();
		m_CraftingContainerSources.Add(base.CurrentGamePlayer.m_ItemContainer);
		m_CraftingContainerSources.Add(m_ItemContainer);
		int currentSelectedObjectIndex = m_CurrentSelectedObjectIndex;
		if (container == null || m_PocketItems == null)
		{
			if (container == null)
			{
			}
			if (m_PocketItems != null)
			{
			}
			return;
		}
		int itemCount = container.GetItemCount();
		itemCount = Mathf.Clamp(itemCount, 0, m_PocketItems.Length);
		for (int i = 0; i < itemCount; i++)
		{
			Item item = container.GetItem(i);
			InventoryItem.ContainerSetup setup;
			if (CanPocketItemBeCrafted(item, out var actionVerb))
			{
				InventoryItem.ContainerSetup containerSetup = new InventoryItem.ContainerSetup();
				m_PocketDefaultSetup.CopyTo(containerSetup);
				containerSetup.m_DefaultContainerSetup = T17ItemTooltip.DisplaySetups.PrimaryAndSecondary;
				containerSetup.m_SecondaryInputText = actionVerb;
				setup = containerSetup;
			}
			else
			{
				setup = m_PocketDefaultSetup;
			}
			m_PocketItems[i].ChangeDisplaySetup(setup);
			if (!(item != null))
			{
				continue;
			}
			m_PocketItems[i].ResetBackgroundColor();
			m_PocketItems[i].SetItemContentImage(item.m_ItemData.m_ItemUIImage);
			m_PocketItems[i].SetItem(item);
			m_PocketItems[i].SetIndexOfRepresentation(i);
			int index = i;
			if (m_PocketInventoryBehaviour != null && m_PocketItems[i].InteractableElement != null)
			{
				m_PocketItems[i].InteractableElement.onClick.RemoveAllListeners();
				m_PocketItems[i].InteractableElement.onClick.AddListener(delegate
				{
					m_PocketInventoryBehaviour.OnItemClicked(index);
				});
				InventoryItem obj = m_PocketItems[i];
				obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			}
			if (i == currentSelectedObjectIndex)
			{
				m_CurrentSelectedObjectIndex = i;
			}
			if (m_PocketInventoryBehaviour == null)
			{
			}
			if (!(m_PocketItems[i].InteractableElement == null))
			{
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_PocketItems, m_PreviousItems);
		}
		container.GetItems(ref m_PreviousItems);
	}

	private void InventoryItemWasSelected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = representationIndex;
	}

	public BaseInventoryBehaviour GetPocketInventoryBehaviour()
	{
		return m_PocketInventoryBehaviour;
	}

	public InventoryItem FindItemInContainer(ItemData item)
	{
		for (int i = 0; i < m_PocketItems.Length; i++)
		{
			if (m_PocketItems[i] != null && m_PocketItems[i].GetLinkedItem() != null && m_PocketItems[i].GetLinkedItem().ItemDataID == item.m_ItemDataID)
			{
				return m_PocketItems[i];
			}
		}
		return null;
	}

	protected override void Update()
	{
		base.Update();
		if (m_CurrentSelectedObjectIndex == -1 || base.CurrentRewiredPlayer == null || !(m_ItemContainer != null) || !base.CurrentRewiredPlayer.GetButtonDown("UI_PutIntoHidden"))
		{
			return;
		}
		Item item = m_ItemContainer.GetItem(m_CurrentSelectedObjectIndex);
		if (!(item != null))
		{
			return;
		}
		CraftManager.Recipe recipe = null;
		if ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Key) || (bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Keycard))
		{
			CraftManager.GetInstance().GetRecipiesThatUseIngredient(item.m_ItemData, m_CachedRecipies);
			if (m_CachedRecipies.Count > 0)
			{
				recipe = m_CachedRecipies[0];
			}
		}
		if (recipe != null)
		{
			if (base.CurrentGamePlayer.m_CharacterStats.Intellect >= (float)recipe.m_Intellect)
			{
				base.CurrentGamePlayer.m_bPendingRequest = true;
				base.CurrentGamePlayer.m_NetView.RPC("Master_CraftItemFromContainers", NetTargets.MasterClient, CraftManager.GetInstance().GetRecipeId(recipe), base.CurrentGamePlayer.m_ItemContainer.NetView.viewID, new int[1] { m_ItemContainer.NetView.viewID }, base.CurrentGamePlayer.m_NetView.viewID);
			}
			else
			{
				SpeechManager.GetInstance().SaySomething(base.CurrentGamePlayer, "Text.Crafting.LowIntellect", SpeechTone.Negative, 2f);
			}
		}
	}

	private bool CanPocketItemBeCrafted(Item item, out string actionVerb)
	{
		actionVerb = null;
		bool result = false;
		if (item != null && ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Key) || (bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Keycard)))
		{
			if ((bool)item.HasFunctionality(BaseItemFunctionality.Functionality.Key))
			{
				actionVerb = m_MouldKeyLocalisationKey;
			}
			else
			{
				actionVerb = m_CloneKeycardLocalisationKey;
			}
			CraftManager.GetInstance().GetRecipiesThatUseIngredient(item.m_ItemData, m_CachedRecipies);
			if (m_CachedRecipies.Count > 0)
			{
				CraftManager.Recipe recipe = m_CachedRecipies[0];
				result = CraftManager.GetInstance().GetCraftingIndiciesForRecipe(recipe, ref m_DummyRemovalInfos, m_CraftingContainerSources);
			}
		}
		return result;
	}
}
