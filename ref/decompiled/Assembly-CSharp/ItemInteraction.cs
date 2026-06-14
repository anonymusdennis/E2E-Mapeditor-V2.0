using System;

public class ItemInteraction : TransferItemsInteraction
{
	[Tooltip("If true, will only accept items if it doesn't aleady have one of that type")]
	public bool m_bUniqueItemsOnly = true;

	private GlobalStart m_GlobalStart;

	private bool m_bItemManagerFinishedLoad;

	protected override void Init()
	{
		base.Init();
		m_GlobalStart = GlobalStart.GetInstance();
		m_bCycleThroughTransferItems = false;
		SetTransferDirection(TransferDirection.ToCharacter);
		ItemContainer itemContainer = m_ItemContainer;
		itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnItemContainerChanged));
		if (m_GlobalStart != null)
		{
			m_GlobalStart.InitManagersCompletedEvent += OnGlobalStart_ItemManagerFinishedLoadEvent;
		}
		else
		{
			m_bItemManagerFinishedLoad = true;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_GlobalStart != null)
		{
			m_GlobalStart.InitManagersCompletedEvent -= OnGlobalStart_ItemManagerFinishedLoadEvent;
			m_GlobalStart = null;
		}
		if (m_ItemContainer != null)
		{
			ItemContainer itemContainer = m_ItemContainer;
			itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Remove(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnItemContainerChanged));
			m_ItemContainer = null;
		}
	}

	private void OnGlobalStart_ItemManagerFinishedLoadEvent()
	{
		m_bItemManagerFinishedLoad = true;
		UpdateState(isFromLoad: true);
	}

	private void OnItemContainerChanged()
	{
		if (m_bItemManagerFinishedLoad)
		{
			UpdateState(isFromLoad: false);
		}
	}

	protected virtual void UpdateState(bool isFromLoad)
	{
	}

	public override bool IsItemTransferable(Item item, TransferDirection directionOverride = TransferDirection.Invalid)
	{
		if (item == null || !base.IsItemTransferable(item, directionOverride))
		{
			return false;
		}
		if (item.GetOwner() != null && item.GetOwner().m_CharacterRole != 0)
		{
			return false;
		}
		switch ((directionOverride != 0) ? directionOverride : GetTransferDirection())
		{
		case TransferDirection.FromCharacter:
			if (m_bUniqueItemsOnly)
			{
				return m_ItemContainer.HasItem(item.ItemDataID) == 0;
			}
			return true;
		case TransferDirection.ToCharacter:
			return m_ItemContainer.HasItem(item.ItemDataID) > 0;
		default:
			return false;
		}
	}

	public virtual void TransferEquippedItem(Character character)
	{
		if (!(character == null) && !(character.GetEquippedItem() == null) && m_ItemContainer.AddItemRPC(character.GetEquippedItem()))
		{
			character.SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (!base.AllowedToInteract(localCharacter))
		{
			return false;
		}
		if (m_ItemContainer == null)
		{
			return false;
		}
		return GetTransferDirection() switch
		{
			TransferDirection.FromCharacter => m_ItemContainer.GetFreeSpaceCount() > 0, 
			TransferDirection.ToCharacter => localCharacter.GetEquippedItem() == null || localCharacter.m_ItemContainer.GetFreeSpaceCount() > 0, 
			_ => false, 
		};
	}

	public override bool CanStartOrContinueInteraction(Character localCharacter)
	{
		if (localCharacter == null || localCharacter.m_ItemContainer == null)
		{
			return false;
		}
		return GetTransferDirection() switch
		{
			TransferDirection.ToCharacter => localCharacter.GetEquippedItem() == null || !localCharacter.m_ItemContainer.IsVisibleFull(), 
			TransferDirection.FromCharacter => !m_ItemContainer.IsVisibleFull(), 
			_ => false, 
		};
	}

	public override void OnCharacterFailedToStart(Character character)
	{
		if (character != null && character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
		{
			TransferDirection transferDirection = GetTransferDirection();
			SpeechManager.GetInstance().SaySomething(character, transferDirection switch
			{
				TransferDirection.ToCharacter => "Text.Player.InventoryFull", 
				TransferDirection.FromCharacter => "Text.Player.InvalidItemDrop", 
				_ => null, 
			}, SpeechTone.Negative, 3f, 10);
		}
	}

	public override bool LeaveCharacterPositionUnAltered()
	{
		return true;
	}
}
