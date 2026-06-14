using System;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[] { typeof(AnimationTrack) })]
[Name("Animation Clip")]
[Description("All animation clips in the same track, will play at an animation layer equal to their track layer order. Thus, animations in tracks on top will play over animations in tracks bellow. You can trim or loop the animation by scaling the clip.")]
public class PlayAnimationClip : ActorActionClip<Animation>, ICrossBlendable, ISubClipContainable, IDirectable
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn;

	[HideInInspector]
	[SerializeField]
	private float _blendOut;

	[Required]
	public AnimationClip animationClip;

	public float clipOffset;

	[Range(0.1f, 2f)]
	public float playbackSpeed = 1f;

	private TransformSnapshot snapShot;

	private Transform mixTransform;

	private AnimationState state;

	private bool isListClip;

	float ISubClipContainable.subClipOffset
	{
		get
		{
			return clipOffset;
		}
		set
		{
			clipOffset = value;
		}
	}

	public override string info => (!animationClip) ? base.info : animationClip.name;

	public override bool isValid => base.isValid && animationClip != null && animationClip.legacy;

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

	public override float blendIn
	{
		get
		{
			return _blendIn;
		}
		set
		{
			_blendIn = value;
		}
	}

	public override float blendOut
	{
		get
		{
			return _blendOut;
		}
		set
		{
			_blendOut = value;
		}
	}

	private AnimationTrack track => (AnimationTrack)base.parent;

	protected override void OnEnter()
	{
		snapShot = new TransformSnapshot(base.actor.gameObject, TransformSnapshot.StoreMode.ChildrenOnly);
		isListClip = base.actor[animationClip.name] != null;
		if (!isListClip)
		{
			base.actor.AddClip(animationClip, animationClip.name);
		}
		mixTransform = track.GetMixTransform();
		if (mixTransform != null)
		{
			base.actor[animationClip.name].AddMixingTransform(mixTransform, recursive: true);
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		state = base.actor[animationClip.name];
		state.time = deltaTime * playbackSpeed;
		float num = animationClip.length / playbackSpeed;
		if (length <= num)
		{
			state.time = Mathf.Min(state.time - clipOffset, animationClip.length);
			state.wrapMode = WrapMode.Once;
		}
		if (length > num)
		{
			state.time = Mathf.Repeat(state.time - clipOffset, animationClip.length);
			state.wrapMode = WrapMode.Loop;
		}
		state.layer = track.layerOrder;
		state.weight = GetClipWeight(deltaTime) * track.GetAnimationWeight();
		state.blendMode = track.animationBlendMode;
		state.enabled = true;
		base.actor.Sample();
	}

	protected override void OnReverse()
	{
		snapShot.Restore();
		state.enabled = false;
		if (!isListClip)
		{
			base.actor.RemoveClip(animationClip);
		}
	}

	protected override void OnExit()
	{
		state.enabled = false;
	}

	protected override void OnReverseEnter()
	{
		state.enabled = true;
	}
}
