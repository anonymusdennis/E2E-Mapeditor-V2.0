public class MultiStageTransferInteraction : TransferItemsInteraction
{
	public IMultistageTransferInteractionResponder m_InteractableItem;

	protected override void Init()
	{
		base.Init();
		if (m_InteractableItem == null)
		{
			m_InteractableItem = GetComponent<IMultistageTransferInteractionResponder>();
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return base.AllowedToInteract(localCharacter) && m_InteractableItem.CanInteract(localCharacter);
	}

	public override bool InteractionVisibility()
	{
		bool flag = base.InteractionVisibility();
		if (m_InteractableItem != null)
		{
			flag &= m_InteractableItem.IsInteractionVisible();
		}
		return flag;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		m_InteractableItem.OnStartInteraction(localCharacter, out var direction, out var itemTypesToTransfer);
		SetTransferDirection(direction);
		SetTransferItemTypes(itemTypesToTransfer);
		base.OnStartInteraction(localCharacter);
	}

	protected override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		m_InteractableItem.OnTransferComplete(item, to, from);
	}

	protected override void OnTransferFailed()
	{
		base.OnTransferFailed();
		m_InteractableItem.OnTransferFailed();
	}
}
