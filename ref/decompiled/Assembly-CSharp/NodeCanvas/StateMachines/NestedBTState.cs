using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[Description("Execute a Behaviour Tree OnEnter. OnExit that Behavior Tree will be stoped or paused based on the relevant specified setting. You can optionaly specify a Success Event and a Failure Event which will be sent when the BT's root node status returns either of the two. If so, use alongside with a CheckEvent on a transition.")]
[Category("Nested")]
[Name("BehaviourTree")]
public class NestedBTState : FSMState, IGraphAssignable
{
	public enum BTExecutionMode
	{
		Once,
		Repeat
	}

	public enum BTExitMode
	{
		StopAndRestart,
		PauseAndResume
	}

	[SerializeField]
	private BBParameter<BehaviourTree> _nestedBT;

	public BTExecutionMode executionMode = BTExecutionMode.Repeat;

	public BTExitMode exitMode;

	public string successEvent;

	public string failureEvent;

	private Dictionary<BehaviourTree, BehaviourTree> instances = new Dictionary<BehaviourTree, BehaviourTree>();

	Graph IGraphAssignable.nestedGraph
	{
		get
		{
			return nestedBT;
		}
		set
		{
			nestedBT = (BehaviourTree)value;
		}
	}

	public BehaviourTree nestedBT
	{
		get
		{
			return _nestedBT.value;
		}
		set
		{
			_nestedBT.value = value;
		}
	}

	Graph[] IGraphAssignable.GetInstances()
	{
		return instances.Values.ToArray();
	}

	protected override void OnEnter()
	{
		if (nestedBT == null)
		{
			Finish(inSuccess: false);
			return;
		}
		CheckInstance();
		nestedBT.repeat = executionMode == BTExecutionMode.Repeat;
		nestedBT.updateInterval = 0f;
		nestedBT.StartGraph(base.graphAgent, base.graphBlackboard, autoUpdate: false, OnFinish);
	}

	protected override void OnUpdate()
	{
		nestedBT.UpdateGraph();
		if (!string.IsNullOrEmpty(successEvent) && nestedBT.rootStatus == Status.Success)
		{
			nestedBT.Stop();
		}
		if (!string.IsNullOrEmpty(failureEvent) && nestedBT.rootStatus == Status.Failure)
		{
			nestedBT.Stop(success: false);
		}
	}

	private void OnFinish(bool success)
	{
		if (base.status == Status.Running)
		{
			if (!string.IsNullOrEmpty(successEvent) && success)
			{
				SendEvent(new EventData(successEvent));
			}
			if (!string.IsNullOrEmpty(failureEvent) && !success)
			{
				SendEvent(new EventData(failureEvent));
			}
			Finish(success);
		}
	}

	protected override void OnExit()
	{
		if (IsInstance(nestedBT) && nestedBT.isRunning)
		{
			if (exitMode == BTExitMode.StopAndRestart)
			{
				nestedBT.Stop();
			}
			else
			{
				nestedBT.Pause();
			}
		}
	}

	protected override void OnPause()
	{
		if (IsInstance(nestedBT) && nestedBT.isRunning)
		{
			nestedBT.Pause();
		}
	}

	private bool IsInstance(BehaviourTree bt)
	{
		return instances.Values.Contains(bt);
	}

	private void CheckInstance()
	{
		if (!IsInstance(nestedBT))
		{
			BehaviourTree value = null;
			if (!instances.TryGetValue(nestedBT, out value))
			{
				value = Graph.Clone(nestedBT);
				instances[nestedBT] = value;
			}
			value.agent = base.graphAgent;
			value.blackboard = base.graphBlackboard;
			nestedBT = value;
		}
	}
}
