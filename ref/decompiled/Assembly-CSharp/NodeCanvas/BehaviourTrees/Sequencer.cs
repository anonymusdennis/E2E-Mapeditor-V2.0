using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Sequencer", false)]
[Color("bf7fff")]
[Category("Composites")]
[Description("Execute the child nodes in order or randonly and return Success if all children return Success, else return Failure\nIf is Dynamic, higher priority child status is revaluated. If a child returns Failure the Sequencer will bail out immediately in Failure too.")]
public class Sequencer : BTComposite
{
	public bool dynamic;

	public bool random;

	private int lastRunningNodeIndex;

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		for (int i = ((!dynamic) ? lastRunningNodeIndex : 0); i < base.outConnections.Count; i++)
		{
			base.status = base.outConnections[i].Execute(agent, blackboard);
			switch (base.status)
			{
			case Status.Running:
				if (dynamic && i < lastRunningNodeIndex)
				{
					base.outConnections[lastRunningNodeIndex].Reset();
				}
				lastRunningNodeIndex = i;
				return Status.Running;
			case Status.Failure:
				if (dynamic && i < lastRunningNodeIndex)
				{
					base.outConnections[lastRunningNodeIndex].Reset();
				}
				return Status.Failure;
			}
		}
		return Status.Success;
	}

	protected override void OnReset()
	{
		lastRunningNodeIndex = 0;
		if (random)
		{
			base.outConnections = Shuffle(base.outConnections);
		}
	}

	public override void OnChildDisconnected(int index)
	{
		if (index != 0 && index == lastRunningNodeIndex)
		{
			lastRunningNodeIndex--;
		}
	}

	public override void OnGraphStarted()
	{
		OnReset();
	}

	private List<Connection> Shuffle(List<Connection> list)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int index = (int)Mathf.Floor(Random.value * (float)(num + 1));
			Connection value = list[num];
			list[num] = list[index];
			list[index] = value;
		}
		return list;
	}
}
