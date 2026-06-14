using System.Collections.Generic;
using UnityEngine;

namespace Slate;

[AddComponentMenu("SLATE/Character")]
public class Character : MonoBehaviour
{
	[SerializeField]
	private List<BlendShapeGroup> _expressions = new List<BlendShapeGroup>();

	[SerializeField]
	private Transform _neckTransform;

	[SerializeField]
	private Transform _headTransform;

	public Transform neck
	{
		get
		{
			return _neckTransform;
		}
		set
		{
			_neckTransform = value;
		}
	}

	public Transform head
	{
		get
		{
			return _headTransform;
		}
		set
		{
			_headTransform = value;
		}
	}

	public List<BlendShapeGroup> expressions => _expressions;

	public BlendShapeGroup FindExpressionByName(string name)
	{
		return expressions.Find((BlendShapeGroup x) => x != null && x.name == name);
	}

	public BlendShapeGroup FindExpressionByUID(string UID)
	{
		return expressions.Find((BlendShapeGroup x) => x != null && x.UID == UID);
	}

	public void SetExpressionWeightByName(string name, float weight)
	{
		BlendShapeGroup blendShapeGroup = FindExpressionByName(name);
		if (blendShapeGroup != null)
		{
			blendShapeGroup.weight = weight;
		}
	}

	public void SetExpressionWeightByUID(string UID, float weight)
	{
		BlendShapeGroup blendShapeGroup = FindExpressionByUID(UID);
		if (blendShapeGroup != null)
		{
			blendShapeGroup.weight = weight;
		}
	}

	public void ResetExpressions()
	{
		for (int i = 0; i < expressions.Count; i++)
		{
			expressions[i].weight = 0f;
		}
	}
}
