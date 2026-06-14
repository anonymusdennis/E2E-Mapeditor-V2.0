using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a blackboard boolean variable at random between min and max value")]
[Category("✫ Blackboard")]
public class SetBooleanRandom : ActionTask
{
	[BlackboardOnly]
	public BBParameter<bool> boolVariable;

	protected override string info => string.Concat("Set ", boolVariable, " Random");

	protected override void OnExecute()
	{
		boolVariable.value = ((Random.Range(0, 2) != 0) ? true : false);
		EndAction();
	}
}
