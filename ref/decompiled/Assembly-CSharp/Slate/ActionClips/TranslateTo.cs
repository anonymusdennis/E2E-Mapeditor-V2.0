using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
public class TranslateTo : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 targetPosition;

	public MiniTransformSpace space;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalPos;

	public override string info => $"Translate To\n{targetPosition}";

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
		originalPos = base.actor.transform.position;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 vector = TransformPoint(targetPosition, (TransformSpace)space);
		if (length == 0f)
		{
			base.actor.transform.position = vector;
		}
		else
		{
			base.actor.transform.position = Easing.Ease(interpolation, originalPos, vector, deltaTime / length);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = originalPos;
	}
}
