using System;
using Slate;

namespace T17.Slate;

[Attachable(new Type[] { typeof(ActorAudioTrack) })]
[Category("Wwise")]
public class TriggerAudioEvent : ActionClip
{
	[Required]
	public string m_AudioEventString;

	public AudioController.SOUND_AREA m_SoundArea = AudioController.SOUND_AREA.SA_INGAME;

	public override string info => "Actor playing " + m_AudioEventString + " in " + m_SoundArea;

	protected override void OnEnter()
	{
		base.OnEnter();
		if (CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			AudioController.SendEvent(m_SoundArea, m_AudioEventString, base.actor);
		}
	}
}
