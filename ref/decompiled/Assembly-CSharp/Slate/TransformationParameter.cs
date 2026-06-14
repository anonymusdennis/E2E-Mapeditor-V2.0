using System;
using UnityEngine;

namespace Slate;

[Serializable]
public struct TransformationParameter : ITransformableHelperParameter
{
	[SerializeField]
	private Transform _transform;

	[SerializeField]
	private Vector3 _position;

	[SerializeField]
	private Vector3 _rotation;

	[SerializeField]
	private TransformSpace _space;

	public bool useAnimation => _transform == null;

	public TransformSpace space
	{
		get
		{
			return _space;
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

	public Vector3 position
	{
		get
		{
			return (!(transform != null)) ? _position : transform.localPosition;
		}
		set
		{
			_position = value;
		}
	}

	public Vector3 rotation
	{
		get
		{
			return (!(transform != null)) ? _rotation : transform.GetLocalEulerAngles();
		}
		set
		{
			_rotation = value;
		}
	}

	public override string ToString()
	{
		return (!(_transform != null)) ? _position.ToString() : _transform.name;
	}
}
