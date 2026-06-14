using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;

namespace NodeCanvas.BehaviourTrees;

public abstract class BTNode : Node
{
	public sealed override Type outConnectionType => typeof(BTConnection);

	public sealed override int maxInConnections => 1;

	public override int maxOutConnections => 0;

	public sealed override bool allowAsPrime => true;

	public override bool showCommentsBottom => true;

	public T AddChild<T>(int childIndex) where T : BTNode
	{
		if (base.outConnections.Count >= maxOutConnections && maxOutConnections != -1)
		{
			return (T)null;
		}
		T val = base.graph.AddNode<T>();
		base.graph.ConnectNodes(this, val, childIndex);
		return val;
	}

	public T AddChild<T>() where T : BTNode
	{
		if (base.outConnections.Count >= maxOutConnections && maxOutConnections != -1)
		{
			return (T)null;
		}
		T val = base.graph.AddNode<T>();
		base.graph.ConnectNodes(this, val);
		return val;
	}

	public List<BTNode> GetAllChildNodesRecursively(bool includeThis)
	{
		List<BTNode> list = new List<BTNode>();
		if (includeThis)
		{
			list.Add(this);
		}
		foreach (BTNode item in base.outConnections.Select((Connection c) => c.targetNode))
		{
			list.AddRange(item.GetAllChildNodesRecursively(includeThis: true));
		}
		return list;
	}

	public Dictionary<BTNode, int> GetAllChildNodesWithDepthRecursively(bool includeThis, int startIndex)
	{
		Dictionary<BTNode, int> dictionary = new Dictionary<BTNode, int>();
		if (includeThis)
		{
			dictionary[this] = startIndex;
		}
		foreach (BTNode item in base.outConnections.Select((Connection c) => c.targetNode))
		{
			foreach (KeyValuePair<BTNode, int> item2 in item.GetAllChildNodesWithDepthRecursively(includeThis: true, startIndex + 1))
			{
				dictionary[item2.Key] = item2.Value;
			}
		}
		return dictionary;
	}
}
