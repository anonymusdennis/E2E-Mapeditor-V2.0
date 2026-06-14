using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Set a blackboard integer variable")]
[Name("Set Integer")]
[Category("✫ Blackboard")]
public class SetInt : ActionTask
{
	[BlackboardOnly]
	public BBParameter<int> valueA;

	public OperationMethod Operation;

	public BBParameter<int> valueB;

	protected override string info => string.Concat(valueA, OperationTools.GetOperationString(Operation), valueB);

	protected override void OnExecute()
	{
		valueA.value = OperationTools.Operate(valueA.value, valueB.value, Operation);
		EndAction();
	}
}
