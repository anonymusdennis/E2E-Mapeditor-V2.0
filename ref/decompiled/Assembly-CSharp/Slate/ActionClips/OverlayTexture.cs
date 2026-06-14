using UnityEngine;

namespace Slate.ActionClips;

[Description("Displays a texture overlay")]
[Category("Rendering")]
public class OverlayTexture : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 2f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.25f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.25f;

	public Texture texture;

	[AnimatableParameter]
	public Color color = Color.white;

	[AnimatableParameter]
	public Vector2 scale = Vector2.one;

	[AnimatableParameter]
	public Vector2 position;

	public EaseType interpolation = EaseType.QuadraticInOut;

	public override string info => string.Format("Overlay '{0}'", (!(texture != null)) ? "NONE" : texture.name);

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
		DirectorGUI.UpdateOverlayTexture(texture, color, scale, position);
	}
}
