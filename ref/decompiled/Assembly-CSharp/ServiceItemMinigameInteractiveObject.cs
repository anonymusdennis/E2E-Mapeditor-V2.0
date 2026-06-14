using UnityEngine;

public abstract class ServiceItemMinigameInteractiveObject : ServiceItemInteractiveObject
{
	[Header("Minigame Requirements")]
	public MinigameCompletionHelper m_MinigameCompletionHelper;

	protected IMinigameMasher m_ButtonMasher;

	protected bool m_bShowMinigameOnStart = true;

	protected bool m_bHasTriggeredTransfer;

	protected abstract IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements);

	protected override void Awake()
	{
		base.Awake();
		m_bAutoServeAfterDelay = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_ButtonMasher = null;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_bShowMinigameOnStart)
		{
			RestAndShowMinigame(localCharacter);
		}
	}

	public override void InteractionStartedEvent(Character interactingCharacter)
	{
		base.InteractionStartedEvent(interactingCharacter);
		m_bHasTriggeredTransfer = false;
	}

	protected void RestAndShowMinigame(Character localCharacter)
	{
		m_MinigameCompletionHelper.ResetForNewUser(localCharacter);
		if (localCharacter.IsPlayer())
		{
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(((Player)localCharacter).m_PlayerCameraManagerBindingID);
			m_ButtonMasher = SetupButtonMasher(playerTrackedUIElements);
			m_ButtonMasher.EnableForPlayer(localCharacter as Player);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		DisableMinigameHud(localCharacter);
	}

	protected void DisableMinigameHud(Character localCharacter)
	{
		if (localCharacter != null && localCharacter.IsPlayer() && m_ButtonMasher != null)
		{
			m_ButtonMasher.Disable();
			m_ButtonMasher = null;
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (!(m_interactingCharacter == null) && (!m_interactingCharacter.IsPlayer() || m_ButtonMasher != null) && ShouldUpdateMinigameLogic())
		{
			bool hasUserCompletedRep = m_interactingCharacter.IsPlayer() && m_ButtonMasher.HasCompletedRep();
			if (m_MinigameCompletionHelper.UpdateUser(hasUserCompletedRep))
			{
				OnFinishedMinigameReps();
			}
		}
	}

	protected virtual bool ShouldUpdateMinigameLogic()
	{
		return m_ServiceComponent.GetLinkedJob().HasWaitingCustomer();
	}

	protected void OnFinishedMinigameReps()
	{
		m_bHasTriggeredTransfer = true;
		DoItemTransferAndRequestStop(m_interactingCharacter);
	}
}
