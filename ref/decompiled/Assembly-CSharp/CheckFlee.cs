using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

[Description("Conditional returns true if we want to flee")]
[Category("★T17 Events")]
public class CheckFlee : ConditionTask<AICharacter>
{
	public BBParameter<AIEventMemory> m_combatant;

	public float m_fEnergyThreshold = 10f;

	public float m_fHealthThreshold = 10f;

	private float m_fFleeHysteresisTimer;

	private float m_fFleeHysteresisTime = 20f;

	private Character m_CombatantCharacter;

	protected override bool OnCheck()
	{
		if (m_combatant.value == null)
		{
			return false;
		}
		if (base.agent.m_Character.m_CharacterRole != 0)
		{
			return false;
		}
		m_CombatantCharacter = m_combatant.value.m_CharacterResponsible;
		if (m_CombatantCharacter.m_bIsKnockedOut)
		{
			base.agent.ForgetEvent(m_combatant.value);
			return false;
		}
		return RunAwayCheck();
	}

	private bool RunAwayCheck()
	{
		RoutineManager instance = RoutineManager.GetInstance();
		if (instance != null)
		{
			Routines currentRoutineBaseType = instance.GetCurrentRoutineBaseType();
			if (currentRoutineBaseType == Routines.Lockdown || currentRoutineBaseType == Routines.LightsOut)
			{
				RoomBlob myCell = base.agent.m_Character.GetMyCell();
				if (myCell != null && base.agent.m_Character.m_CurrentLocation == myCell)
				{
					return false;
				}
			}
		}
		float heavyAttackEnergyCost = m_CombatantCharacter.GetItemCombat().m_CombatConfig.GetHeavyAttackEnergyCost(base.agent.m_CharacterStats.EnergyLevel);
		if (base.agent.m_CharacterStats.Energy < m_fEnergyThreshold || base.agent.m_CharacterStats.Energy < heavyAttackEnergyCost || base.agent.m_CharacterStats.Health < m_fHealthThreshold)
		{
			m_fFleeHysteresisTimer = m_fFleeHysteresisTime;
		}
		if (m_fFleeHysteresisTimer > 0f)
		{
			m_fFleeHysteresisTimer -= BehaviourTree.CurrentTimeSlicedDeltaTime;
		}
		return m_fFleeHysteresisTimer > 0f;
	}
}
