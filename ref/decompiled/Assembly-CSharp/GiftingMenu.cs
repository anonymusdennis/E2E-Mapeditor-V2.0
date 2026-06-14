using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class GiftingMenu : GameMenuBehaviour
{
	public InventoryItem m_GiftSlot;

	public T17CounterObject m_GiftCounter;

	public T17Text m_OpinionLabel;

	public T17Text m_ReceiverLabel;

	public T17RawImage m_Avatar;

	public UI_DrawCharacterToRenderTexture m_CharacterRenderer;

	public T17Image m_AvatarSprite;

	public Sprite m_OverRideSprite;

	public UIAnimatedCoinTicker m_CoinTicker;

	private RenderTexture m_RenderTexture;

	private int m_RenderTextureID;

	private Character m_Gifter;

	private ItemContainer m_GifterContainer;

	private Character m_Receiver;

	private List<Item> m_ItemsToGift = new List<Item>();

	protected override void Awake()
	{
		base.Awake();
		if (m_CoinTicker == null)
		{
			m_CoinTicker = GetComponentInChildren<UIAnimatedCoinTicker>(includeInactive: true);
		}
	}

	protected override void Start()
	{
		base.Start();
		if (m_OpinionLabel != null)
		{
			m_OpinionLabel.m_bNeedsLocalization = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.SetAndDrawCharacter(m_Receiver.m_CharacterCustomisation, m_RenderTexture, Time.deltaTime);
		}
		if (m_OpinionLabel != null)
		{
			m_OpinionLabel.text = m_Receiver.GetOpinionOf(m_Gifter).ToString();
		}
		if (!(m_Gifter != null))
		{
			return;
		}
		int num = Mathf.RoundToInt(m_Gifter.m_CharacterStats.Money);
		if (m_GiftCounter != null && m_GiftCounter.m_MaxValue != num)
		{
			m_GiftCounter.SetMaxValue(num);
			if (m_CoinTicker != null)
			{
				m_CoinTicker.TickToNewValue(m_Gifter.m_CharacterStats.Money);
			}
		}
	}

	public override bool Show(Gamer currentGamer, BaseMenuBehaviour parent, GameObject invoker, bool hideInvoker = true)
	{
		if (!base.Show(currentGamer, parent, invoker, hideInvoker))
		{
			return false;
		}
		SetGifter(m_GameMenuInformation.m_Player);
		SetGifterContainer(m_GameMenuInformation.m_PlayerItemContainer);
		SetReceiver(m_GameMenuInformation.m_MenuRepresentative);
		if (m_GiftSlot != null && m_GiftSlot.GetLinkedItem() == null)
		{
			m_GiftSlot.StopSmoke();
		}
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemClickToCallback();
			m_GameMenuInformation.m_PlayerInventoryBehaviour.SetItemContainerLinks(m_GameMenuInformation.m_PlayerItemContainer, m_GameMenuInformation.m_MenuRepresentativeContainer, m_GameMenuInformation.m_Player);
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
			BaseInventoryBehaviour playerInventoryBehaviour2 = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour2.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Combine(playerInventoryBehaviour2.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
		}
		if (m_CharacterRenderer != null)
		{
			m_CharacterRenderer.RegisterActivationRequest(base.gameObject);
			SetupAvatarRenderTexture();
		}
		if (m_GiftCounter != null)
		{
			m_GiftCounter.SetMaxValue(Mathf.RoundToInt(m_Gifter.m_CharacterStats.Money));
		}
		if (m_CoinTicker != null)
		{
			m_CoinTicker.SetPlayer(m_GameMenuInformation.m_Player);
			m_CoinTicker.Reset();
		}
		return true;
	}

	public override bool Hide(bool restoreInvokerState = true, bool isTabSwitch = false)
	{
		if (m_GameMenuInformation.m_PlayerInventoryBehaviour != null)
		{
			BaseInventoryBehaviour playerInventoryBehaviour = m_GameMenuInformation.m_PlayerInventoryBehaviour;
			playerInventoryBehaviour.OnItemClickedEvent = (BaseInventoryBehaviour.InventoryElementEvent)Delegate.Remove(playerInventoryBehaviour.OnItemClickedEvent, new BaseInventoryBehaviour.InventoryElementEvent(OnInventoryItemClicked));
		}
		if (m_CharacterRenderer != null)
		{
			DestroyAvatarRenderTexture();
			m_CharacterRenderer.UnregisterActivationRequest(base.gameObject);
		}
		if (m_CoinTicker != null)
		{
			m_CoinTicker.Reset();
		}
		ResetGiftSlot();
		m_ItemsToGift.Clear();
		return base.Hide(restoreInvokerState, isTabSwitch);
	}

	public void OnInventoryItemClicked(ItemContainer container, int indexOfItem)
	{
		if (!(container == m_GifterContainer) || !(m_GiftSlot != null))
		{
			return;
		}
		Character characterOwner = m_GifterContainer.GetCharacterOwner();
		Item item = null;
		if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && indexOfItem == -1)
		{
			item = characterOwner.GetEquippedItem();
		}
		if (item == null)
		{
			item = m_GifterContainer.GetItem(indexOfItem);
		}
		if (item != null)
		{
			m_GiftSlot.SetItemContentImage(item.m_ItemData.m_ItemUIImage);
			m_GiftSlot.SetItem(item);
			if (m_GiftSlot.InteractableElement != null)
			{
				m_GiftSlot.InteractableElement.onClick.RemoveAllListeners();
				m_GiftSlot.InteractableElement.onClick.AddListener(delegate
				{
					GiftSlotClicked();
				});
			}
			m_GiftSlot.StartSmoke();
		}
		else
		{
			ResetGiftSlot();
		}
	}

	public void SetGifter(Character character)
	{
		m_Gifter = character;
	}

	public void SetGifterContainer(ItemContainer cont)
	{
		m_GifterContainer = cont;
	}

	public void SetReceiver(Character character)
	{
		m_Receiver = character;
		if (m_ReceiverLabel != null)
		{
			m_ReceiverLabel.m_bNeedsLocalization = false;
			m_ReceiverLabel.text = m_Receiver.m_CharacterCustomisation.m_DisplayName;
		}
	}

	public void GiftItems()
	{
		if ((m_GiftSlot.GetLinkedItem() == null && m_GiftCounter.GetCounterValue() <= 0) || !(m_Receiver != null) || !(m_Gifter != null) || !(m_GifterContainer != null))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (m_GiftSlot.GetLinkedItem() != null)
		{
			flag = true;
			Character characterOwner = m_GifterContainer.GetCharacterOwner();
			if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && m_GiftSlot.GetLinkedItem() == characterOwner.GetEquippedItem())
			{
				flag2 = true;
				m_ItemsToGift.Clear();
				m_ItemsToGift.Add(characterOwner.GetEquippedItem());
			}
			else
			{
				m_GifterContainer.GetItemsWithItemID(ref m_ItemsToGift, m_GiftSlot.GetLinkedItem().m_ItemData.m_ItemDataID, 1);
			}
		}
		else
		{
			m_ItemsToGift.Clear();
		}
		if (m_Receiver.m_CharacterRole == CharacterRole.Guard)
		{
			if (flag2)
			{
				if (!m_Gifter.GetEquippedItem().m_ItemData.m_bCanBeGiftedToGuards)
				{
					ResetForGiftingContrabandItem();
					return;
				}
			}
			else
			{
				for (int num = m_ItemsToGift.Count - 1; num >= 0; num--)
				{
					if (!m_ItemsToGift[num].m_ItemData.m_bCanBeGiftedToGuards)
					{
						ResetForGiftingContrabandItem();
						return;
					}
				}
			}
		}
		if (!flag2)
		{
			ItemManager.GetInstance().GiftItemsRPC(m_Gifter, m_Receiver, m_GifterContainer, m_ItemsToGift, m_GiftCounter.GetCounterValue());
		}
		else
		{
			ItemManager.GetInstance().GiftEquipedItemRPC(m_Gifter, m_Receiver, m_GiftCounter.GetCounterValue());
		}
		ResetGiftSlot();
		if (flag)
		{
			m_GiftSlot.StartSmoke();
		}
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Give_Item_Accept, base.gameObject);
	}

	private void ResetForGiftingContrabandItem()
	{
		SpeechManager.GetInstance().SaySomething(m_Gifter, "Text.Player.TryGiftContrabandToGuard", SpeechTone.Negative, 3f);
		ResetGiftSlot();
		m_ItemsToGift.Clear();
	}

	public void GiftSlotClicked()
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Take_Item, base.gameObject);
		ResetGiftSlot();
	}

	private void ResetGiftSlot()
	{
		m_GiftSlot.SetItemContentImage(null, autoClearIfNull: true, autoDisableIfNull: false);
		m_GiftSlot.SetItem(null);
		if (m_GiftSlot.InteractableElement != null)
		{
			m_GiftSlot.InteractableElement.onClick.RemoveAllListeners();
		}
		if (m_GiftCounter != null)
		{
			m_GiftCounter.Reset();
		}
	}

	private void SetupAvatarRenderTexture()
	{
		DestroyAvatarRenderTexture();
		if (!(m_Avatar != null) || !(m_CharacterRenderer != null))
		{
			return;
		}
		T17RawImage componentInChildren = m_Avatar.GetComponentInChildren<T17RawImage>();
		CharacterRole characterRole = m_GameMenuInformation.m_MenuRepresentative.m_CharacterRole;
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
}
