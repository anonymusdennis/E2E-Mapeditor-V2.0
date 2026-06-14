using System;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[] { typeof(MecanimTrack) })]
[Description("Make the actor look at target position. Please note that 'IK Pass' must be enabled in the Controller.")]
public class AnimateLookAtIK : ActorActionClip<Animator>
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.2f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.2f;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	[AnimatableParameter(0f, 1f)]
	public float bodyWeight = 0.25f;

	[AnimatableParameter(0f, 1f)]
	public float headWeight = 0.95f;

	[AnimatableParameter(0f, 1f)]
	public float eyesWeight = 1f;

	public PositionParameter targetPosition;

	[ShowTrajectory]
	[PositionHandle]
	[AnimatableParameter(link = "targetPosition")]
	public Vector3 targetPositionVector
	{
		get
		{
			return targetPosition.value;
		}
		set
		{
			targetPosition.value = value;
		}
	}

	private AnimatorDispatcher dispatcher => (base.parent as MecanimTrack).dispatcher;

	public override string info => $"Look At IK";

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

	protected override void OnCreate()
	{
		targetPosition.value = ActorPositionInSpace(targetPosition.space);
	}

	protected override void OnAfterValidate()
	{
		SetParameterEnabled("targetPositionVector", targetPosition.useAnimation);
	}

	protected override void OnEnter()
	{
		dispatcher.onAnimatorIK += OnAnimatorIK;
	}

	protected override void OnReverseEnter()
	{
		dispatcher.onAnimatorIK += OnAnimatorIK;
	}

	protected override void OnReverse()
	{
		dispatcher.onAnimatorIK -= OnAnimatorIK;
	}

	protected override void OnExit()
	{
		dispatcher.onAnimatorIK -= OnAnimatorIK;
	}

	private void OnAnimatorIK(int index)
	{
		float num = GetClipWeight() * weight;
		Vector3 lookAtPosition = TransformPoint(targetPosition.value, targetPosition.space);
		base.actor.SetLookAtPosition(lookAtPosition);
		base.actor.SetLookAtWeight(num, bodyWeight, headWeight, eyesWeight, 0.5f);
	}
}
