using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("GameObject")]
public class InstantiateGameObject : ActionTask<Transform>
{
	public BBParameter<Transform> parent;

	public BBParameter<Vector3> clonePosition;

	public BBParameter<Vector3> cloneRotation;

	[BlackboardOnly]
	public BBParameter<GameObject> saveCloneAs;

	protected override string info => string.Concat("Instantiate ", base.agentInfo, " under ", (!parent.value) ? "World" : parent.ToString(), " at ", clonePosition, " as ", saveCloneAs);

	protected override void OnExecute()
	{
		GameObject gameObject = Object.Instantiate(base.agent.gameObject);
		gameObject.transform.SetParent(parent.value);
		gameObject.transform.position = clonePosition.value;
		gameObject.transform.eulerAngles = cloneRotation.value;
		saveCloneAs.value = gameObject;
		EndAction();
	}
}
