using UnityEngine;

namespace Slate.ActionClips;

[Description("Smoothly match the selected transforms of the actor and to the target for a period of time and then back again to their original values. If you don't want to smooth back to the original values, set BlendOut to 0.")]
[Category("Transform")]
public class MatchTransformsToTarget : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 2f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.8f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.8f;

	[Required]
	public Transform targetObject;

	public EaseType interpolation = EaseType.QuadraticInOut;

	public bool matchPosition = true;

	public Vector3 positionOffset;

	public bool matchRotation = true;

	public Vector3 rotationOffset;

	public bool matchScale;

	public Vector3 scaleOffset;

	private Vector3 lastPos;

	private Quaternion lastRot;

	private Vector3 lastScale;

	public override string info => "Match Transforms\n" + ((!targetObject) ? "NONE" : targetObject.name);

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

	public override bool isValid => targetObject != null;

	protected override void OnEnter()
	{
		lastPos = base.actor.transform.position;
		lastRot = base.actor.transform.rotation;
		lastScale = base.actor.transform.localScale;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (matchPosition)
		{
			if (length > 0f)
			{
				base.actor.transform.position = Easing.Ease(interpolation, lastPos, targetObject.position + positionOffset, GetClipWeight(deltaTime));
			}
			else
			{
				base.actor.transform.position = targetObject.position + positionOffset;
			}
		}
		if (matchRotation)
		{
			if (length > 0f)
			{
				base.actor.transform.rotation = Easing.Ease(interpolation, lastRot, targetObject.rotation * Quaternion.Euler(rotationOffset), GetClipWeight(deltaTime));
			}
			else
			{
				base.actor.transform.rotation = targetObject.rotation * Quaternion.Euler(rotationOffset);
			}
		}
		if (matchScale)
		{
			if (length > 0f)
			{
				base.actor.transform.localScale = Easing.Ease(interpolation, lastScale, targetObject.localScale + scaleOffset, GetClipWeight(deltaTime));
			}
			else
			{
				base.actor.transform.localScale = targetObject.localScale + scaleOffset;
			}
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = lastPos;
		base.actor.transform.rotation = lastRot;
		base.actor.transform.localScale = lastScale;
	}
}
