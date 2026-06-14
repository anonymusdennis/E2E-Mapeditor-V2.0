using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Name("Go To")]
[Color("00b9e8")]
[Category("Flow Control")]
[Description("Jump to another Dialogue node. Usefull if that other node is far away to connect, but otherwise it's exactly the same")]
public class GoToNode : DTNode
{
	[SerializeField]
	private DTNode _targetNode;

	public override int maxOutConnections => 0;

	public override string name => "<GO TO>";

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (_targetNode == null)
		{
			return Error("Target node of GOTO node is null");
		}
		base.DLGTree.EnterNode(_targetNode);
		return Status.Success;
	}
}
