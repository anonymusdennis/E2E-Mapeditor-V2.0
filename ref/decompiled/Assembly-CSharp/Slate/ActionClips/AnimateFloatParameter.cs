using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate a float Animator parameter to a value and back to previous value gradualy over a period of time.")]
public class AnimateFloatParameter : MecanimBaseClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.2f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.2f;

	public string parameterName;

	[AnimatableParameter]
	public float value = 1f;

	private float lastValue;

	public override bool isValid => base.isValid && HasParameter(parameterName);

	public override string info => $"'{parameterName}' Parameter";

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
		lastValue = base.actor.GetFloat(parameterName);
	}

	protected override void OnUpdate(float deltaTime)
	{
		base.actor.SetFloat(parameterName, Mathf.Lerp(lastValue, value, GetClipWeight(deltaTime)));
	}

	protected override void OnReverse()
	{
		if (Application.isPlaying)
		{
			base.actor.SetFloat(parameterName, lastValue);
		}
	}
}
