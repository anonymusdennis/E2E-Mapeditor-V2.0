using System;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using UnityEngine;

namespace NodeCanvas.Framework;

public abstract class GraphOwner : MonoBehaviour
{
	public enum EnableAction
	{
		EnableBehaviour,
		DoNothing
	}

	public enum DisableAction
	{
		DisableBehaviour,
		PauseBehaviour,
		DoNothing
	}

	[SerializeField]
	private string boundGraphSerialization;

	[SerializeField]
	private List<UnityEngine.Object> boundGraphObjectReferences;

	[SerializeField]
	public UpdateCategory m_UpdateCategory = UpdateCategory.TimeSlicedNodeCanvas;

	[HideInInspector]
	public EnableAction enableAction;

	[HideInInspector]
	public DisableAction disableAction;

	private Dictionary<Graph, Graph> instances = new Dictionary<Graph, Graph>();

	private bool startCalled;

	private static bool isQuiting;

	public abstract Graph graph { get; set; }

	public abstract IBlackboard blackboard { get; set; }

	public abstract Type graphType { get; }

	public bool isRunning => graph != null && graph.isRunning;

	public bool isPaused => graph != null && graph.isPaused;

	public float elapsedTime => (!(graph != null)) ? 0f : graph.elapsedTime;

	protected Graph GetInstance(Graph originalGraph)
	{
		if (originalGraph == null)
		{
			return null;
		}
		if (instances.Values.Contains(originalGraph))
		{
			return originalGraph;
		}
		Graph value = null;
		if (!instances.TryGetValue(originalGraph, out value))
		{
			value = Graph.Clone(originalGraph);
			instances[originalGraph] = value;
		}
		value.agent = this;
		value.blackboard = blackboard;
		return value;
	}

	public void StartBehaviour()
	{
		graph = GetInstance(graph);
		if (graph != null)
		{
			graph.StartGraph(this, blackboard, autoUpdate: true);
		}
	}

	public void StartBehaviour(Action<bool> callback)
	{
		graph = GetInstance(graph);
		if (graph != null)
		{
			graph.StartGraph(this, blackboard, autoUpdate: true, callback);
		}
	}

	public void PauseBehaviour()
	{
		if (graph != null)
		{
			graph.Pause();
		}
	}

	public void StopBehaviour()
	{
		if (graph != null)
		{
			graph.Stop();
		}
	}

	public void UpdateBehaviour()
	{
		if (graph != null)
		{
			graph.UpdateGraph();
		}
	}

	public void SendEvent(string eventName)
	{
		SendEvent(new EventData(eventName));
	}

	public void SendEvent<T>(string eventName, T eventValue)
	{
		SendEvent(new EventData<T>(eventName, eventValue));
	}

	public void SendEvent(EventData eventData)
	{
		if (graph != null)
		{
			graph.SendEvent(eventData);
		}
	}

	public static void SendGlobalEvent(string eventName)
	{
		Graph.SendGlobalEvent(new EventData(eventName));
	}

	public static void SendGlobalEvent<T>(string eventName, T eventValue)
	{
		Graph.SendGlobalEvent(new EventData<T>(eventName, eventValue));
	}

	protected void OnApplicationQuit()
	{
		isQuiting = true;
	}

	protected void Awake()
	{
		if (graph == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(boundGraphSerialization))
		{
			if (graph.hideFlags == HideFlags.HideInInspector)
			{
				instances[graph] = graph;
				return;
			}
			graph.SetSerializationObjectReferences(boundGraphObjectReferences);
			graph = GetInstance(graph);
		}
		else
		{
			graph = GetInstance(graph);
		}
	}

	protected void Start()
	{
		startCalled = true;
		if (enableAction == EnableAction.EnableBehaviour)
		{
			StartBehaviour();
		}
	}

	protected void OnEnable()
	{
		if (startCalled && enableAction == EnableAction.EnableBehaviour)
		{
			StartBehaviour();
		}
	}

	protected void OnDisable()
	{
		if (!isQuiting)
		{
			if (disableAction == DisableAction.DisableBehaviour)
			{
				StopBehaviour();
			}
			if (disableAction == DisableAction.PauseBehaviour)
			{
				PauseBehaviour();
			}
		}
	}

	protected void OnDestroy()
	{
		if (isQuiting)
		{
			return;
		}
		StopBehaviour();
		foreach (Graph value in instances.Values)
		{
			foreach (Graph allInstancedNestedGraph in value.GetAllInstancedNestedGraphs())
			{
				UnityEngine.Object.Destroy(allInstancedNestedGraph);
			}
			UnityEngine.Object.Destroy(value);
		}
	}
}
public abstract class GraphOwner<T> : GraphOwner where T : Graph
{
	[SerializeField]
	private T _graph;

	[SerializeField]
	private Blackboard _blackboard;

	public sealed override Graph graph
	{
		get
		{
			return _graph;
		}
		set
		{
			_graph = (T)value;
		}
	}

	public T behaviour
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

	public sealed override IBlackboard blackboard
	{
		get
		{
			if (graph != null && graph.useLocalBlackboard)
			{
				return graph.localBlackboard;
			}
			if (_blackboard == null)
			{
				_blackboard = GetComponent<Blackboard>();
			}
			return _blackboard;
		}
		set
		{
			if (!object.ReferenceEquals(_blackboard, value))
			{
				_blackboard = (Blackboard)value;
				if (graph != null && !graph.useLocalBlackboard)
				{
					graph.blackboard = value;
				}
			}
		}
	}

	public sealed override Type graphType => typeof(T);

	public void StartBehaviour(T newGraph)
	{
		SwitchBehaviour(newGraph);
	}

	public void StartBehaviour(T newGraph, Action<bool> callback)
	{
		SwitchBehaviour(newGraph, callback);
	}

	public void SwitchBehaviour(T newGraph)
	{
		SwitchBehaviour(newGraph, null);
	}

	public void SwitchBehaviour(T newGraph, Action<bool> callback)
	{
		StopBehaviour();
		graph = newGraph;
		StartBehaviour(callback);
	}
}
