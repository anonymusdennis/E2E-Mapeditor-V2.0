using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("Priority", false)]
[Color("b3ff7f")]
[Category("Composites")]
[Description("Used for Utility AI, the Priority Selector executes the child with the highest priority value. If it fails, the Prioerity Selector will continue with the next highest priority child until one Succeeds, or until all Fail (similar to how a normal Selector does).")]
public class PrioritySelector : BTComposite
{
	public List<BBParameter<float>> priorities = new List<BBParameter<float>>();

	private List<Connection> orderedConnections = new List<Connection>();

	private int current;

	public override string name => base.name.ToUpper();

	public override void OnChildConnected(int index)
	{
		priorities.Insert(index, new BBParameter<float>
		{
			value = 1f,
			bb = base.graphBlackboard
		});
	}

	public override void OnChildDisconnected(int index)
	{
		priorities.RemoveAt(index);
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.status == Status.Resting)
		{
			orderedConnections = base.outConnections.OrderBy((Connection c) => priorities[base.outConnections.IndexOf(c)].value).Reverse().ToList();
		}
		for (int i = current; i < orderedConnections.Count; i++)
		{
			base.status = orderedConnections[i].Execute(agent, blackboard);
			if (base.status == Status.Success)
			{
				return Status.Success;
			}
			if (base.status == Status.Running)
			{
				current = i;
				return Status.Running;
			}
		}
		return Status.Failure;
	}

	protected override void OnReset()
	{
		current = 0;
	}
}
