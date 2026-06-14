using System;
using UnityEngine;

namespace Slate;

[Serializable]
public class BlendShape
{
	[SerializeField]
	private SkinnedMeshRenderer _skin;

	[SerializeField]
	private string _name;

	[SerializeField]
	private float _weight;

	public SkinnedMeshRenderer skin
	{
		get
		{
			return _skin;
		}
		set
		{
			_skin = value;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public float weight
	{
		get
		{
			return _weight;
		}
		set
		{
			_weight = value;
		}
	}

	public void SetRealWeight(float modWeight)
	{
		if (!(skin == null))
		{
			int blendShapeIndex = skin.GetBlendShapeIndex(name);
			if (blendShapeIndex != -1)
			{
				skin.SetBlendShapeWeight(blendShapeIndex, weight * modWeight * 100f);
			}
		}
	}

	public override string ToString()
	{
		return name;
	}
}
