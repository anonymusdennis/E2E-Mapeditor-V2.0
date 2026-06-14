using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Framework;

[DoNotList]
public class ActionList : ActionTask
{
	public enum ActionsExecutionMode
	{
		ActionsRunInSequence,
		ActionsRunInParallel
	}

	public ActionsExecutionMode executionMode;

	public List<ActionTask> actions = new List<ActionTask>();

	private int currentActionIndex;

	private readonly List<int> finishedIndeces = new List<int>();

	protected override string info
	{
		get
		{
			if (actions.Count == 0)
			{
				return "No Actions";
			}
			string text = string.Empty;
			for (int i = 0; i < actions.Count; i++)
			{
				ActionTask actionTask = actions[i];
				if (actionTask != null && actionTask.isActive)
				{
					string text2 = (actionTask.isPaused ? "<b>||</b> " : ((!actionTask.isRunning) ? string.Empty : "► "));
					text = text + text2 + actionTask.summaryInfo + ((i != actions.Count - 1) ? "\n" : string.Empty);
				}
			}
			return text;
		}
	}

	public override Task Duplicate(ITaskSystem newOwnerSystem)
	{
		ActionList actionList = (ActionList)base.Duplicate(newOwnerSystem);
		actionList.actions.Clear();
		foreach (ActionTask action in actions)
		{
			actionList.AddAction((ActionTask)action.Duplicate(newOwnerSystem));
		}
		return actionList;
	}

	protected override void OnExecute()
	{
		finishedIndeces.Clear();
		currentActionIndex = 0;
	}

	protected override void OnUpdate()
	{
		if (actions.Count == 0)
		{
			EndAction();
			return;
		}
		if (executionMode == ActionsExecutionMode.ActionsRunInParallel)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				if (finishedIndeces.Contains(i))
				{
					continue;
				}
				if (!actions[i].isActive)
				{
					finishedIndeces.Add(i);
					continue;
				}
				switch (actions[i].ExecuteAction(base.agent, base.blackboard))
				{
				case Status.Failure:
					EndAction(false);
					return;
				case Status.Success:
					finishedIndeces.Add(i);
					break;
				}
			}
			if (finishedIndeces.Count == actions.Count)
			{
				EndAction(true);
			}
			return;
		}
		for (int j = currentActionIndex; j < actions.Count; j++)
		{
			if (actions[j].isActive)
			{
				switch (actions[j].ExecuteAction(base.agent, base.blackboard))
				{
				case Status.Failure:
					EndAction(false);
					return;
				case Status.Running:
					currentActionIndex = j;
					return;
				}
			}
		}
		EndAction(true);
	}

	protected override void OnStop()
	{
		for (int i = 0; i < actions.Count; i++)
		{
			actions[i].EndAction(null);
		}
	}

	protected override void OnPause()
	{
		for (int i = 0; i < actions.Count; i++)
		{
			actions[i].PauseAction();
		}
	}

	public override void OnDrawGizmos()
	{
		for (int i = 0; i < actions.Count; i++)
		{
			actions[i].OnDrawGizmos();
		}
	}

	public override void OnDrawGizmosSelected()
	{
		for (int i = 0; i < actions.Count; i++)
		{
			actions[i].OnDrawGizmosSelected();
		}
	}

	public void AddAction(ActionTask action)
	{
		if (action is ActionList)
		{
			Debug.LogWarning("Adding an ActionList within another ActionList is not allowed for clarity");
			return;
		}
		actions.Add(action);
		action.SetOwnerSystem(base.ownerSystem);
	}
}
