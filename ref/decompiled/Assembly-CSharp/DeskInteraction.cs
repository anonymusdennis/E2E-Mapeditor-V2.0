using System.Collections.Generic;

public class DeskInteraction : AnimatedInteraction, IControlledUpdate
{
	public enum InventoryMenus
	{
		Desk,
		Cutlrey
	}

	[Localization]
	public string m_DeskNameOverrideTag;

	[Localization]
	public string m_DeskOpeningTextTag = "Text.Interact.DeskOpening";

	public float m_DeskOpeningTime = 5f;

	public Character m_DeskOwner;

	private AICharacter m_DeskAIOwner;

	public ItemContainer m_LinkedItemContainer;

	public string m_CharacterLocalizationToken = "$CharacterName";

	public string m_PlayOpenSound;

	public string m_PlayCloseSound;

	private bool m_bOpening;

	private bool m_bOpen;

	private float m_ElapsedOpeningTime;

	private T17TrackedUIElement m_TrackedUIElement;

	private ClimbableObject m_ClimbableBehaviour;

	private static List<DeskInteraction> m_InmateDesks;

	private static List<DeskInteraction> m_PlayerInmateDesks;

	private AIEvent m_SearchingDeskEvent;

	private bool m_bAllowPlayingTransition;

	private bool m_bTriedSettingState;

	public InventoryMenus m_MenuToOpen;

	public bool IsOpen => m_bOpen;

	public bool IsOpening => m_bOpening;

	public static List<DeskInteraction> GetInmateDesks()
	{
		return m_InmateDesks;
	}

	public static List<DeskInteraction> GetPlayerDesks()
	{
		return m_PlayerInmateDesks;
	}

	public static void CleanUp()
	{
		if (m_InmateDesks != null)
		{
			m_InmateDesks.Clear();
			m_InmateDesks = null;
		}
		if (m_PlayerInmateDesks != null)
		{
			m_PlayerInmateDesks.Clear();
			m_PlayerInmateDesks = null;
		}
	}

	public void SetOwner(Character owner)
	{
		m_DeskOwner = owner;
		if (owner == null)
		{
			return;
		}
		UpdateNameFromOwner();
		if (m_LinkedItemContainer != null)
		{
			m_LinkedItemContainer.SetCharacterOwner(owner);
		}
		if (owner.m_CharacterStats.m_bIsPlayer)
		{
			if (m_PlayerInmateDesks == null)
			{
				m_PlayerInmateDesks = new List<DeskInteraction>();
			}
			m_PlayerInmateDesks.Add(this);
			return;
		}
		if (m_InmateDesks == null)
		{
			m_InmateDesks = new List<DeskInteraction>();
		}
		m_InmateDesks.Add(this);
		m_DeskAIOwner = m_DeskOwner.GetComponent<AICharacter>();
	}

	public Character GetOwner()
	{
		return m_DeskOwner;
	}

