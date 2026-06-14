public interface IMultistageTransferInteractionResponder
{
	bool CanInteract(Character localCharacter);

	int GetNumberStages();

	int GetCurrentStage();

	void OnStartInteraction(Character localCharacter, out TransferItemsInteraction.TransferDirection direction, out ItemData[] itemTypesToTransfer);

	bool IsInteractionVisible();

	void OnTransferComplete(Item item, ItemContainer to, ItemContainer from);

	void OnTransferFailed();
}
