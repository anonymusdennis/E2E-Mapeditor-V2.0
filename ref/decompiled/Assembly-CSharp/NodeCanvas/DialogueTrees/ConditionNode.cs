using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Category("Flow Control")]
[Name("Task Condition")]
[Description("Execute the first child node if a Condition is true, or the second one if that Condition is false. The Actor selected is used for the Condition check")]
public class ConditionNode : DTNode, ITaskAssignable<ConditionTask>, ITaskAssignable
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

	public override int maxOutConnections => 2;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (base.outConnections.Count == 0)
		{
			return Error("There are no connections on the Dialogue Condition Node");
		}
		if (condition == null)
		{
			return Error("There is no Conidition on the Dialoge Condition Node");
		}
		bool flag = condition.CheckCondition(base.finalActor.transform, base.graphBlackboard);
		base.status = (flag ? Status.Success : Status.Failure);
		base.DLGTree.Continue((!flag) ? 1 : 0);
		return base.status;
	}
}
