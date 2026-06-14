using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Slate.ActionClips;

[Category("GameObject")]
[Description("The selected Behaviours will be Enabled or Disable (based on state option) on the actor if they are not already")]
public class SetBehavioursActiveState : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length;

	[HideInInspector]
	public List<string> behaviourNames = new List<string>();

	public ActiveState activeState = ActiveState.Enable;

	private Dictionary<Behaviour, bool> originalStates;

	private Dictionary<Behaviour, bool> currentStates;

	private bool temporary;

	public override string info => $"{activeState.ToString()}\n({behaviourNames.Count}) Behaviours";

	public override float length
	{
		get
		{
			return _length;
		}
		set
		{
			_length = value;
		}
	}

	protected override void OnEnter()
	{
		originalStates = new Dictionary<Behaviour, bool>();
		currentStates = new Dictionary<Behaviour, bool>();
		foreach (Behaviour item in from c in base.actor.GetComponents<Behaviour>()
			where behaviourNames.Contains(c.GetType().Name)
			select c)
		{
			if (!originalStates.ContainsKey(item))
			{
				originalStates.Add(item, value: true);
			}
			originalStates[item] = item.enabled;
			if (activeState == ActiveState.Toggle)
			{
				item.enabled = !item.enabled;
			}
			else
			{
				item.enabled = activeState == ActiveState.Enable;
			}
			if (!currentStates.ContainsKey(item))
			{
				currentStates.Add(item, value: true);
			}
			currentStates[item] = item.enabled;
			temporary = length > 0f;
		}
	}

	protected override void OnExit()
	{
		if (!temporary)
		{
			return;
		}
		foreach (KeyValuePair<Behaviour, bool> originalState in originalStates)
		{
			if (originalState.Key != null)
			{
				originalState.Key.enabled = !currentStates[originalState.Key];
			}
		}
	}

	protected override void OnReverseEnter()
	{
		if (!temporary)
		{
			return;
		}
		foreach (KeyValuePair<Behaviour, bool> originalState in originalStates)
		{
			if (originalState.Key != null)
			{
				originalState.Key.enabled = currentStates[originalState.Key];
			}
		}
	}

	protected override void OnReverse()
	{
		foreach (KeyValuePair<Behaviour, bool> originalState in originalStates)
		{
			if (originalState.Key != null)
			{
				originalState.Key.enabled = originalState.Value;
			}
		}
	}
}
