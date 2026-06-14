using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

[RequireComponent(typeof(ItemContainer))]
public class TransferItemsInteraction : AnimatedInteraction
{
	public enum TransferDirection
	{
		Invalid,
		ToCharacter,
		FromCharacter
	}

	public delegate void TransferCompleteDelegate(Item item, ItemContainer to, ItemContainer from);

	public bool m_bTransferEquippedItemsOnly;

	public SpeechPODO m_NoItemsLeftSpeech;

	private TransferDirection m_TransferDirection;

	private List<ItemData> m_TransferItemTypes;

	private int m_TransferItemIndex;

	protected bool m_bCycleThroughTransferItems = true;

	private bool m_bStopInteractionNextUpdate;

	protected ItemContainer m_ItemContainer;

	public event TransferCompleteDelegate m_OnTransferComplete;

	protected override void Awake()
	{
		base.Awake();
		m_ItemContainer = GetComponent<ItemContainer>();
	}

	public override InteractionType GetInteractionClassType()
	{
		return InteractionType.InteractiveObject;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		PostOnStartInteraction(localCharacter);
	}

	protected virtual void PostOnStartInteraction(Character localCharacter)
	{
		DoItemTransferAndRequestStop(localCharacter);
	}

	protected void DoItemTransferAndRequestStop(Character localCharacter)
	{
		if (m_ItemContainer != null)
		{
			localCharacter.m_OpenContainer = m_ItemContainer;
		}
		if (m_interactingCharacter != null && m_TransferDirection != 0)
		{
			bool flag = false;
			bool bIsPlayer = m_interactingCharacter.m_CharacterStats.m_bIsPlayer;
			Item equippedItem = m_interactingCharacter.GetEquippedItem();
			if (m_TransferDirection == TransferDirection.FromCharacter && IsItemTransferable(equippedItem))
			{
				if (m_ItemContainer.AddItemRPC(equippedItem))
				{
					m_interactingCharacter.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
					if (bIsPlayer)
					{
						Player player = m_interactingCharacter as Player;
						if (player != null)
						{
							HUDMenuFlow.Instance.GetPlayerInventoryHUD(player.m_PlayerCameraManagerBindingID).RefreshAllSlotsWithCurrentContainer();
						}
					}
					flag = true;
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item, m_interactingCharacter.gameObject);
					OnTransferComplete(equippedItem, m_ItemContainer, null);
				}
			}
			else if (m_TransferDirection == TransferDirection.FromCharacter && !IsItemTransferable(equippedItem) && m_bTransferEquippedItemsOnly && bIsPlayer)
			{
				if (m_TransferItemTypes != null && m_TransferItemTypes.Count > 0 && m_TransferItemTypes[0] != null)
				{
					PerformNoEquippedItemSpeech(m_interactingCharacter);
				}
			}
			else if (!m_bTransferEquippedItemsOnly || !bIsPlayer)
			{
				ItemContainer itemContainer = null;
				ItemContainer itemContainer2 = null;
				switch (m_TransferDirection)
				{
				case TransferDirection.ToCharacter:
					itemContainer = m_ItemContainer;
					itemContainer2 = m_interactingCharacter.m_ItemContainer;
					break;
				case TransferDirection.FromCharacter:
					itemContainer = m_interactingCharacter.m_ItemContainer;
					itemContainer2 = m_ItemContainer;
					break;
				}
				if (itemContainer != null && itemContainer2 != null)
				{
					Item item = FindItemToTransfer(itemContainer);
					if (item != null)
					{
						Character characterOwner = itemContainer2.GetCharacterOwner();
						if (characterOwner != null && characterOwner.m_CharacterStats.m_bIsPlayer && characterOwner.GetEquippedItem() == null && characterOwner.CanEquipItem(item))
						{
							itemContainer.MoveItemToCharacterEquipedSlot(item.m_NetView.viewID, characterOwner.m_NetView.viewID);
							flag = true;
						}
						if (!flag && itemContainer2.AddItemRPC(item))
						{
							itemContainer.RemoveItemRPC(item);
							flag = true;
						}
						if (flag)
						{
							AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item, m_interactingCharacter.gameObject);
							OnTransferComplete(item, itemContainer2, itemContainer);
						}
					}
				}
			}
			if (!flag)
			{
				OnTransferFailed();
			}
		}
		m_bStopInteractionNextUpdate = true;
	}

	protected virtual void PerformNoEquippedItemSpeech(Character character)
	{
		ItemData itemData = m_TransferItemTypes[0];
		List<SpeechManager.Token> list = new List<SpeechManager.Token>();
		list.Add(new SpeechManager.Token("$ItemToGet", itemData.m_ItemLocalizationTag, bIsCharacterNetviewID: false));
		List<SpeechManager.Token> tokens = list;
		SpeechManager.GetInstance().SaySomething(character, "Text.Emote.ItemNotEquipped", tokens, SpeechTone.Positive);
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_bStopInteractionNextUpdate)
		{
			RequestStopInteraction(m_interactingCharacter);
			m_bStopInteractionNextUpdate = false;
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter && localCharacter.m_OpenContainer == m_ItemContainer)
		{
			localCharacter.m_OpenContainer = null;
		}
	}

	public void SetTransferDirection(TransferDirection direction)
	{
		m_TransferDirection = direction;
	}

	public void SetTransferItemTypes(ItemData[] items)
	{
		m_TransferItemIndex = 0;
		m_TransferItemTypes = null;
		if (items == null || items.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] != null)
			{
				if (m_TransferItemTypes == null)
				{
					m_TransferItemTypes = new List<ItemData>();
				}
				m_TransferItemTypes.Add(items[i]);
			}
		}
	}

	public List<ItemData> GetTransferItemTypes()
	{
		return m_TransferItemTypes;
	}

	protected virtual void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		if (this.m_OnTransferComplete != null)
		{
			this.m_OnTransferComplete(item, to, from);
		}
	}

	protected virtual void OnTransferFailed()
	{
		if (m_interactingCharacter != null && m_TransferDirection == TransferDirection.ToCharacter)
		{
			bool flag = false;
			if ((!m_bTransferEquippedItemsOnly) ? (FindItemToTransfer(m_ItemContainer) == null) : (!IsItemTransferable(m_interactingCharacter.GetEquippedItem())))
			{
				SpeechManager.GetInstance().SaySomething(m_interactingCharacter, m_NoItemsLeftSpeech);
			}
		}
	}

	private Item FindItemToTransfer(ItemContainer container)
	{
		if (m_TransferItemTypes == null || m_TransferItemTypes.Count <= 0)
		{
			return null;
		}
		if (container != null)
		{
			Item item = null;
			if (!m_bCycleThroughTransferItems)
			{
				m_TransferItemIndex = 0;
			}
			for (int i = 0; i < m_TransferItemTypes.Count; i++)
			{
				if (!(item == null))
				{
					break;
				}
				item = container.GetFirstItemWithItemID(m_TransferItemTypes[m_TransferItemIndex].m_ItemDataID);
				if (++m_TransferItemIndex >= m_TransferItemTypes.Count)
				{
					m_TransferItemIndex = 0;
				}
			}
			return item;
		}
		return null;
	}

	public virtual bool IsItemTransferable(Item item, TransferDirection directionOverride = TransferDirection.Invalid)
	{
		if (item != null && m_TransferItemTypes != null)
		{
			for (int i = 0; i < m_TransferItemTypes.Count; i++)
			{
				if (item.ItemDataID == m_TransferItemTypes[i].m_ItemDataID)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public TransferDirection GetTransferDirection()
	{
		return m_TransferDirection;
	}

	public ItemContainer GetItemContainer()
	{
		return m_ItemContainer;
	}
}
