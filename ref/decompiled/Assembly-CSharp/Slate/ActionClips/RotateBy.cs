using UnityEngine;

namespace Slate.ActionClips;

[Description("Rotate the actor by specified degrees and optionaly per second")]
[Category("Transform")]
public class RotateBy : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 rotation = new Vector3(0f, 90f, 0f);

	public bool perSecond;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalRot;

	public override string info => string.Format("Rotate{0} By\n{1}", (!perSecond) ? string.Empty : " Per Second", rotation);

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
		originalRot = base.actor.transform.localEulerAngles;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 to = originalRot + rotation * ((!perSecond) ? 1f : length);
		base.actor.transform.localEulerAngles = Easing.Ease(interpolation, originalRot, to, GetClipWeight(deltaTime));
	}

	protected override void OnReverse()
	{
		base.actor.transform.localEulerAngles = originalRot;
	}
}
