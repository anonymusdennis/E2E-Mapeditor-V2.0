using System;
using Slate;
using UnityEngine;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Characters")]
public class CharacterSetNaked : ActionClip
{
	public bool m_bForceIsNaked;

	private CS_HijackIngameCharacter m_HijackedCharacter;

	[HideInInspector]
	public string InfoString = string.Empty;

	public override string info
	{
		get
		{
			if (string.IsNullOrEmpty(InfoString))
			{
				return "Set Naked: " + m_bForceIsNaked;
			}
			return InfoString;
		}
	}

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_HijackedCharacter == null)
		{
			m_HijackedCharacter = base.actor.GetAddComponent<CS_HijackIngameCharacter>();
		}
		m_HijackedCharacter.SetCharacterNaked(m_bForceIsNaked);
	}
}
