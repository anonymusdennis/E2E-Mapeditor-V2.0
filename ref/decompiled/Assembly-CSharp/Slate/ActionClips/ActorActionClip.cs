using System;
using UnityEngine;

namespace Slate.ActionClips;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
public abstract class ActorActionClip : ActionClip
{
}
[Attachable(new Type[] { typeof(ActorActionTrack) })]
public abstract class ActorActionClip<T> : ActionClip where T : Component
{
	[HideInInspector]
	public bool SearchChildrenForComponent;

	private T _actorComponent;

	public new T actor
	{
		get
		{
			if (_actorComponent != null && _actorComponent.gameObject == base.actor)
			{
				return _actorComponent;
			}
			if (!SearchChildrenForComponent)
			{
				return _actorComponent = ((!(base.actor != null)) ? ((T)null) : base.actor.GetComponent<T>());
			}
			return _actorComponent = ((!(base.actor != null)) ? ((T)null) : base.actor.GetComponentInChildren<T>(includeInactive: true));
		}
	}

	public override bool isValid => actor != null;

	public override bool HasNecessaryComponent()
	{
		return actor != null;
	}
}
