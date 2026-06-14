using System;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

[Category("★T17 Action")]
public class Chase : ActionTask<AICharacter>
{
	[BlackboardOnly]
	public BBParameter<AIEventMemory> m_FollowTarget;

	public bool m_bChaseCharacterResponsible;

	public float m_fUpdateTime = 0.5f;

	public float m_fPositionPredictionTime;

	public float m_CloseEnoughDistance = 0.5f;

	public float m_TargetReachedDistanceSqr = 1.21f;

	public bool m_InteractionKick = true;

	public float m_InteractionKickDistanceSqr = 2.25f;

	public bool m_bAllowTeleport;

	public float m_fGiveUpTimePercentage = 0.5f;

	public float m_fOffsetDistance = 0.3f;

	private bool m_bMovingToPosition;

	private float m_fUpdateTimer;

	private Vector3 m_vPreviousPosition;

	private bool m_bTargetMissing;

	private Vector3 m_vOffset = Vector3.zero;

	private Character m_TargetCharacter;

	private Transform m_TargetTransform;

	private float m_fGiveUpTimePercentageVariance;

	private float m_fHiddenGraceTime = 0.5f;

	private bool m_bCanSeeHiddenCharacters = true;

	private float m_fSpinEpoch = -1f;

	private float m_fSpinMinTime = 1f;

	private float m_fSpinMaxTime = 2f;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	private Vector3 m_StartPathLocation;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReachedPath;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	protected override void OnExecute()
	{
		if (m_FollowTarget.value == null || m_FollowTarget.value.m_TargetTransform == null)
		{
			EndAction(false);
			return;
		}
		m_bMovingToPosition = false;
		m_bTargetMissing = false;
		m_fUpdateTimer = 0f;
		m_vPreviousPosition = m_FollowTarget.value.m_TargetTransform.position;
		float num = (float)Math.PI * 2f;
		float f = m_FollowTarget.value.m_SlotPosition * num;
		float num2 = (float)m_FollowTarget.value.m_eEventType / 32f;
		num2 *= 3f;
		num2 *= num;
		m_vOffset.x = Mathf.Sin(f);
		m_vOffset.y = Mathf.Cos(f);
		m_vOffset *= m_fOffsetDistance;
		m_fGiveUpTimePercentageVariance = UnityEngine.Random.value - 0.5f;
		base.agent.m_AIMovement.CancelCurrentPath();
		UpdateChaseTarget();
		if (base.agent.m_Character.m_CharacterRole == CharacterRole.Guard || base.agent.m_Character.m_CharacterRole == CharacterRole.Inmate)
		{
			m_bCanSeeHiddenCharacters = false;
		}
	}

	private void UpdateChaseTarget()
	{
		if (m_bChaseCharacterResponsible)
		{
			m_TargetCharacter = m_FollowTarget.value.m_CharacterResponsible;
		}
		if (m_TargetCharacter != null)
		{
			m_TargetTransform = m_TargetCharacter.transform;
		}
		else
		{
			m_TargetTransform = m_FollowTarget.value.m_TargetTransform;
		}
	}

