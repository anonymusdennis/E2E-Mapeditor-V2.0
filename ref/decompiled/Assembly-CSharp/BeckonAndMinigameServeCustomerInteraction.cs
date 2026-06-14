using UnityEngine;

[RequireComponent(typeof(BeckonCustomer))]
public abstract class BeckonAndMinigameServeCustomerInteraction : ServiceItemMinigameInteractiveObject
{
	[Header("BeckonAndServeCustomerInteraction")]
	public BeckonCustomer m_BeckonCustomerComponent;

	public AnimState m_HasWaitingCustomerAnim;

	private bool m_bHasCustomerToServe;

	protected override void Awake()
	{
		base.Awake();
		if (m_BeckonCustomerComponent == null)
		{
			m_BeckonCustomerComponent = GetComponent<BeckonCustomer>();
		}
		m_bShowMinigameOnStart = false;
	}

	protected override void OnDestroy()
	{
		m_BeckonCustomerComponent = null;
		if (m_ServiceItemJob != null)
		{
			m_ServiceItemJob.CustomerWaitingForServiceChangedEvent -= CustomerWaitingForServiceChangedEvent;
		}
		base.OnDestroy();
	}

	public override void LinkToJob(ServiceItemJob job)
	{
		base.LinkToJob(job);
		job.CustomerWaitingForServiceChangedEvent += CustomerWaitingForServiceChangedEvent;
	}

	protected override bool AllowedToInteract_CustomerRequirementsMet()
	{
		return m_BeckonCustomerComponent.IsAllowedToBeckonNewCustomer();
	}

	public override bool InteractionVisibility()
	{
		return m_BeckonCustomerComponent.IsAllowedToBeckonNewCustomer() && base.InteractionVisibility();
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return m_BeckonCustomerComponent.IsAllowedToBeckonNewCustomer() && base.AllowedToInteract(localCharacter);
	}

	private void CustomerWaitingForServiceChangedEvent(ServiceCustomerViaProxyJob sender, CustomerViaProxy customer, bool isWaitingForService)
	{
		if (!m_ServicePointLinker.m_CustomerPool.Contains(customer.m_AiCustomer))
		{
			return;
		}
		if (isWaitingForService)
		{
			m_bHasCustomerToServe = true;
			if (m_interactingCharacter != null)
			{
				PrepareInteracterForWaitingCustomer(m_interactingCharacter);
			}
			return;
		}
		m_bHasCustomerToServe = false;
		if (m_interactingCharacter != null)
		{
			DisableMinigameHud(m_interactingCharacter);
			m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_HasWaitingCustomerAnim);
		}
	}

	private void PrepareInteracterForWaitingCustomer(Character localCharacter)
	{
		RestAndShowMinigame(localCharacter);
		localCharacter.m_CharacterAnimator.StartAnimation(m_HasWaitingCustomerAnim);
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (!m_bHasCustomerToServe)
		{
			m_BeckonCustomerComponent.CallForNextCustomer();
		}
		else
		{
			PrepareInteracterForWaitingCustomer(localCharacter);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (localCharacter != null)
		{
			localCharacter.m_CharacterAnimator.StopAnimation(m_HasWaitingCustomerAnim);
		}
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (T17NetManager.IsMasterClient && !m_bHasTriggeredTransfer)
		{
			m_BeckonCustomerComponent.CancelRequestForCustomer();
		}
		m_bHasCustomerToServe = false;
	}

	protected override bool ShouldUpdateMinigameLogic()
	{
		return m_bHasCustomerToServe;
	}
}
