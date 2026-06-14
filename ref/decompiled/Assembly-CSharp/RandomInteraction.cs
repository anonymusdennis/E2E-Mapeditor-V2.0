using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class RandomInteraction : ActionTask<AICharacter>
{
	public bool m_bFreeTimeInteractions = true;

	public float m_MinInteractionTime = 2f;

	public float m_MaxInteractionTime = 10f;

	public float m_fSameObjectInteractionCooldown = 15f;

	[BlackboardOnly]
	public BBParameter<Character> kickedByCharacter;

	public bool m_bComplainAboutKick = true;

	private InteractiveObject m_LastUsedInteractiveObject;

	private float m_fSameObjectInteractionCooldownEpoch;

	private float m_InteractionTimer;

	private InteractiveObject m_ReservedInteractiveObject;

	private bool m_bMovingToPosition;

	private bool m_bTargetReached;

	private bool m_bInteracting;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	private NetObjectLock.OnResponse m_OnInteractResponseDel;

	private NetObjectLock.OnRPCKicked m_OnInteractionEndedDel;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		m_OnInteractResponseDel = OnInteractResponse;
		m_OnInteractionEndedDel = OnInteractionEnded;
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		m_ReservedInteractiveObject = null;
		RoomBlob currentLocation = base.agent.m_Character.m_CurrentLocation;
		if (currentLocation == null)
		{
			EndAction(false);
			return;
		}
		bool isInmate = base.agent.m_Character.m_CharacterRole == CharacterRole.Inmate;
		List<InteractiveObject> list = currentLocation.FindObject(m_bFreeTimeInteractions, isInmate, null, null, null, filterReserved: true);
		if (list == null || list.Count == 0)
		{
			EndAction(false);
			return;
		}
		InteractiveObject interactiveObject = list[Random.Range(0, list.Count)];
		if (interactiveObject == null)
		{
			EndAction(false);
			return;
		}
		if (interactiveObject == m_LastUsedInteractiveObject && m_fSameObjectInteractionCooldownEpoch > UpdateManager.time)
		{
			EndAction(false);
			return;
		}
		m_ReservedInteractiveObject = interactiveObject;
		if (!interactiveObject.ReserveObject(base.agent.m_Character, OnReservationRevoked))
		{
			EndAction(false);
			return;
		}
		m_InteractionTimer = Random.Range(m_MinInteractionTime, m_MaxInteractionTime);
		m_bMovingToPosition = false;
		m_bTargetReached = false;
		m_bInteracting = false;
	}

	public void OnReservationRevoked()
	{
		m_ReservedInteractiveObject = null;
		EndAction(false);
	}

	protected override void OnStop()
	{
		if (base.agent.m_Character.IsInteracting() && base.agent.m_Character.GetInteractiveObject() == m_ReservedInteractiveObject)
		{
			base.agent.m_Character.RequestStopInteraction();
		}
		if (m_ReservedInteractiveObject != null)
		{
			m_ReservedInteractiveObject.UnreserveObject(base.agent.m_Character);
		}
	}

	public void OnTargetReached()
	{
		m_bMovingToPosition = false;
		m_bTargetReached = true;
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	public void OnInteractResponse(bool success)
	{
		if (!success)
		{
			EndAction(false);
		}
	}

	public void OnInteractionEnded(bool success, int kickingCharacter)
	{
		if (!success && m_bComplainAboutKick && !base.agent.m_CharacterStats.m_bIsPlayer)
		{
			KickedByCharacter(kickingCharacter);
		}
		EndAction(success);
	}

	protected override void OnUpdate()
	{
		if (m_ReservedInteractiveObject == null)
		{
			EndAction(false);
		}
		else if (m_bInteracting)
		{
			if (m_InteractionTimer > 0f)
			{
				m_InteractionTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
				return;
			}
			m_bInteracting = false;
			EndAction(true);
		}
		else
		{
			if (m_bMovingToPosition)
			{
				return;
			}
			if (!m_bTargetReached)
			{
				m_bMovingToPosition = true;
				base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, m_ReservedInteractiveObject.transform.position);
				return;
			}
			m_bInteracting = true;
			if (base.agent.m_Character.IsInteracting())
			{
				base.agent.m_Character.ForceStopInteraction();
			}
			m_ReservedInteractiveObject.Interact(base.agent.m_Character, m_OnInteractResponseDel, m_OnInteractionEndedDel);
			m_LastUsedInteractiveObject = m_ReservedInteractiveObject;
			m_fSameObjectInteractionCooldownEpoch = UpdateManager.time + m_fSameObjectInteractionCooldown;
		}
	}

	private void KickedByCharacter(int kickingCharacter)
	{
		Character character = T17NetView.Find<Character>(kickingCharacter);
		kickedByCharacter.value = character;
		Character character2 = base.agent.m_Character;
		if (character == null || character2 == null)
		{
			return;
		}
		bool flag = false;
		if (character.m_CharacterStats != null && character.m_CharacterStats.m_bIsPlayer)
		{
			flag = true;
		}
		SpeechManager instance = SpeechManager.GetInstance();
		Character speaker = character2;
		string textID = "Text.Inmates.DetachedFromEquipment";
		SpeechTone tone = SpeechTone.Negative;
		float duration = 2f;
		int priority = 5;
		bool bAllowTextRecolour = flag;
		instance.SaySomething(speaker, textID, tone, duration, priority, -1, ignoreStatus: false, bAllowTextRecolour);
		character2.FaceCharacter(character);
		character2.PauseMovement(1f);
		if (ConfigManager.GetInstance() != null)
		{
			AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
			if (aiConfig != null && character2.m_CharacterOpinions != null)
			{
				character2.m_CharacterOpinions.DecreaseOpinionOf(character, aiConfig.GetOpinionDecreaseWhenKickedOffInterativeObject());
				EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, character.GetStatChangeEffectPosition());
			}
		}
	}
}
