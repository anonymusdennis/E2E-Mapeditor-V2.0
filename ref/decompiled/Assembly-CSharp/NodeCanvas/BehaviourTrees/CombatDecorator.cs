using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.BehaviourTrees;

[Category("★T17")]
[Description("Decorator to put AI into combat state")]
public class CombatDecorator : StateDecorator
{
	public BBParameter<AIEventMemory> m_Target;

	protected override void OnEnter()
	{
		if (m_Target != null && m_Target.value != null)
		{
			m_AICharacter.m_bEnteredCombat = true;
			m_AICharacter.m_Character.SetCharacterTarget(m_Target.value.m_CharacterResponsible);
		}
	}

	protected override void OnUpdate()
	{
		if (m_Target != null && m_Target.value != null)
		{
			m_AICharacter.m_Character.SetCharacterTarget(m_Target.value.m_CharacterResponsible);
		}
	}

	protected override void OnExit()
	{
		m_AICharacter.m_Character.SetCharacterTarget(null);
	}
}
