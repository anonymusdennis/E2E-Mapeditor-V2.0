using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[GraphInfo(packageName = "NodeCanvas", docsURL = "http://nodecanvas.paradoxnotion.com/documentation/", resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/", forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/")]
public class FSM : Graph
{
	private bool hasInitialized;

	private List<IUpdatable> updatableNodes;

	private List<AnyState> anyStates;

	private List<ConcurrentState> concurentStates;

	public FSMState currentState { get; private set; }

	public FSMState previousState { get; private set; }

	public string currentStateName => (currentState == null) ? null : currentState.name;

	public string previousStateName => (previousState == null) ? null : previousState.name;

	public override Type baseNodeType => typeof(FSMState);

	public override bool requiresAgent => true;

	public override bool requiresPrimeNode => true;

	public override bool autoSort => false;

	public override bool useLocalBlackboard => false;

	private event Action<IState> CallbackEnter;

	private event Action<IState> CallbackStay;

	private event Action<IState> CallbackExit;

	protected override void OnGraphStarted()
	{
		if (!hasInitialized)
		{
			hasInitialized = true;
			GatherDelegates();
			updatableNodes = new List<IUpdatable>();
			anyStates = new List<AnyState>();
			concurentStates = new List<ConcurrentState>();
			for (int i = 0; i < base.allNodes.Count; i++)
			{
				Node node = base.allNodes[i];
				if (node is IUpdatable)
				{
					updatableNodes.Add((IUpdatable)node);
				}
				if (node is AnyState)
				{
					anyStates.Add((AnyState)node);
					(node as AnyState).Execute(base.agent, base.blackboard);
				}
				if (node is ConcurrentState)
				{
					concurentStates.Add((ConcurrentState)node);
					(node as ConcurrentState).Execute(base.agent, base.blackboard);
				}
			}
		}
		else
		{
			for (int j = 0; j < anyStates.Count; j++)
			{
				anyStates[j].Execute(base.agent, base.blackboard);
			}
			for (int k = 0; k < concurentStates.Count; k++)
			{
				concurentStates[k].Execute(base.agent, base.blackboard);
			}
		}
		EnterState((previousState != null) ? previousState : ((FSMState)base.primeNode));
	}

	protected override void OnGraphUnpaused()
	{
		EnterState((previousState != null) ? previousState : ((FSMState)base.primeNode));
	}

	protected override void OnGraphUpdate()
	{
		if (currentState == null)
		{
			Stop(success: false);
			return;
		}
		if (currentState.status != Status.Running && currentState.outConnections.Count == 0 && (updatableNodes == null || updatableNodes.Count == 0 || !updatableNodes.Any((IUpdatable n) => n is AnyState)))
		{
			Stop();
			return;
		}
		for (int i = 0; i < updatableNodes.Count; i++)
		{
			updatableNodes[i].Update();
		}
		currentState.Update();
		if (this.CallbackStay != null && currentState != null && currentState.status == Status.Running)
		{
			this.CallbackStay(currentState);
		}
	}

	protected override void OnGraphStoped()
	{
		if (currentState != null)
		{
			if (this.CallbackExit != null)
			{
				this.CallbackExit(currentState);
			}
			currentState.Finish();
			currentState.Reset();
		}
		previousState = null;
		currentState = null;
	}

	protected override void OnGraphPaused()
	{
		previousState = currentState;
		currentState = null;
	}

	public bool EnterState(FSMState newState)
	{
		if (!base.isRunning)
		{
			Debug.LogWarning("Tried to EnterState on an FSM that was not running", this);
			return false;
		}
		if (newState == null)
		{
			Debug.LogWarning("Tried to Enter Null State");
			return false;
		}
		if (currentState != null)
		{
			if (this.CallbackExit != null)
			{
				this.CallbackExit(currentState);
			}
			currentState.Finish();
			currentState.Reset();
		}
		previousState = currentState;
		currentState = newState;
		if (this.CallbackEnter != null)
		{
			this.CallbackEnter(currentState);
		}
		currentState.Execute(base.agent, base.blackboard);
		return true;
	}

	public FSMState TriggerState(string stateName)
	{
		FSMState stateWithName = GetStateWithName(stateName);
		if (stateWithName != null)
		{
			EnterState(stateWithName);
			return stateWithName;
		}
		Debug.LogWarning("No State with name '" + stateName + "' found on FSM '" + base.name + "'");
		return null;
	}

	public string[] GetStateNames()
	{
		return (from n in base.allNodes
			where n.allowAsPrime
			select n.name).ToArray();
	}

	public FSMState GetStateWithName(string name)
	{
		return (FSMState)base.allNodes.Find((Node n) => n.allowAsPrime && n.name == name);
	}

	private void GatherDelegates()
	{
		MonoBehaviour[] components = base.agent.gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour mono in components)
		{
			Type[] paramTypes = new Type[1] { typeof(IState) };
			MethodInfo enterMethod = mono.GetType().RTGetMethod("OnStateEnter", paramTypes);
			MethodInfo stayMethod = mono.GetType().RTGetMethod("OnStateUpdate", paramTypes);
			MethodInfo exitMethod = mono.GetType().RTGetMethod("OnStateExit", paramTypes);
			if (enterMethod != null)
			{
				try
				{
					CallbackEnter += enterMethod.RTCreateDelegate<Action<IState>>(mono);
				}
				catch
				{
					CallbackEnter += delegate(IState m)
					{
						enterMethod.Invoke(mono, new object[1] { m });
					};
				}
			}
			if (stayMethod != null)
			{
				try
				{
					CallbackStay += stayMethod.RTCreateDelegate<Action<IState>>(mono);
				}
				catch
				{
					CallbackStay += delegate(IState m)
					{
						stayMethod.Invoke(mono, new object[1] { m });
					};
				}
			}
			if (exitMethod == null)
			{
				continue;
			}
			try
			{
				CallbackExit += exitMethod.RTCreateDelegate<Action<IState>>(mono);
			}
			catch
			{
				CallbackExit += delegate(IState m)
				{
					exitMethod.Invoke(mono, new object[1] { m });
				};
			}
		}
	}
}
