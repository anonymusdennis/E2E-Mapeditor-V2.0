using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Executes the decorated node without taking into account it's return status, thus making it optional to the parent node for whether it returns Success or Failure.\nThis has the same effect as disabling the node, but instead it executes normaly")]
[Category("Decorators")]
[Name("Optional")]
public class Optional : BTDecorator
{
	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (base.status == Status.Resting)
		{
			base.decoratedConnection.Reset();
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		return (base.status != Status.Running) ? Status.Resting : Status.Running;
	}
}
