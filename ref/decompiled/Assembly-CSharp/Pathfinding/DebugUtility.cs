using System;
using UnityEngine;

namespace Pathfinding;

[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_debug_utility.php")]
public class DebugUtility : MonoBehaviour
{
	public Material defaultMaterial;

	public static DebugUtility active;

	public float offset = 0.2f;

	public bool optimizeMeshes;

	public void Awake()
	{
		active = this;
	}

	public static void DrawCubes(Vector3[] topVerts, Vector3[] bottomVerts, Color[] vertexColors, float width)
	{
		if (active == null)
		{
			active = UnityEngine.Object.FindObjectOfType(typeof(DebugUtility)) as DebugUtility;
		}
		if (active == null)
		{
			throw new NullReferenceException();
		}
		if (topVerts.Length != bottomVerts.Length || topVerts.Length != vertexColors.Length)
		{
			Debug.LogError("Array Lengths are not the same");
			return;
		}
		if (topVerts.Length > 2708)
		{
			Vector3[] array = new Vector3[topVerts.Length - 2708];
			Vector3[] array2 = new Vector3[topVerts.Length - 2708];
			Color[] array3 = new Color[topVerts.Length - 2708];
			for (int i = 2708; i < topVerts.Length; i++)
			{
				ref Vector3 reference = ref array[i - 2708];
				reference = topVerts[i];
				ref Vector3 reference2 = ref array2[i - 2708];
				reference2 = bottomVerts[i];
				ref Color reference3 = ref array3[i - 2708];
				reference3 = vertexColors[i];
			}
			Vector3[] array4 = new Vector3[2708];
			Vector3[] array5 = new Vector3[2708];
			Color[] array6 = new Color[2708];
			for (int j = 0; j < 2708; j++)
			{
				ref Vector3 reference4 = ref array4[j];
				reference4 = topVerts[j];
				ref Vector3 reference5 = ref array5[j];
				reference5 = bottomVerts[j];
				ref Color reference6 = ref array6[j];
				reference6 = vertexColors[j];
			}
			DrawCubes(array, array2, array3, width);
			topVerts = array4;
			bottomVerts = array5;
			vertexColors = array6;
		}
		width /= 2f;
		Vector3[] array7 = new Vector3[topVerts.Length * 4 * 6];
		int[] array8 = new int[topVerts.Length * 6 * 6];
		Color[] array9 = new Color[topVerts.Length * 4 * 6];
		for (int k = 0; k < topVerts.Length; k++)
		{
			Vector3 vector = topVerts[k] + new Vector3(0f, active.offset, 0f);
			Vector3 vector2 = bottomVerts[k] - new Vector3(0f, active.offset, 0f);
			Vector3 vector3 = vector + new Vector3(0f - width, 0f, 0f - width);
			Vector3 vector4 = vector + new Vector3(width, 0f, 0f - width);
			Vector3 vector5 = vector + new Vector3(width, 0f, width);
			Vector3 vector6 = vector + new Vector3(0f - width, 0f, width);
			Vector3 vector7 = vector2 + new Vector3(0f - width, 0f, 0f - width);
			Vector3 vector8 = vector2 + new Vector3(width, 0f, 0f - width);
			Vector3 vector9 = vector2 + new Vector3(width, 0f, width);
			Vector3 vector10 = vector2 + new Vector3(0f - width, 0f, width);
			int num = k * 4 * 6;
			int num2 = k * 6 * 6;
			Color color = vertexColors[k];
			for (int l = num; l < num + 24; l++)
			{
				array9[l] = color;
			}
			array7[num] = vector3;
			array7[num + 1] = vector6;
			array7[num + 2] = vector5;
			array7[num + 3] = vector4;
			num += 4;
			array7[num + 3] = vector7;
			array7[num + 2] = vector10;
			array7[num + 1] = vector9;
			array7[num] = vector8;
			num += 4;
			array7[num] = vector8;
			array7[num + 1] = vector4;
			array7[num + 2] = vector5;
			array7[num + 3] = vector9;
			num += 4;
			array7[num + 3] = vector7;
			array7[num + 2] = vector3;
			array7[num + 1] = vector6;
			array7[num] = vector10;
			num += 4;
			array7[num + 3] = vector9;
			array7[num + 2] = vector10;
			array7[num + 1] = vector6;
			array7[num] = vector5;
			num += 4;
			array7[num] = vector8;
			array7[num + 1] = vector7;
			array7[num + 2] = vector3;
			array7[num + 3] = vector4;
			for (int m = 0; m < 6; m++)
			{
				int num3 = num + m * 4;
				int num4 = num2 + m * 6;
				array8[num4] = num3;
				array8[num4 + 1] = num3 + 1;
				array8[num4 + 2] = num3 + 2;
				array8[num4 + 3] = num3;
				array8[num4 + 4] = num3 + 2;
				array8[num4 + 5] = num3 + 3;
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array7;
		mesh.triangles = array8;
		mesh.colors = array9;
		mesh.name = "VoxelMesh";
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		if (active.optimizeMeshes)
		{
		}
		GameObject gameObject = new GameObject("DebugMesh");
		MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		meshRenderer.material = active.defaultMaterial;
		(gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter).mesh = mesh;
	}

	public static void DrawQuads(Vector3[] verts, float width)
	{
		if (verts.Length >= 16250)
		{
			Vector3[] array = new Vector3[verts.Length - 16250];
			for (int i = 16250; i < verts.Length; i++)
			{
				ref Vector3 reference = ref array[i - 16250];
				reference = verts[i];
			}
			Vector3[] array2 = new Vector3[16250];
			for (int j = 0; j < 16250; j++)
			{
				ref Vector3 reference2 = ref array2[j];
				reference2 = verts[j];
			}
			DrawQuads(array, width);
			verts = array2;
		}
		width /= 2f;
		Vector3[] array3 = new Vector3[verts.Length * 4];
		int[] array4 = new int[verts.Length * 6];
		for (int k = 0; k < verts.Length; k++)
		{
			Vector3 vector = verts[k];
			int num = k * 4;
			ref Vector3 reference3 = ref array3[num];
			reference3 = vector + new Vector3(0f - width, 0f, 0f - width);
			ref Vector3 reference4 = ref array3[num + 1];
			reference4 = vector + new Vector3(0f - width, 0f, width);
			ref Vector3 reference5 = ref array3[num + 2];
			reference5 = vector + new Vector3(width, 0f, width);
			ref Vector3 reference6 = ref array3[num + 3];
			reference6 = vector + new Vector3(width, 0f, 0f - width);
			int num2 = k * 6;
			array4[num2] = num;
			array4[num2 + 1] = num + 1;
			array4[num2 + 2] = num + 2;
			array4[num2 + 3] = num;
			array4[num2 + 4] = num + 2;
			array4[num2 + 5] = num + 3;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array3;
		mesh.triangles = array4;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		GameObject gameObject = new GameObject("DebugMesh");
		MeshRenderer meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		meshRenderer.material = active.defaultMaterial;
		(gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter).mesh = mesh;
	}
}
