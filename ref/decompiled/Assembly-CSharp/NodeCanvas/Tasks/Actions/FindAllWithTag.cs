using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Action will end in Failure if no objects are found")]
[Category("GameObject")]
public class FindAllWithTag : ActionTask
{
	[TagField]
	[RequiredField]
	public string searchTag = "Untagged";

	[BlackboardOnly]
	public BBParameter<List<GameObject>> saveAs;

	protected override string info => "GetObjects '" + searchTag + "' as " + saveAs;

	protected override void OnExecute()
	{
		saveAs.value = GameObject.FindGameObjectsWithTag(searchTag).ToList();
		EndAction(saveAs.value.Count != 0);
	}
}
