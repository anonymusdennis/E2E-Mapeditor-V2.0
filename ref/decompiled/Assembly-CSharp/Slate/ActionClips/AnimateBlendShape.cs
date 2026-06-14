using System.Linq;
using UnityEngine;

namespace Slate.ActionClips;

[Category("Character")]
[Description("Animates a single blend shape chosen from within the whole actor gameobject hierarchy.")]
public class AnimateBlendShape : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[SerializeField]
	[HideInInspector]
	private float _blendIn = 0.25f;

	[HideInInspector]
	[SerializeField]
	private float _blendOut = 0.25f;

	[HideInInspector]
	[SerializeField]
	private string _skinName;

	[HideInInspector]
	[SerializeField]
	private string _shapeName;

	[AnimatableParameter(0f, 1f)]
	public float weight = 1f;

	private float originalWeight;

	private int index;

	private SkinnedMeshRenderer _skinnedMesh;

	public override string info => $"Animate '{shapeName}'";

	public override bool isValid => skinnedMesh != null && skinnedMesh.GetBlendShapeIndex(shapeName) != -1;

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

	public string skinName
	{
		get
		{
			return _skinName;
		}
		set
		{
			_skinName = value;
			_skinnedMesh = null;
		}
	}

	public string shapeName
	{
		get
		{
			return _shapeName;
		}
		set
		{
			_shapeName = value;
		}
	}

	private SkinnedMeshRenderer skinnedMesh
	{
		get
		{
			if (base.actor != null && _skinnedMesh == null)
			{
				_skinnedMesh = base.actor.GetComponentsInChildren<SkinnedMeshRenderer>().ToList().Find((SkinnedMeshRenderer s) => s.name == skinName);
			}
			return _skinnedMesh;
		}
	}

	protected override void OnEnter()
	{
		index = ((!(skinnedMesh != null)) ? (-1) : skinnedMesh.GetBlendShapeIndex(shapeName));
		if (index != -1)
		{
			originalWeight = skinnedMesh.GetBlendShapeWeight(index);
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (index != -1)
		{
			float num = Easing.Ease(EaseType.QuadraticInOut, originalWeight, weight, GetClipWeight(deltaTime));
			skinnedMesh.SetBlendShapeWeight(index, num * 100f);
		}
	}

	protected override void OnReverse()
	{
		if (index != -1)
		{
			skinnedMesh.SetBlendShapeWeight(index, originalWeight);
		}
	}
}
