using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using UnityEngine;

namespace Pathfinding;

[AddComponentMenu("Pathfinding/Link2")]
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_node_link2.php")]
public class NodeLink2 : GraphModifier
{
	protected static Dictionary<GraphNode, NodeLink2> reference = new Dictionary<GraphNode, NodeLink2>();

	public Transform end;

	public float costFactor = 1f;

	public bool oneWay;

	private GraphNode connectedNode1;

	private GraphNode connectedNode2;

	private Vector3 clamped1;

	private Vector3 clamped2;

	private bool postScanCalled;

	private static readonly Color GizmosColor = new Color(0.80784315f, 8f / 15f, 16f / 85f, 0.5f);

	private static readonly Color GizmosColorSelected = new Color(47f / 51f, 41f / 85f, 0.1254902f, 1f);

	public Transform StartTransform => base.transform;

	public Transform EndTransform => end;

	public PointNode startNode { get; private set; }

	public PointNode endNode { get; private set; }

	[Obsolete("Use startNode instead (lowercase s)")]
	public GraphNode StartNode => startNode;

	[Obsolete("Use endNode instead (lowercase e)")]
	public GraphNode EndNode => endNode;

	public static NodeLink2 GetNodeLink(GraphNode node)
	{
		reference.TryGetValue(node, out var value);
		return value;
	}

	public override void OnPostScan()
	{
		InternalOnPostScan();
	}

	public void InternalOnPostScan()
	{
		if (!(EndTransform == null) && !(StartTransform == null))
		{
			if (AstarPath.active.astarData.pointGraph == null)
			{
				PointGraph pointGraph = AstarPath.active.astarData.AddGraph(typeof(PointGraph)) as PointGraph;
				pointGraph.name = "PointGraph (used for node links)";
			}
			if (startNode != null)
			{
				reference.Remove(startNode);
			}
			if (endNode != null)
			{
				reference.Remove(endNode);
			}
			startNode = AstarPath.active.astarData.pointGraph.AddNode((Int3)StartTransform.position);
			endNode = AstarPath.active.astarData.pointGraph.AddNode((Int3)EndTransform.position);
			connectedNode1 = null;
			connectedNode2 = null;
			if (startNode == null || endNode == null)
			{
				startNode = null;
				endNode = null;
				return;
			}
			postScanCalled = true;
			reference[startNode] = this;
			reference[endNode] = this;
			Apply(forceNewCheck: true);
		}
	}

