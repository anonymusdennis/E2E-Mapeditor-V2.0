using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
[Description("Rotate actor transform to look at specified target position for a period of time or permanentely if blend out is zero")]
public class LookAt : ActorActionClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.2f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.2f;

	public bool verticalOnly;

	public EaseType interpolation = EaseType.QuadraticInOut;

	public PositionParameter targetPosition;

	private Quaternion originalRot;

	[PositionHandle]
	[AnimatableParameter(link = "targetPosition")]
	[ShowTrajectory]
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

	public override string info => $"Look At {((!targetPosition.useAnimation) ? targetPosition.ToString() : string.Empty)}";

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
		originalRot = base.actor.transform.rotation;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 vector = TransformPoint(targetPosition.value, targetPosition.space);
		if (verticalOnly)
		{
			vector.y = base.actor.transform.position.y;
		}
		Vector3 forward = vector - base.actor.transform.position;
		if (forward.magnitude > 0.001f)
		{
			Quaternion to = Quaternion.LookRotation(forward);
			base.actor.transform.rotation = Easing.Ease(interpolation, originalRot, to, GetClipWeight(deltaTime));
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.rotation = originalRot;
	}
}
