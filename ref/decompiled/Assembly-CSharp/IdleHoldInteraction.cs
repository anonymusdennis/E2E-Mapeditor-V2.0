using UnityEngine;

[RequireComponent(typeof(T17NetView))]
public class IdleHoldInteraction : AnimatedInteraction
{
	private bool m_InteractionAllowed = true;

	private T17NetView m_NetView;

	protected override void Init()
	{
		base.Init();
		m_NetView = GetComponent<T17NetView>();
		if (!(m_NetView == null))
		{
		}
	}

	protected override void OnDestroy()
	{
		m_NetView = null;
		base.OnDestroy();
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return base.AllowedToInteract(localCharacter) && CanInteract();
	}

	public override bool InteractionVisibility()
	{
		return base.InteractionVisibility() && CanInteract();
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected bool CanInteract()
	{
		return m_InteractionAllowed;
	}

	public void SetCanInteractRPC(bool state)
	{
		m_NetView.RPC("RPC_SetCanInteract", NetTargets.All, state);
	}

	[PunRPC]
	public void RPC_SetCanInteract(bool state)
	{
		m_InteractionAllowed = state;
	}
}
