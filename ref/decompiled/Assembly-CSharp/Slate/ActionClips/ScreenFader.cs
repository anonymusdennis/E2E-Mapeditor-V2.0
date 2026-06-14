using UnityEngine;

namespace Slate.ActionClips;

[Description("An alternative way to fade the screen. Fade out/in can also be done through the Camera Shot clip in the Camera Track.")]
[Category("Rendering")]
public class ScreenFader : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 4f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 1f;

	[AnimatableParameter(0f, 1f)]
	public float fade = 1f;

	[AnimatableParameter]
	public Color outColor = Color.black;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Color lastColor;

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

	protected override void OnEnter()
	{
		lastColor = DirectorGUI.fadeColor;
	}

	protected override void OnUpdate(float deltaTime)
	{
		Color color = outColor;
		color.a = Easing.Ease(interpolation, 0f, 1f, GetClipWeight(deltaTime) * fade);
		DirectorGUI.UpdateFade(color);
	}

	protected override void OnReverse()
	{
		DirectorGUI.UpdateFade(lastColor);
	}
}
