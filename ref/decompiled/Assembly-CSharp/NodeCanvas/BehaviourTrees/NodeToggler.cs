using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Name("Node Toggler")]
[DoNotList]
[Category("Mutators (beta)")]
[Description("Enable, Disable or Toggle one or more nodes with provided tag. In practise their incomming connections are disabled\nBeta Feature!")]
public class NodeToggler : BTNode
{
	public enum ToggleMode
	{
		Enable,
		Disable,
		Toggle
	}

	public ToggleMode toggleMode = ToggleMode.Toggle;

	public string targetNodeTag;

	private List<Node> targetNodes;

	public override void OnGraphStarted()
	{
		targetNodes = base.graph.GetNodesWithTag<Node>(targetNodeTag);
	}

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (string.IsNullOrEmpty(targetNodeTag))
		{
			return Status.Failure;
		}
		if (targetNodes.Count == 0)
		{
			return Status.Failure;
		}
		if (toggleMode == ToggleMode.Enable)
		{
			foreach (Node targetNode in targetNodes)
			{
				targetNode.inConnections[0].isActive = true;
			}
		}
		if (toggleMode == ToggleMode.Disable)
		{
			foreach (Node targetNode2 in targetNodes)
			{
				targetNode2.inConnections[0].isActive = false;
			}
		}
		if (toggleMode == ToggleMode.Toggle)
		{
			foreach (Node targetNode3 in targetNodes)
			{
				targetNode3.inConnections[0].isActive = !targetNode3.inConnections[0].isActive;
			}
		}
		return Status.Success;
	}
}
