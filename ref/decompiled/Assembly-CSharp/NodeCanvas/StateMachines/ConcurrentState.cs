using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Color("ff64cb")]
[Description("Execute a number of Action Tasks and in parallel to any other state, as soon as the FSM is started. All Action Tasks will prematurely be stoped if the FSM stops as well.\nThis is not a state per se and thus it has no transitions as well as it can't be Entered by transitions.")]
[Name("Concurrent")]
public class ConcurrentState : FSMState, IUpdatable, ITaskAssignable
{
	[SerializeField]
	private ActionList _actionList;

	[SerializeField]
	private bool _repeatStateActions;

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

	public override string name => base.name.ToUpper();

	public override int maxInConnections => 0;

	public override int maxOutConnections => 0;

	public override bool allowAsPrime => false;

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
		Update();
	}

	public new void Update()
	{
		if ((base.status == Status.Resting || base.status == Status.Running) && actionList.ExecuteAction(base.graphAgent, base.graphBlackboard) != Status.Running && !repeatStateActions)
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
