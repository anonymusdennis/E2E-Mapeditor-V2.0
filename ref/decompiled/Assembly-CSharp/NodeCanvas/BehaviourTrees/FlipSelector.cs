using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Icon("FlipSelector", false)]
[Color("b3ff7f")]
[Description("Works like a normal Selector, but when a child node returns Success, that child will be moved to the end.\nAs a result, previously Failed children will always be checked first and recently Successful children last")]
[Category("Composites")]
public class FlipSelector : BTComposite
{
	private int current;

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		for (int i = current; i < base.outConnections.Count; i++)
		{
			base.status = base.outConnections[i].Execute(agent, blackboard);
			if (base.status == Status.Running)
			{
				current = i;
				return Status.Running;
			}
			if (base.status == Status.Success)
			{
				SendToBack(i);
				return Status.Success;
			}
		}
		return Status.Failure;
	}

	private void SendToBack(int i)
	{
		Connection item = base.outConnections[i];
		base.outConnections.RemoveAt(i);
		base.outConnections.Add(item);
	}

	protected override void OnReset()
	{
		current = 0;
	}
}
