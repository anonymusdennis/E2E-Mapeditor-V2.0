using UnityEngine;

namespace Slate.ActionClips;

[Category("GameObject")]
[Description("Set the actor active state (visibility) for a period of time or permantentely if length is zero.")]
public class SetActorActiveState : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length;

	public ActiveState activeState = ActiveState.Enable;

	private bool lastState;

	private bool currentState;

	private bool temporary;

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

	public override string info => $"{activeState} Actor";

	protected override void OnEnter()
	{
		lastState = base.actor.activeSelf;
		if (activeState == ActiveState.Toggle)
		{
			base.actor.SetActive(!base.actor.activeSelf);
		}
		else
		{
			base.actor.SetActive(activeState == ActiveState.Enable);
		}
		currentState = base.actor.activeSelf;
		temporary = length > 0f;
	}

	protected override void OnExit()
	{
		if (temporary)
		{
			base.actor.SetActive(!currentState);
		}
	}

	protected override void OnReverseEnter()
	{
		if (temporary)
		{
			base.actor.SetActive(currentState);
		}
	}

	protected override void OnReverse()
	{
		base.actor.SetActive(lastState);
	}
}
