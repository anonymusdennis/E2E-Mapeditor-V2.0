using UnityEngine;

namespace Slate.ActionClips;

[Category("Environment")]
public class AnimateFog : DirectorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn;

	[SerializeField]
	[HideInInspector]
	private float _blendOut;

	[AnimatableParameter]
	public Color fogColor;

	[AnimatableParameter]
	public float fogDensity;

	[AnimatableParameter]
	public float linearFogStartDistance;

	[AnimatableParameter]
	public float linearFogEndDistance;

	private Color wasColor;

	private float wasDensity;

	private float wasStartDistance;

	private float wasEndDistance;

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
		fogColor = RenderSettings.fogColor;
		fogDensity = RenderSettings.fogDensity;
		linearFogStartDistance = RenderSettings.fogStartDistance;
		linearFogEndDistance = RenderSettings.fogEndDistance;
	}

	protected override void OnEnter()
	{
		wasColor = RenderSettings.fogColor;
		wasDensity = RenderSettings.fogDensity;
		wasStartDistance = RenderSettings.fogStartDistance;
		wasEndDistance = RenderSettings.fogEndDistance;
	}

	protected override void OnUpdate(float time)
	{
		float clipWeight = GetClipWeight(time);
		RenderSettings.fogColor = Color.Lerp(wasColor, fogColor, clipWeight);
		RenderSettings.fogDensity = Mathf.Lerp(wasDensity, fogDensity, clipWeight);
		RenderSettings.fogStartDistance = Mathf.Lerp(wasStartDistance, linearFogStartDistance, clipWeight);
		RenderSettings.fogEndDistance = Mathf.Lerp(wasEndDistance, linearFogEndDistance, clipWeight);
	}

	protected override void OnReverse()
	{
		RenderSettings.fogColor = wasColor;
		RenderSettings.fogDensity = wasDensity;
		RenderSettings.fogStartDistance = wasStartDistance;
		RenderSettings.fogEndDistance = wasEndDistance;
	}
}
