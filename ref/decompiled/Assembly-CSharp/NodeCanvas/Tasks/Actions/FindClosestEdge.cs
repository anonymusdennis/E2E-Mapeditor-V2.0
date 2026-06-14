using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Actions;

[Description("Find the closes Navigation Mesh position to the target position")]
[Category("Movement")]
[Name("Find Closest NavMesh Edge")]
public class FindClosestEdge : ActionTask
{
	public BBParameter<Vector3> targetPosition;

	[BlackboardOnly]
	public BBParameter<Vector3> saveFoundPosition;

	private NavMeshHit hit;

	protected override void OnExecute()
	{
		if (NavMesh.FindClosestEdge(targetPosition.value, out hit, -1))
		{
			saveFoundPosition.value = hit.position;
			EndAction(true);
		}
		EndAction(false);
	}
}
