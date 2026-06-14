using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
public class ScaleTo : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 targetScale;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalPos;

	public override string info => $"Scale To\n{targetScale}";

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
		originalPos = base.actor.transform.localScale;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (length == 0f)
		{
			base.actor.transform.localScale = targetScale;
		}
		else
		{
			base.actor.transform.localScale = Easing.Ease(interpolation, originalPos, targetScale, deltaTime / length);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.localScale = originalPos;
	}
}
