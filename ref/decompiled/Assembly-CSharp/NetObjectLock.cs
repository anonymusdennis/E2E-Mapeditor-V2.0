using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectLock : T17MonoBehaviour
{
	public delegate void OnRPCResponse(Character localCharacter);

	public delegate void OnResponse(bool result);

	public delegate void OnRPCKicked(bool exit, int characterID);

	public T17NetView m_NetView;

	public string m_sObjectName = string.Empty;

	public bool m_bIsKickable = true;

	[Localization]
	public string m_InteractActionNameTag = "Text.Interact.Action";

	[Localization]
	public string m_FarAwayActionNameTag;

	public TrackableUIElementsReporter m_TrackableElementReporter;

	public bool m_bIsVisibleToProximityDetector = true;

	private bool m_bIsVisibleToProximityDetectorExternalOverride = true;

	public float m_ProximityPenalty = 1f;

	public ProximityPriorityLayers m_ProximityPriority = ProximityPriorityLayers.Low;

	private bool m_bIsLocked;

	private int m_MC_CharacterId = -1;

	private bool m_bPlayerLocked;

	private int m_MC_PreviousOwnerId;

	private int m_MC_CurrentInteractionId = -1;

	private Character m_RequestLocalCharacter;

	private Character m_LocalCharacter;

	private Character m_NetworkSyncedCharacter;

	private OnRPCResponse m_OnSuccessRequest;

	private OnRPCResponse m_OnFailureRequest;

	private OnRPCResponse m_OnExit_Interactor;

	private OnRPCResponse m_OnExit_InteractorRequest;

	private bool m_bRequestLock;

	private OnResponse m_OnResponseRequest;

	private OnRPCKicked m_OnExit_CharacterRequest;

	private OnRPCKicked m_OnExit_Character;

	private List<InteractiveObject> m_AllInteractions = new List<InteractiveObject>();

	private List<InteractiveObject> m_PrimaryInteractions = new List<InteractiveObject>();

	private List<InteractiveObject> m_SecondaryInteractions = new List<InteractiveObject>();

	private List<InteractiveObject> m_TertiaryInteractions = new List<InteractiveObject>();

	public bool m_bTransferOwnership;

	private bool m_bHasPrimaryHoldInteractions;

	private bool m_bHasSecondaryHoldInteractions;

	private bool m_bHasTertiaryHoldInteractions;

	public void OnGamerDeleteImminent(Gamer gamer)
	{
		if (T17NetManager.IsMasterClient && IsLocked() && m_MC_CharacterId == gamer.m_NetViewID)
		{
			ReleaseLock();
		}
	}

	protected override void Awake()
	{
		Init();
		base.Awake();
		T17NetManager.OnBecameMasterClient += OnBecameMasterClient;
		T17NetManager.OnNewMasterClient += OnNewMasterClient;
	}

	protected virtual void OnDestroy()
	{
		Gamer.OnDeleteImminent -= OnGamerDeleteImminent;
		T17NetManager.OnBecameMasterClient -= OnBecameMasterClient;
		T17NetManager.OnNewMasterClient -= OnNewMasterClient;
		if (IsLocked() && m_NetView != null)
		{
			ReleaseLock();
		}
		m_NetView = null;
		m_TrackableElementReporter = null;
		m_RequestLocalCharacter = null;
		m_LocalCharacter = null;
		m_NetworkSyncedCharacter = null;
		m_AllInteractions.Clear();
		m_PrimaryInteractions.Clear();
		m_SecondaryInteractions.Clear();
		m_TertiaryInteractions.Clear();
		m_OnSuccessRequest = null;
		m_OnFailureRequest = null;
		m_OnResponseRequest = null;
		m_OnExit_InteractorRequest = null;
		m_OnExit_CharacterRequest = null;
		m_RequestLocalCharacter = null;
	}

	protected virtual void Init()
	{
		if (m_NetView == null)
		{
			m_NetView = GetComponent<T17NetView>();
		}
		InteractiveObject[] componentsInChildren = GetComponentsInChildren<InteractiveObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			InteractiveObject interactiveObject = componentsInChildren[i];
			interactiveObject.SetLocalInteractionId(i);
			m_AllInteractions.Add(interactiveObject);
			switch (interactiveObject.m_InteractType)
			{
			case InteractiveObject.InteractiveType.PrimaryInteraction:
				m_PrimaryInteractions.Add(interactiveObject);
				break;
			case InteractiveObject.InteractiveType.PressAndHoldPrimaryInteraction:
				m_bHasPrimaryHoldInteractions = true;
				m_PrimaryInteractions.Add(interactiveObject);
				break;
			case InteractiveObject.InteractiveType.SecondaryInteraction:
				m_SecondaryInteractions.Add(interactiveObject);
				break;
			case InteractiveObject.InteractiveType.PressAndHoldSecondaryInteraction:
				m_bHasSecondaryHoldInteractions = true;
				m_SecondaryInteractions.Add(interactiveObject);
				break;
			case InteractiveObject.InteractiveType.TertiaryInteraction:
				m_TertiaryInteractions.Add(interactiveObject);
				break;
			case InteractiveObject.InteractiveType.PressAndHoldTertiaryInteraction:
				m_bHasTertiaryHoldInteractions = true;
				m_TertiaryInteractions.Add(interactiveObject);
				break;
			}
		}
		if (m_TrackableElementReporter == null)
		{
			m_TrackableElementReporter = GetComponent<TrackableUIElementsReporter>();
			if (m_TrackableElementReporter == null)
			{
				m_TrackableElementReporter = base.gameObject.AddComponent<TrackableUIElementsReporter>();
			}
		}
		m_TrackableElementReporter.SetDelegatorForWhenNearbyNameplatesShouldShow(ShouldShowNameplateWhenNearby);
		m_TrackableElementReporter.SetProximityPriority(m_ProximityPriority, m_ProximityPenalty);
		if (m_TrackableElementReporter != null)
		{
			Localization.Get(m_InteractActionNameTag, out var localized);
			localized = ((!string.IsNullOrEmpty(localized)) ? localized : m_InteractActionNameTag);
			m_TrackableElementReporter.SetDisplayName(localized);
			if (!string.IsNullOrEmpty(m_FarAwayActionNameTag))
			{
				Localization.Get(m_FarAwayActionNameTag, out localized);
				localized = ((!string.IsNullOrEmpty(localized)) ? localized : m_FarAwayActionNameTag);
				m_TrackableElementReporter.SetFarAwayDisplayName(localized);
			}
		}
		Gamer.OnDeleteImminent += OnGamerDeleteImminent;
	}

	public void GetLock(Character character, InteractiveObject intObj, OnRPCResponse OnSuccess, OnRPCResponse OnFailure, OnRPCResponse OnExit_Interactor, OnResponse OnResponse, OnRPCKicked OnExit_Character = null)
	{
		if (m_bRequestLock)
		{
			OnFailure(character);
			return;
		}
		m_bRequestLock = true;
		m_OnSuccessRequest = OnSuccess;
		m_OnFailureRequest = OnFailure;
		m_OnExit_InteractorRequest = OnExit_Interactor;
		m_OnResponseRequest = OnResponse;
		m_OnExit_CharacterRequest = OnExit_Character;
		m_RequestLocalCharacter = character;
		int viewID = character.m_NetView.viewID;
		int ownerId = character.m_NetView.ownerId;
		m_NetView.RPCQuestion("RPC_GetLock", NetTargets.MasterClient, viewID, intObj.GetLocalInteractionId(), ownerId);
	}

	[PunRPC]
	public void RPC_GetLock(int RPCID, int characterID, int interactID, int ownerId, PhotonMessageInfo info)
	{
		bool flag = true;
		InteractiveObject interactiveObject = GetInteractiveObject(interactID);
		Character character = T17NetView.Find<Character>(characterID);
		if (interactiveObject != null && !interactiveObject.AllowedToInteract(character))
		{
			flag = false;
		}
		else if (m_bIsLocked && characterID != m_MC_CharacterId)
		{
			flag = false;
			if (interactiveObject.CanKickAlreadyInteracting())
			{
				Character character2 = T17NetView.Find<Character>(m_MC_CharacterId);
				if ((!(character2 != null) || character2.m_CharacterRole != CharacterRole.Medic) && m_bIsKickable)
				{
					KickInteractingCharacter(characterID);
				}
			}
		}
		else
		{
			flag = true;
			m_bIsLocked = true;
			m_MC_CharacterId = characterID;
			m_MC_CurrentInteractionId = interactID;
			m_MC_PreviousOwnerId = m_NetView.ownerId;
			if (null != character && null != character.m_CharacterStats && character.m_CharacterStats.m_bIsPlayer)
			{
				m_bPlayerLocked = true;
			}
			character.m_MC_NetObjectLockID = m_NetView.viewID;
			if (m_bTransferOwnership)
			{
				m_NetView.TransferOwnership(ownerId);
			}
			if (m_bPlayerLocked)
			{
				SyncState();
			}
			Server_LockStatusChanged(lockStatus: true);
		}
		m_NetView.RPCResponse("RPC_GetLockResponse", RPCID, flag);
	}

	public void KickInteractingCharacter(int kickingCharacter)
	{
		T17NetView target = T17NetView.Find<T17NetView>(m_MC_CharacterId);
		if (m_MC_CharacterId != -1)
		{
			m_NetView.RPC("RPC_Kicked", target, kickingCharacter);
		}
	}

	[PunRPC]
	public void RPC_GetLockResponse(bool success)
	{
		StartCoroutine(GetLockResponse(success));
	}

	private IEnumerator GetLockResponse(bool success)
	{
		if (success && m_bTransferOwnership && !m_NetView.isMine)
		{
			yield return null;
		}
		m_bRequestLock = false;
		if (success)
		{
			if (m_OnSuccessRequest != null)
			{
				m_OnSuccessRequest(m_RequestLocalCharacter);
			}
			m_LocalCharacter = m_RequestLocalCharacter;
			m_OnExit_Interactor = m_OnExit_InteractorRequest;
			m_OnExit_Character = m_OnExit_CharacterRequest;
			Owner_SetRemoteInteractingIdAndBroadcast(m_LocalCharacter);
		}
		else if (m_OnFailureRequest != null)
		{
			m_OnFailureRequest(m_RequestLocalCharacter);
		}
		if (m_OnResponseRequest != null)
		{
			m_OnResponseRequest(success);
		}
		m_OnSuccessRequest = null;
		m_OnFailureRequest = null;
		m_OnResponseRequest = null;
		m_OnExit_InteractorRequest = null;
		m_OnExit_CharacterRequest = null;
		m_RequestLocalCharacter = null;
	}

	[PunRPC]
	public void RPC_Kicked(int kickerCharacterID, PhotonMessageInfo info)
	{
		if ((!m_bTransferOwnership || m_NetView.isMine) && !(m_LocalCharacter == null))
		{
			if (m_OnExit_Character != null)
			{
				m_OnExit_Character(exit: false, kickerCharacterID);
			}
			m_OnExit_Character = null;
			if (m_OnExit_Interactor != null)
			{
				m_OnExit_Interactor(m_LocalCharacter);
			}
			m_OnExit_Interactor = null;
		}
	}

	public void ReleaseLock()
	{
		if (T17NetManager.IsMasterClient)
		{
			Master_ProcessReleaseLock();
		}
		else
		{
			m_NetView.RPCQuestion("RPC_ReleaseLock", NetTargets.MasterClient);
		}
		m_OnExit_Interactor = null;
		if (m_OnExit_Character != null && m_LocalCharacter != null && m_LocalCharacter.m_NetView != null)
		{
			m_OnExit_Character(exit: true, m_LocalCharacter.m_NetView.viewID);
		}
		m_OnExit_Character = null;
		m_LocalCharacter = null;
		Owner_SetRemoteInteractingIdAndBroadcast(null);
	}

	private void Owner_SetRemoteInteractingIdAndBroadcast(Character theCharacter)
	{
		m_NetworkSyncedCharacter = theCharacter;
		m_NetView.PostLevelLoadRPC("RPC_All_SetRemoteInteractingCharacter", NetTargets.Others, (!(m_NetworkSyncedCharacter == null)) ? m_NetworkSyncedCharacter.m_NetView.viewID : (-1));
	}

	[PunRPC]
	private void RPC_All_SetRemoteInteractingCharacter(int characterViewId)
	{
		if (characterViewId != -1)
		{
			Character character = T17NetView.Find<Character>(characterViewId);
			if (character != null)
			{
				m_NetworkSyncedCharacter = character;
			}
		}
		else
		{
			m_NetworkSyncedCharacter = null;
		}
	}

	[PunRPC]
	public void RPC_ReleaseLock(int RPCID)
	{
		Master_ProcessReleaseLock();
		m_NetView.RPCResponse(null, RPCID);
	}

	private void Master_ProcessReleaseLock()
	{
		Server_LockStatusChanged(lockStatus: false);
		if (m_bTransferOwnership)
		{
			m_NetView.TransferOwnership(m_MC_PreviousOwnerId);
		}
		Character character = T17NetView.Find<Character>(m_MC_CharacterId);
		if (character != null)
		{
			character.m_MC_NetObjectLockID = -1;
		}
		m_bIsLocked = false;
		m_MC_CharacterId = -1;
		if (m_bPlayerLocked)
		{
			m_bPlayerLocked = false;
			SyncState();
		}
	}

	private void Server_LockStatusChanged(bool lockStatus)
	{
		InteractiveObject interactiveObject = GetInteractiveObject(m_MC_CurrentInteractionId);
		if (interactiveObject != null)
		{
			interactiveObject.Server_OnLockStatusChanged(m_MC_CharacterId, lockStatus);
		}
	}

	public bool AnyInteractionVisible()
	{
		for (int i = 0; i < m_AllInteractions.Count; i++)
		{
			if (m_AllInteractions[i].InteractionVisibility())
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyInteractionAllowed(Vector3 position)
	{
		for (int i = 0; i < m_AllInteractions.Count; i++)
		{
			if (m_AllInteractions[i].IsEnabled() && m_AllInteractions[i].CheckIfInAllowedDirection(position))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPrimaryHoldInteractionsAvailable(Character character)
	{
		return m_bHasPrimaryHoldInteractions && HasInteractionsAvailable(character, m_PrimaryInteractions, InteractiveObject.InteractiveType.PressAndHoldPrimaryInteraction);
	}

	public bool HasSecondaryHoldInteractionsAvailable(Character character)
	{
		return m_bHasSecondaryHoldInteractions && HasInteractionsAvailable(character, m_SecondaryInteractions, InteractiveObject.InteractiveType.PressAndHoldSecondaryInteraction);
	}

	public bool HasTertiaryHoldInteractionsAvailable(Character character)
	{
		return m_bHasTertiaryHoldInteractions && HasInteractionsAvailable(character, m_TertiaryInteractions, InteractiveObject.InteractiveType.PressAndHoldTertiaryInteraction);
	}

	private bool HasInteractionsAvailable(Character character, List<InteractiveObject> interactions, InteractiveObject.InteractiveType interactType)
	{
		for (int i = 0; i < interactions.Count; i++)
		{
			if (interactions[i].m_InteractType == interactType && interactions[i].AllowedToInteract(character) && interactions[i].CheckIfInAllowedDirection(character.transform.position))
			{
				return true;
			}
		}
		return false;
	}

	public InteractiveObject GetInteractiveObject(int interactionID)
	{
		if (m_AllInteractions == null)
		{
			return null;
		}
		if (interactionID >= 0 && interactionID < m_AllInteractions.Count)
		{
			return m_AllInteractions[interactionID];
		}
		return null;
	}

	public bool ProcessPrimaryInteractions(Character character, bool wasHeld, OnResponse OnInteractResponse = null)
	{
		InteractiveObject.InteractiveType interactType = ((wasHeld && m_bHasPrimaryHoldInteractions) ? InteractiveObject.InteractiveType.PressAndHoldPrimaryInteraction : InteractiveObject.InteractiveType.PrimaryInteraction);
		return ProcessInteractions(character, m_PrimaryInteractions, interactType, OnInteractResponse);
	}

	public bool ProcessSecondaryInteractions(Character character, bool wasHeld, OnResponse OnInteractResponse = null)
	{
		InteractiveObject.InteractiveType interactType = ((!wasHeld || !m_bHasSecondaryHoldInteractions) ? InteractiveObject.InteractiveType.SecondaryInteraction : InteractiveObject.InteractiveType.PressAndHoldSecondaryInteraction);
		return ProcessInteractions(character, m_SecondaryInteractions, interactType, OnInteractResponse);
	}

	public bool ProcessTertiaryInteractions(Character character, bool wasHeld, OnResponse OnInteractResponse = null)
	{
		InteractiveObject.InteractiveType interactType = ((!wasHeld || !m_bHasTertiaryHoldInteractions) ? InteractiveObject.InteractiveType.TertiaryInteraction : InteractiveObject.InteractiveType.PressAndHoldTertiaryInteraction);
		return ProcessInteractions(character, m_TertiaryInteractions, interactType, OnInteractResponse);
	}

	private bool ProcessInteractions(Character character, List<InteractiveObject> interactions, InteractiveObject.InteractiveType interactType, OnResponse OnInteractResponse = null)
	{
		InteractiveObject interactiveObject = null;
		for (int i = 0; i < interactions.Count; i++)
		{
			bool flag = interactions[i].AllowedToInteract(character);
			if (flag && interactions[i].m_InteractType == interactType && interactions[i].IsEnabled() && interactions[i].CheckIfInAllowedDirection(character.transform.position))
			{
				if (interactions[i].CanStartOrContinueInteraction(character))
				{
					interactions[i].Interact(character, OnInteractResponse);
					return true;
				}
				if (interactiveObject == null)
				{
					interactiveObject = interactions[i];
				}
			}
			else if (!flag)
			{
				interactions[i].OnPlayerNotAllowedToInteract(character);
			}
		}
		if (interactiveObject != null)
		{
			interactiveObject.OnCharacterFailedToStart(character);
		}
		return false;
	}

	public bool IsLocked()
	{
		return m_bIsLocked;
	}

	public bool IsPlayerLocked()
	{
		return m_bPlayerLocked;
	}

	private void SyncState()
	{
		if (T17NetManager.IsMasterClient)
		{
			InteractiveObjectManager instance = InteractiveObjectManager.GetInstance();
			if (instance != null)
			{
				instance.OnNetObjectLockSync(this);
			}
			m_NetView.PostLevelLoadRPC("RPC_SyncState", NetTargets.Others, m_bIsLocked, m_bPlayerLocked, m_MC_CharacterId, m_MC_CurrentInteractionId);
		}
	}

	public void SyncStateTo(PhotonPlayer player)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_NetView.RPC("RPC_SyncState", player, m_bIsLocked, m_bPlayerLocked, m_MC_CharacterId, m_MC_CurrentInteractionId);
		}
	}

	[PunRPC]
	private void RPC_SyncState(bool bLocked, bool playerLocked, int characterID, int interactionID)
	{
		m_bIsLocked = bLocked;
		m_MC_CharacterId = characterID;
		m_MC_CurrentInteractionId = interactionID;
		m_bPlayerLocked = playerLocked;
		InteractiveObjectManager instance = InteractiveObjectManager.GetInstance();
		if (instance != null)
		{
			instance.OnNetObjectLockSync(this);
		}
	}

	public bool HasInteractionOfType<T>()
	{
		for (int i = 0; i < m_AllInteractions.Count; i++)
		{
			if (m_AllInteractions[i] is T)
			{
				return true;
			}
		}
		return false;
	}

	public InteractiveObject GetFirstInteractionOfType<T>()
	{
		for (int i = 0; i < m_AllInteractions.Count; i++)
		{
			if (m_AllInteractions[i] is T)
			{
				return m_AllInteractions[i];
			}
		}
		return null;
	}

	public bool ShouldShowNameplateWhenNearby()
	{
		bool flag = false;
		for (int i = 0; i < m_AllInteractions.Count; i++)
		{
			flag |= m_AllInteractions[i].ShouldShowNameplateWhenNearby();
		}
		return flag;
	}

	public Character GetLocalCharacter()
	{
		return m_LocalCharacter;
	}

	public Character GetLocalOrNetworkSycnedCharacter()
	{
		if (m_LocalCharacter != null)
		{
			return m_LocalCharacter;
		}
		return m_NetworkSyncedCharacter;
	}

	private void OnBecameMasterClient()
	{
		m_NetworkSyncedCharacter = null;
	}

	private void OnNewMasterClient(PhotonPlayer newMasterClient)
	{
		m_NetworkSyncedCharacter = null;
	}

	public void SetProximityDetectorExternalOverride(bool bVisible)
	{
		m_bIsVisibleToProximityDetectorExternalOverride = bVisible;
	}

	public bool IsVisibleToProximityDetector()
	{
		return m_bIsVisibleToProximityDetectorExternalOverride & m_bIsVisibleToProximityDetector;
	}
}
