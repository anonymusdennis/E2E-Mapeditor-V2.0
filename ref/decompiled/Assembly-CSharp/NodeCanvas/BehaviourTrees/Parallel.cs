using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Parallel", false)]
[Color("ff64cb")]
[Category("Composites")]
[Description("Execute all child nodes once but simultaneously and return Success or Failure depending on the selected ParallelPolicy.\nIf set to Dynamic, child nodes are repeated until the Policy set is met, or until all children have had a chance to complete at least once.")]
public class Parallel : BTComposite
{
	public enum ParallelPolicy
	{
		FirstFailure,
		FirstSuccess,
		FirstSuccessOrFailure
	}

	public ParallelPolicy policy;

	public bool dynamic;

	private readonly List<Connection> finishedConnections = new List<Connection>();

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		Status status = Status.Resting;
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			if (!dynamic && finishedConnections.Contains(base.outConnections[i]))
			{
				continue;
			}
			if (base.outConnections[i].status != Status.Running && finishedConnections.Contains(base.outConnections[i]))
			{
				base.outConnections[i].Reset();
			}
			base.status = base.outConnections[i].Execute(agent, blackboard);
			if (status == Status.Resting)
			{
				if (base.status == Status.Failure && (policy == ParallelPolicy.FirstFailure || policy == ParallelPolicy.FirstSuccessOrFailure))
				{
					status = Status.Failure;
				}
				if (base.status == Status.Success && (policy == ParallelPolicy.FirstSuccess || policy == ParallelPolicy.FirstSuccessOrFailure))
				{
					status = Status.Success;
				}
			}
			if (base.status != Status.Running && !finishedConnections.Contains(base.outConnections[i]))
			{
				finishedConnections.Add(base.outConnections[i]);
			}
		}
		if (status != Status.Resting)
		{
			ResetRunning();
			return status;
		}
		if (finishedConnections.Count == base.outConnections.Count)
		{
			ResetRunning();
			switch (policy)
			{
			case ParallelPolicy.FirstFailure:
				return Status.Success;
			case ParallelPolicy.FirstSuccess:
				return Status.Failure;
			}
		}
		return Status.Running;
	}

	protected override void OnReset()
	{
		finishedConnections.Clear();
	}

	private void ResetRunning()
	{
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			if (base.outConnections[i].status == Status.Running)
			{
				base.outConnections[i].Reset();
			}
		}
	}
}
