using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class CraftingMenu : GameMenuBehaviour
{
	public Sprite m_UnknownItemSprite;

	[FormerlySerializedAs("m_RecipeItem1")]
	public InventoryItem m_RecipeItemSlot1;

	[FormerlySerializedAs("m_RecipeItem2")]
	public InventoryItem m_RecipeItemSlot2;

	[FormerlySerializedAs("m_RecipeItem3")]
	public InventoryItem m_RecipeItemSlot3;

	public float m_ItemIsIngredientAlpha = 0.25f;

	[Header("Tab Underline graphics")]
	public T17Image m_TabUnderlineGraphic;

	public List<Sprite> m_TabUnderlineSprites;

	[Header("Crafting output labels")]
	public T17Text m_ResultName;

	public T17Text m_ResultType;

	public T17Text m_ResultDescription;

	public CraftingTooltip m_Tooltip;

	public CraftingTooltip m_TooltipSwitchHandheld;

	private CraftingTooltip m_TooltipToShow;

	public bool m_TestSwitchTooltip;

	public T17Button m_CraftButton;

	public GameObject m_RecipeSlotsParent;

	private InventoryItem[] m_RecipeSlots;

	private T17TabPanel m_PageTabPanel;

	private int[] m_IndexArrayForRecipeChecking = new int[16];

	private static Color m_LightRed = new Color32(byte.MaxValue, 79, 95, byte.MaxValue);

	private int m_CurrentOpenPage;

	public Sprite m_UnknownIngredientSprite;

	public float m_HoldToCraftLength = 1f;

	public float m_RecipeButtonClickLength = 0.1f;

	private float m_HoldToCraftTime;

	public bool m_bRestartHoldOnRelease;

	private int m_CurrentClickedRecipeID = -1;

	private const string HOTKEY_CRAFTINGMENU = "Hotkey_CraftingMenu";

	private TemporaryInventoryMenuBehaviour m_TemporaryCraftingInventory;

	public int CurrentOpenPage => m_CurrentOpenPage;

	protected override void Awake()
	{
		base.Awake();
		m_RecipeSlots = m_RecipeSlotsParent.GetComponentsInChildren<InventoryItem>(includeInactive: true);
		m_PageTabPanel = GetComponentInChildren<T17TabPanel>();
	}

	protected override void SingleTimeInitialize()
	{
		base.SingleTimeInitialize();
		m_TemporaryCraftingInventory = new TemporaryInventoryMenuBehaviour(null, new InventoryItem[3] { m_RecipeItemSlot1, m_RecipeItemSlot2, m_RecipeItemSlot3 }, null);
		SetupTooltip();
		m_TemporaryCraftingInventory.ClearItemSlots();
		CloseToolTip(-1);
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		SetupTooltip();
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToCallback();
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryCraftingInventory.OnInventoryItemClicked));
			BaseInventoryBehaviour playerInventoryBehaviour2 = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour2.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Combine(playerInventoryBehaviour2.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryCraftingInventory.OnInventoryItemClicked));
		}
		CraftManager.OnItemCrafted = (CraftManager.CraftEvent)Delegate.Combine(CraftManager.OnItemCrafted, new CraftManager.CraftEvent(OnItemCrafted));
		OpenRecipePage(m_CurrentOpenPage + 1);
		if (m_TooltipToShow != null && currentGamer != null && currentGamer.m_PlayerObject != null)
		{
			m_TooltipToShow.SetCameraBinding(currentGamer.m_PlayerObject.m_PlayerCameraManagerBindingID);
			m_TooltipToShow.SetLocalScaleSplit();
		}
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Combine(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(ResetTooltip));
		}
		if (base.CurrentGamePlayer != null)
		{
			Player currentGamePlayer = base.CurrentGamePlayer;
			currentGamePlayer.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Combine(currentGamePlayer.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnKnockOut));
			m_TemporaryCraftingInventory.UpdatePlayerAndPlayerMenu(base.CurrentGamePlayer, InGameMenuFlow.Instance.GetInventoryMenu(base.CurrentGamePlayer.m_PlayerCameraManagerBindingID));
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (m_TemporaryCraftingInventory != null)
		{
			m_TemporaryCraftingInventory.ClearItemSlots();
		}
		CloseToolTip(-1);
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null && m_TemporaryCraftingInventory != null)
		{
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(m_TemporaryCraftingInventory.OnInventoryItemClicked));
		}
		CraftManager.OnItemCrafted = (CraftManager.CraftEvent)Delegate.Remove(CraftManager.OnItemCrafted, new CraftManager.CraftEvent(OnItemCrafted));
		if (base.CurrentGamePlayer != null)
		{
			Player currentGamePlayer = base.CurrentGamePlayer;
			currentGamePlayer.OnCharacterKnockedOut = (Character.CharacterToCharacterEvent)Delegate.Remove(currentGamePlayer.OnCharacterKnockedOut, new Character.CharacterToCharacterEvent(OnKnockOut));
		}
		if (m_TemporaryCraftingInventory != null)
		{
			m_TemporaryCraftingInventory.RestorePlayerInventoryAlpha();
		}
		if (!base.Hide(restoreInvokerState, isTabSwitch))
		{
			return false;
		}
		if (m_TooltipToShow != null)
		{
			m_TooltipToShow.ResetCameraBinding();
		}
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(ResetTooltip));
		}
		return true;
	}

	public void OpenRecipePage(int page)
	{
		int num = page - 1;
		if (num < 0)
		{
			num = 0;
		}
		if (m_TabUnderlineGraphic != null && page - 1 < m_TabUnderlineSprites.Count)
		{
			m_TabUnderlineGraphic.sprite = m_TabUnderlineSprites[page - 1];
		}
		ClearOldRecipeSlots();
		PopulateRecipeSlots(num);
	}

	private void ClearOldRecipeSlots()
	{
		for (int i = 0; i < m_RecipeSlots.Length; i++)
		{
			m_RecipeSlots[i].SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
			m_RecipeSlots[i].OnItemSelected = null;
			m_RecipeSlots[i].OnItemDeselected = null;
			T17Button interactableElement = m_RecipeSlots[i].InteractableElement;
			interactableElement.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Remove(interactableElement.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(m_RecipeSlots[i].ItemSelected));
			T17Button interactableElement2 = m_RecipeSlots[i].InteractableElement;
			interactableElement2.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Remove(interactableElement2.OnButtonPointerExit, new T17Button.T17ButtonDelegate(m_RecipeSlots[i].ItemDeselected));
			m_RecipeSlots[i].InteractableElement.OnButtonPointerDown = null;
			m_RecipeSlots[i].InteractableElement.OnButtonSelect = null;
			if (m_RecipeSlots[i].InteractableElement != null)
			{
				m_RecipeSlots[i].InteractableElement.onClick.RemoveAllListeners();
			}
		}
	}

	private void PopulateRecipeSlots(int page)
	{
		m_CurrentOpenPage = page;
		int num = 10 * page + 30;
		int num2 = num + 10;
		if (num >= 70)
		{
			num2 = 100;
		}
		CraftManager instance = CraftManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		List<CraftManager.Recipe> currentRecipes = instance.GetCurrentRecipes();
		int count = currentRecipes.Count;
		int num3 = 0;
		for (int i = 0; i < count; i++)
		{
			CraftManager.Recipe recipe = currentRecipes[i];
			if (recipe == null || !(recipe.m_Product != null) || recipe.m_Intellect < num || recipe.m_Intellect >= num2)
			{
				continue;
			}
			bool flag = CraftManager.GetInstance().HasItemsForRecipe(i, base.CurrentGamer.m_PlayerObject.m_ItemContainer, ref m_IndexArrayForRecipeChecking);
			bool isHidden = recipe.m_bHidden && !recipe.m_bDiscovered;
			SetImageOnInventoryItem(ref m_RecipeSlots[num3], recipe.m_Product.m_ItemUIImage, !flag, isHidden);
			m_RecipeSlots[num3].LinkedItemDataID = recipe.m_Product.m_ItemDataID;
			m_RecipeSlots[num3].SetTypeDefForItem(recipe.m_Product);
			InventoryItem obj = m_RecipeSlots[num3];
			obj.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj.OnItemSelected, new InventoryItem.InventoryItemEvent(InventoryItemWasSelected));
			InventoryItem obj2 = m_RecipeSlots[num3];
			obj2.OnItemSelected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj2.OnItemSelected, new InventoryItem.InventoryItemEvent(OnRecipeSlotSelected));
			InventoryItem obj3 = m_RecipeSlots[num3];
			obj3.OnItemDeselected = (InventoryItem.InventoryItemEvent)Delegate.Combine(obj3.OnItemDeselected, new InventoryItem.InventoryItemEvent(CloseToolTip));
			m_RecipeSlots[num3].SetIndexOfRepresentation(i);
			int recipeIndex = i;
			if (m_RecipeSlots[num3].InteractableElement != null)
			{
				m_RecipeSlots[num3].InteractableElement.onClick.RemoveAllListeners();
				m_RecipeSlots[num3].InteractableElement.onClick.AddListener(delegate
				{
					OnRecipeSlotClicked(recipeIndex);
				});
				T17Button interactableElement = m_RecipeSlots[num3].InteractableElement;
				interactableElement.OnButtonSelect = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement.OnButtonSelect, (T17Button.T17ButtonDelegate)delegate
				{
					OnRecipeSlotSelected(recipeIndex);
					InventoryItemWasSelected(recipeIndex);
				});
				T17Button interactableElement2 = m_RecipeSlots[num3].InteractableElement;
				interactableElement2.OnButtonPointerDown = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement2.OnButtonPointerDown, (T17Button.T17ButtonDelegate)delegate
				{
					OnRecipeSlotClicked(recipeIndex);
				});
				T17Button interactableElement3 = m_RecipeSlots[num3].InteractableElement;
				interactableElement3.OnButtonPointerEnter = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement3.OnButtonPointerEnter, new T17Button.T17ButtonDelegate(m_RecipeSlots[num3].ItemSelected));
				T17Button interactableElement4 = m_RecipeSlots[num3].InteractableElement;
				interactableElement4.OnButtonPointerExit = (T17Button.T17ButtonDelegate)Delegate.Combine(interactableElement4.OnButtonPointerExit, new T17Button.T17ButtonDelegate(m_RecipeSlots[num3].ItemDeselected));
			}
			num3++;
			if (num3 >= m_RecipeSlots.Length)
			{
				break;
			}
		}
	}

	private void InventoryItemWasSelected(int representationIndex)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (!(instance != null) || !(base.CurrentGamePlayer != null))
		{
			return;
		}
		List<CraftManager.Recipe> currentRecipes = instance.GetCurrentRecipes();
		if (representationIndex < 0 || representationIndex >= currentRecipes.Count)
		{
			return;
		}
		instance.CheckItemsAvailable(representationIndex, base.CurrentGamePlayer.m_ItemContainer, ref m_IndexArrayForRecipeChecking);
		CraftManager.Recipe recipe = currentRecipes[representationIndex];
		if (recipe == null || !(recipe.m_Product != null))
		{
			return;
		}
		if (!recipe.m_bHidden || recipe.m_bDiscovered)
		{
			if (!(m_TooltipToShow != null))
			{
				return;
			}
			m_TooltipToShow.gameObject.SetActive(value: true);
			if (m_TooltipToShow.m_RecipeItem1 != null && recipe.m_Ingredients[0] != null)
			{
				if (m_IndexArrayForRecipeChecking[0] == -100)
				{
					m_TooltipToShow.m_RecipeItem1.SetBackgroundColor(m_LightRed);
				}
				else
				{
					m_TooltipToShow.m_RecipeItem1.ResetBackgroundColor();
				}
				SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem1, recipe.m_Ingredients[0].m_ItemUIImage);
			}
			if (m_TooltipToShow.m_RecipeItem2 != null && recipe.m_Ingredients[1] != null)
			{
				if (m_IndexArrayForRecipeChecking[1] == -100)
				{
					m_TooltipToShow.m_RecipeItem2.SetBackgroundColor(m_LightRed);
				}
				else
				{
					m_TooltipToShow.m_RecipeItem2.ResetBackgroundColor();
				}
				SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem2, recipe.m_Ingredients[1].m_ItemUIImage);
			}
			if (m_TooltipToShow.m_RecipeItem3 != null && recipe.m_Ingredients[2] != null)
			{
				if (m_IndexArrayForRecipeChecking[2] == -100)
				{
					m_TooltipToShow.m_RecipeItem3.SetBackgroundColor(m_LightRed);
				}
				else
				{
					m_TooltipToShow.m_RecipeItem3.ResetBackgroundColor();
				}
				SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem3, recipe.m_Ingredients[2].m_ItemUIImage);
			}
			if (m_TooltipToShow.m_CraftName != null)
			{
				m_TooltipToShow.m_CraftName.SetNewPlaceHolder(recipe.m_Product.m_ItemLocalizationTag);
				m_TooltipToShow.m_CraftName.SetNewLocalizationTag(recipe.m_Product.m_ItemLocalizationTag);
			}
			T17EventSystem eventSystemForGamer = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
			GameObject gameObject = null;
			GameObject currentPointerOverGameobject = eventSystemForGamer.GetCurrentPointerOverGameobject();
			if (currentPointerOverGameobject != null)
			{
				m_TooltipToShow.SetPositionWithOffset(currentPointerOverGameobject.transform.position);
				gameObject = currentPointerOverGameobject;
			}
			else if (eventSystemForGamer.currentSelectedGameObject != null)
			{
				m_TooltipToShow.SetPositionWithOffset(eventSystemForGamer.currentSelectedGameObject.transform.position);
				gameObject = eventSystemForGamer.currentSelectedGameObject;
			}
			if (!(m_TooltipToShow.m_CraftType != null))
			{
				return;
			}
			InventoryItem inventoryItem = null;
			if (gameObject != null)
			{
				inventoryItem = gameObject.GetComponent<InventoryItem>();
				if (inventoryItem != null)
				{
					string typeDef = inventoryItem.m_TooltipInfo.TypeDef;
					m_TooltipToShow.m_CraftType.m_bNeedsLocalization = false;
					m_TooltipToShow.m_CraftType.SetNewPlaceHolder(typeDef);
					m_TooltipToShow.m_CraftType.SetNewLocalizationTag(typeDef);
				}
			}
			return;
		}
		m_TooltipToShow.gameObject.SetActive(value: true);
		if (m_TooltipToShow.m_RecipeItem1 != null && recipe.m_Ingredients[0] != null)
		{
			SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem1, m_UnknownIngredientSprite);
		}
		if (m_TooltipToShow.m_RecipeItem2 != null && recipe.m_Ingredients[1] != null)
		{
			SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem2, m_UnknownIngredientSprite);
		}
		if (m_TooltipToShow.m_RecipeItem3 != null && recipe.m_Ingredients[2] != null)
		{
			SetImageOnInventoryItem(ref m_TooltipToShow.m_RecipeItem3, m_UnknownIngredientSprite);
		}
		if (m_TooltipToShow.m_CraftName != null)
		{
			m_TooltipToShow.m_CraftName.SetNewPlaceHolder(recipe.m_Product.m_ItemLocalizationTag);
			m_TooltipToShow.m_CraftName.SetNewLocalizationTag(recipe.m_Product.m_ItemLocalizationTag);
		}
		T17EventSystem eventSystemForGamer2 = T17EventSystemsManager.Instance.GetEventSystemForGamer(base.CurrentGamer);
		GameObject gameObject2 = null;
		GameObject currentPointerOverGameobject2 = eventSystemForGamer2.GetCurrentPointerOverGameobject();
		if (currentPointerOverGameobject2 != null)
		{
			m_TooltipToShow.SetPositionWithOffset(currentPointerOverGameobject2.transform.position);
			gameObject2 = currentPointerOverGameobject2;
		}
		else if (eventSystemForGamer2.currentSelectedGameObject != null)
		{
			m_TooltipToShow.SetPositionWithOffset(eventSystemForGamer2.currentSelectedGameObject.transform.position);
			gameObject2 = eventSystemForGamer2.currentSelectedGameObject;
		}
		if (!(m_TooltipToShow.m_CraftType != null))
		{
			return;
		}
		InventoryItem inventoryItem2 = null;
		if (gameObject2 != null)
		{
			inventoryItem2 = gameObject2.GetComponent<InventoryItem>();
			if (inventoryItem2 != null)
			{
				string typeDef2 = inventoryItem2.m_TooltipInfo.TypeDef;
				m_TooltipToShow.m_CraftType.m_bNeedsLocalization = false;
				m_TooltipToShow.m_CraftType.SetNewPlaceHolder(typeDef2);
				m_TooltipToShow.m_CraftType.SetNewLocalizationTag(typeDef2);
			}
		}
	}

	private void SetupTooltip()
	{
		m_TooltipToShow = m_Tooltip;
	}

	private void CloseToolTip(int representationIndex)
	{
		if (m_TooltipToShow != null)
		{
			if (m_TooltipToShow.m_RecipeItem1 != null)
			{
				m_TooltipToShow.m_RecipeItem1.SetItemContentImage(null);
				m_TooltipToShow.m_RecipeItem1.ResetBackgroundColor();
			}
			if (m_TooltipToShow.m_RecipeItem2 != null)
			{
				m_TooltipToShow.m_RecipeItem2.SetItemContentImage(null);
				m_TooltipToShow.m_RecipeItem2.ResetBackgroundColor();
			}
			if (m_TooltipToShow.m_RecipeItem3 != null)
			{
				m_TooltipToShow.m_RecipeItem3.SetItemContentImage(null);
				m_TooltipToShow.m_RecipeItem3.ResetBackgroundColor();
			}
			if (m_TooltipToShow.m_CraftName != null)
			{
				m_TooltipToShow.m_CraftName.SetNewPlaceHolder("-");
				m_TooltipToShow.m_CraftName.SetNewLocalizationTag("-");
			}
			if (m_TooltipToShow.m_CraftType != null)
			{
				m_TooltipToShow.m_CraftType.SetNewPlaceHolder("-");
				m_TooltipToShow.m_CraftType.SetNewLocalizationTag("-");
			}
			m_TooltipToShow.gameObject.SetActive(value: false);
		}
		if (representationIndex == m_CurrentClickedRecipeID)
		{
			m_CurrentClickedRecipeID = -1;
			m_HoldToCraftTime = 0f;
		}
	}

	private void SetImageOnInventoryItem(ref InventoryItem itemslot, Sprite image, bool isFaded = false, bool isHidden = false)
	{
		if (image == null && m_UnknownItemSprite != null)
		{
			image = m_UnknownItemSprite;
		}
		itemslot.SetItemContentImage(image, autoClearIfNull: true, autoDisableIfNull: true, isFaded, isHidden, resetVariation: false);
	}

	private void FillSlots(int itemManagerRecipeIndex)
	{
		if (!(base.CurrentGamePlayer != null) || !(base.CurrentGamePlayer.m_ItemContainer != null))
		{
			return;
		}
		if (!base.CurrentGamePlayer.GetIsKnockedOut())
		{
			CraftManager instance = CraftManager.GetInstance();
			if (!(instance != null))
			{
				return;
			}
			List<CraftManager.Recipe> currentRecipes = instance.GetCurrentRecipes();
			if (!CraftManager.GetInstance().HasItemsForRecipe(itemManagerRecipeIndex, base.CurrentGamePlayer.m_ItemContainer, ref m_IndexArrayForRecipeChecking) && (itemManagerRecipeIndex >= currentRecipes.Count || itemManagerRecipeIndex < 0 || !CheckPlayerAndMenuHasItems(itemManagerRecipeIndex)))
			{
				return;
			}
			m_TemporaryCraftingInventory.ClearItemSlots();
			CraftManager.Recipe recipe = CraftManager.GetInstance().GetCurrentRecipes()[itemManagerRecipeIndex];
			if (recipe.m_bHidden && !recipe.m_bDiscovered)
			{
				return;
			}
			Item[] array = SelectBestItemsForRecipe(recipe, base.CurrentGamePlayer.m_ItemContainer, base.CurrentGamePlayer.GetEquippedItem());
			if (m_RecipeItemSlot1 != null)
			{
				Item item = null;
				if (array.Length >= 1)
				{
					item = array[0];
				}
				if (item != null)
				{
					m_TemporaryCraftingInventory.SetItemForItemSlot(m_RecipeItemSlot1, item);
				}
			}
			if (m_RecipeItemSlot2 != null)
			{
				Item item2 = null;
				if (array.Length >= 2)
				{
					item2 = array[1];
				}
				if (item2 != null)
				{
					m_TemporaryCraftingInventory.SetItemForItemSlot(m_RecipeItemSlot2, item2);
				}
			}
			if (m_RecipeItemSlot3 != null)
			{
				Item item3 = null;
				if (array.Length >= 3)
				{
					item3 = array[2];
				}
				if (item3 != null)
				{
					m_TemporaryCraftingInventory.SetItemForItemSlot(m_RecipeItemSlot3, item3);
				}
			}
		}
		else
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, AudioController.UI_Audio_GO);
		}
	}

	private Item[] SelectBestItemsForRecipe(CraftManager.Recipe recipe, ItemContainer container, Item equipped)
	{
		int ingredientCount = recipe.GetIngredientCount();
		List<int> list = new List<int>();
		if (equipped != null && equipped.IsQuestItem())
		{
			list.Add(equipped.m_QuestItemGroupID);
		}
		int itemCount = container.GetItemCount();
		for (int i = 0; i < itemCount; i++)
		{
			Item item = container.GetItem(i);
			if (item != null && item.IsQuestItem() && !list.Contains(item.m_QuestItemGroupID))
			{
				list.Add(item.m_QuestItemGroupID);
			}
		}
		Dictionary<int, int> groupScores = new Dictionary<int, int>(itemCount);
		for (int j = 0; j < list.Count; j++)
		{
			int value = EvaluateQuestItemGroupForRecipe(list[j], recipe, container, equipped);
			groupScores.Add(list[j], value);
		}
		Item[] array = new Item[ingredientCount];
		int bestGroupID = -1;
		if (list != null && list.Count > 0)
		{
			list.Sort((int x, int y) => groupScores[y].CompareTo(groupScores[x]));
			bestGroupID = list[0];
		}
		bool flag = false;
		bool flag2 = bestGroupID >= 0;
		if (flag2 && groupScores[bestGroupID] >= 0)
		{
			bool flag3 = true;
			List<int> itemIDsForGroup = QuestManager.GetInstance().GetItemIDsForGroup(base.CurrentGamer.m_PlayerObject.m_NetView.viewID, bestGroupID);
			itemCount = Mathf.Min(ingredientCount, itemIDsForGroup.Count);
			for (int k = 0; k < itemCount; k++)
			{
				if (!container.HasSpecificItem(itemIDsForGroup[k]) && (equipped == null || itemIDsForGroup[k] != equipped.m_NetView.viewID))
				{
					flag3 = false;
					break;
				}
			}
			if (flag3)
			{
				for (int l = 0; l < itemCount; l++)
				{
					array[l] = T17NetView.Find<Item>(itemIDsForGroup[l]);
				}
				flag = true;
			}
		}
		if (!flag)
		{
			List<Item> itemList = new List<Item>();
			for (int m = 0; m < ingredientCount; m++)
			{
				ItemData itemData = recipe.m_Ingredients[m];
				if (itemData == null)
				{
					continue;
				}
				container.GetItemsWithItemID(ref itemList, itemData.m_ItemDataID, ingredientCount);
				if (equipped != null && equipped.ItemDataID == itemData.m_ItemDataID)
				{
					itemList.Add(equipped);
				}
				for (int n = 0; n <= m; n++)
				{
					if (array[n] != null)
					{
						itemList.Remove(array[n]);
					}
				}
				if (itemList.Count <= 0)
				{
					continue;
				}
				if (flag2)
				{
					array[m] = itemList.Find((Item x) => x.IsQuestItem() && x.m_QuestItemGroupID == bestGroupID);
				}
				if (array[m] == null)
				{
					array[m] = itemList.Find((Item x) => !x.IsQuestItem());
				}
				if (array[m] == null)
				{
					array[m] = itemList[0];
				}
			}
		}
		return array;
	}

	private int EvaluateQuestItemGroupForRecipe(int groupID, CraftManager.Recipe recipe, ItemContainer container, Item equipped)
	{
		List<int> itemIDsForGroup = QuestManager.GetInstance().GetItemIDsForGroup(base.CurrentGamer.m_PlayerObject.m_NetView.viewID, groupID);
		if (itemIDsForGroup.Count != recipe.m_Ingredients.Length)
		{
			return -1;
		}
		int num = 0;
		for (int i = 0; i < itemIDsForGroup.Count; i++)
		{
			Item item = T17NetView.Find<Item>(itemIDsForGroup[i]);
			if (!(item != null))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < recipe.m_Ingredients.Length; j++)
			{
				if (recipe.m_Ingredients[j] != null && item.ItemDataID == recipe.m_Ingredients[j].m_ItemDataID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num = -1;
				break;
			}
			if (container.HasSpecificItem(itemIDsForGroup[i]) || (equipped != null && itemIDsForGroup[i] == equipped.m_NetView.viewID))
			{
				num++;
			}
		}
		return num;
	}

	public void OnCraftClicked()
	{
		CraftManager instance = CraftManager.GetInstance();
		Item[] itemsCurrentlyInSlots = m_TemporaryCraftingInventory.GetItemsCurrentlyInSlots(includeNulls: true);
		if (itemsCurrentlyInSlots.Length != 3)
		{
			return;
		}
		Item item = itemsCurrentlyInSlots[0];
		Item item2 = itemsCurrentlyInSlots[1];
		Item item3 = itemsCurrentlyInSlots[2];
		if ((!(item == null) || !(item2 == null) || !(item3 == null)) && !(instance == null) && (!(item != null) || instance.HaveRecipeWithItem(item.m_ItemData)) && (!(item2 != null) || instance.HaveRecipeWithItem(item2.m_ItemData)) && (!(item3 != null) || instance.HaveRecipeWithItem(item3.m_ItemData)))
		{
			int recipeIDFromItems = instance.GetRecipeIDFromItems((!(item != null)) ? null : item.m_ItemData, (!(item2 != null)) ? null : item2.m_ItemData, (!(item3 != null)) ? null : item3.m_ItemData);
			if (recipeIDFromItems != -1)
			{
				ExecuteCraft(recipeIDFromItems);
			}
		}
	}

	public void ClearCraftSlot(int slotIndex)
	{
		m_TemporaryCraftingInventory.ClearSlot(slotIndex - 1);
	}

	public ItemData[] GetItemsCurrentlyInSlots()
	{
		return m_TemporaryCraftingInventory.GetItemDataCurrentlyInSlots();
	}

	public int GetCraftPageNumberForItem(ItemData item)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (instance != null && item != null)
		{
			CraftManager.Recipe recipeForProduct = instance.GetRecipeForProduct(item);
			if (recipeForProduct != null)
			{
				float num = (recipeForProduct.m_Intellect - 30) / 10;
				return (int)num;
			}
		}
		return -1;
	}

	public T17Button GetButtonForCraftPage(int pageNumber)
	{
		if (m_PageTabPanel != null && m_PageTabPanel.m_Buttons != null && pageNumber >= 0 && pageNumber < m_PageTabPanel.m_Buttons.Length)
		{
			return m_PageTabPanel.m_Buttons[pageNumber];
		}
		return null;
	}

	public InventoryItem GetRecipeSlotForItem(ItemData itemData)
	{
		for (int i = 0; i < m_RecipeSlots.Length; i++)
		{
			if (m_RecipeSlots[i].LinkedItemDataID == itemData.m_ItemDataID)
			{
				return m_RecipeSlots[i];
			}
		}
		return null;
	}

	public bool CheckPlayerAndMenuHasItems(int recipeID)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (base.CurrentGamePlayer != null && instance != null)
		{
			ItemContainer itemContainer = base.CurrentGamePlayer.m_ItemContainer;
			ItemData[] itemsCurrentlyInSlots = GetItemsCurrentlyInSlots();
			Item equippedItem = base.CurrentGamePlayer.GetEquippedItem();
			int num = itemsCurrentlyInSlots.Length + itemContainer.GetItemCount();
			if (equippedItem != null)
			{
				num++;
			}
			ItemData[] array = new ItemData[num];
			int num2 = 0;
			if (equippedItem != null)
			{
				array[num2] = equippedItem.m_ItemData;
				num2++;
			}
			for (int i = 0; i < itemContainer.GetItemCount(); i++)
			{
				Item item = itemContainer.GetItem(i);
				if (item != null)
				{
					array[num2] = item.m_ItemData;
					num2++;
				}
			}
			for (int j = 0; j < itemsCurrentlyInSlots.Length; j++)
			{
				if (itemsCurrentlyInSlots[j] != null)
				{
					array[num2] = itemsCurrentlyInSlots[j];
					num2++;
				}
			}
			int[] usingItemIndices = new int[8];
			return instance.HasItemsForRecipe(recipeID, array, ref usingItemIndices);
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (m_CurrentClickedRecipeID != -1)
		{
			if (base.CurrentGamer != null && base.CurrentGamer.m_RewiredPlayer.GetButtonUp("UI_Submit"))
			{
				if (m_HoldToCraftTime < m_RecipeButtonClickLength)
				{
					m_HoldToCraftTime = 0f;
				}
				if (m_bRestartHoldOnRelease)
				{
					m_HoldToCraftTime = 0f;
				}
				m_CurrentClickedRecipeID = -1;
			}
			else
			{
				if (m_HoldToCraftTime > m_HoldToCraftLength + m_RecipeButtonClickLength)
				{
					CraftItemInRecipeSlot(m_CurrentClickedRecipeID);
					m_HoldToCraftTime = 0f;
					m_CurrentClickedRecipeID = -1;
				}
				m_HoldToCraftTime += UpdateManager.deltaTime;
			}
		}
		else if (m_HoldToCraftTime > 0f)
		{
			m_HoldToCraftTime -= UpdateManager.deltaTime;
		}
		if (m_TooltipToShow != null && m_TooltipToShow.m_HoldToCraftBar != null)
		{
			m_TooltipToShow.m_HoldToCraftBar.value = 1f / (m_HoldToCraftLength - m_RecipeButtonClickLength) * m_HoldToCraftTime - m_RecipeButtonClickLength;
		}
		if (base.CurrentGamer != null && base.CurrentGamePlayer != null && base.CurrentGamer.m_RewiredPlayer.GetButtonUp("Hotkey_CraftingMenu"))
		{
			base.CurrentGamePlayer.SwallowInputAction("Hotkey_CraftingMenu");
			base.CurrentGamePlayer.RequestCloseContainer();
		}
	}

	private void OnReciepeSlotPointerUp(int recipeSlotID)
	{
		if (m_CurrentClickedRecipeID == recipeSlotID)
		{
			m_CurrentClickedRecipeID = -1;
		}
	}

	private void OnRecipeSlotClicked(int recipeSlotID)
	{
		if (recipeSlotID == m_CurrentClickedRecipeID)
		{
			if (!T17RewiredStandaloneInputModule.IsCurrentActiveModuleUsingController())
			{
				m_CurrentClickedRecipeID = -1;
			}
		}
		else if (CheckHasItemsForRecipe(recipeSlotID))
		{
			CraftManager.Recipe recipeByID = CraftManager.GetInstance().GetRecipeByID(recipeSlotID);
			if (recipeByID != null && (!recipeByID.m_bHidden || recipeByID.m_bDiscovered))
			{
				FillSlots(recipeSlotID);
				m_CurrentClickedRecipeID = recipeSlotID;
			}
			else
			{
				m_CurrentClickedRecipeID = -1;
			}
		}
		else
		{
			m_CurrentClickedRecipeID = -1;
		}
	}

	private bool CheckHasItemsForRecipe(int recipeSlotID)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (instance != null)
		{
			List<CraftManager.Recipe> currentRecipes = instance.GetCurrentRecipes();
			if (CraftManager.GetInstance().HasItemsForRecipe(recipeSlotID, base.CurrentGamer.m_PlayerObject.m_ItemContainer, ref m_IndexArrayForRecipeChecking) || (recipeSlotID < currentRecipes.Count && recipeSlotID >= 0 && CheckPlayerAndMenuHasItems(recipeSlotID)))
			{
				return true;
			}
		}
		return false;
	}

	private void OnRecipeSlotSelected(int recipeSlotID)
	{
		if (recipeSlotID != m_CurrentClickedRecipeID)
		{
			m_CurrentClickedRecipeID = -1;
			m_HoldToCraftTime = 0f;
		}
	}

	private void CraftItemInRecipeSlot(int recipeSlotID)
	{
		if (CheckHasItemsForRecipe(recipeSlotID))
		{
			CraftManager instance = CraftManager.GetInstance();
			if (instance != null)
			{
				ExecuteCraft(recipeSlotID);
			}
		}
	}

	private void ExecuteCraft(int recipeSlotID)
	{
		CraftManager instance = CraftManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		if ((float)instance.GetIntellectRequiredForRecipe(recipeSlotID) > base.CurrentGamePlayer.m_CharacterStats.Intellect)
		{
			SpeechManager.GetInstance().SaySomething(base.CurrentGamePlayer, "Text.Crafting.LowIntellect", SpeechTone.Negative, 2f);
			return;
		}
		Item[] itemsCurrentlyInSlots = m_TemporaryCraftingInventory.GetItemsCurrentlyInSlots(includeNulls: true);
		if (itemsCurrentlyInSlots.Length != 3)
		{
			return;
		}
		Item item = itemsCurrentlyInSlots[0];
		Item item2 = itemsCurrentlyInSlots[1];
		Item item3 = itemsCurrentlyInSlots[2];
		List<Item> list = new List<Item>();
		if (item != null)
		{
			list.Add(item);
		}
		if (item2 != null)
		{
			list.Add(item2);
		}
		if (item3 != null)
		{
			list.Add(item3);
		}
		m_TemporaryCraftingInventory.ClearItemSlots();
		base.CurrentGamePlayer.m_bPendingRequest = true;
		ScoreManager.EventRPC(ScoreManager.Events.ItemCrafted, base.CurrentGamePlayer);
		CraftManager.Recipe recipeByID = instance.GetRecipeByID(recipeSlotID);
		if (recipeByID != null || recipeByID.m_bHidden)
		{
			instance.DiscoverHiddenItem(recipeByID);
		}
		int count = list.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			Item equippedItem = base.CurrentGamePlayer.GetEquippedItem();
			if (equippedItem != null && list[i] == equippedItem)
			{
				array[i] = -1;
				continue;
			}
			int indexOfItem = base.CurrentGamePlayer.m_ItemContainer.GetIndexOfItem(list[i]);
			array[i] = ((indexOfItem < -1) ? (-999) : indexOfItem);
		}
		base.CurrentGamePlayer.m_NetView.RPC("RPC_CraftItem", NetTargets.MasterClient, base.CurrentGamePlayer.m_ItemContainer.GetObjectNetID(), recipeSlotID, array, base.CurrentGamePlayer.m_NetView.viewID);
		OpenRecipePage(m_CurrentOpenPage + 1);
		TutorialManager instance2 = TutorialManager.GetInstance();
		if (instance2 != null)
		{
			instance2.StartTutorialRPC(base.CurrentGamePlayer, TutorialSubject.CraftingTree);
		}
	}

	private void OnItemCrafted(Item itemCrafted, Character crafter)
	{
		OpenRecipePage(m_CurrentOpenPage + 1);
	}

	private void ResetTooltip()
	{
		if (m_TooltipToShow != null)
		{
			m_TooltipToShow.SetLocalScaleSplit();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CameraManager instance = CameraManager.GetInstance();
		if (instance != null)
		{
			instance.OnTargetsUpdated = (CameraManager.CameraManagerHandler)Delegate.Remove(instance.OnTargetsUpdated, new CameraManager.CameraManagerHandler(ResetTooltip));
		}
	}

	private void OnKnockOut(Character thisCharacter, Character otherCharacter)
	{
		m_TemporaryCraftingInventory.ClearItemSlots();
	}
}
