using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Animator")]
[Name("Set IK")]
[EventReceiver(new string[] { "OnAnimatorIK" })]
public class MecanimSetIK : ActionTask<Animator>
{
	public AvatarIKGoal IKGoal;

	[RequiredField]
	public BBParameter<GameObject> goal;

	public BBParameter<float> weight;

	protected override string info => string.Concat("Set '", IKGoal, "' ", goal);

	public void OnAnimatorIK()
	{
		base.agent.SetIKPositionWeight(IKGoal, weight.value);
		base.agent.SetIKPosition(IKGoal, goal.value.transform.position);
		EndAction();
	}
}
