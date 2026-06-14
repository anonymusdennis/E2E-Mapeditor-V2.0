using System;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

public class DialogueTreeController : GraphOwner<DialogueTree>, IDialogueActor
{
	string IDialogueActor.name => base.name;

	Texture2D IDialogueActor.portrait => null;

	Sprite IDialogueActor.portraitSprite => null;

	Color IDialogueActor.dialogueColor => Color.white;

	Vector3 IDialogueActor.dialoguePosition => Vector3.zero;

	Transform IDialogueActor.transform => base.transform;

	public void StartDialogue()
	{
		graph = GetInstance(graph);
		graph.StartGraph(this, blackboard, autoUpdate: true);
	}

	public void StartDialogue(IDialogueActor instigator)
	{
		graph = GetInstance(graph);
		graph.StartGraph((!(instigator is Component)) ? instigator.transform : ((Component)instigator), blackboard, autoUpdate: true);
	}

	public void StartDialogue(IDialogueActor instigator, Action<bool> callback)
	{
		graph = GetInstance(graph);
		graph.StartGraph((!(instigator is Component)) ? instigator.transform : ((Component)instigator), blackboard, autoUpdate: true, callback);
	}

	public void StartDialogue(Action<bool> callback)
	{
		graph = GetInstance(graph);
		graph.StartGraph(this, blackboard, autoUpdate: true, callback);
	}
}
