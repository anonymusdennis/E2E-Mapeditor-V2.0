using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate an Animator layer weight to a value and back to previous value gradualy over a period of time.")]
public class AnimateLayerWeight : MecanimBaseClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.2f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.2f;

	public int layerIndex;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	private float lastValue;

	public override string info => $"Layer '{layerIndex}' Weight";

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

	protected override void OnEnter()
	{
		lastValue = base.actor.GetLayerWeight(layerIndex);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.actor.SetLayerWeight(layerIndex, Mathf.Lerp(lastValue, weight, GetClipWeight(deltaTime)));
	}

	protected override void OnReverse()
	{
		if (Application.isPlaying)
		{
			base.actor.SetLayerWeight(layerIndex, lastValue);
		}
	}
}
