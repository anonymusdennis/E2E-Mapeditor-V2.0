using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("List", false)]
[Name("Int Iterator")]
[Category("★T17")]
[Description("Iterate a list of ints and execute the child node for each element in the list. Keeps iterating until the Termination Condition is met or the whole list is iterated and return the child node status")]
public class IntIterator : BTDecorator
{
	public enum TerminationConditions
	{
		None,
		FirstSuccess,
		FirstFailure
	}

	[BlackboardOnly]
	[RequiredField]
	public BBParameter<List<int>> targetList;

	[BlackboardOnly]
	public BBParameter<int> current;

	[BlackboardOnly]
	public BBParameter<int> storeIndex;

	public BBParameter<int> maxIteration = -1;

	public TerminationConditions terminationCondition;

	public bool resetIndex = true;

	private int currentIndex;

	private List<int> list => (targetList == null) ? null : targetList.value;

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		if (list == null || list.Count == 0)
		{
			return Status.Failure;
		}
		for (int i = currentIndex; i < list.Count; i++)
		{
			current.value = list[i];
			storeIndex.value = i;
			base.status = base.decoratedConnection.Execute(agent, blackboard);
			if (base.status == Status.Success && terminationCondition == TerminationConditions.FirstSuccess)
			{
				return Status.Success;
			}
			if (base.status == Status.Failure && terminationCondition == TerminationConditions.FirstFailure)
			{
				return Status.Failure;
			}
			if (base.status == Status.Running)
			{
				currentIndex = i;
				return Status.Running;
			}
			if (currentIndex == list.Count - 1 || currentIndex == maxIteration.value - 1)
			{
				return base.status;
			}
			base.decoratedConnection.Reset();
			currentIndex++;
		}
		return Status.Running;
	}

	protected override void OnReset()
	{
		if (resetIndex)
		{
			currentIndex = 0;
		}
	}
}