	protected override void OnUpdate()
	{
		base.agent.SetRunning(running: true);
		if (m_FollowTarget.value.m_fTimeSinceSeen > m_FollowTarget.value.m_fEventOracleTime * (m_fGiveUpTimePercentage + m_fGiveUpTimePercentageVariance))
		{
			if (!m_bTargetMissing)
			{
				m_bTargetMissing = true;
				bool flag = false;
				if (m_FollowTarget.value.m_TargetCharacter.m_CharacterStats.m_bIsPlayer)
				{
					flag = true;
				}
				SpeechManager instance = SpeechManager.GetInstance();
				Character character = base.agent.m_Character;
				string textID = "Text.Chase.Missing";
				SpeechTone tone = SpeechTone.Negative;
				float duration = 1f;
				bool bAllowTextRecolour = flag;
				instance.SaySomething(character, textID, tone, duration, 0, -1, ignoreStatus: false, bAllowTextRecolour);
				base.agent.m_Character.PauseMovement(1f);
			}
		}
		else
		{
			m_bTargetMissing = false;
		}
		Vector3 vector = m_TargetTransform.position;
		if (m_fPositionPredictionTime > 0f)
		{
			Vector3 position = m_TargetTransform.position;
			float num = Mathf.Max(BehaviourTree.CurrentTimeSlicedDeltaTime, 0.0001f);
			vector = position + (position - m_vPreviousPosition) / num * m_fPositionPredictionTime;
			m_vPreviousPosition = position;
			Debug.DrawLine(position, vector, Color.magenta);
		}
		Vector3 position2 = base.agent.transform.position;
		float num2 = Vector2.SqrMagnitude(vector - position2);
		if (NavMeshUtil.SameFloorCheck(vector.z, position2.z) && (m_bCanSeeHiddenCharacters || !(m_FollowTarget.value.m_fTimeSinceSeen > m_fHiddenGraceTime)))
		{
			if (m_InteractionKick && num2 < m_InteractionKickDistanceSqr && m_TargetCharacter != null)
			{
				m_TargetCharacter.RequestStopInteraction();
			}
			if (num2 <= m_TargetReachedDistanceSqr)
			{
				EndAction(true);
				return;
			}
		}
		float num3 = 1f;
		if (num2 > 0.01f)
		{
			num3 /= num2;
		}
		Vector3 chasePosition = GetChasePosition(vector);
		base.agent.m_AIMovement.SetChaseTarget(chasePosition, OnTargetReachedNoPath);
		m_fUpdateTimer += num3 * BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (!m_bMovingToPosition || m_fUpdateTimer > m_fUpdateTime)
		{
			m_fUpdateTimer = 0f;
			MoveCloser(m_TargetTransform.position);
		}
	}

	public void OnTargetReachedNoPath()
	{
		TargetReached(fromPath: false);
	}

	public void OnTargetReachedPath()
	{
		TargetReached(fromPath: true);
	}

	public void TargetReached(bool fromPath)
	{
		m_bMovingToPosition = false;
		if (!fromPath)
		{
			if (m_bCanSeeHiddenCharacters || !CharacterHidden())
			{
				EndAction(true);
			}
		}
		else if (IsMedic())
		{
			if (m_TargetTransform == null)
			{
				EndAction(true);
			}
			else
			{
				float num = Vector3.Distance(base.agent.m_Character.GetCachedCurrentPosition(), m_StartPathLocation);
				if (num < 0.1f)
				{
					EndAction(true);
				}
				else
				{
					GraphNode nearestGraphNode = NavMeshUtil.GetNearestGraphNode(m_TargetTransform.position);
					if (nearestGraphNode != null)
					{
						if (((Vector3)nearestGraphNode.position - base.agent.m_Transform.position).sqrMagnitude < 2.25f)
						{
							EndAction(true);
						}
					}
					else
					{
						EndAction(true);
					}
				}
			}
		}
		if (CharacterHidden() && m_fSpinEpoch < UpdateManager.time)
		{
			m_fSpinEpoch = UpdateManager.time + UnityEngine.Random.Range(m_fSpinMinTime, m_fSpinMaxTime);
			base.agent.m_Character.SetFaceDirection((Directionx4)(UnityEngine.Random.Range(0, 4) * 2));
		}
	}

	private bool IsMedic()
	{
		AICharacter aICharacter = base.agent;
		return aICharacter != null && aICharacter.m_Character != null && aICharacter.m_Character.m_CharacterRole == CharacterRole.Medic;
	}

	private bool CharacterHidden()
	{
		if (base.agent != null && base.agent.m_Character != null && m_FollowTarget != null && m_FollowTarget.value != null && m_TargetCharacter != null && m_TargetCharacter.m_bIsHidden && m_FollowTarget.value.m_fTimeSinceSeen > m_fHiddenGraceTime)
		{
			return true;
		}
		return false;
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	private void MoveCloser(Vector3 chaseTarget)
	{
		float closeEnoughDistance = m_CloseEnoughDistance;
		if (!m_bCanSeeHiddenCharacters && m_TargetCharacter != null && m_TargetCharacter.m_bIsHidden)
		{
			closeEnoughDistance = 2f;
		}
		m_StartPathLocation = base.agent.m_Character.GetCachedCurrentPosition();
		m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, chaseTarget, closeEnoughDistance, throttled: true, m_bAllowTeleport);
	}

	private Vector3 GetChasePosition(Vector3 predictedPosition)
	{
		return predictedPosition + m_vOffset;
	}

	protected override void OnStop()
	{
		base.agent.m_AIMovement.SetChaseTarget(null);
		base.agent.SetRunning(running: false);
	}
}