	public void UpdateNameFromOwner()
	{
		if (m_NetObjectLock.m_TrackableElementReporter != null)
		{
			Localization.GetWithKeySwap(m_NetObjectLock.m_InteractActionNameTag, out var localised, m_CharacterLocalizationToken, m_DeskOwner.m_CharacterCustomisation.m_DisplayName);
			localised = ((!string.IsNullOrEmpty(localised)) ? localised : m_NetObjectLock.m_InteractActionNameTag);
			m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localised);
		}
	}

	protected override void Init()
	{
		base.Init();
		if (m_LinkedItemContainer == null)
		{
			m_LinkedItemContainer = GetComponent<ItemContainer>();
		}
		m_ClimbableBehaviour = GetComponent<ClimbableObject>();
		if (m_NetObjectLock.m_TrackableElementReporter != null)
		{
			if (string.IsNullOrEmpty(m_DeskNameOverrideTag))
			{
				Localization.Get("Text.Name.Nobody", out var localized);
				string value = ((!(m_DeskOwner == null) && !(m_DeskOwner.m_CharacterCustomisation == null) && !string.IsNullOrEmpty(m_DeskOwner.m_CharacterCustomisation.m_DisplayName)) ? m_DeskOwner.m_CharacterCustomisation.m_DisplayName : localized);
				Localization.GetWithKeySwap(m_NetObjectLock.m_InteractActionNameTag, out var localised, m_CharacterLocalizationToken, value);
				localised = ((!string.IsNullOrEmpty(localised)) ? localised : m_NetObjectLock.m_InteractActionNameTag);
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localised);
			}
			else
			{
				string localized2 = string.Empty;
				Localization.Get(m_DeskNameOverrideTag, out localized2);
				localized2 = ((!string.IsNullOrEmpty(localized2)) ? localized2 : m_DeskNameOverrideTag);
				m_NetObjectLock.m_TrackableElementReporter.SetDisplayName(localized2);
			}
		}
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Register(this, UpdateCategory.TimeSlicedFastInteractions);
		}
	}

	protected override void OnDestroy()
	{
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.DisableProgressBar();
			m_TrackedUIElement = null;
		}
		if (m_LinkedItemContainer != null)
		{
			if (m_interactingCharacter != null && m_interactingCharacter.m_OpenContainer != null)
			{
				m_interactingCharacter.m_OpenContainer = null;
			}
			m_LinkedItemContainer = null;
		}
		if (UpdateManager.GetInstance() != null)
		{
			UpdateManager.GetInstance().Unregister(this, UpdateCategory.TimeSlicedFastInteractions);
		}
		m_DeskOwner = null;
		m_DeskAIOwner = null;
		m_SearchingDeskEvent = null;
		m_ClimbableBehaviour = null;
		CleanUp();
		m_SearchingDeskEvent = null;
		base.OnDestroy();
	}

	public override void Server_OnLockStatusChanged(int characterID, bool getLock)
	{
		base.Server_OnLockStatusChanged(characterID, getLock);
		if (getLock)
		{
			if (!(m_DeskOwner != null) || !(m_DeskAIOwner != null) || m_DeskOwner.m_NetView.viewID == characterID)
			{
				return;
			}
			PhotonView photonView = PhotonView.Find(characterID);
			if (!(photonView == null))
			{
				Character component = photonView.GetComponent<Character>();
				if (component.m_CharacterEventManager != null)
				{
					m_SearchingDeskEvent = component.m_CharacterEventManager.GetSearchingDeskAIEvent();
				}
			}
		}
		else
		{
			m_SearchingDeskEvent = null;
		}
	}

	public void ControlledUpdate()
	{
		if (T17NetManager.IsMasterClient && m_SearchingDeskEvent != null && m_DeskAIOwner != null)
		{
			bool haveCollisionData = false;
			if (m_DeskAIOwner.m_CharacterUtil.LineOfSight(base.gameObject, out haveCollisionData))
			{
				m_DeskAIOwner.AddEvent(m_SearchingDeskEvent);
			}
		}
	}

	public void ControlledFixedUpdate()
	{
	}

	public void ControlledPreUpdate()
	{
	}

	public void ControlledPreFixedUpdate()
	{
	}

	public bool RequiresControlledUpdate()
	{
		return true;
	}

	public bool RequiresControlledFixedUpdate()
	{
		return false;
	}

	public bool RequiresControlledLateUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreUpdate()
	{
		return false;
	}

	public bool RequiresControlledPreFixedUpdate()
	{
		return false;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (localCharacter != null && localCharacter.m_CharacterRole != 0)
		{
			return true;
		}
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
		m_bAllowPlayingTransition = false;
		base.OnStartInteraction(localCharacter);
		if (m_interactingCharacter != m_DeskOwner && m_LinkedItemContainer != null && !m_LinkedItemContainer.m_bCanLoot)
		{
			localCharacter.SetIsSearchingDesk(value: true);
		}
		if (m_interactingCharacter.m_CharacterStats.m_bIsPlayer && m_interactingCharacter != m_DeskOwner)
		{
			AssignUIElement();
			m_NetObjectLock.m_NetView.RPC("RPC_Opening", NetTargets.All);
			return;
		}
		if (m_LinkedItemContainer != null)
		{
			m_interactingCharacter.m_OpenContainer = m_LinkedItemContainer;
		}
		TryOpenDesk();
		m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_SetOpenClose", NetTargets.All, true);
	}

	public override void RequestStopInteraction(Character localCharacter)
	{
		if (m_bOpening)
		{
			m_bOpening = false;
			base.RequestStopInteraction(localCharacter);
			m_bTriedSettingState = false;
			if ((bool)m_interactingCharacter && (bool)m_interactingCharacter.m_CharacterAnimator)
			{
				m_interactingCharacter.m_CharacterAnimator.StopAnimation(AnimState.UseMed);
				m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.exitAnimation);
			}
		}
		else
		{
			base.RequestStopInteraction(localCharacter);
		}
	}

	public override void ForceStopInteraction(Character localCharacter)
	{
		if (m_bOpening)
		{
			m_bOpening = false;
			base.ForceStopInteraction(localCharacter);
			m_bTriedSettingState = false;
			if ((bool)localCharacter && (bool)localCharacter.m_CharacterAnimator)
			{
				localCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.enterAnimation);
				localCharacter.m_CharacterAnimator.StopAnimation(AnimState.UseMed);
				localCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.exitAnimation);
			}
		}
		else
		{
			base.ForceStopInteraction(localCharacter);
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.DisableProgressBar();
			m_TrackedUIElement = null;
		}
		base.OnExitInteraction(localCharacter);
		if (!string.IsNullOrEmpty(m_PlayCloseSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayCloseSound, base.gameObject);
		}
		if (null != localCharacter)
		{
			localCharacter.SetIsSearchingDesk(value: false);
		}
		m_ElapsedOpeningTime = 0f;
		m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_SetOpenClose", NetTargets.All, false);
	}

	private void AssignUIElement()
	{
		m_TrackedUIElement = m_NetObjectLock.m_TrackableElementReporter.AssignAlwaysVisibleWorldCanvasUIElement();
		if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.EnableProgressBar();
			m_TrackedUIElement.SetProgressBarProgress(0f);
			m_TrackedUIElement.SetProgressBarText(m_DeskOpeningTextTag);
		}
	}

	public override void PlayTransitionAnimation(Character localCharacter, InteractObjAnimData animData, bool enter)
	{
		if (m_bAllowPlayingTransition)
		{
			base.PlayTransitionAnimation(localCharacter, animData, enter);
		}
	}

	public override void PlayTransitionAnimation(Character localCharacter, InteractObjAnimData animData, bool enter, Directionx8 transitionDirection)
	{
		if (m_bAllowPlayingTransition)
		{
			base.PlayTransitionAnimation(localCharacter, animData, enter, transitionDirection);
		}
	}

	public override void SetInteractionObjectAnimatorState(int state)
	{
		if (!m_bAllowPlayingTransition)
		{
			m_bTriedSettingState = true;
		}
		if (m_bAllowPlayingTransition)
		{
			base.SetInteractionObjectAnimatorState(state);
		}
	}

	public override void UpdateInteraction()
	{
		bool bTriedSettingState = m_bTriedSettingState;
		if (!m_bTriedSettingState)
		{
			base.UpdateInteraction();
		}
		if (m_bOpening && m_bTriedSettingState && !bTriedSettingState)
		{
			m_bAllowPlayingTransition = true;
			m_bInteractionReady = true;
			m_interactingCharacter.SetFaceDirection((Directionx4)Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
			m_interactingCharacter.m_CharacterAnimator.StartAnimation(AnimState.UseMed);
		}
		if (!m_bOpening)
		{
			return;
		}
		m_ElapsedOpeningTime += UpdateManager.deltaTime;
		if (m_ElapsedOpeningTime >= m_DeskOpeningTime)
		{
			m_ElapsedOpeningTime = 0f;
			if (m_TrackedUIElement != null)
			{
				m_TrackedUIElement.DisableProgressBar();
				m_TrackedUIElement = null;
			}
			if (m_LinkedItemContainer != null)
			{
				m_interactingCharacter.m_OpenContainer = m_LinkedItemContainer;
			}
			TryOpenDesk();
			m_NetObjectLock.m_NetView.PostLevelLoadRPC("RPC_SetOpenClose", NetTargets.All, true);
		}
		else if (m_TrackedUIElement != null)
		{
			m_TrackedUIElement.SetProgressBarProgress(m_ElapsedOpeningTime / m_DeskOpeningTime);
		}
	}

	protected override bool LeaveCharacterPositionUnAlteredDuringWalk()
	{
		return true;
	}

	private void TryOpenDesk()
	{
		m_bTriedSettingState = false;
		m_bAllowPlayingTransition = true;
		if (m_interactingCharacter.m_CharacterAnimator != null)
		{
			m_interactingCharacter.m_CharacterAnimator.StopAnimation(AnimState.UseMed);
		}
		if (m_bFindNearestInteractionPosition)
		{
			PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true, Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
		}
		else
		{
			PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true);
		}
		if (m_InteractionObjectAnimator != null)
		{
			SetInteractionObjectAnimatorState(1);
		}
		if (!m_interactingCharacter.m_CharacterStats.m_bIsPlayer || !(m_interactingCharacter.m_OpenContainer != null))
		{
			return;
		}
		InGameRootMenu.InGameMenuTypeToOpen menuType = InGameRootMenu.InGameMenuTypeToOpen.Desk;
		BaseMenuBehaviour.InGameMenuTypes inGameMenuTypes = BaseMenuBehaviour.InGameMenuTypes.DeskInventory;
		if (!string.IsNullOrEmpty(m_PlayCloseSound))
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, m_PlayOpenSound, base.gameObject);
		}
		switch (m_MenuToOpen)
		{
		case InventoryMenus.Desk:
			menuType = InGameRootMenu.InGameMenuTypeToOpen.Desk;
			inGameMenuTypes = BaseMenuBehaviour.InGameMenuTypes.DeskInventory;
			break;
		case InventoryMenus.Cutlrey:
			menuType = InGameRootMenu.InGameMenuTypeToOpen.Cutlrey;
			inGameMenuTypes = BaseMenuBehaviour.InGameMenuTypes.CutlreyInventory;
			break;
		}
		((Player)m_interactingCharacter).ViewContainer(m_interactingCharacter.m_OpenContainer, menuType);
		InGameMenuFlow.Instance.GetCorrectIGMData(((Player)m_interactingCharacter).m_PlayerCameraManagerBindingID, out var data);
		if (data == null || !(data.m_PlayerRootMenu != null))
		{
			return;
		}
		GameMenuBehaviour gameMenuBehaviour = (GameMenuBehaviour)data.m_PlayerRootMenu.GetCurrentOpenMenu();
		if (!(gameMenuBehaviour != null) || gameMenuBehaviour.m_MenuType != inGameMenuTypes)
		{
			return;
		}
		DeskMenu deskMenu = (DeskMenu)gameMenuBehaviour;
		if (deskMenu != null)
		{
			deskMenu.SetDeskOwner(m_DeskOwner);
			if (!string.IsNullOrEmpty(m_DeskNameOverrideTag))
			{
				deskMenu.SetMenuName(m_DeskNameOverrideTag);
			}
			if (m_DeskOwner == m_interactingCharacter)
			{
				data.PlayerInventory.SetAlternateHiddenContainer(ref m_LinkedItemContainer);
				deskMenu.PopulateHiddenCompartementWithItemContainer(ref m_LinkedItemContainer, firstTimeInit: true);
				deskMenu.GetHiddenCompInventoryBehaviour().SetItemContainerLinks(m_LinkedItemContainer, m_interactingCharacter.m_ItemContainer, (Player)m_interactingCharacter);
			}
		}
	}

	private bool CanInteract()
	{
		if (m_ClimbableBehaviour != null && m_ClimbableBehaviour.NumCharactersOnUs > 0)
		{
			return false;
		}
		return true;
	}

	[PunRPC]
	public void RPC_Opening(PhotonMessageInfo info)
	{
		m_bOpening = true;
		m_bOpen = false;
	}

	public void Open()
	{
		m_bOpening = false;
		m_bOpen = true;
	}

	public void Close()
	{
		m_bOpening = false;
		m_bOpen = false;
	}

	[PunRPC]
	public void RPC_SetOpenClose(bool open)
	{
		if (open)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	public bool AllowRefresh()
	{
		return m_DeskOwner == null || m_DeskAIOwner != null || RoutineManager.GetInstance().GetDaysElapsed() == 0;
	}

	public override bool AllowOtherPlayerHUDInteractions()
	{
		if (m_bOpening)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldCancelOnOtherHUDInteractions()
	{
		if (m_bOpening)
		{
			return true;
		}
		return false;
	}

	public override bool CanCloseContainer(ItemContainer itemContainer)
	{
		return itemContainer == m_LinkedItemContainer;
	}

	public void ControlledLateUpdate()
	{
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}
}
