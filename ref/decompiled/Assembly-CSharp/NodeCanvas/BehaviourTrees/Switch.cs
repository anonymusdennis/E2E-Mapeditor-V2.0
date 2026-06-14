using System;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("IndexSwitcher", false)]
[Color("b3ff7f")]
[Category("Composites")]
[Description("Executes ONE child based on the provided int or enum and return it's status. If 'case' change while a child is running, that child will be interrupted before the new child is executed")]
public class Switch : BTComposite
{
	public enum CaseSelectionMode
	{
		IndexBased,
		EnumBased
	}

	public enum OutOfRangeMode
	{
		ReturnFailure,
		LoopIndex
	}

	[BlackboardOnly]
	public BBObjectParameter enumCase = new BBObjectParameter(typeof(Enum));

	public BBParameter<int> intCase;

	public CaseSelectionMode selectionMode = CaseSelectionMode.EnumBased;

	public OutOfRangeMode outOfRangeMode;

	private int current;

	private int runningIndex;

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.outConnections.Count == 0)
		{
			return Status.Failure;
		}
		if (selectionMode == CaseSelectionMode.IndexBased)
		{
			current = intCase.value;
			if (outOfRangeMode == OutOfRangeMode.LoopIndex)
			{
				current = Mathf.Abs(current) % base.outConnections.Count;
			}
		}
		else
		{
			current = (int)enumCase.value;
		}
		if (runningIndex != current)
		{
			base.outConnections[runningIndex].Reset();
		}
		if (current < 0 || current >= base.outConnections.Count)
		{
			return Status.Failure;
		}
		base.status = base.outConnections[current].Execute(agent, blackboard);
		if (base.status == Status.Running)
		{
			runningIndex = current;
		}
		return base.status;
	}
}
