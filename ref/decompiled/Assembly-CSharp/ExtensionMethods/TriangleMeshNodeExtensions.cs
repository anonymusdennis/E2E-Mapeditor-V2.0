using Pathfinding;
using UnityEngine;

namespace ExtensionMethods;

public static class TriangleMeshNodeExtensions
{
	public static Vector3 GetVertexPos(this TriangleMeshNode node, int i)
	{
		INavmeshHolder navmeshHolder = TriangleMeshNode.GetNavmeshHolder(node.GraphIndex);
		return (Vector3)navmeshHolder.GetVertex(node.GetVertexIndex(i));
	}
}
