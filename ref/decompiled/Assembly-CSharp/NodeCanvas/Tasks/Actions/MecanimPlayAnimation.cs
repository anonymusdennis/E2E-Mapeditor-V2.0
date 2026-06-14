using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Name("Play Animation")]
[Category("Animator")]
public class MecanimPlayAnimation : ActionTask<Animator>
{
	public BBParameter<int> layerIndex;

	[RequiredField]
	public BBParameter<string> stateName;

	[SliderField(0, 1)]
	public float transitTime = 0.25f;

	public bool waitUntilFinish;

	private AnimatorStateInfo stateInfo;

	private bool played;

	protected override string info => "Anim '" + stateName.ToString() + "'";

	protected override void OnExecute()
	{
		if (string.IsNullOrEmpty(stateName.value))
		{
			EndAction();
			return;
		}
		played = false;
		AnimatorStateInfo currentAnimatorStateInfo = base.agent.GetCurrentAnimatorStateInfo(layerIndex.value);
		base.agent.CrossFade(stateName.value, transitTime / currentAnimatorStateInfo.length, layerIndex.value);
	}

	protected override void OnUpdate()
	{
		stateInfo = base.agent.GetCurrentAnimatorStateInfo(layerIndex.value);
		if (waitUntilFinish)
		{
			if (stateInfo.IsName(stateName.value))
			{
				played = true;
				if (base.elapsedTime >= stateInfo.length / base.agent.speed)
				{
					EndAction();
				}
			}
			else if (played)
			{
				EndAction();
			}
		}
		else if (base.elapsedTime >= transitTime)
		{
			EndAction();
		}
	}
}
