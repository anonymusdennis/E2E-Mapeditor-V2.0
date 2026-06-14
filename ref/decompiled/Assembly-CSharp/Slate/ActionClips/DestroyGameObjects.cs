using System.Collections.Generic;
using UnityEngine;

namespace Slate.ActionClips;

[Description("Destroy all gameobjects in the list (PlayMode only).\nYou should NOT use this clip to destroy actors.")]
[Category("Control")]
public class DestroyGameObjects : DirectorActionClip
{
	public List<GameObject> targetObjects = new List<GameObject>();

	public override string info => $"Destroy\n({targetObjects.Count}) GameObjects";

	protected override void OnEnter()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (GameObject targetObject in targetObjects)
		{
			if (targetObject != null)
			{
				Object.Destroy(targetObject);
			}
		}
	}
}
