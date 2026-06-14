using System;
using AUTOGEN_T17Wwise_Enums;
using Slate;

namespace T17.Slate;

[Attachable(new Type[] { typeof(DirectorAudioTrack) })]
[Category("Wwise")]
public class SetWwiseSwitch : DirectorActionClip
{
	[Required]
	public string m_SwitchState;

	public Switch_Group m_SwitchGroup;

	public bool m_bRunEvenDuringSkip;

	public override string info => "Setting " + m_SwitchGroup.ToString() + " to " + m_SwitchState;

	protected override void OnEnter()
	{
		base.OnEnter();
		if (m_bRunEvenDuringSkip || CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			AudioController.SetSwitch(m_SwitchGroup, m_SwitchState, AudioController.InGameMusicAndAmbienceObject);
		}
	}
}
