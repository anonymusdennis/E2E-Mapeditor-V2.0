using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("State Decorator")]
[Category("★T17")]
public class StateDecorator : BTDecorator
{
	private bool m_bSet;

	protected AICharacter m_AICharacter;

	protected override void OnReset()
	{
		if (m_bSet)
		{
			m_bSet = false;
			if (m_AICharacter != null)
			{
				m_AICharacter.m_bBTStateReset = true;
			}
			OnExit();
		}
		base.OnReset();
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (!m_bSet || m_AICharacter.m_bBTStateDirty)
		{
			m_bSet = true;
			if (m_AICharacter == null)
			{
				m_AICharacter = agent.GetComponent<AICharacter>();
			}
			OnEnter();
		}
		Status status = base.decoratedConnection.Execute(agent, blackboard);
		if (m_bSet)
		{
			OnUpdate();
		}
		if (m_bSet && status != Status.Running)
		{
			m_bSet = false;
			OnExit();
		}
		return status;
	}

	protected virtual void OnEnter()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void OnExit()
	{
	}
}
