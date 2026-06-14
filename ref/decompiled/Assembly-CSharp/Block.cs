using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

[Category("★T17 Action")]
public class Block : ActionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	private Transform m_CombatantTransform;

	private Character m_CombatantCharacter;

	public float m_fBlockMinTime = 5f;

	public float m_fBlockMaxTime = 5f;

	private float m_fBlockTimer;

	private float m_fEnterDistance = 2f;

	private float m_fExitDistance = 3f;

	protected override void OnExecute()
	{
		if (m_combatant.value == null)
		{
			EndAction(false);
			return;
		}
		m_CombatantCharacter = m_combatant.value.m_CharacterResponsible;
		m_CombatantTransform = m_CombatantCharacter.m_Transform;
		if (GetDistance() > m_fEnterDistance)
		{
			EndAction(false);
		}
		else
		{
			m_fBlockTimer = Random.Range(m_fBlockMinTime, m_fBlockMaxTime);
		}
	}

	private float GetDistance()
	{
		return Vector3.Distance(base.agent.m_Transform.position, m_CombatantTransform.position);
	}

	protected override void OnUpdate()
	{
		if (m_CombatantCharacter.m_bIsKnockedOut)
		{
			base.agent.ForgetEvent(m_combatant.value);
			EndAction(true);
		}
		if (GetDistance() > m_fExitDistance)
		{
			EndAction(false);
			return;
		}
		base.agent.m_Character.CombatBlock(doBlock: true);
		base.agent.m_Character.CalcFaceDirection(m_CombatantTransform.position - base.agent.m_Transform.position);
		m_fBlockTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		if (m_fBlockTimer <= 0f)
		{
			PretendHitOtherCharacter();
			EndAction(true);
		}
	}

	private void PretendHitOtherCharacter()
	{
		if (m_CombatantCharacter == null || m_CombatantCharacter.m_CharacterStats == null || m_CombatantCharacter.m_CharacterStats.m_bIsPlayer)
		{
			return;
		}
		AIPlayer aIPlayer = m_CombatantCharacter as AIPlayer;
		if (aIPlayer != null)
		{
			Character character = base.agent.m_Character;
			if (character != null && character.m_CharacterEventManager != null)
			{
				AIEvent attackingAIEvent = base.agent.m_Character.m_CharacterEventManager.GetAttackingAIEvent();
				aIPlayer.m_AICharacter.AddEvent(attackingAIEvent);
			}
		}
	}

	protected override void OnStop()
	{
		base.agent.m_Character.CombatBlock(doBlock: false);
	}
}
