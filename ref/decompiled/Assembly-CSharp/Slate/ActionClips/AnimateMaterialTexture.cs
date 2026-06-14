using UnityEngine;

namespace Slate.ActionClips;

[Category("Renderer")]
[Description("Animate a material's texture offset and scale over time")]
public class AnimateMaterialTexture : ActorActionClip<Renderer>
{
	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.2f;

	[SerializeField]
	[HideInInspector]
	private float _blendOut = 0.2f;

	[ShaderPropertyPopup(typeof(Texture))]
	public string propertyName = "_MainTex";

	[AnimatableParameter]
	public Vector2 offset;

	[AnimatableParameter]
	public Vector2 scale = Vector2.one;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector2 originalOffset;

	private Vector2 originalScale;

	private Material sharedMat;

	private Material instanceMat;

	public override string info => $"Animate '{propertyName}'";

	public override bool isValid => base.actor != null && base.actor.sharedMaterial != null && base.actor.sharedMaterial.HasProperty(propertyName);

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
		DoSet();
	}

	protected override void OnReverseEnter()
	{
		DoSet();
	}

	protected override void OnUpdate(float time)
	{
		float clipWeight = GetClipWeight(time);
		Vector3 vector = Easing.Ease(interpolation, originalOffset, offset, clipWeight);
		Vector3 vector2 = Easing.Ease(interpolation, originalScale, scale, clipWeight);
		instanceMat.SetTextureOffset(propertyName, vector);
		instanceMat.SetTextureScale(propertyName, vector2);
	}

	protected override void OnReverse()
	{
		DoReset();
	}

	protected override void OnExit()
	{
		DoReset();
	}

	private void DoSet()
	{
		sharedMat = base.actor.sharedMaterial;
		instanceMat = Object.Instantiate(sharedMat);
		base.actor.material = instanceMat;
		originalOffset = instanceMat.GetTextureOffset(propertyName);
		originalScale = instanceMat.GetTextureScale(propertyName);
	}

	private void DoReset()
	{
		Object.DestroyImmediate(instanceMat);
		base.actor.sharedMaterial = sharedMat;
	}
}
