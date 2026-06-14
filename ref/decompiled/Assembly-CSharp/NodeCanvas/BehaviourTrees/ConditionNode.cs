using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Check a condition and return Success or Failure")]
[Icon("Condition", false)]
[Name("Condition")]
public class ConditionNode : BTNode, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	[SerializeField]
	private ConditionTask _condition;

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

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (condition != null)
		{
			return condition.CheckCondition(agent, blackboard) ? Status.Success : Status.Failure;
		}
		return Status.Failure;
	}
}
