using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Name("FSM")]
[Description("Execute a nested FSM OnEnter and Stop that FSM OnExit. This state is Finished when the nested FSM is finished as well")]
[Category("Nested")]
public class NestedFSMState : FSMState, IGraphAssignable
{
	[SerializeField]
	protected BBParameter<FSM> _nestedFSM;

	private Dictionary<FSM, FSM> instances = new Dictionary<FSM, FSM>();

	Graph IGraphAssignable.nestedGraph
	{
		get
		{
			return nestedFSM;
		}
		set
		{
			nestedFSM = (FSM)value;
		}
	}

	public FSM nestedFSM
	{
		get
		{
			return _nestedFSM.value;
		}
		set
		{
			_nestedFSM.value = value;
		}
	}

	Graph[] IGraphAssignable.GetInstances()
	{
		return instances.Values.ToArray();
	}

	protected override void OnEnter()
	{
		if (nestedFSM == null)
		{
			Finish(inSuccess: false);
			return;
		}
		CheckInstance();
		nestedFSM.StartGraph(base.graphAgent, base.graphBlackboard, autoUpdate: false, base.Finish);
	}

	protected override void OnUpdate()
	{
		nestedFSM.UpdateGraph();
	}

	protected override void OnExit()
	{
		if (IsInstance(nestedFSM) && (nestedFSM.isRunning || nestedFSM.isPaused))
		{
			nestedFSM.Stop();
		}
	}

	protected override void OnPause()
	{
		if (IsInstance(nestedFSM))
		{
			nestedFSM.Pause();
		}
	}

	private bool IsInstance(FSM fsm)
	{
		return instances.Values.Contains(fsm);
	}

	private void CheckInstance()
	{
		if (!IsInstance(nestedFSM))
		{
			FSM value = null;
			if (!instances.TryGetValue(nestedFSM, out value))
			{
				value = Graph.Clone(nestedFSM);
				instances[nestedFSM] = value;
			}
			value.agent = base.graphAgent;
			value.blackboard = base.graphBlackboard;
			nestedFSM = value;
		}
	}
}
