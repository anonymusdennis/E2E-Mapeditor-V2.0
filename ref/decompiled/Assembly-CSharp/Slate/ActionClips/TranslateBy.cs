using UnityEngine;

namespace Slate.ActionClips;

[Description("Translate the actor by specified value and optionaly per second")]
[Category("Transform")]
public class TranslateBy : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	public Vector3 translation = new Vector3(0f, 0f, 2f);

	public bool perSecond;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector3 originalPos;

	public override string info => string.Format("Translate{0} By\n{1}", (!perSecond) ? string.Empty : " Per Second", translation);

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
		originalPos = base.actor.transform.localPosition;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 to = originalPos + translation * ((!perSecond) ? 1f : length);
		base.actor.transform.localPosition = Easing.Ease(interpolation, originalPos, to, deltaTime / length);
	}

	protected override void OnReverse()
	{
		base.actor.transform.localPosition = originalPos;
	}
}
