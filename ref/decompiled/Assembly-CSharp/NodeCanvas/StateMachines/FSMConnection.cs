using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.StateMachines;

public class FSMConnection : Connection, ITaskAssignable<ConditionTask>, ITaskAssignable
{
	[SerializeField]
	private ConditionTask _condition;

	public ConditionTask condition
	{
		get
		{
			return _condition;
		}
		set
		{
			_condition = value;
		}
	}

	public Task task
	{
		get
		{
			return condition;
		}
		set
		{
			condition = (ConditionTask)value;
		}
	}

	public void PerformTransition()
	{
		(base.graph as FSM).EnterState((FSMState)base.targetNode);
	}
}
