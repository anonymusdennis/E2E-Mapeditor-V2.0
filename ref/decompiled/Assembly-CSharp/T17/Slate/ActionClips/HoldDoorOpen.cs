using System;
using Slate;
using Slate.ActionClips;
using UnityEngine;

namespace T17.Slate.ActionClips;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Doors")]
public class HoldDoorOpen : ActorActionClip
{
	private Door m_Door;

	[SerializeField]
	[HideInInspector]
	private float _length = 1f;

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

	public override string info => "Hodor";

	protected override void OnEnter()
	{
		base.OnEnter();
		m_Door = base.actor.GetComponent<Door>();
		if (m_Door == null)
		{
		}
		m_Door.Cutscene_SetDoorOpen(shouldOpenDoor: true);
	}

	protected override void OnExit()
	{
		base.OnExit();
		if (m_Door == null)
		{
		}
		m_Door.Cutscene_SetDoorOpen(shouldOpenDoor: false);
	}
}
