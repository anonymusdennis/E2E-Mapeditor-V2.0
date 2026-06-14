using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicMesh : AlembicElement
{
	[Serializable]
	public class Split
	{
		public Vector3[] positionCache;

		public Vector3[] normalCache;

		public Vector2[] uvCache;

		public Vector4[] tangentCache;

		public Mesh mesh;

		public GameObject host;

		public bool clear;

		public int submeshCount;

		public bool active;

		public Vector3 center;

		public Vector3 size;
	}

	[Serializable]
	public class Submesh
	{
		public int[] indexCache;

		public int facesetIndex;

		public int splitIndex;

		public int index;

		public bool update;
	}

	public AbcAPI.aiFaceWindingOverride m_faceWinding = AbcAPI.aiFaceWindingOverride.InheritStreamSetting;

	public AbcAPI.aiNormalsModeOverride m_normalsMode = AbcAPI.aiNormalsModeOverride.InheritStreamSetting;

	public AbcAPI.aiTangentsModeOverride m_tangentsMode = AbcAPI.aiTangentsModeOverride.InheritStreamSetting;

	public bool m_cacheTangentsSplits = true;

	[HideInInspector]
	public bool hasFacesets;

	[HideInInspector]
	public List<Submesh> m_submeshes = new List<Submesh>();

	[HideInInspector]
	public List<Split> m_splits = new List<Split>();

	private AbcAPI.aiMeshSummary m_summary;

	private AbcAPI.aiMeshSampleSummary m_sampleSummary;

	private bool m_freshSetup;

	public static IntPtr GetArrayPtr(Array a)
	{
		return Marshal.UnsafeAddrOfPinnedArrayElement(a, 0);
	}

	private void UpdateSplits(int numSplits)
	{
		Split split = null;
		if (m_summary.topologyVariance == AbcAPI.aiTopologyVariance.Heterogeneous || numSplits > 1)
		{
			for (int i = 0; i < numSplits; i++)
			{
				if (i >= m_splits.Count)
				{
					Split split2 = new Split();
					split2.positionCache = new Vector3[0];
					split2.normalCache = new Vector3[0];
					split2.uvCache = new Vector2[0];
					split2.tangentCache = new Vector4[0];
					split2.mesh = null;
					split2.host = null;
					split2.clear = true;
					split2.submeshCount = 0;
					split2.active = true;
					split2.center = Vector3.zero;
					split2.size = Vector3.zero;
					split = split2;
					m_splits.Add(split);
				}
				else
				{
					m_splits[i].active = true;
				}
			}
		}
		else if (m_splits.Count == 0)
		{
			Split split2 = new Split();
			split2.positionCache = new Vector3[0];
			split2.normalCache = new Vector3[0];
			split2.uvCache = new Vector2[0];
			split2.tangentCache = new Vector4[0];
			split2.mesh = null;
			split2.host = m_trans.gameObject;
			split2.clear = true;
			split2.submeshCount = 0;
			split2.active = true;
			split2.center = Vector3.zero;
			split2.size = Vector3.zero;
			split = split2;
			m_splits.Add(split);
		}
		else
		{
			m_splits[0].active = true;
		}
		for (int j = numSplits; j < m_splits.Count; j++)
		{
			m_splits[j].active = false;
		}
	}

	public override void AbcSetup(AlembicStream abcStream, AbcAPI.aiObject abcObj, AbcAPI.aiSchema abcSchema)
	{
		base.AbcSetup(abcStream, abcObj, abcSchema);
		AbcAPI.aiPolyMeshGetSummary(abcSchema, ref m_summary);
		m_freshSetup = true;
	}

	public override void AbcDestroy()
	{
	}

	public override void AbcGetConfig(ref AbcAPI.aiConfig config)
	{
		if (m_normalsMode != AbcAPI.aiNormalsModeOverride.InheritStreamSetting)
		{
			config.normalsMode = (AbcAPI.aiNormalsMode)m_normalsMode;
		}
		if (m_tangentsMode != AbcAPI.aiTangentsModeOverride.InheritStreamSetting)
		{
			config.tangentsMode = (AbcAPI.aiTangentsMode)m_tangentsMode;
		}
		if (m_faceWinding != AbcAPI.aiFaceWindingOverride.InheritStreamSetting)
		{
			config.swapFaceWinding = m_faceWinding == AbcAPI.aiFaceWindingOverride.Swap;
		}
		config.cacheTangentsSplits = m_cacheTangentsSplits;
		AlembicMaterial component = m_trans.GetComponent<AlembicMaterial>();
		config.forceUpdate = m_freshSetup || ((!(component != null)) ? hasFacesets : component.HasFacesetsChanged());
	}

	public override void AbcSampleUpdated(AbcAPI.aiSample sample, bool topologyChanged)
	{
		AlembicMaterial component = m_trans.GetComponent<AlembicMaterial>();
		if (component != null)
		{
			if (component.HasFacesetsChanged())
			{
				AbcVerboseLog("AlembicMesh.AbcSampleUpdated: Facesets updated, force topology update");
				topologyChanged = true;
			}
			hasFacesets = component.GetFacesetsCount() > 0;
		}
		else if (hasFacesets)
		{
			AbcVerboseLog("AlembicMesh.AbcSampleUpdated: Facesets cleared, force topology update");
			topologyChanged = true;
			hasFacesets = false;
		}
		if (m_freshSetup)
		{
			topologyChanged = true;
			m_freshSetup = false;
		}
		AbcAPI.aiPolyMeshGetSampleSummary(sample, ref m_sampleSummary, topologyChanged);
		AbcAPI.aiPolyMeshData data = default(AbcAPI.aiPolyMeshData);
		UpdateSplits(m_sampleSummary.splitCount);
		for (int i = 0; i < m_sampleSummary.splitCount; i++)
		{
			Split split = m_splits[i];
			split.clear = topologyChanged;
			split.active = true;
			int newSize = AbcAPI.aiPolyMeshGetVertexBufferLength(sample, i);
			Array.Resize(ref split.positionCache, newSize);
			data.positions = GetArrayPtr(split.positionCache);
			if (m_sampleSummary.hasNormals)
			{
				Array.Resize(ref split.normalCache, newSize);
				data.normals = GetArrayPtr(split.normalCache);
			}
			else
			{
				Array.Resize(ref split.normalCache, 0);
				data.normals = IntPtr.Zero;
			}
			if (m_sampleSummary.hasUVs)
			{
				Array.Resize(ref split.uvCache, newSize);
				data.uvs = GetArrayPtr(split.uvCache);
			}
			else
			{
				Array.Resize(ref split.uvCache, 0);
				data.uvs = IntPtr.Zero;
			}
			if (m_sampleSummary.hasTangents)
			{
				Array.Resize(ref split.tangentCache, newSize);
				data.tangents = GetArrayPtr(split.tangentCache);
			}
			else
			{
				Array.Resize(ref split.tangentCache, 0);
				data.tangents = IntPtr.Zero;
			}
			AbcAPI.aiPolyMeshFillVertexBuffer(sample, i, ref data);
			split.center = data.center;
			split.size = data.size;
		}
		if (topologyChanged)
		{
			AbcAPI.aiFacesets facesets = default(AbcAPI.aiFacesets);
			AbcAPI.aiSubmeshSummary smi = default(AbcAPI.aiSubmeshSummary);
			AbcAPI.aiSubmeshData data2 = default(AbcAPI.aiSubmeshData);
			if (component != null)
			{
				component.GetFacesets(ref facesets);
			}
			int num = AbcAPI.aiPolyMeshPrepareSubmeshes(sample, ref facesets);
			if (m_submeshes.Count > num)
			{
				m_submeshes.RemoveRange(num, m_submeshes.Count - num);
			}
			for (int j = 0; j < m_sampleSummary.splitCount; j++)
			{
				m_splits[j].submeshCount = AbcAPI.aiPolyMeshGetSplitSubmeshCount(sample, j);
			}
			while (AbcAPI.aiPolyMeshGetNextSubmesh(sample, ref smi))
			{
				if (smi.splitIndex >= m_splits.Count)
				{
					Debug.Log("Invalid split index");
					continue;
				}
				Submesh submesh = null;
				if (smi.index < m_submeshes.Count)
				{
					submesh = m_submeshes[smi.index];
				}
				else
				{
					Submesh submesh2 = new Submesh();
					submesh2.indexCache = new int[0];
					submesh2.facesetIndex = -1;
					submesh2.splitIndex = -1;
					submesh2.index = -1;
					submesh2.update = true;
					submesh = submesh2;
					m_submeshes.Add(submesh);
				}
				submesh.facesetIndex = smi.facesetIndex;
				submesh.splitIndex = smi.splitIndex;
				submesh.index = smi.splitSubmeshIndex;
				submesh.update = true;
				Array.Resize(ref submesh.indexCache, 3 * smi.triangleCount);
				data2.indices = GetArrayPtr(submesh.indexCache);
				AbcAPI.aiPolyMeshFillSubmeshIndices(sample, ref smi, ref data2);
			}
			if (component != null)
			{
				component.AknowledgeFacesetsChanges();
			}
		}
		else
		{
			for (int k = 0; k < m_submeshes.Count; k++)
			{
				m_submeshes[k].update = false;
			}
		}
		AbcDirty();
	}

	public override void AbcUpdate()
	{
		if (!AbcIsDirty())
		{
			return;
		}
		bool flag = m_summary.topologyVariance == AbcAPI.aiTopologyVariance.Heterogeneous || m_sampleSummary.splitCount > 1;
		for (int i = 0; i < m_splits.Count; i++)
		{
			Split split = m_splits[i];
			if (split.active)
			{
				if (split.host == null)
				{
					if (flag)
					{
						string text = m_trans.gameObject.name + "_split_" + i;
						Transform transform = m_trans.FindChild(text);
						if (transform == null)
						{
							GameObject gameObject = new GameObject();
							gameObject.name = text;
							transform = gameObject.GetComponent<Transform>();
							transform.parent = m_trans;
							transform.localPosition = Vector3.zero;
							transform.localEulerAngles = Vector3.zero;
							transform.localScale = Vector3.one;
						}
						split.host = transform.gameObject;
					}
					else
					{
						split.host = m_trans.gameObject;
					}
				}
				if (split.mesh == null)
				{
					split.mesh = AddMeshComponents(m_abcObj, split.host);
					split.mesh.name = split.host.name;
				}
				if (split.clear)
				{
					split.mesh.Clear();
				}
				split.mesh.vertices = split.positionCache;
				split.mesh.normals = split.normalCache;
				split.mesh.tangents = split.tangentCache;
				split.mesh.uv = split.uvCache;
				split.mesh.bounds = new Bounds(split.center, split.size);
				if (split.clear)
				{
					split.mesh.subMeshCount = split.submeshCount;
					MeshRenderer component = split.host.GetComponent<MeshRenderer>();
					Material[] sharedMaterials = component.sharedMaterials;
					int num = sharedMaterials.Length;
					if (num != split.submeshCount)
					{
						Material[] array = new Material[split.submeshCount];
						int num2 = ((num >= split.submeshCount) ? split.submeshCount : num);
						for (int j = 0; j < num2; j++)
						{
							array[j] = sharedMaterials[j];
						}
						component.sharedMaterials = array;
					}
				}
				split.clear = false;
				split.host.SetActive(value: true);
			}
			else
			{
				split.host.SetActive(value: false);
			}
		}
		for (int k = 0; k < m_submeshes.Count; k++)
		{
			Submesh submesh = m_submeshes[k];
			if (submesh.update)
			{
				m_splits[submesh.splitIndex].mesh.SetIndices(submesh.indexCache, MeshTopology.Triangles, submesh.index);
				submesh.update = false;
			}
		}
		if (!m_sampleSummary.hasNormals && !m_sampleSummary.hasTangents)
		{
			for (int l = 0; l < m_sampleSummary.splitCount; l++)
			{
				m_splits[l].mesh.RecalculateNormals();
			}
		}
		AbcClean();
	}

	private Mesh AddMeshComponents(AbcAPI.aiObject abc, GameObject gameObject)
	{
		Mesh mesh = null;
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (meshFilter == null || meshFilter.sharedMesh == null)
		{
			mesh = new Mesh();
			mesh.MarkDynamic();
			if (meshFilter == null)
			{
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			meshFilter.sharedMesh = mesh;
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			if (component == null)
			{
				component = gameObject.AddComponent<MeshRenderer>();
			}
		}
		else
		{
			mesh = meshFilter.sharedMesh;
		}
		return mesh;
	}
}
