using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class GotoOutfitWeaponCrate : ActionTask<AICharacter>
{
	public BBParameter<InteractiveObject> m_TargetObject;

	private bool m_bMovingToPosition;

	private Vector3 m_TargetLocation;

	private bool m_bTargetSet;

	private bool m_bSkipLast;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		m_bTargetSet = false;
		m_TargetObject.value = null;
		m_TargetLocation = base.agent.m_Transform.position;
		m_bMovingToPosition = false;
	}

	public void OnTargetReached()
	{
		if (m_TargetObject.value != null)
		{
			Vector3 position = m_TargetObject.value.transform.position;
			Vector3 vector = position - base.agent.m_Character.m_CachedCurrentPosition;
			Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(vector);
			base.agent.m_Character.SetFaceDirection(headAndBodyDirection);
		}
		EndAction(true);
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	protected override void OnUpdate()
	{
		if (!m_bTargetSet)
		{
			m_bTargetSet = true;
			if (base.agent.m_Character.m_CharacterRole == CharacterRole.Inmate)
			{
				m_TargetLocation = GetOutfitLocationInmate(out var desk);
				m_TargetObject.value = desk;
				m_bSkipLast = true;
			}
			else if (base.agent.m_Character.m_CharacterRole == CharacterRole.Guard)
			{
				m_TargetLocation = GetOutfitLocationGuard(out var crate);
				m_TargetObject.value = crate;
				m_bSkipLast = false;
			}
			else
			{
				m_TargetLocation = base.agent.m_Transform.position;
			}
		}
		if (!m_bMovingToPosition)
		{
			AIMovement aIMovement = base.agent.m_AIMovement;
			T17_ABPath.PathCallback onTargetReachedDel = m_OnTargetReachedDel;
			T17_ABPath.PathCallback onPathCancelledDel = m_OnPathCancelledDel;
			Vector3 targetLocation = m_TargetLocation;
			bool bSkipLast = m_bSkipLast;
			m_bMovingToPosition = aIMovement.TravelToPosition(onTargetReachedDel, onPathCancelledDel, targetLocation, 0.1f, throttled: false, allowTeleport: false, bSkipLast);
		}
	}

	private Vector3 GetOutfitLocationInmate(out InteractiveObject desk)
	{
		RoomBlob myCell = base.agent.m_Character.GetMyCell();
		if (myCell == null)
		{
			desk = null;
			return base.agent.m_Transform.position;
		}
		RoomBlob_Cell roomBlobData = myCell.GetRoomBlobData<RoomBlob_Cell>();
		desk = null;
		if (roomBlobData != null)
		{
			desk = roomBlobData.GetCellObject(typeof(DeskInteraction), base.agent.m_Character);
		}
		if (desk == null)
		{
			return base.agent.m_Transform.position;
		}
		return desk.transform.position;
	}

	private Vector3 GetOutfitLocationGuard(out InteractiveObject crate)
	{
		List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.GuardQuarters);
		float num = float.MaxValue;
		RoomBlob roomBlob = null;
		RoomBlob_GuardQuarters roomBlob_GuardQuarters = null;
		for (int i = 0; i < allRoomsByLocation.Count; i++)
		{
			RoomBlob roomB = allRoomsByLocation[i];
			RoomBlob roomA = base.agent.m_Character.m_CurrentLocation;
			float num2 = RoomUtility.GetInstance().GetDistanceEstimate(ref roomA, ref roomB);
			RoomBlob_GuardQuarters roomBlobData = roomB.GetRoomBlobData<RoomBlob_GuardQuarters>();
			if (roomBlobData == null || roomBlobData.m_Crates == null || roomBlobData.m_Crates.Count == 0)
			{
				num2 += 1000f;
			}
			else
			{
				roomBlob_GuardQuarters = roomBlobData;
			}
			if (num2 <= num)
			{
				num = num2;
				roomBlob = roomB;
			}
		}
		if (roomBlob == null)
		{
			crate = null;
			return base.agent.m_Transform.position;
		}
		if (roomBlob_GuardQuarters != null && roomBlob_GuardQuarters.m_Crates != null && roomBlob_GuardQuarters.m_Crates.Count > 0)
		{
			int count = roomBlob_GuardQuarters.m_Crates.Count;
			crate = roomBlob_GuardQuarters.m_Crates[Random.Range(0, count)];
			if (crate != null)
			{
				return crate.transform.position;
			}
		}
		crate = null;
		Vector3 position = base.agent.m_Transform.position;
		if (!roomBlob.GetRandomPositionInRoom(CharacterRole.Guard, ref position))
		{
			return base.agent.m_Transform.position;
		}
		return position;
	}
}
