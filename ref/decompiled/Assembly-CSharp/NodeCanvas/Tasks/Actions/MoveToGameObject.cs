using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Name("Move To Target")]
[Category("Movement")]
public class MoveToGameObject : ActionTask<NavMeshAgent>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> speed = 3f;

	public float keepDistance = 0.1f;

	private Vector3? lastRequest;

	protected override string info => "GoTo " + target.ToString();

	protected override void OnExecute()
	{
		base.agent.speed = speed.value;
		if ((base.agent.transform.position - target.value.transform.position).magnitude < base.agent.stoppingDistance + keepDistance)
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
		Vector3 position = target.value.transform.position;
		if (lastRequest != position && !base.agent.SetDestination(position))
		{
			EndAction(false);
			return;
		}
		lastRequest = position;
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

	public override void OnDrawGizmos()
	{
		if (target.value != null)
		{
			Gizmos.DrawWireSphere(target.value.transform.position, keepDistance);
		}
	}
}
