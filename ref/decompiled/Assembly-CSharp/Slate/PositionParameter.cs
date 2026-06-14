using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Slate;

[Serializable]
public struct PositionParameter : ITransformableHelperParameter
{
	[SerializeField]
	[FormerlySerializedAs("transform")]
	private Transform _transform;

	[FormerlySerializedAs("vector")]
	[SerializeField]
	private Vector3 _vector;

	[SerializeField]
	private TransformSpace _space;

	public bool useAnimation => _transform == null;

	public TransformSpace space
	{
		get
		{
			return (!(transform != null)) ? _space : TransformSpace.WorldSpace;
		}
		private set
		{
			_space = value;
		}
	}

	public Transform transform
	{
		get
		{
			return _transform;
		}
		private set
		{
			_transform = value;
		}
	}

	public Vector3 value
	{
		get
		{
			return (!(transform != null)) ? _vector : transform.position;
		}
		set
		{
			_vector = value;
		}
	}

	public override string ToString()
	{
		return (!(transform != null)) ? _vector.ToString() : transform.name;
	}
}
