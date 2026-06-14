using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class DeskMenu : GameMenuBehaviour
{
	public delegate void ContainerRegisteredEvent();

	public GameObject m_DrawerParent;

	public GameObject m_HiddenCompartementParent;

	private InventoryItem[] m_DrawerItems;

	private InventoryItem[] m_HiddenItems;

	private List<Item> m_PreviousDrawerItems = new List<Item>();

	private List<Item> m_PreviousHiddenItems = new List<Item>();

	private ItemContainer m_DrawerContainer;

	private ItemContainer m_HiddenCompContainer;

	private BaseInventoryBehaviour m_DrawerInventoryBehaviour;

	private BaseInventoryBehaviour m_HiddenCompInventoryBehaviour;

	private T17_PassThroughNavigation[] m_HiddenPassThroughNavigations;

	private Character m_DeskOwner;

	private int m_CurrentSelectedObjectIndex = -1;

	private bool m_bSelectedObjectIsInHidden;

	public ContainerRegisteredEvent OnDeskContainerRegistered;

	public ContainerRegisteredEvent OnDeskContainerDeregistered;

	[Header("Inventory Item setup for when there is a hidden item compartment to add to. Default the other Container Setup as if there isn't one")]
	public InventoryItem.ContainerSetup m_MainCompartmentSetupWithHidden;

	public InventoryItem.ContainerSetup m_MainCompartmentSetupWithoutHidden;

	public InventoryItem.ContainerSetup m_PlayerInventorySetupWithHidden;

	public bool m_bSetTitleFromDeskOwner = true;

	public void SetDeskOwner(Character owner)
	{
		m_DeskOwner = owner;
		if (m_bSetTitleFromDeskOwner)
		{
			string localized = string.Empty;
			Localization.Get("Text.Name.Nobody", out localized);
			if (m_DeskOwner != null)
			{
				localized = m_DeskOwner.m_CharacterCustomisation.m_DisplayName;
			}
			string localised = string.Empty;
			Localization.GetWithKeySwap("Text.Interact.DeskName", out localised, "$CharacterName", localized);
			SetMenuName(localised, localize: false);
		}
		else
		{
			SetMenuName(MenuName);
		}
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (m_DrawerParent != null)
		{
			m_DrawerItems = m_DrawerParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_DrawerInventoryBehaviour = m_DrawerParent.GetComponent<BaseInventoryBehaviour>();
		}
		if (m_HiddenCompartementParent != null)
		{
			m_HiddenItems = m_HiddenCompartementParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
			m_HiddenCompInventoryBehaviour = m_HiddenCompartementParent.GetComponent<BaseInventoryBehaviour>();
			m_HiddenPassThroughNavigations = m_HiddenCompartementParent.GetComponentsInChildren<T17_PassThroughNavigation>(includeInactive: true);
		}
		ClearEntireDesk();
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		DoSingleTimeInitialize();
		if (m_HiddenCompartementParent != null)
		{
			m_HiddenCompartementParent.SetActive(value: false);
		}
		if (m_HiddenPassThroughNavigations != null)
		{
			for (int i = 0; i < m_HiddenPassThroughNavigations.Length; i++)
			{
				m_HiddenPassThroughNavigations[i].SetPassThrough();
			}
		}
		PopulateDrawerWithItemContainer(ref m_GameMenuInformation.m_MenuRepresentativeContainer, firstTimeInit: true);
		m_DrawerInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_Player);
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
		if (OnDeskContainerRegistered != null)
		{
			OnDeskContainerRegistered();
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_HiddenCompContainer != null)
		{
			m_HiddenCompContainer.LogItemInfomation();
		}
		m_DrawerContainer = null;
		m_HiddenCompContainer = null;
		ClearEntireDesk();
		DisableSmokesOnInventoryItems(m_DrawerItems);
		DisableSmokesOnInventoryItems(m_HiddenItems);
		if (m_HiddenCompartementParent != null)
		{
			m_HiddenCompartementParent.SetActive(value: false);
		}
		if (m_HiddenPassThroughNavigations != null)
		{
			for (int i = 0; i < m_HiddenPassThroughNavigations.Length; i++)
			{
				if (m_HiddenPassThroughNavigations[i] != null)
				{
					m_HiddenPassThroughNavigations[i].RestorePassThrough();
				}
			}
		}
		if (m_DrawerItems != null)
		{
			for (int j = 0; j < m_DrawerItems.Length; j++)
			{
				if (m_DrawerItems[j] != null)
				{
					InventoryItem obj = m_DrawerItems[j];
					obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
					InventoryItem obj2 = m_DrawerItems[j];
					obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
				}
			}
		}
		if (m_HiddenItems != null)
		{
			for (int k = 0; k < m_HiddenItems.Length; k++)
			{
				if (m_HiddenItems[k] != null)
				{
					InventoryItem obj3 = m_HiddenItems[k];
					obj3.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj3.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
					InventoryItem obj4 = m_HiddenItems[k];
					obj4.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj4.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
				}
			}
		}
		if (OnDeskContainerDeregistered != null)
		{
			OnDeskContainerDeregistered();
		}
		return true;
	}

	public void ClearEntireDesk()
	{
		ClearDrawer();
		ClearHidden();
	}

	public void ClearDrawer()
	{
		m_CurrentSelectedObjectIndex = -1;
		if (m_DrawerItems == null)
		{
			return;
		}
		for (int i = 0; i < m_DrawerItems.Length; i++)
		{
			if (m_DrawerItems[i] != null)
			{
				m_DrawerItems[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
				m_DrawerItems[i].SetIndexOfRepresentation(-1);
				m_DrawerItems[i].OnItemSelected = null;
				m_DrawerItems[i].OnItemDeselected = null;
				if (m_DrawerItems[i].InteractableElement != null)
				{
					m_DrawerItems[i].InteractableElement.onClick.RemoveAllListeners();
				}
			}
		}
	}

	public void ClearHidden()
	{
		if (m_HiddenItems == null)
		{
			return;
		}
		for (int i = 0; i < m_HiddenItems.Length; i++)
		{
			if (m_HiddenItems[i] != null)
			{
				m_HiddenItems[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
				m_HiddenItems[i].SetIndexOfRepresentation(-1);
				if (m_HiddenItems[i].InteractableElement != null)
				{
					m_HiddenItems[i].InteractableElement.onClick.RemoveAllListeners();
				}
			}
		}
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
		if (m_DrawerContainer != null)
		{
			PopulateDrawerWithItemContainer(ref m_DrawerContainer, firstTimeInit: false);
		}
		if (m_HiddenCompContainer != null)
		{
			PopulateHiddenCompartementWithItemContainer(ref m_HiddenCompContainer, firstTimeInit: false);
		}
	}

	public void PopulateDrawerWithItemContainer(ref ItemContainer container, bool firstTimeInit)
	{
		int currentSelectedObjectIndex = m_CurrentSelectedObjectIndex;
		ClearEntireDesk();
		if (currentSelectedObjectIndex != -1)
		{
			m_CurrentSelectedObjectIndex = currentSelectedObjectIndex;
		}
		m_DrawerContainer = container;
		if (container == null || m_DrawerItems == null)
		{
			if (container == null)
			{
				T17NetManager.LogGoogleException("DrawerInventory trying to populate with null container");
			}
			if (m_DrawerItems == null)
			{
				T17NetManager.LogGoogleException("DrawerInventory trying to populate with null inventoryItems");
			}
			return;
		}
		int num = m_DrawerItems.Length;
		for (int i = 0; i < num; i++)
		{
			if (m_DrawerItems[i] == null)
			{
				string text = "Drawer item " + i + " in desk menu for gamer";
				text = ((base.CurrentGamer != null) ? (text + " (primary? " + base.CurrentGamer.m_bPrimaryLocal + ") is null") : (text + " (that is null) is also null"));
				T17NetManager.LogGoogleException(text);
				continue;
			}
			m_DrawerItems[i].ResetBackgroundColor();
			m_DrawerItems[i].SetIndexOfRepresentation(i);
			InventoryItem obj = m_DrawerItems[i];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			InventoryItem obj2 = m_DrawerItems[i];
			obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemWasDeselected));
			Item item = container.GetItem(i);
			if (!(item != null))
			{
				continue;
			}
			if (item.m_ItemData == null)
			{
				T17NetManager.LogGoogleException("Desk menu trying to set up item " + item.name + " but it doesn't have any item data");
			}
			else
			{
				m_DrawerItems[i].SetItemContentImage(item.m_ItemData.m_ItemUIImage);
			}
			m_DrawerItems[i].SetItem(item);
			if (m_HiddenCompContainer != null)
			{
				m_DrawerItems[i].ChangeDisplaySetup(m_MainCompartmentSetupWithHidden);
			}
			else
			{
				m_DrawerItems[i].ChangeDisplaySetup(m_MainCompartmentSetupWithoutHidden);
			}
			int index = i;
			if (m_DrawerInventoryBehaviour != null && m_DrawerItems[i].InteractableElement != null)
			{
				m_DrawerItems[i].InteractableElement.onClick.RemoveAllListeners();
				m_DrawerItems[i].InteractableElement.onClick.AddListener(delegate
				{
					m_DrawerInventoryBehaviour.OnItemClicked(index);
				});
			}
			if (m_DrawerInventoryBehaviour == null)
			{
			}
			if (!(m_DrawerItems[i].InteractableElement == null))
			{
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_DrawerItems, m_PreviousDrawerItems);
		}
		container.GetItems(ref m_PreviousDrawerItems);
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentRewiredPlayer == null || !(m_HiddenCompContainer != null) || !base.CurrentRewiredPlayer.GetButtonDown("UI_PutIntoHidden"))
		{
			return;
		}
		int num = m_CurrentSelectedObjectIndex;
		int num2 = -1;
		if (base.CachedEventSystem != null && !T17RewiredStandaloneInputModule.IsControllerDrivingInput(base.CurrentGamer))
		{
			GameObject currentMouseOverGO = base.CachedEventSystem.GetCurrentPointerOverGameobject();
			num2 = m_DrawerItems.FindIndex((InventoryItem item) => item.gameObject == currentMouseOverGO);
			if (num2 == -1)
			{
				num2 = m_HiddenItems.FindIndex((InventoryItem item) => item.gameObject == currentMouseOverGO);
				if (num2 != -1)
				{
					m_bSelectedObjectIsInHidden = true;
				}
			}
			else
			{
				m_bSelectedObjectIsInHidden = false;
			}
			num = num2;
		}
		if (num == -1)
		{
			return;
		}
		if (!m_bSelectedObjectIsInHidden)
		{
			Item item2 = m_DrawerContainer.GetItem(num);
			if (item2 != null)
			{
				if (!item2.IsQuestItem() || (item2.IsQuestItem() && QuestManager.GetInstance().DoesPlayerOwnQuestItem(item2.m_NetView.viewID, m_DeskOwner.m_NetView.viewID)))
				{
					m_DrawerContainer.SwitchItemCompartmentToHiddenRPC(item2);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Give_Item, AudioController.UI_Audio_GO);
				}
				else
				{
					SpeechManager.GetInstance().SaySomething(m_DeskOwner, "Text.Dialog.NotOurQuestItem", SpeechTone.Negative);
				}
			}
		}
		else
		{
			Item hiddenItem = m_DrawerContainer.GetHiddenItem(num);
			if (hiddenItem != null)
			{
				m_DrawerContainer.SwitchItemCompartmentToMainRPC(hiddenItem);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Take_Item, AudioController.UI_Audio_GO);
			}
		}
	}

	private void InventoryItemWasSelected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		if (eventSystemForGamer != null && representationIndex >= 0 && representationIndex < m_DrawerItems.Length)
		{
			GameObject currentSelectedGameObject = eventSystemForGamer.currentSelectedGameObject;
			GameObject currentPointerOverGameobject = eventSystemForGamer.GetCurrentPointerOverGameobject();
			if (m_DrawerItems[representationIndex].gameObject == currentSelectedGameObject || m_DrawerItems[representationIndex].gameObject == currentPointerOverGameobject)
			{
				m_CurrentSelectedObjectIndex = representationIndex;
				m_bSelectedObjectIsInHidden = false;
			}
		}
	}

	private void InventoryItemWasDeselected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
	}

	private void HiddenInventoryItemWasSelected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
		T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		if (eventSystemForGamer != null && representationIndex >= 0 && representationIndex < m_HiddenItems.Length)
		{
			GameObject currentSelectedGameObject = eventSystemForGamer.currentSelectedGameObject;
			if (m_HiddenItems[representationIndex].gameObject == currentSelectedGameObject)
			{
				m_CurrentSelectedObjectIndex = representationIndex;
				m_bSelectedObjectIsInHidden = true;
			}
		}
	}

	private void HiddenInventoryItemWasDeselected(int representationIndex)
	{
		m_CurrentSelectedObjectIndex = -1;
	}

	public BaseInventoryBehaviour GetDrawerInventoryBehaviour()
	{
		return m_DrawerInventoryBehaviour;
	}

	public void PopulateHiddenCompartementWithItemContainer(ref ItemContainer container, bool firstTimeInit)
	{
		ClearHidden();
		if (m_HiddenCompartementParent != null)
		{
			m_HiddenCompartementParent.SetActive(value: true);
			m_GameMenuInformation.m_PlayerInventoryMenu.SetInventoryItemSetups(m_PlayerInventorySetupWithHidden);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Take_Item, AudioController.UI_Audio_GO);
		}
		if (m_HiddenPassThroughNavigations != null)
		{
			for (int i = 0; i < m_HiddenPassThroughNavigations.Length; i++)
			{
				m_HiddenPassThroughNavigations[i].RestorePassThrough();
			}
		}
		m_HiddenCompContainer = container;
		if (container == null || m_HiddenItems == null)
		{
			if (container == null)
			{
			}
			if (m_HiddenItems != null)
			{
			}
			return;
		}
		int hiddenItemCount = container.GetHiddenItemCount();
		hiddenItemCount = Mathf.Clamp(hiddenItemCount, 0, m_HiddenItems.Length);
		for (int j = 0; j < hiddenItemCount; j++)
		{
			Item hiddenItem = container.GetHiddenItem(j);
			if (!(hiddenItem != null))
			{
				continue;
			}
			m_HiddenItems[j].SetItemContentImage(hiddenItem.m_ItemData.m_ItemUIImage);
			m_HiddenItems[j].SetItem(hiddenItem);
			InventoryItem obj = m_HiddenItems[j];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(HiddenInventoryItemWasSelected));
			InventoryItem obj2 = m_HiddenItems[j];
			obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(HiddenInventoryItemWasDeselected));
			m_HiddenItems[j].SetIndexOfRepresentation(j);
			int index = j;
			if (m_HiddenCompInventoryBehaviour != null && m_HiddenItems[j].InteractableElement != null)
			{
				m_HiddenItems[j].InteractableElement.onClick.RemoveAllListeners();
				m_HiddenItems[j].InteractableElement.onClick.AddListener(delegate
				{
					m_HiddenCompInventoryBehaviour.OnHiddenItemClicked(index);
				});
			}
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_HiddenItems, m_PreviousHiddenItems);
		}
		m_HiddenCompContainer.GetHiddenItems(ref m_PreviousHiddenItems);
		hiddenItemCount = m_DrawerItems.Length;
		for (int k = 0; k < hiddenItemCount; k++)
		{
			m_DrawerItems[k].ChangeDisplaySetup(m_MainCompartmentSetupWithHidden);
		}
	}

	public BaseInventoryBehaviour GetHiddenCompInventoryBehaviour()
	{
		return m_HiddenCompInventoryBehaviour;
	}

	public void OnItemRemovedEvent(ItemContainer container, Item item)
	{
		bool flag = false;
		DeskInteraction myDesk = m_GameMenuInformation.m_Player.GetMyDesk();
		if (myDesk != null)
		{
			flag = myDesk.m_LinkedItemContainer == container;
		}
		if (!flag && !container.m_bCanLoot)
		{
			SetPlayerLooting();
		}
	}

	private void SetPlayerLooting()
	{
		if (m_GameMenuInformation.m_Player != null)
		{
			CharacterNetEvents.SendSetLootingEvent(m_GameMenuInformation.m_Player);
		}
	}

	public InventoryItem FindItemInDesk(ItemData item)
	{
		for (int i = 0; i < m_DrawerItems.Length; i++)
		{
			if (m_DrawerItems[i] != null && m_DrawerItems[i].GetLinkedItem() != null && m_DrawerItems[i].GetLinkedItem().ItemDataID == item.m_ItemDataID)
			{
				return m_DrawerItems[i];
			}
		}
		for (int j = 0; j < m_HiddenItems.Length; j++)
		{
			if (m_HiddenItems[j] != null && m_HiddenItems[j].GetLinkedItem() != null && m_HiddenItems[j].GetLinkedItem().ItemDataID == item.m_ItemDataID)
			{
				return m_HiddenItems[j];
			}
		}
		return null;
	}

	public ItemContainer GetDrawerContainer()
	{
		return m_DrawerContainer;
	}

	public ItemContainer GetHiddenCompartmentContainer()
	{
		return m_HiddenCompContainer;
	}
}
