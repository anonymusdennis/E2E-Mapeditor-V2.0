using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("Movement")]
public class RotateTowards : ActionTask<Transform>
{
	[RequiredField]
	public BBParameter<GameObject> target;

	public BBParameter<float> speed = 2f;

	[SliderField(1, 180)]
	public BBParameter<float> angleDifference = 5f;

	public bool repeat;

	protected override void OnExecute()
	{
		Rotate();
	}

	protected override void OnUpdate()
	{
		Rotate();
	}

	private void Rotate()
	{
		if (Vector3.Angle(target.value.transform.position - base.agent.position, base.agent.forward) > angleDifference.value)
		{
			Vector3 vector = target.value.transform.position - base.agent.position;
			base.agent.rotation = Quaternion.LookRotation(Vector3.RotateTowards(base.agent.forward, vector, speed.value * UpdateManager.deltaTime, 0f));
		}
		else if (!repeat)
		{
			EndAction();
		}
	}
}
