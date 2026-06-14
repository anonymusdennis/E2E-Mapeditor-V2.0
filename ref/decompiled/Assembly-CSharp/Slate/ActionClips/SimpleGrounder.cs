using UnityEngine;

namespace Slate.ActionClips;

[Description("Grounds the actor gameobject to the nearest collider object beneath it")]
[Category("Transform")]
public class SimpleGrounder : ActorActionClip
{
	[HideInInspector]
	[SerializeField]
	private float _length = 1f;

	[Range(1f, 100f)]
	public float maxCheckDistance = 10f;

	[Min(0f)]
	public float offset;

	private Vector3 lastPos;

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
		lastPos = base.actor.transform.position;
	}

	protected override void OnUpdate(float time)
	{
		Vector3 vector = base.actor.transform.position + new Vector3(0f, maxCheckDistance, 0f);
		RaycastHit[] array = Physics.RaycastAll(new Ray(vector, Vector3.down), maxCheckDistance * 2f);
		for (int i = 0; i < array.Length; i++)
		{
			RaycastHit raycastHit = array[i];
			if (raycastHit.distance > maxCheckDistance && raycastHit.transform != base.actor.transform)
			{
				vector.y = raycastHit.point.y + offset;
				base.actor.transform.position = vector;
				break;
			}
		}
	}

	protected override void OnReverse()
	{
		base.actor.transform.position = lastPos;
	}
}
