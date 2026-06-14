using System;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[] { typeof(AnimatorTrack) })]
[Name("Animation Clip")]
public class PlayAnimatorClip : ActorActionClip<Animator>, ICrossBlendable, ISubClipContainable, IDirectable
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn;

	[HideInInspector]
	[SerializeField]
	private float _blendOut;

	[HideInInspector]
	public AnimationClip animationClip;

	public float clipOffset;

	[Required]
	public GameObject m_AnimationOriginModel;

	[HideInInspector]
	[SerializeField]
	public string m_AnimationSourcePrefabLocation;

	[SerializeField]
	[HideInInspector]
	public string m_AnimationName = string.Empty;

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

	public override bool isValid => base.isValid && animationClip != null && !animationClip.legacy;

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

	private AnimatorTrack track => (AnimatorTrack)base.parent;

	protected override void OnEnter()
	{
		track.EnableClip(this);
	}

	protected override void OnReverseEnter()
	{
		track.EnableClip(this);
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		track.UpdateClip(this, time - clipOffset, previousTime - clipOffset, GetClipWeight(time));
	}

	protected override void OnExit()
	{
		track.DisableClip(this);
	}

	protected override void OnReverse()
	{
		track.DisableClip(this);
	}
}
