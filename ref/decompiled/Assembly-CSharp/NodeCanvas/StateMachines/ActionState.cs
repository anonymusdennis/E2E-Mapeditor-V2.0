using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Description("Execute a number of Action Tasks OnEnter. All actions will be stoped OnExit. This state is Finished when all Actions are finished as well")]
[Name("Action State")]
public class ActionState : FSMState, ITaskAssignable
{
	[SerializeField]
	private ActionList _actionList;

	[SerializeField]
	private bool _repeatStateActions;

	public Task task
	{
		get
		{
			return actionList;
		}
		set
		{
			actionList = (ActionList)value;
		}
	}

	public ActionList actionList
	{
		get
		{
			return _actionList;
		}
		set
		{
			_actionList = value;
		}
	}

	public bool repeatStateActions
	{
		get
		{
			return _repeatStateActions;
		}
		set
		{
			_repeatStateActions = value;
		}
	}

	public override void OnValidate(Graph assignedGraph)
	{
		if (actionList == null)
		{
			actionList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
			actionList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
		}
	}

	protected override void OnEnter()
	{
		OnUpdate();
	}

	protected override void OnUpdate()
	{
		if (actionList.ExecuteAction(base.graphAgent, base.graphBlackboard) != Status.Running && !repeatStateActions)
		{
			Finish();
		}
	}

	protected override void OnExit()
	{
		actionList.EndAction(null);
	}

	protected override void OnPause()
	{
		actionList.PauseAction();
	}
}
