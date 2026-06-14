using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Name("Multiple Task Condition")]
[Description("Will continue with the first child node which condition returns true. The Dialogue Actor selected will be used for the checks")]
[Category("Flow Control")]
public class MultipleConditionNode : DTNode, ISubTasksContainer
{
	public List<ConditionTask> conditions = new List<ConditionTask>();

	public override int maxOutConnections => -1;

	public Task[] GetTasks()
	{
		return conditions.ToArray();
	}

	public override void OnChildConnected(int index)
	{
		conditions.Insert(index, null);
	}

	public override void OnChildDisconnected(int index)
	{
		conditions.RemoveAt(index);
	}

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (base.outConnections.Count == 0)
		{
			return Error("There are no connections on the Dialogue Condition Node");
		}
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			if (conditions[i] == null || conditions[i].CheckCondition(base.finalActor.transform, base.graphBlackboard))
			{
				base.DLGTree.Continue(i);
				return Status.Success;
			}
		}
		Debug.LogWarning("No condition is true. Dialogue Stops");
		base.DLGTree.Stop(success: false);
		return Status.Failure;
	}
}
