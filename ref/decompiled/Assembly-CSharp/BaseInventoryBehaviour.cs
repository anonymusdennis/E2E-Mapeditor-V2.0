using AUTOGEN_T17Wwise_Enums;
using UnityEngine;

public class BaseInventoryBehaviour : MonoBehaviour
{
	public delegate void InventoryElementEvent(ItemContainer container, int indexOfItem);

	private enum ItemClickAction
	{
		SwapItemWithAnotherInventory,
		CallbackClickEvent,
		DoNothing,
		Ignore
	}

	private ItemContainer m_ItemContainer;

	private ItemContainer m_OtherItemContainer;

	private Player m_InteractingPlayer;

	public InventoryElementEvent OnItemClickedEvent;

	private ItemClickAction m_CurrentItemClickAction;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
	}

	public void SetItemClickToSwitchIventory()
	{
		m_CurrentItemClickAction = ItemClickAction.SwapItemWithAnotherInventory;
	}

	public void SetItemClickToCallback()
	{
		m_CurrentItemClickAction = ItemClickAction.CallbackClickEvent;
	}

	public void SetItemClickToNothing()
	{
		m_CurrentItemClickAction = ItemClickAction.DoNothing;
	}

	public void SetItemClickToIgnore()
	{
		m_CurrentItemClickAction = ItemClickAction.Ignore;
	}

	public void SetItemContainerLinks(ItemContainer sourceIC, ItemContainer otherIC, Player interactingPlayer)
	{
		m_ItemContainer = sourceIC;
		m_OtherItemContainer = otherIC;
		m_InteractingPlayer = interactingPlayer;
	}

	public bool OnEquipedItemClicked()
	{
		switch (m_CurrentItemClickAction)
		{
		case ItemClickAction.SwapItemWithAnotherInventory:
		{
			if (!(m_InteractingPlayer != null) || !(m_ItemContainer != null) || !(m_OtherItemContainer != null) || !(m_ItemContainer != m_OtherItemContainer))
			{
				break;
			}
			Item equippedItem = m_InteractingPlayer.GetEquippedItem();
			if (equippedItem != null)
			{
				if (m_ItemContainer == m_InteractingPlayer.m_ItemContainer)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Give_Item, AudioController.UI_Audio_GO);
				}
				m_InteractingPlayer.m_NetView.RPC("RPC_MASTER_PutEquipedItemIntoContainer", NetTargets.MasterClient, m_InteractingPlayer.m_NetView.viewID, m_OtherItemContainer.GetObjectNetID(), false);
				return true;
			}
			break;
		}
		case ItemClickAction.CallbackClickEvent:
			if (OnItemClickedEvent != null)
			{
				OnItemClickedEvent(m_ItemContainer, -1);
				return true;
			}
			break;
		case ItemClickAction.DoNothing:
			return true;
		}
		return false;
	}

	public bool OnItemClicked(int index)
	{
		if (m_InteractingPlayer != null && m_ItemContainer != null && m_OtherItemContainer != null)
		{
			Item item = m_ItemContainer.GetItem(index);
			if (item.IsInUse())
			{
				return false;
			}
			if (item.IsQuestItem() && !QuestManager.GetInstance().DoesPlayerOwnQuestItem(item.m_NetView.viewID, m_InteractingPlayer.m_NetView.viewID))
			{
				SpeechManager.GetInstance().SaySomething(m_InteractingPlayer, "Text.Dialog.NotOurQuestItem", SpeechTone.Negative);
				return false;
			}
			if (m_OtherItemContainer == m_InteractingPlayer.m_ItemContainer)
			{
				if (m_InteractingPlayer.m_ItemContainer.GetFreeSpaceCount() > 0)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Take_Item, AudioController.UI_Audio_GO);
				}
				else if (m_OtherItemContainer != m_InteractingPlayer.m_ItemContainer)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Unavailable, AudioController.UI_Audio_GO);
				}
			}
			else if (m_ItemContainer == m_InteractingPlayer.m_ItemContainer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Give_Item, AudioController.UI_Audio_GO);
			}
		}
		switch (m_CurrentItemClickAction)
		{
		case ItemClickAction.SwapItemWithAnotherInventory:
			if (m_InteractingPlayer != null && m_ItemContainer != null && m_OtherItemContainer != null && m_ItemContainer != m_OtherItemContainer)
			{
				int num = ((!(m_ItemContainer == null)) ? m_ItemContainer.GetObjectNetID() : (-1));
				int num2 = ((!(m_OtherItemContainer == null)) ? m_OtherItemContainer.GetObjectNetID() : (-1));
				Item item2 = m_ItemContainer.GetItem(index);
				int num3 = ((!(item2 == null)) ? item2.m_NetView.viewID : (-1));
				m_InteractingPlayer.m_bPendingRequest = true;
				m_InteractingPlayer.m_NetView.RPC("RPC_MASTER_SelectInventoryItem", NetTargets.MasterClient, num, num2, num3, false, false);
				return true;
			}
			break;
		case ItemClickAction.CallbackClickEvent:
			if (OnItemClickedEvent != null)
			{
				OnItemClickedEvent(m_ItemContainer, index);
				return true;
			}
			break;
		case ItemClickAction.DoNothing:
			return true;
		}
		return false;
	}

	public bool OnHiddenItemClicked(int index)
	{
		Item hiddenItem = m_ItemContainer.GetHiddenItem(index);
		if (m_InteractingPlayer != null && m_ItemContainer != null && m_OtherItemContainer != null)
		{
			if (m_OtherItemContainer == m_InteractingPlayer.m_ItemContainer)
			{
				if (m_InteractingPlayer.m_ItemContainer.GetFreeSpaceCount() > 0)
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Take_Item, AudioController.UI_Audio_GO);
				}
				else
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Unavailable, AudioController.UI_Audio_GO);
				}
			}
			else if (m_ItemContainer == m_InteractingPlayer.m_ItemContainer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_UI_Give_Item, AudioController.UI_Audio_GO);
			}
		}
		switch (m_CurrentItemClickAction)
		{
		case ItemClickAction.SwapItemWithAnotherInventory:
			if (m_InteractingPlayer != null && m_ItemContainer != null && m_OtherItemContainer != null && m_ItemContainer != m_OtherItemContainer)
			{
				int num = ((!(m_ItemContainer == null)) ? m_ItemContainer.GetObjectNetID() : (-1));
				int num2 = ((!(m_OtherItemContainer == null)) ? m_OtherItemContainer.GetObjectNetID() : (-1));
				int num3 = ((!(hiddenItem == null)) ? hiddenItem.m_NetView.viewID : (-1));
				m_InteractingPlayer.m_bPendingRequest = true;
				m_InteractingPlayer.m_NetView.RPC("RPC_MASTER_SelectInventoryItem", NetTargets.MasterClient, num, num2, num3, true, false);
				return true;
			}
			break;
		case ItemClickAction.CallbackClickEvent:
			if (OnItemClickedEvent != null)
			{
				OnItemClickedEvent(m_ItemContainer, index);
			}
			return true;
		case ItemClickAction.DoNothing:
			return true;
		}
		return false;
	}
}
