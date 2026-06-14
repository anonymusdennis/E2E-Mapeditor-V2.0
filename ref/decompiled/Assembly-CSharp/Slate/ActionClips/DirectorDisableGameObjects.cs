using System.Collections.Generic;
using UnityEngine;

namespace Slate.ActionClips;

[Description("All gameobjects in the list will be disabled if not already")]
[Name("Disable Game Objects")]
[Category("Control")]
public class DirectorDisableGameObjects : DirectorActionClip
{
	public List<GameObject> targetObjects = new List<GameObject>();

	private Dictionary<GameObject, bool> states;

	public override string info => $"Disable\n({targetObjects.Count}) GameObjects";

	protected override void OnEnter()
	{
		states = new Dictionary<GameObject, bool>();
		foreach (GameObject targetObject in targetObjects)
		{
			states[targetObject] = targetObject.activeSelf;
			targetObject.SetActive(value: false);
		}
	}

	protected override void OnReverse()
	{
		foreach (KeyValuePair<GameObject, bool> state in states)
		{
			if (state.Key != null)
			{
				state.Key.SetActive(state.Value);
			}
		}
	}
}
