using UnityEngine;

public class GeneratorInteraction : AnimatedInteraction
{
	private Generator m_Generator;

	private Animator m_Animator;

	private int m_AnimOnHash = -1;

	protected override void Init()
	{
		base.Init();
		m_Animator = GetComponentInChildren<Animator>();
		m_AnimOnHash = Animator.StringToHash("On");
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return CanInteract();
	}

	public override bool InteractionVisibility()
	{
		return CanInteract();
	}

	public override void InteractionEndedEvent(Character interactingCharacter)
	{
		base.InteractionEndedEvent(interactingCharacter);
		if (null != interactingCharacter)
		{
			interactingCharacter.ClearRemoteInterativeObject();
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_Generator != null)
		{
			m_Generator.DisableGenerator();
		}
	}

	public override void UpdateInteraction()
	{
		RequestStopInteraction(m_interactingCharacter);
		base.UpdateInteraction();
	}

	public override bool LeaveCharacterPositionUnAltered()
	{
		return true;
	}

	private bool CanInteract()
	{
		return m_Generator != null && m_Generator.GeneratorActive();
	}

	public void SetGenerator(Generator generator)
	{
		m_Generator = generator;
	}

	public void SetState(bool bOn)
	{
		if (m_Animator != null && m_AnimOnHash != -1)
		{
			m_Animator.SetBool(m_AnimOnHash, bOn);
		}
	}
}
