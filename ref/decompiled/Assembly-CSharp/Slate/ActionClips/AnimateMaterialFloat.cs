using UnityEngine;

namespace Slate.ActionClips;

[Category("Renderer")]
public class AnimateMaterialFloat : ActorActionClip<Renderer>
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[HideInInspector]
	[SerializeField]
	private float _blendIn = 0.2f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.2f;

	[ShaderPropertyPopup(typeof(float))]
	public string propertyName;

	[AnimatableParameter]
	public float value;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private float originalValue;

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

	protected override void OnUpdate(float deltaTime)
	{
		float num = Easing.Ease(interpolation, originalValue, value, GetClipWeight(deltaTime));
		instanceMat.SetFloat(propertyName, num);
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
		originalValue = instanceMat.GetFloat(propertyName);
	}

	private void DoReset()
	{
		Object.DestroyImmediate(instanceMat);
		base.actor.sharedMaterial = sharedMat;
	}
}
