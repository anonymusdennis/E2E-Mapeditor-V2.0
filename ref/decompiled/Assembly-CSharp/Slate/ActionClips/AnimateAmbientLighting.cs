using UnityEngine;

namespace Slate.ActionClips;

[Category("Environment")]
public class AnimateAmbientLighting : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn;

	[HideInInspector]
	[SerializeField]
	private float _blendOut;

	[AnimatableParameter(0f, 10f)]
	public float ambientIntensity;

	[AnimatableParameter]
	public Color ambientColor;

	private float wasIntensity;

	private Color wasColor;

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

	protected override void OnCreate()
	{
		ambientIntensity = RenderSettings.ambientIntensity;
		ambientColor = RenderSettings.ambientLight;
	}

	protected override void OnEnter()
	{
		wasIntensity = RenderSettings.ambientIntensity;
		wasColor = RenderSettings.ambientLight;
	}

	protected override void OnUpdate(float time)
	{
		float clipWeight = GetClipWeight(time);
		RenderSettings.ambientIntensity = Mathf.Lerp(wasIntensity, ambientIntensity, clipWeight);
		RenderSettings.ambientLight = Color.Lerp(wasColor, ambientColor, clipWeight);
	}

	protected override void OnReverse()
	{
		RenderSettings.ambientIntensity = wasIntensity;
		RenderSettings.ambientLight = wasColor;
	}
}
