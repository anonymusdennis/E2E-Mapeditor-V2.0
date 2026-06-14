using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Reposition : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	public float m_ExitDistance = 2.5f;

	private Transform m_CombatantTransform;

	private Character m_CombatantCharacter;

	private bool m_bMovingToPosition;

	private T17_ABPath.PathCallback m_OnTargetReachedDel;

	private T17_ABPath.PathCallback m_OnPathCancelledDel;

	protected override string OnInit()
	{
		m_OnTargetReachedDel = OnTargetReached;
		m_OnPathCancelledDel = OnPathCancelled;
		return base.OnInit();
	}

	public void OnTargetReached()
	{
		m_bMovingToPosition = false;
		EndAction(true);
	}

	public void OnPathCancelled()
	{
		m_bMovingToPosition = false;
		EndAction(false);
	}

	protected override void OnExecute()
	{
		if (m_combatant.value == null)
		{
			EndAction(false);
			return;
		}
		m_CombatantCharacter = m_combatant.value.m_CharacterResponsible;
		m_CombatantTransform = m_CombatantCharacter.m_Transform;
	}

	protected override void OnUpdate()
	{
		base.agent.SetRunning(running: false);
		if (m_CombatantCharacter.m_bIsKnockedOut)
		{
			base.agent.ForgetEvent(m_combatant.value);
			EndAction(true);
		}
		if (Vector2.SqrMagnitude(m_CombatantTransform.position - base.agent.m_Transform.position) > m_ExitDistance)
		{
			EndAction(false);
		}
		else if (!m_bMovingToPosition)
		{
			int currentGraphIndex = base.agent.m_AIMovement.GetCurrentGraphIndex();
			Vector3 nearbyPosition = NavMeshUtil.GetNearbyPosition(base.agent.m_Transform.position, currentGraphIndex);
			m_bMovingToPosition = base.agent.m_AIMovement.TravelToPosition(m_OnTargetReachedDel, m_OnPathCancelledDel, nearbyPosition, 0.2f);
		}
	}
}
