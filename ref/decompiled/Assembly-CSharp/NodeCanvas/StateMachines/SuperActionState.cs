using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Description("The Super Action State provides finer control on when to execute actions. This state is never Finished by it's own if there is any Actions in the OnUpdate list and thus OnFinish transitions will never be called in that case. OnExit Actions are only called for 1 frame when the state exits.")]
public class SuperActionState : FSMState, ISubTasksContainer
{
	[SerializeField]
	private ActionList _onEnterList;

	[SerializeField]
	private ActionList _onUpdateList;

	[SerializeField]
	private ActionList _onExitList;

	private bool enterListFinished;

	public Task[] GetTasks()
	{
		return new Task[3] { _onEnterList, _onUpdateList, _onExitList };
	}

	public override void OnValidate(Graph assignedGraph)
	{
		if (_onEnterList == null)
		{
			_onEnterList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
			_onEnterList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
		}
		if (_onUpdateList == null)
		{
			_onUpdateList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
			_onUpdateList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
		}
		if (_onExitList == null)
		{
			_onExitList = (ActionList)Task.Create(typeof(ActionList), assignedGraph);
			_onExitList.executionMode = ActionList.ActionsExecutionMode.ActionsRunInParallel;
		}
	}

	protected override void OnEnter()
	{
		enterListFinished = false;
		OnUpdate();
	}

	protected override void OnUpdate()
	{
		if (!enterListFinished && _onEnterList.ExecuteAction(base.graphAgent, base.graphBlackboard) != Status.Running)
		{
			enterListFinished = true;
			if (_onUpdateList.actions.Count == 0)
			{
				Finish();
			}
		}
		_onUpdateList.ExecuteAction(base.graphAgent, base.graphBlackboard);
	}

	protected override void OnExit()
	{
		_onEnterList.EndAction(null);
		_onUpdateList.EndAction(null);
		_onExitList.ExecuteAction(base.graphAgent, base.graphBlackboard);
		_onExitList.EndAction(null);
	}

	protected override void OnPause()
	{
		_onEnterList.PauseAction();
		_onUpdateList.PauseAction();
	}
}
