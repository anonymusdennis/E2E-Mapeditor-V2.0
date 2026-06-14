using UnityEngine;

namespace Slate.ActionClips;

[Category("Transform")]
[Description("Attach an object to a child transform of the actor (or the actor itself) either permantentely or temporary if length is greater than zero.")]
public class AttachObject : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[Required]
	public Transform targetObject;

	public string childTransformName;

	public Vector3 localPosition;

	public Vector3 localRotation;

	public Vector3 localScale = Vector3.one;

	private TransformSnapshot snapshot;

	private bool temporary;

	public override bool isValid => targetObject != null;

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
		snapshot = new TransformSnapshot(targetObject.gameObject, TransformSnapshot.StoreMode.RootOnly);
		Transform transform = base.actor.transform.FindInChildren(childTransformName, includeHidden: true);
		if (transform == null)
		{
			Debug.LogError($"Child Transform with name '{childTransformName}', can't be found on actor '{base.actor.name}' hierarchy", base.actor.gameObject);
			return;
		}
		targetObject.SetParent(transform);
		targetObject.localPosition = localPosition;
		targetObject.localEulerAngles = localRotation;
		targetObject.localScale = localScale;
	}

	private void UnDo()
	{
		snapshot.Restore();
	}
}
