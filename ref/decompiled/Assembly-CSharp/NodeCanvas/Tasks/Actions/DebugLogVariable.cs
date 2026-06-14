using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

[Description("Logs the value of a variable in the console")]
[Category("✫ Utility")]
public class DebugLogVariable : ActionTask
{
	[BlackboardOnly]
	public BBParameter<object> log;

	public BBParameter<string> prefix;

	public float secondsToRun = 1f;

	public CompactStatus finishStatus = CompactStatus.Success;

	protected override string info => string.Concat("Log '", log, "'", (!(secondsToRun > 0f)) ? string.Empty : (" for " + secondsToRun + " sec."));

	protected override void OnExecute()
	{
		Debug.Log($"<b>({base.agent.gameObject.name}) ({prefix.value}) | Var '{log.name}' = </b> {log.value}", base.agent.gameObject);
	}

	protected override void OnUpdate()
	{
		if (base.elapsedTime >= secondsToRun)
		{
			EndAction(finishStatus == CompactStatus.Success);
		}
	}
}
