using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
public class RotateTo : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 targetRotation;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalPos;

	public override string info => $"Rotate To\n{targetRotation}";

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

	protected override void OnEnter()
	{
		originalPos = base.actor.transform.eulerAngles;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (length == 0f)
		{
			base.actor.transform.eulerAngles = targetRotation;
		}
		else
		{
			base.actor.transform.eulerAngles = Easing.Ease(interpolation, originalPos, targetRotation, deltaTime / length);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.eulerAngles = originalPos;
	}
}
