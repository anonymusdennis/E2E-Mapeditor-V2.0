using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("Decorators")]
[Description("Remap the child node's status to another status. Used to either invert the child's return status or to always return a specific status.")]
[Icon("Remap", false)]
[Name("Remap")]
public class Remapper : BTDecorator
{
	public enum RemapStatus
	{
		Failure,
		Success
	}

	public RemapStatus successRemap = RemapStatus.Success;

	public RemapStatus failureRemap;

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (base.decoratedConnection == null)
		{
			return Status.Resting;
		}
		base.status = base.decoratedConnection.Execute(agent, blackboard);
		return base.status switch
		{
			Status.Success => (Status)successRemap, 
			Status.Failure => (Status)failureRemap, 
			_ => base.status, 
		};
	}
}
