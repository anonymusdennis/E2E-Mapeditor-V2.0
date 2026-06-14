using System;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

public abstract class DTNode : Node
{
	[SerializeField]
	private string _actorName = "INSTIGATOR";

	[SerializeField]
	private string _actorParameterID;

	public override string name
	{
		get
		{
			if (DLGTree.definedActorParameterNames.Contains(actorName))
			{
				return $"#{base.ID} {actorName}";
			}
			return $"#{base.ID} <color=#d63e3e>* {_actorName} *</color>";
		}
	}

	public override int maxInConnections => -1;

	public override int maxOutConnections => 1;

	public sealed override Type outConnectionType => typeof(DTConnection);

	public sealed override bool allowAsPrime => true;

	public sealed override bool showCommentsBottom => false;

	protected DialogueTree DLGTree => (DialogueTree)base.graph;

	protected string actorName
	{
		get
		{
			DialogueTree.ActorParameter parameterByID = DLGTree.GetParameterByID(_actorParameterID);
			return (parameterByID == null) ? _actorName : parameterByID.name;
		}
		set
		{
			if (_actorName != value && !string.IsNullOrEmpty(value))
			{
				_actorName = value;
				_actorParameterID = DLGTree.GetParameterByName(value)?.ID;
			}
		}
	}

	protected IDialogueActor finalActor
	{
		get
		{
			IDialogueActor actorReferenceByID = DLGTree.GetActorReferenceByID(_actorParameterID);
			return (actorReferenceByID == null) ? DLGTree.GetActorReferenceByName(_actorName) : actorReferenceByID;
		}
	}
}
