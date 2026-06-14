using System;
using System.Diagnostics;
using UnityEngine;

namespace Pathfinding;

public class BBTree
{
	private struct BBTreeBox
	{
		public IntRect rect;

		public MeshNode node;

		public int left;

		public int right;

		public bool IsLeaf => node != null;

		public BBTreeBox(IntRect rect)
		{
			node = null;
			this.rect = rect;
			left = (right = -1);
		}

		public BBTreeBox(MeshNode node, IntRect rect)
		{
			this.node = node;
			this.rect = rect;
			left = (right = -1);
		}

		public bool Contains(Vector3 p)
		{
			Int3 @int = (Int3)p;
			return rect.Contains(@int.x, @int.z);
		}
	}

	private BBTreeBox[] arr = new BBTreeBox[6];

	private int count;

	public static Stopwatch watch = new Stopwatch();

	public Rect Size
	{
		get
		{
			if (count == 0)
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
			IntRect rect = arr[0].rect;
			return Rect.MinMaxRect((float)rect.xmin * 0.001f, (float)rect.ymin * 0.001f, (float)rect.xmax * 0.001f, (float)rect.ymax * 0.001f);
		}
	}

	public void Clear()
	{
		count = 0;
	}

	private void EnsureCapacity(int c)
	{
		if (arr.Length < c)
		{
			BBTreeBox[] array = new BBTreeBox[Math.Max(c, (int)((float)arr.Length * 1.5f))];
			for (int i = 0; i < count; i++)
			{
				ref BBTreeBox reference = ref array[i];
				reference = arr[i];
			}
			arr = array;
		}
	}

	private int GetBox(MeshNode node, IntRect bounds)
	{
		if (count >= arr.Length)
		{
			EnsureCapacity(count + 1);
		}
		ref BBTreeBox reference = ref arr[count];
		reference = new BBTreeBox(node, bounds);
		count++;
		return count - 1;
	}

	private int GetBox(IntRect rect)
	{
		if (count >= arr.Length)
		{
			EnsureCapacity(count + 1);
		}
		ref BBTreeBox reference = ref arr[count];
		reference = new BBTreeBox(rect);
		count++;
		return count - 1;
	}

	public void RebuildFrom(MeshNode[] nodes)
	{
		Clear();
		if (nodes.Length != 0)
		{
			EnsureCapacity(Mathf.CeilToInt((float)nodes.Length * 2.1f));
			int[] array = new int[nodes.Length];
			for (int i = 0; i < nodes.Length; i++)
			{
				array[i] = i;
			}
			IntRect[] array2 = new IntRect[nodes.Length];
			for (int j = 0; j < nodes.Length; j++)
			{
				MeshNode meshNode = nodes[j];
				Int3 vertex = meshNode.GetVertex(0);
				Int3 vertex2 = meshNode.GetVertex(1);
				Int3 vertex3 = meshNode.GetVertex(2);
				IntRect intRect = new IntRect(vertex.x, vertex.z, vertex.x, vertex.z).ExpandToContain(vertex2.x, vertex2.z).ExpandToContain(vertex3.x, vertex3.z);
				array2[j] = intRect;
			}
			RebuildFromInternal(nodes, array, array2, 0, nodes.Length, odd: false);
		}
	}

	private static int SplitByX(MeshNode[] nodes, int[] permutation, int from, int to, int divider)
	{
		int num = to;
		for (int i = from; i < num; i++)
		{
			if (nodes[permutation[i]].position.x > divider)
			{
				num--;
				int num2 = permutation[num];
				permutation[num] = permutation[i];
				permutation[i] = num2;
				i--;
			}
		}
		return num;
	}

	private static int SplitByZ(MeshNode[] nodes, int[] permutation, int from, int to, int divider)
	{
		int num = to;
		for (int i = from; i < num; i++)
		{
			if (nodes[permutation[i]].position.z > divider)
			{
				num--;
				int num2 = permutation[num];
				permutation[num] = permutation[i];
				permutation[i] = num2;
				i--;
			}
		}
		return num;
	}

	private int RebuildFromInternal(MeshNode[] nodes, int[] permutation, IntRect[] nodeBounds, int from, int to, bool odd)
	{
		if (to - from == 1)
		{
			return GetBox(nodes[permutation[from]], nodeBounds[permutation[from]]);
		}
		IntRect rect = NodeBounds(permutation, nodeBounds, from, to);
		int box = GetBox(rect);
		if (to - from == 2)
		{
			arr[box].left = GetBox(nodes[permutation[from]], nodeBounds[permutation[from]]);
			arr[box].right = GetBox(nodes[permutation[from + 1]], nodeBounds[permutation[from + 1]]);
			return box;
		}
		int num;
		if (odd)
		{
			int divider = (rect.xmin + rect.xmax) / 2;
			num = SplitByX(nodes, permutation, from, to, divider);
		}
		else
		{
			int divider2 = (rect.ymin + rect.ymax) / 2;
			num = SplitByZ(nodes, permutation, from, to, divider2);
		}
		if (num == from || num == to)
		{
			if (!odd)
			{
				int divider3 = (rect.xmin + rect.xmax) / 2;
				num = SplitByX(nodes, permutation, from, to, divider3);
			}
			else
			{
				int divider4 = (rect.ymin + rect.ymax) / 2;
				num = SplitByZ(nodes, permutation, from, to, divider4);
			}
			if (num == from || num == to)
			{
				num = (from + to) / 2;
			}
		}
		arr[box].left = RebuildFromInternal(nodes, permutation, nodeBounds, from, num, !odd);
		arr[box].right = RebuildFromInternal(nodes, permutation, nodeBounds, num, to, !odd);
		return box;
	}

