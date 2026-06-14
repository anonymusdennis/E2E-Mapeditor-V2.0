using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("Decorators")]
[Description("Returns Running until the assigned condition becomes true")]
[Icon("WaitUntil", false)]
public class WaitUntil : BTDecorator, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	[SerializeField]
	private ConditionTask _condition;

	private bool accessed;

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

	private ConditionTask condition
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

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (condition == null)
		{
			return base.decoratedConnection.Execute(agent, blackboard);
		}
		if (accessed)
		{
			return base.decoratedConnection.Execute(agent, blackboard);
		}
		if (condition.CheckCondition(agent, blackboard))
		{
			accessed = true;
		}
		return (!accessed) ? Status.Running : base.decoratedConnection.Execute(agent, blackboard);
	}

	protected override void OnReset()
	{
		accessed = false;
	}
}
