using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("GameObject")]
public class DestroyGameObject : ActionTask<Transform>
{
	protected override string info => $"Destroy {base.agentInfo}";

	protected override void OnUpdate()
	{
		Object.Destroy(base.agent.gameObject);
		EndAction();
	}
}
