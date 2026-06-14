using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Description("Executes an action and returns Success or Failure. Returns Running until the action is finished")]
[Name("Action")]
[Icon("Action", false)]
public class ActionNode : BTNode, ITaskAssignable<ActionTask>, ITaskAssignable
{
	[SerializeField]
	private ActionTask _action;

	public Task task
	{
		get
		{
			return action;
		}
		set
		{
			action = (ActionTask)value;
		}
	}

	public ActionTask action
	{
		get
		{
			return _action;
		}
		set
		{
			_action = value;
		}
	}

	public override string name => base.name.ToUpper();

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (action == null)
		{
			return Status.Failure;
		}
		if (base.status == Status.Resting || base.status == Status.Running)
		{
			return action.ExecuteAction(agent, blackboard);
		}
		return base.status;
	}

	protected override void OnReset()
	{
		if (action != null)
		{
			action.EndAction(null);
		}
	}

	public override void OnGraphPaused()
	{
		if (action != null)
		{
			action.PauseAction();
		}
	}
}
