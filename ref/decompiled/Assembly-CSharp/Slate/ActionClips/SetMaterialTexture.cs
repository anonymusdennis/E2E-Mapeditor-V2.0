using UnityEngine;

namespace Slate.ActionClips;

[Category("Renderer")]
public class SetMaterialTexture : ActorActionClip<Renderer>
{
	[SerializeField]
	[HideInInspector]
	private float _length;

	[ShaderPropertyPopup(typeof(Texture))]
	public string propertyName = "_MainTex";

	public Texture texture;

	private Material sharedMat;

	private Material instanceMat;

	private bool temporary;

	public override string info => string.Format("Set Texture\n{0}", (!texture) ? "null" : texture.name);

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

	protected override void OnEnter()
	{
		temporary = length > 0f;
		DoSet();
	}

	protected override void OnReverseEnter()
	{
		if (temporary)
		{
			DoSet();
		}
	}

	protected override void OnReverse()
	{
		DoReset();
	}

	protected override void OnExit()
	{
		if (temporary)
		{
			DoReset();
		}
	}

	private void DoSet()
	{
		sharedMat = base.actor.sharedMaterial;
		instanceMat = Object.Instantiate(sharedMat);
		base.actor.material = instanceMat;
		instanceMat.SetTexture(propertyName, texture);
	}

	private void DoReset()
	{
		Object.DestroyImmediate(instanceMat);
		base.actor.sharedMaterial = sharedMat;
	}
}
