using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class RoomInteraction : ActionTask<AICharacter>
{
	public string m_InteractionType;

	public RoomBlob.eLocation m_RoomLocation;

	public bool m_bFreeTimeObjects;

	public bool m_bReserve = true;

	public float m_fMinRunDistance = 5f;

	public float m_fMaxRunDistance = 10f;

	private float m_fRunDistance = 5f;

	private Vector3 m_vTarget = Vector3.zero;

	[BlackboardOnly]
	public BBParameter<Character> kickedByCharacter;

	private InteractiveObject m_ReservedInteractiveObject;

	private bool m_bMovingToPosition;

	private bool m_bTargetReached;

	private bool m_bInteracting;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	private NetObjectLock.OnResponse m_OnInteractResponseDel;

	private NetObjectLock.OnRPCKicked m_OnInteractionEndedDel;

	protected override string info => string.Concat("Room Interaction", '\n', m_RoomLocation, ":", m_InteractionType);

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
		InteractiveObject interactiveObject = base.agent.FindObject(m_RoomLocation, m_InteractionType, m_bFreeTimeObjects, m_bReserve);
		if (interactiveObject == null)
		{
			EndAction(false);
			return;
		}
		m_ReservedInteractiveObject = interactiveObject;
		if (m_bReserve && !interactiveObject.ReserveObject(base.agent.m_Character, OnReservationRevoked))
		{
			EndAction(false);
			return;
		}
		m_bMovingToPosition = false;
		m_bTargetReached = false;
		m_bInteracting = false;
		m_fRunDistance = Random.Range(m_fMinRunDistance, m_fMaxRunDistance);
		m_fRunDistance *= m_fRunDistance;
		kickedByCharacter.value = null;
	}

	protected override void OnStop()
	{
		if (m_bInteracting && base.agent.m_Character.IsInteracting() && base.agent.m_Character.GetInteractiveObject() == m_ReservedInteractiveObject)
		{
			base.agent.m_Character.RequestStopInteraction();
		}
		if (m_ReservedInteractiveObject != null && m_bReserve)
		{
			m_ReservedInteractiveObject.UnreserveObject(base.agent.m_Character);
		}
		base.agent.SetRunning(running: false);
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
		m_bInteracting = success;
		if (!success)
		{
			EndAction(false);
		}
	}

	public void OnInteractionEnded(bool success, int kickingCharacter)
	{
		if (!success)
		{
			KickedByCharacter(kickingCharacter);
		}
		m_bInteracting = false;
		EndAction(success);
	}

	public void OnReservationRevoked()
	{
		m_ReservedInteractiveObject = null;
		EndAction(false);
	}

	protected override void OnUpdate()
	{
		if (m_ReservedInteractiveObject == null)
		{
			EndAction(false);
		}
		else if (!m_bMovingToPosition)
		{
			if (!m_bTargetReached)
			{
				m_bMovingToPosition = true;
				m_vTarget = m_ReservedInteractiveObject.transform.position;
				base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, m_vTarget);
			}
			else
			{
				if (m_bInteracting)
				{
					return;
				}
				if (!m_bReserve || m_ReservedInteractiveObject.HasReservation(base.agent.m_Character))
				{
					if (base.agent.m_Character.IsInteracting())
					{
						base.agent.m_Character.ForceStopInteraction();
					}
					m_ReservedInteractiveObject.Interact(base.agent.m_Character, m_OnInteractResponseDel, m_OnInteractionEndedDel);
					m_bInteracting = true;
				}
				else
				{
					EndAction(false);
				}
			}
		}
		else if (Vector3.Scale(m_vTarget - base.agent.m_Transform.position, FloorManager.FLOOR_SCALE).sqrMagnitude > m_fRunDistance)
		{
			base.agent.SetRunning(running: true);
		}
		else
		{
			base.agent.SetRunning(running: false);
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
