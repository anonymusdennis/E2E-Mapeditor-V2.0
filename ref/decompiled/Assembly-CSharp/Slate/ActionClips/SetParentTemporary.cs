using System;
using UnityEngine;

namespace Slate.ActionClips;

[Obsolete("Use Set Parent")]
[Category("Transform")]
public class SetParentTemporary : ActorActionClip
{
	public float _length = 1f;

	public Transform newParent;

	public bool matchPosition;

	public bool matchRotation;

	public bool matchScale;

	private Transform originalParent;

	private Vector3 originalPos;

	private Quaternion originalRot;

	private Vector3 originalScale;

	public override string info => string.Format("Set Parent Temporary\n{0}", (!(newParent != null)) ? "none" : newParent.name);

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
		originalParent = base.actor.transform.parent;
		originalPos = base.actor.transform.localPosition;
		originalRot = base.actor.transform.localRotation;
		originalScale = base.actor.transform.localScale;
	}

	protected override void OnUpdate(float deltaTime)
	{
		if (deltaTime < length)
		{
			base.actor.transform.SetParent(newParent, worldPositionStays: true);
			if (matchPosition)
			{
				base.actor.transform.localPosition = Vector3.zero;
			}
			if (matchRotation)
			{
				base.actor.transform.localEulerAngles = Vector3.zero;
			}
			if (matchScale)
			{
				base.actor.transform.localScale = Vector3.one;
			}
		}
		else
		{
			base.actor.transform.SetParent(originalParent, worldPositionStays: true);
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.SetParent(originalParent, worldPositionStays: true);
		base.actor.transform.localPosition = originalPos;
		base.actor.transform.localRotation = originalRot;
		base.actor.transform.localScale = originalScale;
	}
}
