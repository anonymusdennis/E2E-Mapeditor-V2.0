using UnityEngine;

namespace Slate.ActionClips;

[Category("Utility")]
public class DebugLog : DirectorActionClip
{
	public string text;

	public override string info => $"Debug Log\n'{text}'";

	protected override void OnEnter()
	{
		Debug.Log($"<b>Cutscene:</b> {text}");
	}
}
