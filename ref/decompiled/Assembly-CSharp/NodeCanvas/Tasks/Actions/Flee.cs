using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement")]
[Description("Flees away from the target")]
public class Flee : ActionTask<NavMeshAgent>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> speed = 4f;

	public BBParameter<float> fledDistance = 10f;

	public BBParameter<float> lookAheadDistance = 5f;

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		if ((base.agent.transform.position - target.value.transform.position).magnitude >= fledDistance.value)
		{
			EndAction();
		}
		else
		{
			DoFlee();
		}
	}

	protected override void OnUpdate()
	{
		if (!base.agent.pathPending && (base.agent.transform.position - target.value.transform.position).magnitude >= fledDistance.value)
		{
			EndAction();
		}
		else
		{
			DoFlee();
		}
	}

	private void DoFlee()
	{
		Vector3 destination = base.agent.transform.position + (base.agent.transform.position - target.value.transform.position).normalized * lookAheadDistance.value;
		base.agent.SetDestination(destination);
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
