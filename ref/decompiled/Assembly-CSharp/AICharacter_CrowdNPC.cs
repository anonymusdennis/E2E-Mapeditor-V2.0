using System.Collections.Generic;
using UnityEngine;

public class AICharacter_CrowdNPC : AICharacter
{
	private bool m_bSeated;

	private bool m_bWaving;

	private bool m_bCrowdAnimatorFeature;

	private bool m_bItsShowTime;

	private bool m_bAnimatorEnabledFromNPCManager = true;

	private bool m_bAnimatorEnabledFromCulling = true;

	private Animator m_Animator;

	private int _m_CrowdMemberID = -1;

	private int m_SiblingCount;

	private float m_fShowTimeDelay = 30f;

	private float m_fShowTimeDelayVariance = 5f;

	private float m_fShowTimeDelayTimer;

	private bool m_bCalcSeatedPosition;

	[SerializeField]
	private float m_fWaveVariance = 0.6f;

	private float m_fWavePosition;

	private static RoomBlob m_TargetRoom;

	private bool m_bWasSaveLoaded;

	private bool m_bBehaviourActive;

	public int m_CrowdMemberID
	{
		get
		{
			if (_m_CrowdMemberID == -1)
			{
				_m_CrowdMemberID = base.transform.GetSiblingIndex();
				m_SiblingCount = base.transform.parent.childCount;
			}
			return _m_CrowdMemberID;
		}
	}

	public override T17BehaviourManager.INITSTATE StartInit()
	{
		if (PrisonSnapshotIO.IsThereSaveData())
		{
			m_bWasSaveLoaded = true;
		}
		RoutineManager.GetInstance().OnRoutineChanged += AICharacter_CrowdNPC_OnRoutineChanged;
		return base.StartInit();
	}

	private void AICharacter_CrowdNPC_OnRoutineChanged(RoutinesData.Routine oldRoutine, RoutinesData.Routine newRoutine, bool forceEnd)
	{
		if (oldRoutine != null)
		{
			m_bWasSaveLoaded = false;
		}
	}

