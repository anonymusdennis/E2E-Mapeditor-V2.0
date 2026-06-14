using System.Collections.Generic;
using UnityEngine;

namespace UTJ;

[RequireComponent(typeof(AlembicPoints))]
[AddComponentMenu("UTJ/Alembic/Points Renderer")]
[ExecuteInEditMode]
public class AlembicPointsRenderer : MonoBehaviour
{
	private const int TextureWidth = 2048;

	public bool m_makeChildRenderers = true;

	public Mesh m_mesh;

	public Material[] m_materials;

	public bool m_cast_shadow;

	public bool m_receive_shadow;

	public float m_count_rate = 1f;

	public Vector3 m_model_scale = Vector3.one;

	public Vector3 m_trans_scale = Vector3.one;

	private int m_instances_par_batch;

	private Mesh m_expanded_mesh;

	private Bounds m_bounds;

	private List<List<Material>> m_actual_materials;

	[SerializeField]
	private List<MeshRenderer> m_child_renderers;

	private RenderTexture m_texPositions;

	private RenderTexture m_texVelocities;

	private RenderTexture m_texIDs;

	public const int MaxVertices = 65000;

	public static int ceildiv(int v, int d)
	{
		return v / d + ((v % d != 0) ? 1 : 0);
	}

	public static Vector3 mul(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	private static RenderTexture CreateDataTexture(int w, int h, RenderTextureFormat f)
	{
		RenderTexture renderTexture = new RenderTexture(w, h, 0, f);
		renderTexture.filterMode = FilterMode.Point;
		renderTexture.useMipMap = false;
		renderTexture.autoGenerateMips = false;
		renderTexture.Create();
		return renderTexture;
	}

	public static Mesh CreateExpandedMesh(Mesh mesh, int required_instances, out int instances_par_batch)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] array = ((mesh.normals != null && mesh.normals.Length != 0) ? mesh.normals : null);
		Vector4[] array2 = ((mesh.tangents != null && mesh.tangents.Length != 0) ? mesh.tangents : null);
		Vector2[] array3 = ((mesh.uv != null && mesh.uv.Length != 0) ? mesh.uv : null);
		Color[] array4 = ((mesh.colors != null && mesh.colors.Length != 0) ? mesh.colors : null);
		int[] array5 = ((mesh.triangles != null && mesh.triangles.Length != 0) ? mesh.triangles : null);
		instances_par_batch = Mathf.Min(65000 / mesh.vertexCount, required_instances);
		Vector3[] array6 = new Vector3[vertices.Length * instances_par_batch];
		Vector2[] array7 = new Vector2[vertices.Length * instances_par_batch];
		Vector3[] array8 = ((array != null) ? new Vector3[array.Length * instances_par_batch] : null);
		Vector4[] array9 = ((array2 != null) ? new Vector4[array2.Length * instances_par_batch] : null);
		Vector2[] array10 = ((array3 != null) ? new Vector2[array3.Length * instances_par_batch] : null);
		Color[] array11 = ((array4 != null) ? new Color[array4.Length * instances_par_batch] : null);
		int[] array12 = ((array5 != null) ? new int[array5.Length * instances_par_batch] : null);
		for (int i = 0; i < instances_par_batch; i++)
		{
			for (int j = 0; j < vertices.Length; j++)
			{
				int num = i * vertices.Length + j;
				ref Vector3 reference = ref array6[num];
				reference = vertices[j];
				ref Vector2 reference2 = ref array7[num];
				reference2 = new Vector2(i, j);
			}
			if (array8 != null)
			{
				for (int k = 0; k < array.Length; k++)
				{
					int num2 = i * array.Length + k;
					ref Vector3 reference3 = ref array8[num2];
					reference3 = array[k];
				}
			}
			if (array9 != null)
			{
				for (int l = 0; l < array2.Length; l++)
				{
					int num3 = i * array2.Length + l;
					ref Vector4 reference4 = ref array9[num3];
					reference4 = array2[l];
				}
			}
			if (array10 != null)
			{
				for (int m = 0; m < array3.Length; m++)
				{
					int num4 = i * array3.Length + m;
					ref Vector2 reference5 = ref array10[num4];
					reference5 = array3[m];
				}
			}
			if (array11 != null)
			{
				for (int n = 0; n < array4.Length; n++)
				{
					int num5 = i * array4.Length + n;
					ref Color reference6 = ref array11[num5];
					reference6 = array4[n];
				}
			}
			if (array12 != null)
			{
				for (int num6 = 0; num6 < array5.Length; num6++)
				{
					int num7 = i * array5.Length + num6;
					array12[num7] = i * vertices.Length + array5[num6];
				}
			}
		}
		Mesh mesh2 = new Mesh();
		mesh2.vertices = array6;
		mesh2.normals = array8;
		mesh2.tangents = array9;
		mesh2.uv = array10;
		mesh2.colors = array11;
		mesh2.uv2 = array7;
		mesh2.triangles = array12;
		return mesh2;
	}

	private Material CloneMaterial(Material src, int nth)
	{
		Material material = new Material(src);
		material.SetInt("_BatchBegin", nth * m_instances_par_batch);
		material.SetTexture("_PositionBuffer", m_texPositions);
		material.SetTexture("_VelocityBuffer", m_texVelocities);
		material.SetTexture("_IDBuffer", m_texIDs);
		if (material.renderQueue >= 3000)
		{
			material.renderQueue += nth + 1;
		}
		return material;
	}

	public void RefleshMaterials()
	{
		m_actual_materials = null;
		Flush();
	}

	public void Flush()
	{
		if (m_mesh == null)
		{
			Debug.LogWarning("AlembicPointsRenderer: mesh is not assigned");
			return;
		}
		if (m_materials == null || m_materials.Length == 0 || (m_materials.Length == 1 && m_materials[0] == null))
		{
			Debug.LogWarning("AlembicPointsRenderer: material is not assigned");
			return;
		}
		AlembicPoints component = GetComponent<AlembicPoints>();
		AbcAPI.aiPointsData abcData = component.abcData;
		int abcPeakVertexCount = component.abcPeakVertexCount;
		int count = abcData.count;
		m_bounds.center = mul(abcData.boundsCenter, m_trans_scale);
		m_bounds.extents = mul(abcData.boundsExtents, m_trans_scale);
		if (count == 0)
		{
			return;
		}
		if (m_texPositions == null || !m_texPositions.IsCreated())
		{
			int h = ceildiv(abcPeakVertexCount, 2048);
			m_texPositions = CreateDataTexture(2048, h, RenderTextureFormat.ARGBFloat);
			m_texVelocities = CreateDataTexture(2048, h, RenderTextureFormat.ARGBFloat);
			m_texIDs = CreateDataTexture(2048, h, RenderTextureFormat.RFloat);
		}
		TextureWriter.Write(m_texPositions, abcData.positions, abcData.count, TextureWriter.tDataFormat.Float3);
		TextureWriter.Write(m_texIDs, abcData.ids, abcData.count, TextureWriter.tDataFormat.LInt);
		if (m_expanded_mesh != null && m_expanded_mesh.name != m_mesh.name + "_expanded")
		{
			m_expanded_mesh = null;
		}
		if (m_expanded_mesh == null)
		{
			m_expanded_mesh = CreateExpandedMesh(m_mesh, abcPeakVertexCount, out m_instances_par_batch);
			m_expanded_mesh.UploadMeshData(markNoLogerReadable: true);
			m_expanded_mesh.name = m_mesh.name + "_expanded";
		}
		if (m_actual_materials == null)
		{
			m_actual_materials = new List<List<Material>>();
			while (m_actual_materials.Count < m_materials.Length)
			{
				m_actual_materials.Add(new List<Material>());
			}
		}
		Transform component2 = GetComponent<Transform>();
		m_expanded_mesh.bounds = m_bounds;
		m_count_rate = Mathf.Max(m_count_rate, 0f);
		count = Mathf.Min((int)((float)count * m_count_rate), (int)((float)abcPeakVertexCount * m_count_rate));
		int batch_count = ceildiv(count, m_instances_par_batch);
		for (int i = 0; i < m_actual_materials.Count; i++)
		{
			List<Material> list = m_actual_materials[i];
			while (list.Count < batch_count)
			{
				Material item = CloneMaterial(m_materials[i], list.Count);
				list.Add(item);
			}
		}
		Matrix4x4 localToWorldMatrix = component2.localToWorldMatrix;
		for (int j = 0; j < m_actual_materials.Count; j++)
		{
			List<Material> list2 = m_actual_materials[j];
			for (int k = 0; k < list2.Count; k++)
			{
				Material material = list2[k];
				material.SetInt("_BatchBegin", k * m_instances_par_batch);
				material.SetTexture("_PositionBuffer", m_texPositions);
				material.SetTexture("_VelocityBuffer", m_texVelocities);
				material.SetTexture("_IDBuffer", m_texIDs);
				material.SetInt("_NumInstances", count);
				material.SetVector("_CountRate", new Vector4(m_count_rate, 1f / m_count_rate, 0f, 0f));
				material.SetVector("_ModelScale", m_model_scale);
				material.SetVector("_TransScale", m_trans_scale);
				material.SetMatrix("_Transform", localToWorldMatrix);
			}
		}
		if (m_makeChildRenderers)
		{
			if (m_child_renderers == null)
			{
				m_child_renderers = new List<MeshRenderer>();
			}
			while (m_child_renderers.Count < m_materials.Length)
			{
				m_child_renderers.Add(MakeChildRenderer(m_child_renderers.Count));
			}
			for (int l = 0; l < m_actual_materials.Count; l++)
			{
				List<Material> list3 = m_actual_materials[l];
				list3.RemoveRange(batch_count, list3.Count - batch_count);
				m_child_renderers[l].sharedMaterials = list3.ToArray();
			}
			for (int m = 0; m < m_child_renderers.Count; m++)
			{
				MeshFilter component3 = m_child_renderers[m].GetComponent<MeshFilter>();
				if (component3.sharedMesh != m_expanded_mesh)
				{
					component3.sharedMesh = m_expanded_mesh;
				}
			}
			return;
		}
		if (m_child_renderers != null)
		{
			foreach (MeshRenderer child_renderer in m_child_renderers)
			{
				if (child_renderer != null)
				{
					Object.DestroyImmediate(child_renderer.gameObject);
				}
			}
			m_child_renderers = null;
		}
		int layer = base.gameObject.layer;
		Matrix4x4 matrix = Matrix4x4.identity;
		m_actual_materials.ForEach(delegate(List<Material> a)
		{
			for (int n = 0; n < batch_count; n++)
			{
				Graphics.DrawMesh(m_expanded_mesh, matrix, a[n], layer, null, 0, null, m_cast_shadow, m_receive_shadow);
			}
		});
	}

	private MeshRenderer MakeChildRenderer(int i)
	{
		GameObject gameObject = new GameObject();
		Transform component = gameObject.GetComponent<Transform>();
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer result = gameObject.AddComponent<MeshRenderer>();
		gameObject.name = "MeshRenderer[" + i + "]";
		component.SetParent(GetComponent<Transform>());
		meshFilter.sharedMesh = m_expanded_mesh;
		return result;
	}

	private void ReleaseGPUResoureces()
	{
		if (m_actual_materials != null)
		{
			m_actual_materials.ForEach(delegate(List<Material> a)
			{
				a.Clear();
			});
		}
		if (m_texPositions != null)
		{
			m_texPositions.Release();
			m_texPositions = null;
		}
		if (m_texVelocities != null)
		{
			m_texVelocities.Release();
			m_texVelocities = null;
		}
		if (m_texIDs != null)
		{
			m_texIDs.Release();
			m_texIDs = null;
		}
		m_bounds = default(Bounds);
	}

	private void OnDisable()
	{
		ReleaseGPUResoureces();
	}

	private void LateUpdate()
	{
		Flush();
	}
}
