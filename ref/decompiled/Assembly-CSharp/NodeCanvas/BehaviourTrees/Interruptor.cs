using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Interrupt the child node and return Failure if the condition is or becomes true while running. Otherwise execute and return the child Status")]
[Icon("Interruptor", false)]
[Category("Decorators")]
[Name("Interrupt")]
public class Interruptor : BTDecorator, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	[SerializeField]
	private ConditionTask _condition;

	public ConditionTask condition
	{
		get
		{
			return _condition;
		}
		set
		{
			_condition = value;
		}
	}

	public Task task
	{
		get
		{
			return condition;
		}
		set
		{
			condition = (ConditionTask)value;
		}
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (condition == null || !condition.CheckCondition(agent, blackboard))
		{
			return base.decoratedConnection.Execute(agent, blackboard);
		}
		if (base.decoratedConnection.status == Status.Running)
		{
			base.decoratedConnection.Reset();
		}
		return Status.Failure;
	}
}