	private static IntRect NodeBounds(int[] permutation, IntRect[] nodeBounds, int from, int to)
	{
		if (to - from <= 0)
		{
			throw new ArgumentException();
		}
		IntRect result = nodeBounds[permutation[from]];
		for (int i = from + 1; i < to; i++)
		{
			IntRect intRect = nodeBounds[permutation[i]];
			result.xmin = Math.Min(result.xmin, intRect.xmin);
			result.ymin = Math.Min(result.ymin, intRect.ymin);
			result.xmax = Math.Max(result.xmax, intRect.xmax);
			result.ymax = Math.Max(result.ymax, intRect.ymax);
		}
		return result;
	}

	public NNInfo Query(Vector3 p, NNConstraint constraint)
	{
		if (count == 0)
		{
			return new NNInfo(null);
		}
		NNInfo nnInfo = default(NNInfo);
		SearchBox(0, p, constraint, ref nnInfo);
		nnInfo.UpdateInfo();
		return nnInfo;
	}

	public NNInfo QueryCircle(Vector3 p, float radius, NNConstraint constraint)
	{
		if (count == 0)
		{
			return new NNInfo(null);
		}
		NNInfo nnInfo = new NNInfo(null);
		SearchBoxCircle(0, p, radius, constraint, ref nnInfo);
		nnInfo.UpdateInfo();
		return nnInfo;
	}

	public NNInfo QueryClosest(Vector3 p, NNConstraint constraint, out float distance)
	{
		distance = float.PositiveInfinity;
		return QueryClosest(p, constraint, ref distance, new NNInfo(null));
	}

	public NNInfo QueryClosestXZ(Vector3 p, NNConstraint constraint, ref float distance, NNInfo previous)
	{
		if (count == 0)
		{
			return previous;
		}
		SearchBoxClosestXZ(0, p, ref distance, constraint, ref previous);
		return previous;
	}

