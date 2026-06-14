using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using ParadoxNotion.Services;
using UnityEngine;

namespace NodeCanvas.Framework;

[Serializable]
public abstract class Graph : ScriptableObject, ITaskSystem, ISerializationCallbackReceiver, IScriptableComponent
{
	[SerializeField]
	private string _serializedGraph;

	[SerializeField]
	private List<UnityEngine.Object> _objectReferences;

	[SerializeField]
	private bool _deserializationFailed;

	[NonSerialized]
	private bool hasEnabled;

	[NonSerialized]
	private bool hasDeserialized;

	private string _name = string.Empty;

	private string _comments = string.Empty;

	private Vector2 _translation = new Vector2(-5000f, -5000f);

	private float _zoomFactor = 1f;

	private List<Node> _nodes = new List<Node>();

	private Node _primeNode;

	private List<CanvasGroup> _canvasGroups;

	private BlackboardSource _localBlackboard;

	[NonSerialized]
	private Component _agent;

	[NonSerialized]
	private IBlackboard _blackboard;

	[NonSerialized]
	private static List<Graph> runningGraphs = new List<Graph>();

	[NonSerialized]
	private float timeStarted;

	[NonSerialized]
	private bool _isRunning;

	[NonSerialized]
	private bool _isPaused;

	UnityEngine.Object ITaskSystem.contextObject => this;

	public abstract Type baseNodeType { get; }

	public abstract bool requiresAgent { get; }

	public abstract bool requiresPrimeNode { get; }

	public abstract bool autoSort { get; }

	public abstract bool useLocalBlackboard { get; }

	public new string name
	{
		get
		{
			return (!string.IsNullOrEmpty(_name)) ? _name : GetType().Name;
		}
		set
		{
			_name = value;
		}
	}

	public string graphComments
	{
		get
		{
			return _comments;
		}
		set
		{
			_comments = value;
		}
	}

	public float elapsedTime => (!isRunning && !isPaused) ? 0f : (UpdateManager.time - timeStarted);

	public bool isRunning
	{
		get
		{
			return _isRunning;
		}
		private set
		{
			_isRunning = value;
		}
	}

	public bool isPaused
	{
		get
		{
			return _isPaused;
		}
		private set
		{
			_isPaused = value;
		}
	}

	public List<Node> allNodes
	{
		get
		{
			return _nodes;
		}
		private set
		{
			_nodes = value;
		}
	}

	public Node primeNode
	{
		get
		{
			return _primeNode;
		}
		set
		{
			if (_primeNode == value || (value != null && !value.allowAsPrime))
			{
				return;
			}
			if (isRunning)
			{
				if (_primeNode != null)
				{
					_primeNode.Reset();
				}
				value?.Reset();
			}
			RecordUndo("Set Start");
			_primeNode = value;
			UpdateNodeIDs(alsoReorderList: true);
		}
	}

	public List<CanvasGroup> canvasGroups
	{
		get
		{
			return _canvasGroups;
		}
		set
		{
			_canvasGroups = value;
		}
	}

	public Vector2 translation
	{
		get
		{
			return _translation;
		}
		set
		{
			_translation = value;
		}
	}

	public float zoomFactor
	{
		get
		{
			return _zoomFactor;
		}
		set
		{
			_zoomFactor = value;
		}
	}

	public Component agent
	{
		get
		{
			return _agent;
		}
		set
		{
			_agent = value;
		}
	}

	public IBlackboard blackboard
	{
		get
		{
			if (useLocalBlackboard)
			{
				return localBlackboard;
			}
			return _blackboard;
		}
		set
		{
			if (_blackboard != value && !isRunning && !useLocalBlackboard)
			{
				_blackboard = value;
			}
		}
	}

	public BlackboardSource localBlackboard
	{
		get
		{
			if (object.ReferenceEquals(_localBlackboard, null))
			{
				_localBlackboard = new BlackboardSource();
				_localBlackboard.name = "Local Blackboard";
			}
			return _localBlackboard;
		}
	}

