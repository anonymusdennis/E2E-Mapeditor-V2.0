using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Description("Makes the agent wander randomly within the navigation map")]
[Category("Movement")]
public class Wander : ActionTask<NavMeshAgent>
{
	public BBParameter<float> speed = 4f;

	public BBParameter<float> stoppingDistance = 0.1f;

	public BBParameter<float> minWanderDistance = 5f;

	public BBParameter<float> maxWanderDistance = 20f;

	public bool repeat = true;

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		base.agent.stoppingDistance = stoppingDistance.value;
		DoWander();
	}

	protected override void OnUpdate()
	{
		if (!base.agent.pathPending && base.agent.remainingDistance <= base.agent.stoppingDistance)
		{
			if (repeat)
			{
				DoWander();
			}
			else
			{
				EndAction();
			}
		}
	}

	private void DoWander()
	{
		minWanderDistance.value = Mathf.Min(minWanderDistance.value, maxWanderDistance.value);
		Vector3 vector = Random.insideUnitSphere * maxWanderDistance.value + base.agent.transform.position;
		while ((vector - base.agent.transform.position).sqrMagnitude < minWanderDistance.value)
		{
			vector = Random.insideUnitSphere * maxWanderDistance.value + base.agent.transform.position;
		}
		base.agent.SetDestination(vector);
	}

	protected override void OnPause()
	{
		OnStop();
	}

	protected override void OnStop()
	{
		if (base.agent.gameObject.activeSelf)
		{
			base.agent.ResetPath();
		}
	}
}
