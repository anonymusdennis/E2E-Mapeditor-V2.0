using System;
using Slate;
using Slate.ActionClips;
using UnityEngine;

namespace T17.Slate.ActionClips;

[Attachable(new Type[] { typeof(ActorActionTrack) })]
[Category("T17 Characters")]
public class CharacterSpeak : ActorActionClip
{
	public string m_TextID;

	public SpeechTone m_SpeechTone;

	public CutsceneSpeechBubbleHandler.BubblePresets m_Preset = CutsceneSpeechBubbleHandler.BubblePresets.Unassigned;

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

	public override string info => "'" + m_TextID + "' (" + m_SpeechTone.ToString() + ")";

	protected override void OnEnter()
	{
		base.OnEnter();
		CutsceneSpeechBubbleHandler componentInChildren = base.actor.GetComponentInChildren<CutsceneSpeechBubbleHandler>(includeInactive: true);
		if (componentInChildren != null)
		{
			string localized = string.Empty;
			if (m_Preset != CutsceneSpeechBubbleHandler.BubblePresets.Unassigned)
			{
				componentInChildren.SetPreset(m_Preset);
			}
			if (Localization.Get(m_TextID, out localized, -1))
			{
				componentInChildren.NewSpeech(localized, m_SpeechTone, length, 10000, bAllowTextColourControl: false);
			}
			else
			{
				componentInChildren.NewSpeech(m_TextID, m_SpeechTone, length, 10000, bAllowTextColourControl: false);
			}
		}
	}
}
