using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions;

[Category("Animator")]
[Name("Is In Transition")]
public class MecanimIsInTransition : ConditionTask<Animator>
{
	public BBParameter<int> layerIndex;

	protected override string info => "Mec.Is In Transition";

	protected override bool OnCheck()
	{
		return base.agent.IsInTransition(layerIndex.value);
	}
}
