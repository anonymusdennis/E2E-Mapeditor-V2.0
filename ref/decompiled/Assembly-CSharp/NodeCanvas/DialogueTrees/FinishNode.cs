using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Description("End the dialogue in Success or Failure.\nNote: A Dialogue will anyway End in Succcess if it has reached a node without child connections.")]
[Category("Flow Control")]
[Name("Finish")]
public class FinishNode : DTNode
{
	public bool finishState = true;

	public override string name => "FINISH";

	public override int maxOutConnections => 0;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		base.status = (finishState ? Status.Success : Status.Failure);
		base.DLGTree.Stop(finishState);
		return base.status;
	}
}
