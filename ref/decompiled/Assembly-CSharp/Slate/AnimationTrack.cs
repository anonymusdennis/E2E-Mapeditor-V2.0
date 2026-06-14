using System;
using System.Linq;
using UnityEngine;

namespace Slate;

[Description("The Animation Track works with the legacy 'Animation' Component. Each Animation Track represents a different layer of the animation system. The zero layered track (bottom) will blend in/out with the default animation clip set on the Animation Component of the actor if any, while all other Animation Tracks will play above.")]
[Attachable(new Type[] { typeof(ActorGroup) })]
[Icon("NavMeshAgent Icon")]
public class AnimationTrack : CutsceneTrack
{
	[Range(0f, 1f)]
	[SerializeField]
	private float _weight = 1f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _blendIn = 0.5f;

	[Range(0f, 1f)]
	[SerializeField]
	private float _blendOut = 0.5f;

	[SerializeField]
	private AnimationBlendMode _animationBlendMode;

	[SerializeField]
	private string _mixTransformName = string.Empty;

	private Animation anim;

	private AnimationState state;

	public override string info
	{
		get
		{
			string arg = ((animationBlendMode != 0) ? "Additive" : "Override");
			return string.Format("Layer: {0}, {1} {2}", (base.layerOrder == 0) ? (-1) : (base.layerOrder - 11), arg, (!string.IsNullOrEmpty(mixTransformName)) ? (", " + mixTransformName) : string.Empty);
		}
	}

	public override float blendIn => _blendIn;

	public override float blendOut => _blendOut;

	public float weight => _weight;

	public AnimationBlendMode animationBlendMode
	{
		get
		{
			return _animationBlendMode;
		}
		private set
		{
			_animationBlendMode = value;
		}
	}

	public string mixTransformName
	{
		get
		{
			return _mixTransformName;
		}
		private set
		{
			_mixTransformName = value;
		}
	}

	protected override bool OnInitialize()
	{
		base.layerOrder += 11;
		anim = base.actor.GetComponent<Animation>();
		if (anim == null)
		{
			Debug.LogError("The Animation Track requires the actor to have the 'Animation' Component attached", base.actor);
			return false;
		}
		return true;
	}

	protected override void OnEnter()
	{
		anim = base.actor.GetComponent<Animation>();
		if (anim == null || anim.clip == null || anim.IsPlaying(anim.clip.name))
		{
			state = null;
		}
		else if (anim.playAutomatically)
		{
			state = anim[anim.clip.name];
			state.layer = 10;
			state.wrapMode = WrapMode.Loop;
			state.blendMode = AnimationBlendMode.Blend;
			state.enabled = true;
		}
	}

	protected override void OnUpdate(float time, float previousTime)
	{
		if (state != null)
		{
			state.time = Mathf.Repeat(time, state.length);
			state.weight = GetAnimationWeight();
			anim.Sample();
		}
	}

	protected override void OnExit()
	{
		if (state != null)
		{
			state.enabled = false;
		}
	}

	protected override void OnReverseEnter()
	{
		if (state != null)
		{
			state.enabled = true;
		}
	}

	protected override void OnReverse()
	{
		if (state != null)
		{
			state.enabled = false;
		}
	}

	public float GetAnimationWeight()
	{
		return Easing.Ease(EaseType.QuadraticInOut, 0f, weight, GetTrackWeight(base.root.currentTime - startTime));
	}

	public Transform GetMixTransform()
	{
		if (string.IsNullOrEmpty(mixTransformName))
		{
			return null;
		}
		Transform transform = base.actor.transform.GetComponentsInChildren<Transform>().FirstOrDefault((Transform t) => t.name == mixTransformName);
		if (transform == null)
		{
			Debug.LogWarning("Cant find transform with name '" + mixTransformName + "' for PlayAnimation Action", base.actor);
		}
		return transform;
	}
}
