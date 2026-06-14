using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_simple_smooth_modifier.php")]
[RequireComponent(typeof(Seeker))]
[AddComponentMenu("Pathfinding/Modifiers/T17 Path Modifier")]
public class T17PathModifier : MonoModifier
{
	[Tooltip("Number of times to apply smoothing")]
	public int iterations = 1;

	[Tooltip("Number of times to cut corners")]
	public int cutCornerIterations = 1;

	[Tooltip("Determines how much smoothing to apply in each smooth iteration. 0.5 usually produces the nicest looking curves")]
	public float strength = 1f;

	[Tooltip("Toggle to divide all lines in equal length segments")]
	public bool uniformLength = true;

	[Tooltip("The length of each segment in the smoothed path. A high value yields rough paths and low value yields very smooth paths, but is slower")]
	public float maxSegmentLength = 0.5f;

	public override int Order => 50;

	public override void Apply(Path path)
	{
		T17_ABPath t17_ABPath = (T17_ABPath)path;
		if (t17_ABPath.vectorPath == null || t17_ABPath.path == null)
		{
			Debug.LogWarning("Can't process NULL path (has another modifier logged an error?)");
		}
		else
		{
			if (t17_ABPath.path.Count == 0)
			{
				return;
			}
			if (t17_ABPath.path_bSkipLastNode)
			{
				int index = t17_ABPath.path.Count - 1;
				int num = t17_ABPath.path.Count - 2;
				if (num >= 0)
				{
					t17_ABPath.path[index] = t17_ABPath.path[num];
					t17_ABPath.vectorPath[index] = t17_ABPath.vectorPath[num];
				}
			}
			if (t17_ABPath.vectorPath.Count < 2)
			{
				return;
			}
			for (int i = 0; i < iterations; i++)
			{
				bool flag = true;
				for (int num2 = t17_ABPath.path.Count - 2; num2 >= 2; num2--)
				{
					if (t17_ABPath.path[num2 - 1].m_bHasDamagableTile || t17_ABPath.path[num2].m_bHasDamagableTile || t17_ABPath.path[num2 + 1].m_bHasDamagableTile)
					{
						flag = false;
					}
					else
					{
						if (num2 % 3 != 0)
						{
							continue;
						}
						if (flag)
						{
							GraphNode n = t17_ABPath.path[num2 - 1];
							GraphNode n2 = t17_ABPath.path[num2 + 1];
							if (n.m_bHasDamagableTile || n2.m_bHasDamagableTile || t17_ABPath.path[num2].m_bHasDamagableTile)
							{
								Debug.LogError("Trying to shortcut a damageable tile! Skipping");
								continue;
							}
							Vector3 v = t17_ABPath.vectorPath[num2 - 1];
							Vector3 v2 = t17_ABPath.vectorPath[num2 + 1];
							if (CanCutCorner(ref n, ref n2, ref v, ref v2))
							{
								t17_ABPath.path.RemoveAt(num2);
							}
						}
						flag = true;
					}
				}
			}
			List<Vector3> list = SmoothSimple(t17_ABPath);
			if (list != t17_ABPath.vectorPath)
			{
				ListPool<Vector3>.Release(t17_ABPath.vectorPath);
				t17_ABPath.vectorPath = list;
			}
		}
	}

	private bool CanCutCorner(ref GraphNode n1, ref GraphNode n2, ref Vector3 v1, ref Vector3 v2)
	{
		if (n1 != null && n2 != null)
		{
			NavGraph graph = AstarData.GetGraph(n1);
			NavGraph graph2 = AstarData.GetGraph(n2);
			if (graph != graph2)
			{
				return false;
			}
			if (graph != null && graph is IRaycastableGraph raycastableGraph)
			{
				Vector3 vector = v1 - v2;
				Vector3 normalized = new Vector3(0f - vector.y, vector.x, 0f).normalized;
				Vector3 start = v1 + normalized * 0.5f;
				Vector3 end = v2 + normalized * 0.5f;
				Vector3 start2 = v1 - normalized * 0.5f;
				Vector3 end2 = v2 - normalized * 0.5f;
				GraphHitInfo hit;
				bool flag = raycastableGraph.Linecast(start2, end2, n1, out hit);
				bool flag2 = raycastableGraph.Linecast(start, end, n1, out hit);
				return !flag && !flag2;
			}
		}
		return false;
	}

	public List<Vector3> SmoothSimple(Path p)
	{
		List<GraphNode> path = p.path;
		float num = 0f;
		for (int i = 0; i < path.Count - 1; i++)
		{
			Vector2 a = (Vector3)path[i].position;
			Vector2 b = (Vector3)path[i + 1].position;
			num += Vector2.Distance(a, b);
		}
		int num2 = Mathf.FloorToInt(num / maxSegmentLength);
		List<Vector3> list = ListPool<Vector3>.Claim(num2 + 2);
		float num3 = 0f;
		for (int j = 0; j < path.Count - 1; j++)
		{
			Vector2 vector = (Vector3)path[j].position;
			Vector2 b2 = (Vector3)path[j + 1].position;
			if (path[j].GraphIndex == path[j + 1].GraphIndex)
			{
				float num4 = Vector2.Distance(vector, b2);
				for (num3 = 0f; num3 < num4; num3 += maxSegmentLength)
				{
					float num5 = num3 / num4;
					Vector3 item = Vector2.Lerp(vector, b2, num5);
					int num6 = j;
					if (num5 >= 0.5f)
					{
						num6++;
					}
					float z = (float)num6 + 0.5f;
					item.z = z;
					list.Add(item);
				}
			}
			else
			{
				Vector3 item2 = vector;
				float z2 = (float)j + 0.5f;
				item2.z = z2;
				list.Add(item2);
			}
		}
		Vector3 vector2 = (Vector3)path[path.Count - 1].position;
		vector2.z = path.Count - 1;
		int count = list.Count;
		if (count > 0)
		{
			list[count - 1] = vector2;
		}
		else
		{
			list.Add(vector2);
		}
		if (strength > 0f)
		{
			for (int k = 0; k < iterations; k++)
			{
				Vector3 vector3 = list[0];
				for (int l = 1; l < list.Count - 1; l++)
				{
					Vector3 vector4 = list[l];
					Vector3 value = list[l];
					if (path[Mathf.FloorToInt(vector3.z)].GraphIndex != path[Mathf.FloorToInt(list[l + 1].z)].GraphIndex)
					{
						vector3 = vector4;
						continue;
					}
					Vector2 vector5 = Vector2.Lerp(vector4, (vector3 + list[l + 1]) / 2f, strength);
					value.x = vector5.x;
					value.y = vector5.y;
					list[l] = value;
					vector3 = vector4;
				}
			}
		}
		return list;
	}
}
