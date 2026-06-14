using UnityEngine;

namespace Slate.ActionClips;

[Category("Paths")]
[Description("Put the actor on a path to follow for the duration of the clip from path start to path end, or by using speed if 'Use Speed' is true. If you want to animate the rotation of the actor separately, leave Look Ahead to 0.")]
public class FollowPath : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 5f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn;

	[SerializeField]
	[HideInInspector]
	private float _blendOut;

	[Required]
	public Path path;

	public bool useSpeed;

	[Min(0.01f)]
	public float speed = 3f;

	[Range(0f, 1f)]
	public float lookAhead;

	public EaseType blendInterpolation = EaseType.QuadraticInOut;

	private Vector3 lastPos;

	private Quaternion lastRot;

	public override string info => string.Format("Follow Path\n{0}", (!(path != null)) ? "NONE" : path.name);

	public override float length
	{
		get
		{
			return (!useSpeed || !(path != null)) ? _length : (path.length / speed);
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

	public override bool isValid => path != null;

	protected override void OnEnter()
	{
		lastPos = base.actor.transform.position;
		lastRot = base.actor.transform.rotation;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (length == 0f)
		{
			base.actor.transform.position = path.GetPointAt(0f);
			return;
		}
		Vector3 pointAt = path.GetPointAt(deltaTime / length);
		base.actor.transform.position = Easing.Ease(blendInterpolation, lastPos, pointAt, GetClipWeight(deltaTime));
		if (lookAhead > 0f)
		{
			Vector3 pointAt2 = path.GetPointAt(deltaTime / length + lookAhead);
			Vector3 forward = pointAt2 - base.actor.transform.position;
			if (forward.magnitude > 0.001f)
			{
				Quaternion to = Quaternion.LookRotation(forward);
				base.actor.transform.rotation = Easing.Ease(blendInterpolation, lastRot, to, GetClipWeight(deltaTime));
			}
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = lastPos;
		base.actor.transform.rotation = lastRot;
	}
}
