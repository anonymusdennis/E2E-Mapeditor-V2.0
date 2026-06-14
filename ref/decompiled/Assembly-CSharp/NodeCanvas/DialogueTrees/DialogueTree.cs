using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[GraphInfo(packageName = "NodeCanvas", docsURL = "http://nodecanvas.paradoxnotion.com/documentation/", resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/", forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/")]
public class DialogueTree : Graph
{
	[Serializable]
	private struct DerivedSerializationData
	{
		public List<ActorParameter> actorParameters;
	}

	[Serializable]
	public class ActorParameter
	{
		[SerializeField]
		private string _keyName;

		[SerializeField]
		private string _id;

		[SerializeField]
		private UnityEngine.Object _actorObject;

		private IDialogueActor _actor;

		public string name
		{
			get
			{
				return _keyName;
			}
			set
			{
				_keyName = value;
			}
		}

		public string ID => (!string.IsNullOrEmpty(_id)) ? _id : (_id = Guid.NewGuid().ToString());

		public IDialogueActor actor
		{
			get
			{
				if (_actor == null)
				{
					_actor = _actorObject as IDialogueActor;
				}
				return _actor;
			}
			set
			{
				_actor = value;
				_actorObject = value as UnityEngine.Object;
			}
		}

		public ActorParameter()
		{
		}

		public ActorParameter(string name)
		{
			this.name = name;
		}

		public ActorParameter(string name, IDialogueActor actor)
		{
			this.name = name;
			this.actor = actor;
		}

		public override string ToString()
		{
			return name;
		}
	}

	public const string INSTIGATOR_NAME = "INSTIGATOR";

	[SerializeField]
	private List<ActorParameter> _actorParameters = new List<ActorParameter>();

	private DTNode currentNode;

	public static DialogueTree currentDialogue { get; private set; }

	public static DialogueTree previousDialogue { get; private set; }

	public override Type baseNodeType => typeof(DTNode);

	public override bool requiresAgent => false;

	public override bool requiresPrimeNode => true;

	public override bool autoSort => true;

	public override bool useLocalBlackboard => true;

	public List<ActorParameter> actorParameters => _actorParameters;

	public List<string> definedActorParameterNames
	{
		get
		{
			List<string> list = actorParameters.Select((ActorParameter r) => r.name).ToList();
			list.Insert(0, "INSTIGATOR");
			return list;
		}
	}

	public static event Action<DialogueTree> OnDialogueStarted;

	public static event Action<DialogueTree> OnDialoguePaused;

	public static event Action<DialogueTree> OnDialogueFinished;

	public static event Action<SubtitlesRequestInfo> OnSubtitlesRequest;

	public static event Action<MultipleChoiceRequestInfo> OnMultipleChoiceRequest;

	public override object OnDerivedDataSerialization()
	{
		DerivedSerializationData derivedSerializationData = default(DerivedSerializationData);
		derivedSerializationData.actorParameters = _actorParameters;
		return derivedSerializationData;
	}

	public override void OnDerivedDataDeserialization(object data)
	{
		if (data is DerivedSerializationData)
		{
			_actorParameters = ((DerivedSerializationData)data).actorParameters;
		}
	}

	public ActorParameter GetParameterByID(string id)
	{
		return actorParameters.Find((ActorParameter p) => p.ID == id);
	}

	public ActorParameter GetParameterByName(string paramName)
	{
		return actorParameters.Find((ActorParameter p) => p.name == paramName);
	}

	public IDialogueActor GetActorReferenceByID(string id)
	{
		ActorParameter parameterByID = GetParameterByID(id);
		return (parameterByID == null) ? null : GetActorReferenceByName(parameterByID.name);
	}

	public IDialogueActor GetActorReferenceByName(string paramName)
	{
		if (paramName == "INSTIGATOR")
		{
			if (base.agent is IDialogueActor)
			{
				return (IDialogueActor)base.agent;
			}
			if (base.agent != null)
			{
				return new ProxyDialogueActor(base.agent.gameObject.name, base.agent.transform);
			}
			return new ProxyDialogueActor("Null Instigator", null);
		}
		ActorParameter actorParameter = actorParameters.Find((ActorParameter r) => r.name == paramName);
		if (actorParameter != null && actorParameter.actor != null)
		{
			return actorParameter.actor;
		}
		Debug.Log($"<b>DialogueTree:</b> An actor entry '{paramName}' on DialogueTree has no reference. A dummy Actor will be used with the entry Key for name", this);
		return new ProxyDialogueActor(paramName, null);
	}

