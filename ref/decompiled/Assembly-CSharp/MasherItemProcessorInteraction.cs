using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ImmediateItemProcessor))]
public abstract class MasherItemProcessorInteraction : TransferItemsInteraction
{
	[Header("MasherItemProcessorInteraction behaviours")]
	public int m_NumRepsForCompletion = 5;

	public float m_TimeForAutoCompletion = 5f;

	public SpeechPODO m_DontHaveItemsSpeech;

	public MinigameMasherSettingsContainer m_MinigameSettingsContainer;

	private int m_RepsDone;

	private float m_TimeUntilAutoCompletion;

	private IMinigameMasher m_ButtonMasher;

	private ImmediateItemProcessor m_ItemProcessor;

	protected abstract IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements);

	protected override void Awake()
	{
		base.Awake();
		m_ItemProcessor = GetComponent<ImmediateItemProcessor>();
		if (!(m_ItemProcessor == null))
		{
		}
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			AssignTransferItemsIfUnassigned();
			return localCharacter.HasItemsOnPerson(GetTransferItemTypes());
		}
		return false;
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			List<ItemData> out_ItemsMissing = new List<ItemData>();
			if (!localCharacter.HasItemsOnPerson(GetTransferItemTypes(), ref out_ItemsMissing))
			{
				ItemData itemData = out_ItemsMissing[0];
				m_DontHaveItemsSpeech.m_TextId = "Text.Emote.ItemNotEquipped";
				List<SpeechManager.Token> list = new List<SpeechManager.Token>();
				list.Add(new SpeechManager.Token("$ItemToGet", itemData.m_ItemLocalizationTag, bIsCharacterNetviewID: false));
				List<SpeechManager.Token> tokens = list;
				SpeechManager.GetInstance().SaySomething(localCharacter, m_DontHaveItemsSpeech.m_TextId, tokens, m_DontHaveItemsSpeech.m_SpeechTone, m_DontHaveItemsSpeech.m_Duration, m_DontHaveItemsSpeech.m_Priority);
				return true;
			}
			return false;
		}
		return true;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		AssignTransferItemsIfUnassigned();
		base.OnStartInteraction(localCharacter);
		m_RepsDone = 0;
		if (localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			PerPlayerTrackedUIElements playerTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(((Player)localCharacter).m_PlayerCameraManagerBindingID);
			m_ButtonMasher = SetupButtonMasher(playerTrackedUIElements);
			m_ButtonMasher.EnableForPlayer(localCharacter as Player);
		}
		else
		{
			m_TimeUntilAutoCompletion = m_TimeForAutoCompletion;
		}
	}

	private void AssignTransferItemsIfUnassigned()
	{
		if (m_ItemProcessor != null)
		{
			List<ItemData> transferItemTypes = GetTransferItemTypes();
			if (transferItemTypes == null || transferItemTypes.Count == 0)
			{
				SetTransferDirection(TransferDirection.FromCharacter);
				SetTransferItemTypes(m_ItemProcessor.GetInputItemTypes());
			}
		}
	}

	protected override void PostOnStartInteraction(Character localCharacter)
	{
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (!(m_interactingCharacter != null) || m_RepsDone == m_NumRepsForCompletion)
		{
			return;
		}
		if (m_ButtonMasher != null)
		{
			if (m_ButtonMasher.HasCompletedRep())
			{
				m_RepsDone++;
				if (m_RepsDone == m_NumRepsForCompletion)
				{
					DoItemTransferAndRequestStop(m_interactingCharacter);
				}
			}
		}
		else if (m_TimeUntilAutoCompletion >= 0f)
		{
			m_TimeUntilAutoCompletion -= UpdateManager.deltaTime;
			if (m_TimeUntilAutoCompletion < 0f)
			{
				DoItemTransferAndRequestStop(m_interactingCharacter);
			}
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter && null != localCharacter.m_CharacterStats && localCharacter.m_CharacterStats.m_bIsPlayer && m_ButtonMasher != null)
		{
			m_ButtonMasher.Disable();
			m_ButtonMasher = null;
		}
	}

	public override bool AllowOtherPlayerHUDInteractions()
	{
		return false;
	}

	protected override void OnTransferComplete(Item item, ItemContainer to, ItemContainer from)
	{
		base.OnTransferComplete(item, to, from);
		if (m_ItemProcessor != null)
		{
			int itemDataID = item.ItemDataID;
			m_ItemContainer.RemoveItemRPC(item, releaseToManager: true);
			m_ItemProcessor.ConvertItemForCharacter(itemDataID, m_interactingCharacter);
		}
	}
}
