using System;
using System.Collections.Generic;
using AUTOGEN_T17Wwise_Enums;
using BitStream;
using SaveHelpers;
using UnityEngine;

public class Character : T17NetworkBehaviour, Saveable, IControlledUpdate, StencilInterface
{
	public delegate void RoomChange(RoomBlob previousRoom, RoomBlob newRoom);

	public delegate void EquippedItemHandler(Character character, Item equippedItem);

	public delegate void ReachedRoutineLocationHandler(Character character, RoutinesData.Routine routine);

	public delegate void ReceivedGiftHandler(Character gifter, int[] itemDataIDs, int money);

	public delegate void InteractHandler(InteractiveObject obj);

	public delegate void FloorChangeHandler();

	public delegate void CharacterEvent();

	public enum GamelogicRunModes
	{
		All,
		AudioOnly,
		NonAudioOnly
	}

	public struct CharacterPinData
	{
		public PinManager.Pin.PinFilterType m_FilterType;

		public Sprite m_Sprite;

		public SpriteAnimation m_Animation;

		public bool m_Edgable;

		public bool m_FloorTrackable;

		public CharacterPinData(PinManager.Pin.PinFilterType filterType, Sprite sprite, SpriteAnimation animation, bool edgeable, bool floorTrackable)
		{
			m_FilterType = filterType;
			m_Sprite = sprite;
			m_Animation = animation;
			m_Edgable = edgeable;
			m_FloorTrackable = floorTrackable;
		}
	}

	public delegate void CharacterToCharacterEvent(Character thisCharacter, Character otherCharacter);

	[Serializable]
	private class SaveData_Character_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public Vector3 P;

		public bool IPU;

		public int D4;

		public short VID;

		public int LII;

		public Vector3 LISP;

		public bool TRY;

		public bool OD;

		public byte FL;

		public ulong STS;

		public ulong STS2;

		public int KO;

		public int BND;

		public float BTM;

		public ulong ACK;

		public bool SFP;

		public bool RTLR;

		public float GTRT;

		public string EXTRA;

