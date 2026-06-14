using System.Collections.Generic;
using UnityEngine;

public class RoomMeshGenerator : MonoBehaviour
{
	public class Triangulator
	{
		private List<Vector3> m_points = new List<Vector3>();

		public Triangulator(Vector3[] points)
		{
			m_points = new List<Vector3>(points);
		}

		public int[] Triangulate()
		{
			List<int> list = new List<int>();
			int count = m_points.Count;
			if (count < 3)
			{
				return list.ToArray();
			}
			int[] array = new int[count];
			if (Area() > 0f)
			{
				for (int i = 0; i < count; i++)
				{
					array[i] = i;
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					array[j] = count - 1 - j;
				}
			}
			int num = count;
			int num2 = 2 * num;
			int num3 = 0;
			int num4 = num - 1;
			while (num > 2)
			{
				if (num2-- <= 0)
				{
					return list.ToArray();
				}
				int num5 = num4;
				if (num <= num5)
				{
					num5 = 0;
				}
				num4 = num5 + 1;
				if (num <= num4)
				{
					num4 = 0;
				}
				int num6 = num4 + 1;
				if (num <= num6)
				{
					num6 = 0;
				}
				if (Snip(num5, num4, num6, num, array))
				{
					int item = array[num5];
					int item2 = array[num4];
					int item3 = array[num6];
					list.Add(item);
					list.Add(item2);
					list.Add(item3);
					num3++;
					int num7 = num4;
					for (int k = num4 + 1; k < num; k++)
					{
						array[num7] = array[k];
						num7++;
					}
					num--;
					num2 = 2 * num;
				}
			}
			list.Reverse();
			return list.ToArray();
		}

		private float Area()
		{
			int count = m_points.Count;
			float num = 0f;
			int index = count - 1;
			int num2 = 0;
			while (num2 < count)
			{
				Vector2 vector = m_points[index];
				Vector2 vector2 = m_points[num2];
				num += vector.x * vector2.y - vector2.x * vector.y;
				index = num2++;
			}
			return num * 0.5f;
		}

