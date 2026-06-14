using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Timeout", false)]
[Description("Interupts decorated child node and returns Failure if the child node is still Running after the timeout period")]
[Category("Decorators")]
public class Timeout : BTDecorator
{
	public BBParameter<float> timeout = 1f;

	private float timer;

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		if (base.status == Status.Running)
		{
			timer += UpdateManager.deltaTime;
		}
		if (timer < timeout.value)
		{
			return base.status;
		}
		timer = 0f;
		base.decoratedConnection.Reset();
		return Status.Failure;
	}

	protected override void OnReset()
	{
		timer = 0f;
	}
}
