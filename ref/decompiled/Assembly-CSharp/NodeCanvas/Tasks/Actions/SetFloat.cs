using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a blackboard float variable")]
[Category("✫ Blackboard")]
public class SetFloat : ActionTask
{
	[BlackboardOnly]
	public BBParameter<float> valueA;

	public OperationMethod Operation;

	public BBParameter<float> valueB;

	protected override string info => string.Concat(valueA, OperationTools.GetOperationString(Operation), valueB);

	protected override void OnExecute()
	{
		valueA.value = OperationTools.Operate(valueA.value, valueB.value, Operation);
		EndAction(true);
	}
}
