using System;
using Slate;

namespace T17.Slate;

[Category("Wwise")]
[Attachable(new Type[] { typeof(DirectorAudioTrack) })]
public class StopRoutineMusic : DirectorActionClip
{
	public override string info => "Stopping routine music";

	protected override void OnEnter()
	{
		base.OnEnter();
		if (CutsceneManagerBase.GetState() != CutsceneManagerBase.States.SkippingCurrent)
		{
			AudioController.StopRoutineMusic(RoutineManager.GetInstance().GetCurrentRoutineMusic());
		}
	}
}
