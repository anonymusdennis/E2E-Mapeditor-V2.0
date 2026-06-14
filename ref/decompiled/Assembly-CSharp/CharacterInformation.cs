using System;
using UnityEngine;

public class CharacterInformation : GameMenuBehaviour
{
	public T17RawImage m_Avatar;

	public T17Text m_NameLabel;

	public T17Text m_JobTitle;

	public T17Text m_TimeServedLabel;

	public T17Text m_TimeServedTitle;

	public T17Text m_MoneyLabel;

	public T17InfoObject m_StrengthStat;

	public T17InfoObject m_CardioStat;

	public T17InfoObject m_IntellectStat;

	public InventoryItem m_OutfitItem;

	public InventoryItem m_EquipedItem;

	public T17InfoObject m_HealthStat;

	public T17InfoObject m_StaminaStat;

	public T17InfoObject m_HeatStat;

	public T17InfoObject m_OpinionStat;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17Image m_AvatarSprite;

	public Sprite m_OverRideSprite;

	[Header("Equipped Item tooltips")]
	public InventoryItem.ContainerSetup m_EquippedItems_Takeable;

	public InventoryItem.ContainerSetup m_EquippedItems_NotTakeable;

	public InventoryItem.ContainerSetup m_CantEquipItemsSetup;

	public InventoryItem.ContainerSetup m_CanEquipItemsSetup;

	private Character m_CharacterToMonitor;

	private Character m_OpinionTarget;