	public override void OnGraphsPostUpdate()
	{
		if (!AstarPath.active.isScanning)
		{
			if (connectedNode1 != null && connectedNode1.Destroyed)
			{
				connectedNode1 = null;
			}
			if (connectedNode2 != null && connectedNode2.Destroyed)
			{
				connectedNode2 = null;
			}
			if (!postScanCalled)
			{
				OnPostScan();
			}
			else
			{
				Apply(forceNewCheck: false);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying && AstarPath.active != null && AstarPath.active.astarData != null && AstarPath.active.astarData.pointGraph != null && !AstarPath.active.isScanning)
		{
			AstarPath.RegisterSafeUpdate(OnGraphsPostUpdate);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		postScanCalled = false;
		if (startNode != null)
		{
			reference.Remove(startNode);
		}
		if (endNode != null)
		{
			reference.Remove(endNode);
		}
		if (startNode != null && endNode != null)
		{
			startNode.RemoveConnection(endNode);
			endNode.RemoveConnection(startNode);
			if (connectedNode1 != null && connectedNode2 != null)
			{
				startNode.RemoveConnection(connectedNode1);
				connectedNode1.RemoveConnection(startNode);
				endNode.RemoveConnection(connectedNode2);
				connectedNode2.RemoveConnection(endNode);
			}
		}
	}

	private void RemoveConnections(GraphNode node)
	{
		node.ClearConnections(alsoReverse: true);
	}

	[ContextMenu("Recalculate neighbours")]
	private void ContextApplyForce()
	{
		if (Application.isPlaying)
		{
			Apply(forceNewCheck: true);
			if (AstarPath.active != null)
			{
				AstarPath.active.FloodFill();
			}
		}
	}

	public void Apply(bool forceNewCheck)
	{
		NNConstraint none = NNConstraint.None;
		int graphIndex = (int)startNode.GraphIndex;
		none.graphMask = ~(1 << graphIndex);
		startNode.SetPosition((Int3)StartTransform.position);
		endNode.SetPosition((Int3)EndTransform.position);
		RemoveConnections(startNode);
		RemoveConnections(endNode);
		uint cost = (uint)Mathf.RoundToInt((float)((Int3)(StartTransform.position - EndTransform.position)).costMagnitude * costFactor);
		startNode.AddConnection(endNode, cost);
		endNode.AddConnection(startNode, cost);
		if (connectedNode1 == null || forceNewCheck)
		{
			NNInfo nearest = AstarPath.active.GetNearest(StartTransform.position, none);
			connectedNode1 = nearest.node;
			clamped1 = nearest.clampedPosition;
		}
		if (connectedNode2 == null || forceNewCheck)
		{
			NNInfo nearest2 = AstarPath.active.GetNearest(EndTransform.position, none);
			connectedNode2 = nearest2.node;
			clamped2 = nearest2.clampedPosition;
		}
		if (connectedNode2 != null && connectedNode1 != null)
		{
			connectedNode1.AddConnection(startNode, (uint)Mathf.RoundToInt((float)((Int3)(clamped1 - StartTransform.position)).costMagnitude * costFactor));
			if (!oneWay)
			{
				connectedNode2.AddConnection(endNode, (uint)Mathf.RoundToInt((float)((Int3)(clamped2 - EndTransform.position)).costMagnitude * costFactor));
			}
			if (!oneWay)
			{
				startNode.AddConnection(connectedNode1, (uint)Mathf.RoundToInt((float)((Int3)(clamped1 - StartTransform.position)).costMagnitude * costFactor));
			}
			endNode.AddConnection(connectedNode2, (uint)Mathf.RoundToInt((float)((Int3)(clamped2 - EndTransform.position)).costMagnitude * costFactor));
		}
	}

	private void DrawCircle(Vector3 o, float r, int detail, Color col)
	{
		Vector3 from = new Vector3(Mathf.Cos(0f) * r, 0f, Mathf.Sin(0f) * r) + o;
		Gizmos.color = col;
		for (int i = 0; i <= detail; i++)
		{
			float f = (float)i * (float)Math.PI * 2f / (float)detail;
			Vector3 vector = new Vector3(Mathf.Cos(f) * r, 0f, Mathf.Sin(f) * r) + o;
			Gizmos.DrawLine(from, vector);
			from = vector;
		}
	}

	private void DrawGizmoBezier(Vector3 p1, Vector3 p2)
	{
		Vector3 vector = p2 - p1;
		if (!(vector == Vector3.zero))
		{
			Vector3 rhs = Vector3.Cross(Vector3.up, vector);
			Vector3 normalized = Vector3.Cross(vector, rhs).normalized;
			normalized *= vector.magnitude * 0.1f;
			Vector3 p3 = p1 + normalized;
			Vector3 p4 = p2 + normalized;
			Vector3 from = p1;
			for (int i = 1; i <= 20; i++)
			{
				float t = (float)i / 20f;
				Vector3 vector2 = AstarSplines.CubicBezier(p1, p3, p4, p2, t);
				Gizmos.DrawLine(from, vector2);
				from = vector2;
			}
		}
	}

	public virtual void OnDrawGizmosSelected()
	{
		OnDrawGizmos(selected: true);
	}

	public void OnDrawGizmos()
	{
		OnDrawGizmos(selected: false);
	}

	public void OnDrawGizmos(bool selected)
	{
		Color color = ((!selected) ? GizmosColor : GizmosColorSelected);
		if (StartTransform != null)
		{
			DrawCircle(StartTransform.position, 0.4f, 10, color);
		}
		if (EndTransform != null)
		{
			DrawCircle(EndTransform.position, 0.4f, 10, color);
		}
		if (StartTransform != null && EndTransform != null)
		{
			Gizmos.color = color;
			DrawGizmoBezier(StartTransform.position, EndTransform.position);
			if (selected)
			{
				Vector3 normalized = Vector3.Cross(Vector3.up, EndTransform.position - StartTransform.position).normalized;
				DrawGizmoBezier(StartTransform.position + normalized * 0.1f, EndTransform.position + normalized * 0.1f);
				DrawGizmoBezier(StartTransform.position - normalized * 0.1f, EndTransform.position - normalized * 0.1f);
			}
		}
	}

	internal static void SerializeReferences(GraphSerializationContext ctx)
	{
		List<NodeLink2> modifiersOfType = GraphModifier.GetModifiersOfType<NodeLink2>();
		ctx.writer.Write(modifiersOfType.Count);
		foreach (NodeLink2 item in modifiersOfType)
		{
			ctx.writer.Write(item.uniqueID);
			ctx.SerializeNodeReference(item.startNode);
			ctx.SerializeNodeReference(item.endNode);
			ctx.SerializeNodeReference(item.connectedNode1);
			ctx.SerializeNodeReference(item.connectedNode2);
			ctx.SerializeVector3(item.clamped1);
			ctx.SerializeVector3(item.clamped2);
			ctx.writer.Write(item.postScanCalled);
		}
	}

	internal static void DeserializeReferences(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			ulong key = ctx.reader.ReadUInt64();
			GraphNode graphNode = ctx.DeserializeNodeReference();
			GraphNode graphNode2 = ctx.DeserializeNodeReference();
			GraphNode graphNode3 = ctx.DeserializeNodeReference();
			GraphNode graphNode4 = ctx.DeserializeNodeReference();
			Vector3 vector = ctx.DeserializeVector3();
			Vector3 vector2 = ctx.DeserializeVector3();
			bool flag = ctx.reader.ReadBoolean();
			if (GraphModifier.usedIDs.TryGetValue(key, out var value))
			{
				NodeLink2 nodeLink = value as NodeLink2;
				if (nodeLink != null)
				{
					reference[graphNode] = nodeLink;
					reference[graphNode2] = nodeLink;
					if (nodeLink.startNode != null)
					{
						reference.Remove(nodeLink.startNode);
					}
					if (nodeLink.endNode != null)
					{
						reference.Remove(nodeLink.endNode);
					}
					nodeLink.startNode = graphNode as PointNode;
					nodeLink.endNode = graphNode2 as PointNode;
					nodeLink.connectedNode1 = graphNode3;
					nodeLink.connectedNode2 = graphNode4;
					nodeLink.postScanCalled = flag;
					nodeLink.clamped1 = vector;
					nodeLink.clamped2 = vector2;
					continue;
				}
				throw new Exception("Tried to deserialize a NodeLink2 reference, but the link was not of the correct type or it has been destroyed.\nIf a NodeLink2 is included in serialized graph data, the same NodeLink2 component must be present in the scene when loading the graph data.");
			}
			throw new Exception("Tried to deserialize a NodeLink2 reference, but the link could not be found in the scene.\nIf a NodeLink2 is included in serialized graph data, the same NodeLink2 component must be present in the scene when loading the graph data.");
		}
	}
}
