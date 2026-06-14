using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Find the closest game object of tag to the agent")]
[Category("GameObject")]
public class FindClosestWithTag : ActionTask<Transform>
{
	[TagField]
	[RequiredField]
	public BBParameter<string> searchTag;

	public BBParameter<bool> ignoreChildren;

	[BlackboardOnly]
	public BBParameter<GameObject> saveObjectAs;

	[BlackboardOnly]
	public BBParameter<float> saveDistanceAs;

	protected override void OnExecute()
	{
		List<GameObject> list = GameObject.FindGameObjectsWithTag(searchTag.value).ToList();
		if (list.Count == 0)
		{
			saveObjectAs.value = null;
			saveDistanceAs.value = 0f;
			EndAction(false);
			return;
		}
		GameObject value = null;
		float num = float.PositiveInfinity;
		foreach (GameObject item in list)
		{
			if (!(item.transform == base.agent) && (!ignoreChildren.value || !item.transform.IsChildOf(base.agent)))
			{
				float num2 = Vector3.Distance(item.transform.position, base.agent.position);
				if (num2 < num)
				{
					num = num2;
					value = item;
				}
			}
		}
		saveObjectAs.value = value;
		saveDistanceAs.value = num;
		EndAction();
	}
}
