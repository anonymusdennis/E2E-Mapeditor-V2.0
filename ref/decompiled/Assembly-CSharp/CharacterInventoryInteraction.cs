using System.Collections;
using UnityEngine;

public class CharacterInventoryInteraction : InteractiveObject
{
	public ItemContainer m_LinkedItemContainer;

	public Character m_ThisCharacter;

	protected override void Init()
	{
		base.Init();
		if (m_LinkedItemContainer == null)
		{
			m_LinkedItemContainer = GetComponent<ItemContainer>();
		}
		if (!(m_ThisCharacter == null))
		{
		}
	}

	public override void Server_OnLockStatusChanged(int characterId, bool getLock)
	{
		PhotonView photonView = PhotonView.Find(characterId);
		if (photonView == null)
		{
			return;
		}
		Character component = photonView.GetComponent<Character>();
		if (component == null)
		{
			return;
		}
		if (getLock)
		{
			m_ThisCharacter.SetBusyRPC(busy: true);
			component.SetBusyRPC(busy: true);
			bool flag = m_ThisCharacter.IsInteracting();
			bool isKnockedOut = m_ThisCharacter.GetIsKnockedOut();
			if (!flag && !isKnockedOut && m_ThisCharacter.m_CharacterAnimator.m_CharacterAnimator != null)
			{
				m_ThisCharacter.FaceCharacter(component);
			}
		}
		else
		{
			m_ThisCharacter.SetBusyRPC(busy: false);
			component.SetBusyRPC(busy: false);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		if (m_LinkedItemContainer != null)
		{
			localCharacter.m_OpenContainer = m_LinkedItemContainer;
		}
		if (!(m_ThisCharacter != null) || !localCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		if (localCharacter.m_OpenContainer != null)
		{
			InGameRootMenu.InGameMenuTypeToOpen inGameMenuTypeToOpen = InGameRootMenu.InGameMenuTypeToOpen.Inmate;
			inGameMenuTypeToOpen = GetMenuToOpenForPlayerInteraction(localCharacter, localCharacter.m_OpenContainer.m_ContainerType);
			if (localCharacter.m_CharacterStats.m_bIsPlayer)
			{
				Player component = localCharacter.GetComponent<Player>();
				if (component != null && !component.ViewContainer(localCharacter.m_OpenContainer, inGameMenuTypeToOpen))
				{
					StartCoroutine(DelayedStopInteraction());
				}
			}
		}
		bool success;
		Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(m_ThisCharacter, out success);
		if (success)
		{
			vendorForCharacter.SetBlockExternalChangesRPC(blocked: true);
		}
	}

	private InGameRootMenu.InGameMenuTypeToOpen GetMenuToOpenForPlayerInteraction(Character localCharacter, ItemContainer.ItemContainerType containerType)
	{
		InGameRootMenu.InGameMenuTypeToOpen result = InGameRootMenu.InGameMenuTypeToOpen.Inmate;
		switch (containerType)
		{
		case ItemContainer.ItemContainerType.Guard:
			result = ((!m_ThisCharacter.m_bIsKnockedOut) ? InGameRootMenu.InGameMenuTypeToOpen.Guard : InGameRootMenu.InGameMenuTypeToOpen.DownedGuard);
			break;
		case ItemContainer.ItemContainerType.Inmate:
			result = ((!m_ThisCharacter.m_bIsKnockedOut) ? ((!VendorManager.GetInstance().IsVendor(m_ThisCharacter)) ? InGameRootMenu.InGameMenuTypeToOpen.Inmate : InGameRootMenu.InGameMenuTypeToOpen.ShopInmate) : InGameRootMenu.InGameMenuTypeToOpen.DownedInmate);
			break;
		}
		return result;
	}

	private IEnumerator DelayedStopInteraction()
	{
		yield return new WaitForEndOfFrame();
		m_interactingCharacter.RequestStopInteraction();
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		base.OnExitInteraction(localCharacter);
		bool success;
		Vendor vendorForCharacter = VendorManager.GetInstance().GetVendorForCharacter(m_ThisCharacter, out success);
		if (success)
		{
			vendorForCharacter.SetBlockExternalChangesRPC(blocked: false);
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

	public override bool AllowedToInteract(Character localCharacter)
	{
		if (base.AllowedToInteract(localCharacter))
		{
			if (m_ThisCharacter.m_CharacterStats.m_bIsPlayer)
			{
				return m_ThisCharacter.GetIsKnockedOut();
			}
			AICharacter component = m_ThisCharacter.GetComponent<AICharacter>();
			if (component != null && component.IsInCombatState())
			{
				return false;
			}
			if (LevelScript.GetCurrentLevelInfo() == null || LevelScript.GetCurrentLevelInfo().m_PrisonType != LevelScript.PRISON_TYPE.Tutorial)
			{
				return true;
			}
			if (localCharacter.m_CharacterStats.m_bIsPlayer)
			{
				InGameRootMenu.InGameMenuTypeToOpen menuToOpenForPlayerInteraction = GetMenuToOpenForPlayerInteraction(localCharacter, m_LinkedItemContainer.m_ContainerType);
				return InGameMenuFlow.Instance.HasMenusToOpen(menuToOpenForPlayerInteraction, (localCharacter as Player).m_PlayerCameraManagerBindingID);
			}
			return true;
		}
		return false;
	}

	public override bool CanKickAlreadyInteracting()
	{
		return false;
	}

	public override bool SerialiseInteractionForLoad()
	{
		return false;
	}
}
