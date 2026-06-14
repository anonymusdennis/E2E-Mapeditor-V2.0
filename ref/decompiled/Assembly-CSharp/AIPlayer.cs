using System.Collections.Generic;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using UnityEngine;

public class AIPlayer : Character
{
	[Header("AI Player Data")]
	public AICharacter m_AICharacter;

	public Blackboard m_Blackboard;

	private bool m_bPhysicsEnabled;

	public bool m_bCanBeQuestTarget = true;

	public override void Init()
	{
		base.Init();
		if (m_CharacterRole == CharacterRole.Guard)
		{
			if (m_bCanBeQuestTarget)
			{
				QuestManager.GetInstance().RegisterQuestableGuard(this);
			}
			OpinionManager.GetInstance().RegisterOpinionCharacter(this);
		}
		else if (m_CharacterRole == CharacterRole.Inmate)
		{
			if (m_bCanBeQuestTarget)
			{
				QuestManager.GetInstance().RegisterQuestableInmate(this);
			}
			if (!m_bIsRobinsonCharacter)
			{
				VendorManager.GetInstance().RegisterPotentialVendor(this);
			}
			OpinionManager.GetInstance().RegisterOpinionCharacter(this);
			if (m_bIsRobinsonCharacter)
			{
				m_AICharacter.SetRobinsonWanderingSpeech();
			}
		}
	}

	public override void OnInteractionStart()
	{
		base.OnInteractionStart();
	}

	public override void OnInteractionExit()
	{
		base.OnInteractionExit();
		m_AICharacter.m_AIMovement.CancelCurrentPath();
	}

	public override void SetIsKnockedOut(bool knockedOut, Character characterResponsible)
	{
		if (T17NetManager.IsMasterClient)
		{
			m_AICharacter.OnKnockedOut();
		}
		base.SetIsKnockedOut(knockedOut, characterResponsible);
	}

	protected override void OwnerFixedUpdate()
	{
		base.OwnerFixedUpdate();
		bool flag = m_bIsDashing || m_fKnockBackStunTimer > 0f;
		if (m_bPhysicsEnabled != flag)
		{
			TogglePhysicsControl(flag);
		}
	}

	public virtual void TogglePhysicsControl(bool enable)
	{
		m_bPhysicsEnabled = enable;
		if (enable)
		{
			m_RigidBody.isKinematic = false;
			if (!m_PhysicsCollider.activeInHierarchy)
			{
				m_PhysicsCollider.SetActive(value: true);
			}
			return;
		}
		m_AICharacter.m_AIMovement.CancelCurrentPath();
		m_RigidBody.isKinematic = true;
		if (m_PhysicsCollider.activeInHierarchy)
		{
			m_PhysicsCollider.SetActive(value: false);
		}
	}

	public override void AddAllowedDoor(Door door, Item itemAllowingAccess = null)
	{
		base.AddAllowedDoor(door, itemAllowingAccess);
		if (m_AICharacter != null && m_AICharacter.m_AIMovement != null)
		{
			m_AICharacter.m_AIMovement.AddDoor(door);
		}
	}

	public override void RemoveAllowedDoor(int doorID)
	{
		base.RemoveAllowedDoor(doorID);
		m_AICharacter.m_AIMovement.ClearAllowedDoors();
		Dictionary<int, FastList<Item>>.Enumerator enumerator = m_AllowedDoors.GetEnumerator();
		while (enumerator.MoveNext())
		{
			FastList<Item> value = enumerator.Current.Value;
			if (value != null && value.Count > 0)
			{
				int key = enumerator.Current.Key;
				Door doorByID = DoorManager.GetInstance().GetDoorByID(key);
				if (doorByID != null)
				{
					m_AICharacter.m_AIMovement.AddDoor(doorByID);
				}
			}
		}
	}

	public override void ClearAllowedDoors()
	{
		base.ClearAllowedDoors();
		if (m_AICharacter != null && m_AICharacter.m_AIMovement != null)
		{
			m_AICharacter.m_AIMovement.ClearAllowedDoors();
		}
	}

	public override void SetJobRoom(RoomBlob jobRoom)
	{
		base.SetJobRoom(jobRoom);
		if (m_CharacterRole != 0 || m_Blackboard == null)
		{
			return;
		}
		BehaviourTree value = null;
		if (jobRoom != null)
		{
			RoomBlob_JobRoom roomBlobData = jobRoom.GetRoomBlobData<RoomBlob_JobRoom>();
			if (roomBlobData != null)
			{
				value = roomBlobData.m_JobBehaviour;
			}
		}
		m_Blackboard.SetValue("JobBehaviour", value);
	}

	public override void RegainConsciousness()
	{
		base.RegainConsciousness();
		m_AICharacter.OnRegainConsciousness();
	}

	protected override void OnEscapeBindings()
	{
		m_AICharacter.OnEscapeBindings();
	}

	private void OnDisable()
	{
		SetIsDisabled(disabled: true);
	}

	private void OnEnable()
	{
		if (m_bIsDisabled)
		{
			SetIsDisabled(disabled: false);
		}
	}
}
