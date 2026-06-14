using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

[Category("★T17 Action")]
public class DashAttack : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	public float m_MaxDashDistance = 4f;

	private GraphHitInfo hitInfo = default(GraphHitInfo);

	private Transform m_CombatantTransform;

	protected override void OnExecute()
	{
		if (m_combatant.value == null)
		{
			EndAction(false);
		}
		else
		{
			m_CombatantTransform = m_combatant.value.m_CharacterResponsible.m_Transform;
		}
	}

	protected override void OnUpdate()
	{
		Character characterResponsible = m_combatant.value.m_CharacterResponsible;
		if (characterResponsible.m_bIsKnockedOut)
		{
			base.agent.ForgetEvent(m_combatant.value);
			EndAction(true);
			return;
		}
		if (!NavMeshUtil.NavigationLineOfSight(base.agent.m_AIMovement.GetCurrentGraphIndex(), base.agent.m_Transform.position, characterResponsible.transform.position, ref hitInfo))
		{
			EndAction(false);
			return;
		}
		if ((base.agent.m_Transform.position - m_CombatantTransform.position).magnitude > m_MaxDashDistance)
		{
			EndAction(false);
			return;
		}
		if (characterResponsible.m_bIsHidden)
		{
			EndAction(false);
			return;
		}
		base.agent.m_Character.CalcFaceDirection(m_CombatantTransform.position - base.agent.m_Transform.position);
		if (!base.agent.m_Character.ChargeAttack(attackReleased: false))
		{
			EndAction(true);
		}
	}

	protected override void OnStop()
	{
		base.agent.m_Character.ResetChargeAttack();
	}
}
