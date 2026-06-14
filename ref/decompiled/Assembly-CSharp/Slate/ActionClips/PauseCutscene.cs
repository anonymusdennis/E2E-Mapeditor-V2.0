using UnityEngine;

namespace Slate.ActionClips;

[Description("Pauses the Cutscene. It's up to other scripts to resume it.")]
[Category("Utility")]
public class PauseCutscene : DirectorActionClip
{
	protected override void OnEnter()
	{
		if (Application.isPlaying)
		{
			(base.root as Cutscene).Pause();
		}
	}
}
