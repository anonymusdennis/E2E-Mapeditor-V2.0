using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class GoTo : ActionTask<AICharacter>
{
	public BBParameter<GameObject> m_Target;

	public BBParameter<InteractiveObject> m_InteractiveObjTarget;

	public BBParameter<AIEventMemory> m_EventTarget;

	public BBParameter<Vector3> m_VectorTarget;

	public BBParameter<bool> m_Running = false;

	public BBParameter<bool> m_bSkipLastNode = false;

	public BBParameter<bool> m_GoToInteractionPoint = false;

	public BBParameter<int> m_OverrideFaceDirection;

	public bool m_bAllowTeleport;

	public bool m_bAllowVentLayer = true;

	public bool m_bSetVectorPosition = true;

	private Vector3 m_TargetVector;

	private bool m_bMovingToPosition;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelled;

	private const float CLOSE_ENOUGH_DISTANCE = 0.2f;

	protected override string info
	{
		get
		{
			string text = string.Empty;
			if (m_Target != null && m_Target.name != null)
			{
				text = m_Target.name;
			}
			else if (m_InteractiveObjTarget != null && m_InteractiveObjTarget.name != null)
			{
				text = m_InteractiveObjTarget.name;
			}
			else if (m_EventTarget != null && m_EventTarget.name != null)
			{
				text = m_EventTarget.name;
			}
			else if (m_VectorTarget != null)
			{
				text = m_VectorTarget.name;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "NULL";
			}
			return "Go To: " + text;
		}
	}

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelled = OnPathCancelled;
		return base.OnInit();
	}

	public void OnTargetReached()
	{
		m_bMovingToPosition = false;
		if (!m_OverrideFaceDirection.isNone && m_OverrideFaceDirection.value != 0)
		{
			FacingDirectionIncInvalid value = (FacingDirectionIncInvalid)m_OverrideFaceDirection.value;
			base.agent.m_Character.SetFaceDirection(value);
		}
		else
		{
			Vector3 vector = m_TargetVector - base.agent.m_Transform.position;
			Directionx4 headAndBodyDirection = Direction.VectorToNearestDirectionx4(vector);
			base.agent.m_Character.SetFaceDirection(headAndBodyDirection);
		}
		EndAction(true);
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
	}

	protected override void OnExecute()
	{
		m_bMovingToPosition = false;
		if (m_Target != null && m_Target.value != null)
		{
			m_TargetVector = m_Target.value.transform.position;
		}
		else if (m_InteractiveObjTarget != null && m_InteractiveObjTarget.value != null)
		{
			m_TargetVector = m_InteractiveObjTarget.value.transform.position;
			if (m_GoToInteractionPoint.value)
			{
				AnimatedInteraction animatedInteraction = m_InteractiveObjTarget.value as AnimatedInteraction;
				if (animatedInteraction != null)
				{
					Vector3 interactionPositionOffset = animatedInteraction.m_InteractionPositionOffset;
					interactionPositionOffset.z = 0f;
					m_TargetVector += interactionPositionOffset;
				}
			}
		}
		else if (m_EventTarget != null && m_EventTarget.value != null)
		{
			m_TargetVector = m_EventTarget.value.m_vEventLocation;
		}
		else
		{
			if (m_VectorTarget == null || !m_bSetVectorPosition)
			{
				EndAction(false);
				return;
			}
			m_TargetVector = m_VectorTarget.value;
		}
		if (!m_bAllowVentLayer && FloorManager.GetInstance() != null)
		{
			FloorManager.GetInstance().EnsureNonVentPosition(ref m_TargetVector);
		}
		if (Vector3.Distance(base.agent.m_Character.m_CachedCurrentPosition, m_TargetVector) <= 0.2f)
		{
			EndAction(true);
		}
		else
		{
			base.agent.SetRunning(m_Running.value);
		}
	}

	protected override void OnUpdate()
	{
		base.agent.SetRunning(m_Running.value);
		if (!m_bMovingToPosition)
		{
			m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelled, m_TargetVector, 0.2f, throttled: false, m_bAllowTeleport, m_bSkipLastNode.value);
		}
	}

	protected override void OnStop()
	{
		base.agent.SetRunning(running: false);
	}
}
