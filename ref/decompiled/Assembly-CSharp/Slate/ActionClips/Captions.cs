using UnityEngine;

namespace Slate.ActionClips;

[Category("Rendering")]
[Description("Shows closed captions at the bottom of the screen. Note that the Play Audio clips of the Audio Track are also able to show subtitles in sync with the audio. Use this for non audible subtitles or captions.")]
public class Captions : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 2f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.25f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.25f;

	[Multiline(5)]
	public string text = "[wind blowing]";

	public Color color = Color.white;

	public EaseType interpolation = EaseType.QuadraticInOut;

	public override string info => $"<i>'{text}'</i>";

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
		DirectorGUI.UpdateSubtitles(text, color);
	}
}
