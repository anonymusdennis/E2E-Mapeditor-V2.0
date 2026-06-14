using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Shield", false)]
[Description("Protect the decorated child from running if another Guard with the same token is already guarding (Running) that token.\nGuarding is global for all of the agent's Behaviour Trees.")]
[Name("Guard")]
[Category("Decorators")]
public class Guard : BTDecorator
{
	public enum GuardMode
	{
		ReturnFailure,
		WaitUntilReleased
	}

	public BBParameter<string> token;

	public GuardMode ifGuarded;

	private bool isGuarding;

	private static readonly Dictionary<GameObject, List<Guard>> guards = new Dictionary<GameObject, List<Guard>>();

	private static List<Guard> AgentGuards(Component agent)
	{
		return guards[agent.gameObject];
	}

	public override void OnGraphStarted()
	{
		SetGuards(base.graphAgent);
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Failure;
		}
		if (agent != base.graphAgent)
		{
			SetGuards(agent);
		}
		for (int i = 0; i < AgentGuards(agent).Count; i++)
		{
			Guard guard = AgentGuards(agent)[i];
			if (guard != this && guard.isGuarding && guard.token.value == token.value)
			{
				return (ifGuarded != 0) ? Status.Running : Status.Failure;
			}
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		if (base.status == Status.Running)
		{
			isGuarding = true;
			return Status.Running;
		}
		isGuarding = false;
		return base.status;
	}

	protected override void OnReset()
	{
		isGuarding = false;
	}

	private void SetGuards(Component guardAgent)
	{
		if (!guards.ContainsKey(guardAgent.gameObject))
		{
			guards[guardAgent.gameObject] = new List<Guard>();
		}
		if (!AgentGuards(guardAgent).Contains(this) && !string.IsNullOrEmpty(token.value))
		{
			AgentGuards(guardAgent).Add(this);
		}
	}
}