	protected override void OnStart()
	{
		NPCManager.GetInstance().AddCrowdNPC(this);
		m_Animator = GetComponentInChildren<Animator>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_TargetRoom = null;
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			instance.OnRoutineChanged -= AICharacter_CrowdNPC_OnRoutineChanged;
		}
		instance = null;
	}

	protected override void OnUpdate()
	{
		if (RoutineManager.GetInstance() == null)
		{
			return;
		}
		Routines currentRoutineBaseType = RoutineManager.GetInstance().GetCurrentRoutineBaseType();
		if (currentRoutineBaseType == Routines.ShowTime || currentRoutineBaseType == Routines.ShowerTime)
		{
			if (!m_bItsShowTime && NPCManager.GetInstance().AllowToTakeASeatAtShowTime(_m_CrowdMemberID))
			{
				m_bItsShowTime = true;
				float num = 1f - (float)m_CrowdMemberID / (float)m_SiblingCount;
				if (!m_bWasSaveLoaded)
				{
					m_fShowTimeDelayTimer = num * m_fShowTimeDelay;
					m_fShowTimeDelay += Random.Range(0f, m_fShowTimeDelayVariance);
				}
				else
				{
					m_fShowTimeDelayTimer = 0f;
				}
			}
		}
		else if (m_bItsShowTime)
		{
			m_bItsShowTime = false;
			float num2 = (float)m_CrowdMemberID / (float)m_SiblingCount;
			m_fShowTimeDelayTimer = num2 * m_fShowTimeDelay;
			m_fShowTimeDelay += Random.Range(0f, m_fShowTimeDelayVariance);
		}
		if (m_fShowTimeDelayTimer > 0f)
		{
			m_fShowTimeDelayTimer -= UpdateManager.deltaTime;
		}
	}

	public float GetCrowdSeatingPosition()
	{
		if (!m_bCalcSeatedPosition)
		{
			RoomWaypoint crowdWaypoint = GetCrowdWaypoint();
			if (!(crowdWaypoint == null))
			{
				Vector3 position = crowdWaypoint.GetPosition();
				float num = Mathf.Atan2(position.y, position.x);
				m_fWavePosition = num + Random.Range(0f, m_fWaveVariance);
			}
			m_bCalcSeatedPosition = true;
		}
		return m_fWavePosition;
	}

	public RoomWaypoint GetCrowdWaypoint()
	{
		if (m_TargetRoom == null)
		{
			List<RoomBlob> allRoomsByLocation = RoomManager.GetInstance().GetAllRoomsByLocation(RoomBlob.eLocation.CrowdSeating);
			if (allRoomsByLocation == null || allRoomsByLocation.Count == 0)
			{
				return null;
			}
			m_TargetRoom = allRoomsByLocation[0];
		}
		List<RoomWaypoint> waypointList = m_TargetRoom.GetWaypointList();
		if (m_CrowdMemberID >= waypointList.Count)
		{
			return null;
		}
		return waypointList[m_CrowdMemberID];
	}

	public void DoWave(bool shouldBeActive)
	{
		if (shouldBeActive && !m_bWaving && m_bSeated)
		{
			m_bWaving = true;
			m_Character.m_CharacterAnimator.StartAnimation(AnimState.IdleMexicanWave);
		}
		if (!shouldBeActive && m_bWaving)
		{
			m_bWaving = false;
			m_Character.m_CharacterAnimator.StopAnimation(AnimState.IdleMexicanWave);
		}
	}

	public void SetIsSeated(bool seated)
	{
		m_bSeated = seated;
		if (!seated)
		{
			m_Character.m_bSpecialStencilSkip = false;
			StopAllSeatedAnimations();
		}
		else
		{
			m_Character.m_bSpecialStencilSkip = true;
		}
		CheckControlledUpdateActive();
	}

	public bool IsSeated()
	{
		return m_bSeated;
	}

	public void SetCrowdAnimatorFeature(bool banimatorFeature)
	{
		m_bCrowdAnimatorFeature = banimatorFeature;
	}

	public bool IsCrowdAnimatorFeature()
	{
		return m_bCrowdAnimatorFeature;
	}

	private void StopAllSeatedAnimations()
	{
		if (m_bWaving)
		{
			if (IsCrowdAnimatorFeature())
			{
				SetCrowdAnimatorFeature(banimatorFeature: false);
				ControllAnimatorFromNPCManager(bEnable: true);
			}
			m_Character.m_CharacterAnimator.StopAnimation(AnimState.IdleMexicanWave);
		}
	}

	public bool IsShowTime()
	{
		bool flag = ((!m_bItsShowTime) ? (m_fShowTimeDelayTimer > 0f) : (m_fShowTimeDelayTimer <= 0f));
		if (!flag && IsCrowdAnimatorFeature())
		{
			SetCrowdAnimatorFeature(banimatorFeature: false);
			ControllAnimatorFromNPCManager(bEnable: true);
		}
		return flag;
	}

	public void ControllAnimatorFromNPCManager(bool bEnable)
	{
		bool flag = m_Animator.enabled;
		m_bAnimatorEnabledFromNPCManager = bEnable;
		bool flag2 = (!bEnable && !flag) || (m_bAnimatorEnabledFromNPCManager & m_bAnimatorEnabledFromCulling);
		m_Animator.enabled = flag2;
		if (!flag && flag2)
		{
			m_Character.m_CharacterAnimator.OnAnimatorEnabled();
		}
	}

	public void ControllAnimatorFromCulling(bool bEnable)
	{
		bool flag = m_Animator.enabled;
		m_bAnimatorEnabledFromCulling = bEnable;
		bool flag2 = m_bAnimatorEnabledFromNPCManager & m_bAnimatorEnabledFromCulling;
		m_Animator.enabled = flag2;
		if (!flag && flag2)
		{
			m_Character.m_CharacterAnimator.OnAnimatorEnabled();
		}
	}

	public void SetBehaviourIsActive(bool isActive)
	{
		m_bBehaviourActive = isActive;
		CheckControlledUpdateActive();
	}

	public void CheckControlledUpdateActive()
	{
		bool requiresControlledUpdate = m_bBehaviourActive && !m_bSeated;
		m_AIMovement.SetRequiresControlledUpdate(requiresControlledUpdate);
	}
}
