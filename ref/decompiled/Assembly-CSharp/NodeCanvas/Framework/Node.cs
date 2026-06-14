using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Services;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
[SpoofAOT]
public abstract class Node
{
	[SerializeField]
	private Vector2 _position = Vector2.zero;

	[SerializeField]
	private string _name;

	[SerializeField]
	private string _tag;

	[SerializeField]
	private string _comment;

	[SerializeField]
	private bool _isBreakpoint;

	private Graph _graph;

	private List<Connection> _inConnections = new List<Connection>();

	private List<Connection> _outConnections = new List<Connection>();

	private int _ID;

	[NonSerialized]
	private Status _status = Status.Resting;

	[NonSerialized]
	private string _nodeName;

	[NonSerialized]
	private string _nodeDescription;

	private bool _preInitialised;

	public Vector2 nodePosition
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Graph graph
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = value;
		}
	}

	public int ID
	{
		get
		{
			return _ID;
		}
		set
		{
			_ID = value;
		}
	}

	public List<Connection> inConnections
	{
		get
		{
			return _inConnections;
		}
		protected set
		{
			_inConnections = value;
		}
	}

	public List<Connection> outConnections
	{
		get
		{
			return _outConnections;
		}
		protected set
		{
			_outConnections = value;
		}
	}

	private string customName
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public string tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public string nodeComment
	{
		get
		{
			return _comment;
		}
		set
		{
			_comment = value;
		}
	}

	public bool isBreakpoint
	{
		get
		{
			return _isBreakpoint;
		}
		set
		{
			_isBreakpoint = value;
		}
	}

	public virtual string name
	{
		get
		{
			if (!string.IsNullOrEmpty(customName))
			{
				return customName;
			}
			if (string.IsNullOrEmpty(_nodeName))
			{
				NameAttribute nameAttribute = GetType().RTGetAttribute<NameAttribute>(inherited: false);
				_nodeName = ((nameAttribute == null) ? GetType().FriendlyName().SplitCamelCase() : nameAttribute.name);
			}
			return _nodeName;
		}
		set
		{
			customName = value;
		}
	}

	public virtual string description
	{
		get
		{
			if (string.IsNullOrEmpty(_nodeDescription))
			{
				DescriptionAttribute descriptionAttribute = GetType().RTGetAttribute<DescriptionAttribute>(inherited: false);
				_nodeDescription = ((descriptionAttribute == null) ? "No Description" : descriptionAttribute.description);
			}
			return _nodeDescription;
		}
	}

	public abstract int maxInConnections { get; }

	public abstract int maxOutConnections { get; }

	public abstract Type outConnectionType { get; }

	public abstract bool allowAsPrime { get; }

	public abstract bool showCommentsBottom { get; }

	public Status status
	{
		get
		{
			return _status;
		}
		protected set
		{
			_status = value;
		}
	}

	public Component graphAgent => (!(graph != null)) ? null : graph.agent;

	public IBlackboard graphBlackboard => (!(graph != null)) ? null : graph.blackboard;

	private bool isChecked { get; set; }

	public Node()
	{
	}

	public static Node Create(Graph targetGraph, Type nodeType, Vector2 pos)
	{
		if (targetGraph == null)
		{
			Debug.LogError("Can't Create a Node without providing a Target Graph");
			return null;
		}
		Node node = (Node)Activator.CreateInstance(nodeType);
		node.graph = targetGraph;
		node.nodePosition = pos;
		BBParameter.SetBBFields(node, targetGraph.blackboard);
		node.OnValidate(targetGraph);
		return node;
	}

	public Node Duplicate(Graph targetGraph)
	{
		if (targetGraph == null)
		{
			Debug.LogError("Can't duplicate a Node without providing a Target Graph");
			return null;
		}
		Node node = JSONSerializer.Deserialize<Node>(JSONSerializer.Serialize(typeof(Node), this));
		targetGraph.allNodes.Add(node);
		node.inConnections.Clear();
		node.outConnections.Clear();
		if (targetGraph == graph)
		{
			node.nodePosition += new Vector2(50f, 50f);
		}
		node.graph = targetGraph;
		BBParameter.SetBBFields(node, targetGraph.blackboard);
		if (this is ITaskAssignable { task: not null } taskAssignable)
		{
			(node as ITaskAssignable).task = taskAssignable.task.Duplicate(targetGraph);
		}
		node.OnValidate(targetGraph);
		return node;
	}

	public virtual void OnValidate(Graph assignedGraph)
	{
	}

	public virtual void OnDestroy()
	{
	}

	public Status Execute(Component agent, IBlackboard blackboard)
	{
		if (isChecked)
		{
			BehaviourTreeOwner behaviourTreeOwner = agent as BehaviourTreeOwner;
			if (behaviourTreeOwner != null && AIBehaviourRecoveryManager.GetInstance() != null)
			{
				AIBehaviourRecoveryManager.GetInstance().RecoverAgent(behaviourTreeOwner);
			}
			return Error(string.Empty);
		}
		isChecked = true;
		status = OnExecute(agent, blackboard);
		isChecked = false;
		return status;
	}

	private IEnumerator YieldBreak(Component agent, IBlackboard blackboard)
	{
		Debug.Break();
		yield return null;
		status = OnExecute(agent, blackboard);
	}

	protected Status Error(string log)
	{
		return Status.Error;
	}

	public virtual void PreInitialise()
	{
		if (!_preInitialised)
		{
			for (int i = 0; i < outConnections.Count; i++)
			{
				outConnections[i].PreInitialise();
			}
			_preInitialised = true;
		}
	}

	public void Reset(bool recursively = true)
	{
		if (status != Status.Resting && !isChecked)
		{
			OnReset();
			status = Status.Resting;
			isChecked = true;
			for (int i = 0; i < outConnections.Count; i++)
			{
				outConnections[i].Reset(recursively);
			}
			isChecked = false;
		}
	}

	public void ForceReset(bool recursively = true)
	{
		OnForceReset();
		status = Status.Resting;
		for (int i = 0; i < outConnections.Count; i++)
		{
			outConnections[i].ForceReset(recursively);
		}
		isChecked = false;
	}

	public void SendEvent(EventData eventData)
	{
		graph.SendEvent(eventData);
	}

	public void RegisterEvents(params string[] eventNames)
	{
		RegisterEvents(graphAgent, eventNames);
	}

	public void RegisterEvents(Component targetAgent, params string[] eventNames)
	{
		if (targetAgent == null)
		{
			Debug.LogError("Null Agent provided for event registration");
			return;
		}
		MessageRouter messageRouter = targetAgent.GetComponent<MessageRouter>();
		if (messageRouter == null)
		{
			messageRouter = targetAgent.gameObject.AddComponent<MessageRouter>();
		}
		messageRouter.Register(this, eventNames);
	}

	public void UnRegisterEvents(params string[] eventNames)
	{
		UnRegisterEvents(graphAgent, eventNames);
	}

	public void UnRegisterEvents(Component targetAgent, params string[] eventNames)
	{
		if (!(targetAgent == null))
		{
			MessageRouter component = targetAgent.GetComponent<MessageRouter>();
			if (component != null)
			{
				component.UnRegister(this, eventNames);
			}
		}
	}

	public void UnregisterAllEvents()
	{
		UnregisterAllEvents(graphAgent);
	}

	public void UnregisterAllEvents(Component targetAgent)
	{
		if (!(targetAgent == null))
		{
			MessageRouter component = targetAgent.GetComponent<MessageRouter>();
			if (component != null)
			{
				component.UnRegister(this);
			}
		}
	}

	public bool IsNewConnectionAllowed()
	{
		return IsNewConnectionAllowed(null);
	}

	public bool IsNewConnectionAllowed(Node sourceNode)
	{
		if (sourceNode != null)
		{
			if (this == sourceNode)
			{
				Debug.LogWarning("Node can't connect to itself");
				return false;
			}
			if (sourceNode.outConnections.Count >= sourceNode.maxOutConnections && sourceNode.maxOutConnections != -1)
			{
				Debug.LogWarning("Source node can have no more out connections.");
				return false;
			}
		}
		if (this == graph.primeNode && maxInConnections == 1)
		{
			Debug.LogWarning("Target node can have no more connections");
			return false;
		}
		if (maxInConnections <= inConnections.Count && maxInConnections != -1)
		{
			Debug.LogWarning("Target node can have no more connections");
			return false;
		}
		return true;
	}

	public int AssignIDToGraph(int lastID)
	{
		if (isChecked)
		{
			return lastID;
		}
		isChecked = true;
		lastID++;
		ID = lastID;
		for (int i = 0; i < outConnections.Count; i++)
		{
			lastID = outConnections[i].targetNode.AssignIDToGraph(lastID);
		}
		return lastID;
	}

	public void ResetRecursion()
	{
		if (isChecked)
		{
			isChecked = false;
			for (int i = 0; i < outConnections.Count; i++)
			{
				outConnections[i].targetNode.ResetRecursion();
			}
		}
	}

	protected Coroutine StartCoroutine(IEnumerator routine)
	{
		return MonoManager.current.StartCoroutine(routine);
	}

	protected void StopCoroutine(Coroutine routine)
	{
		MonoManager.current.StopCoroutine(routine);
	}

	public List<Node> GetParentNodes()
	{
		if (inConnections.Count != 0)
		{
			return inConnections.Select((Connection c) => c.sourceNode).ToList();
		}
		return new List<Node>();
	}

	public List<Node> GetChildNodes()
	{
		if (outConnections.Count != 0)
		{
			return outConnections.Select((Connection c) => c.targetNode).ToList();
		}
		return new List<Node>();
	}

	protected virtual Status OnExecute(Component agent, IBlackboard blackboard)
	{
		return status;
	}

	protected virtual void OnReset()
	{
	}

	protected virtual void OnForceReset()
	{
		OnReset();
	}

	public virtual void OnParentConnected(int connectionIndex)
	{
	}

	public virtual void OnParentDisconnected(int connectionIndex)
	{
	}

	public virtual void OnChildConnected(int connectionIndex)
	{
	}

	public virtual void OnChildDisconnected(int connectionIndex)
	{
	}

	public virtual void OnGraphStarted()
	{
	}

	public virtual void OnGraphStoped()
	{
	}

	public virtual void OnGraphPaused()
	{
	}

	public virtual void OnGraphUnpaused()
	{
	}

	public sealed override string ToString()
	{
		ITaskAssignable taskAssignable = this as ITaskAssignable;
		return $"{name} ({((taskAssignable == null || taskAssignable.task == null) ? string.Empty : taskAssignable.task.ToString())})";
	}

	public void OnDrawGizmos()
	{
		if (this is ITaskAssignable && (this as ITaskAssignable).task != null)
		{
			(this as ITaskAssignable).task.OnDrawGizmos();
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (this is ITaskAssignable && (this as ITaskAssignable).task != null)
		{
			(this as ITaskAssignable).task.OnDrawGizmosSelected();
		}
	}
}
