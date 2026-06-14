using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AUTOGEN_T17Wwise_Enums;
using Rewired;
using UnityEngine;

public class Player : Character
{
	public delegate bool DeliverItem(Player player, Character tryDeliverTo, bool onlyCheck);

	private struct PlayerTransitionData
	{
		public int m_TransitionFloorID;

		public bool m_bTransitionDown;

		public float m_TransitionOffset;

		public float m_TransitionOffsetX;

		public Hole m_HoleInTransRange;

		public VentCover m_VentInTransRange;

		public StaticLadder m_StaticLadderInTransRange;
	}

	public enum PlayerInputs
	{
		Interact = 1,
		StopInteract = 2,
		UseItem = 4,
		PickUpItem = 8,
		PickUpTileCover = 16,
		Transition = 32,
		Attack = 64,
		Block = 128,
		TargetCharacter = 256,
		ShowSelfMenu = 512,
		ShowMap = 1024,
		UNUSED_01 = 2048,
		ShowNamePlates = 4096,
		CloseContainer = 8192,
		DropItemHUD = 16384,
		TagTile = 32768,
		Emote = 65536,
		MAX = 65537
	}

	public enum PTE_State
	{
		Idle,
		Waiting,
		Updated
	}

	[Serializable]
	public class PlayerFightSaveData
	{
		public List<int> m_KnockedOutInmateViewIDs = new List<int>();
	}

	[Serializable]
	private class SaveData_Player_AdditionalPayload_V1 : PrisonSnapshotIO.SnapshotData_Base
	{
		public int CON_ST_M;
	}

	[Header("Player Data")]
	public int m_PlayerNumber;

	public Gamer m_Gamer;

	public CameraManager.PlayerBindingID m_PlayerCameraManagerBindingID;

	public int m_SpawnIndex = -1;

	public float m_PressAndHoldDownTime = 1f;

	public float m_DropItemHoldTime = 0.5f;

	private float m_TimeDropButtonHeld;

	public Platform.RumbleController m_GettingHitRumble = new Platform.RumbleController();

	public Platform.RumbleController m_HittingRumble = new Platform.RumbleController();

	public Platform.RumbleController m_BlockingRumble = new Platform.RumbleController();

	public Platform.LightBarEffect m_GettingHitLight = new Platform.LightBarEffect();

	private T17_ABPath.ArrowPathCallback m_RoutinePathFinishedCallback;

	private T17_ABPath.ArrowPathCallback m_ObjectivePathFinishedCallback;

	private bool m_bHaveNamePlate;

	private static WaitForSeconds m_WaitForHideIcon = new WaitForSeconds(0.33f);

	public float m_IncidentalDisplayHealthTime = 3f;

	private bool m_bBrowsingHUDMenu;

	private bool m_bBrowsingMainMap;

	private bool m_bBrowsingSmallMenu;

	private bool m_bBrowsingPauseMenu;

	private PerPlayerTrackedUIElements m_MyTrackedUIElements;

	private T17TrackedUIElement m_MyTrackedElement;

	private T17TrackedUIElement m_CharacterTargetElement;

	private const int MAX_PROX_CHARACTERS = 4;

	private const int MAX_PROX_INTERACTS = 2;

	private const int MAX_PROX_SORTED = 1;

	private const int MAX_PROX_HOLES = 2;

	private const int MAX_PROX_TILES = 4;

	public const int MAX_PROX_COUNT = 7;

	private TrackableUIElementsReporter[] m_CurrentListOfPrioritySortedTrackedElements = new TrackableUIElementsReporter[1];

	private List<NetObjectLock> m_CurrentNearbyInteractiveObjects = new List<NetObjectLock>();

	private List<TrackableUIElementsReporter> m_CurrentNearbyCharacters = new List<TrackableUIElementsReporter>();

	private List<TrackableUIElementsReporter> m_TrackedElementReporters = new List<TrackableUIElementsReporter>();

	private List<TrackableUIElementsReporter> m_RemovedReporters = new List<TrackableUIElementsReporter>();

	private List<Item> m_CurrentNearbyItems = new List<Item>();

	private List<TrackableUIElementsReporter> m_FarAwayReporters = new List<TrackableUIElementsReporter>();

	private NetObjectLock m_NearestInteractiveObject;

	private Item m_NearestItem;

	private TrackableUIElementsReporter m_NearestCharacter;

	private DamagableTile m_NearestDamagableTile;

	private NetObjectLock m_MouseOverInteractiveObject;

	private Item m_MouseOverItem;

	private TrackableUIElementsReporter m_MouseOverCharacter;

	private float broadPassMaxMouseDist = float.PositiveInfinity;

	private bool m_bIsInMinigame;

	private bool m_bMapRequested;

	private bool m_bInventoryRequested;

	private Mouse m_PlayerMouse;

	private bool m_bNeedsToUpdateCharacterTarget;

	private Hole[] m_ListOfCurrentProximityHoles = new Hole[2];

	private Hole[] m_ListOfCurrentProximityHolesAbove = new Hole[2];

	private DamagableTile[] m_ListOfCurrentProximityTiles = new DamagableTile[4];

	private T17TrackedUIElement[] m_OldProximityElements = new T17TrackedUIElement[7];

	private Dictionary<TrackableUIElementsReporter, float> m_ActiveReportersWithElementsInProximity = new Dictionary<TrackableUIElementsReporter, float>();

	public float m_TimeUntilNameplatesFade = 2f;

	public float m_NameplatesFadeTime = 0.5f;

	private float m_ReusableFloat;

	private float m_ProximityCheckTime = 0.2f;

	private float m_ElapsedProximityTime;

	private bool m_bEnabledWorldNamePlates;

	private bool m_bAllUITrackersWereDisabled;

	private bool m_bAttackCharging;

	private bool m_bCharacterTargettingEnabled;

	private FastList<Character> m_RecentlyHitCharacters = new FastList<Character>();

	private static List<Character> m_AllCharacters = null;

	private float m_LastTargetChangeTime;

	public float m_TargetChangeInterval = 0.2f;

	private List<Character> m_TargetCharacterList = new List<Character>();

	private List<Character> m_PrevTargetsList = new List<Character>();

	public float m_TargetOffscreenLength = 2f;

	private float m_TargetOffscreenTimer = 1f;

	private const float HELD_BUTTON_TIME = 1.5f;

	private PlayerInventoryHUD m_PlayerInventoryHUD;

	private float m_PrimaryTimePressed;

	private bool m_bPrimaryProcessed;

	private float m_SecondaryTimePressed;

	private bool m_bSecondaryProcessed;

	private float m_TertiaryTimePressed;

	private bool m_bTertiaryProcessed;

	private const float PLAYER_TIME_PRESSED_EPS = 0.001f;

	private bool m_bInteractionActionBlocked;

	private float m_StopInteractionTimePressed;

	private bool m_bStopInteractionProcessed;

	private bool m_bDropitemProcessed;

	private bool m_bMapCloseProcessed;

	private bool m_bSmallMenuCloseProcessed;

	private float m_StartTimeWhenHeatIsZero = -1f;

	private bool m_bSentHeatStat;

	private int m_NumberShowTimesMissed;

	private const int STAGE_FRIGHT_ACHIEVEMENT_QUOTA = 3;

	private int m_ActiveQuests;

	public DeliverItem TryDeliverItem;

	private int m_ObjectiveArrowID = -1;

	private Vector3 m_ObjectiveDestinationVec = Vector3.zero;

	private T17NetView m_ObjectiveDestinationNetView;

	private Character m_ObjectiveCharacter;

	private bool m_bShowObjectiveTargetIndicator;

	protected int m_RoutineArrowID = -1;

	private RoomBlob m_RoutineDestinationRoom;

	private T17NetView m_RoutineDestinationNetView;

	private Vector3 m_RoutineDestinationVec = Vector3.zero;

	public Sprite m_HomeIcon;

	private int m_HomePinID = -1;

	public Sprite m_PlayerArrowIcon;

	private int m_PlayerArrowPinID = -1;

	public MapItemTracker m_MapItemTracker;

	private long m_HintBitfield;

	private int m_CellDoorCode = -1;

	private PlayerTransitionData m_TransitionData;

	private PlayerTransitionData m_PreviousTransitionData;

	private bool m_bDisplayTutorials = true;

	private static List<Player> m_AllPlayers = new List<Player>();

	private static Vector3 m_HidePlayerPosition = new Vector2(-500f, -500f);

	private static readonly Vector3 m_EffectOffsetLanded = new Vector3(0f, 0f, -0.1f);

	private int m_PreviousTileRow = -1;

	private int m_PreviousTileColumn = -1;

	public string m_PlayerJoinedLocalization = "Text.System.PlayerJoined";

	public string m_PlayerLeftLocalization = "Text.System.PlayerLeft";

	public string m_PlayerNameToken = "$PlayerName";

	public PlayerPathing m_PlayerPathing;

	private float m_PreviousEndUseTime;

	private bool m_bWasUsingItem;

	private T17TrackedUIElement m_HideCharacterIconCoroutineElement;

	private TrackableUIElementsReporter m_HideCharacterIconCoroutineReporter;

	private float m_fMinimumInputThreshold = 0.1f;

	private bool m_bUseKeyIsDown;

	private bool m_bCloseInventoryOnPauseMenuHide;

	private int m_PlayerInputEnabledMask = -1;

	private ItemData m_AllowedUsableItem;

	public PTE_State m_PTE_State;

	private List<string> m_SwallowedInputActions = new List<string>();

	public PlayerFightSaveData m_PlayerFightSaveData;

	private bool m_bCheckPrimaryInteractionFromInventoryClose;

	private bool _m_bCollisionDisabled_Debug;

	private bool _m_bCollisionDisabled_Interacting;

	private bool _m_bCollisionDisabled_Carried;

	private bool _m_bCollisionDisabled_IsRemote;

	private float m_TimeWhenStartedSolitaryConfinement = -1f;

	private bool m_SolitaryActive;

	public bool IsBrowsingHudMenu => m_bBrowsingHUDMenu;

	public bool IsBrowsingMainMap
	{
		get
		{
			return m_bBrowsingMainMap;
		}
		set
		{
			m_bBrowsingMainMap = value;
			if (!m_bBrowsingMainMap)
			{
				m_bMapCloseProcessed = true;
			}
		}
	}

	public bool IsBrowsingSmallMenu
	{
		get
		{
			return m_bBrowsingSmallMenu;
		}
		set
		{
			m_bBrowsingSmallMenu = value;
			if (!m_bBrowsingSmallMenu)
			{
				m_bSmallMenuCloseProcessed = true;
			}
		}
	}

	public bool IsBrowsingPauseMenu => m_bBrowsingPauseMenu;

	public int ActiveQuests => m_ActiveQuests;

	public int ObjectiveArrowID => m_ObjectiveArrowID;

	public int ObjectiveArrowTargetNetViewID => (!(m_ObjectiveDestinationNetView == null)) ? m_ObjectiveDestinationNetView.viewID : (-1);

	public bool bDisplayTutorials
	{
		get
		{
			return m_bDisplayTutorials;
		}
		set
		{
			m_bDisplayTutorials = value;
		}
	}

	public static Vector3 HidePlayerPosition => m_HidePlayerPosition;

	public bool m_bCollisionDisabled_Debug
	{
		get
		{
			return _m_bCollisionDisabled_Debug;
		}
		set
		{
			_m_bCollisionDisabled_Debug = value;
			Update_CollisionEnabledState();
		}
	}

	protected bool m_bCollisionDisabled_Interacting
	{
		get
		{
			return _m_bCollisionDisabled_Interacting;
		}
		set
		{
			_m_bCollisionDisabled_Interacting = value;
			Update_CollisionEnabledState();
		}
	}

	protected bool m_bCollisionDisabled_Carried
	{
		get
		{
			return _m_bCollisionDisabled_Carried;
		}
		set
		{
			_m_bCollisionDisabled_Carried = value;
			Update_CollisionEnabledState();
		}
	}

	protected bool m_bCollisionDisabled_IsRemote
	{
		get
		{
			return _m_bCollisionDisabled_IsRemote;
		}
		set
		{
			_m_bCollisionDisabled_IsRemote = value;
			Update_CollisionEnabledState();
		}
	}

	private void Update_CollisionEnabledState()
	{
		if (!(m_PhysicsCollider == null))
		{
			bool active = !(m_bCollisionDisabled_Debug || m_bCollisionDisabled_Interacting || m_bCollisionDisabled_Carried || m_bCollisionDisabled_IsRemote);
			m_PhysicsCollider.SetActive(active);
		}
	}

	public void Cleanup()
	{
		if (m_AllCharacters != null)
		{
			m_AllCharacters.Clear();
			m_AllCharacters = null;
		}
		m_AllPlayers.Clear();
	}

