using UnityEngine;

namespace Slate.ActionClips;

[Category("Paths")]
[Description("Animate the actor's position and look at target position on a Path.")]
public class AnimateOnPath : ActorActionClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 5f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn;

	[Required]
	public Path path;

	[AnimatableParameter(0f, 1f)]
	public float positionOnPath;

	[ShowTrajectory]
	[PositionHandle]
	[AnimatableParameter]
	public Vector3 lookAtTargetPosition;

	public EaseType blendInterpolation = EaseType.QuadraticInOut;

	private Vector3 lastPos;

	private Quaternion lastRot;

	public override string info => string.Format("Animate On Path '{0}'", (!(path != null)) ? "NONE" : path.name);

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

	public override bool isValid => path != null;

	public override TransformSpace defaultTransformSpace => TransformSpace.CutsceneSpace;

	protected override void OnEnter()
	{
		lastPos = base.actor.transform.position;
		lastRot = base.actor.transform.rotation;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (length == 0f)
		{
			base.actor.transform.position = path.GetPointAt(positionOnPath);
			return;
		}
		Vector3 pointAt = path.GetPointAt(positionOnPath);
		base.actor.transform.position = Easing.Ease(blendInterpolation, lastPos, pointAt, GetClipWeight(deltaTime));
		Vector3 vector = TransformPoint(lookAtTargetPosition, defaultTransformSpace);
		Vector3 forward = vector - base.actor.transform.position;
		if (forward.magnitude > 0.001f)
		{
			Quaternion to = Quaternion.LookRotation(forward);
			base.actor.transform.rotation = Easing.Ease(blendInterpolation, lastRot, to, GetClipWeight(deltaTime));
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = lastPos;
		base.actor.transform.rotation = lastRot;
	}
}
