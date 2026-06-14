using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using NodeCanvas.StateMachines;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[Category("Nested")]
[Icon("FSM", false)]
[Description("NestedFSM can be assigned an entire FSM. This node will return Running for as long as the FSM is Running. If a Success or Failure State is selected, then it will return Success or Failure as soon as the Nested FSM enters that state at which point the FSM will also be stoped. If the Nested FSM ends otherwise, this node will return Success.")]
[Name("FSM")]
public class NestedFSM : BTNode, IGraphAssignable
{
	[SerializeField]
	private BBParameter<FSM> _nestedFSM;

	private Dictionary<FSM, FSM> instances = new Dictionary<FSM, FSM>();

	public string successState;

	public string failureState;

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

	public override string name => base.name.ToUpper();

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

	protected override Status OnExecute(Component agent, IBlackboard blackboard)
	{
		if (nestedFSM == null || nestedFSM.primeNode == null)
		{
			return Status.Failure;
		}
		if (base.status == Status.Resting)
		{
			CheckInstance();
		}
		if (base.status == Status.Resting || nestedFSM.isPaused)
		{
			base.status = Status.Running;
			nestedFSM.StartGraph(agent, blackboard, autoUpdate: false, OnFSMFinish);
		}
		if (base.status == Status.Running)
		{
			nestedFSM.UpdateGraph();
		}
		if (!string.IsNullOrEmpty(successState) && nestedFSM.currentStateName == successState)
		{
			nestedFSM.Stop();
			return Status.Success;
		}
		if (!string.IsNullOrEmpty(failureState) && nestedFSM.currentStateName == failureState)
		{
			nestedFSM.Stop(success: false);
			return Status.Failure;
		}
		return base.status;
	}

	private void OnFSMFinish(bool success)
	{
		if (base.status == Status.Running)
		{
			base.status = (success ? Status.Success : Status.Failure);
		}
	}

	protected override void OnReset()
	{
		if (IsInstance(nestedFSM))
		{
			nestedFSM.Stop();
		}
	}

	public override void OnGraphPaused()
	{
		if (IsInstance(nestedFSM))
		{
			nestedFSM.Pause();
		}
	}

	public override void OnGraphStoped()
	{
		if (IsInstance(nestedFSM))
		{
			nestedFSM.Stop();
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
