using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Switch the entire Behaviour Tree of BehaviourTreeOwner")]
[Category("✫ Utility")]
public class SwitchBehaviourTree : ActionTask<BehaviourTreeOwner>
{
	[RequiredField]
	public BBParameter<BehaviourTree> behaviourTree;

	protected override string info => $"Switch Behaviour {behaviourTree}";

	protected override void OnExecute()
	{
		base.agent.SwitchBehaviour(behaviourTree.value);
		EndAction();
	}
}
