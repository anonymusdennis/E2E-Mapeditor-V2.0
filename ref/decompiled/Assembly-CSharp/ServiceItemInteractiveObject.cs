using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ServiceCustomer))]
public class ServiceItemInteractiveObject : TransferItemsInteraction
{
	protected ServiceCustomer m_ServiceComponent;

	[Header("ServiceItemInteractiveObject")]
	public SpeechPODO m_NoEquippedItemSpeech;

	public SpeechPODO m_PlayerSpeechWhenServing;

	public bool m_bAutoServeAfterDelay = true;

	public float m_DelayBeforeService = 0.5f;

	private float m_InteractingDuration;

	private bool m_bHasDoneTransfer;

	public CustomerServicePointLinker m_ServicePointLinker;

	protected ServiceItemJob m_ServiceItemJob;

	protected override void Awake()
	{
		base.Awake();
		m_ServiceComponent = GetComponent<ServiceCustomer>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_ServiceItemJob = null;
		m_ServicePointLinker = null;
		m_ServiceComponent = null;
	}

	public virtual void LinkToJob(ServiceItemJob job)
	{
		m_ServiceItemJob = job;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return base.AllowedToInteract(localCharacter) && m_ServiceItemJob.DoesCharacterHaveAnyFinishedProducts(localCharacter, m_bTransferEquippedItemsOnly) && AllowedToInteract_CustomerRequirementsMet() && CanCharacterDoJob(localCharacter);
	}

	protected virtual bool AllowedToInteract_CustomerRequirementsMet()
	{
		return m_ServiceComponent.DoesLinkedJobHavePendingCustomer();
	}

	private bool CanCharacterDoJob(Character localCharacter)
	{
		if (localCharacter is Player)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (DoesCharacterHaveRelevantJob(allPlayers[i]))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private bool DoesCharacterHaveRelevantJob(Character character)
	{
		BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(character);
		return charactersJob != null && charactersJob == m_ServiceItemJob;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_InteractingDuration = 0f;
		m_bHasDoneTransfer = false;
	}

	protected override void PostOnStartInteraction(Character localCharacter)
	{
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (!m_bHasDoneTransfer && m_bAutoServeAfterDelay)
		{
			m_InteractingDuration += UpdateManager.deltaTime;
			if (m_InteractingDuration >= m_DelayBeforeService)
			{
				m_bHasDoneTransfer = true;
				DoItemTransferAndRequestStop(m_interactingCharacter);
			}
		}
	}

	protected override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		m_ServiceComponent.ServiceActionPerformed(item.m_ItemData, m_interactingCharacter);
		if (m_interactingCharacter.IsPlayer() && m_PlayerSpeechWhenServing.IsSet())
		{
			SpeechManager.GetInstance().SaySomething(m_interactingCharacter, m_PlayerSpeechWhenServing);
		}
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			if (!m_ServiceItemJob.DoesCharacterHaveAnyFinishedProducts(localCharacter, m_bTransferEquippedItemsOnly))
			{
				PerformNoEquippedItemSpeech(localCharacter);
				return true;
			}
			return false;
		}
		return true;
	}

	protected override void PerformNoEquippedItemSpeech(Character character)
	{
		SpeechManager.GetInstance().SaySomething(character, m_NoEquippedItemSpeech);
	}
}
