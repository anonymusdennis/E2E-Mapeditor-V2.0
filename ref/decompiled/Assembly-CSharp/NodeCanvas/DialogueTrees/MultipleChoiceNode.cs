using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Description("Prompt a Dialogue Multiple Choice. A choice will be available if the connection's condition is true or there is no condition on that connection. The Actor selected is used for the Condition checks as well as will Say the selection if the option is checked.")]
[Name("Multiple Choice")]
public class MultipleChoiceNode : DTNode, ISubTasksContainer
{
	[Serializable]
	public class Choice
	{
		public bool isUnfolded = true;

		public Statement statement;

		public ConditionTask condition;

		public Choice()
		{
		}

		public Choice(Statement statement)
		{
			this.statement = statement;
		}
	}

	public float availableTime;

	public bool saySelection;

	public List<Choice> availableChoices = new List<Choice>();

	public override int maxOutConnections => availableChoices.Count;

	public Task[] GetTasks()
	{
		return (availableChoices == null) ? new Task[0] : availableChoices.Select((Choice c) => c.condition).ToArray();
	}

	public override void OnChildConnected(int index)
	{
	}

	public override void OnChildDisconnected(int index)
	{
	}

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (base.outConnections.Count == 0)
		{
			return Error("There are no connections to the Multiple Choice Node!");
		}
		Dictionary<IStatement, int> dictionary = new Dictionary<IStatement, int>();
		for (int i = 0; i < availableChoices.Count; i++)
		{
			ConditionTask condition = availableChoices[i].condition;
			if (condition == null || condition.CheckCondition(base.finalActor.transform, bb))
			{
				Statement key = availableChoices[i].statement.BlackboardReplace(bb);
				dictionary[key] = i;
			}
		}
		if (dictionary.Count == 0)
		{
			Debug.Log("Multiple Choice Node has no available options. Dialogue Ends");
			base.DLGTree.Stop(success: false);
			return Status.Failure;
		}
		MultipleChoiceRequestInfo multipleChoiceRequestInfo = new MultipleChoiceRequestInfo(dictionary, availableTime, OnOptionSelected);
		multipleChoiceRequestInfo.showLastStatement = base.inConnections.Count > 0 && base.inConnections[0].sourceNode is StatementNode;
		DialogueTree.RequestMultipleChoices(multipleChoiceRequestInfo);
		return Status.Running;
	}

	private void OnOptionSelected(int index)
	{
		base.status = Status.Success;
		Action action = delegate
		{
			base.DLGTree.Continue(index);
		};
		if (saySelection)
		{
			Statement statement = availableChoices[index].statement.BlackboardReplace(base.graphBlackboard);
			SubtitlesRequestInfo info = new SubtitlesRequestInfo(base.finalActor, statement, action);
			DialogueTree.RequestSubtitles(info);
		}
		else
		{
			action();
		}
	}
}
