using UnityEngine;

namespace Slate.ActionClips;

[Description("Show text on screen")]
[Category("Rendering")]
public class OverlayText : DirectorActionClip
{
	[SerializeField]
	[HideInInspector]
	private float _length = 2f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.25f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.25f;

	[Multiline(5)]
	public string text = "In a galaxy far far away...";

	public TextAnchor anchor = TextAnchor.MiddleCenter;

	public EaseType interpolation = EaseType.QuadraticInOut;

	[AnimatableParameter]
	public Color color = Color.white;

	[AnimatableParameter]
	public float size = 26f;

	[AnimatableParameter]
	public Vector2 position;

	public override string info => $"'{text}'";

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

	protected override void OnUpdate(float deltaTime)
	{
		Color color = this.color;
		color.a = Easing.Ease(interpolation, 0f, this.color.a, GetClipWeight(deltaTime));
		DirectorGUI.UpdateOverlayText(text, color, size, anchor, position);
	}
}
