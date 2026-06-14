using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[Name("Sub Dialogue Tree")]
[Icon("Dialogue", false)]
[Description("Execute a Sub Dialogue Tree. When that Dialogue Tree is finished, this node will continue instead if it has a connection.\nUseful for making reusable and self-contained Dialogue Trees.")]
[Category("Nested")]
public class SubDialogueTree : DTNode, IGraphAssignable, ISubParametersContainer
{
	[SerializeField]
	private BBParameter<DialogueTree> _subTree;

	[SerializeField]
	private Dictionary<string, string> actorParametersMap = new Dictionary<string, string>();

	[SerializeField]
	private Dictionary<string, BBObjectParameter> variablesMap = new Dictionary<string, BBObjectParameter>();

	private Dictionary<DialogueTree, DialogueTree> instances = new Dictionary<DialogueTree, DialogueTree>();

	Graph IGraphAssignable.nestedGraph
	{
		get
		{
			return subTree;
		}
		set
		{
			subTree = (DialogueTree)value;
		}
	}

	public override string name => "#" + base.ID + " SUB DIALOGUE";

	public DialogueTree subTree
	{
		get
		{
			return _subTree.value;
		}
		set
		{
			_subTree.value = value;
		}
	}

	BBParameter[] ISubParametersContainer.GetIncludeParseParameters()
	{
		return variablesMap.Values.ToArray();
	}

	Graph[] IGraphAssignable.GetInstances()
	{
		return instances.Values.ToArray();
	}

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (subTree == null)
		{
			return Error("No Sub Dialogue Tree assigned!");
		}
		CheckInstance();
		SetActorParametersMapping();
		SetVariablesMapping();
		subTree.StartGraph((!(base.finalActor is Component)) ? base.finalActor.transform : ((Component)base.finalActor), bb, autoUpdate: true, OnSubDialogueFinish);
		return Status.Running;
	}

	private void SetActorParametersMapping()
	{
		foreach (KeyValuePair<string, string> item in actorParametersMap)
		{
			DialogueTree.ActorParameter parameterByID = subTree.GetParameterByID(item.Key);
			DialogueTree.ActorParameter parameterByID2 = base.DLGTree.GetParameterByID(item.Value);
			if (parameterByID != null && parameterByID2 != null)
			{
				subTree.SetActorReference(parameterByID.name, parameterByID2.actor);
			}
		}
	}

	private void SetVariablesMapping()
	{
		foreach (KeyValuePair<string, BBObjectParameter> item in variablesMap)
		{
			if (!item.Value.isNone)
			{
				Variable variableByID = subTree.blackboard.GetVariableByID(item.Key);
				if (variableByID != null)
				{
					variableByID.value = item.Value.value;
				}
			}
		}
	}

	private void OnSubDialogueFinish(bool success)
	{
		base.status = (success ? Status.Success : Status.Failure);
		base.DLGTree.Continue();
	}

	public override void OnGraphStoped()
	{
		if (IsInstance(subTree))
		{
			subTree.Stop();
		}
	}

	public override void OnGraphPaused()
	{
		if (IsInstance(subTree))
		{
			subTree.Pause();
		}
	}

	private bool IsInstance(DialogueTree dt)
	{
		return instances.Values.Contains(dt);
	}

	private void CheckInstance()
	{
		if (!IsInstance(subTree))
		{
			DialogueTree value = null;
			if (!instances.TryGetValue(subTree, out value))
			{
				value = Graph.Clone(subTree);
				instances[subTree] = value;
			}
			value.agent = base.graphAgent;
			value.blackboard = base.graphBlackboard;
			subTree = value;
		}
	}
}
