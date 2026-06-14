using UnityEngine;

namespace Slate.ActionClips;

[Category("GameObject")]
[Description("Set or animate the actor gameobject visibility.")]
public class AnimateActorVisibility : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[AnimatableParameter]
	public bool visible;

	private bool wasVisible;

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

	protected override void OnCreate()
	{
		visible = base.actor.activeSelf;
	}

	protected override void OnEnter()
	{
		wasVisible = base.actor.activeSelf;
	}

	protected override void OnUpdate(float time)
	{
		base.actor.SetActive(visible);
	}

	protected override void OnReverse()
	{
		base.actor.SetActive(wasVisible);
	}
}
