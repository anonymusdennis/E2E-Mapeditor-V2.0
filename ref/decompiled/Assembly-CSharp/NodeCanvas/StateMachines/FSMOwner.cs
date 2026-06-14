using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.StateMachines;

[AddComponentMenu("NodeCanvas/FSM Owner")]
public class FSMOwner : GraphOwner<FSM>
{
	public string currentStateName => (!(base.behaviour != null)) ? null : base.behaviour.currentStateName;

	public string previousStateName => (!(base.behaviour != null)) ? null : base.behaviour.previousStateName;

	public FSMState TriggerState(string stateName)
	{
		if (base.behaviour != null)
		{
			return base.behaviour.TriggerState(stateName);
		}
		return null;
	}

	public string[] GetStateNames()
	{
		if (base.behaviour != null)
		{
			return base.behaviour.GetStateNames();
		}
		return null;
	}
}
