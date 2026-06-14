using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
[Description("Set the parent of the actor gameobject temporarily, or permanently if length is zero")]
public class SetTransformParent : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length;

	public Transform newParent;

	public bool resetPosition;

	public bool resetRotation;

	public bool resetScale;

	private Transform originalParent;

	private Vector3 originalPos;

	private Quaternion originalRot;

	private Vector3 originalScale;

	private bool temporary;

	public override string info => string.Format("Set Parent\n{0}", (!(newParent != null)) ? "none" : newParent.name);

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
		Do();
	}

	protected override void OnReverseEnter()
	{
		if (temporary)
		{
			Do();
		}
	}

	protected override void OnExit()
	{
		if (temporary)
		{
			UnDo();
		}
	}

	protected override void OnReverse()
	{
		UnDo();
	}

	private void Do()
	{
		originalParent = base.actor.transform.parent;
		originalPos = base.actor.transform.localPosition;
		originalRot = base.actor.transform.localRotation;
		originalScale = base.actor.transform.localScale;
		base.actor.transform.SetParent(newParent, worldPositionStays: true);
		if (resetPosition)
		{
			base.actor.transform.localPosition = Vector3.zero;
		}
		if (resetRotation)
		{
			base.actor.transform.localEulerAngles = Vector3.zero;
		}
		if (resetScale)
		{
			base.actor.transform.localScale = Vector3.one;
		}
	}

	private void UnDo()
	{
		base.actor.transform.SetParent(originalParent, worldPositionStays: true);
		base.actor.transform.localPosition = originalPos;
		base.actor.transform.localRotation = originalRot;
		base.actor.transform.localScale = originalScale;
	}
}