	private void SearchBoxClosestXZ(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			Vector3 constClampedPosition = bBTreeBox.node.ClosestPointOnNodeXZ(p);
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				float num = (constClampedPosition.x - p.x) * (constClampedPosition.x - p.x) + (constClampedPosition.z - p.z) * (constClampedPosition.z - p.z);
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = constClampedPosition;
					closestDist = (float)Math.Sqrt(num);
				}
				else if (num < closestDist * closestDist)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = constClampedPosition;
					closestDist = (float)Math.Sqrt(num);
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, closestDist))
			{
				SearchBoxClosestXZ(bBTreeBox.left, p, ref closestDist, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, closestDist))
			{
				SearchBoxClosestXZ(bBTreeBox.right, p, ref closestDist, constraint, ref nnInfo);
			}
		}
	}

	public NNInfo QueryClosest(Vector3 p, NNConstraint constraint, ref float distance, NNInfo previous)
	{
		if (count == 0)
		{
			return previous;
		}
		SearchBoxClosest(0, p, ref distance, constraint, ref previous);
		return previous;
	}

	private void SearchBoxClosest(int boxi, Vector3 p, ref float closestDist, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!NodeIntersectsCircle(bBTreeBox.node, p, closestDist))
			{
				return;
			}
			Vector3 vector = bBTreeBox.node.ClosestPointOnNode(p);
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				float sqrMagnitude = (vector - p).sqrMagnitude;
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
					closestDist = (float)Math.Sqrt(sqrMagnitude);
				}
				else if (sqrMagnitude < closestDist * closestDist)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
					closestDist = (float)Math.Sqrt(sqrMagnitude);
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, closestDist))
			{
				SearchBoxClosest(bBTreeBox.left, p, ref closestDist, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, closestDist))
			{
				SearchBoxClosest(bBTreeBox.right, p, ref closestDist, constraint, ref nnInfo);
			}
		}
	}

	public MeshNode QueryInside(Vector3 p, NNConstraint constraint)
	{
		return (count == 0) ? null : SearchBoxInside(0, p, constraint);
	}

	private MeshNode SearchBoxInside(int boxi, Vector3 p, NNConstraint constraint)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (bBTreeBox.node.ContainsPoint((Int3)p) && (constraint == null || constraint.Suitable(bBTreeBox.node)))
			{
				return bBTreeBox.node;
			}
		}
		else
		{
			if (arr[bBTreeBox.left].Contains(p))
			{
				MeshNode meshNode = SearchBoxInside(bBTreeBox.left, p, constraint);
				if (meshNode != null)
				{
					return meshNode;
				}
			}
			if (arr[bBTreeBox.right].Contains(p))
			{
				MeshNode meshNode = SearchBoxInside(bBTreeBox.right, p, constraint);
				if (meshNode != null)
				{
					return meshNode;
				}
			}
		}
		return null;
	}

	private void SearchBoxCircle(int boxi, Vector3 p, float radius, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!NodeIntersectsCircle(bBTreeBox.node, p, radius))
			{
				return;
			}
			Vector3 vector = bBTreeBox.node.ClosestPointOnNode(p);
			float sqrMagnitude = (vector - p).sqrMagnitude;
			if (nnInfo.node == null)
			{
				nnInfo.node = bBTreeBox.node;
				nnInfo.clampedPosition = vector;
			}
			else if (sqrMagnitude < (nnInfo.clampedPosition - p).sqrMagnitude)
			{
				nnInfo.node = bBTreeBox.node;
				nnInfo.clampedPosition = vector;
			}
			if (constraint == null || constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
				}
				else if (sqrMagnitude < (nnInfo.constClampedPosition - p).sqrMagnitude)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
					nnInfo.constClampedPosition = vector;
				}
			}
		}
		else
		{
			if (RectIntersectsCircle(arr[bBTreeBox.left].rect, p, radius))
			{
				SearchBoxCircle(bBTreeBox.left, p, radius, constraint, ref nnInfo);
			}
			if (RectIntersectsCircle(arr[bBTreeBox.right].rect, p, radius))
			{
				SearchBoxCircle(bBTreeBox.right, p, radius, constraint, ref nnInfo);
			}
		}
	}

	private void SearchBox(int boxi, Vector3 p, NNConstraint constraint, ref NNInfo nnInfo)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		if (bBTreeBox.node != null)
		{
			if (!bBTreeBox.node.ContainsPoint((Int3)p))
			{
				return;
			}
			if (nnInfo.node == null)
			{
				nnInfo.node = bBTreeBox.node;
			}
			else if (Mathf.Abs(((Vector3)bBTreeBox.node.position).y - p.y) < Mathf.Abs(((Vector3)nnInfo.node.position).y - p.y))
			{
				nnInfo.node = bBTreeBox.node;
			}
			if (constraint.Suitable(bBTreeBox.node))
			{
				if (nnInfo.constrainedNode == null)
				{
					nnInfo.constrainedNode = bBTreeBox.node;
				}
				else if (Mathf.Abs((float)bBTreeBox.node.position.y - p.y) < Mathf.Abs((float)nnInfo.constrainedNode.position.y - p.y))
				{
					nnInfo.constrainedNode = bBTreeBox.node;
				}
			}
		}
		else
		{
			if (arr[bBTreeBox.left].Contains(p))
			{
				SearchBox(bBTreeBox.left, p, constraint, ref nnInfo);
			}
			if (arr[bBTreeBox.right].Contains(p))
			{
				SearchBox(bBTreeBox.right, p, constraint, ref nnInfo);
			}
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
		if (count != 0)
		{
			OnDrawGizmos(0, 0);
		}
	}

	private void OnDrawGizmos(int boxi, int depth)
	{
		BBTreeBox bBTreeBox = arr[boxi];
		Vector3 vector = (Vector3)new Int3(bBTreeBox.rect.xmin, 0, bBTreeBox.rect.ymin);
		Vector3 vector2 = (Vector3)new Int3(bBTreeBox.rect.xmax, 0, bBTreeBox.rect.ymax);
		Vector3 vector3 = (vector + vector2) * 0.5f;
		Vector3 vector4 = (vector2 - vector3) * 2f;
		vector4 = new Vector3(vector4.x, 1f, vector4.z);
		vector3.y += depth * 2;
		Gizmos.color = AstarMath.IntToColor(depth, 1f);
		Gizmos.DrawCube(vector3, vector4);
		if (bBTreeBox.node == null)
		{
			OnDrawGizmos(bBTreeBox.left, depth + 1);
			OnDrawGizmos(bBTreeBox.right, depth + 1);
		}
	}

	private static bool NodeIntersectsCircle(MeshNode node, Vector3 p, float radius)
	{
		if (float.IsPositiveInfinity(radius))
		{
			return true;
		}
		return (p - node.ClosestPointOnNode(p)).sqrMagnitude < radius * radius;
	}

	private static bool RectIntersectsCircle(IntRect r, Vector3 p, float radius)
	{
		if (float.IsPositiveInfinity(radius))
		{
			return true;
		}
		Vector3 vector = p;
		p.x = Math.Max(p.x, (float)r.xmin * 0.001f);
		p.x = Math.Min(p.x, (float)r.xmax * 0.001f);
		p.z = Math.Max(p.z, (float)r.ymin * 0.001f);
		p.z = Math.Min(p.z, (float)r.ymax * 0.001f);
		return (p.x - vector.x) * (p.x - vector.x) + (p.z - vector.z) * (p.z - vector.z) < radius * radius;
	}

	private static IntRect ExpandToContain(IntRect r, IntRect r2)
	{
		return IntRect.Union(r, r2);
	}
}
