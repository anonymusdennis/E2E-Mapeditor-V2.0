using System;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("GameObject")]
[Obsolete("Use Get Property instead")]
public class GetGameObjectPosition : ActionTask<Transform>
{
	[BlackboardOnly]
	public BBParameter<Vector3> saveAs;

	protected override string info => "Get " + base.agentInfo + " position as " + saveAs;

	protected override void OnExecute()
	{
		saveAs.value = base.agent.position;
		EndAction();
	}
}
