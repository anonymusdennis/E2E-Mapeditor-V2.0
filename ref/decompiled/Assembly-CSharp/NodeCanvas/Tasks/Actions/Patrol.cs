using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement")]
[Description("Move Randomly or Progressively between various game object positions taken from the list provided")]
public class Patrol : ActionTask<NavMeshAgent>
{
	public enum PatrolMode
	{
		Progressive,
		Random
	}

	[RequiredField]
	public BBParameter<List<GameObject>> targetList;

	public BBParameter<PatrolMode> patrolMode = PatrolMode.Random;

	public BBParameter<float> speed = 3f;

	public float keepDistance = 0.1f;

	private int index = -1;

	private Vector3? lastRequest;

	protected override string info => $"{patrolMode} Patrol {targetList}";

	protected override void OnExecute()
	{
		if (targetList.value.Count == 0)
		{
			EndAction(false);
			return;
		}
		if (targetList.value.Count == 1)
		{
			index = 0;
		}
		else if (patrolMode.value == PatrolMode.Random)
		{
			int num;
			for (num = Random.Range(0, targetList.value.Count); num == index; num = Random.Range(0, targetList.value.Count))
			{
			}
			index = num;
		}
		else if (patrolMode.value == PatrolMode.Progressive)
		{
			index = (int)Mathf.Repeat(index + 1, targetList.value.Count);
		}
		GameObject gameObject = targetList.value[index];
		if (gameObject == null)
		{
			Debug.LogWarning("List's game object is null on MoveToFromList Action");
			EndAction(false);
			return;
		}
		Vector3 position = gameObject.transform.position;
		base.agent.speed = speed.value;
		if ((base.agent.transform.position - position).magnitude < base.agent.stoppingDistance + keepDistance)
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
		Vector3 position = targetList.value[index].transform.position;
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
		lastRequest = null;
		if (base.agent.gameObject.activeSelf)
		{
			base.agent.ResetPath();
		}
	}

	protected override void OnPause()
	{
		OnStop();
	}

	public override void OnDrawGizmosSelected()
	{
		if (!base.agent || targetList.value == null)
		{
			return;
		}
		foreach (GameObject item in targetList.value)
		{
			if ((bool)item)
			{
				Gizmos.DrawSphere(item.transform.position, 0.1f);
			}
		}
	}
}
