using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Actions;

[Description("Start, Resume, Pause, Stop a GraphOwner's behaviour")]
[Category("✫ Utility")]
[Name("Control Graph Owner")]
public class GraphOwnerControl : ActionTask<GraphOwner>
{
	public enum Control
	{
		StartBehaviour,
		StopBehaviour,
		PauseBehaviour
	}

	public Control control;

	public bool waitActionFinish = true;

	protected override string info => base.agentInfo + "." + control;

	protected override void OnExecute()
	{
		StartCoroutine(Do());
	}

	private IEnumerator Do()
	{
		yield return null;
		if (control == Control.StartBehaviour)
		{
			if (waitActionFinish)
			{
				base.agent.StartBehaviour(delegate(bool s)
				{
					EndAction(s);
				});
			}
			else
			{
				EndAction();
				base.agent.StartBehaviour();
			}
		}
		if (control == Control.StopBehaviour)
		{
			EndAction();
			base.agent.StopBehaviour();
		}
		if (control == Control.PauseBehaviour)
		{
			EndAction();
			base.agent.PauseBehaviour();
		}
	}

	protected override void OnStop()
	{
		if (waitActionFinish && control == Control.StartBehaviour)
		{
			base.agent.StopBehaviour();
		}
	}
}
