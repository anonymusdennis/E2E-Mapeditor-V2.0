using UnityEngine;

[RequireComponent(typeof(DelayedPassiveItemProcessor))]
public class DelayedPassiveItemProcessorInteraction : TransferItemsInteraction
{
	protected DelayedPassiveItemProcessor m_ItemProcessor;

	protected override void Init()
	{
		m_ItemProcessor = GetComponent<DelayedPassiveItemProcessor>();
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		bool flag = base.AllowedToInteract(localCharacter) && !IsProcessorTooBusyToInteractWith();
		if (localCharacter != null && localCharacter.m_CharacterStats != null && localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			flag &= m_ItemProcessor.GetState() > DelayedPassiveItemProcessor.State.CreatingItem || m_ItemProcessor.WillAcceptInput(localCharacter.GetEquippedItem());
		}
		return flag;
	}

	private bool IsProcessorTooBusyToInteractWith()
	{
		return m_ItemProcessor.IsBusy() && !m_ItemProcessor.m_bIsInterruptable;
	}

	public override bool InteractionVisibility()
	{
		return base.InteractionVisibility() && !IsProcessorTooBusyToInteractWith();
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		SetTransferDirection(TransferDirection.Invalid);
		if (!(m_ItemProcessor == null))
		{
			switch (m_ItemProcessor.GetState())
			{
			case DelayedPassiveItemProcessor.State.Idle:
			case DelayedPassiveItemProcessor.State.Interrupted:
				SetTransferDirection(TransferDirection.FromCharacter);
				SetTransferItemTypes(m_ItemProcessor.GetInputItemTypes());
				break;
			case DelayedPassiveItemProcessor.State.FinishedCreatingItem:
				SetTransferDirection(TransferDirection.ToCharacter);
				SetTransferItemTypes(m_ItemProcessor.GetOutputItemTypes().ToArray());
				break;
			case DelayedPassiveItemProcessor.State.ProcessingItem:
				if (!m_ItemProcessor.m_bIsInterruptable || m_ItemProcessor.IsItemSpawnInProgress())
				{
					break;
				}
				if (localCharacter.m_ItemContainer.GetFreeSpaceCount() > 0 || localCharacter.GetEquippedItem() == null)
				{
					Item item = m_ItemProcessor.CancelProcessing(removeFromProcessor: true);
					if (item != null)
					{
						if (localCharacter.GetEquippedItem() == null)
						{
							localCharacter.SetEquippedItem(item);
						}
						else if (localCharacter.m_ItemContainer.GetFreeSpaceCount() > 0)
						{
							localCharacter.m_ItemContainer.AddItemRPC(item);
						}
						else
						{
							ItemManager.GetInstance().RequestReleaseItem(item);
						}
					}
				}
				else
				{
					m_ItemProcessor.CancelProcessing(removeFromProcessor: false);
				}
				break;
			}
		}
		base.OnStartInteraction(localCharacter);
	}

	protected override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		if (item != null && to == m_ItemContainer && m_ItemProcessor != null)
		{
			m_ItemProcessor.StartProcessingItem(item);
		}
	}
}
