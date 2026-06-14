using System;
using AUTOGEN_T17Wwise_Enums;
using Slate;

namespace T17.Slate;

[Attachable(new Type[] { typeof(DirectorAudioTrack) })]
[Category("Wwise")]
public class SetWwiseState : DirectorActionClip
{
	[Required]
	public string m_State;

	public State_Group m_StateGroup;

	public AudioController.SOUND_AREA m_SoundArea = AudioController.SOUND_AREA.SA_INGAME;

	public bool m_bRunEvenDuringSkip;

	public override string info => "Setting " + m_StateGroup.ToString() + " to " + m_State;

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_bRunEvenDuringSkip || CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			AudioController.SetState(m_StateGroup, m_State);
		}
	}
}
