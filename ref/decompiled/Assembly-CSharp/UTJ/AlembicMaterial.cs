using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ;

[ExecuteInEditMode]
public class AlembicMaterial : MonoBehaviour
{
	public class Assignment
	{
		public Material material;

		public List<int> faces;
	}

	[Serializable]
	public class Facesets
	{
		public int[] faceCounts;

		public int[] faceIndices;
	}

	private bool dirty;

	private List<Material> materials = new List<Material>();

	[HideInInspector]
	public Facesets facesetsCache = new Facesets
	{
		faceCounts = new int[0],
		faceIndices = new int[0]
	};

	private void Start()
	{
	}

	private void Update()
	{
		if (materials.Count <= 0)
		{
			return;
		}
		AlembicMesh component = base.gameObject.GetComponent<AlembicMesh>();
		if (component != null)
		{
			int num = 0;
			int num2 = 0;
			if (component.m_submeshes.Count < materials.Count)
			{
				Debug.Log("\"" + component.name + "\": Not enough submeshes for all assigned materials. (" + materials.Count + " material(s) for " + component.m_submeshes.Count + " submesh(es))");
				return;
			}
			MeshRenderer component2 = base.gameObject.GetComponent<MeshRenderer>();
			foreach (AlembicMesh.Submesh submesh in component.m_submeshes)
			{
				if (submesh.splitIndex != num)
				{
					num2 = 0;
				}
				MeshRenderer meshRenderer = null;
				Transform transform = base.gameObject.transform.FindChild(base.gameObject.name + "_split_" + submesh.splitIndex);
				if (!(transform == null))
				{
					meshRenderer = ((submesh.splitIndex != 0 || transform.gameObject.activeSelf) ? transform.gameObject.GetComponent<MeshRenderer>() : component2);
				}
				else
				{
					if (submesh.splitIndex > 0)
					{
						Debug.Log("Invalid split index");
						return;
					}
					meshRenderer = component2;
				}
				if (meshRenderer == null)
				{
					Debug.Log("No renderer on \"" + base.gameObject.name + "\" to assign materials to");
					return;
				}
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				if (submesh.facesetIndex != -1)
				{
					if (submesh.facesetIndex < 0 || submesh.facesetIndex >= materials.Count)
					{
						Debug.Log("Invalid faceset index");
						return;
					}
					if (num2 >= sharedMaterials.Length)
					{
						Debug.Log("No material for submesh");
						return;
					}
					if (sharedMaterials[num2] != materials[submesh.facesetIndex])
					{
						sharedMaterials[num2] = materials[submesh.facesetIndex];
						meshRenderer.sharedMaterials = sharedMaterials;
						if (submesh.splitIndex == 0 && meshRenderer != component2 && component2 != null && sharedMaterials.Length == 1)
						{
							component2.sharedMaterials = sharedMaterials;
						}
					}
				}
				num = submesh.splitIndex;
				num2++;
			}
		}
		materials.Clear();
	}

	public int GetFacesetsCount()
	{
		return facesetsCache.faceCounts.Length;
	}

	public void GetFacesets(ref AbcAPI.aiFacesets facesets)
	{
		facesets.count = facesetsCache.faceCounts.Length;
		facesets.faceCounts = Marshal.UnsafeAddrOfPinnedArrayElement(facesetsCache.faceCounts, 0);
		facesets.faceIndices = Marshal.UnsafeAddrOfPinnedArrayElement(facesetsCache.faceIndices, 0);
	}

	public bool HasFacesetsChanged()
	{
		return dirty;
	}

	public void AknowledgeFacesetsChanges()
	{
		dirty = false;
	}

	public void UpdateAssignments(List<Assignment> assignments)
	{
		int num = 0;
		int num2 = 0;
		materials.Clear();
		if (facesetsCache.faceCounts.Length < assignments.Count)
		{
			Array.Resize(ref facesetsCache.faceCounts, assignments.Count);
			dirty = true;
		}
		for (int i = 0; i < assignments.Count; i++)
		{
			materials.Add(assignments[i].material);
			int count = assignments[i].faces.Count;
			dirty = dirty || facesetsCache.faceCounts[num] != count;
			facesetsCache.faceCounts[num++] = count;
			if (facesetsCache.faceIndices.Length < num2 + count)
			{
				Array.Resize(ref facesetsCache.faceIndices, num2 + count);
				dirty = true;
			}
			int num3 = 0;
			while (num3 < count)
			{
				int num4 = assignments[i].faces[num3];
				dirty = dirty || facesetsCache.faceIndices[num2] != num4;
				facesetsCache.faceIndices[num2] = num4;
				num3++;
				num2++;
			}
		}
	}
}
