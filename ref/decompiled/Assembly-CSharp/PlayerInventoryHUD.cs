using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class PlayerInventoryHUD : BaseMenuBehaviour
{
	public InventoryItem m_CurrentEquippedItem;

	public GameObject m_ExpandParent;

	public string m_CycleLeft = "HUD_CycleLeft";

	public string m_CycleRight = "HUD_CycleRight";

	public string m_DropItem = "HUD_Drop";

	public string m_HotKeySlot1 = "HUD_HotKeySlot1";

	public string m_HotKeySlot2 = "HUD_HotKeySlot2";

	public string m_HotKeySlot3 = "HUD_HotKeySlot3";

	public string m_HotKeySlot4 = "HUD_HotKeySlot4";

	public string m_HotKeySlot5 = "HUD_HotKeySlot5";

	public string m_InventoryToggle = "HUD_InventoryToggle";

	public float m_ScrollTime = 0.2f;

	private float m_ElapsedScrollTime;

	public readonly float m_AutoHideTime = 2f;

	private InventoryItem[] m_ExpandItemList;

	private ItemContainer m_CurrentContainer;

	private int m_CurrentSelectedObjectIndex = -1;

	private List<Item> m_PreviousItems = new List<Item>();

	private float m_ElapsedHideTime;

	private bool m_bIsExpandVisible;

	private bool m_bHasPlayerInteractedWithExpand;

	private bool m_bUsedButtonsToClose;

	private bool m_bIsAlwaysVisible;

	private bool m_bLastSelectionWasWithMouse;

	private bool m_bHasMouseOver;

	public int CurrentSelectedObjectIndex => m_CurrentSelectedObjectIndex;

	public bool HasMouseOver => m_bHasMouseOver;

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		if (!(m_ExpandParent != null))
		{
			return;
		}
		m_ExpandItemList = m_ExpandParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
		for (int num = m_ExpandItemList.Length - 1; num >= 0; num--)
		{
			T17Button interactableElement = m_ExpandItemList[num].InteractableElement;
			if (interactableElement != null)
			{
				T17Button interactableElement2 = m_ExpandItemList[num].InteractableElement;
				interactableElement2.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement2.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(OnPointerEnter));
				T17Button interactableElement3 = m_ExpandItemList[num].InteractableElement;
				interactableElement3.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement3.OnButtonPointerExit, new T17Button.T17ButtonDelegate(OnPointerExit));
				m_ExpandItemList[num].InteractableElement.m_CanUIReselectDelegate = () => false;
			}
		}
		if (m_CurrentEquippedItem != null)
		{
			T17Button interactableElement4 = m_CurrentEquippedItem.InteractableElement;
			interactableElement4.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement4.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(OnPointerEnter));
			T17Button interactableElement5 = m_CurrentEquippedItem.InteractableElement;
			interactableElement5.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement5.OnButtonPointerExit, new T17Button.T17ButtonDelegate(OnPointerExit));
			m_CurrentEquippedItem.InteractableElement.m_CanUIReselectDelegate = () => false;
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (!m_bIsAlwaysVisible)
		{
			HideInventory();
		}
		return true;
	}

	public override void SetGamePlayer(Player gamePlayer)
	{
		base.SetGamePlayer(gamePlayer);
		if (base.CurrentGamePlayer != null)
		{
			PopulateWithItemContainer(firstTimeInit: true);
		}
	}

	public void FlashInventoryWithoutFocus()
	{
		if (m_ExpandParent != null && !m_ExpandParent.activeSelf)
		{
			ShowExpandParent(show: true);
		}
		else if (!m_bIsExpandVisible)
		{
			PopulateWithItemContainer(firstTimeInit: true);
		}
		m_ElapsedHideTime = 0f;
	}

	public void ShowInventory(bool selectCurrent = true)
	{
		if (m_ExpandParent != null)
		{
			ShowExpandParent(show: true);
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Hud_In, AudioController.UI_Audio_GO);
		if (base.CurrentGamer != null)
		{
			T17EventSystem cachedEventSystem = base.CachedEventSystem;
			if (cachedEventSystem != null && m_CurrentEquippedItem != null && selectCurrent)
			{
				cachedEventSystem.SetSelectedGameObject(null, forceSet: true);
				cachedEventSystem.SetSelectedGameObject(m_CurrentEquippedItem.gameObject, forceSet: true);
			}
		}
	}

	public void HideInventory()
	{
		if (!m_bUsedButtonsToClose)
		{
			if (m_ExpandParent != null)
			{
				ShowExpandParent(show: false);
				m_bHasPlayerInteractedWithExpand = false;
				m_bIsAlwaysVisible = false;
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Hud_Out, AudioController.UI_Audio_GO);
		}
		DisableSmokesOnInventoryItems(m_ExpandItemList);
		if (m_CurrentContainer != null)
		{
			m_CurrentContainer.GetItems(ref m_PreviousItems);
		}
	}

	private void ShowExpandParent(bool show)
	{
		m_ExpandParent.SetActive(show);
		m_bIsExpandVisible = show;
	}

	public void UpdateExpandParentVisibility()
	{
	}

	public bool HasItemInCurrentSlot()
	{
		if (null != m_CurrentContainer && m_CurrentSelectedObjectIndex != -1)
		{
			return m_CurrentContainer.GetItem(m_CurrentSelectedObjectIndex) != null;
		}
		if (null != base.CurrentGamePlayer)
		{
			return base.CurrentGamePlayer.GetEquippedItem() != null;
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentRewiredPlayer == null || base.CurrentGamer == null || T17DialogBoxManager.HasDialogsForGamer(base.CurrentGamer) || base.CurrentGamer.m_PlayerObject.IsBrowsingPauseMenu)
		{
			return;
		}
		if (base.CurrentRewiredPlayer.GetButtonUp(m_InventoryToggle))
		{
			m_bIsAlwaysVisible = !m_bIsAlwaysVisible;
			if (m_bIsAlwaysVisible && !m_bIsExpandVisible)
			{
				ShowInventory(selectCurrent: false);
			}
			bool flag = !m_bIsAlwaysVisible && m_bIsExpandVisible;
			if (flag & !m_bHasMouseOver)
			{
				HideInventory();
			}
		}
		if (m_bUsedButtonsToClose && !base.CurrentRewiredPlayer.GetButton("Attack") && !base.CurrentRewiredPlayer.GetButton("Use") && !base.CurrentRewiredPlayer.GetButton("PrimaryInteraction") && !m_bHasMouseOver)
		{
			m_bUsedButtonsToClose = false;
			if (!m_bIsAlwaysVisible)
			{
				HideInventory();
			}
		}
		T17EventSystem cachedEventSystem = base.CachedEventSystem;
		if (cachedEventSystem != null && m_ExpandItemList != null)
		{
			bool flag2 = false;
			flag2 = T17RewiredStandaloneInputModule.UsedSharedKeyboardAction("Use", "UI_Submit", base.CurrentRewiredPlayer);
			if (m_bIsExpandVisible && (base.CurrentRewiredPlayer.GetButtonDown("Attack") || (base.CurrentRewiredPlayer.GetButtonDown("Use") && !flag2)))
			{
				OnItemClicked(m_CurrentSelectedObjectIndex);
				return;
			}
			int newIndex = m_CurrentSelectedObjectIndex;
			int num = 0;
			if (base.CurrentRewiredPlayer.GetButtonDown(m_CycleLeft))
			{
				m_ElapsedScrollTime = 0f;
				Cycle(ref newIndex, isLeft: true);
				m_bLastSelectionWasWithMouse = false;
			}
			else if (base.CurrentRewiredPlayer.GetButton(m_CycleLeft))
			{
				num++;
				m_ElapsedScrollTime += UpdateManager.deltaTime;
				if (m_ElapsedScrollTime >= m_ScrollTime)
				{
					m_ElapsedScrollTime = 0f;
					Cycle(ref newIndex, isLeft: true);
					m_bLastSelectionWasWithMouse = false;
				}
			}
			if (base.CurrentRewiredPlayer.GetButtonDown(m_CycleRight))
			{
				m_ElapsedScrollTime = 0f;
				Cycle(ref newIndex, isLeft: false);
				m_bLastSelectionWasWithMouse = false;
			}
			else if (base.CurrentRewiredPlayer.GetButton(m_CycleRight))
			{
				num++;
				m_ElapsedScrollTime += UpdateManager.deltaTime;
				if (m_ElapsedScrollTime >= m_ScrollTime)
				{
					m_ElapsedScrollTime = 0f;
					Cycle(ref newIndex, isLeft: false);
					m_bLastSelectionWasWithMouse = false;
				}
			}
			if (num >= 2)
			{
				newIndex = m_CurrentSelectedObjectIndex;
			}
			if (!T17RewiredStandaloneInputModule.IsControllerDrivingInput(base.CurrentGamer) && Cursor.visible)
			{
				int listIndex = 0;
				m_bHasMouseOver = GetHasMouseOver(ref listIndex);
				if (m_bHasMouseOver && m_CurrentSelectedObjectIndex != listIndex)
				{
					newIndex = (m_CurrentSelectedObjectIndex = listIndex);
					if (HasSelectedGameObject())
					{
						cachedEventSystem.SetSelectedGameObject(null, forceSet: true);
					}
					m_bLastSelectionWasWithMouse = true;
					m_ElapsedHideTime = 0f;
				}
				else if (!m_bHasMouseOver && m_bLastSelectionWasWithMouse)
				{
					newIndex = (m_CurrentSelectedObjectIndex = -1);
					if (HasSelectedGameObject())
					{
						cachedEventSystem.SetSelectedGameObject(null, forceSet: true);
					}
					m_bLastSelectionWasWithMouse = false;
				}
			}
			else
			{
				m_bHasMouseOver = false;
				if (m_bLastSelectionWasWithMouse)
				{
					newIndex = (m_CurrentSelectedObjectIndex = -1);
					if (HasSelectedGameObject())
					{
						cachedEventSystem.SetSelectedGameObject(null);
					}
					m_bLastSelectionWasWithMouse = false;
				}
			}
			int num2 = newIndex;
			if (base.CurrentRewiredPlayer.GetButtonDown(m_HotKeySlot1))
			{
				newIndex = 0;
			}
			else if (base.CurrentRewiredPlayer.GetButtonDown(m_HotKeySlot2))
			{
				newIndex = 1;
			}
			else if (base.CurrentRewiredPlayer.GetButtonDown(m_HotKeySlot3))
			{
				newIndex = 2;
			}
			else if (base.CurrentRewiredPlayer.GetButtonDown(m_HotKeySlot4))
			{
				newIndex = 3;
			}
			else if (base.CurrentRewiredPlayer.GetButtonDown(m_HotKeySlot5))
			{
				newIndex = 4;
			}
			if (newIndex != m_CurrentSelectedObjectIndex)
			{
				cachedEventSystem.SetSelectedGameObject(null);
				if (newIndex == -1)
				{
					cachedEventSystem.SetSelectedGameObject(m_CurrentEquippedItem.gameObject, !m_bHasMouseOver);
				}
				else
				{
					cachedEventSystem.SetSelectedGameObject(m_ExpandItemList[newIndex].gameObject, !m_bHasMouseOver);
				}
			}
			if (num2 != newIndex)
			{
				OnItemClicked(newIndex);
				m_bLastSelectionWasWithMouse = false;
				m_ElapsedHideTime = 0f;
				m_bUsedButtonsToClose = false;
				m_bHasPlayerInteractedWithExpand = true;
				ShowInventory();
			}
			if (!m_bIsExpandVisible && m_bHasMouseOver)
			{
				m_bHasPlayerInteractedWithExpand = false;
				m_ElapsedHideTime = 0f;
				ShowInventory(selectCurrent: false);
			}
		}
		if (cachedEventSystem != null && m_CurrentEquippedItem != null && !IsExpandedHudVisible() && m_CurrentEquippedItem.gameObject == cachedEventSystem.currentSelectedGameObject)
		{
			OnItemClicked(m_CurrentSelectedObjectIndex, bForceMouse: true);
			if (!m_bIsAlwaysVisible)
			{
				HideInventory();
			}
		}
		if (!(m_ElapsedHideTime < m_AutoHideTime))
		{
			return;
		}
		m_ElapsedHideTime += UpdateManager.deltaTime;
		if (!(m_ElapsedHideTime >= m_AutoHideTime))
		{
			return;
		}
		if (!m_bHasMouseOver)
		{
			OnItemClicked(m_CurrentSelectedObjectIndex, bForceMouse: true);
			if (!m_bIsAlwaysVisible)
			{
				HideInventory();
			}
		}
		else
		{
			m_ElapsedHideTime = 0f;
		}
	}

	private void Cycle(ref int newIndex, bool isLeft)
	{
		m_bHasPlayerInteractedWithExpand = true;
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Tab, AudioController.UI_Audio_GO);
		m_ElapsedHideTime = 0f;
		if (!m_bIsExpandVisible)
		{
			ShowInventory();
		}
		else if (isLeft)
		{
			if (newIndex <= -1)
			{
				newIndex = m_ExpandItemList.Length - 1;
			}
			else
			{
				newIndex--;
			}
		}
		else if (newIndex >= m_ExpandItemList.Length - 1)
		{
			newIndex = -1;
		}
		else
		{
			newIndex++;
		}
	}

	private void OnItemClicked(int index, bool bForceMouse = false)
	{
		if (!bForceMouse && !m_bHasMouseOver && base.CurrentRewiredPlayer != null && base.CurrentRewiredPlayer.controllers != null && base.CurrentRewiredPlayer.controllers.GetLastActiveController() != null && base.CurrentRewiredPlayer.controllers.GetLastActiveController().type == ControllerType.Mouse)
		{
			return;
		}
		m_ElapsedScrollTime = 0f;
		m_bUsedButtonsToClose = true;
		if (index != -1 && !base.CurrentGamePlayer.GetIsKnockedOut())
		{
			Item item = base.CurrentGamePlayer.m_ItemContainer.GetItem(index);
			if (base.CurrentGamePlayer.CanEquipItem(item))
			{
				if (item != null && item.OutfitData != null)
				{
					base.CurrentGamePlayer.SetOutFit(item);
				}
				else
				{
					base.CurrentGamePlayer.SetEquippedItem(item);
					if (item != null)
					{
						m_CurrentEquippedItem.StartSmoke();
					}
				}
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Select, AudioController.UI_Audio_GO);
			}
		}
		if (base.CurrentGamer != null)
		{
			T17EventSystem cachedEventSystem = base.CachedEventSystem;
			if (cachedEventSystem != null && m_CurrentEquippedItem != null && HasSelectedGameObject())
			{
				cachedEventSystem.SetSelectedGameObject(null, bForceMouse);
			}
		}
		RefreshAllSlotsWithCurrentContainer();
		if (!m_bIsAlwaysVisible)
		{
			HideInventory();
		}
		else
		{
			m_bHasPlayerInteractedWithExpand = false;
		}
		if (base.CurrentGamePlayer != null)
		{
			m_ElapsedHideTime = m_AutoHideTime;
		}
	}

	private void InventoryItemSelected(int indexInList)
	{
		m_CurrentSelectedObjectIndex = -1;
		T17EventSystem cachedEventSystem = base.CachedEventSystem;
		if (!(cachedEventSystem != null))
		{
			return;
		}
		if (indexInList >= 0 && indexInList < m_ExpandItemList.Length)
		{
			GameObject currentSelectedGameObject = cachedEventSystem.currentSelectedGameObject;
			if (m_ExpandItemList[indexInList].gameObject == currentSelectedGameObject)
			{
				m_CurrentSelectedObjectIndex = indexInList;
			}
		}
		else
		{
			m_CurrentSelectedObjectIndex = indexInList;
		}
	}

	private void InventoryItemDeselected(int indexInList)
	{
		m_CurrentSelectedObjectIndex = -1;
	}

	public void ResetCycleAndHideTimers()
	{
		m_ElapsedHideTime = 0f;
		m_ElapsedScrollTime = 0f;
	}

	public void PopulateWithItemContainer(bool firstTimeInit)
	{
		if (base.CurrentGamePlayer == null)
		{
			return;
		}
		ClearInventoryData();
		m_CurrentContainer = base.CurrentGamePlayer.m_ItemContainer;
		if (m_CurrentContainer == null || m_ExpandItemList == null)
		{
			if (m_CurrentContainer == null)
			{
			}
			if (m_ExpandItemList != null)
			{
			}
			return;
		}
		int itemCount = m_CurrentContainer.GetItemCount();
		itemCount = Mathf.Clamp(itemCount, 0, m_ExpandItemList.Length);
		ItemContainer currentContainer = m_CurrentContainer;
		currentContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(currentContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		ItemContainer currentContainer2 = m_CurrentContainer;
		currentContainer2.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(currentContainer2.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		for (int i = 0; i < m_ExpandItemList.Length; i++)
		{
			if (base.CachedEventSystem != null && base.CachedEventSystem.currentSelectedGameObject == m_ExpandItemList[i].gameObject)
			{
				m_CurrentSelectedObjectIndex = i;
			}
			if (i < itemCount)
			{
				Item item = m_CurrentContainer.GetItem(i);
				if (item != null && item.m_ItemData != null)
				{
					m_ExpandItemList[i].ResetBackgroundColor();
					m_ExpandItemList[i].SetItemContentImage(item.m_ItemData.m_ItemUIImage);
					m_ExpandItemList[i].SetItem(item);
				}
			}
			else
			{
				m_ExpandItemList[i].StopSmoke();
			}
			int index = i;
			if (m_ExpandItemList[i].InteractableElement != null)
			{
				m_ExpandItemList[i].InteractableElement.onClick.RemoveAllListeners();
				m_ExpandItemList[i].InteractableElement.onClick.AddListener(delegate
				{
					OnItemClicked(index);
				});
			}
			m_ExpandItemList[i].SetIndexOfRepresentation(i);
			InventoryItem obj = m_ExpandItemList[i];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemSelected));
			InventoryItem obj2 = m_ExpandItemList[i];
			obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemDeselected));
		}
		Item equippedItem = base.CurrentGamePlayer.GetEquippedItem();
		if (null != equippedItem && null != m_CurrentEquippedItem && equippedItem.m_ItemData != null)
		{
			m_CurrentEquippedItem.SetItemContentImage(equippedItem.m_ItemData.m_ItemUIImage);
			m_CurrentEquippedItem.SetItem(null);
			m_CurrentEquippedItem.SetItem(equippedItem);
			m_CurrentEquippedItem.SetIndexOfRepresentation(-1);
			InventoryItem currentEquippedItem = m_CurrentEquippedItem;
			currentEquippedItem.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(currentEquippedItem.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemSelected));
			InventoryItem currentEquippedItem2 = m_CurrentEquippedItem;
			currentEquippedItem2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(currentEquippedItem2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemDeselected));
			if (m_CurrentEquippedItem.InteractableElement != null)
			{
				m_CurrentEquippedItem.InteractableElement.onClick.RemoveAllListeners();
				m_CurrentEquippedItem.InteractableElement.onClick.AddListener(delegate
				{
					OnItemClicked(-1);
				});
			}
		}
		if (equippedItem == null)
		{
			m_CurrentEquippedItem.StopSmoke();
		}
		if (!firstTimeInit)
		{
			InventoryItem.RunSmokesOnNewItems(m_ExpandItemList, m_PreviousItems);
		}
		m_CurrentContainer.GetItems(ref m_PreviousItems);
	}

	public void RefreshCurrentEquippedItem()
	{
		if (!(m_CurrentEquippedItem != null) || !(base.CurrentGamePlayer != null))
		{
			return;
		}
		Item equippedItem = base.CurrentGamePlayer.GetEquippedItem();
		if (equippedItem == null)
		{
			m_CurrentEquippedItem.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
			return;
		}
		if (equippedItem.m_ItemData == null)
		{
			T17NetManager.LogGoogleException("Player Inventory Hud equipped item " + equippedItem.transform.name + " does not have any item data");
		}
		else
		{
			m_CurrentEquippedItem.SetItemContentImage(equippedItem.m_ItemData.m_ItemUIImage);
		}
		m_CurrentEquippedItem.SetItem(equippedItem);
	}

	public void RefreshAllSlotsWithCurrentContainer()
	{
		if (m_CurrentContainer != null)
		{
			PopulateWithItemContainer(firstTimeInit: false);
		}
	}

	public void ClearInventoryData()
	{
		m_CurrentSelectedObjectIndex = -1;
		for (int i = 0; i < m_ExpandItemList.Length; i++)
		{
			m_ExpandItemList[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
			m_ExpandItemList[i].OnItemSelected = null;
			m_ExpandItemList[i].OnItemDeselected = null;
			if (m_ExpandItemList[i].InteractableElement != null)
			{
				m_ExpandItemList[i].InteractableElement.onClick.RemoveAllListeners();
			}
		}
		if (null != m_CurrentEquippedItem && base.CurrentGamePlayer.GetEquippedItem() == null)
		{
			m_CurrentEquippedItem.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
			m_CurrentEquippedItem.OnItemSelected = null;
			m_CurrentEquippedItem.OnItemDeselected = null;
			if (m_CurrentEquippedItem.InteractableElement != null)
			{
				m_CurrentEquippedItem.InteractableElement.onClick.RemoveAllListeners();
			}
		}
	}

	public bool IsExpandedHudVisible()
	{
		return m_bIsExpandVisible;
	}

	public bool IsPlayerInteractingWithExpandedHud()
	{
		return m_bIsExpandVisible && m_bHasPlayerInteractedWithExpand;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_CurrentContainer != null)
		{
			ItemContainer currentContainer = m_CurrentContainer;
			currentContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(currentContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(RefreshAllSlotsWithCurrentContainer));
		}
		if (m_CurrentEquippedItem != null)
		{
			InventoryItem currentEquippedItem = m_CurrentEquippedItem;
			currentEquippedItem.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(currentEquippedItem.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemSelected));
			InventoryItem currentEquippedItem2 = m_CurrentEquippedItem;
			currentEquippedItem2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Remove(currentEquippedItem2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemDeselected));
		}
		for (int i = 0; i < m_ExpandItemList.Length; i++)
		{
			if (m_ExpandItemList[i] != null)
			{
				InventoryItem obj = m_ExpandItemList[i];
				obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemSelected));
				InventoryItem obj2 = m_ExpandItemList[i];
				obj2.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Remove(obj2.OnItemDeselected, new InventoryItem.InventoryItemEvent(InventoryItemDeselected));
			}
		}
	}

	public InventoryItem GetInventoryItemForItem(ItemData item)
	{
		if (item != null)
		{
			for (int i = 0; i < m_ExpandItemList.Length; i++)
			{
				if (m_ExpandItemList[i].LinkedItemDataID == item.m_ItemDataID)
				{
					return m_ExpandItemList[i];
				}
			}
		}
		return null;
	}

	private bool GetHasMouseOver(ref int listIndex)
	{
		if (base.CachedEventSystem != null)
		{
			GameObject currentPointerOverGameobject = base.CachedEventSystem.GetCurrentPointerOverGameobject();
			if (currentPointerOverGameobject == null)
			{
				listIndex = -1;
				return false;
			}
			if (m_CurrentEquippedItem.gameObject == currentPointerOverGameobject)
			{
				listIndex = -1;
				return true;
			}
			for (int num = m_ExpandItemList.Length - 1; num >= 0; num--)
			{
				if (m_ExpandItemList[num].gameObject == currentPointerOverGameobject)
				{
					listIndex = num;
					return true;
				}
			}
		}
		listIndex = -1;
		return false;
	}

	private bool HasSelectedGameObject()
	{
		if (base.CachedEventSystem != null)
		{
			GameObject currentSelectedGameObject = base.CachedEventSystem.currentSelectedGameObject;
			if (currentSelectedGameObject == null)
			{
				return false;
			}
			if (m_CurrentEquippedItem.gameObject == currentSelectedGameObject)
			{
				return true;
			}
			for (int num = m_ExpandItemList.Length - 1; num >= 0; num--)
			{
				if (m_ExpandItemList[num].gameObject == currentSelectedGameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnPointerEnter(T17Button sender)
	{
		HUDMenuFlow.Instance.AddMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
	}

	public void OnPointerExit(T17Button sender)
	{
		HUDMenuFlow.Instance.RemoveMouseHUDItem(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID, base.gameObject);
	}
}
