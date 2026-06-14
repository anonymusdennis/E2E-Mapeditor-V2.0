using UnityEngine;

namespace Slate.ActionClips;

[Description("Scroll/Offset the material texture. Usefull for scrolling backgrounds, screens, or other similar effects.")]
[Category("Renderer")]
public class ScrollMaterialTexture : ActorActionClip<Renderer>
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[ShaderPropertyPopup(typeof(Texture))]
	public string propertyName = "_MainTex";

	public Vector2 speed = new Vector2(1f, 0f);

	public EaseType interpolation = EaseType.QuadraticInOut;

	private Vector2 originalOffset;

	private Material sharedMat;

	private Material instanceMat;

	public override string info => $"Scroll Texture / sec\n{speed}";

	public override bool isValid => base.actor != null && base.actor.sharedMaterial.HasProperty(propertyName);

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
		sharedMat = base.actor.sharedMaterial;
		instanceMat = Object.Instantiate(sharedMat);
		base.actor.material = instanceMat;
		originalOffset = instanceMat.GetTextureOffset(propertyName);
	}

	protected override void OnUpdate(float deltaTime)
	{
		Vector3 vector = Easing.Ease(interpolation, originalOffset, originalOffset + speed * length, GetClipWeight(deltaTime));
		instanceMat.SetTextureOffset(propertyName, vector);
	}

	protected override void OnReverse()
	{
		Object.DestroyImmediate(instanceMat);
		base.actor.sharedMaterial = sharedMat;
	}
}
