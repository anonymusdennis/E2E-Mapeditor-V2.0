using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AnimatedInteraction : InteractiveObject, ICullingWrapperListener
{
	[Tooltip("An offset from the centre of the interaction transform of where the character will be lerped to.")]
	public Vector3 m_InteractionPositionOffset = Vector3.zero;

	public bool bForceFaceInteractionTarget;

	public bool m_bFindNearestInteractionPosition;

	public InteractObjAnimData m_AnimationData;

	[FormerlySerializedAs("m_ValidInterationDirections")]
	public Directionx8[] m_ValidAnimationDirections;

	public Animator m_InteractionObjectAnimator;

	private bool m_bTransitionToTarget;

	private bool m_bTransitionFromTarget;

	protected bool m_bInteractionReady;

	private List<Vector3> m_ExitPositions;

	protected Vector3 m_vExitPosition;

	protected Vector3 m_vInteractPosition;

	public float m_Z_Offset_Interaction;

	protected Transform m_VisualTransform;

	private float m_fTimer;

	private float m_fLerpTimer;

	private float m_fLerpTime;

	public float m_Transition_Z_Offset = -0.2f;

	public const int IDLE_STATE = 0;

	public const int START_STATE = 1;

	public const int PLAY_STATE = 2;

	public const int STOP_STATE = 3;

	public const int PLAYSTATE_A = 4;

	public const int PLAYSTATE_B = 5;

	public const string SPECIAL_TO_IDLE_TRIGGER = "SpecialToIdle";

	public const string SPECIAL_TO_STOP_TRIGGER = "SpecialToStop";

	public const string HOLD_SPECIAL = "HoldSpecialAnim";

	public const string ANIM_STATE_PARAMETER = "AnimState";

	public Directionx8[] m_DeniedExitDirections;

	private Directionx8[] m_PermittedExitDirections;

	public Directionx4 m_NonAnimatingFaceDirection = Directionx4.Down;

	private int m_CurrentAnimState;

	protected int m_NormalizedAnimStateHash = -1;

	private bool m_bAnimationIntegerNeedsSet;

	public int CurrentAnimState => m_CurrentAnimState;

	protected override void Init()
	{
		base.Init();
		if (m_ValidAnimationDirections == null || m_ValidAnimationDirections.Length == 0)
		{
			m_ValidAnimationDirections = Direction.FourDirections;
		}
		Animator componentInChildren = base.transform.GetComponentInChildren<Animator>();
		MeshRenderer componentInChildren2 = base.transform.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			m_VisualTransform = componentInChildren.transform;
		}
		else if (componentInChildren2 != null)
		{
			m_VisualTransform = componentInChildren2.transform;
		}
		else if (base.transform.parent != null)
		{
			componentInChildren = base.transform.parent.GetComponentInChildren<Animator>();
			componentInChildren2 = base.transform.parent.GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				m_VisualTransform = componentInChildren.transform;
			}
			else if (componentInChildren2 != null)
			{
				m_VisualTransform = componentInChildren2.transform;
			}
		}
		if (m_VisualTransform == null)
		{
			m_VisualTransform = base.transform;
		}
		List<Directionx8> list = new List<Directionx8>(Direction.AllDirections);
		if (m_DeniedExitDirections != null)
		{
			for (int i = 0; i < m_DeniedExitDirections.Length; i++)
			{
				list.Remove(m_DeniedExitDirections[i]);
			}
		}
		m_PermittedExitDirections = list.ToArray();
	}

	protected virtual bool LeaveCharacterPositionUnAlteredDuringWalk()
	{
		return LeaveCharacterPositionUnAltered();
	}

	public override void Walk(Vector2 walk)
	{
		float num = 0f;
		if (m_interactingCharacter != null && m_interactingCharacter.m_CharacterStats != null && m_interactingCharacter.m_CharacterStats.m_bIsPlayer)
		{
			num = m_StopInteractionWalkThreshold;
		}
		if (walk.magnitude > num && m_bInteractionReady)
		{
			if (LeaveCharacterPositionUnAlteredDuringWalk() && (m_bFindNearestInteractionPosition || LeaveCharacterPositionUnAltered()))
			{
				m_vExitPosition = m_interactingCharacter.transform.position;
			}
			else
			{
				Vector3 nearestValidPosition = NavMeshUtil.GetNearestValidPosition(m_vInteractPosition, walk, m_PermittedExitDirections, ref m_ExitPositions, includeNodesOnDoors: false);
				m_vExitPosition.x = nearestValidPosition.x;
				m_vExitPosition.y = nearestValidPosition.y;
			}
			RequestStopInteraction(m_interactingCharacter);
		}
	}

	protected override void OnStartInteraction(Character localCharacter)
	{
		if (m_bTransitionToTarget)
		{
			return;
		}
		base.OnStartInteraction(localCharacter);
		if (LeaveCharacterPositionUnAltered())
		{
			m_vInteractPosition = localCharacter.transform.position;
			m_vExitPosition = localCharacter.transform.position;
		}
		else if (m_bFindNearestInteractionPosition)
		{
			Vector3 vInteractPosition = FindClosestInteractionNode(localCharacter);
			m_vInteractPosition = vInteractPosition;
			m_vInteractPosition.z = localCharacter.transform.position.z;
			m_vExitPosition = m_vInteractPosition;
		}
		else
		{
			m_vExitPosition = localCharacter.transform.position;
			m_vInteractPosition = base.transform.position;
			m_vInteractPosition.x += m_InteractionPositionOffset.x;
			m_vInteractPosition.y += m_InteractionPositionOffset.y;
			m_vInteractPosition.z = localCharacter.transform.position.z;
		}
		UpdateInteractionZ_PreTransitionStart();
		m_bTransitionToTarget = true;
		m_bTransitionFromTarget = false;
		m_bInteractionReady = false;
		m_fTimer = 0f;
		localCharacter.OnInteractionStart();
		m_fLerpTimer = ((!(m_AnimationData != null)) ? 0f : m_AnimationData.lerpDuration);
		m_fLerpTime = m_fLerpTimer;
		if (m_fLerpTimer <= 0f)
		{
			if (m_AnimationData != null)
			{
				if (m_bFindNearestInteractionPosition || bForceFaceInteractionTarget)
				{
					PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true, Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
				}
				else
				{
					PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true);
				}
			}
			if (m_InteractionObjectAnimator != null)
			{
				SetInteractionObjectAnimatorState(1);
			}
		}
		else if (m_AnimationData.walkWhilstLerping)
		{
			Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(m_vInteractPosition - localCharacter.transform.position);
			localCharacter.m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Walk);
			localCharacter.SetFaceDirection(headAndBodyDirection);
		}
		else if (bForceFaceInteractionTarget)
		{
			PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true, Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
		}
	}

	public Vector3 FindClosestInteractionNode(Character localCharacter)
	{
		Vector3 position = base.transform.position;
		List<Vector3> list = new List<Vector3>();
		Vector3 nodePos = Vector3.zero;
		Vector3 pos = position + Direction.m_vUp;
		Vector3 pos2 = position + Direction.m_vDown;
		Vector3 pos3 = position + Direction.m_vLeft;
		Vector3 pos4 = position + Direction.m_vRight;
		if (NavMeshUtil.GetPositionOnNavMesh(pos, out nodePos))
		{
			list.Add(nodePos);
		}
		if (NavMeshUtil.GetPositionOnNavMesh(pos2, out nodePos))
		{
			list.Add(nodePos);
		}
		if (NavMeshUtil.GetPositionOnNavMesh(pos3, out nodePos))
		{
			list.Add(nodePos);
		}
		if (NavMeshUtil.GetPositionOnNavMesh(pos4, out nodePos))
		{
			list.Add(nodePos);
		}
		float num = float.MaxValue;
		Vector3 position2 = localCharacter.transform.position;
		Vector3 result = position2;
		for (int i = 0; i < list.Count; i++)
		{
			float num2 = Vector3.Distance(position2, list[i]);
			if (num2 < num)
			{
				num = num2;
				result = list[i];
			}
		}
		return result;
	}

	protected override void OnExitInteraction(Character localCharacter)
	{
		if (m_bInteractionReady)
		{
			if (m_AnimationData != null && localCharacter != null)
			{
				CharacterAnimator characterAnimator = localCharacter.m_CharacterAnimator;
				characterAnimator.StopAnimation(m_AnimationData.interactingAnimation);
				characterAnimator.StopAnimation(m_AnimationData.enterAnimation);
			}
			InteractionReadyEnd();
		}
		else if (m_AnimationData != null && localCharacter != null)
		{
			CharacterAnimator characterAnimator2 = localCharacter.m_CharacterAnimator;
			characterAnimator2.StopAnimation(m_AnimationData.enterAnimation);
			characterAnimator2.StopAnimation(m_AnimationData.interactingAnimation);
			characterAnimator2.StopAnimation(m_AnimationData.exitAnimation);
		}
		if (m_InteractionObjectAnimator != null)
		{
			SetInteractionObjectAnimatorState(0);
		}
		base.OnExitInteraction(localCharacter);
		m_bTransitionFromTarget = false;
		m_bTransitionToTarget = false;
		m_bInteractionReady = false;
		m_fTimer = 0f;
		if (null != localCharacter)
		{
			localCharacter.OnInteractionExit();
		}
	}

	public override void RequestStopInteraction(Character localCharacter)
	{
		if (!m_bTransitionFromTarget)
		{
			m_bTransitionToTarget = false;
			m_bTransitionFromTarget = true;
			m_bInteractionReady = false;
			if (m_bFindNearestInteractionPosition)
			{
				PlayTransitionAnimation(localCharacter, m_AnimationData, enter: false, Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
			}
			else
			{
				PlayTransitionAnimation(localCharacter, m_AnimationData, enter: false);
			}
			if (m_InteractionObjectAnimator != null)
			{
				SetInteractionObjectAnimatorState(3);
			}
			InteractionReadyEnd();
		}
	}

	public virtual void PlayTransitionAnimation(Character localCharacter, InteractObjAnimData animData, bool enter)
	{
		if (!(animData == null))
		{
			m_fTimer = animData.duration;
			m_fLerpTime = m_fTimer;
			if (Mathf.Approximately(m_fLerpTime, 0f))
			{
			}
			Directionx8 directionx = Directionx8.Up;
			if (enter)
			{
				localCharacter.m_CharacterAnimator.StartAnimation(animData.enterAnimation);
				directionx = ((CanEnterPlayState(localCharacter) || !(m_fTimer < 0.05f)) ? Direction.VectorToNearestDirection(localCharacter.transform.position - m_vInteractPosition, m_ValidAnimationDirections) : ((Directionx8)m_NonAnimatingFaceDirection));
			}
			else
			{
				localCharacter.m_CharacterAnimator.StopAnimation(animData.interactingAnimation);
				localCharacter.m_CharacterAnimator.StopAnimation(animData.enterAnimation);
				localCharacter.m_CharacterAnimator.StartAnimation(animData.exitAnimation);
				directionx = Direction.VectorToNearestDirection(m_vInteractPosition - m_vExitPosition, m_ValidAnimationDirections);
			}
			localCharacter.SetFaceDirection((Directionx4)directionx);
			if (enter)
			{
				UpdateInteractionZ_Interacting();
			}
		}
	}

	public virtual void PlayTransitionAnimation(Character localCharacter, InteractObjAnimData animData, bool enter, Directionx8 transitionDirection)
	{
		if (!(animData == null) && !(localCharacter == null))
		{
			m_fTimer = animData.duration;
			m_fLerpTime = m_fTimer;
			if (Mathf.Approximately(m_fLerpTime, 0f))
			{
			}
			if (enter)
			{
				localCharacter.m_CharacterAnimator.StartAnimation(animData.enterAnimation);
			}
			else
			{
				localCharacter.m_CharacterAnimator.StopAnimation(animData.interactingAnimation);
				localCharacter.m_CharacterAnimator.StopAnimation(animData.enterAnimation);
				localCharacter.m_CharacterAnimator.StartAnimation(animData.exitAnimation);
			}
			localCharacter.SetFaceDirection((Directionx4)transitionDirection);
			if (enter)
			{
				UpdateInteractionZ_Interacting();
			}
		}
	}

	public override void UpdateInteraction()
	{
		base.UpdateInteraction();
		if (m_bTransitionToTarget)
		{
			if (m_fLerpTimer > 0f)
			{
				m_fLerpTimer -= UpdateManager.deltaTime;
				float t = Mathf.Clamp01((m_fLerpTime - m_fLerpTimer) / m_fLerpTime);
				if (null != m_interactingCharacter)
				{
					Vector3 vector = Vector3.Lerp(m_vStartingPosition, m_vInteractPosition, t);
					m_interactingCharacter.transform.position = vector;
					m_interactingCharacter.m_CachedCurrentPosition = vector;
				}
				if (m_fLerpTimer <= 0f)
				{
					if (null != m_interactingCharacter)
					{
						if (m_bFindNearestInteractionPosition || bForceFaceInteractionTarget)
						{
							PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true, Direction.VectorToNearestDirection(base.transform.position - m_vInteractPosition, m_ValidAnimationDirections));
						}
						else
						{
							PlayTransitionAnimation(m_interactingCharacter, m_AnimationData, enter: true);
						}
					}
					if (m_InteractionObjectAnimator != null)
					{
						SetInteractionObjectAnimatorState(1);
					}
				}
			}
			else if (m_fTimer > 0f)
			{
				m_fTimer -= UpdateManager.deltaTime;
			}
			else
			{
				m_bTransitionToTarget = false;
				m_bInteractionReady = true;
				bool flag = CanEnterPlayState(m_interactingCharacter);
				if (null != m_interactingCharacter)
				{
					m_interactingCharacter.transform.position = m_vInteractPosition;
					m_interactingCharacter.m_CachedCurrentPosition = m_vInteractPosition;
					if (m_AnimationData != null && m_AnimationData.interactingAnimation != AnimState.COUNT)
					{
						m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.enterAnimation);
						if (flag)
						{
							m_interactingCharacter.m_CharacterAnimator.StartAnimation(m_AnimationData.interactingAnimation);
						}
						else
						{
							m_interactingCharacter.SetFaceDirection(m_NonAnimatingFaceDirection);
						}
					}
				}
				m_interactingCharacter.m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Stand);
				InteractionReadyStart();
				if (m_InteractionObjectAnimator != null && flag)
				{
					SetInteractionObjectAnimatorState(2);
				}
			}
		}
		else if (m_bTransitionFromTarget)
		{
			if (m_fTimer > 0f)
			{
				m_fTimer -= UpdateManager.deltaTime;
				if (m_fTimer <= 0f)
				{
					if (m_AnimationData != null && null != m_interactingCharacter)
					{
						m_interactingCharacter.m_CharacterAnimator.StopAnimation(m_AnimationData.exitAnimation);
					}
					m_fLerpTimer = m_AnimationData.lerpDuration;
					m_fLerpTime = m_fLerpTimer;
					if (m_fLerpTimer <= 0f)
					{
						if (null != m_interactingCharacter)
						{
							m_interactingCharacter.transform.position = m_vExitPosition;
							m_interactingCharacter.m_CachedCurrentPosition = m_vExitPosition;
						}
					}
					else if (m_AnimationData.walkWhilstLerping && null != m_interactingCharacter)
					{
						Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(m_vExitPosition - m_interactingCharacter.transform.position);
						m_interactingCharacter.m_CharacterAnimator.CharacterSpeedChanged(CharacterSpeed.Walk);
						m_interactingCharacter.SetFaceDirection(headAndBodyDirection);
					}
					UpdateInteractionZ_PostTransitionEnd();
				}
			}
			else if (m_fLerpTimer > 0f)
			{
				m_fLerpTimer -= UpdateManager.deltaTime;
				float t2 = Mathf.Clamp01((m_fLerpTime - m_fLerpTimer) / m_fLerpTime);
				if (null != m_interactingCharacter)
				{
					Vector3 vector2 = Vector3.Lerp(m_vInteractPosition, m_vExitPosition, t2);
					m_interactingCharacter.transform.position = vector2;
					m_interactingCharacter.m_CachedCurrentPosition = vector2;
				}
			}
			else
			{
				if (null != m_interactingCharacter)
				{
					m_interactingCharacter.transform.position = m_vExitPosition;
					m_interactingCharacter.m_CachedCurrentPosition = m_vExitPosition;
				}
				m_bTransitionFromTarget = false;
				OnExitInteraction(m_interactingCharacter);
			}
		}
		else if (m_bInteractionReady)
		{
			InteractionReadyUpdate();
		}
		CheckForAnimationStatesNeedingSet();
	}

	private void CheckForAnimationStatesNeedingSet()
	{
		if (m_bAnimationIntegerNeedsSet && m_InteractionObjectAnimator != null && m_InteractionObjectAnimator.isActiveAndEnabled)
		{
			m_InteractionObjectAnimator.playbackTime = 1f;
			m_InteractionObjectAnimator.SetInteger("AnimState", m_CurrentAnimState);
			m_bAnimationIntegerNeedsSet = false;
		}
	}

	public void OnCullingWrapperEnabled()
	{
		CheckForAnimationStatesNeedingSet();
	}

	public virtual void SetInteractionObjectAnimatorState(int state)
	{
		if (state == m_CurrentAnimState)
		{
			return;
		}
		m_CurrentAnimState = state;
		if (m_InteractionObjectAnimator != null)
		{
			if (m_InteractionObjectAnimator.isActiveAndEnabled)
			{
				m_bAnimationIntegerNeedsSet = false;
				m_InteractionObjectAnimator.SetInteger("AnimState", m_CurrentAnimState);
			}
			else
			{
				m_bAnimationIntegerNeedsSet = true;
			}
		}
	}

	public virtual void InteractionReadyStart()
	{
		SendEvent(InteractiveEventType.InteractionReadyStart);
	}

	public virtual void InteractionReadyUpdate()
	{
	}

	public virtual void InteractionReadyEnd(bool interruption = false)
	{
		SendEvent(InteractiveEventType.InteractionReadyEnd);
	}

	public override InteractionType GetInteractionClassType()
	{
		return InteractionType.AnimatedInteractiveObject;
	}

	private void OnDrawGizmosSelected()
	{
		if (!m_bFindNearestInteractionPosition)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(base.transform.position + (Vector3)(Vector2)m_InteractionPositionOffset, 0.2f);
		}
	}

	protected virtual void UpdateInteractionZ_PreTransitionStart()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_Transition_Z_Offset);
		}
	}

	protected virtual void UpdateInteractionZ_Interacting()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_Z_Offset_Interaction);
		}
	}

	protected virtual void UpdateInteractionZ_PostTransitionEnd()
	{
		if (m_VisualTransform != null && m_interactingCharacter != null)
		{
			m_interactingCharacter.SetAnimatedInteractionZ(m_VisualTransform.position.z + m_Transition_Z_Offset);
		}
	}

	public override void SetNormalizedAnimTime(float normalisedTime)
	{
		base.SetNormalizedAnimTime(normalisedTime);
		if (m_InteractionObjectAnimator != null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_InteractionObjectAnimator.GetCurrentAnimatorStateInfo(0);
			if (currentAnimatorStateInfo.shortNameHash == m_NormalizedAnimStateHash)
			{
				m_InteractionObjectAnimator.CrossFade(currentAnimatorStateInfo.fullPathHash, 0f, -1, normalisedTime);
				m_InteractionObjectAnimator.playbackTime = 0f;
			}
		}
	}

	public override void ForceNormalisedAnimTime(float normalisedTime)
	{
		base.ForceNormalisedAnimTime(normalisedTime);
		if (m_InteractionObjectAnimator != null)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_InteractionObjectAnimator.GetCurrentAnimatorStateInfo(0);
			m_InteractionObjectAnimator.CrossFade(currentAnimatorStateInfo.fullPathHash, 0f, -1, normalisedTime);
		}
	}

	public override void ResetNormalizedAnimTime()
	{
		base.ResetNormalizedAnimTime();
		if (m_InteractionObjectAnimator != null)
		{
			m_InteractionObjectAnimator.playbackTime = 1f;
		}
	}

	protected virtual bool CanEnterPlayState(Character localCharacter)
	{
		return true;
	}
}
