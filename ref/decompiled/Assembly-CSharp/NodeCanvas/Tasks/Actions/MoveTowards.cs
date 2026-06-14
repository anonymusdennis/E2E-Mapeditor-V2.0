using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement")]
public class MoveTowards : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> speed = 2f;

	[SliderField(0.1f, 10f)]
	public BBParameter<float> stopDistance = 0.1f;

	public bool repeat;

	protected override void OnExecute()
	{
		Move();
	}

	protected override void OnUpdate()
	{
		Move();
	}

	private void Move()
	{
		if ((base.agent.position - target.value.transform.position).magnitude > stopDistance.value)
		{
			base.agent.position = Vector3.MoveTowards(base.agent.position, target.value.transform.position, speed.value * UpdateManager.deltaTime);
		}
		else if (!repeat)
		{
			EndAction();
		}
	}
}
