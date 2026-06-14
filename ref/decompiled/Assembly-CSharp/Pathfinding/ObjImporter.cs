using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pathfinding;

public class ObjImporter
{
	private struct meshStruct
	{
		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector2[] uv;

		public int[] triangles;

		public Vector3[] faceData;

		public string fileName;
	}

	public static Mesh ImportFile(string filePath)
	{
		if (!File.Exists(filePath))
		{
			Debug.LogError("No file was found at '" + filePath + "'");
			return null;
		}
		meshStruct mesh = createMeshStruct(filePath);
		populateMeshStruct(ref mesh);
		Vector3[] array = new Vector3[mesh.faceData.Length];
		Vector2[] array2 = new Vector2[mesh.faceData.Length];
		Vector3[] array3 = new Vector3[mesh.faceData.Length];
		int num = 0;
		Vector3[] faceData = mesh.faceData;
		for (int i = 0; i < faceData.Length; i++)
		{
			Vector3 vector = faceData[i];
			ref Vector3 reference = ref array[num];
			reference = mesh.vertices[(int)vector.x - 1];
			if (vector.y >= 1f)
			{
				ref Vector2 reference2 = ref array2[num];
				reference2 = mesh.uv[(int)vector.y - 1];
			}
			if (vector.z >= 1f)
			{
				ref Vector3 reference3 = ref array3[num];
				reference3 = mesh.normals[(int)vector.z - 1];
			}
			num++;
		}
		Mesh mesh2 = new Mesh();
		mesh2.vertices = array;
		mesh2.uv = array2;
		mesh2.normals = array3;
		mesh2.triangles = mesh.triangles;
		mesh2.RecalculateBounds();
		return mesh2;
	}

	private static meshStruct createMeshStruct(string filename)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		meshStruct result = default(meshStruct);
		result.fileName = filename;
		StreamReader streamReader = File.OpenText(filename);
		string s = streamReader.ReadToEnd();
		streamReader.Dispose();
		using (StringReader stringReader = new StringReader(s))
		{
			string text = stringReader.ReadLine();
			char[] separator = new char[1] { ' ' };
			while (text != null)
			{
				if (!text.StartsWith("f ") && !text.StartsWith("v ") && !text.StartsWith("vt ") && !text.StartsWith("vn "))
				{
					text = stringReader.ReadLine();
					if (text != null)
					{
						text = text.Replace("  ", " ");
					}
					continue;
				}
				text = text.Trim();
				string[] array = text.Split(separator, 50);
				switch (array[0])
				{
				case "v":
					num2++;
					break;
				case "vt":
					num3++;
					break;
				case "vn":
					num4++;
					break;
				case "f":
					num5 = num5 + array.Length - 1;
					num += 3 * (array.Length - 2);
					break;
				}
				text = stringReader.ReadLine();
				if (text != null)
				{
					text = text.Replace("  ", " ");
				}
			}
		}
		result.triangles = new int[num];
		result.vertices = new Vector3[num2];
		result.uv = new Vector2[num3];
		result.normals = new Vector3[num4];
		result.faceData = new Vector3[num5];
		return result;
	}

	private static void populateMeshStruct(ref meshStruct mesh)
	{
		StreamReader streamReader = File.OpenText(mesh.fileName);
		string s = streamReader.ReadToEnd();
		streamReader.Close();
		using StringReader stringReader = new StringReader(s);
		string text = stringReader.ReadLine();
		char[] separator = new char[1] { ' ' };
		char[] separator2 = new char[1] { '/' };
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		while (text != null)
		{
			if (!text.StartsWith("f ") && !text.StartsWith("v ") && !text.StartsWith("vt ") && !text.StartsWith("vn ") && !text.StartsWith("g ") && !text.StartsWith("usemtl ") && !text.StartsWith("mtllib ") && !text.StartsWith("vt1 ") && !text.StartsWith("vt2 ") && !text.StartsWith("vc ") && !text.StartsWith("usemap "))
			{
				text = stringReader.ReadLine();
				if (text != null)
				{
					text = text.Replace("  ", " ");
				}
				continue;
			}
			text = text.Trim();
			string[] array = text.Split(separator, 50);
			switch (array[0])
			{
			case "v":
			{
				ref Vector3 reference3 = ref mesh.vertices[num3];
				reference3 = new Vector3(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), Convert.ToSingle(array[3]));
				num3++;
				break;
			}
			case "vt":
			{
				ref Vector2 reference2 = ref mesh.uv[num5];
				reference2 = new Vector2(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
				num5++;
				break;
			}
			case "vt1":
			{
				ref Vector2 reference = ref mesh.uv[num6];
				reference = new Vector2(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
				num6++;
				break;
			}
			case "vt2":
			{
				ref Vector2 reference5 = ref mesh.uv[num7];
				reference5 = new Vector2(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
				num7++;
				break;
			}
			case "vn":
			{
				ref Vector3 reference4 = ref mesh.normals[num4];
				reference4 = new Vector3(Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), Convert.ToSingle(array[3]));
				num4++;
				break;
			}
			case "f":
			{
				int num8 = 1;
				List<int> list = new List<int>();
				while (num8 < array.Length && (string.Empty + array[num8]).Length > 0)
				{
					Vector3 vector = default(Vector3);
					string[] array2 = array[num8].Split(separator2, 3);
					vector.x = Convert.ToInt32(array2[0]);
					if (array2.Length > 1)
					{
						if (array2[1] != string.Empty)
						{
							vector.y = Convert.ToInt32(array2[1]);
						}
						vector.z = Convert.ToInt32(array2[2]);
					}
					num8++;
					mesh.faceData[num2] = vector;
					list.Add(num2);
					num2++;
				}
				for (num8 = 1; num8 + 2 < array.Length; num8++)
				{
					mesh.triangles[num] = list[0];
					num++;
					mesh.triangles[num] = list[num8];
					num++;
					mesh.triangles[num] = list[num8 + 1];
					num++;
				}
				break;
			}
			}
			text = stringReader.ReadLine();
			if (text != null)
			{
				text = text.Replace("  ", " ");
			}
		}
	}
}
