using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Blackboard")]
[Description("Use this to get a variable on any blackboard by overriding the agent")]
public class GetOtherBlackboardVariable : ActionTask<Blackboard>
{
	[RequiredField]
	public BBParameter<string> targetVariableName;

	[BlackboardOnly]
	public BBObjectParameter saveAs;

	protected override string info => $"{saveAs} = {targetVariableName}";

	protected override void OnExecute()
	{
		Variable variable = base.agent.GetVariable(targetVariableName.value);
		saveAs.value = variable.value;
		EndAction();
	}
}