		private bool Snip(int u, int v, int w, int n, int[] V)
		{
			Vector2 a = m_points[V[u]];
			Vector2 b = m_points[V[v]];
			Vector2 c = m_points[V[w]];
			if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
			{
				return false;
			}
			for (int i = 0; i < n; i++)
			{
				if (i != u && i != v && i != w)
				{
					Vector2 p = m_points[V[i]];
					if (InsideTriangle(a, b, c, p))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
		{
			float num = C.x - B.x;
			float num2 = C.y - B.y;
			float num3 = A.x - C.x;
			float num4 = A.y - C.y;
			float num5 = B.x - A.x;
			float num6 = B.y - A.y;
			float num7 = P.x - A.x;
			float num8 = P.y - A.y;
			float num9 = P.x - B.x;
			float num10 = P.y - B.y;
			float num11 = P.x - C.x;
			float num12 = P.y - C.y;
			float num13 = num * num10 - num2 * num9;
			float num14 = num5 * num8 - num6 * num7;
			float num15 = num3 * num12 - num4 * num11;
			return num13 >= 0f && num15 >= 0f && num14 >= 0f;
		}
	}

	private class PatchMesh
	{
		public int searchDirection;

		public Vector3 a;

		public Vector3 b;

		public Vector3 c;

		public Vector3 d;
	}

	private static Vector3[] m_clockWiseSearchDirections = new Vector3[4]
	{
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 1f, 0f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(0f, -1f, 0f)
	};

	private static RoomManager m_RoomManager;

	private static List<Vector3> m_Vertices = new List<Vector3>();

	public static RoomOcclusionMesh GenerateRoomMesh(RoomBlob roomBlob, RoomFloor roomFloor)
	{
		if (m_RoomManager == null)
		{
			m_RoomManager = RoomManager.GetInstance();
		}
		RoomOcclusionMesh roomOcclusionMesh = roomBlob.GetComponent<RoomOcclusionMesh>();
		MeshFilter meshFilter = roomBlob.GetComponent<MeshFilter>();
		if (roomOcclusionMesh == null)
		{
			roomOcclusionMesh = roomBlob.gameObject.AddComponent<RoomOcclusionMesh>();
		}
		roomOcclusionMesh.m_FloorIndex = roomFloor.m_FloorIndex;
		if (meshFilter == null)
		{
			meshFilter = roomBlob.gameObject.AddComponent<MeshFilter>();
		}
		List<CombineInstance> list = new List<CombineInstance>();
		List<PatchMesh> list2 = new List<PatchMesh>();
		for (int i = 0; i < roomBlob.m_FirstRoomTiles.Count; i++)
		{
			m_Vertices.Clear();
			Vector3 vector = roomBlob.m_FirstRoomTiles[i];
			Vector3 vector2 = vector;
			int num = 0;
			AddVertex(vector2, num, roomBlob, roomFloor);
			bool flag = false;
			int num2 = 0;
			bool flag2 = false;
			Vector3 a = Vector3.zero;
			Vector3 b = Vector3.zero;
			bool flag3 = false;
			do
			{
				int perpendicularDirectionIndex = GetPerpendicularDirectionIndex(num);
				Vector3 vector3 = vector2 + m_clockWiseSearchDirections[num];
				Vector3 vector4 = vector2 + m_clockWiseSearchDirections[perpendicularDirectionIndex];
				Vector3 vector5 = vector4 + m_clockWiseSearchDirections[num];
				RoomBlob roomBlob2 = null;
				RoomBlob roomBlob3 = null;
				RoomBlob roomBlob4 = null;
				if (IsTileWithinBounds(vector3, roomFloor.m_FloorWidth, roomFloor.m_FloorHeight))
				{
					roomBlob2 = m_RoomManager.LookUpRoom(vector3, roomFloor);
				}
				if (IsTileWithinBounds(vector4, roomFloor.m_FloorWidth, roomFloor.m_FloorHeight))
				{
					roomBlob3 = m_RoomManager.LookUpRoom(vector4, roomFloor);
				}
				if (IsTileWithinBounds(vector5, roomFloor.m_FloorWidth, roomFloor.m_FloorHeight))
				{
					roomBlob4 = m_RoomManager.LookUpRoom(vector5, roomFloor);
				}
				bool flag4 = roomBlob3 != null && roomBlob3 == roomBlob;
				bool flag5 = !flag4 && (roomBlob2 == null || roomBlob2 != roomBlob);
				bool flag6 = roomBlob4 != null && roomBlob4 == roomBlob;
				bool flag7 = roomBlob2 != null && roomBlob2.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors && roomBlob2.location != RoomBlob.eLocation.BuildingBoundary;
				bool flag8 = roomBlob3 != null && roomBlob3.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors && roomBlob3.location != RoomBlob.eLocation.BuildingBoundary;
				bool flag9 = roomBlob4 != null && roomBlob4.m_subLocation == RoomBlob.RoomSubIdentity_Location.Indoors && roomBlob4.location != RoomBlob.eLocation.BuildingBoundary;
				bool flag10 = flag4 || flag5;
				bool flag11 = flag8 && !flag6 && !flag5 && flag9;
				bool flag12 = flag10 || !flag8 || !flag9;
				if (flag11 && !flag)
				{
					a = vector2;
					b = vector4;
					flag = true;
				}
				else if (flag12 && flag)
				{
					Vector3 vector6 = vector2;
					Vector3 vector7 = vector4;
					PatchMesh patchMesh = new PatchMesh();
					patchMesh.searchDirection = num;
					patchMesh.a = a;
					patchMesh.b = b;
					Vector3 vector8 = Vector3.zero;
					if (flag7 && flag9)
					{
						vector8 = m_clockWiseSearchDirections[num];
					}
					patchMesh.c = vector7 + vector8;
					patchMesh.d = vector6 + vector8;
					list2.Add(patchMesh);
					flag = false;
				}
				if (flag4)
				{
					num = perpendicularDirectionIndex;
					AddVertex(vector2, num, roomBlob, roomFloor);
				}
				if (flag5)
				{
					AddVertex(vector2, num, roomBlob, roomFloor);
					num = GetNextSearchDirectionIndex(num);
				}
				if (!flag5)
				{
					RoomBlob roomBlob5 = m_RoomManager.LookUpRoom(vector2 + m_clockWiseSearchDirections[num], roomFloor);
					if (roomBlob5 == roomBlob)
					{
						vector2 += m_clockWiseSearchDirections[num];
					}
				}
				if (vector2 == vector && num == 0 && flag3)
				{
					flag2 = true;
				}
				flag3 = true;
				num2 = (flag10 ? (num2 + 1) : 0);
				if (num2 > 4)
				{
					Debug.LogError("Something when wrong when generating the Room Mesh for " + roomBlob);
					break;
				}
			}
			while (!flag2);
			Mesh mesh = new Mesh();
			mesh.vertices = m_Vertices.ToArray();
			mesh.uv = new Vector2[m_Vertices.Count];
			Triangulator triangulator = new Triangulator(mesh.vertices);
			mesh.triangles = triangulator.Triangulate();
			CombineInstance item = default(CombineInstance);
			item.mesh = mesh;
			item.transform = roomBlob.transform.localToWorldMatrix;
			list.Add(item);
		}
		for (int j = 0; j < list2.Count; j++)
		{
			m_Vertices.Clear();
			PatchMesh patchMesh2 = list2[j];
			switch (patchMesh2.searchDirection)
			{
			case 0:
				AddVertex(patchMesh2.a, 2, roomBlob, roomFloor);
				AddVertex(patchMesh2.b, 3, roomBlob, roomFloor);
				AddVertex(patchMesh2.c, 0, roomBlob, roomFloor);
				AddVertex(patchMesh2.d, 1, roomBlob, roomFloor);
				break;
			case 1:
				AddVertex(patchMesh2.d, 2, roomBlob, roomFloor);
				AddVertex(patchMesh2.a, 3, roomBlob, roomFloor);
				AddVertex(patchMesh2.b, 0, roomBlob, roomFloor);
				AddVertex(patchMesh2.c, 1, roomBlob, roomFloor);
				break;
			case 2:
				AddVertex(patchMesh2.c, 2, roomBlob, roomFloor);
				AddVertex(patchMesh2.d, 3, roomBlob, roomFloor);
				AddVertex(patchMesh2.a, 0, roomBlob, roomFloor);
				AddVertex(patchMesh2.b, 1, roomBlob, roomFloor);
				break;
			case 3:
				AddVertex(patchMesh2.b, 2, roomBlob, roomFloor);
				AddVertex(patchMesh2.c, 3, roomBlob, roomFloor);
				AddVertex(patchMesh2.d, 0, roomBlob, roomFloor);
				AddVertex(patchMesh2.a, 1, roomBlob, roomFloor);
				break;
			}
			Mesh mesh2 = new Mesh();
			mesh2.vertices = m_Vertices.ToArray();
			mesh2.uv = new Vector2[m_Vertices.Count];
			Triangulator triangulator2 = new Triangulator(mesh2.vertices);
			mesh2.triangles = triangulator2.Triangulate();
			CombineInstance item2 = default(CombineInstance);
			item2.mesh = mesh2;
			item2.transform = roomBlob.transform.localToWorldMatrix;
			list.Add(item2);
		}
		Mesh mesh3 = new Mesh();
		mesh3.CombineMeshes(list.ToArray(), mergeSubMeshes: true, useMatrices: false);
		mesh3.RecalculateNormals();
		roomOcclusionMesh.m_RoomMesh = mesh3;
		meshFilter.sharedMesh = mesh3;
		return roomOcclusionMesh;
	}

	public static bool HasRoomGotOcclusionMesh(RoomBlob roomBlob)
	{
		RoomOcclusionMesh component = roomBlob.GetComponent<RoomOcclusionMesh>();
		if (component != null && component.m_RoomMesh != null)
		{
			return true;
		}
		return false;
	}

	private static void AddVertex(Vector3 roomTilePos, int searchDirection, RoomBlob room, RoomFloor floor)
	{
		roomTilePos += new Vector3(0.5f, 0.5f, 0f);
		Vector3 zero = Vector3.zero;
		switch (searchDirection)
		{
		case -1:
			zero.x = -0.345f;
			zero.y = 0.62f;
			break;
		case 0:
			zero.x = 0.345f;
			zero.y = 0.62f;
			break;
		case 1:
			zero.x = 0.345f;
			zero.y = -0.062f;
			break;
		case 2:
			zero.x = -0.345f;
			zero.y = -0.062f;
			break;
		case 3:
			zero.x = -0.345f;
			zero.y = 0.62f;
			break;
		}
		Vector3 tileWorldPos = RoomUtility.RoomGridToWorld(roomTilePos, floor) + zero;
		tileWorldPos -= room.transform.position;
		if (m_Vertices.FindIndex((Vector3 x) => x == tileWorldPos) == -1)
		{
			m_Vertices.Add(tileWorldPos);
		}
	}

	private static int GetPerpendicularDirectionIndex(int searchDirectionIndex)
	{
		return (searchDirectionIndex <= 0) ? (m_clockWiseSearchDirections.Length - 1) : (searchDirectionIndex - 1);
	}

	private static int GetNextSearchDirectionIndex(int searchDirectionIndex)
	{
		return (searchDirectionIndex < m_clockWiseSearchDirections.Length - 1) ? (searchDirectionIndex + 1) : 0;
	}

	private static bool IsTileWithinBounds(Vector3 tilePosition, int floorWidth, int floorHeight)
	{
		return tilePosition.x >= 0f && tilePosition.x <= (float)floorWidth && tilePosition.y >= 0f && tilePosition.y <= (float)floorHeight;
	}

	public static bool CanTraceBetweenTilesInRoom(Vector3 roomStartPos, Vector3 searchTilePos, RoomFloor roomFloor)
	{
		if (m_RoomManager == null)
		{
			m_RoomManager = RoomManager.GetInstance();
		}
		Vector3 vector = roomStartPos;
		RoomBlob roomBlob = m_RoomManager.LookUpRoom(vector, roomFloor);
		bool result = false;
		int num = 0;
		do
		{
			if (vector == searchTilePos)
			{
				result = true;
				break;
			}
			int perpendicularDirectionIndex = GetPerpendicularDirectionIndex(num);
			Vector3 vector2 = vector + m_clockWiseSearchDirections[num];
			Vector3 vector3 = vector + m_clockWiseSearchDirections[perpendicularDirectionIndex];
			RoomBlob roomBlob2 = null;
			if (IsTileWithinBounds(vector3, roomFloor.m_FloorWidth, roomFloor.m_FloorHeight))
			{
				roomBlob2 = m_RoomManager.LookUpRoom(vector3, roomFloor);
			}
			if (roomBlob2 != null && roomBlob2 == roomBlob)
			{
				num = perpendicularDirectionIndex;
			}
			else
			{
				roomBlob2 = ((!IsTileWithinBounds(vector2, roomFloor.m_FloorWidth, roomFloor.m_FloorHeight)) ? null : m_RoomManager.LookUpRoom(vector2, roomFloor));
				if (roomBlob2 == null || roomBlob2 != roomBlob)
				{
					num = GetNextSearchDirectionIndex(num);
				}
			}
			roomBlob2 = m_RoomManager.LookUpRoom(vector + m_clockWiseSearchDirections[num], roomFloor);
			if (roomBlob2 == roomBlob)
			{
				vector += m_clockWiseSearchDirections[num];
			}
		}
		while (vector != roomStartPos);
		return result;
	}
}
