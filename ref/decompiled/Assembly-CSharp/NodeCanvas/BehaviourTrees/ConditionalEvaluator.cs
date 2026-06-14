using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Execute and return the child node status if the condition is true, otherwise return Failure. The condition is evaluated only once in the first Tick and when the node is not already Running unless it is set as 'Dynamic' in which case it will revaluate even while running")]
[Icon("Accessor", false)]
[Name("Conditional")]
[Category("Decorators")]
public class ConditionalEvaluator : BTDecorator, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	public bool isDynamic;

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
		if (isDynamic)
		{
			if (condition.CheckCondition(agent, blackboard))
			{
				return base.decoratedConnection.Execute(agent, blackboard);
			}
			base.decoratedConnection.Reset();
			return Status.Failure;
		}
		if (base.status != Status.Running && condition.CheckCondition(agent, blackboard))
		{
			accessed = true;
		}
		return accessed ? base.decoratedConnection.Execute(agent, blackboard) : Status.Failure;
	}

	protected override void OnReset()
	{
		accessed = false;
	}
}
