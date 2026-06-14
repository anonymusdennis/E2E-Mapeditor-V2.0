using System;
using UnityEngine;

namespace Slate.ActionClips;

[Description("Animate an actor IK Goal. Please note that 'IK Pass' must be enabled in the Controller.")]
[Attachable(new Type[] { typeof(MecanimTrack) })]
public class AnimateLimbIK : ActorActionClip<Animator>
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.2f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.2f;

	public AvatarIKGoal IKGoal = AvatarIKGoal.RightHand;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	public TransformationParameter IKTarget;

	private Vector3 lastPos;

	private Quaternion lastRot;

	private float lastWeight;

	private bool isEnter;

	private bool isReverse;

	[PositionHandle]
	[ShowTrajectory]
	[AnimatableParameter(link = "IKTarget")]
	public Vector3 targetPosition
	{
		get
		{
			return IKTarget.position;
		}
		set
		{
			IKTarget.position = value;
		}
	}

	[AnimatableParameter(link = "IKTarget")]
	public Vector3 targetRotation
	{
		get
		{
			return IKTarget.rotation;
		}
		set
		{
			IKTarget.rotation = value;
		}
	}

	private AnimatorDispatcher dispatcher => (base.parent as MecanimTrack).dispatcher;

	public override string info => $"'{IKGoal.ToString()}' IK";

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
		IKTarget.position = ActorPositionInSpace(IKTarget.space);
	}

	protected override void OnAfterValidate()
	{
		SetParameterEnabled("targetPosition", IKTarget.useAnimation);
		SetParameterEnabled("targetRotation", IKTarget.useAnimation);
	}

	protected override void OnEnter()
	{
		dispatcher.onAnimatorIK += OnAnimatorIK;
		isEnter = true;
	}

	protected override void OnReverseEnter()
	{
		dispatcher.onAnimatorIK += OnAnimatorIK;
	}

	protected override void OnReverse()
	{
		dispatcher.onAnimatorIK -= OnAnimatorIK;
		isReverse = true;
	}

	protected override void OnExit()
	{
		dispatcher.onAnimatorIK -= OnAnimatorIK;
	}

	private void OnAnimatorIK(int index)
	{
		if (isEnter)
		{
			isEnter = false;
			lastPos = base.actor.GetIKPosition(IKGoal);
			lastRot = base.actor.GetIKRotation(IKGoal);
			lastWeight = base.actor.GetIKPositionWeight(IKGoal);
		}
		else if (isReverse)
		{
			isReverse = false;
			base.actor.SetIKPosition(IKGoal, lastPos);
			base.actor.SetIKRotation(IKGoal, lastRot);
			base.actor.SetIKPositionWeight(IKGoal, lastWeight);
			base.actor.SetIKRotationWeight(IKGoal, lastWeight);
		}
		else
		{
			float value = GetClipWeight() * weight;
			Vector3 goalPosition = TransformPoint(IKTarget.position, IKTarget.space);
			Quaternion goalRotation = Quaternion.Euler(targetRotation);
			base.actor.SetIKPosition(IKGoal, goalPosition);
			base.actor.SetIKRotation(IKGoal, goalRotation);
			base.actor.SetIKPositionWeight(IKGoal, value);
			base.actor.SetIKRotationWeight(IKGoal, value);
		}
	}
}