	public event Action<bool> OnFinish;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		Serialize();
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		Deserialize();
	}

	public void Serialize()
	{
		if (_objectReferences != null && _objectReferences.Count > 0 && _objectReferences.Any((UnityEngine.Object o) => o != null))
		{
			hasDeserialized = false;
		}
	}

	public void Deserialize()
	{
		if (!hasDeserialized || !JSONSerializer.applicationPlaying)
		{
			hasDeserialized = true;
			Deserialize(_serializedGraph, validate: false, _objectReferences);
		}
	}

	protected void OnEnable()
	{
		if (!hasEnabled)
		{
			hasEnabled = true;
			Validate();
		}
	}

	protected void OnDisable()
	{
	}

	protected void OnDestroy()
	{
	}

	public string Serialize(bool pretyJson, List<UnityEngine.Object> objectReferences)
	{
		if (_deserializationFailed)
		{
			_deserializationFailed = false;
			return _serializedGraph;
		}
		if (objectReferences == null)
		{
			objectReferences = (_objectReferences = new List<UnityEngine.Object>());
		}
		UpdateNodeIDs(alsoReorderList: true);
		return JSONSerializer.Serialize(typeof(GraphSerializationData), new GraphSerializationData(this), pretyJson, objectReferences);
	}

	public GraphSerializationData Deserialize(string serializedGraph, bool validate, List<UnityEngine.Object> objectReferences)
	{
		if (string.IsNullOrEmpty(serializedGraph))
		{
			return null;
		}
		if (objectReferences == null)
		{
			objectReferences = _objectReferences;
		}
		try
		{
			GraphSerializationData graphSerializationData = JSONSerializer.Deserialize<GraphSerializationData>(serializedGraph, objectReferences);
			if (LoadGraphData(graphSerializationData, validate))
			{
				_deserializationFailed = false;
				_serializedGraph = serializedGraph;
				_objectReferences = objectReferences;
				return graphSerializationData;
			}
			_deserializationFailed = true;
			return null;
		}
		catch (Exception ex)
		{
			Debug.LogError($"<b>(Deserialization Error:)</b> {ex.ToString()}", this);
			_deserializationFailed = true;
			return null;
		}
	}

	private bool LoadGraphData(GraphSerializationData data, bool validate)
	{
		if (data == null)
		{
			Debug.LogError("Can't Load graph, cause of null GraphSerializationData provided");
			return false;
		}
		if (data.type != GetType())
		{
			Debug.LogError("Can't Load graph, cause of different Graph type serialized and required");
			return false;
		}
		data.Reconstruct(this);
		_name = data.name;
		_comments = data.comments;
		_translation = data.translation;
		_zoomFactor = data.zoomFactor;
		_nodes = data.nodes;
		_primeNode = data.primeNode;
		_canvasGroups = data.canvasGroups;
		_localBlackboard = data.localBlackboard;
		if (validate)
		{
			Validate();
		}
		return true;
	}

	public virtual object OnDerivedDataSerialization()
	{
		return null;
	}

	public virtual void OnDerivedDataDeserialization(object data)
	{
	}

	public void GetSerializationData(out string json, out List<UnityEngine.Object> references)
	{
		json = _serializedGraph;
		references = ((_objectReferences == null) ? null : new List<UnityEngine.Object>(_objectReferences));
	}

	public void SetSerializationObjectReferences(List<UnityEngine.Object> references)
	{
		_objectReferences = references;
	}

	public static T Clone<T>(T graph) where T : Graph
	{
		return UnityEngine.Object.Instantiate(graph);
	}

	public string SerializeLocalBlackboard()
	{
		return JSONSerializer.Serialize(typeof(BlackboardSource), _localBlackboard, pretyJson: false, _objectReferences);
	}

	public bool DeserializeLocalBlackboard(string json)
	{
		try
		{
			_localBlackboard = JSONSerializer.Deserialize<BlackboardSource>(json, _objectReferences);
			if (_localBlackboard == null)
			{
				_localBlackboard = new BlackboardSource();
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError($"<b>(Deserialization Error:)</b> {ex.ToString()}", this);
			return false;
		}
	}

	public void CopySerialized(Graph target)
	{
		string serializedGraph = Serialize(pretyJson: false, target._objectReferences);
		target.Deserialize(serializedGraph, validate: true, _objectReferences);
	}

	public void Validate()
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			try
			{
				allNodes[i].OnValidate(this);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
		}
		List<Task> allTasksOfType = GetAllTasksOfType<Task>();
		for (int j = 0; j < allTasksOfType.Count; j++)
		{
			try
			{
				allTasksOfType[j].OnValidate(this);
			}
			catch (Exception ex2)
			{
				Debug.LogError(ex2.ToString());
			}
		}
		OnGraphValidate();
		if (Application.isPlaying && useLocalBlackboard)
		{
			localBlackboard.InitializePropertiesBinding(null, callSetter: false);
		}
	}

	protected virtual void OnGraphValidate()
	{
	}

	public static List<Node> CopyNodesToGraph(List<Node> nodes, Graph targetGraph)
	{
		if (targetGraph == null)
		{
			return null;
		}
		List<Node> list = new List<Node>();
		Dictionary<Connection, KeyValuePair<int, int>> dictionary = new Dictionary<Connection, KeyValuePair<int, int>>();
		foreach (Node node in nodes)
		{
			Node item = node.Duplicate(targetGraph);
			list.Add(item);
			if (node == node.graph.primeNode && targetGraph != node.graph)
			{
				targetGraph.primeNode = item;
			}
			foreach (Connection outConnection in node.outConnections)
			{
				dictionary[outConnection] = new KeyValuePair<int, int>(nodes.IndexOf(outConnection.sourceNode), nodes.IndexOf(outConnection.targetNode));
			}
		}
		foreach (KeyValuePair<Connection, KeyValuePair<int, int>> item2 in dictionary)
		{
			if (item2.Value.Value != -1)
			{
				Node newSource = list[item2.Value.Key];
				Node newTarget = list[item2.Value.Value];
				item2.Key.Duplicate(newSource, newTarget);
			}
		}
		return list;
	}

	public void UpdateReferences()
	{
		UpdateNodeBBFields();
		SendTaskOwnerDefaults();
	}

	private void UpdateNodeBBFields()
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			BBParameter.SetBBFields(allNodes[i], blackboard);
		}
	}

	public void SendTaskOwnerDefaults()
	{
		List<Task> allTasksOfType = GetAllTasksOfType<Task>();
		for (int i = 0; i < allTasksOfType.Count; i++)
		{
			allTasksOfType[i].SetOwnerSystem(this);
		}
	}

	public void UpdateNodeIDs(bool alsoReorderList)
	{
		int lastID = 0;
		if (primeNode != null)
		{
			lastID = primeNode.AssignIDToGraph(lastID);
		}
		List<Node> list = allNodes.OrderBy((Node n) => n.inConnections.Count != 0).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			lastID = list[i].AssignIDToGraph(lastID);
		}
		for (int j = 0; j < allNodes.Count; j++)
		{
			allNodes[j].ResetRecursion();
		}
		if (alsoReorderList)
		{
			allNodes = allNodes.OrderBy((Node node) => node.ID).ToList();
		}
	}

	public void StartGraph(Component agent, IBlackboard blackboard, bool autoUpdate, Action<bool> callback = null)
	{
		if (isRunning)
		{
			if (callback != null)
			{
				OnFinish += callback;
			}
			Debug.LogWarning("<b>Graph:</b> Graph is already Active.");
			return;
		}
		if (agent == null && requiresAgent)
		{
			Debug.LogWarning("<b>Graph:</b> You've tried to start a graph with null Agent.");
			return;
		}
		if (primeNode == null && requiresPrimeNode)
		{
			Debug.LogWarning("<b>Graph:</b> You've tried to start graph without 'Start' node");
			return;
		}
		if (blackboard == null)
		{
			if (agent != null)
			{
				Debug.Log(string.Concat("<b>Graph:</b> Graph started without blackboard. Looking for blackboard on agent '", agent.gameObject, "'..."), agent.gameObject);
				blackboard = agent.GetComponent(typeof(IBlackboard)) as IBlackboard;
				if (blackboard != null)
				{
					Debug.Log("<b>Graph:</b> Blackboard found");
				}
			}
			if (blackboard == null)
			{
				Debug.LogWarning("<b>Graph:</b> Started with null Blackboard. Using Local instead...");
				blackboard = localBlackboard;
			}
		}
		this.agent = agent;
		this.blackboard = blackboard;
		UpdateReferences();
		if (callback != null)
		{
			this.OnFinish = callback;
		}
		isRunning = true;
		runningGraphs.Add(this);
		if (autoUpdate)
		{
			MonoManager.current.onUpdate += UpdateGraph;
		}
		if (!isPaused)
		{
			timeStarted = UpdateManager.time;
			OnGraphStarted();
		}
		else
		{
			OnGraphUnpaused();
		}
		for (int i = 0; i < allNodes.Count; i++)
		{
			if (!isPaused)
			{
				allNodes[i].OnGraphStarted();
			}
			else
			{
				allNodes[i].OnGraphUnpaused();
			}
		}
		isPaused = false;
	}

	public void Stop(bool success = true)
	{
		if (isRunning || isPaused)
		{
			runningGraphs.Remove(this);
			MonoManager.current.onUpdate -= UpdateGraph;
			isRunning = false;
			isPaused = false;
			for (int i = 0; i < allNodes.Count; i++)
			{
				allNodes[i].Reset(recursively: false);
				allNodes[i].OnGraphStoped();
			}
			OnGraphStoped();
			if (this.OnFinish != null)
			{
				this.OnFinish(success);
				this.OnFinish = null;
			}
		}
	}

	public void Pause()
	{
		if (isRunning)
		{
			runningGraphs.Remove(this);
			MonoManager.current.onUpdate -= UpdateGraph;
			isRunning = false;
			isPaused = true;
			for (int i = 0; i < allNodes.Count; i++)
			{
				allNodes[i].OnGraphPaused();
			}
			OnGraphPaused();
		}
	}

	public void UpdateGraph()
	{
		OnGraphUpdate();
	}

	protected virtual void OnGraphStarted()
	{
	}

	protected virtual void OnGraphUpdate()
	{
	}

	protected virtual void OnGraphStoped()
	{
	}

	protected virtual void OnGraphPaused()
	{
	}

	protected virtual void OnGraphUnpaused()
	{
	}

	protected void UnRegisterFromMonoManagerUpdate()
	{
		MonoManager.current.onUpdate -= UpdateGraph;
	}

	public void SendEvent(string name)
	{
		SendEvent(new EventData(name));
	}

	public void SendEvent<T>(string name, T value)
	{
		SendEvent(new EventData<T>(name, value));
	}

	public void SendEvent(EventData eventData)
	{
		if (isRunning && eventData != null && agent != null)
		{
			MessageRouter component = agent.GetComponent<MessageRouter>();
			if (component != null)
			{
				component.Dispatch("OnCustomEvent", eventData);
				component.Dispatch(eventData.name, eventData.value);
			}
		}
	}

	public static void SendGlobalEvent(string name)
	{
		SendGlobalEvent(new EventData(name));
	}

	public static void SendGlobalEvent<T>(string name, T value)
	{
		SendGlobalEvent(new EventData<T>(name, value));
	}

	public static void SendGlobalEvent(EventData eventData)
	{
		List<GameObject> list = new List<GameObject>();
		Graph[] array = runningGraphs.ToArray();
		foreach (Graph graph in array)
		{
			if (graph.agent != null && !list.Contains(graph.agent.gameObject))
			{
				list.Add(graph.agent.gameObject);
				graph.SendEvent(eventData);
			}
		}
	}

	public Node GetNodeWithID(int searchID)
	{
		if (searchID <= allNodes.Count && searchID >= 0)
		{
			return allNodes.Find((Node n) => n.ID == searchID);
		}
		return null;
	}

	public List<T> GetAllNodesOfType<T>() where T : Node
	{
		return allNodes.OfType<T>().ToList();
	}

	public T GetNodeWithTag<T>(string name) where T : Node
	{
		foreach (T item in allNodes.OfType<T>())
		{
			T current = item;
			if (current.tag == name)
			{
				return current;
			}
		}
		return (T)null;
	}

	public List<T> GetNodesWithTag<T>(string name) where T : Node
	{
		List<T> list = new List<T>();
		foreach (T item in allNodes.OfType<T>())
		{
			T current = item;
			if (current.tag == name)
			{
				list.Add(current);
			}
		}
		return list;
	}

	public List<T> GetAllTagedNodes<T>() where T : Node
	{
		List<T> list = new List<T>();
		foreach (T item in allNodes.OfType<T>())
		{
			T current = item;
			if (!string.IsNullOrEmpty(current.tag))
			{
				list.Add(current);
			}
		}
		return list;
	}

	public T GetNodeWithName<T>(string name) where T : Node
	{
		foreach (T item in allNodes.OfType<T>())
		{
			T current = item;
			if (StripNameColor(current.name).ToLower() == name.ToLower())
			{
				return current;
			}
		}
		return (T)null;
	}

	private string StripNameColor(string name)
	{
		if (name.StartsWith("<") && name.EndsWith(">"))
		{
			name = name.Replace(name.Substring(0, name.IndexOf(">") + 1), string.Empty);
			name = name.Replace(name.Substring(name.IndexOf("<"), name.LastIndexOf(">") + 1 - name.IndexOf("<")), string.Empty);
		}
		return name;
	}

	public List<Node> GetRootNodes()
	{
		return allNodes.Where((Node n) => n.inConnections.Count == 0).ToList();
	}

	public List<T> GetAllNestedGraphs<T>(bool recursive) where T : Graph
	{
		List<T> list = new List<T>();
		foreach (IGraphAssignable item in allNodes.OfType<IGraphAssignable>())
		{
			if (item.nestedGraph is T)
			{
				if (!list.Contains((T)item.nestedGraph))
				{
					list.Add((T)item.nestedGraph);
				}
				if (recursive)
				{
					list.AddRange(item.nestedGraph.GetAllNestedGraphs<T>(recursive));
				}
			}
		}
		return list;
	}

	public List<Graph> GetAllInstancedNestedGraphs()
	{
		List<Graph> list = new List<Graph>();
		foreach (IGraphAssignable item in allNodes.OfType<IGraphAssignable>())
		{
			Graph[] instances = item.GetInstances();
			list.AddRange(instances);
			Graph[] array = instances;
			foreach (Graph graph in array)
			{
				list.AddRange(graph.GetAllInstancedNestedGraphs());
			}
		}
		return list;
	}

	public List<T> GetAllTasksOfType<T>() where T : Task
	{
		List<Task> list = new List<Task>();
		List<T> list2 = new List<T>();
		for (int i = 0; i < allNodes.Count; i++)
		{
			Node node = allNodes[i];
			if (node is ITaskAssignable && (node as ITaskAssignable).task != null)
			{
				list.Add((node as ITaskAssignable).task);
			}
			if (node is ISubTasksContainer)
			{
				list.AddRange((node as ISubTasksContainer).GetTasks());
			}
			for (int j = 0; j < node.outConnections.Count; j++)
			{
				Connection connection = node.outConnections[j];
				if (connection is ITaskAssignable && (connection as ITaskAssignable).task != null)
				{
					list.Add((connection as ITaskAssignable).task);
				}
				if (connection is ISubTasksContainer)
				{
					list.AddRange((connection as ISubTasksContainer).GetTasks());
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			Task task = list[k];
			if (task is ActionList)
			{
				list2.AddRange((task as ActionList).actions.OfType<T>());
			}
			if (task is ConditionList)
			{
				list2.AddRange((task as ConditionList).conditions.OfType<T>());
			}
			if (task is T)
			{
				list2.Add((T)task);
			}
		}
		return list2;
	}

	public BBParameter[] GetDefinedParameters()
	{
		List<BBParameter> list = new List<BBParameter>();
		List<object> list2 = new List<object>();
		list2.AddRange(GetAllTasksOfType<Task>().Cast<object>());
		list2.AddRange(allNodes.Cast<object>());
		for (int i = 0; i < list2.Count; i++)
		{
			object obj = list2[i];
			if (obj is Task)
			{
				Task task = (Task)obj;
				if (task.agentIsOverride && !string.IsNullOrEmpty(task.overrideAgentParameterName))
				{
					list.Add(typeof(Task).RTGetField("overrideAgent").GetValue(task) as BBParameter);
				}
			}
			foreach (BBParameter objectBBParameter in BBParameter.GetObjectBBParameters(obj))
			{
				if (objectBBParameter != null && objectBBParameter.useBlackboard && !objectBBParameter.isNone)
				{
					list.Add(objectBBParameter);
				}
			}
		}
		return list.ToArray();
	}

	public void CreateDefinedParameterVariables(IBlackboard bb)
	{
		BBParameter[] definedParameters = GetDefinedParameters();
		foreach (BBParameter bBParameter in definedParameters)
		{
			bBParameter.PromoteToVariable(bb);
		}
	}

	public T AddNode<T>() where T : Node
	{
		return (T)AddNode(typeof(T));
	}

	public T AddNode<T>(Vector2 pos) where T : Node
	{
		return (T)AddNode(typeof(T), pos);
	}

	public Node AddNode(Type nodeType)
	{
		return AddNode(nodeType, new Vector2(50f, 50f));
	}

	public Node AddNode(Type nodeType, Vector2 pos)
	{
		if (!nodeType.RTIsSubclassOf(baseNodeType))
		{
			Debug.LogWarning(string.Concat(nodeType, " can't be added to ", GetType().FriendlyName(), " graph"));
			return null;
		}
		Node node = Node.Create(this, nodeType, pos);
		RecordUndo("New Node");
		allNodes.Add(node);
		if (primeNode == null)
		{
			primeNode = node;
		}
		UpdateNodeIDs(alsoReorderList: false);
		return node;
	}

	public void RemoveNode(Node node, bool recordUndo = true)
	{
		if (!allNodes.Contains(node))
		{
			Debug.LogWarning("Node is not part of this graph");
			return;
		}
		node.OnDestroy();
		Connection[] array = node.inConnections.ToArray();
		foreach (Connection connection in array)
		{
			RemoveConnection(connection);
		}
		Connection[] array2 = node.outConnections.ToArray();
		foreach (Connection connection2 in array2)
		{
			RemoveConnection(connection2);
		}
		if (recordUndo)
		{
			RecordUndo("Delete Node");
		}
		allNodes.Remove(node);
		if (node == primeNode)
		{
			primeNode = GetNodeWithID(2);
		}
		UpdateNodeIDs(alsoReorderList: false);
	}

	public Connection ConnectNodes(Node sourceNode, Node targetNode)
	{
		return ConnectNodes(sourceNode, targetNode, sourceNode.outConnections.Count);
	}

	public Connection ConnectNodes(Node sourceNode, Node targetNode, int indexToInsert)
	{
		if (!targetNode.IsNewConnectionAllowed(sourceNode))
		{
			return null;
		}
		RecordUndo("New Connection");
		Connection connection = Connection.Create(sourceNode, targetNode, indexToInsert);
		sourceNode.OnChildConnected(indexToInsert);
		targetNode.OnParentConnected(targetNode.inConnections.IndexOf(connection));
		UpdateNodeIDs(alsoReorderList: false);
		return connection;
	}

	public void RemoveConnection(Connection connection, bool recordUndo = true)
	{
		if (Application.isPlaying)
		{
			connection.Reset();
		}
		if (recordUndo)
		{
			RecordUndo("Delete Connection");
		}
		connection.OnDestroy();
		connection.sourceNode.OnChildDisconnected(connection.sourceNode.outConnections.IndexOf(connection));
		connection.targetNode.OnParentDisconnected(connection.targetNode.inConnections.IndexOf(connection));
		connection.sourceNode.outConnections.Remove(connection);
		connection.targetNode.inConnections.Remove(connection);
		UpdateNodeIDs(alsoReorderList: false);
	}

	private void RecordUndo(string name)
	{
	}
}
