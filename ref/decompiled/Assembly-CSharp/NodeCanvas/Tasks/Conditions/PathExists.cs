using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.AI;

namespace NodeCanvas.Tasks.Conditions;

[Description("Check if a path exists for the agent and optionaly save the resulting path positions")]
[Category("Movement")]
public class PathExists : ConditionTask<NavMeshAgent>
{
	public BBParameter<Vector3> targetPosition;

	[BlackboardOnly]
	public BBParameter<List<Vector3>> savePathAs;

	protected override bool OnCheck()
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		base.agent.CalculatePath(targetPosition.value, navMeshPath);
		savePathAs.value = navMeshPath.corners.ToList();
		return navMeshPath.status == NavMeshPathStatus.PathComplete;
	}
}
