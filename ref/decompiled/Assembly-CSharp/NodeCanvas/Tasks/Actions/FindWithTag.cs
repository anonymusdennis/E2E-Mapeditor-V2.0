using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Category("GameObject")]
public class FindWithTag : ActionTask
{
	[TagField]
	[RequiredField]
	public string searchTag = "Untagged";

	[BlackboardOnly]
	public BBParameter<GameObject> saveAs;

	protected override string info => "GetObject '" + searchTag + "' as " + saveAs;

	protected override void OnExecute()
	{
		saveAs.value = GameObject.FindWithTag(searchTag);
		EndAction(true);
	}
}