	public void SetActorReference(string paramName, IDialogueActor actor)
	{
		ActorParameter actorParameter = actorParameters.Find((ActorParameter p) => p.name == paramName);
		if (actorParameter == null)
		{
			Debug.LogError($"There is no defined Actor key name '{paramName}'");
		}
		else
		{
			actorParameter.actor = actor;
		}
	}

	public void SetActorReferences(Dictionary<string, IDialogueActor> actors)
	{
		foreach (KeyValuePair<string, IDialogueActor> pair in actors)
		{
			ActorParameter actorParameter = actorParameters.Find((ActorParameter p) => p.name == pair.Key);
			if (actorParameter == null)
			{
				Debug.LogWarning($"There is no defined Actor key name '{pair.Key}'. Seting actor skiped");
			}
			else
			{
				actorParameter.actor = pair.Value;
			}
		}
	}

	public void Continue(int index = 0)
	{
		if (base.isRunning)
		{
			if (index < 0 || index > currentNode.outConnections.Count - 1)
			{
				Stop();
			}
			else
			{
				EnterNode((DTNode)currentNode.outConnections[index].targetNode);
			}
		}
	}

	public void EnterNode(DTNode node)
	{
		currentNode = node;
		currentNode.Reset(recursively: false);
		if (currentNode.Execute(base.agent, base.blackboard) == Status.Error)
		{
			Stop(success: false);
		}
	}

	public static void RequestSubtitles(SubtitlesRequestInfo info)
	{
		if (DialogueTree.OnSubtitlesRequest != null)
		{
			DialogueTree.OnSubtitlesRequest(info);
		}
		else
		{
			Debug.LogWarning("<b>DialogueTree:</b> Subtitle Request event has no subscribers. Make sure to add the default '@DialogueGUI' prefab or create your own GUI.");
		}
	}

	public static void RequestMultipleChoices(MultipleChoiceRequestInfo info)
	{
		if (DialogueTree.OnMultipleChoiceRequest != null)
		{
			DialogueTree.OnMultipleChoiceRequest(info);
		}
		else
		{
			Debug.LogWarning("<b>DialogueTree:</b> Multiple Choice Request event has no subscribers. Make sure to add the default '@DialogueGUI' prefab or create your own GUI.");
		}
	}

	protected override void OnGraphStarted()
	{
		previousDialogue = currentDialogue;
		currentDialogue = this;
		Debug.Log($"<b>DialogueTree:</b> Dialogue Started '{base.name}'");
		if (DialogueTree.OnDialogueStarted != null)
		{
			DialogueTree.OnDialogueStarted(this);
		}
		if (!(base.agent is IDialogueActor))
		{
			Debug.Log("<b>DialogueTree:</b> INSTIGATOR agent used in DialogueTree does not implement IDialogueActor. A dummy actor will be used.");
		}
		currentNode = ((currentNode == null) ? ((DTNode)base.primeNode) : currentNode);
		EnterNode(currentNode);
	}

	protected override void OnGraphUnpaused()
	{
		currentNode = ((currentNode == null) ? ((DTNode)base.primeNode) : currentNode);
		EnterNode(currentNode);
		Debug.Log($"<b>DialogueTree:</b> Dialogue Resumed '{base.name}'");
		if (DialogueTree.OnDialogueStarted != null)
		{
			DialogueTree.OnDialogueStarted(this);
		}
	}

	protected override void OnGraphStoped()
	{
		currentDialogue = previousDialogue;
		previousDialogue = null;
		currentNode = null;
		Debug.Log($"<b>DialogueTree:</b> Dialogue Finished '{base.name}'");
		if (DialogueTree.OnDialogueFinished != null)
		{
			DialogueTree.OnDialogueFinished(this);
		}
	}

	protected override void OnGraphPaused()
	{
		Debug.Log($"<b>DialogueTree:</b> Dialogue Paused '{base.name}'");
		if (DialogueTree.OnDialoguePaused != null)
		{
			DialogueTree.OnDialoguePaused(this);
		}
	}
}