		public SaveData_Character_V1()
		{
			m_Version = 1;
		}
	}

	public Rigidbody m_RigidBody;

	public Transform m_Transform;

	[Header("Network")]
	public T17NetView m_NetView;

	private NetObjectLock m_NetObjectLock;

	protected Vector2 m_vNetworkedPosition;

	protected Vector2 m_vNetworkedPositionPrevious;

	protected Vector2 m_vNetworkedPositionPreviousLocal;

	protected Vector2 m_vVelocity;

	private float m_vNetworkedVelocity;

	private bool m_bCalcFacingDirection;

	private float m_fLastMessageLatency;

	private float m_fLastNetworkLocalTime;

	private float m_fLastNetworkFixedLocalTime;

	private float m_fLastSentPacketTime;

	private float m_fCurrentSentPacketTime;

	private int m_TimeSincePacketCounter;

	public const int m_MaxAmountOfPacketTimeInterpolation = 120;

	private float[] m_TimeSinceLastPacketArray;

	public float m_NextKeyFrame;

	public float m_KeyFrameDelay = 1f;

	private static int m_TimeSincePacketSmoothener = 0;

	private static bool m_UseNewPrediction = true;

	private static float timeSmootheningAmount = 3f;

	private static float predictionAmount = 0.8f;

	private static float latencyCompensationAmount = 0f;

	private static float behindCompensationDistance = 0.5f;

	private static float behindCompensationFactor = 50f;

	protected bool m_bIsDisabled;

	public bool m_bPendingRequest;

	public bool m_bSpecialStencilSkip;

	protected bool m_bInstantPositionUpdate;

	public bool m_bSerialiseInit;

	public bool m_bSpawnPointInit;

	private LevelScript.PRISON_ENUM m_CurrentLevel;

	[Header("Character Stuff")]
	public string m_CharacterName = string.Empty;

	public CharacterRole m_CharacterRole;

	public CharacterStats m_CharacterStats;

	public CharacterOpinions m_CharacterOpinions;

	public CharacterCustomisation m_CharacterCustomisation;

	public TrackableUIElementsReporter m_TrackableElementReporter;

	public bool m_bActionRenderersRequired;

	public float m_ProximityPenalty = 1f;

	public ProximityPriorityLayers m_ConsciousProximityLayer = ProximityPriorityLayers.Lowest;

	public ProximityPriorityLayers m_UnconsciousProximityLayer;

	[HideInInspector]
	public bool m_bIsRobinsonCharacter;

	[Header("Item Stuff")]
	public ItemContainer m_ItemContainer;

	public CharacterEvent OnOutfitChanged;

	public CharacterEvent OnEquipedItemChanged;

	private Item m_Outfit;

	protected Item m_EquippedItem;

	private int m_OutfitRequestId = -1;

	private int m_WeaponRequestId = -1;

	private int m_ItemEventID = -1;

	[Header("Animation and Movement")]
	public CharacterMovement m_CharacterMovement;

	public CharacterAnimator m_CharacterAnimator;

	protected Vector2 m_WalkVector;

	private Vector2 m_vFacingDirection = Vector2.up;

	public Directionx4 m_x4FacingDirection;

	public Directionx8 m_x8FacingDirection;

	private float m_fPauseMovementTimer;

	protected Vector3 m_PreviousPosition = Vector3.zero;

	public float m_StandingStillTimeout = 5f;

	private float m_StandingStillTime;

	private float m_StandingStillTimerVar = 1f;

	private AnimState m_StandingStillAnimState = AnimState.Hammer;

	private int m_StandingStillEquipID;

	private Transform m_AnimationTransform;

	protected AnimatedInteraction m_CurrentSerializedAnimatedInteraction;

	protected AnimatedInteraction m_CurrentDeserializedAnimatedInteraction;

	private int m_LastAppliedDeserialiseAnimatedInteractionState;

	private bool m_EnableLayerAnimator;

	public static readonly Vector3 m_DefaultAnimatorPosition = new Vector3(0f, -0.27f, -0.8f);

	private float m_AnimatedInteractionLocalZ;

	private bool m_bItemInUse;

	private static readonly Vector2 m_EffectOffsetStat = new Vector3(-0.2f, 0.75f);

	public int m_WallLayerMask;

	public int m_NoKnockbackLayerMask;

	private List<CharacterPinData> m_RequiredPins = new List<CharacterPinData>();

	private CharacterPinData m_ActivePin;

	[Header("Vision")]
	public float m_fFoV = 1.8f;

	public float m_fTouchingVisionRadius = 0.7f;

	public float m_fVisionDistance = 10f;

	public CharacterUtil m_CharacterUtil;

	public bool m_bIsHidden;

	[Header("World / Interaction")]
	private bool m_bIsInteractionRequestInFlight;

	private float m_InteractionRequestTimeStamp;

	private const float REQUEST_IN_FLIGHT_TIMEOUT_SECONDS = 10f;

	protected int m_PendingInteractingObjectNetID = -1;

	protected int m_PendingInteractingID = -1;

	protected Vector3 m_PendingInteractionStartPosition;

	protected InteractiveObject m_RemoteInteractingObject;

	protected InteractiveObject m_InteractingObject;

	public int m_MC_NetObjectLockID = -1;

	public ProximityDetector m_ProximityDetector;

	public MouseDetector m_MouseDetector;

	private static bool m_bDebugNoRountinePenalty = false;

	private RoomBlob _m_RoutineTargetLocation;

	private const float IS_TARDY_REFRESH_INTERVAL = 0.25f;

	private float m_TimeUntilTardyRefreshCheck;

	protected bool m_bRoutineTargetLocationReached = true;

	protected bool m_bSnapshotIsBeingRestored;

	private float m_fGetToRoutineTimer;

	[Header("Event")]
	public CharacterEventManager m_CharacterEventManager;

	[Header("Physics")]
	public GameObject m_PhysicsCollider;

	public SphereCollider m_PhysicsSphereCol;

	protected static LayerMask m_CharacterLayerMask;

	public Transform m_CharacterTrigger;

	public SphereCollider m_CharacterSphereTrigger;

	private List<Character> m_CharactersToHitCache = new List<Character>();

	private const int MAX_COLLISION_CHECK_COLLIDERS = 8;

	private Collider[] m_LastCollisionCheckResults = new Collider[8];

	private float m_fSmashAttackChargeTimer;

	protected float m_fKnockBackStunTimer;

	private float m_fAttackRecoveryTime;

	private float m_fBoundEscapeTime;

	private float m_fTimeLastHit;

	public static CharacterComparer CharacterTComparer = default(CharacterComparer);

	private Dictionary<Character, float> m_LastTimeAttackedBy = new Dictionary<Character, float>(CharacterTComparer);

	public float m_CarryOffset = -0.04f;

	public float m_OnDeskOffset = -0.5f;

	protected CarryObjectInteraction m_CarriedObject;

	protected Character m_CarriedCharacter;

	[Header("UI")]
	private Character _m_CharacterTarget;

	public CharacterSpeechBubbleHandler m_SpeechBubbleHandler;

	public CharacterIconHandler m_IconHandler;

	private bool _m_bIsKnockedOut;

	private bool _m_bIsBound;

	private bool _m_bIsNaked;

	private bool _m_bHasContraband;

	private bool _m_bIsNaughtyLocation;

	private bool _m_bIsTardy;

	private bool _m_bIsMissing;

	private bool _m_bIsStandingOnDesk;

	private bool _m_bIsWanted;

	private bool _m_bIsSuspicious;

	private bool _m_bIsDigging;

	private bool _m_bIsChipping;

	private bool _m_bIsCutting;

	private bool _m_bIsSearchingDesk;

	private bool _m_bIsLooting;

	private float m_IsLootingTime = 0.3f;

	private float m_IsLootingTimer;

	private bool _m_bIsAttacking;

	private float m_IsAttackingTime = 0.3f;

	private float m_IsAttackingTimer;

	private bool _m_bIsDisguised;

	private bool _m_WearingDisguise;

	private bool m_bIsRegisteredForTrackingUI;

	private bool _m_bHasQuestAvailable;

	private bool _m_bIsVendor;

	private bool _m_bIsWantedForSolitary;

	private bool _m_IsPreparingToBeCarried;

	protected bool m_bIsGamerControlled;

	private bool _m_bHaveAnyQuotaDone;

	public bool m_bIsBlocking;

	public bool m_bIsDashing;

	public bool m_bStartingToClimb;

	private Character m_PickedUpBy;

	private bool m_bHasTray;

	private bool m_bBusy;

	protected bool m_bIsBeingDestroyed;

	private RoomBlob m_JobRoom;

	private bool m_JobComplete;

	public ItemContainer m_OpenContainer;

	private ClimbableObject m_ClimbableObject;

	protected Dictionary<int, FastList<Item>> m_AllowedDoors = new Dictionary<int, FastList<Item>>();

	private Item m_AccessKey;

	private int m_AccessKeySubCode;

	public CharacterToCharacterEvent OnCharacterKnockedOut;

	public CharacterToCharacterEvent OnCharacterSetTargetCharacter;

	public CharacterToCharacterEvent OnCharacterTookDamage;

	public CharacterToCharacterEvent OnCharacterTiedUp;

	private static bool DEBUG_TELEPORT_QUESTAI_TO_PLAYER = false;

	public static float DEBUG_InteractingZOffset = 0f;

	private SaveDataRegister m_SaveData;

	private BitField m_SLZSerializer = new BitField();

	private BitField m_SLZDeserializer = new BitField();

	private bool m_bForceSerialize;

	private int m_LastReadFrameCount = -1;

	private int m_LastLocationUpdateFrameCount = -1;

	private float m_SuspiciousStateTimer;

	public Sprite m_MapIcon;

	protected int m_PinID = -1;

	public CharacterSerializer.CharacterSerializerListType m_PreviousSerializeRate = CharacterSerializer.CharacterSerializerListType.Low;

	public CharacterSerializer.CharacterSerializerListType m_SerializeRate = CharacterSerializer.CharacterSerializerListType.Low;

	public CharacterSerializer.CharacterSerializerListType m_SerializeRateOverride = CharacterSerializer.CharacterSerializerListType.COUNT;

	public bool m_bCurrentPositionDirty = true;

	private Vector3 _m_CachedCurrentPosition;

	public float m_CharacterID;

	private static List<Character> m_AllCharacters = new List<Character>();

	public static int TOTAL_INMATE_COUNT = 0;

	private static TrayInteraction m_TrayInteractionCache = null;

	private Vector3 m_CachedMapTopLeft;

	private Vector3 m_CachedMapBottomRight = Vector3.zero;

	private int m_LastCloseFrame;

	protected FloorManager.Floor m_CurrentFloor;

	protected int m_CurrentTileRow;

	protected int m_CurrentTileColumn;

	private RoomBlob m_MyCell;

	private BedEventManager m_CharacterBedEventManager;

	private int m_targetTileRow = -1;

	private int m_targetTileColumn = -1;

	private RoomBlob _m_MasterClientCurrentLocation;

	private Vector3 m_MasterClientLocationPos;

	public int m_isInside;

	private bool m_bExitSolitaryFreePass;

	private RoomBlob _m_CurrentLocation;

	private static int numUpdatedLocks = 0;

	private bool m_bPurpleDoorLocksChanged;

	private static float x4Test = Mathf.Tan((float)Math.PI / 4f);

	private static float x8TestLow = Mathf.Tan((float)Math.PI / 8f);

	private static float x8TestHigh = Mathf.Tan((float)Math.PI * 3f / 8f);

	private int m_CharacterListIndex = -1;

	private int m_SLZ_NetObjectLockID = -1;

	private BitStreamWriter m_NetSerializeWriter;

	private FastList<byte> m_NetSerializeByteList = new FastList<byte>();

	private int m_SLZ_CharacterState;

	private int m_SLZ_koCharacterNetID;

	private int m_SLZ_boundCharacterNetID;

	private float m_SLZ_GetToRoutineTimer;

	private int m_SLZ_KeyNetID = -1;

	private int m_SLZ_KeySubCode;

	private int m_SLZ_PreviousDeserializeCharacterStateInt;

	private int m_SLZ_floorIndex;

	private float m_SLZ_AnimatedZ;

	private bool m_bHijackedAnimatorActive;

	private int m_CharacterSerializeIndex = -1;

	public Vector2 WalkVector => m_WalkVector;

	public AnimatedInteraction CurrentSerializedAnimatedInteraction => m_CurrentSerializedAnimatedInteraction;

	public AnimatedInteraction CurrentDeserializedAnimatedInteraction => m_CurrentDeserializedAnimatedInteraction;

	public bool IsInteractionRequestInFlight => m_bIsInteractionRequestInFlight && m_InteractionRequestTimeStamp + 10f > UpdateManager.time;

	protected RoomBlob m_RoutineTargetLocation
	{
		get
		{
			return _m_RoutineTargetLocation;
		}
		set
		{
			_m_RoutineTargetLocation = value;
			if (!m_bSnapshotIsBeingRestored)
			{
				m_bRoutineTargetLocationReached = _m_RoutineTargetLocation == null || _m_RoutineTargetLocation == m_CurrentLocation;
			}
		}
	}

	public bool HasReachedRoutineLocation => m_bRoutineTargetLocationReached;

	public float GetToRoutineTimer => m_fGetToRoutineTimer;

	public float SmashAttackChargeTimer => m_fSmashAttackChargeTimer;

	public Character m_CharacterTarget => _m_CharacterTarget;

	public bool m_bIsKnockedOut => _m_bIsKnockedOut;

	public bool m_bIsBound => _m_bIsBound;

	public bool m_bIsNaked => _m_bIsNaked;

	public bool m_bHasContraband => _m_bHasContraband;

	public bool m_bIsNaughtyLocation => _m_bIsNaughtyLocation;

	public bool m_bIsTardy => _m_bIsTardy;

	public bool m_bIsMissing => _m_bIsMissing;

	public bool m_bIsStandingOnDesk => _m_bIsStandingOnDesk;

	public bool m_bIsCarryingObject => IsCarrying();

	public bool m_bIsWanted => _m_bIsWanted;

	public bool m_bIsSuspicious => _m_bIsSuspicious;

	public bool m_bIsDigging => _m_bIsDigging;

	public bool m_bIsChipping => _m_bIsChipping;

	public bool m_bIsCutting => _m_bIsCutting;

	public bool m_bIsSearchingDesk => _m_bIsSearchingDesk;

	public bool m_bIsLooting => _m_bIsLooting;

	public bool m_bIsAttacking => _m_bIsAttacking;

	public bool m_bIsDisguised => _m_bIsDisguised;

	public bool m_WearingDisguise => _m_WearingDisguise;

	public bool m_bHasQuestAvailable => _m_bHasQuestAvailable;

	public bool m_bIsVendor => _m_bIsVendor;

	public bool m_bIsWantedForSolitary => _m_bIsWantedForSolitary;

	public bool IsPreparingToBeCarried
	{
		get
		{
			return _m_IsPreparingToBeCarried;
		}
		set
		{
			_m_IsPreparingToBeCarried = value;
		}
	}

	public bool m_bHaveAnyQuotaDone => _m_bHaveAnyQuotaDone;

	public Vector3 m_CachedCurrentPosition
	{
		get
		{
			if (m_bCurrentPositionDirty)
			{
				_m_CachedCurrentPosition = m_Transform.position;
				m_bCurrentPositionDirty = false;
			}
			return _m_CachedCurrentPosition;
		}
		set
		{
			m_bCurrentPositionDirty = false;
			_m_CachedCurrentPosition = value;
		}
	}

	public FloorManager.Floor CurrentFloor
	{
		get
		{
			if (m_CurrentFloor == null)
			{
				FloorManager instance = FloorManager.GetInstance();
				if (instance != null)
				{
					m_CurrentFloor = instance.FindFloorAtZ(m_CachedCurrentPosition.z);
				}
			}
			return m_CurrentFloor;
		}
		set
		{
			m_CurrentFloor = value;
		}
	}

	public RoomBlob m_MasterClientCurrentLocation
	{
		get
		{
			return _m_MasterClientCurrentLocation;
		}
		set
		{
			if (value != _m_MasterClientCurrentLocation)
			{
				if (_m_MasterClientCurrentLocation != null)
				{
					_m_MasterClientCurrentLocation.ExitRoom(this);
				}
				if (value != null)
				{
					value.EnterRoom(this);
				}
				_m_MasterClientCurrentLocation = value;
			}
		}
	}

	public RoomBlob m_CurrentLocation
	{
		get
		{
			return _m_CurrentLocation;
		}
		set
		{
			if (value != _m_CurrentLocation)
			{
				RoomBlob previousRoom = _m_CurrentLocation;
				_m_CurrentLocation = value;
				if (this.OnRoomChanged != null)
				{
					this.OnRoomChanged(previousRoom, value);
				}
				m_isInside = ((value != null && value.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors) ? 1 : 0);
			}
		}
	}

	public event RoomChange OnRoomChanged;

	public event EquippedItemHandler EquippedItemChangedEvent;

	public event ReachedRoutineLocationHandler ReachedRoutineLocationEvent;

	public event ReachedRoutineLocationHandler MissedRoutineLocationEvent;

	public event ReceivedGiftHandler ReceivedGiftEvent;

	public event InteractHandler OnInteractEvent;

	public event FloorChangeHandler OnFloorChangedEvent;

	public float GetTimeLastHit()
	{
		return m_fTimeLastHit;
	}

	public virtual void SetCarriedObject(CarryObjectInteraction obj)
	{
		m_CarriedObject = obj;
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsCarryingObject(IsCarryingSomethingNaughty());
		}
	}

	protected virtual void SetCarriedCharacter(Character character)
	{
		m_CarriedCharacter = character;
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsCarryingObject(IsCarryingSomethingNaughty());
		}
	}

	public bool IsCarrying()
	{
		return null != m_CarriedObject || null != m_CarriedCharacter;
	}

	public bool IsCarryingObject()
	{
		return m_CarriedObject != null;
	}

	public StencilInterface GetCarrying()
	{
		return m_CarriedCharacter;
	}

	public CarryObjectInteraction GetCarriedObject()
	{
		return m_CarriedObject;
	}

	public Character GetCarryingCharacter()
	{
		return m_CarriedCharacter;
	}

	private bool IsCarryingSomethingNaughty()
	{
		if (null != m_CarriedObject && m_CarriedObject.m_Decoration == CarryObjectInteraction.AI_Decorations.Job && m_CurrentLocation == m_JobRoom)
		{
			return false;
		}
		if (_m_bIsDisguised)
		{
			return false;
		}
		return IsCarrying();
	}

	public bool GetIsGamerControlled()
	{
		return m_bIsGamerControlled;
	}

	public virtual bool GetIsImmobilised()
	{
		return m_bIsKnockedOut || m_bIsBound || m_bBusy || m_bStartingToClimb || m_fPauseMovementTimer > 0f || m_fKnockBackStunTimer > 0f || (m_EquippedItem != null && m_EquippedItem.IsImmobilisingOwner());
	}

	public bool GetIsKnockedOut()
	{
		return m_bIsKnockedOut;
	}

	public bool GetIsSleeping()
	{
		if (m_CharacterStats == null)
		{
			return false;
		}
		StatModifierEnum characterState = m_CharacterStats.GetCharacterState();
		return characterState == StatModifierEnum.Sleeping || characterState == StatModifierEnum.SleepingInOwnBed || characterState == StatModifierEnum.MedicalSleeping;
	}

	public bool GetIsSleepingInOwnBed()
	{
		StatModifierEnum characterState = m_CharacterStats.GetCharacterState();
		return characterState == StatModifierEnum.SleepingInOwnBed;
	}

	public bool GetIsMedicalSleeping()
	{
		StatModifierEnum characterState = m_CharacterStats.GetCharacterState();
		return characterState == StatModifierEnum.MedicalSleeping;
	}

	public static List<Character> GetAllCharacters()
	{
		return m_AllCharacters;
	}

	public void SetIsAttacking(bool attacking)
	{
		if (_m_bIsAttacking == attacking)
		{
			return;
		}
		_m_bIsAttacking = attacking;
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsAttacking(m_bIsAttacking);
		}
		m_IsAttackingTimer = m_IsAttackingTime;
		if (_m_bIsAttacking)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Attacking);
			}
		}
	}

	public virtual void SetIsKnockedOut(bool knockedOut, Character characterResponsible)
	{
		if (_m_bIsKnockedOut != knockedOut)
		{
			_m_bIsKnockedOut = knockedOut;
			if (knockedOut)
			{
				m_CharacterAnimator.StartAnimation(AnimState.Knockout);
				m_CharacterAnimator.ShowSpotlightKnockedOut(enabled: false);
			}
			else
			{
				m_CharacterAnimator.StopAnimation(AnimState.Knockout);
				m_CharacterAnimator.ShowSpotlightKnockedOut(enabled: true);
			}
			if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsKnockedOut(m_bIsKnockedOut, characterResponsible);
			}
			if (knockedOut && m_ClimbableObject != null)
			{
				m_ClimbableObject.ForceCharacterOffObject(this, shouldRepositionForDismount: false);
			}
			if (!knockedOut && T17NetManager.IsMasterClient && _m_bIsWantedForSolitary && !IsInSolitary() && (m_CurrentLocation == null || m_CurrentLocation.GetRoomBlobData<RoomBlob_Solitary>() == null))
			{
				NPCManager.GetInstance().RespondToKnownEscapeAttempt(this);
			}
			UpdateProximityLayer();
		}
	}

	public void SetIsBound(bool isBound, Item itemResponsible, Character characterResponsible)
	{
		if (_m_bIsBound == isBound)
		{
			return;
		}
		_m_bIsBound = isBound;
		if (isBound)
		{
			m_CharacterAnimator.StartAnimation(AnimState.Bound);
			if (itemResponsible != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(itemResponsible.BoundMaterial, ItemData.ITEM_ANIMATION_TYPE.IAT_SINGLE);
			}
			if (OnCharacterTiedUp != null)
			{
				OnCharacterTiedUp(this, characterResponsible);
			}
		}
		else
		{
			m_CharacterAnimator.StopAnimation(AnimState.Bound);
			if (m_EquippedItem != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(m_EquippedItem.HeldMaterial, m_EquippedItem.HeldType);
			}
			else
			{
				m_CharacterAnimator.SetMaterialHandHeld(null);
			}
		}
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsBound(_m_bIsBound, characterResponsible);
		}
	}

	public virtual void SetIsSurrendered(bool surrendered)
	{
		if (surrendered)
		{
			m_NetView.RPC("RPC_SetIsSurrendered", NetTargets.MasterClient, surrendered);
			DamageSelf(this, 200f);
			m_CharacterAnimator.StartAnimation(AnimState.Surrender);
		}
		else
		{
			m_CharacterAnimator.StopAnimation(AnimState.Surrender);
		}
	}

	[PunRPC]
	public void RPC_SetIsSurrendered(bool surrendered, PhotonMessageInfo info)
	{
		if (surrendered)
		{
			SolitaryManager.GetInstance().SetWantedForSolitary(this, sendToSolitary: true, bSurrendered: true);
		}
	}

	public void SetIsNaked(bool value)
	{
		if (_m_bIsNaked == value)
		{
			return;
		}
		_m_bIsNaked = value;
		UpdateNakedEvent();
		UpdateNakedVisuals();
		if (T17NetManager.IsMasterClient && _m_bIsNaked)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Naked);
			}
		}
		if (m_NetView.isMine && _m_bIsNaked)
		{
			CheckForNakedDinnerAchievement();
		}
	}

	private void UpdateNakedEvent()
	{
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
		{
			bool flag = m_CurrentLocation != null && m_CurrentLocation.location == RoomBlob.eLocation.Shower;
			bool enable = _m_bIsNaked && !flag;
			m_CharacterEventManager.IsNaked(enable);
		}
	}

	public void SetIsNaughtyLocation(bool value)
	{
		if (_m_bIsNaughtyLocation == value)
		{
			return;
		}
		_m_bIsNaughtyLocation = value;
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		bool flag = _m_bIsNaughtyLocation;
		if (_m_bIsDisguised)
		{
			flag = false;
		}
		if (m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsNaughtyLocation(flag);
		}
		if (flag)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_NaughtyLocation);
			}
		}
	}

	public void SetHasContraband(bool value)
	{
		_m_bHasContraband = value;
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null && m_CharacterStats.m_bIsPlayer)
		{
			m_CharacterEventManager.HasContraband(_m_bHasContraband);
		}
	}

	public void SetIsTardy(bool value)
	{
		_m_bIsTardy = value;
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsTardy;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsTardy(enable);
			}
		}
	}

	public void SetIsMissing(bool value)
	{
		_m_bIsMissing = value;
		if (T17NetManager.IsMasterClient && m_CharacterBedEventManager != null)
		{
			m_CharacterBedEventManager.IsMissing(_m_bIsMissing);
		}
	}

	public void SetIsStandingOnDesk(bool value)
	{
		if (_m_bIsStandingOnDesk != value)
		{
			_m_bIsStandingOnDesk = value;
			OnStandingOnDeskChange(_m_bIsStandingOnDesk);
		}
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsStandingOnDesk;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsStandingOnDesk(enable);
			}
		}
	}

	public void SetIsWanted(bool value)
	{
		_m_bIsWanted = value;
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsWanted;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsWanted(enable);
			}
		}
	}

	public void SetIsSuspicious(bool value)
	{
		_m_bIsSuspicious = value;
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsSuspicious;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsSuspicious(enable);
			}
		}
	}

	public void SetIsDigging(bool value)
	{
		if (_m_bIsDigging == value)
		{
			return;
		}
		_m_bIsDigging = value;
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsDigging(_m_bIsDigging);
		}
		if (_m_bIsDigging)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Digging);
			}
		}
	}

	public void SetIsChipping(bool value)
	{
		if (_m_bIsChipping == value)
		{
			return;
		}
		_m_bIsChipping = value;
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsChipping(_m_bIsChipping);
		}
		if (_m_bIsChipping)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Chipping);
			}
		}
	}

	public void SetIsCutting(bool value)
	{
		if (_m_bIsCutting == value)
		{
			return;
		}
		_m_bIsCutting = value;
		if (!T17NetManager.IsMasterClient)
		{
			return;
		}
		if (m_CharacterEventManager != null)
		{
			m_CharacterEventManager.IsCutting(_m_bIsCutting);
		}
		if (_m_bIsCutting)
		{
			GuardTowerManager instance = GuardTowerManager.GetInstance();
			if (instance != null)
			{
				instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Cutting);
			}
		}
	}

	public void SetIsLooting(bool value)
	{
		if (_m_bIsLooting == value)
		{
			return;
		}
		_m_bIsLooting = value;
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsLooting;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsLooting(enable);
			}
			m_IsLootingTimer = m_IsLootingTime;
		}
	}

	public void SetIsSearchingDesk(bool value)
	{
		_m_bIsSearchingDesk = value;
		if (T17NetManager.IsMasterClient)
		{
			bool enable = _m_bIsSearchingDesk;
			if (_m_bIsDisguised)
			{
				enable = false;
			}
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsSearchingDesk(enable);
			}
		}
	}

	public void SetIsDisguised(bool value)
	{
		_m_bIsDisguised = value;
		if (T17NetManager.IsMasterClient)
		{
			if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.IsDisguised(_m_bIsDisguised);
			}
			SetCarriedCharacter(m_CarriedCharacter);
			SetCarriedObject(m_CarriedObject);
			_m_bIsNaughtyLocation = !_m_bIsNaughtyLocation;
			SetIsNaughtyLocation(!_m_bIsNaughtyLocation);
			_m_bIsTardy = !_m_bIsTardy;
			SetIsTardy(!_m_bIsTardy);
			_m_bIsStandingOnDesk = !_m_bIsStandingOnDesk;
			SetIsStandingOnDesk(!_m_bIsStandingOnDesk);
			_m_bIsWanted = !_m_bIsWanted;
			SetIsWanted(!_m_bIsWanted);
			_m_bIsSuspicious = !_m_bIsSuspicious;
			SetIsSuspicious(!_m_bIsSuspicious);
			_m_bIsLooting = !_m_bIsLooting;
			SetIsLooting(!_m_bIsLooting);
			_m_bIsSearchingDesk = !_m_bIsSearchingDesk;
			SetIsSearchingDesk(!_m_bIsSearchingDesk);
		}
	}

	public void SetClimbableObject(ClimbableObject climbableObject)
	{
		if (!(m_ClimbableObject == climbableObject))
		{
			if (m_ClimbableObject != null)
			{
				m_ClimbableObject.SetCharacterOnUs(this, onOff: false);
			}
			m_ClimbableObject = climbableObject;
			if (m_ClimbableObject != null)
			{
				m_ClimbableObject.SetCharacterOnUs(this, onOff: true);
			}
		}
	}

	public bool IsClimbingOnObject()
	{
		return m_ClimbableObject != null;
	}

	public void SetHasQuest(bool value)
	{
		_m_bHasQuestAvailable = value;
		if (!DEBUG_TELEPORT_QUESTAI_TO_PLAYER || !(GetComponent<AIMovement>() != null))
		{
			return;
		}
		List<Player> allPlayers = Player.GetAllPlayers();
		int i = 0;
		for (int count = allPlayers.Count; i < count; i++)
		{
			Player player = allPlayers[i];
			if (player.m_Gamer != null)
			{
				Teleport(player.m_CachedCurrentPosition);
				PauseMovement((!_m_bHasQuestAvailable) ? 0f : 10f);
			}
		}
	}

	public void SetIsVendor(bool value)
	{
		_m_bIsVendor = value;
	}

	public void SetIsWantedForSolitary(bool value)
	{
		if (_m_bIsWantedForSolitary == value)
		{
			return;
		}
		_m_bIsWantedForSolitary = value;
		if (_m_bIsWantedForSolitary)
		{
			if (T17NetManager.IsMasterClient)
			{
				GuardTowerManager instance = GuardTowerManager.GetInstance();
				if (instance != null)
				{
					instance.AlertGuardTowerRPC(this, AIEvent.EventType.Character_Wanted);
				}
			}
		}
		else if (m_NetView.isMine)
		{
			OnCharacterReleasedFromSolitary();
		}
	}

	public void OnCharacterReleasedFromSolitary()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			ResetGetToRoutineTimer();
			SetupTargetRoom(instance.GetCurrentRoutineBaseType());
		}
	}

	public bool IsInSolitary()
	{
		SolitaryManager instance = SolitaryManager.GetInstance();
		if (instance != null)
		{
			return m_bIsWantedForSolitary && instance.GetTimeRemainining(this) > 0f;
		}
		return false;
	}

	public void SetHasTray(bool hasTray, Material trayMat = null, bool force = false)
	{
		if (m_bHasTray == hasTray && !force)
		{
			return;
		}
		m_bHasTray = hasTray;
		if (hasTray)
		{
			m_bActionRenderersRequired = true;
			if (m_CharacterAnimator != null)
			{
				m_CharacterAnimator.TrayStateChanged(TrayState.With);
			}
			if (trayMat == null)
			{
				if (m_TrayInteractionCache == null)
				{
					RoomBlob firstRoomByLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.MealHall);
					if (firstRoomByLocation != null)
					{
						List<InteractiveObject> list = firstRoomByLocation.FindObject(searchFreeTime: false, isInmate: true, typeof(TrayInteraction));
						if (list != null && list.Count > 0)
						{
							m_TrayInteractionCache = list[0] as TrayInteraction;
						}
					}
				}
				if (m_TrayInteractionCache != null)
				{
					trayMat = m_TrayInteractionCache.GetRandomTrayMaterial();
				}
				if (!(trayMat == null))
				{
				}
			}
			if (m_CharacterAnimator != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(trayMat, ItemData.ITEM_ANIMATION_TYPE.IAT_SINGLE);
			}
		}
		else if (m_CharacterAnimator != null)
		{
			m_bActionRenderersRequired = true;
			m_CharacterAnimator.TrayStateChanged(TrayState.Without);
			if (m_EquippedItem != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(m_EquippedItem.HeldMaterial, m_EquippedItem.HeldType);
			}
			else
			{
				m_CharacterAnimator.SetMaterialHandHeld(null);
			}
		}
	}

	public bool GetHasTray()
	{
		return m_bHasTray;
	}

	public void UpdateNakedVisuals()
	{
		bool forceNaked = _m_bIsNaked || m_CharacterStats.m_CharacterState == StatModifierEnum.Shower;
		if (m_CharacterCustomisation != null)
		{
			m_CharacterCustomisation.SetForceNaked(forceNaked);
			if (m_EquippedItem != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(m_EquippedItem.HeldMaterial, m_EquippedItem.HeldType);
			}
			else
			{
				m_CharacterAnimator.SetMaterialHandHeld(null);
			}
		}
	}

	public StencilInterface GetPickedUpBy()
	{
		return m_PickedUpBy;
	}

	public Character GetPickedUpByCharacter()
	{
		return m_PickedUpBy;
	}

	public bool IsBeingCarried()
	{
		return null != m_PickedUpBy;
	}

	public virtual void SetPickedUp(Character carrier)
	{
		m_PickedUpBy = carrier;
		if (null != m_PickedUpBy)
		{
			m_PickedUpBy.SetCarriedCharacter(this);
		}
		m_CharacterAnimator.StartAnimation(AnimState.KnockoutHold);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item.ToString(), base.gameObject);
	}

	public bool GetIsHiddenOrDisabled()
	{
		return m_bIsHidden || m_bIsDisabled;
	}

	public bool ConsiderForCloseCheck()
	{
		return !m_bSpecialStencilSkip;
	}

	public void SetLastCloseFrame(int framenum)
	{
		m_LastCloseFrame = 0;
	}

	public int GetLastCloseFrame()
	{
		return m_LastCloseFrame;
	}

	public virtual void SetDropped(Vector3 dropPosition)
	{
		if (m_PickedUpBy != null)
		{
			m_PickedUpBy.SetCarriedCharacter(null);
			m_PickedUpBy = null;
			m_CharacterAnimator.StopAnimation(AnimState.KnockoutHold);
			Teleport(dropPosition);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_KO, base.gameObject);
		}
	}

	private void UpdatePickedUp()
	{
		if (!(m_PickedUpBy != null))
		{
			return;
		}
		if (_m_bIsKnockedOut)
		{
			Teleport(m_PickedUpBy.m_CachedCurrentPosition + CarryInteraction.m_vCarryOffset);
			m_CharacterAnimator.HeadAndBodyFaceDirection(m_PickedUpBy.m_x4FacingDirection);
			if (m_TrackableElementReporter != null)
			{
				m_TrackableElementReporter.UpdateTrackedElementPositions();
			}
		}
		else
		{
			SetDropped(m_CachedCurrentPosition - CarryInteraction.m_vCarryOffset);
		}
	}

	public void SetBusyRPC(bool busy)
	{
		RPC_SetBusy(busy);
		m_NetView.PostLevelLoadRPC("RPC_SetBusy", NetTargets.Others, busy);
	}

	[PunRPC]
	public void RPC_SetBusy(bool busy)
	{
		m_bBusy = busy;
	}

	public virtual void SetIsDisabled(bool disabled)
	{
		if (disabled != m_bIsDisabled)
		{
			m_bIsDisabled = disabled;
			if (!m_bIsDisabled)
			{
				RetriggerEvents();
			}
			else if (m_CharacterEventManager != null)
			{
				m_CharacterEventManager.OnCharacterDisabled();
			}
		}
	}

	public bool GetIsDisabled()
	{
		return m_bIsDisabled;
	}

	public virtual void SetJobRoom(RoomBlob jobRoom)
	{
		m_JobRoom = jobRoom;
	}

	public RoomBlob GetJobRoom()
	{
		return m_JobRoom;
	}

	public virtual void SetJobComplete(bool jobComplete)
	{
		m_JobComplete = jobComplete;
	}

	public bool GetJobComplete()
	{
		return m_JobComplete;
	}

	public virtual void SetHaveAnyQuotaDone(bool haveAnyQuotaDone)
	{
		_m_bHaveAnyQuotaDone = haveAnyQuotaDone;
	}

	public virtual void AddAllowedDoor(Door door, Item itemAllowingAccess = null)
	{
		int local_DoorID = door.Local_DoorID;
		if (!m_AllowedDoors.ContainsKey(local_DoorID))
		{
			m_AllowedDoors.Add(local_DoorID, new FastList<Item>(8));
		}
		if (!m_AllowedDoors[local_DoorID].Contains(itemAllowingAccess))
		{
			m_AllowedDoors[local_DoorID].Add(itemAllowingAccess);
		}
	}

	public virtual void RemoveAllowedDoor(int doorID)
	{
		if (m_AllowedDoors.ContainsKey(doorID))
		{
			m_AllowedDoors[doorID].Clear();
		}
	}

	public virtual void ClearAllowedDoors()
	{
		Dictionary<int, FastList<Item>>.Enumerator enumerator = m_AllowedDoors.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Clear();
		}
	}

	public bool IsAllowedThroughDoor(int doorID)
	{
		if (!m_AllowedDoors.ContainsKey(doorID))
		{
			return false;
		}
		if (m_AllowedDoors[doorID].Count == 0)
		{
			return false;
		}
		return true;
	}

	public FastList<Item> GetItemsAllowingThroughDoor(int doorID)
	{
		if (m_AllowedDoors.ContainsKey(doorID))
		{
			return m_AllowedDoors[doorID];
		}
		return null;
	}

	protected virtual void RoomChanged(RoomBlob previousRoom, RoomBlob newRoom)
	{
		if (newRoom == null)
		{
			SetIsNaughtyLocation(value: false);
			if (m_bHasTray)
			{
				SetHasTray(hasTray: false);
			}
			if (IsPlayer())
			{
				AudioController.SetSwitch(Switch_Group.Player_Footsteps, "Concrete", base.gameObject);
			}
			return;
		}
		if (IsPlayer() && (previousRoom == null || newRoom.m_FloorMaterial != previousRoom.m_FloorMaterial))
		{
			AudioController.SetSwitch(Switch_Group.Player_Footsteps, newRoom.m_FloorMaterial.ToString(), base.gameObject);
		}
		if (m_CharacterRole == CharacterRole.Inmate && m_bHasTray && newRoom.location != RoomBlob.eLocation.MealHall)
		{
			SetHasTray(hasTray: false);
		}
		if (m_NetView != null && m_NetView.isMine)
		{
			if (m_CharacterRole == CharacterRole.Inmate)
			{
				bool isNaughtyLocation = LocationIsNaughty(newRoom);
				SetIsNaughtyLocation(isNaughtyLocation);
				if ((previousRoom != null && previousRoom.location == RoomBlob.eLocation.Shower) || (newRoom != null && newRoom.location == RoomBlob.eLocation.Shower))
				{
					UpdateNakedEvent();
				}
				if ((previousRoom == null || previousRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors) && newRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Outdoors)
				{
					GuardTowerManager.GetInstance().AlertGuardTowerRPC(this);
				}
			}
			if (newRoom == m_RoutineTargetLocation)
			{
				if (!m_bRoutineTargetLocationReached && IsPlayer())
				{
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_Routine_Success, base.gameObject);
				}
				CheckForNakedDinnerAchievement();
				bool bRoutineTargetLocationReached = m_bRoutineTargetLocationReached;
				m_bRoutineTargetLocationReached = true;
				m_fGetToRoutineTimer = 0f;
				SetIsTardy(value: false);
				SetIsMissing(value: false);
				if (!bRoutineTargetLocationReached && ((RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.JobTime && JobsManager.GetInstance().GetCharactersJob(this) == null) || RoutineManager.GetInstance().GetCurrentRoutineBaseType() != Routines.JobTime))
				{
					HandleRoutineReachedEvent(RoutineManager.GetInstance().GetCurrentRoutine());
				}
			}
			else if (RoutineManager.GetInstance() != null && m_fGetToRoutineTimer <= 0f)
			{
				Routines currentRoutineBaseType = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
				if (currentRoutineBaseType == Routines.Lockdown || currentRoutineBaseType == Routines.LightsOut)
				{
					SetIsTardy(value: true);
					SetIsMissing(value: true);
				}
			}
			if (m_CharacterRole == CharacterRole.Inmate && SolitaryManager.GetInstance() != null)
			{
				if (newRoom != null)
				{
					RoomBlob_Solitary roomBlobData = newRoom.GetRoomBlobData<RoomBlob_Solitary>();
					if (roomBlobData != null)
					{
						m_bExitSolitaryFreePass = true;
						if (m_bIsWantedForSolitary)
						{
							SetIsNaughtyLocation(value: false);
						}
						bool success;
						if (!m_bIsWantedForSolitary)
						{
							roomBlobData.ReleaseCharacter(this);
						}
						else if (!(roomBlobData == SolitaryManager.GetInstance().GetCellForCharacter(this, out success)))
						{
							roomBlobData.ReleaseCharacter(this);
						}
					}
				}
				if (previousRoom != null)
				{
					RoomBlob_Solitary roomBlobData2 = previousRoom.GetRoomBlobData<RoomBlob_Solitary>();
					if (roomBlobData2 != null)
					{
						roomBlobData2.LockToCharacter(this);
					}
				}
			}
		}
		if (previousRoom != null && previousRoom.location == RoomBlob.eLocation.JobRoom)
		{
			DoContrabandCheck();
		}
		if (newRoom != null && newRoom.location == RoomBlob.eLocation.JobRoom)
		{
			DoContrabandCheck();
		}
	}

	private void CheckForNakedDinnerAchievement()
	{
		if (m_bIsNaked && m_CurrentLocation != null && m_CurrentLocation.location == RoomBlob.eLocation.MealHall && RoutineManager.GetInstance().GetCurrentRoutineSubType() == RoutineSubTypes.DinnerTime)
		{
			Player player = (Player)this;
			if (player != null && player.m_Gamer != null && player.m_Gamer.IsLocal())
			{
				StatSystem.GetInstance().IncStat(27, 1f, player.m_Gamer, string.Empty);
			}
		}
	}

	public virtual void HandleRoutineReachedEvent(RoutinesData.Routine routine)
	{
		if (this.ReachedRoutineLocationEvent != null)
		{
			this.ReachedRoutineLocationEvent(this, routine);
		}
	}

	public virtual void SetMyCell(RoomBlob roomblob)
	{
		m_MyCell = roomblob;
	}

	public void SetMyBed(BedInteraction bed)
	{
		if (bed != null)
		{
			m_CharacterBedEventManager = bed.GetComponent<BedEventManager>();
		}
	}

	public RoomBlob GetMyCell()
	{
		return m_MyCell;
	}

	public void SetLocation(RoomBlob location)
	{
		m_CurrentLocation = location;
	}

	public bool LocationIsNaughty(RoomBlob location)
	{
		if (location.location == RoomBlob.eLocation.InmateCell && location != m_MyCell && JobsManager.GetInstance().GetCharactersJobType(this) != JobType.Plumbing)
		{
			return true;
		}
		if (location.location == RoomBlob.eLocation.JobRoom)
		{
			return location != m_JobRoom;
		}
		if (location.m_subRules == RoomBlob.RoomSubIdentity_Rules.OffLimits)
		{
			if (m_bExitSolitaryFreePass)
			{
				m_bExitSolitaryFreePass = false;
				return false;
			}
			return true;
		}
		return false;
	}

	public void SetTargetTile(int tileRow, int tileColumn)
	{
		m_targetTileRow = tileRow;
		m_targetTileColumn = tileColumn;
	}

	public int GetTargetTileRow()
	{
		return m_targetTileRow;
	}

	public int GetTargetTileColumn()
	{
		return m_targetTileColumn;
	}

	public void SetCharacterTarget(Character target)
	{
		if (!(_m_CharacterTarget != target))
		{
			return;
		}
		if (target == null)
		{
			m_CharacterAnimator.CombatLockedState(lockedOn: false);
		}
		else
		{
			m_CharacterAnimator.CombatLockedState(lockedOn: true);
			FaceCharacter(target);
			if (OnCharacterSetTargetCharacter != null)
			{
				OnCharacterSetTargetCharacter(this, target);
			}
		}
		_m_CharacterTarget = target;
	}

	protected override void Awake()
	{
		base.Awake();
		m_StandingStillTime = m_StandingStillTimeout;
		m_NetObjectLock = GetComponent<NetObjectLock>();
		m_TimeSinceLastPacketArray = new float[120];
		for (int i = 0; i < 120; i++)
		{
			m_TimeSinceLastPacketArray[i] = 1f / 30f;
		}
		m_WallLayerMask = (1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Fence"));
		m_NoKnockbackLayerMask = (1 << LayerMask.NameToLayer("Wall")) | (1 << LayerMask.NameToLayer("Fence")) | (1 << LayerMask.NameToLayer("StaticMapObject"));
		m_bInstantPositionUpdate = false;
		if (m_TrackableElementReporter == null)
		{
			m_TrackableElementReporter = GetComponent<TrackableUIElementsReporter>();
		}
		UpdateProximityLayer();
		if (m_ItemContainer == null)
		{
			m_ItemContainer = base.gameObject.GetComponent<ItemContainer>();
		}
		RoomManager.RegisterForRoomAssignment(this);
		if (m_PhysicsCollider != null)
		{
			m_PhysicsSphereCol = m_PhysicsCollider.GetComponent<SphereCollider>();
		}
		if (m_CharacterTrigger != null)
		{
			m_CharacterSphereTrigger = m_CharacterTrigger.GetComponent<SphereCollider>();
		}
		m_CachedCurrentPosition = m_Transform.position;
		if (!m_AllCharacters.Contains(this))
		{
			m_AllCharacters.Add(this);
		}
		m_CharacterListIndex = FindMyIndexForAllCharacters();
	}

	protected virtual void Start()
	{
		if (m_CharacterAnimator != null)
		{
			switch (m_CharacterRole)
			{
			case CharacterRole.Dog:
				m_CharacterAnimator.SetCharacterAnimatorType(CharacterAnimator.ANIMATOR_TYPE.AT_DOG);
				break;
			case CharacterRole.Cameraman:
				m_CharacterAnimator.SetCharacterAnimatorType(CharacterAnimator.ANIMATOR_TYPE.AT_CAMERAMAN);
				break;
			default:
				m_CharacterAnimator.SetCharacterAnimatorType(CharacterAnimator.ANIMATOR_TYPE.AT_CLIVE);
				break;
			}
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (m_CharacterCustomisation != null && !m_CharacterCustomisation.IsInited())
		{
			return T17BehaviourManager.INITSTATE.IS_DEPS;
		}
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
		Init();
		if (m_CharacterRole == CharacterRole.Inmate && !m_CharacterStats.m_bIsPlayer)
		{
			TOTAL_INMATE_COUNT++;
		}
		return base.StartInit();
	}

	public virtual void Init()
	{
		if (m_CharacterRole != CharacterRole.Crowd)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.Character);
		}
		else
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.CrowdCharacter);
		}
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null)
		{
			m_CurrentLevel = currentLevelInfo.m_PrisonEnum;
		}
		else
		{
			m_CurrentLevel = LevelScript.PRISON_ENUM.Unassigned;
		}
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
			if (m_NetView == null)
			{
				Debug.LogErrorFormat("Character.Init: Failed to find NetView : {0}", base.gameObject.name);
			}
		}
		if (m_NetView != null)
		{
			m_SaveData = new SaveDataRegister(this, m_NetView.viewID, bIsMajorManagerComponent: false);
		}
		OnRoomChanged += RoomChanged;
		if (m_ItemContainer != null)
		{
			m_ItemContainer.SetCharacterOwner(this);
			ItemContainer.ItemContainerType containerType = ItemContainer.ItemContainerType.Inmate;
			switch (m_CharacterRole)
			{
			case CharacterRole.Inmate:
				containerType = ItemContainer.ItemContainerType.Inmate;
				break;
			case CharacterRole.Guard:
				containerType = ItemContainer.ItemContainerType.Guard;
				break;
			case CharacterRole.Dog:
				containerType = ItemContainer.ItemContainerType.Dog;
				break;
			}
			m_ItemContainer.m_ContainerType = containerType;
			if (m_CharacterRole == CharacterRole.Inmate)
			{
				ItemContainer itemContainer = m_ItemContainer;
				itemContainer.OnItemsChangedEvent = (ItemContainer.ItemContainerChangedEvent)Delegate.Combine(itemContainer.OnItemsChangedEvent, new ItemContainer.ItemContainerChangedEvent(OnItemsChanged));
			}
		}
		if (ConfigManager.GetInstance() != null)
		{
			CharacterConfig characterConfig = null;
			if (m_CharacterStats != null && m_CharacterStats.m_bIsPlayer)
			{
				characterConfig = ConfigManager.GetInstance().playerConfig;
			}
			else
			{
				switch (m_CharacterRole)
				{
				case CharacterRole.Inmate:
					characterConfig = ConfigManager.GetInstance().inmateConfig;
					break;
				case CharacterRole.Guard:
				{
					PrisonAlertness starRating = PrisonAlertness.ZeroStars;
					AICharacter_Guard component = GetComponent<AICharacter_Guard>();
					if (component != null)
					{
						starRating = component.m_ActiveAlertness;
					}
					characterConfig = ConfigManager.GetInstance().GetGuardConfig(starRating);
					break;
				}
				case CharacterRole.Dog:
					characterConfig = ConfigManager.GetInstance().dogConfig;
					break;
				case CharacterRole.Dolphin:
					if (m_CharacterCustomisation != null)
					{
						m_CharacterCustomisation.SetDisplayName("Snooty");
					}
					break;
				}
			}
			if (characterConfig != null && m_CharacterStats != null)
			{
				m_CharacterStats.ApplyCharacterConfig(characterConfig);
			}
		}
		if (m_CharacterStats != null)
		{
			CharacterStats characterStats = m_CharacterStats;
			characterStats.OnHeatStatChanged = (CharacterStats.CharacterStatsEvent)Delegate.Combine(characterStats.OnHeatStatChanged, new CharacterStats.CharacterStatsEvent(HeatChanged));
		}
		if (!m_bIsRegisteredForTrackingUI)
		{
			m_bIsRegisteredForTrackingUI = true;
			WorldCanvasTrackedUIElements.RegisterCharacter(this);
			Player.RegisterCharacter(this);
		}
		m_CharacterLayerMask = LayerMask.GetMask("Characters");
		if (m_TrackableElementReporter != null && m_CharacterCustomisation != null)
		{
			m_TrackableElementReporter.SetDisplayName(m_CharacterCustomisation.m_DisplayName);
		}
		UpdateProximityLayer();
		m_MasterClientCurrentLocation = null;
		m_CurrentLocation = null;
		m_PreviousPosition = m_Transform.position;
		if (m_CharacterRole == CharacterRole.Inmate)
		{
			RoutineManager instance = RoutineManager.GetInstance();
			instance.OnRoutineChanged -= RoutineChanged;
			instance.OnRoutineChanged += RoutineChanged;
		}
		if (m_CharacterCustomisation != null)
		{
			m_StandingStillTimerVar = UnityEngine.Random.Range(0.1f, 2f);
			int num = UnityEngine.Random.Range(0, 99);
			m_StandingStillTime = (float)(num % 50 / 50) + UnityEngine.Random.Range(0f - m_StandingStillTimerVar, m_StandingStillTimerVar) + m_StandingStillTimeout;
		}
		if (m_CharacterStats != null)
		{
			m_CharacterStats.StateChangedEvent -= CharacterStats_StateChangedEvent;
			m_CharacterStats.StateChangedEvent += CharacterStats_StateChangedEvent;
		}
		FloorManager.GetInstance().GetGroundTileExtents(ref m_CachedMapTopLeft, ref m_CachedMapBottomRight);
		RoutineManager.GetInstance().OnPurpleDoorLockStatusChanged += Character_OnPurpleDoorLockStatusChanged;
	}

	private void Character_OnPurpleDoorLockStatusChanged(bool areOpen)
	{
		m_bPurpleDoorLocksChanged = true;
	}

	private void UpdatePurpleDoorLockStatus()
	{
		if (!m_bPurpleDoorLocksChanged || (!IsPlayer() && numUpdatedLocks > 5))
		{
			return;
		}
		numUpdatedLocks++;
		m_bPurpleDoorLocksChanged = false;
		bool flag = false;
		if (GetEquippedItem() != null && (bool)GetEquippedItem().HasFunctionality(BaseItemFunctionality.Functionality.Key))
		{
			flag = true;
		}
		else if (m_ItemContainer != null)
		{
			List<Item> items = new List<Item>();
			m_ItemContainer.GetItems(ref items);
			if (items.Find((Item x) => x != null && (bool)x.HasFunctionality(BaseItemFunctionality.Functionality.Key)) != null)
			{
				flag = true;
			}
			else
			{
				m_ItemContainer.GetHiddenItems(ref items);
				if (items.Find((Item x) => x != null && (bool)x.HasFunctionality(BaseItemFunctionality.Functionality.Key)) != null)
				{
					flag = true;
				}
			}
		}
		if (flag && DoorManager.GetInstance() != null)
		{
			DoorManager.GetInstance().SetUpCharacterKeys(this);
		}
	}

	public int GetCharacterListIndex()
	{
		return m_CharacterListIndex;
	}

	protected virtual void CharacterStats_StateChangedEvent(StatModifierEnum oldState, StatModifierEnum newState)
	{
		if (oldState == StatModifierEnum.Shower || newState == StatModifierEnum.Shower)
		{
			UpdateNakedVisuals();
		}
	}

	public void RequestStartingItemsRPC()
	{
		if (m_ItemContainer == null || PrisonSnapshotIO.IsThereSaveData())
		{
			return;
		}
		if (m_CharacterRole != 0)
		{
			ItemManager.GetInstance().AssignKeyRPC(m_NetView.ownerId, KeyFunctionality.KeyColour.Black, OnItemMgrKeyResponse, ref m_ItemEventID);
		}
		else
		{
			int num = ItemManager.GetInstance().AssignKeyRPC(m_NetView.ownerId, KeyFunctionality.KeyColour.Yellow, OnItemMgrKeyResponse, ref m_ItemEventID);
			if (num == -1)
			{
				Debug.Log(" ERROR Yellow Key  ERROR ");
			}
		}
		if (m_CharacterRole == CharacterRole.Visitor)
		{
			ItemManager.GetInstance().AssignKeyRPC(m_NetView.ownerId, KeyFunctionality.KeyColour.Silver, OnItemMgrKeyResponse, ref m_ItemEventID);
		}
	}

	private void OnItemMgrKeyResponse(Item item, int eventID)
	{
		if (!(item != null))
		{
			return;
		}
		KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
		if (keyFunctionality != null)
		{
			m_AccessKey = item;
			if (eventID == m_ItemEventID)
			{
				m_ItemEventID = -1;
			}
			SetAccessKeyEnabled(enabled: true);
			SetAccessKeyCode(0);
		}
	}

	public virtual void OnDisplayNameChanged(string newName)
	{
		if (m_TrackableElementReporter != null)
		{
			m_TrackableElementReporter.SetDisplayName(newName);
		}
		if (m_ItemContainer != null)
		{
			m_ItemContainer.SetName(newName);
		}
		DeskInteraction myDesk = GetMyDesk();
		if (myDesk != null)
		{
			myDesk.UpdateNameFromOwner();
		}
		PinManager instance = PinManager.GetInstance();
		if (instance != null)
		{
			instance.UpdatePinTooltipTag(m_PinID, newName, localise: false);
		}
	}

	public void ForcePhotonSerialiseViewRPC()
	{
		m_NetView.RPC("RPC_ForcePhotonSerialiseViewRPC", m_NetView);
	}

	[PunRPC]
	public void RPC_ForcePhotonSerialiseViewRPC()
	{
		ForcePhotonSerialiseView();
	}

	public void ForcePhotonSerialiseView()
	{
		m_bForceSerialize = true;
	}

	public virtual void OnDestroy()
	{
		m_bIsBeingDestroyed = true;
		UpdateManager instance = UpdateManager.GetInstance();
		if (instance != null)
		{
			instance.Unregister(this, UpdateCategory.Character);
		}
		if (m_bIsRegisteredForTrackingUI)
		{
			m_bIsRegisteredForTrackingUI = false;
			WorldCanvasTrackedUIElements.UnRegisterCharacter(this);
			Player.UnRegisterCharacter(this);
		}
		if (m_SaveData != null)
		{
			m_SaveData.Dispose();
		}
		m_AllCharacters.Remove(this);
		if (m_PinID != -1)
		{
			PinManager instance2 = PinManager.GetInstance();
			if (instance2 != null)
			{
				instance2.RemovePin(m_PinID);
			}
		}
		m_PinID = -1;
		QuestManager instance3 = QuestManager.GetInstance();
		if (instance3 != null)
		{
			instance3.CleanUpQuestGiver(this);
		}
		CullingObjectCollector instance4 = CullingObjectCollector.GetInstance();
		if (instance4 != null)
		{
			instance4.RemoveCharacter(this);
		}
		DoorManager instance5 = DoorManager.GetInstance();
		if (instance5 != null)
		{
			instance5.OnCharacterDestroy(this);
		}
		RoutineManager instance6 = RoutineManager.GetInstance();
		if (instance6 != null)
		{
			instance6.OnPurpleDoorLockStatusChanged -= Character_OnPurpleDoorLockStatusChanged;
		}
		ForceStopInteraction();
		CharacterStencilRenderer.RemoveCharacterMeshRenderers(this);
		WorldCanvasTrackedUIElements.UnRegisterCharacter(this);
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		if (m_CharacterStats != null)
		{
			m_CharacterStats.StateChangedEvent -= CharacterStats_StateChangedEvent;
			m_CharacterStats = null;
		}
		m_JobRoom = null;
		RoomManager.UnregisterForRoomAssignment(this);
		OnRoomChanged -= RoomChanged;
		if (m_NetObjectLock != null)
		{
			if (m_NetObjectLock.IsLocked() && m_NetObjectLock.m_NetView != null)
			{
				m_NetObjectLock.ReleaseLock();
			}
			m_NetObjectLock = null;
		}
		m_NetView = null;
		m_TrackableElementReporter = null;
		OnOutfitChanged = null;
		OnEquipedItemChanged = null;
		m_CharacterEventManager = null;
	}

	public static void CleanUp()
	{
		m_AllCharacters.Clear();
		m_TrayInteractionCache = null;
	}

	public void ControlledUpdate()
	{
		if (m_CharacterRole != CharacterRole.Crowd)
		{
			UpdatePurpleDoorLockStatus();
			if (T17NetManager.IsConnectedToGameServerAndReady && m_NetView.isMine)
			{
				OwnerUpdate();
			}
		}
		if (m_EnableLayerAnimator)
		{
			LayerAnimator();
		}
	}

	public void ControlledFixedUpdate()
	{
		if (T17NetManager.IsMasterClient)
		{
			MasterClientFixedUpdate();
		}
		if (m_NetView.isMine)
		{
			OwnerFixedUpdate();
		}
		else
		{
			RemoteFixedUpdate();
		}
	}

	protected virtual void MasterClientFixedUpdate()
	{
		if (m_IsAttackingTimer > 0f)
		{
			m_IsAttackingTimer -= UpdateManager.fixedDeltaTime;
		}
		else if (_m_bIsAttacking)
		{
			SetIsAttacking(attacking: false);
		}
		if (m_IsLootingTimer > 0f)
		{
			m_IsLootingTimer -= UpdateManager.fixedDeltaTime;
		}
		else if (_m_bIsLooting)
		{
			SetIsLooting(value: false);
		}
		Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
		if ((m_MasterClientLocationPos - cachedCurrentPosition).sqrMagnitude > 2f)
		{
			m_MasterClientLocationPos = cachedCurrentPosition;
			if (RoomManager.GetInstance() != null)
			{
				m_MasterClientCurrentLocation = RoomManager.GetInstance().LookUpRoom(cachedCurrentPosition, CurrentFloor);
			}
		}
	}

	protected virtual void OwnerUpdate()
	{
		if (m_fAttackRecoveryTime > 0f)
		{
			m_fAttackRecoveryTime -= UpdateManager.deltaTime;
		}
		if (m_fPauseMovementTimer > 0f)
		{
			m_fPauseMovementTimer -= UpdateManager.deltaTime;
		}
		if (m_fKnockBackStunTimer > 0f)
		{
			m_fKnockBackStunTimer -= UpdateManager.deltaTime;
		}
		if (_m_bIsBound)
		{
			m_fBoundEscapeTime -= UpdateManager.deltaTime;
			if (m_fBoundEscapeTime <= 0f)
			{
				EscapeBindings();
			}
		}
		if (m_fGetToRoutineTimer > 0f && !m_bIsKnockedOut)
		{
			m_fGetToRoutineTimer -= UpdateManager.deltaTime;
			if (m_fGetToRoutineTimer <= 0f && !m_bRoutineTargetLocationReached && !m_bIsWantedForSolitary && !GetIsDisabled())
			{
				Routines currentRoutineBaseType = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
				bool flag = true;
				if (currentRoutineBaseType == Routines.JobTime)
				{
					BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(this);
					if (charactersJob != null && (charactersJob.QuotaAchieved == charactersJob.QuotaTarget || !charactersJob.DoesEmployeeHaveToReportToJobRoom()))
					{
						flag = false;
					}
				}
				if (flag)
				{
					SetIsTardy(value: true);
				}
				if (currentRoutineBaseType == Routines.Lockdown || currentRoutineBaseType == Routines.LightsOut)
				{
					SetIsMissing(value: true);
				}
			}
		}
		if (m_InteractingObject != null)
		{
			m_InteractingObject.UpdateInteraction();
		}
		if (m_bIsWanted || m_bIsSuspicious || m_SuspiciousStateTimer > 0f)
		{
			CheckWantedClear();
		}
		if (m_CharacterRole != CharacterRole.Crowd && m_CharacterRole != CharacterRole.Invisible && m_StandingStillTime > 0f)
		{
			m_StandingStillTime -= UpdateManager.deltaTime;
			if (m_StandingStillTime <= 0f)
			{
				if (!m_bHasTray && m_CharacterAnimator.GetAnimState() == AnimState.Idle)
				{
					ItemData itemDataID = null;
					m_StandingStillAnimState = m_CharacterAnimator.GetRandomStandingStillAnimState(ref itemDataID);
					m_StandingStillEquipID = 0;
					if (itemDataID != null)
					{
						ItemManager.GetInstance().AssignItemRPC(m_NetView.ownerId, itemDataID.m_ItemDataID, OnStandingAnimItemSpawn, ref m_StandingStillEquipID);
					}
					if (m_StandingStillEquipID == 0)
					{
						m_CharacterAnimator.StartAnimation(m_StandingStillAnimState);
					}
					if (m_CharacterAnimator.OnOneShotDone == new CharacterAnimator.OneShotDone(OnOneShotStandingAnim))
					{
						OnOneShotStandingAnim();
						CharacterAnimator characterAnimator = m_CharacterAnimator;
						characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotStandingAnim));
					}
					CharacterAnimator characterAnimator2 = m_CharacterAnimator;
					characterAnimator2.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Combine(characterAnimator2.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotStandingAnim));
				}
				else
				{
					m_StandingStillTime = UnityEngine.Random.Range(0f - m_StandingStillTimerVar, m_StandingStillTimerVar) + m_StandingStillTimeout;
				}
			}
		}
		m_bItemInUse = m_EquippedItem != null && m_EquippedItem.IsInUse();
		if (!m_bIsTardy)
		{
			return;
		}
		m_TimeUntilTardyRefreshCheck -= UpdateManager.deltaTime;
		if (!(m_TimeUntilTardyRefreshCheck <= 0f))
		{
			return;
		}
		m_TimeUntilTardyRefreshCheck += 0.25f;
		Routines currentRoutineBaseType2 = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
		if (currentRoutineBaseType2 == Routines.JobTime)
		{
			BaseJob charactersJob2 = JobsManager.GetInstance().GetCharactersJob(this);
			if (charactersJob2 != null && (charactersJob2.QuotaAchieved == charactersJob2.QuotaTarget || !charactersJob2.DoesEmployeeHaveToReportToJobRoom()))
			{
				SetIsTardy(value: false);
			}
		}
	}

	private void OnStandingAnimItemSpawn(Item item, int eventID)
	{
		if (m_StandingStillEquipID == eventID)
		{
			SetEquippedItem(item);
			UseEquippedItemRPC(item, bUse: true);
			m_CharacterAnimator.StartAnimation(m_StandingStillAnimState);
		}
	}

	public void OnOneShotStandingAnim()
	{
		m_CharacterAnimator.StopOneShotAnim(m_StandingStillAnimState);
		m_StandingStillTime = UnityEngine.Random.Range(0f - m_StandingStillTimerVar, m_StandingStillTimerVar) + m_StandingStillTimeout;
		CharacterAnimator characterAnimator = m_CharacterAnimator;
		characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotStandingAnim));
		if (m_StandingStillEquipID != 0)
		{
			UseEquippedItemRPC(GetEquippedItem(), bUse: false);
			RemoveItemRPC(GetEquippedItem(), RPC_CallContexts.Unknown, release: true);
			m_StandingStillEquipID = 0;
		}
	}

	protected virtual void OwnerFixedUpdate()
	{
	}

	public void SetAnimatedInteractionZ(float fPosZ)
	{
		float num = 0f;
		m_AnimatedInteractionLocalZ = m_Transform.InverseTransformPoint(0f, 0f, fPosZ + num).z;
	}

	public void SetUseItemZ(float fPosZ)
	{
		m_AnimatedInteractionLocalZ = fPosZ;
	}

	public string GetLogForScreen()
	{
		return m_EnableLayerAnimator + " :" + ((!(m_InteractingObject == null)) ? m_InteractingObject.name : "null") + m_CharacterAnimator.m_CharacterAnimator.gameObject.transform.localPosition.ToString() + " - " + base.name;
	}

	private bool ShouldHackAnimatorOffsetPosition(Vector3 tilePosition)
	{
		return m_CurrentLevel == LevelScript.PRISON_ENUM.DLC04 && (double)tilePosition.x > -16.2 && (double)tilePosition.x < 14.9 && (double)tilePosition.y > 13.53 && (double)tilePosition.y < 16.9;
	}

	private void LayerAnimator()
	{
		Vector3 defaultAnimatorPosition = m_DefaultAnimatorPosition;
		if (LevelScript.GetInstance().m_Processed)
		{
			InteractiveObject interactiveObject = m_InteractingObject;
			if (null == interactiveObject)
			{
				interactiveObject = m_RemoteInteractingObject;
			}
			bool flag = false;
			if (interactiveObject == null || interactiveObject.GetInteractionClassType() != InteractiveObject.InteractionType.AnimatedInteractiveObject || interactiveObject.LeaveCharacterPositionUnAltered())
			{
				FloorManager.GetInstance().GetTileCentrePosition(CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Transform.position, out var centredPosition);
				if (!m_bItemInUse)
				{
					if (m_PickedUpBy != null && ShouldHackAnimatorOffsetPosition(centredPosition))
					{
						defaultAnimatorPosition.z = m_PickedUpBy.GetZOffsetForCharacter();
						flag = true;
					}
					else
					{
						defaultAnimatorPosition.z = GetZOffsetForCharacter(centredPosition);
					}
				}
				else
				{
					defaultAnimatorPosition.z = m_AnimatedInteractionLocalZ;
				}
				defaultAnimatorPosition.z += -0.0035f;
				if (!flag && m_PickedUpBy != null)
				{
					defaultAnimatorPosition.z += m_CarryOffset;
				}
				if (!IsPlayer() && m_CharacterRole == CharacterRole.Ghost)
				{
					defaultAnimatorPosition.z -= 0.05f;
				}
				if (m_bHasTray)
				{
					defaultAnimatorPosition.z += -0.019f;
				}
			}
			else
			{
				defaultAnimatorPosition.x = 0f;
				defaultAnimatorPosition.y = 0f;
				defaultAnimatorPosition.z = m_AnimatedInteractionLocalZ;
			}
		}
		if (m_AnimationTransform == null && m_CharacterAnimator != null && m_CharacterAnimator.m_CharacterAnimator != null)
		{
			m_AnimationTransform = m_CharacterAnimator.m_CharacterAnimator.gameObject.transform;
		}
		if (m_AnimationTransform != null)
		{
			m_AnimationTransform.localPosition = defaultAnimatorPosition;
			Vector3 localScale = m_AnimationTransform.localScale;
			localScale.z = 20f;
			m_AnimationTransform.localScale = localScale;
		}
	}

	public float GetZOffsetForCharacter()
	{
		FloorManager.GetInstance().GetTileCentrePosition(CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, m_Transform.position, out var centredPosition);
		return GetZOffsetForCharacter(centredPosition);
	}

	public float GetZOffsetForCharacter(Vector3 tilePosition)
	{
		return LayerHelper.GetZOffset(tilePosition.y);
	}

	public void EnableLayerAnimator(bool bEnable)
	{
		if (m_EnableLayerAnimator != bEnable)
		{
			m_EnableLayerAnimator = bEnable;
			if (m_EnableLayerAnimator)
			{
				LayerAnimator();
			}
		}
	}

	public void EnableTrackededElementRendering(bool bEnable)
	{
		if (m_TrackableElementReporter != null)
		{
			m_TrackableElementReporter.SetAllowedToRender(bEnable);
		}
	}

	public void ApplyNetworkPrediciton()
	{
		if (!m_UseNewPrediction)
		{
			if (!(m_PickedUpBy == null))
			{
				return;
			}
			float magnitude = (m_vNetworkedPosition - m_vNetworkedPositionPrevious).magnitude;
			Vector3 zero = Vector3.zero;
			if (magnitude > CharacterSerializer.CharacterTeleportDistance)
			{
				zero = m_vNetworkedPosition;
			}
			else
			{
				float num = 0f;
				if (m_fCurrentSentPacketTime > 0f && m_fLastSentPacketTime > 0f)
				{
					num = m_fCurrentSentPacketTime - m_fLastSentPacketTime;
				}
				float fLatency = 0f;
				PeerLatency.GetPeerLatency(m_NetView.ownerId, ref fLatency);
				if (float.IsNaN(fLatency) || float.IsInfinity(fLatency))
				{
					fLatency = 0f;
				}
				float num2 = magnitude * 2.2f * fLatency;
				Vector2 vector = m_vNetworkedPosition - m_vNetworkedPositionPrevious;
				vector.Normalize();
				m_vVelocity = vector * num2;
				if (m_bCalcFacingDirection)
				{
					CalcFaceDirection(m_vVelocity);
				}
				CharacterSpeed speed = CalcNetworkSpeed(m_vVelocity.magnitude);
				m_CharacterAnimator.CharacterSpeedChanged(speed);
				float num3 = UpdateManager.smoothTime - m_fLastNetworkLocalTime + UpdateManager.fixedDeltaTime;
				float num4 = m_fLastNetworkLocalTime + num;
				float num5 = num4 - m_fLastNetworkLocalTime + fLatency;
				float t = 0f;
				if (num5 > 0f)
				{
					t = num3 / num5;
				}
				Vector2 b = m_vNetworkedPosition + m_vVelocity;
				Vector2 vector2 = Vector2.Lerp(m_vNetworkedPositionPreviousLocal, b, t);
				zero = new Vector3(vector2.x, vector2.y, m_CurrentFloor.m_zPos);
			}
			if (zero.x < m_CachedMapTopLeft.x)
			{
				zero.x = m_CachedMapTopLeft.x;
			}
			if (zero.x > m_CachedMapBottomRight.x)
			{
				zero.x = m_CachedMapBottomRight.x;
			}
			if (zero.y > m_CachedMapTopLeft.y)
			{
				zero.y = m_CachedMapTopLeft.y;
			}
			if (zero.y < m_CachedMapBottomRight.y)
			{
				zero.y = m_CachedMapBottomRight.y;
			}
			if (m_bIsStandingOnDesk)
			{
				zero.z += m_OnDeskOffset;
			}
			m_CachedCurrentPosition = zero;
			m_Transform.position = zero;
			UpdateCurrentLocationWithValidation(performPositionChecks: true);
		}
		else
		{
			if (!(m_PickedUpBy == null))
			{
				return;
			}
			Vector3 vector3 = m_vNetworkedPosition;
			Vector3 vector4 = m_vNetworkedPositionPrevious;
			Vector3 vector5 = m_vNetworkedPositionPreviousLocal;
			float fCurrentSentPacketTime = m_fCurrentSentPacketTime;
			float fLastSentPacketTime = m_fLastSentPacketTime;
			Vector3 zero2 = Vector3.zero;
			float magnitude2 = (vector3 - vector5).magnitude;
			float num6 = Mathf.Max(fCurrentSentPacketTime - fLastSentPacketTime, 0.03f);
			float num7 = UpdateManager.fixedTime - m_fLastNetworkFixedLocalTime;
			m_TimeSinceLastPacketArray[m_TimeSincePacketCounter++] = num6;
			if (m_TimeSincePacketCounter > m_TimeSincePacketSmoothener)
			{
				m_TimeSincePacketCounter = 0;
			}
			float num8 = 0f;
			if (m_TimeSincePacketSmoothener > 0)
			{
				for (int i = 0; i < m_TimeSincePacketSmoothener; i++)
				{
					num8 += m_TimeSinceLastPacketArray[i];
				}
				num8 /= (float)m_TimeSincePacketSmoothener;
			}
			else
			{
				num8 = num6;
			}
			Vector3 vector6 = vector3 - vector4;
			Vector3 vector7 = Vector3.zero;
			if (vector6.magnitude > 0.001f)
			{
				vector7 = vector6.normalized * m_vNetworkedVelocity;
			}
			if (m_bCalcFacingDirection)
			{
				CalcFaceDirection(vector7);
			}
			CharacterSpeed speed2 = CalcNetworkSpeed(m_vNetworkedVelocity);
			m_CharacterAnimator.CharacterSpeedChanged(speed2);
			if (magnitude2 > behindCompensationDistance)
			{
				m_vNetworkedPositionPreviousLocal = Vector2.Lerp(vector5, vector3, (magnitude2 - behindCompensationDistance) / behindCompensationFactor);
			}
			Vector3 vector8 = Vector2.Lerp(vector5, vector3 + vector7 * m_fLastMessageLatency * latencyCompensationAmount, num7 / (num8 * timeSmootheningAmount));
			if (num7 < 0.25f)
			{
				vector8 += vector7 * num7 * predictionAmount;
			}
			else
			{
				float t2 = (num7 - 0.25f) / 0.25f;
				vector8 += vector7 * (0.25f + Mathf.Lerp(0f, 0.125f, t2)) * predictionAmount;
			}
			zero2 = ((!(magnitude2 > CharacterSerializer.CharacterTeleportDistance)) ? new Vector3(vector8.x, vector8.y, m_CurrentFloor.m_zPos) : new Vector3(m_vNetworkedPosition.x, m_vNetworkedPosition.y, m_CurrentFloor.m_zPos));
			if (zero2.x < m_CachedMapTopLeft.x)
			{
				zero2.x = m_CachedMapTopLeft.x;
			}
			if (zero2.x > m_CachedMapBottomRight.x)
			{
				zero2.x = m_CachedMapBottomRight.x;
			}
			if (zero2.y > m_CachedMapTopLeft.y)
			{
				zero2.y = m_CachedMapTopLeft.y;
			}
			if (zero2.y < m_CachedMapBottomRight.y)
			{
				zero2.y = m_CachedMapBottomRight.y;
			}
			if (m_bIsStandingOnDesk)
			{
				zero2.z += m_OnDeskOffset;
			}
			m_CachedCurrentPosition = zero2;
			m_Transform.position = zero2;
			UpdateCurrentLocationWithValidation(performPositionChecks: true);
		}
	}

	protected virtual void RemoteFixedUpdate()
	{
		ApplyNetworkPrediciton();
	}

	public virtual void OnInteractionStart()
	{
		m_PhysicsCollider.SetActive(value: false);
	}

	public virtual void OnInteractionExit()
	{
	}

	public void OnStartToClimbOnObject(ClimbableObject climbableObject)
	{
		if (m_NetView.isMine)
		{
			m_CharacterAnimator.StartAnimation(AnimState.IdleDeskPrime);
			m_bStartingToClimb = true;
		}
	}

	public void OnCancelClimbOnObject(ClimbableObject climbableObject)
	{
		if (m_NetView.isMine)
		{
			m_CharacterAnimator.StopAnimation(AnimState.IdleDeskPrime);
			m_bStartingToClimb = false;
		}
	}

	public void OnClimbOnObject(ClimbableObject climbableObject)
	{
		if (!m_NetView.isMine)
		{
			return;
		}
		m_bStartingToClimb = false;
		Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
		if (m_CharacterStats.m_bIsPlayer)
		{
			Vector3 position = climbableObject.transform.position;
			if (!m_bIsStandingOnDesk)
			{
				cachedCurrentPosition.x = position.x;
				cachedCurrentPosition.y = position.y + 0.25f;
			}
		}
		if (!m_CharacterStats.m_bIsPlayer || !m_bIsStandingOnDesk)
		{
			cachedCurrentPosition.z -= 0.5f;
		}
		m_CachedCurrentPosition = cachedCurrentPosition;
		m_Transform.position = m_CachedCurrentPosition;
		if (m_CharacterStats.m_bIsPlayer & !m_bIsStandingOnDesk)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Climb_Desk, base.gameObject);
			PauseMovement(0.7f);
		}
		if (!m_bIsStandingOnDesk)
		{
			m_CharacterAnimator.StopAnimation(AnimState.IdleDeskPrime);
			m_CharacterAnimator.StartAnimation(AnimState.IdleDeskClimb);
		}
		SetIsStandingOnDesk(value: true);
		SetClimbableObject(climbableObject);
	}

	public void OnClimbOffObject(ClimbableObject climbableObject, bool isMovingOnToNewClimbable, bool shouldRepositionOnDismount)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (m_ClimbableObject != climbableObject || instance == null || !m_NetView.isMine)
		{
			return;
		}
		if (shouldRepositionOnDismount && m_CurrentFloor.m_FloorIndex == instance.FindFloorIndexAtZ(climbableObject.transform.position.z))
		{
			Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
			if (m_CharacterStats.m_bIsPlayer && !isMovingOnToNewClimbable)
			{
				cachedCurrentPosition.y -= 0.25f;
			}
			if (!m_CharacterStats.m_bIsPlayer || !isMovingOnToNewClimbable)
			{
				cachedCurrentPosition.z = m_CurrentFloor.m_zPos;
			}
			m_CachedCurrentPosition = cachedCurrentPosition;
			m_Transform.position = m_CachedCurrentPosition;
		}
		if (!isMovingOnToNewClimbable)
		{
			SetIsStandingOnDesk(value: false);
		}
		SetClimbableObject(null);
		if (!isMovingOnToNewClimbable)
		{
			m_CharacterAnimator.StopAnimation(AnimState.IdleDeskClimb);
		}
	}

	public void OnTransitionOnObject(ClimbableObject climbableObject)
	{
		if (m_NetView.isMine)
		{
			m_bStartingToClimb = false;
			Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
			if (m_CharacterStats.m_bIsPlayer)
			{
				Vector3 position = climbableObject.transform.position;
				cachedCurrentPosition.x = position.x;
				cachedCurrentPosition.y = position.y + 0.25f;
			}
			cachedCurrentPosition.z += m_OnDeskOffset;
			m_CachedCurrentPosition = cachedCurrentPosition;
			m_Transform.position = m_CachedCurrentPosition;
			SetIsStandingOnDesk(value: true);
			SetClimbableObject(climbableObject);
		}
	}

	public void ForceClimbOffObject()
	{
		if (m_NetView.isMine && m_ClimbableObject != null)
		{
			m_ClimbableObject.ForceCharacterOffObject(this, shouldRepositionForDismount: true);
		}
	}

	private void HeatChanged(float heat)
	{
		float num = ConfigManager.GetInstance().aiConfig.DisguiseBreakHeat;
		if (m_bIsDisguised && heat >= num)
		{
			SetIsDisguised(value: false);
		}
		else if (!m_bIsDisguised && m_WearingDisguise && heat < num)
		{
			SetIsDisguised(value: true);
		}
		if (!m_bIsKnockedOut)
		{
			if (!m_bIsSuspicious && heat >= (float)ConfigManager.GetInstance().aiConfig.GuardSuspiciousHeat)
			{
				SetIsSuspicious(value: true);
			}
			if (!m_bIsWanted && heat >= (float)ConfigManager.GetInstance().aiConfig.GuardWantedHeat)
			{
				SetIsWanted(value: true);
				SetIsSuspicious(value: false);
			}
		}
	}

	private void CheckWantedClear()
	{
		if (m_bIsKnockedOut)
		{
			if (m_bIsWanted)
			{
				SetIsWanted(value: false);
			}
			if (m_bIsSuspicious)
			{
				SetIsSuspicious(value: false);
			}
			return;
		}
		if ((!m_bIsSuspicious || m_SuspiciousStateTimer > 0f) && m_CharacterStats.Heat <= (float)ConfigManager.GetInstance().aiConfig.GuardSuspiciousHeat)
		{
			SetIsSuspicious(value: false);
			m_SuspiciousStateTimer = 0f;
		}
		else
		{
			m_SuspiciousStateTimer -= UpdateManager.deltaTime;
			if (m_SuspiciousStateTimer <= 0f)
			{
				SetIsSuspicious(!m_bIsSuspicious);
				if (m_bIsSuspicious)
				{
					m_SuspiciousStateTimer = ConfigManager.GetInstance().aiConfig.GetGuardFollowTime();
				}
				else
				{
					m_SuspiciousStateTimer = ConfigManager.GetInstance().aiConfig.GetGuardIgnoreTime();
				}
			}
		}
		if (m_CharacterStats.Heat < (float)ConfigManager.GetInstance().aiConfig.GuardWantedHeat)
		{
			SetIsWanted(value: false);
		}
	}

	public virtual void ProcessStartingItems()
	{
		if (!PrisonSnapshotIO.IsThereSaveData() && m_ItemContainer != null)
		{
			EquipStartingOutfit();
			EquipStartingWeapon();
		}
	}

	public void EquipStartingOutfit(bool forceSet = false, bool bClearOutOldOutfit = false)
	{
		bool outfit = true;
		bool forceSet2 = forceSet;
		bool removeOldOutfitFromInventory = bClearOutOldOutfit;
		EquipStartingItem(outfit, forceSet2, tellOthers: false, removeOldOutfitFromInventory);
	}

	public void EquipStartingWeapon(bool forceSet = false)
	{
		EquipStartingItem(outfit: false, forceSet);
	}

	private void EquipStartingItem(bool outfit, bool forceSet = false, bool tellOthers = false, bool removeOldOutfitFromInventory = false)
	{
		ItemData defaultStartingItem = GetDefaultStartingItem(outfit);
		if (defaultStartingItem == null || defaultStartingItem.m_ItemDataID <= 0)
		{
			if (forceSet)
			{
				if (outfit)
				{
					SetOutFit(null, bTellOthers: true, bAddOldToInventory: false);
				}
				else
				{
					SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
				}
			}
			return;
		}
		Item item = ((!outfit) ? GetEquippedItem() : GetOutFit());
		int num = ((!(item == null)) ? item.ItemDataID : (-1));
		int itemDataID = defaultStartingItem.m_ItemDataID;
		if ((num <= 0 && itemDataID <= 0) || num == itemDataID)
		{
			return;
		}
		int itemCount = m_ItemContainer.GetItemCount();
		for (int i = 0; i < itemCount; i++)
		{
			Item item2 = m_ItemContainer.GetItem(i);
			if (item2.ItemDataID == itemDataID)
			{
				if (outfit)
				{
					SetOutFit(item2, tellOthers);
				}
				else
				{
					SetEquippedItem(item2, tellOthers);
				}
				return;
			}
		}
		ItemManager instance = ItemManager.GetInstance();
		if (!(instance != null) || !(m_NetView != null) || !(defaultStartingItem != null))
		{
			return;
		}
		if (outfit)
		{
			if (removeOldOutfitFromInventory)
			{
				instance.AssignItemRPC(m_NetView.ownerId, defaultStartingItem.m_ItemDataID, OnItemMgrResponseAddToInventory_OutfitRemove, ref m_OutfitRequestId);
			}
			else
			{
				instance.AssignItemRPC(m_NetView.ownerId, defaultStartingItem.m_ItemDataID, OnItemMgrResponseAddToInventory_Outfit, ref m_OutfitRequestId);
			}
		}
		else
		{
			instance.AssignItemRPC(m_NetView.ownerId, defaultStartingItem.m_ItemDataID, OnItemMgrResponseAddToInventory_Weapon, ref m_WeaponRequestId);
		}
	}

	public ItemData GetDefaultStartingItem(bool outfit)
	{
		ItemData result = null;
		if (m_ItemContainer == null || m_ItemContainer.m_StartingItems == null)
		{
			return null;
		}
		for (int i = 0; i < m_ItemContainer.m_StartingItems.Count; i++)
		{
			ItemData itemData = m_ItemContainer.m_StartingItems[i];
			if (!(itemData != null))
			{
				continue;
			}
			if (outfit)
			{
				if (itemData.IsOutfit())
				{
					result = itemData;
					break;
				}
			}
			else if (!itemData.IsOutfit() && itemData.m_CanBeEquiped && !itemData.HasFunctionality(BaseItemFunctionality.Functionality.Key) && !itemData.HasFunctionality(BaseItemFunctionality.Functionality.Keycard))
			{
				result = itemData;
				break;
			}
		}
		return result;
	}

	private void OnItemMgrResponseAddToInventory_Outfit(Item item, int eventID)
	{
		if (m_OutfitRequestId == eventID)
		{
			SetOutFit(item);
		}
	}

	private void OnItemMgrResponseAddToInventory_OutfitRemove(Item item, int eventID)
	{
		if (m_OutfitRequestId == eventID)
		{
			SetOutFit(item, bTellOthers: true, bAddOldToInventory: false);
		}
	}

	private void OnItemMgrResponseAddToInventory_Weapon(Item item, int eventID)
	{
		if (m_WeaponRequestId == eventID)
		{
			SetEquippedItem(item);
		}
	}

	public virtual void ProcessPins()
	{
		if (m_MapIcon != null && m_PinID == -1)
		{
			m_ActivePin.m_FilterType = PinManager.Pin.PinFilterType.Characters;
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = true;
			GameObject target = base.gameObject;
			Sprite mapIcon = m_MapIcon;
			bool bUpdatePosition = true;
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorbyIndex(1);
			PinManager.Pin.PinFilterType filterType = m_ActivePin.m_FilterType;
			m_PinID = instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition, floor, null, filterType, edgable: false, floorTrackable: false, directional: false, m_CharacterCustomisation.m_DisplayName, localiseToolTipTag: false);
		}
		if (m_bIsVendor)
		{
			SetPinImage(VendorManager.GetInstance().GetVendorForCharacter(this, out var _).m_MapIcon, PinManager.Pin.PinFilterType.Shops);
		}
		if (m_bHasQuestAvailable)
		{
			SetPinImage(QuestManager.GetInstance().GetMapQuestSprite(this), PinManager.Pin.PinFilterType.Favours);
		}
		if (m_PinID != -1)
		{
			PinManager.GetInstance().UpdatePinIconSprite(m_PinID, m_ActivePin.m_Sprite, m_ActivePin.m_FilterType, m_ActivePin.m_Animation, m_ActivePin.m_Edgable, m_ActivePin.m_FloorTrackable);
		}
	}

	public void RemoveItemRPC(Item item, RPC_CallContexts context, bool release = false, bool addToOldInventory = true)
	{
		if (!(item == null))
		{
			if (m_EquippedItem == item)
			{
				SetEquippedItem(null, bTellOthers: true, addToOldInventory && !release, context);
			}
			if (m_Outfit == item)
			{
				SetOutFit(null, bTellOthers: true, addToOldInventory && !release, context);
			}
			if (m_ItemContainer != null && m_ItemContainer.HasSpecificItem(item.m_NetView.viewID))
			{
				m_ItemContainer.RemoveItemRPC(item, release, context);
			}
			else if (release)
			{
				ItemManager.GetInstance().RequestReleaseItem(item, context);
			}
		}
	}

	public Item GetOutFit()
	{
		return m_Outfit;
	}

	public bool SetOutfitState(Item outfit, out int iViewId, bool bAddOldToInventory, RPC_CallContexts callContext)
	{
		iViewId = -1;
		if (m_Outfit == outfit)
		{
			return false;
		}
		if (outfit != null && null != m_ItemContainer)
		{
			m_ItemContainer.RemoveItemRPC(outfit, releaseToManager: false, callContext);
		}
		if (null != m_Outfit)
		{
			m_Outfit.SetOwner(null);
			if (bAddOldToInventory && null != m_ItemContainer && !m_ItemContainer.LOCAL_AddItem(m_Outfit, bInToHidden: false))
			{
				bool flag = false;
				if (m_EquippedItem == null)
				{
					flag = SetEquippedItem(m_Outfit, bTellOthers: false);
				}
				if (!flag)
				{
					ItemManager.GetInstance().RequestReleaseItem(m_Outfit, callContext);
				}
			}
		}
		m_Outfit = outfit;
		if (m_Outfit != null)
		{
			m_Outfit.SetOwner(this);
		}
		if (m_Outfit != null)
		{
			iViewId = m_Outfit.m_NetView.viewID;
		}
		if (m_CharacterCustomisation != null)
		{
			m_CharacterCustomisation.SetOutfit(m_Outfit);
		}
		float value = 0f;
		if (m_Outfit != null && m_Outfit.OutfitData != null)
		{
			value = (float)m_Outfit.OutfitData.m_OutfitAppearance;
		}
		AudioController.SetParameter(Game_Parameter.Character_Outfit, value, base.gameObject);
		SetIsNaked(-1 == iViewId);
		return true;
	}

	public bool SetOutFit(Item outfit, bool bTellOthers = true, bool bAddOldToInventory = true, RPC_CallContexts context = RPC_CallContexts.Unknown)
	{
		if (m_Outfit == outfit)
		{
			return false;
		}
		int iViewId = -1;
		bool flag = SetOutfitState(outfit, out iViewId, bAddOldToInventory, context);
		if (OnOutfitChanged != null)
		{
			OnOutfitChanged();
		}
		_m_WearingDisguise = false;
		AIConfig aiConfig = ConfigManager.GetInstance().aiConfig;
		bool isDisguised = false;
		if (null != aiConfig && null != outfit)
		{
			Item_Outfit.OutFitType type = outfit.OutfitData.m_Type;
			if (type == Item_Outfit.OutFitType.Guard || type == Item_Outfit.OutFitType.Medic || type == Item_Outfit.OutFitType.Civilian)
			{
				_m_WearingDisguise = true;
				isDisguised = m_CharacterStats.Heat < (float)aiConfig.DisguiseBreakHeat;
			}
		}
		SetIsDisguised(isDisguised);
		DoorManager instance = DoorManager.GetInstance();
		if (null != instance)
		{
			instance.SetUpCharacterKeys(this);
		}
		if (bTellOthers && flag && context != 0)
		{
			m_NetView.RPC("RPC_SetOutfit", NetTargets.Others, iViewId, bAddOldToInventory);
		}
		return flag;
	}

	[PunRPC]
	public void RPC_SetOutfit(int itemViewID, bool bAddOldToInventory, PhotonMessageInfo info)
	{
		Item item = null;
		if (itemViewID != -1)
		{
			PhotonView photonView = PhotonView.Find(itemViewID);
			if (photonView != null)
			{
				item = photonView.gameObject.GetComponent<Item>();
				if (!(item != null))
				{
				}
			}
		}
		SetOutFit(item, bTellOthers: false, bAddOldToInventory, RPC_CallContexts.All);
	}

	public Item GetEquippedItem()
	{
		return m_EquippedItem;
	}

	public bool SetEquippedItemState(Item item, out int iViewId, bool bAddOldToInventory, RPC_CallContexts callContext)
	{
		iViewId = -1;
		bool flag = false;
		if (m_EquippedItem == item)
		{
			return false;
		}
		if (item != null && null != m_ItemContainer && m_ItemContainer.HasSpecificItem(item.m_NetView.viewID))
		{
			m_ItemContainer.RemoveItemRPC(item, releaseToManager: false, callContext);
		}
		if (m_EquippedItem != null)
		{
			flag = true;
			if (m_EquippedItem.IsInUse())
			{
				m_EquippedItem.CancelUsing();
			}
			if (m_EquippedItem != null)
			{
				m_EquippedItem.SetOwner(null);
				if (bAddOldToInventory && null != m_ItemContainer && !m_EquippedItem.IsMagicItem() && !m_ItemContainer.LOCAL_AddItem(m_EquippedItem, bInToHidden: false))
				{
					ItemManager.GetInstance().RequestReleaseItem(m_EquippedItem, callContext);
				}
			}
		}
		m_EquippedItem = item;
		if (m_EquippedItem != null)
		{
			m_EquippedItem.SetOwner(this);
			flag = true;
		}
		if (flag)
		{
			DoorManager.GetInstance().SetUpCharacterKeys(this);
		}
		if (m_EquippedItem == null)
		{
			m_CharacterAnimator.CombatStateChanged(CombatState.UnarmedCombat);
			m_CharacterAnimator.SetMaterialHandHeld(null);
		}
		else
		{
			m_CharacterAnimator.SetMaterialHandHeld(m_EquippedItem.HeldMaterial, m_EquippedItem.HeldType);
			if (m_EquippedItem.CombatData == null)
			{
				m_CharacterAnimator.CombatStateChanged(CombatState.UnarmedCombat);
			}
			else
			{
				m_CharacterAnimator.CombatStateChanged(m_EquippedItem.CombatData.m_CombatAnimation);
			}
		}
		if (m_EquippedItem != null)
		{
			iViewId = m_EquippedItem.m_NetView.viewID;
		}
		if (this.EquippedItemChangedEvent != null)
		{
			this.EquippedItemChangedEvent(this, m_EquippedItem);
		}
		DoContrabandCheck();
		return true;
	}

	public void UseEquippedItemRPC(Item equipedItem, bool bUse)
	{
		if (equipedItem != null && m_NetView != null && (bool)equipedItem.m_NetView)
		{
			SetHasTray(hasTray: false);
			m_NetView.GameplayRPC("RPC_UseEquippedItem", NetTargets.All, equipedItem.m_NetView.viewID, bUse);
		}
	}

	[PunRPC]
	public void RPC_UseEquippedItem(int itemViewID, bool bUse, PhotonMessageInfo info)
	{
		Item item = T17NetView.Find<Item>(itemViewID);
		if (!(item != null))
		{
			return;
		}
		if (bUse)
		{
			if (item.UseMaterial != null)
			{
				m_CharacterAnimator.SetMaterialHandHeld(item.UseMaterial, item.UseType);
			}
			else
			{
				m_CharacterAnimator.SetMaterialHandHeld(item.HeldMaterial, item.UseType);
			}
			m_bActionRenderersRequired = true;
		}
		else
		{
			m_CharacterAnimator.SetMaterialHandHeld(item.HeldMaterial, item.HeldType);
			m_bActionRenderersRequired = false;
		}
	}

	public virtual bool CanEquipItem(Item item)
	{
		if (m_EquippedItem == item)
		{
			return false;
		}
		if (m_EquippedItem != null && !m_EquippedItem.CanBeSwitchedOut())
		{
			return false;
		}
		if (item != null && item.m_ItemData != null && !item.m_ItemData.m_CanBeEquiped)
		{
			return false;
		}
		return true;
	}

	public virtual bool SetEquippedItem(Item equipedItem, bool bTellOthers = true, bool bAddOldToInventory = true, RPC_CallContexts context = RPC_CallContexts.Unknown)
	{
		if (!CanEquipItem(equipedItem))
		{
			return false;
		}
		if (m_bHasTray)
		{
			SetHasTray(hasTray: false);
		}
		int iViewId = -1;
		bool flag = SetEquippedItemState(equipedItem, out iViewId, bAddOldToInventory, context);
		if (OnEquipedItemChanged != null)
		{
			OnEquipedItemChanged();
		}
		if (bTellOthers && flag && context != 0)
		{
			m_NetView.PostLevelLoadRPC("RPC_SetEquipedItem", NetTargets.Others, iViewId, bAddOldToInventory);
		}
		return flag;
	}

	[PunRPC]
	public void RPC_SetEquipedItem(int itemViewID, bool bAddOldToInventory, PhotonMessageInfo info)
	{
		Item item = null;
		if (itemViewID != -1)
		{
			PhotonView photonView = PhotonView.Find(itemViewID);
			if (photonView != null)
			{
				item = photonView.gameObject.GetComponent<Item>();
				if (!(item != null))
				{
				}
			}
		}
		SetEquippedItem(item, bTellOthers: false, bAddOldToInventory);
	}

	public Item_Combat GetItemCombat()
	{
		Item equippedItem = GetEquippedItem();
		if (equippedItem == null || equippedItem.CombatData == null)
		{
			return ConfigManager.GetInstance().combatConfig.m_UnarmedCombatConfig;
		}
		return equippedItem.CombatData;
	}

	public float GetEquippedItemAttackRange()
	{
		if (m_CharacterRole == CharacterRole.Dog)
		{
			return ConfigManager.GetInstance().combatConfig.m_fCombatDoggieNearHitDistance;
		}
		return GetItemCombat().m_fAttackRange;
	}

	public bool ChargeAttack(bool attackReleased)
	{
		if (m_fAttackRecoveryTime > 0f)
		{
			return false;
		}
		GlobalCombatConfig combatConfig = ConfigManager.GetInstance().combatConfig;
		if (combatConfig == null)
		{
			return false;
		}
		if (m_bIsBlocking)
		{
			CombatBlock(doBlock: false);
		}
		m_fSmashAttackChargeTimer += UpdateManager.deltaTime;
		m_CharacterAnimator.SetIsCharging(charging: true);
		m_CharacterAnimator.StartAnimation(AnimState.CombatCharge);
		bool flag = false;
		if (m_fSmashAttackChargeTimer >= combatConfig.m_fSmashAttackFullChargeTime && m_fSmashAttackChargeTimer < combatConfig.m_fSmashAttackDashTime)
		{
			if (!m_bIsDashing && m_CharacterRole == CharacterRole.Inmate)
			{
				EnergyModifier energyLevel = m_CharacterStats.EnergyLevel;
				float heavyAttackEnergyCost = GetItemCombat().m_CombatConfig.GetHeavyAttackEnergyCost(energyLevel);
				if (m_CharacterStats.HasEnoughEnergyForTask(heavyAttackEnergyCost))
				{
					m_CharacterStats.DecreaseEnergyRPC(heavyAttackEnergyCost);
				}
				else
				{
					flag = true;
				}
			}
			if (m_CharacterTarget != null)
			{
				Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
				Vector3 cachedCurrentPosition2 = m_CharacterTarget.m_CachedCurrentPosition;
				if (((Vector2)(cachedCurrentPosition2 - cachedCurrentPosition)).magnitude < GetEquippedItemAttackRange())
				{
					attackReleased = true;
				}
			}
			m_bIsDashing = true;
			m_CharacterAnimator.SetIsCharging(charging: false);
		}
		attackReleased |= m_fSmashAttackChargeTimer >= combatConfig.m_fSmashAttackDashTime;
		attackReleased = attackReleased || flag;
		if (attackReleased)
		{
			m_CharacterAnimator.SetIsCharging(charging: false);
			m_CharacterAnimator.StopAnimation(AnimState.CombatCharge);
			if (m_fSmashAttackChargeTimer < combatConfig.m_fSmashAttackCommitTime)
			{
				Attack();
				m_fAttackRecoveryTime = GetItemCombat().m_fRecoveryTime;
			}
			else if (!flag)
			{
				SmashAttack();
				m_fAttackRecoveryTime = GetItemCombat().m_fRecoveryTime;
			}
			ResetChargeAttack();
			return false;
		}
		return true;
	}

	public void ResetChargeAttack()
	{
		m_bIsDashing = false;
		m_fSmashAttackChargeTimer = 0f;
		if (m_CharacterAnimator != null)
		{
			m_CharacterAnimator.SetIsCharging(charging: false);
			m_CharacterAnimator.StopAnimation(AnimState.CombatCharge);
		}
	}

	public bool Attack()
	{
		Item_Combat itemCombat = GetItemCombat();
		if (m_CharacterRole == CharacterRole.Inmate)
		{
			float normalAttackEnergyCost = itemCombat.m_CombatConfig.GetNormalAttackEnergyCost(m_CharacterStats.EnergyLevel);
			if (!m_CharacterStats.HasEnoughEnergyForTask(normalAttackEnergyCost))
			{
				return false;
			}
			m_CharacterStats.DecreaseEnergyRPC(normalAttackEnergyCost);
		}
		m_bActionRenderersRequired = true;
		CharacterAnimator characterAnimator = m_CharacterAnimator;
		characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotDone));
		CharacterAnimator characterAnimator2 = m_CharacterAnimator;
		characterAnimator2.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Combine(characterAnimator2.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotDone));
		if (m_CharacterRole != CharacterRole.Dog)
		{
			m_CharacterAnimator.DoAttackAnimation(normalAttack: true, playRandom: true);
		}
		else
		{
			m_CharacterAnimator.DoAttackAnimation(normalAttack: true);
		}
		return true;
	}

	public void SmashAttack()
	{
		m_bActionRenderersRequired = true;
		CharacterAnimator characterAnimator = m_CharacterAnimator;
		characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotDone));
		CharacterAnimator characterAnimator2 = m_CharacterAnimator;
		characterAnimator2.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Combine(characterAnimator2.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotDone));
		m_CharacterAnimator.DoAttackAnimation(normalAttack: false);
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Super_Hit, base.gameObject);
		PauseMovement(ConfigManager.GetInstance().combatConfig.m_fSmashAttackAttackTime);
	}

	public void OnOneShotDone()
	{
		m_bActionRenderersRequired = false;
		CharacterAnimator characterAnimator = m_CharacterAnimator;
		characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotDone));
	}

	public void AttackAnimationDoDamageEvent(bool normalDamage, GamelogicRunModes processingMode)
	{
		bool flag = processingMode == GamelogicRunModes.AudioOnly || processingMode == GamelogicRunModes.All;
		bool flag2 = processingMode == GamelogicRunModes.NonAudioOnly || processingMode == GamelogicRunModes.All;
		Item_Combat itemCombat = GetItemCombat();
		float radius = itemCombat.m_fAttackRange;
		if (m_CharacterRole == CharacterRole.Dog)
		{
			radius = ConfigManager.GetInstance().combatConfig.m_fCombatDoggieNearHitDistance;
		}
		int num = EscapistsRaycast.OverlapSphereNonAlloc(m_CharacterTrigger.position, radius, m_CharacterLayerMask, QueryTriggerInteraction.Collide);
		if (flag)
		{
			if (m_CharacterRole != CharacterRole.Dog)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Swing, base.gameObject);
			}
			else
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Dog_Snarl, base.gameObject);
			}
		}
		m_CharactersToHitCache.Clear();
		bool flag3 = false;
		float num2 = ConfigManager.GetInstance().combatConfig.m_fCombatNearHitDistance;
		if (m_CharacterRole == CharacterRole.Dog)
		{
			num2 = ConfigManager.GetInstance().combatConfig.m_fCombatDoggieNearHitDistance;
		}
		Collider[] colliderOverlapList = EscapistsRaycast.ColliderOverlapList;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = colliderOverlapList[i].gameObject;
			if (gameObject.transform.parent == base.transform)
			{
				continue;
			}
			Vector3 position = gameObject.transform.position;
			float num3 = Vector3.Distance(position, m_CharacterTrigger.position);
			Vector3 lhs = Direction.DirectionToVector(m_x8FacingDirection);
			Vector3 vector = position - m_CharacterTrigger.position;
			if (num3 > num2)
			{
				float magnitude = vector.magnitude;
				float num4 = Vector3.Dot(lhs, vector);
				float value = num4 / magnitude;
				float num5 = Mathf.Abs(57.29578f * Mathf.Acos(Mathf.Clamp(value, -1f, 1f)));
				if (num5 > itemCombat.m_fAttackAngle / 2f)
				{
					continue;
				}
			}
			Character componentInParent = gameObject.GetComponentInParent<Character>();
			if (componentInParent == null || (componentInParent.m_CharacterRole != 0 && componentInParent.m_CharacterRole != CharacterRole.Guard) || (m_CharacterRole == CharacterRole.Guard && componentInParent.m_CharacterRole != 0))
			{
				continue;
			}
			if (m_CharacterRole == CharacterRole.Inmate)
			{
				int num6 = EscapistsRaycast.RaycastAll(m_CharacterTrigger.position, vector, num3, m_WallLayerMask, QueryTriggerInteraction.Ignore);
				if (num6 >= 1 && vector.sqrMagnitude > 0.25f)
				{
					continue;
				}
			}
			if (componentInParent == m_CharacterTarget)
			{
				flag3 = true;
				break;
			}
			m_CharactersToHitCache.Add(componentInParent);
		}
		StrengthModifier strengthLevel = m_CharacterStats.StrengthLevel;
		float num7 = 0f;
		num7 = ((!normalDamage) ? itemCombat.m_CombatConfig.GetHeavyAttackDamage(strengthLevel) : itemCombat.m_CombatConfig.GetNormalAttackDamage(strengthLevel));
		int itemViewID = -1;
		if (m_EquippedItem != null)
		{
			itemViewID = m_EquippedItem.m_NetView.viewID;
		}
		bool flag4 = false;
		if (flag3)
		{
			DamageCharacter(m_CharacterTarget, num7, itemViewID, normalDamage, processingMode);
			flag4 = true;
		}
		else if (m_CharacterStats.m_bIsPlayer)
		{
			for (int j = 0; j < m_CharactersToHitCache.Count; j++)
			{
				Character target = m_CharactersToHitCache[j];
				DamageCharacter(target, num7, itemViewID, normalDamage, processingMode);
				flag4 = true;
			}
		}
		else if (flag && m_CharactersToHitCache.Count > 0)
		{
			m_CharactersToHitCache.Sort(delegate(Character h1, Character h2)
			{
				float sqrMagnitude = (h1.transform.position - m_Transform.position).sqrMagnitude;
				float sqrMagnitude2 = (h2.transform.position - m_Transform.position).sqrMagnitude;
				if (sqrMagnitude > sqrMagnitude2)
				{
					return 1;
				}
				return (sqrMagnitude != sqrMagnitude2) ? (-1) : 0;
			});
			Character target2 = m_CharactersToHitCache[0];
			DamageCharacter(target2, num7, itemViewID, normalDamage, GamelogicRunModes.AudioOnly);
		}
		if (!flag4 || !flag2 || !(itemCombat != null))
		{
			return;
		}
		if (GetEquippedItem() != null)
		{
			if (itemCombat.m_HealthDecay > 0)
			{
				GetEquippedItem().DecreaseHealth(itemCombat.m_HealthDecay);
			}
			return;
		}
		GlobalCombatConfig combatConfig = ConfigManager.GetInstance().combatConfig;
		if (combatConfig != null && combatConfig.m_UnarmedCombatConfig != null && itemCombat.m_HealthDecay > 0)
		{
			GetEquippedItem().DecreaseHealth(combatConfig.m_UnarmedCombatConfig.m_HealthDecay);
		}
	}

	public virtual void DamageCharacter(Character target, float damage, int itemViewID, bool normalDamage, GamelogicRunModes processingMode)
	{
		bool flag = processingMode == GamelogicRunModes.AudioOnly || processingMode == GamelogicRunModes.All;
		if (processingMode == GamelogicRunModes.NonAudioOnly || processingMode == GamelogicRunModes.All)
		{
			CharacterNetEvents.SendDamageCharacterEvent(target, this, damage, (short)itemViewID, normalDamage);
			if (IsPlayer() && m_EquippedItem != null)
			{
				GoogleAnalyticsV3.LogCommericalAnalyticEvent("Item Combat", m_EquippedItem.m_ItemData.m_ItemLocalizationTag + " Equipped In Combat", string.Empty, 0L);
			}
		}
		if (flag)
		{
			DamageCharacterEvent(target, damage, (short)itemViewID, normalDamage, processingMode);
		}
		if (null != target)
		{
			target.m_fTimeLastHit = UpdateManager.time;
		}
	}

	public void DamageCharacterEvent(Character targetCharacter, float damage, short itemViewID, bool normalDamage, GamelogicRunModes processingMode)
	{
		if (targetCharacter == null || targetCharacter.m_bIsKnockedOut)
		{
			return;
		}
		bool flag = processingMode == GamelogicRunModes.AudioOnly || processingMode == GamelogicRunModes.All;
		bool flag2 = processingMode == GamelogicRunModes.NonAudioOnly || processingMode == GamelogicRunModes.All;
		if (m_CharacterStats.m_bIsPlayer && targetCharacter.m_CharacterStats.m_bIsPlayer && (targetCharacter.m_CharacterStats.GetCharacterState() == StatModifierEnum.MedicalSleeping || (LevelScript.GetCurrentLevelInfo() != null && LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport && targetCharacter.m_CharacterStats.GetCharacterState() == StatModifierEnum.SleepingInOwnBed)))
		{
			return;
		}
		GlobalCombatConfig combatConfig = ConfigManager.GetInstance().combatConfig;
		if (flag2)
		{
			CharacterNetEvents.SendSetAttackingEvent(this);
			if (PhotonNetwork.isMasterClient)
			{
				AICharacter component = targetCharacter.GetComponent<AICharacter>();
				if (component != null)
				{
					AIEvent attackingAIEvent = m_CharacterEventManager.GetAttackingAIEvent();
					component.AddEvent(attackingAIEvent);
					if (!m_CharacterStats.m_bIsPlayer)
					{
						SpeechManager.GetInstance().SaySomething(this, "Text.Inmates.Attacked", SpeechTone.Negative);
					}
				}
			}
			if (m_CharacterStats.m_bIsPlayer)
			{
				Player component2 = GetComponent<Player>();
				TutorialManager.GetInstance().StartTutorialRPC(component2, TutorialSubject.Combat);
				if (targetCharacter.m_CharacterStats.m_bIsPlayer && m_EquippedItem != null)
				{
					Item equippedItem = targetCharacter.GetEquippedItem();
					if (equippedItem != null && m_EquippedItem.m_ItemData.m_ItemDataID == StatsTracking.ENERGYSWORD_ITEM_ID && equippedItem.m_ItemData.m_ItemDataID == StatsTracking.ENERGYSWORD_ITEM_ID)
					{
						StatSystem.GetInstance().IncStat(24, 1f, component2.m_Gamer, string.Empty);
					}
				}
			}
		}
		if (targetCharacter.m_bIsBlocking && targetCharacter.m_CharacterStats.Energy > 1f)
		{
			EnergyModifier energyLevel = m_CharacterStats.EnergyLevel;
			float num = 0f;
			Item_Combat item_Combat = combatConfig.m_UnarmedCombatConfig;
			if (itemViewID != -1)
			{
				PhotonView photonView = PhotonView.Find(itemViewID);
				Item component3 = photonView.GetComponent<Item>();
				if (!(component3 == null))
				{
					Item_Combat combatData = component3.CombatData;
					if (combatData != null)
					{
						item_Combat = combatData;
					}
				}
			}
			num = ((!normalDamage) ? item_Combat.m_CombatConfig.GetHeavyAttackBlockCost(energyLevel) : item_Combat.m_CombatConfig.GetNormalAttackBlockCost(energyLevel));
			if (flag)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Block, base.gameObject);
			}
			if (!flag2)
			{
				return;
			}
			targetCharacter.ReduceEnergy(num);
			targetCharacter.KnockBackCharacter(targetCharacter.m_CachedCurrentPosition - m_CachedCurrentPosition, combatConfig.m_fKnockBackPowerOnBlock);
			if (targetCharacter.m_CharacterStats.m_bIsPlayer)
			{
				Player player = (Player)targetCharacter;
				if (player.m_Gamer.IsLocal() && Platform.GetInstance() != null)
				{
					Platform.GetInstance().DoControllerRumble(player.m_BlockingRumble, player.m_Gamer.m_RewiredPlayer.id);
				}
			}
			targetCharacter.m_fTimeLastHit = UpdateManager.time;
		}
		else if (!targetCharacter.TakeDamage(damage, this, processingMode) && flag2)
		{
			EffectManager.PlayEffect(EffectManager.effectType.AnimatedPunchEffect, targetCharacter.transform.position);
			targetCharacter.KnockBackCharacter(targetCharacter.m_CachedCurrentPosition - m_CachedCurrentPosition, combatConfig.m_fKnockBackPowerOnDamage);
			targetCharacter.m_fTimeLastHit = UpdateManager.time;
		}
	}

	public void DamageSelf(Character self, float damage)
	{
		CharacterNetEvents.SendDamageSelfEvent(self, damage);
	}

	public void DamageSelfEvent(float damage)
	{
		if (!m_bIsKnockedOut && !TakeDamage(damage, null))
		{
			EffectManager.PlayEffect(EffectManager.effectType.AnimatedPunchEffect, m_CachedCurrentPosition);
		}
	}

	private void ReduceEnergy(float amount)
	{
		m_CharacterStats.DecreaseEnergyRPC(amount);
	}

	protected virtual bool TakeDamage(float damage, Character attacker, GamelogicRunModes processingMode = GamelogicRunModes.All)
	{
		if (m_CharacterStats == null)
		{
			return false;
		}
		bool flag = processingMode == GamelogicRunModes.AudioOnly || processingMode == GamelogicRunModes.All;
		bool flag2 = processingMode == GamelogicRunModes.NonAudioOnly || processingMode == GamelogicRunModes.All;
		if (flag2)
		{
			if (OnCharacterTookDamage != null)
			{
				OnCharacterTookDamage(this, attacker);
			}
			if (m_CharacterRole == CharacterRole.Guard && attacker != null)
			{
				attacker.m_CharacterStats.IncreaseHeat(100f, CharacterStats.MessageGameplayReasons.DidDamageDuringCombat);
				GuardTowerManager.GetInstance().AlertGuardTowerRPC(attacker, AIEvent.EventType.Character_Attacking);
				if (attacker.m_CharacterStats.m_bIsPlayer)
				{
					EffectManager.PlayEffect(EffectManager.effectType.HeatIncreased, GetStatChangeEffectPosition(), m_NetView.photonView);
					Item equippedItem = attacker.GetEquippedItem();
					if (equippedItem != null && equippedItem.m_ItemData.m_ItemDataID == StatsTracking.CAKE_ITEM_ID)
					{
						StatSystem.GetInstance().IncStat(22, 1f, ((Player)attacker).m_Gamer, string.Empty);
					}
				}
			}
		}
		if (m_Outfit != null && m_Outfit.OutfitData != null && m_Outfit.OutfitData.m_ArmourConfig != null)
		{
			damage *= 1f - m_Outfit.OutfitData.m_ArmourConfig.DamageReduction;
		}
		bool flag3 = m_CharacterStats.Health - damage < 1f;
		if (flag2)
		{
			m_CharacterAnimator.StartAnimation(AnimState.CombatRecoil);
			m_CharacterStats.DecreaseHealth(damage);
		}
		string switchState = "Generic";
		if (m_EquippedItem != null)
		{
			switch (m_EquippedItem.MaterialType)
			{
			case ItemData.MATERIAL_TYPE.MAT_METAL:
				switchState = "Metal";
				break;
			case ItemData.MATERIAL_TYPE.MAT_WOOD:
				switchState = "Wood";
				break;
			}
		}
		if (flag)
		{
			AudioController.SetSwitch(Switch_Group.Player_Hit, switchState, base.gameObject);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_Hit, base.gameObject);
		}
		if (flag2)
		{
			if (flag3)
			{
				if (IsPlayer())
				{
					if (!SolitaryManager.GetInstance().IsWantedForSolitary(this))
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Infirmary Visits", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Infirmary Visit", Gamer.GetGamerCount() + " Player", 0L);
					}
					else
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("Solitary Visits", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " Solitary Visit", Gamer.GetGamerCount() + " Player", 0L);
					}
					if (attacker != null)
					{
						GoogleAnalyticsV3.LogCommericalAnalyticEvent("KO by " + attacker.m_CharacterRole, LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " KO by " + attacker.m_CharacterRole, Gamer.GetGamerCount() + " Player", 0L);
					}
				}
				else if (attacker != null && attacker.IsPlayer())
				{
					GoogleAnalyticsV3.LogCommericalAnalyticEvent("NPC " + m_CharacterRole.ToString() + " KOd by Player", LevelScript.GetCurrentLevelInfo().m_PrisonEnum.ToString() + " NPC " + m_CharacterRole.ToString() + " KOd by Player", Gamer.GetGamerCount() + " Player", 0L);
				}
				if (attacker != null && attacker.m_CharacterRole == CharacterRole.Dog && m_CharacterStats.m_bIsPlayer)
				{
					Player component = GetComponent<Player>();
					if (component != null && component.m_Gamer.IsLocal())
					{
						StatSystem.GetInstance().IncStat(19, 1f, component.m_Gamer, string.Empty);
					}
				}
				if (attacker != null)
				{
					ScoreManager.EventRPC(ScoreManager.Events.KnockedOutCharacter, attacker);
				}
				ForceStopInteraction();
			}
			else
			{
				RequestStopInteraction();
				m_fTimeLastHit = UpdateManager.time;
			}
			Item equippedItem2 = GetEquippedItem();
			if ((bool)equippedItem2 && equippedItem2.IsInUse())
			{
				equippedItem2.CancelUsing();
			}
			if (null != attacker && null != attacker.m_CharacterStats && attacker.m_CharacterStats.m_bIsPlayer && m_CharacterOpinions != null)
			{
				bool flag4 = m_LastTimeAttackedBy.ContainsKey(attacker);
				float elapsedSeconds = RoutineManager.GetInstance().GetElapsedSeconds();
				float num = 0f;
				if (flag4)
				{
					num = elapsedSeconds - m_LastTimeAttackedBy[attacker];
				}
				if (!flag4 || num > (float)OpinionManager.GetInstance().GetAttackOpinionLossInterval(m_CharacterRole))
				{
					int attackOpinionLoss = OpinionManager.GetInstance().GetAttackOpinionLoss(m_CharacterRole);
					m_CharacterOpinions.DecreaseOpinionOf(attacker, attackOpinionLoss);
					EffectManager.PlayEffect(EffectManager.effectType.OpinionDecrease, GetStatChangeEffectPosition(), m_NetView.photonView);
				}
				if (!flag4)
				{
					m_LastTimeAttackedBy.Add(attacker, elapsedSeconds);
				}
				m_LastTimeAttackedBy[attacker] = elapsedSeconds;
			}
		}
		if (flag3)
		{
			if (flag2)
			{
				m_NetView.GameplayRPC("RPC_ALL_CharacterKnockedOutFromDamage", NetTargets.All, m_NetView.viewID);
				m_CharacterStats.SetHeat(0f, CharacterStats.MessageGameplayReasons.KnockedOut);
				m_bIsDashing = false;
				if (null != attacker && null != attacker.m_CharacterStats && attacker.m_CharacterStats.m_bIsPlayer)
				{
					if (m_CharacterRole == CharacterRole.Guard)
					{
						StatSystem.GetInstance().IncStat(3, 1f, ((Player)attacker).m_Gamer, string.Empty);
					}
					else if (m_CharacterRole == CharacterRole.Inmate)
					{
						StatSystem.GetInstance().IncStat(4, 1f, ((Player)attacker).m_Gamer, string.Empty);
						if (((Player)attacker).m_Gamer.m_bPrimaryLocal)
						{
							((Player)attacker).PlayerKnockedOutInmate(m_NetView.viewID);
						}
					}
				}
				if (m_bHasQuestAvailable)
				{
					QuestManager.GetInstance().PauseQuestOfferTimer(this, paused: true);
				}
				if (m_bIsVendor)
				{
					bool success;
					Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(this, out success);
					if (success)
					{
						vendorForCharacter.PauseExpireTimer();
					}
				}
				if (!NavMeshUtil.GetPositionOnNavMesh(m_CachedCurrentPosition, out var nodePos))
				{
					m_CachedCurrentPosition = nodePos;
					m_Transform.position = m_CachedCurrentPosition;
				}
				SetIsAttacking(attacking: false);
				SetIsChipping(value: false);
				SetIsCutting(value: false);
				SetIsDigging(value: false);
				SetIsLooting(value: false);
				SetIsNaughtyLocation(value: false);
				SetIsSuspicious(value: false);
				SetIsWanted(value: false);
				SetHasTray(hasTray: false);
				SetIsSearchingDesk(value: false);
				SetIsStandingOnDesk(value: false);
				SetIsTardy(value: false);
				SetIsMissing(value: false);
				SetCarriedObject(null);
				SetCarriedCharacter(null);
				SetIsKnockedOut(knockedOut: true, attacker);
				ReportKnockedOutRPC(attacker);
				m_SpeechBubbleHandler.ClearSpeechBuffer();
				if (m_IconHandler != null)
				{
					m_IconHandler.SetIconsHidden(hidden: true);
				}
			}
			return true;
		}
		return false;
	}

	[PunRPC]
	public void RPC_ALL_CharacterKnockedOutFromDamage(int characterViewId)
	{
		Character character = T17NetView.Find<Character>(characterViewId);
		if (character != null)
		{
			if (character.m_CharacterStats.m_bIsPlayer)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_KO_Birds, character.gameObject);
			}
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_KO, character.gameObject);
		}
	}

	public void BindRPC(Item bindingItem, float bindDuration, Character binder)
	{
		m_NetView.RPC("RPC_Bind", m_NetView, bindingItem.m_NetView.viewID, bindDuration, binder.m_NetView.viewID);
	}

	[PunRPC]
	public void RPC_Bind(int bindingItemID, float bindDuration, int binderID, PhotonMessageInfo info)
	{
		Item itemResponsible = null;
		if (bindingItemID != -1)
		{
			itemResponsible = T17NetView.Find<Item>(bindingItemID);
		}
		Character characterResponsible = null;
		if (binderID != -1)
		{
			characterResponsible = T17NetView.Find<Character>(binderID);
		}
		if (!(m_CharacterStats == null))
		{
			m_fBoundEscapeTime = bindDuration;
			ForceStopInteraction();
			Item equippedItem = GetEquippedItem();
			if ((bool)equippedItem && equippedItem.IsInUse())
			{
				equippedItem.CancelUsing();
			}
			m_bIsDashing = false;
			SetIsBound(isBound: true, itemResponsible, characterResponsible);
		}
	}

	public void CombatBlock(bool doBlock)
	{
		if (m_fSmashAttackChargeTimer > 0f)
		{
			doBlock = false;
		}
		if (!doBlock)
		{
			if (m_bIsBlocking)
			{
				m_CharacterAnimator.StopAnimation(AnimState.CombatBlock);
				m_CharacterStats.UnSetCharacterState(StatModifierEnum.Blocking);
				m_bIsBlocking = false;
			}
		}
		else if (!m_bIsBlocking && m_CharacterStats.Energy > 1f)
		{
			m_CharacterAnimator.StartAnimation(AnimState.CombatBlock);
			m_CharacterStats.SetCharacterState(StatModifierEnum.Blocking);
			m_bIsBlocking = true;
		}
	}

	public void KnockBackCharacter(Vector2 direction, float power)
	{
		if (IsInteracting())
		{
			return;
		}
		m_fKnockBackStunTimer = ConfigManager.GetInstance().combatConfig.m_fKnockBackStunTime;
		bool flag = false;
		Vector3 position = m_Transform.position;
		int num = EscapistsRaycast.RaycastAll(position, direction, 1.4f, m_NoKnockbackLayerMask, QueryTriggerInteraction.Ignore);
		RaycastHit[] raycastHitList = EscapistsRaycast.RaycastHitList;
		for (int i = 0; i < num; i++)
		{
			float sqrMagnitude = (raycastHitList[i].point - m_Transform.position).sqrMagnitude;
			if (sqrMagnitude <= 1f)
			{
				if (!flag)
				{
					power /= 2f;
					flag = true;
				}
				if (sqrMagnitude <= 0.36f)
				{
					return;
				}
			}
		}
		m_CharacterMovement.KnockBack(direction.normalized * power);
	}

	public virtual void RegainConsciousness()
	{
		m_NetView.RPC("RPC_RegainConsciousness", m_NetView);
	}

	[PunRPC]
	public void RPC_RegainConsciousness(PhotonMessageInfo info)
	{
		if (!m_bIsKnockedOut || m_bIsDisabled)
		{
			return;
		}
		if (m_CharacterStats.m_bIsPlayer)
		{
			Player component = GetComponent<Player>();
			StatSystem.GetInstance().IncStat(5, 1f, component.m_Gamer, string.Empty);
		}
		else
		{
			SolitaryManager.GetInstance().SetWantedForSolitary(this, sendToSolitary: false);
		}
		SetIsSurrendered(surrendered: false);
		SetIsKnockedOut(knockedOut: false, null);
		if (_m_bIsNaked && m_CharacterRole == CharacterRole.Inmate)
		{
			EquipStartingItem(outfit: true, forceSet: false, tellOthers: true);
		}
		if (m_bHasQuestAvailable)
		{
			QuestManager.GetInstance().PauseQuestOfferTimer(this, paused: false);
		}
		if (m_bIsVendor)
		{
			bool success;
			Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(this, out success);
			if (success)
			{
				vendorForCharacter.UnpauseExpireTimer();
			}
		}
		if (m_IconHandler != null)
		{
			m_IconHandler.SetIconsHidden(hidden: false);
		}
		ResetGetToRoutineTimer();
	}

	public virtual void EscapeBindings()
	{
		m_NetView.RPC("RPC_EscapeBindings", m_NetView);
	}

	[PunRPC]
	public void RPC_EscapeBindings(PhotonMessageInfo info)
	{
		if (m_bIsBound && !m_bIsDisabled)
		{
			RegainConsciousness();
			SetIsBound(isBound: false, null, null);
			OnEscapeBindings();
		}
	}

	protected virtual void OnEscapeBindings()
	{
	}

	protected virtual void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine routine, bool forced)
	{
		if (!m_NetView.isMine)
		{
			return;
		}
		if ((oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime) || (routine != null && routine.m_BaseRoutineType == Routines.JobTime))
		{
			DoContrabandCheck();
		}
		bool flag = !m_bRoutineTargetLocationReached || (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.JobTime && m_RoutineTargetLocation == null);
		if (oldRoutine != null && oldRoutine.m_BaseRoutineType != Routines.Lockdown)
		{
			if (oldRoutine.m_BaseRoutineType == Routines.JobTime && flag)
			{
				BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(this);
				bool flag2 = charactersJob != null && JobsManager.GetInstance().WasJobInFirstTimeGracePeriod(oldRoutine, charactersJob);
				flag = !m_bHaveAnyQuotaDone && !flag2 && !m_bHaveAnyQuotaDone;
				if (m_bHaveAnyQuotaDone)
				{
					SetHaveAnyQuotaDone(haveAnyQuotaDone: false);
				}
			}
			if (flag && !m_bIsWantedForSolitary && oldRoutine.m_BaseRoutineType != Routines.Lockdown && routine.m_BaseRoutineType != Routines.Lockdown)
			{
				if (!m_bDebugNoRountinePenalty && m_fGetToRoutineTimer <= 0f)
				{
					m_CharacterStats.IncreaseHeat(oldRoutine.m_AddedHeatWhenMissed);
					int addedAlertnessWhenMissed = oldRoutine.m_AddedAlertnessWhenMissed;
					m_NetView.RPC("RPC_RoutineMissedAlertness", NetTargets.MasterClient, addedAlertnessWhenMissed);
				}
				HandleRoutineMissedEvent(oldRoutine);
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_UI, Events.Play_Routine_Fail, base.gameObject);
			}
		}
		if (!m_bSnapshotIsBeingRestored && (oldRoutine == null || routine == null || routine.m_BaseRoutineType != Routines.Lockdown || (oldRoutine.m_BaseRoutineType != Routines.LightsOut && oldRoutine.m_BaseRoutineType != Routines.Lockdown)))
		{
			ResetGetToRoutineTimer();
		}
		if (routine != null)
		{
			SetupTargetRoom(routine.m_BaseRoutineType);
		}
		if (m_bSnapshotIsBeingRestored)
		{
			if (m_RoutineTargetLocation != null || (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.FreeTime))
			{
				m_bSnapshotIsBeingRestored = false;
			}
			else if (oldRoutine == null && routine.m_BaseRoutineType == Routines.FreeTime)
			{
				m_bSnapshotIsBeingRestored = false;
			}
		}
		if (HasReachedRoutineLocation)
		{
			if ((RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.JobTime && JobsManager.GetInstance().GetCharactersJob(this) == null) || RoutineManager.GetInstance().GetCurrentRoutineBaseType() != Routines.JobTime)
			{
				HandleRoutineReachedEvent(RoutineManager.GetInstance().GetCurrentRoutine());
			}
			if (m_CharacterStats.m_bIsPlayer)
			{
				CheckForNakedDinnerAchievement();
			}
		}
		if (!m_CharacterStats.m_bIsPlayer && (routine == null || routine.m_BaseRoutineType != Routines.MealTime))
		{
			SetHasTray(hasTray: false);
		}
	}

	public virtual void HandleRoutineMissedEvent(RoutinesData.Routine oldRoutine)
	{
		if (this.MissedRoutineLocationEvent != null)
		{
			this.MissedRoutineLocationEvent(this, oldRoutine);
		}
	}

	protected virtual bool SetupTargetRoom(Routines routinetype)
	{
		m_RoutineTargetLocation = null;
		bool result = false;
		switch (routinetype)
		{
		case Routines.RollCall:
			m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.RollCall);
			break;
		case Routines.MealTime:
			m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.MealHall);
			break;
		case Routines.Exercise:
			m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.Gym);
			break;
		case Routines.ShowerTime:
			m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.Shower);
			break;
		case Routines.LightsOut:
			m_RoutineTargetLocation = m_MyCell;
			break;
		case Routines.Lockdown:
			m_RoutineTargetLocation = m_MyCell;
			break;
		case Routines.JobTime:
			if (m_JobRoom == null)
			{
				if (m_CharacterStats.m_bIsPlayer)
				{
					m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.JobOffice);
				}
			}
			else
			{
				m_RoutineTargetLocation = JobsManager.GetInstance().SetRoutineInformationForCharacter(this);
				result = true;
			}
			break;
		case Routines.ShowTime:
			m_RoutineTargetLocation = RoomManager.GetInstance().GetFirstRoomByLocation(RoomBlob.eLocation.ShowTime);
			break;
		}
		return result;
	}

	[PunRPC]
	public void RPC_RoutineMissedAlertness(int alertnessIncrease, PhotonMessageInfo info)
	{
		PrisonAlertnessManager.GetInstance().IncrementAlertnessBy(alertnessIncrease, this, PrisonAlertnessManager.AlertnessReason.MissedRoutine);
	}

	protected void SetAccessKeyEnabled(bool enabled)
	{
		if (!(m_AccessKey == null) && !(m_ItemContainer == null))
		{
			bool flag = m_ItemContainer.HasSpecificItem(m_AccessKey.m_NetView.viewID, m_AccessKey.ItemDataID, isQuestItem: false, lookIntoHidden: true);
			if (enabled && !flag)
			{
				m_ItemContainer.AddItemRPC(m_AccessKey, intoHidden: true);
			}
			else if (!enabled && flag)
			{
				m_ItemContainer.RemoveItemRPC(m_AccessKey);
			}
		}
	}

	protected void SetAccessKeyCode(int subCode)
	{
		if (m_AccessKey == null)
		{
			return;
		}
		KeyFunctionality keyFunctionality = (KeyFunctionality)m_AccessKey.HasFunctionality(BaseItemFunctionality.Functionality.Key);
		if (keyFunctionality != null)
		{
			int subCode2 = keyFunctionality.SubCode;
			if (subCode2 != subCode)
			{
				keyFunctionality.SetKeySubCode(subCode);
				m_AccessKeySubCode = subCode;
				DoorManager.GetInstance().SetUpCharacterKeys(this);
			}
		}
	}

	public void ResetGetToRoutineTimer(bool bForce = false)
	{
		if ((m_NetView.isMine || bForce) && m_CharacterRole == CharacterRole.Inmate)
		{
			m_fGetToRoutineTimer = 0f;
			RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
			if (currentRoutine != null)
			{
				m_fGetToRoutineTimer = currentRoutine.m_TimeToGetToRoutine;
			}
			SetIsTardy(value: false);
			SetIsMissing(value: false);
		}
	}

	public bool IsSpeaking()
	{
		if (m_SpeechBubbleHandler != null)
		{
			return m_SpeechBubbleHandler.IsProcessingSpeech();
		}
		return false;
	}

	private string GetSpeechDecorationString(SpeechDecorations decoration)
	{
		if (decoration == SpeechDecorations.FivePrisonStars)
		{
			return "[TICON=Star][TICON=Star][TICON=Star][TICON=Star][TICON=Star]";
		}
		return string.Empty;
	}

	private bool Hack_CanPlaySpeech()
	{
		if (GlobalStart.GetInstance().GetCurrentSelectedPrisonEnum() == LevelScript.PRISON_ENUM.DLC05)
		{
			List<Player> allPlayers = Player.GetAllPlayers();
			return allPlayers.FindIndex((Player x) => x != null && x.m_Gamer != null && x.CurrentFloor == CurrentFloor && x.m_Gamer.IsLocal()) != -1;
		}
		return true;
	}

	[PunRPC]
	public void RPC_SaySomething(string textID, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration, PhotonMessageInfo info)
	{
		if (!(m_SpeechBubbleHandler != null) || !Hack_CanPlaySpeech())
		{
			return;
		}
		string localized = string.Empty;
		if (decoration == SpeechDecorations.None)
		{
			if (Localization.Get(textID, out localized, forcedVariation))
			{
				m_SpeechBubbleHandler.NewSpeech(localized, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
			}
			else
			{
				m_SpeechBubbleHandler.NewSpeech(textID, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
			}
			return;
		}
		string text = GetSpeechDecorationString(decoration);
		if (!string.IsNullOrEmpty(text))
		{
			text = '\n' + text;
		}
		if (Localization.Get(textID, out localized, forcedVariation))
		{
			m_SpeechBubbleHandler.NewSpeech(localized + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
		}
		else
		{
			m_SpeechBubbleHandler.NewSpeech(textID + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
		}
	}

	[PunRPC]
	public void RPC_SaySomethingWithReplaceNetViewID(string textID, string token, int replacementViewID, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration, PhotonMessageInfo info)
	{
		if (!(m_SpeechBubbleHandler != null) || !Hack_CanPlaySpeech())
		{
			return;
		}
		string text = GetSpeechDecorationString(decoration);
		if (!string.IsNullOrEmpty(text))
		{
			text = '\n' + text;
		}
		string localized = string.Empty;
		if (Localization.Get(textID, out localized, forcedVariation))
		{
			Character character = T17NetView.Find<Character>(replacementViewID);
			if (character != null)
			{
				localized = localized.Replace(token, character.m_CharacterCustomisation.m_DisplayName);
			}
			m_SpeechBubbleHandler.NewSpeech(localized + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
		}
	}

	[PunRPC]
	public void RPC_SaySomethingWithReplaceString(string textID, string token, string replacementID, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration, PhotonMessageInfo info)
	{
		if (!(m_SpeechBubbleHandler != null) || !Hack_CanPlaySpeech())
		{
			return;
		}
		string text = GetSpeechDecorationString(decoration);
		if (!string.IsNullOrEmpty(text))
		{
			text = '\n' + text;
		}
		string localized = string.Empty;
		if (Localization.Get(textID, out localized, forcedVariation))
		{
			string localized2 = string.Empty;
			if (Localization.Get(replacementID, out localized2))
			{
				localized = localized.Replace(token, localized2);
				m_SpeechBubbleHandler.NewSpeech(localized + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
			}
		}
	}

	[PunRPC]
	public void RPC_SaySomethingWithDirectReplaceString(string textID, string token, string replacementString, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration, PhotonMessageInfo info)
	{
		if (m_SpeechBubbleHandler != null && Hack_CanPlaySpeech())
		{
			string text = GetSpeechDecorationString(decoration);
			if (!string.IsNullOrEmpty(text))
			{
				text = '\n' + text;
			}
			string localized = string.Empty;
			if (Localization.Get(textID, out localized, forcedVariation))
			{
				localized = localized.Replace(token, replacementString);
				m_SpeechBubbleHandler.NewSpeech(localized + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
			}
		}
	}

	[PunRPC]
	public void RPC_SaySomethingWithReplacements(string textID, string tokenOne, object replacementOne, bool bIsOneCharacterNetviewID, string tokenTwo, object replacementTwo, bool bIsTwoCharacterNetviewID, SpeechTone tone, float duration, int priority, int forcedVariation, bool bAllowTextRecolour, SpeechDecorations decoration, PhotonMessageInfo info)
	{
		if (!(m_SpeechBubbleHandler != null) || !Hack_CanPlaySpeech())
		{
			return;
		}
		string text = GetSpeechDecorationString(decoration);
		if (!string.IsNullOrEmpty(text))
		{
			text = '\n' + text;
		}
		string localized = string.Empty;
		if (!Localization.Get(textID, out localized, forcedVariation))
		{
			return;
		}
		string localized2 = string.Empty;
		if (bIsOneCharacterNetviewID)
		{
			Character character = T17NetView.Find<Character>((int)replacementOne);
			if (character != null)
			{
				localized2 = character.m_CharacterCustomisation.m_DisplayName;
			}
		}
		else if (!Localization.Get((string)replacementOne, out localized2))
		{
			return;
		}
		string localized3 = string.Empty;
		if (bIsTwoCharacterNetviewID)
		{
			Character character2 = T17NetView.Find<Character>((int)replacementTwo);
			if (character2 != null)
			{
				localized3 = character2.m_CharacterCustomisation.m_DisplayName;
			}
		}
		else if (!Localization.Get((string)replacementTwo, out localized3))
		{
			return;
		}
		localized = localized.Replace(tokenOne, localized2);
		localized = localized.Replace(tokenTwo, localized3);
		m_SpeechBubbleHandler.NewSpeech(localized + text, tone, duration, priority, m_CharacterRole == CharacterRole.Guard && bAllowTextRecolour);
	}

	public int GetOpinionOf(Character other)
	{
		if (m_CharacterOpinions != null)
		{
			return m_CharacterOpinions.GetOpinionOf(other);
		}
		return 50;
	}

	public Vector3 GetStatChangeEffectPosition()
	{
		Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
		float x = cachedCurrentPosition.x;
		Vector2 effectOffsetStat = m_EffectOffsetStat;
		cachedCurrentPosition.x = x + effectOffsetStat.x;
		float y = cachedCurrentPosition.y;
		Vector2 effectOffsetStat2 = m_EffectOffsetStat;
		cachedCurrentPosition.y = y + effectOffsetStat2.y;
		FloorManager instance = FloorManager.GetInstance();
		if (instance != null)
		{
			cachedCurrentPosition.z = (float)m_CurrentFloor.m_zPos + (float)instance.m_FloorOffset * 0.98f;
		}
		return cachedCurrentPosition;
	}

	public void Walk(Vector2 desiredVelocity, CharacterSpeed speedOverride = CharacterSpeed.COUNT)
	{
		if (!m_NetView.isMine)
		{
			return;
		}
		if (m_fPauseMovementTimer > 0f || m_fKnockBackStunTimer > 0f)
		{
			m_CharacterMovement.Stand();
			return;
		}
		if (GetIsImmobilised())
		{
			m_CharacterMovement.Immobile();
			return;
		}
		if (null != m_InteractingObject && m_InteractingObject.OverrideWalk())
		{
			m_InteractingObject.Walk(desiredVelocity);
			return;
		}
		Vector2 desiredDirection = desiredVelocity;
		if (m_CharacterTarget != null)
		{
			Vector3 cachedCurrentPosition = m_CachedCurrentPosition;
			Vector3 cachedCurrentPosition2 = m_CharacterTarget.m_CachedCurrentPosition;
			Vector2 vector = cachedCurrentPosition2 - cachedCurrentPosition;
			bool haveCollisionData = false;
			if (m_CharacterUtil.LineOfSight(cachedCurrentPosition2, out haveCollisionData) || m_CharacterStats.m_bIsPlayer)
			{
				desiredDirection = vector;
			}
		}
		if (m_CharacterMovement.Walk(desiredVelocity, speedOverride) || (m_bIsDashing && !(m_CharacterTarget != null)))
		{
			return;
		}
		if (m_StandingStillTime <= 0f)
		{
			m_CharacterAnimator.StopOneShotAnim(m_StandingStillAnimState);
			if (m_CharacterAnimator.OnOneShotDone == new CharacterAnimator.OneShotDone(OnOneShotStandingAnim))
			{
				OnOneShotStandingAnim();
			}
			CharacterAnimator characterAnimator = m_CharacterAnimator;
			characterAnimator.OnOneShotDone = (CharacterAnimator.OneShotDone)Delegate.Remove(characterAnimator.OnOneShotDone, new CharacterAnimator.OneShotDone(OnOneShotStandingAnim));
			m_StandingStillAnimState = AnimState.Hammer;
		}
		m_StandingStillTime = UnityEngine.Random.Range(0f - m_StandingStillTimerVar, m_StandingStillTimerVar) + m_StandingStillTimeout;
		CalcFaceDirection(desiredDirection);
	}

	public void PauseMovement(float pauseTimer, bool force = false)
	{
		m_fPauseMovementTimer = ((!force) ? Mathf.Max(pauseTimer, m_fPauseMovementTimer) : pauseTimer);
		m_CharacterMovement.Immobile();
	}

	public bool IsInteracting()
	{
		return m_InteractingObject != null || m_RemoteInteractingObject != null;
	}

	public void ForceStopInteraction()
	{
		if (!m_bIsBeingDestroyed && IsInteracting())
		{
			m_NetView.RPC("RPC_ForceStopInteraction", m_NetView);
		}
	}

	[PunRPC]
	public void RPC_ForceStopInteraction(PhotonMessageInfo info)
	{
		if (!m_bIsBeingDestroyed && null != m_InteractingObject)
		{
			m_InteractingObject.ForceStopInteraction(this);
		}
	}

	public void RequestStopInteraction()
	{
		if (!m_bIsBeingDestroyed && IsInteracting())
		{
			m_NetView.RPC("RPC_RequestStopInteraction", m_NetView);
		}
	}

	[PunRPC]
	public void RPC_RequestStopInteraction(PhotonMessageInfo info)
	{
		if (!m_bIsBeingDestroyed && null != m_InteractingObject)
		{
			m_InteractingObject.RequestStopInteraction(this);
		}
	}

	public void RemoteForceInteraction(InteractiveObject target)
	{
		if (!m_bIsBeingDestroyed)
		{
			int viewID = target.m_NetObjectLock.m_NetView.viewID;
			int localInteractionId = target.GetLocalInteractionId();
			m_NetView.RPC("RPC_RemoteForceInteraction", m_NetView, viewID, localInteractionId);
		}
	}

	[PunRPC]
	public void RPC_RemoteForceInteraction(int netLockID, int interactionID, PhotonMessageInfo info)
	{
		if (m_bIsBeingDestroyed)
		{
			return;
		}
		if (null != m_InteractingObject)
		{
			m_InteractingObject.ForceStopInteraction(this);
		}
		NetObjectLock netObjectLock = T17NetView.Find<NetObjectLock>(netLockID);
		if (!(netObjectLock == null))
		{
			InteractiveObject interactiveObject = netObjectLock.GetInteractiveObject(interactionID);
			if (!(interactiveObject == null))
			{
				interactiveObject.Interact(this);
			}
		}
	}

	public void FaceCharacter(Character targetChar)
	{
		CalcFaceDirection(targetChar.m_CachedCurrentPosition - m_CachedCurrentPosition);
	}

	public void FacePosition(Vector3 targetPos)
	{
		CalcFaceDirection(targetPos - m_CachedCurrentPosition);
	}

	public CharacterSpeed CalcNetworkSpeed(float fSpeed)
	{
		CharacterSpeed characterSpeed = CharacterSpeed.COUNT;
		if (m_CharacterMovement == null)
		{
			return CharacterSpeed.Stand;
		}
		float travelDistance = m_CharacterMovement.GetTravelDistance(CharacterSpeed.Walk);
		if (fSpeed > travelDistance * 1.1f)
		{
			return CharacterSpeed.Run;
		}
		if (fSpeed > 0.01f)
		{
			return CharacterSpeed.Walk;
		}
		return CharacterSpeed.Stand;
	}

	public void CalcFaceDirection(Vector2 desiredDirection)
	{
		if (m_vFacingDirection.x == desiredDirection.x && m_vFacingDirection.y == desiredDirection.y)
		{
			return;
		}
		m_vFacingDirection = desiredDirection;
		m_vFacingDirection.NormalizeAndMag(out var mag);
		if (mag <= float.Epsilon)
		{
			return;
		}
		float num = 0f;
		bool flag = false;
		Directionx4 x4FacingDirection = Directionx4.Up;
		if (desiredDirection.x == 0f)
		{
			if (desiredDirection.y > 0f)
			{
				m_x8FacingDirection = Directionx8.Up;
				x4FacingDirection = Directionx4.Up;
			}
			else
			{
				m_x8FacingDirection = Directionx8.Down;
				x4FacingDirection = Directionx4.Down;
			}
			flag = true;
		}
		else if (desiredDirection.y == 0f)
		{
			if (desiredDirection.x > 0f)
			{
				m_x8FacingDirection = Directionx8.Right;
				x4FacingDirection = Directionx4.Right;
			}
			else
			{
				m_x8FacingDirection = Directionx8.Left;
				x4FacingDirection = Directionx4.Left;
			}
			flag = true;
		}
		else
		{
			num = desiredDirection.x / desiredDirection.y;
		}
		if (!flag)
		{
			bool flag2 = desiredDirection.x > 0f;
			if (desiredDirection.y > 0f)
			{
				if (flag2)
				{
					if (num > x8TestHigh)
					{
						m_x8FacingDirection = Directionx8.Right;
					}
					else if (num > x8TestLow)
					{
						m_x8FacingDirection = Directionx8.UpRight;
					}
					else
					{
						m_x8FacingDirection = Directionx8.Up;
					}
					x4FacingDirection = ((num > x4Test) ? Directionx4.Right : Directionx4.Up);
				}
				else
				{
					if (num < 0f - x8TestHigh)
					{
						m_x8FacingDirection = Directionx8.Left;
					}
					else if (num < 0f - x8TestLow)
					{
						m_x8FacingDirection = Directionx8.UpLeft;
					}
					else
					{
						m_x8FacingDirection = Directionx8.Up;
					}
					x4FacingDirection = ((num < 0f - x4Test) ? Directionx4.Left : Directionx4.Up);
				}
			}
			else if (flag2)
			{
				if (num < 0f - x8TestHigh)
				{
					m_x8FacingDirection = Directionx8.Right;
				}
				else if (num < 0f - x8TestLow)
				{
					m_x8FacingDirection = Directionx8.DownRight;
				}
				else
				{
					m_x8FacingDirection = Directionx8.Down;
				}
				x4FacingDirection = ((!(num < 0f - x4Test)) ? Directionx4.Down : Directionx4.Right);
			}
			else
			{
				if (num > x8TestHigh)
				{
					m_x8FacingDirection = Directionx8.Left;
				}
				else if (num > x8TestLow)
				{
					m_x8FacingDirection = Directionx8.DownLeft;
				}
				else
				{
					m_x8FacingDirection = Directionx8.Down;
				}
				x4FacingDirection = ((!(num > x4Test)) ? Directionx4.Down : Directionx4.Left);
			}
		}
		switch (m_x8FacingDirection)
		{
		case Directionx8.Up:
		case Directionx8.Left:
		case Directionx8.Down:
		case Directionx8.Right:
			m_x4FacingDirection = x4FacingDirection;
			break;
		case Directionx8.UpLeft:
			if (m_x4FacingDirection == Directionx4.Down || m_x4FacingDirection == Directionx4.Right)
			{
				m_x4FacingDirection = x4FacingDirection;
			}
			break;
		case Directionx8.DownLeft:
			if (m_x4FacingDirection == Directionx4.Up || m_x4FacingDirection == Directionx4.Right)
			{
				m_x4FacingDirection = x4FacingDirection;
			}
			break;
		case Directionx8.DownRight:
			if (m_x4FacingDirection == Directionx4.Up || m_x4FacingDirection == Directionx4.Left)
			{
				m_x4FacingDirection = x4FacingDirection;
			}
			break;
		case Directionx8.UpRight:
			if (m_x4FacingDirection == Directionx4.Down || m_x4FacingDirection == Directionx4.Left)
			{
				m_x4FacingDirection = x4FacingDirection;
			}
			break;
		}
		SetFaceDirection(m_x4FacingDirection, updateFacingDirections: false);
	}

	public void SetFaceDirection(FacingDirectionIncInvalid headAndBodyDirection, bool updateFacingDirections = true)
	{
		if (headAndBodyDirection != 0)
		{
			Directionx4 headAndBodyDirection2 = (Directionx4)(headAndBodyDirection - 1);
			SetFaceDirection(headAndBodyDirection2, updateFacingDirections);
		}
	}

	public void SetFaceDirection(Directionx4 headAndBodyDirection, bool updateFacingDirections = true)
	{
		if (updateFacingDirections)
		{
			m_x4FacingDirection = headAndBodyDirection;
			m_x8FacingDirection = (Directionx8)headAndBodyDirection;
			m_vFacingDirection = Direction.DirectionToVector(headAndBodyDirection);
		}
		if (m_CharacterAnimator != null)
		{
			m_CharacterAnimator.HeadAndBodyFaceDirection(headAndBodyDirection);
			m_CharacterAnimator.SetSpotlightDirection(m_x8FacingDirection, m_vFacingDirection);
		}
	}

	public Vector2 GetFacingDirection()
	{
		return m_vFacingDirection;
	}

	public void DropInventoryItem(Item item)
	{
		if (!(item == null))
		{
			int viewID = item.m_NetView.viewID;
			if (m_ItemContainer.HasSpecificItem(viewID) && DropItemCheck(item, m_CachedCurrentPosition))
			{
				m_ItemContainer.RemoveItemRPC(item);
				item.DropItemInLevel(this, m_CachedCurrentPosition);
			}
		}
	}

	public void DropInventoryItem(ItemContainer container, int index)
	{
		Item item = container.GetItem(index);
		if (!(item == null) && DropItemCheck(item, m_CachedCurrentPosition))
		{
			container.RemoveItemRPC(item);
			item.DropItemInLevel(this, m_CachedCurrentPosition);
		}
	}

	public bool DropEquipedItem(Vector3 position)
	{
		Item equippedItem = GetEquippedItem();
		if (equippedItem != null && DropItemCheck(equippedItem, position))
		{
			SetEquippedItem(null, bTellOthers: true, bAddOldToInventory: false);
			equippedItem.DropItemInLevel(this, position);
			return true;
		}
		return false;
	}

	public bool DropItemCheck(Item item, Vector3 position, bool silent = false)
	{
		if (!silent && CurrentFloor.IsUnderGround() && item.HasFunctionality(BaseItemFunctionality.Functionality.FillHole) != null && RoomManager.GetInstance().LookUpRoom(position) == null)
		{
			if (m_CharacterStats.m_bIsPlayer)
			{
				SpeechManager.GetInstance().SaySomething(this, "Text.Player.InvalidSoilDrop", SpeechTone.Negative, 3f, 10);
			}
			return false;
		}
		Vector3 nodePos = default(Vector3);
		if (NavMeshUtil.GetPositionOnNavMesh(position, out nodePos))
		{
			List<Item> list = null;
			if (m_ProximityDetector.GetAnyItems(ref list) > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if ((list[i].transform.position - nodePos).sqrMagnitude < 0.1f)
					{
						if (m_CharacterStats.m_bIsPlayer && !silent)
						{
							SpeechManager.GetInstance().SaySomething(this, "Text.Player.InvalidItemDrop", SpeechTone.Negative, 3f, 10);
						}
						return false;
					}
				}
			}
			float num = 1.5f;
			Vector3 vector = new Vector3(0f, 0f, 1f);
			Vector3 origin = nodePos - vector * num;
			int layerMask = 1 << LayerMask.NameToLayer("Door");
			if (EscapistsRaycast.RaycastAll(origin, vector, num, layerMask, QueryTriggerInteraction.Collide) > 0)
			{
				if (m_CharacterStats.m_bIsPlayer && !silent)
				{
					SpeechManager.GetInstance().SaySomething(this, "Text.Player.CantDropItemHere", SpeechTone.Negative, 3f, 10);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void OnItemsChanged()
	{
		DoContrabandCheck();
	}

	private void DoContrabandCheck()
	{
		bool flag = false;
		if (m_CharacterRole != 0)
		{
			return;
		}
		RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
		if (currentRoutine != null && currentRoutine.m_BaseRoutineType == Routines.JobTime)
		{
			BaseJob charactersJob = JobsManager.GetInstance().GetCharactersJob(this);
			if (charactersJob != null && m_CurrentLocation != null && m_CurrentLocation == charactersJob.Room && !m_ItemContainer.HasKeyItem())
			{
				bool flag2 = false;
				if (m_EquippedItem != null && m_EquippedItem.m_ItemData != null && (m_EquippedItem.m_ItemData.HasFunctionality(BaseItemFunctionality.Functionality.Key) != null || m_EquippedItem.m_ItemData.HasFunctionality(BaseItemFunctionality.Functionality.Keycard) != null))
				{
					flag2 = true;
				}
				if (!flag2)
				{
					SetHasContraband(value: false);
					return;
				}
			}
		}
		if (m_ItemContainer.HasItemWithFunctionality(BaseItemFunctionality.Functionality.HideContraband) == 0)
		{
			flag = m_ItemContainer.HasContrabandItems();
			if (!flag && m_EquippedItem != null && m_EquippedItem.m_ItemData != null)
			{
				flag = m_EquippedItem.m_ItemData.IsContraband();
			}
		}
		SetHasContraband(flag);
	}

	public virtual bool Teleport(Vector3 newPosition)
	{
		FloorManager instance = FloorManager.GetInstance();
		if (null == instance)
		{
			return false;
		}
		FloorManager.Floor floor = instance.FindFloorAtZ(newPosition.z);
		if (floor == null)
		{
			return false;
		}
		return Teleport(newPosition, floor);
	}

	public virtual bool Teleport(Vector3 pos, FloorManager.Floor newFloor, bool instantUpdate = true)
	{
		Vector3 vector = pos;
		if (newFloor != null)
		{
			vector.z = newFloor.m_zPos;
		}
		m_vNetworkedPosition = vector;
		m_vNetworkedPositionPrevious = vector;
		m_vNetworkedPositionPreviousLocal = vector;
		m_PreviousPosition = vector;
		m_CachedCurrentPosition = vector;
		m_Transform.position = m_CachedCurrentPosition;
		m_vVelocity = Vector2.zero;
		m_bInstantPositionUpdate = instantUpdate;
		UpdateCurrentLocationWithValidation(performPositionChecks: false);
		ChangeFloor(newFloor);
		return true;
	}

	public virtual bool ChangeFloor(FloorManager.Floor floor)
	{
		if (floor == null)
		{
			return false;
		}
		if (floor != CurrentFloor)
		{
			int floorIndex = CurrentFloor.m_FloorIndex;
			CurrentFloor = floor;
			OnFloorChange(floorIndex);
		}
		return true;
	}

	protected virtual void OnFloorChange(int oldFloorIndex)
	{
		TriggerCameraRefresh();
		if (this.OnFloorChangedEvent != null)
		{
			this.OnFloorChangedEvent();
		}
	}

	protected virtual void OnStandingOnDeskChange(bool bIsOnDesk)
	{
	}

	public void TriggerCameraRefresh()
	{
		CameraManager instance = CameraManager.GetInstance();
		Camera targetCharactersCamera = instance.GetTargetCharactersCamera(this);
		if (targetCharactersCamera != null)
		{
			instance.SetCullerUpdateMode(targetCharactersCamera, CullerUpdateMode.ForcedNextFrameOnly);
		}
	}

	public int CheckForCollisions(int layerMask)
	{
		Array.Clear(m_LastCollisionCheckResults, 0, m_LastCollisionCheckResults.Length);
		if (m_PhysicsCollider != null && m_PhysicsCollider.GetActive() && m_PhysicsSphereCol != null)
		{
			return Physics.OverlapSphereNonAlloc(m_PhysicsSphereCol.transform.position + m_PhysicsSphereCol.center, m_PhysicsSphereCol.radius, m_LastCollisionCheckResults, layerMask);
		}
		return 0;
	}

	public Collider[] GetLastCollisionCheckResults()
	{
		return m_LastCollisionCheckResults;
	}

	public override string ToString()
	{
		string text = m_CharacterName;
		if (m_CharacterCustomisation != null)
		{
			text = m_CharacterCustomisation.m_DisplayName;
		}
		return "\"" + text + "\" " + base.name + " " + GetType();
	}

	public void SetCharacterSleeping(bool isSleeping)
	{
		StatModifierEnum statModifierEnum = StatModifierEnum.Sleeping;
		if (m_MyCell != null && m_InteractingObject != null)
		{
			RoomBlob_Cell roomBlobData = m_MyCell.GetRoomBlobData<RoomBlob_Cell>();
			if (roomBlobData != null && m_InteractingObject == roomBlobData.GetCellObject(typeof(BedInteraction), this))
			{
				statModifierEnum = StatModifierEnum.SleepingInOwnBed;
			}
		}
		if (isSleeping)
		{
			m_CharacterStats.SetCharacterState(statModifierEnum);
			m_CharacterAnimator.ShowSleeping(bIsSleep: true);
			return;
		}
		m_CharacterAnimator.ShowSleeping(bIsSleep: false);
		if (m_CharacterStats.m_bIsPlayer && statModifierEnum == StatModifierEnum.SleepingInOwnBed)
		{
			TutorialManager instance = TutorialManager.GetInstance();
			PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
			if (instance != null && currentLevelInfo != null && currentLevelInfo.m_PrisonType != LevelScript.PRISON_TYPE.Transport)
			{
				instance.StartTutorialRPC(this as Player, TutorialSubject.Routines);
			}
		}
		m_CharacterStats.UnSetCharacterState(statModifierEnum);
	}

	public bool HasItemsOnPerson(List<ItemData> items)
	{
		List<ItemData> out_ItemsMissing = null;
		return HasItemsOnPerson(items, ref out_ItemsMissing);
	}

	public bool HasItemsOnPerson(List<ItemData> items, ref List<ItemData> out_ItemsMissing)
	{
		List<ItemData> itemsFoundInContainer = new List<ItemData>();
		if (m_ItemContainer.HasOneOfEachItem(items, itemsFoundInContainer))
		{
			if (out_ItemsMissing != null)
			{
				out_ItemsMissing.Clear();
			}
			return true;
		}
		if (out_ItemsMissing == null && items.Count - itemsFoundInContainer.Count > 2)
		{
			return false;
		}
		List<ItemData> list = new List<ItemData>(items);
		list.RemoveAll((ItemData x) => itemsFoundInContainer.Contains(x));
		ItemData equippedItemData = ((!(GetEquippedItem() != null)) ? null : GetEquippedItem().m_ItemData);
		if (equippedItemData != null)
		{
			list.RemoveAll((ItemData x) => x.m_ItemDataID == equippedItemData.m_ItemDataID);
		}
		ItemData outfitItemData = ((!(GetOutFit() != null)) ? null : GetOutFit().m_ItemData);
		if (outfitItemData != null)
		{
			list.RemoveAll((ItemData x) => x.m_ItemDataID == outfitItemData.m_ItemDataID);
		}
		if (out_ItemsMissing != null)
		{
			out_ItemsMissing = list;
		}
		return list.Count == 0;
	}

	public bool HasItemOnPerson(ItemData itemToSearchFor)
	{
		if (m_EquippedItem != null && m_EquippedItem.ItemDataID == itemToSearchFor.m_ItemDataID)
		{
			return true;
		}
		if (m_Outfit != null && m_Outfit.ItemDataID == itemToSearchFor.m_ItemDataID)
		{
			return true;
		}
		if (m_ItemContainer.GetItemWithItemDataId(itemToSearchFor.m_ItemDataID) != null)
		{
			return true;
		}
		return false;
	}

	public void SetInteractionRequestInFlight(bool state)
	{
		m_bIsInteractionRequestInFlight = state;
		if (m_bIsInteractionRequestInFlight)
		{
			m_InteractionRequestTimeStamp = UpdateManager.time;
		}
	}

	private int FindMyIndexForAllCharacters()
	{
		for (int num = m_AllCharacters.Count - 1; num >= 0; num--)
		{
			if (m_AllCharacters[num] == this)
			{
				return num;
			}
		}
		return -1;
	}

	public virtual void NetworkSerializeWrite(BitStreamWriter bitWriter, bool KeyFrame)
	{
		m_bForceSerialize |= KeyFrame;
		m_NetSerializeByteList.Clear();
		if (m_NetSerializeWriter == null)
		{
			m_NetSerializeWriter = new BitStreamWriter(m_NetSerializeByteList);
		}
		else
		{
			m_NetSerializeWriter.Reset(m_NetSerializeByteList);
		}
		bitWriter.Write((uint)m_CharacterSerializeIndex, 8);
		bool bWriteFacingDirection = false;
		SerializePosition(bitWriter, out bWriteFacingDirection);
		SerializeAnimation(bitWriter, bWriteFacingDirection);
		SerializeCharacterState(bitWriter);
		bitWriter.Write(m_bForceSerialize);
		if (m_CharacterStats != null)
		{
			m_CharacterStats.SerializeToView(bitWriter);
		}
		if (WriteThing(ref m_SLZ_NetObjectLockID, m_MC_NetObjectLockID, bitWriter))
		{
			bitWriter.Write((uint)m_MC_NetObjectLockID, 12);
		}
		m_bForceSerialize = false;
	}

	private byte[] CompareByteArray(byte[] a, FastList<byte> b)
	{
		if (m_bForceSerialize || a == null || b == null || a.Length != b.Count)
		{
			return b.ToArray();
		}
		for (int num = a.Length - 1; num >= 0; num--)
		{
			if (a[num] != b[num])
			{
				return b.ToArray();
			}
		}
		return null;
	}

	public virtual void NetworkSerializeRead(BitStreamReader bitReader, float fSentFrame)
	{
		bool bReadFacingDirection = false;
		DeserializePosition(bitReader, fSentFrame, out bReadFacingDirection);
		DeserializeAnimation(bitReader, bReadFacingDirection);
		DeserializeCharacterState(bitReader);
		if (bitReader.ReadBit())
		{
			m_bSerialiseInit = true;
		}
		if (m_CharacterStats != null)
		{
			m_CharacterStats.DeserializeFromView(bitReader);
		}
		if (bitReader.ReadBit())
		{
			m_MC_NetObjectLockID = (int)bitReader.ReadUInt32(12);
		}
		m_LastReadFrameCount = UpdateManager.frameCount;
	}

	public virtual void PlayerControlledSet(bool bValue)
	{
	}

	public virtual bool IsPlayer()
	{
		return false;
	}

	private void SerializeCharacterState(BitStreamWriter bitWriter)
	{
		int num = 0;
		if (_m_bIsKnockedOut)
		{
			AIEvent knockedOutAIEvent = m_CharacterEventManager.GetKnockedOutAIEvent();
			if (knockedOutAIEvent != null)
			{
				Character characterResponsible = knockedOutAIEvent.m_CharacterResponsible;
				if (characterResponsible != null)
				{
					num = characterResponsible.m_NetView.viewID;
				}
			}
		}
		int num2 = 0;
		if (_m_bIsBound)
		{
			AIEvent boundAIEvent = m_CharacterEventManager.GetBoundAIEvent();
			if (boundAIEvent != null)
			{
				Character characterResponsible2 = boundAIEvent.m_CharacterResponsible;
				if (characterResponsible2 != null)
				{
					num2 = characterResponsible2.m_NetView.viewID;
				}
			}
		}
		int num3 = 0;
		int num4 = 0;
		if (m_AccessKey != null)
		{
			num3 = m_AccessKey.m_NetView.viewID;
			num4 = m_AccessKeySubCode;
		}
		bool flag = m_SLZ_KeyNetID != num3;
		bool flag2 = m_SLZ_KeySubCode != num4;
		bool flag3 = m_SLZ_koCharacterNetID != num;
		bool flag4 = m_SLZ_boundCharacterNetID != num2;
		bool flag5 = m_SLZ_GetToRoutineTimer != m_fGetToRoutineTimer;
		m_SLZSerializer.Reset();
		m_SLZSerializer.Set(_m_bIsNaked);
		m_SLZSerializer.Set(_m_bHasContraband);
		m_SLZSerializer.Set(_m_bIsNaughtyLocation);
		m_SLZSerializer.Set(_m_bIsTardy);
		m_SLZSerializer.Set(_m_bIsMissing);
		m_SLZSerializer.Set(_m_bIsStandingOnDesk);
		m_SLZSerializer.Set(_m_bIsWanted);
		m_SLZSerializer.Set(_m_bIsSuspicious);
		m_SLZSerializer.Set(_m_bIsDigging);
		m_SLZSerializer.Set(_m_bIsChipping);
		m_SLZSerializer.Set(_m_bIsCutting);
		m_SLZSerializer.Set(_m_bIsSearchingDesk);
		m_SLZSerializer.Set(m_bIsKnockedOut);
		m_SLZSerializer.Set(_m_bIsBound);
		m_SLZSerializer.Set(m_bIsHidden);
		m_SLZSerializer.Set(m_bExitSolitaryFreePass);
		m_SLZSerializer.Set(m_bIsGamerControlled);
		m_SLZSerializer.Set(GetIsDisabled());
		m_SLZSerializer.Set(m_bItemInUse);
		m_SLZSerializer.Set(flag);
		m_SLZSerializer.Set(flag2);
		m_SLZSerializer.Set(flag3);
		m_SLZSerializer.Set(flag4);
		m_SLZSerializer.Set(flag5);
		m_SLZSerializer.Set(m_bIsRobinsonCharacter);
		int @int = m_SLZSerializer.GetInt(32);
		if (WriteThing(ref m_SLZ_CharacterState, @int, bitWriter))
		{
			bitWriter.Write((uint)@int, 25);
			if (flag)
			{
				bitWriter.Write((uint)num3, 12);
			}
			if (flag2)
			{
				bitWriter.Write((uint)num4, 6);
			}
			if (flag3)
			{
				bitWriter.Write((uint)num, 12);
			}
			if (flag4)
			{
				bitWriter.Write((uint)num2, 12);
			}
			if (flag5)
			{
				bitWriter.Write((uint)m_fGetToRoutineTimer, 8);
			}
		}
	}

	private void DeserializeCharacterState(BitStreamReader bitReader)
	{
		int num = 0;
		int num2 = 0;
		if (!bitReader.ReadBit())
		{
			return;
		}
		m_SLZ_PreviousDeserializeCharacterStateInt = (int)bitReader.ReadUInt32(25);
		m_SLZDeserializer.Reset();
		m_SLZDeserializer.Set(32, m_SLZ_PreviousDeserializeCharacterStateInt);
		SetIsNaked(m_SLZDeserializer.GetBool());
		SetHasContraband(m_SLZDeserializer.GetBool());
		SetIsNaughtyLocation(m_SLZDeserializer.GetBool());
		SetIsTardy(m_SLZDeserializer.GetBool());
		SetIsMissing(m_SLZDeserializer.GetBool());
		SetIsStandingOnDesk(m_SLZDeserializer.GetBool());
		SetIsWanted(m_SLZDeserializer.GetBool());
		SetIsSuspicious(m_SLZDeserializer.GetBool());
		SetIsDigging(m_SLZDeserializer.GetBool());
		SetIsChipping(m_SLZDeserializer.GetBool());
		SetIsCutting(m_SLZDeserializer.GetBool());
		SetIsSearchingDesk(m_SLZDeserializer.GetBool());
		bool @bool = m_SLZDeserializer.GetBool();
		bool bool2 = m_SLZDeserializer.GetBool();
		m_bIsHidden = m_SLZDeserializer.GetBool();
		m_bExitSolitaryFreePass = m_SLZDeserializer.GetBool();
		bool bool3 = m_SLZDeserializer.GetBool();
		bool bool4 = m_SLZDeserializer.GetBool();
		m_bItemInUse = m_SLZDeserializer.GetBool();
		bool bool5 = m_SLZDeserializer.GetBool();
		bool bool6 = m_SLZDeserializer.GetBool();
		bool bool7 = m_SLZDeserializer.GetBool();
		bool bool8 = m_SLZDeserializer.GetBool();
		bool bool9 = m_SLZDeserializer.GetBool();
		m_bIsRobinsonCharacter = m_SLZDeserializer.GetBool();
		if (bool5)
		{
			int num3 = -1;
			num3 = (int)bitReader.ReadUInt32(12);
			if (m_AccessKey == null && num3 != -1 && num3 != 0)
			{
				m_AccessKey = T17NetView.Find<Item>(num3);
			}
		}
		if (bool6)
		{
			int num4 = 0;
			num4 = (int)bitReader.ReadUInt32(6);
			if (m_AccessKeySubCode != num4)
			{
				SetAccessKeyCode(num4);
			}
		}
		if (bool7)
		{
			num = (int)bitReader.ReadUInt32(12);
		}
		if (bool8)
		{
			num2 = (int)bitReader.ReadUInt32(12);
		}
		if (bool9)
		{
			m_fGetToRoutineTimer = bitReader.ReadUInt32(8);
		}
		Character characterResponsible = null;
		if (@bool && num > 0 && !_m_bIsKnockedOut)
		{
			characterResponsible = T17NetView.Find<Character>(num);
		}
		if (GetIsKnockedOut() != @bool)
		{
			SetIsKnockedOut(@bool, characterResponsible);
		}
		Character characterResponsible2 = null;
		if (bool2 && num2 > 0 && !_m_bIsBound)
		{
			characterResponsible2 = T17NetView.Find<Character>(num2);
		}
		SetIsBound(bool2, null, characterResponsible2);
		m_bIsGamerControlled = bool3;
		PlayerControlledSet(bool3);
		if (bool4 != GetIsDisabled())
		{
			SetIsDisabled(bool4);
		}
	}

	private void SerializeAnimation(BitStreamWriter bitWriter, bool bWriteFacingDirection)
	{
		bool flag = m_InteractingObject != null && m_InteractingObject.GetInteractionClassType() == InteractiveObject.InteractionType.AnimatedInteractiveObject;
		bool flag2 = false;
		if (!flag && m_CurrentSerializedAnimatedInteraction != null)
		{
			flag = true;
			flag2 = true;
		}
		if (flag && m_CurrentSerializedAnimatedInteraction == null)
		{
			m_CurrentSerializedAnimatedInteraction = (AnimatedInteraction)m_InteractingObject;
		}
		short num = -1;
		int bits = 0;
		int bits2 = 0;
		if (flag)
		{
			num = (short)m_CurrentSerializedAnimatedInteraction.m_NetObjectLock.m_NetView.viewID;
			bits = m_CurrentSerializedAnimatedInteraction.GetLocalInteractionId();
			bits2 = m_CurrentSerializedAnimatedInteraction.CurrentAnimState;
		}
		bitWriter.Write((uint)m_CharacterAnimator.GetAnimState(), 8);
		bitWriter.Write((uint)m_CharacterAnimator.GetCombatState(), 2);
		bitWriter.Write(m_CharacterAnimator.ControllingNormalizedTime);
		if (m_CharacterAnimator.ControllingNormalizedTime)
		{
			bitWriter.Write(m_CharacterAnimator.AnimatorNormalizedTime);
		}
		bool lockedOn = m_CharacterAnimator.GetLockedOn();
		bitWriter.Write(lockedOn);
		if (lockedOn || bWriteFacingDirection)
		{
			bitWriter.Write((byte)((int)m_CharacterAnimator.GetDirectionx4() >> 1), 2);
		}
		bitWriter.Write(flag);
		if (flag)
		{
			bitWriter.Write((uint)num, 12);
			bitWriter.Write((uint)bits, 16);
			bitWriter.Write((uint)bits2, 4);
		}
		bitWriter.Write(m_bHasTray);
		bitWriter.Write(m_CharacterAnimator.IsCharging());
		if (flag2)
		{
			m_CurrentSerializedAnimatedInteraction = null;
		}
	}

	private void DeserializeAnimation(BitStreamReader bitReader, bool bReadFacingDirection)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		short num3 = -1;
		int interactionID = 0;
		int num4 = 0;
		float num5 = -1f;
		bool flag2 = false;
		num = bitReader.ReadByte(8);
		if (m_CharacterAnimator.GetAnimState() != (AnimState)num && num >= 0 && num < m_CharacterAnimator.m_AnimStateData.animStateData.Length)
		{
			m_CharacterAnimator.NetStateChanged((AnimState)num);
		}
		num2 = bitReader.ReadByte(2);
		m_CharacterAnimator.CombatStateChanged((CombatState)num2);
		if (bitReader.ReadBit())
		{
			num5 = bitReader.ReadFloat32();
			if (m_CharacterAnimator != null)
			{
				if (!m_CharacterAnimator.ControllingNormalizedTime)
				{
					m_CharacterAnimator.BeginControllingNormalizedTime();
				}
				m_CharacterAnimator.SetNormaisedTime(num5);
			}
		}
		else if (m_CharacterAnimator.ControllingNormalizedTime)
		{
			m_CharacterAnimator.FinishControllingNormalizedTime();
		}
		bool flag3 = bitReader.ReadBit();
		m_CharacterAnimator.CombatLockedState(flag3);
		if (flag3 || bReadFacingDirection)
		{
			m_bCalcFacingDirection = false;
			byte headAndBodyDirection = (byte)(bitReader.ReadByte(2) << 1);
			SetFaceDirection((Directionx4)headAndBodyDirection);
		}
		else
		{
			m_bCalcFacingDirection = true;
		}
		flag = bitReader.ReadBit();
		if (flag)
		{
			num3 = (short)bitReader.ReadUInt16(12);
			interactionID = bitReader.ReadUInt16(16);
			num4 = bitReader.ReadUInt16(4);
		}
		if (flag)
		{
			if (num3 != -1 && (m_CurrentDeserializedAnimatedInteraction == null || m_CurrentDeserializedAnimatedInteraction.m_NetObjectLock.m_NetView.viewID != num3))
			{
				NetObjectLock netObjectLock = T17NetView.Find<NetObjectLock>(num3);
				if (netObjectLock != null)
				{
					m_CurrentDeserializedAnimatedInteraction = (AnimatedInteraction)netObjectLock.GetInteractiveObject(interactionID);
				}
			}
			if (m_CurrentDeserializedAnimatedInteraction != null && m_CurrentDeserializedAnimatedInteraction.CurrentAnimState != num4)
			{
				m_CurrentDeserializedAnimatedInteraction.SetInteractionObjectAnimatorState(num4);
				m_LastAppliedDeserialiseAnimatedInteractionState = num4;
			}
		}
		else if (m_LastAppliedDeserialiseAnimatedInteractionState != 0 && m_CurrentDeserializedAnimatedInteraction != null)
		{
			m_CurrentDeserializedAnimatedInteraction.SetInteractionObjectAnimatorState(0);
			m_LastAppliedDeserialiseAnimatedInteractionState = 0;
		}
		bool hasTray = bitReader.ReadBit();
		SetHasTray(hasTray);
		m_CharacterAnimator.SetIsCharging(bitReader.ReadBit());
	}

	private void SerializePosition(BitStreamWriter bitWriter, out bool bWriteFacingDirection)
	{
		Vector2 vector = m_CachedCurrentPosition;
		if (WriteThing(ref m_SLZ_floorIndex, CurrentFloor.m_FloorIndex, bitWriter))
		{
			bitWriter.Write((uint)CurrentFloor.m_FloorIndex, 5);
		}
		float magnitude = m_RigidBody.velocity.magnitude;
		bWriteFacingDirection = magnitude <= 0.01f;
		bitWriter.Write(bWriteFacingDirection);
		if (bWriteFacingDirection)
		{
			bitWriter.Write(CompressFloat(vector.x), 16);
			bitWriter.Write(CompressFloat(vector.y), 16);
		}
		else
		{
			bitWriter.Write(CompressFloat(magnitude), 16);
			bitWriter.Write(CompressFloat(vector.x), 16);
			bitWriter.Write(CompressFloat(vector.y), 16);
		}
		if (WriteThing(ref m_SLZ_AnimatedZ, m_AnimatedInteractionLocalZ, bitWriter))
		{
			bitWriter.Write(CompressFloat(m_AnimatedInteractionLocalZ), 16);
		}
		bitWriter.Write(m_bInstantPositionUpdate);
		m_bInstantPositionUpdate = false;
	}

	private void DeserializePosition(BitStreamReader bitReader, float fSentTime, out bool bReadFacingDirection)
	{
		int num = -1;
		bool flag = false;
		num = ((!bitReader.ReadBit()) ? CurrentFloor.m_FloorIndex : ((int)bitReader.ReadUInt32(5)));
		if (m_fLastNetworkLocalTime != UpdateManager.smoothTime)
		{
			m_vNetworkedPositionPrevious = m_vNetworkedPosition;
			m_fLastSentPacketTime = m_fCurrentSentPacketTime;
		}
		PeerLatency.GetPeerLatency(m_NetView.ownerId, ref m_fLastMessageLatency);
		if (float.IsNaN(m_fLastMessageLatency) || float.IsInfinity(m_fLastMessageLatency))
		{
			m_fLastMessageLatency = 0f;
		}
		m_vNetworkedPositionPreviousLocal = m_Transform.position;
		bReadFacingDirection = bitReader.ReadBit();
		if (bReadFacingDirection)
		{
			m_vNetworkedVelocity = 0f;
			m_vNetworkedPosition.x = UnCompressFloat(bitReader.ReadUInt32(16));
			m_vNetworkedPosition.y = UnCompressFloat(bitReader.ReadUInt32(16));
		}
		else
		{
			m_vNetworkedVelocity = UnCompressFloat(bitReader.ReadUInt32(16));
			m_vNetworkedPosition.x = UnCompressFloat(bitReader.ReadUInt32(16));
			m_vNetworkedPosition.y = UnCompressFloat(bitReader.ReadUInt32(16));
		}
		m_fCurrentSentPacketTime = fSentTime;
		m_fLastNetworkLocalTime = UpdateManager.smoothTime;
		m_fLastNetworkFixedLocalTime = UpdateManager.fixedTime;
		if (bitReader.ReadBit())
		{
			m_AnimatedInteractionLocalZ = UnCompressFloat(bitReader.ReadUInt32(16));
		}
		flag = bitReader.ReadBit();
		FloorManager instance = FloorManager.GetInstance();
		if (m_PickedUpBy == null && instance != null)
		{
			if (flag)
			{
				m_vVelocity = Vector2.zero;
				FloorManager.Floor newFloor = instance.FindFloorbyIndex(num);
				Teleport(m_vNetworkedPosition, newFloor);
			}
			else if (CurrentFloor.m_FloorIndex != num)
			{
				FloorManager.Floor floor = instance.FindFloorbyIndex(num);
				ChangeFloor(floor);
			}
		}
	}

	public string CreateSnapshot()
	{
		SaveData_Character_V1 saveData_Character_V = new SaveData_Character_V1();
		saveData_Character_V.P = m_CachedCurrentPosition;
		saveData_Character_V.IPU = m_bInstantPositionUpdate;
		if (m_CharacterAnimator != null)
		{
			saveData_Character_V.D4 = (int)m_CharacterAnimator.GetDirectionx4();
		}
		saveData_Character_V.FL = (byte)CurrentFloor.m_FloorIndex;
		saveData_Character_V.TRY = m_bHasTray;
		saveData_Character_V.STS = 0uL;
		saveData_Character_V.STS2 = 0uL;
		if (m_CharacterStats != null)
		{
			m_CharacterStats.Serialize(ref saveData_Character_V.STS, ref saveData_Character_V.STS2);
		}
		int kO = -1;
		if (_m_bIsKnockedOut)
		{
			AIEvent knockedOutAIEvent = m_CharacterEventManager.GetKnockedOutAIEvent();
			kO = ((knockedOutAIEvent == null || !(knockedOutAIEvent.m_CharacterResponsible != null)) ? m_NetView.viewID : knockedOutAIEvent.m_CharacterResponsible.m_NetView.viewID);
		}
		saveData_Character_V.KO = kO;
		int bND = -1;
		float bTM = 0f;
		if (_m_bIsBound)
		{
			AIEvent boundAIEvent = m_CharacterEventManager.GetBoundAIEvent();
			if (boundAIEvent != null && boundAIEvent.m_CharacterResponsible != null)
			{
				bND = boundAIEvent.m_CharacterResponsible.m_NetView.viewID;
				bTM = m_fBoundEscapeTime;
			}
		}
		saveData_Character_V.BND = bND;
		saveData_Character_V.BTM = bTM;
		if (m_InteractingObject != null && m_CharacterStats != null && m_CharacterStats.m_bIsPlayer && m_InteractingObject.SerialiseInteractionForLoad())
		{
			saveData_Character_V.VID = (short)m_InteractingObject.m_NetObjectLock.m_NetView.viewID;
			saveData_Character_V.LISP = m_InteractingObject.GetInteractionStartPosition();
			saveData_Character_V.LII = m_InteractingObject.GetLocalInteractionId();
		}
		saveData_Character_V.OD = _m_bIsStandingOnDesk;
		saveData_Character_V.ACK = 0uL;
		if (m_AccessKey != null)
		{
			int uValue = 0;
			KeyFunctionality keyFunctionality = (KeyFunctionality)m_AccessKey.HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (keyFunctionality != null)
			{
				uValue = keyFunctionality.SubCode;
			}
			BitField bitField = new BitField();
			bitField.Set(12, (uint)m_AccessKey.m_NetView.viewID);
			bitField.Set(6, (uint)uValue);
			saveData_Character_V.ACK = (ulong)bitField;
		}
		saveData_Character_V.SFP = m_bExitSolitaryFreePass;
		saveData_Character_V.RTLR = m_bRoutineTargetLocationReached;
		saveData_Character_V.GTRT = m_fGetToRoutineTimer;
		saveData_Character_V.EXTRA = GenerateAdditionalSavePayload();
		return JsonUtility.ToJson(saveData_Character_V);
	}

	public void ReportKnockedOutRPC(Character attacker)
	{
		if (m_NetView != null)
		{
			int num = ((!(attacker == null)) ? attacker.m_NetView.viewID : (-1));
			m_NetView.PostLevelLoadRPC("RPC_OnCharacterKnockedOut", NetTargets.All, num);
		}
	}

	[PunRPC]
	public void RPC_OnCharacterKnockedOut(int attackerNetViewID, PhotonMessageInfo info)
	{
		if (OnCharacterKnockedOut != null)
		{
			Character otherCharacter = null;
			if (attackerNetViewID != -1)
			{
				otherCharacter = T17NetView.Find<Character>(attackerNetViewID);
			}
			OnCharacterKnockedOut(this, otherCharacter);
		}
	}

	public void StartedFromSnapshot()
	{
		RestoreSnapshot();
	}

	private void RestoreSnapshot()
	{
		if (m_SaveData == null || string.IsNullOrEmpty(m_SaveData.GetSaveData()))
		{
			return;
		}
		PrisonSnapshotIO.SnapshotData_Base snapshotData_Base = null;
		try
		{
			snapshotData_Base = JsonUtility.FromJson<PrisonSnapshotIO.SnapshotData_Base>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (snapshotData_Base == null || snapshotData_Base.m_Version != 1)
		{
			return;
		}
		SaveData_Character_V1 saveData_Character_V = null;
		try
		{
			saveData_Character_V = JsonUtility.FromJson<SaveData_Character_V1>(m_SaveData.GetSaveData());
		}
		catch
		{
		}
		if (saveData_Character_V == null)
		{
			return;
		}
		SetFaceDirection((Directionx4)saveData_Character_V.D4);
		SetHasTray(saveData_Character_V.TRY);
		if (m_CharacterStats != null)
		{
			m_CharacterStats.Deserialize(saveData_Character_V.STS, saveData_Character_V.STS2);
		}
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorbyIndex(saveData_Character_V.FL);
		Teleport(saveData_Character_V.P, floor);
		m_CurrentLocation = RoomManager.GetInstance().LookUpRoom(saveData_Character_V.P, floor);
		if (saveData_Character_V.BND > 0)
		{
			Character characterResponsible = T17NetView.Find<Character>(saveData_Character_V.BND);
			m_fBoundEscapeTime = saveData_Character_V.BTM;
			SetIsBound(isBound: true, null, characterResponsible);
		}
		if (saveData_Character_V.KO > 0)
		{
			if (saveData_Character_V.KO == m_NetView.viewID)
			{
				SetIsKnockedOut(knockedOut: true, null);
			}
			else
			{
				Character characterResponsible2 = T17NetView.Find<Character>(saveData_Character_V.KO);
				SetIsKnockedOut(knockedOut: true, characterResponsible2);
			}
		}
		if (m_CharacterStats != null && m_CharacterStats.m_bIsPlayer)
		{
			m_PendingInteractingObjectNetID = saveData_Character_V.VID;
			m_PendingInteractionStartPosition = saveData_Character_V.LISP;
			m_PendingInteractingID = saveData_Character_V.LII;
		}
		_m_bIsStandingOnDesk = false;
		if (saveData_Character_V.ACK != 0)
		{
			BitField bitField = new BitField(saveData_Character_V.ACK);
			int uInt = (int)bitField.GetUInt(12);
			int uInt2 = (int)bitField.GetUInt(6);
			if (uInt != -1)
			{
				m_AccessKey = T17NetView.Find<Item>(uInt);
				if (!IsPlayer())
				{
				}
			}
			SetAccessKeyCode(uInt2);
		}
		m_bExitSolitaryFreePass = saveData_Character_V.SFP;
		m_bRoutineTargetLocationReached = saveData_Character_V.RTLR;
		m_fGetToRoutineTimer = saveData_Character_V.GTRT;
		RestoreAdditionalSavePayload(saveData_Character_V.EXTRA);
		m_bSnapshotIsBeingRestored = true;
	}

	protected virtual string GenerateAdditionalSavePayload()
	{
		return null;
	}

	protected virtual void RestoreAdditionalSavePayload(string payload)
	{
	}

	public virtual bool ShouldBoundCameraDoShakes()
	{
		return false;
	}

	public DeskInteraction GetMyDesk()
	{
		DeskInteraction result = null;
		if (m_MyCell != null)
		{
			RoomBlob_Cell roomBlobData = m_MyCell.GetRoomBlobData<RoomBlob_Cell>();
			if (roomBlobData != null)
			{
				InteractiveObject cellObject = roomBlobData.GetCellObject(typeof(DeskInteraction), this);
				if (cellObject != null)
				{
					result = cellObject as DeskInteraction;
				}
			}
		}
		return result;
	}

	public void SetPinImage(Sprite newImage, PinManager.Pin.PinFilterType filter, SpriteAnimation animation = null, bool edgeable = false, bool floorTrackable = false)
	{
		if ((newImage == null && animation == null) || filter == PinManager.Pin.PinFilterType.Count)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < m_RequiredPins.Count; i++)
		{
			if (filter == m_RequiredPins[i].m_FilterType)
			{
				m_RequiredPins[i] = new CharacterPinData(filter, newImage, animation, edgeable, floorTrackable);
				num = i;
				break;
			}
		}
		if (num < 0)
		{
			m_RequiredPins.Add(new CharacterPinData(filter, newImage, animation, edgeable, floorTrackable));
			num = m_RequiredPins.Count - 1;
		}
		if (filter >= m_ActivePin.m_FilterType)
		{
			PinManager instance = PinManager.GetInstance();
			if (instance != null)
			{
				instance.UpdatePinIconSprite(m_PinID, newImage, filter, animation, edgeable, floorTrackable);
				m_ActivePin = m_RequiredPins[num];
			}
		}
	}

	public void ResetPinImage(PinManager.Pin.PinFilterType filter)
	{
		for (int i = 0; i < m_RequiredPins.Count; i++)
		{
			if (m_RequiredPins[i].m_FilterType == filter)
			{
				m_RequiredPins.Remove(m_RequiredPins[i]);
			}
		}
		if (m_RequiredPins.Count > 0)
		{
			CharacterPinData activePin = new CharacterPinData(PinManager.Pin.PinFilterType.All, null, null, edgeable: false, floorTrackable: false);
			for (int j = 0; j < m_RequiredPins.Count; j++)
			{
				if (activePin.m_FilterType <= m_RequiredPins[j].m_FilterType)
				{
					activePin = m_RequiredPins[j];
				}
			}
			PinManager instance = PinManager.GetInstance();
			if (instance != null)
			{
				instance.UpdatePinIconSprite(m_PinID, activePin.m_Sprite, activePin.m_FilterType, activePin.m_Animation, activePin.m_Edgable, activePin.m_FloorTrackable);
				m_ActivePin = activePin;
			}
		}
		else
		{
			PinManager instance2 = PinManager.GetInstance();
			if (instance2 != null)
			{
				instance2.UpdatePinIconSprite(m_PinID, m_MapIcon, PinManager.Pin.PinFilterType.Characters);
				m_ActivePin = new CharacterPinData(PinManager.Pin.PinFilterType.All, null, null, edgeable: false, floorTrackable: false);
			}
		}
	}

	public void HideNPCPin()
	{
		if (m_PinID != -1)
		{
			PinManager instance = PinManager.GetInstance();
			if (instance != null)
			{
				instance.RemovePin(m_PinID);
			}
		}
		m_PinID = -1;
	}

	public virtual void ShowNPCPin()
	{
		if (m_MapIcon != null && m_PinID == -1)
		{
			m_ActivePin.m_FilterType = PinManager.Pin.PinFilterType.Characters;
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = true;
			GameObject target = base.gameObject;
			Sprite mapIcon = m_MapIcon;
			bool bUpdatePosition = true;
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorbyIndex(1);
			PinManager.Pin.PinFilterType filterType = m_ActivePin.m_FilterType;
			m_PinID = instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition, floor, null, filterType, edgable: false, floorTrackable: false, directional: false, m_CharacterCustomisation.m_DisplayName, localiseToolTipTag: false);
		}
	}

	public void OnBecameMasterClient()
	{
		if (!m_bIsBeingDestroyed)
		{
			Character character = (Character)GetPickedUpBy();
			if (null != character && character.m_CharacterRole == CharacterRole.Medic)
			{
				SetDropped(m_CachedCurrentPosition - CarryInteraction.m_vCarryOffset);
			}
			PrepareAICharacter();
			RetriggerEvents();
		}
	}

	public void PrepareAICharacter()
	{
		if (!(m_CharacterStats != null) || !m_CharacterStats.m_bIsPlayer)
		{
			ResetChargeAttack();
			if (m_CharacterAnimator != null)
			{
				m_CharacterAnimator.ResetAnims();
			}
			SetHasTray(m_bHasTray, null, force: true);
			SetFaceDirection(m_x4FacingDirection);
			CombatState combatState = CombatState.UnarmedCombat;
			if (m_EquippedItem != null && m_EquippedItem.CombatData != null)
			{
				combatState = m_EquippedItem.CombatData.m_CombatAnimation;
			}
			if (m_CharacterAnimator != null)
			{
				m_CharacterAnimator.CombatStateChanged(combatState);
			}
			if (m_CharacterStats != null)
			{
				m_CharacterStats.SetCharacterState(StatModifierEnum.None);
			}
			m_bIsHidden = false;
		}
	}

	private void RetriggerEvents()
	{
		if (T17NetManager.IsMasterClient && m_CharacterEventManager != null)
		{
			AIEvent knockedOutAIEvent = m_CharacterEventManager.GetKnockedOutAIEvent();
			_m_bIsKnockedOut = !_m_bIsKnockedOut;
			SetIsKnockedOut(!m_bIsKnockedOut, knockedOutAIEvent?.m_CharacterResponsible);
			knockedOutAIEvent = m_CharacterEventManager.GetBoundAIEvent();
			_m_bIsBound = !_m_bIsBound;
			SetIsBound(!m_bIsBound, null, knockedOutAIEvent?.m_CharacterResponsible);
			_m_bIsAttacking = !_m_bIsAttacking;
			SetIsAttacking(!m_bIsAttacking);
			_m_bIsChipping = !_m_bIsChipping;
			SetIsChipping(!m_bIsChipping);
			_m_bIsCutting = !_m_bIsCutting;
			SetIsCutting(!m_bIsCutting);
			_m_bIsDigging = !_m_bIsDigging;
			SetIsDigging(!m_bIsDigging);
			_m_bIsDisguised = !_m_bIsDisguised;
			SetIsDisguised(!m_bIsDisguised);
			_m_bIsLooting = !_m_bIsLooting;
			SetIsLooting(!m_bIsLooting);
			_m_bIsNaked = !_m_bIsNaked;
			SetIsNaked(!_m_bIsNaked);
			_m_bIsSearchingDesk = !_m_bIsSearchingDesk;
			SetIsSearchingDesk(!m_bIsSearchingDesk);
			_m_bIsStandingOnDesk = !_m_bIsStandingOnDesk;
			SetIsStandingOnDesk(!m_bIsStandingOnDesk);
			_m_bIsSuspicious = !_m_bIsSuspicious;
			SetIsSuspicious(!m_bIsSuspicious);
			_m_bIsTardy = !_m_bIsTardy;
			SetIsTardy(!m_bIsTardy);
			_m_bIsWanted = !_m_bIsWanted;
			SetIsWanted(!m_bIsWanted);
			SetCarriedObject(m_CarriedObject);
			SetCarriedCharacter(m_CarriedCharacter);
			m_bHasTray = !m_bHasTray;
			SetHasTray(!m_bHasTray);
		}
	}

	public bool WriteThing<T>(ref T first, T second, BitStreamWriter bitWriter)
	{
		bool flag = m_bForceSerialize || !first.Equals(second);
		bitWriter.Write(flag);
		first = second;
		return flag;
	}

	public void Cutscenes_SetHijacked(bool isHijacked)
	{
		if (isHijacked)
		{
			m_bHijackedAnimatorActive = m_CharacterAnimator.m_CharacterAnimator.gameObject.activeSelf;
			m_CharacterAnimator.m_CharacterAnimator.gameObject.SetActive(value: false);
		}
		else
		{
			m_CharacterAnimator.m_CharacterAnimator.gameObject.SetActive(m_bHijackedAnimatorActive);
		}
		m_CharacterAnimator.Cutscenes_SetHijacked(isHijacked);
	}

	public void SetInteractingObjectRPC(InteractiveObject value)
	{
		if (!(m_InteractingObject != value))
		{
			return;
		}
		m_SerializeRateOverride = CharacterSerializer.CharacterSerializerListType.High;
		if (m_NetView != null)
		{
			int num = 0;
			int num2 = 0;
			if (value != null && value.m_NetViewID != null)
			{
				num = value.m_NetViewID.viewID;
				num2 = value.GetLocalInteractionId();
			}
			m_NetView.PostLevelLoadRPC("RPC_SetInteractiveObject", NetTargets.Others, (short)num, (short)num2);
		}
		m_RemoteInteractingObject = null;
		m_InteractingObject = value;
		if (m_NetView.isMine && this.OnInteractEvent != null)
		{
			this.OnInteractEvent(m_InteractingObject);
		}
	}

	[PunRPC]
	public void RPC_SetInteractiveObject(short netViewID, short interactionID, PhotonMessageInfo info)
	{
		SetRemoteInteractiveObject(netViewID, interactionID);
	}

	public void ClearRemoteInterativeObject()
	{
		m_RemoteInteractingObject = null;
	}

	public void SetRemoteInteractiveObject(short netViewID, short interactionID)
	{
		m_InteractingObject = null;
		if (netViewID == 0)
		{
			m_RemoteInteractingObject = null;
			return;
		}
		m_RemoteInteractingObject = null;
		NetObjectLock netObjectLock = T17NetView.Find<NetObjectLock>(netViewID);
		if (netObjectLock != null)
		{
			m_RemoteInteractingObject = netObjectLock.GetInteractiveObject(interactionID);
		}
	}

	public void SetInteractingObject_Local(InteractiveObject value)
	{
		if (m_InteractingObject != value)
		{
			m_InteractingObject = value;
		}
	}

	public InteractiveObject GetInteractiveObject()
	{
		return m_InteractingObject;
	}

	public InteractiveObject GetRemoteInteractiveObject()
	{
		return m_RemoteInteractingObject;
	}

	public int GetCharacterSerializerIndex()
	{
		return m_CharacterSerializeIndex;
	}

	public void SetCharacterSerializerIndex(int index)
	{
		m_CharacterSerializeIndex = index;
	}

	public void ReceivedGift(Character gifter, int[] itemDataIDs, int money)
	{
		if (this.ReceivedGiftEvent != null)
		{
			this.ReceivedGiftEvent(gifter, itemDataIDs, money);
		}
	}

	public void ControlledLateUpdate()
	{
		numUpdatedLocks = 0;
		if (m_CarriedCharacter != null)
		{
			m_CarriedCharacter.UpdatePickedUp();
		}
		if (null != m_CarriedObject)
		{
			m_CarriedObject.UpdateCarriedPosition();
		}
	}

	public void ControlledPreFixedUpdate()
	{
		m_bCurrentPositionDirty = true;
	}

	public void ControlledPreUpdate()
	{
		m_bCurrentPositionDirty = true;
		if (m_NetView.isMine)
		{
			UpdateCurrentLocationWithValidation(performPositionChecks: true);
		}
		m_PreviousPosition = m_CachedCurrentPosition;
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return true;
	}

	public bool RequiresControlledLateUpdate()
	{
		return true;
	}

	public bool RequiresControlledPreUpdate()
	{
		return true;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return true;
	}

	private void UpdateCurrentLocationWithValidation(bool performPositionChecks)
	{
		if (RoomManager.GetInstance() != null)
		{
			bool flag = true;
			RoomBlob currentLocation = m_CurrentLocation;
			RoomBlob roomBlob = RoomManager.GetInstance().LookUpRoom(m_CachedCurrentPosition, CurrentFloor);
			if (roomBlob != currentLocation)
			{
				flag &= ChangeRoomCheck(currentLocation, roomBlob);
				if (flag)
				{
					m_CurrentLocation = roomBlob;
				}
				else if (!IsPlayer())
				{
				}
			}
			if (performPositionChecks)
			{
				if (m_CurrentLocation != null && m_CurrentLocation.location == RoomBlob.eLocation.BuildingBoundary)
				{
					flag &= BuildingBoundaryCheck();
				}
				if (!flag)
				{
					m_CachedCurrentPosition = m_PreviousPosition;
					m_Transform.position = m_CachedCurrentPosition;
					m_CharacterMovement.Immobile();
				}
			}
		}
		m_LastLocationUpdateFrameCount = UpdateManager.frameCount;
	}

	private bool ChangeRoomCheck(RoomBlob prevLocation, RoomBlob newLocation)
	{
		bool result = true;
		if (!CurrentFloor.IsTheGroundFloor() && newLocation == null && prevLocation.location == RoomBlob.eLocation.BuildingBoundary)
		{
			result = false;
		}
		else if (null != m_CarriedObject && prevLocation != null && m_InteractingObject != null && m_InteractingObject.GetInteractionClassType() == InteractiveObject.InteractionType.PortableInteractiveObject)
		{
			result = false;
		}
		if (IsInteractionRequestInFlight)
		{
			result = false;
		}
		return result;
	}

	protected virtual bool BuildingBoundaryCheck()
	{
		return true;
	}

	private void UpdateProximityLayer()
	{
		if (m_TrackableElementReporter != null)
		{
			ProximityPriorityLayers layer = m_ConsciousProximityLayer;
			if (m_bIsKnockedOut)
			{
				layer = m_UnconsciousProximityLayer;
			}
			m_TrackableElementReporter.SetProximityPriority(layer, m_ProximityPenalty);
		}
	}

	public Directionx4 GetFacingDirectionEnum()
	{
		return m_x4FacingDirection;
	}

	public Vector3 GetCachedCurrentPosition()
	{
		return m_CachedCurrentPosition;
	}

	public float GetCharacterID()
	{
		return m_CharacterID;
	}

	public int GetFloorIndex()
	{
		return CurrentFloor.m_FloorIndex;
	}

	public NetObjectLock GetNetObjectLock()
	{
		return m_NetObjectLock;
	}

	public int GetNetworkLastReadFrameCount()
	{
		return m_LastReadFrameCount;
	}

	public int GetLastLocationUpdateFrameCount()
	{
		return m_LastLocationUpdateFrameCount;
	}

	public virtual bool GetPermissionToForceAnimatorUpdateOnEnable()
	{
		return true;
	}

	public bool IsPendingPurpleLockProcess()
	{
		return m_bPurpleDoorLocksChanged;
	}

	public unsafe static uint CompressFloat(float val)
	{
		if (Mathf.Approximately(val, 0f))
		{
			return 0u;
		}
		uint num = 0u;
		uint num2 = *(uint*)(&val);
		return (ushort)(((num2 >> 16) & 0x8000u) | (((num2 & 0x7F800000) - 939524096 >> 13) & 0x7C00u) | ((num2 >> 13) & 0x3FFu));
	}

	public unsafe static float UnCompressFloat(uint val)
	{
		if (val == 0)
		{
			return 0f;
		}
		float num = 0f;
		uint num2 = ((val & 0x8000) << 16) | ((val & 0x7C00) + 114688 << 13) | ((val & 0x3FF) << 13);
		return *(float*)(&num2);
	}
}
