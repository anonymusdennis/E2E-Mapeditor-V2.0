using UnityEngine;

public class InteractiveObject : T17MonoBehaviour
{
	public enum InteractiveType
	{
		PrimaryInteraction,
		SecondaryInteraction,
		TertiaryInteraction,
		PressAndHoldPrimaryInteraction,
		PressAndHoldSecondaryInteraction,
		PressAndHoldTertiaryInteraction
	}

	public enum InteractiveEventType : byte
	{
		InteractionReadyStart,
		InteractionStarted,
		InteractionReadyEnd,
		InteractionEnded
	}

	public enum CharacterResctrictions
	{
		AnyoneCanUse,
		PlayersOnly,
		AiOnly
	}

	public delegate void OnReservationRevoked();

	public enum InteractionType
	{
		InteractiveObject,
		AnimatedInteractiveObject,
		PortableInteractiveObject
	}

	public InteractiveType m_InteractType;

	public CharacterResctrictions m_AllowedCharacterTypes;

	public bool m_bIsEnabled = true;

	public bool m_bInteractionVisibility = true;

	public Directionx4[] m_ValidInteractingDirections = new Directionx4[4]
	{
		Directionx4.Down,
		Directionx4.Left,
		Directionx4.Right,
		Directionx4.Up
	};

	[Range(0f, 1f)]
	public float m_AI_InteractionAffinity = 0.5f;

	protected Character m_interactingCharacter;

	protected Vector3 m_vStartingPosition;

	private OnReservationRevoked m_OnReservationRevoked;

	[ReadOnly]
	public bool m_bObjectReserved;

	[ReadOnly]
	public int m_ReservingCharacterID = -1;

	public NetObjectLock m_NetObjectLock;

	private int m_LocalInteractionID = -1;

	public T17NetView m_NetViewID;

	public bool m_bLeaveCharacterPositionUnaltered;

	private bool m_bCanBeUsedOutsideJobTime = true;

	protected float m_StopInteractionWalkThreshold = 0.4f;

	public uint m_Tag;

