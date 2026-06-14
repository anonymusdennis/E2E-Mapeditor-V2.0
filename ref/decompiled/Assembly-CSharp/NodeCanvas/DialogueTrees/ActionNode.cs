using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Description("Execute an Action Task for the Dialogue Actor selected.")]
[Name("Task Action")]
public class ActionNode : DTNode, ITaskAssignable<ActionTask>, ITaskAssignable
{
	[SerializeField]
	private ActionTask _action;

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

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (action == null)
		{
			return Error("Action is null on Dialogue Action Node");
		}
		base.status = Status.Running;
		StartCoroutine(UpdateAction(base.finalActor.transform));
		return base.status;
	}

	private IEnumerator UpdateAction(Component actionAgent)
	{
		while (base.status == Status.Running)
		{
			Status actionStatus = action.ExecuteAction(actionAgent, base.graphBlackboard);
			if (actionStatus != Status.Running)
			{
				OnActionEnd(actionStatus == Status.Success);
				break;
			}
			yield return null;
		}
	}

	private void OnActionEnd(bool success)
	{
		if (success)
		{
			base.status = Status.Success;
			base.DLGTree.Continue();
		}
		else
		{
			base.status = Status.Failure;
			base.DLGTree.Stop(success: false);
		}
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
