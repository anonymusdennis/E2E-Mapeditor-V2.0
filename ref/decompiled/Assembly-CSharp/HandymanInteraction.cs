using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetObjectLock))]
[RequireComponent(typeof(T17NetView))]
public abstract class HandymanInteraction : AnimatedInteraction
{
	public delegate void InteractionHandler(HandymanInteraction sender, Character interactingCharacter);

	[Header("Handyman Interaction")]
	public JobType m_JobType;

	public List<ItemData> m_RequiredItemsForInteractions = new List<ItemData>();

	public ItemData m_EquippedItemRequirement;

	public SpeechPODO m_DontHaveItemsSpeech;

	public bool m_bCanDoWithoutPlayerJob;

	public int m_NumRepsForCompletion = 1;

	public float m_TimeForAutoCompletion = 5f;

	private float m_TimeUntilAutoCompletion;

	private string m_ItemRequiredTag = string.Empty;

	public Animator m_Animator;

	public string m_AnimatorActiveBool;

	public string m_AnimatorGotFixedBool = "gotFixed";

	public GameObject m_EnabledStateGO;

	public GameObject m_DisabledStateGO;

	public float m_InteractingZOffset = -0.1f;

	private int m_RepsDone;

	private bool m_bIsActive;

	private bool m_bGotFixed;

	private IMinigameMasher m_ButtonMasher;

	public event InteractionHandler FixedEvent;

	protected abstract IMinigameMasher SetupButtonMasher(PerPlayerTrackedUIElements trackedUIElements);

	protected override void Start()
	{
		base.Start();
		if (m_JobType != 0)
		{
		}
	}

	protected override void OnDestroy()
	{
		m_RequiredItemsForInteractions.Clear();
		this.FixedEvent = null;
		base.OnDestroy();
	}

	protected override void Init()
	{
		base.Init();
		RPC_SetActiveState(m_bIsActive, m_bGotFixed);
	}

	public void Local_ResetReps()
	{
		m_RepsDone = 0;
	}

	public void Local_SetGotFixed(bool gotFixed)
	{
		RPC_SetActiveState(m_bIsActive, gotFixed);
	}

	public void Local_SetForJobTimeActive(bool state, bool? gotFixed)
	{
		RPC_SetActiveState(state, (!gotFixed.HasValue) ? m_bGotFixed : gotFixed.Value);
	}

	public bool NeedsJobTimeFixing()
	{
		return m_bIsActive;
	}

	private void SetActiveState(bool state, bool gotFixed)
	{
		m_NetViewID.RPC("RPC_SetActiveState", NetTargets.All, state, gotFixed);
	}

	[PunRPC]
	protected virtual void RPC_SetActiveState(bool state, bool gotFixed)
	{
		if (m_Animator != null)
		{
			if (!string.IsNullOrEmpty(m_AnimatorActiveBool))
			{
				m_Animator.SetBool(m_AnimatorActiveBool, state);
			}
			if (!string.IsNullOrEmpty(m_AnimatorGotFixedBool))
			{
				m_Animator.SetBool(m_AnimatorGotFixedBool, gotFixed);
			}
		}
		if (m_EnabledStateGO != null)
		{
			m_EnabledStateGO.SetActive(state);
		}
		if (m_DisabledStateGO != null)
		{
			m_DisabledStateGO.SetActive(!state);
		}
		m_bIsActive = state;
		m_bGotFixed = gotFixed;
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
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

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		if (null != localCharacter && null != localCharacter.m_CharacterStats && localCharacter.m_CharacterStats.m_bIsPlayer && m_ButtonMasher != null)
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
		if (!ShouldUpdateInteraction() || !(m_interactingCharacter != null) || m_RepsDone == m_NumRepsForCompletion)
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
					OnFixed();
				}
			}
		}
		else
		{
			m_TimeUntilAutoCompletion -= UpdateManager.deltaTime;
			if (m_TimeUntilAutoCompletion < 0f)
			{
				OnFixed();
			}
		}
	}

	protected virtual bool ShouldUpdateInteraction()
	{
		return m_bIsActive;
	}

	protected virtual void OnFixed()
	{
		if (m_bIsActive && this.FixedEvent != null)
		{
			this.FixedEvent(this, m_interactingCharacter);
		}
		RequestStopInteraction(m_interactingCharacter);
		SetActiveState(state: false, gotFixed: true);
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return m_bIsActive && base.AllowedToInteract(localCharacter) && CanCharacterDoJob(localCharacter) && DoesCharacterSatasifysItemRequirement(localCharacter);
	}

	protected bool DoesCharacterSatasifysItemRequirement(Character localCharacter)
	{
		bool flag = false;
		m_ItemRequiredTag = string.Empty;
		if (m_EquippedItemRequirement != null)
		{
			Item equippedItem = localCharacter.GetEquippedItem();
			if (equippedItem != null && equippedItem.ItemDataID == m_EquippedItemRequirement.m_ItemDataID)
			{
				flag = true;
			}
			else
			{
				m_ItemRequiredTag = m_EquippedItemRequirement.m_ItemLocalizationTag;
			}
		}
		else
		{
			flag = true;
		}
		if (m_RequiredItemsForInteractions.Count > 0)
		{
			List<ItemData> out_ItemsMissing = new List<ItemData>();
			flag &= localCharacter.HasItemsOnPerson(m_RequiredItemsForInteractions, ref out_ItemsMissing);
			if (out_ItemsMissing.Count > 0)
			{
				m_ItemRequiredTag = out_ItemsMissing[0].m_ItemLocalizationTag;
			}
		}
		else
		{
			flag = flag && true;
		}
		return flag;
	}

	public override bool InteractionVisibility()
	{
		return m_bIsActive && base.InteractionVisibility();
	}

	private bool DoesCharacterHaveRelevantJob(Character character)
	{
		BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(character);
		return charactersJob != null && charactersJob.m_Type == m_JobType;
	}

	private bool CanCharacterDoJob(Character localCharacter)
	{
		if (localCharacter is Player)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			for (int i = 0; i < allPlayers.Count; i++)
			{
				if (m_bCanDoWithoutPlayerJob || DoesCharacterHaveRelevantJob(allPlayers[i]))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public override bool AllowOtherPlayerHUDInteractions()
	{
		return false;
	}

	public override bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (!base.OnPlayerNotAllowedToInteract(localCharacter))
		{
			if (IsPossibleToInteractWith() && CanCharacterDoJob(localCharacter) && !DoesCharacterSatasifysItemRequirement(localCharacter))
			{
				m_DontHaveItemsSpeech.m_TextId = "Text.Emote.ItemNotEquipped";
				List<SpeechManager.Token> list = new List<SpeechManager.Token>();
				list.Add(new SpeechManager.Token("$ItemToGet", m_ItemRequiredTag, bIsCharacterNetviewID: false));
				List<SpeechManager.Token> tokens = list;
				SpeechManager.GetInstance().SaySomething(localCharacter, m_DontHaveItemsSpeech.m_TextId, tokens, m_DontHaveItemsSpeech.m_SpeechTone, m_DontHaveItemsSpeech.m_Duration, m_DontHaveItemsSpeech.m_Priority);
				return true;
			}
			return false;
		}
		return true;
	}

	public virtual bool IsPossibleToInteractWith()
	{
		return m_bIsActive;
	}

	protected override void UpdateInteractionZ_PreTransitionStart()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_Interacting()
	{
		SetInteractionZ();
	}

	protected override void UpdateInteractionZ_PostTransitionEnd()
	{
		SetInteractionZ();
	}

	private void SetInteractionZ()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_InteractingZOffset);
		}
	}
}
