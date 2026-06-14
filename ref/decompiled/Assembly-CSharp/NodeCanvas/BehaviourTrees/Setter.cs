using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Set", false)]
[Description("Set another Agent for the rest of the Tree dynamicaly from this point and on. All nodes under this will be executed for the new agent")]
[Name("Override Agent")]
[Category("Decorators")]
public class Setter : BTDecorator
{
	public BBParameter<GameObject> newAgent;

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (newAgent.value != null)
		{
			agent = newAgent.value.transform;
		}
		return base.decoratedConnection.Execute(agent, blackboard);
	}
}
