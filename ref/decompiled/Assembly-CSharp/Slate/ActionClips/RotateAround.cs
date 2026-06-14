using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
[Description("Rotate the actor around target position or object by specified degrees and optionaly per second.")]
public class RotateAround : ActorActionClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	public Vector3 rotation = new Vector3(0f, 360f, 0f);

	public bool perSecond;

	public bool lookTarget;

	public EaseType interpolation = EaseType.QuadraticInOut;

	public PositionParameter targetPosition;

	private Vector3 originalPos;

	private Quaternion originalRot;

	private Vector3 targetOriginalPos;

	[AnimatableParameter(link = "targetPosition")]
	[PositionHandle]
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

	public override string info => string.Format("Rotate {0}{1} Around\n{2}", rotation, (!perSecond) ? string.Empty : " Per Second", (!targetPosition.useAnimation) ? targetPosition.ToString() : string.Empty);

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

	public override float blendIn => length;

	protected override void OnAfterValidate()
	{
		SetParameterEnabled("targetPositionVector", targetPosition.useAnimation);
	}

	protected override void OnEnter()
	{
		originalPos = base.actor.transform.position;
		originalRot = base.actor.transform.rotation;
		targetOriginalPos = TransformPoint(targetPosition.value, targetPosition.space);
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 vector = TransformPoint(targetPosition.value, targetPosition.space);
		Vector3 to = originalPos + rotation * ((!perSecond) ? 1f : length);
		Vector3 euler = Easing.Ease(interpolation, Vector3.zero, to, GetClipWeight(deltaTime));
		Quaternion quaternion = Quaternion.Euler(euler);
		Vector3 vector2 = quaternion * (originalPos - targetOriginalPos) + targetOriginalPos;
		base.actor.transform.position = vector2 + (vector - targetOriginalPos);
		if (lookTarget)
		{
			base.actor.transform.rotation = Quaternion.LookRotation(vector - base.actor.transform.position);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = originalPos;
		base.actor.transform.rotation = originalRot;
	}
}