	public Vector3 m_PCSplitscreenCameraOffset = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
		if (m_NetViewID == null)
		{
			m_NetViewID = GetComponent<T17NetView>();
		}
	}

	protected virtual void Start()
	{
		if (m_NetObjectLock == null)
		{
			m_NetObjectLock = GetComponent<NetObjectLock>();
			if (m_NetObjectLock == null)
			{
				m_NetObjectLock = base.gameObject.AddComponent<NetObjectLock>();
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (m_NetObjectLock != null)
		{
			if (m_NetObjectLock.IsLocked() && m_NetObjectLock.m_NetView != null)
			{
				m_NetObjectLock.ReleaseLock();
			}
			m_NetObjectLock = null;
		}
		m_NetViewID = null;
		m_OnReservationRevoked = null;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		Init();
		return base.StartInit();
	}

	protected virtual void Init()
	{
		InteractiveObjectManager instance = InteractiveObjectManager.GetInstance();
		if (instance != null)
		{
			instance.AddInteracteractiveObject(this);
		}
	}

	public void SetLocalInteractionId(int id)
	{
		m_LocalInteractionID = id;
	}

	public int GetLocalInteractionId()
	{
		return m_LocalInteractionID;
	}

	public virtual bool InteractionVisibility()
	{
		return base.gameObject.activeInHierarchy && m_bInteractionVisibility;
	}

	public bool ObjectReserved()
	{
		return m_bObjectReserved;
	}

	public bool HasReservation(Character character)
	{
		return m_ReservingCharacterID == character.m_NetView.viewID;
	}

	public bool ReserveObject(Character reservingCharacter, OnReservationRevoked onReservationRevoked = null, bool forced = false)
	{
		if (reservingCharacter == null)
		{
			return false;
		}
		return ReserveObject(reservingCharacter.m_NetView.viewID, onReservationRevoked, forced);
	}

	public bool ReserveObject(int characterID, OnReservationRevoked onReservationRevoked = null, bool forced = false)
	{
		if (!m_bObjectReserved || forced)
		{
			if (m_OnReservationRevoked != null)
			{
				m_OnReservationRevoked();
				m_OnReservationRevoked = null;
			}
			m_OnReservationRevoked = onReservationRevoked;
			m_bObjectReserved = true;
			m_ReservingCharacterID = characterID;
			return true;
		}
		return false;
	}

	public void UnreserveObject(Character reservingCharacter)
	{
		if (!(reservingCharacter == null))
		{
			UnreserveObject(reservingCharacter.m_NetView.viewID);
		}
	}

	public void UnreserveObject(int characterId)
	{
		if (characterId == m_ReservingCharacterID)
		{
			m_bObjectReserved = false;
			m_ReservingCharacterID = -1;
			m_OnReservationRevoked = null;
		}
	}

	public void Interact(Character character, NetObjectLock.OnResponse OnInteractResponse = null, NetObjectLock.OnRPCKicked OnInteractionEnded = null)
	{
		if (character.IsInteractionRequestInFlight)
		{
			OnInteractResponse?.Invoke(result: false);
			OnStartInteractionFailed(character);
		}
		else
		{
			character.SetInteractionRequestInFlight(state: true);
			m_NetObjectLock.GetLock(character, this, OnStartInteraction, OnStartInteractionFailed, RequestStopInteraction, OnInteractResponse, OnInteractionEnded);
		}
	}

	public virtual void RequestStopInteraction(Character localCharacter)
	{
		OnExitInteraction(localCharacter);
	}

	public virtual void ForceStopInteraction(Character localCharacter)
	{
		OnExitInteraction(localCharacter);
	}

	public void RaiseInteractionEndedForHostMigration()
	{
		SendEvent(InteractiveEventType.InteractionEnded);
	}

	public virtual void Walk(Vector2 walk)
	{
		if (walk.magnitude > m_StopInteractionWalkThreshold)
		{
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	public virtual bool OverrideWalk()
	{
		return true;
	}

	public virtual void UpdateInteraction()
	{
	}

	protected virtual void OnStartInteraction(Character localCharacter)
	{
		if (localCharacter.m_bIsStandingOnDesk)
		{
			localCharacter.ForceClimbOffObject();
		}
		localCharacter.SetInteractingObjectRPC(this);
		m_interactingCharacter = localCharacter;
		localCharacter.SetInteractionRequestInFlight(state: false);
		m_vStartingPosition = localCharacter.transform.position;
		localCharacter.m_RigidBody.velocity = Vector3.zero;
		SendEvent(InteractiveEventType.InteractionStarted);
	}

	protected virtual void OnExitInteraction(Character localCharacter)
	{
		if (null != localCharacter)
		{
			if (localCharacter.m_OpenContainer != null && CanCloseContainer(localCharacter.m_OpenContainer))
			{
				if (localCharacter.m_CharacterStats.m_bIsPlayer)
				{
					Player player = localCharacter as Player;
					if (player != null)
					{
						player.RequestCloseContainer();
					}
				}
				else
				{
					localCharacter.m_OpenContainer = null;
				}
			}
			localCharacter.SetInteractingObjectRPC(null);
			localCharacter.SetInteractionRequestInFlight(state: false);
		}
		m_NetObjectLock.ReleaseLock();
		SendEvent(InteractiveEventType.InteractionEnded);
		m_interactingCharacter = null;
	}

	protected virtual void OnStartInteractionFailed(Character localCharacter)
	{
		localCharacter.SetInteractionRequestInFlight(state: false);
	}

	public virtual void OnCharacterFailedToStart(Character character)
	{
	}

	public virtual void Server_OnLockStatusChanged(int characterID, bool getLock)
	{
		if (getLock)
		{
			if (m_ReservingCharacterID != characterID)
			{
				ReserveObject(characterID, null, forced: true);
			}
		}
		else
		{
			UnreserveObject(characterID);
		}
	}

	public bool CheckIfInAllowedDirection(Vector3 interactPosition)
	{
		Vector3 position = base.transform.position;
		for (int i = 0; i < m_ValidInteractingDirections.Length; i++)
		{
			switch (m_ValidInteractingDirections[i])
			{
			case Directionx4.Up:
				if (interactPosition.y > position.y)
				{
					return true;
				}
				break;
			case Directionx4.Left:
				if (interactPosition.x < position.x)
				{
					return true;
				}
				break;
			case Directionx4.Down:
				if (interactPosition.y < position.y)
				{
					return true;
				}
				break;
			case Directionx4.Right:
				if (interactPosition.x > position.x)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public virtual bool AllowedToInteract(Character localCharacter)
	{
		if (localCharacter != null)
		{
			switch (m_AllowedCharacterTypes)
			{
			case CharacterResctrictions.AiOnly:
				if (localCharacter.IsPlayer())
				{
					return false;
				}
				break;
			case CharacterResctrictions.PlayersOnly:
				if (!localCharacter.IsPlayer())
				{
					return false;
				}
				break;
			}
		}
		return m_bIsEnabled && CanBeUsedForCurrentRoutine();
	}

	public virtual bool OnPlayerNotAllowedToInteract(Character localCharacter)
	{
		if (localCharacter.m_CharacterStats.m_bIsPlayer && !SatasifiesJobRoutine())
		{
			SpeechManager.GetInstance().SaySomething(localCharacter, "Text.Player.Interactions.OnlyUsableDuringJobtime", SpeechTone.Negative, 3f);
			return true;
		}
		return false;
	}

	public virtual bool CanStartOrContinueInteraction(Character localCharacter)
	{
		return true;
	}

	public virtual InteractionType GetInteractionClassType()
	{
		return InteractionType.InteractiveObject;
	}

	public virtual bool SerialiseInteractionForLoad()
	{
		return true;
	}

	public virtual bool AllowOtherPlayerHUDInteractions()
	{
		return true;
	}

	public virtual bool ShouldCancelOnOtherHUDInteractions()
	{
		return false;
	}

	public virtual bool CanCloseContainer(ItemContainer itemContainer)
	{
		return true;
	}

	public virtual bool CanKickAlreadyInteracting()
	{
		return true;
	}

	protected float GetOffsetFromFloor()
	{
		if (FloorManager.GetInstance() != null)
		{
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(base.transform.position.z);
			if (floor != null)
			{
				return base.transform.position.z - (float)floor.m_zPos;
			}
		}
		return 0f;
	}

	protected Vector3 GetEffectPositionForCharacterInInteraction(Character character)
	{
		return character.GetStatChangeEffectPosition();
	}

	public bool IsEnabled()
	{
		return m_bIsEnabled;
	}

	public void SetEnabled(bool enabled)
	{
		m_bIsEnabled = enabled;
	}

	protected bool CanBeUsedForCurrentRoutine()
	{
		return SatasifiesJobRoutine();
	}

	protected bool SatasifiesJobRoutine()
	{
		return ((!(RoutineManager.GetInstance() != null)) ? Routines.UNASSIGNED : RoutineManager.GetInstance().GetCurrentRoutineBaseType()) switch
		{
			Routines.UNASSIGNED => true, 
			Routines.JobTime => true, 
			_ => m_bCanBeUsedOutsideJobTime, 
		};
	}

	public void SetCanBeUsedOutsideJobTime(bool canBeUsed)
	{
		m_bCanBeUsedOutsideJobTime = canBeUsed;
	}

	public virtual void SetTag(uint newTag)
	{
		m_Tag = newTag;
	}

	public virtual bool LeaveCharacterPositionUnAltered()
	{
		return m_bLeaveCharacterPositionUnaltered;
	}

	public virtual void SendEvent(InteractiveEventType eventType)
	{
		InteractiveObjectManager instance = InteractiveObjectManager.GetInstance();
		if (instance != null)
		{
			instance.SendEvent(this, eventType, (short)((!(m_interactingCharacter != null)) ? (-1) : m_interactingCharacter.GetCharacterSerializerIndex()));
		}
	}

	public virtual void InteractionReadyStartEvent(Character interactingCharacter)
	{
	}

	public virtual void InteractionStartedEvent(Character interactingCharacter)
	{
	}

	public virtual void InteractionReadyEndEvent(Character interactingCharacter)
	{
	}

	public virtual void InteractionEndedEvent(Character interactingCharacter)
	{
	}

	public virtual void SetNormalizedAnimTime(float normailsedTime)
	{
	}

	public virtual void ForceNormalisedAnimTime(float normalisedTime)
	{
	}

	public virtual void ResetNormalizedAnimTime()
	{
	}

	public Vector3 GetInteractionStartPosition()
	{
		return m_vStartingPosition;
	}

	public virtual bool ShouldShowNameplateWhenNearby()
	{
		return true;
	}

	public Character GetLocalInteractingCharacter()
	{
		return m_interactingCharacter;
	}

	public virtual void OnLateJoiningInteractionCatchup(Character character)
	{
	}

	public Vector3 GetCameraOffset()
	{
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			Player player = m_interactingCharacter as Player;
			if (HUDMenuFlow.Instance.HasHorizontallySplitscreen(player.m_PlayerCameraManagerBindingID))
			{
				return m_PCSplitscreenCameraOffset;
			}
		}
		return Vector3.zero;
	}

	public virtual bool ShouldResetAnimatorWithInteractiveUser()
	{
		return false;
	}
}
