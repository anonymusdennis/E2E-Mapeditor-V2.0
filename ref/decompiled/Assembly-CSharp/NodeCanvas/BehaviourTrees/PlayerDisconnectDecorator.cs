using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("★T17")]
[Description("Player Disconnect Check - Forget event and do not run child behaviour if the Character responsible disconnects")]
public class PlayerDisconnectDecorator : BTDecorator
{
	public BBParameter<AIEventMemory> m_CurrentEvent;

	public bool m_CheckCharacterResponsible = true;

	public override string name => (!m_CheckCharacterResponsible) ? ("Character Disconnect (Char Target)" + '\n' + "[" + m_CurrentEvent.name + "]") : ("Character Disconnect (Char Responsible)" + '\n' + "[" + m_CurrentEvent.name + "]");

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		AIEventMemory value = m_CurrentEvent.value;
		if (value != null)
		{
			bool flag = false;
			if ((!m_CheckCharacterResponsible) ? (value.m_TargetCharacter != null && value.m_TargetCharacter.GetIsDisabled()) : (value.m_CharacterResponsible != null && value.m_CharacterResponsible.GetIsDisabled()))
			{
				AICharacter component = agent.GetComponent<AICharacter>();
				component.ForgetEvent(value);
				base.decoratedConnection.Reset();
				return Status.Failure;
			}
		}
		return base.decoratedConnection.Execute(agent, blackboard);
	}
}
