using UnityEngine;

namespace Slate.ActionClips;

[Description("Scale the actor by specified value and optionlay per second")]
[Category("Transform")]
public class ScaleBy : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 scale = Vector3.one;

	public bool perSecond;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalScale;

	public override string info => string.Format("Scale{0} By\n{1}", (!perSecond) ? string.Empty : " Per Second", scale);

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
		originalScale = base.actor.transform.localScale;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 to = originalScale + scale * ((!perSecond) ? 1f : length);
		base.actor.transform.localScale = Easing.Ease(interpolation, originalScale, to, deltaTime / length);
	}

	protected override void OnReverse()
	{
		base.actor.transform.localScale = originalScale;
	}
}
