using AUTOGEN_T17Wwise_Enums;
using UnityEngine;
using UnityEngine.Serialization;

public class CarryInteraction : InteractiveObject
{
	[FormerlySerializedAs("m_Carried")]
	public Character m_KOCharacter;

	[FormerlySerializedAs("m_CarriedCharacterAnimator")]
	public CharacterAnimator m_KOCharacterAnimator;

	public static Vector3 m_vCarryOffset = new Vector3(0f, 0.5f, 0f);

	public T17NetView m_NetView;

	[ReadOnly]
	public Character m_Carrier;

	[ReadOnly]
	public CharacterAnimator m_CarrierCharacterAnimator;

	public float m_fPickupAnimationTime = 1f;

	private float m_fPickupAnimationTimer;

	protected override void Awake()
	{
		base.Awake();
		m_NetView = m_NetObjectLock.m_NetView;
	}

	protected override void OnDestroy()
	{
		m_NetView = null;
		m_Carrier = null;
		m_CarrierCharacterAnimator = null;
		m_KOCharacter = null;
		m_KOCharacterAnimator = null;
		base.OnDestroy();
	}

	public override bool InteractionVisibility()
	{
		return m_KOCharacter.m_bIsKnockedOut;
	}

	private void PickUpCharacter()
	{
		m_KOCharacter.m_Transform.position = m_Carrier.m_Transform.position + m_vCarryOffset;
		m_NetView.PostLevelLoadRPC("RPC_PickUpCharacter", NetTargets.All, m_Carrier.m_NetView.viewID);
		m_Carrier.m_CharacterAnimator.StartAnimation(AnimState.IdleCarry);
	}

	[PunRPC]
	public void RPC_PickUpCharacter(int netId, PhotonMessageInfo info)
	{
		if (null == m_Carrier || (null != m_Carrier && m_Carrier.m_NetView.viewID != netId))
		{
			m_Carrier = T17NetView.Find<Character>(netId);
		}
		m_KOCharacter.SetPickedUp(m_Carrier);
		if (null != m_Carrier)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_Pickup_Item.ToString(), m_Carrier.gameObject);
		}
		m_NetView.PostLevelLoadRPC("RPC_All_SetCharacterPreparingToBeCarried", NetTargets.All, m_KOCharacter.m_NetView.viewID, false);
	}

	private void DropCharacter()
	{
		Vector3 vDir = Vector3.zero;
		if (m_CarrierCharacterAnimator != null)
		{
			vDir = Direction.DirectionToVector(m_CarrierCharacterAnimator.GetDirectionx4());
		}
		Vector3 nearestValidPosition = NavMeshUtil.GetNearestValidPosition(m_Carrier.m_Transform.position, vDir);
		m_NetView.PostLevelLoadRPC("RPC_DropCharacter", NetTargets.All, nearestValidPosition.x, nearestValidPosition.y, nearestValidPosition.z);
		m_Carrier.m_CharacterAnimator.StopAnimation(AnimState.IdleCarry);
	}

	[PunRPC]
	public void RPC_DropCharacter(float x, float y, float z, PhotonMessageInfo info)
	{
		Vector3 dropped = default(Vector3);
		dropped.x = x;
		dropped.y = y;
		dropped.z = z;
		m_KOCharacter.SetDropped(dropped);
		if (null != m_Carrier)
		{
			AudioController.SendEvent(AudioController.SOUND_AREA.SA_INGAME, Events.Play_Player_PutDown_Item.ToString(), m_Carrier.gameObject);
		}
		m_NetView.PostLevelLoadRPC("RPC_All_SetCharacterPreparingToBeCarried", NetTargets.All, m_KOCharacter.m_NetView.viewID, false);
	}

	public override bool OverrideWalk()
	{
		return false;
	}

	public override bool AllowedToInteract(Character localCharacter)
	{
		return m_KOCharacter.m_bIsKnockedOut;
	}

	public override void RequestStopInteraction(Character localCharacter)
	{
		if (!(null != m_Carrier) || m_Carrier.m_CharacterRole != CharacterRole.Medic || !(localCharacter != m_Carrier))
		{
			OnExitInteraction(localCharacter);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		base.OnStartInteraction(localCharacter);
		m_Carrier = localCharacter;
		m_CarrierCharacterAnimator = localCharacter.m_CharacterAnimator;
		if (m_Carrier != null && m_Carrier.m_CharacterRole == CharacterRole.Medic)
		{
			m_fPickupAnimationTimer = m_fPickupAnimationTime;
			m_Carrier.PauseMovement(m_fPickupAnimationTimer);
			m_Carrier.m_CharacterAnimator.StartAnimation(AnimState.UseLow);
			m_NetView.PostLevelLoadRPC("RPC_All_SetCharacterPreparingToBeCarried", NetTargets.All, m_KOCharacter.m_NetView.viewID, true);
		}
		else
		{
			PickUpCharacter();
		}
	}

	[PunRPC]
	private void RPC_All_SetCharacterPreparingToBeCarried(int characterId, bool value)
	{
		Character character = T17NetView.Find<Character>(characterId);
		if (character != null)
		{
			character.IsPreparingToBeCarried = value;
		}
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		m_NetView.PostLevelLoadRPC("RPC_All_SetCharacterPreparingToBeCarried", NetTargets.All, m_KOCharacter.m_NetView.viewID, false);
		DropCharacter();
		base.OnExitInteraction(localCharacter);
		if (m_Carrier != null && m_fPickupAnimationTimer > 0f)
		{
			m_Carrier.m_CharacterAnimator.StopAnimation(AnimState.UseLow);
		}
		m_Carrier = null;
		m_CarrierCharacterAnimator = null;
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		bool flag = false;
		if (!m_KOCharacter.m_bIsKnockedOut)
		{
			flag = true;
		}
		if (m_fPickupAnimationTimer > 0f)
		{
			m_fPickupAnimationTimer -= UpdateManager.deltaTime;
			if (m_fPickupAnimationTimer <= 0f)
			{
				m_Carrier.m_CharacterAnimator.StopAnimation(AnimState.UseLow);
				PickUpCharacter();
			}
			else if (flag)
			{
				m_Carrier.m_CharacterAnimator.StopAnimation(AnimState.UseLow);
			}
		}
		if (flag)
		{
			OnExitInteraction(m_Carrier);
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
}
