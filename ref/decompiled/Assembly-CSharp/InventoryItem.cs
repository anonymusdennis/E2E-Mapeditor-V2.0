using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IT17EventHelper
{
	[Serializable]
	public class ContainerSetup
	{
		public T17ItemTooltip.DisplaySetups m_DefaultContainerSetup = T17ItemTooltip.DisplaySetups.PrimaryOnly;

		public string m_PrimaryInputText = "Text.Interaction.Take";

		public string m_SecondaryInputText = "Text.Interaction.Use";

		public string m_CancelInputText = "Text.Interaction.Drop";

		public void CopyTo(ContainerSetup destination)
		{
			destination.m_DefaultContainerSetup = m_DefaultContainerSetup;
			destination.m_PrimaryInputText = m_PrimaryInputText;
			destination.m_SecondaryInputText = m_SecondaryInputText;
			destination.m_CancelInputText = m_CancelInputText;
		}
	}

	public delegate void InventoryItemEvent(int index);

	public class TooltipInfo
	{
		public string Name = string.Empty;

		public string TypeDef = string.Empty;
	}

	public T17Image m_ContentObject;

	public T17Text m_DurabilityText;

	public bool m_bNeedsTooltip;

	public Animator m_SmokeAnimator;

	public InventoryItemEvent OnItemSelected;

	public InventoryItemEvent OnItemDeselected;

	[Header("Button Sprite Variations")]
	public Sprite[] m_ButtonSpriteVariations = new Sprite[0];

	private const int BUTTON_VARIATION_DEFAULT = 0;

	private const int BUTTON_VARIATION_QUEST_NOTMINE = 1;

	private const int BUTTON_VARIATION_QUEST = 2;

	[HideInInspector]
	private T17Button m_InteractableElement;

	private T17Image m_BackgroundImage;

	private Color32 m_OriginalBackgroundColor;

	private int m_IndexOfRepresentation = -1;

	private bool m_bFadeInProgress;

	[Header("Tooltip Settings")]
	public ContainerSetup m_TooltipSetup;

	private Item m_LinkedToItem;

	private int m_LinkedItemDataID = -1;

	public T17Text m_SlotNumberLabel;

	private Gamer AttachedGamer;

	private T17EventSystem m_CachedEventSystem;

	private static string OUTFIT_Localized = string.Empty;

	private static string ARMOUR_Localized = string.Empty;

	private static string WEAPON_Localized = string.Empty;

	private static string TOOL_Localized = string.Empty;

	private static string COMPONENT_Localized = string.Empty;

	private static string CONSUMABLE_Localized = string.Empty;

	private static string ADDENERGY_Localized = string.Empty;

	private static string ADDHEALTH_Localized = string.Empty;

	[HideInInspector]
	public TooltipInfo m_TooltipInfo = new TooltipInfo();

	private T17ItemTooltip m_MyTooltip;

	public T17Button InteractableElement
	{
		get
		{
			CheckForInteractableElement();
			return m_InteractableElement;
		}
	}

	public int LinkedItemDataID
	{
		get
		{
			if (m_LinkedToItem != null)
			{
				return m_LinkedToItem.ItemDataID;
			}
			return m_LinkedItemDataID;
		}
		set
		{
			m_LinkedItemDataID = value;
		}
	}

	private void Awake()
	{
		CheckForInteractableElement();
		if (m_bNeedsTooltip && m_InteractableElement != null)
		{
			T17Button interactableElement = m_InteractableElement;
			interactableElement.OnButtonSelect = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement.OnButtonSelect, new T17Button.T17ButtonDelegate(DisplayToolTip));
			T17Button interactableElement2 = m_InteractableElement;
			interactableElement2.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement2.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(DisplayToolTip));
			T17Button interactableElement3 = m_InteractableElement;
			interactableElement3.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement3.OnButtonPointerExit, new T17Button.T17ButtonDelegate(HideToolTip));
		}
		if (m_InteractableElement != null)
		{
			T17Button interactableElement4 = m_InteractableElement;
			interactableElement4.OnButtonSelect = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement4.OnButtonSelect, new T17Button.T17ButtonDelegate(ItemSelected));
			T17Button interactableElement5 = m_InteractableElement;
			interactableElement5.OnButtonDeselect = (T17Button.T17ButtonDeselectDelegate)Delegate.Combine(interactableElement5.OnButtonDeselect, new T17Button.T17ButtonDeselectDelegate(ButtonDeselected));
		}
		m_BackgroundImage = GetComponent<T17Image>();
		if (m_BackgroundImage != null)
		{
			m_OriginalBackgroundColor = m_BackgroundImage.color;
		}
		if (string.IsNullOrEmpty(OUTFIT_Localized))
		{
			Localization.Get("Text.ItemTooltip.Outfit", out OUTFIT_Localized);
		}
		if (string.IsNullOrEmpty(ARMOUR_Localized))
		{
			Localization.Get("Text.ItemTooltip.Armour", out ARMOUR_Localized);
		}
		if (string.IsNullOrEmpty(WEAPON_Localized))
		{
			Localization.Get("Text.ItemTooltip.Weapon", out WEAPON_Localized);
		}
		if (string.IsNullOrEmpty(TOOL_Localized))
		{
			Localization.Get("Text.ItemTooltip.Tool", out TOOL_Localized);
		}
		if (string.IsNullOrEmpty(COMPONENT_Localized))
		{
			Localization.Get("Text.ItemTooltip.Component", out COMPONENT_Localized);
		}
		if (string.IsNullOrEmpty(CONSUMABLE_Localized))
		{
			Localization.Get("Text.ItemTooltip.Consumable", out CONSUMABLE_Localized);
		}
		if (string.IsNullOrEmpty(ADDENERGY_Localized))
		{
			Localization.Get("Text.ItemTooltip.Con_AddEnergy", out ADDENERGY_Localized);
		}
		if (string.IsNullOrEmpty(ADDHEALTH_Localized))
		{
			Localization.Get("Text.ItemTooltip.Con_AddHealth", out ADDHEALTH_Localized);
		}
		if (m_SmokeAnimator == null)
		{
			m_SmokeAnimator = GetComponentInChildren<Animator>(includeInactive: true);
		}
		if (m_SmokeAnimator != null)
		{
			ItemSmokeAnimListener.SetupListenerToObject(m_SmokeAnimator.gameObject, this);
		}
		bool flag = false;
		flag = true;
		if (m_SlotNumberLabel != null)
		{
			m_SlotNumberLabel.gameObject.SetActive(flag);
		}
	}

	private void Start()
	{
		DisableDurability();
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(ResetTooltip));
		}
	}

	protected virtual void OnDestroy()
	{
		HideToolTip(null);
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(ResetTooltip));
		}
	}

	private void Update()
	{
		if (m_LinkedToItem != null && m_LinkedToItem.m_ItemData != null && m_LinkedToItem.m_ItemData.IsTool())
		{
			SetDurability(m_LinkedToItem.Health);
		}
	}

	public void SetItemContentImage(Sprite image, bool autoClearIfNull = true, bool autoDisableIfNull = true, bool faded = false, bool hidden = false, bool resetVariation = true)
	{
		ResetBackgroundColor();
		if (resetVariation)
		{
			SetButtonVariation(0);
		}
		m_LinkedItemDataID = -1;
		if (!(m_ContentObject != null))
		{
			return;
		}
		if (image == null)
		{
			SetItem(null);
			if (autoClearIfNull)
			{
				ClearItemContentImage();
			}
			if (autoDisableIfNull)
			{
				DisableItem();
			}
			return;
		}
		m_ContentObject.sprite = image;
		if (hidden)
		{
			Color color = m_ContentObject.color;
			color.r = 0f;
			color.g = 0f;
			color.b = 0f;
			color.a = 0.25f;
			m_ContentObject.color = color;
		}
		else if (faded)
		{
			Color color2 = m_ContentObject.color;
			color2.r = 1f;
			color2.g = 1f;
			color2.b = 1f;
			color2.a = 0.25f;
			m_ContentObject.color = color2;
		}
		else
		{
			Color color3 = m_ContentObject.color;
			color3.r = 1f;
			color3.g = 1f;
			color3.b = 1f;
			color3.a = 1f;
			m_ContentObject.color = color3;
		}
		if (!m_bFadeInProgress)
		{
			m_ContentObject.gameObject.SetActive(value: true);
		}
		EnableItem();
	}

	public virtual void SetItem(Item item)
	{
		bool flag = false;
		if (m_LinkedToItem != null)
		{
			m_LinkedToItem.OnQuestItemStatusChanged -= RefreshItem;
		}
		if (m_LinkedToItem != item)
		{
			flag = true;
		}
		m_LinkedToItem = item;
		m_LinkedItemDataID = -1;
		if (m_LinkedToItem != null)
		{
			m_LinkedToItem.OnQuestItemStatusChanged += RefreshItem;
		}
		if (!(item != null) || item.m_ItemData == null)
		{
		}
		if (!flag)
		{
			return;
		}
		DisableDurability();
		if (m_LinkedToItem == null)
		{
			m_TooltipInfo.Name = string.Empty;
			m_TooltipInfo.TypeDef = string.Empty;
			HideToolTip(null);
			return;
		}
		if (item.IsQuestItem())
		{
			int playerViewID = ((AttachedGamer == null || !(AttachedGamer.m_PlayerObject != null)) ? (-1) : AttachedGamer.m_PlayerObject.m_NetView.viewID);
			QuestManager instance = QuestManager.GetInstance();
			if (instance != null && instance.DoesPlayerOwnQuestItem(item.m_NetView.viewID, playerViewID))
			{
				if (item.m_QuestItemGroupID >= 0)
				{
					SetButtonVariation(2 + item.m_QuestItemGroupID);
				}
				else
				{
					SetButtonVariation(2);
				}
			}
			else
			{
				SetButtonVariation(1);
			}
		}
		else
		{
			SetButtonVariation(0);
		}
		m_TooltipInfo.Name = item.ItemName;
		string empty = string.Empty;
		empty += ((!item.m_ItemData.IsWeapon()) ? string.Empty : (", " + WEAPON_Localized));
		empty += ((!item.m_ItemData.IsOutfit()) ? string.Empty : (", " + ((!(item.m_ItemData.m_OutfitData.m_ArmourConfig != null)) ? OUTFIT_Localized : ARMOUR_Localized)));
		empty += ((!item.m_ItemData.IsComponent()) ? string.Empty : (", " + COMPONENT_Localized));
		empty += ((!item.m_ItemData.IsTool()) ? string.Empty : (", " + TOOL_Localized));
		if (item.m_ItemData.IsConsumable())
		{
			empty = empty + ", " + CONSUMABLE_Localized;
		}
		if (empty.Length > 2)
		{
			empty = empty.Substring(2);
		}
		m_TooltipInfo.TypeDef = empty;
		TryDisplayToolTip();
	}

	private void TryDisplayToolTip()
	{
		if (AttachedGamer != null && m_CachedEventSystem != null && m_InteractableElement != null && m_CachedEventSystem.currentSelectedGameObject == m_InteractableElement.gameObject)
		{
			DisplayToolTip(null);
		}
	}

	private void SetButtonVariation(int variation)
	{
		if (m_BackgroundImage != null && m_ButtonSpriteVariations != null && m_ButtonSpriteVariations.Length > 0)
		{
			if (variation >= 0 && variation < m_ButtonSpriteVariations.Length)
			{
				m_BackgroundImage.sprite = m_ButtonSpriteVariations[variation];
			}
		}
	}

	private void ClearItemContentImage()
	{
		ResetBackgroundColor();
		SetButtonVariation(0);
		if (m_ContentObject != null)
		{
			m_ContentObject.sprite = null;
			m_ContentObject.gameObject.SetActive(value: false);
		}
	}

	public void SetDurability(int durability)
	{
		if (m_DurabilityText != null)
		{
			m_DurabilityText.gameObject.SetActive(value: true);
			m_DurabilityText.text = NumberToStringCache.GetPercentageString(durability);
		}
	}

	public void DisableDurability()
	{
		if (m_DurabilityText != null)
		{
			m_DurabilityText.gameObject.SetActive(value: false);
		}
	}

	public void DisableItem()
	{
		if (m_InteractableElement != null)
		{
			m_InteractableElement.interactable = false;
		}
	}

	public void EnableItem()
	{
		if (m_InteractableElement != null)
		{
			m_InteractableElement.interactable = true;
		}
	}

	public void SetBackgroundColor(Color32 color)
	{
		if (m_BackgroundImage != null)
		{
			m_BackgroundImage.color = color;
		}
	}

	public void ResetBackgroundColor()
	{
		if (m_BackgroundImage != null)
		{
			m_BackgroundImage.color = m_OriginalBackgroundColor;
		}
	}

	public void RefreshItem()
	{
		if (m_LinkedToItem != null)
		{
			if (m_LinkedToItem.m_ItemData == null)
			{
				m_LinkedToItem.OnQuestItemStatusChanged -= RefreshItem;
				return;
			}
			Item linkedToItem = m_LinkedToItem;
			SetItem(null);
			SetItem(linkedToItem);
		}
	}

	public void SetIndexOfRepresentation(int index)
	{
		m_IndexOfRepresentation = index;
	}

	private void DisplayToolTip(T17Button sender)
	{
		if (!(T17TooltipManager.Instance != null))
		{
			return;
		}
		if (m_MyTooltip == null)
		{
			m_MyTooltip = T17TooltipManager.Instance.GetTooltip();
			if (m_MyTooltip != null)
			{
				m_MyTooltip.SetDelegateToHoldersRewiredId(GetOwnersRewiredId);
			}
		}
		if (!(m_MyTooltip != null))
		{
			return;
		}
		if (m_TooltipInfo != null && !string.IsNullOrEmpty(m_TooltipInfo.Name))
		{
			if (!m_MyTooltip.gameObject.activeSelf)
			{
				m_MyTooltip.gameObject.SetActive(value: true);
			}
			UpdateToolTipInfo();
			RectTransform component = m_MyTooltip.GetComponent<RectTransform>();
			RectTransform component2 = GetComponent<RectTransform>();
			component.transform.SetParent(component2.parent, worldPositionStays: true);
			if (AttachedGamer != null && AttachedGamer.m_PlayerObject != null)
			{
				m_MyTooltip.SetCameraBinding(AttachedGamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
				m_MyTooltip.SetLocalScaleSplit();
			}
			component.transform.localPosition = Vector3.zero;
			Vector2 vector = SwitchToRectTransform(component2, component);
			float x = vector.x + (component.rect.width * 0.5f + component2.rect.width * 0.3f);
			float y = vector.y + (component.rect.height * 0.5f + component2.rect.height * 0.3f);
			component.anchoredPosition = new Vector3(x, y);
		}
		else
		{
			HideToolTip(null);
		}
	}

	public void ChangeDisplaySetup(ContainerSetup setup)
	{
		m_TooltipSetup = setup;
		UpdateToolTipInfo();
	}

	private void UpdateToolTipInfo()
	{
		if (m_MyTooltip == null || m_TooltipInfo == null || string.IsNullOrEmpty(m_TooltipInfo.Name) || m_TooltipSetup == null)
		{
			return;
		}
		if (m_MyTooltip.m_ItemName != null)
		{
			if (string.IsNullOrEmpty(m_TooltipInfo.Name))
			{
				if (m_MyTooltip.m_ItemName.gameObject.activeSelf)
				{
					m_MyTooltip.m_ItemName.gameObject.SetActive(value: false);
				}
			}
			else
			{
				if (!m_MyTooltip.m_ItemName.gameObject.activeSelf)
				{
					m_MyTooltip.m_ItemName.gameObject.SetActive(value: true);
				}
				if (m_MyTooltip.m_ItemName.m_LocalizationTag != m_TooltipInfo.Name)
				{
					m_MyTooltip.m_ItemName.SetNewPlaceHolder(m_TooltipInfo.Name);
					m_MyTooltip.m_ItemName.SetNewLocalizationTag(m_TooltipInfo.Name);
				}
			}
		}
		if (m_MyTooltip.m_ItemType != null)
		{
			if (string.IsNullOrEmpty(m_TooltipInfo.TypeDef))
			{
				if (m_MyTooltip.m_ItemType.gameObject.activeSelf)
				{
					m_MyTooltip.m_ItemType.gameObject.SetActive(value: false);
				}
			}
			else
			{
				if (!m_MyTooltip.m_ItemType.gameObject.activeSelf)
				{
					m_MyTooltip.m_ItemType.gameObject.SetActive(value: true);
				}
				if (m_MyTooltip.m_ItemName.text != m_TooltipInfo.TypeDef)
				{
					m_MyTooltip.m_ItemType.m_bNeedsLocalization = false;
					m_MyTooltip.m_ItemType.text = m_TooltipInfo.TypeDef;
				}
			}
		}
		if (m_LinkedToItem == null || m_LinkedToItem.m_ItemData == null || m_MyTooltip.m_SecondaryInputLabel == null || m_MyTooltip.m_PrimaryInputLabel == null || m_MyTooltip.m_CancelInputLabel == null)
		{
			return;
		}
		m_MyTooltip.SetTooltipBackground(m_LinkedToItem.m_ItemData.IsContraband());
		m_MyTooltip.SetDisplayForContext(m_TooltipSetup.m_DefaultContainerSetup);
		SetUpLabelWithString(m_MyTooltip.m_SecondaryInputLabel, m_TooltipSetup.m_SecondaryInputText);
		SetUpLabelWithString(m_MyTooltip.m_PrimaryInputLabel, m_TooltipSetup.m_PrimaryInputText);
		SetUpLabelWithString(m_MyTooltip.m_CancelInputLabel, m_TooltipSetup.m_CancelInputText);
		if (m_TooltipSetup.m_DefaultContainerSetup == T17ItemTooltip.DisplaySetups.ItemFunctionalitiesAndCancel)
		{
			BaseItemFunctionality firstFunctionality = m_LinkedToItem.GetFirstFunctionality();
			if (firstFunctionality == null)
			{
				m_MyTooltip.m_SecondaryInputContainer.SetActive(value: false);
			}
			else
			{
				m_MyTooltip.m_SecondaryInputLabel.SetLocalisedTextCatchAll(firstFunctionality.m_ActionVerb);
			}
		}
	}

	private void SetUpLabelWithString(T17Text textLabel, string localisedString)
	{
		if (textLabel.gameObject.activeSelf && textLabel.m_LocalizationTag != localisedString)
		{
			textLabel.SetLocalisedTextCatchAll(localisedString);
		}
	}

	private Vector2 SwitchToRectTransform(RectTransform from, RectTransform to)
	{
		Vector2 vector = new Vector2(from.rect.width * from.pivot.x + from.rect.xMin, from.rect.height * from.pivot.y + from.rect.yMin);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, from.position);
		screenPoint += vector;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(to, screenPoint, null, out var localPoint);
		Vector2 vector2 = new Vector2(to.rect.width * to.pivot.x + to.rect.xMin, to.rect.height * to.pivot.y + to.rect.yMin);
		return to.anchoredPosition + localPoint - vector2;
	}

	public void HideToolTip(T17Button sender)
	{
		if (m_MyTooltip != null)
		{
			T17TooltipManager.Instance.ReleaseTooltip(m_MyTooltip);
			m_MyTooltip = null;
		}
	}

	private void CheckForInteractableElement()
	{
		if (m_InteractableElement == null && base.gameObject != null)
		{
			m_InteractableElement = GetComponentInChildren<T17Button>(includeInactive: true);
		}
	}

	public void ItemSelected(T17Button sender)
	{
		if (OnItemSelected != null)
		{
			OnItemSelected(m_IndexOfRepresentation);
		}
	}

	private void ButtonDeselected(T17Button sender, BaseEventData eventData)
	{
		bool flag = false;
		if (sender.IsBeingReselectedViaClick())
		{
			flag = true;
		}
		if (!flag)
		{
			ItemDeselected(sender);
			HideToolTip(sender);
		}
	}

	public void ItemDeselected(T17Button sender)
	{
		HideToolTip(null);
		if (OnItemDeselected != null)
		{
			OnItemDeselected(m_IndexOfRepresentation);
		}
	}

	public int GetOwnersRewiredId()
	{
		if (AttachedGamer != null && AttachedGamer.m_RewiredPlayer != null)
		{
			return AttachedGamer.m_RewiredPlayer.id;
		}
		return 0;
	}

	public void SetGamerForEventSystem(Gamer gamer, T17EventSystem gamersEventSystem)
	{
		AttachedGamer = gamer;
		if (gamer != null)
		{
			if (gamersEventSystem == null)
			{
				m_CachedEventSystem = T17EventSystemsManager.Instance.GetEventSystemForGamer(AttachedGamer);
			}
			else
			{
				m_CachedEventSystem = gamersEventSystem;
			}
		}
		else
		{
			m_CachedEventSystem = null;
		}
		TryDisplayToolTip();
	}

	public void StartSmoke()
	{
		m_bFadeInProgress = true;
		m_ContentObject.gameObject.SetActive(value: false);
		if (m_SmokeAnimator != null)
		{
			m_SmokeAnimator.gameObject.SetActive(value: true);
			m_SmokeAnimator.Play("Smoke");
		}
		else
		{
			SmokeCriticalPointCallback();
			SmokeCompleteCallback();
		}
	}

	public void StopSmoke()
	{
		if (m_SmokeAnimator != null)
		{
			m_SmokeAnimator.gameObject.SetActive(value: false);
		}
		if (m_LinkedToItem != null)
		{
			m_ContentObject.gameObject.SetActive(value: true);
		}
		m_bFadeInProgress = false;
	}

	public void SmokeCriticalPointCallback()
	{
		if (m_LinkedToItem != null)
		{
			m_ContentObject.gameObject.SetActive(value: true);
		}
	}

	public void SmokeCompleteCallback()
	{
		StopSmoke();
	}

	public Item GetLinkedItem()
	{
		return m_LinkedToItem;
	}

	public static void RunSmokesOnNewItems(InventoryItem[] guiElements, List<Item> previousItemsList)
	{
		if (guiElements == null || previousItemsList == null)
		{
			return;
		}
		for (int i = 0; i < guiElements.Length; i++)
		{
			if (guiElements[i] != null)
			{
				Item linkedItem = guiElements[i].GetLinkedItem();
				if (linkedItem != null && previousItemsList.IndexOf(linkedItem) == -1)
				{
					guiElements[i].StartSmoke();
				}
			}
		}
	}

	public T17EventSystem GetDomain()
	{
		return null;
	}

	public GameObject GetGameobject()
	{
		return base.gameObject;
	}

	public void SetTypeDefForItem(ItemData itemData)
	{
		string empty = string.Empty;
		empty += ((!itemData.IsWeapon()) ? string.Empty : (", " + WEAPON_Localized));
		empty += ((!itemData.IsOutfit()) ? string.Empty : (", " + ((!(itemData.m_OutfitData.m_ArmourConfig != null)) ? OUTFIT_Localized : ARMOUR_Localized)));
		empty += ((!itemData.IsComponent()) ? string.Empty : (", " + COMPONENT_Localized));
		empty += ((!itemData.IsTool()) ? string.Empty : (", " + TOOL_Localized));
		if (itemData.IsConsumable())
		{
			empty = empty + ", " + CONSUMABLE_Localized;
		}
		if (empty.Length > 2)
		{
			empty = empty.Substring(2);
		}
		m_TooltipInfo.TypeDef = empty;
	}

	public ItemData GetItemDataForItem()
	{
		if (m_LinkedToItem != null)
		{
			return m_LinkedToItem.m_ItemData;
		}
		return null;
	}

	private void OnDisable()
	{
		HideToolTip(null);
	}

	private void ResetTooltip()
	{
		if (base.isActiveAndEnabled && m_MyTooltip != null && m_MyTooltip.isActiveAndEnabled)
		{
			m_MyTooltip.SetLocalScaleSplit();
		}
	}

	public bool CanReselectOnMouseDisable()
	{
		return true;
	}

	public bool ReleaseSelectionOnPointerClickOrExit()
	{
		return true;
	}
}
