using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[AddComponentMenu("NodeCanvas/Behaviour Tree Owner")]
public class BehaviourTreeOwner : GraphOwner<BehaviourTree>
{
	public bool repeat
	{
		get
		{
			return !(base.behaviour != null) || base.behaviour.repeat;
		}
		set
		{
			if (base.behaviour != null)
			{
				base.behaviour.repeat = value;
			}
		}
	}

	public float updateInterval
	{
		get
		{
			return (!(base.behaviour != null)) ? 0f : base.behaviour.updateInterval;
		}
		set
		{
			if (base.behaviour != null)
			{
				base.behaviour.updateInterval = value;
			}
		}
	}

	public bool useUpdateManager
	{
		get
		{
			return base.behaviour != null && base.behaviour.useUpdateManager;
		}
		set
		{
			if (base.behaviour != null)
			{
				base.behaviour.useUpdateManager = value;
			}
		}
	}

	public Status rootStatus => (!(base.behaviour != null)) ? Status.Resting : base.behaviour.rootStatus;

	public Status Tick()
	{
		if (base.behaviour == null)
		{
			Debug.LogWarning("There is no Behaviour Tree assigned", base.gameObject);
			return Status.Resting;
		}
		return base.behaviour.Tick(this, blackboard);
	}
}
