using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Characters")]
public class SetCharacterLayering : ActionClip
{
	public bool m_EnableLayering = true;

	private CS_HijackIngameCharacter m_CutSceneCharacter;

	[HideInInspector]
	public string InfoString = string.Empty;

	public override string info
	{
		get
		{
			if (string.IsNullOrEmpty(InfoString))
			{
				return "Enable Character Layering: " + m_EnableLayering;
			}
			return InfoString;
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_CutSceneCharacter == null)
		{
			m_CutSceneCharacter = base.actor.GetComponent<CS_HijackIngameCharacter>();
		}
		if (m_CutSceneCharacter != null)
		{
			m_CutSceneCharacter.EnableLayerAnimator(m_EnableLayering);
		}
	}
}
