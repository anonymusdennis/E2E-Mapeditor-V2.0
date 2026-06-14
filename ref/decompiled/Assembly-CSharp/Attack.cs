using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Attack : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	public float m_fKickDistanceSqr = 2.25f;

	private float m_fAttackTimer;

	private bool m_bAttacking;

	private Transform m_CombatantTransform;

	private Character m_CombatantCharacter;

	protected override void OnExecute()
	{
		if (m_combatant.value == null)
		{
			EndAction(false);
			return;
		}
		m_CombatantCharacter = m_combatant.value.m_CharacterResponsible;
		if (m_CombatantCharacter == null)
		{
			EndAction(false);
		}
		else
		{
			m_CombatantTransform = m_CombatantCharacter.m_Transform;
		}
		m_bAttacking = false;
	}

	protected override void OnUpdate()
	{
		if (m_CombatantCharacter.m_bIsKnockedOut)
		{
			EndAction(true);
			return;
		}
		Vector3 position = base.agent.transform.position;
		Vector3 position2 = m_CombatantTransform.position;
		if (!NavMeshUtil.SameFloorCheck(position2.z, position.z))
		{
			EndAction(false);
			return;
		}
		float num = Vector2.SqrMagnitude(position - position2);
		if (num < m_fKickDistanceSqr)
		{
			m_CombatantCharacter.RequestStopInteraction();
		}
		float num2 = base.agent.m_Character.GetEquippedItemAttackRange() * base.agent.m_Character.GetEquippedItemAttackRange();
		if (num > num2)
		{
			EndAction(false);
			return;
		}
		if (!m_bAttacking)
		{
			base.agent.m_Character.Attack();
			m_bAttacking = true;
			m_fAttackTimer = Mathf.Max(0.5f, m_CombatantCharacter.GetItemCombat().m_fRecoveryTime);
			return;
		}
		m_fAttackTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_fAttackTimer < 0f)
		{
			EndAction(false);
		}
	}
}
