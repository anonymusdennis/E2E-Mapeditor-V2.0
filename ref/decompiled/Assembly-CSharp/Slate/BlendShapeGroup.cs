using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slate;

[Serializable]
public class BlendShapeGroup
{
	[SerializeField]
	private string _UID;

	[SerializeField]
	private string _name = "(rename me)";

	[SerializeField]
	private float _weight;

	[SerializeField]
	private List<BlendShape> _blendShapes = new List<BlendShape>();

	public string UID
	{
		get
		{
			return _UID;
		}
		private set
		{
			_UID = value;
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
			SetBlendWeights();
		}
	}

	public List<BlendShape> blendShapes => _blendShapes;

	public BlendShapeGroup()
	{
		UID = Guid.NewGuid().ToString();
	}

	private void SetBlendWeights()
	{
		for (int i = 0; i < blendShapes.Count; i++)
		{
			blendShapes[i].SetRealWeight(weight);
		}
	}

	public override string ToString()
	{
		return name;
	}
}
