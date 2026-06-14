using UnityEngine;

namespace Slate.ActionClips;

[Category("Renderer")]
public class AnimateMaterialColor : ActorActionClip<Renderer>
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

	[ShaderPropertyPopup(typeof(Color))]
	public string propertyName = "_Color";

	[AnimatableParameter]
	public Color color = Color.white;

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Color originalColor;

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
		Color color = Easing.Ease(interpolation, originalColor, this.color, GetClipWeight(deltaTime));
		instanceMat.SetColor(propertyName, color);
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
		originalColor = instanceMat.GetColor(propertyName);
	}

	private void DoReset()
	{
		Object.DestroyImmediate(instanceMat);
		base.actor.sharedMaterial = sharedMat;
	}
}
