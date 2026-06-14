using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Camera")]
public class SetCameraView : ActionClip
{
	public CameraView m_CameraView;

	private CutsceneCameraTrackableObject m_CachedTrackableObject;

	[HideInInspector]
	public string InfoString = string.Empty;

	public override string info
	{
		get
		{
			if (string.IsNullOrEmpty(InfoString))
			{
				return "Camera view: " + m_CameraView;
			}
			return InfoString;
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_CachedTrackableObject == null)
		{
			m_CachedTrackableObject = base.actor.GetAddComponent<CutsceneCameraTrackableObject>();
		}
		m_CachedTrackableObject.m_CameraView = m_CameraView;
	}
}
