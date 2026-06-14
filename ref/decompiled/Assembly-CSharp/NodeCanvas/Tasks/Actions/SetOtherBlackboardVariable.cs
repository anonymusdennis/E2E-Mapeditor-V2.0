using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Category("✫ Blackboard")]
[Description("Use this to set a variable on any blackboard by overriding the agent")]
public class SetOtherBlackboardVariable : ActionTask<Blackboard>
{
	[RequiredField]
	public BBParameter<string> targetVariableName;

	public BBObjectParameter newValue;

	protected override string info => $"<b>{targetVariableName.ToString()}</b> = {((newValue == null) ? string.Empty : newValue.ToString())}";

	protected override void OnExecute()
	{
		base.agent.SetValue(targetVariableName.value, newValue.value);
		EndAction();
	}
}
