using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a blackboard float variable at random between min and max value")]
[Category("✫ Blackboard")]
public class SetFloatRandom : ActionTask
{
	public BBParameter<float> minValue;

	public BBParameter<float> maxValue;

	[BlackboardOnly]
	public BBParameter<float> floatVariable;

	protected override string info => string.Concat("Set ", floatVariable, " Random(", minValue, ", ", maxValue, ")");

	protected override void OnExecute()
	{
		floatVariable.value = Random.Range(minValue.value, maxValue.value);
		EndAction();
	}
}
