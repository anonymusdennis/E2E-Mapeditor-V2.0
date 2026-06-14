using System;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace NodeCanvas.Framework;

[SpoofAOT]
public abstract class Connection
{
	[SerializeField]
	private Node _sourceNode;

	[SerializeField]
	private Node _targetNode;

	[SerializeField]
	private bool _isDisabled;

	[NonSerialized]
	private Status _status = Status.Resting;

	public Node sourceNode
	{
		get
		{
			return _sourceNode;
		}
		protected set
		{
			_sourceNode = value;
		}
	}

	public Node targetNode
	{
		get
		{
			return _targetNode;
		}
		protected set
		{
			_targetNode = value;
		}
	}

	public bool isActive
	{
		get
		{
			return !_isDisabled;
		}
		set
		{
			if (!_isDisabled && !value)
			{
				Reset();
			}
			_isDisabled = !value;
		}
	}

	public Status status
	{
		get
		{
			return _status;
		}
		set
		{
			_status = value;
		}
	}

	protected Graph graph => sourceNode.graph;

	public Connection()
	{
	}

	public static Connection Create(Node source, Node target, int sourceIndex)
	{
		if (source == null || target == null)
		{
			Debug.LogError("Can't Create a Connection without providing Source and Target Nodes");
			return null;
		}
		if (source is MissingNode)
		{
			Debug.LogError("Creating new Connections from a 'MissingNode' is not allowed. Please resolve the MissingNode node first");
			return null;
		}
		Connection connection = (Connection)Activator.CreateInstance(source.outConnectionType);
		connection.sourceNode = source;
		connection.targetNode = target;
		source.outConnections.Insert(sourceIndex, connection);
		target.inConnections.Add(connection);
		connection.OnValidate(sourceIndex, target.inConnections.IndexOf(connection));
		return connection;
	}

	public Connection Duplicate(Node newSource, Node newTarget)
	{
		if (newSource == null || newTarget == null)
		{
			Debug.LogError("Can't Duplicate a Connection without providing NewSource and NewTarget Nodes");
			return null;
		}
		Connection connection = JSONSerializer.Deserialize<Connection>(JSONSerializer.Serialize(typeof(Connection), this));
		connection.SetSource(newSource, isRelink: false);
		connection.SetTarget(newTarget, isRelink: false);
		if (this is ITaskAssignable { task: not null } taskAssignable)
		{
			(connection as ITaskAssignable).task = taskAssignable.task.Duplicate(newSource.graph);
		}
		connection.OnValidate(newSource.outConnections.IndexOf(connection), newTarget.inConnections.IndexOf(connection));
		return connection;
	}

	public virtual void OnValidate(int sourceIndex, int targetIndex)
	{
	}

	public virtual void OnDestroy()
	{
	}

	public void SetSource(Node newSource, bool isRelink = true)
	{
		if (isRelink)
		{
			int connectionIndex = sourceNode.outConnections.IndexOf(this);
			sourceNode.OnChildDisconnected(connectionIndex);
			newSource.OnChildConnected(connectionIndex);
			sourceNode.outConnections.Remove(this);
		}
		newSource.outConnections.Add(this);
		sourceNode = newSource;
	}

	public void SetTarget(Node newTarget, bool isRelink = true)
	{
		if (isRelink)
		{
			int connectionIndex = targetNode.inConnections.IndexOf(this);
			targetNode.OnParentDisconnected(connectionIndex);
			newTarget.OnParentConnected(connectionIndex);
			targetNode.inConnections.Remove(this);
		}
		newTarget.inConnections.Add(this);
		targetNode = newTarget;
	}

	public Status Execute(Component agent, IBlackboard blackboard)
	{
		if (!isActive)
		{
			return Status.Resting;
		}
		status = targetNode.Execute(agent, blackboard);
		return status;
	}

	public void PreInitialise()
	{
		targetNode.PreInitialise();
	}

	public void Reset(bool recursively = true)
	{
		if (status != Status.Resting)
		{
			status = Status.Resting;
			if (recursively)
			{
				targetNode.Reset(recursively);
			}
		}
	}

	public void ForceReset(bool recursively = true)
	{
		status = Status.Resting;
		if (recursively)
		{
			targetNode.ForceReset(recursively);
		}
	}
}
