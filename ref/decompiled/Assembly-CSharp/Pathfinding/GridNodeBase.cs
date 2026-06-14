using System;
using System.Collections.Generic;
using Pathfinding.Serialization;

namespace Pathfinding;

public abstract class GridNodeBase : GraphNode
{
	private const int GridFlagsWalkableErosionOffset = 8;

	private const int GridFlagsWalkableErosionMask = 256;

	private const int GridFlagsWalkableTmpOffset = 9;

	private const int GridFlagsWalkableTmpMask = 512;

	protected int nodeInGridIndex;

	protected ushort gridFlags;

	public GraphNode[] connections;

	public uint[] connectionCosts;

	private static readonly Version VERSION_3_8_3 = new Version(3, 8, 3);

	public int NodeInGridIndex
	{
		get
		{
			return nodeInGridIndex;
		}
		set
		{
			nodeInGridIndex = value;
		}
	}

	public bool WalkableErosion
	{
		get
		{
			return (gridFlags & 0x100) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFEFFu) | (value ? 256u : 0u));
		}
	}

	public bool TmpWalkable
	{
		get
		{
			return (gridFlags & 0x200) != 0;
		}
		set
		{
			gridFlags = (ushort)((gridFlags & 0xFFFFFDFFu) | (value ? 512u : 0u));
		}
	}

	protected GridNodeBase(AstarPath astar)
		: base(astar)
	{
	}

	public override void FloodFill(Stack<GraphNode> stack, uint region)
	{
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode graphNode = connections[i];
			if (graphNode.Area != region)
			{
				graphNode.Area = region;
				stack.Push(graphNode);
			}
		}
	}

	public override void ClearConnections(bool alsoReverse)
	{
		if (alsoReverse && connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				connections[i].RemoveConnection(this);
			}
		}
		connections = null;
		connectionCosts = null;
	}

	public override bool ContainsConnection(GraphNode node)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i] == node)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void GetConnections(GraphNodeDelegate del)
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				del(connections[i]);
			}
		}
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		ushort pathID = handler.PathID;
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode graphNode = connections[i];
			PathNode pathNode2 = handler.GetPathNode(graphNode);
			if (pathNode2.parent == pathNode && pathNode2.pathID == pathID)
			{
				graphNode.UpdateRecursiveG(path, pathNode2, handler);
			}
		}
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		ushort pathID = handler.PathID;
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode graphNode = connections[i];
			if (path.CanTraverse(graphNode))
			{
				PathNode pathNode2 = handler.GetPathNode(graphNode);
				uint num = connectionCosts[i];
				if (pathNode2.pathID != pathID)
				{
					pathNode2.parent = pathNode;
					pathNode2.pathID = pathID;
					pathNode2.cost = num;
					pathNode2.H = path.CalculateHScore(graphNode);
					graphNode.UpdateG(path, pathNode2);
					handler.PushNode(pathNode2);
				}
				else if (pathNode.G + num + path.GetTraversalCost(graphNode) < pathNode2.G)
				{
					pathNode2.cost = num;
					pathNode2.parent = pathNode;
					graphNode.UpdateRecursiveG(path, pathNode2, handler);
				}
				else if (pathNode2.G + num + path.GetTraversalCost(this) < pathNode.G && graphNode.ContainsConnection(this))
				{
					pathNode.parent = pathNode2;
					pathNode.cost = num;
					UpdateRecursiveG(path, pathNode, handler);
				}
			}
		}
	}

	public override void AddConnection(GraphNode node, uint cost)
	{
		if (node == null)
		{
			throw new ArgumentNullException();
		}
		if (connections != null)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i] == node)
				{
					connectionCosts[i] = cost;
					return;
				}
			}
		}
		int num = ((connections != null) ? connections.Length : 0);
		GraphNode[] array = new GraphNode[num + 1];
		uint[] array2 = new uint[num + 1];
		for (int j = 0; j < num; j++)
		{
			array[j] = connections[j];
			array2[j] = connectionCosts[j];
		}
		array[num] = node;
		array2[num] = cost;
		connections = array;
		connectionCosts = array2;
	}

	public override void RemoveConnection(GraphNode node)
	{
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i] == node)
			{
				int num = connections.Length;
				GraphNode[] array = new GraphNode[num - 1];
				uint[] array2 = new uint[num - 1];
				for (int j = 0; j < i; j++)
				{
					array[j] = connections[j];
					array2[j] = connectionCosts[j];
				}
				for (int k = i + 1; k < num; k++)
				{
					array[k - 1] = connections[k];
					array2[k - 1] = connectionCosts[k];
				}
				connections = array;
				connectionCosts = array2;
				break;
			}
		}
	}

	public override void SerializeReferences(GraphSerializationContext ctx)
	{
		if (connections == null)
		{
			ctx.writer.Write(-1);
			return;
		}
		ctx.writer.Write(connections.Length);
		for (int i = 0; i < connections.Length; i++)
		{
			ctx.SerializeNodeReference(connections[i]);
			ctx.writer.Write(connectionCosts[i]);
		}
	}

	public override void DeserializeReferences(GraphSerializationContext ctx)
	{
		if (ctx.meta.version < VERSION_3_8_3)
		{
			return;
		}
		int num = ctx.reader.ReadInt32();
		if (num == -1)
		{
			connections = null;
			connectionCosts = null;
			return;
		}
		connections = new GraphNode[num];
		connectionCosts = new uint[num];
		for (int i = 0; i < num; i++)
		{
			connections[i] = ctx.DeserializeNodeReference();
			connectionCosts[i] = ctx.reader.ReadUInt32();
		}
	}
}
