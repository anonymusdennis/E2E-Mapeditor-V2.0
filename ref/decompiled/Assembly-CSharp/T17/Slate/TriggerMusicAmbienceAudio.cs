using System;
using Slate;

namespace T17.Slate;

[Attachable(new Type[] { typeof(DirectorAudioTrack) })]
[Category("Wwise")]
public class TriggerMusicAmbienceAudio : DirectorActionClip
{
	[Required]
	public string m_AudioEvent;

	public AudioController.SOUND_AREA m_SoundArea = AudioController.SOUND_AREA.SA_INGAME;

	public bool m_bRunEvenDuringSkip;

	public override string info => "Ambiance in  " + m_SoundArea.ToString() + " to " + m_AudioEvent;

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_bRunEvenDuringSkip || CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			AudioController.SendEvent(m_SoundArea, m_AudioEvent, AudioController.InGameMusicAndAmbienceObject);
		}
	}
}
