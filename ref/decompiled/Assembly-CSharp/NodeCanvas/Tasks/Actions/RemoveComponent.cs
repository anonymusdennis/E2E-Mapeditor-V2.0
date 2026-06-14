using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("GameObject")]
public class RemoveComponent<T> : ActionTask<Transform> where T : Component
{
	protected override string info => $"Remove '{typeof(T).Name}'";

	protected override void OnExecute()
	{
		T component = base.agent.GetComponent<T>();
		if (component != null)
		{
			Object.Destroy(component);
			EndAction(true);
		}
		else
		{
			EndAction(false);
		}
	}
}
