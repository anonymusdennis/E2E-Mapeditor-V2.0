using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Note that this is slow.\nAction will end in Failure if no objects are found")]
[Category("GameObject")]
public class FindAllWithName : ActionTask
{
	[RequiredField]
	public BBParameter<string> searchName = "GameObject";

	[BlackboardOnly]
	public BBParameter<List<GameObject>> saveAs;

	protected override string info => string.Concat("GetObjects '", searchName, "' as ", saveAs);

	protected override void OnExecute()
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = Object.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in array)
		{
			if (gameObject.name == searchName.value)
			{
				list.Add(gameObject);
			}
		}
		saveAs.value = list;
		EndAction(list.Count != 0);
	}
}
