using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Execute the child nodes in order or randonly until the first that returns Success and return Success as well. If none returns Success, then returns Failure.\nIf is Dynamic, then higher priority children Status are revaluated and if one returns Success the Selector will select that one and bail out immediately in Success too")]
[Icon("Selector", false)]
[Color("b3ff7f")]
[Category("Composites")]
public class Selector : BTComposite
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
			case Status.Success:
				if (dynamic && i < lastRunningNodeIndex)
				{
					base.outConnections[lastRunningNodeIndex].Reset();
				}
				return Status.Success;
			}
		}
		return Status.Failure;
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