	private ItemContainer m_ItemRemovalContainer;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private bool m_EquipAndOutFitCallbacksSetup;

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		m_EquipAndOutFitCallbacksSetup = false;
		SetCharacterInfo(m_GameMenuInformation.m_MenuRepresentative, m_GameMenuInformation.m_Player);
		SetItemRemovalContainer(m_GameMenuInformation.m_PlayerItemContainer);
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_Player);
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToCallback();
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
			BaseInventoryBehaviour playerInventoryBehaviour2 = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour2.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Combine(playerInventoryBehaviour2.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
		}
		m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
		SetupAvatarRenderTexture();
		UpdateUIForMonitoredCharacter();
		m_CharacterRenderer.SetAndDrawCharacter(m_CharacterToMonitor.m_CharacterCustomisation, m_RenderTexture, Time.deltaTime);
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (base.Hide(restoreInvokerState, isTabSwitch))
		{
			if (m_CharacterToMonitor != null)
			{
				Character characterToMonitor = m_CharacterToMonitor;
				characterToMonitor.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Remove(characterToMonitor.OnEquipedItemChanged, new Character.CharacterEvent(RefreshEquipedItemSlot));
				Character characterToMonitor2 = m_CharacterToMonitor;
				characterToMonitor2.OnOutfitChanged = (Character.CharacterEvent)Delegate.Remove(characterToMonitor2.OnOutfitChanged, new Character.CharacterEvent(RefreshOutfitSlot));
			}
			if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
			{
				BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
				playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
			}
			DestroyAvatarRenderTexture();
			m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
			m_CharacterToMonitor = null;
			return true;
		}
		return false;
	}

	protected override void Update()
	{
		base.Update();
		if (m_CharacterToMonitor != null)
		{
			UpdateUIForMonitoredCharacter();
			m_CharacterRenderer.SetAndDrawCharacter(m_CharacterToMonitor.m_CharacterCustomisation, m_RenderTexture, Time.deltaTime);
		}
	}

	public override void SetGamePlayer(Player gamePlayer)
	{
		base.SetGamePlayer(gamePlayer);
		if (gamePlayer != null && m_MenuType == InGameMenuTypes.SelfCharacterInfo)
		{
			SetCharacterInfo(gamePlayer);
			SetItemRemovalContainer(gamePlayer.m_ItemContainer);
		}
	}

	public void SetCharacterInfo(Character character, Character opinionTarget = null)
	{
		m_CharacterToMonitor = character;
		m_OpinionTarget = opinionTarget;
		if (m_NameLabel != null && m_CharacterToMonitor != null && m_CharacterToMonitor.m_CharacterCustomisation != null)
		{
			m_NameLabel.m_bNeedsLocalization = false;
			m_NameLabel.text = m_CharacterToMonitor.m_CharacterCustomisation.m_DisplayName;
		}
		UpdateUIForMonitoredCharacter();
	}

	private void UpdateUIForMonitoredCharacter()
	{
		if (!(m_CharacterToMonitor != null))
		{
			return;
		}
		if (!m_EquipAndOutFitCallbacksSetup)
		{
			RefreshOutfitSlot();
			RefreshEquipedItemSlot();
			Character characterToMonitor = m_CharacterToMonitor;
			characterToMonitor.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Remove(characterToMonitor.OnEquipedItemChanged, new Character.CharacterEvent(RefreshEquipedItemSlot));
			Character characterToMonitor2 = m_CharacterToMonitor;
			characterToMonitor2.OnEquipedItemChanged = (Character.CharacterEvent)Delegate.Combine(characterToMonitor2.OnEquipedItemChanged, new Character.CharacterEvent(RefreshEquipedItemSlot));
			Character characterToMonitor3 = m_CharacterToMonitor;
			characterToMonitor3.OnOutfitChanged = (Character.CharacterEvent)Delegate.Remove(characterToMonitor3.OnOutfitChanged, new Character.CharacterEvent(RefreshOutfitSlot));
			Character characterToMonitor4 = m_CharacterToMonitor;
			characterToMonitor4.OnOutfitChanged = (Character.CharacterEvent)Delegate.Combine(characterToMonitor4.OnOutfitChanged, new Character.CharacterEvent(RefreshOutfitSlot));
			m_EquipAndOutFitCallbacksSetup = true;
		}
		if (!(m_CharacterToMonitor.m_CharacterStats != null))
		{
			return;
		}
		if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.Dolphin || m_CharacterToMonitor.m_CharacterRole == CharacterRole.SpecificQuestGiver)
		{
			if (m_StrengthStat != null)
			{
				m_StrengthStat.SetValues(100, 100f);
			}
			if (m_CardioStat != null)
			{
				m_CardioStat.SetValues(100, 100f);
			}
			if (m_IntellectStat != null)
			{
				m_IntellectStat.SetValues(100, 100f);
			}
			if (m_HealthStat != null)
			{
				m_HealthStat.SetValues(100, 100f);
			}
			if (m_StaminaStat != null)
			{
				m_StaminaStat.SetValues(100, 100f);
			}
			if (m_HeatStat != null)
			{
				m_HeatStat.SetValues(0, 100f);
			}
			if (m_MoneyLabel != null)
			{
				m_MoneyLabel.text = string.Empty + 0;
			}
		}
		else
		{
			if (m_StrengthStat != null)
			{
				m_StrengthStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Strength), 100f);
			}
			if (m_CardioStat != null)
			{
				m_CardioStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Cardio), 100f);
			}
			if (m_IntellectStat != null)
			{
				m_IntellectStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Intellect), 100f);
			}
			if (m_HealthStat != null)
			{
				m_HealthStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Health), 100f);
			}
			if (m_StaminaStat != null)
			{
				m_StaminaStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Energy), 100f);
			}
			if (m_HeatStat != null)
			{
				m_HeatStat.SetValues(Mathf.CeilToInt(m_CharacterToMonitor.m_CharacterStats.Heat), 100f);
			}
			if (m_MoneyLabel != null)
			{
				m_MoneyLabel.text = Mathf.FloorToInt(m_CharacterToMonitor.m_CharacterStats.Money).ToString();
			}
		}
		AICharacter_SpecificQuestGiver component = m_CharacterToMonitor.GetComponent<AICharacter_SpecificQuestGiver>();
		if (m_TimeServedTitle != null)
		{
			if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.Inmate)
			{
				m_TimeServedTitle.SetLocalisedTextCatchAll("Text.Stats.TimeServed");
			}
			else if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.Dolphin)
			{
				m_TimeServedTitle.SetLocalisedTextCatchAll("Text.Stats.DolphinDaysWorked");
			}
			else if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.SpecificQuestGiver && component != null)
			{
				m_TimeServedTitle.SetLocalisedTextCatchAll(component.m_TimeServedLabelString);
			}
			else
			{
				m_TimeServedTitle.SetLocalisedTextCatchAll("Text.Stats.DaysWorked");
			}
		}
		if (m_TimeServedLabel != null)
		{
			m_TimeServedLabel.m_bNeedsLocalization = false;
			string localised;
			if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.Dolphin)
			{
				localised = string.Empty + (1973 + 23 * RoutineManager.GetInstance().GetDaysElapsed());
			}
			else if (m_CharacterToMonitor.m_CharacterRole == CharacterRole.SpecificQuestGiver && component != null)
			{
				localised = component.GetLocalisedDaysServedString();
			}
			else
			{
				int num = (int)m_CharacterToMonitor.m_CharacterStats.m_SentenceBaseLine - m_CharacterToMonitor.m_CharacterStats.RemainingSentence;
				int value = num + 1;
				Localization.GetWithKeySwap("Text.Format.TimeServed", out localised, "$days", value);
			}
			m_TimeServedLabel.SetNewLocalizationTag(localised);
		}
		if (m_JobTitle != null)
		{
			string localisedTextCatchAll = "Text.Game.Roles.NoJob";
			switch (m_CharacterToMonitor.m_CharacterRole)
			{
			case CharacterRole.Dog:
				localisedTextCatchAll = "Text.Game.Roles.Dog";
				break;
			case CharacterRole.Guard:
				localisedTextCatchAll = "Text.Game.Roles.Guard";
				break;
			case CharacterRole.JobGuy:
				localisedTextCatchAll = "Text.Game.Roles.JobGuy";
				break;
			case CharacterRole.Maintenance:
				localisedTextCatchAll = "Text.Game.Roles.MaintenanceMan";
				break;
			case CharacterRole.Medic:
				localisedTextCatchAll = "Text.Game.Roles.Medic";
				break;
			case CharacterRole.Visitor:
				localisedTextCatchAll = "Text.Game.Roles.Visitor";
				break;
			case CharacterRole.Inmate:
			{
				BaseJob baseJob = null;
				if (JobsManager.GetInstance() != null)
				{
					baseJob = JobsManager.GetInstance().GetCharactersJob(m_CharacterToMonitor);
				}
				if (baseJob != null && baseJob.m_Info != null)
				{
					localisedTextCatchAll = baseJob.m_Info.m_NameTextID;
				}
				break;
			}
			case CharacterRole.Dolphin:
				localisedTextCatchAll = "Text.Stats.DolphinJob";
				break;
			case CharacterRole.SpecificQuestGiver:
			{
				AICharacter_SpecificQuestGiver component2 = m_CharacterToMonitor.GetComponent<AICharacter_SpecificQuestGiver>();
				if (component2 != null)
				{
					localisedTextCatchAll = component2.m_JobString;
				}
				break;
			}
			}
			m_JobTitle.SetLocalisedTextCatchAll(localisedTextCatchAll);
		}
		if (m_MenuType != InGameMenuTypes.SelfCharacterInfo && m_OpinionStat != null && m_OpinionTarget != null)
		{
			m_OpinionStat.SetValue(Mathf.CeilToInt(m_CharacterToMonitor.GetOpinionOf(m_OpinionTarget)).ToString());
		}
		InventoryItem.ContainerSetup setup = m_EquippedItems_NotTakeable;
		InventoryItem.ContainerSetup inventoryItemSetups = m_CantEquipItemsSetup;
		if (m_MenuType == InGameMenuTypes.SelfCharacterInfo || m_CharacterToMonitor.GetIsKnockedOut())
		{
			setup = m_EquippedItems_Takeable;
			inventoryItemSetups = m_CanEquipItemsSetup;
		}
		if (m_OutfitItem != null)
		{
			m_OutfitItem.ChangeDisplaySetup(setup);
		}
		if (m_EquipedItem != null)
		{
			m_EquipedItem.ChangeDisplaySetup(setup);
		}
		if (m_GameMenuInformation.m_PlayerInventoryMenu != null)
		{
			m_GameMenuInformation.m_PlayerInventoryMenu.SetInventoryItemSetups(inventoryItemSetups);
		}
	}

	public void SetItemRemovalContainer(ItemContainer container)
	{
		m_ItemRemovalContainer = container;
	}

	private void SetupAvatarRenderTexture()
	{
		DestroyAvatarRenderTexture();
		if (!(m_Avatar != null) || !(m_CharacterRenderer != null))
		{
			return;
		}
		T17RawImage componentInChildren = m_Avatar.GetComponentInChildren<T17RawImage>();
		CharacterRole characterRole = m_CharacterToMonitor.m_CharacterRole;
		if (characterRole == CharacterRole.Dolphin)
		{
			if (m_OverRideSprite != null && m_AvatarSprite != null)
			{
				m_AvatarSprite.enabled = true;
				m_Avatar.enabled = false;
				m_AvatarSprite.sprite = m_OverRideSprite;
			}
			return;
		}
		if (m_AvatarSprite != null)
		{
			m_AvatarSprite.enabled = false;
		}
		m_Avatar.enabled = true;
		int num = Mathf.FloorToInt(componentInChildren.rectTransform.rect.width);
		int num2 = Mathf.FloorToInt(componentInChildren.rectTransform.rect.height);
		if (num > 0 && num2 > 0)
		{
			m_RenderTexture = m_CharacterRenderer.CreateRenderTexture(num, num2, ref m_RenderTextureID);
		}
		componentInChildren.texture = m_RenderTexture;
	}

	private void DestroyAvatarRenderTexture()
	{
		if (m_RenderTexture != null)
		{
			m_CharacterRenderer.CleanupRenderTexture(ref m_RenderTextureID);
			m_RenderTexture = null;
		}
		if (m_Avatar != null)
		{
			m_Avatar.texture = null;
		}
	}

	private void RefreshOutfitSlot()
	{
		if (m_CharacterToMonitor == null)
		{
			return;
		}
		Item outFit = m_CharacterToMonitor.GetOutFit();
		if (m_OutfitItem != null && outFit != null && outFit.m_ItemData != null)
		{
			m_OutfitItem.SetItem(outFit);
			m_OutfitItem.SetItemContentImage(outFit.m_ItemData.m_ItemUIImage, autoClearIfNull: true, autoDisableIfNull: false);
			if (m_OutfitItem.InteractableElement != null && m_OutfitItem.InteractableElement.onClick != null)
			{
				m_OutfitItem.InteractableElement.onClick.RemoveAllListeners();
				m_OutfitItem.InteractableElement.onClick.AddListener(delegate
				{
					OnOutfitSlotClicked();
				});
			}
		}
		else if (m_OutfitItem != null)
		{
			m_OutfitItem.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
		}
	}

	private void RefreshEquipedItemSlot()
	{
		if (m_CharacterToMonitor == null)
		{
			return;
		}
		Item equippedItem = m_CharacterToMonitor.GetEquippedItem();
		if (m_EquipedItem != null && equippedItem != null && equippedItem.m_ItemData != null)
		{
			m_EquipedItem.SetItemContentImage(equippedItem.m_ItemData.m_ItemUIImage, autoClearIfNull: true, autoDisableIfNull: false);
			m_EquipedItem.SetItem(equippedItem);
			if (m_EquipedItem.InteractableElement != null && m_EquipedItem.InteractableElement.onClick != null)
			{
				m_EquipedItem.InteractableElement.onClick.RemoveAllListeners();
				m_EquipedItem.InteractableElement.onClick.AddListener(delegate
				{
					OnEquipedItemSlotClicked();
				});
			}
		}
		else if (m_EquipedItem != null)
		{
			m_EquipedItem.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
		}
	}

	private void OnOutfitSlotClicked()
	{
		if (m_CharacterToMonitor == null || m_ItemRemovalContainer == null || (m_MenuType != InGameMenuTypes.SelfCharacterInfo && !m_CharacterToMonitor.GetIsKnockedOut()))
		{
			return;
		}
		Item outFit = m_CharacterToMonitor.GetOutFit();
		if (outFit != null)
		{
			if (m_ItemRemovalContainer.AddItemRPC(outFit))
			{
				m_CharacterToMonitor.SetOutFit(null, bTellOthers: true, bAddOldToInventory: false);
			}
			RefreshOutfitSlot();
		}
	}

	private void OnEquipedItemSlotClicked()
	{
		if (m_CharacterToMonitor == null || m_ItemRemovalContainer == null || (m_MenuType != InGameMenuTypes.SelfCharacterInfo && !m_CharacterToMonitor.GetIsKnockedOut()))
		{
			return;
		}
		Item equippedItem = m_CharacterToMonitor.GetEquippedItem();
		bool flag = equippedItem != null;
		if (flag && m_MenuType == InGameMenuTypes.SelfCharacterInfo)
		{
			flag = equippedItem.CanBeSwitchedOut();
		}
		if (flag)
		{
			if (m_ItemRemovalContainer.AddItemRPC(equippedItem))
			{
				m_CharacterToMonitor.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
			}
			RefreshEquipedItemSlot();
		}
	}

	public void OnInventoryItemClicked(ItemContainer container, int indexOfItem)
	{
		if (container == null || indexOfItem < -1)
		{
			return;
		}
		Item item = null;
		if (indexOfItem >= 0)
		{
			item = container.GetItem(indexOfItem);
		}
		else
		{
			Character characterOwner = container.GetCharacterOwner();
			if (characterOwner != null)
			{
				item = characterOwner.GetEquippedItem();
			}
		}
		if (item == null || item.m_ItemData == null || !item.m_ItemData.m_CanBeEquiped || !(m_CharacterToMonitor != null) || !(m_CharacterToMonitor.m_ItemContainer != null))
		{
			return;
		}
		bool flag = false;
		bool flag2 = indexOfItem >= 0;
		if (m_CharacterToMonitor.m_CharacterStats != null && m_CharacterToMonitor.m_CharacterStats.m_bIsPlayer)
		{
			if (m_CharacterToMonitor == base.CurrentGamePlayer)
			{
				flag = true;
			}
		}
		else if (m_CharacterToMonitor.GetIsKnockedOut())
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (flag2)
		{
			bool flag3 = false;
			if ((!(item.OutfitData != null)) ? m_CharacterToMonitor.SetEquippedItem(item) : m_CharacterToMonitor.SetOutFit(item))
			{
				container.RemoveItemRPC(item);
			}
			return;
		}
		bool flag4 = m_CharacterToMonitor.m_ItemContainer.GetFreeSpaceCount() > 0;
		Item equippedItem = m_CharacterToMonitor.GetEquippedItem();
		if (flag4)
		{
			m_CharacterToMonitor.SetEquippedItem(null);
		}
		else if (equippedItem.OutfitData != null)
		{
			m_CharacterToMonitor.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
			Item outFit = m_CharacterToMonitor.GetOutFit();
			m_CharacterToMonitor.SetOutFit(equippedItem, bTellOthers: true, bAddOldToInventory: false);
			m_CharacterToMonitor.SetEquippedItem(outFit, bTellOthers: true, bAddOldToInventory: false);
		}
	}
}
