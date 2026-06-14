using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Name("Set Layer Weight")]
[Category("Animator")]
public class MecanimSetLayerWeight : ActionTask<Animator>
{
	public BBParameter<int> layerIndex;

	[SliderField(0, 1)]
	public BBParameter<float> layerWeight;

	[SliderField(0, 1)]
	public float transitTime;

	private float currentValue;

	protected override string info => string.Concat("Set Layer ", layerIndex, ", weight ", layerWeight);

	protected override void OnExecute()
	{
		currentValue = base.agent.GetLayerWeight(layerIndex.value);
	}

	protected override void OnUpdate()
	{
		base.agent.SetLayerWeight(layerIndex.value, Mathf.Lerp(currentValue, layerWeight.value, base.elapsedTime / transitTime));
		if (base.elapsedTime >= transitTime)
		{
			EndAction(true);
		}
	}
}
