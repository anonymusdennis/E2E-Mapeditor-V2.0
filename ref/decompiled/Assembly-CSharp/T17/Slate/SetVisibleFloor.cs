using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 FloorPosition")]
public class SetVisibleFloor : ActionClip
{
	public int m_FloorIndex;

	private CutsceneFlooredMonobehaviour m_CachedTrackableObject;

	[HideInInspector]
	public string InfoString = string.Empty;

	public override string info
	{
		get
		{
			if (string.IsNullOrEmpty(InfoString))
			{
				return "Visible Floor to " + m_FloorIndex;
			}
			return InfoString;
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_CachedTrackableObject == null)
		{
			m_CachedTrackableObject = base.actor.GetAddComponent<CutsceneFlooredMonobehaviour>();
		}
		m_CachedTrackableObject.m_FloorIndex = m_FloorIndex;
	}
}
