using UnityEngine;

namespace Slate.ActionClips;

[Description("Instantely set the transforms of the actor.")]
[Category("Transform")]
public class SetTransformValues : ActorActionClip
{
	public bool setPosition = true;

	public Vector3 position;

	public MiniTransformSpace space;

	public bool setRotation;

	public Vector3 rotation;

	public bool setScale;

	public Vector3 scale = Vector3.one;

	private TransformSnapshot undo;

	protected override void OnEnter()
	{
		undo = new TransformSnapshot(base.actor, TransformSnapshot.StoreMode.RootOnly);
		if (setPosition)
		{
			base.actor.transform.position = TransformPoint(position, (TransformSpace)space);
		}
		if (setRotation)
		{
			base.actor.transform.localEulerAngles = rotation;
		}
		if (setScale)
		{
			base.actor.transform.localScale = scale;
		}
	}

	protected override void OnReverse()
	{
		undo.Restore();
	}
}