	protected override void Awake()
	{
		m_RoutinePathFinishedCallback = RoutinePathFinished;
		m_ObjectivePathFinishedCallback = ObjectivePathFinished;
		base.Awake();
		m_PlayerCameraManagerBindingID = CameraManager.PlayerBindingID.CM_PBID_UNSET;
		m_AllPlayers.Add(this);
		if (m_ItemContainer != null)
		{
			ItemContainer itemContainer = m_ItemContainer;
			itemContainer.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Combine(itemContainer.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnItemAdded));
		}
	}

	protected override void Start()
	{
		base.Start();
		Gamer.OnDeleteImminent += RemoveGamer;
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		m_CharacterStats.StartInit();
		m_CharacterEventManager.StartInit();
		m_MapItemTracker.StartInit();
		m_CharacterCustomisation.StartInit();
		T17BehaviourManager.INITSTATE result = base.StartInit();
		m_ItemContainer.StartInit();
		if (m_Gamer != null && m_Gamer.IsLocal() && m_Gamer.m_bPrimaryLocal && null != PlayerDataManager.GetInstance() && PlayerDataManager.GetInstance().GetPrimaryPlayerFightData() != null)
		{
			m_PlayerFightSaveData = new PlayerFightSaveData();
			m_PlayerFightSaveData.m_KnockedOutInmateViewIDs = new List<int>(PlayerDataManager.GetInstance().GetPrimaryPlayerFightData());
		}
		return result;
	}

	public override void Init()
	{
		base.Init();
		if (m_Gamer == null)
		{
			m_Gamer = Gamer.GetGamerByViewID(m_NetView.viewID);
		}
		if (null == m_NetView)
		{
		}
		PlayerDataManager.PlayerSpecificInfo playerSpecificStuff = PlayerDataManager.GetInstance().GetPlayerSpecificStuff(m_PlayerNumber);
		if (playerSpecificStuff != null)
		{
			m_PlayerArrowIcon = playerSpecificStuff.mapIcon;
			m_HomeIcon = playerSpecificStuff.homeIcon;
		}
		if (m_Gamer != null)
		{
			if (!m_Gamer.IsLocal() || T17NetManager.IsMasterClient || m_Gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.CharacterGranted)
			{
				SetGamer(m_Gamer);
			}
		}
		else
		{
			RoutineManager.GetInstance().OnRoutineChanged -= RoutineChanged;
			SetIsDisabled(bDisabled: true);
		}
		if (playerSpecificStuff != null)
		{
			Platform instance = Platform.GetInstance();
			if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && instance != null)
			{
				instance.SetLightBarData(m_Gamer.m_RewiredPlayer.id, playerSpecificStuff.colour);
			}
		}
		PlayerDataManager.GetInstance().RegisterPlayer(this);
		if (T17NetRoomManager.Instance != null)
		{
			T17NetRoomManager instance2 = T17NetRoomManager.Instance;
			instance2.OnRoomTypeChanged = (T17NetRoomManager.RoomTypeChanged)Delegate.Combine(instance2.OnRoomTypeChanged, new T17NetRoomManager.RoomTypeChanged(OnRoomTypeChange));
		}
	}

	protected override void CharacterStats_StateChangedEvent(StatModifierEnum oldState, StatModifierEnum newState)
	{
		base.CharacterStats_StateChangedEvent(oldState, newState);
		if (oldState == StatModifierEnum.SleepingInOwnBed && newState != StatModifierEnum.SleepingInOwnBed)
		{
			RoutineManager.GetInstance().OnPlayerSleepingInOwnBed(this, isSleeping: false);
		}
		else if (oldState != StatModifierEnum.SleepingInOwnBed && newState == StatModifierEnum.SleepingInOwnBed)
		{
			RoutineManager.GetInstance().OnPlayerSleepingInOwnBed(this, isSleeping: true);
		}
	}

	public override void OnDestroy()
	{
		m_AllPlayers.Remove(this);
		if (PlayerDataManager.GetInstance() != null)
		{
			PlayerDataManager.GetInstance().UnregisterPlayer(this);
		}
		if (CameraManager.GetInstance() != null)
		{
			CameraManager.GetInstance().RemoveTarget(this);
		}
		if (m_MyTrackedUIElements != null && m_MyTrackedElement != null)
		{
			m_MyTrackedUIElements.ReleaseTrackedUIElement(m_MyTrackedElement);
			m_MyTrackedElement = null;
			m_CharacterTargetElement = null;
		}
		if (GlobalHintManager.GetInstance() != null)
		{
			GlobalHintManager.GetInstance().RemovePlayerBitfield(m_Gamer);
		}
		TagManager instance = TagManager.GetInstance();
		if (instance != null && m_NetView != null)
		{
			instance.ClearDestroyedPlayer(m_NetView.viewID);
		}
		Gamer.OnDeleteImminent -= RemoveGamer;
		base.OnInteractEvent -= OnInteract_Chair;
		ItemContainer itemContainer = m_ItemContainer;
		itemContainer.OnItemAddedEvent = (ItemContainer.ItemContainerAddedHandler)Delegate.Remove(itemContainer.OnItemAddedEvent, new ItemContainer.ItemContainerAddedHandler(OnItemAdded));
		if (T17NetRoomManager.Instance != null)
		{
			T17NetRoomManager instance2 = T17NetRoomManager.Instance;
			instance2.OnRoomTypeChanged = (T17NetRoomManager.RoomTypeChanged)Delegate.Remove(instance2.OnRoomTypeChanged, new T17NetRoomManager.RoomTypeChanged(OnRoomTypeChange));
		}
		m_CurrentNearbyInteractiveObjects.Clear();
		m_CurrentNearbyCharacters.Clear();
		m_TrackedElementReporters.Clear();
		m_RemovedReporters.Clear();
		m_CurrentNearbyItems.Clear();
		m_NearestCharacter = null;
		m_NearestDamagableTile = null;
		m_NearestItem = null;
		m_NearestInteractiveObject = null;
		m_ActiveReportersWithElementsInProximity.Clear();
		m_RecentlyHitCharacters.Clear();
		m_TargetCharacterList.Clear();
		m_PlayerInventoryHUD = null;
		TryDeliverItem = null;
		m_ObjectiveDestinationNetView = null;
		m_RoutineDestinationRoom = null;
		m_RoutineDestinationNetView = null;
		m_MapItemTracker = null;
		m_RoutinePathFinishedCallback = null;
		m_ObjectivePathFinishedCallback = null;
		m_HideCharacterIconCoroutineElement = null;
		m_HideCharacterIconCoroutineReporter = null;
		m_AllowedUsableItem = null;
		if (m_ObjectiveCharacter != null)
		{
			m_ObjectiveCharacter.OnFloorChangedEvent -= HandleObjectiveArrow;
			m_ObjectiveCharacter = null;
		}
		base.OnDestroy();
	}

	public void ResetPlayer()
	{
		if (GetIsKnockedOut())
		{
			RegainConsciousness();
		}
		if (base.m_bIsBound)
		{
			EscapeBindings();
		}
		m_bCollisionDisabled_IsRemote = true;
		ForceStopInteraction();
		SetIsAttacking(attacking: false);
		SetIsChipping(value: false);
		SetIsCutting(value: false);
		SetIsDigging(value: false);
		SetIsLooting(value: false);
		SetIsNaughtyLocation(value: false);
		SetIsNaked(value: false);
		SetIsSuspicious(value: false);
		SetIsWanted(value: false);
		SetHasTray(hasTray: false);
		SetIsSearchingDesk(value: false);
		SetIsStandingOnDesk(value: false);
		SetIsTardy(value: false);
		SetIsMissing(value: false);
		SetCarriedObject(null);
		SetCarriedCharacter(null);
		m_bIsHidden = false;
		m_RemoteInteractingObject = null;
		m_CurrentSerializedAnimatedInteraction = null;
		m_CurrentDeserializedAnimatedInteraction = null;
		if (m_CharacterAnimator != null)
		{
			m_CharacterAnimator.ResetAnims();
		}
	}

	public void RemoveGamer(Gamer gamer)
	{
		if (gamer == null || m_Gamer != gamer)
		{
			return;
		}
		CullingObjectCollector instance = CullingObjectCollector.GetInstance();
		if (instance != null)
		{
			instance.HideAllMode(bHide: false, m_PlayerCameraManagerBindingID);
		}
		InGameMenuFlow instance2 = InGameMenuFlow.Instance;
		if (instance2 != null)
		{
			instance2.HideMenu(this, m_PlayerCameraManagerBindingID);
		}
		m_StartTimeWhenHeatIsZero = -1f;
		m_bSentHeatStat = false;
		m_bIsGamerControlled = false;
		if (m_IconHandler != null)
		{
			m_IconHandler.RemoveIcon(CharacterIconHandler.IconType.InMenus);
		}
		HandleGamerOwnershipOfRoomBlob(GetMyCell(), isNowMine: false);
		ChatFeedManager instance3 = ChatFeedManager.GetInstance();
		if (instance3 != null)
		{
			instance3.SendSystemMessageLocalized_RPC(m_PlayerLeftLocalization, m_PlayerNameToken, m_Gamer.m_GamerName, ChatFeedManager.MessageTag.System);
		}
		m_MapItemTracker.OnGamerDisconnected();
		base.OnInteractEvent -= OnInteract_Chair;
		if (null != RoutineManager.GetInstance())
		{
			RoutineManager.GetInstance().OnRoutineChanged -= RoutineChanged;
		}
		PinManager instance4 = PinManager.GetInstance();
		if (instance4 != null)
		{
			if (m_PinID != -1)
			{
				instance4.RemovePin(m_PinID);
				m_PinID = -1;
			}
			if (m_PlayerArrowPinID != -1)
			{
				instance4.RemovePin(m_PlayerArrowPinID);
				m_PlayerArrowPinID = -1;
			}
			if (m_HomePinID != -1)
			{
				instance4.RemovePin(m_HomePinID);
				m_HomePinID = -1;
			}
		}
		if (TagManager.GetInstance() != null)
		{
			TagManager.GetInstance().OnGamerDisconnected(this);
		}
		ObjectiveManager instance5 = ObjectiveManager.GetInstance();
		if (instance5 != null)
		{
			instance5.CleanupPrisonObjectives(this);
		}
		if (T17NetManager.IsMasterClient)
		{
			m_ItemContainer.ReleaseLock();
			if (m_MC_NetObjectLockID != -1)
			{
				NetObjectLock netObjectLock = T17NetView.Find<NetObjectLock>(m_MC_NetObjectLockID);
				if (netObjectLock != null)
				{
					netObjectLock.ReleaseLock();
				}
			}
			NetObjectLock component = base.gameObject.GetComponent<NetObjectLock>();
			component.KickInteractingCharacter(m_NetView.viewID);
			JobsManager.GetInstance().RemoveCharacterFromJob(this);
			SetBusyRPC(busy: false);
			SetSwagBagForPlayer();
			if (m_CarriedCharacter != null)
			{
				m_CarriedCharacter.SetDropped(m_Transform.position);
			}
			if (m_RemoteInteractingObject != null)
			{
				m_RemoteInteractingObject.ForceStopInteraction(this);
				if (m_RemoteInteractingObject != null && m_RemoteInteractingObject.m_NetObjectLock != null)
				{
					m_RemoteInteractingObject.m_NetObjectLock.ReleaseLock();
				}
			}
		}
		ResetPlayer();
		EffectManager.PlayEffect(EffectManager.effectType.PlayerLeaveDust, base.transform.position);
		Teleport(m_HidePlayerPosition);
		SetIsDisabled(bDisabled: true);
		if (m_Gamer.IsLocal())
		{
			ClearOutPlayer();
		}
		m_Gamer = null;
	}

	private void ClearOutPlayer()
	{
		HUDMenuFlow.Instance.DestoryPlayerHUD(m_PlayerCameraManagerBindingID);
		InGameMenuFlow.Instance.DestroyPlayerIGM(m_PlayerCameraManagerBindingID);
		if (CameraManager.GetInstance() != null)
		{
			CameraManager.GetInstance().RemoveTarget(m_PlayerCameraManagerBindingID, m_HidePlayerPosition);
		}
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			GlobalStart.GetInstance().SkipFrames(2);
			T17EventSystem.ApplyCategories(m_Gamer.m_RewiredPlayer, T17EventSystem.InputCateogryStates.Assignment);
			m_bDisplayTutorials = true;
			TutorialManager.GetInstance().ClearSaveData(m_SpawnIndex);
			GlobalHintManager.GetInstance().RemovePlayerBitfield(m_Gamer);
		}
		if (m_MyTrackedUIElements != null && m_MyTrackedElement != null)
		{
			m_MyTrackedUIElements.ReleaseTrackedUIElement(m_MyTrackedElement, doLayerMaskChanges: false);
			m_MyTrackedElement = null;
		}
		int num = 0;
		for (int i = 0; i < m_CurrentListOfPrioritySortedTrackedElements.Length; i++)
		{
			if (m_CurrentListOfPrioritySortedTrackedElements[i] != null)
			{
				m_OldProximityElements[num] = m_CurrentListOfPrioritySortedTrackedElements[i].GetUITrackedElement(m_PlayerCameraManagerBindingID);
				if (m_OldProximityElements[num] != null)
				{
					m_MyTrackedUIElements.ReleaseTrackedUIElementWithoutDisable(m_OldProximityElements[num]);
					num++;
				}
			}
		}
		m_OldProximityElements = new T17TrackedUIElement[7];
		if (m_HideCharacterIconCoroutineReporter != null && m_HideCharacterIconCoroutineReporter.CharacterOwner != null)
		{
			m_HideCharacterIconCoroutineReporter.CharacterOwner.m_IconHandler.HideCharacterIcon(hide: false);
		}
	}

	public override void PlayerControlledSet(bool bValue)
	{
		if (bValue && m_Gamer != null && m_Gamer.IsLocal() && m_NetView != null && !m_NetView.isMine && m_Gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.CharacterGranted)
		{
			m_NetView.TransferOwnership(T17NetManager.PhotonPlayerID);
			m_Gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.CharacterOwned;
		}
	}

	public override void SetIsDisabled(bool bDisabled)
	{
		base.SetIsDisabled(bDisabled);
		PinManager instance = PinManager.GetInstance();
		if (instance != null)
		{
			ProcessPins();
			instance.ReassignPins();
		}
		if (!bDisabled)
		{
			if (GetOutFit() == null)
			{
				SetIsNaked(value: true);
			}
			OnItemsChanged();
		}
	}

	public void EnableInLevel()
	{
		if (!m_Gamer.IsLocal() || m_Gamer.m_eCharacterSelectionStage != Gamer.CharacterSelectionStage.InGame || !m_NetView.isMine)
		{
			return;
		}
		m_Gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.EnabledInGame;
		SetIsDisabled(bDisabled: false);
		m_bCollisionDisabled_IsRemote = false;
		RoomBlob myCell = GetMyCell();
		if (!(myCell != null))
		{
			return;
		}
		RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
		if (!(roomBlobData != null))
		{
			return;
		}
		SpawnPoint spawnPointForCharacter = roomBlobData.GetSpawnPointForCharacter(this);
		if (spawnPointForCharacter != null && (!PrisonSnapshotIO.IsThereSaveData() || !m_Gamer.m_bPrimaryLocal))
		{
			Teleport(spawnPointForCharacter.transform.position);
			if (m_PlayerCameraManagerBindingID != 0)
			{
				CameraManager.GetInstance().RecalculateCameraIndexOfBlurEffect(m_PlayerCameraManagerBindingID);
			}
			ForceStopInteraction();
			if (spawnPointForCharacter.m_AttachedBed != null)
			{
				spawnPointForCharacter.m_AttachedBed.m_bOnLevelEnteredInteraction = true;
				spawnPointForCharacter.m_AttachedBed.Interact(this);
			}
		}
	}

	public void SetGamer(Gamer gamer)
	{
		m_Gamer = gamer;
		if (gamer.m_eCharacterSelectionStage != Gamer.CharacterSelectionStage.EnabledInGame)
		{
			SetIsDisabled(bDisabled: true);
			m_bCollisionDisabled_IsRemote = !m_Gamer.IsLocal();
		}
		PlayerDataManager.PlayerSpecificInfo playerSpecificStuff = PlayerDataManager.GetInstance().GetPlayerSpecificStuff(m_PlayerNumber);
		Platform instance = Platform.GetInstance();
		if (playerSpecificStuff != null && instance != null && m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			instance.SetLightBarData(m_Gamer.m_RewiredPlayer.id, playerSpecificStuff.colour);
		}
		if (gamer != null)
		{
			gamer.UpdateGamer(gamer.m_iControllerIndex, gamer.m_PhotonID, gamer.m_NetViewID, null, this, bPrimarySet: false, bPrimary: false, gamer.m_PlatformUniqueID);
			HandleGamerOwnershipOfRoomBlob(GetMyCell(), isNowMine: true);
		}
		if (m_Gamer.IsLocal())
		{
			m_NetView.TransferOwnership(T17NetManager.PhotonPlayerID);
			m_Gamer.m_eCharacterSelectionStage = Gamer.CharacterSelectionStage.CharacterOwned;
		}
		if (T17NetManager.IsMasterClient && m_CharacterStats != null && !PrisonSnapshotIO.IsThereSaveData())
		{
			m_CharacterStats.RestoreHealthRPC();
			m_CharacterStats.RestoreEnergyRPC();
			m_CharacterStats.RestoreHeatRPC();
		}
		RoutineManager instance2 = RoutineManager.GetInstance();
		if (instance2 != null)
		{
			instance2.OnRoutineChanged -= RoutineChanged;
			instance2.OnRoutineChanged += RoutineChanged;
		}
		JobsManager instance3 = JobsManager.GetInstance();
		if (instance3 != null)
		{
			instance3.OnJobLost = (JobsManager.JobEvent)Delegate.Remove(instance3.OnJobLost, new JobsManager.JobEvent(JobLost));
			instance3.OnJobLost = (JobsManager.JobEvent)Delegate.Combine(instance3.OnJobLost, new JobsManager.JobEvent(JobLost));
		}
		GlobalHintManager instance4 = GlobalHintManager.GetInstance();
		if (instance4 != null && m_Gamer != null && m_Gamer.IsLocal() && instance4.CreateNewPlayerBitfield(m_Gamer) && !instance4.GetHintBitfield(m_Gamer, LevelScript.GetCurrentLevelInfo().m_PrisonEnum, out m_HintBitfield))
		{
			m_HintBitfield = 0L;
		}
		TutorialManager instance5 = TutorialManager.GetInstance();
		if (instance5 != null && gamer.IsLocal())
		{
			instance5.AddPlayerSave(this);
			if (instance5.CheckTutorialNeeded(this, TutorialSubject.JobOffice))
			{
				base.OnInteractEvent -= OnInteract_Chair;
				base.OnInteractEvent += OnInteract_Chair;
			}
		}
		ProcessHomePin();
		PinManager instance6 = PinManager.GetInstance();
		if (instance6 != null)
		{
			ProcessPins();
			instance6.ReassignPins();
		}
		if (m_Gamer != null)
		{
			m_bIsGamerControlled = true;
		}
		else
		{
			m_bIsGamerControlled = false;
		}
		ChatFeedManager instance7 = ChatFeedManager.GetInstance();
		if (instance7 != null && m_Gamer.IsLocal())
		{
			instance7.SendSystemMessageLocalized_RPC(m_PlayerJoinedLocalization, m_PlayerNameToken, m_Gamer.m_GamerName, ChatFeedManager.MessageTag.System, bMasterClientOnly: false);
			instance7.ShowOnlineModeMessage(T17NetRoomManager.CurrentGameRoomType, bDisplayToAllPlayers: false, m_Gamer);
		}
		ObjectiveManager instance8 = ObjectiveManager.GetInstance();
		if (instance8 != null)
		{
			instance8.AssignPrisonObjectives(this);
			instance8.ShowCurrentTrackingObjective(this);
			if (!m_Gamer.m_bPrimaryLocal)
			{
				instance8.RemoveAllTrees(this);
			}
			else if (T17NetManager.IsMasterClient)
			{
				for (int i = 0; i < m_AllPlayers.Count; i++)
				{
					if (m_AllPlayers[i] != null && m_AllPlayers[i] != this)
					{
						instance8.RemoveAllTrees(m_AllPlayers[i]);
					}
				}
			}
		}
		if (m_Gamer != null)
		{
			if (m_PendingInteractingObjectNetID > 0)
			{
				NetObjectLock netObjectLock = T17NetView.Find<NetObjectLock>(m_PendingInteractingObjectNetID);
				if (netObjectLock != null)
				{
					InteractiveObject interactiveObject = netObjectLock.GetInteractiveObject(m_PendingInteractingID);
					if (interactiveObject != null)
					{
						base.transform.position = m_PendingInteractionStartPosition;
						interactiveObject.Interact(this);
					}
				}
			}
			m_PendingInteractingID = -1;
			m_PendingInteractingObjectNetID = -1;
		}
		m_bCloseInventoryOnPauseMenuHide = false;
	}

	public void SetSpawnIndex(int number)
	{
		m_PlayerNumber = number;
		m_SpawnIndex = number;
	}

	private void SetMenuAndHUDItems()
	{
		if (HUDMenuFlow.Instance != null && m_MyTrackedElement == null && m_TrackableElementReporter != null)
		{
			m_MyTrackedUIElements = HUDMenuFlow.Instance.GetPlayerTrackedUIElements(m_PlayerCameraManagerBindingID);
			if (m_MyTrackedUIElements != null)
			{
				m_MyTrackedUIElements.AttachFirstUnusedElementToReporer(m_TrackableElementReporter, -1, isElementFarAway: false, attemptToFindHistoricallyAssignedElement: false);
				m_MyTrackedElement = m_TrackableElementReporter.GetUITrackedElement(m_PlayerCameraManagerBindingID);
				m_MyTrackedUIElements.SetBaseElementDepth((float)base.CurrentFloor.m_zPos + HUDMenuFlow.WorldOffsetZ);
			}
		}
		if (HUDMenuFlow.Instance != null)
		{
			m_PlayerInventoryHUD = HUDMenuFlow.Instance.GetPlayerInventoryHUD(m_PlayerCameraManagerBindingID);
		}
	}

	public void SetUpGamer(Gamer gamer)
	{
		if (gamer == null)
		{
			return;
		}
		if (gamer.IsLocal())
		{
			m_PlayerCameraManagerBindingID = InGameMenuFlow.Instance.GetCameraIndexForGamer(gamer);
			if (m_PlayerCameraManagerBindingID != 0)
			{
				CameraManager.GetInstance().SetTarget(CameraManager.GetInstance().m_PlayerSelectCameraPosition, m_PlayerCameraManagerBindingID);
			}
			SetMenuAndHUDItems();
			if (!gamer.m_bPrimaryLocal || !T17NetManager.IsMasterClient || !PrisonSnapshotIO.IsThereSaveData())
			{
				SolitaryManager.GetInstance().SetWantedForSolitary(this, sendToSolitary: false);
				if (RoutineManager.GetInstance() != null)
				{
					SetupTargetRoom(RoutineManager.GetInstance().GetCurrentRoutineBaseType());
				}
				ResetGetToRoutineTimer(bForce: true);
			}
		}
		else if (T17NetManager.IsMasterClient)
		{
			SolitaryManager.GetInstance().SetWantedForSolitary(this, sendToSolitary: false);
		}
	}

	public static List<Player> GetAllPlayers()
	{
		return m_AllPlayers;
	}

	public void InitHUD(int ownerID)
	{
		if (ownerID == T17NetManager.PhotonPlayerID && HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.OpenPlayerHUD(this, m_PlayerCameraManagerBindingID);
		}
	}

	public void ProcessHomePin()
	{
		if (m_HomeIcon == null)
		{
			return;
		}
		BedInteraction bedInteraction = null;
		RoomBlob myCell = GetMyCell();
		if (myCell != null)
		{
			RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
			if (roomBlobData != null)
			{
				SpawnPoint spawnPointForCharacter = roomBlobData.GetSpawnPointForCharacter(this);
				if (spawnPointForCharacter != null && spawnPointForCharacter.m_AttachedBed != null)
				{
					bedInteraction = spawnPointForCharacter.m_AttachedBed;
				}
			}
		}
		if (bedInteraction == null)
		{
			return;
		}
		if (m_HomePinID != -1)
		{
			PinManager.GetInstance().RemovePin(m_HomePinID);
			m_HomePinID = -1;
		}
		if (m_Gamer != null)
		{
			Localization.GetWithKeySwap("Text.Map.CharacterHome", out var localised, "$NAME", m_CharacterCustomisation.m_DisplayName);
			Player[] array = null;
			if (ConfigManager.GetInstance() != null && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
			{
				array = new Player[1] { this };
			}
			FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(bedInteraction.m_PinLocation.transform.position.z);
			PinManager instance = PinManager.GetInstance();
			bool bForMainMap = true;
			bool bForMiniMap = true;
			GameObject pinLocation = bedInteraction.m_PinLocation;
			Sprite homeIcon = m_HomeIcon;
			bool bUpdatePosition = false;
			FloorManager.Floor floor2 = floor;
			Player[] players = array;
			PinManager.Pin.PinFilterType filterType = PinManager.Pin.PinFilterType.All;
			bool edgable = true;
			bool floorTrackable = true;
			string toolTipTag = localised;
			m_HomePinID = instance.CreatePin(bForMainMap, bForMiniMap, pinLocation, homeIcon, bUpdatePosition, floor2, players, filterType, edgable, floorTrackable, directional: false, toolTipTag, localiseToolTipTag: false);
		}
	}

	public override void ProcessPins()
	{
		if (!(m_MapIcon != null) || GetIsDisabled())
		{
			return;
		}
		bool flag = m_PinID == -1;
		bool flag2 = m_PlayerArrowPinID == -1;
		PinManager instance = PinManager.GetInstance();
		FloorManager instance2 = FloorManager.GetInstance();
		ConfigManager instance3 = ConfigManager.GetInstance();
		if (instance3 != null && instance != null && instance2 != null && m_CharacterCustomisation != null && m_Gamer != null && instance3.gameType == PrisonConfig.ConfigType.Versus)
		{
			if (flag2)
			{
				bool bForMainMap = true;
				bool bForMiniMap = true;
				GameObject target = base.gameObject;
				Sprite playerArrowIcon = m_PlayerArrowIcon;
				bool bUpdatePosition = true;
				FloorManager.Floor floor = instance2.FindFloorbyIndex(1);
				PinManager.Pin.PinFilterType filterType = PinManager.Pin.PinFilterType.Characters;
				bool edgable = true;
				bool floorTrackable = true;
				bool directional = true;
				string displayName = m_CharacterCustomisation.m_DisplayName;
				bool localiseToolTipTag = false;
				Player[] players = new Player[1] { this };
				m_PlayerArrowPinID = instance.CreatePin(bForMainMap, bForMiniMap, target, playerArrowIcon, bUpdatePosition, floor, players, filterType, edgable, floorTrackable, directional, displayName, localiseToolTipTag, bOverrideIconScale: false, default(Vector3), null, default(Vector3), isPlayer: true);
			}
			if (flag)
			{
				bool localiseToolTipTag = true;
				bool directional = true;
				GameObject target = base.gameObject;
				Sprite playerArrowIcon = m_MapIcon;
				bool floorTrackable = true;
				FloorManager.Floor floor = instance2.FindFloorbyIndex(1);
				PinManager.Pin.PinFilterType filterType = PinManager.Pin.PinFilterType.Characters;
				bool edgable = false;
				bool bUpdatePosition = false;
				bool bForMiniMap = false;
				string displayName = m_CharacterCustomisation.m_DisplayName;
				bool bForMainMap = false;
				int netViewID = m_Gamer.m_NetViewID;
				m_PinID = instance.CreatePin(localiseToolTipTag, directional, target, playerArrowIcon, floorTrackable, floor, null, filterType, edgable, bUpdatePosition, bForMiniMap, displayName, bForMainMap, bOverrideIconScale: false, default(Vector3), null, default(Vector3), isPlayer: true, netViewID);
			}
		}
		else if (flag2)
		{
			m_PlayerArrowPinID = instance.CreatePin(bForMainMap: true, bForMiniMap: true, base.gameObject, m_PlayerArrowIcon, bUpdatePosition: true, instance2.FindFloorbyIndex(1), null, PinManager.Pin.PinFilterType.Characters, edgable: true, floorTrackable: true, directional: true, m_CharacterCustomisation.m_DisplayName, localiseToolTipTag: false, bOverrideIconScale: false, default(Vector3), null, default(Vector3), isPlayer: true);
		}
	}

	public override void OnDisplayNameChanged(string newName)
	{
		base.OnDisplayNameChanged(newName);
		if (m_MyTrackedElement != null)
		{
			m_MyTrackedElement.SetNamePlateText(newName);
			if (m_bHaveNamePlate)
			{
				m_MyTrackedElement.EnableNamePlate();
			}
			else
			{
				m_MyTrackedElement.DisableNamePlate();
			}
		}
		PinManager instance = PinManager.GetInstance();
		if (instance != null)
		{
			Localization.GetWithKeySwap("Text.Map.CharacterHome", out var localised, "$NAME", m_CharacterCustomisation.m_DisplayName);
			instance.UpdatePinTooltipTag(m_HomePinID, localised, localise: false);
			instance.UpdatePinTooltipTag(m_PlayerArrowPinID, m_CharacterCustomisation.m_DisplayName, localise: false);
		}
	}

	public override bool GetIsImmobilised()
	{
		return base.GetIsImmobilised() || (m_Gamer != null && T17DialogBoxManager.HasDialogsForGamer(m_Gamer)) || IsBrowsingPauseMenu;
	}

	public bool GetCombatReady()
	{
		return base.m_bIsKnockedOut || base.m_bIsBound || m_bStartingToClimb || (m_EquippedItem != null && m_EquippedItem.IsImmobilisingOwner()) || (m_Gamer != null && T17DialogBoxManager.HasDialogsForGamer(m_Gamer)) || IsBrowsingPauseMenu;
	}

	protected override void OwnerUpdate()
	{
		Gamer gamerByViewID = Gamer.GetGamerByViewID(m_NetView.viewID);
		if (!T17NetManager.IsConnectedToGameServerAndReady || gamerByViewID == null)
		{
			return;
		}
		base.OwnerUpdate();
		UpdateEquipedItemTargetting();
		UpdateMyTrackedElement();
		UpdateHoleTransitions();
		bool bFullUpdate = m_PTE_State != PTE_State.Idle;
		if (UpdateProximityTrackedElements(bFullUpdate))
		{
			m_PTE_State = PTE_State.Updated;
		}
		if (m_NetView.isMine)
		{
			if (base.m_RoutineTargetLocation != null && base.m_RoutineTargetLocation.location == RoomBlob.eLocation.JobOffice && m_RoutineArrowID != -1 && !AICharacter_JobOfficer.CharacterOnTime())
			{
				CancelRoutineArrow();
				m_CharacterStats.IncreaseHeat(ConfigManager.GetInstance().jobConfig.m_MissedJobOfficerHeatIncrease);
				base.m_RoutineTargetLocation = null;
				m_bRoutineTargetLocationReached = true;
			}
			if (m_CharacterStats.Heat <= 0f && !m_bSentHeatStat)
			{
				if (m_StartTimeWhenHeatIsZero < 0f)
				{
					m_StartTimeWhenHeatIsZero = RoutineManager.GetInstance().GetElapsedSeconds();
				}
				else
				{
					float num = RoutineManager.GetInstance().GetElapsedSeconds() - m_StartTimeWhenHeatIsZero;
					num /= 3600f;
					if (num >= 72f)
					{
						m_bSentHeatStat = true;
						StatSystem.GetInstance().IncStat(15, 3f, m_Gamer, string.Empty);
					}
				}
			}
			else
			{
				m_StartTimeWhenHeatIsZero = -1f;
			}
		}
		ProcessInput();
		if (FloorManager.GetInstance() != null)
		{
			FloorManager.GetInstance().GetTileGridPoint(base.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.m_CachedCurrentPosition, out m_PreviousTileRow, out m_PreviousTileColumn);
		}
	}

	protected override void OwnerFixedUpdate()
	{
		Gamer gamerByViewID = Gamer.GetGamerByViewID(m_NetView.viewID);
		if (!T17NetManager.IsConnectedToGameServerAndReady || gamerByViewID == null)
		{
			return;
		}
		bool flag = IsCorrectGamer();
		Rewired.Player player = ((m_Gamer == null) ? null : gamerByViewID.m_RewiredPlayer);
		if (flag && player != null)
		{
			m_WalkVector.x = player.GetAxis("Move_Horizontal");
			m_WalkVector.y = player.GetAxis("Move_Vertical");
			if (player.GetButton("Walk") && m_WalkVector.sqrMagnitude > m_fMinimumInputThreshold)
			{
				Walk(m_WalkVector, CharacterSpeed.Walk);
			}
			else
			{
				Walk(m_WalkVector);
			}
		}
		base.OwnerFixedUpdate();
		if (!flag)
		{
			return;
		}
		if (m_bCharacterTargettingEnabled)
		{
			Vector3 cachedCurrentPosition = base.m_CachedCurrentPosition;
			Vector3 vector = Vector3.zero;
			bool flag2 = false;
			bool flag3 = UpdateManager.time > m_LastTargetChangeTime + m_TargetChangeInterval;
			bool flag4 = false;
			if (base.m_CharacterTarget != null && player != null && flag3 && m_PlayerMouse == null)
			{
				if (player.GetButton("CombatTarget_Next"))
				{
					flag4 = true;
				}
				float axis = player.GetAxis("CombatTarget_Horizontal");
				float axis2 = player.GetAxis("CombatTarget_Vertical");
				if (Mathf.Abs(axis) + Mathf.Abs(axis2) > 0.1f)
				{
					flag2 = true;
					vector = new Vector3(axis, axis2, 0f);
					vector.Normalize();
					vector *= 10f;
					m_LastTargetChangeTime = Time.time;
				}
				else
				{
					if (!flag4)
					{
						return;
					}
					Vector2 facingDirection = GetFacingDirection();
					vector = new Vector3(facingDirection.x, facingDirection.y, 0f);
					vector.Normalize();
					vector *= 10f;
					m_LastTargetChangeTime = Time.time;
				}
			}
			m_bNeedsToUpdateCharacterTarget = false;
			m_TargetCharacterList.Clear();
			Camera camera = CameraManager.GetInstance().GetCamera(m_PlayerCameraManagerBindingID);
			int count = m_AllCharacters.Count;
			Character character = null;
			for (int i = 0; i < count; i++)
			{
				character = m_AllCharacters[i];
				if (!(character == null) && !character.GetIsDisabled() && !character.m_bIsKnockedOut && !character.m_bIsBound && (character.m_CharacterRole == CharacterRole.Guard || character.m_CharacterRole == CharacterRole.Inmate) && !(character == this) && character.CurrentFloor == base.CurrentFloor)
				{
					Vector3 vector2 = camera.WorldToViewportPoint(character.m_CachedCurrentPosition);
					if (vector2.x >= 0f && vector2.x <= 1f && vector2.y >= 0f && vector2.y <= 1f && vector2.z > 0f)
					{
						m_TargetCharacterList.Add(character);
					}
				}
			}
			if (m_TargetCharacterList.Count == 0)
			{
				DisableTargetElement();
				return;
			}
			for (int num = m_PrevTargetsList.Count - 1; num >= 0; num--)
			{
				bool flag5 = false;
				for (int j = 0; j < m_TargetCharacterList.Count; j++)
				{
					if (m_PrevTargetsList[num].GetCharacterID() == m_TargetCharacterList[j].GetCharacterID())
					{
						flag5 = true;
						break;
					}
				}
				if (!flag5)
				{
					m_PrevTargetsList.RemoveAt(num);
				}
			}
			if (flag2 || flag4 || base.m_CharacterTarget == null || m_PlayerMouse != null)
			{
				Character character2 = null;
				float num2 = float.MaxValue;
				if (m_PrevTargetsList.Count == m_TargetCharacterList.Count)
				{
					m_PrevTargetsList.Clear();
				}
				if (m_PlayerMouse != null)
				{
					TrackableUIElementsReporter character3 = new TrackableUIElementsReporter();
					m_MouseDetector.GetCurrentCharacter(ref character3);
					if (character3 != null)
					{
						Character characterOwner = character3.CharacterOwner;
						if (characterOwner != null && (characterOwner.m_CharacterRole == CharacterRole.Inmate || characterOwner.m_CharacterRole == CharacterRole.Guard))
						{
							character2 = character3.CharacterOwner;
						}
					}
					if (character2 == null)
					{
						Vector2 screenPosition = m_PlayerMouse.screenPosition;
						Vector2 offset = default(Vector2);
						MouseDetector.GetMouseToCameraOffset(camera, ref offset);
						screenPosition += offset;
						float num3 = broadPassMaxMouseDist;
						for (int k = 0; k < m_TargetCharacterList.Count; k++)
						{
							float sqrMagnitude = (base.m_CachedCurrentPosition - m_TargetCharacterList[k].m_CachedCurrentPosition).sqrMagnitude;
							if (sqrMagnitude < num3)
							{
								character2 = m_TargetCharacterList[k];
								num3 = sqrMagnitude;
							}
						}
					}
					m_PlayerMouse = null;
				}
				else
				{
					for (int l = 0; l < m_TargetCharacterList.Count; l++)
					{
						Vector2 p = m_TargetCharacterList[l].m_CachedCurrentPosition;
						if (flag4)
						{
							bool flag6 = false;
							for (int m = 0; m < m_PrevTargetsList.Count; m++)
							{
								if (m_PrevTargetsList[m].GetCharacterID() == m_TargetCharacterList[l].GetCharacterID())
								{
									flag6 = true;
									break;
								}
							}
							if (flag6)
							{
								continue;
							}
						}
						float num4 = ForwardDistanceToSegment(p, cachedCurrentPosition, (Vector2)cachedCurrentPosition + (Vector2)vector, flag4);
						if (num4 < num2 && m_TargetCharacterList[l] != base.m_CharacterTarget)
						{
							character2 = m_TargetCharacterList[l];
							num2 = num4;
						}
					}
				}
				if (character2 != null && base.m_CharacterTarget != character2)
				{
					if (base.m_CharacterTarget == null)
					{
						TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.Combat);
					}
					ResetCharacterTargetElement();
					m_CharacterTargetElement = m_MyTrackedUIElements.AttachFirstUnusedElementToReporer(character2.m_TrackableElementReporter, 1, isElementFarAway: false, attemptToFindHistoricallyAssignedElement: false);
					if (m_CharacterTargetElement != null)
					{
						character2.m_TrackableElementReporter.SetDisplayFlagsForElement(m_CharacterTargetElement, isFarAway: false);
						m_CharacterTargetElement.SetNameplateHighlight(highlight: true);
					}
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_LockOn, base.gameObject);
				}
				if (character2 == null)
				{
					return;
				}
				SetCharacterTarget(character2);
				if (flag4)
				{
					m_PrevTargetsList.Add(character2);
				}
			}
			m_TargetOffscreenTimer = m_TargetOffscreenLength;
		}
		else
		{
			m_PrevTargetsList.Clear();
		}
	}

	protected void LateUpdate()
	{
		m_SwallowedInputActions.Clear();
	}

	private void ResetCharacterTargetElement()
	{
		if (m_CharacterTargetElement != null)
		{
			m_CharacterTargetElement.DisableNamePlate();
			if (m_CharacterTargetElement.HasFlag(4u))
			{
				m_CharacterTargetElement.DisableIcon();
			}
			m_MyTrackedUIElements.ReleaseTrackedUIElement(m_CharacterTargetElement);
			m_CharacterTargetElement = null;
		}
	}

	public T17TrackedUIElement GetCharTargetElement()
	{
		return m_CharacterTargetElement;
	}

	public PerPlayerTrackedUIElements GetTrackedUIElements()
	{
		return m_MyTrackedUIElements;
	}

	public float ForwardDistanceToSegment(Vector2 P, Vector2 A, Vector2 B, bool bAllowTargetsBehind)
	{
		float num = 1f;
		Vector2 vector = B - A;
		Vector2 lhs = P - A;
		float num2 = Vector2.Dot(lhs, vector);
		if (num2 == 0f)
		{
			return Vector2.Distance(P, A);
		}
		if (num2 < 0f)
		{
			if (!bAllowTargetsBehind)
			{
				return float.MaxValue;
			}
			num = 5f;
		}
		float num3 = Vector2.Dot(vector, vector);
		if (num3 <= num2)
		{
			return Vector2.Distance(P, B);
		}
		float num4 = num2 / num3;
		Vector2 b = A + num4 * vector;
		return Vector2.Distance(P, b) * num;
	}

	public override void DamageCharacter(Character target, float damage, int itemViewID, bool normalDamage, GamelogicRunModes processingMode)
	{
		base.DamageCharacter(target, damage, itemViewID, normalDamage, processingMode);
		if (!m_RecentlyHitCharacters.Contains(target))
		{
			m_RecentlyHitCharacters.Add(target);
		}
		if (Platform.GetInstance() != null && m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			Platform.GetInstance().DoControllerRumble(m_HittingRumble, m_Gamer.m_RewiredPlayer.id);
		}
	}

	protected override bool TakeDamage(float damage, Character attacker, GamelogicRunModes processingMode = GamelogicRunModes.All)
	{
		bool result = base.TakeDamage(damage, attacker, processingMode);
		if (Platform.GetInstance() != null && m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
		{
			Platform.GetInstance().DoControllerRumble(m_GettingHitRumble, m_Gamer.m_RewiredPlayer.id);
			Platform.GetInstance().StartLightBarEffect(m_GettingHitLight, m_Gamer.m_RewiredPlayer.id);
		}
		return result;
	}

	public void PlayerKnockedOutInmate(int inmateViewID)
	{
		if (LevelScript.GetCurrentLevelInfo().m_PrisonType == LevelScript.PRISON_TYPE.Transport)
		{
			return;
		}
		if (m_PlayerFightSaveData == null)
		{
			m_PlayerFightSaveData = new PlayerFightSaveData();
		}
		if (!m_PlayerFightSaveData.m_KnockedOutInmateViewIDs.Contains(inmateViewID))
		{
			m_PlayerFightSaveData.m_KnockedOutInmateViewIDs.Add(inmateViewID);
			int count = m_PlayerFightSaveData.m_KnockedOutInmateViewIDs.Count;
			if (count >= Character.TOTAL_INMATE_COUNT)
			{
				StatSystem.GetInstance().IncStat(9, 1f, m_Gamer, string.Empty);
			}
		}
	}

	private void ProcessInput()
	{
		ProcessShowNamePlates();
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && !m_Gamer.m_bIsInPlayerSelectMenu && m_Gamer.m_eCharacterSelectionStage == Gamer.CharacterSelectionStage.EnabledInGame && !ProcessStopInteraction() && !ProcessInteractions() && !ProcessAttack() && !ProcessBlock() && !ProcessCharacterTargetting() && !ProcessShowMap() && !ProcessShowPauseMenu() && !ProcessShowSelfMenu() && !ProcessUseItem() && !ProcessPickUpItem() && !ProcessTransition() && !ProcessPickUpTileCover() && !ProcessCloseContainer() && !ProcessDropItemHUD() && !ProcessTagTile())
		{
		}
	}

	private bool ProcessStopInteraction()
	{
		if (!CheckInputEnabled(PlayerInputs.StopInteract))
		{
			return false;
		}
		if (InGameMenuFlow.Instance != null && InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID) && !IsBrowsingSmallMenu)
		{
			m_StopInteractionTimePressed = 0f;
			return false;
		}
		if (IsBrowsingPauseMenu)
		{
			m_bStopInteractionProcessed = true;
			m_StopInteractionTimePressed = 0f;
			return false;
		}
		if (m_bMapCloseProcessed)
		{
			if (!m_Gamer.m_RewiredPlayer.GetButton("StopInteraction"))
			{
				m_bMapCloseProcessed = false;
			}
			return false;
		}
		if (m_bSmallMenuCloseProcessed)
		{
			if (!m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
			{
				m_bSmallMenuCloseProcessed = false;
			}
			else
			{
				SetBlockInteraction();
			}
			return false;
		}
		if (!IsInteracting())
		{
			m_StopInteractionTimePressed = 0f;
			return false;
		}
		if (base.m_bIsCarryingObject)
		{
			if (!m_bStopInteractionProcessed && !m_bPrimaryProcessed)
			{
				if (m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
				{
					m_StopInteractionTimePressed += UpdateManager.deltaTime;
				}
				if (m_StopInteractionTimePressed >= 0.001f)
				{
					bool button = m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction");
					if (!button || m_StopInteractionTimePressed > 0f)
					{
						m_bStopInteractionProcessed = true;
						m_StopInteractionTimePressed = 0f;
						m_MyTrackedUIElements.HidePressAndHold();
						RequestStopInteraction();
						m_bDropitemProcessed = true;
						m_bPrimaryProcessed = true;
					}
					if (!button)
					{
						m_StopInteractionTimePressed = 0f;
						m_MyTrackedUIElements.HidePressAndHold();
					}
					return true;
				}
			}
		}
		else if (IsBrowsingSmallMenu && m_Gamer.m_RewiredPlayer.GetButtonUp("PrimaryInteraction"))
		{
			m_bStopInteractionProcessed = true;
			RequestStopInteraction();
		}
		if (!m_bStopInteractionProcessed)
		{
			if (m_Gamer.m_RewiredPlayer.GetButton("StopInteraction"))
			{
				m_StopInteractionTimePressed += UpdateManager.deltaTime;
			}
			if (m_StopInteractionTimePressed >= 0.001f)
			{
				bool button2 = m_Gamer.m_RewiredPlayer.GetButton("StopInteraction");
				if (!button2 || m_StopInteractionTimePressed > 0f)
				{
					m_bStopInteractionProcessed = true;
					m_StopInteractionTimePressed = 0f;
					m_MyTrackedUIElements.HidePressAndHold();
					RequestStopInteraction();
					m_bDropitemProcessed = true;
				}
				if (!button2)
				{
					m_StopInteractionTimePressed = 0f;
					m_MyTrackedUIElements.HidePressAndHold();
				}
				return true;
			}
		}
		else if (!m_Gamer.m_RewiredPlayer.GetButton("StopInteraction"))
		{
			m_bStopInteractionProcessed = false;
		}
		return false;
	}

	public void SetBlockInteraction()
	{
		m_bInteractionActionBlocked = true;
	}

	private bool ProcessInteractions()
	{
		if (!CheckInputEnabled(PlayerInputs.Interact))
		{
			return ReturnFalseAndCheckInteractionInputProcessed();
		}
		if (m_PlayerInventoryHUD != null && m_PlayerInventoryHUD.IsExpandedHudVisible() && m_PlayerInventoryHUD.IsPlayerInteractingWithExpandedHud())
		{
			return ReturnFalseAndCheckInteractionInputProcessed();
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return ReturnFalseAndCheckInteractionInputProcessed();
		}
		if (m_bBrowsingHUDMenu || GetIsImmobilised() || IsInteracting() || base.m_bIsStandingOnDesk || HasValidTransitionInteraction())
		{
			return ReturnFalseAndCheckInteractionInputProcessed();
		}
		if (m_bInteractionActionBlocked)
		{
			if (m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
			{
				return ReturnFalseAndCheckInteractionInputProcessed();
			}
			m_bInteractionActionBlocked = false;
		}
		if (m_NearestInteractiveObject == null)
		{
			return ReturnFalseAndCheckInteractionInputProcessed();
		}
		if (m_NearestDamagableTile != null && m_NearestDamagableTile.IsHoldingItem())
		{
			Vector3 position = m_Transform.position;
			Vector2 rhs = (m_NearestInteractiveObject.transform.position - position).normalized;
			Vector2 rhs2 = (m_NearestDamagableTile.transform.position - position).normalized;
			Vector2 facingDirection = GetFacingDirection();
			if (Vector2.Dot(facingDirection, rhs2) > Vector2.Dot(facingDirection, rhs))
			{
				return ReturnFalseAndCheckInteractionInputProcessed();
			}
		}
		if (!m_bPrimaryProcessed && !m_bCheckPrimaryInteractionFromInventoryClose)
		{
			if (m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
			{
				m_PrimaryTimePressed += UpdateManager.deltaTime;
			}
			if (m_PrimaryTimePressed >= 0.001f)
			{
				if ((double)m_PrimaryTimePressed > 0.2 && m_NearestInteractiveObject.HasPrimaryHoldInteractionsAvailable(this))
				{
					m_MyTrackedUIElements.SetPressAndHoldPercentage(m_PrimaryTimePressed / m_PressAndHoldDownTime, base.transform.position);
				}
				if (!m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction") || m_PrimaryTimePressed >= m_PressAndHoldDownTime)
				{
					m_bPrimaryProcessed = true;
					m_MyTrackedUIElements.HidePressAndHold();
					m_NearestInteractiveObject.ProcessPrimaryInteractions(this, m_PrimaryTimePressed >= m_PressAndHoldDownTime);
					m_PrimaryTimePressed = 0f;
				}
				return true;
			}
		}
		else if (!m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
		{
			m_bPrimaryProcessed = false;
			m_bCheckPrimaryInteractionFromInventoryClose = false;
		}
		if (!m_bSecondaryProcessed)
		{
			if (m_Gamer.m_RewiredPlayer.GetButton("SecondaryInteraction"))
			{
				m_SecondaryTimePressed += UpdateManager.deltaTime;
			}
			if (m_SecondaryTimePressed >= 0.001f)
			{
				if ((double)m_SecondaryTimePressed > 0.2 && m_NearestInteractiveObject.HasSecondaryHoldInteractionsAvailable(this))
				{
					m_MyTrackedUIElements.SetPressAndHoldPercentage(m_SecondaryTimePressed / m_PressAndHoldDownTime, base.transform.position);
				}
				if (!m_Gamer.m_RewiredPlayer.GetButton("SecondaryInteraction") || m_SecondaryTimePressed >= m_PressAndHoldDownTime)
				{
					m_bSecondaryProcessed = true;
					m_MyTrackedUIElements.HidePressAndHold();
					bool result = m_NearestInteractiveObject.ProcessSecondaryInteractions(this, m_SecondaryTimePressed >= m_PressAndHoldDownTime);
					m_SecondaryTimePressed = 0f;
					return result;
				}
			}
		}
		else if (!m_Gamer.m_RewiredPlayer.GetButton("SecondaryInteraction"))
		{
			m_bSecondaryProcessed = false;
		}
		if (!m_bTertiaryProcessed)
		{
			if (m_Gamer.m_RewiredPlayer.GetButton("TertiaryInteraction"))
			{
				m_TertiaryTimePressed += UpdateManager.deltaTime;
			}
			if (m_TertiaryTimePressed >= 0.001f)
			{
				if ((double)m_TertiaryTimePressed > 0.2 && m_NearestInteractiveObject.HasTertiaryHoldInteractionsAvailable(this))
				{
					m_MyTrackedUIElements.SetPressAndHoldPercentage(m_TertiaryTimePressed / m_PressAndHoldDownTime, base.transform.position);
				}
				if (!m_Gamer.m_RewiredPlayer.GetButton("TertiaryInteraction") || m_TertiaryTimePressed >= m_PressAndHoldDownTime)
				{
					m_bTertiaryProcessed = true;
					m_MyTrackedUIElements.HidePressAndHold();
					m_NearestInteractiveObject.ProcessTertiaryInteractions(this, m_TertiaryTimePressed >= m_PressAndHoldDownTime);
					m_TertiaryTimePressed = 0f;
					return true;
				}
			}
		}
		else if (!m_Gamer.m_RewiredPlayer.GetButton("TertiaryInteraction"))
		{
			m_bTertiaryProcessed = false;
		}
		return false;
	}

	private bool ReturnFalseAndCheckInteractionInputProcessed()
	{
		if (m_bPrimaryProcessed && !m_Gamer.m_RewiredPlayer.GetButton("PrimaryInteraction"))
		{
			m_bPrimaryProcessed = false;
		}
		if (m_bSecondaryProcessed && !m_Gamer.m_RewiredPlayer.GetButton("SecondaryInteraction"))
		{
			m_bSecondaryProcessed = false;
		}
		if (m_bTertiaryProcessed && !m_Gamer.m_RewiredPlayer.GetButton("TertiaryInteraction"))
		{
			m_bTertiaryProcessed = false;
		}
		if (m_PrimaryTimePressed > 0f || m_SecondaryTimePressed > 0f || m_TertiaryTimePressed > 0f)
		{
			if (m_MyTrackedUIElements != null)
			{
				m_MyTrackedUIElements.HidePressAndHold();
			}
			m_PrimaryTimePressed = 0f;
			m_SecondaryTimePressed = 0f;
			m_TertiaryTimePressed = 0f;
		}
		return false;
	}

	private bool ProcessShowMap()
	{
		bool bMapRequested = m_bMapRequested;
		m_bMapRequested = false;
		if (!CheckInputEnabled(PlayerInputs.ShowMap))
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (IsBrowsingHudMenu || m_OpenContainer != null || IsBrowsingPauseMenu)
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (m_InteractingObject != null && m_InteractingObject.m_NetObjectLock != null && m_InteractingObject.m_NetObjectLock.HasInteractionOfType<DeskInteraction>())
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonDown("OpenMainMap") || bMapRequested)
		{
			InGameMenuFlow.Instance.OpenMap(this, m_PlayerCameraManagerBindingID);
			return true;
		}
		return false;
	}

	private bool ProcessShowSelfMenu()
	{
		bool bInventoryRequested = m_bInventoryRequested;
		m_bInventoryRequested = false;
		if (!CheckInputEnabled(PlayerInputs.ShowSelfMenu))
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (IsBrowsingHudMenu || IsBrowsingPauseMenu)
		{
			return false;
		}
		bool flag = IsButtonUpAndNotSwallowed("Hotkey_CraftingMenu");
		bool flag2 = IsButtonUpAndNotSwallowed("Hotkey_Journal");
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			InGameMenuFlow.PlayerIGMData data = null;
			InGameMenuFlow.Instance.GetCorrectIGMData(m_PlayerCameraManagerBindingID, out data);
			if (data != null && data.m_PlayerRootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf)
			{
				int num = -1;
				if (flag)
				{
					num = 1;
				}
				if (flag2)
				{
					num = 2;
				}
				if (num != -1 && data.m_PlayerRootMenu.m_MainTabPanel.CurrentTabIndex != num)
				{
					data.m_PlayerRootMenu.m_MainTabPanel.AttemptToSetTabIndex(num);
				}
			}
			return false;
		}
		if (m_OpenContainer != null)
		{
			return false;
		}
		if (m_InteractingObject != null && !m_InteractingObject.AllowOtherPlayerHUDInteractions())
		{
			return false;
		}
		if (flag)
		{
			RequestInventoryOpen(1);
			return true;
		}
		if (flag2)
		{
			RequestInventoryOpen(2);
			return true;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonDown("Open Ingame Menu") || bInventoryRequested)
		{
			if (m_bIsInMinigame && !bInventoryRequested && m_Gamer != null && m_Gamer.m_RewiredPlayer != null && T17RewiredStandaloneInputModule.UsedSharedKeyboardAction("Open Ingame Menu", "Alternate_Key1", m_Gamer.m_RewiredPlayer))
			{
				return false;
			}
			RequestInventoryOpen();
			return true;
		}
		return false;
	}

	private void RequestInventoryOpen(int iTabIndex = 0)
	{
		int objectNetID = m_ItemContainer.GetObjectNetID();
		PlayerInventoryHUD inventoryHud = GetInventoryHud();
		if (inventoryHud != null && inventoryHud.IsExpandedHudVisible())
		{
			inventoryHud.HideInventory();
		}
		m_bPendingRequest = true;
		if (m_InteractingObject != null && m_InteractingObject.ShouldCancelOnOtherHUDInteractions())
		{
			RequestStopInteraction();
		}
		m_NetView.RPCQuestion("RPC_GrabInventoryLock", NetTargets.MasterClient, objectNetID, InGameRootMenu.InGameMenuTypeToOpen.MainSelf, iTabIndex);
	}

	private bool ProcessShowPauseMenu()
	{
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (IsBrowsingPauseMenu)
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("Pause"))
		{
			InGameMenuFlow.Instance.OpenPauseMenu(this);
			return true;
		}
		return false;
	}

	[PunRPC]
	private void RPC_GrabInventoryLock(int RPCID, int itemContainerID, InGameRootMenu.InGameMenuTypeToOpen openType, int iTabIndex, PhotonMessageInfo info)
	{
		ItemContainer itemContainer = null;
		PhotonView photonView = null;
		if (itemContainerID != -1)
		{
			photonView = PhotonView.Find(itemContainerID);
			itemContainer = photonView.GetComponent<ItemContainer>();
		}
		bool flag = false;
		if (itemContainer != null && m_Gamer != null)
		{
			if (itemContainer.GrabLock(m_Gamer.m_PhotonID))
			{
				m_OpenContainer = itemContainer;
				flag = true;
			}
			else
			{
				flag = false;
			}
		}
		m_NetView.RPCResponse("RPC_GrabInventoryLockResponse", RPCID, itemContainerID, flag, openType, iTabIndex);
	}

	[PunRPC]
	private void RPC_GrabInventoryLockResponse(int itemContainerID, bool success, InGameRootMenu.InGameMenuTypeToOpen openType, int iTabIndex, PhotonMessageInfo info)
	{
		if (DebugHelpers.LogGroupActive(DebugHelpers.LogGroup.PlayerInventory))
		{
		}
		m_bPendingRequest = false;
		if (!success)
		{
			return;
		}
		ItemContainer itemContainer = null;
		PhotonView photonView = null;
		if (itemContainerID != -1)
		{
			photonView = PhotonView.Find(itemContainerID);
			itemContainer = photonView.GetComponent<ItemContainer>();
		}
		if (!(itemContainer != null))
		{
			return;
		}
		InGameRootMenu.InGameMenuTypeToOpen inGameMenuTypeToOpen = openType;
		if (inGameMenuTypeToOpen == InGameRootMenu.InGameMenuTypeToOpen.MainSelf && itemContainer != m_ItemContainer)
		{
			switch (itemContainer.m_ContainerType)
			{
			case ItemContainer.ItemContainerType.Desk:
				inGameMenuTypeToOpen = InGameRootMenu.InGameMenuTypeToOpen.Desk;
				break;
			case ItemContainer.ItemContainerType.Cutlrey:
				inGameMenuTypeToOpen = InGameRootMenu.InGameMenuTypeToOpen.Cutlrey;
				break;
			case ItemContainer.ItemContainerType.Guard:
				inGameMenuTypeToOpen = ((!itemContainer.GetCharacterOwner().m_bIsKnockedOut) ? InGameRootMenu.InGameMenuTypeToOpen.Guard : InGameRootMenu.InGameMenuTypeToOpen.DownedGuard);
				break;
			case ItemContainer.ItemContainerType.Inmate:
				inGameMenuTypeToOpen = ((!itemContainer.GetCharacterOwner().m_bIsKnockedOut) ? InGameRootMenu.InGameMenuTypeToOpen.Inmate : InGameRootMenu.InGameMenuTypeToOpen.DownedInmate);
				break;
			case ItemContainer.ItemContainerType.SwagBag:
				inGameMenuTypeToOpen = InGameRootMenu.InGameMenuTypeToOpen.SwagBag;
				break;
			}
		}
		m_OpenContainer = itemContainer;
		ViewContainer(m_OpenContainer, inGameMenuTypeToOpen, iTabIndex);
	}

	private IEnumerator DelayedViewContainer(ItemContainer itemContainer, InGameRootMenu.InGameMenuTypeToOpen menuType)
	{
		yield return new WaitForEndOfFrame();
		m_bPendingRequest = false;
		ViewContainer(m_OpenContainer, menuType);
	}

	private bool ProcessAttack()
	{
		if (m_bIsBlocking)
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID) || GetIsImmobilised() || IsInteracting() || !CheckInputEnabled(PlayerInputs.Attack))
		{
			if (m_bAttackCharging)
			{
				m_bAttackCharging = false;
				ResetChargeAttack();
				if (!(m_MyTrackedUIElements != null))
				{
				}
			}
			return false;
		}
		SetMenuAndHUDItems();
		if (m_MyTrackedUIElements != null)
		{
			if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null)
			{
				if (m_Gamer.m_RewiredPlayer.GetButtonDown("Attack"))
				{
					m_bAttackCharging = true;
				}
				if (m_bAttackCharging)
				{
					bool flag = m_Gamer.m_RewiredPlayer.GetButtonUp("Attack") || IsClimbingOnObject();
					if (flag)
					{
						TutorialManager instance = TutorialManager.GetInstance();
						if (instance != null)
						{
							instance.StartTutorialRPC(this, TutorialSubject.Combat);
						}
					}
					m_bAttackCharging = ChargeAttack(flag);
					if (!((double)base.SmashAttackChargeTimer > 0.2))
					{
					}
				}
			}
			if ((!m_bAttackCharging || !m_bIsDashing) && !(base.SmashAttackChargeTimer <= 0f))
			{
			}
		}
		return false;
	}

	private bool ProcessBlock()
	{
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID) || GetCombatReady() || IsInteracting() || !CheckInputEnabled(PlayerInputs.Block))
		{
			CombatBlock(doBlock: false);
			return false;
		}
		bool button = m_Gamer.m_RewiredPlayer.GetButton("Block");
		if (!m_bIsBlocking && button)
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(this, TutorialSubject.Combat);
			}
			CombatBlock(doBlock: true);
			return true;
		}
		if (m_bIsBlocking && !button)
		{
			CombatBlock(doBlock: false);
		}
		return false;
	}

	private bool ProcessCharacterTargetting()
	{
		if (!CheckInputEnabled(PlayerInputs.TargetCharacter))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		bool flag = !(GetCombatReady() || IsInteracting());
		if (m_bCharacterTargettingEnabled && !flag)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_LockOn_Disable, base.gameObject);
			DisableTargetElement();
			return false;
		}
		if (m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.GetButtonDown("CombatTargetting") && flag)
		{
			bool flag2 = false;
			m_MouseDetector.MouseOverCharacterManualUpdate();
			m_MouseDetector.GetCurrentCharacter(ref m_MouseOverCharacter);
			if (m_MouseOverCharacter != null)
			{
				Character characterOwner = m_MouseOverCharacter.CharacterOwner;
				if (characterOwner == null || characterOwner != base.m_CharacterTarget)
				{
					bool flag3 = false;
					IList<InputActionSourceData> list = null;
					list = m_Gamer.m_RewiredPlayer.GetCurrentInputSources("CombatTargetting");
					for (int num = list.Count - 1; num >= 0; num--)
					{
						if (list[num].controllerType == ControllerType.Mouse)
						{
							flag3 = true;
							m_PlayerMouse = (Mouse)list[num].controller;
							break;
						}
					}
					flag2 = true;
					if (!m_bCharacterTargettingEnabled || (m_bCharacterTargettingEnabled && !flag3))
					{
						m_bCharacterTargettingEnabled = !m_bCharacterTargettingEnabled;
					}
				}
			}
			if (!flag2)
			{
				m_bCharacterTargettingEnabled = !m_bCharacterTargettingEnabled;
			}
			if (!m_bCharacterTargettingEnabled)
			{
				AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Combat_LockOn_Disable, base.gameObject);
				DisableTargetElement();
			}
			else
			{
				m_bNeedsToUpdateCharacterTarget = true;
			}
			return true;
		}
		return false;
	}

	private bool ProcessUseItem()
	{
		if (!CheckInputEnabled(PlayerInputs.UseItem))
		{
			return false;
		}
		if (m_AllowedUsableItem != null && m_EquippedItem != null && m_AllowedUsableItem.m_ItemDataID != m_EquippedItem.ItemDataID)
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting())
		{
			return false;
		}
		if (m_OpenContainer != null || IsBrowsingHudMenu || IsBrowsingPauseMenu)
		{
			return false;
		}
		if (m_EquippedItem == null || m_EquippedItem.IsInUse())
		{
			return false;
		}
		if (m_PlayerInventoryHUD.IsPlayerInteractingWithExpandedHud())
		{
			return false;
		}
		bool flag = false;
		if (m_EquippedItem.IsPressAndHoldMultiUse())
		{
			if (m_Gamer.m_RewiredPlayer.GetButtonDown("Use"))
			{
				m_PreviousEndUseTime = 0f;
				m_bWasUsingItem = false;
				flag = true;
			}
			else if (m_Gamer.m_RewiredPlayer.GetButton("Use"))
			{
				float buttonTimePressed = m_Gamer.m_RewiredPlayer.GetButtonTimePressed("Use");
				if (m_bWasUsingItem)
				{
					m_PreviousEndUseTime = buttonTimePressed;
				}
				if (buttonTimePressed - m_PreviousEndUseTime >= 0.1f)
				{
					flag = true;
				}
			}
		}
		else if (m_bUseKeyIsDown && m_Gamer.m_RewiredPlayer.GetButtonUp("Use"))
		{
			m_bUseKeyIsDown = false;
		}
		else
		{
			flag = m_Gamer.m_RewiredPlayer.GetButtonUp("Use");
		}
		m_bWasUsingItem = false;
		if (flag)
		{
			if (m_bCharacterTargettingEnabled && !m_EquippedItem.m_ItemData.IsConsumable())
			{
				return false;
			}
			if (m_EquippedItem.CanUse())
			{
				m_EquippedItem.Use();
				m_bWasUsingItem = true;
				return true;
			}
		}
		return false;
	}

	private bool ProcessPickUpItem()
	{
		if (!CheckInputEnabled(PlayerInputs.PickUpItem))
		{
			return false;
		}
		if (T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (IsBrowsingHudMenu || IsBrowsingPauseMenu)
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting() || base.m_bIsStandingOnDesk)
		{
			return false;
		}
		PlayerInventoryHUD inventoryHud = GetInventoryHud();
		if (inventoryHud != null && inventoryHud.IsPlayerInteractingWithExpandedHud())
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("Pickup") && m_NearestItem != null)
		{
			ItemContainer levelItemContainer = LevelScript.GetInstance().m_LevelItemContainer;
			if (levelItemContainer.HasSpecificItem(m_NearestItem.m_NetView.viewID))
			{
				if (m_ItemContainer.IsVisibleFull())
				{
					if (!(m_EquippedItem == null) || !CanEquipItem(m_NearestItem))
					{
						TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.DiscardItem);
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
						return false;
					}
					m_NetView.RPC("RPC_Master_RequestPickupFloorItem", NetTargets.MasterClient, m_NetView.viewID, m_NearestItem.m_NetView.viewID, true);
				}
				else
				{
					m_NetView.RPC("RPC_Master_RequestPickupFloorItem", NetTargets.MasterClient, m_NetView.viewID, m_NearestItem.m_NetView.viewID, false);
				}
				return true;
			}
		}
		return false;
	}

	[PunRPC]
	private void RPC_Master_RequestPickupFloorItem(int characterViewId, int floorItemViewId, bool shouldEquipItem, PhotonMessageInfo info)
	{
		Item item = T17NetView.Find<Item>(floorItemViewId);
		Player player = T17NetView.Find<Player>(characterViewId);
		ItemContainer levelItemContainer = LevelScript.GetInstance().m_LevelItemContainer;
		if (!levelItemContainer.HasSpecificItem(floorItemViewId))
		{
			return;
		}
		if (shouldEquipItem)
		{
			if (player.m_EquippedItem == null && player.CanEquipItem(item))
			{
				levelItemContainer.RemoveItemRPC(item);
				SetEquippedItem(item);
				m_NetView.RPC("RPC_Client_PickedupItem", info.sender, true);
			}
		}
		else if (!player.m_ItemContainer.IsVisibleFull())
		{
			levelItemContainer.RemoveItemRPC(item);
			if (!player.m_ItemContainer.AddItemRPC(item))
			{
				item.DropItemInLevel(this, player.transform.position);
				return;
			}
			m_NetView.RPC("RPC_Client_PickedupItem", info.sender, false);
		}
	}

	[PunRPC]
	private void RPC_Client_PickedupItem(bool wasItemEquipped)
	{
		AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Take_Item, base.gameObject);
		if (wasItemEquipped)
		{
			PlayerInventoryHUD inventoryHud = GetInventoryHud();
			if (inventoryHud != null)
			{
				inventoryHud.m_CurrentEquippedItem.StartSmoke();
			}
		}
	}

	private bool ProcessPickUpTileCover()
	{
		if (!CheckInputEnabled(PlayerInputs.PickUpTileCover))
		{
			return false;
		}
		if (T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (IsBrowsingHudMenu || IsBrowsingPauseMenu)
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting() || m_PlayerInventoryHUD.IsPlayerInteractingWithExpandedHud())
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("Pickup") && m_NearestDamagableTile != null && m_NearestDamagableTile.IsHoldingItem())
		{
			if (!m_ItemContainer.IsVisibleFull() || m_EquippedItem == null)
			{
				FloorManager.TileSystem_Type systemType = ((m_NearestDamagableTile.m_DamageAction == DamagableTile.DamageAction.Cut || m_NearestDamagableTile.m_DamageAction == DamagableTile.DamageAction.Chip || m_NearestDamagableTile.m_DamageAction == DamagableTile.DamageAction.Dig) ? FloorManager.TileSystem_Type.TileSystem_Wall : FloorManager.TileSystem_Type.TileSystem_Ground);
				int row = 0;
				int column = 0;
				if (FloorManager.GetInstance().GetTileGridPoint(m_NearestDamagableTile.CurrentFloor, systemType, m_NearestDamagableTile.transform.position, out row, out column))
				{
					FloorManager.GetInstance().RemoveTileItem(m_NearestDamagableTile.CurrentFloor, systemType, row, column, m_ItemContainer);
					AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Take_Item, base.gameObject);
					return true;
				}
			}
			TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.DiscardItem);
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Unavailable, base.gameObject);
			return false;
		}
		return false;
	}

	private bool ProcessTransition()
	{
		if (!CheckInputEnabled(PlayerInputs.Transition))
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting())
		{
			return false;
		}
		if (m_OpenContainer != null)
		{
			return false;
		}
		if (m_PlayerInventoryHUD != null && m_PlayerInventoryHUD.IsPlayerInteractingWithExpandedHud())
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("Open"))
		{
			PlayerTransitionData transitionData = m_TransitionData;
			if (transitionData.m_HoleInTransRange != null)
			{
				bool flag = DoTransition(transitionData.m_HoleInTransRange.gameObject, transitionData.m_TransitionFloorID, transitionData.m_bTransitionDown, transitionData.m_TransitionOffset);
				if (flag)
				{
					TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.UndergroundDig);
				}
				return flag;
			}
			if (transitionData.m_VentInTransRange != null)
			{
				return DoTransition(transitionData.m_VentInTransRange.gameObject, transitionData.m_TransitionFloorID, transitionData.m_bTransitionDown, transitionData.m_TransitionOffset);
			}
			if (transitionData.m_StaticLadderInTransRange != null)
			{
				return DoTransitionLadder(transitionData.m_StaticLadderInTransRange.gameObject, transitionData.m_TransitionFloorID, transitionData.m_bTransitionDown, new Vector2(transitionData.m_TransitionOffsetX, transitionData.m_TransitionOffset));
			}
		}
		return false;
	}

	private void GetValidTransitionHole()
	{
		m_TransitionData.m_TransitionOffset = -1f;
		m_TransitionData.m_TransitionFloorID = -1;
		m_TransitionData.m_bTransitionDown = false;
		m_TransitionData.m_HoleInTransRange = null;
		if (m_CurrentFloor.IsUnderGround())
		{
			FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(m_CurrentFloor);
			Hole hole = FloorManager.GetInstance().GetHole(m_CurrentTileRow + -1, m_CurrentTileColumn, floor.m_FloorIndex);
			if (hole != null && hole.IsFullyDug())
			{
				m_TransitionData.m_TransitionOffset = 0f;
				m_TransitionData.m_TransitionFloorID = 1;
				m_TransitionData.m_bTransitionDown = false;
				m_TransitionData.m_HoleInTransRange = hole;
			}
		}
		else
		{
			if (m_CurrentFloor.IsVent() || base.m_bIsStandingOnDesk)
			{
				return;
			}
			Hole hole2 = FloorManager.GetInstance().GetHole(m_CurrentTileRow, m_CurrentTileColumn, m_CurrentFloor.m_FloorIndex);
			if (hole2 != null && hole2.IsFullyDug())
			{
				FloorManager.Floor floor2 = FloorManager.GetInstance().DownAFloor(m_CurrentFloor);
				if (floor2.IsVent())
				{
					floor2 = FloorManager.GetInstance().DownAFloor(floor2);
				}
				int transitionFloorID = m_CurrentFloor.m_FloorIndex - floor2.m_FloorIndex;
				int row = m_CurrentTileRow + 1;
				if (FloorManager.GetInstance().CheckIsInBounds(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, row, m_CurrentTileColumn) && !FloorManager.GetInstance().CheckTileExists(floor2, FloorManager.TileSystem_Type.TileSystem_Wall, row, m_CurrentTileColumn) && FloorManager.GetInstance().IsFloorClear(floor2, row, m_CurrentTileColumn) && FloorManager.GetInstance().GetRock(row, m_CurrentTileColumn, floor2.m_FloorIndex) == null)
				{
					m_TransitionData.m_TransitionOffset = -1f;
					m_TransitionData.m_TransitionFloorID = transitionFloorID;
					m_TransitionData.m_bTransitionDown = true;
					m_TransitionData.m_HoleInTransRange = hole2;
				}
			}
		}
	}

	private void GetValidTransitionVent()
	{
		m_TransitionData.m_TransitionOffset = -1f;
		m_TransitionData.m_TransitionFloorID = -1;
		m_TransitionData.m_bTransitionDown = false;
		m_TransitionData.m_VentInTransRange = null;
		if (!m_CurrentFloor.IsUnderGround())
		{
			FloorManager.Floor currentFloor = m_CurrentFloor;
			VentCover ventCover = FloorManager.GetInstance().GetVentCover(m_CurrentTileRow, m_CurrentTileColumn, currentFloor.m_FloorIndex);
			if ((bool)ventCover && ventCover.HasBeenRemoved() && !base.m_bIsStandingOnDesk)
			{
				float transitionOffset = ((!m_CurrentFloor.IsVent()) ? (-1f) : 0f);
				m_TransitionData.m_TransitionOffset = transitionOffset;
				m_TransitionData.m_TransitionFloorID = 1;
				m_TransitionData.m_bTransitionDown = true;
				m_TransitionData.m_VentInTransRange = ventCover;
			}
			FloorManager.Floor floor = FloorManager.GetInstance().UpAFloor(currentFloor);
			int num = (m_CurrentFloor.IsVent() ? (-1) : 0);
			ventCover = FloorManager.GetInstance().GetVentCover(m_CurrentTileRow + num, m_CurrentTileColumn, floor.m_FloorIndex);
			if ((bool)ventCover && ventCover.HasBeenRemoved() && (m_CurrentFloor.IsVent() || base.m_bIsStandingOnDesk))
			{
				m_TransitionData.m_TransitionOffset = 0f;
				m_TransitionData.m_TransitionFloorID = 1;
				m_TransitionData.m_bTransitionDown = false;
				m_TransitionData.m_VentInTransRange = ventCover;
			}
		}
	}

	private void GetValidTransitionStaticLadder()
	{
		m_TransitionData.m_TransitionOffset = -1f;
		m_TransitionData.m_TransitionFloorID = -1;
		m_TransitionData.m_bTransitionDown = false;
		m_TransitionData.m_StaticLadderInTransRange = null;
		FloorManager.Floor currentFloor = m_CurrentFloor;
		FloorManager instance = FloorManager.GetInstance();
		bool flag = false;
		StaticLadder staticLadder = instance.GetStaticLadder(m_CurrentTileRow - 1, m_CurrentTileColumn, currentFloor.m_FloorIndex);
		if (staticLadder != null)
		{
			flag = true;
		}
		if (staticLadder == null)
		{
			staticLadder = instance.GetStaticLadder(m_CurrentTileRow + 1, m_CurrentTileColumn, currentFloor.m_FloorIndex);
		}
		if (staticLadder == null)
		{
			staticLadder = instance.GetStaticLadder(m_CurrentTileRow, m_CurrentTileColumn - 1, currentFloor.m_FloorIndex);
		}
		if (staticLadder == null)
		{
			staticLadder = instance.GetStaticLadder(m_CurrentTileRow, m_CurrentTileColumn + 1, currentFloor.m_FloorIndex);
		}
		if (!(staticLadder != null) || (!flag && !staticLadder.m_bOmniDirectionalEnter))
		{
			return;
		}
		Vector2 vector = staticLadder.m_ExitDirection switch
		{
			Directionx4.Up => new Vector2(0f, 1f), 
			Directionx4.Left => new Vector2(-1f, 0f), 
			Directionx4.Down => new Vector2(0f, -1f), 
			Directionx4.Right => new Vector2(1f, 0f), 
			_ => new Vector2(0f, 0f), 
		};
		if (staticLadder.DownwardTransition)
		{
			if (!staticLadder.m_bTransitionsToFromVent)
			{
				vector.y += -1f;
			}
			m_TransitionData.m_TransitionOffsetX = vector.x;
			m_TransitionData.m_TransitionOffset = vector.y;
			m_TransitionData.m_bTransitionDown = true;
			m_TransitionData.m_TransitionFloorID = staticLadder.NumFloorTransitions;
			m_TransitionData.m_StaticLadderInTransRange = staticLadder;
		}
		else
		{
			if (!staticLadder.m_bTransitionsToFromVent)
			{
				vector.y += 1f;
			}
			m_TransitionData.m_TransitionOffsetX = vector.x;
			m_TransitionData.m_TransitionOffset = vector.y;
			m_TransitionData.m_bTransitionDown = false;
			m_TransitionData.m_TransitionFloorID = staticLadder.NumFloorTransitions;
			m_TransitionData.m_StaticLadderInTransRange = staticLadder;
		}
	}

	private bool DoTransition(GameObject transitionObj, int numFloors, bool transitionDown, float offsetY)
	{
		if (transitionObj != null)
		{
			Vector3 vector = new Vector3(0f, 0f, -FloorManager.GetInstance().m_FloorOffset * numFloors);
			if (!transitionDown)
			{
				vector *= -1f;
			}
			Vector3 newPosition = base.transform.position + vector;
			newPosition.x = transitionObj.transform.position.x;
			newPosition.y = transitionObj.transform.position.y + offsetY;
			if (Teleport(newPosition))
			{
				if (transitionDown && !m_CurrentFloor.IsVent() && !m_CurrentFloor.IsUnderGround())
				{
					int layerMask = 1 << LayerMask.NameToLayer("DynamicMapObject");
					int num = CheckForCollisions(layerMask);
					if (num > 0)
					{
						Collider[] lastCollisionCheckResults = GetLastCollisionCheckResults();
						for (int i = 0; i < lastCollisionCheckResults.Length; i++)
						{
							if (lastCollisionCheckResults[i] != null)
							{
								ClimbableObject component = lastCollisionCheckResults[i].GetComponent<ClimbableObject>();
								if (component != null)
								{
									OnTransitionOnObject(component);
									break;
								}
							}
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	private bool DoTransitionLadder(GameObject transitionObj, int numFloors, bool transitionDown, Vector2 offset)
	{
		if (transitionObj != null)
		{
			Vector3 vector = new Vector3(0f, 0f, -FloorManager.GetInstance().m_FloorOffset * numFloors);
			if (!transitionDown)
			{
				vector *= -1f;
			}
			Vector3 newPosition = base.transform.position + vector;
			newPosition.x = transitionObj.transform.position.x + offset.x;
			newPosition.y = transitionObj.transform.position.y + offset.y;
			if (Teleport(newPosition))
			{
				if (transitionDown && !m_CurrentFloor.IsVent() && !m_CurrentFloor.IsUnderGround())
				{
					int layerMask = 1 << LayerMask.NameToLayer("DynamicMapObject");
					int num = CheckForCollisions(layerMask);
					if (num > 0)
					{
						Collider[] lastCollisionCheckResults = GetLastCollisionCheckResults();
						for (int i = 0; i < lastCollisionCheckResults.Length; i++)
						{
							if (lastCollisionCheckResults[i] != null)
							{
								ClimbableObject component = lastCollisionCheckResults[i].GetComponent<ClimbableObject>();
								if (component != null)
								{
									OnTransitionOnObject(component);
									break;
								}
							}
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	private bool ProcessCloseContainer()
	{
		if (!CheckInputEnabled(PlayerInputs.CloseContainer))
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (m_OpenContainer == null)
		{
			return false;
		}
		if (IsBrowsingPauseMenu || IsBrowsingMainMap)
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("UI_Close"))
		{
			m_bPendingRequest = true;
			InGameMenuFlow.PlayerIGMData data = null;
			InGameMenuFlow.Instance.GetCorrectIGMData(m_PlayerCameraManagerBindingID, out data);
			bool flag = false;
			if (data != null)
			{
				flag = data.m_PlayerRootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf;
			}
			m_NetView.RPC("RPC_CloseContainer", NetTargets.MasterClient);
			if (IsInteracting() && !flag)
			{
				RequestStopInteraction();
			}
			return true;
		}
		return false;
	}

	public bool RequestCloseContainer()
	{
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (m_OpenContainer == null)
		{
			return false;
		}
		m_bPendingRequest = true;
		m_NetView.RPC("RPC_CloseContainer", NetTargets.MasterClient);
		return true;
	}

	private bool ProcessDropItemHUD()
	{
		if (!CheckInputEnabled(PlayerInputs.DropItemHUD))
		{
			return false;
		}
		if (m_bPendingRequest || T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance == null || InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting())
		{
			StopAndHideDropHudItem();
			return false;
		}
		SetMenuAndHUDItems();
		if (m_PlayerInventoryHUD != null && !m_PlayerInventoryHUD.HasItemInCurrentSlot())
		{
			StopAndHideDropHudItem();
			return false;
		}
		if (m_bDropitemProcessed)
		{
			if (!m_Gamer.m_RewiredPlayer.GetButton("UI_Drop"))
			{
				m_bDropitemProcessed = false;
				if (m_MyTrackedUIElements != null)
				{
					m_MyTrackedUIElements.HidePressAndHold();
				}
			}
			return true;
		}
		bool button = m_Gamer.m_RewiredPlayer.GetButton("UI_Drop");
		if (m_Gamer.m_RewiredPlayer.GetButtonDown("UI_Drop"))
		{
			m_TimeDropButtonHeld = 0f;
		}
		if (button)
		{
			PlayerInventoryHUD inventoryHud = GetInventoryHud();
			bool flag = false;
			if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && inventoryHud != null)
			{
				Rewired.Player rewiredPlayer = m_Gamer.m_RewiredPlayer;
				IList<InputActionSourceData> currentInputSources = rewiredPlayer.GetCurrentInputSources("UI_Drop");
				for (int num = currentInputSources.Count() - 1; num >= 0; num--)
				{
					if (currentInputSources[num].controllerType == ControllerType.Mouse)
					{
						flag = true;
					}
				}
			}
			if (!flag || (flag && inventoryHud.HasMouseOver))
			{
				m_TimeDropButtonHeld += UpdateManager.deltaTime;
				if (m_TimeDropButtonHeld > m_DropItemHoldTime)
				{
					m_TimeDropButtonHeld = 0f;
					m_bDropitemProcessed = true;
					if (m_MyTrackedUIElements != null)
					{
						m_MyTrackedUIElements.HidePressAndHold();
					}
					int currentSelectedObjectIndex = m_PlayerInventoryHUD.CurrentSelectedObjectIndex;
					if (currentSelectedObjectIndex != -1)
					{
						DropInventoryItem(m_ItemContainer, currentSelectedObjectIndex);
						HUDMenuFlow instance = HUDMenuFlow.Instance;
						if (instance != null && inventoryHud != null)
						{
							inventoryHud.ResetCycleAndHideTimers();
						}
						return true;
					}
					if (DropEquipedItem(base.transform.position))
					{
						HUDMenuFlow instance2 = HUDMenuFlow.Instance;
						if (instance2 != null && inventoryHud != null)
						{
							inventoryHud.PopulateWithItemContainer(firstTimeInit: true);
						}
						return true;
					}
				}
				else if (m_MyTrackedUIElements != null)
				{
					m_MyTrackedUIElements.SetPressAndHoldPercentage(m_TimeDropButtonHeld / m_DropItemHoldTime, base.transform.position);
				}
			}
			else if (m_TimeDropButtonHeld != 0f)
			{
				m_TimeDropButtonHeld = 0f;
				if (m_MyTrackedUIElements != null)
				{
					m_MyTrackedUIElements.HidePressAndHold();
				}
			}
		}
		else if (m_MyTrackedUIElements != null)
		{
			m_MyTrackedUIElements.HidePressAndHold();
		}
		return button;
	}

	private void StopAndHideDropHudItem()
	{
		if (m_Gamer.m_RewiredPlayer.GetButton("UI_Drop"))
		{
			m_bDropitemProcessed = false;
			m_TimeDropButtonHeld = 0f;
			if (m_MyTrackedUIElements != null)
			{
				m_MyTrackedUIElements.HidePressAndHold();
			}
		}
	}

	private bool ProcessTagTile()
	{
		if (!CheckInputEnabled(PlayerInputs.TagTile))
		{
			return false;
		}
		if (T17DialogBoxManager.HasDialogsForGamer(m_Gamer))
		{
			return false;
		}
		if (InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (IsBrowsingHudMenu || IsBrowsingPauseMenu)
		{
			return false;
		}
		if (GetIsImmobilised() || IsInteracting())
		{
			return false;
		}
		if (m_Gamer.m_RewiredPlayer.GetButtonUp("Tag"))
		{
			int num = m_CurrentTileRow;
			int num2 = m_CurrentTileColumn;
			switch (m_x4FacingDirection)
			{
			case Directionx4.Up:
				num--;
				break;
			case Directionx4.Left:
				num2--;
				break;
			case Directionx4.Down:
				num++;
				break;
			case Directionx4.Right:
				num2++;
				break;
			}
			if (num != -1 && num2 != -1)
			{
				FloorManager.GetInstance().GetTileSystemBounds(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
				if (num > 0 && num < maxRows && num2 > 0 && num2 < maxColumns)
				{
					if (TagManager.GetInstance().RemoveTag(this, num, num2, m_CurrentFloor.m_FloorIndex))
					{
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Tag_Reject, base.gameObject);
						return true;
					}
					if (TagManager.GetInstance().PlaceTagForPlayer(this, num, num2, m_CurrentFloor.m_FloorIndex))
					{
						StatSystem.GetInstance().IncStat(28, 1f, m_Gamer, string.Empty);
						AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_UI_Tag_Select, base.gameObject);
					}
					return true;
				}
			}
		}
		return false;
	}

	private bool ProcessShowNamePlates()
	{
		if (!CheckInputEnabled(PlayerInputs.ShowNamePlates))
		{
			return false;
		}
		if (m_Gamer != null && m_Gamer.m_RewiredPlayer != null && m_Gamer.m_RewiredPlayer.GetButtonUp("ShowNamePlates"))
		{
			m_bEnabledWorldNamePlates = !m_bEnabledWorldNamePlates;
			WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(m_CurrentFloor.m_FloorIndex);
			if (uIElementsWorldCanvas != null)
			{
				if (m_bEnabledWorldNamePlates)
				{
					m_MyTrackedUIElements.DisableTrackers();
					uIElementsWorldCanvas.EnableAllNameplatesForCamera(m_PlayerCameraManagerBindingID);
				}
				else
				{
					m_MyTrackedUIElements.EnableTrackers();
					uIElementsWorldCanvas.DisableAllNameplatesForCamera(m_PlayerCameraManagerBindingID);
				}
				return true;
			}
		}
		return false;
	}

	public void DisableAllUITrackers()
	{
		if (m_bAllUITrackersWereDisabled)
		{
			return;
		}
		m_bAllUITrackersWereDisabled = true;
		if (m_bEnabledWorldNamePlates)
		{
			WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(m_CurrentFloor.m_FloorIndex);
			if (uIElementsWorldCanvas != null)
			{
				uIElementsWorldCanvas.DisableAllNameplatesForCamera(m_PlayerCameraManagerBindingID);
			}
		}
		else
		{
			m_MyTrackedUIElements.DisableTrackers();
		}
	}

	public void RestoreAllUITrackers()
	{
		if (!m_bAllUITrackersWereDisabled)
		{
			return;
		}
		m_bAllUITrackersWereDisabled = false;
		if (m_bEnabledWorldNamePlates)
		{
			WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(m_CurrentFloor.m_FloorIndex);
			if (uIElementsWorldCanvas != null)
			{
				uIElementsWorldCanvas.DisableAllNameplatesForCamera(m_PlayerCameraManagerBindingID);
			}
		}
		else
		{
			m_MyTrackedUIElements.EnableTrackers();
		}
	}

	private void UpdateEquipedItemTargetting()
	{
		if (!(m_MyTrackedUIElements != null))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		Item equippedItem = GetEquippedItem();
		if (equippedItem != null && equippedItem.RequiresTargetting())
		{
			if (equippedItem.IsInUse())
			{
				flag2 = true;
				flag = false;
			}
			else
			{
				int num = m_CurrentTileRow;
				int num2 = m_CurrentTileColumn;
				if (!base.m_bIsStandingOnDesk)
				{
					switch (m_x4FacingDirection)
					{
					case Directionx4.Up:
						num--;
						break;
					case Directionx4.Left:
						num2--;
						break;
					case Directionx4.Down:
						num++;
						break;
					case Directionx4.Right:
						num2++;
						break;
					}
					BaseItemFunctionality baseItemFunctionality = equippedItem.HasFunctionality(BaseItemFunctionality.Functionality.Climb);
					if (baseItemFunctionality != null)
					{
						ClimbFunctionality climbFunctionality = baseItemFunctionality as ClimbFunctionality;
						if (climbFunctionality != null && climbFunctionality.m_EquipAction == ClimbFunctionality.EquipAction.ClimbUp)
						{
							num += -1;
						}
					}
				}
				FloorManager.GetInstance().GetTileSystemBounds(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, out var maxRows, out var maxColumns);
				if (num >= 0 && num < maxRows && num2 >= 0 && num2 < maxColumns)
				{
					SetTargetTile(num, num2);
					flag2 = true;
					if (!m_bCharacterTargettingEnabled && !IsInteracting() && !base.m_bIsKnockedOut && !base.m_bIsBound && (GetIsImmobilised() || m_WalkVector.magnitude <= 0.001f))
					{
						Sprite validTargetHUDSpriteOverride = null;
						bool bValidPosition = equippedItem.CanUse(m_x4FacingDirection, out validTargetHUDSpriteOverride);
						m_MyTrackedUIElements.ShowTarget(num, num2, bValidPosition, validTargetHUDSpriteOverride);
						flag = true;
					}
				}
			}
		}
		if (!flag2)
		{
			SetTargetTile(-1, -1);
		}
		if (!flag)
		{
			m_MyTrackedUIElements.HideTarget();
		}
	}

	private void UpdateMyTrackedElement()
	{
		FloorManager.GetInstance().GetTileGridPoint(m_CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.transform.position, out m_CurrentTileRow, out m_CurrentTileColumn);
		if (m_MyTrackedElement != null && m_bHaveNamePlate)
		{
			m_MyTrackedElement.SetNamePlateText(m_Gamer.m_PhotonID + ", " + m_Gamer.m_GamerName);
		}
	}

	private void UpdateHoleTransitions()
	{
		GetValidTransitionHole();
		if (m_MyTrackedUIElements == null)
		{
			SetMenuAndHUDItems();
		}
		if (!(m_MyTrackedUIElements != null))
		{
			return;
		}
		if (m_TransitionData.m_HoleInTransRange != null)
		{
			if (m_TransitionData.m_HoleInTransRange != m_PreviousTransitionData.m_HoleInTransRange || m_TransitionData.m_TransitionFloorID != m_PreviousTransitionData.m_TransitionFloorID || m_TransitionData.m_bTransitionDown != m_PreviousTransitionData.m_bTransitionDown)
			{
				m_MyTrackedUIElements.ShowClimbIcon(m_TransitionData.m_bTransitionDown, m_TransitionData.m_HoleInTransRange.TileRow, m_TransitionData.m_HoleInTransRange.TileColumn, this);
			}
		}
		else
		{
			GetValidTransitionVent();
			if (m_TransitionData.m_VentInTransRange != null)
			{
				if (m_TransitionData.m_VentInTransRange != m_PreviousTransitionData.m_VentInTransRange || m_TransitionData.m_TransitionFloorID != m_PreviousTransitionData.m_TransitionFloorID || m_TransitionData.m_bTransitionDown != m_PreviousTransitionData.m_bTransitionDown)
				{
					m_MyTrackedUIElements.ShowClimbIcon(m_TransitionData.m_bTransitionDown, m_TransitionData.m_VentInTransRange.TileRow, m_TransitionData.m_VentInTransRange.TileColumn, this);
				}
			}
			else
			{
				GetValidTransitionStaticLadder();
				if (m_TransitionData.m_StaticLadderInTransRange != null)
				{
					if (m_TransitionData.m_StaticLadderInTransRange != m_PreviousTransitionData.m_StaticLadderInTransRange || m_TransitionData.m_TransitionFloorID != m_PreviousTransitionData.m_TransitionFloorID || m_TransitionData.m_bTransitionDown != m_PreviousTransitionData.m_bTransitionDown)
					{
						m_MyTrackedUIElements.ShowClimbIcon(m_TransitionData.m_bTransitionDown, m_TransitionData.m_StaticLadderInTransRange.TileRow, m_TransitionData.m_StaticLadderInTransRange.TileColumn, this);
					}
				}
				else
				{
					m_MyTrackedUIElements.HideClimbIcon();
				}
			}
		}
		m_PreviousTransitionData = m_TransitionData;
	}

	public bool HasValidTransitionInteraction()
	{
		return m_TransitionData.m_HoleInTransRange != null || m_TransitionData.m_VentInTransRange != null || m_TransitionData.m_StaticLadderInTransRange != null;
	}

	private bool UpdateProximityTrackedElements(bool bFullUpdate)
	{
		bool flag = false;
		if (bFullUpdate)
		{
			if (m_MyTrackedUIElements != null)
			{
				if (m_ElapsedProximityTime >= m_ProximityCheckTime)
				{
					flag = true;
					int i = 0;
					for (int j = 0; j < m_CurrentListOfPrioritySortedTrackedElements.Length; j++)
					{
						if (m_CurrentListOfPrioritySortedTrackedElements[j] != null)
						{
							m_OldProximityElements[i] = m_CurrentListOfPrioritySortedTrackedElements[j].GetUITrackedElement(m_PlayerCameraManagerBindingID);
							if (m_OldProximityElements[i] != null)
							{
								m_MyTrackedUIElements.ReleaseTrackedUIElementWithoutDisable(m_OldProximityElements[i]);
								i++;
							}
						}
					}
					for (int k = 0; k < m_ListOfCurrentProximityHoles.Length; k++)
					{
						if (m_ListOfCurrentProximityHoles[k] != null && m_ListOfCurrentProximityHoles[k].m_TrackableElementReporter != null)
						{
							m_OldProximityElements[i] = m_ListOfCurrentProximityHoles[k].m_TrackableElementReporter.GetUITrackedElement(m_PlayerCameraManagerBindingID);
							if (m_OldProximityElements[i] != null)
							{
								m_MyTrackedUIElements.ReleaseTrackedUIElementWithoutDisable(m_OldProximityElements[i]);
								i++;
							}
						}
					}
					for (int l = 0; l < m_ListOfCurrentProximityHolesAbove.Length; l++)
					{
						if (m_ListOfCurrentProximityHolesAbove[l] != null && m_ListOfCurrentProximityHolesAbove[l].m_TrackableElementReporter != null)
						{
							m_OldProximityElements[i] = m_ListOfCurrentProximityHolesAbove[l].m_TrackableElementReporter.GetUITrackedElement(m_PlayerCameraManagerBindingID);
							if (m_OldProximityElements[i] != null)
							{
								m_OldProximityElements[i].SetAttachedPositionOffset(0f, 0f);
								m_MyTrackedUIElements.ReleaseTrackedUIElementWithoutDisable(m_OldProximityElements[i]);
								i++;
							}
						}
					}
					for (int m = 0; m < m_ListOfCurrentProximityTiles.Length; m++)
					{
						if (m_ListOfCurrentProximityTiles[m] != null && m_ListOfCurrentProximityTiles[m].m_TrackableElementReporter != null)
						{
							m_OldProximityElements[i] = m_ListOfCurrentProximityTiles[m].m_TrackableElementReporter.GetUITrackedElement(m_PlayerCameraManagerBindingID);
							if (m_OldProximityElements[i] != null)
							{
								m_OldProximityElements[i].SetAttachedPositionOffset(0f, 0f);
								m_MyTrackedUIElements.ReleaseTrackedUIElementWithoutDisable(m_OldProximityElements[i]);
								i++;
							}
						}
					}
					for (; i < 7; i++)
					{
						m_OldProximityElements[i] = null;
					}
					int num = 0;
					m_NearestInteractiveObject = null;
					m_NearestCharacter = null;
					m_NearestItem = null;
					m_NearestDamagableTile = null;
					bool flag2 = IsInteracting();
					if (!flag2 && m_ProximityDetector != null)
					{
						m_ProximityDetector.GetAnyInteractiveObjects(ref m_CurrentNearbyInteractiveObjects);
						m_ProximityDetector.GetAnyCharacters(ref m_CurrentNearbyCharacters);
						m_ProximityDetector.GetAnyItems(ref m_CurrentNearbyItems);
						for (int num2 = m_CurrentNearbyInteractiveObjects.Count - 1; num2 >= 0; num2--)
						{
							Character localOrNetworkSycnedCharacter = m_CurrentNearbyInteractiveObjects[num2].GetLocalOrNetworkSycnedCharacter();
							NetObjectLock netObjectLock = ((!(localOrNetworkSycnedCharacter != null) || !(localOrNetworkSycnedCharacter.GetNetObjectLock() != null)) ? null : localOrNetworkSycnedCharacter.GetNetObjectLock());
							if (netObjectLock != null)
							{
								m_CurrentNearbyInteractiveObjects[num2] = null;
								if (!m_CurrentNearbyInteractiveObjects.Contains(netObjectLock))
								{
									m_CurrentNearbyInteractiveObjects.Add(netObjectLock);
								}
							}
						}
						if (m_MouseDetector != null)
						{
							m_MouseDetector.GetCurrentInteractiveObject(ref m_MouseOverInteractiveObject);
							m_MouseDetector.GetCurrentCharacter(ref m_MouseOverCharacter);
							m_MouseDetector.GetCurrentItem(ref m_MouseOverItem);
							if (m_MouseOverCharacter != null && m_MouseOverCharacter.CharacterOwner != null && m_MouseOverCharacter.CharacterOwner.m_CharacterRole == CharacterRole.Ghost)
							{
								m_MouseOverCharacter = null;
							}
						}
					}
					m_TrackedElementReporters.Clear();
					if (!flag2 && !base.m_bIsKnockedOut && !m_bCharacterTargettingEnabled && !base.m_bIsStandingOnDesk)
					{
						if (!HasValidTransitionInteraction())
						{
							for (int n = 0; n < m_CurrentNearbyInteractiveObjects.Count; n++)
							{
								if (m_CurrentNearbyInteractiveObjects[n] != null && m_CurrentNearbyInteractiveObjects[n].IsVisibleToProximityDetector())
								{
									m_TrackedElementReporters.Add(m_CurrentNearbyInteractiveObjects[n].m_TrackableElementReporter);
								}
							}
							for (int num3 = 0; num3 < m_CurrentNearbyCharacters.Count; num3++)
							{
								if (m_CurrentNearbyCharacters[num3] != null && (!(m_CurrentNearbyCharacters[num3].CharacterOwner != null) || m_CurrentNearbyCharacters[num3].CharacterOwner.m_CharacterRole != CharacterRole.Ghost))
								{
									m_TrackedElementReporters.Add(m_CurrentNearbyCharacters[num3]);
								}
							}
						}
						for (int num4 = 0; num4 < m_CurrentNearbyItems.Count; num4++)
						{
							if (m_CurrentNearbyItems[num4].TrackableUIElementReporter != null)
							{
								m_TrackedElementReporters.Add(m_CurrentNearbyItems[num4].TrackableUIElementReporter);
							}
						}
						m_FarAwayReporters.Clear();
						if (m_MouseOverItem != null && m_MouseOverItem.TrackableUIElementReporter != null)
						{
							if (!m_TrackedElementReporters.Contains(m_MouseOverItem.TrackableUIElementReporter))
							{
								m_FarAwayReporters.Add(m_MouseOverItem.TrackableUIElementReporter);
							}
							m_TrackedElementReporters.Add(m_MouseOverItem.TrackableUIElementReporter);
						}
						if (m_MouseOverInteractiveObject != null && m_MouseOverInteractiveObject.m_TrackableElementReporter != null && m_MouseOverInteractiveObject.IsVisibleToProximityDetector())
						{
							if (!m_TrackedElementReporters.Contains(m_MouseOverInteractiveObject.m_TrackableElementReporter))
							{
								m_FarAwayReporters.Add(m_MouseOverInteractiveObject.m_TrackableElementReporter);
							}
							m_TrackedElementReporters.Add(m_MouseOverInteractiveObject.m_TrackableElementReporter);
						}
						if (m_MouseOverCharacter != null && m_MouseOverCharacter.CharacterOwner != this)
						{
							if (!m_TrackedElementReporters.Contains(m_MouseOverCharacter))
							{
								m_FarAwayReporters.Add(m_MouseOverCharacter);
							}
							m_TrackedElementReporters.Add(m_MouseOverCharacter);
						}
					}
					if (m_ProximityDetector != null)
					{
						m_ProximityDetector.SortListByDirection(m_TrackedElementReporters, base.transform.position, GetFacingDirection());
					}
					int num5 = Mathf.Min(m_CurrentListOfPrioritySortedTrackedElements.Length, m_TrackedElementReporters.Count);
					for (int num6 = 0; num6 < num5; num6++)
					{
						m_CurrentListOfPrioritySortedTrackedElements[num6] = m_TrackedElementReporters[num6];
					}
					for (int num7 = num5; num7 < m_CurrentListOfPrioritySortedTrackedElements.Length; num7++)
					{
						m_CurrentListOfPrioritySortedTrackedElements[num7] = null;
					}
					if (m_NearestInteractiveObject == null && !base.m_bIsStandingOnDesk)
					{
						for (int num8 = 0; num8 < m_CurrentNearbyInteractiveObjects.Count; num8++)
						{
							if (m_CurrentNearbyInteractiveObjects[num8] != null && m_CurrentNearbyInteractiveObjects[num8].m_TrackableElementReporter != null && m_CurrentNearbyInteractiveObjects[num8].IsVisibleToProximityDetector() && m_CurrentNearbyInteractiveObjects[num8].m_TrackableElementReporter == m_CurrentListOfPrioritySortedTrackedElements[0])
							{
								m_NearestInteractiveObject = m_CurrentNearbyInteractiveObjects[num8];
								break;
							}
						}
						ProcessTrackedElementTutorials_Interactive(null, m_NearestInteractiveObject);
					}
					if (m_NearestInteractiveObject == null && !base.m_bIsStandingOnDesk)
					{
						for (int num9 = 0; num9 < m_CurrentNearbyCharacters.Count; num9++)
						{
							if (m_CurrentNearbyCharacters[num9] != null && m_CurrentNearbyCharacters[num9] == m_CurrentListOfPrioritySortedTrackedElements[0])
							{
								m_NearestCharacter = m_CurrentNearbyCharacters[num9];
								break;
							}
						}
					}
					if (m_NearestInteractiveObject == null && m_NearestCharacter == null && !base.m_bIsStandingOnDesk)
					{
						Item nearestItem = m_NearestItem;
						for (int num10 = 0; num10 < m_CurrentNearbyItems.Count; num10++)
						{
							if (m_CurrentNearbyItems[num10] != null && m_CurrentNearbyItems[num10].TrackableUIElementReporter != null && m_CurrentNearbyItems[num10].TrackableUIElementReporter == m_CurrentListOfPrioritySortedTrackedElements[0])
							{
								m_NearestItem = m_CurrentNearbyItems[num10];
								break;
							}
						}
						ProcessTrackedElementTutorials_Items(nearestItem, m_NearestItem);
					}
					int num11 = 0;
					int num12 = 0;
					int num13 = 0;
					if (m_ProximityDetector != null)
					{
						num11 = m_ProximityDetector.GetSortedNearestHoles(ref m_ListOfCurrentProximityHoles, includeFullyDug: false);
						num12 = m_ProximityDetector.GetSortedNearestHoles_FromUnderground(ref m_ListOfCurrentProximityHolesAbove, includeFullyDug: false);
						num13 = m_ProximityDetector.GetSortedNearestDamagedTiles(ref m_ListOfCurrentProximityTiles);
					}
					bool flag3 = false;
					int num14 = 0;
					while (num14 < num5 && num < 7)
					{
						if (m_CurrentListOfPrioritySortedTrackedElements[num14].CharacterOwner == null || (m_CurrentListOfPrioritySortedTrackedElements[num14].CharacterOwner != null && !m_CurrentListOfPrioritySortedTrackedElements[num14].CharacterOwner.GetIsDisabled()))
						{
							flag3 = m_FarAwayReporters.Contains(m_CurrentListOfPrioritySortedTrackedElements[num14]);
							T17TrackedUIElement element = AttachUnusedElementToReporter(m_CurrentListOfPrioritySortedTrackedElements[num14], num, flag3);
							SetupFadingNameplateForReporter(element, m_CurrentListOfPrioritySortedTrackedElements[num14]);
						}
						num14++;
						num++;
					}
					int num15 = 0;
					while (num15 < num11 && num < 7)
					{
						flag3 = m_FarAwayReporters.Contains(m_ListOfCurrentProximityHoles[num15].m_TrackableElementReporter);
						AttachUnusedElementToReporter(m_ListOfCurrentProximityHoles[num15].m_TrackableElementReporter, num + 1, flag3);
						num15++;
						num++;
					}
					int num16 = 0;
					while (num16 < num12 && num < 7)
					{
						flag3 = m_FarAwayReporters.Contains(m_ListOfCurrentProximityHolesAbove[num16].m_TrackableElementReporter);
						T17TrackedUIElement t17TrackedUIElement = AttachUnusedElementToReporter(m_ListOfCurrentProximityHolesAbove[num16].m_TrackableElementReporter, num + 1, flag3);
						t17TrackedUIElement.SetAttachedPositionOffset(0f, -1f);
						num16++;
						num++;
					}
					int num17 = 0;
					while (num17 < num13 && num < 7)
					{
						if (m_ListOfCurrentProximityTiles[num17].m_DamageAction == DamagableTile.DamageAction.Hole)
						{
							num--;
						}
						else
						{
							flag3 = m_FarAwayReporters.Contains(m_ListOfCurrentProximityTiles[num17].m_TrackableElementReporter);
							T17TrackedUIElement t17TrackedUIElement2 = AttachUnusedElementToReporter(m_ListOfCurrentProximityTiles[num17].m_TrackableElementReporter, num + 1, flag3);
							if (m_ListOfCurrentProximityTiles[num17].CurrentFloor.m_FloorIndex == base.CurrentFloor.m_FloorIndex + 1)
							{
								t17TrackedUIElement2.SetAttachedPositionOffset(0f, -1f);
							}
						}
						num17++;
						num++;
					}
					m_NearestDamagableTile = null;
					if (num13 > 0)
					{
						m_NearestDamagableTile = m_ListOfCurrentProximityTiles[0];
					}
					for (int num18 = 0; num18 < 7; num18++)
					{
						if (!(m_OldProximityElements[num18] != null) || (!(m_OldProximityElements[num18].AttachedTo == null) && m_OldProximityElements[num18].Flags != 0))
						{
							continue;
						}
						TrackableUIElementsReporter trackableUIElementsReporter = null;
						if (m_OldProximityElements[num18].Flags != 0)
						{
							m_OldProximityElements[num18].BeginFadeOut();
							trackableUIElementsReporter = m_OldProximityElements[num18].GhostedTo;
						}
						if (trackableUIElementsReporter != null)
						{
							m_MyTrackedUIElements.AddGhostElement(m_OldProximityElements[num18]);
							continue;
						}
						if (m_OldProximityElements[num18].IsNameplateActive())
						{
							m_OldProximityElements[num18].DisableNamePlate();
						}
						if (m_OldProximityElements[num18].HasFlag(4u))
						{
							m_OldProximityElements[num18].DisableIcon();
						}
						m_OldProximityElements[num18].ResetAll();
						m_OldProximityElements[num18].gameObject.SetActive(value: false);
					}
					m_RemovedReporters.Clear();
					m_RemovedReporters.AddRange(m_ActiveReportersWithElementsInProximity.Keys);
					for (int num19 = m_RemovedReporters.Count - 1; num19 >= 0; num19--)
					{
						if (!m_TrackedElementReporters.Contains(m_RemovedReporters[num19]))
						{
							m_ActiveReportersWithElementsInProximity.Remove(m_RemovedReporters[num19]);
						}
					}
					for (int num20 = 0; num20 < num5; num20++)
					{
						T17TrackedUIElement uITrackedElement = m_CurrentListOfPrioritySortedTrackedElements[num20].GetUITrackedElement(m_PlayerCameraManagerBindingID);
						if (!(uITrackedElement != null))
						{
							continue;
						}
						T17TrackedUIElement.NameplateStyle nameplateStyle = T17TrackedUIElement.NameplateStyle.Default;
						if (uITrackedElement.AttachedTo != null && uITrackedElement.AttachedTo.CharacterOwner != null && !uITrackedElement.AttachedTo.CharacterOwner.m_CharacterStats.m_bIsPlayer && (uITrackedElement.AttachedTo.CharacterOwner.m_CharacterRole == CharacterRole.Inmate || uITrackedElement.AttachedTo.CharacterOwner.m_CharacterRole == CharacterRole.Guard))
						{
							int opinionOf = uITrackedElement.AttachedTo.CharacterOwner.GetOpinionOf(this);
							if (opinionOf > OpinionManager.GetInstance().GetHighOpinionThreshold())
							{
								nameplateStyle = T17TrackedUIElement.NameplateStyle.Positive;
							}
							else if (opinionOf < OpinionManager.GetInstance().GetLowOpinionThreshold())
							{
								nameplateStyle = T17TrackedUIElement.NameplateStyle.Negative;
							}
						}
						if (uITrackedElement.GetNameplateStyle() != nameplateStyle)
						{
							uITrackedElement.SetNameplateStyle(nameplateStyle);
						}
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				m_ElapsedProximityTime = 0f;
			}
		}
		if (m_MyTrackedUIElements != null && !m_bNeedsToUpdateCharacterTarget)
		{
			if (base.m_CharacterTarget == null)
			{
				m_bCharacterTargettingEnabled = false;
			}
			else
			{
				int cameraID = CameraManager.GetInstance().GetCameraID(m_PlayerCameraManagerBindingID);
				if (!CullingObjectCollector.GetInstance().IsCharacterVisibleToCamera(cameraID, base.m_CharacterTarget))
				{
					m_TargetOffscreenTimer -= UpdateManager.deltaTime;
				}
				else
				{
					m_TargetOffscreenTimer = m_TargetOffscreenLength;
				}
				if (base.m_CharacterTarget.m_bIsKnockedOut || m_TargetOffscreenTimer < 0f)
				{
					DisableTargetElement();
				}
			}
			if (m_bCharacterTargettingEnabled)
			{
				m_MyTrackedUIElements.ShowCombatTarget(base.m_CharacterTarget.m_Transform.position, isFriendly: true);
				m_MyTrackedUIElements.ShowCombatTargetHealth(base.m_CharacterTarget);
			}
			else
			{
				m_MyTrackedUIElements.HideCombatTarget();
				m_MyTrackedUIElements.HideCombatTargetHealth();
			}
			float time = UpdateManager.time;
			for (int num21 = m_RecentlyHitCharacters.Count - 1; num21 >= 0; num21--)
			{
				if (!(m_RecentlyHitCharacters[num21] == null))
				{
					if (m_RecentlyHitCharacters[num21] == base.m_CharacterTarget)
					{
						m_MyTrackedUIElements.HideCharacterHealth(base.m_CharacterTarget);
					}
					else if (!m_RecentlyHitCharacters[num21].m_bIsKnockedOut && time - m_RecentlyHitCharacters[num21].GetTimeLastHit() < m_IncidentalDisplayHealthTime)
					{
						if (m_RecentlyHitCharacters[num21].CurrentFloor == m_CurrentFloor)
						{
							m_MyTrackedUIElements.ShowCharacterHealth(m_RecentlyHitCharacters[num21]);
						}
						else
						{
							m_MyTrackedUIElements.HideCharacterHealth(m_RecentlyHitCharacters[num21]);
						}
					}
					else
					{
						m_MyTrackedUIElements.HideCharacterHealth(m_RecentlyHitCharacters[num21]);
						m_RecentlyHitCharacters.RemoveAt(num21);
					}
				}
			}
			if (m_bCharacterTargettingEnabled || time - GetTimeLastHit() < m_IncidentalDisplayHealthTime)
			{
				m_MyTrackedUIElements.ShowPlayerHealth(this);
			}
			else
			{
				m_MyTrackedUIElements.HidePlayerHealth();
			}
		}
		m_ElapsedProximityTime += UpdateManager.deltaTime;
		return flag;
	}

	private void DisableTargetElement()
	{
		ResetCharacterTargetElement();
		SetCharacterTarget(null);
		m_bCharacterTargettingEnabled = false;
	}

	private void SetupFadingNameplateForReporter(T17TrackedUIElement element, TrackableUIElementsReporter reporter)
	{
		if (!(reporter != null) || !(element != null))
		{
			return;
		}
		if (!m_ActiveReportersWithElementsInProximity.ContainsKey(reporter))
		{
			m_ActiveReportersWithElementsInProximity.Add(reporter, UpdateManager.time);
			element.StopActiveFading();
			element.SetAlphaTo(1f);
			return;
		}
		float num = UpdateManager.time - m_ActiveReportersWithElementsInProximity[reporter] - m_TimeUntilNameplatesFade;
		if (num > 0f)
		{
			if (num > 60f)
			{
				m_ActiveReportersWithElementsInProximity[reporter] = UpdateManager.time;
				element.SetAlphaTo(1f);
			}
			else
			{
				m_ReusableFloat = 1f - num / m_NameplatesFadeTime;
				element.SetAlphaTo(m_ReusableFloat);
				element.StartActiveFading(m_NameplatesFadeTime);
			}
		}
	}

	private T17TrackedUIElement AttachUnusedElementToReporter(TrackableUIElementsReporter reporter, int priority, bool isFarAway)
	{
		T17TrackedUIElement t17TrackedUIElement = m_MyTrackedUIElements.AttachFirstUnusedElementToReporer(reporter, priority, isFarAway, attemptToFindHistoricallyAssignedElement: true);
		if (t17TrackedUIElement != null)
		{
			bool flag = (isFarAway || reporter.ShouldShowNearbyNameplateToPlayer()) && !t17TrackedUIElement.IsNameplateActive();
			reporter.SetDisplayFlagsForElement(t17TrackedUIElement, isFarAway);
			m_HideCharacterIconCoroutineElement = t17TrackedUIElement;
			m_HideCharacterIconCoroutineReporter = reporter;
			if (flag)
			{
				StartCoroutine(HideCharacterIconwhenNameplateAppear());
			}
		}
		return t17TrackedUIElement;
	}

	private IEnumerator HideCharacterIconwhenNameplateAppear()
	{
		Character lastOwner = null;
		if (m_HideCharacterIconCoroutineReporter.CharacterOwner != null && m_HideCharacterIconCoroutineReporter.CharacterOwner.m_IconHandler != null)
		{
			m_HideCharacterIconCoroutineReporter.CharacterOwner.m_IconHandler.HideCharacterIcon(hide: true);
			lastOwner = m_HideCharacterIconCoroutineReporter.CharacterOwner;
		}
		if (lastOwner == null || lastOwner.m_IconHandler == null)
		{
			yield break;
		}
		do
		{
			yield return m_WaitForHideIcon;
			if (m_HideCharacterIconCoroutineReporter.CharacterOwner == null && lastOwner != null)
			{
				lastOwner.m_IconHandler.HideCharacterIcon(hide: false);
				lastOwner = null;
			}
			else if (m_HideCharacterIconCoroutineReporter.CharacterOwner != lastOwner)
			{
				if (lastOwner != null)
				{
					lastOwner.m_IconHandler.HideCharacterIcon(hide: false);
				}
				lastOwner = m_HideCharacterIconCoroutineReporter.CharacterOwner;
				lastOwner.m_IconHandler.HideCharacterIcon(hide: true);
			}
		}
		while (m_HideCharacterIconCoroutineElement.IsNameplateActive());
		if (lastOwner != null)
		{
			lastOwner.m_IconHandler.HideCharacterIcon(hide: false);
		}
	}

	private bool ProcessTrackedElementTutorials_Interactive(NetObjectLock prevNearestObject, NetObjectLock newNearestObject)
	{
		TutorialManager instance = TutorialManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (prevNearestObject == null && newNearestObject != null && !base.m_bIsStandingOnDesk)
		{
			Character characterOwner = newNearestObject.m_TrackableElementReporter.CharacterOwner;
			if (characterOwner != null && characterOwner.m_bIsKnockedOut)
			{
				instance.StartTutorialRPC(this, TutorialSubject.PickUpLoot);
			}
			RoutinesData.Routine currentRoutine = RoutineManager.GetInstance().GetCurrentRoutine();
			if (currentRoutine != null && currentRoutine.m_SubRoutineType != RoutineSubTypes.MorningRollCall && currentRoutine.m_SubRoutineType != RoutineSubTypes.BreakfastTime && currentRoutine.m_SubRoutineType != RoutineSubTypes.LightsOut && TutorialManager.GetInstance().CheckTutorialNeeded(this, TutorialSubject.Desks) && newNearestObject.HasInteractionOfType<DeskInteraction>())
			{
				instance.StartTutorialRPC(this, TutorialSubject.Desks);
			}
		}
		return true;
	}

	private bool ProcessTrackedElementTutorials_Items(Item prevNearest, Item newNearest)
	{
		TutorialManager instance = TutorialManager.GetInstance();
		if (instance == null)
		{
			return false;
		}
		if (prevNearest == null && newNearest != null)
		{
			instance.StartTutorialRPC(this, TutorialSubject.TakeItem);
		}
		return true;
	}

	public override void OnInteractionStart()
	{
		base.OnInteractionStart();
		m_bCollisionDisabled_Interacting = true;
	}

	public override void OnInteractionExit()
	{
		base.OnInteractionExit();
		m_bCollisionDisabled_Interacting = false;
		DoorManager instance = DoorManager.GetInstance();
		if (!m_bIsBeingDestroyed && instance != null)
		{
			instance.SetUpCharacterKeys(this);
		}
	}

	public void IncreaseActiveQuests()
	{
		m_ActiveQuests++;
	}

	public void DecreaseActiveQuests()
	{
		m_ActiveQuests--;
		if (m_ActiveQuests >= 0)
		{
		}
	}

	private bool IsCorrectGamer()
	{
		return m_Gamer != null && m_NetView.viewID == m_Gamer.m_NetViewID;
	}

	public void SetBrowsingHUDMenu(bool browsing)
	{
		m_bBrowsingHUDMenu = browsing;
	}

	public void SetBrowsingPauseMenu(bool browsing)
	{
		m_bBrowsingPauseMenu = browsing;
	}

	public void ShowBrowsingMenusRPC(bool browsing)
	{
		m_NetView.GameplayRPC("RPC_ShowBrowsingMenus", NetTargets.All, browsing);
	}

	[PunRPC]
	private void RPC_ShowBrowsingMenus(bool browsing, PhotonMessageInfo info)
	{
		ConfigManager instance = ConfigManager.GetInstance();
		if (!(instance != null) || instance.gameType != PrisonConfig.ConfigType.Versus)
		{
			if (browsing)
			{
				m_IconHandler.DisplayIcon(CharacterIconHandler.IconType.InMenus);
			}
			else
			{
				m_IconHandler.RemoveIcon(CharacterIconHandler.IconType.InMenus);
			}
		}
	}

	public void CloseInventory()
	{
		m_bPendingRequest = false;
		InGameMenuFlow.Instance.HideMenu(this, m_PlayerCameraManagerBindingID);
		InGameMenuFlow.Instance.CleanupInventory(m_ItemContainer, m_OpenContainer, this, m_PlayerCameraManagerBindingID);
		ShowBrowsingMenusRPC(browsing: false);
		m_bCheckPrimaryInteractionFromInventoryClose = true;
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.OpenPlayerHUD(this, m_PlayerCameraManagerBindingID);
		}
		if (T17RewiredStandaloneInputModule.UsedSharedKeyboardAction("UI_Submit", "Use", m_Gamer.m_RewiredPlayer) && m_Gamer.m_RewiredPlayer.GetButton("UI_Submit"))
		{
			m_bUseKeyIsDown = true;
		}
		m_bCloseInventoryOnPauseMenuHide = false;
	}

	private IEnumerator DelayedOpenPlayerHUD()
	{
		yield return new WaitForEndOfFrame();
		if (HUDMenuFlow.Instance != null)
		{
			HUDMenuFlow.Instance.OpenPlayerHUD(this, m_PlayerCameraManagerBindingID);
		}
	}

	public bool ViewContainer(ItemContainer itemContainer, InGameRootMenu.InGameMenuTypeToOpen menuType = InGameRootMenu.InGameMenuTypeToOpen.MainSelf, int iTabIndex = 0)
	{
		if (!InGameMenuFlow.Instance.HasMenusToOpen(menuType, m_PlayerCameraManagerBindingID))
		{
			return false;
		}
		if (!InGameMenuFlow.Instance.AnyMenusOpen(m_PlayerCameraManagerBindingID))
		{
			m_OpenContainer = itemContainer;
			InGameMenuFlow.Instance.PrepareMenuSetToOpen(menuType, m_PlayerCameraManagerBindingID);
			InGameMenuFlow.Instance.SetUpInventory(m_ItemContainer, m_OpenContainer, this, m_PlayerCameraManagerBindingID, menuType);
			InGameMenuFlow.Instance.OpenMenu(menuType, this, m_PlayerCameraManagerBindingID);
			ShowBrowsingMenusRPC(browsing: true);
			if (HUDMenuFlow.Instance != null)
			{
				HUDMenuFlow.Instance.HideMenu(m_PlayerCameraManagerBindingID);
			}
			InGameMenuFlow.PlayerIGMData data = null;
			InGameMenuFlow.Instance.GetCorrectIGMData(m_PlayerCameraManagerBindingID, out data);
			if (data != null && data.m_PlayerRootMenu.m_CurrentInGameMenuType == InGameRootMenu.InGameMenuTypeToOpen.MainSelf)
			{
				data.m_PlayerRootMenu.m_MainTabPanel.AttemptToSetTabIndex(iTabIndex);
			}
			return true;
		}
		return false;
	}

	public void OnContainerViewed(ItemContainer container)
	{
		if (m_MapItemTracker != null)
		{
			m_MapItemTracker.OnContainerViewed(container);
		}
	}

	public void OnContainerClosed(ItemContainer container)
	{
		if (m_MapItemTracker != null)
		{
			m_MapItemTracker.OnContainerClosed(container);
		}
	}

	protected override void RoomChanged(RoomBlob previousRoom, RoomBlob newRoom)
	{
		base.RoomChanged(previousRoom, newRoom);
		if (m_NetView == null || !m_NetView.isMine)
		{
			return;
		}
		if (newRoom != null)
		{
			if (newRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Outdoors && (previousRoom == null || previousRoom.m_subLocation != RoomBlob.RoomSubIdentity_Location.Outdoors))
			{
				AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Outside_Building.ToString(), AudioController.InGameMusicAndAmbienceObject);
				AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.Outside_Building.ToString());
			}
			else if (newRoom.location == RoomBlob.eLocation.Shower && (previousRoom == null || previousRoom.location != RoomBlob.eLocation.Shower))
			{
				AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Inside_Showers.ToString(), AudioController.InGameMusicAndAmbienceObject);
				AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.Inside_Building.ToString());
			}
			else if (newRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors && (previousRoom == null || previousRoom.m_subLocation == RoomBlob.RoomSubIdentity_Location.Outdoors || previousRoom.location == RoomBlob.eLocation.Shower))
			{
				SetMusicStatesBasedOnFloor(m_CurrentFloor.m_FloorType);
			}
		}
		if ((newRoom != null && newRoom.location == RoomBlob.eLocation.InmateCell) || (previousRoom != null && previousRoom.location == RoomBlob.eLocation.InmateCell))
		{
			int accessKeyCode = 0;
			bool accessKeyEnabled = true;
			Routines currentRoutineBaseType = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
			bool flag = currentRoutineBaseType == Routines.LightsOut || currentRoutineBaseType == Routines.Lockdown;
			if (previousRoom != null && previousRoom.location == RoomBlob.eLocation.InmateCell)
			{
				accessKeyCode = (flag ? m_CellDoorCode : 0);
				accessKeyEnabled = true;
			}
			if (newRoom != null && newRoom.location == RoomBlob.eLocation.InmateCell)
			{
				accessKeyCode = 0;
				bool flag2 = true;
				if (flag && newRoom == GetMyCell())
				{
					flag2 = false;
					if (currentRoutineBaseType == Routines.LightsOut && ConfigManager.GetInstance() != null && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
					{
						flag2 = true;
					}
				}
				accessKeyEnabled = flag2;
			}
			SetAccessKeyCode(accessKeyCode);
			SetAccessKeyEnabled(accessKeyEnabled);
		}
		TutorialManager.GetInstance().CheckRoomTutorials(this, previousRoom, newRoom);
	}

	protected override void RoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine routine, bool forced)
	{
		base.RoutineChanged(oldRoutine, routine, forced);
		if (m_NetView == null || !m_NetView.isMine)
		{
			return;
		}
		bool flag = (routine != null && routine.m_BaseRoutineType == Routines.LightsOut) || (routine != null && routine.m_BaseRoutineType == Routines.Lockdown);
		bool flag2 = (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.LightsOut) || (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.Lockdown);
		if (!flag && !flag2)
		{
			return;
		}
		int accessKeyCode = 0;
		bool accessKeyEnabled = true;
		if (flag2 && !flag)
		{
			accessKeyCode = 0;
			accessKeyEnabled = true;
		}
		if (flag)
		{
			bool flag3 = base.m_CurrentLocation == null || base.m_CurrentLocation != GetMyCell();
			if (routine.m_BaseRoutineType == Routines.LightsOut && ConfigManager.GetInstance() != null && ConfigManager.GetInstance().gameType == PrisonConfig.ConfigType.Versus)
			{
				flag3 = true;
				accessKeyCode = 0;
			}
			else
			{
				accessKeyCode = ((!(base.m_CurrentLocation == null) && base.m_CurrentLocation.location != RoomBlob.eLocation.InmateCell) ? m_CellDoorCode : 0);
			}
			accessKeyEnabled = flag3;
		}
		SetAccessKeyCode(accessKeyCode);
		SetAccessKeyEnabled(accessKeyEnabled);
	}

	public override bool Teleport(Vector3 newPosition)
	{
		int floorIndex = base.CurrentFloor.m_FloorIndex;
		if (FloorManager.GetInstance() == null)
		{
			return false;
		}
		FloorManager.Floor floor = FloorManager.GetInstance().FindFloorAtZ(newPosition.z);
		if (floor == null)
		{
			return false;
		}
		if (base.CurrentFloor != floor)
		{
			SetMusicStatesBasedOnFloor(floor.m_FloorType);
		}
		bool result = Teleport(newPosition, floor);
		if (floorIndex < m_CurrentFloor.m_FloorIndex)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Floor_Up, base.gameObject);
		}
		else if (floorIndex > m_CurrentFloor.m_FloorIndex)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Floor_Down, base.gameObject);
		}
		return result;
	}

	protected override void OnFloorChange(int oldFloorIndex)
	{
		base.OnFloorChange(oldFloorIndex);
		if (m_MyTrackedUIElements != null)
		{
			m_MyTrackedUIElements.SetBaseElementDepth((float)base.CurrentFloor.m_zPos + HUDMenuFlow.WorldOffsetZ);
			m_MyTrackedUIElements.EnableTrackers();
		}
		WorldCanvasTrackedUIElements uIElementsWorldCanvas = HUDMenuFlow.Instance.GetUIElementsWorldCanvas(oldFloorIndex);
		if (uIElementsWorldCanvas != null)
		{
			m_bEnabledWorldNamePlates = false;
			uIElementsWorldCanvas.DisableAllNameplatesForCamera(m_PlayerCameraManagerBindingID);
		}
		HandleRoutineArrow();
		HandleObjectiveArrow();
	}

	protected override void OnStandingOnDeskChange(bool bIsOnDesk)
	{
		base.OnStandingOnDeskChange(bIsOnDesk);
		if (m_MyTrackedUIElements != null)
		{
			float num = ((!bIsOnDesk) ? base.CurrentFloor.m_zPos : FloorManager.GetInstance().UpAFloor(base.CurrentFloor).m_zPos);
			m_MyTrackedUIElements.SetBaseElementDepth(num + HUDMenuFlow.WorldOffsetZ);
			m_MyTrackedUIElements.HidePressAndHold();
		}
		m_bPrimaryProcessed = true;
		m_PrimaryTimePressed = 0f;
		m_bSecondaryProcessed = true;
		m_SecondaryTimePressed = 0f;
		m_bTertiaryProcessed = true;
		m_TertiaryTimePressed = 0f;
	}

	public void SetMusicStatesBasedOnFloor(FloorManager.FLOOR_TYPE floortype)
	{
		switch (floortype)
		{
		case FloorManager.FLOOR_TYPE.Floor_Roof:
			AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.On_Roof.ToString());
			AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.On_Roof.ToString(), AudioController.InGameMusicAndAmbienceObject);
			break;
		case FloorManager.FLOOR_TYPE.Floor_Vent:
			AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.Inside_Building.ToString());
			AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Inside_Vent.ToString(), AudioController.InGameMusicAndAmbienceObject);
			break;
		case FloorManager.FLOOR_TYPE.Floor_Prison:
			AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.Inside_Building.ToString());
			AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Inside_Building.ToString(), AudioController.InGameMusicAndAmbienceObject);
			break;
		case FloorManager.FLOOR_TYPE.Floor_UnderGround:
			AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.Underground.ToString());
			AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Underground.ToString(), AudioController.InGameMusicAndAmbienceObject);
			break;
		default:
			AudioController.SetState(State_Group.Music_Player_Position, Music_Player_Position.None.ToString());
			AudioController.SetSwitch(Switch_Group.Player_Amb_Position, Player_Amb_Position.Inside_Building.ToString(), AudioController.InGameMusicAndAmbienceObject);
			break;
		}
	}

	public FloorManager.Floor GetCurrentFloor()
	{
		return m_CurrentFloor;
	}

	public int GetCurrentTileRow()
	{
		return m_CurrentTileRow;
	}

	public int GetCurrentTileColumn()
	{
		return m_CurrentTileColumn;
	}

	public PerPlayerTrackedUIElements GetMyTrackedUIElements()
	{
		return m_MyTrackedUIElements;
	}

	public static void RegisterCharacter(Character character)
	{
		if (m_AllCharacters == null)
		{
			m_AllCharacters = new List<Character>(50);
		}
		m_AllCharacters.Add(character);
	}

	public static void UnRegisterCharacter(Character character)
	{
		if (m_AllCharacters == null)
		{
			m_AllCharacters = new List<Character>(50);
		}
		m_AllCharacters.Remove(character);
	}

	public int GetRandomHintIndex()
	{
		int totalHintCount = GlobalHintManager.GetInstance().GetTotalHintCount(LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
		int num = UnityEngine.Random.Range(0, totalHintCount);
		int num2 = num;
		do
		{
			if (num2 >= 64)
			{
				num2 = 0;
			}
			if ((m_HintBitfield & (1 << num2)) <= 0)
			{
				return num2;
			}
			num2++;
		}
		while (num2 != num - 1);
		return -1;
	}

	public int GetRandomHintIndex(int[] indexesToIgnore)
	{
		int totalHintCount = GlobalHintManager.GetInstance().GetTotalHintCount(LevelScript.GetCurrentLevelInfo().m_PrisonEnum);
		int num = UnityEngine.Random.Range(0, totalHintCount);
		int num2 = num;
		do
		{
			if (num2 >= totalHintCount)
			{
				num2 = 0;
			}
			if ((m_HintBitfield & (1 << num2)) <= 0 && !CheckIfIndexShouldBeIgnored(num2, indexesToIgnore))
			{
				return num2;
			}
			num2++;
		}
		while (num2 != num - 1);
		return -1;
	}

	private bool CheckIfIndexShouldBeIgnored(int indexToTest, int[] indexesToIgnore)
	{
		for (int i = 0; i < indexesToIgnore.Length; i++)
		{
			if (indexToTest == indexesToIgnore[i])
			{
				return true;
			}
		}
		return false;
	}

	public void SetHintIndexAsFound(int index, bool found = true)
	{
		if (index >= 0 || index < GlobalHintManager.GetInstance().GetTotalHintCount(LevelScript.GetCurrentLevelInfo().m_PrisonEnum))
		{
			if (found)
			{
				m_HintBitfield |= 1L << index;
			}
			else
			{
				m_HintBitfield &= 1L << index;
			}
			GlobalHintManager.GetInstance().SetHintBitfield(m_Gamer, LevelScript.GetCurrentLevelInfo().m_PrisonEnum, m_HintBitfield);
		}
	}

	public bool IsHintFound(int indexToTest)
	{
		if (indexToTest >= 0 && indexToTest < GlobalHintManager.GetInstance().GetTotalHintCount(LevelScript.GetCurrentLevelInfo().m_PrisonEnum) && (m_HintBitfield & (1L << indexToTest)) <= 0)
		{
			return false;
		}
		return true;
	}

	public override bool SetEquippedItem(Item equipedItem, bool bTellOthers = true, bool bAddOldToItemContainer = true, RPC_CallContexts callContext = RPC_CallContexts.Unknown)
	{
		if (equipedItem != null && m_CharacterStats.m_bIsPlayer)
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null && instance.ItemFunctionalityCheck(equipedItem))
			{
				instance.StartTutorialRPC(this, TutorialSubject.UseItem);
			}
		}
		bool result = base.SetEquippedItem(equipedItem, bTellOthers, bAddOldToItemContainer, callContext);
		if (m_PlayerInventoryHUD != null)
		{
			m_PlayerInventoryHUD.RefreshCurrentEquippedItem();
		}
		return result;
	}

	public override void SetMyCell(RoomBlob roomblob)
	{
		RoomBlob myCell = GetMyCell();
		if (myCell != null)
		{
			HandleGamerOwnershipOfRoomBlob(myCell, isNowMine: false);
		}
		base.SetMyCell(roomblob);
		RoomBlob myCell2 = GetMyCell();
		if (myCell2 != null && m_Gamer != null)
		{
			HandleGamerOwnershipOfRoomBlob(myCell2, isNowMine: true);
		}
		ProcessHomePin();
	}

	private void HandleGamerOwnershipOfRoomBlob(RoomBlob cell, bool isNowMine)
	{
		if (!(cell != null))
		{
			return;
		}
		RoomBlob_Cell roomBlobData = cell.GetRoomBlobData<RoomBlob_Cell>();
		if (roomBlobData != null)
		{
			DeskInteraction deskInteraction = roomBlobData.GetCellObject(typeof(DeskInteraction), this) as DeskInteraction;
			if (deskInteraction != null && deskInteraction.m_LinkedItemContainer != null)
			{
				deskInteraction.m_LinkedItemContainer.m_bShouldConsiderItemRefresh = !isNowMine;
			}
			Door door = roomBlobData.m_Door;
			if (door != null)
			{
				m_CellDoorCode = door.m_DoorKeySubCode;
			}
		}
	}

	public override bool ShouldBoundCameraDoShakes()
	{
		return true;
	}

	public override void SetCarriedObject(CarryObjectInteraction obj)
	{
		if (m_CarriedObject != obj && null != obj)
		{
			TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.PutDownObject);
		}
		base.SetCarriedObject(obj);
	}

	protected override void SetCarriedCharacter(Character character)
	{
		if (m_CarriedCharacter != character && null != character)
		{
			TutorialManager.GetInstance().StartTutorialRPC(this, TutorialSubject.PutDownObject);
		}
		base.SetCarriedCharacter(character);
	}

	private void JobLost(Character jobLoser, JobType jobLost)
	{
		if (jobLoser == this)
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(this, TutorialSubject.JobSabotaged);
			}
		}
	}

	private void OnItemAdded(ItemContainer container, Item item, bool intoHidden)
	{
		if (m_Gamer == null || !m_Gamer.IsLocal() || intoHidden)
		{
			return;
		}
		PlayerInventoryHUD inventoryHud = GetInventoryHud();
		if (inventoryHud != null)
		{
			inventoryHud.FlashInventoryWithoutFocus();
		}
		if (SignificantItemsStore.GetInstance() != null && SignificantItemsStore.GetInstance().ShouldItemTriggerQuickMoldTutorial(item))
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(this, TutorialSubject.QuickMold);
			}
		}
	}

	public override void SetIsKnockedOut(bool knockedOut, Character characterResponsible)
	{
		base.SetIsKnockedOut(knockedOut, characterResponsible);
		if (knockedOut)
		{
			TutorialManager instance = TutorialManager.GetInstance();
			if (instance != null)
			{
				instance.StartTutorialRPC(this, TutorialSubject.StatusDecrease);
			}
		}
	}

	public void CLIENT_StartedSolitaryConfinement(bool enteringSolitary)
	{
		if (m_SolitaryActive)
		{
			UpdateSolitaryStat();
		}
		if (enteringSolitary)
		{
			m_TimeWhenStartedSolitaryConfinement = RoutineManager.GetInstance().GetElapsedSeconds();
			m_SolitaryActive = true;
		}
	}

	private void UpdateSolitaryStat()
	{
		float num = RoutineManager.GetInstance().GetElapsedSeconds() - m_TimeWhenStartedSolitaryConfinement;
		num /= 60f;
		if (num > 1f)
		{
			StatSystem.GetInstance().IncStat(21, num, m_Gamer, string.Empty);
		}
		m_TimeWhenStartedSolitaryConfinement = -1f;
		m_SolitaryActive = false;
	}

	private PlayerInventoryHUD GetInventoryHud()
	{
		if (m_PlayerInventoryHUD == null && HUDMenuFlow.Instance != null)
		{
			m_PlayerInventoryHUD = HUDMenuFlow.Instance.GetPlayerInventoryHUD(m_PlayerCameraManagerBindingID);
		}
		return m_PlayerInventoryHUD;
	}

	public override void SetJobRoom(RoomBlob jobRoom)
	{
		base.SetJobRoom(jobRoom);
		if (RoutineManager.GetInstance().GetCurrentRoutineBaseType() == Routines.JobTime)
		{
			SetupTargetRoom(RoutineManager.GetInstance().GetCurrentRoutineBaseType());
		}
	}

	private void OnRoomTypeChange(T17NetRoomGameView.GameRoomType roomType)
	{
		if (!NetCreateRoomHelper.IsResolving())
		{
			ChatFeedManager instance = ChatFeedManager.GetInstance();
			if (instance != null && m_Gamer != null && m_Gamer.IsLocal())
			{
				instance.ShowOnlineModeMessage(T17NetRoomManager.CurrentGameRoomType, bDisplayToAllPlayers: false, m_Gamer);
			}
		}
	}

	protected override bool SetupTargetRoom(Routines routinetype)
	{
		bool flag = base.SetupTargetRoom(routinetype);
		PrisonData.LevelInfo currentLevelInfo = LevelScript.GetCurrentLevelInfo();
		if (currentLevelInfo != null && currentLevelInfo.m_PrisonType == LevelScript.PRISON_TYPE.Tutorial)
		{
			return flag;
		}
		if (!flag)
		{
			SetRoutineArrowTarget(base.m_RoutineTargetLocation);
		}
		return true;
	}

	public void SetRoutineArrowTarget(RoomBlob room)
	{
		m_RoutineDestinationRoom = room;
		m_RoutineDestinationNetView = null;
		m_RoutineDestinationVec = Vector3.zero;
		HandleRoutineArrow();
	}

	public void SetRoutineArrowTarget(T17NetView netview)
	{
		m_RoutineDestinationNetView = netview;
		m_RoutineDestinationRoom = null;
		m_RoutineDestinationVec = Vector3.zero;
		HandleRoutineArrow();
	}

	public void SetRoutineArrowTarget(Vector3 pos)
	{
		m_RoutineDestinationVec = pos;
		m_RoutineDestinationRoom = null;
		m_RoutineDestinationNetView = null;
		HandleRoutineArrow();
	}

	public void CancelRoutineArrow()
	{
		ArrowManager instance = ArrowManager.GetInstance();
		if (instance != null)
		{
			if (m_RoutineArrowID != -1)
			{
				instance.CancelArrow(m_NetView, m_RoutineArrowID);
				m_RoutineArrowID = -1;
			}
			m_RoutineDestinationRoom = null;
			m_RoutineDestinationNetView = null;
			m_RoutineDestinationVec = Vector3.zero;
		}
	}

	public void HandleRoutineArrow()
	{
		if ((m_RoutineDestinationRoom != null || m_RoutineDestinationNetView != null || m_RoutineDestinationVec != Vector3.zero) && !base.m_bIsWantedForSolitary)
		{
			if (m_PlayerPathing != null)
			{
				Vector3 targetPosition = Vector3.zero;
				if (m_RoutineDestinationRoom != null)
				{
					targetPosition = m_RoutineDestinationRoom.GetArrowPosition();
				}
				else if (m_RoutineDestinationNetView != null)
				{
					targetPosition = m_RoutineDestinationNetView.transform.position;
				}
				else if (m_RoutineDestinationVec != Vector3.zero)
				{
					targetPosition = m_RoutineDestinationVec;
				}
				UpdatePassableDoors();
				m_PlayerPathing.PathToDestination(targetPosition, m_RoutinePathFinishedCallback, null, bCheckedLockedDoors: true);
			}
		}
		else
		{
			CancelRoutineArrow();
		}
	}

	public void RoutinePathFinished(Vector3 stairPosition, bool pathChangesFloor)
	{
		if ((m_RoutineDestinationRoom == null && m_RoutineDestinationVec == Vector3.zero && m_RoutineDestinationNetView == null) || base.m_bIsWantedForSolitary)
		{
			return;
		}
		ArrowManager instance = ArrowManager.GetInstance();
		if (!(instance != null) || !(m_NetView != null))
		{
			return;
		}
		if (pathChangesFloor)
		{
			FloorManager instance2 = FloorManager.GetInstance();
			if (instance2 != null)
			{
				int targetfloorindex = instance2.FindFloorIndexAtZ(stairPosition.z);
				m_RoutineArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.RoutineArrow, stairPosition, m_RoutineArrowID, bShowOnscreenIndicator: false, targetfloorindex, pathChangesFloor);
			}
		}
		else if (m_RoutineDestinationRoom != null)
		{
			m_RoutineArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.RoutineArrow, m_RoutineDestinationRoom, m_RoutineArrowID, bShowOnscreenIndicator: false);
		}
		else if (m_RoutineDestinationNetView != null)
		{
			m_RoutineArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.RoutineArrow, m_RoutineDestinationNetView, m_RoutineArrowID);
		}
		else if (m_RoutineDestinationVec != Vector3.zero)
		{
			m_RoutineArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.RoutineArrow, m_RoutineDestinationVec, m_RoutineArrowID, bShowOnscreenIndicator: false);
		}
		else
		{
			CancelRoutineArrow();
		}
	}

	public void SetObjectiveArrowTarget(Vector3 position, bool showTargetIndicator = true)
	{
		m_ObjectiveDestinationVec = position;
		m_ObjectiveDestinationNetView = null;
		m_bShowObjectiveTargetIndicator = showTargetIndicator;
		HandleObjectiveArrow();
	}

	public void SetObjectiveArrowTarget(T17NetView target)
	{
		m_ObjectiveDestinationNetView = target;
		m_ObjectiveDestinationVec = Vector3.zero;
		m_bShowObjectiveTargetIndicator = true;
		HandleObjectiveArrow();
	}

	public void CancelObjectiveArrow()
	{
		ArrowManager instance = ArrowManager.GetInstance();
		if (instance != null)
		{
			if (m_ObjectiveArrowID != -1)
			{
				instance.CancelArrow(m_NetView, m_ObjectiveArrowID);
				m_ObjectiveArrowID = -1;
			}
			m_ObjectiveDestinationVec = Vector3.zero;
			m_ObjectiveDestinationNetView = null;
			m_bShowObjectiveTargetIndicator = false;
			if (m_ObjectiveCharacter != null)
			{
				m_ObjectiveCharacter.OnFloorChangedEvent -= HandleObjectiveArrow;
				m_ObjectiveCharacter = null;
			}
		}
	}

	private void HandleObjectiveArrow()
	{
		if (m_ObjectiveDestinationNetView == null && m_ObjectiveCharacter != null)
		{
			m_ObjectiveCharacter.OnFloorChangedEvent -= HandleObjectiveArrow;
			m_ObjectiveCharacter = null;
		}
		if (m_ObjectiveDestinationNetView != null || m_ObjectiveDestinationVec != Vector3.zero)
		{
			if (!(m_PlayerPathing != null))
			{
				return;
			}
			Vector3 targetPosition = Vector3.zero;
			if (m_ObjectiveDestinationNetView != null)
			{
				targetPosition = m_ObjectiveDestinationNetView.transform.position;
				Character character = T17NetView.Find<Character>(m_ObjectiveDestinationNetView.viewID);
				if (m_ObjectiveCharacter != character)
				{
					if (m_ObjectiveCharacter != null)
					{
						m_ObjectiveCharacter.OnFloorChangedEvent -= HandleObjectiveArrow;
					}
					m_ObjectiveCharacter = character;
					if (m_ObjectiveCharacter != null)
					{
						m_ObjectiveCharacter.OnFloorChangedEvent += HandleObjectiveArrow;
					}
				}
			}
			else if (m_ObjectiveDestinationVec != Vector3.zero)
			{
				targetPosition = m_ObjectiveDestinationVec;
			}
			UpdatePassableDoors();
			m_PlayerPathing.PathToDestination(targetPosition, m_ObjectivePathFinishedCallback);
		}
		else
		{
			CancelObjectiveArrow();
		}
	}

	public void ObjectivePathFinished(Vector3 stairPosition, bool pathChangesFloor)
	{
		if (m_ObjectiveDestinationNetView == null && m_ObjectiveDestinationVec == Vector3.zero)
		{
			return;
		}
		ArrowManager instance = ArrowManager.GetInstance();
		if (!(instance != null) || !(m_NetView != null))
		{
			return;
		}
		if (pathChangesFloor)
		{
			FloorManager instance2 = FloorManager.GetInstance();
			if (instance2 != null)
			{
				int targetfloorindex = instance2.FindFloorIndexAtZ(stairPosition.z);
				m_ObjectiveArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.ObjectiveArrow, stairPosition, m_ObjectiveArrowID, bShowOnscreenIndicator: false, targetfloorindex, pathChangesFloor);
			}
		}
		else if (m_ObjectiveDestinationNetView != null)
		{
			m_ObjectiveArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.ObjectiveArrow, m_ObjectiveDestinationNetView, m_ObjectiveArrowID, m_bShowObjectiveTargetIndicator);
		}
		else if (m_ObjectiveDestinationVec != Vector3.zero)
		{
			m_ObjectiveArrowID = instance.SetArrowTargetRPC(m_NetView, ArrowManager.ArrowType.ObjectiveArrow, m_ObjectiveDestinationVec, m_ObjectiveArrowID, m_bShowObjectiveTargetIndicator);
		}
		else
		{
			CancelObjectiveArrow();
		}
	}

	public bool CheckInputEnabled(PlayerInputs inputEnum)
	{
		if (inputEnum >= (PlayerInputs)0 && inputEnum <= (PlayerInputs)2147483647 && (int)((uint)m_PlayerInputEnabledMask & (uint)inputEnum) > 0)
		{
			return true;
		}
		return false;
	}

	public void SetInputEnabled(PlayerInputs inputEnum, bool enabled)
	{
		if (inputEnum >= (PlayerInputs)0 && inputEnum <= (PlayerInputs)2147483647)
		{
			if (enabled)
			{
				m_PlayerInputEnabledMask |= (int)inputEnum;
			}
			else
			{
				m_PlayerInputEnabledMask &= (int)(~inputEnum);
			}
		}
	}

	public void SetCameraTargetToPlayer()
	{
		if (m_Gamer != null && m_Gamer.IsLocal())
		{
			m_PlayerCameraManagerBindingID = InGameMenuFlow.Instance.GetCameraIndexForGamer(m_Gamer);
			if (m_PlayerCameraManagerBindingID != 0)
			{
				CameraManager.GetInstance().SetTarget(this, m_PlayerCameraManagerBindingID);
			}
			TriggerCameraRefresh();
			HandleRoutineArrow();
		}
	}

	public void SetSwagBagForPlayer()
	{
		SwagBagManager instance = SwagBagManager.GetInstance();
		if (!(instance != null))
		{
			return;
		}
		SwagBagInteraction swagBag = instance.GetSwagBag(m_SpawnIndex);
		if (swagBag != null)
		{
			RoomBlob myCell = GetMyCell();
			if (myCell != null)
			{
				swagBag.PlaceSwagBagInCell(this, myCell, m_ItemContainer, GetOutFit(), m_EquippedItem, m_CharacterCustomisation.m_DisplayName);
				EquipStartingOutfit(forceSet: false, bClearOutOldOutfit: true);
				EquipStartingWeapon(forceSet: true);
			}
		}
	}

	public void GamerNotSetOnLoad()
	{
		if (m_Gamer == null && PrisonSnapshotIO.IsThereSaveData())
		{
			if (m_ItemContainer != null)
			{
				SetSwagBagForPlayer();
				Teleport(m_HidePlayerPosition);
			}
			QuestManager instance = QuestManager.GetInstance();
			if (instance != null)
			{
				instance.RemoveQuestsForPlayer(this, bSpecificQuestsOnly: true);
			}
		}
	}

	public override void SetPickedUp(Character carrier)
	{
		base.SetPickedUp(carrier);
		m_bCollisionDisabled_Carried = true;
	}

	public override void SetDropped(Vector3 dropPosition)
	{
		base.SetDropped(dropPosition);
		m_bCollisionDisabled_Carried = false;
	}

	public void OnInteract_Chair(InteractiveObject obj)
	{
		if (!(base.m_CurrentLocation != null) || base.m_CurrentLocation.location != RoomBlob.eLocation.JobOffice)
		{
			return;
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null && instance.GetCurrentRoutineBaseType() == Routines.JobTime)
		{
			TutorialManager instance2 = TutorialManager.GetInstance();
			if (instance2 != null)
			{
				instance2.StartTutorialRPC(this, TutorialSubject.JobOffice);
				base.OnInteractEvent -= OnInteract_Chair;
			}
		}
	}

	public void SetPlayerCameraForCursceneEnd(float timeToFadeUp)
	{
		CutsceneManagerBase.CutsceneFinishedEvent -= SetPlayerCameraForCursceneEnd;
		SetCameraTargetToPlayer();
	}

	protected override bool BuildingBoundaryCheck()
	{
		bool result = true;
		bool flag = m_EquippedItem != null && m_EquippedItem.IsInUse();
		bool flag2 = base.m_CurrentLocation != null && base.m_CurrentLocation.location == RoomBlob.eLocation.BuildingBoundary;
		if (!flag && !base.CurrentFloor.IsTheGroundFloor() && flag2)
		{
			FloorManager.GetInstance().GetTileGridPoint(base.CurrentFloor, FloorManager.TileSystem_Type.TileSystem_Ground, base.m_CachedCurrentPosition, out var row, out var column);
			if (row != m_PreviousTileRow || column != m_PreviousTileColumn)
			{
				FloorManager.Floor groundFloor;
				int groundRow;
				int groundColumn;
				bool groundIsClear;
				int num = FloorManager.GetInstance().FindGround(base.CurrentFloor, row, column, out groundFloor, out groundRow, out groundColumn, out groundIsClear);
				if (num > 0)
				{
					bool flag3 = false;
					if (num == 1)
					{
						if (groundIsClear)
						{
							flag3 = true;
						}
						else if (!m_SpeechBubbleHandler.IsProcessingSpeech())
						{
							SpeechManager.GetInstance().SaySomething(this, "Text.Player.DropNotClear", SpeechTone.Negative, 3f, 10);
						}
					}
					else if (!m_SpeechBubbleHandler.IsProcessingSpeech())
					{
						SpeechManager.GetInstance().SaySomething(this, "Text.Player.DropTooHigh", SpeechTone.Negative, 3f, 10);
					}
					if (flag3)
					{
						if (FloorManager.GetInstance().GetTileCentrePosition(groundFloor, FloorManager.TileSystem_Type.TileSystem_Ground, groundRow, groundColumn, out var worldPosition))
						{
							Teleport(worldPosition);
							PauseMovement(0.1f);
							m_CharacterMovement.Immobile();
							EffectManager.PlayEffect(EffectManager.effectType.LandedJump, worldPosition + m_EffectOffsetLanded);
						}
					}
					else
					{
						result = false;
					}
				}
			}
		}
		return result;
	}

	public void OnMinigameEntered()
	{
		m_bIsInMinigame = true;
	}

	public void OnMinigameExited()
	{
		m_bIsInMinigame = false;
	}

	public void RequestMapOpen()
	{
		m_bMapRequested = true;
	}

	public void RequestToOpenInventory()
	{
		m_bInventoryRequested = true;
	}

	public static int GetAllCharactersListSize()
	{
		if (m_AllCharacters != null)
		{
			return m_AllCharacters.Count;
		}
		return -1;
	}

	public static Character GetCharacterAtIndex(int index)
	{
		if (m_AllCharacters == null || index < 0 || index >= m_AllCharacters.Count)
		{
			return null;
		}
		return m_AllCharacters[index];
	}

	public override bool IsPlayer()
	{
		return true;
	}

	public void SwallowInputAction(string action)
	{
		if (!m_SwallowedInputActions.Contains(action))
		{
			m_SwallowedInputActions.Add(action);
		}
	}

	public bool IsInputActionSwallowed(string action)
	{
		return m_SwallowedInputActions.Contains(action);
	}

	public bool IsButtonUpAndNotSwallowed(string action)
	{
		return m_Gamer.m_RewiredPlayer.GetButtonUp(action) && !IsInputActionSwallowed(action);
	}

	public void SetAllowedItem(ItemData item)
	{
		m_AllowedUsableItem = item;
	}

	private void UpdatePassableDoors()
	{
		m_PlayerPathing.ClearDoors();
		if (m_ItemContainer != null)
		{
			for (int i = 0; i < m_ItemContainer.GetItemCount(); i++)
			{
				Item item = m_ItemContainer.GetItem(i);
				KeyFunctionality keyFunctionality = (KeyFunctionality)item.HasFunctionality(BaseItemFunctionality.Functionality.Key);
				if (keyFunctionality != null)
				{
					m_PlayerPathing.AddKeyColour(keyFunctionality.m_KeyColour);
				}
			}
		}
		Item equippedItem = GetEquippedItem();
		if (equippedItem != null)
		{
			KeyFunctionality keyFunctionality2 = (KeyFunctionality)equippedItem.HasFunctionality(BaseItemFunctionality.Functionality.Key);
			if (keyFunctionality2 != null)
			{
				m_PlayerPathing.AddKeyColour(keyFunctionality2.m_KeyColour);
			}
		}
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null && instance.PurpleDoorsOpen)
		{
			m_PlayerPathing.AddKeyColour(KeyFunctionality.KeyColour.Purple);
		}
		m_PlayerPathing.AddKeyColour(KeyFunctionality.KeyColour.Yellow);
		m_PlayerPathing.AddKeyColour(KeyFunctionality.KeyColour.Solitary);
	}

	public override void ShowNPCPin()
	{
		if (m_MapIcon != null && m_PinID == -1)
		{
			PinManager instance = PinManager.GetInstance();
			FloorManager instance2 = FloorManager.GetInstance();
			if (instance != null && instance2 != null)
			{
				bool bForMainMap = true;
				bool bForMiniMap = true;
				GameObject target = base.gameObject;
				Sprite mapIcon = m_MapIcon;
				bool bUpdatePosition = true;
				FloorManager.Floor floor = instance2.FindFloorbyIndex(1);
				PinManager.Pin.PinFilterType filterType = PinManager.Pin.PinFilterType.Characters;
				bool edgable = false;
				bool floorTrackable = false;
				bool directional = false;
				string displayName = m_CharacterCustomisation.m_DisplayName;
				bool localiseToolTipTag = false;
				int netViewID = m_Gamer.m_NetViewID;
				m_PinID = instance.CreatePin(bForMainMap, bForMiniMap, target, mapIcon, bUpdatePosition, floor, null, filterType, edgable, floorTrackable, directional, displayName, localiseToolTipTag, bOverrideIconScale: false, default(Vector3), null, default(Vector3), isPlayer: true, netViewID);
			}
		}
	}

	public bool GetCloseInventoryOnPauseMenuHide()
	{
		return m_bCloseInventoryOnPauseMenuHide;
	}

	public void SetCloseInventoryOnPauseMenuHide()
	{
		if (m_bBrowsingPauseMenu)
		{
			m_bCloseInventoryOnPauseMenuHide = true;
		}
	}

	public override bool GetPermissionToForceAnimatorUpdateOnEnable()
	{
		if (m_Gamer != null && m_Gamer.IsLocal() && GetIsKnockedOut() && !m_CharacterAnimator.HasUnappliedStateChange())
		{
			return false;
		}
		return base.GetPermissionToForceAnimatorUpdateOnEnable();
	}

	protected override string GenerateAdditionalSavePayload()
	{
		SaveData_Player_AdditionalPayload_V1 saveData_Player_AdditionalPayload_V = new SaveData_Player_AdditionalPayload_V1();
		saveData_Player_AdditionalPayload_V.CON_ST_M = m_NumberShowTimesMissed;
		return JsonUtility.ToJson(saveData_Player_AdditionalPayload_V);
	}

	protected override void RestoreAdditionalSavePayload(string payload)
	{
		if (string.IsNullOrEmpty(payload))
		{
			return;
		}
		try
		{
			SaveData_Player_AdditionalPayload_V1 saveData_Player_AdditionalPayload_V = JsonUtility.FromJson<SaveData_Player_AdditionalPayload_V1>(payload);
			if (saveData_Player_AdditionalPayload_V != null)
			{
				m_NumberShowTimesMissed = saveData_Player_AdditionalPayload_V.CON_ST_M;
			}
		}
		catch (Exception)
		{
		}
	}

	public override void HandleRoutineMissedEvent(RoutinesData.Routine oldRoutine)
	{
		if (oldRoutine != null && oldRoutine.m_BaseRoutineType == Routines.ShowTime && m_Gamer != null && m_NetView.isMine)
		{
			m_NumberShowTimesMissed++;
			if (m_NumberShowTimesMissed == 3)
			{
				StatSystem.GetInstance().IncStat(51, 1f, m_Gamer, string.Empty);
			}
		}
		base.HandleRoutineMissedEvent(oldRoutine);
	}

	public override void HandleRoutineReachedEvent(RoutinesData.Routine routine)
	{
		base.HandleRoutineReachedEvent(routine);
		if (routine != null && routine.m_BaseRoutineType == Routines.ShowTime)
		{
			m_NumberShowTimesMissed = 0;
		}
	}

	public void SwallowStopInteraction()
	{
		m_bStopInteractionProcessed = true;
	}
}
