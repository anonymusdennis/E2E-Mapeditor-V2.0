using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Name("Move To Target Position")]
[Category("Movement")]
public class MoveToPosition : ActionTask<NavMeshAgent>
{
	public BBParameter<Vector3> targetPosition;

	public BBParameter<float> speed = 3f;

	public float keepDistance = 0.1f;

	private Vector3? lastRequest;

	protected override string info => "GoTo " + targetPosition.ToString();

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		if ((base.agent.transform.position - targetPosition.value).magnitude < base.agent.stoppingDistance + keepDistance)
		{
			EndAction(true);
		}
		else
		{
			Go();
		}
	}

	protected override void OnUpdate()
	{
		Go();
	}

	private void Go()
	{
		Vector3? vector = lastRequest;
		if ((!vector.HasValue || vector.GetValueOrDefault() != targetPosition.value) && !base.agent.SetDestination(targetPosition.value))
		{
			EndAction(false);
			return;
		}
		lastRequest = targetPosition.value;
		if (!base.agent.pathPending && base.agent.remainingDistance <= base.agent.stoppingDistance + keepDistance)
		{
			EndAction(true);
		}
	}

	protected override void OnStop()
	{
		Vector3? vector = lastRequest;
		if (vector.HasValue && base.agent.gameObject.activeSelf)
		{
			base.agent.ResetPath();
		}
		lastRequest = null;
	}

	protected override void OnPause()
	{
		OnStop();
	}
}
