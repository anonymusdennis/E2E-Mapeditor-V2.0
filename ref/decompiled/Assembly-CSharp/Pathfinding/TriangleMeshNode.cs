using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding;

public class TriangleMeshNode : MeshNode
{
	public int v0;

	public int v1;

	public int v2;

	protected static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];

	public TriangleMeshNode(AstarPath astar)
		: base(astar)
	{
	}

	public static INavmeshHolder GetNavmeshHolder(uint graphIndex)
	{
		return _navmeshHolders[graphIndex];
	}

	public static void SetNavmeshHolder(int graphIndex, INavmeshHolder graph)
	{
		if (_navmeshHolders.Length <= graphIndex)
		{
			INavmeshHolder[] array = new INavmeshHolder[graphIndex + 1];
			for (int i = 0; i < _navmeshHolders.Length; i++)
			{
				array[i] = _navmeshHolders[i];
			}
			_navmeshHolders = array;
		}
		_navmeshHolders[graphIndex] = graph;
	}

	public void UpdatePositionFromVertices()
	{
		INavmeshHolder navmeshHolder = GetNavmeshHolder(base.GraphIndex);
		position = (navmeshHolder.GetVertex(v0) + navmeshHolder.GetVertex(v1) + navmeshHolder.GetVertex(v2)) * 0.333333f;
	}

	public int GetVertexIndex(int i)
	{
		return i switch
		{
			0 => v0, 
			1 => v1, 
			_ => v2, 
		};
	}

	public int GetVertexArrayIndex(int i)
	{
		return GetNavmeshHolder(base.GraphIndex).GetVertexArrayIndex(i switch
		{
			0 => v0, 
			1 => v1, 
			_ => v2, 
		});
	}

	public override Int3 GetVertex(int i)
	{
		return GetNavmeshHolder(base.GraphIndex).GetVertex(GetVertexIndex(i));
	}

	public override int GetVertexCount()
	{
		return 3;
	}

	public override Vector3 ClosestPointOnNode(Vector3 p)
	{
		INavmeshHolder navmeshHolder = GetNavmeshHolder(base.GraphIndex);
		return Polygon.ClosestPointOnTriangle((Vector3)navmeshHolder.GetVertex(v0), (Vector3)navmeshHolder.GetVertex(v1), (Vector3)navmeshHolder.GetVertex(v2), p);
	}

	public override Vector3 ClosestPointOnNodeXZ(Vector3 p)
	{
		INavmeshHolder navmeshHolder = GetNavmeshHolder(base.GraphIndex);
		Int3 vertex = navmeshHolder.GetVertex(v0);
		Int3 vertex2 = navmeshHolder.GetVertex(v1);
		Int3 vertex3 = navmeshHolder.GetVertex(v2);
		Vector2 vector = Polygon.ClosestPointOnTriangle(new Vector2((float)vertex.x * 0.001f, (float)vertex.z * 0.001f), new Vector2((float)vertex2.x * 0.001f, (float)vertex2.z * 0.001f), new Vector2((float)vertex3.x * 0.001f, (float)vertex3.z * 0.001f), new Vector2(p.x, p.z));
		return new Vector3(vector.x, p.y, vector.y);
	}

	public override bool ContainsPoint(Int3 p)
	{
		INavmeshHolder navmeshHolder = GetNavmeshHolder(base.GraphIndex);
		Int3 vertex = navmeshHolder.GetVertex(v0);
		Int3 vertex2 = navmeshHolder.GetVertex(v1);
		Int3 vertex3 = navmeshHolder.GetVertex(v2);
		if ((long)(vertex2.x - vertex.x) * (long)(p.z - vertex.z) - (long)(p.x - vertex.x) * (long)(vertex2.z - vertex.z) > 0)
		{
			return false;
		}
		if ((long)(vertex3.x - vertex2.x) * (long)(p.z - vertex2.z) - (long)(p.x - vertex2.x) * (long)(vertex3.z - vertex2.z) > 0)
		{
			return false;
		}
		if ((long)(vertex.x - vertex3.x) * (long)(p.z - vertex3.z) - (long)(p.x - vertex3.x) * (long)(vertex.z - vertex3.z) > 0)
		{
			return false;
		}
		return true;
	}

	public override void UpdateRecursiveG(Path path, PathNode pathNode, PathHandler handler)
	{
		UpdateG(path, pathNode);
		handler.PushNode(pathNode);
		if (connections == null)
		{
			return;
		}
		for (int i = 0; i < connections.Length; i++)
		{
			GraphNode graphNode = connections[i];
			PathNode pathNode2 = handler.GetPathNode(graphNode);
			if (pathNode2.parent == pathNode && pathNode2.pathID == handler.PathID)
			{
				graphNode.UpdateRecursiveG(path, pathNode2, handler);
			}
		}
	}

	public override void Open(Path path, PathNode pathNode, PathHandler handler)
	{
		if (connections == null)
		{
			return;
		}
		bool flag = pathNode.flag2;
		for (int num = connections.Length - 1; num >= 0; num--)
		{
			GraphNode graphNode = connections[num];
			if (path.CanTraverse(graphNode))
			{
				PathNode pathNode2 = handler.GetPathNode(graphNode);
				if (pathNode2 != pathNode.parent)
				{
					uint num2 = connectionCosts[num];
					if (flag || pathNode2.flag2)
					{
						num2 = path.GetConnectionSpecialCost(this, graphNode, num2);
					}
					if (pathNode2.pathID != handler.PathID)
					{
						pathNode2.node = graphNode;
						pathNode2.parent = pathNode;
						pathNode2.pathID = handler.PathID;
						pathNode2.cost = num2;
						pathNode2.H = path.CalculateHScore(graphNode);
						graphNode.UpdateG(path, pathNode2);
						handler.PushNode(pathNode2);
					}
					else if (pathNode.G + num2 + path.GetTraversalCost(graphNode) < pathNode2.G)
					{
						pathNode2.cost = num2;
						pathNode2.parent = pathNode;
						graphNode.UpdateRecursiveG(path, pathNode2, handler);
					}
					else if (pathNode2.G + num2 + path.GetTraversalCost(this) < pathNode.G && graphNode.ContainsConnection(this))
					{
						pathNode.parent = pathNode2;
						pathNode.cost = num2;
						UpdateRecursiveG(path, pathNode, handler);
					}
				}
			}
		}
	}

	public int SharedEdge(GraphNode other)
	{
		GetPortal(other, null, null, backwards: false, out var aIndex, out var _);
		return aIndex;
	}

	public override bool GetPortal(GraphNode _other, List<Vector3> left, List<Vector3> right, bool backwards)
	{
		int aIndex;
		int bIndex;
		return GetPortal(_other, left, right, backwards, out aIndex, out bIndex);
	}

	public bool GetPortal(GraphNode _other, List<Vector3> left, List<Vector3> right, bool backwards, out int aIndex, out int bIndex)
	{
		aIndex = -1;
		bIndex = -1;
		if (_other.GraphIndex != base.GraphIndex)
		{
			return false;
		}
		TriangleMeshNode triangleMeshNode = _other as TriangleMeshNode;
		int num = (GetVertexIndex(0) >> 12) & 0x7FFFF;
		int num2 = (triangleMeshNode.GetVertexIndex(0) >> 12) & 0x7FFFF;
		if (num != num2 && GetNavmeshHolder(base.GraphIndex) is RecastGraph)
		{
			for (int i = 0; i < connections.Length; i++)
			{
				if (connections[i].GraphIndex != base.GraphIndex && connections[i] is NodeLink3Node nodeLink3Node && nodeLink3Node.GetOther(this) == triangleMeshNode && left != null)
				{
					nodeLink3Node.GetPortal(triangleMeshNode, left, right, backwards: false);
					return true;
				}
			}
			INavmeshHolder navmeshHolder = GetNavmeshHolder(base.GraphIndex);
			navmeshHolder.GetTileCoordinates(num, out var x, out var z);
			navmeshHolder.GetTileCoordinates(num2, out var x2, out var z2);
			int num3;
			if (Math.Abs(x - x2) == 1)
			{
				num3 = 0;
			}
			else
			{
				if (Math.Abs(z - z2) != 1)
				{
					throw new Exception("Tiles not adjacent (" + x + ", " + z + ") (" + x2 + ", " + z2 + ")");
				}
				num3 = 2;
			}
			int vertexCount = GetVertexCount();
			int vertexCount2 = triangleMeshNode.GetVertexCount();
			int num4 = -1;
			int num5 = -1;
			for (int j = 0; j < vertexCount; j++)
			{
				int num6 = GetVertex(j)[num3];
				for (int k = 0; k < vertexCount2; k++)
				{
					if (num6 == triangleMeshNode.GetVertex((k + 1) % vertexCount2)[num3] && GetVertex((j + 1) % vertexCount)[num3] == triangleMeshNode.GetVertex(k)[num3])
					{
						num4 = j;
						num5 = k;
						j = vertexCount;
						break;
					}
				}
			}
			aIndex = num4;
			bIndex = num5;
			if (num4 != -1)
			{
				Int3 vertex = GetVertex(num4);
				Int3 vertex2 = GetVertex((num4 + 1) % vertexCount);
				int i2 = ((num3 != 2) ? 2 : 0);
				int val = Math.Min(vertex[i2], vertex2[i2]);
				int val2 = Math.Max(vertex[i2], vertex2[i2]);
				val = Math.Max(val, Math.Min(triangleMeshNode.GetVertex(num5)[i2], triangleMeshNode.GetVertex((num5 + 1) % vertexCount2)[i2]));
				val2 = Math.Min(val2, Math.Max(triangleMeshNode.GetVertex(num5)[i2], triangleMeshNode.GetVertex((num5 + 1) % vertexCount2)[i2]));
				if (vertex[i2] < vertex2[i2])
				{
					vertex[i2] = val;
					vertex2[i2] = val2;
				}
				else
				{
					vertex[i2] = val2;
					vertex2[i2] = val;
				}
				if (left != null)
				{
					left.Add((Vector3)vertex);
					right.Add((Vector3)vertex2);
				}
				return true;
			}
		}
		else if (!backwards)
		{
			int num7 = -1;
			int num8 = -1;
			int vertexCount3 = GetVertexCount();
			int vertexCount4 = triangleMeshNode.GetVertexCount();
			for (int l = 0; l < vertexCount3; l++)
			{
				int vertexIndex = GetVertexIndex(l);
				for (int m = 0; m < vertexCount4; m++)
				{
					if (vertexIndex == triangleMeshNode.GetVertexIndex((m + 1) % vertexCount4) && GetVertexIndex((l + 1) % vertexCount3) == triangleMeshNode.GetVertexIndex(m))
					{
						num7 = l;
						num8 = m;
						l = vertexCount3;
						break;
					}
				}
			}
			aIndex = num7;
			bIndex = num8;
			if (num7 == -1)
			{
				for (int n = 0; n < connections.Length; n++)
				{
					if (connections[n].GraphIndex != base.GraphIndex && connections[n] is NodeLink3Node nodeLink3Node2 && nodeLink3Node2.GetOther(this) == triangleMeshNode && left != null)
					{
						nodeLink3Node2.GetPortal(triangleMeshNode, left, right, backwards: false);
						return true;
					}
				}
				return false;
			}
			if (left != null)
			{
				left.Add((Vector3)GetVertex(num7));
				right.Add((Vector3)GetVertex((num7 + 1) % vertexCount3));
			}
		}
		return true;
	}

	public override void SerializeNode(GraphSerializationContext ctx)
	{
		base.SerializeNode(ctx);
		ctx.writer.Write(v0);
		ctx.writer.Write(v1);
		ctx.writer.Write(v2);
	}

	public override void DeserializeNode(GraphSerializationContext ctx)
	{
		base.DeserializeNode(ctx);
		v0 = ctx.reader.ReadInt32();
		v1 = ctx.reader.ReadInt32();
		v2 = ctx.reader.ReadInt32();
	}
}
