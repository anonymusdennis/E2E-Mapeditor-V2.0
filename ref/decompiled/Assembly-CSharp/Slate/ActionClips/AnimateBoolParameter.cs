using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate a bool Animator parameter and reset it back to previous value after a period of time.")]
public class AnimateBoolParameter : MecanimBaseClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	public string parameterName;

	[AnimatableParameter]
	public bool value = true;

	private bool lastValue;

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

	protected override void OnEnter()
	{
		lastValue = base.actor.GetBool(parameterName);
	}

	protected override void OnUpdate(float time)
	{
		base.actor.SetBool(parameterName, value);
	}

	protected override void OnExit()
	{
		if (length > 0f)
		{
			base.actor.SetBool(parameterName, lastValue);
		}
	}

	protected override void OnReverse()
	{
		if (Application.isPlaying)
		{
			base.actor.SetBool(parameterName, lastValue);
		}
	}
}
