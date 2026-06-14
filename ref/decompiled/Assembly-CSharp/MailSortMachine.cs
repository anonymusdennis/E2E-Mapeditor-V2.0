using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarryableObjectConsumer))]
public class MailSortMachine : TransferItemsInteraction
{
	public List<ItemData> m_PossibleItemsToDispense;

	public int m_NumItemsToDispense = 3;

	public Animator m_Animator;

	public string m_NoMailResetTrigger = "Reset";

	private CarryableObjectConsumer m_Consumer;

	private List<int> m_ItemCreateResponseIds = new List<int>();

	private int m_ImmediateItemCreateResponseId;

	protected override void Awake()
	{
		base.Awake();
		m_Consumer = GetComponent<CarryableObjectConsumer>();
	}

	protected override void Start()
	{
		base.Start();
		if (m_Consumer != null)
		{
			m_Consumer.InputDroppedOnUsEvent += Consumer_InputDroppedOnUsEvent;
			m_Consumer.FinishedConsumingEvent += Consumer_FinishedConsumingEvent;
			if (m_Animator == null)
			{
				m_Animator = m_Consumer.m_Animator;
			}
		}
	}

	protected override void OnDestroy()
	{
		if (m_Consumer != null)
		{
			m_Consumer.InputDroppedOnUsEvent -= Consumer_InputDroppedOnUsEvent;
			m_Consumer.FinishedConsumingEvent -= Consumer_FinishedConsumingEvent;
		}
		base.OnDestroy();
	}

	private void Consumer_InputDroppedOnUsEvent(CarryableObjectConsumer consumer, CarryObjectInteraction theObject)
	{
		if (m_Consumer.m_Animator == null)
		{
			SpawnItemsInContainer();
		}
	}

	private void Consumer_FinishedConsumingEvent(CarryableObjectConsumer consumer)
	{
		SpawnItemsInContainer();
	}

	public override bool InteractionVisibility()
	{
		return base.InteractionVisibility() && m_ItemContainer.GetItemCount() != 0;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		bool flag = m_Consumer != null && m_Consumer.IsProcessing();
		return base.AllowedToInteract(localCharacter) && !flag;
	}

	private void SpawnItemsInContainer()
	{
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		m_ItemCreateResponseIds.Clear();
		int num = Mathf.Min(m_ItemContainer.GetFreeSpaceCount(), m_NumItemsToDispense);
		if (m_PossibleItemsToDispense.Count != 0)
		{
			for (int i = 0; i < num; i++)
			{
				ItemData itemData = m_PossibleItemsToDispense[Random.Range(0, m_PossibleItemsToDispense.Count)];
				m_ItemCreateResponseIds.Add(ItemManager.GetInstance().AssignItemRPC(0, itemData.m_ItemDataID, OnItemMgrResponseAddToDispenser, ref m_ImmediateItemCreateResponseId));
			}
		}
	}

	private void OnItemMgrResponseAddToDispenser(Item item, int eventID)
	{
		if (item != null && (eventID == m_ImmediateItemCreateResponseId || m_ItemCreateResponseIds.Contains(eventID)) && !m_ItemContainer.AddItemRPC(item))
		{
			ItemManager.GetInstance().RequestReleaseItem(item);
		}
	}

	protected override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		if (from == m_ItemContainer && from.GetItemCount() == 0 && m_Animator != null)
		{
			m_Animator.SetTrigger(m_NoMailResetTrigger);
		}
	}
}
