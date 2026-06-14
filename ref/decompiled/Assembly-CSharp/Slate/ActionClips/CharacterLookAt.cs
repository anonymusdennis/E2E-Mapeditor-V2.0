using UnityEngine;

namespace Slate.ActionClips;

[Name("Character Head Look At")]
[Category("Character")]
public class CharacterLookAt : ActorActionClip<Character>
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.25f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.25f;

	public EaseType interpolation = EaseType.QuadraticInOut;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	public PositionParameter targetPosition;

	private Quaternion originalRot1;

	private Quaternion originalRot2;

	[PositionHandle]
	[ShowTrajectory]
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

	public override string info => $"Head Look At {((!targetPosition.useAnimation) ? targetPosition.ToString() : string.Empty)}";

	public override bool isValid => base.actor != null && base.actor.head != null && base.actor.neck != null;

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
		if (isValid)
		{
			targetPosition.value = InverseTransformPoint(base.actor.head.position, targetPosition.space);
		}
	}

	protected override void OnAfterValidate()
	{
		SetParameterEnabled("targetPositionVector", targetPosition.useAnimation);
	}

	protected override void OnEnter()
	{
		originalRot1 = base.actor.head.rotation;
		originalRot2 = base.actor.neck.rotation;
	}

	protected override void OnUpdate(float time)
	{
		Vector3 vector = TransformPoint(targetPosition.value, targetPosition.space);
		float num = GetClipWeight(time) * weight;
		Quaternion to = Quaternion.LookRotation(vector - base.actor.neck.position);
		base.actor.neck.rotation = Easing.Ease(interpolation, originalRot2, to, num * 0.5f);
		Quaternion to2 = Quaternion.LookRotation(vector - base.actor.head.position);
		base.actor.head.rotation = Easing.Ease(interpolation, originalRot1, to2, num);
	}

	protected override void OnReverse()
	{
		base.actor.head.rotation = originalRot1;
		base.actor.neck.rotation = originalRot2;
	}
}
